using System;

namespace NesEmulator
{
    public partial class PPU
    {
        CPU cpu;
        private int[] display;

        private MirrorType Mirror;

        readonly int[] palette = {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400, 0x503000, 0x007800,
            0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000, 0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC,
            0xD800CC, 0xE40058, 0xF83800, 0xE45C10, 0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000,
            0x000000, 0x000000, 0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000, 0xFCFCFC, 0xA4E4FC,
            0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8, 0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8,
            0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };

        public PPU(int[] display)
        {
            this.display = display;
        }

        public void SetCPU(CPU cpu)
        {
            this.cpu = cpu;
        }

        public void SetMirrorType(MirrorType m)
        {
            this.Mirror = m;
        }

        public string GenLog()
        {
            return this.scanline + ", " + this.cycle + ", V:  " + this.V.ToString("x4");
        }

        public void DumpPAL()
        {
            for (int i = 0x3f00; i < 0x3f20; i++)
            {
                if (i % 0x10 == 0)
                {
                    Console.WriteLine();
                    Console.Write(i.ToString("x4"));
                }
                Console.Write(" " + this[i].ToString("x2"));
            }
            Console.WriteLine();
        }

        public void DumpVRAM()
        {
            for (int i = 0x2000; i < 0x3000; i++)
            {
                if (i % 0x10 == 0)
                {
                    Console.WriteLine();
                    Console.Write(i.ToString("x4"));
                }
                Console.Write(" " + this[i].ToString("x2"));
            }
            Console.WriteLine();
        }

    }
}
