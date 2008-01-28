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
        private int val;

        public Random()
        {
            int max = Hardware.RTC.GetHours();
            max *= (int)Hardware.RTC.GetMinutes();
            max *= (int)Hardware.RTC.GetSeconds();
            val = (max / 432);
        }

        public int Next()
        {
            return val;
        }
    }
}
