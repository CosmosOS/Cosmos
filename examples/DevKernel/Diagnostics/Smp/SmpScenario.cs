// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cosmos.Kernel.System.Threading;
using SysThread = System.Threading.Thread;

namespace DevKernel.Diagnostics.Smp;

// The script the SMP monitor plays, one step after another and round after round, on a
// thread of its own: the monitor only draws what the scheduler does with it.
//
//   0  the CPUs         nothing runs yet
//   1  placement        a first wave of spinners starts, one after another
//   2  time sharing     a second wave: twice the threads on the same CPUs
//   3  affinity         every spinner pinned to one CPU
//   4  load balancing   the pin is released: the CPUs out of work pull
//   5  wake placement   the spinners sleep between bursts, and are placed at each wake
//   6  migration        one running thread is sent from CPU to CPU
//   7  thread pool      work items run on the pool's workers
//   8  collection       every thread allocates, the collector stops the CPUs
//
// The thread that plays the script takes, as its first act, the CPU set the kernel gives
// a new thread (KernelCpus.DefaultAffinity), whichever shell started the monitor. A thread
// takes its creator's set, so the workers start out where any thread of the kernel would.
internal static class SmpScenario
{
    public const int MaxWorkers = MaxWave * Waves;
    public const int MaxPoolThreads = 8;
    public const int StepCount = 9;

    private const int UnknownThreadId = SmpWorker.UnknownThreadId;

    // Threads started by one wave: one per CPU of the worker set, up to this many.
    private const int MaxWave = 4;

    // The first wave gives each CPU a thread, the second makes them share it.
    private const int Waves = 2;

    // The script sleeps in slices this long, so a stop or a skip is seen quickly.
    private const int HoldSliceMs = 20;

    private const int IntroMs = 3000;
    private const int StaggerMs = 450;
    private const int WaveHoldMs = 2800;
    private const int PinnedMs = 4000;
    private const int PullMs = 4000;
    private const int WakeMs = 5500;
    private const int HopMs = 650;
    private const int Hops = 7;
    private const int PoolItemsPerCpu = 16;
    private const int PoolBurnMs = 250;
    private const int PoolBudgetMs = 6000;
    private const int PoolTailMs = 800;
    private const int CollectStaggerMs = 150;
    private const int CollectMs = 4200;
    private const int RetireBudgetMs = 1500;

    private static readonly SmpWorker?[] s_workers = new SmpWorker?[MaxWorkers];
    private static readonly int[] s_poolThreadIds = new int[MaxPoolThreads];
    private static readonly string[] s_titles = new string[StepCount];
    private static readonly string[] s_headlines = new string[StepCount];
    private static readonly string[] s_details = new string[StepCount];

    private static uint[] s_workerCpus = [];
    private static uint[] s_onlineCpus = [];
    private static CpuSet s_workerSet;

    // Threads of one wave, and of the waves together.
    private static int s_wave;
    private static int s_crowd;

    // The CPU the affinity step pins every worker to.
    private static uint s_pinCpu;

    private static int s_directorThreadId = UnknownThreadId;
    private static int s_directorRunning;
    private static int s_stop;
    private static int s_skip;
    private static int s_step;
    private static int s_poolOpen;
    private static int s_poolDone;

    // The step the script is playing.
    public static int Step => Volatile.Read(ref s_step);

    public static int DirectorThreadId => Volatile.Read(ref s_directorThreadId);

    // Whether the script's thread is still there. It outlives a Stop that ran out of time.
    public static bool IsRunning => Volatile.Read(ref s_directorRunning) != 0;

    private static bool Stopping => Volatile.Read(ref s_stop) != 0;

    private static bool Interrupted => Stopping || Volatile.Read(ref s_skip) != 0;

    public static string Title(int step)
    {
        return s_titles[step];
    }

    public static string Headline(int step)
    {
        return s_headlines[step];
    }

    public static string Detail(int step)
    {
        return s_details[step];
    }

