using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ben.Kernel.Capabilities
{
    public abstract class Capability : ICapability
    {
        public static NullCapability NullCapability = new NullCapability();

        public bool IsNull
        {
            get { return true; }
        } 


        protected ulong capId;  // do not expose. 

    //    public EndPointCapability EndPointCapability;




        
    }
}
