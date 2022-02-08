using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86
{
    [Plug("System.Runtime.Intrinsics.X86.Sse41, System.Private.CoreLib")]
    class Sse41Impl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
    [Plug("System.Runtime.Intrinsics.X86.Sse41+X64, System.Private.CoreLib")]
    class Sse41X86Impl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
}
