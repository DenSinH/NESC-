using System;
using NesEmulator.Audio;

namespace NesEmulator
{
    public class APU
    {
        private double amplitude = 0.05;
        public Pulse pulse1;
        public Pulse pulse2;
        public Noise noise;
        public Triangle triangle;
        public DMC dmc;
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
                bool OldFrameInterrupt = FrameInterrupt && !IRQInhibit;
                FrameInterrupt = false;
                

                return (byte)(
                    (this.dmc.Interrupt ? 0x80 : 0) |
                    (OldFrameInterrupt ? 0x40 : 0) |
                    (this.dmc.BytesRemaining > 0 ? 0x10 : 0) |
                    (this.noise.GetLengthCounter() ? 0x08 : 0) |
                    (this.triangle.GetLengthCounter() ? 0x04 : 0) |
                    (this.pulse2.GetLengthCounter() ? 0x02 : 0) |
                    (this.pulse1.GetLengthCounter() ? 0x01 : 0)
                    );
            }
            set
            {
                this.dmc.Interrupt = false;
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

                if (!EnableDMC)
                {
                    this.dmc.BytesRemaining = 0;
                }
                else
                {
                    if (this.dmc.BytesRemaining == 0)
                    {
                        this.dmc.StartSample();
                    }
                }

                this.dmc.IRQEnable = false;
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
                if (IRQInhibit)
                {
                    FrameInterrupt = false;
                }

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
            this.dmc = new DMC(amplitude, nes);

            this.nes = nes;
        }

        public void ChangeAmplitude(double dAmp)
        {
            this.amplitude += dAmp;
            if (this.amplitude < 0)
            {
                this.amplitude = 0;
            }

            this.SetAmplitude(this.amplitude);
        }

        public void SetAmplitude(double amp)
        {
            this.amplitude = amp;
            this.pulse1.SetAmplitude(amp);
            this.pulse2.SetAmplitude(amp);
            this.noise.SetAmplitude(amp);
            this.triangle.SetAmplitude(amp);
            this.dmc.SetAmplitude(amp);
        }

        public void POWERUP()
        {
            this.pulse1.PULSE4000 = 0;
            this.pulse1.PULSESWEEP = 0;
            this.pulse1.PULSE4003 = 0;
            this.pulse2.PULSE4000 = 0;
            this.pulse2.PULSESWEEP = 0;
            this.pulse2.PULSE4003 = 0;

            this.noise.NOISE400C = 0;
            this.noise.MODE_PERIOD = 0;
            this.noise.NOISE400F = 0;

            this.triangle.TRIANGLE4008 = 0;
            this.triangle.TRIANGLE400B = 0;

            this.dmc.DIRECTLOAD = 0;
            this.dmc.FLAGS = 0;
            this.dmc.SAMPLEADDRESS = 0;
            this.dmc.SAMPLELENGTH = 0;
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
            if (this.EnableDMC)
            {
                Sample += (short)(4 * this.dmc.GetSample());
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

            this.dmc.Step();

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
