using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.MM
{
    internal struct MemoryUsage
    {
        internal ulong TotalMemory;
        internal ulong NonPagedMemory;
        internal ulong PagedMemory;
        internal MemoryPressure pressure;
        internal MemoryDetails details;


    }
}
