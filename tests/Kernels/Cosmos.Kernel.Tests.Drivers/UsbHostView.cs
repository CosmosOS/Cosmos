// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Drivers.Pci.Bus.Ehci;
using Cosmos.Kernel.Drivers.Pci.Bus.Ohci;
using Cosmos.Kernel.Drivers.Pci.Bus.Xhci;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// What the USB keyboard and mouse tests read of the host controller above
/// their device, whichever of the three shipped host drivers holds it: the
/// state classes share these members by name, not by a common type. A
/// snapshot of the state: the bus is the live object, and a test that
/// waited takes a fresh view before reading a counter again.
/// </summary>
internal sealed class UsbHostView
{
    /// <summary>True once the host's hot-plug thread started; false without a scheduler.</summary>
    public bool HotPlugRunning { get; }

    /// <summary>True when an interrupt is connected to the host's handler.</summary>
    public bool HasInterrupt { get; }

    /// <summary>True when the host polls for its events instead.</summary>
    public bool IsPolling { get; }

    /// <summary>The bus the host's probe created: its ordinal and its device count.</summary>
    public UsbBus Bus { get; }

    /// <summary>How many pipes the host had closed when the view was taken.</summary>
    public int PipesClosed { get; }

    private UsbHostView(bool hotPlugRunning, bool hasInterrupt, bool isPolling, UsbBus bus, int pipesClosed)
    {
        HotPlugRunning = hotPlugRunning;
        HasInterrupt = hasInterrupt;
        IsPolling = isPolling;
        Bus = bus;
        PipesClosed = pipesClosed;
    }

    /// <summary>Reads a view off a binding's driver state.</summary>
    /// <param name="state">The driver state of a node's binding.</param>
    /// <returns>The view, or null when the state is not one of the three shipped host states.</returns>
    public static UsbHostView? Of(object? state)
    {
        return state switch
        {
            XhciState xhci => new UsbHostView(xhci.HotPlugRunning, xhci.HasInterrupt, xhci.IsPolling, xhci.Bus, xhci.PipesClosed),
            EhciState ehci => new UsbHostView(ehci.HotPlugRunning, ehci.HasInterrupt, ehci.IsPolling, ehci.Bus, ehci.PipesClosed),
            OhciState ohci => new UsbHostView(ohci.HotPlugRunning, ohci.HasInterrupt, ohci.IsPolling, ohci.Bus, ohci.PipesClosed),
            _ => null
        };
    }
}
