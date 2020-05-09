﻿using System;

namespace NesEmulator.Audio
{
    public abstract class Channel
    {
        protected static byte[] LengthCounterLookup =
        {
            10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };

        protected UInt16 t;
        public UInt16 Period;
        protected byte LengthCounter;
        public bool LengthCounterHalt;

        public abstract void Step();
        public abstract short GetSample();
        public abstract void QuarterFrame();
        public abstract void HalfFrame();

        public void SetLengthCounter(int value)
        {
            if (value == -1)
            {
                this.LengthCounter = 0;
            }
            else
            {
                this.LengthCounter = Channel.LengthCounterLookup[value];
            }
        }

        public bool GetLengthCounter()
        {
            return LengthCounter > 0;
        }
    }
}