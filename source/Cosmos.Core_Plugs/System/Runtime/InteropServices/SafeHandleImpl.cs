using System.Runtime.InteropServices;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.InteropServices
{
    [Plug(Target = typeof(SafeHandle))]
    public static class SafeHandleImpl
    {
        public static void InternalDispose(object aThis)
        {
        }

        public static void InternalFinalize(object aThis)
        {
        }
    }
}
