using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading;

[Plug(TargetName = "System.Threading.ThreadPoolBoundHandleOverlapped, System.Private.CoreLib")]
internal class ThreadPoolBoundHandleOverlapped
{
    public static void Cctor()
    {
    }
}
