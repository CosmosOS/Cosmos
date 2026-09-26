// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver for QEMU's edu device (1234:11e8), written against the driver
/// kit's seam alone: it maps BAR 0, exercises the identification, liveness
/// and factorial registers, round-trips a buffer through the DMA engine,
/// and services the interrupt edu raises on request. edu has no MSI-X, so
/// the kit polls its handler from the timer. It also publishes a mouse,
/// which its handler reports through when a cell raises
/// <see cref="MouseRaise"/>, so a report travels from interrupt context to
/// the kernel's mouse manager. Probe runs during the driver
/// pass, before any test does, so it records what it saw and the edu cells
/// assert on that afterwards. It binds whatever it saw, so one wrong
/// register fails its own cell and not the ranking cells as well.
/// </summary>
internal sealed class EduDriver : PciDriver
{
    /// <summary>The registration's name, and the owner the edu function gets.</summary>
    public const string Name = "edu";

    /// <summary>QEMU's vendor ID.</summary>
    public const ushort VendorId = 0x1234;

    /// <summary>The edu device's ID.</summary>
    public const ushort DeviceId = 0x11E8;

    /// <summary>What the identification register reads: version 1.0 and the 0xed signature.</summary>
    public const uint ExpectedIdentification = 0x0100_00ED;

    /// <summary>Written to the liveness register, which reads back its bitwise complement.</summary>
    public const uint LivenessPattern = 0x1234_5678;

    /// <summary>Handed to the factorial unit.</summary>
    public const uint FactorialInput = 10;

    /// <summary>10!, which fits the unit's 32-bit result.</summary>
    public const uint ExpectedFactorial = 3_628_800;

    /// <summary>
    /// Highest address edu's DMA engine can reach: it masks every address to
    /// 28 bits, so a buffer has to end below 2^28.
    /// </summary>
    public const ulong DmaAddressLimit = (1UL << 28) - 1;

    /// <summary>The size of BAR 0.</summary>
    public const ulong BarLength = 1024 * 1024;

    // BAR 0 registers (QEMU docs/specs/edu.rst). Below 0x80 the device
    // answers 32-bit accesses only; the DMA registers also take 64-bit ones.
    private const int RegisterBar = 0;
    private const ulong IdentificationRegister = 0x00;
    private const ulong LivenessRegister = 0x04;
    private const ulong FactorialRegister = 0x08;
    private const ulong StatusRegister = 0x20;
    private const ulong InterruptStatusRegister = 0x24;
    private const ulong InterruptRaiseRegister = 0x60;
    private const ulong InterruptAcknowledgeRegister = 0x64;
    private const ulong DmaSourceRegister = 0x80;
    private const ulong DmaDestinationRegister = 0x88;
    private const ulong DmaCountRegister = 0x90;
    private const ulong DmaCommandRegister = 0x98;

    /// <summary>Status bit set while the factorial unit computes.</summary>
    private const uint StatusComputing = 0x01;

    /// <summary>DMA command bit that starts a transfer; the device clears it when done.</summary>
    private const ulong DmaCommandStart = 0x01;

    /// <summary>DMA command bit for the device-to-RAM direction; clear means RAM to device.</summary>
    private const ulong DmaCommandToRam = 0x02;

    /// <summary>Where edu's own 4 KiB DMA buffer sits in the addresses its DMA engine takes.</summary>
    private const ulong DeviceBufferAddress = 0x40000;

    /// <summary>
    /// One page. The allocator hands a one-page request the lowest free page,
    /// which is what makes a failure to allocate below the limit mean that no
    /// free page lies below it.
    /// </summary>
    private const int DmaBufferLength = 4096;

    /// <summary>Bytes sent to the device and back.</summary>
    private const int TransferLength = 256;

    /// <summary>Where in the buffer the device writes the bytes back, apart from where they came from.</summary>
    private const int ReadBackOffset = 2048;

    /// <summary>
    /// How many 1 ms waits a poll allows. The DMA engine starts a transfer
    /// 100 ms of QEMU virtual time after the command; two seconds leave room
    /// for a slow host.
    /// </summary>
    private const int PollLimit = 2000;

