﻿using System;
using System.Diagnostics;


namespace NesEmulator
{
    partial class PPU
    {
        public byte[] oam = new byte[0x100];           // object attribute memory

        // pattern tables are in the mapper
        public byte[] VRAM = new byte[0x1000];         // nametables + attribute tables
        public byte[] PaletteRAM = new byte[0x20];      // palette ram indices

        public byte this[int index]
        {
            get
            {
                if (index < 0x2000)
                {
                    return this.Mapper.PPURead(index);
                }
                else if (index < 0x3f00)
                {
                    index %= 0x1000;

                    // todo: Mirroring, currently default Horizontal
                    switch (this.Mapper.Mirror)
                    {
                        case MirrorType.Horizontal:
                            if (index < 0x800)
                            {
                                return this.VRAM[index % 0x400];
                            }
                            else
                            {
                                return this.VRAM[0x800 + (index % 0x400)];
                            }
                        case MirrorType.Vertical:
                            return this.VRAM[index % 0x800];
                        case MirrorType.SingleScreen:
                            return this.VRAM[index % 0x400];
                        case MirrorType.FourScreen:
                            return this.VRAM[index];
                        default:
                            throw new Exception("Unknown mirroring type: " + this.Mapper.Mirror);
                    }
                }
                else if (index < 0x4000)
                {
                    index &= 0x1f;
                    if (index == 0x0010) index = 0x0000;
                    else if (index == 0x0014) index = 0x0004;
                    else if (index == 0x0018) index = 0x0008;
                    else if (index == 0x001c) index = 0x000c;

                    return this.PaletteRAM[index];
                }
                else if (index < 0x10000)
                {
                    return this[index % 0x4000];
                }
                else
                {
                    throw new IndexOutOfRangeException("Index " + index + " out of range for PPU internal memory");
                }
            }

            set
            {
                if (index < 0x2000)
                {
                    this.Mapper.PPUWrite(index, value);
                }
                else if (index < 0x3f00)
                {
                    index %= 0x1000;

                    // todo: Mirroring, currently default Horizontal
                    switch (this.Mapper.Mirror)
                    {
                        case MirrorType.Horizontal:
                            if (index < 0x800)
                            {
                                this.VRAM[index % 0x400] = value;
                            } else
                            {
                                this.VRAM[0x800 + (index % 0x400)] = value;
                            }
                            break;
                        case MirrorType.Vertical:
                            this.VRAM[index % 0x800] = value;
                            break;
                        case MirrorType.SingleScreen:
                            this.VRAM[index % 0x400] = value;
                            break;
                        case MirrorType.FourScreen:
                            this.VRAM[index] = value;
                            break;
                        default:
                            throw new Exception("Unknown mirroring type: " + this.Mapper.Mirror);
                    }
                }
                else if (index < 0x4000)
                {
                    index &= 0x1f;
                    if (index == 0x0010) index = 0x0000;
                    else if (index == 0x0014) index = 0x0004;
                    else if (index == 0x0018) index = 0x0008;
                    else if (index == 0x001c) index = 0x000c;

                    this.PaletteRAM[index] = value;
                }

                else if (index < 0x10000)
                {
                    this[index % 0x4000] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException("Index " + index + " out of range for PPU internal memory");
                }
            }

        }


    }
}
