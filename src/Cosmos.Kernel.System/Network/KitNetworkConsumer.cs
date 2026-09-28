// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The network manager's consumer of the driver kit: every network
/// interface a kit driver publishes becomes a <see cref="KitNetworkDevice"/>
/// in the manager's table, and leaves it when withdrawn. A received frame
/// is copied into an array and handed to the stack's handler under
/// <see cref="InternalCpu.DisableInterruptsScope"/>: the stack has no locks
/// and was serialized until now by running inside the driver's interrupt
/// handler, so the scope restores that atomicity on the single CPU (a
/// driver's transmit lock nests inside it, since the kit's lock scope saves
/// and restores the interrupt flag). That is the migration-period rule
/// until the stack gets a lock of its own. A frame the stack's handler
/// throws on is dropped and logged here: the kit cancels a work item that
/// throws, which would stop the driver's receive path for good. Installed
/// by <see cref="NetworkManager.Initialize"/>, before the driver stage runs.
/// <para>
/// Contexts: <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run on
/// the kit worker in thread context; <see cref="OnReceive"/> needs thread
/// context too, since it allocates the copy, which the kit's network
/// drivers give it by delivering from a work item; <see cref="OnLinkChanged"/>
/// runs in the driver's context and only writes a flag.
/// </para>
/// </summary>
internal sealed class KitNetworkConsumer : NetworkConsumer
{
    private KitNetworkDevice[] _adapters = [];

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The manager's table is full: the kit leaves the device unconsumed and the publishing probe fails, rather than binding an interface the ring never sees.</exception>
    public override void OnPublished(PublishedDevice device)
    {
        INetworkInterface network = (INetworkInterface)device.Device;
        KitNetworkDevice adapter = new(device, network);
        if (!NetworkManager.RegisterDevice(adapter))
        {
            throw new InvalidOperationException("the network manager's device table is full");
        }

        KitNetworkDevice[] current = _adapters;
        KitNetworkDevice[] adapters = new KitNetworkDevice[current.Length + 1];
        Array.Copy(current, adapters, current.Length);
        adapters[current.Length] = adapter;
        _adapters = adapters;
    }

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device)
    {
        KitNetworkDevice? adapter = Find(device);
        if (adapter is null)
        {
            return;
        }

        KitNetworkDevice[] current = _adapters;
        KitNetworkDevice[] adapters = new KitNetworkDevice[current.Length - 1];
        int kept = 0;
        for (int i = 0; i < current.Length; i++)
        {
            if (!ReferenceEquals(current[i], adapter))
            {
                adapters[kept++] = current[i];
            }
        }

        _adapters = adapters;
        NetworkManager.UnregisterDevice(adapter);
    }

    /// <inheritdoc/>
    public override void OnReceive(PublishedDevice device, ReadOnlySpan<byte> frame)
    {
        KitNetworkDevice? adapter = Find(device);
        if (adapter is null)
        {
            return;
        }

        // The stack installs its handler when an address is configured on
        // the device; until then the frame is dropped here, as the drivers
        // dropped it before.
        PacketReceivedHandler? handler = adapter.OnPacketReceived;
        if (handler is null)
        {
            return;
        }

        byte[] copy = frame.ToArray();
        using (InternalCpu.DisableInterruptsScope())
        {
            try
            {
                handler(copy, copy.Length);
            }
            catch (Exception exception)
            {
                Serial.WriteString("[NetworkManager] packet handler threw: ");
                Serial.WriteString(exception.Message);
                Serial.WriteString("\n");
            }
        }
    }

    /// <inheritdoc/>
    public override void OnLinkChanged(PublishedDevice device, bool up) => Find(device)?.LinkUp = up;

    /// <summary>The adapter of a published device, by reference: a scan of a copy-on-write array, allocation-free. Any context.</summary>
    private KitNetworkDevice? Find(PublishedDevice device)
    {
        KitNetworkDevice[] adapters = _adapters;
        for (int i = 0; i < adapters.Length; i++)
        {
            if (ReferenceEquals(adapters[i].Published, device))
            {
                return adapters[i];
            }
        }

        return null;
    }
}
