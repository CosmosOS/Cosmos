using System;
using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading;

[Plug(TargetName = "System.Threading.OverlappedData, System.Private.CoreLib")]
internal class OverlappedDataImpl
{
    [PlugMethod(Signature =
        "System_Threading_OverlappedData__System_Threading_OverlappedData_GetOverlappedFromNative_System_Threading_NativeOverlapped__")]
    public static unsafe object GetOverlappedFromNative(NativeOverlapped* a) => throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_Void__System_Threading_OverlappedData_FreeNativeOverlapped_System_Threading_NativeOverlapped__")]
    public static unsafe void FreeNativeOverlaapped(NativeOverlapped* a) => throw new NotImplementedException();
}
