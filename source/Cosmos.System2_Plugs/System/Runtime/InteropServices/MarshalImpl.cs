using System;
using System.Runtime.InteropServices;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Runtime.InteropServices;

[Plug(typeof(Marshal))]
public static class MarshalImpl
{
    public static void SetLastWin32Error(int aInt) => throw new NotImplementedException();
}
