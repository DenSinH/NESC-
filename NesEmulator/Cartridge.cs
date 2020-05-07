using System;
using System.IO;
using NesEmulator.Mappers;

namespace NesEmulator
{
    class Cartridge
    {
        private string filename;
        private FileStream fs;

        private byte PRGSize, CHRSize;

        // Byte 6 data:
        private bool BatteryPackedPRGRam, Trainer, IgnoreMirrorControl;
        private MirrorType Mirror;

        // Byte 7 data:
        private bool VSUnisystem, PlayChoice10, INES2;

        // Byte 8 data:
        private byte PRGRAMSize;

        private Mapper Mapper;

        public Cartridge(string filename)
        {
            this.filename = filename;
            this.fs = File.OpenRead(filename);
            byte MapperNumber;

            // NES\n Header
            for (int i = 0; i < 4; i++)
            {
                fs.ReadByte();
            }

            // Bytes 4 and 5
            this.PRGSize = (byte)this.fs.ReadByte();
            this.CHRSize = (byte)this.fs.ReadByte();

            // Byte 6
            byte Flags = (byte)this.fs.ReadByte();

            this.IgnoreMirrorControl = (Flags & 0x08) > 0;
            // todo: ignore mirror control

            if ((Flags & 0x01) == 0)
            {
                this.Mirror = MirrorType.Horizontal;
            } else
            {
                this.Mirror = MirrorType.Vertical;
            }

            if (IgnoreMirrorControl)
            {
                this.Mirror = MirrorType.FourScreen;
            }

            this.BatteryPackedPRGRam = (Flags & 0x02) > 0;

            // todo: deal with trainer
            this.Trainer = (Flags & 0x04) > 0;
            MapperNumber = (byte)((Flags & 0xf0) >> 4);

            // Byte 7
            Flags = (byte)this.fs.ReadByte();

            this.VSUnisystem = (Flags & 0x01) > 0;
            this.PlayChoice10 = (Flags & 0x02) > 0;
            this.INES2 = (Flags & 0x0c) > 0;

            MapperNumber = (byte)(MapperNumber | (Flags & 0xf0));

            // Byte 8
            this.PRGRAMSize = (byte)fs.ReadByte();

            // Byte 8-15, Not implemented
            for (int i = 0; i < 7; i++)
            {
                fs.ReadByte();
            }

            switch (MapperNumber)
            {
                case 0:
                    this.Mapper = new Mapper_000(fs, Mirror, PRGSize, CHRSize);
                    break;
                case 2:
                    this.Mapper = new Mapper_002(fs, Mirror, PRGSize, CHRSize);
                    break;
                case 3:
                    this.Mapper = new Mapper_003(fs, Mirror, PRGSize, CHRSize);
                    break;
                case 4:
                    this.Mapper = new Mapper_004(fs, Mirror, PRGSize, CHRSize, IgnoreMirrorControl);
                    break;
                default:
                    throw new Exception(string.Format("Mapper {0:3} not implemented yet", MapperNumber));
            }
        }

        public void LoadTo(NES nes)
        {
            nes.SetMapper(this.Mapper);
            nes.ppu.SetMapper(this.Mapper);
            nes.cpu.SetMapper(this.Mapper);
            this.fs.Close();
        }
    }
}
