using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private const int MenuStripHeight = 20;
        private const double scale = 2;

        private NES nes;
        private Thread PlayThread;

        public Visual(NES nes)
        {
            InitializeComponent();
            this.Size = new Size((int) (scale * width), (int) (scale * height + MenuStripHeight));

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

            this.Text = "NES Emulator";

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 17;
            timer.Tick += new EventHandler(Tick);
            timer.Start();

            MenuStrip ms = new MenuStrip();
            ToolStripMenuItem windowMenu = new ToolStripMenuItem("Game");
            ToolStripMenuItem windowNewMenu = new ToolStripMenuItem("Open", null, new EventHandler(LoadGame));

            windowMenu.DropDownItems.Add(windowNewMenu);
            ((ToolStripDropDownMenu)(windowMenu.DropDown)).ShowImageMargin = false;
            ((ToolStripDropDownMenu)(windowMenu.DropDown)).ShowCheckMargin = false;

            // Assign the ToolStripMenuItem that displays 
            // the list of child forms.
            ms.MdiWindowListItem = windowMenu;

            // Add the window ToolStripMenuItem to the MenuStrip.
            ms.Items.Add(windowMenu);

            // Dock the MenuStrip to the top of the form.
            ms.Dock = DockStyle.Top;

            // The Form.MainMenuStrip property determines the merge target.
            this.MainMenuStrip = ms;

            // Add the MenuStrip last.
            // This is important for correct placement in the z-order.
            this.Controls.Add(ms);

            this.Load += new EventHandler(Visual_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Visual_Paint);

            this.KeyDown += new KeyEventHandler(Visual_KeyDown);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.nes.speaker.ShutDown();
            this.nes.ShutDown = true;
            base.OnFormClosing(e);
        }

        private void Visual_KeyDown(object sender, KeyEventArgs e)
        {
            // Debugging keys
            if (e.KeyCode == Keys.O)
            {
                this.nes.ppu.DumpOAM();
            }
            else if (e.KeyCode == Keys.V)
            {
                this.nes.ppu.DumpVRAM();
            }
            else if (e.KeyCode == Keys.P)
            {
                this.nes.ppu.DumpPAL();
            }
            else if (e.KeyCode == Keys.Add)
            {
                this.nes.apu.ChangeAmplitude(0.005);
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                this.nes.apu.ChangeAmplitude(-0.005);
            }
        }

        private void Visual_Paint(object sender, PaintEventArgs e)
        {
            if (Backbuffer != null)
            {
                // no image scaling for crisp pixels!
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(this.Backbuffer, 0, MenuStripHeight, this.Size.Width, this.Size.Height - MenuStripHeight);
            }
        }

        private void Visual_CreateBackBuffer(object sender, EventArgs e)
        {
            this.Backbuffer?.Dispose();
            
            Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        private void Draw()
        {

            // ref: https://github.com/Xyene/Emulator.NES/blob/master/dotNES/Renderers/SoftwareRenderer.cs
            if (Backbuffer != null)
            {
                this.Backbuffer?.Dispose();
                //lock (this.rawBitmap)
                //{
                    _rawBitmap = GCHandle.Alloc(this.rawBitmap, GCHandleType.Pinned);
                    this.Backbuffer = new Bitmap(width, height, width * 4,
                                PixelFormat.Format32bppRgb, _rawBitmap.AddrOfPinnedObject());
                //}
                
                _rawBitmap.Free();
                Invalidate();  // set so that updated pixels are invalidated
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            Draw();
        }

        private void LoadGame(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "../../roms/";
                openFileDialog.Filter = "NES ROMS (*.nes)|*.nes|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.nes.ShutDown = true;
                    if (this.PlayThread != null)
                    {
                        this.PlayThread.Join();
                    }

                    this.PlayThread = new Thread(() => Play(openFileDialog.FileName));
                    this.PlayThread.SetApartmentState(ApartmentState.STA);
                    this.PlayThread.Start();
                }
            }
        }

        private void Play(string filename)
        {
            Cartridge cartridge = new Cartridge(filename);
            cartridge.LoadTo(this.nes);

            this.nes.cpu.POWERUP();
            this.nes.ppu.POWERUP();
            this.nes.apu.POWERUP();
            this.nes.ShutDown = false;

            this.nes.Run(false);

        }
    }
}
