using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Capabilities
{

    public abstract class CapabilityBase : ICapability
    {
        public static NullCapability NullCapability = new NullCapability();

        public bool IsNull
        {
            get { return true; }
        }


       protected ulong capId;  // do not expose. 


       protected string WebKey
       {
           get { throw new NotImplementedException(); }
       }
        //    public EndPointCapability EndPointCapability;








       public abstract IRevokeCapability GetRevokeCapability();
    }
}