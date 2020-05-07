using System;
using System.IO;

namespace NesEmulator.Mappers
{
    class Mapper_002 : Mapper
    {
        /*
        https://wiki.nesdev.com/w/index.php/UxROM#Overview
        CPU $8000-$BFFF: 16 KB switchable PRG ROM bank
        CPU $C000-$FFFF: 16 KB PRG ROM bank, fixed to the last bank
        */

        private byte PRGSize, CHRSize;
        private byte[,] PRGROM;
        private byte BankSelect;

        private byte[] CHRROM;
        private byte[] ExpansionROM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)
        private MirrorType _Mirror;

        public Mapper_002(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize) : base(fs, m)
        {
            this._Mirror = m;
            this.PRGSize = PRGSize;
            this.CHRSize = CHRSize;
            this.BankSelect = 0;

            this.PRGROM = new byte[this.PRGSize, 0x4000];
            this.CHRROM = new byte[0x2000];  // No window
            this.ExpansionROM = new byte[0x8000 - 0x4020];

            for (int i = 0; i < this.PRGSize; i++)
            {
                for (int j = 0; j < 0x4000; j++)
                {
                    this.PRGROM[i, j] = (byte)fs.ReadByte();
                }
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
                throw new Exception("Cannot change mirror mode for Mapper 002");
            }
        }

        public override byte CPURead(int index)
        {
            if (index < 0x8000)
            {
                return this.ExpansionROM[index - 0x4020];
            }
            else if (index < 0xc000)
            {
                return this.PRGROM[this.BankSelect, index - 0x8000];
            }
            else
            {
                // Might go over 0x10000, we just mirror this down. This should not happen though
                return this.PRGROM[this.PRGSize -1 , index - 0xc000];
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
                this.BankSelect = (byte)(value & 0x0f);
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
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 002", index));
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
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 002", index));
            }
        }
    }
}
