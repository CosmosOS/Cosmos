using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console
{
    public class MT
    {
        private int mt_index;
        private int[] mt_buffer = new int[624];

        public MT()
            : this(100)
        {
        }

        public MT(int seed)
        {
            for (int i = 0; i < 624; i++)
                mt_buffer[i] = RandomNumber() + seed++;
            RandomNumber();
        }

        public int Next()
        {
            return RandomNumber();
        }

        private int RandomNumber()
        {
            if (mt_index == 624)
            {
                mt_index = 0;
                int i = 0;
                int s;
                for (; i < 624 - 397; i++)
                {
                    s = (int)(mt_buffer[i] & 0x80000000) | (mt_buffer[i + 1] & 0x7FFFFFFF);
                    mt_buffer[i] = (int)(mt_buffer[i + 397] ^ (s >> 1) ^ ((s & 1) * 0x9908B0DF));
                }
                for (; i < 623; i++)
                {
                    s = (int)(mt_buffer[i] & 0x80000000) | (mt_buffer[i + 1] & 0x7FFFFFFF);
                    mt_buffer[i] = (int)(mt_buffer[i - (624 - 397)] ^ (s >> 1) ^ ((s & 1) * 0x9908B0DF));
                }

                s = (int)(mt_buffer[623] & 0x80000000) | (mt_buffer[0] & 0x7FFFFFFF);
                mt_buffer[623] = (int)(mt_buffer[396] ^ (s >> 1) ^ ((s & 1) * 0x9908B0DF));
            }
            return mt_buffer[mt_index++];
        }
    }
}
