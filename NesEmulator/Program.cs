using System;
using System.Threading;
using System.Windows.Forms;

namespace NesEmulator
{

    class Program
    {
        [STAThread]
        static void Main()
        {
            int[] rawBitmap = new int[0x100 * 0xf0 * 3];
            NES nes = new NES(rawBitmap);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Visual vis = new Visual(nes);

            Application.Run(vis);
        }
    }
}
