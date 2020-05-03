using System;
using System.IO;

namespace NesEmulator
{
    class Cartridge
    {
        private string filename;
        private FileStream fs;

        private byte PRGsize, CHRSize;
        private bool BatteryPackedPRGRam;
        private MirrorType Mirror;
        private byte mapper;

        public Cartridge(string filename)
        {
            this.filename = filename;
            this.fs = File.OpenRead(filename);

            for (int i = 0; i < 4; i++)
            {
                // NES\n Header
                fs.ReadByte();
            }

            this.PRGsize = (byte)this.fs.ReadByte();
            this.CHRSize = (byte)this.fs.ReadByte();

            byte Flags6 = (byte)this.fs.ReadByte();

            if ((Flags6 & 0x01) == 0)
            {
                this.Mirror = MirrorType.Horizontal;
            } else
            {
                this.Mirror = MirrorType.Vertical;
            }

            // FLAGS, Not implemented

            for (int i = 0; i < 9; i++)
            {
                fs.ReadByte();
            }
        }

        public void LoadTo(NES nes)
        {
            this.LoadTo(nes.cpu, nes.ppu);
        }

        public void LoadTo(CPU cpu, PPU ppu)
        {
            ppu.SetMirrorType(this.Mirror);

            // load rom into memory  

            this.xNROM(cpu, ppu);

            this.fs.Close();
        }

        private void xNROM(CPU cpu, PPU ppu)
        {
            // todo: PRG RAM

            for (int i = 0; i < 0x4000; i++)
            {
                cpu[0x8000 + i] = (byte)this.fs.ReadByte();
                if (this.PRGsize == 1)
                {
                    cpu[0xc000 + i] = cpu[0x8000 + i];
                }
            }

            if (this.PRGsize == 2)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    cpu[0xc000 + i] = (byte)this.fs.ReadByte();
                }
            }

            if (this.CHRSize == 1)
            {
                for (int i = 0; i < 0x2000; i++)
                {
                    ppu[i] = (byte)this.fs.ReadByte();
                }
            }

            Console.WriteLine(this.filename + "Loaded");

        }

    }
}
