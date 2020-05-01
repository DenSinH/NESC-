using System;

namespace NesEmulator
{
    partial class CPU
    {
        public class CPUMEM
        {
            private byte[] storage;

            public byte[] pc;
            public byte ac, x, y, sr, sp;

            public PPU ppu;

            public readonly int[] nmiVector = { 0xfffa, 0xfffb };
            public readonly int[] resetVector = { 0xfffc, 0xfffd };
            public readonly int[] irqVector = { 0xfffe, 0xffff };

            public CPUMEM()
            {
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
            }

            public byte this[int index]
            {
                get
                {
                    if (index >= 0)
                    {
                        switch (this.Map(index))
                        {
                            case 0x2000:
                                break;
                                // throw new Exception("Cannot 'get' PPUCTRL");
                            case 0x2001:
                                break;
                                // throw new Exception("Cannot 'get' PPUMASK");
                            case 0x2002:
                                return this.ppu.PPUSTATUS;
                            case 0x2003:
                                break;
                                // throw new Exception("Cannot 'get' OAMADDR");
                            case 0x2004:
                                return this.ppu.OAMDATA;
                            case 0x2005:
                                break;
                                // throw new Exception("Cannot 'get' PPUSCROLL");
                            case 0x2006:
                                break;
                                // throw new Exception("Cannot 'get' PPUADDR");
                            case 0x2007:
                                return this.ppu.PPUDATA;
                            default:
                                break;
                        }
                        return this.storage[this.Map(index)];
                    }
                    return this.GetSpecial(index);
                }
                set
                {
                    if (index >= 0)
                    {

                        switch (this.Map(index))
                        {
                            case 0x2000:
                                this.ppu.PPUCTRL = value;
                                return;
                            case 0x2001:
                                this.ppu.PPUMASK = value;
                                return;
                            case 0x2002:
                                break;
                                // throw new Exception("Cannot 'set' PPUSTATUS");
                            case 0x2003:
                                this.ppu.OAMADDR = value;
                                return;
                            case 0x2004:
                                this.ppu.OAMDATA = value;
                                return;
                            case 0x2005:
                                this.ppu.PPUSCROLL = value;
                                return;
                            case 0x2006:
                                this.ppu.PPUADDR = value;
                                return;
                            case 0x2007:
                                this.ppu.PPUDATA = value;
                                return;
                            default:
                                break;
                        }

                        this.storage[this.Map(index)] = value;
                    }
                    else
                    {
                        this.SetSpecial(index, value);
                    }
                }
            }

            public byte this[byte ll, byte hh]
            {
                get
                {
                    return this.storage[this.Map(0x100 * hh + ll)];
                }
                set
                {
                    this.storage[this.Map(0x100 * hh + ll)] = value; ;
                }
            }

            protected int Map(int index)
                {
                    if (index < 0)
                    {
                        return index;
                    }
                    else if (index < 0x2000)
                    {
                        return index % 0x800;
                    }
                    else if (index < 0x4000)
                    {
                        return 0x2000 + (index % 8);
                    }
                    else
                    {
                        return index;
                    }
                }

            protected byte GetSpecial(int index)
            {
                if (index == -0x100)
                {
                    return this.ac;
                }
                return (byte)(-index - 0x200);
            }

            protected void SetSpecial(int index, byte value)
            {
                if (index == -0x100)
                {
                    this.ac = value;
                }
                else
                {
                    throw new Exception("Unknown special index code for CPU memory: " + index);
                }
            }

            public byte getCurrent()
            {
                return this.storage[this.getPc()];
            }

            public int getPc()
            {
                return 0x100 * this.pc[0] + this.pc[1];
            }

            public void setPc(byte ll, byte hh)
            {
                this.pc[0] = hh;
                this.pc[1] = ll;
            }

            public void setNZ(byte val)
            {
                this.setFlag('N', (byte)(val >> 7));
                this.setFlag('Z', (byte)((val == 0) ? 1 : 0));
            }

            public void setFlag(char flag, byte value)
            {
                if ((value & 0xfe) > 0)
                {
                    throw new Exception("Cannot set flag to " + value);
                }

                switch (flag)
                {
                    case 'N': this.sr = (byte)((this.sr & 0b0111_1111) | value * 0b1000_0000); break;
                    case 'V': this.sr = (byte)((this.sr & 0b1011_1111) | value * 0b0100_0000); break;
                    case 'B': this.sr = (byte)((this.sr & 0b1110_1111) | value * 0b0001_0000); break;
                    case 'D': this.sr = (byte)((this.sr & 0b1111_0111) | value * 0b0000_1000); break;
                    case 'I': this.sr = (byte)((this.sr & 0b1111_1011) | value * 0b0000_0100); break;
                    case 'Z': this.sr = (byte)((this.sr & 0b1111_1101) | value * 0b0000_0010); break;
                    case 'C': this.sr = (byte)((this.sr & 0b1111_1110) | value * 0b0000_0001); break;
                    default: throw new Exception("Unknown flag: " + flag);
                }
            }

            public byte getFlag(char flag)
            {
                switch (flag)
                {
                    case 'N': return (byte)((this.sr & 0b1000_0000) >> 7);
                    case 'V': return (byte)((this.sr & 0b0100_0000) >> 6);
                    case 'B': return (byte)((this.sr & 0b0001_0000) >> 4);
                    case 'D': return (byte)((this.sr & 0b0000_1000) >> 3);
                    case 'I': return (byte)((this.sr & 0b0000_0100) >> 2);
                    case 'Z': return (byte)((this.sr & 0b0000_0010) >> 1);
                    case 'C': return (byte)((this.sr & 0b0000_0001));
                    default: throw new Exception("Unknown flag: " + flag);
                };
            }

            public void incrPc(int amt)
            {
                int c = (this.pc[1] + amt > 0xff) ? 1 : 0;
                this.pc[1] += (byte)amt;
                if (c == 1)
                {
                    this.pc[0]++;
                }
            }

            public void incrPc()
            {
                this.incrPc(1);
            }

            public void push(byte value)
            {
                if (this.sp == 0)
                {
                    Console.Error.WriteLine("Cannot push onto full stack");
                }
                this.storage[0x100 + this.sp] = value;
                this.sp--;
            }

            public byte pull()
            {
                if (this.sp == 0xff)
                {
                    Console.Error.WriteLine("Cannot pull from empty stack");
                }
                this.sp++;
                return this.storage[0x100 + this.sp];
            }

        }

    }
}