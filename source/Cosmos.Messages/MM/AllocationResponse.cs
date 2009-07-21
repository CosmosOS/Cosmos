using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.MM
{
    public class AllocationResponse : MMMessage
    {
        private UIntPtr address;

        public UIntPtr Address
        {
            get { return address; }

        }


        public AllocationResponse(UIntPtr Address)
        {
            address = Address;
        }
    }
}
