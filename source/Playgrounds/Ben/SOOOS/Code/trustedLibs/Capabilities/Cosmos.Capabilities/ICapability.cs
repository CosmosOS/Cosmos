using System;
namespace Ben.Kernel.Capabilities
{
    // we must use an interface here as we do NOT 
    //wish to include a base class in an assembly visable for users
    interface ICapability   //: IPersistable
    {
        void Revoke(); 

        bool IsNull { get;  }
    }
}
