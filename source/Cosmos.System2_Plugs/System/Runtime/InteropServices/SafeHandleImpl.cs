using System.Runtime.InteropServices;
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

        public static void Dispose(SafeHandle aThis)
        {
            throw new NotImplementedException();
        }
    }
}