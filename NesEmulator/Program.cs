using System;
using System.Threading;
using System.Windows.Forms;

using System.Diagnostics;

namespace NesEmulator
{

    class Program
    {
        public static void Run(byte[] rawBitmap)
        {
            CPU cpu = new CPU();
            cpu.Load("../../roms/nestest.nes", 0xc000 - 0x10);
            cpu.SetPc(0xc000);

            // cpu.Load("../../roms/6502_functional_test.bin");
            // cpu.SetPc(0x400);

            // cpu.Load("../../roms/TTL6502.bin", 0xe000);

            Console.WriteLine("Starting run in 2 seconds");
            Thread.Sleep(2000);

            Stopwatch s = Stopwatch.StartNew();
            cpu.Run();
            s.Stop();

            Console.WriteLine(1000 * cpu.GetCycle() / (double)s.ElapsedMilliseconds);

            while (true)
            {
                lock (rawBitmap)
                {
                    try
                    {
                        Random rnd = new Random();
                        rnd.NextBytes(rawBitmap);
                    }
                    catch (ArgumentNullException)
                    {
                        Console.WriteLine("Something went wrong");
                    }
                }
                Thread.Sleep(1);
            }

        }

        [STAThread]
        static void Main()
        {
            byte[] rawBitmap = new byte[0x100 * 0xf0 * 3];

            Thread t = new Thread(() => Run(rawBitmap));
            t.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Visual(rawBitmap));
        }
    }
}
