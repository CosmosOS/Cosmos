// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// Everything <see cref="KeyboardDriver"/> holds for one bound device, hung
/// off <see cref="DeviceBinding.DriverState"/>: the kit resources it
/// acquired, the counters its handler, work items and thread advance, and
/// the keyboard contract it publishes. The test reads it back through the
/// node's binding to assert what ran, where, and in what order. Written
/// over the public seam only: where the work item ran is not asked of the
/// engine but proven by counting, see <see cref="KeyWorkRunsAtHandlerExit"/>.
/// </summary>
public sealed class KeyboardState : IKeyboard
{
    /// <summary>Bytes of the register window the node carries.</summary>
    public const int WindowBytes = 64;

    /// <summary>Offset of the first byte of the pattern the probe writes into the window.</summary>
    public const int PatternOffset = 0;

    /// <summary>Length of the pattern the probe writes.</summary>
    public const int PatternLength = 16;

    /// <summary>Value of the pattern's first byte; each following byte is one more.</summary>
    public const byte PatternFirstByte = 0xA0;

    /// <summary>Offset of the scan code register the handler reads.</summary>
    public const int ScanCodeOffset = 0x10;

    /// <summary>Offset of the flags register the handler reads; bit 0 is a release.</summary>
    public const int FlagsOffset = 0x11;

    /// <summary>Bytes of the DMA buffer the probe allocates.</summary>
    public const int DmaBytes = 256;

    /// <summary>Alignment of the DMA buffer.</summary>
    public const int DmaAlignment = 4096;

    /// <summary>Value of the DMA pattern's first byte; each following byte is one more.</summary>
    public const byte DmaFirstByte = 0x5A;

    /// <summary>Interval of the periodic work item.</summary>
    public const uint PeriodicIntervalMilliseconds = 20;

    /// <summary>How long one pass of the driver thread waits on its event before looking at the detach flag again.</summary>
    public const uint ThreadWaitSliceMilliseconds = 50;

    private const byte ReleasedFlag = 1;

    private readonly DeviceBinding _binding;
    private volatile int _interruptCount;
    private volatile int _keyWorkRuns;
    private volatile int _periodicRuns;
    private volatile int _eventWakes;
    private volatile int _keyWorkRunsAtHandlerExit;
    private volatile bool _threadStarted;
    private volatile bool _threadExited;
    private volatile bool _threadExitedBeforeDetach;
    private KeyboardLeds _leds;

    /// <summary>
    /// Takes the memory the driver already acquired and creates the two work
    /// items, whose callbacks count on this instance, through the binding.
    /// </summary>
    internal KeyboardState(DeviceBinding binding, RegisterWindow window, DmaBuffer dma, DeviceEvent keyEvent, bool dmaWasZeroed)
    {
        _binding = binding;
        Window = window;
        Dma = dma;
        KeyEvent = keyEvent;
        DmaWasZeroed = dmaWasZeroed;
        KeyWork = binding.CreateWorkItem(OnKeyWork);
        PeriodicWork = binding.CreateWorkItem(OnPeriodic);
    }

    /// <inheritdoc/>
    public string Name => "synthetic-kbd";

    /// <summary>The register window over the node's RAM page.</summary>
    public RegisterWindow Window { get; }

    /// <summary>The DMA buffer the probe filled with a pattern.</summary>
    public DmaBuffer Dma { get; }

    /// <summary>True when every byte of <see cref="Dma"/> read zero right after allocation.</summary>
    public bool DmaWasZeroed { get; }

    /// <summary>The event the handler signals and the driver thread waits on.</summary>
    public DeviceEvent KeyEvent { get; }

    /// <summary>The work item the handler schedules.</summary>
    public WorkItem KeyWork { get; }

    /// <summary>The work item the platform timer schedules every <see cref="PeriodicIntervalMilliseconds"/>.</summary>
    public WorkItem PeriodicWork { get; }

    /// <summary>The sink the handler reports keys to; set once the keyboard is published.</summary>
    public KeyboardSink? Sink { get; internal set; }

    /// <summary>The connected interrupt; set once the handler is requested.</summary>
    public InterruptHandle? Handle { get; internal set; }

    /// <summary>The driver thread; set once it started.</summary>
    public DriverThread? Thread { get; internal set; }

    /// <summary>True when the periodic work was accepted by the kit.</summary>
    public bool PeriodicScheduled { get; internal set; }

    /// <summary>How many kit resources the probe holds, for the accounting assertion.</summary>
    public int ExpectedHeldResourceCount { get; internal set; }

    /// <summary>The indicators the ring last asked for.</summary>
    public KeyboardLeds Leds => _leds;

    /// <summary>How many times the handler ran.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>How many times the work item the handler schedules ran.</summary>
    public int KeyWorkRuns => _keyWorkRuns;

    /// <summary>
    /// What <see cref="KeyWorkRuns"/> read as the handler returned, after it
    /// had scheduled <see cref="KeyWork"/>. The synthetic dispatch runs the
    /// handler with interrupts disabled, so the item can only have run by
    /// then if the kit ran it inline, inside the dispatch; a value equal to
    /// the count before the raise proves it was deferred, and the run the
    /// test sees after <c>WaitForQueuedJobs</c> is the worker's.
    /// </summary>
    public int KeyWorkRunsAtHandlerExit => _keyWorkRunsAtHandlerExit;

    /// <summary>How many times the periodic work item ran.</summary>
    public int PeriodicRuns => _periodicRuns;

    /// <summary>How many waits of the driver thread returned with a signal.</summary>
    public int EventWakes => _eventWakes;

    /// <summary>True once the driver thread's body began.</summary>
    public bool ThreadStarted => _threadStarted;

    /// <summary>True once the driver thread's body returned.</summary>
    public bool ThreadExited => _threadExited;

    /// <summary>What <see cref="ThreadExited"/> read when the kit called the driver's detach hook: true proves the join preceded it.</summary>
    public bool ThreadExitedBeforeDetach => _threadExitedBeforeDetach;

    /// <inheritdoc/>
    public void SetLeds(KeyboardLeds leds) => _leds = leds;

    /// <summary>
    /// The interrupt handler: reads the scan code and flags registers through
    /// the window, reports the key, wakes the thread, hands the rest to the
    /// work item and notes how often that item had run by then. Allocation-free.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    public void OnInterrupt(InterruptContext context)
    {
        byte scanCode = Window.Read8(ScanCodeOffset);
        bool released = (Window.Read8(FlagsOffset) & ReleasedFlag) != 0;
        _interruptCount++;
        Sink?.Report(scanCode, released);
        context.Signal(KeyEvent);
        context.Schedule(KeyWork);
        _keyWorkRunsAtHandlerExit = _keyWorkRuns;
    }

    /// <summary>The work item the handler schedules: counts its runs.</summary>
    public void OnKeyWork() => _keyWorkRuns++;

    /// <summary>The periodic work item: counts its runs.</summary>
    public void OnPeriodic() => _periodicRuns++;

    /// <summary>The driver thread's body: waits on the key event until the binding is being torn down.</summary>
    public void ThreadMain()
    {
        _threadStarted = true;
        while (!_binding.IsDetaching)
        {
            if (_binding.Wait(KeyEvent, ThreadWaitSliceMilliseconds))
            {
                _eventWakes++;
            }
        }

        _threadExited = true;
    }

    /// <summary>Called from the driver's detach hook: keeps what the thread's exit flag said at that moment.</summary>
    internal void RecordDetach() => _threadExitedBeforeDetach = _threadExited;
}
