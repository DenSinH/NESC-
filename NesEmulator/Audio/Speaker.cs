using System;
using System.Threading;
using NAudio.Wave;

namespace NesEmulator.Audio
{
    public class Speaker
    {
        class ShutDownEvent
        {
            public bool ShutDown;
        }

        private readonly WaveFormat wf;
        private readonly BufferedWaveProvider Buffer;
        private byte[] TempBuffer;
        private int TempBufferedSamples = 0;
        private readonly Thread Playback;
        private ShutDownEvent sd;

        public Speaker(WaveFormat wf)
        {
            this.wf = wf;
            this.Buffer = new BufferedWaveProvider(wf);
            this.TempBuffer = new byte[150];

            this.sd = new ShutDownEvent();
            this.sd.ShutDown = false;

            this.Playback = new Thread(() => Play(this.Buffer, this.sd));
            this.Playback.Start();
        }

        public void AddSample(ushort Sample)
        {
            byte[] bytes = BitConverter.GetBytes(Sample);

            TempBuffer[TempBufferedSamples] = bytes[0];
            TempBuffer[TempBufferedSamples + 1] = bytes[1];
            TempBufferedSamples += 2;

            if (TempBufferedSamples == TempBuffer.Length)
            {
                lock (this.Buffer)
                {
                    this.Buffer.AddSamples(TempBuffer, 0, TempBuffer.Length);
                }
                TempBufferedSamples = 0;
            }
        }

        public void ClearBuffer()
        {
            this.Buffer.ClearBuffer();
        }

        public void ShutDown()
        {
            lock (this.sd)
            {
                this.sd.ShutDown = true;
            }
        }

        public bool NeedMoreSamples()
        {
            // Longer buffer: more delay, less artifacts
            return this.Buffer.BufferedBytes <= 7.5 * TempBuffer.Length;
        }

        private static void Play(BufferedWaveProvider bf, ShutDownEvent sd)
        {
            WaveOut wo = new WaveOut();
            // wo.DesiredLatency = 10;
            wo.NumberOfBuffers = 50;
            wo.Init(bf);
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing && !sd.ShutDown)
            {
                Thread.Sleep(500);
            }
            wo.Dispose();
        }
    }
}
