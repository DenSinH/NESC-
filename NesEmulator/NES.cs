using System;
using System.Threading;

namespace NesEmulator
{
    class NES
    {
        public CPU cpu;
        public PPU ppu;

        private const int width = 256;
        private const int height = 240;

        public int[] display;

        public NES(int[] display)
        {
            this.display = display;

            this.cpu = new CPU();
            this.ppu = new PPU(display);
            this.ppu.SetCPU(this.cpu);
        }

        public void Run()
        {
            // assume cartridge is loaded
            this.cpu.RESET();

            // temporary
            // this.cpu.SetPc(0xc000);
            this.cpu.Run();
            Console.WriteLine(cpu.GetCycle());

            while (true)
            {
                lock (this.display)
                {
                    try
                    {
                        Random rnd = new Random();
                        for (int i = 0; i < 0x100 * 0xf0; i++)
                        {
                            this.display[i] = rnd.Next(0, 0xff);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        Console.WriteLine("Something went wrong");
                    }
                }
                Thread.Sleep(17);
            }
        }

    }
}
