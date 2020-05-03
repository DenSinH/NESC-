using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

using Newtonsoft.Json;


namespace NesEmulator
{

    public partial class CPU
    {
        public int cycle;
        private bool kil = false;

        public NES nes;

        private int oper;
        private byte pageChange;

        private InstructionCaller[] instructions;

        public CPU(NES nes)
        {
            // initialize memory
            this.nes = nes;

            this.storage = new byte[0x10000];
            // memory map from https://wiki.nesdev.com/w/index.php/CPU_memory_map
            for (int i = 0; i < 0x10000; i++)
            {
                this.storage[i] = 0;
            }

            this.pc = new byte[2];
            this.pc[0] = new byte();
            this.pc[1] = new byte();

            this.ac = 0;
            this.x = 0;
            this.y = 0;
            this.sr = 0x24;
            this.sp = 0x00;


            this.cycle = 0;

            // load opcodes
            string opcodeJson = File.ReadAllText("../../data/AllOpcodes.json");
            Dictionary<string, string> opcodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(opcodeJson);

            this.instructions = new InstructionCaller[0x100];

            foreach (KeyValuePair<string, string> entry in opcodes)
            {
                string[] instructionString = entry.Value.Split(' ');
                Func<InstructionMode, int> instruction;

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
                    case "JSR": instruction = this.JSR; break;
                    case "JMP": instruction = this.JMP; break;
                    default: throw new Exception("Unknown instruction: " + instructionString[0]);
                }

                InstructionMode mode;
                switch (instructionString[1])
                {
                    case "A": mode = InstructionMode.A; break;
                    case "abs": mode = InstructionMode.abs; break;
                    case "abs,X": mode = InstructionMode.absX; break;
                    case "abs,Y": mode = InstructionMode.absY; break;
                    case "#": mode = InstructionMode.imm; break;
                    case "impl": mode = InstructionMode.impl; break;
                    case "ind": mode = InstructionMode.ind; break;
                    case "X,ind": mode = InstructionMode.Xind; break;
                    case "ind,Y": mode = InstructionMode.indY; break;
                    case "rel": mode = InstructionMode.rel; break;
                    case "zpg": mode = InstructionMode.zpg; break;
                    case "zpg,X": mode = InstructionMode.zpgX; break;
                    case "zpg,Y": mode = InstructionMode.zpgY; break;
                    default: throw new Exception("Unknown mode: " + instructionString[1]);
                }

                if (instruction != null)
                {
                    this.instructions[int.Parse(entry.Key, NumberStyles.HexNumber)] = new InstructionCaller(instruction, mode);
                }
            }
        }

        public string GenLog()
        {
            int opcode = this.getCurrent();
            InstructionCaller ic = this.instructions[opcode];

            return string.Format(
                        "    {0}  {1:x2} {2}: {3,5}\t\t{4:x2} {5:x2}     A:{6:x2} X:{7:x2} Y:{8:x2} P:{9:x2} SP:{10:x2}        CYC:{11}",
                        this.getPc().ToString("x2"),
                        opcode,
                        ic.getName(), // instruction,
                        ic.mode, // mode,
                        this[this.getPc() + 1].ToString("x2"),
                        this[this.getPc() + 2].ToString("x2"),
                        this.ac,
                        this.x,
                        this.y,
                        this.sr,
                        this.sp,
                        this.cycle
                    );
        }

        public void RESET()
        {
            // Interrupt information from https://www.pagetable.com/?p=410
            this.push(this[0x100]);
            this.push(this[0x1ff]);
            this.push(this[0x1fe]);
            this.setPc(
                this[this.resetVector[0]],
                this[this.resetVector[1]]
                );
            this.cycle = 7;
        }

        public int IRQ()
        {
            // Interrupt information from https://www.pagetable.com/?p=410
            this.push(this.pc[0]);
            this.push(this.pc[1]);
            this.push((byte)((this.sr & 0b1110_1111) | 0b0010_0100));
            this.setPc(this[this.irqVector[0]], this[this.irqVector[1]]);

            return 8;
        }

