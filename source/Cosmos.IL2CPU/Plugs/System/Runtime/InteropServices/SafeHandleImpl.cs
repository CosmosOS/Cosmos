using System.Runtime.InteropServices;

namespace Cosmos.IL2CPU.Plugs.System.Runtime.InteropServices
{
    [Plug(Target=typeof(SafeHandle))]
    public static class SafeHandleImpl
    {
        public static void InternalDispose(object aThis) { }
        public static void InternalFinalize(object aThis) { }
    }
}