    /// <summary>Config offset of the Command register.</summary>
    private const ushort CommandOffset = 0x04;

    /// <summary>
    /// Raised through edu's interrupt raise register during Probe. The
    /// handler acknowledges whatever it finds in the interrupt status, so a
    /// handler that ran before Bound would have cleared it by Probe's end.
    /// </summary>
    public const uint ProbeRaise = 0x1;

    /// <summary>
    /// Raised by the mouse cell: the handler then reports one movement
    /// through the mouse Probe published, with the values below.
    /// </summary>
    public const uint MouseRaise = 0x8;

    /// <summary>The horizontal movement the handler reports for <see cref="MouseRaise"/>.</summary>
    public const int MouseDeltaX = 5;

    /// <summary>The vertical movement the handler reports for <see cref="MouseRaise"/>.</summary>
    public const int MouseDeltaY = 7;

    /// <summary>The wheel movement the handler reports for <see cref="MouseRaise"/>: one notch up.</summary>
    public const int MouseWheel = -1;

    /// <summary>
    /// How long Probe keeps the raise pending before checking nobody took it:
    /// more than two polling periods on x64, where the timer ticks about
    /// every 55 ms, and many on ARM64.
    /// </summary>
    private const int ProbeRaiseHoldMilliseconds = 150;

    // Written from the interrupt handler or the driver-work thread, read by
    // the cells: fields behind Volatile reads rather than auto-properties.
    private static int s_handlerCalls;
    private static uint s_acknowledged;
    private static int s_handlerWorkRuns;
    private static uint s_handlerWorkThreadId;
    private static bool s_handlerWorkOnIdleThread;
    private static int s_probeWorkRuns;
    private static bool s_probeWorkRanBeforeBound;
    private static uint s_probeWorkThreadId;
    private static bool s_probeWorkOnIdleThread;

    /// <summary>Set by Probe as its last step, just before it returns Bound; read by the handler and the work items.</summary>
    private static volatile bool s_probeReturned;

    /// <summary>The handler's work item, scheduled for every interrupt it services.</summary>
    private static DeviceWorkItem? s_handlerWork;

    /// <summary>The mouse Probe published, which the handler reports through.</summary>
    private static MouseReporter? s_mouse;

    /// <summary>True once Probe ran.</summary>
    public static bool Probed { get; private set; }

    /// <summary>The context Probe was handed, kept to check what it allows after Probe.</summary>
    public static PciDeviceContext? Context { get; private set; }

    /// <summary>The Command register as Probe found it, after the earlier candidates' teardown.</summary>
    public static ushort CommandAtProbe { get; private set; }

    /// <summary>BAR 0 as Probe mapped it, or null when the mapping failed.</summary>
    public static MmioRegion? Registers { get; private set; }

    /// <summary>What the identification register read.</summary>
    public static uint Identification { get; private set; }

    /// <summary>What the liveness register read after <see cref="LivenessPattern"/> was written.</summary>
    public static uint Liveness { get; private set; }

    /// <summary>True when the factorial unit finished within the poll limit.</summary>
    public static bool FactorialCompleted { get; private set; }

    /// <summary>The factorial unit's result.</summary>
    public static uint Factorial { get; private set; }

    /// <summary>True when a read at the region's length threw ArgumentOutOfRangeException.</summary>
    public static bool PastEndRefused { get; private set; }

    /// <summary>True when a 64-bit read straddling the region's end threw ArgumentOutOfRangeException.</summary>
    public static bool StraddlingRefused { get; private set; }

    /// <summary>True when a 32-bit read at offset 2 threw ArgumentOutOfRangeException.</summary>
    public static bool MisalignedRefused { get; private set; }

    /// <summary>True when the DMA check passed: a round trip, or a failure the allocator explains.</summary>
    public static bool DmaPassed { get; private set; }

    /// <summary>What the DMA check did and found, for the cell's report; null until it ran.</summary>
    public static string? DmaReport { get; private set; }

    /// <summary>What the first TryRequestInterrupts answered.</summary>
    public static bool InterruptsGranted { get; private set; }

    /// <summary>True when PublishMouse gave Probe a mouse.</summary>
    public static bool MousePublished => s_mouse is not null;

