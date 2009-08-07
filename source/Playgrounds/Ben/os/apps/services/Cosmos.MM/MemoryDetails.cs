using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.MM
{
    public struct MemoryDetails
    {
        public float fragmentation;// 1 - (largest segment + 2nd largest segment /2  etc )/ amount Free if 50 Meg left , and largest seg = 25 then frag = 50%. if 5 Meg left fragm = 90%.
        public float amountFree;
        public ulong largestSegment;
        public uint numPages;
        public ulong memoryManaged;
        public uint pageSize;
    }
}
