﻿using System;
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

        private Func<string, int> method;
        public string mode;

        public InstructionCaller(Func<string, int> method, string mode)
        {
            this.method = method;
            this.mode = mode;
        }

        public int call()
        {
            return this.method(this.mode);
        }

    }

    partial class CPU
    {
        private const bool makeLog = false;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CPUMEM mem;
        private int cycle;
        private bool kil = false;

        private PureByte oper;
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
                Func<string, int> instruction;

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

                if (instruction != null)
                {
                    this.instructions[int.Parse(entry.Key, NumberStyles.HexNumber)] = new InstructionCaller(instruction, instructionString[1]);
                }
            }

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
            this.RESET();
        }

        public void Load(string filename)
        {
            this.Load(filename, 0);
        }

        public void RESET()
        {
            this.mem.push(this.mem.get(0x100));
            this.mem.push(this.mem.get(0x1ff));
            this.mem.push(this.mem.get(0x1fe));
            this.mem.setPc(
                this.mem.get(this.mem.resetVector[0]),
                this.mem.get(this.mem.resetVector[1])
                );
            this.cycle = 7;
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
            int opcode = this.mem.getCurrent().unsigned();
            this.mem.incrPc();

            if (makeLog)
            {
                this.log(
                    string.Format(
                        "    {0}  {1} {2}: {3,5}\t\t{4}     A:{5} X:{6} Y:{7} P:{8} SP:{9}        CYC:{10}",
                        (this.mem.getPc() - 1).ToString("x2"),
                        opcode,
                        "INSTRUCTION", // instruction,
                        "MODE", // mode,
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
                PureByte ll = this.mem.getCurrent();
                this.mem.incrPc();
                PureByte hh = this.mem.getCurrent();
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
                                    this.mem.get(hh.unsigned() * 0x100 + ll.unsigned()),
                                    this.mem.get(hh.unsigned() * 0x100 + ((ll.unsigned() + 1) % 0x100))
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
                byte v = (byte)(this.mem.pc[1].unsigned() + 1 > 0xff ? 1 : 0);
                this.mem.push(this.mem.pc[0].unsigned() + v);
                this.mem.push(this.mem.pc[1].unsigned() + 1);

                PureByte ll = this.mem.getCurrent();
                this.mem.incrPc();
                PureByte hh = this.mem.getCurrent();
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
            this.getOper(ic.mode);

            if (this.oper.shared)
            {
                lock (this.oper)
                {
                    return ic.call();
                }
            }
            return ic.call();
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
