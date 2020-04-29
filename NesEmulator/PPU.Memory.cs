using System;


namespace NesEmulator
{
    partial class PPU
    {
        private byte[] oam = new byte[0x100];        // object attribute memory

        private byte[] chr = new byte[0x2000];       // pattern tables
        private byte[] vram = new byte[0x1000];      // nametables
        private byte[] paletteRam = new byte[0x20];  // palette ram indices

        private int vPointer;

        public byte this[int index]
        {
            get
            {
                if (index < 0x2000)
                {
                    return this.chr[index];
                }
                else if (index < 0x3f00)
                {
                    return this.vram[(index - 0x2000) % 0x1000];
                }
                else if (index < 0x4000)
                {
                    return this.paletteRam[(index - 0x3f00) % 0x20];
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
                    this.chr[index] = value;
                }
                else if (index < 0x3f00)
                {
                    this.vram[(index - 0x2000) % 0x1000] = value;
                }
                else if (index < 0x4000)
                {
                    this.paletteRam[(index - 0x3f00) % 0x20] = value;
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
