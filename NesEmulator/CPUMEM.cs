using System;

namespace NesEmulator
{
    class CPUMEM : Memory
    {
        public PureByte[] pc;
        public PureByte ac, x, y, sr, sp;

        public readonly int[] nmiVector = { 0xfffa, 0xfffb };
        public readonly int[] resetVector = { 0xfffc, 0xfffd };
        public readonly int[] irqVector = { 0xfffe, 0xffff };

        public CPUMEM()
        {
            this.storage = new PureByte[0x10000];
            // memory map from https://wiki.nesdev.com/w/index.php/CPU_memory_map
            for (int i = 0; i < 0x800; i++)
            {
                this.storage[i] = new PureByte();
                this.storage[0x800 + i] = this.storage[i];
                this.storage[0x1000 + i] = this.storage[i];
                this.storage[0x1800 + i] = this.storage[i];
            }
            for (int i = 0; i < 8; i++)
            {
                this.storage[0x2000 + i] = new PureByte(true);
                for (int j = 8; j < 0x1ff8; j += 8)
                {
                    this.storage[0x2000 + j + i] = this.storage[i];
                }
            }

            for (int i = 0x4000; i < 0x10000; i++)
            {
                this.storage[i] = new PureByte();
            }

            this.pc = new PureByte[2];
            this.pc[0] = new PureByte();
            this.pc[1] = new PureByte();

            this.ac = new PureByte();
            this.x = new PureByte();
            this.y = new PureByte();
            this.sr = new PureByte(0x24);
            this.sp = new PureByte(0x00);
        }

        public PureByte getCurrent()
        {
            return this.storage[this.getPc()];
        }

        public int getPc()
        {
            return 0x100 * this.pc[0].unsigned() + this.pc[1].unsigned();
        }

        public void setPc(int ll, int hh)
        {
            this.pc[0].set(hh);
            this.pc[1].set(ll);
        }

        public void setPc(PureByte ll, PureByte hh)
        {
            this.pc[0].set(hh);
            this.pc[1].set(ll);
        }

        public void setNZ(PureByte val)
        {
            this.setFlag('N', (byte)(val.negative() ? 1 : 0));
            this.setFlag('Z', (byte)(val.zero() ? 1 : 0));
        }

        public void setFlag(char flag, byte value)
        {
            if ((value & 0xfe) > 0)
            {
                throw new Exception("Cannot set flag to " + value);
            }

            switch (flag)
            {
                case 'N': this.sr.setBit(7, value); break;
                case 'V': this.sr.setBit(6, value); break;
                case 'B': this.sr.setBit(4, value); break;
                case 'D': this.sr.setBit(3, value); break;
                case 'I': this.sr.setBit(2, value); break;
                case 'Z': this.sr.setBit(1, value); break;
                case 'C': this.sr.setBit(0, value); break;
                default : throw new Exception("Unknown flag: " + flag);
            }
        }

        public byte getFlag(char flag)
        {
            switch (flag)
            {
                case 'N': return this.sr.getBit(7);
                case 'V': return this.sr.getBit(6);
                case 'B': return this.sr.getBit(4);
                case 'D': return this.sr.getBit(3);
                case 'I': return this.sr.getBit(2);
                case 'Z': return this.sr.getBit(1);
                case 'C': return this.sr.getBit(0);
                default : throw new Exception("Unknown flag: "+ flag);
            };
        }

        public void incrPc(int amt)
        {
            int c = (this.pc[1].unsigned() + amt > 0xff) ? 1 : 0;
            this.pc[1].add(amt);
            if (c == 1)
            {
                this.pc[0].add(c);
            }
        }

        public void incrPc()
        {
            this.incrPc(1);
        }

        public void reset()
        {
            this.setPc(
                    this.storage[resetVector[0]],
                    this.storage[resetVector[1]]
            );
            this.sp.set(0xff);
            // todo: Other reset actions
        }

        public void push(PureByte value)
        {
            if (this.sp.unsigned() == 0)
            {
                Console.Error.WriteLine("Cannot push onto full stack");
            }
            this.storage[0x100 + this.sp.unsigned()].set(value);
            this.sp.decr();
        }

        public void push(int value)
        {
            if (this.sp.unsigned() == 0)
            {
                Console.Error.WriteLine("Cannot push onto full stack");
            }
            this.storage[0x100 + this.sp.unsigned()].set(value);
            this.sp.decr();
        }

        public PureByte pull()
        {
            if (this.sp.unsigned() >= 0xff)
            {
                Console.Error.WriteLine("Cannot pull from empty stack");
            }
            this.sp.incr();
            return this.storage[0x100 + this.sp.unsigned()];
        }

    }
}
