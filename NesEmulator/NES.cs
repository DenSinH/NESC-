using System;
using System.Threading;

using NLog;

namespace NesEmulator
{
    public class NES
    {
        private const byte makeLog = 1;  // 0: no log | 1: Console | 2: File + Console
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
            
            int dcycles;
            while (true)
            {

                dcycles = this.cpu.Step();
                this.cpu.cycle += dcycles;

                if (this.ppu.ThrowNMI)
                {
                    this.ppu.ThrowNMI = false;
                    this.cpu.cycle += this.cpu.NMI();
                }
                

                for (int i = 0; i < 3 * dcycles; i++)
                {
                    this.ppu.Step();
                }

                if (debug && (this.cpu.cycle >= 130000)) 
                {
                    // this.ppu.DumpVRAM();
                    this.Log(this.cpu.GenLog() + " || PPU: " + this.ppu.GenLog());
                    
                    // this.ppu.DrawNametable(0, 1);
                    // this.ppu.drawSpriteTable(1, 0);
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
