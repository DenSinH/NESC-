using System;
using System.IO;

namespace NesEmulator.Mappers
{
    class Mapper_000 : Mapper
    {
        /*
        https://wiki.nesdev.com/w/index.php/NROM#Overview
        PRG ROM size: 16 KiB for NROM-128, 32 KiB for NROM-256 (DIP-28 standard pinout)
        PRG ROM bank size: Not bankswitched
        PRG RAM: 2 or 4 KiB, not bankswitched, only in Family Basic (but most emulators provide 8)
        CHR capacity: 8 KiB ROM (DIP-28 standard pinout) but most emulators support RAM
        CHR bank size: Not bankswitched, see CNROM
        Nametable mirroring: Solder pads select vertical or horizontal mirroring
        Subject to bus conflicts: Yes, but irrelevant
        */

        private byte PRGSize, CHRSize;
        private byte[] PRGROM;
        private byte[] CHRROM;
        private byte[] ExpansionROM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)
        private MirrorType _Mirror;

        public Mapper_000(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize) : base(fs, m)
        {
            this._Mirror = m;
            this.PRGSize = PRGSize;
            this.CHRSize = CHRSize;

            this.PRGROM = new byte[0x4000 * this.PRGSize];
            this.CHRROM = new byte[0x2000 * this.CHRSize];
            this.ExpansionROM = new byte[0x8000 - 0x4020];

            for (int i = 0; i < this.PRGROM.Length; i++)
            {
                this.PRGROM[i] = (byte)fs.ReadByte();
            }

            for (int i = 0; i < this.CHRROM.Length; i++)
            {
                this.CHRROM[i] = (byte)fs.ReadByte();
            }

        }

        public override MirrorType Mirror
        {
            get
            {
                return this._Mirror;
            }
            set
            {
                throw new Exception("Cannot change mirror mode for Mapper 000");
            }
        }

        public override byte CPURead(int index)
        {
            if (index < 0x8000)
            {
                return this.ExpansionROM[index - 0x4020];
            }
            else
            {
                // Might go over 0x10000, we just mirror this down. This should not happen though
                return this.PRGROM[(index - 0x8000) % this.PRGROM.Length];
            }
        }

        public override void CPUWrite(int index, byte value)
        {
            if (index < 0x8000)
            {
                this.ExpansionROM[index - 0x4020] = value;
            }
            else
            {
                // Might go over 0x10000, we just mirror this down. This should not happen though
                this.PRGROM[(index - 0x8000) % this.PRGROM.Length] = value;
            }
        }

        public override byte PPURead(int index)
        {
            if (index < 0x2000)
            {
                return this.CHRROM[index];
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }

        public override void PPUWrite(int index, byte value)
        {
            if (index < 0x2000)
            {
                this.CHRROM[index] = value;
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }
    }
}
