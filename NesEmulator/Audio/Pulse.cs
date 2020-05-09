using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesEmulator.Audio
{
    public class Pulse : Channel
    {
        private double amplitude;
        
        private byte sequence;
        private byte index;

        public byte PULSE4000
        {
            set
            {
                this.SetDutyCycle((byte)((value & 0xc0) >> 6));
                this.LengthCounterHalt = (value & 0x20) > 0;
                this.ConstantVolume = (value & 0x10) > 0;
                this.VolumeReset = (byte)(value & 0x0f);
            }
        }

        public byte PULSE4003
        {
            set
            {
                this.SetLengthCounter((value & 0xf8) >> 3);
                this.Period = (ushort)((this.Period & 0x00ff) | ((value & 0x07) << 8));
                this.VolumeStart = true;
            }
        }
        
        private bool SweepEnabled, SweepNegate, SweepReload;
        private byte SweepDividerPeriod, SweepShiftCount, SweepDividerCount;

        public byte PULSESWEEP
        {
            set
            {
                SweepEnabled = (value & 0x80) > 0;
                SweepDividerPeriod = (byte)((value & 0x70) >> 4);
                SweepNegate = (value & 0x08) > 0;
                SweepShiftCount = (byte)(value & 0x07);
                
                SweepReload = true;
            }
        }

        private bool VolumeStart;
        private bool ConstantVolume;
        private byte VolumeReset;
        private byte VolumeDivider, VolumeDecay;

        public Pulse(double amplitude)
        {
            this.t = 0;
            this.Period = 1;
            this.index = 0;
            this.sequence = 0b0100_0000;  // 12.5% duty cycle by default. This is just an arbitrary choice

            this.amplitude = amplitude;
        }

        public void SetDutyCycle(byte value)
        {
            switch (value)
            {
                case 0:
                    this.sequence = 0b0100_0000;
                    break;
                case 1:
                    this.sequence = 0b0110_0000;
                    break;
                case 2:
                    this.sequence = 0b0111_1000;
                    break;
                case 3:
                    this.sequence = 0b1001_1111;
                    break;
                default:
                    throw new Exception("Unknown duty cycle for pulse channel: " + value);
            }
        }

        public override void Step()
        {
            t--;
            if (t == 0)
            {
                this.index++;
                this.index &= 0x07;
                t = this.TargetPeriod();
            }
        }

        public override short GetSample()
        {
            if (this.LengthCounter == 0)
            {
                return 0;
            }
            
            if (this.TargetPeriod() > 0x7ff || this.Period < 8)
            {
                // Sweep unit mute
                return 0;
            }

            double SampleValue = ((this.sequence & (0x01 << this.index)) > 0 ? this.amplitude / 2 : -this.amplitude / 2);

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

        private ushort TargetPeriod()
        {
            ushort ChangeAmount = (ushort)(this.Period >> SweepShiftCount);
            if (SweepNegate)
            {
                return (ushort)(this.Period - ChangeAmount - 1);
            }
            else
            {
                return (ushort)(this.Period + ChangeAmount);
            }
        }

        public override void QuarterFrame()
        {
            // volume envelope
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
            if (!LengthCounterHalt)
            {
                if (LengthCounter > 0)
                {
                    this.LengthCounter--;
                }
            }
            
            // clock Sweep unit
            if (SweepEnabled)
            {
                if (SweepDividerCount == 0)
                {
                    if (this.TargetPeriod() > 0x7ff || this.Period < 8)
                    {
                        // muted
                        SweepEnabled = false;
                    }

                    else if (this.SweepShiftCount > 0)
                    {
                        this.Period = this.TargetPeriod();
                    }

                    SweepDividerCount = SweepDividerPeriod;
                    SweepReload = false;
                }
                else if (SweepReload)
                {
                    SweepDividerCount = SweepDividerPeriod;
                    SweepReload = false;
                }
                else
                {
                    SweepDividerCount--;
                }
            }
        }
    }
}
