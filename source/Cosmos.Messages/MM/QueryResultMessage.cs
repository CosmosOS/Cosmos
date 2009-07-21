using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.MM
{
    public class QueryResponse : MMMessage
    {
        public UIntPtr Page;
            PageEntry entry;


        public QueryResponse(PageEntry entry)
        {
            this.entry = entry; 
        }
    }
}
