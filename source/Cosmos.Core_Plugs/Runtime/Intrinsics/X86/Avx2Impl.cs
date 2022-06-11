using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86;

[Plug("System.Runtime.Intrinsics.X86.Avx2, System.Private.CoreLib")]
internal class Avx2Impl
{
    public static bool get_IsSupported() => false;
}

[Plug("System.Runtime.Intrinsics.X86.Avx2+X64, System.Private.CoreLib")]
internal class Avx2X86Impl
{
    public static bool get_IsSupported() => false;
}
