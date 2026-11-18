// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using Cosmos.Kernel.System.Diagnostics;

namespace DevKernel.Diagnostics.Smp;

// Reads the scheduler once per frame and keeps what the monitor draws: the threads and
// the CPU each is on, each CPU's load, and for each CPU a timeline of who ran there.
//
// The timeline is made from the scheduler's own accounting, not from looking at which
// thread is on a CPU when the frame is drawn: every thread's run time is read at each
// frame, and what it gained since the last one is credited to the CPU it is on. A column
// of the timeline is those gains summed over FramesPerColumn frames, one share per
// thread. A share is a fraction of the time the scheduler accounted for on that CPU, the
// idle thread's included: it charges whole ticks, so measured against the wall clock a
// CPU that never idles would read a little under or over its full time at every column.
internal sealed class SmpSampler
{
    // Bands in one column of a CPU's timeline: a thread each, the last one for every
    // thread past them together.
    public const int MaxShares = 10;

    public const int FramesPerColumn = 2;

    public const int FullPermille = 1000;
    public const int PercentMax = 100;

    // Frames a CPU's load is averaged over: the accounting moves a tick at a time, and a
    // frame holds only a few.
    private const int LoadWindowFrames = 6;

    private const int MovedHighlightFrames = 14;

    private const double NsPerSecond = 1_000_000_000.0;

    // The least a CPU's whole is taken to be, in percent of the clock. The tenth left is
    // the tick the accounting may lag by: one out of the ten of a column at the default
    // tick, with room to spare over the longer window of the load.
    private const int ClockFloorPercent = 90;

    // Sort keys that keep the threads in one order from frame to frame.
    private const int MonitorKey = 0;
    private const int WorkerKeyBase = 10;
    private const int PoolKeyBase = 100;
    private const int DirectorKey = 200;
    private const int ManagedKeyBase = 1000;
    private const int KernelKeyBase = 2000;

    private readonly SmpThread[] _threads;
    private readonly int[] _order;
    private readonly int _cpuCount;
    private readonly int _monitorThreadId;

    private readonly ulong[] _windowBusyNs;
    private readonly ulong[] _windowIdleNs;
    private readonly ulong[] _windowElapsedNs;
    private int _windowNext;

    private readonly int[] _loadPercent;
    private readonly int[] _runQueue;

    private readonly int[] _shareSlot;
    private readonly ulong[] _shareNs;
    private readonly int[] _shareCount;
    private readonly ulong[] _otherNs;
    private readonly ulong[] _accountedNs;
    private ulong _columnElapsedNs;
    private int _columnFrames;

    private readonly int _columns;
    private readonly int[] _columnColor;
    private readonly ushort[] _columnPermille;
    private readonly byte[] _columnShares;
    private readonly bool[] _columnCollected;
    private int _columnNext;
    private int _collectionsAtColumn;

    private long _lastTimestamp;

    // Threads of the registry, idle threads left out.
    public int ThreadCount { get; private set; }

    // Times a thread was found on another CPU than at the frame before.
    public int Moves { get; private set; }

    // Load of the whole machine, in percent of all its CPUs.
    public int TotalLoadPercent { get; private set; }

    public int Collections { get; private set; }

    // Columns the timelines hold so far, up to their capacity.
    public int ColumnCount { get; private set; }

    // Number of entries of Order that are in use.
    public int OrderCount { get; private set; }

    public SmpThread[] Threads => _threads;

    // The registry slots of the live threads that are not idle threads, in drawing order.
    public int[] Order => _order;

