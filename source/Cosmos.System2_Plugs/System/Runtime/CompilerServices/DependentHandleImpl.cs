using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Runtime.CompilerServices
{
    [Plug("System.Runtime.CompilerServices.DependentHandle, System.Private.CoreLib")]
    class DependentHandleImpl
    {
        public static object nGetPrimaryAndSecondary(IntPtr aIntPtr, ref object aObject)
        {
            throw new NotImplementedException();
        }

        public static object nGetPrimary(IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        public static IntPtr nInitialize(object aObject, object aObject2)
        {
            throw new NotImplementedException();
        }
    }
}