    // Starts the script. monitorPinned says whether the monitor moved to the housekeeping
    // CPU, which the first step tells. Not to be called while IsRunning: two scripts would
    // share the workers.
    public static void Start(bool monitorPinned)
    {
        ReadCpus();
        WriteTexts(monitorPinned);
        ForgetPoolThreads();

        Volatile.Write(ref s_stop, 0);
        Volatile.Write(ref s_skip, 0);
        Volatile.Write(ref s_step, 0);
        Volatile.Write(ref s_poolOpen, 0);
        Volatile.Write(ref s_directorThreadId, UnknownThreadId);

        // Raised before the thread exists, so that a Stop right after waits for it, and
        // lowered again when the thread could not start.
        Volatile.Write(ref s_directorRunning, 1);
        bool started = false;
        try
        {
            SysThread director = new(Direct);
            director.Start();
            started = true;
        }
        finally
        {
            if (!started)
            {
                Volatile.Write(ref s_directorRunning, 0);
            }
        }
    }

    // Ends the script and waits for its thread and its workers to leave, up to budgetMs.
    // IsRunning tells whether they did.
    public static void Stop(int budgetMs)
    {
        Volatile.Write(ref s_stop, 1);

        long deadline = SmpClock.DeadlineIn(budgetMs);
        while (IsRunning && !SmpClock.HasPassed(deadline))
        {
            SysThread.Sleep(HoldSliceMs);
        }
    }

    // Ends the current step early.
    public static void Skip()
    {
        Volatile.Write(ref s_skip, 1);
    }

    // The index of the worker whose thread has this scheduler id, or -1.
    public static int WorkerIndexOf(int threadId)
    {
        for (int i = 0; i < MaxWorkers; i++)
        {
            SmpWorker? worker = Volatile.Read(ref s_workers[i]);
            if (worker is not null && worker.ThreadId == threadId)
            {
                return i;
            }
        }

        return -1;
    }

    // The rank of the pool thread with this scheduler id among those seen running a work
    // item of the script, or -1.
    public static int PoolOrdinalOf(int threadId)
    {
        for (int i = 0; i < MaxPoolThreads; i++)
        {
            if (Volatile.Read(ref s_poolThreadIds[i]) == threadId)
            {
                return i;
            }
        }

        return -1;
    }

    private static void ReadCpus()
    {
        int count = KernelCpus.Count;
        s_workerSet = KernelCpus.DefaultAffinity;
        s_onlineCpus = CpusOf(KernelCpus.Online, count);
        s_workerCpus = CpusOf(s_workerSet, count);

        // The default set names at least one CPU the scheduler manages: the kernel refuses
        // any other.
        s_wave = Math.Clamp(s_workerCpus.Length, 1, MaxWave);
        s_crowd = s_wave * Waves;
        s_pinCpu = s_workerCpus.Length > 0 ? s_workerCpus[0] : KernelCpus.Housekeeping.First;
    }

    private static uint[] CpusOf(CpuSet set, int count)
    {
        int named = 0;
        for (uint cpu = 0; cpu < count; cpu++)
        {
            if (set.Contains(cpu))
            {
                named++;
            }
        }

        uint[] cpus = new uint[named];
        int next = 0;
        for (uint cpu = 0; cpu < count; cpu++)
        {
            if (set.Contains(cpu))
            {
                cpus[next++] = cpu;
            }
        }

        return cpus;
    }

