using System;
using System.IO;

namespace NesEmulator.Mappers
{
    class Mapper_004 : Mapper
    {
        /*
        https://wiki.nesdev.com/w/index.php/MMC3
        CPU $6000-$7FFF: 8 KB PRG RAM bank (optional)
        CPU $8000-$9FFF (or $C000-$DFFF): 8 KB switchable PRG ROM bank
        CPU $A000-$BFFF: 8 KB switchable PRG ROM bank
        CPU $C000-$DFFF (or $8000-$9FFF): 8 KB PRG ROM bank, fixed to the second-last bank
        CPU $E000-$FFFF: 8 KB PRG ROM bank, fixed to the last bank
        PPU $0000-$07FF (or $1000-$17FF): 2 KB switchable CHR bank
        PPU $0800-$0FFF (or $1800-$1FFF): 2 KB switchable CHR bank
        PPU $1000-$13FF (or $0000-$03FF): 1 KB switchable CHR bank
        PPU $1400-$17FF (or $0400-$07FF): 1 KB switchable CHR bank
        PPU $1800-$1BFF (or $0800-$0BFF): 1 KB switchable CHR bank
        PPU $1C00-$1FFF (or $0C00-$0FFF): 1 KB switchable CHR bank
        */

        private byte PRGSize, CHRSize;
        private byte[,] PRGROM;
        private byte[,] CHRROM;
        private byte[] ExpansionROM; // Adresses $4020 - $8000 (Expansion ROM/SRAM)
        private bool IgnoreMirrorControl;
        private MirrorType _Mirror;

        private byte RegisterSelect, PRGROMBankMode, CHRA12Inversion;
        private byte BankSelect
        {
            set
            {
                CHRA12Inversion = (byte)(value >> 7);
                PRGROMBankMode = (byte)((value >> 6) & 0x01);
                RegisterSelect = (byte)(value & 0x07);
            }
        }

        private byte[] MMC3Registers;

        private byte PRGRAMWriteProtection, PRGRAMChipEnable;
        private byte PRGRAMProtect
        {
            set
            {
                PRGRAMChipEnable = (byte)(value >> 7);
                PRGRAMWriteProtection = (byte)((value >> 6) & 0x01);
            }
        }
        private byte IRQLatch;
        private byte IRQCounter;
        private bool IRQRequest;
        private bool IRQReload = false;
        private bool IRQEnabled = false;

        public Mapper_004(FileStream fs, MirrorType m, byte PRGSize, byte CHRSize, bool IgnoreMirrorControl) : base(fs, m)
        {
            this._Mirror = m;
            this.PRGSize = PRGSize;
            this.CHRSize = CHRSize;

            this.PRGROM = new byte[2 * this.PRGSize, 0x2000];  // 8KB banks
            this.CHRROM = new byte[8 * this.CHRSize, 0x400];   // 1KB banks (or 2KB, but smallest is 1KB)
            this.ExpansionROM = new byte[0x8000 - 0x4020];

            this.IgnoreMirrorControl = IgnoreMirrorControl;
            this.MMC3Registers = new byte[8];

            for (int i = 0; i < 2 * this.PRGSize; i++)
            {
                for (int j = 0; j < 0x2000; j++)
                {
                    this.PRGROM[i, j] = (byte)fs.ReadByte();
                }
            }

            for (int i = 0; i < 8 * this.CHRSize; i++)
            {
                for (int j = 0; j < 0x400; j++)
                {
                    this.CHRROM[i, j] = (byte)fs.ReadByte();
                }
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
                throw new Exception("Cannot change mirror mode for Mapper 004");
            }
        }

        public override byte CPURead(int index)
        {
            if (index < 0x8000)
            {
                if (PRGRAMChipEnable == 1)
                {
                    return this.ExpansionROM[index - 0x4020];
                }
                return 0;
            }
            else if (index < 0xa000)
            {
                if (PRGROMBankMode == 0)
                {
                    return this.PRGROM[MMC3Registers[6], index - 0x8000];
                }
                else
                {
                    return this.PRGROM[2 * this.PRGSize - 2, index - 0x8000];
                }
            }
            else if (index < 0xc000)
            {
                return this.PRGROM[MMC3Registers[7], index - 0xa000];
            }
            else if (index < 0xe000)
            {
                if (PRGROMBankMode == 0)
                {
                    return this.PRGROM[2 * this.PRGSize - 2, index - 0xc000];
                }
                else
                {
                    return this.PRGROM[MMC3Registers[6], index - 0xc000];
                }
            }
            else if (index < 0x10000)
            {
                return this.PRGROM[2 * this.PRGSize - 1, index - 0xe000];
            }
            else
            {
                throw new Exception("Index out of range for Mapper_004: " + index);
            }
        }

