using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core
{
    /// <summary>
    /// Manages basic CPU ID functions
    /// </summary>
    public static unsafe class CPUID
    {
        /// <summary>
        /// Returns the CPU vendor name
        /// </summary>
        /// <returns>CPU vendor name</returns>
        public static string GetVendorName()
        {
            if (canreadcpuid() > 0)
            {
                int[] raw = new int[3];

                fixed (int* ptr = raw)
                    fetchcpuvendor(ptr);

                return new string(new char[] {
                    (char)(raw[0] >> 24),
                    (char)((raw[0] >> 16) & 0xff),
                    (char)((raw[0] >> 8) & 0xff),
                    (char)(raw[0] & 0xff),
                    (char)(raw[1] >> 24),
                    (char)((raw[1] >> 16) & 0xff),
                    (char)((raw[1] >> 8) & 0xff),
                    (char)(raw[1] & 0xff),
                    (char)(raw[2] >> 24),
                    (char)((raw[2] >> 16) & 0xff),
                    (char)((raw[2] >> 8) & 0xff),
                    (char)(raw[2] & 0xff),
                });
            }
            else
                return "\0";
        }

        [PlugMethod(PlugRequired = true)]
        internal static extern int canreadcpuid(); // plugged;

        [PlugMethod(PlugRequired = true)]
        internal static extern void fetchcpuvendor(int* target); // plugged;
    }
}

