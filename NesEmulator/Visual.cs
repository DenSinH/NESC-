using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NesEmulator
{
    public partial class Visual : Form
    {
        private Bitmap Backbuffer;
        private int[] rawBitmap;
        private GCHandle _rawBitmap;

        private const int width = 0x100;
        private const int height = 0xf0;
        private const double scale = 2;

        private NES nes;

        public Visual(NES nes)
        {
            InitializeComponent();
            this.Size = new Size((int) (scale * width), (int) (scale * height));

            this.nes = nes;
            this.rawBitmap = nes.display;

            // disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer, true
            );

            Timer timer = new Timer();
            timer.Interval = 17;
            timer.Tick += new EventHandler(Tick);
            timer.Start();
            
            this.Load += new EventHandler(Visual_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Visual_Paint);

            this.KeyDown += new KeyEventHandler(Visual_KeyDown);
        }

        void Visual_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Add)
            //{
                
            //}
        }

        void Visual_Paint(object sender, PaintEventArgs e)
        {
            if (Backbuffer != null)
            {
                e.Graphics.DrawImage(this.Backbuffer, 0, 0, this.Size.Width, this.Size.Height);
            }
        }

        void Visual_CreateBackBuffer(object sender, EventArgs e)
        {
            this.Backbuffer?.Dispose();
            
            Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        void Draw()
        {

            // ref: https://github.com/Xyene/Emulator.NES/blob/master/dotNES/Renderers/SoftwareRenderer.cs
            if (Backbuffer != null)
            {
                this.Backbuffer?.Dispose();
                lock (this.rawBitmap)
                {
                    _rawBitmap = GCHandle.Alloc(this.rawBitmap, GCHandleType.Pinned);
                    this.Backbuffer = new Bitmap(width, height, width * 4,
                                PixelFormat.Format32bppRgb, _rawBitmap.AddrOfPinnedObject());
                }

                _rawBitmap.Free();
                Invalidate();  // set so that updated pixels are invalidated
            }
        }

        void Tick(object sender, EventArgs e)
        {
            Draw();
        }
    }
}
