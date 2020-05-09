using System;
using System.Threading;
using System.Windows.Forms;

namespace NesEmulator
{

    class Program
    {
        public static void Run(NES nes)
        {      
            // fails brk: brk, 16-special
            Cartridge cartridge = new Cartridge("../../roms/zelda.nes");
            cartridge.LoadTo(nes);

            nes.Run(true);

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
