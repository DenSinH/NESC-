using System;

namespace NesEmulator.Audio
{
    public class Noise : Channel
    {
        /* https://wiki.nesdev.com/w/index.php/APU_Noise */

        private readonly Random rand = new Random();

        // Same as Pulse
        private bool VolumeStart;
        private bool ConstantVolume;
        private byte VolumeReset;
        private byte VolumeDivider, VolumeDecay;

        public byte NOISE400C
        {
            set
            {
                this.LengthCounterHalt = (value & 0x20) > 0;
                this.ConstantVolume = (value & 0x10) > 0;
                this.VolumeReset = (byte)(value & 0x0f);
            }
        }

        private bool Mode;
        private UInt16[] PeriodModes =
        {
            4, 8, 14, 30, 60, 88, 118, 148, 188, 236, 354, 472, 708,  944, 1890, 3778 // PAL Based
        };

        public byte MODE_PERIOD  // $400e
        {
            set
            {
                Mode = (value & 0x80) > 0;
                Period = PeriodModes[value & 0x0f];
            }
        }

        public byte NOISE400F
        {
            set
            {
                this.SetLengthCounter((value & 0xf8) >> 3);
                this.VolumeStart = true;
            }
        }

        private UInt16 ShiftRegister;

        public Noise(double amplitude) : base(amplitude)
        {
            this.ShiftRegister = 1;
        }

        public override short GetSample()
        {
            if (this.LengthCounter == 0)
            {
                // Length counter mute
                return 0;
            }

            if ((this.ShiftRegister & 0x01) == 0)
            {
                return 0;
            }

            double SampleValue = rand.NextDouble() * this.amplitude;

            if (ConstantVolume)
            {
                SampleValue *= VolumeReset / 16.0;
            }
            else
            {
                SampleValue *= VolumeDecay / 16.0;
            }
            return (short)(SampleValue * UInt16.MaxValue);
        }

        public override void QuarterFrame()
        {
            /* same as Pulse */

            // volume envelope
            /* https://wiki.nesdev.com/w/index.php/APU_Envelope */
            if (!VolumeStart)
            {
                if (VolumeDivider == 0)
                {
                    VolumeDivider = VolumeReset;
                    if (VolumeDecay == 0 && LengthCounterHalt)
                    {
                        VolumeDecay = 0x0f;
                    }
                    else if (VolumeDecay > 0)
                    {
                        VolumeDecay--;
                    }
                }
                else
                {
                    VolumeDivider--;
                }
            }
            else
            {
                VolumeStart = false;
                VolumeDecay = 0x0f;
                VolumeDivider = VolumeReset;
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
            byte Feedback;
            if (Mode)
            {
                Feedback = (byte)((ShiftRegister & 0x01) ^ ((ShiftRegister & 0x01) << 6));
            }
            else
            {
                Feedback = (byte)((ShiftRegister & 0x01) ^ ((ShiftRegister & 0x01) << 1));
            }

            ShiftRegister <<= 1;
            ShiftRegister |= Feedback;
        }
    }
}
