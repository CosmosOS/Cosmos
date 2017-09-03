using global::System;
using Sys = System;
//using System;
using System.Text;
using System.Collections.Generic;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.IL2CPU.API;
using Cosmos.HAL;
using System.Numerics;

namespace Cosmos.Core_Plugs.System
{
    [Plug(TargetName = "System.Random, System.Private.CoreLib")]
    public static class RandomImpl
    {
        public static void Cctor()
        {
        }

        public static void Ctor(Random aThis)
        {
        }

        public static void Ctor(Random aThis, int Seed)
        {

        }

        public static int GenerateGlobalSeed()
        {
            throw new NotImplementedException("GenerateGlobalSeed() not implemented yet");
        }
        

        public static int Next(Random aThis)
        {
            return (int)(Sample() * 2147483647);
        }

        public static int Next(Random aThis, int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue");
            }
            return (int)(Sample() * maxValue);
        }

        public static int Next(Random aThis, int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue");
            }
            long num = maxValue - (long)minValue;
            if (num <= 2147483647L)
            {
                return (int)(Sample() * (double)num) + minValue;
            }
            return (int)((long)(GetSampleForLargeRange(aThis) * (double)num) + (long)minValue);
        }

        public static double NextDouble(Random aThis)
        {
            return Sample();
        }

        public static void NextBytes(Random aThis, byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(Sample() % 256);
        }

        public static double GetSampleForLargeRange(Random aThis)
        {
            int num = (int)Sample();
            bool flag = Sample() % 2 == 0;
            if (flag)
            {
                num = -num;
            }
            double num2 = (double)num;
            num2 += 2147483646.0;
            return num2 / 4294967293.0;
        }
        static ulong next = 1;

        public static double Sample()
        {
            next = next * 1103515245 + 12345;
            uint seed = (uint)next;
            uint m_w = (uint)(seed >> 16);
            uint m_z = (uint)(seed % 4294967296);
            m_z = 36969 * (m_z & 65535) + (m_z >> 16);
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);
            uint u = (m_z << 16) + m_w;
            double uniform = (u + 1.0) * 2.328306435454494e-10;
            return uniform;
        }

        public static double SampleSeed(int Seed)
        {
            next = (ulong)Seed;
            uint seed = (uint)next;
            uint m_w = (uint)(seed >> 16);
            uint m_z = (uint)(seed % 4294967296);
            m_z = 36969 * (m_z & 65535) + (m_z >> 16);
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);
            uint u = (m_z << 16) + m_w;
            double uniform = (u + 1.0) * 2.328306435454494e-10;
            return uniform;
        }
    }
}
