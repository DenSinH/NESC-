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

        public abstract byte CPURead(int index);
        public abstract void CPUWrite(int index, byte value);

        public abstract byte PPURead(int index);
        public abstract void PPUWrite(int index, byte value);

        public virtual void At260OfVisibleScanline() { } 
        public virtual bool DoIRQ() { return false; }
    }
}
