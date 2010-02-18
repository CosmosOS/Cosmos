using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Capabilities
{
    public interface ICapability
    {
        IRevokeCapability GetRevokeCapability(); 

        bool IsNull { get; }
    }


    public interface IRevokeCapability :ICapability
    {
        void Revoke();

    }
}
