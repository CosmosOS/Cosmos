using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86;

[Plug("System.Runtime.Intrinsics.X86.Lzcnt, System.Private.CoreLib")]
internal class LzcntImpl
{
    public static bool get_IsSupported() => false;
}

[Plug("System.Runtime.Intrinsics.X86.Lzcnt+X64, System.Private.CoreLib")]
internal class X64LzcntImpl
{
    public static bool get_IsSupported() => false;
}
