using System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(UIntPtr))]
public static class UIntPtrImpl
{
    //  //[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
    public static string ToString(UIntPtr aThis) => "<UIntPtr>";
    //}

    public static int GetHashCode(ref UIntPtr aThis) => (int)aThis;
}
