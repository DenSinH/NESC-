using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

using Newtonsoft.Json;
using NLog;


namespace NesEmulator
{

    class InstructionCaller
    {
        // wrapper for an instruction with mode

        private Func<byte, int> method;
        public byte mode;
        /*
        Modes:
        0: A        ....	Accumulator	 	        OPC A	 	    operand is AC (implied single byte instruction)
        1: abs      ....	absolute	 	        OPC $LLHH	 	operand is address $HHLL *
        2: abs,X    ....	absolute, X-indexed	 	OPC $LLHH,X	 	operand is address; effective address is address incremented by X with carry **
        3: abs,Y    ....	absolute, Y-indexed	 	OPC $LLHH,Y	 	operand is address; effective address is address incremented by Y with carry **
        4: #	    ....	immediate	 	        OPC #$BB	 	operand is byte BB
        5: impl	    ....	implied	 	            OPC	 	        operand implied
        6: ind	    ....	indirect	 	        OPC ($LLHH)	 	operand is address; effective address is contents of word at address: C.w($HHLL)
        7: X,ind    ....	X-indexed, indirect	 	OPC ($LL,X)	 	operand is zeropage address; effective address is word in (LL + X, LL + X + 1), inc. without carry: C.w($00LL + X)
        8: ind,Y    ....	indirect, Y-indexed	 	OPC ($LL),Y	 	operand is zeropage address; effective address is word in (LL, LL + 1) incremented by Y with carry: C.w($00LL) + Y
        9: rel      ....	relative	 	        OPC $BB	 	    branch target is PC + signed offset BB ***
        10: zpg     ....	zeropage	 	        OPC $LL	 	    operand is zeropage address (hi-byte is zero, address = $00LL)
        11: zpg,X   ....	zeropage, X-indexed	 	OPC $LL,X	 	operand is zeropage address; effective address is address incremented by X without carry **
        12: zpg,Y   ....	zeropage, Y-indexed	 	OPC $LL,Y	 	operand is zeropage address; effective address is address incremented by Y without carry **
        
        13: internal
        */

        public InstructionCaller(Func<byte, int> method, byte mode)
        {
            this.method = method;
            this.mode = mode;
        }

        public int call()
        {
            return this.method(this.mode);
        }

        public string getName()
        {
            return this.method.Method.Name;
        }

    }

    partial class CPU
    {
        private const bool makeLog = true;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CPUMEM mem;
        private int cycle;
        private bool kil = false;

        private int oper;
        private byte pageChange;

        private InstructionCaller[] instructions;

