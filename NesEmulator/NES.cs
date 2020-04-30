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

        public void Run()
        {
            // assume cartridge is loaded
            this.cpu.RESET();

            // temporary
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
                    dcycles = this.cpu.NMI();
                }
                this.cpu.cycle += dcycles;

                for (int i = 0; i < 3 * dcycles; i++)
                {
                    this.ppu.Step();
                }

            }
        }

        public void PaletteTest()
        {
            // assume cartridge is loaded
            this.cpu.RESET();

            // temporary
            // this.cpu.SetPc(0xc000);
            int cycles = 0;
            int dcycles;
            while (true)
            {
                dcycles = this.cpu.Step();
                this.cpu.cycle += dcycles;

                cycles += dcycles;

                // Would be one frame
                if (cycles > 26_667)
                {
                    cycles -= 26_667;
                    this.ppu.drawSpriteTable(0, this.PaletteSelect);
                }
            }
        }

        public void NextPalette()
        {
            this.PaletteSelect = (byte)((this.PaletteSelect + 1) % 8);
            Console.WriteLine(this.PaletteSelect);
        }

    }
}
