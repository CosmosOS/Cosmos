using Cosmos.IL2CPU.Plugs;
using System;

namespace Cosmos.IL2CPU.CustomImplementation.System
{
    [Plug(Target = typeof(Delegate))]
    public static unsafe class DelegateImpl
    {
        //[PlugMethod(Signature = "System_Boolean__System_Delegate_InternalEqualTypes_System_Object__System_Object_")]
        public static bool InternalEqualTypes(object a, object b)
        {
            //return *aDelegate1 == *aDelegate2;
            return false;
        }
    }
}