using System.Runtime.InteropServices;
using Cosmos.IL2CPU.API;

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