        public int NMI()
        {
            // Interrupt information from https://www.pagetable.com/?p=410
            this.push(this.pc[0]);
            this.push(this.pc[1]);
            this.push((byte)((this.sr & 0b1110_1111) | 0b0010_0100));
            this.setPc(this[this.nmiVector[0]], this[this.nmiVector[1]]);

            return 7;
        }

        public void SetPc(int val)
        {
            this.pc[0] = (byte)(val / 0x100);
            this.pc[1] = (byte)(val % 0x100);
        }

        public void Run()
        {
            while (!this.kil)
            {
                this.cycle += this.Step();
            }
        }

        public int Step()
        {
            int opcode = this.getCurrent();
            this.incrPc();

            InstructionCaller ic = this.instructions[opcode];

            if (ic == null)
            {
                // Unknown opcode is interpreted as KIL instruction
                this.kil = true;
                return 0;
            }

            this.getOper(ic.mode);

            return ic.call();
        }

        private void getOper(InstructionMode mode)
        {
            byte ll;
            byte hh;

            switch (mode)
            {
                case InstructionMode.A:
                    this.oper = -0x100;
                    this.pageChange = (byte)0;
                    return;
                case InstructionMode.abs:
                    ll = this.getCurrent();
                    this.incrPc();
                    hh = this.getCurrent();
                    this.incrPc();

                    this.oper = 0x100 * hh + ll;
                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.absX:
                    ll = this.getCurrent();
                    this.incrPc();
                    hh = this.getCurrent();
                    this.incrPc();

                    this.oper = (hh * 0x100 + ll + this.x) % 0x10000;
                    this.pageChange = (byte)(ll + this.x > 0xff ? 1 : 0);
                    return;

                case InstructionMode.absY:
                    ll = this.getCurrent();
                    this.incrPc();
                    hh = this.getCurrent();
                    this.incrPc();

                    this.oper = (hh * 0x100 + ll + this.y) % 0x10000;
                    this.pageChange = (byte)(ll + this.y > 0xff ? 1 : 0);
                    return;

                case InstructionMode.imm:
                    this.oper = -0x200 - this.getCurrent();
                    this.incrPc();

                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.impl:
                    this.oper = -1;
                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.ind:  // only used by JMP
                    ll = this.getCurrent();
                    this.incrPc();
                    hh = this.getCurrent();
                    this.incrPc();

                    this.oper = 0x100 * hh + ll;
                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.Xind:  // X,ind
                    ll = this.getCurrent();
                    this.incrPc();

                    this.oper = this[(ll + this.x) % 0x100] + 0x100 * this[(ll + this.x + 1) % 0x100];
                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.indY:  // ind,Y
                    ll = this.getCurrent();
                    this.incrPc();

                    int effective_low = this[ll];
                    int effective_high = this[(ll + 1) % 0x100] * 0x100;
                    this.oper = (effective_high + effective_low + this.y) % 0x10000;
                    
                    this.pageChange = (byte)(effective_low + this.y > 0xff ? 1 : 0);
                    return;

                case InstructionMode.rel:  // rel
                    this.oper = this.getCurrent();
                    this.incrPc();

                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.zpg:  // zpg
                    ll = this.getCurrent();
                    this.incrPc();

                    this.oper = ll;
                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.zpgX:  // zpg,X
                    ll = this.getCurrent();
                    this.incrPc();

                    this.oper = (ll + this.x) % 0x100;
                    this.pageChange = (byte)0;
                    return;

                case InstructionMode.zpgY:  // zpg,Y
                    ll = this.getCurrent();
                    this.incrPc();

                    this.oper = (ll + this.y) % 0x100;
                    this.pageChange = (byte)0;
                    return;

                default:
                    throw new Exception("Unknown mode: " + mode);
            }
        }

        private int branch()
        {
            int target = this.pc[1] + unchecked((sbyte)this.oper);
            int c = 0;
            if (target > 0xff)
            {
                c = 1;
            }
            else if (target < 0)
            {
                c = -1;
            }

            this.pc[1] = (byte)target;
            if (c == 1)
            {
                this.pc[0]++;
            } else if (c == -1)
            {
                this.pc[0]--;
            }

            return c;
        }
    }
}
