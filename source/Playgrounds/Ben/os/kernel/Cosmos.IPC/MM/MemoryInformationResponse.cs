using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.MM
{
    public class MemoryInformationResponse : MMMessage
    {
        public ulong TotalMemory { get; set; }
        public ulong NonPagedMemory { get; set; }
        public ulong PagedMemory { get; set; }
        public MemoryPressure Pressure { get; set; }
        public MemoryDetails PagedMemoryDetails { get; set; } 



       
    }
}
