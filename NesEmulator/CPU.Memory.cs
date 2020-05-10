using System;

namespace NesEmulator
{
    partial class CPU
    {
        private byte[] storage;

        public byte[] pc;
        public byte ac, x, y, sr, sp;

        public readonly int[] nmiVector = { 0xfffa, 0xfffb };
        public readonly int[] resetVector = { 0xfffc, 0xfffd };
        public readonly int[] irqVector = { 0xfffe, 0xffff };

        public byte this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    int i = this.Map(index);
                    switch (i)
                    {
                        case 0x2000:
                            Console.Error.WriteLine("Cannot 'get' PPUCTRL");
                            break;
                        case 0x2001:
                            Console.Error.WriteLine("Cannot 'get' PPUMASK");
                            break;
                        case 0x2002:
                            return this.nes.ppu.PPUSTATUS;
                        case 0x2003:
                            Console.Error.WriteLine("Cannot 'get' OAMADDR");
                            break;
                        case 0x2004:
                            return this.nes.ppu.OAMDATA;
                        case 0x2005:
                            Console.Error.WriteLine("Cannot 'get' PPUSCROLL");
                            break;
                        case 0x2006:
                            Console.Error.WriteLine("Cannot 'get' PPUADDR");
                            break;
                        case 0x2007:
                            return this.nes.ppu.PPUDATA;
                        case 0x4014:
                            Console.Error.WriteLine("Cannot 'get' OAMDMA");
                            break;

                        case 0x4015:
                            return this.nes.apu.APUSTATUS;

                        case 0x4016:
                            byte data = (byte)(((this.nes.ControllerState[0] & 0x80) > 0) ? 1 : 0);
                            this.nes.ControllerState[0] <<= 1;
                            return data;
                        case 0x4017:
                            data = (byte)(((this.nes.ControllerState[1] & 0x80) > 0) ? 1 : 0);
                            this.nes.ControllerState[1] <<= 1;
                            return data;
                        default:
                            break;
                    }

                    if (i >= 0x4020)
                    {
                        return this.Mapper.CPURead(i);
                    }

                    return this.storage[i];
                }
                return this.GetSpecial(index);
            }
            set
            {
                if (index >= 0)
                {
                    int i = this.Map(index);
                    switch (i)
                    {
                        case 0x2000:
                            this.nes.ppu.PPUCTRL = value;
                            return;
                        case 0x2001:
                            this.nes.ppu.PPUMASK = value;
                            return;
                        case 0x2002:
                            Console.Error.WriteLine("Cannot 'set' PPUSTATUS");
                            break;
                        case 0x2003:
                            this.nes.ppu.OAMADDR = value;
                            return;
                        case 0x2004:
                            this.nes.ppu.OAMDATA = value;
                            return;
                        case 0x2005:
                            this.nes.ppu.PPUSCROLL = value;
                            return;
                        case 0x2006:
                            this.nes.ppu.PPUADDR = value;
                            return;
                        case 0x2007:
                            this.nes.ppu.PPUDATA = value;
                            return;

                        case 0x4000:
                            this.nes.apu.pulse1.PULSE4000 = value;
                            return;
                        case 0x4001:
                            this.nes.apu.pulse1.PULSESWEEP = value;
                            return;
                        case 0x4002:
                            this.nes.apu.pulse1.Period = (ushort)((this.nes.apu.pulse1.Period & 0xff00) | value);
                            return;
                        case 0x4003:
                            this.nes.apu.pulse1.PULSE4003 = value;
                            return;

                        case 0x4004:
                            this.nes.apu.pulse2.PULSE4000 = value;
                            return;
                        case 0x4005:
                            this.nes.apu.pulse2.PULSESWEEP = value;
                            return;
                        case 0x4006:
                            this.nes.apu.pulse2.Period = (ushort)((this.nes.apu.pulse2.Period & 0xff00) | value);
                            return;
                        case 0x4007:
                            this.nes.apu.pulse2.PULSE4003 = value;
                            return;

                        case 0x4008:
                            this.nes.apu.triangle.TRIANGLE4008 = value;
                            return;
                        case 0x4009:
                            // unused
                            return;
                        case 0x400a:
                            this.nes.apu.triangle.Period = (ushort)((this.nes.apu.triangle.Period & 0xff00) | value);
                            return;
                        case 0x400b:
                            this.nes.apu.triangle.TRIANGLE400B = value;
                            return;

                        case 0x400c:
                            this.nes.apu.noise.NOISE400C = value;
                            return;
                        case 0x400d:
                            // unused
                            return;
                        case 0x400e:
                            this.nes.apu.noise.MODE_PERIOD = value;
                            return;
                        case 0x400f:
                            this.nes.apu.noise.NOISE400F = value;
                            return;

                        case 0x4010:
                            this.nes.apu.dmc.FLAGS = value;
                            return;
                        case 0x4011:
                            this.nes.apu.dmc.DIRECTLOAD = value;
                            return;
                        case 0x4012:
                            this.nes.apu.dmc.SAMPLEADDRESS = value;
                            return;
                        case 0x4013:
                            this.nes.apu.dmc.SAMPLELENGTH = value;
                            return;

                        case 0x4014:
                            this.nes.DMAActive = true;
                            this.nes.DMAAddr = 0;
                            this.nes.DMAPage = value;
                            this.cycle += 513 + (this.cycle % 2);
                            return;

                        case 0x4015:
                            this.nes.apu.APUSTATUS = value;
                            return;

                        case 0x4016:
                            this.nes.ControllerState[0] = this.nes.controllers[0].PollKeysPressed();
                            return;
                        case 0x4017:
                            this.nes.ControllerState[1] = this.nes.controllers[1].PollKeysPressed();
                            this.nes.apu.FrameCounter = value;
                            return;
                        default:
                            break;
                    }

                    if (i >= 0x4020)
                    {
                        this.Mapper.CPUWrite(i, value);
                    }
                    else
                    {
                        this.storage[i] = value;
                    }
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
            return this[this.getPc()];
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