using System;
using System.IO;

namespace NesEmulator.Mappers
{
    class Mapper_001 : Mapper
    {
        /*
        https://wiki.nesdev.com/w/index.php/MMC1
        CPU $6000-$7FFF: 8 KB PRG RAM bank, (optional)
        CPU $8000-$BFFF: 16 KB PRG ROM bank, either switchable or fixed to the first bank
        CPU $C000-$FFFF: 16 KB PRG ROM bank, either fixed to the last bank or switchable
        PPU $0000-$0FFF: 4 KB switchable CHR bank
        PPU $1000-$1FFF: 4 KB switchable CHR bank
        Through writes to the MMC1 control register,
            it is possible for the program to swap the fixed and switchable PRG ROM banks
            or to set up 32 KB PRG bankswitching (like BNROM),
            but most games use the default setup, which is similar to that of UxROM.

        */

        private byte PRGSize, CHRSize;
        private byte[,] PRGROM;
        private byte[,] CHRROM;

        private MirrorType _Mirror;

        byte LowPRGBank, HighPRGBank;
        byte LowCHRBank, HighCHRBank;

        private byte[] ExpansionRÖM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)

        public Mapper_001(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize) : base(fs, m)
        {
            this._Mirror = m;
            this.PRGSize = PRGSize;
            this.CHRSize = CHRSize;

            this.PRGROM = new byte[this.PRGSize, 0x4000];
            this.CHRROM = new byte[this.CHRSize, 0x2000];
            this.ExpansionRÖM = new byte[0x8000 - 0x4020];

            for (int i = 0; i < this.PRGSize; i++)
            {
                for (int j = 0; j < 0x4000; j++)
                {
                    this.PRGROM[i, j] = (byte)fs.ReadByte();
                }
            }

            for (int i = 0; i < this.CHRSize; i++)
            {
                for (int j = 0; j < 0x2000; j++)
                {
                    this.PRGROM[i, j] = (byte)fs.ReadByte();
                }
            }

            this.LowPRGBank = 0;                          // first
            this.HighPRGBank = (byte)(this.PRGSize - 1);  // last
            this.LowCHRBank = 0;
            this.HighCHRBank = (byte)(this.CHRSize - 1);
        }

        public override MirrorType Mirror
        {
            get
            {
                return this._Mirror;
            }
            set
            {
                // todo
                throw new Exception("Cannot change mirror mode for Mapper 001");
            }
        }

        public override byte cpuRead(int index)
        {
            if (index < 0x8000)
            {
                return this.ExpansionRÖM[index - 0x4020];
            }
            else if (index < 0xc000)
            {
                return this.PRGROM[this.LowPRGBank, index - 0x8000];
            }
            else
            {
                // Might go over 0x10000, we just mirror this down. This should not happen though
                return this.PRGROM[this.HighPRGBank, index - 0xc000];
            }
        }

        public override void cpuWrite(int index, byte value)
        {
            if (index < 0x8000)
            {
                this.ExpansionRÖM[index - 0x4020] = value;
            }
            else if (index < 0xa000)
            {
                // Register 0
                this.Mirror = (value & 0x01) == 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                if ((value & 0x02) > 0)
                {
                    this.Mirror = MirrorType.SingleScreen;
                }


            }
        }

        public override byte ppuRead(int index)
        {
            if (index < 0x1000)
            {
                return this.CHRROM[this.LowCHRBank, index];
            }
            else if (index < 0x2000)
            {
                return this.CHRROM[this.HighCHRBank, index - 0x1000];
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }

        public override void ppuWrite(int index, byte value)
        {
            if (index < 0x1000)
            {
                this.CHRROM[this.LowCHRBank, index] = value;
            }
            else if (index < 0x2000)
            {
                this.CHRROM[this.HighCHRBank, index - 0x1000] = value;
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }
    }
}
