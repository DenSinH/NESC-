using System;

namespace NesEmulator
{
    class PPU
    {

        // Register info from https://wiki.nesdev.com/w/index.php/PPU_registers
        // ! LOCK ALL THESE WHEN USING !
        private byte PPUCTRL;    // VPHB SINN : NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B),
                                     // sprite tile select (S), increment mode (I), nametable select (NN)

        private byte PPUMASK;    // BGRs bMmG: color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M),
                                     // background left column enable (m), greyscale (G)

        private byte PPUSTATUS;  // VSO- ----: vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006

        private byte OAMADDR;    // aaaa aaaa: OAM read/write address
        private byte OAMDATA;    // dddd dddd: OAM data read/write
        private byte PPUSCROLL;  // xxxx xxxx: fine scroll position (two writes: X scroll, Y scroll)
        private byte PPUADDR;    // aaaa aaaa: PPU read/write address (two writes: most significant byte, least significant byte)
        private byte PPUDATA;    // dddd dddd: PPU data read/write
        private byte OAMDMA;     // aaaa aaaa: OAM DMA high address

        private PPUMEM mem;
        private PPUOAM oam;

        public PPU(CPU cpu)
        {
            //this.PPUCTRL = cpu.mem.get(0x2000);
            //this.PPUMASK = cpu.mem.get(0x2001);
            //this.PPUSTATUS = cpu.mem.get(0x2002);
            //this.OAMADDR = cpu.mem.get(0x2003);
            //this.OAMDATA = cpu.mem.get(0x2004);
            //this.PPUSCROLL = cpu.mem.get(0x2005);
            //this.PPUADDR = cpu.mem.get(0x2006);
            //this.PPUDATA = cpu.mem.get(0x2007);
            //this.OAMDMA = cpu.mem.get(0x4014);

            this.mem = new PPUMEM();
            this.oam = new PPUOAM();
        }

    }
}
