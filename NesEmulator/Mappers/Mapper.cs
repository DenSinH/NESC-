using System;
using System.IO;

namespace NesEmulator.Mappers
{
    public abstract class Mapper
    {
        public Mapper(FileStream fs, MirrorType m)
        {

        }

        public abstract MirrorType Mirror
        {
            get;
            set;
        }

        public abstract byte cpuRead(int index);
        public abstract void cpuWrite(int index, byte value);

        public abstract byte ppuRead(int index);
        public abstract void ppuWrite(int index, byte value);

    }
}
