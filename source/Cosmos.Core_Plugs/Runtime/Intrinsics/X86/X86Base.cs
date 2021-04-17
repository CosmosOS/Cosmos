using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.Runtime.Intrinsics.X86
{
    [Plug("System.Runtime.Intrinsics.X86.X86Base, System.Private.CoreLib")]
    class X86BaseImpl
    {
        public static bool get_IsSupported()
        {
            return false;
        }
    }
}
