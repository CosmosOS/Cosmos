using System;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System;

[Plug(Target = typeof(IntPtr))]
public static class IntPtrImpl
{
    //  //[PlugMethod(Signature="System_String___System_IntPtr_ToString____")]
    public static string ToString(IntPtr aThis) => "<IntPtr>";
    //}

    public static int GetHashCode(ref IntPtr aThis) => (int)aThis;
}
