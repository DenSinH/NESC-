using System;
using NesEmulator.Audio;

namespace NesEmulator
{
    public class APU
    {
        const double amplitude = 0.05;
        public Pulse pulse1;
        public Pulse pulse2;
        public Noise noise;
        public Triangle triangle;
        private int cycle;

        private NES nes;

        private bool EnableDMC, EnableNoise, EnableTriangle, EnablePulse2, EnablePulse1;
        public bool FrameInterrupt;

        /* $4015 */
        public byte APUSTATUS
        {
            get
            {
                // todo: get
                bool OldFrameInterrupt = FrameInterrupt;
                FrameInterrupt = false;

                return (byte)(
                    (OldFrameInterrupt ? 0x40 : 0) |
                    (this.noise.GetLengthCounter() ? 0x08 : 0) |
                    (this.triangle.GetLengthCounter() ? 0x04 : 0) |
                    (this.pulse2.GetLengthCounter() ? 0x02 : 0) |
                    (this.pulse1.GetLengthCounter() ? 0x01 : 0)
                    );
            }
            set
            {
                EnableDMC = (value & 0x10) > 0;
                EnableNoise = (value & 0x08) > 0;
                EnableTriangle = (value & 0x04) > 0;
                EnablePulse2 = (value & 0x02) > 0;
                EnablePulse1 = (value & 0x01) > 0;

                if (!EnablePulse1)
                {
                    this.pulse1.SetLengthCounter(-1);
                }
                if (!EnablePulse2)
                {
                    this.pulse2.SetLengthCounter(-1);
                }
                if (!EnableNoise)
                {
                    this.noise.SetLengthCounter(-1);
                }
                if (!EnableTriangle)
                {
                    this.triangle.SetLengthCounter(-1);
                }

            }
        }

        private bool Mode, IRQInhibit;

        /* $4017 */
        public byte FrameCounter
        {
            get
            {
                return (byte)((Mode ? 0x80 : 0) | (IRQInhibit ? 0x40 : 0));
            }
            set
            {
                Mode = (value & 0x80) > 0;
                IRQInhibit = (value & 0x40) > 0;
                /*
                After 3 or 4 CPU clock cycles*, the timer is reset.
                If the mode flag is set, then both "quarter frame" and "half frame" signals are also generated.
                */
                this.cycle = 0;
                this.QuarterFrame();
                this.HalfFrame();
            }
        }

        public APU(NES nes)
        {
            this.pulse1 = new Pulse(amplitude);
            this.pulse2 = new Pulse(amplitude);
            this.noise = new Noise(amplitude);
            this.triangle = new Triangle(amplitude);

            this.nes = nes;
        }

        private void QuarterFrame()
        {
            this.pulse1.QuarterFrame();
            this.pulse2.QuarterFrame();
            this.noise.QuarterFrame();
            this.triangle.QuarterFrame();
        }

        private void HalfFrame()
        {
            this.pulse1.HalfFrame();
            this.pulse2.HalfFrame();
            this.noise.HalfFrame();
            this.triangle.HalfFrame();
        }

        public ushort GetSample()
        {
            // samples are mixed together
            short Sample = 0;
            if (this.EnablePulse1)
            {
                Sample += this.pulse1.GetSample();
            }
            if (this.EnablePulse2)
            {
                Sample += this.pulse2.GetSample();
            }
            if (this.EnableNoise)
            {
                // see https://wiki.nesdev.com/w/index.php/APU_Mixer#Linear_Approximation (about half of the others)
                Sample += (short)(this.noise.GetSample() >> 1);
            }
            if (this.EnableTriangle)
            {
                Sample += this.triangle.GetSample();
            }
            return (ushort)(Sample);
        }

        public void Step()
        {
            this.pulse1.Step();
            this.pulse2.Step();
            this.noise.Step();

            /*
             * From https://wiki.nesdev.com/w/index.php/APU_Triangle:
             * Unlike the pulse channels, this timer ticks at the rate of the CPU clock rather than the APU (CPU/2) clock.
             */
            this.triangle.Step();
            this.triangle.Step();

            if (Mode)
            {
                // 5 step sequence
                switch (this.cycle)
                {
                    case 3728:
                        this.QuarterFrame();
                        break;
                    case 7456:
                        this.QuarterFrame();
                        this.HalfFrame();
                        break;
                    case 11185:
                        this.QuarterFrame();
                        break;
                    case 18640:
                        this.QuarterFrame();
                        this.HalfFrame();
                        break;
                    case 18641:
                        this.cycle = 0;
                        break;
                }
            }
            else
            {
                // 4 step sequence
                switch (this.cycle)
                {
                    case 3728:
                        this.QuarterFrame();
                        break;
                    case 7456:
                        this.QuarterFrame();
                        this.HalfFrame();
                        break;
                    case 11185:
                        this.QuarterFrame();
                        break;
                    case 14914:
                        if (!IRQInhibit)
                        {
                            FrameInterrupt = true;
                        }
                        this.QuarterFrame();
                        this.HalfFrame();
                        break;
                    case 14915:
                        if (!IRQInhibit)
                        {
                            FrameInterrupt = true;
                        }
                        this.cycle = 0;
                        break;
                }
            }

            this.cycle++;
        }
    }
}
