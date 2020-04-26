using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using NLog;


namespace NesEmulator
{

    partial class CPU
    {

        private const bool makeLog = false;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CPUMEM mem;
        private int cycle;
        private bool kil = false;

        private PureByte oper;
        private byte pageChange;

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

            this.getOper(mode);

            switch (instruction)
            {
                case "ADC": return this.ADC(mode);
                case "ALR": return this.ALR(mode);
                case "AND": return this.AND(mode);
                case "ARR": return this.ARR(mode);
                case "ASL": return this.ASL(mode);
                case "BCC": return this.BCC(mode);
                case "BCS": return this.BCS(mode);
                case "BEQ": return this.BEQ(mode);
                case "BIT": return this.BIT(mode);
                case "BMI": return this.BMI(mode);
                case "BNE": return this.BNE(mode);
                case "BPL": return this.BPL(mode);
                case "BRK": return this.BRK(mode);
                case "BVC": return this.BVC(mode);
                case "BVS": return this.BVS(mode);
                case "CLC": return this.CLC(mode);
                case "CLD": return this.CLD(mode);
                case "CLI": return this.CLI(mode);
                case "CLV": return this.CLV(mode);
                case "CMP": return this.CMP(mode);
                case "CPX": return this.CPX(mode);
                case "CPY": return this.CPY(mode);
                case "DCP": return this.DCP(mode);
                case "DEC": return this.DEC(mode);
                case "DEX": return this.DEX(mode);
                case "DEY": return this.DEY(mode);
                case "EOR": return this.EOR(mode);
                case "INC": return this.INC(mode);
                case "INX": return this.INX(mode);
                case "INY": return this.INY(mode);
                case "ISC": return this.ISC(mode);
                case "LAX": return this.LAX(mode);
                case "LDA": return this.LDA(mode);
                case "LDX": return this.LDX(mode);
                case "LDY": return this.LDY(mode);
                case "LSR": return this.LSR(mode);
                case "NOP": return this.NOP(mode);
                case "ORA": return this.ORA(mode);
                case "PHA": return this.PHA(mode);
                case "PHP": return this.PHP(mode);
                case "PLA": return this.PLA(mode);
                case "PLP": return this.PLP(mode);
                case "RLA": return this.RLA(mode);
                case "ROL": return this.ROL(mode);
                case "ROR": return this.ROR(mode);
                case "RRA": return this.RRA(mode);
                case "RTI": return this.RTI(mode);
                case "RTS": return this.RTS(mode);
                case "SAX": return this.SAX(mode);
                case "SBC": return this.SBC(mode);
                case "SEC": return this.SEC(mode);
                case "SED": return this.SED(mode);
                case "SEI": return this.SEI(mode);
                case "SLO": return this.SLO(mode);
                case "SRE": return this.SRE(mode);
                case "STA": return this.STA(mode);
                case "STX": return this.STX(mode);
                case "STY": return this.STY(mode);
                case "TAX": return this.TAX(mode);
                case "TAY": return this.TAY(mode);
                case "TSX": return this.TSX(mode);
                case "TXA": return this.TXA(mode);
                case "TXS": return this.TXS(mode);
                case "TYA": return this.TYA(mode);
                case "XAA": return this.XAA(mode);
                default: throw new Exception("Unknown instruction: " + mode);
            }
        }

        private void getOper(string mode)
        {
            PureByte ll;
            PureByte hh;

            switch (mode)
            {
                case "impl":
                    this.oper = new PureByte();
                    this.pageChange = (byte)0;
                    return;
                case "A":
                    this.oper = this.mem.ac;
                    this.pageChange = (byte)0;
                    return;
                case "abs":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get(ll, hh);
                    this.pageChange = (byte)0;
                    return;

                case "abs,X":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get((hh.unsigned() * 0x100 + ll.unsigned() + this.mem.x.unsigned()) % 0x10000);
                    this.pageChange = (byte)(ll.unsigned() + this.mem.x.unsigned() > 0xff ? 1 : 0);
                    return;

                case "abs,Y":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get((hh.unsigned() * 0x100 + ll.unsigned() + this.mem.y.unsigned()) % 0x10000);
                    this.pageChange = (byte)(ll.unsigned() + this.mem.y.unsigned() > 0xff ? 1 : 0);
                    return;

                case "#":
                    this.oper = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.pageChange = (byte)0;
                    return;

                case "X,ind":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get(
                                    this.mem.get((ll.unsigned() + this.mem.x.unsigned()) % 0x100),
                                    this.mem.get((ll.unsigned() + this.mem.x.unsigned() + 1) % 0x100)
                            );
                    this.pageChange = (byte)0;
                    return;

                case "ind,Y":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    int effective_low = this.mem.get(ll.unsigned()).unsigned();
                    int effective_high = this.mem.get((ll.unsigned() + 1) % 0x100).unsigned() * 0x100;
                    int effective = (effective_high + effective_low + this.mem.y.unsigned()) % 0x10000;

                    this.oper = this.mem.get(effective);
                    this.pageChange = (byte)(effective_low + this.mem.y.unsigned() > 0xff ? 1 : 0);
                    return;

                case "rel":
                    this.oper = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.pageChange = (byte)0;
                    return;

                case "zpg":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get(ll.unsigned());
                    this.pageChange = (byte)0;
                    return;

                case "zpg,X":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get((ll.unsigned() + this.mem.x.unsigned()) % 0x100);
                    this.pageChange = (byte)0;
                    return;

                case "zpg,Y":
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem.get((ll.unsigned() + this.mem.y.unsigned()) % 0x100);
                    this.pageChange = (byte)0;
                    return;

                default:
                    throw new Exception("Unknown mode: " + mode);
            }
        }

        private int branch()
        {
            int target = this.mem.pc[1].unsigned() + this.oper.signed();
            int c = 0;
            if (target > 0xff)
            {
                c = 1;
            }
            else if (target < 0)
            {
                c = -1;
            }

            this.mem.pc[1].add(this.oper.signed());
            if (c != 0)
            {
                this.mem.pc[0].add(c);
            }

            return c;
        }
    }
}
