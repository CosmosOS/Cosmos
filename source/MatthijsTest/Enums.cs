using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatthijsTest
{
    [Flags]
    public enum RegularStatusFlagsEnum : byte
    {
        None = 0,
        /// <summary>
        /// ERR
        /// </summary>
        Error = 1,
        /// <summary>
        /// DRQ
        /// </summary>
        DataRequest = 1 << 3,
        /// <summary>
        /// Overlapped Mode Service Request (SRV)
        /// </summary>
        OverlappedServiceRequest = 1 << 4,
        /// <summary>
        /// DF
        /// </summary>
        DriveFault = 1 << 5,
        Ready = 1 << 6,
        Busy = 1 << 7
    }

    public enum CommandEnum : byte
    {
        Identify = 0xEC
    }

    public static class EnumExtensions
    {
        public static bool HasFlags(this RegularStatusFlagsEnum @this, RegularStatusFlagsEnum flags)
        {
            return (@this & flags) == flags;
        }
    }
}
