using System;
using System.IO;

namespace NesEmulator.Mappers
{
    class Mapper_007 : Mapper
    {
        private byte PRGSize, CHRSize;
        private byte[,] PRGROM;
        private byte[,] CHRRAM;
        private byte[] ExpansionROM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)

        private byte VRAMSelect, BankSelect;
        
        private MirrorType _Mirror;

        public Mapper_007(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize) : base(fs, m)
        {
            this.PRGSize = PRGSize;
            this.CHRSize = 1;

            this._Mirror = m;

            this.PRGROM = new byte[this.PRGSize >> 1, 0x8000];  // 32KB banks
            this.CHRRAM = new byte[this.CHRSize << 1, 0x1000];   // 4KB bank
            this.ExpansionROM = new byte[0x8000 - 0x4020];

            for (int i = 0; i < PRGSize >> 1; i++)
            {
                for (int j = 0; j < 0x8000; j++)
                {
                    this.PRGROM[i, j] = (byte)fs.ReadByte();
                }
            }
        }

        public override MirrorType Mirror { get => this._Mirror; set => throw new NotImplementedException(); }

        public override byte CPURead(int index)
        {
            if (index < 0x8000)
            {
                return this.ExpansionROM[index - 0x4020];
            }
            else
            {
                return this.PRGROM[BankSelect, index - 0x8000];
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
                this.BankSelect = (byte)(value & 0x07);
                this.VRAMSelect = (byte)(((value & 0x10) > 0) ? 1 : 0);
            }
        }

        public override byte PPURead(int index)
        {
            if (index < 0x2000)
            {
                return this.CHRRAM[index >> 12, index & 0xfff];
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 007", index));
            }
        }

        public override void PPUWrite(int index, byte value)
        {
            if (index < 0x2000)
            {
                this.CHRRAM[index >> 12, index & 0xfff] = value;
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 007", index));
            }
        }
    }
}
