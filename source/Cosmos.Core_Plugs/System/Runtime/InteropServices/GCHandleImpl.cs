using System;
using System.Runtime.InteropServices;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.InteropServices;

[Plug(typeof(GCHandle))]
internal class GCHandleImpl
{
    public static void InternalFree(IntPtr aIntPtr) => throw new NotImplementedException();

    public static IntPtr InternalAlloc(object aObject, GCHandleType aGCHandleType) =>
        throw new NotImplementedException();
}