    /// <summary>The buttons the handler reports held for <see cref="MouseRaise"/>.</summary>
    // A property, not a const: the patcher cannot write back an assembly
    // holding a constant of an enum type another kernel assembly declares.
    public static MouseButtons MouseReportButtons => MouseButtons.Left | MouseButtons.Middle;

    /// <summary>True when a second TryRequestInterrupts in the same Probe threw InvalidOperationException.</summary>
    public static bool SecondRequestThrew { get; private set; }

    /// <summary>True when DeviceEvent.Wait threw InvalidOperationException inside Probe.</summary>
    public static bool WaitInProbeThrew { get; private set; }

    /// <summary>edu's interrupt status as Probe left it, after keeping <see cref="ProbeRaise"/> pending for a while.</summary>
    public static uint InterruptStatusAtProbeEnd { get; private set; }

    /// <summary>True when the handler ran while Probe was still running.</summary>
    public static bool HandlerRanBeforeBound { get; private set; }

    /// <summary>The event the handler signals for every interrupt it services; null until Probe created it.</summary>
    public static DeviceEvent? Event { get; private set; }

    /// <summary>What the Probe-time work item's first Schedule answered.</summary>
    public static bool ProbeWorkScheduled { get; private set; }

    /// <summary>What a second Schedule of the Probe-time work item answered, while the first was still pending.</summary>
    public static bool ProbeWorkScheduledAgain { get; private set; }

    /// <summary>True when TryCreateWorkItem gave Probe both work items.</summary>
    public static bool WorkItemsCreated { get; private set; }

    /// <summary>Calls of the handler, serviced or not: when polled, one per timer tick once Bound.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <summary>Every interrupt status bit the handler acknowledged.</summary>
    public static uint Acknowledged => Volatile.Read(ref s_acknowledged);

    /// <summary>Runs of the work item the handler schedules.</summary>
    public static int HandlerWorkRuns => Volatile.Read(ref s_handlerWorkRuns);

    /// <summary>The scheduler's ID for the thread the handler's work item last ran on.</summary>
    public static uint HandlerWorkThreadId => Volatile.Read(ref s_handlerWorkThreadId);

    /// <summary>True when the handler's work item last ran on the idle thread.</summary>
    public static bool HandlerWorkOnIdleThread => Volatile.Read(ref s_handlerWorkOnIdleThread);

    /// <summary>Runs of the work item Probe scheduled.</summary>
    public static int ProbeWorkRuns => Volatile.Read(ref s_probeWorkRuns);

    /// <summary>True when the Probe-time work item ran while Probe was still running.</summary>
    public static bool ProbeWorkRanBeforeBound => Volatile.Read(ref s_probeWorkRanBeforeBound);

    /// <summary>The scheduler's ID for the thread the Probe-time work item ran on.</summary>
    public static uint ProbeWorkThreadId => Volatile.Read(ref s_probeWorkThreadId);

    /// <summary>True when the Probe-time work item ran on the idle thread.</summary>
    public static bool ProbeWorkOnIdleThread => Volatile.Read(ref s_probeWorkOnIdleThread);

    /// <summary>
    /// Raises an edu interrupt with <paramref name="bits"/>, which the handler
    /// finds in the interrupt status register. Thread context, once Bound.
    /// </summary>
    public static void Raise(uint bits) => Registers?.Write32(InterruptRaiseRegister, bits);

    /// <summary>edu's interrupt status register now; 0 when BAR 0 never mapped.</summary>
    public static uint ReadInterruptStatus() => Registers?.Read32(InterruptStatusRegister) ?? 0;

