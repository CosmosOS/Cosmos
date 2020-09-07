namespace Cosmos.Core
{
    /// <summary>
    /// ProcessorInformation class. Used to get vendor information from the CPU.
    /// </summary>
    public unsafe class ProcessorInformation
    {
        /// <summary>
        /// Returns the Processor's vendor name
        /// </summary>
        /// <returns>CPU Vendor name</returns>
        public static string GetVendorName()
        {
            if (CanReadCPUID() > 0)
            {
                int[] raw = new int[3];

                fixed (int* ptr = raw)
                    FetchCPUVendor(ptr);

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

        /// <summary>
        /// Check if can read CPU ID.
        /// </summary>
        /// <returns>int value.</returns>
        internal static int CanReadCPUID() => 0; //plugged

        /// <summary>
        /// Fetch CPU vendor.
        /// </summary>
        /// <param name="target">pointer to target.</param>
        internal static void FetchCPUVendor(int* target) { } //plugged

        /// <summary>
        /// Returns the number of CPU cycles since startup of the current CPU core
        /// </summary>
        /// <returns>Number of CPU cycles since startup</returns>
        public static long GetCycleCount() => 0; //plugged

        /// <summary>
        /// Returns the number of CPU cycles per seconds
        /// </summary>
        /// <returns>Number of CPU cycles per seconds</returns>
        public static long GetCycleRate() => 0; //plugged
    }
}
