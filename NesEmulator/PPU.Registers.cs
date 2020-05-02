using System;

namespace NesEmulator
{
    partial class PPU
    {

        // Register info from https://wiki.nesdev.com/w/index.php/PPU_registers
        // ! LOCK ALL THESE WHEN USING !

        private byte NMIEnable, MasterSlave, SpriteHeight, BGTileSelect, SpriteTileSelect, IncrementMode;

        /*  $2000  */
        public byte PPUCTRL     // VPHB SINN : NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B),
        {                       // sprite tile select (S), increment mode (I), nametable select (NN)
            // Write Only
            set
            {
                NMIEnable = (byte)(value >> 7);
                MasterSlave = (byte)((value >> 6) & 0x01);
                SpriteHeight = (byte)((value >> 5) & 0x01);
                BGTileSelect = (byte)((value >> 4) & 0x01);
                SpriteTileSelect = (byte)((value >> 3) & 0x01);
                IncrementMode = (byte)((value >> 2) & 0x01);
                TNTY = (byte)((value >> 1) & 0x01);
                TNTX = (byte)(value & 0x01);
            }
        }

        // ignored for now
        byte BlueEmphasis, GreenEmphasis, RedEmphasis, SpriteEnable, BGEnable, SpriteLeftColumnEnable, BGLeftColumnEnable, Greyscale;

        /*  $2001  */
        public byte PPUMASK         // BGRs bMmG: color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M),
        {                           // background left column enable (m), greyscale (G)
            // Write Only
            set
            {
                BlueEmphasis = (byte)(value >> 7);
                GreenEmphasis = (byte)((value >> 6) & 0x01);
                RedEmphasis = (byte)((value >> 5) & 0x01);
                SpriteEnable = (byte)((value >> 4) & 0x01);
                BGEnable = (byte)((value >> 3) & 0x01);
                SpriteLeftColumnEnable = (byte)((value >> 2) & 0x01);
                BGLeftColumnEnable = (byte)((value >> 1) & 0x01);
                Greyscale = (byte)(value & 0x01);
            }
        } 

        byte VBlank, Sprite0Hit, SpriteOverflow;

        /*  $2002  */
        public byte PPUSTATUS  // VSO- ----: vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006
        {
            // Read Only
            get
            {
                // Reads reset the address latch
                w = 0;

                byte data = (byte)((VBlank << 7) + (Sprite0Hit << 6) + (SpriteOverflow << 5));
                // Reads reset the VBlank bit to 0
                VBlank = 0; 
                
                return data;
            }
        }

        private byte _oamaddr = 0;
        /*  $2003   */
        public byte OAMADDR    // aaaa aaaa: OAM read/write address
        {
            // Write Only
            set
            {
                _oamaddr = value;
            }
        }
        
        /*  $2004  */
        public byte OAMDATA    // dddd dddd: OAM data read/write
        {
            // Read/Write
            get
            {
                return oam[_oamaddr];
            }
            set
            {
                oam[_oamaddr] = value;
                _oamaddr++;
            }
        }

        // Scrolling information at https://wiki.nesdev.com/w/index.php/PPU_scrolling
        private byte w = 0;  // first or second toggle of Address Latch
        private byte FineX;
        private byte FineY, NTY, NTX, CourseY, CourseX;  // Nametable X, Nametable Y; V register values

        /* From above mentioned page:
         * The 15 bit registers t and v are composed this way during rendering:
         *   yyy NN YYYYY XXXXX
         *   ||| || ||||| +++++-- coarse X scroll
         *   ||| || +++++-------- coarse Y scroll
         *   ||| ++-------------- nametable select
         *   +++----------------- fine Y scroll
         */

        private UInt16 V
        {
            get
            {
                return (UInt16)(
                        (FineY << 12) +
                        (NTY << 11) +
                        (NTX << 10) +
                        (CourseY << 5) +
                        (CourseX)
                    );
            }
            set
            {
                FineY = (byte)((value >> 12) & 0x07);
                NTY = (byte)((value >> 11) & 0x01);
                NTX = (byte)((value >> 10) & 0x01);
                CourseY = (byte)((value >> 5) & 0x1f);
                CourseX = (byte)(value & 0x1f);
            }
        }

