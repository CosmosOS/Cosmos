using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.InteropServices
{
    [Plug(typeof(global::System.Runtime.InteropServices.GCHandle))]
    class GCHandleImpl
    {
        public static void InternalFree(IntPtr aIntPtr)
        {
            throw new NotImplementedException();
        }

        public static IntPtr InternalAlloc(object aObject, GCHandleType aGCHandleType)
        {
            throw new NotImplementedException();
        }
    }
}
