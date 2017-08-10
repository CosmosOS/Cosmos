using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys
{
    public static class BootInfo
    {
        public static uint GetVbeControlInfoAddr()
        {
            return UInt32.MaxValue;
        }

        public static uint GetVbeModeInfoAddr()
        {
            return UInt32.MaxValue;
        }

        public static ushort GetVbeMode()
        {
            return ushort.MaxValue;
        }
    }
}
