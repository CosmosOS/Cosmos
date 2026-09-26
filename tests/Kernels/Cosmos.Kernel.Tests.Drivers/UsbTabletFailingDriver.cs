// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver for QEMU's USB tablet, a HID interface with no boot subclass or
/// protocol (03/00/00). Offered the tablet present at boot, it opens its
/// interrupt IN endpoint, publishes a mouse, asks the tablet for an idle
/// report every 4 ms, and fails. The endpoint cannot be closed, so the kit
/// must offer the tablet to no other driver, and the reports that keep
/// arriving must all stop at the kit. It keeps that attempt's context and
/// mouse for the cells to check that neither can be used any more. Offered
/// a tablet plugged in later, it declines having opened nothing, which lets
/// the kit offer the tablet to the next candidate.
/// </summary>
internal sealed class UsbTabletFailingDriver : UsbDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "usb-tablet-fails";

    /// <summary>bInterfaceSubClass of a HID interface without the boot protocol.</summary>
    public const byte NoSubclass = 0x00;

    /// <summary>bInterfaceProtocol of a HID interface without the boot protocol.</summary>
    public const byte NoProtocol = 0x00;

    // Written by the handler, in interrupt context, which must never run it:
    // the attempt failed.
    private static int s_handlerCalls;

    private static int s_probes;

    /// <summary>Probes that ran: the failing one, then one per tablet plugged in later.</summary>
    public static int Probes => Volatile.Read(ref s_probes);

    /// <summary>The context of the failed attempt, or null when Probe never ran.</summary>
    public static UsbDeviceContext? Context { get; private set; }

    /// <summary>What OpenInterruptIn answered.</summary>
    public static bool InterruptPipeOpened { get; private set; }

    /// <summary>How SET_IDLE for a report every 4 ms ended, or null when Probe never got that far.</summary>
    public static UsbTransferStatus? SetIdleStatus { get; private set; }

    /// <summary>The mouse the failed attempt published, or null when Probe never got that far.</summary>
    public static MouseReporter? Mouse { get; private set; }

    /// <summary>Calls of the failed attempt's report handler.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <inheritdoc />
    protected internal override ProbeResult Probe(UsbDeviceContext context)
    {
        ProbeLog.Record(Name, context.Path);
        if (Interlocked.Increment(ref s_probes) > 1)
        {
            return ProbeResult.Declined;
        }

        Context = context;

        UsbInterfaceInfo usbInterface = context.Interface;
        if (!usbInterface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint))
        {
            return ProbeResult.Declined;
        }

        Mouse = context.PublishMouse();
        InterruptPipeOpened = context.OpenInterruptIn(endpoint, OnReport);
        SetIdleStatus = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, UsbBootMouseDriver.SetIdleRequest,
            UsbBootMouseDriver.ReportEvery4Milliseconds, usbInterface.Number);
        return ProbeResult.Failed;
    }

    private static void OnReport(ReadOnlySpan<byte> report) => s_handlerCalls++;
}
