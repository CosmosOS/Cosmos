using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ben.Services.MM
{
    public sealed class MemoryInformationResponse : MMMessage
    {
        public ulong TotalMemory { get; set; }
        public ulong NonPagedMemory { get; set; }
        public ulong PagedMemory { get; set; }
        public MemoryPressure Pressure { get; set; }
        public MemoryDetails PagedMemoryDetails { get; set; }


        public MemoryInformationResponse(ushort id)
            : base(id)
        {
        }

        public override void Validate()
        {

        }
       
    }
}
