namespace NesEmulator
{
    abstract class Memory
    {
        protected byte[] storage;

        protected abstract int Map(int index);

        // Can provide special indices to set or get registers
        protected abstract byte GetSpecial(int index);

        protected abstract void SetSpecial(int index, byte value);

        public byte this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    return this.storage[this.Map(index)];
                }
                return this.GetSpecial(index);
            }
            set
            {
                if (index >= 0)
                {
                    this.storage[this.Map(index)] = value;
                } else
                {
                    this.SetSpecial(index, value);
                }
            }
        }

        public byte this[byte ll, byte hh]
        {
            get
            {
                return this.storage[this.Map(0x100 * hh + ll)];
            }
            set
            {
                this.storage[this.Map(0x100 * hh + ll)] = value;;
            }
        }
    }
}
