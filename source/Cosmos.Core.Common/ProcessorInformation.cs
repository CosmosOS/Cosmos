namespace Cosmos.Core
{
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

        internal static int CanReadCPUID() => 0; //plugged

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
