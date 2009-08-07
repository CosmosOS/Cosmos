using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cosmos.Kernel.MM
{
    public class FreeMemoryRequest : MMMessage
    {
        //Process Process;  SENDER ONLY ? 
        public UIntPtr Address;
        public uint Pages; 

    }
}
