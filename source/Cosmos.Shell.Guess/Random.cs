using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Guess
{
    /// <summary>
    /// Uses nasty security holed pointer logic to get the next
    /// value.
    /// </summary>
    public unsafe class Random
    {
        private byte* val;

        public Random()
        {
            int max = Hardware.RTC.GetHours();
            max *= (int)Hardware.RTC.GetMinutes();
            max *= (int)Hardware.RTC.GetSeconds();
            for (int i = 0; i < max; i++)
                Next();
            val += (byte)max;
        }

        public byte Next()
        {
            byte nxt = *val;
            val = (byte*)nxt;
            return nxt;
        }
    }
}
