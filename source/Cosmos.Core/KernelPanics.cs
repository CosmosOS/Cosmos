using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core
{
    /// <summary>
    /// Kernel panic codes used by cosmos
    /// </summary>
    public static class KernelPanics
    {
        public const uint VMT_MethodNotFound = 0x1;
        public const uint VMT_MethodFoundButAddressInvalid = 0x2;
        public const uint VMT_MethodAddressesNull = 0x3;
        public const uint VMT_MethodIndexesNull = 0x4;
        public const uint VMT_TypeIdInvalid = 0x5;
    }
}
