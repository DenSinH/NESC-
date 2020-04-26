namespace NesEmulator
{
    class PPUOAM : Memory
    {
        public PPUOAM()
        {
            this.storage = new PureByte[0x100];

            // memory map from https://wiki.nesdev.com/w/index.php/PPU_memory_map
            for (int i = 0; i < 0x100; i++)
            {
                this.storage[i] = new PureByte();
            }
        }
    }
}
