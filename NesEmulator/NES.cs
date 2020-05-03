using System;
using System.Threading;

using NLog;

namespace NesEmulator
{
    public class NES
    {
        /* Debugging */
        private const byte makeLog = 1;  // 0: no log | 1: Console | 2: File + Console
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /* Components */
        public CPU cpu;
        public PPU ppu;

        public byte DMAPage, DMAAddr, DMAData;
        public bool DMAActive, DMAStart;

        public readonly Controller[] controllers = new Controller[2];
        public byte[] ControllerState = { 0, 0 };
        
        /* Display */
        private const int width = 256;
        private const int height = 240;

        public int[] display;

        public NES(int[] display)
        {
            this.display = display;

            this.cpu = new CPU(this);
            this.ppu = new PPU(this);

            this.controllers[0] = new XInputController();
            this.controllers[1] = new KeyboardController();
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
            int GlobalCycles = 0;
            while (true)
            {
                if (!this.DMAActive)
                {
                    dcycles = this.cpu.Step();
                    this.cpu.cycle += dcycles;
                }
                else
                {
                    dcycles = 1;
                }

                if (this.ppu.ThrowNMI)
                {
                    this.ppu.ThrowNMI = false;
                    this.cpu.cycle += this.cpu.NMI();
                }
                

                for (int i = 0; i < 3 * dcycles; i++)
                {
                    // DMA transfer: 
                    if (!DMAStart)
                    {
                        if ((GlobalCycles % 2) == 1)
                        {
                            DMAStart = true;
                        }
                    }
                    else
                    {
                        if ((GlobalCycles % 2) == 1)
                        {
                            DMAData = this.cpu[(DMAPage << 8) | DMAAddr];
                        }
                        else
                        {
                            this.ppu.oam[DMAAddr] = DMAData;
                            DMAAddr++;

                            if (DMAAddr == 0)
                            {
                                DMAStart = false;
                                DMAActive = false;
                            }
                        }
                    }

                    this.ppu.Step();
                    GlobalCycles++;
                }

                if (debug && (this.cpu.cycle >= 1000000)) 
                {
                    // Console.WriteLine(controller.PollKeysPressed());
                    // this.ppu.DumpVRAM();
                    // this.Log(this.cpu.GenLog() + " || PPU: " + this.ppu.GenLog());
                    
                    // this.ppu.DrawNametable(0, 1);
                    // this.ppu.drawSpriteTable(1, 0);
                    // Console.ReadKey();
                }

            }
        }

        public void Run()
        {
            this.Run(false);
        }
    }
}
