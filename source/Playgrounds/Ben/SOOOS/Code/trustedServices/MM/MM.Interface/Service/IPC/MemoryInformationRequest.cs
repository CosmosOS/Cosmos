using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ben.Services.MM
{
    public sealed class MemoryInformationRequest : MMMessage
    {



        public MemoryInformationRequest(ushort id)
            : base(id)
        {
        }


        public override void Validate()
        {

        }
    }
}
