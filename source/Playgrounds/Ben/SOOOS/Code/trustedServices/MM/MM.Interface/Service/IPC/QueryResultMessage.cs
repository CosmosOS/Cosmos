using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ben.Services.MM
{
    public sealed class QueryResponse : MMMessage
    {
        public UIntPtr page { get; set; }
        uint ownerPid { get; set; }


        public QueryResponse(ushort id)
            : base(id)
        {
        }

        public override void Validate()
        {

        }

    }
}
