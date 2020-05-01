using System;
using System.Threading;

namespace NesEmulator
{
    public class NES
    {
        public CPU cpu;
        public PPU ppu;

        private byte PaletteSelect = 0;

        private const int width = 256;
        private const int height = 240;

        public int[] display;

        public NES(int[] display)
        {
            this.display = display;

            this.cpu = new CPU();
            this.ppu = new PPU(display);

            this.cpu.SetPPU(this.ppu);
            this.ppu.SetCPU(this.cpu);
        }

        public void Run(bool PaletteTest)
        {
            // assume cartridge is loaded
            this.cpu.RESET();

            // for only testing cpu on nestest.nes:
            // this.cpu.SetPc(0xc000);

            int cycles = 0;
            int dcycles;
            while (true)
            {
                if (!this.ppu.ThrowNMI)
                {
                    dcycles = this.cpu.Step();
                }
                else
                {
                    this.ppu.ThrowNMI = false;
                    dcycles = this.cpu.NMI();
                }
                cycles += dcycles;
                this.cpu.cycle += dcycles;

                for (int i = 0; i < 3 * dcycles; i++)
                {
                    this.ppu.Step();
                }

                if (PaletteTest && cycles > 26_666)
                {
                    // Would be one frame
                    cycles -= 26_666;
                    this.ppu.drawSpriteTable(0, this.PaletteSelect);

                    for (int y = 0; y < 30; y++)
                    {
                        for (int x = 0; x < 32; x++)
                        {
                            Console.Write(this.ppu.VRAM[y * 32 + x].ToString("x2") + " ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();

                    for (int i = 0; i < 0x20; i++)
                    {
                        Console.WriteLine(this.ppu.PaletteRAM[i]);
                    }
                }

            }
        }

        public void Run()
        {
            this.Run(false);
        }

        public void NextPalette()
        {
            this.PaletteSelect = (byte)((this.PaletteSelect + 1) % 8);
            Console.WriteLine(this.PaletteSelect);
        }

    }
}
