// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Threading;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Threading;
using SysThread = System.Threading.Thread;

namespace DevKernel.Diagnostics.Smp;

// One thread of the load the SMP monitor shows. The scenario starts it, changes what it
// does and asks it to leave; it never kills it.
internal sealed class SmpWorker
{
    public const int UnknownThreadId = -1;

    // A spinner reads its orders again this often.
    private const int SpinSliceMs = 20;

    private const int BurstBurnMs = 350;
    private const int BurstSleepMs = 50;

    private const int AllocationBytes = 2048;
    private const int AllocationsPerSlice = 512;

    private const int IdReadAttempts = 16;

    private int _mode;
    private int _exitRequested;
    private int _exited;
    private int _threadId = UnknownThreadId;

    public SysThread Thread { get; }

    // The scheduler's id of the worker's thread, once the worker has run and read it.
    public int ThreadId => Volatile.Read(ref _threadId);

    public bool HasExited => Volatile.Read(ref _exited) != 0;

    public SmpWorkerMode Mode
    {
        get => (SmpWorkerMode)Volatile.Read(ref _mode);
        set => Volatile.Write(ref _mode, (int)value);
    }

    public SmpWorker(SmpWorkerMode mode)
    {
        _mode = (int)mode;
        Thread = new SysThread(Run);
    }

    public void Start()
    {
        Thread.Start();
    }

    public void RequestExit()
    {
        Volatile.Write(ref _exitRequested, 1);
    }

    // The scheduler's id of the calling thread. The diagnostics report the thread a CPU
    // runs, and a thread may change CPU between asking which CPU it is on and reading that
    // CPU's thread. The answer is kept when the CPU is still the same after the read, and
    // the same id comes out twice in a row.
    public static int ReadCurrentThreadId()
    {
        int previous = UnknownThreadId;

        for (int attempt = 0; attempt < IdReadAttempts; attempt++)
        {
            uint cpu = KernelCpus.Current;
            if (!SchedulerDiagnostics.TryGetCurrentThread(cpu, out KernelThreadInfo info)
                || KernelCpus.Current != cpu
                || info.IsIdle)
            {
                previous = UnknownThreadId;
                continue;
            }

            if ((int)info.Id == previous)
            {
                return previous;
            }

            previous = (int)info.Id;
        }

        return UnknownThreadId;
    }

    // Keeps the CPU busy for that long. The empty loop is the point: it is the load.
    public static void Burn(int milliseconds)
    {
        long deadline = SmpClock.DeadlineIn(milliseconds);
        while (!SmpClock.HasPassed(deadline))
        {
        }
    }

    private void Run()
    {
        try
        {
            Volatile.Write(ref _threadId, ReadCurrentThreadId());

            while (Volatile.Read(ref _exitRequested) == 0)
            {
                switch (Mode)
                {
                    case SmpWorkerMode.Burst:
                        Burn(BurstBurnMs);
                        SysThread.Sleep(BurstSleepMs);
                        break;
                    case SmpWorkerMode.Allocate:
                        Allocate();
                        break;
                    default:
                        Burn(SpinSliceMs);
                        break;
                }
            }
        }
        finally
        {
            Volatile.Write(ref _exited, 1);
        }
    }

    // Garbage only: each array is dropped as soon as the next one replaces it.
    private static void Allocate()
    {
        byte[]? last = null;
        for (int i = 0; i < AllocationsPerSlice; i++)
        {
            last = new byte[AllocationBytes];
            last[0] = (byte)i;
        }

        GC.KeepAlive(last);
    }
}

// What a worker of the SMP monitor's load does with its CPU time.
internal enum SmpWorkerMode
{
    // Burns CPU and never blocks: only a pull or a change of its CPU set moves it.
    Spin,

    // Burns, then sleeps: every wake goes through the policy's placement again.
    Burst,

    // Allocates short-lived objects, so that collections follow one another.
    Allocate,
}
