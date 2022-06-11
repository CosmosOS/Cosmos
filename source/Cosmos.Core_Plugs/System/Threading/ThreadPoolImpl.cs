using System;
using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading;

[Plug(typeof(ThreadPool))]
internal class ThreadPoolImpl
{
    public static bool GetEnableWorkerTracking() => throw new NotImplementedException();

    [PlugMethod(Signature = "Interop_BOOL__System_Threading_ThreadPool_RequestWorkerThread__")]
    public static bool RequestWorkerThread() => throw new NotImplementedException();

    public static void NotifyWorkItemProgressNative() => throw new NotImplementedException();
}