    public SmpSampler(int cpuCount, int columns, int monitorThreadId)
    {
        _cpuCount = cpuCount;
        _columns = columns < 1 ? 1 : columns;
        _monitorThreadId = monitorThreadId;

        int slots = SchedulerDiagnostics.ThreadSlotCount;
        _threads = new SmpThread[slots];
        _order = new int[slots];

        _windowBusyNs = new ulong[cpuCount * LoadWindowFrames];
        _windowIdleNs = new ulong[cpuCount * LoadWindowFrames];
        _windowElapsedNs = new ulong[LoadWindowFrames];
        _loadPercent = new int[cpuCount];
        _runQueue = new int[cpuCount];

        _shareSlot = new int[cpuCount * MaxShares];
        _shareNs = new ulong[cpuCount * MaxShares];
        _shareCount = new int[cpuCount];
        _otherNs = new ulong[cpuCount];
        _accountedNs = new ulong[cpuCount];

        _columnColor = new int[cpuCount * _columns * MaxShares];
        _columnPermille = new ushort[cpuCount * _columns * MaxShares];
        _columnShares = new byte[cpuCount * _columns];
        _columnCollected = new bool[_columns];

        _lastTimestamp = Stopwatch.GetTimestamp();
        _collectionsAtColumn = MemoryDiagnostics.TotalCollections;
        Collections = _collectionsAtColumn;
    }

    public int LoadPercent(int cpu)
    {
        return _loadPercent[cpu];
    }

    public int RunQueue(int cpu)
    {
        return _runQueue[cpu];
    }

    // The ring index of the column drawn age columns before the newest (0 is the newest).
    public int ColumnAt(int age)
    {
        return (_columnNext - 1 - age + _columns * 2) % _columns;
    }

    public int ShareCount(int cpu, int column)
    {
        return _columnShares[cpu * _columns + column];
    }

    public int ShareColor(int cpu, int column, int share)
    {
        return _columnColor[(cpu * _columns + column) * MaxShares + share];
    }

    public int SharePermille(int cpu, int column, int share)
    {
        return _columnPermille[(cpu * _columns + column) * MaxShares + share];
    }

    // Whether the collector ran while this column was filling.
    public bool Collected(int column)
    {
        return _columnCollected[column];
    }

    public void Sample()
    {
        long now = Stopwatch.GetTimestamp();
        ulong elapsedNs = (ulong)((now - _lastTimestamp) * NsPerSecond / Stopwatch.Frequency);
        _lastTimestamp = now;

        int window = _windowNext;
        _windowNext = (_windowNext + 1) % LoadWindowFrames;
        _windowElapsedNs[window] = elapsedNs;
        for (int cpu = 0; cpu < _cpuCount; cpu++)
        {
            _windowBusyNs[cpu * LoadWindowFrames + window] = 0;
            _windowIdleNs[cpu * LoadWindowFrames + window] = 0;
        }

        ReadThreads(window);
        ReadCpus();
        SortOrder();

        Collections = MemoryDiagnostics.TotalCollections;

        _columnElapsedNs += elapsedNs;
        _columnFrames++;
        if (_columnFrames >= FramesPerColumn)
        {
            CloseColumn();
        }
    }

    private void ReadThreads(int window)
    {
        int threadCount = 0;
        int orderCount = 0;
        int slots = Math.Min(SchedulerDiagnostics.ThreadSlotCount, _threads.Length);

        for (int slot = 0; slot < slots; slot++)
        {
            ref SmpThread thread = ref _threads[slot];

            if (!SchedulerDiagnostics.TryGetThreadInSlot(slot, out KernelThreadInfo info))
            {
                thread.Live = false;
                continue;
            }

            // A slot is taken again once its thread has left: another id is another thread.
            if (!thread.Live || thread.Id != info.Id)
            {
                thread = default;
                thread.Live = true;
                thread.Id = info.Id;
                thread.Cpu = info.CpuId;
                thread.RuntimeNs = info.TotalRuntimeNs;
            }

            ulong gainedNs = info.TotalRuntimeNs > thread.RuntimeNs ? info.TotalRuntimeNs - thread.RuntimeNs : 0;
            thread.RuntimeNs = info.TotalRuntimeNs;
            thread.State = info.State;

            if (info.IsIdle)
            {
                thread.Cpu = info.CpuId;
                if (info.CpuId < _cpuCount)
                {
                    _windowIdleNs[(int)info.CpuId * LoadWindowFrames + window] += gainedNs;
                    _accountedNs[info.CpuId] += gainedNs;
                }

                continue;
            }

            Classify(ref thread, info);

            if (info.CpuId != thread.Cpu)
            {
                Moves++;
                thread.MovedFrames = MovedHighlightFrames;
                thread.Cpu = info.CpuId;
            }
            else if (thread.MovedFrames > 0)
            {
                thread.MovedFrames--;
            }

            threadCount++;
            _order[orderCount++] = slot;

            if (gainedNs > 0 && info.CpuId < _cpuCount)
            {
                int cpu = (int)info.CpuId;
                _windowBusyNs[cpu * LoadWindowFrames + window] += gainedNs;
                _accountedNs[cpu] += gainedNs;
                Credit(cpu, slot, gainedNs);
            }
        }

        ThreadCount = threadCount;
        OrderCount = orderCount;
    }

