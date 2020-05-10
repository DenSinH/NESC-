using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator.Audio
{
    public class Triangle : Channel
    {
        /*
         https://wiki.nesdev.com/w/index.php/APU_Triangle
        */

        private byte[] sequence =
        {
            15, 14, 13, 12, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0,
            0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15
        };
        private int index;

        private byte LinearCounter, LinearCounterReload;
        private bool LinearCounterReloadFlag;

        public Triangle(double amplitude) : base(amplitude)
        {
            this.index = 0;
        }

        /* Very similar to Pulse: */
        public byte TRIANGLE4008
        {
            set
            {
                this.LengthCounterHalt = (value & 0x80) > 0;
                this.LinearCounterReload = (byte)(value & 0x7f);
            }
        }

        public byte TRIANGLE400B
        {
            set
            {
                this.SetLengthCounter((value & 0xf8) >> 3);
                this.Period = (ushort)((this.Period & 0x00ff) | ((value & 0x07) << 8));
                LinearCounterReloadFlag = true;
            }
        }

        public override short GetSample()
        {
            if (this.Period < 2)
            {
                // Ultrasonic frequencies
                return 0;
            }
            
            double SampleValue = this.amplitude * this.sequence[this.index] / 16.0;

            return (short)(SampleValue * UInt16.MaxValue);
        }

        public override void QuarterFrame()
        {
            // Linear counter clock
            if (LinearCounterReloadFlag)
            {
                LinearCounter = LinearCounterReload;
            }
            else if (LinearCounter != 0)
            {
                LinearCounter--;
            }

            if (this.LengthCounterHalt)  // also "Control Flag"
            {
                LinearCounterReloadFlag = false;
            }
        }

        public override void HalfFrame()
        {
            // clock Length counter
            /*
             * https://wiki.nesdev.com/w/index.php/APU_Frame_Counter
             * https://wiki.nesdev.com/w/index.php/APU_Length_Counter
             */
            if (!LengthCounterHalt)
            {
                if (LengthCounter > 0)
                {
                    this.LengthCounter--;
                }
            }
        }

        protected override void OnTimer0()
        {
            if (this.LinearCounter > 0 && this.LengthCounter > 0)
            {
                this.index++;
                this.index &= 0x1f;  // 32 step sequence
            }
        }
    }
}
