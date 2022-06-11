using System.Runtime.InteropServices;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Runtime.InteropServices;

[Plug(Target = typeof(Marshal))]
public static class MarshalImpl
{
    public static void CCtor()
    {
    }

    public static int GetLastWin32Error() => 0;
}
