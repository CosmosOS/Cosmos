using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.MM
{
    public class MemoryAllocationRequest : MMMessage
    {

        public UIntPtr Address; 
        public ulong NumBytes;
        public ulong ReservedBytes;
        public uint Allignment;
        public PageType PageType;
        public PageSharing Sharing;

    }
}
