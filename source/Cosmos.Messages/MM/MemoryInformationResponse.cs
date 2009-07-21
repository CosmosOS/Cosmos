using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.MM
{
    public class MemoryInformationResponse : MMMessage
    {
        public ulong TotalMemory;
        public ulong NonPagedMemory;
        public ulong PagedMemory;
        public MemoryPressure Pressure;
        public MemoryDetails PagedMemoryDetails;



        internal MemoryInformationResponse(MemoryUsage usage)
        {

            TotalMemory = usage.TotalMemory;
            NonPagedMemory = usage.NonPagedMemory;
            PagedMemory = usage.PagedMemory;
            Pressure = usage.pressure;
            PagedMemoryDetails = usage.details;


        }
    }
}
