using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Guess
{
    /// <summary>
    /// Uses nasty security holed pointer logic to get the next
    /// value. This is our PRNG
    /// </summary>
    public unsafe class Random
    {
        private int a = 214013;
        private int x = 0x72535;
        private int c = 2531011;


        public Random() : this( 
                (int)Cosmos.Hardware.Global.TickCount 
                + Cosmos.Hardware.RTC.GetSeconds())
        {
        }

        public Random(int seed)
        {
            x = seed;
        }

        public int Next(int p)
        {
            x = (a * x + c);
            return x % p;
        }

    }
}
