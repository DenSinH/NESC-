using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace NesEmulator
{
    public partial class VideoDebug : Form
    {
        private Bitmap PatternTable0Bmp, PatternTable1Bmp;
        private int[][] PatternTablesRaw;
        private Bitmap[] Palettes;
        private int[][] PalettesRaw;

        private PictureBox[] PaletteBoxes;

        GCHandle rawBitmap;

        const int width = 254;
        const int height = 240;

        private int PaletteSelected = 0;

        public VideoDebug()
        {
            InitializeComponent();

            this.PatternTable0Bmp = new Bitmap(this.PatternTable0.Width, this.PatternTable0.Height);
            this.PatternTable1Bmp = new Bitmap(this.PatternTable1.Width, this.PatternTable1.Height);

            this.PatternTablesRaw = new int[2][];
            this.PatternTablesRaw[0] = new int[PatternTable0.Width * PatternTable0.Height];
            this.PatternTablesRaw[1] = new int[PatternTable1.Width * PatternTable1.Height];
            
            this.Palettes = new Bitmap[8];
            this.PaletteBoxes = new PictureBox[8];
            this.PaletteBoxes[0] = Palette0;
            this.PaletteBoxes[1] = Palette1;
            this.PaletteBoxes[2] = Palette2;
            this.PaletteBoxes[3] = Palette3;

            this.PaletteBoxes[4] = Palette4;
            this.PaletteBoxes[5] = Palette5;
            this.PaletteBoxes[6] = Palette6;
            this.PaletteBoxes[7] = Palette7;

            this.PalettesRaw = new int[8][];

            for (int i = 0; i < 8; i++)
            {
                this.PalettesRaw[i] = new int[4];
                this.Palettes[i] = new Bitmap(128, 32);
            }
        }

        private void Palette0_Click(object sender, EventArgs e)
        {
            PaletteSelected = 0;
        }

        private void Palette1_Click(object sender, EventArgs e)
        {
            PaletteSelected = 1;
        }

        private void Palette2_Click(object sender, EventArgs e)
        {
            PaletteSelected = 2;
        }

        private void Palette3_Click(object sender, EventArgs e)
        {
            PaletteSelected = 3;
        }

        private void Palette4_Click(object sender, EventArgs e)
        {
            PaletteSelected = 4;
        }

        private void Palette5_Click(object sender, EventArgs e)
        {
            PaletteSelected = 5;
        }

        private void Palette6_Click(object sender, EventArgs e)
        {
            PaletteSelected = 6;
        }

        private void Palette7_Click(object sender, EventArgs e)
        {
            PaletteSelected = 7;
        }

        public void UpdateVisual(NES nes)
        {
            if (nes.ppu == null)
            {
                return;
            }
            else if (nes.ppu.Mapper == null)
            {
                return;
            }

            int i, j;
            // Copy Spritetables
            byte lower, upper;
            bool changed;
            for (i = 0; i < 2; i++)
            {
                // Check if updating spritetable is necessary
                changed = false;
                for (int SampleY = 0; SampleY < 0x10; SampleY++)
                {
                    for (int SampleX = 0; SampleX < 0x10; SampleX++)
                    {
                        lower = (byte)(nes.ppu[(i * 0x1000) + (SampleY * 0x100) + (SampleX * 0x10) + 4] >> 4);
                        upper = (byte)(nes.ppu[(i * 0x1000) + (SampleY * 0x100) + (SampleX * 0x10) + 4 + 8] >> 4);

                        if (this.PatternTablesRaw[i][(8 * SampleX + 3) + 128 * (8 * SampleY + 4)] !=
                            PPU.palette[nes.ppu[0x3f00 | (PaletteSelected << 2) | 2 * (upper & 0x01) | (lower & 0x01)] & 0x3f])
                        {
                            changed = true;
                            break;
                        }
                    }
                    if (changed)
                        break;
                }
                if (!changed)
                    continue;

                for (int SpriteTableTileY = 0; SpriteTableTileY < 0x10; SpriteTableTileY++)
                {
                    for (int SpriteTableTileX = 0; SpriteTableTileX < 0x10; SpriteTableTileX++)
                    {
                        for (int row = 0; row < 8; row++)
                        {
                            lower = nes.ppu[(i * 0x1000) + (SpriteTableTileY * 0x100) + (SpriteTableTileX * 0x10) + row];
                            upper = nes.ppu[(i * 0x1000) + (SpriteTableTileY * 0x100) + (SpriteTableTileX * 0x10) + row + 8];

                            for (byte bit = 0; bit < 8; bit++)
                            {
                                this.PatternTablesRaw[i][(8 * SpriteTableTileX + (7 - bit)) + 128 * (8 * SpriteTableTileY + row)] = 
                                    PPU.palette[nes.ppu[0x3f00 | (PaletteSelected << 2) |  2 * (upper & 0x01) | (lower & 0x01)] & 0x3f];

                                upper >>= 1;
                                lower >>= 1;
                            }

                        }
                    }
                }

                if (i == 0)
                {
                    this.PatternTable0Bmp?.Dispose();
                    rawBitmap = GCHandle.Alloc(this.PatternTablesRaw[0], GCHandleType.Pinned);
                    this.PatternTable0Bmp = new Bitmap(128, 128, 128 * 4,
                                                       PixelFormat.Format32bppRgb, rawBitmap.AddrOfPinnedObject());
                    this.PatternTable0.Image = PatternTable0Bmp;
                    rawBitmap.Free();
                }
                else
                {
                    this.PatternTable1Bmp?.Dispose();
                    rawBitmap = GCHandle.Alloc(this.PatternTablesRaw[1], GCHandleType.Pinned);
                    this.PatternTable1Bmp = new Bitmap(128, 128, 128 * 4,
                                                       PixelFormat.Format32bppRgb, rawBitmap.AddrOfPinnedObject());
                    this.PatternTable1.Image = PatternTable1Bmp;
                    rawBitmap.Free();
                }
            }

            // Copy Palette
            for (i = 0; i < 8; i++)
            {
                changed = false;
                for (j = 0; j < 4; j++)
                {
                    if (this.PalettesRaw[i][j] != PPU.palette[nes.ppu[0x3f00 | (i << 2) | j] & 0x3f])
                    {
                        this.PalettesRaw[i][j] = PPU.palette[nes.ppu[0x3f00 | (i << 2) | j] & 0x3f];
                        changed = true;
                    }
                }

                if (!changed)
                    continue;
                
                Graphics g = Graphics.FromImage(this.Palettes[i]);
                g.Clear(Color.White);
                for (j = 0; j < 4; j++)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(this.PalettesRaw[i][j] | 0xff00_0000))), 31 * j, 0, 31, 31);
                }
                this.PaletteBoxes[i].Image = this.Palettes[i];
            }

            // Copy OAM
            StringBuilder sb = new StringBuilder(64 * 23);
            for (i = 0; i < 64; i++)
            {
                sb.AppendLine(string.Format("{0:d2}: Y:{1:x2} T:{2:x2} A:{3:x2} X:{4:x2}",
                    i, nes.ppu.oam[4 * i], nes.ppu.oam[4 * i + 1], nes.ppu.oam[4 * i + 2], nes.ppu.oam[4 * i + 3]));
            }
            this.OAMText.Text = sb.ToString();

            // Update CPU values
            this.AContent.Text = nes.cpu.ac.ToString("X2");
            this.XContent.Text = nes.cpu.x.ToString("X2");
            this.YContent.Text = nes.cpu.y.ToString("X2");
            this.PCContent.Text = nes.cpu.getPc().ToString("X4");

            this.NFlag.ForeColor = nes.cpu.getFlag('N') == 1 ? Color.Green : Color.Red;
            this.VFlag.ForeColor = nes.cpu.getFlag('V') == 1 ? Color.Green : Color.Red;
            this.BFlag.ForeColor = nes.cpu.getFlag('B') == 1 ? Color.Green : Color.Red;
            this.IFlag.ForeColor = nes.cpu.getFlag('I') == 1 ? Color.Green : Color.Red;
            this.ZFlag.ForeColor = nes.cpu.getFlag('Z') == 1 ? Color.Green : Color.Red;
            this.CFlag.ForeColor = nes.cpu.getFlag('C') == 1 ? Color.Green : Color.Red;

        }

        private void VideoDebug_Load(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