        private byte TFineY, TNTY, TNTX, TCourseY, TCourseX;  // T(emp) register values
        private UInt16 T
        {
            get
            {
                return (UInt16)(
                        (TFineY << 12) +
                        (TNTY << 11) +
                        (TNTX << 10) +
                        (TCourseY << 5) +
                        (TCourseX)
                    );
            }
        }

        private void IncrementCourseX()
        /* From https://wiki.nesdev.com/w/index.php/PPU_scrolling#Wrapping_around*/
        {
            if (CourseX == 31)      // if coarse X == 31
            {
                CourseX = 0;        // coarse X = 0
                NTX ^= 1;           // switch horizontal nametable
            }
            else
            {
                CourseX++;          // increment coarse X
            }
        }

        private void IncrementCourseY()
        /* From https://wiki.nesdev.com/w/index.php/PPU_scrolling#Wrapping_around*/
        {
            if (FineY != 7)                                 // if fine Y < 7 
            {
                FineY++;                                    // increment fine Y
            }
            else
            {
                FineY = 0;                                  // fine Y = 0
                if (CourseY == 29)
                {
                    CourseY = 0;                            // coarse Y = 0
                    NTY ^= 1;                               // switch vertical nametable
                }
                else if (CourseY == 31)
                {
                    CourseY = 0;                            // coarse Y = 0, nametable not switched
                }
                else
                {
                    CourseY++;                              // increment coarse Y
                }
                
            }
        }

        /*  $2005  */
        public byte PPUSCROLL
        {
            // Write x2
            set
            {
                if (w == 0)
                {
                    TCourseX = (byte)((value >> 3) & 0x1f);
                    FineX = (byte)(value & 0x07);
                    w = 1;
                }
                else
                {
                    TCourseY = (byte)((value >> 3) & 0x1f);
                    TFineY = (byte)(value & 0x07);
                    w = 0;
                }
            }
        }

        /*  $2006  */
        public byte PPUADDR    // aaaa aaaa: PPU read/write address (two writes: most significant byte, least significant byte)
        {
            // Write x2
            set
            {
                if (w == 0)
                {
                    TFineY = (byte)((value >> 4) & 0x07);
                    TNTY = (byte)((value >> 3) & 0x01);
                    TNTX = (byte)((value >> 2) & 0x01);
                    TCourseY = (byte)((TCourseY & 0x07) | ((value & 0x03) << 3));
                    w = 1;
                    
                } else
                {
                    TCourseY = (byte)((TCourseY & 0x18) | ((value >> 5) & 0x07));
                    TCourseX = (byte)(value & 0x1f);
                    V = T;
                    w = 0;
                }
            }
        }

        /*  $2007  */
        private byte DataBuffer;
        public byte PPUDATA    // dddd dddd: PPU data read/write
        {
            // todo: outside of rendering strange behavior
            get
            {
                byte data = DataBuffer;
                DataBuffer = this[V];

                if (V >= 0x3f00)
                {
                    // Pallete RAM can be accessed directly
                    data = DataBuffer;
                }

                if (((BGEnable == 1) || (SpriteEnable == 1)) && ((this.scanline >= 0) && (this.scanline < 240) && (this.cycle < 256)))
                {
                    IncrementCourseX();
                    IncrementCourseY();
                }
                else
                {
                    V += (UInt16)((IncrementMode == 1) ? 32 : 1);
                }

                return data;
            }
            set
            {
                this[V] = value;
                
                if (((BGEnable == 1) || (SpriteEnable == 1)) && ((this.scanline >= 0) && (this.scanline < 240) && (this.cycle < 256)))
                {
                    IncrementCourseX();
                    IncrementCourseY();
                }
                else
                {
                    V += (UInt16)((IncrementMode == 1) ? 32 : 1);
                }
            }
        }
        
        public byte OAMDMA;     // aaaa aaaa: OAM DMA high address
    }
}
