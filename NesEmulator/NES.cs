using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator
{
    class NES
    {
        private CPU cpu;
        private PPU ppu; // todo: make this ppu not ppumem

        public NES()
        {
            this.cpu = new CPU();
            this.ppu = new PPU(cpu);
        }

    }
}
