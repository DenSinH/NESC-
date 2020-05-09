using System;
using SharpDX;

using NesEmulator.Mappers;
using NesEmulator.Audio;
using NLog;

namespace NesEmulator
{
    public class NES
    {
        /* Debugging */
        private byte makeLog = 1;  // 0: no log | 1: Console | 2: File + Console
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /* Components */
        public CPU cpu;
        public PPU ppu;
        public APU apu;
        public Mapper Mapper;

        public byte DMAPage, DMAAddr, DMAData;
        public bool DMAActive, DMAStart;

        public readonly Controller[] controllers = new Controller[2];
        public byte[] ControllerState = { 0, 0 };
        
        /* Display */
        private const int width = 256;
        private const int height = 240;

        public int[] display;
        public bool ShutDown = false;

        /* Audio */
        public const int SampleRate = 44100;
        public readonly Speaker speaker = new Speaker(new NAudio.Wave.WaveFormat(SampleRate, 16, 1));

        /* Sync */
        private const int PPUClockRate = 5369318;
        private double t = 0;
        private const double dtPPUCycle = 1.0 / PPUClockRate;
        private const double dtAudioCycle = 1.0 / SampleRate;

        public NES(int[] display)
        {
            this.display = display;

            this.cpu = new CPU(this);
            this.ppu = new PPU(this);
            this.apu = new APU(this);
            
            this.controllers[0] = new XInputController();
            try
            {
                this.controllers[0].PollKeysPressed();
            }
            catch (SharpDX.SharpDXException)
            {
                this.controllers[0] = new KeyboardController();
            }

            this.controllers[1] = new KeyboardController();
        }

        public void SetMapper(Mapper m)
        {
            this.Mapper = m;
        }

        private void Log(string message)
        {
            // for logging results
            if (makeLog > 0)
            {
                Console.WriteLine(message);
                if (makeLog > 1)
                {
                    logger.Debug(message);
                }
            }
        }

        public void Run(bool debug)
        {
            // assume cartridge is loaded
            this.cpu.RESET();

            this.speaker.ClearBuffer();
            
            int dcycles;
            int GlobalCycles = 0;
            while (!this.ShutDown)
            {
                if (this.speaker.NeedMoreSamples())
                {
                    if (!this.DMAActive)
                    {
                        dcycles = this.cpu.Step();
                        this.cpu.cycle += dcycles;
                    }
                    else
                    {
                        dcycles = 1;
                    }

                    if (this.ppu.ThrowNMI)
                    {
                        this.ppu.ThrowNMI = false;
                        this.cpu.cycle += this.cpu.NMI();
                    }

                    if (this.Mapper.DoIRQ())
                    {
                        this.cpu.cycle += this.cpu.IRQ();
                    }

                    for (int i = 0; i < 3 * dcycles; i++)
                    {
                        // DMA transfer: 
                        if (DMAActive)
                        {
                            if (!DMAStart)
                            {
                                if ((GlobalCycles % 2) == 1)
                                {
                                    DMAStart = true;
                                }
                            }
                            else
                            {
                                if ((GlobalCycles % 2) == 0)
                                {
                                    DMAData = this.cpu[(DMAPage << 8) | DMAAddr];
                                }
                                else
                                {
                                    this.ppu.oam[(this.ppu.OAMAddr + DMAAddr) % 0x100] = DMAData;
                                    DMAAddr++;

                                    if (DMAAddr == 0)
                                    {
                                        DMAStart = false;
                                        DMAActive = false;
                                    }
                                }
                            }
                        }

                        this.ppu.Step();

                        if (GlobalCycles % 6 == 0)
                        {
                            // APU runs at half the speed of the CPU
                            this.apu.Step();
                        }

                        t += dtPPUCycle;
                        if (t > dtAudioCycle)
                        {
                            this.speaker.AddSample(this.apu.GetSample());
                            t -= dtAudioCycle;
                        }

                        GlobalCycles++;
                    }

                    if (debug && (this.cpu.cycle > 85798))
                    {
                        //this.Log(this.cpu.GenLog() + " || PPU: " + this.ppu.GenLog());

                        //this.ppu.DrawNametable(0, 0);
                        //this.ppu.drawSpriteTable(1, 0);
                        //Console.ReadKey();
                    }

                }
            }
        }

        public void Run()
        {
            this.Run(false);
        }
    }
}
