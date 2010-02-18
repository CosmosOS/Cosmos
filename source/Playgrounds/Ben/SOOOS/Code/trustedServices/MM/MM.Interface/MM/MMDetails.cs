using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;

namespace MM
{

    [IPCStruct]
    public struct MMDetails
    {
        public float Fragmentation;// 1 - (largest segment + 2nd largest segment /2  etc )/ amount Free if 50 Meg left , and largest seg = 25 then frag = 50%. if 5 Meg left fragm = 90%.
        public float InternalFragmentation;
        public ulong BytesFree;
        public ulong BytesManaged;
        public ulong LargestSegment;
        public uint AllocsInLastSecond;
        public uint FreesInLastSecond;

    }
}
