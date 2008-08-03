using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio
{
    public class PCMStream
    {
        int rate;
        short[] data;
        double phase;
        public PCMStream(int rate, short[] data, double phase)
        {
            this.rate = rate;
            this.data = data;
            this.phase = phase;

        }

    }
}
