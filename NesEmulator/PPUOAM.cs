namespace NesEmulator
{
    class PPUOAM : Memory
    {
        public PPUOAM()
        {
            this.storage = new byte[0x100];

            // memory map from https://wiki.nesdev.com/w/index.php/PPU_memory_map
            for (int i = 0; i < 0x100; i++)
            {
                this.storage[i] = 0;
            }
        }

        protected override byte GetSpecial(int index)
        {
            throw new System.NotImplementedException();
        }

        protected override int Map(int index)
        {
            throw new System.NotImplementedException();
        }

        protected override void SetSpecial(int index, byte value)
        {
            throw new System.NotImplementedException();
        }
    }
}