        public override void CPUWrite(int index, byte value)
        {
            if (index < 0x8000)
            {
                if (PRGRAMChipEnable == 1 && PRGRAMWriteProtection == 0)
                {
                    this.ExpansionROM[index - 0x4020] = value;
                }
            }
            else if (index < 0xa000)
            {
                if ((index & 0x01) == 0)  // even
                {
                    BankSelect = value;
                }
                else  // odd
                {
                    if (RegisterSelect == 6 || RegisterSelect == 7)
                    {
                        this.MMC3Registers[RegisterSelect] = (byte)(value & 0x3f);  // ignore top 2 bits
                    }
                    else if (RegisterSelect == 0 || RegisterSelect == 1)
                    {
                        this.MMC3Registers[RegisterSelect] = (byte)(value & 0xfe);  // ignore bottom bit
                    }
                    else
                    {
                        this.MMC3Registers[RegisterSelect] = value;
                    }
                }
            }
            else if (index < 0xc000)
            {
                if ((index & 0x01) == 0)  // even
                {
                    if (!IgnoreMirrorControl)
                    {
                        this._Mirror = ((value & 0x01) == 0) ? MirrorType.Vertical : MirrorType.Horizontal;
                    }
                }
                else  // odd
                {
                    PRGRAMProtect = value;
                }
            }
            else if (index < 0xe000)
            {
                if ((index & 0x01) == 0)  // even
                {
                    IRQLatch = value;
                }
                else  // odd
                {
                    IRQReload = true;
                }
            }
            else if (index < 0x10000)
            {
                if ((index & 0x01) == 0)  // even
                {
                    IRQEnabled = false;
                }
                else  // odd
                {
                    IRQEnabled = true;
                }
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for cpu: Mapper_004", index));
            }
        }

        public override byte PPURead(int index)
        {
            byte InversionVal = 0;
            if (index >= 0x1000)
            {
                InversionVal = 1;
                index -= 0x1000;
            }

            if (index < 0x400)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    return this.CHRROM[MMC3Registers[0], index];
                }
                else
                {
                    return this.CHRROM[MMC3Registers[2], index];
                }
            }
            else if (index < 0x800)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    return this.CHRROM[MMC3Registers[0] + 1, index - 0x400];
                }
                else
                {
                    return this.CHRROM[MMC3Registers[3], index - 0x400];
                }
            }
            else if (index < 0xc00)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    return this.CHRROM[MMC3Registers[1], index - 0x800];
                }
                else
                {
                    return this.CHRROM[MMC3Registers[4], index - 0x800];
                }
            }
            else if (index < 0x1000)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    return this.CHRROM[MMC3Registers[1] + 1, index - 0xc00];
                }
                else
                {
                    return this.CHRROM[MMC3Registers[5], index - 0xc00];
                }
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper_004", index));
            }
        }

        public override void PPUWrite(int index, byte value)
        {
            byte InversionVal = 0;
            if (index >= 0x1000)
            {
                InversionVal = 1;
                index -= 0x1000;
            }

            if (index < 0x400)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    this.CHRROM[MMC3Registers[0], index] = value;
                }
                else
                {
                    this.CHRROM[MMC3Registers[2], index] = value;
                }
            }
            else if (index < 0x800)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    this.CHRROM[MMC3Registers[0] + 1, index - 0x400] = value;
                }
                else
                {
                    this.CHRROM[MMC3Registers[3], index - 0x400] = value;
                }
            }
            else if (index < 0xc00)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    this.CHRROM[MMC3Registers[1], index - 0x800] = value;
                }
                else
                {
                    this.CHRROM[MMC3Registers[4], index - 0x800] = value;
                }
            }
            else if (index < 0x1000)
            {
                if (CHRA12Inversion == InversionVal)
                {
                    this.CHRROM[MMC3Registers[1] + 1, index - 0xc00] = value;
                }
                else
                {
                    this.CHRROM[MMC3Registers[5], index - 0xc00] = value;
                }
            }
            else
            {
                throw new Exception(string.Format("Index {0:x2} out of range for ppu: Mapper_004", index));
            }
        }

        public override void At260OfVisibleScanline()
        {
            if (IRQCounter == 0 || IRQReload)
            {
                IRQCounter = IRQLatch;
                IRQReload = false;
            }
            else
            {
                IRQCounter--;
            }

            if (IRQCounter == 0 && IRQEnabled)
            {
                IRQRequest = true;
            }
        }

        public override bool DoIRQ()
        {
            bool Request = IRQRequest;
            IRQRequest = false;
            return Request;
        }

    }
}
