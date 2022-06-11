using System;
using System.Threading;
using Cosmos.HAL;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading;

[Plug(Target = typeof(Thread))]
public static class ThreadImpl
{
    public static void Sleep(TimeSpan timeout) => Global.PIT.Wait((uint)timeout.TotalMilliseconds);

    public static void Sleep(int millisecondsTimeout) => Global.PIT.Wait((uint)millisecondsTimeout);

    public static bool Yield() => throw new NotImplementedException("Thread.Yield()");

    public static int get_ManagedThreadId(object aItem) => throw new NotImplementedException();

    public static int GetCurrentProcessorNumber() => throw new NotImplementedException();

    public static int GetOptimalMaxSpinWaitsPerSpinIterationInternal() => throw new NotImplementedException();

    public static Thread GetCurrentThreadNative() => null;

    public static void MemoryBarrier()
    {
    }

    public static void SetBackgroundNative(Thread aThis, bool aBool) => throw new NotImplementedException();

    public static void StartupSetApartmentStateInternal(Thread aThis) => throw new NotImplementedException();

    public static void StartInternal(Thread aThis) => throw new NotImplementedException();

    public static void SetStart(Thread aThis, Delegate aDelegate, int aInt) => throw new NotImplementedException();

    public static void SpinWait(int aInt) => throw new NotImplementedException();
}
