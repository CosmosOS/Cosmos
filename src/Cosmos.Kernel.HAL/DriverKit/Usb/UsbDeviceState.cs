// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// What the kit keeps about one attached device: the host's device object,
/// where it is in the tree, the descriptors the enumeration core read, the
/// interface nodes it published and the binding that published them. The
/// tree links (<see cref="Children"/>, and this state's place in its
/// parent's list) change under the bus lock only; everything else is fixed
/// at attach. The control pipe is one per device, so the state also holds
/// the claim that serializes the interfaces' control transfers.
/// </summary>
internal sealed class UsbDeviceState
{
    /// <summary>How long a second interface waits between two looks at the control claim, in microseconds.</summary>
    private const uint ControlRetryMicroseconds = 10;

    private SchedSpinLock _controlLock;
    private bool _controlBusy;

    internal UsbDeviceState(UsbDevice device, UsbBus bus, UsbDeviceState? parent, byte port, string portPath,
        byte[] configuration, UsbInterfaceInfo[] interfaces, DeviceBinding owner)
    {
        Device = device;
        Bus = bus;
        Parent = parent;
        Port = port;
        PortPath = portPath;
        Configuration = configuration;
        Interfaces = interfaces;
        Owner = owner;
    }

    /// <summary>The host's device object.</summary>
    internal UsbDevice Device { get; }

    /// <summary>The bus the device is on.</summary>
    internal UsbBus Bus { get; }

    /// <summary>The hub the device hangs off, or null for a root port.</summary>
    internal UsbDeviceState? Parent { get; }

    /// <summary>The 1-based port number on the parent hub or the controller.</summary>
    internal byte Port { get; }

    /// <summary>The dotted port chain from the controller down: <c>1-2</c>, <c>1-2.1</c>.</summary>
    internal string PortPath { get; }

    /// <summary>The whole configuration descriptor as read.</summary>
    internal byte[] Configuration { get; }

    /// <summary>The interfaces of the first configuration, in descriptor order.</summary>
    internal UsbInterfaceInfo[] Interfaces { get; }

    /// <summary>The interface nodes, one per interface in interface order; empty until published, or when the device has no interface.</summary>
    internal DeviceNode[] Nodes { get; set; } = [];

    /// <summary>The devices attached on this hub's ports. Changed and read under the bus lock.</summary>
    internal List<UsbDeviceState> Children { get; } = [];

    /// <summary>The host or hub binding that published <see cref="Nodes"/>.</summary>
    internal DeviceBinding Owner { get; }

    /// <summary>
    /// Runs one control transfer with the device's control pipe claimed:
    /// another interface of the same device mid-transfer makes the caller
    /// wait, in passes of <see cref="ControlRetryMicroseconds"/> outside the
    /// lock, bounded by that transfer's own timeout; each pass re-reads
    /// <see cref="UsbDevice.IsDisconnected"/> and returns Disconnected
    /// without claiming. Thread context.
    /// </summary>
    /// <param name="setup">The request.</param>
    /// <param name="data">The OUT data or the IN buffer.</param>
    internal UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data)
    {
        while (true)
        {
            if (Device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            bool claimed;
            using (_controlLock.AcquireIrqSafe())
            {
                claimed = !_controlBusy;
                if (claimed)
                {
                    _controlBusy = true;
                }
            }

            if (claimed)
            {
                break;
            }

            Owner.Delay(ControlRetryMicroseconds);
        }

        try
        {
            return Device.ControlTransfer(setup, data);
        }
        finally
        {
            using (_controlLock.AcquireIrqSafe())
            {
                _controlBusy = false;
            }
        }
    }
}
