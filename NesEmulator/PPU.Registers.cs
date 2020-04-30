using System;

namespace NesEmulator
{
    partial class PPU
    {

        // Register info from https://wiki.nesdev.com/w/index.php/PPU_registers
        // ! LOCK ALL THESE WHEN USING !

        private byte NMIEnable, MasterSlave, SpriteHeight, BGTileSelect, SpriteTileSelect, IncrementMode, NametableSelect;
        public byte PPUCTRL     // VPHB SINN : NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B),
        {                        // sprite tile select (S), increment mode (I), nametable select (NN)
            // Write Only
            set
            {
                NMIEnable = (byte)(value >> 7);
                MasterSlave = (byte)((value >> 6) & 0x01);
                SpriteHeight = (byte)((value >> 5) & 0x01);
                BGTileSelect = (byte)((value >> 4) & 0x01);
                SpriteTileSelect = (byte)((value >> 3) & 0x01);
                IncrementMode = (byte)((value >> 2) & 0x01);
                NametableSelect = (byte)(value & 0x03);
            }
        }

        // ignored for now
        byte ColorEmphasis, SpriteEnable, BGEnable, SpriteLeftColumnEnable, BGLeftColumnEnable, Greyscale;
        public byte PPUMASK;    // BGRs bMmG: color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M),
                                 // background left column enable (m), greyscale (G)

        byte VBlank, Sprite0Hit, SpriteOverflow;
        public byte PPUSTATUS  // VSO- ----: vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006
        {
            // Read Only
            get
            {
                // Reads reset the address latch
                AddressLatch = true;
                _ppuaddr = 0;

                // Reads reset the VBlank bit to 0
                VBlank = 0; 

                // 1 is supposed to be VBlank, now 1 for testing
                return (byte)((1 << 7) + (Sprite0Hit << 6) + (SpriteOverflow << 5));
            }
        }

        private byte _oamaddr = 0;
        public byte OAMADDR    // aaaa aaaa: OAM read/write address
        {
            // Write Only
            set
            {
                _oamaddr = value;
            }
        }
        
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

        // Ignored for now
        public byte PPUSCROLL;  // xxxx xxxx: fine scroll position (two writes: X scroll, Y scroll)

        private UInt16 _ppuaddr;
        private bool AddressLatch = true;
        private byte DataBuffer = 0;


        public byte PPUADDR    // aaaa aaaa: PPU read/write address (two writes: most significant byte, least significant byte)
        {
            // Write x2
            set
            {
                if (AddressLatch)
                {
                    _ppuaddr = (UInt16)((_ppuaddr & 0x00ff) | (value << 8));
                    
                } else
                {
                    _ppuaddr = (UInt16)((_ppuaddr & 0xff00) | value);
                }
                AddressLatch ^= true;
            }
        }

        public byte PPUDATA    // dddd dddd: PPU data read/write
        {
            get
            {
                byte data = DataBuffer;
                DataBuffer = this[_ppuaddr];

                if (_ppuaddr >= 0x3f00)
                {
                    // Pallete RAM can be accessed directly
                    data = DataBuffer;
                }
                _ppuaddr += (UInt16)((IncrementMode == 1) ? 32 : 1);
                return data;
            }
            set
            {
                this[_ppuaddr] = value;
                _ppuaddr += (UInt16)((IncrementMode == 1) ? 32 : 1);
            }
        }
        public byte OAMDMA;     // aaaa aaaa: OAM DMA high address

    }
}
