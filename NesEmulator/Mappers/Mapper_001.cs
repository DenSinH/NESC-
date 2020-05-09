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
        */

        private byte PRGSize, CHRSize;
        private byte[,] PRGROM;
        private byte[,] CHRROM;
        private byte[] ExpansionROM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)

        private byte LoadRegister;
        private byte WriteCount;

        private byte Mirroring, PRGROMBankMode, CHRROMBankMode;
        private byte Control
        {
            get
            {
                return (byte)(
                    (CHRROMBankMode << 4) |
                    (PRGROMBankMode << 2) |
                    Mirroring
                    );
            }
            set
            {
                Mirroring = (byte)(value & 0x03);
                PRGROMBankMode = (byte)((value >> 2) & 0x03);
                CHRROMBankMode = (byte)((value >> 5) & 0x01);
            }
        }

        private byte CHRBank0, CHRBank1, PRGBank;
        private bool CHRRAMEnabled = false;
        private byte[] CHRRAM;

        public Mapper_001(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize) : base(fs, m)
        {
            this.PRGSize = PRGSize;
            this.CHRSize = CHRSize;

            if (CHRSize == 0)
            {
                // NesDev wiki (MMC1 Page):
                // MMC1 can do CHR banking in 4KB chunks. Known carts with CHR RAM have 8 KiB, so that makes 2 banks
                CHRRAMEnabled = true;
                this.CHRRAM = new byte[0x2000];
            }

            this.PRGROM = new byte[this.PRGSize, 0x4000];  // 16KB banks
            this.CHRROM = new byte[this.CHRSize, 0x1000];   // 4KB banks
            this.ExpansionROM = new byte[0x8000 - 0x4020];

            for (int i = 0; i < PRGSize; i++)
            {
                for (int j = 0; j < 0x4000; j++)
                {
                    this.PRGROM[i, j] = (byte)fs.ReadByte();
                }
            }

            for (int i = 0; i < CHRSize; i++)
            {
                for (int j = 0; j < 0x1000; j++)
                {
                    this.CHRROM[i, j] = (byte)fs.ReadByte();
                }
            }
        }

        public override MirrorType Mirror
        {
            get
            {
                switch (Mirroring)
                {
                    case 0:
                    case 1:
                        return MirrorType.SingleScreen;
                    case 2:
                        return MirrorType.Vertical;
                    case 3:
                        return MirrorType.Horizontal;
                    default:
                        throw new Exception("Cannot have Mirroring set to " + Mirroring);
                }
            }
            set
            {
                throw new Exception("Cannot change mirror mode for Mapper 001");
            }
        }

        public override byte CPURead(int index)
        {
            if (index < 0x8000)
            {
                if ((PRGBank & 0x10) == 0)
                {
                    // Console.Error.WriteLine("Read from PRGRAM @" + index.ToString("x4") + ", even though it is disabled");
                }
                return this.ExpansionROM[index - 0x4020];
            }
            else
            {
                switch (PRGROMBankMode)
                {
                    case 0:
                    case 1:
                        // 32KB mode
                        if (index < 0xc000)
                        {
                            return this.PRGROM[PRGBank & 0x0e, index - 0x8000];
                        }
                        else
                        {
                            return this.PRGROM[(PRGBank & 0x0e) + 1, index - 0xc000];
                        }
                    case 2:
                        if (index < 0xc000)
                        {
                            return this.PRGROM[0, index - 0x8000];
                        }
                        else
                        {
                            return this.PRGROM[PRGBank & 0x0f, index - 0xc000];
                        }
                    case 3:
                        if (index < 0xc000)
                        {
                            return this.PRGROM[PRGBank & 0x0f, index - 0x8000];
                        }
                        else
                        {
                            return this.PRGROM[this.PRGSize - 1, index - 0xc000];
                        }
                    default:
                        throw new Exception("Cannot have PRGRomBankMode " + PRGROMBankMode);
                }
            }
        }

        public override void CPUWrite(int index, byte value)
        {
            if (index < 0x8000)
            {
                if ((PRGBank & 0x10) == 0)
                {
                    // Console.Error.WriteLine("Wrote to PRGRAM @" + index.ToString("x4") + ", even though it is disabled");
                }
                this.ExpansionROM[index - 0x4020] = value;
            }
            else
            {
                if (WriteCount < 5)
                {
                    if ((value & 0x80) > 0)
                    {
                        LoadRegister = 0;
                        WriteCount = 0;
                        // Control = (byte)(Control & 0x0c);
                    }
                    else
                    {
                        LoadRegister = (byte)(LoadRegister | ((value & 0x01) << WriteCount));
                        WriteCount++;
                    }
                }

                if (WriteCount == 5)
                {
                    if (index < 0xa000)
                    {
                        Control = (byte)(LoadRegister & 0x1f);
                    }
                    else if (index < 0xc000)
                    {
                        CHRBank0 = (byte)(LoadRegister & 0x1f);
                    }
                    else if (index < 0xe000)
                    {
                        CHRBank1 = (byte)(LoadRegister & 0x1f);
                    }
                    else if (index < 0x10000)
                    {
                        PRGBank = (byte)(LoadRegister & 0x1f);
                    }
                    else
                    {
                        throw new Exception(string.Format("Index {0:x2} out of range for cpu: Mapper 001", index));
                    }
                    WriteCount = 0;
                    LoadRegister = 0;
                }
            }
        }

        public override byte PPURead(int index)
        {
            if (CHRRAMEnabled)
            {
                return this.CHRRAM[index];
            }

            // Single screen mirroring
            if (Mirroring == 0)
            {
                index %= 0x1000;
            }
            else if (Mirroring == 1)
            {
                index = 0x1000 + (index % 0x1000);
            }

            if (index < 0x2000)
            {
                if (CHRROMBankMode == 0)
                {
                    // 8KB mode
                    if (index < 0x1000)
                    {
                        return this.CHRROM[CHRBank0 & 0x1e, index];
                    }
                    else
                    {
                        return this.CHRROM[(CHRBank0 & 0x1e) + 1, index - 0x1000];
                    }
                }
                else
                {
                    if (index < 0x1000)
                    {
                        return this.CHRROM[CHRBank0, index];
                    }
                    else
                    {
                        return this.CHRROM[CHRBank1, index - 0x1000];
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }

        public override void PPUWrite(int index, byte value)
        {
            if (this.CHRRAMEnabled)
            {
                this.CHRRAM[index] = value;
                return;
            }

            // Single screen mirroring
            if (Mirroring == 0)
            {
                index %= 0x1000;
            }
            else if (Mirroring == 1)
            {
                index = 0x1000 + (index % 0x1000);
            }

            if (index < 0x2000)
            {
                if (CHRROMBankMode == 0)
                {
                    // 8KB mode
                    if (index < 0x1000)
                    {
                        this.CHRROM[CHRBank0 & 0x1e, index] = value;
                    }
                    else
                    {
                        this.CHRROM[CHRBank0 & 0x1e + 1, index - 0x1000] = value;
                    }
                }
                else
                {
                    // 8KB mode
                    if (index < 0x1000)
                    {
                        this.CHRROM[CHRBank0, index] = value;
                    }
                    else
                    {
                        this.CHRROM[CHRBank1, index - 0x1000] = value;
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper 000", index));
            }
        }

    }
}