        public CPU()
        {
            // initialize memory
            this.mem = new CPUMEM();
            this.cycle = 7; // todo: cpu boot sequence

            // load opcodes

            string opcodeJson = File.ReadAllText("../../data/AllOpcodes.json");
            Dictionary<string, string> opcodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(opcodeJson);

            this.instructions = new InstructionCaller[0x100];

            foreach (KeyValuePair<string, string> entry in opcodes)
            {
                string[] instructionString = entry.Value.Split(' ');
                Func<byte, int> instruction;

                switch (instructionString[0])
                {
                    case "ADC": instruction = this.ADC; break;
                    case "ALR": instruction = this.ALR; break;
                    case "AND": instruction = this.AND; break;
                    case "ARR": instruction = this.ARR; break;
                    case "ASL": instruction = this.ASL; break;
                    case "BCC": instruction = this.BCC; break;
                    case "BCS": instruction = this.BCS; break;
                    case "BEQ": instruction = this.BEQ; break;
                    case "BIT": instruction = this.BIT; break;
                    case "BMI": instruction = this.BMI; break;
                    case "BNE": instruction = this.BNE; break;
                    case "BPL": instruction = this.BPL; break;
                    case "BRK": instruction = this.BRK; break;
                    case "BVC": instruction = this.BVC; break;
                    case "BVS": instruction = this.BVS; break;
                    case "CLC": instruction = this.CLC; break;
                    case "CLD": instruction = this.CLD; break;
                    case "CLI": instruction = this.CLI; break;
                    case "CLV": instruction = this.CLV; break;
                    case "CMP": instruction = this.CMP; break;
                    case "CPX": instruction = this.CPX; break;
                    case "CPY": instruction = this.CPY; break;
                    case "DCP": instruction = this.DCP; break;
                    case "DEC": instruction = this.DEC; break;
                    case "DEX": instruction = this.DEX; break;
                    case "DEY": instruction = this.DEY; break;
                    case "EOR": instruction = this.EOR; break;
                    case "INC": instruction = this.INC; break;
                    case "INX": instruction = this.INX; break;
                    case "INY": instruction = this.INY; break;
                    case "ISC": instruction = this.ISC; break;
                    case "LAX": instruction = this.LAX; break;
                    case "LDA": instruction = this.LDA; break;
                    case "LDX": instruction = this.LDX; break;
                    case "LDY": instruction = this.LDY; break;
                    case "LSR": instruction = this.LSR; break;
                    case "NOP": instruction = this.NOP; break;
                    case "ORA": instruction = this.ORA; break;
                    case "PHA": instruction = this.PHA; break;
                    case "PHP": instruction = this.PHP; break;
                    case "PLA": instruction = this.PLA; break;
                    case "PLP": instruction = this.PLP; break;
                    case "RLA": instruction = this.RLA; break;
                    case "ROL": instruction = this.ROL; break;
                    case "ROR": instruction = this.ROR; break;
                    case "RRA": instruction = this.RRA; break;
                    case "RTI": instruction = this.RTI; break;
                    case "RTS": instruction = this.RTS; break;
                    case "SAX": instruction = this.SAX; break;
                    case "SBC": instruction = this.SBC; break;
                    case "SEC": instruction = this.SEC; break;
                    case "SED": instruction = this.SED; break;
                    case "SEI": instruction = this.SEI; break;
                    case "SLO": instruction = this.SLO; break;
                    case "SRE": instruction = this.SRE; break;
                    case "STA": instruction = this.STA; break;
                    case "STX": instruction = this.STX; break;
                    case "STY": instruction = this.STY; break;
                    case "TAX": instruction = this.TAX; break;
                    case "TAY": instruction = this.TAY; break;
                    case "TSX": instruction = this.TSX; break;
                    case "TXA": instruction = this.TXA; break;
                    case "TXS": instruction = this.TXS; break;
                    case "TYA": instruction = this.TYA; break;
                    case "XAA": instruction = this.XAA; break;
                    case "JSR":
                    case "JMP":
                        instruction = null; break;
                    default: throw new Exception("Unknown instruction: " + instructionString[0]);
                }

                byte mode;
                switch (instructionString[1])
                {
                    case "A": mode = 0; break;
                    case "abs": mode = 1; break;
                    case "abs,X": mode = 2; break;
                    case "abs,Y": mode = 3; break;
                    case "#": mode = 4; break;
                    case "impl": mode = 5; break;
                    case "ind": mode = 6; break;
                    case "X,ind": mode = 7; break;
                    case "ind,Y": mode = 8; break;
                    case "rel": mode = 9; break;
                    case "zpg": mode = 10; break;
                    case "zpg,X": mode = 11; break;
                    case "zpg,Y": mode = 12; break;
                    default: throw new Exception("Unknown mode: " + instructionString[1]);
                }

                if (instruction != null)
                {
                    this.instructions[int.Parse(entry.Key, NumberStyles.HexNumber)] = new InstructionCaller(instruction, mode);
                }
            }

        }

        private void log(string message)
        {
            // for logging results
            logger.Debug(message);
            Console.WriteLine(message);
        }

        public void RESET()
        {
            this.mem.push(this.mem[0x100]);
            this.mem.push(this.mem[0x1ff]);
            this.mem.push(this.mem[0x1fe]);
            this.mem.setPc(
                this.mem[this.mem.resetVector[0]],
                this.mem[this.mem.resetVector[1]]
                );
            Console.WriteLine(this.mem.pc[0].ToString("x2") + this.mem.pc[1].ToString("x2"));
            this.cycle = 7;
        }

        public void IRQ()
        {
            this.mem.push(this.mem.pc[0]);
            this.mem.push(this.mem.pc[1]);
            this.mem.push((byte)((this.mem.sr & 0b1110_1111) + 0b0010_0000));
            this.mem.setPc(this.mem[this.mem.irqVector[0]], this.mem[this.mem.irqVector[1]]);
        }

        public void NMI()
        {
            this.mem.push(this.mem.pc[0]);
            this.mem.push(this.mem.pc[1]);
            this.mem.push((byte)((this.mem.sr & 0b1110_1111) + 0b0010_0000));
            this.mem.setPc(this.mem[this.mem.nmiVector[0]], this.mem[this.mem.nmiVector[1]]);
        }

        public int GetCycle()
        {
            return this.cycle;
        }

