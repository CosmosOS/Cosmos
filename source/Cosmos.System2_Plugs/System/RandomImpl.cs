using System;
using IL2CPU.API.Attribs;
using Cosmos.HAL;

namespace Cosmos.System_Plugs.System
{
    /*
     * Code taken from link below and modified to fit the scheme
     * https://referencesource.microsoft.com/#mscorlib/system/random.cs,62cd8ffb36191d74,references
     */
    [Plug(Target = typeof(Random))]
    public static class RandomImpl
    {
        //
        // Private Constants
        //
        private const int MBIG = Int32.MaxValue;
        private const int MSEED = 161803398;
        private const int MZ = 0;
        private static int counter = 0;

        //
        // Member Variables
        //
        private static int inext;
        private static int inextp;
        private static int[] SeedArray = new int[56];


        public static void Ctor(Random aThis)
        {
            Ctor(aThis, GenerateSeed());
        }

        public static void Ctor(Random aThis, int seed)
        {
            //empty ATM
            int ii;

            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            int subtraction = seed == Int32.MinValue ? Int32.MaxValue : Math.Abs(seed);
            var mj = MSEED - subtraction;
            SeedArray[55] = mj;
            var mk = 1;
            for (int i = 1; i < 55; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = 21 * i % 55;
                SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0)
                {
                    mk += MBIG;
                }
                mj = SeedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                    if (SeedArray[i] < 0)
                    {
                        SeedArray[i] += MBIG;
                    }
                }
            }
            inext = 0;
            inextp = 21;
            seed = 1;
        }

        private static double Sample()
        {
            //Including this division at the end gives us significantly improved
            //random number distribution.
            return InternalSample() * (1.0 / MBIG);
        }

        private static int InternalSample()
        {
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= 56)
            {
                locINext = 1;
            }
            if (++locINextp >= 56)
            {
                locINextp = 1;
            }

            var retVal = SeedArray[locINext] - SeedArray[locINextp];

            if (retVal == MBIG)
            {
                retVal--;
            }
            if (retVal < 0)
            {
                retVal += MBIG;
            }

            SeedArray[locINext] = retVal;

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        public static int Next(Random aThis, int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentNullException(nameof(maxValue), "Argument out of range, must be postive.");
            }
            return (int)(Sample() * maxValue);
        }

        public static int Next(Random aThis)
        {
            return InternalSample();
        }

        private static double GetSampleForLargeRange()
        {
            // The distribution of double value returned by Sample
            // is not distributed well enough for a large range.
            // If we use Sample for a range [Int32.MinValue..Int32.MaxValue)
            // We will end up getting even numbers only.

            int result = InternalSample();
            // Note we can't use addition here. The distribution will be bad if we do that.
            bool negative = InternalSample() % 2 == 0 ? true : false;  // decide the sign based on second sample
            if (negative)
            {
                result = -result;
            }
            double d = result;
            d += Int32.MaxValue - 1; // get a number in range [0 .. 2 * Int32MaxValue - 1)
            d /= 2 * (uint)Int32.MaxValue - 1;
            return d;
        }

        public static int Next(Random aThis, int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Argument out of range, must be postive");
            }

            long range = (long)maxValue - minValue;
            if (range <= (long)Int32.MaxValue)
            {
                return (int)(Sample() * range) + minValue;
            }
            else
            {
                return (int)((long)(GetSampleForLargeRange() * range) + minValue);
            }
        }

        public static void NextBytes(Random aThis, byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(InternalSample() % (Byte.MaxValue + 1));
            }
        }

        public static double NextDouble(Random aThis)
        {
            return Sample();
        }

        private static int GenerateSeed()
        {
            counter++;

            if (counter == Int32.MaxValue - 1)
            {
                counter = 0;
            }

            return counter + (int)(Cosmos.Core.CPU.GetCPUUptime() / 50) * 1000;
        }
    }
}
