using System;
using System.Threading;
using NAudio.Wave;

namespace NesEmulator.Audio
{
    public class Speaker
    {
        readonly WaveFormat wf;
        readonly BufferedWaveProvider Buffer;
        byte[] TempBuffer;
        int TempBufferedSamples = 0;
        readonly Thread Playback;

        public Speaker(WaveFormat wf)
        {
            this.wf = wf;
            this.Buffer = new BufferedWaveProvider(wf);
            this.TempBuffer = new byte[1000];

            this.Playback = new Thread(() => Play(this.Buffer));
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

        public bool NeedMoreSamples()
        {
            // Longer buffer: more delay, less artifacts
            return this.Buffer.BufferedBytes <= 2.0 * TempBuffer.Length;
        }

        public static void Play(BufferedWaveProvider bf)
        {
            WaveOut wo = new WaveOut();
            // wo.DesiredLatency = 10;
            wo.NumberOfBuffers = 50;
            wo.Init(bf);
            wo.DesiredLatency = 0;
            wo.Play();
            while (wo.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(500);
            }
            wo.Dispose();
        }
    }
}