    private static void WriteTexts(bool monitorPinned)
    {
        int count = KernelCpus.Count;
        int workerCpus = s_workerCpus.Length;
        string workers = Describe(s_workerCpus);

        // With one worker CPU nothing can move from a CPU to another: the steps that show
        // a move say so instead.
        bool spread = workerCpus > 1;

        s_titles[0] = "THE CPUS";
        s_headlines[0] = count > 1
            ? $"{count} CPUs: CPU {KernelCpus.Housekeeping.First} keeps interrupts and kernel threads, {workers} {(spread ? "take" : "takes")} the work"
            : "One CPU. Start QEMU with -smp 4 to see threads spread over several";
        s_details[0] = monitorPinned
            ? "KernelCpus.Housekeeping and KernelCpus.Compute. This monitor pinned itself to the housekeeping CPU (ThreadAffinity.Current)."
            : "KernelCpus.Housekeeping and KernelCpus.Compute. This monitor shares its CPU with the threads it shows.";

        s_titles[1] = "PLACEMENT";
        s_headlines[1] = s_wave < workerCpus
            ? $"{Count(s_wave, "busy thread")} started: the scheduler gives each a compute CPU of its own"
            : $"{Count(s_wave, "busy thread")} started: the scheduler puts one on each compute CPU";
        s_details[1] = "SelectCpu at creation: a CPU of the thread's set that carries clearly less load. One color is one thread, one lane is one CPU.";

        s_titles[2] = "TIME SHARING";
        s_headlines[2] = $"{s_wave} more started: {Count(s_crowd, "thread")} on {Count(workerCpus, "CPU")}, {SharingOn(workerCpus)}";
        s_details[2] = "Stride scheduling on every CPU: the bands of a timeline are the shares of the threads that ran there.";

        s_titles[3] = "AFFINITY";
        s_headlines[3] = spread
            ? $"All {s_crowd} pinned to CPU {s_pinCpu}: they move there and share that one CPU"
            : $"All {s_crowd} pinned to CPU {s_pinCpu}, the one CPU they had: nothing moves";
        s_details[3] = "ThreadAffinity.Pin: a queued thread moves at once, a running one is evicted at its next switch. The CPUs they leave go idle.";

        s_titles[4] = "LOAD BALANCING";
        s_headlines[4] = spread
            ? "Pin released: each CPU with nothing to run pulls a waiting thread"
            : "Pin released: no other compute CPU is there to pull a waiting thread";
        s_details[4] = "Balance: a CPU out of work pulls one thread from the longest run queue. A CPU that runs a thread pulls nothing, so the rest stays put.";

        s_titles[5] = "WAKE PLACEMENT";
        s_headlines[5] = spread
            ? "Threads that sleep are placed again at every wake: the load evens out"
            : "Threads that sleep are placed again at every wake, here on their one CPU";
        s_details[5] = "SelectCpu at every wake: a thread leaves for a CPU under 80% of its own CPU's load. Each CPU's tick wakes its own sleepers.";

        s_titles[6] = "MIGRATION";
        s_headlines[6] = count > 1
            ? $"One running thread, sent to another CPU every {HopMs} ms"
            : "One running thread: with one CPU there is nowhere to send it";
        s_details[6] = "The set change evicts it from its CPU; it resumes on the next one with its stack, its registers and its thread statics.";

        s_titles[7] = "THREAD POOL";
        s_headlines[7] = $"{PoolItemsPerCpu * workerCpus} work items queued: the pool's workers run them on the compute CPUs";
        s_details[7] = "ThreadPool.QueueUserWorkItem. Environment.ProcessorCount counts the CPUs new threads may use, and the pool sizes itself on it.";

        s_titles[8] = "GARBAGE COLLECTION";
        s_headlines[8] = "Every thread allocates: each collection stops all the CPUs at once";
        s_details[8] = "The collector stops every CPU, scans every thread's stack and lets them go on. A white mark over the timelines is a collection.";
    }

    // "1 thread", "6 threads".
    public static string Count(int count, string noun)
    {
        return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
    }

    // What the second wave does to the CPUs, which depends on how many there are.
    private static string SharingOn(int workerCpus)
    {
        if (s_wave == workerCpus)
        {
            return "each CPU shares its time";
        }

        return s_crowd > workerCpus
            ? "so some CPUs share their time"
            : "a CPU each, and none has to share";
    }

    // "CPU 1", "CPUs 1-3" for a run of consecutive CPUs, "CPUs 1,3,5" otherwise.
    private static string Describe(uint[] cpus)
    {
        if (cpus.Length == 0)
        {
            return "no CPU";
        }

        if (cpus.Length == 1)
        {
            return $"CPU {cpus[0]}";
        }

        bool consecutive = true;
        for (int i = 1; i < cpus.Length; i++)
        {
            if (cpus[i] != cpus[i - 1] + 1)
            {
                consecutive = false;
                break;
            }
        }

        if (consecutive)
        {
            return $"CPUs {cpus[0]}-{cpus[cpus.Length - 1]}";
        }

        string list = cpus[0].ToString();
        for (int i = 1; i < cpus.Length; i++)
        {
            list += $",{cpus[i]}";
        }

        return $"CPUs {list}";
    }

