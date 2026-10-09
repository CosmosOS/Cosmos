using System;
using System.Diagnostics;
using System.Threading;
using SysThread = System.Threading.Thread;

namespace DevKernel.Diagnostics;

// Pool of worker threads that alternate between burning CPU and sleeping, used to
// create a controllable load for the monitor to display.
//
// Workers exit on their own: the pool never kills a thread, it only publishes how
// many of them are expected to leave and each worker claims one of those requests
// at the top of its loop.
internal static class CpuStressPool
{
    public const int MaxThreads = 8;

    private const int BurnMs = 30;
    private const int IdleMs = 400;

    private static int s_alive;
    private static int s_exitRequests;
    private static int s_shutdown;

    public static int Alive => Volatile.Read(ref s_alive);

    public static int ExitRequests => Volatile.Read(ref s_exitRequests);

    // Threads that are running and have not been asked to leave yet.
    public static int Active
    {
        get
        {
            int active = Alive - ExitRequests;
            return active < 0 ? 0 : active;
        }
    }

    public static void Reset()
    {
        Volatile.Write(ref s_alive, 0);
        Volatile.Write(ref s_exitRequests, 0);
        Volatile.Write(ref s_shutdown, 0);
    }

    // Spawns or retires workers so that Active converges towards targetThreads.
    public static void SetThreadCount(int targetThreads)
    {
        int active = Active;

        for (int i = active; i < targetThreads; i++)
        {
            Interlocked.Increment(ref s_alive);
            try
            {
                new SysThread(BurnLoop).Start();
            }
            catch
            {
                Interlocked.Decrement(ref s_alive);
                break;
            }
        }

        for (int i = targetThreads; i < active; i++)
        {
            Interlocked.Increment(ref s_exitRequests);
        }
    }

    // Asks every worker to leave and waits for them, up to timeoutMs.
    public static void ShutdownAndWait(int timeoutMs)
    {
        Volatile.Write(ref s_shutdown, 1);

        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * timeoutMs / 1000;
        while (Alive > 0 && Stopwatch.GetTimestamp() < deadline)
        {
            SysThread.Sleep(50);
        }
    }

    private static void BurnLoop()
    {
        long burnTicks = Stopwatch.Frequency * BurnMs / 1000;

        try
        {
            while (Volatile.Read(ref s_shutdown) == 0)
            {
                if (ClaimExitRequest())
                {
                    return;
                }

                long burnEnd = Stopwatch.GetTimestamp() + burnTicks;
                while (Stopwatch.GetTimestamp() < burnEnd)
                {
                }

                SysThread.Sleep(IdleMs);
            }
        }
        finally
        {
            Interlocked.Decrement(ref s_alive);
        }
    }

    // Takes one pending exit request, if any, and reports whether this thread got it.
    private static bool ClaimExitRequest()
    {
        if (Interlocked.Decrement(ref s_exitRequests) >= 0)
        {
            return true;
        }

        Interlocked.Increment(ref s_exitRequests);
        return false;
    }
}
