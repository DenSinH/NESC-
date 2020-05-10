using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator.Audio
{
    public class DMC
    {
        private double amplitude;
        public void SetAmplitude(double amp)
        {
            this.amplitude = amp;
        }

        private NES nes;

        private bool SampleBufferFilled;
        private byte SampleBuffer;
        private UInt16 t;

        private static readonly UInt16[] RateIndex =
        {
            398, 354, 316, 298, 276, 236, 210, 198, 176, 148, 132, 118,  98,  78,  66,  50  // PAL based
        };

        public bool IRQEnable, Interrupt;
        private bool Loop;
        private UInt16 Rate;

        public byte FLAGS
        {
            set
            {
                IRQEnable = (value & 0x80) > 0;
                Loop = (value & 0x40) > 0;
                Rate = (ushort)(RateIndex[value & 0x0f] >> 1);  // measured in CPU cycles 
            }
        }

        private byte OutputLevel;

        public byte DIRECTLOAD
        {
            set
            {
                OutputLevel = (byte)(value & 0x0c);
            }
        }

        private ushort SampleAddress, SampleLength;
        public ushort CurrentAddress, BytesRemaining;
        private byte ShiftRegister, BitsRemaining;
        private bool Silence;

        public byte SAMPLEADDRESS
        {
            set
            {
                SampleAddress = (ushort)(0xc000 + value * 64);  // From: https://wiki.nesdev.com/w/index.php/APU_DMC
            }
        }

        public byte SAMPLELENGTH
        {
            set
            {
                SampleLength = (ushort)(value * 16 + 1);
            }
        }

        public void StartSample()
        {
            this.CurrentAddress = SampleAddress;
            this.BytesRemaining = SampleLength;
        }

        public DMC(double amplitude, NES nes)
        {
            this.amplitude = amplitude;
            this.nes = nes;  // for reading CPU memory

            this.t = 1;
            this.Rate = RateIndex[0];

            this.BitsRemaining = 8;
            this.OutputLevel = 64;
        }

        public void Step()
        {
            if (!SampleBufferFilled)
            {
                if (BytesRemaining > 0)
                {
                    // todo: pause CPU (is this relevant?)
                    this.SampleBuffer = this.nes.cpu[CurrentAddress];
                    this.CurrentAddress++;
                    SampleBufferFilled = true;
                    BytesRemaining--;

                    if (BytesRemaining == 0)
                    {
                        if (Loop)
                        {
                            CurrentAddress = SampleAddress;
                            BytesRemaining = SampleLength;
                        }
                        else if (IRQEnable)
                        {
                            Interrupt = true;
                        }
                    }
                }
            }

            t--;
            if (t == 0)
            {
                this.OnTimer0();
                t = this.Rate;
                // Console.WriteLine(this.Rate);
            }
        }

        private void OnTimer0()
        {
            if (!Silence)
            {
                if ((ShiftRegister & 0x01) == 1)
                {
                    if (OutputLevel <= 125)
                    {
                        OutputLevel += 2;
                    }
                }
                else
                {
                    if (OutputLevel >= 2)
                    {
                        OutputLevel -= 2;
                    }
                }
            }

            ShiftRegister >>= 1;
            BitsRemaining--;
            if (BitsRemaining == 0)
            {
                BitsRemaining = 8;
                if (!SampleBufferFilled)
                {
                    // If the sample buffer is empty, then the silence flag is set
                    Silence = true;
                }
                else
                {
                    // otherwise, the silence flag is cleared and the sample buffer is emptied into the shift register.
                    Silence = false;
                    ShiftRegister = SampleBuffer;
                    SampleBufferFilled = false;
                }
            }

        }

        public short GetSample()
        {
            double SampleValue = this.amplitude * (OutputLevel / 127.0);
            return (short)(SampleValue * UInt16.MaxValue);
        }

    }
}
