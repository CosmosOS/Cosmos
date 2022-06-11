using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86;

[Plug("System.Runtime.Intrinsics.X86.Bmi1, System.Private.CoreLib")]
internal class Bmi1Impl
{
    public static bool get_IsSupported() => false;
}