    private static void Direct()
    {
        try
        {
            // Whichever shell started the monitor, the script plays where the kernel puts
            // a new thread, and the threads it starts take that set from it.
            ThreadAffinity.Current = s_workerSet;
            Volatile.Write(ref s_directorThreadId, SmpWorker.ReadCurrentThreadId());

            // A round ends with its workers still there: the first step retires them.
            while (!Stopping)
            {
                for (int step = 0; step < StepCount && !Stopping; step++)
                {
                    Volatile.Write(ref s_skip, 0);
                    Volatile.Write(ref s_step, step);
                    Play(step);
                }
            }
        }
        finally
        {
            RetireWorkers(0);
            Volatile.Write(ref s_directorRunning, 0);
        }
    }

    private static void Play(int step)
    {
        switch (step)
        {
            case 0:
                RetireWorkers(0);
                Hold(IntroMs);
                break;
            case 1:
                RetireWorkers(0);
                StartWave(0, s_wave, SmpWorkerMode.Spin, StaggerMs);
                Hold(WaveHoldMs);
                break;
            case 2:
                EnsureWorkers(s_wave, SmpWorkerMode.Spin);
                StartWave(s_wave, s_wave, SmpWorkerMode.Spin, StaggerMs);
                Hold(WaveHoldMs);
                break;
            case 3:
                EnsureWorkers(s_crowd, SmpWorkerMode.Spin);
                SetModes(SmpWorkerMode.Spin);
                PinWorkers(s_pinCpu);
                Hold(PinnedMs);
                break;
            case 4:
                EnsureWorkers(s_crowd, SmpWorkerMode.Spin);
                SetAffinities(s_workerSet);
                Hold(PullMs);
                break;
            case 5:
                EnsureWorkers(s_crowd, SmpWorkerMode.Spin);
                SetAffinities(s_workerSet);
                SetModes(SmpWorkerMode.Burst);
                Hold(WakeMs);
                break;
            case 6:
                PlayHops();
                break;
            case 7:
                PlayPool();
                break;
            default:
                RetireWorkers(0);
                StartWave(0, s_wave, SmpWorkerMode.Allocate, CollectStaggerMs);
                Hold(CollectMs);
                break;
        }
    }

    // One spinner alone, pinned to each online CPU in turn.
    private static void PlayHops()
    {
        EnsureWorkers(1, SmpWorkerMode.Spin);
        RetireWorkers(1);

        SmpWorker? runner = Volatile.Read(ref s_workers[0]);
        if (runner is null || runner.HasExited)
        {
            Hold(HopMs);
            return;
        }

        runner.Mode = SmpWorkerMode.Spin;

        for (int hop = 0; hop < Hops; hop++)
        {
            uint cpu = s_onlineCpus[(hop + 1) % s_onlineCpus.Length];
            ThreadAffinity.Pin(runner.Thread, cpu);

            if (!Hold(HopMs))
            {
                break;
            }
        }

        ThreadAffinity.Set(runner.Thread, s_workerSet);
    }

    private static void PlayPool()
    {
        RetireWorkers(0);

        // The pool lets its threads go when they have been idle for a while and starts
        // others at the next round, and an id is never used twice: the ranks are dealt
        // anew at each round.
        ForgetPoolThreads();

        int items = PoolItemsPerCpu * s_workerCpus.Length;
        int queued = 0;
        Volatile.Write(ref s_poolDone, 0);
        Volatile.Write(ref s_poolOpen, 1);

        for (int i = 0; i < items; i++)
        {
            if (ThreadPool.QueueUserWorkItem(PoolWork))
            {
                queued++;
            }
        }

        long deadline = SmpClock.DeadlineIn(PoolBudgetMs);
        while (Volatile.Read(ref s_poolDone) < queued && !Interrupted && !SmpClock.HasPassed(deadline))
        {
            SysThread.Sleep(HoldSliceMs);
        }

        // The items still queued when the step ends early go through without their burn,
        // so none is left running under the next step's banner.
        Volatile.Write(ref s_poolOpen, 0);

        Hold(PoolTailMs);
    }

    private static void PoolWork(object? state)
    {
        NotePoolThread(SmpWorker.ReadCurrentThreadId());

        if (Volatile.Read(ref s_poolOpen) != 0)
        {
            SmpWorker.Burn(PoolBurnMs);
        }

        Interlocked.Increment(ref s_poolDone);
    }

