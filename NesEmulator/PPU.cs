using System;
using NesEmulator.Mappers;

namespace NesEmulator
{
    public partial class PPU
    {
        NES nes;
        
        public Mapper Mapper;

        readonly int[] palette = {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400, 0x503000, 0x007800,
            0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000, 0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC,
            0xD800CC, 0xE40058, 0xF83800, 0xE45C10, 0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000,
            0x000000, 0x000000, 0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000, 0xFCFCFC, 0xA4E4FC,
            0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8, 0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8,
            0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };

        public PPU(NES nes)
        {
            this.nes = nes;
        }

        public void SetMapper(Mapper m)
        {
            this.Mapper = m;
        }

        public void POWERUP()
        {
            // https://wiki.nesdev.com/w/index.php/PPU_power_up_state
            this.PPUCTRL = 0;
            this.PPUMASK = 0;
            this.w = 0;
            this.PPUSCROLL = 0;
            this.PPUDATA = 0;
            this.OddFrame = false;
        }
        
        /*
        ====================
          Debugging  stuff
        ====================
        */

        public string GenLog()
        {
            return this.scanline + ", " + this.cycle + ", V:  " + this.V.ToString("x4");
        }

        public void DumpPAL()
        {
            for (int i = 0x3f00; i < 0x3f20; i++)
            {
                if (i % 0x10 == 0)
                {
                    Console.WriteLine();
                    Console.Write(i.ToString("x4"));
                }
                Console.Write(" " + this[i].ToString("x2"));
            }
            Console.WriteLine();
        }

        public void DumpVRAM()
        {
            for (int i = 0x2000; i < 0x3000; i++)
            {
                if (i % 0x10 == 0)
                {
                    Console.WriteLine();
                    Console.Write(i.ToString("x4"));
                }
                Console.Write(" " + this[i].ToString("x2"));
            }
            Console.WriteLine();
        }

        public void DumpOAM()
        {
            for (int n = 0; n < 64; n++)
            {
                for (int m = 0; m < 4; m++)
                {
                    Console.Write(this.oam[4 * n + m].ToString("x2") + " : ");
                }
                Console.WriteLine();
            }
        }

        public void DrawNametable(int NT, int PT)
        {
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    int TilePatternID = this.VRAM[NT * 0x400 + 32 * y + x];
                    for (int finey = 0; finey < 8; finey++)
                    {
                        byte PatternLow = this[0x1000 * PT + (TilePatternID << 4) + finey];
                        byte PatternHigh = this[0x1000 * PT + (TilePatternID << 4) + finey + 8];

                        for (int finex = 0; finex < 8; finex++)
                        {
                            int X = 8 * x + (7 - finex);
                            int Y = 8 * y + finey;

                            byte paletteinternal = (byte)(2 * (PatternHigh & 0x01) + (PatternLow & 0x01));

                            PatternLow >>= 1;
                            PatternHigh >>= 1;

                            this.SetPixel(X, Y, 0, 0, paletteinternal);

                        }
                    }
                }
            }
        }

        // draws spritetable 'left' and palette on screen
        public void drawSpriteTable(byte left, byte PaletteNumber)
        {
            byte lower, upper;

            for (int SpriteTableTileY = 0; SpriteTableTileY < 0x10; SpriteTableTileY++)
            {
                for (int SpriteTableTileX = 0; SpriteTableTileX < 0x10; SpriteTableTileX++)
                {
                    for (int row = 0; row < 8; row++)
                    {
                        lower = this[(left * 0x1000) + (SpriteTableTileY * 0x100) + (SpriteTableTileX * 0x10) + row];
                        upper = this[(left * 0x1000) + (SpriteTableTileY * 0x100) + (SpriteTableTileX * 0x10) + row + 8];

                        for (byte bit = 0; bit < 8; bit++)
                        {
                            this.SetPixel(8 * SpriteTableTileX + (7 - bit), (8 * SpriteTableTileY + row),
                                PaletteNumber / 4, PaletteNumber % 4, 2 * (upper & 0x01) + (lower & 0x01));

                            upper >>= 1;
                            lower >>= 1;
                        }

                    }
                }
            }

            for (int i = 0; i < 0x20; i++)
            {
                for (int y = 0; y < 8; y++)
                {
                    this.SetPixel(16 * 8 + 4 * i, y, i >> 4, (i >> 2) & 0x03, i % 4);
                    this.SetPixel(16 * 8 + 4 * i + 1, y, i >> 4, (i >> 2) & 0x03, i % 4);
                    this.SetPixel(16 * 8 + 4 * i + 2, y, i >> 4, (i >> 2) & 0x03, i % 4);
                    this.SetPixel(16 * 8 + 4 * i + 3, y, i >> 4, (i >> 2) & 0x03, i % 4);
                }
            }
        }
    }
}
