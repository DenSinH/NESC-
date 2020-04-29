using System;
using System.IO;

namespace NesEmulator
{
    class Cartridge
    {
        private string filename;

        private byte PRGsize, CHRSize;
        private bool mirroring, BatteryPackedPRGRam;
        private byte mapper;

        public Cartridge(string filename)
        {
            this.filename = filename;
        }

        public void LoadTo(NES nes)
        {
            this.LoadTo(nes.cpu, nes.ppu);
        }

        public void LoadTo(CPU cpu, PPU ppu)
        {
            // load rom into memory
            using (FileStream fs = File.OpenRead(filename))
            {
                for (int i = 0; i < 4; i++)
                {
                    // NES\n Header
                    fs.ReadByte();
                }

                this.PRGsize = (byte)fs.ReadByte();
                this.CHRSize = (byte)fs.ReadByte();

                // FLAGS, Not implemented

                for (int i = 0; i < 10; i++)
                {
                    fs.ReadByte();
                }

                this.xNROM(fs, cpu, ppu);
            }
        }

        private void xNROM(FileStream fs, CPU cpu, PPU ppu)
        {
            // todo: PRG RAM

            for (int i = 0; i < 0x4000; i++)
            {
                cpu.mem[0x8000 + i] = (byte)fs.ReadByte();
                if (this.PRGsize == 1)
                {
                    cpu.mem[0xc000 + i] = cpu.mem[0x8000 + i];
                }
            }

            if (this.PRGsize == 2)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    cpu.mem[0xc000 + i] = (byte)fs.ReadByte();
                }
            }

            if (this.CHRSize == 1)
            {
                for (int i = 0; i < 0x2000; i++)
                {
                    ppu[i] = (byte)fs.ReadByte();
                }
            }

            Console.WriteLine(this.filename + "Loaded");

        }

    }
}
