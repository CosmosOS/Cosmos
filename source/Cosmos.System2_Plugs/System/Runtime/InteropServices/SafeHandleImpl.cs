using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Runtime.InteropServices
{
    [Plug(typeof(SafeHandle))]
    public class SafeHandleImpl
    {
        public static bool ReleaseHandle(SafeHandle aThis)
        {
            throw new NotImplementedException();
        }
    }
}
