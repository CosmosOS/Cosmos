using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware2.Audio
{
    public class PCMStream
    {
        double freq;
        char[] data;
        public PCMStream(double freq, char[] data)
        {
            this.freq = freq;
            this.data = data;
        }
        public char[] getData()
        {
            return data;
        }
        public double getFreq()
        {
            return freq;
        }
    }
}
