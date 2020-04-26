namespace NesEmulator
{
    class Memory
    {
        protected PureByte[] storage;

        public PureByte get(int item)
        {
            return this.storage[item];
        }

        public PureByte get(PureByte ll, PureByte hh)
        {
            return this.storage[0x100 * hh.unsigned() + ll.unsigned()];
        }

        public void set(int index, PureByte value)
        {
            this.storage[index].set(value);
        }

        public void set(int index, int value)
        {
            this.storage[index].set(value);
        }
    }
}
