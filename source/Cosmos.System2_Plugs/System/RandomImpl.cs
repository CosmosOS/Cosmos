using IL2CPU.API.Attribs;

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
        private const int mBIG = int.MaxValue;
        private const int mSEED = 161803398;
        private const int mZ = 0;
        private static int counter = 0;

        //
        // Member Variables
        //
        private static int inext;
        private static int inextp;
        private static readonly int[] seedArray = new int[56];


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
            int subtraction = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
            var mj = mSEED - subtraction;
            seedArray[55] = mj;
            var mk = 1;
            for (int i = 1; i < 55; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = 21 * i % 55;
                seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0)
                {
                    mk += mBIG;
                }
                mj = seedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    seedArray[i] -= seedArray[1 + (i + 30) % 55];
                    if (seedArray[i] < 0)
                    {
                        seedArray[i] += mBIG;
                    }
                }
            }
            inext = 0;
            inextp = 21;
        }

        private static double Sample()
        {
            //Including this division at the end gives us significantly improved
            //random number distribution.
            return InternalSample() * (1.0 / mBIG);
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

            var retVal = seedArray[locINext] - seedArray[locINextp];

            if (retVal == mBIG)
            {
                retVal--;
            }
            if (retVal < 0)
            {
                retVal += mBIG;
            }

            seedArray[locINext] = retVal;

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
            d += int.MaxValue - 1; // get a number in range [0 .. 2 * Int32MaxValue - 1)
            d /= 2 * (uint)int.MaxValue - 1;
            return d;
        }

        public static int Next(Random aThis, int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Argument out of range, must be postive");
            }

            long range = (long)maxValue - minValue;
            if (range <= int.MaxValue)
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
                buffer[i] = (byte)(InternalSample() % (byte.MaxValue + 1));
            }
        }

        public static double NextDouble(Random aThis)
        {
            return Sample();
        }

        private static int GenerateSeed()
        {
            counter++;

            if (counter == int.MaxValue - 1)
            {
                counter = 0;
            }

            return counter + (int)(Core.CPU.GetCPUUptime() / 50) * 1000;
        }
    }
}