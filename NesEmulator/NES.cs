using System;
using System.Threading;

using NLog;

namespace NesEmulator
{
    public class NES
    {
        private const byte makeLog = 2;  // 0: no log | 1: Console | 2: File + Console
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

        private void Log(string message)
        {
            // for logging results
            if (makeLog > 0)
            {
                Console.WriteLine(message);
                if (makeLog > 1)
                {
                    logger.Debug(message);
                }
            }
        }

        public void Run(bool debug)
        {
            // assume cartridge is loaded
            this.cpu.RESET();

            // for only testing cpu on nestest.nes:
            //this.cpu.SetPc(0xc000);
            //this.cpu.Run();

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

                if (debug&& (this.cpu.cycle >= 116006))
                {
                    this.ppu.DumpVRAM();
                    this.Log(this.cpu.GenLog() + " || PPU: " + this.ppu.GenLog());
                    Console.ReadKey();
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