    private void Classify(ref SmpThread thread, KernelThreadInfo info)
    {
        int id = (int)info.Id;
        thread.Ordinal = 0;

        if (id == _monitorThreadId)
        {
            thread.Role = SmpThreadRole.Monitor;
            return;
        }

        int worker = SmpScenario.WorkerIndexOf(id);
        if (worker >= 0)
        {
            thread.Role = SmpThreadRole.Worker;
            thread.Ordinal = worker;
            return;
        }

        int pool = SmpScenario.PoolOrdinalOf(id);
        if (pool >= 0)
        {
            thread.Role = SmpThreadRole.Pool;
            thread.Ordinal = pool;
            return;
        }

        if (id == SmpScenario.DirectorThreadId)
        {
            thread.Role = SmpThreadRole.Director;
            return;
        }

        thread.Role = info.IsManaged ? SmpThreadRole.Managed : SmpThreadRole.Kernel;
    }

    private void ReadCpus()
    {
        ulong elapsedNs = 0;
        for (int i = 0; i < LoadWindowFrames; i++)
        {
            elapsedNs += _windowElapsedNs[i];
        }

        int total = 0;
        for (int cpu = 0; cpu < _cpuCount; cpu++)
        {
            ulong busyNs = 0;
            ulong idleNs = 0;
            for (int i = 0; i < LoadWindowFrames; i++)
            {
                busyNs += _windowBusyNs[cpu * LoadWindowFrames + i];
                idleNs += _windowIdleNs[cpu * LoadWindowFrames + i];
            }

            ulong wholeNs = WholeOf(busyNs + idleNs, elapsedNs);
            int percent = wholeNs == 0 ? 0 : (int)(busyNs * PercentMax / wholeNs);
            _loadPercent[cpu] = percent > PercentMax ? PercentMax : percent;
            total += _loadPercent[cpu];

            _runQueue[cpu] = SchedulerDiagnostics.GetRunQueueCount((uint)cpu);
        }

        TotalLoadPercent = _cpuCount == 0 ? 0 : total / _cpuCount;
    }

    private int KeyOf(int slot)
    {
        ref SmpThread thread = ref _threads[slot];
        return thread.Role switch
        {
            SmpThreadRole.Monitor => MonitorKey,
            SmpThreadRole.Worker => WorkerKeyBase + thread.Ordinal,
            SmpThreadRole.Pool => PoolKeyBase + thread.Ordinal,
            SmpThreadRole.Director => DirectorKey,
            SmpThreadRole.Managed => ManagedKeyBase + slot,
            _ => KernelKeyBase + slot,
        };
    }

    // Insertion sort: a few dozen threads, nearly in order already.
    private void SortOrder()
    {
        for (int i = 1; i < OrderCount; i++)
        {
            int slot = _order[i];
            int key = KeyOf(slot);
            int j = i - 1;

            while (j >= 0 && KeyOf(_order[j]) > key)
            {
                _order[j + 1] = _order[j];
                j--;
            }

            _order[j + 1] = slot;
        }
    }

