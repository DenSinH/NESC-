using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator
{
    partial class PPU
    {
        public bool ThrowNMI = false;
        public bool FinishedFrame = false;
        bool OddFrame = false;

        private int scanline = 0;
        private int cycle = 0;

        byte BGNextTileID;
        byte BGNextTileAttribute;

        byte BGNextPatternLow;
        byte BGNextPatternHigh;

        UInt16 BGShifterPatternLow;
        UInt16 BGShifterPatternHigh;
        UInt16 BGShifterAttributeLow;
        UInt16 BGShifterAttributeHigh;

        private void LoadBGShifterNext()
        {
            BGShifterPatternLow = (UInt16)((BGShifterPatternLow & 0xff00) | BGNextPatternLow);
            BGShifterPatternHigh = (UInt16)((BGShifterPatternHigh & 0xff00) | BGNextPatternHigh);

            BGShifterAttributeLow = (UInt16)((BGShifterAttributeLow & 0xff00) | (((BGNextTileAttribute & 0b01) == 0b01) ? 0xff : 0x00));
            BGShifterAttributeHigh = (UInt16)((BGShifterAttributeHigh & 0xff00) | (((BGNextTileAttribute & 0b10) == 0b10) ? 0xff : 0x00));
        }

        private void ShiftBGShifters()
        {
            if (BGEnable == 1)
            {
                BGShifterPatternLow <<= 1;
                BGShifterPatternHigh <<= 1;

                BGShifterAttributeLow <<= 1;
                BGShifterAttributeHigh <<= 1;
            }
        }

        private void SetPixel(int x, int y, int BGSpriteSelect, int PaletteStart, int PaletteInternal)
        {
            /*
             x: x position on screen
             y: y position on screen
             BGSpriteSelect: BG or Sprite selected
             PaletteStart: Highest 3 bits of chosen palette in palette ram 
             PaletteInternal: Lowest 2 bits of chosen palette in palette ram
             */
            if ((x >= 0) && (x < 0x100) && (y >= 0) && (y < 0xf0))
            { 
                this.display[0x100 * y + x] = this.palette[
                            this[0x3f00 + (BGSpriteSelect << 5) + (PaletteStart << 2) + PaletteInternal] & 0x3f
                    ];
            }
            
        }

        public void Step()
        {
            if (this.scanline == -1)
            {
                // Pre-render scanline
                if (this.cycle == 1)
                {
                    // Set VBlank flag to 0 on second cycle
                    this.VBlank = 0;
                }
                else if ((280 <= cycle) && (cycle <= 304))
                {
                    if (BGEnable == 1 || SpriteEnable == 1)
                    {
                        // vert(v) = vert(t) each tick
                        FineY = TFineY;
                        CourseY = TCourseY;
                        NTY = TNTY;
                    }
                }
            }

            if (this.scanline < 240)
            {
                // Visible scanlines and pre-render line
                if (this.cycle == 0)
                {
                    // Idle cycle
                    //if ((this.scanline == 0) && this.OddFrame)
                    //{
                    //    // Odd frame skip
                    //    this.cycle = 1;
                    //}
                }
                else if ((this.cycle <= 256) || (this.cycle >= 321 && this.cycle <= 337))
                {
                    ShiftBGShifters();

                    // More detailed, see diagram: https://wiki.nesdev.com/w/images/4/4f/Ppu.svg
                    switch (this.cycle % 8)
                    {
                        case 1:
                            // there is room for the next byte (tile) of data
                            LoadBGShifterNext();

                            // get the next tile id from the nametable
                            BGNextTileID = this[0x2000 | (V & 0x0fff)];  // 12 bit adress within nametables
                            break;

                        case 3:
                            // get the corresponding attribute adress
                            // this is in the same nametable (NTY, NTX), but starts at 
                            // 0010 (NTY)(NTX)11 11YY YXXX 
                            // where YYY are the top 3 bits of CourseY, and XXX the top 3 of CourseX
                            // this is because of the way the attribute tables are split: https://wiki.nesdev.com/w/index.php/PPU_attribute_tables

                            BGNextTileAttribute = this[
                                0x23C0 + // the ones in the binary formula
                                (NTY << 11) +
                                (NTX << 10) +
                                ((CourseY >> 2) << 3) +
                                (CourseX >> 2)
                                ];

                            // This must be split into the 4 quadrants of the 32x32 pixel attribute "region"
                            // Attribute bit is BR BL TR TL (bottom/top right/left)
                            // left if CourseX % 2 == 0
                            // top if CourseY % 2 == 0

                            // if bottom: ignore first 4 bits, so shift right
                            if (CourseY % 2 == 1)
                            {
                                BGNextTileAttribute >>= 4;
                            }
                            // if right: ignore first 2 bits, so shift right
                            if (CourseX % 2 == 1)
                            {
                                BGNextTileAttribute >>= 2;
                            }

                            // then we end up with the relevant 2 bits on the rightmost end of this value
                            BGNextTileAttribute &= 0x03;
                            break;
                        case 5:
                            // Fetch background pattern low byte
                            BGNextPatternHigh = this[
                                (BGTileSelect << 12) +  // pattern table $0000 or $1000
                                (BGNextTileID << 4) +
                                (FineY)
                                ];
                            break;

                        case 7:
                            // Fetch background pattern high byte
                            BGNextPatternHigh = this[
                                (BGTileSelect << 12) +  // pattern table $0000 or $1000
                                (BGNextTileID << 4) +
                                (FineY + 8)             // 8 below is high byte
                                ];
                            break;

                        default:
                            break;
                    }
                }
                else if (this.cycle <= 320)
                {
                    // Garbage fetches

                }
                else
                {
                    // Ignored Nametable Fetches
                }
            }
            else if (this.scanline == 240)
            {
                // Idle scanline
            }
            else if (this.scanline == 241)
            {
                if (this.cycle == 1)
                {
                    // set VBlank flag to 1 on post-render line
                    this.VBlank = 1;
                    if (this.NMIEnable == 1)
                    {
                        this.ThrowNMI = true;
                    }

                    this.FinishedFrame = true;
                    this.OddFrame ^= true;
                }
            }
            else
            {
                // VBlank lines
            }

            // if rendering is enabled (from https://wiki.nesdev.com/w/index.php/PPU_scrolling#At_dot_256_of_each_scanline)
            if ((SpriteEnable == 1) || (BGEnable == 1))
            {
                if (cycle == 256)
                {
                    // inc vert(v)
                    IncrementCourseY();
                }
                else if (cycle == 257)
                {
                    // hori(v) == hori(t)
                    NTX = TNTX;
                    CourseX = TCourseX;
                    LoadBGShifterNext();
                }
                else if ((cycle == 328) || (cycle == 336) || ((cycle < 256) && (cycle > 0) && ((cycle % 8) == 0)))
                {
                    // Inc hori(v)
                    IncrementCourseX();
                }
            }

            byte BGPattern;  // value of the pixel to be rendered, calculated from the patterns (values 00, 01, 10, 11)
                             // determines the color within the palette
            byte BGPalette;  // index of the palette used, calculated from the attribute shifter

            // Actually display pixel on screen
            if (BGEnable == 1)
            {
                // relevant bit to get from the shifters by fine x scrolling
                UInt16 BitMask = (UInt16)(0x8000 >> FineX);

                byte BGPatternLow = (byte)(((BGShifterPatternLow & BitMask) > 0) ? 1 : 0);
                byte BGPatternHigh = (byte)(((BGShifterPatternHigh & BitMask) > 0) ? 1 : 0);

                BGPattern = (byte)((BGPatternHigh << 1) | BGPatternLow);

                byte BGPaletteLow = (byte)(((BGShifterAttributeLow & BitMask) > 0) ? 1 : 0);
                byte BGPaletteHigh = (byte)(((BGShifterAttributeHigh & BitMask) > 0) ? 1 : 0);

                BGPalette = (byte)((BGPaletteHigh << 1) | BGPaletteLow);

                this.SetPixel(this.cycle - 1, this.scanline, 0, BGPalette, BGPattern);
            }


            cycle++;
            if (cycle > 340)
            {
                cycle = 0;
                scanline++;
                if (scanline > 260)
                {
                    scanline = -1;
                }
            }
        }

        // for testing purposes
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
                        lower = this.PatternTable[(left * 0x1000) + (SpriteTableTileY * 0x100) + (SpriteTableTileX * 0x10) + row];
                        upper = this.PatternTable[(left * 0x1000) + (SpriteTableTileY * 0x100) + (SpriteTableTileX * 0x10) + row + 8];

                        for (byte bit = 0; bit < 8; bit++)
                        {
                            this.SetPixel(8 * SpriteTableTileX + (7 - bit), (8 * SpriteTableTileY + row),
                                PaletteNumber / 4, PaletteNumber % 4, (upper & 0x01) + (lower & 0x01));

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