    private static void ForgetPoolThreads()
    {
        for (int i = 0; i < MaxPoolThreads; i++)
        {
            Volatile.Write(ref s_poolThreadIds[i], UnknownThreadId);
        }
    }

    // Records a pool thread's id in the first free slot, unless it is there already. Pool
    // threads call this at the same time, so a slot is claimed with a compare-and-swap.
    private static void NotePoolThread(int threadId)
    {
        if (threadId == UnknownThreadId)
        {
            return;
        }

        for (int i = 0; i < MaxPoolThreads; i++)
        {
            int seen = Volatile.Read(ref s_poolThreadIds[i]);
            if (seen == UnknownThreadId)
            {
                seen = Interlocked.CompareExchange(ref s_poolThreadIds[i], threadId, UnknownThreadId);
                if (seen == UnknownThreadId)
                {
                    return;
                }
            }

            if (seen == threadId)
            {
                return;
            }
        }
    }

    // Sleeps for a step's duration. False when the script is stopping or the step was skipped.
    private static bool Hold(int milliseconds)
    {
        long deadline = SmpClock.DeadlineIn(milliseconds);
        while (!Interrupted && !SmpClock.HasPassed(deadline))
        {
            SysThread.Sleep(HoldSliceMs);
        }

        return !Interrupted;
    }

    private static bool TryGetActive(int index, [NotNullWhen(true)] out SmpWorker? worker)
    {
        worker = Volatile.Read(ref s_workers[index]);
        return worker is not null && !worker.HasExited;
    }

    private static bool IsActive(int index)
    {
        return TryGetActive(index, out _);
    }

    private static void StartWave(int first, int count, SmpWorkerMode mode, int staggerMs)
    {
        for (int i = first; i < first + count && i < MaxWorkers; i++)
        {
            if (!IsActive(i))
            {
                StartWorker(i, mode);
            }

            if (!Hold(staggerMs))
            {
                return;
            }
        }
    }

    // Starts, with no pause, the workers below count that a skipped step left out.
    private static void EnsureWorkers(int count, SmpWorkerMode mode)
    {
        for (int i = 0; i < count && i < MaxWorkers; i++)
        {
            if (!IsActive(i))
            {
                StartWorker(i, mode);
            }
        }
    }

    private static void StartWorker(int index, SmpWorkerMode mode)
    {
        SmpWorker worker = new(mode);
        try
        {
            worker.Start();
        }
        catch (Exception)
        {
            // The thread did not start, for want of memory in all likelihood: the step
            // goes on with the workers it has.
            return;
        }

        Volatile.Write(ref s_workers[index], worker);
    }

    private static void SetModes(SmpWorkerMode mode)
    {
        for (int i = 0; i < MaxWorkers; i++)
        {
            if (TryGetActive(i, out SmpWorker? worker))
            {
                worker.Mode = mode;
            }
        }
    }

    // A worker that left between the look and the call is no thread of the scheduler's
    // any more: the call answers false, and there is nothing left to move.
    private static void SetAffinities(CpuSet set)
    {
        for (int i = 0; i < MaxWorkers; i++)
        {
            if (TryGetActive(i, out SmpWorker? worker))
            {
                ThreadAffinity.Set(worker.Thread, set);
            }
        }
    }

    private static void PinWorkers(uint cpu)
    {
        for (int i = 0; i < MaxWorkers; i++)
        {
            if (TryGetActive(i, out SmpWorker? worker))
            {
                ThreadAffinity.Pin(worker.Thread, cpu);
            }
        }
    }

    // Asks the workers from index first on to leave and waits for them. A worker that has
    // left keeps its slot until another takes it, so the monitor still names its thread
    // for the few frames the scheduler takes to drop it.
    private static void RetireWorkers(int first)
    {
        for (int i = first; i < MaxWorkers; i++)
        {
            Volatile.Read(ref s_workers[i])?.RequestExit();
        }

        bool gone = false;
        long deadline = SmpClock.DeadlineIn(RetireBudgetMs);
        while (!gone && !SmpClock.HasPassed(deadline))
        {
            gone = true;
            for (int i = first; i < MaxWorkers; i++)
            {
                if (IsActive(i))
                {
                    gone = false;
                }
            }

            if (!gone)
            {
                SysThread.Sleep(HoldSliceMs);
            }
        }
    }
}
