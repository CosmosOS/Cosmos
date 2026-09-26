// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace DevKernel.Drivers;

/// <summary>
/// HID boot-protocol mouse, one instance per bound interface, written
/// against the driver kit alone. No built-in driver matches 03/01/02 (the
/// keyboard driver takes 03/01/01), and neither the host controller nor the
/// hub driver knows this class exists. The kit offers it every boot mouse
/// interface: the ones present at boot, and those of every mouse plugged in
/// later.
/// </summary>
/// <remarks>
/// The driver overrides no Remove: when the mouse is pulled out, the kit
/// takes the mouse it published back out of the mouse manager on its own,
/// and nothing else needs letting go of.
/// </remarks>
internal sealed class UsbBootMouseDriver : UsbDriver
{
    /// <summary>The registration's name, and the owner every boot mouse interface gets.</summary>
    public const string Name = "usb-boot-mouse";

    /// <summary>bInterfaceClass of HID (HID 1.11 §4.1).</summary>
    private const byte HidClass = 0x03;

    /// <summary>bInterfaceSubClass of a boot interface (HID 1.11 §4.2).</summary>
    private const byte BootSubclass = 0x01;

    /// <summary>bInterfaceProtocol of a boot mouse (HID 1.11 §4.3).</summary>
    private const byte MouseProtocol = 0x02;

    /// <summary>SET_IDLE (HID 1.11 §7.2.4).</summary>
    private const byte SetIdleRequest = 0x0A;

    /// <summary>SET_PROTOCOL (HID 1.11 §7.2.6).</summary>
    private const byte SetProtocolRequest = 0x0B;

    /// <summary>SET_PROTOCOL's value selecting the boot protocol.</summary>
    private const ushort BootProtocol = 0;

    /// <summary>SET_IDLE's value for reports on change only: duration 0 in the high byte.</summary>
    private const ushort ReportOnChange = 0;

    /// <summary>A boot mouse report: buttons, X and Y, then an optional wheel (HID 1.11 appendix B.2).</summary>
    private const int MinimumReportLength = 3;

    /// <summary>The report's first byte holds the buttons in its low three bits.</summary>
    private const int ButtonBits = 0x07;

    // Set in Probe; the kit calls the report handler only after Probe
    // returns Bound, so the null check below is for the compiler.
    private MouseReporter? _mouse;

    /// <summary>The registration the kernel passes to DriverManager.Register: the HID boot mouse interface.</summary>
    /// <returns>A registration named <see cref="Name"/>.</returns>
    public static UsbDriverRegistration CreateRegistration() =>
        new(Name, static () => new UsbBootMouseDriver(), UsbMatch.Interface(HidClass, BootSubclass, MouseProtocol));

    /// <inheritdoc />
    protected override ProbeResult Probe(UsbDeviceContext context)
    {
        UsbInterfaceInfo usbInterface = context.Interface;
        if (!usbInterface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint))
        {
            return ProbeResult.Declined;
        }

        if (context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetProtocolRequest, BootProtocol,
                usbInterface.Number) != UsbTransferStatus.Success)
        {
            return ProbeResult.Failed;
        }

        // Report on change only. Some mice stall SET_IDLE and still work, so
        // its status is ignored.
        _ = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetIdleRequest, ReportOnChange,
            usbInterface.Number);

        // The endpoint starts transferring as it opens, but the kit drops the
        // reports until Probe returns Bound, then hands the mouse to the
        // mouse manager before it lets the first one through to OnReport.
        _mouse = context.PublishMouse();
        return context.OpenInterruptIn(endpoint, OnReport) ? ProbeResult.Bound : ProbeResult.Failed;
    }

    /// <summary>
    /// The report handler. Interrupt context: no allocation, no throw, no
    /// string, no lock.
    /// </summary>
    private void OnReport(ReadOnlySpan<byte> report)
    {
        if (report.Length < MinimumReportLength || _mouse is not { } mouse)
        {
            return;
        }

        MouseButtons buttons = (MouseButtons)(report[0] & ButtonBits);

        // HID counts wheel up as positive, the kit as negative. HID's Y
        // already grows downward, as the kit's does.
        int wheel = report.Length > MinimumReportLength ? -(sbyte)report[3] : 0;
        mouse.Report((sbyte)report[1], (sbyte)report[2], wheel, buttons);
    }
}
