using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Ben.Services.MM
{
    public class QueryPageRequest : MMMessage
    {
        public UIntPtr Address;

        
        public QueryPageRequest(ushort id)
            : base(id)
        {
        }


        public override void Validate()
        {

        }

    }
}
