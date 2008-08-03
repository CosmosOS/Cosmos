using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokuTest.SampleSounds
{
    public static class SoundSamples
    {

        public static Cosmos.Hardware.Audio.PCMStream generateSineWaveForm(double freq, int rate, double phase)
        {
            double max_phase = 1.0 / freq;
            double step = 1.0 / (double)rate;
            double res;
            float fres;
            //PCMStream pcmStream = new PCMStream();
            List<int> frames = new List<int>();

            int ind_chan;
            //while (count-- > 0)
            {/*
                for (ind_chan = 0; ind_chan < nChannels; ind_chan++)
                {
                    if (sampleSize == 8)
                    {
                        if (chn == channel)
                        {
                        }
                    }
                }*/
            }
            return null;
        }
    }
}
