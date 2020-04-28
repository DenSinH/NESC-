using System;


namespace NesEmulator
{
    class PPUMEM : Memory
    {

        public PPUMEM()
        {
            this.storage = new byte[0x10000];
            // memory map from https://wiki.nesdev.com/w/index.php/PPU_memory_map
            for (int i = 0; i < 0x3000; i++)
            {
                this.storage[i] = new byte();
            }
            for (int i = 0; i < 0xf00; i++)
            {
                this.storage[0x3000 + i] = this.storage[0x2000 + i];
            }
            for (int i = 0; i < 0x20; i++)
            {
                this.storage[0x3f00 + i] = new byte();
                for (int j = 0; j < 0xe0; j += 0x20)
                {
                    this.storage[0x3f20 + j + i] = this.storage[0x3f00 + i];
                }
            }

            for (int i = 0; i < 0x4000; i++)
            {
                this.storage[0x4000 + i] = this.storage[i];
                this.storage[0x8000 + i] = this.storage[i];
                this.storage[0xd000 + i] = this.storage[i];
            }

        }

        protected override int Map(int index)
        {
            throw new NotImplementedException();
        }

        protected override byte GetSpecial(int index)
        {
            throw new NotImplementedException();
        }

        protected override void SetSpecial(int index, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
