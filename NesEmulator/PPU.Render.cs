using System;

namespace NesEmulator
{
    partial class PPU
    {
        public bool ThrowNMI = false;
        public bool FinishedFrame = false;
        bool OddFrame = false;

        private int scanline = 0;
        private int cycle = 0;

        private byte[,] SecondaryOAM = new byte[8, 4];          // sprites on next scanline
        private byte SpritesFoundNext, SpritesFound;
        private bool FinishedEvaluation;
        private byte n;                                         // sprite data indexer, as in https://wiki.nesdev.com/w/index.php/PPU_sprite_evaluation

        private byte[] SpriteShiftersPatternLow = new byte[8];  // sprite high tile data
        private byte[] SpriteShiftersPatternHigh = new byte[8]; // sprite low tile data
        private byte[] SpriteLatches = new byte[8];             // sprite attribute bytes
        private byte[] SpriteCounters = new byte[8];            // sprite X position counters;
                                                                // I'm not decrementing them like happens in the NES, but just comparing them manually
        private bool Sprite0Active, Sprite0ActiveNext, Sprite0Rendered;

        private byte BGNextTileID;
        private byte BGNextTileAttribute;

        private byte BGNextPatternLow;
        private byte BGNextPatternHigh;

        private UInt16 BGShifterPatternLow;
        private UInt16 BGShifterPatternHigh;
        private UInt16 BGShifterAttributeLow;
        private UInt16 BGShifterAttributeHigh;

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
             BGSpriteSelect: BG or Sprite selected (0: BG, 1: Sprite)
             PaletteStart: Highest 3 bits of chosen palette in palette ram 
             PaletteInternal: Lowest 2 bits of chosen palette in palette ram
             */
            if ((x >= 0) && (x < 0x100) && (y >= 0) && (y < 0xf0))
            { 
                lock (this.nes.display)
                {
                    this.nes.display[0x100 * y + x] = this.palette[
                        this[0x3f00 | (BGSpriteSelect << 4) | (PaletteStart << 2) | PaletteInternal] & 0x3f
                ];
                }
            }
            
        }

