using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;
using NLog;

using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NesEmulator
{

    class OperResult
    {

        public PureByte oper;
        public byte pageChange;

        public OperResult(PureByte oper, byte pageChange)
        {
            if ((pageChange & 0xfe) != 0)
            {
                throw new Exception("Page change must be 0 or 1");
            }
            this.oper = oper;
            this.pageChange = pageChange;
        }

    }

    public class CPU
    {

        private const bool makeLog = false;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CPUMEM mem;
        private int cycle;
        private bool kil = false;

        private Dictionary<string, string> opcodes;
        private Dictionary<string, string> illegalOpcodes;

        public CPU()
        {
            // initialize memory
            this.mem = new CPUMEM();
            this.cycle = 7; // todo: cpu boot sequence

            // load opcodes

            string opcodeJson = File.ReadAllText("../../data/opcodes.json");
            this.opcodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(opcodeJson);

            string illegalOpcodeJson = File.ReadAllText("../../data/illegal_opcodes.json");
            this.illegalOpcodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(illegalOpcodeJson);
        }

        private void log(string message)
        {
            // for logging results
            logger.Debug(message);
            Console.WriteLine(message);
        }

        public void Load(string filename, int start)
        {
            // load rom into memory
            using (FileStream fs = File.OpenRead(filename))
            {
                int current;
                for (int i = start; i < 0x10000; i++)
                {
                    current = fs.ReadByte();
                    if (current == -1)
                    {
                        break;
                    }

                    this.mem.set(i, current);
                }
            }
            this.log(filename + " loaded");
        }

        public void Load(string filename)
        {
            this.Load(filename, 0);
        }

        public int GetCycle()
        {
            return this.cycle;
        }

        public void SetPc(int val)
        {
            this.mem.pc[0].set(val / 0x100);
            this.mem.pc[1].set(val % 0x100);
        }

        public void Run()
        {
            while (!this.kil)
            {
                this.cycle += this.step();
            }
        }

        public int step()
        {
            string opcode = this.mem.getCurrent().hex();
            this.mem.incrPc();

            string instructionString;
            try
            {
                instructionString = this.opcodes[opcode];
            } catch (KeyNotFoundException)
            {
                try
                {
                    instructionString = this.illegalOpcodes[opcode];
                } catch (KeyNotFoundException)
                {
                    this.kil = true;
                    return 0;
                }
            }

            string instruction = instructionString.Split(' ')[0];
            string mode = instructionString.Split(' ')[1];

            if (makeLog)
            {
                this.log(
                    string.Format(
                        "    {0}  {1} {2}: {3,5}\t\t{4}     A:{5} X:{6} Y:{7} P:{8} SP:{9}        CYC:{10}",
                        (this.mem.getPc() - 1).ToString("x2"),
                        opcode,
                        instruction,
                        mode,
                        this.mem.getCurrent().unsigned().ToString("x2"),
                        this.mem.ac.hex(),
                        this.mem.x.hex(),
                        this.mem.y.hex(),
                        this.mem.sr.hex(),
                        this.mem.sp.hex(),
                        this.cycle
                    )
                );
            }
            
            return this.execute(instruction, mode);
        }

        private int execute(string instruction, string mode)
        {
            /*
            * JMP and JSR instructions work slightly differently,
            * as we need to operate on 2 bytes instead of a memory address
            */

            if (instruction.Equals("JMP"))
            {
                /*
                JMP  Jump to New Location
                    (PC+1) -> PCL                    N Z C I D V
                    (PC+2) -> PCH                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    absolute      JMP oper      4C    3     3
                    indirect      JMP (oper)    6C    3     5
                */
                PureByte ll = this.mem.getCurrent();
                this.mem.incrPc();
                PureByte hh = this.mem.getCurrent();
                this.mem.incrPc();

                switch (mode)
                {
                    case "abs": {
                            this.mem.setPc(ll, hh);
                            return 3;
                        }
                    case "ind": {
                            this.mem.setPc(
                                    this.mem.get(hh.unsigned() * 0x100 + ll.unsigned()),
                                    this.mem.get(hh.unsigned() * 0x100 + ((ll.unsigned() + 1) % 0x100))
                            );
                            return 5;
                        }
                    default: throw new Exception("Unknown mode for JMP: " + mode);
                }
            }
            else if (instruction.Equals("JSR"))
            {
                /*
                JSR  Jump to New Location Saving Return Address
                    push (PC+2),                     N Z C I D V
                    (PC+1) -> PCL                    - - - - - -
                    (PC+2) -> PCH
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    absolute      JSR oper      20    3     6
                */
                byte v = (byte)(this.mem.pc[1].unsigned() + 1 > 0xff ? 0b1 : 0b0);
                this.mem.push(new PureByte(this.mem.pc[0].unsigned() + v));
                this.mem.push(new PureByte(this.mem.pc[1].unsigned() + 1));

                PureByte ll = this.mem.getCurrent();
                this.mem.incrPc();
                PureByte hh = this.mem.getCurrent();
                this.mem.setPc(ll, hh);

                if (!mode.Equals("abs"))
                {
                    throw new Exception("Unknown mode for JSR: " + mode);
                }
                return 6;
            }

            OperResult or = this.getOper(mode);

            switch (instruction)
            {
                case "ADC": return this.ADC(or.oper, mode, or.pageChange);
                case "ALR": return this.ALR(or.oper, mode, or.pageChange);
                case "AND": return this.AND(or.oper, mode, or.pageChange);
                case "ARR": return this.ARR(or.oper, mode, or.pageChange);
                case "ASL": return this.ASL(or.oper, mode, or.pageChange);
                case "BCC": return this.BCC(or.oper, mode, or.pageChange);
                case "BCS": return this.BCS(or.oper, mode, or.pageChange);
                case "BEQ": return this.BEQ(or.oper, mode, or.pageChange);
                case "BIT": return this.BIT(or.oper, mode, or.pageChange);
                case "BMI": return this.BMI(or.oper, mode, or.pageChange);
                case "BNE": return this.BNE(or.oper, mode, or.pageChange);
                case "BPL": return this.BPL(or.oper, mode, or.pageChange);
                case "BRK": return this.BRK(or.oper, mode, or.pageChange);
                case "BVC": return this.BVC(or.oper, mode, or.pageChange);
                case "BVS": return this.BVS(or.oper, mode, or.pageChange);
                case "CLC": return this.CLC(or.oper, mode, or.pageChange);
                case "CLD": return this.CLD(or.oper, mode, or.pageChange);
                case "CLI": return this.CLI(or.oper, mode, or.pageChange);
                case "CLV": return this.CLV(or.oper, mode, or.pageChange);
                case "CMP": return this.CMP(or.oper, mode, or.pageChange);
                case "CPX": return this.CPX(or.oper, mode, or.pageChange);
                case "CPY": return this.CPY(or.oper, mode, or.pageChange);
                case "DCP": return this.DCP(or.oper, mode, or.pageChange);
                case "DEC": return this.DEC(or.oper, mode, or.pageChange);
                case "DEX": return this.DEX(or.oper, mode, or.pageChange);
                case "DEY": return this.DEY(or.oper, mode, or.pageChange);
                case "EOR": return this.EOR(or.oper, mode, or.pageChange);
                case "INC": return this.INC(or.oper, mode, or.pageChange);
                case "INX": return this.INX(or.oper, mode, or.pageChange);
                case "INY": return this.INY(or.oper, mode, or.pageChange);
                case "ISC": return this.ISC(or.oper, mode, or.pageChange);
                case "LAX": return this.LAX(or.oper, mode, or.pageChange);
                case "LDA": return this.LDA(or.oper, mode, or.pageChange);
                case "LDX": return this.LDX(or.oper, mode, or.pageChange);
                case "LDY": return this.LDY(or.oper, mode, or.pageChange);
                case "LSR": return this.LSR(or.oper, mode, or.pageChange);
                case "NOP": return this.NOP(or.oper, mode, or.pageChange);
                case "ORA": return this.ORA(or.oper, mode, or.pageChange);
                case "PHA": return this.PHA(or.oper, mode, or.pageChange);
                case "PHP": return this.PHP(or.oper, mode, or.pageChange);
                case "PLA": return this.PLA(or.oper, mode, or.pageChange);
                case "PLP": return this.PLP(or.oper, mode, or.pageChange);
                case "RLA": return this.RLA(or.oper, mode, or.pageChange);
                case "ROL": return this.ROL(or.oper, mode, or.pageChange);
                case "ROR": return this.ROR(or.oper, mode, or.pageChange);
                case "RRA": return this.RRA(or.oper, mode, or.pageChange);
                case "RTI": return this.RTI(or.oper, mode, or.pageChange);
                case "RTS": return this.RTS(or.oper, mode, or.pageChange);
                case "SAX": return this.SAX(or.oper, mode, or.pageChange);
                case "SBC": return this.SBC(or.oper, mode, or.pageChange);
                case "SEC": return this.SEC(or.oper, mode, or.pageChange);
                case "SED": return this.SED(or.oper, mode, or.pageChange);
                case "SEI": return this.SEI(or.oper, mode, or.pageChange);
                case "SLO": return this.SLO(or.oper, mode, or.pageChange);
                case "SRE": return this.SRE(or.oper, mode, or.pageChange);
                case "STA": return this.STA(or.oper, mode, or.pageChange);
                case "STX": return this.STX(or.oper, mode, or.pageChange);
                case "STY": return this.STY(or.oper, mode, or.pageChange);
                case "TAX": return this.TAX(or.oper, mode, or.pageChange);
                case "TAY": return this.TAY(or.oper, mode, or.pageChange);
                case "TSX": return this.TSX(or.oper, mode, or.pageChange);
                case "TXA": return this.TXA(or.oper, mode, or.pageChange);
                case "TXS": return this.TXS(or.oper, mode, or.pageChange);
                case "TYA": return this.TYA(or.oper, mode, or.pageChange);
                case "XAA": return this.XAA(or.oper, mode, or.pageChange);
                default: throw new Exception("Unknown instruction: " + mode);
            }
        }

        private OperResult getOper(string mode)
        {
            PureByte ll;
            PureByte hh;

            switch (mode)
            {
                case "impl":
                    return new OperResult(new PureByte(), (byte)0);
                case "A":
                    return new OperResult(this.mem.ac, (byte)0);
                case "abs":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(this.mem.get(ll, hh), (byte)0);

                case "abs,X":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(
                            this.mem.get((hh.unsigned() * 0x100 + ll.unsigned() + this.mem.x.unsigned()) % 0x10000),
                            (byte)(ll.unsigned() + this.mem.x.unsigned() > 0xff ? 1 : 0)
                    );

                case "abs,Y":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(
                            this.mem.get((hh.unsigned() * 0x100 + ll.unsigned() + this.mem.y.unsigned()) % 0x10000),
                            (byte)(ll.unsigned() + this.mem.y.unsigned() > 0xff ? 1 : 0)
                    );

                case "#":
                    PureByte oper = this.mem.getCurrent();
                    this.mem.incrPc();
                    return new OperResult(oper, (byte)0);

                case "X,ind":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(
                            this.mem.get(
                                    this.mem.get((ll.unsigned() + this.mem.x.unsigned()) % 0x100),
                                    this.mem.get((ll.unsigned() + this.mem.x.unsigned() + 1) % 0x100)
                            ), (byte)0
                    );

                case "ind,Y":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    int effective_low = this.mem.get(ll.unsigned()).unsigned();
                    int effective_high = this.mem.get((ll.unsigned() + 1) % 0x100).unsigned() * 0x100;
                    int effective = (effective_high + effective_low + this.mem.y.unsigned()) % 0x10000;

                    return new OperResult(
                            this.mem.get(effective), (byte)(effective_low + this.mem.y.unsigned() > 0xff ? 1 : 0)
                    );

                case "rel":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(ll, (byte)0);

                case "zpg":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(this.mem.get(ll.unsigned()), (byte)0);

                case "zpg,X":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(this.mem.get((ll.unsigned() + this.mem.x.unsigned()) % 0x100), (byte)0);

                case "zpg,Y":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    return new OperResult(this.mem.get((ll.unsigned() + this.mem.y.unsigned()) % 0x100), (byte)0);

                default:
                    throw new Exception("Unknown mode: " + mode);
            }
        }

        private int branch(PureByte oper)
        {
            int target = this.mem.pc[1].unsigned() + oper.signed();
            int c = 0;
            if (target > 0xff)
            {
                c = 1;
            }
            else if (target < 0)
            {
                c = -1;
            }

            this.mem.pc[1].add(oper.signed());
            if (c != 0)
            {
                this.mem.pc[0].add(c);
            }

            return c;
        }

        private int ADC(PureByte oper, string mode, int pageChange)
        {
            /*
            ADC  Add Memory to Accumulator with Carry
                    A + M + C -> A, C                N Z C I D V
                                                    + + + - - +
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     ADC #oper     69    2     2;
                    zeropage      ADC oper      65    2     3;
                    zeropage,X    ADC oper,X    75    2     4;
                    absolute      ADC oper      6D    3     4;
                    absolute,X    ADC oper,X    7D    3     4*
                    absolute,Y    ADC oper,Y    79    3     4*
                    (indirect,X)  ADC (oper,X)  61    2     6;
                    (indirect),Y  ADC (oper),Y  71    2     5*
            */

            int unsigned_result = this.mem.ac.unsigned() + oper.unsigned() + this.mem.getFlag('C');
            int signed_result = this.mem.ac.signed() + oper.signed() + this.mem.getFlag('C');

            this.mem.ac.set(unsigned_result);

            this.mem.setNZ(this.mem.ac);
            this.mem.setFlag('C', (byte)(unsigned_result > 0xff ? 1 : 0));
            this.mem.setFlag('V', (byte)(signed_result > 127 || signed_result < -128 ? 1 : 0));

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X":
                case"abs": return 4;
                case "abs,X":
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int AND(PureByte oper, string mode, int pageChange)
        {
            /*
            AND  AND Memory with Accumulator
                    A AND M -> A                     N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     AND #oper     29    2     2;
                    zeropage      AND oper      25    2     3;
                    zeropage,X    AND oper,X    35    2     4;
                    absolute      AND oper      2D    3     4;
                    absolute,X    AND oper,X    3D    3     4*
                    absolute,Y    AND oper,Y    39    3     4*
                    (indirect,X)  AND (oper,X)  21    2     6;
                    (indirect),Y  AND (oper),Y  31    2     5*
            */

            this.mem.ac.and(oper);
            this.mem.setNZ(this.mem.ac);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ASL(PureByte oper, string mode, int pageChange)
        {
            /*
            ASL  Shift Left One Bit (Memory || Accumulator);
                    C <- [76543210] <- 0             N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    accumulator   ASL A         0A    1     2;
                    zeropage      ASL oper      06    2     5;
                    zeropage,X    ASL oper,X    16    2     6;
                    absolute      ASL oper      0E    3     6;
                    absolute,X    ASL oper,X    1E    3     7;
            */

            this.mem.setFlag('C', (byte)(oper.unsigned() >= 128 ? 1 : 0));
            oper.lshift();
            this.mem.setNZ(oper);

            switch (mode)
            {
                case "A": return 2;
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int BCC(PureByte oper, string mode, int pageChange)
        {
            /*
            BCC  Branch on Carry Clear
                    branch on C = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BCC oper      90    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('C') == 0)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BCS(PureByte oper, string mode, int pageChange)
        {
            /*
            BCS  Branch on Carry Set
                    branch on C = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BCS oper      B0    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('C') == 1)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BEQ(PureByte oper, string mode, int pageChange)
        {
            /*
            BEQ  Branch on Result Zero
                    branch on Z = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BEQ oper      F0    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('Z') == 1)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BIT(PureByte oper, string mode, int pageChange)
        {
            /*
            BIT  Test Bits in Memory with Accumulator
                    bits 7 && 6 of operand are transfered to bit 7 && 6 of SR (N,V);
                    the zeroflag is set to the result of operand AND accumulator.
                    A AND M, M7 -> N, M6 -> V        N Z C I D V
                                                    M7 + - - - M6;
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      BIT oper      24    2     3;
                    absolute      BIT oper      2C    3     4;
            */

            this.mem.setFlag('N', oper.getBit(7));
            this.mem.setFlag('V', oper.getBit(6));
            this.mem.setFlag('Z', (byte)((oper.unsigned() & this.mem.ac.unsigned()) == 0 ? 1 : 0));

            switch (mode)
            {
                case "zpg": return 3;
                case "abs": return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int BMI(PureByte oper, string mode, int pageChange)
        {
            /*
            BMI  Branch on Result Minus
                    branch on N = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BMI oper      30    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('N') == 1)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BNE(PureByte oper, string mode, int pageChange)
        {
            /*
            BNE  Branch on Result not Zero
                    branch on Z = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BNE oper      D0    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('Z') == 0)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BPL(PureByte oper, string mode, int pageChange)
        {
            /*
            BPL  Branch on Result Plus
                    branch on N = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BPL oper      10    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('N') == 0)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BRK(PureByte oper, string mode, int pageChange)
        {
            /*
            BRK  Force Break
                    interrupt,                       N Z C I D V
                    push PC+2, push SR               - - - 1 - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       BRK           00    1     7;
            */

            this.mem.setFlag('I', (byte)1);
            this.mem.push(this.mem.pc[0]);
            this.mem.push(this.mem.pc[1]);
            this.mem.push(new PureByte(this.mem.sr.unsigned() | 0b00100000));
            this.mem.setPc(this.mem.get(this.mem.irqVector[0]), this.mem.get(this.mem.irqVector[1]));

            if ("impl".Equals(mode))
            {
                return 7;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BVC(PureByte oper, string mode, int pageChange)
        {
            /*
            BVC  Branch on Overflow Clear
                    branch on V = 0                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BVC oper      50    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('V') == 0)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int BVS(PureByte oper, string mode, int pageChange)
        {
            /*
            BVS  Branch on Overflow Set
                    branch on V = 1                  N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    relative      BVC oper      70    2     2**
            */

            if ("rel".Equals(mode))
            {
                if (this.mem.getFlag('V') == 1)
                {
                    if (this.branch(oper) == 1)
                    {
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLC(PureByte oper, string mode, int pageChange)
        {
            /*
            CLC  Clear Carry Flag
                    0 -> C                           N Z C I D V
                                                    - - 0 - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLC           18    1     2;
            */

            this.mem.setFlag('C', (byte)0);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLD(PureByte oper, string mode, int pageChange)
        {
            /*
            CLD  Clear Decimal Mode
                    0 -> D                           N Z C I D V
                                                    - - - - 0 -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLD           D8    1     2;
            */

            this.mem.setFlag('D', (byte)0);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLI(PureByte oper, string mode, int pageChange)
        {
            /*
            CLI  Clear Interrupt Disable Bit
                    0 -> I                           N Z C I D V
                                                    - - - 0 - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLI           58    1     2;
            */

            this.mem.setFlag('I', (byte)0);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CLV(PureByte oper, string mode, int pageChange)
        {
            /*
            CLV  Clear Overflow Flag
                    0 -> V                           N Z C I D V
                                                    - - - - - 0;
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       CLV           B8    1     2;
            */

            this.mem.setFlag('V', (byte)0);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int CMP(PureByte oper, string mode, int pageChange)
        {
            /*
            CMP  Compare Memory with Accumulator
                    A - M                            N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     CMP #oper     C9    2     2;
                    zeropage      CMP oper      C5    2     3;
                    zeropage,X    CMP oper,X    D5    2     4;
                    absolute      CMP oper      CD    3     4;
                    absolute,X    CMP oper,X    DD    3     4*
                    absolute,Y    CMP oper,Y    D9    3     4*
                    (indirect,X)  CMP (oper,X)  C1    2     6;
                    (indirect),Y  CMP (oper),Y  D1    2     5*
            */

            this.mem.setFlag('C', (byte)(this.mem.ac.unsigned() >= oper.unsigned() ? 1 : 0));
            this.mem.setFlag('Z', (byte)(this.mem.ac.unsigned() == oper.unsigned() ? 1 : 0));
            this.mem.setFlag('N', (new PureByte(this.mem.ac.unsigned() - oper.unsigned())).getBit(7));

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int CPX(PureByte oper, string mode, int pageChange)
        {
            /*
            CPX  Compare Memory && Index X
                    X - M                            N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     CPX #oper     E0    2     2;
                    zeropage      CPX oper      E4    2     3;
                    absolute      CPX oper      EC    3     4;
            */

            this.mem.setFlag('C', (byte)(this.mem.x.unsigned() >= oper.unsigned() ? 1 : 0));
            this.mem.setFlag('Z', (byte)(this.mem.x.unsigned() == oper.unsigned() ? 1 : 0));
            this.mem.setFlag('N', (new PureByte(this.mem.x.unsigned() - oper.unsigned())).getBit(7));

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "abs": return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int CPY(PureByte oper, string mode, int pageChange)
        {
            /*
            CPY  Compare Memory && Index Y
                    Y - M                            N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     CPY #oper     C0    2     2;
                    zeropage      CPY oper      C4    2     3;
                    absolute      CPY oper      CC    3     4;
            */

            this.mem.setFlag('C', (byte)(this.mem.y.unsigned() >= oper.unsigned() ? 1 : 0));
            this.mem.setFlag('Z', (byte)(this.mem.y.unsigned() == oper.unsigned() ? 1 : 0));
            this.mem.setFlag('N', (new PureByte(this.mem.y.unsigned() - oper.unsigned())).getBit(7));

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "abs": return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int DEC(PureByte oper, string mode, int pageChange)
        {
            /*
            DEC  Decrement Memory by One
                    M - 1 -> M                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      DEC oper      C6    2     5;
                    zeropage,X    DEC oper,X    D6    2     6;
                    absolute      DEC oper      CE    3     6;
                    absolute,X    DEC oper,X    DE    3     7;
            */

            oper.decr();
            this.mem.setNZ(oper);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int DEX(PureByte oper, string mode, int pageChange)
        {
            /*
            DEX  Decrement Index X by One
                    X - 1 -> X                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       DEC           CA    1     2;
            */

            this.mem.x.decr();
            this.mem.setNZ(this.mem.x);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int DEY(PureByte oper, string mode, int pageChange)
        {
            /*
            DEY  Decrement Index Y by One
                    Y - 1 -> Y                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       DEC           88    1     2;
            */

            this.mem.y.decr();
            this.mem.setNZ(this.mem.y);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int EOR(PureByte oper, string mode, int pageChange)
        {
            /*
            EOR  Exclusive-OR Memory with Accumulator
                    A EOR M -> A                     N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     EOR #oper     49    2     2;
                    zeropage      EOR oper      45    2     3;
                    zeropage,X    EOR oper,X    55    2     4;
                    absolute      EOR oper      4D    3     4;
                    absolute,X    EOR oper,X    5D    3     4*
                    absolute,Y    EOR oper,Y    59    3     4*
                    (indirect,X)  EOR (oper,X)  41    2     6;
                    (indirect),Y  EOR (oper),Y  51    2     5*
            */

            this.mem.ac.xor(oper);
            this.mem.setNZ(this.mem.ac);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int INC(PureByte oper, string mode, int pageChange)
        {
            /*
            INC  Increment Memory by One
                    M + 1 -> M                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      INC oper      E6    2     5;
                    zeropage,X    INC oper,X    F6    2     6;
                    absolute      INC oper      EE    3     6;
                    absolute,X    INC oper,X    FE    3     7;
            */

            oper.incr();
            this.mem.setNZ(oper);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int INX(PureByte oper, string mode, int pageChange)
        {
            /*
            INX  Increment Index X by One
                    X + 1 -> X                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       INX           E8    1     2;
            */

            this.mem.x.incr();
            this.mem.setNZ(this.mem.x);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int INY(PureByte oper, string mode, int pageChange)
        {
            /*
            INY  Increment Index Y by One
                    Y + 1 -> Y                       N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       INY           C8    1     2;
            */

            this.mem.y.incr();
            this.mem.setNZ(this.mem.y);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int LDA(PureByte oper, string mode, int pageChange)
        {
            /*
            LDA  Load Accumulator with Memory
                    M -> A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     LDA #oper     A9    2     2;
                    zeropage      LDA oper      A5    2     3;
                    zeropage,X    LDA oper,X    B5    2     4;
                    absolute      LDA oper      AD    3     4;
                    absolute,X    LDA oper,X    BD    3     4*
                    absolute,Y    LDA oper,Y    B9    3     4*
                    (indirect,X)  LDA (oper,X)  A1    2     6;
                    (indirect),Y  LDA (oper),Y  B1    2     5*
            */

            this.mem.ac.set(oper);
            this.mem.setNZ(this.mem.ac);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LDX(PureByte oper, string mode, int pageChange)
        {
            /*
            LDX  Load Index X with Memory
                    M -> X                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     LDX #oper     A2    2     2;
                    zeropage      LDX oper      A6    2     3;
                    zeropage,Y    LDX oper,Y    B6    2     4;
                    absolute      LDX oper      AE    3     4;
                    absolute,Y    LDX oper,Y    BE    3     4*
            */

            this.mem.x.set(oper);
            this.mem.setNZ(this.mem.x);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,Y": return 4;
                case "abs": return 4;
                case "abs,Y": return 4 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LDY(PureByte oper, string mode, int pageChange)
        {
            /*
            LDY  Load Index Y with Memory
                    M -> Y                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     LDY #oper     A0    2     2;
                    zeropage      LDY oper      A4    2     3;
                    zeropage,X    LDY oper,X    B4    2     4;
                    absolute      LDY oper      AC    3     4;
                    absolute,X    LDY oper,X    BC    3     4*
            */

            this.mem.y.set(oper);
            this.mem.setNZ(this.mem.y);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LSR(PureByte oper, string mode, int pageChange)
        {
            /*
            LSR  Shift One Bit Right (Memory || Accumulator);
                0 -> [76543210] -> C             N Z C I D V
                                                0 + + - - -
                addressing    assembler    opc  bytes  cyles
                --------------------------------------------
                accumulator   LSR A         4A    1     2;
                zeropage      LSR oper      46    2     5;
                zeropage,X    LSR oper,X    56    2     6;
                absolute      LSR oper      4E    3     6;
                absolute,X    LSR oper,X    5E    3     7;
            */

            byte c = oper.getBit(0);
            oper.rshift();
            this.mem.setNZ(oper);
            this.mem.setFlag('C', c);

            switch (mode)
            {
                case "A": return 2;
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int NOP(PureByte oper, string mode, int pageChange)
        {
            /*
            NOP  No Operation
                    ---                              N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       NOP           EA    1     2;
            */

            switch (mode)
            {
                case "impl": return 2;
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "zpg,Y": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ORA(PureByte oper, string mode, int pageChange)
        {
            /*
            ORA  OR Memory with Accumulator
                    A OR M -> A                      N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     ORA #oper     09    2     2;
                    zeropage      ORA oper      05    2     3;
                    zeropage,X    ORA oper,X    15    2     4;
                    absolute      ORA oper      0D    3     4;
                    absolute,X    ORA oper,X    1D    3     4*
                    absolute,Y    ORA oper,Y    19    3     4*
                    (indirect,X)  ORA (oper,X)  01    2     6;
                    (indirect),Y  ORA (oper),Y  11    2     5*
            */

            this.mem.ac.or(oper);
            this.mem.setNZ(this.mem.ac);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int PHA(PureByte oper, string mode, int pageChange)
        {
            /*
            PHA  Push Accumulator on Stack
                    push A                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PHA           48    1     3;
            */

            this.mem.push(this.mem.ac);

            if ("impl".Equals(mode))
            {
                return 3;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int PHP(PureByte oper, string mode, int pageChange)
        {
            /*
            PHP  Push Processor Status on Stack
                    push SR                          N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PHP           08    1     3;
            */

            this.mem.push(new PureByte(this.mem.sr.unsigned() | 0b00100000));

            if ("impl".Equals(mode))
            {
                return 3;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int PLA(PureByte oper, string mode, int pageChange)
        {
            /*
            PLA  Pull Accumulator from Stack
                    pull A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PLA           68    1     4;
            */

            this.mem.ac.set(this.mem.pull());
            this.mem.setNZ(this.mem.ac);

            if ("impl".Equals(mode))
            {
                return 4;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int PLP(PureByte oper, string mode, int pageChange)
        {
            /*
            PLP  Pull Processor Status from Stack
                    pull SR                          N Z C I D V
                                                    from stack
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       PLP           28    1     4;
            */

            this.mem.sr.set((this.mem.pull().unsigned() | 0x20) & 0xef);

            if ("impl".Equals(mode))
            {
                return 4;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int ROL(PureByte oper, string mode, int pageChange)
        {
            /*
            ROL  Rotate One Bit Left (Memory || Accumulator);
                    C <- [76543210] <- C             N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    accumulator   ROL A         2A    1     2;
                    zeropage      ROL oper      26    2     5;
                    zeropage,X    ROL oper,X    36    2     6;
                    absolute      ROL oper      2E    3     6;
                    absolute,X    ROL oper,X    3E    3     7;
            */

            byte c = oper.rol(this.mem.getFlag('C'));
            this.mem.setFlag('C', c);
            this.mem.setNZ(oper);

            switch (mode)
            {
                case "A": return 2;
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ROR(PureByte oper, string mode, int pageChange)
        {
            /*
            ROR  Rotate One Bit Right (Memory || Accumulator);
                    C -> [76543210] -> C             N Z C I D V
                                                    + + + - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    accumulator   ROR A         6A    1     2;
                    zeropage      ROR oper      66    2     5;
                    zeropage,X    ROR oper,X    76    2     6;
                    absolute      ROR oper      6E    3     6;
                    absolute,X    ROR oper,X    7E    3     7;
            */
            
            byte c = oper.ror(this.mem.getFlag('C'));
            this.mem.setFlag('C', c);
            this.mem.setNZ(oper);

            switch (mode)
            {
                case "A": return 2;
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int RTI(PureByte oper, string mode, int pageChange)
        {
            /*
                RTI  Return from Interrupt
                    pull SR, pull PC                 N Z C I D V
                                                    from stack
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       RTI           40    1     6;
                */

            this.mem.sr.set((this.mem.pull().unsigned() | 0x20) & 0xef);
            this.mem.pc[1].set(this.mem.pull());
            this.mem.pc[0].set(this.mem.pull());

            if ("impl".Equals(mode))
            {
                return 6;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");

        }

        private int RTS(PureByte oper, string mode, int pageChange)
        {
            /*
            RTS  Return from Subroutine
                    pull PC, PC+1 -> PC              N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       RTS           60    1     6;
            */

            this.mem.pc[1].set(this.mem.pull());
            this.mem.pc[0].set(this.mem.pull());

            this.mem.incrPc();

            if ("impl".Equals(mode))
            {
                return 6;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SBC(PureByte oper, string mode, int pageChange)
        {
            /*
            SBC  Subtract Memory from Accumulator with Borrow
                    A - M - C -> A                   N Z C I D V
                                                    + + + - - +
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    immidiate     SBC #oper     E9    2     2;
                    zeropage      SBC oper      E5    2     3;
                    zeropage,X    SBC oper,X    F5    2     4;
                    absolute      SBC oper      ED    3     4;
                    absolute,X    SBC oper,X    FD    3     4*
                    absolute,Y    SBC oper,Y    F9    3     4*
                    (indirect,X)  SBC (oper,X)  E1    2     6;
                    (indirect),Y  SBC (oper),Y  F1    2     5*
            */
            this.ADC(new PureByte(oper.unsigned() ^ 0xff), mode, pageChange);

            switch (mode)
            {
                case "#": return 2;
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 4 + pageChange;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SEC(PureByte oper, string mode, int pageChange)
        {
            /*
            SEC  Set Carry Flag
                    1 -> C                           N Z C I D V
                                                    - - 1 - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       SEC           38    1     2;
            */

            this.mem.setFlag('C', (byte)1);

            switch (mode)
            {
                case "impl": return 2;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SED(PureByte oper, string mode, int pageChange)
        {
            /*
            SED  Set Decimal Flag
                    1 -> D                           N Z C I D V
                                                    - - - - 1 -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       SED           F8    1     2;
            */

            this.mem.setFlag('D', (byte)1);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SEI(PureByte oper, string mode, int pageChange)
        {
            /*
            SEI  Set Interrupt Disable Status
                    1 -> I                           N Z C I D V
                                                    - - - 1 - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       SEI           78    1     2;
            */

            this.mem.setFlag('I', (byte)1);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int STA(PureByte oper, string mode, int pageChange)
        {
            /*
            STA  Store Accumulator in Memory
                    A -> M                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      STA oper      85    2     3;
                    zeropage,X    STA oper,X    95    2     4;
                    absolute      STA oper      8D    3     4;
                    absolute,X    STA oper,X    9D    3     5;
                    absolute,Y    STA oper,Y    99    3     5;
                    (indirect,X)  STA (oper,X)  81    2     6;
                    (indirect),Y  STA (oper),Y  91    2     6;
            */

            oper.set(this.mem.ac);

            switch (mode)
            {
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                case "abs,X": return 5;
                case "abs,Y": return 5;
                case "X,ind": return 6;
                case "ind,Y": return 6;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int STX(PureByte oper, string mode, int pageChange)
        {
            /*
            STX  Store Index X in Memory
                    X -> M                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      STX oper      86    2     3;
                    zeropage,Y    STX oper,Y    96    2     4;
                    absolute      STX oper      8E    3     4;
            */

            oper.set(this.mem.x);

            switch (mode)
            {
                case "zpg": return 3;
                case "zpg,Y": return 4;
                case "abs": return 4;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int STY(PureByte oper, string mode, int pageChange)
        {
            /*
            STY  Sore Index Y in Memory
                    Y -> M                           N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    zeropage      STY oper      84    2     3;
                    zeropage,X    STY oper,X    94    2     4;
                    absolute      STY oper      8C    3     4;
            */

            oper.set(this.mem.y);

            switch (mode)
            {
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "abs": return 4;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int TAX(PureByte oper, string mode, int pageChange)
        {
            /*
            TAX  Transfer Accumulator to Index X
                    A -> X                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TAX           AA    1     2;
            */

            this.mem.x.set(this.mem.ac);
            this.mem.setNZ(this.mem.x);

            switch (mode)
            {
                case "impl": return 2;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int TAY(PureByte oper, string mode, int pageChange)
        {
            /*
            TAY  Transfer Accumulator to Index Y
                    A -> Y                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TAY           A8    1     2;
            */

            this.mem.y.set(this.mem.ac);
            this.mem.setNZ(this.mem.y);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int TSX(PureByte oper, string mode, int pageChange)
        {
            /*
            TSX  Transfer Stack Pointer to Index X
                    SP -> X                          N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TSX           BA    1     2;
            */

            this.mem.x.set(this.mem.sp);
            this.mem.setNZ(this.mem.x);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int TXA(PureByte oper, string mode, int pageChange)
        {
            /*
            TXA  Transfer Index X to Accumulator
                    X -> A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TXA           8A    1     2;
            */

            this.mem.ac.set(this.mem.x);
            this.mem.setNZ(this.mem.ac);

            switch (mode)
            {
                case "impl": return 2;
                case "internal": return 0;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int TXS(PureByte oper, string mode, int pageChange)
        {
            /*
            TXS  Transfer Index X to Stack Register
                    X -> SP                          N Z C I D V
                                                    - - - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TXS           9A    1     2;
            */

            this.mem.sp.set(this.mem.x);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int TYA(PureByte oper, string mode, int pageChange)
        {
            /*
            TYA  Transfer Index Y to Accumulator
                    Y -> A                           N Z C I D V
                                                    + + - - - -
                    addressing    assembler    opc  bytes  cyles
                    --------------------------------------------
                    implied       TYA           98    1     2;
            */

            this.mem.ac.set(this.mem.y);
            this.mem.setNZ(this.mem.ac);

            if ("impl".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SLO(PureByte oper, string mode, int pageChange)
        {
            /*
            ASO    ***    (SLO);
            This opcode ASLs the contents of a memory location && then ORs the result
            with the accumulator.

            Supported modes:

            ASO abcd        ;0F cd ab    ;No. Cycles= 6;
            ASO abcd,X      ;1F cd ab    ;            7;
            ASO abcd,Y      ;1B cd ab    ;            7;
            ASO ab          ;07 ab       ;            5;
            ASO ab,X        ;17 ab       ;            6;
            ASO (ab,X)      ;03 ab       ;            8;
            ASO (ab),Y      ;13 ab       ;            8;

            (Sub-instructions: ORA, ASL);

            Here is an example of how you might use this opcode:

            ASO $C010       ;0F 10 C0;

            Here is the same code using equivalent instructions.

            ASL $C010;
            ORA $C010;
            */

            this.ASL(oper, "internal", 0);
            this.ORA(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "abs,Y": return 7;
                case "X,ind": return 8;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int RLA(PureByte oper, string mode, int pageChange)
        {
            /*
            RLA    ***
            RLA ROLs the contents of a memory location && then ANDs the result with
            the accumulator.

            Supported modes:

            RLA abcd        ;2F cd ab    ;No. Cycles= 6;
            RLA abcd,X      ;3F cd ab    ;            7;
            RLA abcd,Y      ;3B cd ab    ;            7;
            RLA ab          ;27 ab       ;            5;
            RLA ab,X        ;37 ab       ;            6;
            RLA (ab,X)      ;23 ab       ;            8;
            RLA (ab),Y      ;33 ab       ;            8;

            (Sub-instructions: AND, ROL);

            Here's an example of how you might write it in a program.

            RLA $FC,X       ;37 FC

            Here's the same code using equivalent instructions.

            ROL $FC,X
            AND $FC,X
            */

            this.ROL(oper, "internal", 0);
            this.AND(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "abs,Y": return 7;
                case "X,ind": return 8;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SRE(PureByte oper, string mode, int pageChange)
        {
            /*
            LSE    ***   (SRE);
            LSE LSRs the contents of a memory location && then EORs the result with
            the accumulator.

            Supported modes:

            LSE abcd        ;4F cd ab    ;No. Cycles= 6;
            LSE abcd,X      ;5F cd ab    ;            7;
            LSE abcd,Y      ;5B cd ab    ;            7;
            LSE ab          ;47 ab       ;            5;
            LSE ab,X        ;57 ab       ;            6;
            LSE (ab,X)      ;43 ab       ;            8;
            LSE (ab),Y      ;53 ab       ;            8;

            (Sub-instructions: EOR, LSR);

            Example:

            LSE $C100,X     ;5F 00 C1;

            Here's the same code using equivalent instructions.

            LSR $C100,X
            EOR $C100,X
            */

            this.LSR(oper, "internal", 0);
            this.EOR(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "abs,Y": return 7;
                case "X,ind": return 8;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int RRA(PureByte oper, string mode, int pageChange)
        {
            /*
            RRA    ***
            RRA RORs the contents of a memory location && then ADCs the result with
            the accumulator.

            Supported modes:

            RRA abcd        ;6F cd ab    ;No. Cycles= 6;
            RRA abcd,X      ;7F cd ab    ;            7;
            RRA abcd,Y      ;7B cd ab    ;            7;
            RRA ab          ;67 ab       ;            5;
            RRA ab,X        ;77 ab       ;            6;
            RRA (ab,X)      ;63 ab       ;            8;
            RRA (ab),Y      ;73 ab       ;            8;

            (Sub-instructions: ADC, ROR);

            Example:

            RRA $030C       ;6F 0C 03;

            Equivalent instructions:

            ROR $030C
            ADC $030C
            */
         
            this.ROR(oper, "internal", 0);
            this.ADC(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "abs,Y": return 7;
                case "X,ind": return 8;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SAX(PureByte oper, string mode, int pageChange)
        {
            /*
            AXS    ***    (SAX);
            AXS ANDs the contents of the A && X registers (without changing the
            contents of either register) && stores the result in memory.
            AXS does not affect any flags in the processor status register.

            Supported modes:

            AXS abcd        ;8F cd ab    ;No. Cycles= 4;
            AXS ab          ;87 ab       ;            3;
            AXS ab,Y        ;97 ab       ;            4;
            AXS (ab,X)      ;83 ab       ;            6;

            (Sub-instructions: STA, STX);

            Example:

            AXS $FE         ;87 FE

            Here's the same code using equivalent instructions.

            STX $FE
            PHA
            AND $FE
            STA $FE
            PLA
            */

            oper.set(this.mem.ac.get() & this.mem.x.get());

            switch (mode)
            {
                case "zpg": return 4;
                case "zpg,Y": return 3;
                case "abs": return 4;
                case "X,ind": return 6;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int LAX(PureByte oper, string mode, int pageChange)
        {
            /*
            LAX    ***
            This opcode loads both the accumulator && the X register with the contents
            of a memory location.

            Supported modes:

            LAX abcd        ;AF cd ab    ;No. Cycles= 4;
            LAX abcd,Y      ;BF cd ab    ;            4*
            LAX ab          ;A7 ab       ;*=add 1     3;
            LAX ab,Y        ;B7 ab       ;if page     4;
            LAX (ab,X)      ;A3 ab       ;boundary    6;
            LAX (ab),Y      ;B3 ab       ;is crossed  5*

            (Sub-instructions: LDA, LDX);

            Example:

            LAX $8400,Y     ;BF 00 84;

            Equivalent instructions:

            LDA $8400,Y
            LDX $8400,Y
            */

            this.LDA(oper, "internal", 0);
            this.LDX(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 3;
                case "zpg,Y": return 4;
                case "abs": return 4;
                case "abs,Y": return 4 + pageChange;
                case "X,ind": return 6;
                case "ind,Y": return 5 + pageChange;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int DCP(PureByte oper, string mode, int pageChange)
        {
            /*
            DCM    ***    (DCP);
            This opcode DECs the contents of a memory location && then CMPs the result
            with the A register.

            Supported modes:

            DCM abcd        ;CF cd ab    ;No. Cycles= 6;
            DCM abcd,X      ;DF cd ab    ;            7;
            DCM abcd,Y      ;DB cd ab    ;            7;
            DCM ab          ;C7 ab       ;            5;
            DCM ab,X        ;D7 ab       ;            6;
            DCM (ab,X)      ;C3 ab       ;            8;
            DCM (ab),Y      ;D3 ab       ;            8;

            (Sub-instructions: CMP, DEC);

            Example:

            DCM $FF         ;C7 FF

            Equivalent instructions:

            DEC $FF
            CMP $FF
            */

            this.DEC(oper, "internal", 0);
            this.CMP(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "abs,Y": return 7;
                case "X,ind": return 8;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ISC(PureByte oper, string mode, int pageChange)
        {
            /*
            INS    ***    (ISC);
            This opcode INCs the contents of a memory location && then SBCs the result
            from the A register.

            Supported modes:

            INS abcd        ;EF cd ab    ;No. Cycles= 6;
            INS abcd,X      ;FF cd ab    ;            7;
            INS abcd,Y      ;FB cd ab    ;            7;
            INS ab          ;E7 ab       ;            5;
            INS ab,X        ;F7 ab       ;            6;
            INS (ab,X)      ;E3 ab       ;            8;
            INS (ab),Y      ;F3 ab       ;            8;

            (Sub-instructions: SBC, INC);

            Example:

            INS $FF         ;E7 FF

            Equivalent instructions:

            INC $FF
            SBC $FF
            */

            this.INC(oper, "internal", 0);
            this.SBC(oper, "internal", 0);

            switch (mode)
            {
                case "zpg": return 5;
                case "zpg,X": return 6;
                case "abs": return 6;
                case "abs,X": return 7;
                case "abs,Y": return 7;
                case "X,ind": return 8;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int ALR(PureByte oper, string mode, int pageChange)
        {
            /*
            ALR    ***
            This opcode ANDs the contents of the A register with an immediate value and
            then LSRs the result.

            One supported mode:

            ALR #ab         ;4B ab       ;No. Cycles= 2;

            Example:

            ALR #$FE        ;4B FE

            Equivalent instructions:

            AND #$FE
            LSR A
            */

            this.AND(oper, "internal", 0);
            this.LSR(this.mem.ac, "A", 0);

            if ("#".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int ARR(PureByte oper, string mode, int pageChange)
        {
            /*
            ARR    ***
            This opcode ANDs the contents of the A register with an immediate value and
            then RORs the result.

            One supported mode:

            ARR #ab         ;6B ab       ;No. Cycles= 2;

            Here's an example of how you might write it in a program.

            ARR #$7F        ;6B 7F

            Here's the same code using equivalent instructions.

            AND #$7F
            ROR A
            */

            this.AND(oper, "internal", 0);
            this.ROR(this.mem.ac, "A", 0);

            if ("#".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int XAA(PureByte oper, string mode, int pageChange)
        {
            /*
            XAA    ***
            XAA transfers the contents of the X register to the A register && then
            ANDs the A register with an immediate value.

            One supported mode:

            XAA #ab         ;8B ab       ;No. Cycles= 2;

            Example:

            XAA #$44        ;8B 44;

            Equivalent instructions:

            TXA
            AND #$44;
            */

            this.TXA(new PureByte(), "internal", 0);
            this.AND(oper, "internal", 0);

            if ("#".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int OAL(PureByte oper, string mode, int pageChange)
        {
            /*
            OAL    ***
            This opcode ORs the A register with #$EE, ANDs the result with an immediate
            value, && then stores the result in both A && X.

            One supported mode:

            OAL #ab         ;AB ab       ;No. Cycles= 2;

            Here's an example of how you might use this opcode:

            OAL #$AA        ;AB AA

            Here's the same code using equivalent instructions:

            ORA #$EE
            AND #$AA
            TAX
            */

            this.ORA(new PureByte(0xee), "#", 0);
            this.AND(oper, "internal", 0);
            this.TAX(new PureByte(), "internal", 0);

            if ("#".Equals(mode))
            {
                return 2;
            }
            throw new Exception("mode '" + mode + "' invalid for instruction");
        }

        private int SKB(PureByte oper, string mode, int pageChange)
        {
            /*
            SKB stands for skip next byte.
            Opcodes: 80, 82, C2, E2, 04, 14, 34, 44, 54, 64, 74, D4, F4.
            Takes 2, 3, || 4 cycles to execute.
            */

            this.mem.incrPc();

            switch (mode)
            {
                case "zpg": return 3;
                case "zpg,X": return 4;
                case "#": return 6;
                case "ind,Y": return 8;
                default: throw new Exception("mode '" + mode + "' invalid for instruction");
            };
        }

        private int SKW(PureByte oper, string mode, int pageChange)
        {
            /*
            SKW skips next word (two bytes).
            Opcodes: 0C, 1C, 3C, 5C, 7C, DC, FC.
            Takes 4 cycles to execute.
            */

            this.mem.incrPc(2);

            return 4;
        }

    }
}
