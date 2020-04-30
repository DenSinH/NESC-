using System;


namespace NesEmulator
{
    partial class PPU
    {
        private byte[] oam = new byte[0x100];        // object attribute memory

        public byte[] PatternTable = new byte[0x2000];       // pattern tables
        private byte[] VRAM = new byte[0x1000];      // nametables
        public byte[] PaletteRAM = new byte[0x20];  // palette ram indices

        public byte this[int index]
        {
            get
            {
                if (index < 0x2000)
                {
                    return this.PatternTable[index];
                }
                else if (index < 0x3f00)
                {
                    return this.VRAM[(index - 0x2000) % 0x1000];
                }
                else if (index < 0x4000)
                {
                    index &= 0x1f;
                    if (index == 0x0010) index = 0x0000;
                    else if (index == 0x0014) index = 0x0004;
                    else if (index == 0x0018) index = 0x0008;
                    else if (index == 0x0010) index = 0x000c;

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
                    this.PatternTable[index] = value;
                }
                else if (index < 0x3f00)
                {
                    this.VRAM[(index - 0x2000) % 0x1000] = value;
                }
                else if (index < 0x4000)
                {
                    index &= 0x1f;
                    if (index == 0x0010) index = 0x0000;
                    else if (index == 0x0014) index = 0x0004;
                    else if (index == 0x0018) index = 0x0008;
                    else if (index == 0x0010) index = 0x000c;

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