    /// <inheritdoc />
    // protected internal, not protected: this assembly sees the HAL's
    // internals, so the override must keep the base's full accessibility.
    protected internal override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);
        Probed = true;
        Context = context;
        CommandAtProbe = context.Function.ReadConfig16(CommandOffset);

        if (!context.TryMapBar(RegisterBar, out MmioRegion? registers))
        {
            context.WriteLog("BAR 0 did not map");
            return ProbeResult.Failed;
        }

        Registers = registers;
        Identification = registers.Read32(IdentificationRegister);

        registers.Write32(LivenessRegister, LivenessPattern);
        Liveness = registers.Read32(LivenessRegister);

        CheckFactorial(context, registers);
        CheckAccessChecks(registers);
        CheckDma(context, registers);
        SetUpInterrupts(context, registers);

        // Last: the handler and the work items compare against it to tell
        // whether they ran before the kit armed them.
        s_probeReturned = true;
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Creates the event and the work items, requests interrupts, then
    /// raises one and keeps it pending for a few polling periods: the kit
    /// must not let the handler take it before Probe returns.
    /// </summary>
    private static void SetUpInterrupts(PciDeviceContext context, MmioRegion registers)
    {
        s_mouse = context.PublishMouse();

        DeviceEvent created = context.CreateEvent();
        Event = created;
        WaitInProbeThrew = WaitThrows(created);

        if (context.TryCreateWorkItem(OnProbeWork, out DeviceWorkItem? probeWork)
            && context.TryCreateWorkItem(OnHandlerWork, out DeviceWorkItem? handlerWork))
        {
            WorkItemsCreated = true;
            s_handlerWork = handlerWork;
            ProbeWorkScheduled = probeWork.Schedule();
            ProbeWorkScheduledAgain = probeWork.Schedule();
        }

        InterruptsGranted = context.TryRequestInterrupts(OnInterrupt);
        SecondRequestThrew = RequestThrows(context);

        registers.Write32(InterruptRaiseRegister, ProbeRaise);
        context.Delay(TimeSpan.FromMilliseconds(ProbeRaiseHoldMilliseconds));
        InterruptStatusAtProbeEnd = registers.Read32(InterruptStatusRegister);
    }

    private static bool WaitThrows(DeviceEvent deviceEvent)
    {
        try
        {
            deviceEvent.Wait();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private static bool RequestThrows(PciDeviceContext context)
    {
        try
        {
            context.TryRequestInterrupts(OnInterrupt);
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    /// <summary>
    /// The interrupt handler. Interrupt context: no allocation, no throw, no
    /// string. Polled, it runs on every tick, so it reads edu's interrupt
    /// status to see whether there is anything to service, acknowledges
    /// what it found, and hands the rest to thread context.
    /// </summary>
    private static void OnInterrupt(int vector)
    {
        s_handlerCalls++;
        if (!s_probeReturned)
        {
            HandlerRanBeforeBound = true;
        }

        if (Registers is not { } registers)
        {
            return;
        }

        uint status = registers.Read32(InterruptStatusRegister);
        if (status == 0)
        {
            return;
        }

        registers.Write32(InterruptAcknowledgeRegister, status);
        if ((status & MouseRaise) != 0)
        {
            s_mouse?.Report(MouseDeltaX, MouseDeltaY, MouseWheel, MouseReportButtons);
        }

        s_acknowledged |= status;
        Event?.Signal();
        s_handlerWork?.Schedule();
    }

    /// <summary>The work item Probe scheduled: driver-work thread, once Bound.</summary>
    private static void OnProbeWork()
    {
        if (!s_probeReturned)
        {
            Volatile.Write(ref s_probeWorkRanBeforeBound, true);
        }

        if (KernelState.TryGetCurrentThread(out uint id, out bool isIdle))
        {
            Volatile.Write(ref s_probeWorkThreadId, id);
            Volatile.Write(ref s_probeWorkOnIdleThread, isIdle);
        }

        Interlocked.Increment(ref s_probeWorkRuns);
    }

    /// <summary>The work item the handler schedules: driver-work thread.</summary>
    private static void OnHandlerWork()
    {
        if (KernelState.TryGetCurrentThread(out uint id, out bool isIdle))
        {
            Volatile.Write(ref s_handlerWorkThreadId, id);
            Volatile.Write(ref s_handlerWorkOnIdleThread, isIdle);
        }

        Interlocked.Increment(ref s_handlerWorkRuns);
    }

    private static void CheckFactorial(PciDeviceContext context, MmioRegion registers)
    {
        registers.Write32(FactorialRegister, FactorialInput);
        for (int poll = 0; poll < PollLimit; poll++)
        {
            if ((registers.Read32(StatusRegister) & StatusComputing) == 0)
            {
                FactorialCompleted = true;
                Factorial = registers.Read32(FactorialRegister);
                return;
            }

            context.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    private static void CheckAccessChecks(MmioRegion registers)
    {
        try
        {
            _ = registers.Read32(registers.Length);
        }
        catch (ArgumentOutOfRangeException)
        {
            PastEndRefused = true;
        }

        try
        {
            _ = registers.Read64(registers.Length - sizeof(uint));
        }
        catch (ArgumentOutOfRangeException)
        {
            StraddlingRefused = true;
        }

        try
        {
            _ = registers.Read32(2);
        }
        catch (ArgumentOutOfRangeException)
        {
            MisalignedRefused = true;
        }
    }

    /// <summary>
    /// Sends a pattern from a DMA buffer below edu's 28-bit limit into the
    /// device and back to another part of the buffer. When no buffer below
    /// the limit could be had, asks for one page with no limit instead: the
    /// allocator hands out its lowest free page, so that page's address shows
    /// whether any page below the limit was free at all.
    /// </summary>
    private static void CheckDma(PciDeviceContext context, MmioRegion registers)
    {
        if (!context.TryAllocateDma(DmaBufferLength, DmaAddressLimit, out DmaBuffer? buffer))
        {
            if (!context.TryAllocateDma(DmaBufferLength, ulong.MaxValue, out DmaBuffer? lowest))
            {
                DmaReport = "no DMA memory at all";
                return;
            }

            DmaPassed = lowest.DeviceAddress + DmaBufferLength - 1 > DmaAddressLimit;
            DmaReport = $"no buffer below 2^28; the lowest free page is at 0x{lowest.DeviceAddress:X}";
            return;
        }

        // Not all zeros: the buffer comes zeroed, and a transfer that never
        // happened must not read back as a round trip.
        Span<byte> data = buffer.Span;
        for (int i = 0; i < TransferLength; i++)
        {
            data[i] = (byte)(i * 7 + 3);
        }

        // edu's DMA reads go nowhere until the function may master the bus.
        context.EnableBusMastering();

        if (!RunDma(context, registers, buffer.DeviceAddress, DeviceBufferAddress, 0))
        {
            DmaReport = $"the RAM-to-device transfer from 0x{buffer.DeviceAddress:X} did not complete";
            return;
        }

        if (!RunDma(context, registers, DeviceBufferAddress, buffer.DeviceAddress + ReadBackOffset, DmaCommandToRam))
        {
            DmaReport = $"the device-to-RAM transfer to 0x{buffer.DeviceAddress + ReadBackOffset:X} did not complete";
            return;
        }

        for (int i = 0; i < TransferLength; i++)
        {
            if (data[ReadBackOffset + i] != data[i])
            {
                DmaReport = $"byte {i} came back as 0x{data[ReadBackOffset + i]:X2}, sent 0x{data[i]:X2}";
                return;
            }
        }

        DmaPassed = true;
        DmaReport = $"{TransferLength} bytes round-tripped through the buffer at 0x{buffer.DeviceAddress:X}";
    }

    /// <summary>
    /// Runs one transfer of <see cref="TransferLength"/> bytes and waits for
    /// the device to clear the start bit. The write barrier ahead of the
    /// command write orders the pattern's stores before the device can read
    /// them; the read barrier after each command read orders the caller's
    /// checks after the device's writes.
    /// </summary>
    private static bool RunDma(PciDeviceContext context, MmioRegion registers, ulong source, ulong destination, ulong direction)
    {
        registers.Write64(DmaSourceRegister, source);
        registers.Write64(DmaDestinationRegister, destination);
        registers.Write64(DmaCountRegister, TransferLength);
        registers.Write64(DmaCommandRegister, DmaCommandStart | direction);

        for (int poll = 0; poll < PollLimit; poll++)
        {
            if ((registers.Read64(DmaCommandRegister) & DmaCommandStart) == 0)
            {
                return true;
            }

            context.Delay(TimeSpan.FromMilliseconds(1));
        }

        return false;
    }
}