        public void SetPc(int val)
        {
            this.mem.pc[0] = (byte)(val / 0x100);
            this.mem.pc[1] = (byte)(val % 0x100);
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
            int opcode = this.mem.getCurrent();
            this.mem.incrPc();

            /*
            * JMP and JSR instructions work slightly differently,
            * as we need to operate on 2 bytes instead of a memory address
            */

            if (opcode == 0x4c || opcode == 0x6c)
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
                byte ll = this.mem.getCurrent();
                this.mem.incrPc();
                byte hh = this.mem.getCurrent();
                this.mem.incrPc();

                switch (opcode)
                {
                    case 0x4c:
                        {
                            this.mem.setPc(ll, hh);
                            return 3;
                        }
                    case 0x6c:
                        {
                            // indirect wraps around with the lower byte. This is a glitch/feature in the MOS6502 processor
                            this.mem.setPc(
                                    this.mem[hh * 0x100 + ll],
                                    this.mem[hh * 0x100 + ((ll + 1) % 0x100)]
                            );
                            return 5;
                        }
                    default: break;
                }
            }
            else if (opcode == 0x20)
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
                byte v = (byte)(this.mem.pc[1] + 1 > 0xff ? 1 : 0);
                this.mem.push((byte)(this.mem.pc[0] + v));
                this.mem.push((byte)(this.mem.pc[1] + 1));

                byte ll = this.mem.getCurrent();
                this.mem.incrPc();
                byte hh = this.mem.getCurrent();
                this.mem.setPc(ll, hh);
                return 6;
            }

            InstructionCaller ic = this.instructions[opcode];

            if (ic == null)
            {
                // Unknown opcode is interpreted as KIL instruction
                this.kil = true;
                return 0;
            }
            if (makeLog)
            {
                this.log(
                    string.Format(
                        "    {0}  {1:x2} {2}: {3,5}\t\t{4}     A:{5:x2} X:{6:x2} Y:{7:x2} P:{8:x2} SP:{9:x2}        CYC:{10}",
                        (this.mem.getPc() - 1).ToString("x2"),
                        opcode,
                        ic.getName(), // instruction,
                        ic.mode, // mode,
                        this.mem.getCurrent().ToString("x2"),
                        this.mem.ac,
                        this.mem.x,
                        this.mem.y,
                        this.mem.sr,
                        this.mem.sp,
                        this.cycle
                    )
                );
            }

            this.getOper(ic.mode);

            return ic.call();
        }

        private void getOper(int mode)
        {
            byte ll;
            byte hh;

            switch (mode)
            {
                case 0:  // A
                    this.oper = -0x100;
                    this.pageChange = (byte)0;
                    return;
                case 1:  // abs
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = 0x100 * hh + ll;
                    this.pageChange = (byte)0;
                    return;

                case 2:  // abs,X
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = (hh * 0x100 + ll + this.mem.x) % 0x10000;
                    this.pageChange = (byte)(ll + this.mem.x > 0xff ? 1 : 0);
                    return;

                case 3:  // abs,Y
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();
                    hh = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = (hh * 0x100 + ll + this.mem.y) % 0x10000;
                    this.pageChange = (byte)(ll + this.mem.y > 0xff ? 1 : 0);
                    return;

                case 4:  // #
                    this.oper = -0x200 - this.mem.getCurrent();
                    this.mem.incrPc();

                    this.pageChange = (byte)0;
                    return;

                case 5:  // impl
                    this.oper = -1;
                    this.pageChange = (byte)0;
                    return;

                case 6:  // ind, only used by JMP
                    break;

                case 7:  // X,ind
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = this.mem[(ll + this.mem.x) % 0x100] + 0x100 * this.mem[(ll + this.mem.x + 1) % 0x100];
                    this.pageChange = (byte)0;
                    return;

                case 8:  // ind,Y
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    int effective_low = this.mem[ll];
                    int effective_high = this.mem[(ll + 1) % 0x100] * 0x100;
                    this.oper = (effective_high + effective_low + this.mem.y) % 0x10000;
                    
                    this.pageChange = (byte)(effective_low + this.mem.y > 0xff ? 1 : 0);
                    return;

                case 9:  // rel
                    this.oper = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.pageChange = (byte)0;
                    return;

                case 10:  // zpg
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = ll;
                    this.pageChange = (byte)0;
                    return;

                case 11:  // zpg,X
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = (ll + this.mem.x) % 0x100;
                    this.pageChange = (byte)0;
                    return;

                case 12:  // zpg,Y
                    ll = this.mem.getCurrent();
                    this.mem.incrPc();

                    this.oper = (ll + this.mem.y) % 0x100;
                    this.pageChange = (byte)0;
                    return;

                default:
                    throw new Exception("Unknown mode: " + mode);
            }
        }

        private int branch()
        {
            int target = this.mem.pc[1] + unchecked((sbyte)this.oper);
            int c = 0;
            if (target > 0xff)
            {
                c = 1;
            }
            else if (target < 0)
            {
                c = -1;
            }

            this.mem.pc[1] = (byte)target;
            if (c == 1)
            {
                this.mem.pc[0]++;
            } else if (c == -1)
            {
                this.mem.pc[0]--;
            }

            return c;
        }
    }
}
