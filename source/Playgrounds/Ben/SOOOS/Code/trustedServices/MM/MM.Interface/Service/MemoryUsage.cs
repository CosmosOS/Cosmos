using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;

namespace MM
{
    public enum MemoryPressure : byte { None, Low, Medium, High, Critical }

    [IPCStruct]
    public struct MemoryUsage
    {
        internal ulong TotalMemory;
        internal ulong NonPagedMemory;
        internal ulong PagedMemory;
        internal MemoryPressure Pressure; // policy based
        internal MMDetails Details;


    }


    
}
