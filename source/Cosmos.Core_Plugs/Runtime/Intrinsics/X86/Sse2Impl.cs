using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86
{
    [Plug("System.Runtime.Intrinsics.X86.Sse2+X64", "System.Private.CoreLib")]
    class X86Impl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
}
