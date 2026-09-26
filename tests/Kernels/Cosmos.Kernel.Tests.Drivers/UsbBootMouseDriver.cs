// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The driver kit's sample HID boot-protocol mouse driver, with what the
/// cells need recorded on the side. No built-in driver matches 03/01/02
/// (the keyboard driver takes 03/01/01), and neither xHCI nor the hub driver
/// knows this class exists. One difference from the sample: it asks QEMU's
/// mouse for an idle report every 4 ms rather than on change only, so that
/// reports arrive while Probe runs, which the kit must hold back, and after
/// it, which must reach the handler. A cell asks for reports on change once
/// it saw them. It also creates a work item and an event the unplug cells
/// check are cancelled, can hold its work item running across an unplug,
/// and records its Remove. The mouse is pulled out and plugged back in, so
/// the static record describes the latest binding, and the instance count
/// how many the factory created.
/// </summary>
internal sealed class UsbBootMouseDriver : UsbDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "usb-boot-mouse";

    /// <summary>bInterfaceClass of HID (HID 1.11 §4.1).</summary>
    public const byte HidClass = 0x03;

    /// <summary>bInterfaceSubClass of a boot interface (HID 1.11 §4.2).</summary>
    public const byte BootSubclass = 0x01;

    /// <summary>bInterfaceProtocol of a boot mouse (HID 1.11 §4.3).</summary>
    public const byte MouseProtocol = 0x02;

    /// <summary>SET_IDLE (HID 1.11 §7.2.4).</summary>
    public const byte SetIdleRequest = 0x0A;

    /// <summary>SET_PROTOCOL (HID 1.11 §7.2.6).</summary>
    public const byte SetProtocolRequest = 0x0B;

    /// <summary>SET_IDLE's value for reports on change only: duration 0 in the high byte.</summary>
    public const ushort ReportOnChange = 0;

    /// <summary>SET_IDLE's value for a report every 4 ms: duration 1, in 4 ms units, in the high byte.</summary>
    public const ushort ReportEvery4Milliseconds = 0x0100;

    /// <summary>SET_PROTOCOL's value selecting the boot protocol.</summary>
    private const ushort BootProtocol = 0;

    /// <summary>A boot mouse report: buttons, X and Y, then an optional wheel (HID 1.11 appendix B.2).</summary>
    private const int MinimumReportLength = 3;

    /// <summary>
    /// How long Probe lets the opened endpoint run: a dozen idle periods, so
    /// reports do arrive while the handler must not see them.
    /// </summary>
    private const long ReportWindowMilliseconds = 50;

    /// <summary>
    /// Longest a held work item waits for the kit to report the device gone,
    /// so a kit that never does frees the driver-work thread in the end.
    /// </summary>
    private const long WorkHoldLimitMilliseconds = 3000;

    /// <summary>
    /// How long a held work item keeps running once the kit reported the
    /// device gone: the kit's unplug is then under way, and must wait for it
    /// before it calls Remove.
    /// </summary>
    private const long WorkOverlapMilliseconds = 200;

    private const long MillisecondsPerSecond = 1000;

    // Written by the handler, in interrupt context, and read by the cells.
    private static int s_handlerCalls;
    private static int s_callsWhileProbing;
    private static int s_lastReportLength;
    private static int s_forwardedReports;
    private static int s_forwardedX;
    private static int s_forwardedY;
    private static int s_lastForwardedButtons;

    // Written by Probe, the work item and Remove, and read by the cells.
    private static int s_instances;
    private static int s_workRuns;
    private static int s_removeCalls;
    private static long s_workFinishedAt;
    private static long s_removeStartedAt;

    /// <summary>Set for as long as Probe runs, which the handler checks.</summary>
    private static volatile bool s_probing;

    /// <summary>Set by a cell before it schedules the work item it pulls the mouse out under.</summary>
    private static volatile bool s_holdWork;

    /// <summary>Set by the held work item once it runs.</summary>
    private static volatile bool s_workHolding;

    private MouseReporter? _mouse;
    private UsbDeviceContext? _context;

    /// <summary>The buttons the last forwarded report held, which an idle report repeats.</summary>
    private MouseButtons _buttons;

    /// <summary>True once Probe ran.</summary>
    public static bool Probed { get; private set; }

    /// <summary>The context of the binding, or null when Probe never ran.</summary>
    public static UsbDeviceContext? Context { get; private set; }

    /// <summary>How SET_PROTOCOL to the boot protocol ended, or null when Probe never got that far.</summary>
    public static UsbTransferStatus? SetProtocolStatus { get; private set; }

    /// <summary>How SET_IDLE for a report every 4 ms ended, or null when Probe never got that far.</summary>
    public static UsbTransferStatus? SetIdleStatus { get; private set; }

    /// <summary>What OpenInterruptIn answered.</summary>
    public static bool InterruptPipeOpened { get; private set; }

    /// <summary>How GET_DESCRIPTOR(device) through the context ended during Probe, or null when Probe never got that far.</summary>
    public static UsbTransferResult? ProbeDescriptorResult { get; private set; }

    /// <summary>The mouse Probe published, or null when it never got that far.</summary>
    public static MouseReporter? Mouse { get; private set; }

    /// <summary>Calls of the report handler.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <summary>Calls of the report handler made while Probe still ran: the kit must have held all of them back.</summary>
    public static int CallsWhileProbing => Volatile.Read(ref s_callsWhileProbing);

    /// <summary>Length of the last report the handler got.</summary>
    public static int LastReportLength => Volatile.Read(ref s_lastReportLength);

    /// <summary>Reports the handler passed on to the published mouse: every one but the idle repeats.</summary>
    public static int ForwardedReports => Volatile.Read(ref s_forwardedReports);

    /// <summary>X movement of every report passed on, summed.</summary>
    public static int ForwardedX => Volatile.Read(ref s_forwardedX);

    /// <summary>Y movement of every report passed on, summed.</summary>
    public static int ForwardedY => Volatile.Read(ref s_forwardedY);

    /// <summary>Buttons of the last report passed on, as the report's first byte holds them.</summary>
    public static int LastForwardedButtons => Volatile.Read(ref s_lastForwardedButtons);

    /// <summary>Instances the registration's factory created, one per offering of a mouse interface.</summary>
    public static int Instances => Volatile.Read(ref s_instances);

    /// <summary>The scheduler's ID for the thread the latest Probe ran on.</summary>
    public static uint ProbeThreadId { get; private set; }

    /// <summary>Whether the latest Probe ran on its CPU's idle thread, the thread that boots the kernel.</summary>
    public static bool ProbeOnIdleThread { get; private set; }

    /// <summary>The work item the latest Probe created, or null when it got none.</summary>
    public static DeviceWorkItem? WorkItem { get; private set; }

    /// <summary>The event the latest Probe created, or null when Probe never got that far.</summary>
    public static DeviceEvent? Event { get; private set; }

    /// <summary>Runs of the work item's callback, across every binding.</summary>
    public static int WorkRuns => Volatile.Read(ref s_workRuns);

    /// <summary>True once a held work item started, and so holds the driver-work thread.</summary>
    public static bool WorkHolding => s_workHolding;

    /// <summary>Stopwatch time a held work item returned at, or 0 before.</summary>
    public static long WorkFinishedAt => Volatile.Read(ref s_workFinishedAt);

    /// <summary>Calls of Remove, across every binding.</summary>
    public static int RemoveCalls => Volatile.Read(ref s_removeCalls);

    /// <summary>Stopwatch time the last Remove started at, or 0 before.</summary>
    public static long RemoveStartedAt => Volatile.Read(ref s_removeStartedAt);

    /// <summary>The context the last Remove was given, or null before.</summary>
    public static UsbDeviceContext? RemovedContext { get; private set; }

    /// <summary>What the context reported for IsPresent when the last Remove ran.</summary>
    public static bool PresentAtRemove { get; private set; }

    /// <summary>Whether the binding's mouse was already withdrawn when the last Remove ran.</summary>
    public static bool MouseWithdrawnAtRemove { get; private set; }

    /// <summary>Whether the driver-work thread still ran a work item of the binding when the last Remove ran.</summary>
    public static bool WorkRunningAtRemove { get; private set; }

    /// <summary>Counts the instances the registration's factory created.</summary>
    public UsbBootMouseDriver()
    {
        Interlocked.Increment(ref s_instances);
    }

    /// <summary>The registration the kernel passes to Register.</summary>
    public static UsbDriverRegistration CreateRegistration() =>
        new(Name, static () => new UsbBootMouseDriver(), UsbMatch.Interface(HidClass, BootSubclass, MouseProtocol));

    /// <inheritdoc />
    // protected internal, not protected: this assembly sees the HAL's
    // internals, so the override must keep the base's full accessibility.
    protected internal override ProbeResult Probe(UsbDeviceContext context)
    {
        ProbeLog.Record(Name, context.Path);
        Probed = true;
        Context = context;
        _context = context;
        _ = KernelState.TryGetCurrentThread(out uint threadId, out bool isIdle);
        ProbeThreadId = threadId;
        ProbeOnIdleThread = isIdle;
        s_probing = true;
        try
        {
            UsbInterfaceInfo usbInterface = context.Interface;
            if (!usbInterface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint))
            {
                return ProbeResult.Declined;
            }

            SetProtocolStatus = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetProtocolRequest, BootProtocol,
                usbInterface.Number);
            if (SetProtocolStatus != UsbTransferStatus.Success)
            {
                return ProbeResult.Failed;
            }

            SetIdleStatus = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetIdleRequest, ReportEvery4Milliseconds,
                usbInterface.Number);

            _mouse = context.PublishMouse();
            Mouse = _mouse;
            Event = context.CreateEvent();
            WorkItem = context.TryCreateWorkItem(OnWork, out DeviceWorkItem? workItem) ? workItem : null;
            InterruptPipeOpened = context.OpenInterruptIn(endpoint, OnReport);
            if (!InterruptPipeOpened)
            {
                return ProbeResult.Failed;
            }

            // Reports arrive meanwhile. The control transfer after the wait
            // drains the controller's events on its way, so on a polled
            // controller too the reports queued meanwhile reach the kit
            // before Probe returns.
            context.Delay(TimeSpan.FromMilliseconds(ReportWindowMilliseconds));
            Span<byte> descriptor = stackalloc byte[UsbDescriptors.DeviceDescriptorLength];
            ProbeDescriptorResult = context.ControlIn(UsbRequestKind.Standard, UsbRecipient.Device, UsbDescriptors.GetDescriptorRequest,
                UsbDescriptors.DeviceDescriptorValue, 0, descriptor);
            return ProbeResult.Bound;
        }
        finally
        {
            s_probing = false;
        }
    }

    /// <summary>
    /// Records that the mouse left the bus and what the kit had done by
    /// then: the context not present, the mouse withdrawn, no work item of
    /// the binding running.
    /// </summary>
    protected internal override void Remove(UsbDeviceContext context)
    {
        Volatile.Write(ref s_removeStartedAt, Stopwatch.GetTimestamp());
        RemovedContext = context;
        PresentAtRemove = context.IsPresent;
        MouseWithdrawnAtRemove = _mouse is { } mouse && mouse.Device.IsWithdrawn;
        WorkRunningAtRemove = DriverWorkQueue.IsRunningItemOf(context);
        Interlocked.Increment(ref s_removeCalls);
    }

    /// <summary>
    /// Makes the next run of the work item hold the driver-work thread until
    /// the kit reports the device gone, and a while after, so the unplug
    /// finds it running.
    /// </summary>
    public static void HoldWorkAcrossUnplug()
    {
        s_workHolding = false;
        s_holdWork = true;
    }

    /// <summary>The work item's callback, on the driver-work thread.</summary>
    private void OnWork()
    {
        Interlocked.Increment(ref s_workRuns);
        if (!s_holdWork || _context is not { } context)
        {
            return;
        }

        s_holdWork = false;
        s_workHolding = true;
        long ticksPerMillisecond = Stopwatch.Frequency / MillisecondsPerSecond;
        long startedAt = Stopwatch.GetTimestamp();
        while (context.IsPresent && Stopwatch.GetTimestamp() - startedAt < ticksPerMillisecond * WorkHoldLimitMilliseconds)
        {
        }

        long goneAt = Stopwatch.GetTimestamp();
        while (Stopwatch.GetTimestamp() - goneAt < ticksPerMillisecond * WorkOverlapMilliseconds)
        {
        }

        Volatile.Write(ref s_workFinishedAt, Stopwatch.GetTimestamp());
    }

    // Interrupt context: no allocation, throw or lock.
    private void OnReport(ReadOnlySpan<byte> report)
    {
        if (s_probing)
        {
            s_callsWhileProbing++;
        }

        s_handlerCalls++;
        s_lastReportLength = report.Length;
        if (report.Length < MinimumReportLength || _mouse is not { } mouse)
        {
            return;
        }

        MouseButtons buttons = (MouseButtons)(report[0] & 0x07);
        int deltaX = (sbyte)report[1];
        int deltaY = (sbyte)report[2];

        // HID counts wheel up as positive, the kit as negative.
        int wheel = report.Length > MinimumReportLength ? -(sbyte)report[3] : 0;

        // An idle report repeats the state the device last reported. Passed
        // on, it would put the buttons back to that state over a report a
        // cell made through the reporter meanwhile.
        if (deltaX == 0 && deltaY == 0 && wheel == 0 && buttons == _buttons)
        {
            return;
        }

        _buttons = buttons;
        mouse.Report(deltaX, deltaY, wheel, buttons);

        // After the report, so a cell that sees the count finds the mouse
        // manager already moved.
        s_forwardedX += deltaX;
        s_forwardedY += deltaY;
        s_lastForwardedButtons = (int)buttons;
        Volatile.Write(ref s_forwardedReports, s_forwardedReports + 1);
    }
}
