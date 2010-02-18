using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ben.Services.MM
{
    public class FreeMemoryRequest : MMMessage
    {
        //Process Process;  SENDER ONLY ? 
        public UIntPtr Address;
        public uint Pages;

        public FreeMemoryRequest(ushort id)
            : base(id)
        {
        }

        public override void Validate()
        {

        }

    }
}
