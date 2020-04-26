using System;

namespace NesEmulator
{
    class PureByte
    {
        private byte val;

        public PureByte()
        {
            this.val = 0;
        }

        public PureByte(int value)
        {
            this.val = (byte)value;
        }

        public PureByte(PureByte other)
        {
            this.val = other.val;
        }

        public byte get()
        {
            return this.val;
        }

        public void set(int value)
        {
            this.val = (byte) value;
        }

        public void set(PureByte other)
        {
            this.val = other.get();
        }

        public byte getBit(int item)
        {
            if (!((0 <= item) && (item < 8)))
            {
                throw new Exception("Cannot get bit " + item + "from Byte object");
            }
            return (byte)((this.val & (1 << item)) > 0 ? 1 : 0);
        }

        public void setBit(int item, byte value)
        {
            if (!((0 <= item) && (item < 8)))
            {
                throw new Exception("Cannot set bit " + item + "from Byte object");
            }
            if (value == 0)
            {
                this.val &= (byte) ~(1 << item);
            }
            else
            {
                this.val |= (byte) (1 << item);
            }
        }

        public override string ToString()
        {
            return this.val.ToString("x2");
        }

        public int signed()
        {
            if (this.val > 127)
            {
                return ((int) this.val) - 256;
            }
            return this.val;
        }

        public int unsigned()
        {
            return this.val;
        }

        public void add(PureByte other)
        {
            this.val += other.val;
        }

        public void add(int other)
        {
            this.val += (byte) other;
        }

        public void sub(PureByte other)
        {
            this.val -= other.val;
        }

        public void sub(int other)
        {
            this.val -= (byte) other;
        }

        public void and(PureByte other)
        {
            this.val &= other.val;
        }

        public void and(int other)
        {
            this.val &= (byte) other;
        }

        public void or(PureByte other)
        {
            this.val |= other.val;
        }

        public void or(int other)
        {
            this.val |= (byte) other;
        }

        public void xor(PureByte other)
        {
            this.val ^= other.val;
        }

        public void xor(int other)
        {
            this.val ^= (byte) other;
        }

        public bool equals(PureByte other)
        {
            return this.val == other.val;
        }

        public bool equals(int other)
        {
            return ((byte)other) == this.val;
        }

        public void lshift()
        {
            this.val = (byte) ((this.val << 1) & 0xfe);
        }

        public void rshift()
        {
            this.val = (byte)((this.val >> 1) & 0x7f);
        }

        public void rshift(int other)
        {
            this.val = (byte)((this.val >> other) & 0x7f);
        }

        public void invert()
        {
            this.val = (byte)(~this.val & 0xff);
        }

        public byte rol(byte next)
        {
            if ((next & 0xfe) > 0)
            {
                throw new Exception("next bit must be 1 or 0, not " + next);
            }

            byte c = this.getBit(7);
            this.lshift();
            this.val += next;
            return c;
        }

        public byte ror(byte next)
        {
            if ((next & 0xfe) > 0)
            {
                throw new Exception("next bit must be 1 or 0, not " + next);
            }

            byte c = this.getBit(0);
            this.rshift();
            this.val += (byte) (next << 7);
            return c;
        }

        public void incr()
        {
            this.val++;
        }

        public void incr(byte amt)
        {
            this.val += amt;
        }

        public void decr()
        {
            this.val--;
        }

        public void decr(byte amt)
        {
            this.val -= amt;
        }

        public bool negative()
        {
            return this.val >> 7 != 0;
        }

        public bool zero()
        {
            return this.val == 0;
        }

        public string hex()
        {
            return this.val.ToString("x");
        }
    }
}
