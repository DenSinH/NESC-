using System;

namespace NesEmulator
{
    abstract class Channel
    {

        public static int SampleRate = 44100;

        public abstract short GetSample();

    }
}
