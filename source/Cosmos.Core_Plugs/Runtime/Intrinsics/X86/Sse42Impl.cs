using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86
{
    [Plug("System.Runtime.Intrinsics.X86.Sse42, System.Private.CoreLib")]
    class Sse42Impl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
    [Plug("System.Runtime.Intrinsics.X86.Sse42+X64, System.Private.CoreLib")]
    class Sse42X86Impl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
}
