﻿using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.HAL;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(Random))]
    public class RandomImpl
    {
        public static void Ctor(Random aThis)
        {
            //empty
        }

        public static void Ctor(Random aThis, int seed)
        {
            //empty ATM
        }

        public static int Next(Random aThis, int maxValue)
        {
            return (int)(GetUniform() * maxValue);
        }

        public static int Next(Random aThis)
        {
            return (int)(GetUniform() * int.MaxValue);
        }

        public static int Next(Random aThis, int minValue, int maxValue)
        {
            uint diff = (uint)(maxValue - minValue);
            if (diff <= 1)
                return minValue;
            return (int)((uint)(GetUniform() * diff) + minValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(GetUniform() * (byte.MaxValue + 1));
            }
        }

        public static double NextDouble()
        {
            return GetUniform();
        }

        static double GetUniform()
        {
            uint seed = (uint)RTC.Second;
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