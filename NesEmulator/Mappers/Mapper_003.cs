using System;
using System.IO;

namespace NesEmulator.Mappers
{
    class Mapper_003 : Mapper
    {
        /*
        https://wiki.nesdev.com/w/index.php/UxROM#Overview
        CPU $8000-$BFFF: 16 KB switchable PRG ROM bank
        CPU $C000-$FFFF: 16 KB PRG ROM bank, fixed to the last bank
        */

        private byte PRGSize, CHRSize;
        private byte[] PRGROM;
        private byte[] CHRROM;
        private byte BankSelect;

        private byte[] ExpansionROM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)
        private MirrorType _Mirror;

        public Mapper_003(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize) : base(fs, m)
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

        public override byte cpuRead(int index)
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

        public override void cpuWrite(int index, byte value)
        {
            if (index < 0x8000)
            {
                this.ExpansionROM[index - 0x4020] = value;
            }
            else
            {
                this.BankSelect = (byte)(value & 0x03);
            }
        }

        public override byte ppuRead(int index)
        {
            if (index < 0x2000)
            {
                return this.CHRROM[this.BankSelect * 0x2000 + index];
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }

        public override void ppuWrite(int index, byte value)
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
