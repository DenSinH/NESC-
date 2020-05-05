using System;
using System.Threading;
using System.Windows.Forms;

using System.Diagnostics;

namespace NesEmulator
{

    class Program
    {
        public static void Run(NES nes)
        {
            //CPU cpu = new CPU();
            //cpu.Load("../../roms/nestest.nes", 0xc000 - 0x10);
            //cpu.SetPc(0xc000);

            // cpu.Load("../../roms/6502_functional_test.bin");
            // cpu.SetPc(0x400);

            // cpu.Load("../../roms/TTL6502.bin", 0xe000);

            //Console.WriteLine("Starting run in 2 seconds");
            //Thread.Sleep(2000);

            //Stopwatch s = Stopwatch.StartNew();
            //cpu.Run();
            //s.Stop();

            // Console.WriteLine(1000 * cpu.GetCycle() / (double)s.ElapsedMilliseconds);
            
            // fails brk: brk, 16-special
            Cartridge nestest = new Cartridge("../../roms/smb.nes");
            nestest.LoadTo(nes);

            nes.Run(false);

        }

        [STAThread]
        static void Main()
        {
            int[] rawBitmap = new int[0x100 * 0xf0 * 3];
            NES nes = new NES(rawBitmap);

            Thread t = new Thread(() => Run(nes));
            t.SetApartmentState(ApartmentState.STA);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Visual vis = new Visual(nes);

            t.Start();
            Application.Run(vis);
        }
    }
}