        public void Step()
        {
            /*
            ======================
             BACKGROUND RENDERING
            ======================
             */
            if (this.scanline == -1)
            {
                // Pre-render scanline
                if (this.cycle == 1)
                {
                    // Set VBlank flag to 0 on second cycle
                    this.VBlank = 0;
                    this.Sprite0Hit = 0;
                    this.SpriteOverflow = 0;
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
                else if (cycle == 339)
                {
                    if ((this.scanline == 0) && this.OddFrame)
                    {
                        // odd frame skip
                        this.cycle = 340;
                    }
                }
            }

            if (this.scanline < 240)
            {
                // Visible scanlines and pre-render line
                if (this.cycle == 0)
                {
                    // Idle cycle
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
                                0x23C0 | // the ones in the binary formula
                                (NTY << 11) |
                                (NTX << 10) |
                                ((CourseY >> 2) << 3) |
                                (CourseX >> 2)
                                ];

                            // This must be split into the 4 quadrants of the 32x32 pixel attribute "region"
                            // Attribute bit is BR BL TR TL (bottom/top right/left)
                            // left if CourseX % 4 == 0, 1
                            // top if CourseY % 4 == 0, 1

                            // if bottom: ignore first 4 bits, so shift right
                            if (CourseY % 4 > 1)
                            {
                                BGNextTileAttribute >>= 4;
                            }
                            // if right: ignore first 2 bits, so shift right
                            if (CourseX % 4 > 1)
                            {
                                BGNextTileAttribute >>= 2;
                            }

                            // then we end up with the relevant 2 bits on the rightmost end of this value
                            BGNextTileAttribute &= 0x03;
                            break;
                        case 5:
                            // Fetch background pattern low byte
                            BGNextPatternLow = this.PatternTable[
                                (BGTableSelect << 12) |  // pattern table $0000 or $1000
                                (BGNextTileID << 4) |
                                (FineY)
                                ];
                            break;

                        case 7:
                            // Fetch background pattern high byte
                            BGNextPatternHigh = this.PatternTable[
                                (BGTableSelect << 12) |  // pattern table $0000 or $1000
                                (BGNextTileID << 4) |
                                (FineY + 8)              // 8 below is high byte
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

                // if rendering is enabled (from https://wiki.nesdev.com/w/index.php/PPU_scrolling#At_dot_256_of_each_scanline)
                if ((SpriteEnable == 1) || (BGEnable == 1))
                {
                    if (cycle == 256)
                    {
                        // inc vert(v)
                        IncrementY();
                    }
                    else if (cycle == 257)
                    {
                        // hori(v) == hori(t)
                        NTX = TNTX;
                        CourseX = TCourseX;
                        // LoadBGShifterNext();  why did I do this?
                    }
                    else if ((cycle == 328) || (cycle == 336) || ((cycle < 256) && (cycle > 0) && ((cycle % 8) == 0)))
                    {
                        // Inc hori(v)
                        IncrementCourseX();
                    }
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

            /*
            ======================
              SPRITE   RENDERING
            ======================
             */

            if ((this.scanline >= 0) && (this.scanline < 240))
            {
                if (this.cycle == 0)
                {
                    // idle cycle
                    // I reset n and FinishedEvaluation here too for ease
                    FinishedEvaluation = false;
                    n = 0; SpritesFoundNext = 0;

                    // reset Secondary OAM and Sprite activeness
                    for (int i = 0; i < 8; i++)
                    {
                        for (int att = 0; att < 4; att++)
                        {
                            this.SecondaryOAM[i, att] = 0xff;
                        }
                    }
                }
                else if (this.cycle <= 64)
                {
                    // resetting secondary OAM
                    // for ease I just do this on cycle 0

                }
                else if (this.cycle <= 256)
                {
                    if (this.cycle % 2 == 1)
                    {
                        if (!FinishedEvaluation)
                        {
                            // odd cycles: I do everything in odd cycles, because it does not make much sense to copy data twice, this just makes it slower
                            SecondaryOAM[SpritesFoundNext, 0] = (byte)(this.oam[4 * n] + 0);

                            if ((SecondaryOAM[SpritesFoundNext, 0] + ((SpriteHeight == 1) ? 16 : 8) > this.scanline) &&
                                (SecondaryOAM[SpritesFoundNext, 0] <= this.scanline))
                            {
                                for (byte m = 1; m < 4; m++)
                                {
                                    SecondaryOAM[SpritesFoundNext, m] = this.oam[4 * n + m];
                                }

                                if (n == 0)
                                {
                                    // We found sprite 0!
                                    Sprite0ActiveNext = true;
                                }

                                SpritesFoundNext++;
                            }

                            n++;
                            if (n == 64 || SpritesFoundNext == 8)
                            {
                                FinishedEvaluation = true;

                                // Detect for spriteoverflow
                                // There is a bug in this, which I also emulate here
                                // OAM is scanned diagonally, instead of linearly to check for matching y-coordinates
                                if ((BGEnable == 1) || (SpriteEnable) == 1)
                                {
                                    if (SpritesFoundNext == 8)
                                    {
                                        int m = 0;
                                        for (byte _n = n; _n < 64; _n++)
                                        {
                                            if ((this.oam[4 * _n + m] + ((SpriteHeight == 1) ? 16 : 8) > this.scanline) &&
                                                (this.oam[4 * _n + m] <= this.scanline))
                                            {
                                                SpriteOverflow = 1;
                                                break;
                                            }
                                            m = (m + 1) & 0x3;
                                        }
                                    }
                                }

                                // reset n to use in the following cycles
                                n = 0;
                            }
                        }
                    }
                }
                else if (this.cycle == 257)
                {
                    // This is just to set the counters to 0xff for unused tiles
                    // The first cycle would just fetch a Y-coordinate, which is irrelevant, so we can do this here
                    for (int i = SpritesFoundNext; i < 8; i++)
                    {
                        SpriteCounters[i] = 0xff;
                    }
                    Sprite0Active = Sprite0ActiveNext;
                    Sprite0ActiveNext = false;
                    SpritesFound = SpritesFoundNext;
                }
                else if (this.cycle < 320)
                {
                    if (n < SpritesFoundNext)
                    {
                        switch (this.cycle % 8)
                        {
                            case 1:
                                // Read Y-Coordinate, this is not needed
                                break;
                            case 2:
                                // Read Attributes
                                SpriteLatches[n] = SecondaryOAM[n, 2];
                                break;
                            case 3:
                                // Read X-Coordinate
                                SpriteCounters[n] = SecondaryOAM[n, 3];
                                break;
                            case 4:
                                // Fetch Low Sprite Tile Byte
                                if (SpriteHeight == 0)
                                {
                                    // 8x8 mode
                                    if ((SecondaryOAM[n, 2] & 0x80) == 0)
                                    {
                                        // No vertical mirroring
                                        SpriteShiftersPatternLow[n] = this.PatternTable[
                                            (SpriteTableSelect << 12) |
                                            (SecondaryOAM[n, 1] << 4) |
                                            (this.scanline - SecondaryOAM[n, 0])
                                            ];
                                    }
                                    else
                                    {
                                        // Vertical mirroring
                                        SpriteShiftersPatternLow[n] = this.PatternTable[
                                            (SpriteTableSelect << 12) |
                                            (SecondaryOAM[n, 1] << 4) |
                                            (7 - (this.scanline - SecondaryOAM[n, 0]))
                                            ];
                                    }
                                    
                                } 
                                else
                                {
                                    // 8x16 mode
                                    if ((SecondaryOAM[n, 2] & 0x80) == 0)
                                    {
                                        // No vertical mirroring
                                        if (this.scanline - SecondaryOAM[n, 0] < 8)
                                        {
                                            // Top tile
                                            SpriteShiftersPatternLow[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            ((SecondaryOAM[n, 1] & 0xfe) << 4) |
                                            (this.scanline - SecondaryOAM[n, 0])
                                            ];
                                        }
                                        else
                                        {
                                            // Bottom tile
                                            SpriteShiftersPatternLow[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            (((SecondaryOAM[n, 1] & 0xfe) + 1) << 4) |
                                            (this.scanline - SecondaryOAM[n, 0] - 8)
                                            ];
                                        }
                                        
                                    }
                                    else
                                    {
                                        // Vertical mirroring
                                        if (this.scanline - SecondaryOAM[n, 0] < 8)
                                        {
                                            // Top tile, turns into bottom tile
                                            SpriteShiftersPatternLow[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            (((SecondaryOAM[n, 1] & 0xfe) + 1) << 4) |
                                            (7 - (this.scanline - SecondaryOAM[n, 0]))
                                            ];
                                        }
                                        else
                                        {
                                            // Bottom tile, turns into top tile
                                            SpriteShiftersPatternLow[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            ((SecondaryOAM[n, 1] & 0xfe) << 4) |
                                            (7 - (this.scanline - SecondaryOAM[n, 0] - 8))
                                            ];
                                        }
                                    }
                                }

                                // Mirror the loaded sprite horizontally if necessary
                                if ((SecondaryOAM[n, 2] & 0b0100_0000) > 0)
                                {
                                    byte Mirrored = 0;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        Mirrored |= (byte)((((SpriteShiftersPatternLow[n] >> i) & 0x01) > 0 ? 1 : 0) << (7 - i));
                                    }
                                    SpriteShiftersPatternLow[n] = Mirrored;
                                }

                                break;
                            case 6:
                                // Fetch Low Sprite Tile Byte
                                if (SpriteHeight == 0)
                                {
                                    // 8x8 mode
                                    if ((SecondaryOAM[n, 2] & 0x80) == 0)
                                    {
                                        // No vertical mirroring
                                        SpriteShiftersPatternHigh[n] = this.PatternTable[
                                            (SpriteTableSelect << 12) |
                                            (SecondaryOAM[n, 1] << 4) |
                                            (8 + (this.scanline - SecondaryOAM[n, 0]))
                                            ];
                                    }
                                    else
                                    {
                                        // Vertical mirroring
                                        SpriteShiftersPatternHigh[n] = this.PatternTable[
                                            (SpriteTableSelect << 12) |
                                            (SecondaryOAM[n, 1] << 4) |
                                            (0xf - (this.scanline - SecondaryOAM[n, 0]))
                                            ];
                                    }
                                }
                                else
                                {
                                    // 8x16 mode
                                    if ((SecondaryOAM[n, 2] & 0x80) == 0)
                                    {
                                        // No vertical mirroring
                                        if (this.scanline - SecondaryOAM[n, 0] < 8)
                                        {
                                            // Top tile
                                            SpriteShiftersPatternHigh[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            ((SecondaryOAM[n, 1] & 0xfe) << 4) |
                                            (8 + (this.scanline - SecondaryOAM[n, 0]))
                                            ];
                                        }
                                        else
                                        {
                                            // Bottom tile
                                            SpriteShiftersPatternHigh[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            (((SecondaryOAM[n, 1] & 0xfe) + 1) << 4) |
                                            (8 + (this.scanline - SecondaryOAM[n, 0] - 8))
                                            ];
                                        }

                                    }
                                    else
                                    {
                                        // Vertical mirroring
                                        if (this.scanline - SecondaryOAM[n, 0] < 8)
                                        {
                                            // Top tile, turns into bottom tile
                                            SpriteShiftersPatternHigh[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            (((SecondaryOAM[n, 1] & 0xfe) + 1) << 4) |
                                            (0xf - (this.scanline - SecondaryOAM[n, 0]))
                                            ];
                                        }
                                        else
                                        {
                                            // Bottom tile, turns into top tile
                                            SpriteShiftersPatternHigh[n] = this.PatternTable[
                                            ((SecondaryOAM[n, 1] & 0x01) << 12) |
                                            ((SecondaryOAM[n, 1] & 0xfe) << 4) |
                                            (0xf - (this.scanline - SecondaryOAM[n, 0] - 8))
                                            ];
                                        }
                                    }
                                }

                                // Mirror the loaded sprite horizontally if necessary
                                if ((SecondaryOAM[n, 2] & 0b0100_0000) > 0)
                                {
                                    byte Mirrored = 0;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        Mirrored |= (byte)((((SpriteShiftersPatternHigh[n] >> i) & 0x01) > 0 ? 1 : 0) << (7 - i));
                                    }
                                    SpriteShiftersPatternHigh[n] = Mirrored;
                                }
                                break;
                            case 0:
                                n++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            /*
            ===================
              PROCESSING PIXEL
            ===================
            */
            if ((BGEnable == 1) || (SpriteEnable == 1))
            {
                if (((this.cycle >= 9) && (this.cycle < 256)) || (SpriteLeftColumnEnable == 1) || (BGLeftColumnEnable == 1))
                {
                    byte SpritePixel = 0;
                    byte SpritePalette = 0;
                    bool SpritePriority = true;  // true is behind background

                    if (SpriteEnable == 1)
                    {
                        if ((this.cycle >= 0) && (this.cycle < 256))
                        {
                            for (int i = 0; i < SpritesFound; i++)
                            {
                                if ((SpriteCounters[i] < this.cycle) && (SpriteCounters[i] >= this.cycle - ((SpriteHeight == 1) ? 16 : 8)))
                                {
                                    // Horizontal flipping is done when loading
                                    byte SpritePixelLow = (byte)(((SpriteShiftersPatternLow[i] & 0x80) > 0) ? 1 : 0);
                                    SpriteShiftersPatternLow[i] <<= 1;
                                    byte SpritePixelHigh = (byte)(((SpriteShiftersPatternHigh[i] & 0x80) > 0) ? 1 : 0);
                                    SpriteShiftersPatternHigh[i] <<= 1;

                                    // Sprite overlapping
                                    if ((SpritePixelHigh | SpritePixelLow) > 0)
                                    {
                                        SpritePixel = (byte)((SpritePixelHigh << 1) | SpritePixelLow);

                                        SpritePalette = (byte)(SpriteLatches[i] & 0x3);

                                        SpritePriority = (SpriteLatches[i] & 0x20) > 0;
                                        
                                        if (Sprite0Active && (i == 0))
                                        {
                                            Sprite0Rendered = true;
                                        }
                                    }
                                }
                            }
                        }
                    }


                    byte BGPixel = 0;  // value of the pixel to be rendered, calculated from the patterns (values 00, 01, 10, 11)
                                       // determines the color within the palette
                    byte BGPalette = 0;  // index of the palette used, calculated from the attribute shifter

                    // Actually display pixel on screen
                    if (BGEnable == 1)
                    {
                        // relevant bit to get from the shifters by fine x scrolling
                        UInt16 BitMask = (UInt16)(0x8000 >> FineX);

                        byte BGPixelLow = (byte)(((BGShifterPatternLow & BitMask) > 0) ? 1 : 0);
                        byte BGPixelHigh = (byte)(((BGShifterPatternHigh & BitMask) > 0) ? 1 : 0);

                        BGPixel = (byte)((BGPixelHigh << 1) | BGPixelLow);

                        byte BGPaletteLow = (byte)(((BGShifterAttributeLow & BitMask) > 0) ? 1 : 0);
                        byte BGPaletteHigh = (byte)(((BGShifterAttributeHigh & BitMask) > 0) ? 1 : 0);

                        BGPalette = (byte)((BGPaletteHigh << 1) | BGPaletteLow);
                    }

                    /*
                     Priority multiplexer decision table
                    BG pixel	Sprite pixel	Priority	Output
                    0	        0	            X	        BG ($3F00)
                    0	        1-3	            X	        Sprite
                    1-3	        0	            X	        BG
                    1-3	        1-3	            0	        Sprite
                    1-3	        1-3	            1	        BG
                     */
                    
                    if (((this.cycle > 8) && (this.cycle < 240)) || (SpriteLeftColumnEnable == 1) || (BGLeftColumnEnable == 1))
                    {
                        if ((SpritePixel == 0) || (SpritePriority && (BGPixel != 0)))
                        {
                            this.SetPixel(this.cycle - 8, this.scanline - 8, 0, BGPalette, BGPixel);  // todo: why offset?
                        }
                        else
                        {
                            this.SetPixel(this.cycle - 8, this.scanline - 8, 1, SpritePalette, SpritePixel);  // todo: why offset?
                        }

                        // Sprite0 hit detection
                        if (Sprite0Rendered && (BGPixel != 0) && (SpritePixel != 0))
                        {
                            Sprite0Hit = 1;
                        }
                    }
                    
                }
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
    }
}
