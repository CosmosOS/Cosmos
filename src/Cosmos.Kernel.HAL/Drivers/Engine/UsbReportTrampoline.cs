// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// Stands between an interrupt IN pipe and a driver's
/// <see cref="UsbReportHandler"/>: <see cref="OnReport"/> is the
/// <see cref="UsbInterruptHandler"/> the host controller calls for every
/// completed transfer. The pipe starts transferring the moment it is
/// opened, during Probe, and the host controller can never close it, so
/// every report reaches this trampoline; only an armed one passes it on.
/// Reports that arrive before the binding is Bound, after an attempt that
/// opened the pipe was declined or failed, or after the device left are
/// dropped, never buffered.
/// </summary>
internal sealed class UsbReportTrampoline
{
    private readonly UsbReportHandler _handler;

    /// <summary>
    /// Built in thread context when the pipe is opened, so that the
    /// interrupt path, where a string cannot be built, can still name the
    /// driver and the interface when a handler throws.
    /// </summary>
    private readonly string _failureMessage;

    /// <summary>Set once the binding is Bound; cleared first thing when it is torn down or unplugged.</summary>
    private volatile bool _armed;

    internal UsbReportTrampoline(UsbReportHandler handler, string driverName, string path)
    {
        _handler = handler;
        _failureMessage = $"[Drivers] {driverName} {path}: the report handler threw";
    }

    /// <summary>Lets reports through to the driver's handler.</summary>
    internal void Arm() => _armed = true;

    /// <summary>Stops reports at the trampoline, from the next one on.</summary>
    internal void Disarm() => _armed = false;

    /// <summary>
    /// The pipe's handler. Interrupt context: the host controller's
    /// interrupt handler, or a thread draining its events with interrupts
    /// masked. It allocates nothing, and reaches the driver through a
    /// delegate, not an interface.
    /// </summary>
    internal void OnReport(ReadOnlySpan<byte> report)
    {
        if (!_armed)
        {
            return;
        }

        try
        {
            _handler(report);
        }
        catch (Exception exception)
        {
            // A handler must not throw. Unwinding into the host controller's
            // event loop would leave its event ring half drained, so the
            // kernel halts, as it does for a PCI interrupt handler; this
            // only makes sure the panic says which driver it was.
            Serial.WriteString(exception.Message);
            Serial.WriteString("\n");
            Panic.Halt(_failureMessage);
        }
    }
}