    // Adds run time to a thread's share of the column a CPU is filling.
    private void Credit(int cpu, int slot, ulong gainedNs)
    {
        int first = cpu * MaxShares;
        int count = _shareCount[cpu];

        for (int i = 0; i < count; i++)
        {
            if (_shareSlot[first + i] == slot)
            {
                _shareNs[first + i] += gainedNs;
                return;
            }
        }

        if (count < MaxShares - 1)
        {
            _shareSlot[first + count] = slot;
            _shareNs[first + count] = gainedNs;
            _shareCount[cpu] = count + 1;
            return;
        }

        _otherNs[cpu] += gainedNs;
    }

    // Turns what each CPU accumulated into one column of its timeline and starts the next.
    private void CloseColumn()
    {
        int column = _columnNext;
        _columnNext = (_columnNext + 1) % _columns;
        if (ColumnCount < _columns)
        {
            ColumnCount++;
        }

        _columnCollected[column] = Collections != _collectionsAtColumn;
        _collectionsAtColumn = Collections;

        for (int cpu = 0; cpu < _cpuCount; cpu++)
        {
            int first = cpu * MaxShares;
            int count = _shareCount[cpu];

            SortShares(first, count);

            int target = (cpu * _columns + column) * MaxShares;
            int written = 0;
            int left = FullPermille;
            ulong wholeNs = WholeOf(_accountedNs[cpu], _columnElapsedNs);

            for (int i = 0; i < count && left > 0; i++)
            {
                ref SmpThread thread = ref _threads[_shareSlot[first + i]];
                int permille = PermilleOf(_shareNs[first + i], wholeNs, left);
                if (permille == 0)
                {
                    continue;
                }

                _columnColor[target + written] = SmpPalette.Of(thread.Role, thread.Ordinal).ToArgb();
                _columnPermille[target + written] = (ushort)permille;
                written++;
                left -= permille;
            }

            int other = PermilleOf(_otherNs[cpu], wholeNs, left);
            if (other > 0)
            {
                _columnColor[target + written] = SmpPalette.Kernel.ToArgb();
                _columnPermille[target + written] = (ushort)other;
                written++;
            }

            _columnShares[cpu * _columns + column] = (byte)written;
            _shareCount[cpu] = 0;
            _otherNs[cpu] = 0;
            _accountedNs[cpu] = 0;
        }

        _columnElapsedNs = 0;
        _columnFrames = 0;
    }

    // The time a CPU's shares are fractions of: what the scheduler accounted for on it,
    // its idle thread included. Whole ticks are charged, so that is within a tick of the
    // clock, over or under. Further under, the CPU was held with its tick masked, as a
    // collection holds every CPU: the shares are then fractions of the clock's floor, and
    // the time nobody was charged for shows as a gap above them.
    private static ulong WholeOf(ulong accountedNs, ulong elapsedNs)
    {
        return Math.Max(accountedNs, elapsedNs * ClockFloorPercent / PercentMax);
    }

    private static int PermilleOf(ulong ns, ulong wholeNs, int left)
    {
        if (wholeNs == 0)
        {
            return 0;
        }

        ulong permille = ns * FullPermille / wholeNs;
        return permille > (ulong)left ? left : (int)permille;
    }

    // Orders a CPU's shares as the threads are ordered, so a thread's band keeps its
    // place in the stack from one column to the next.
    private void SortShares(int first, int count)
    {
        for (int i = 1; i < count; i++)
        {
            int slot = _shareSlot[first + i];
            ulong ns = _shareNs[first + i];
            int key = KeyOf(slot);
            int j = i - 1;

            while (j >= 0 && KeyOf(_shareSlot[first + j]) > key)
            {
                _shareSlot[first + j + 1] = _shareSlot[first + j];
                _shareNs[first + j + 1] = _shareNs[first + j];
                j--;
            }

            _shareSlot[first + j + 1] = slot;
            _shareNs[first + j + 1] = ns;
        }
    }
}
