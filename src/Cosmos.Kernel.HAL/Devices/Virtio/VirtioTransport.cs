// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Virtio;

/// <summary>
/// Driver callback invoked from interrupt context when the device signals.
/// <paramref name="isrStatus"/> carries the transport ISR bits
/// (<see cref="VirtioTransport.IsrQueue"/> / <see cref="VirtioTransport.IsrConfig"/>);
/// the transport has already acknowledged the interrupt.
/// </summary>
internal delegate void VirtioInterruptHandler(uint isrStatus);

/// <summary>
/// Wires a platform line interrupt (e.g. a GIC SPI) to a transport dispatch
/// handler. Supplied by the platform initializer that owns the bus, so the
/// shared transport stays free of per-arch interrupt-controller calls.
/// </summary>
internal delegate void VirtioIrqEnable(uint intid, InterruptManager.IrqDelegate handler);

/// <summary>
/// Transport-independent core of a virtio device (virtio spec 1.x): device
/// status handshake, feature negotiation, virtqueue setup, notifications,
/// interrupts, and device-specific config access. Device drivers (VirtioNet,
/// VirtioKeyboard, VirtioMouse) are written against this class only;
/// <see cref="VirtioMmioTransport"/> and <see cref="VirtioPciTransport"/>
/// implement the per-transport register access.
/// </summary>
internal abstract class VirtioTransport
{
    // Virtio device types (virtio spec section 5).
    public const uint DeviceTypeNetwork = 1;
    public const uint DeviceTypeBlock = 2;
    public const uint DeviceTypeConsole = 3;
    public const uint DeviceTypeEntropy = 4;
    public const uint DeviceTypeGpu = 16;
    public const uint DeviceTypeInput = 18;

    // Device status bits (virtio spec section 2.1).
    public const byte StatusAcknowledge = 1;
    public const byte StatusDriver = 2;
    public const byte StatusDriverOk = 4;
    public const byte StatusFeaturesOk = 8;
    public const byte StatusFailed = 128;

    // ISR status bits: bit 0 = used-ring update, bit 1 = configuration change.
    public const uint IsrQueue = 1;
    public const uint IsrConfig = 2;

    /// <summary>VIRTIO_F_VERSION_1 (bit 32): device and driver use virtio 1.x semantics.</summary>
    private const ulong FeatureVersion1 = 1UL << 32;

    /// <summary>Virtio device type this transport was probed as.</summary>
    public abstract uint DeviceType { get; }

    /// <summary>Short transport name for serial logs ("MMIO" / "PCI").</summary>
    public abstract string TransportName { get; }

    /// <summary>True once VIRTIO_F_VERSION_1 has been negotiated (modern device semantics).</summary>
    public bool Version1Negotiated { get; private set; }

    /// <summary>
    /// Resets the device and announces the driver (status 0, then ACKNOWLEDGE, then DRIVER).
    /// </summary>
    public void BeginInit()
    {
        SetStatus(0);
        AfterReset();
        AddStatus(StatusAcknowledge);
        AddStatus(StatusDriver);
    }

    /// <summary>
    /// Negotiates features: accepts the requested device-specific low-32 bits
    /// that the device offers, plus VIRTIO_F_VERSION_1 when offered. Returns
    /// false if the device rejects the selection (FEATURES_OK not accepted).
    /// </summary>
    public bool NegotiateFeatures(uint requestedLow, out uint negotiatedLow)
    {
        ulong offered = ReadDeviceFeatures();
        ulong accepted = offered & requestedLow;
        if ((offered & FeatureVersion1) != 0)
        {
            accepted |= FeatureVersion1;
        }

        WriteDriverFeatures(accepted);
        Version1Negotiated = (accepted & FeatureVersion1) != 0;
        negotiatedLow = (uint)accepted;

        if (!SupportsFeaturesOk)
        {
            return true;
        }

        AddStatus(StatusFeaturesOk);
        if ((GetStatus() & StatusFeaturesOk) == 0)
        {
            Serial.Write("[Virtio] Device rejected feature selection\n");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Allocates and activates virtqueue <paramref name="index"/>, clamped to
    /// the device's maximum size. Returns null if the queue is unavailable.
    /// </summary>
    public Virtqueue? CreateQueue(ushort index, uint preferredSize)
    {
        if (IsQueueInUse(index))
        {
            Serial.Write("[Virtio] Queue ");
            Serial.WriteNumber((uint)index);
            Serial.Write(" already in use\n");
            return null;
        }

        uint maxSize = GetQueueMaxSize(index);
        if (maxSize == 0)
        {
            Serial.Write("[Virtio] Queue ");
            Serial.WriteNumber((uint)index);
            Serial.Write(" not available\n");
            return null;
        }

        uint queueSize = maxSize < preferredSize ? maxSize : preferredSize;
        Virtqueue queue = new Virtqueue(queueSize);
        if (!ActivateQueue(index, queue))
        {
            return null;
        }

        return queue;
    }

    /// <summary>Completes initialization (DRIVER_OK): the device goes live.</summary>
    public void FinishInit() => AddStatus(StatusDriverOk);

    /// <summary>Tells the device the driver has given up on it.</summary>
    public void Fail() => AddStatus(StatusFailed);

    /// <summary>Sets device status bits on top of the currently set ones.</summary>
    protected void AddStatus(byte bits) => SetStatus((byte)(GetStatus() | bits));

    /// <summary>
    /// Transport hook run right after device reset (legacy MMIO guest page
    /// size, PCI MSI-X config vector — reset clears both).
    /// </summary>
    protected virtual void AfterReset()
    {
    }

    /// <summary>Whether the transport supports the FEATURES_OK handshake step (virtio 1.x).</summary>
    protected abstract bool SupportsFeaturesOk { get; }

    protected abstract byte GetStatus();
    protected abstract void SetStatus(byte status);
    protected abstract ulong ReadDeviceFeatures();
    protected abstract void WriteDriverFeatures(ulong features);
    protected abstract bool IsQueueInUse(ushort index);
    protected abstract uint GetQueueMaxSize(ushort index);
    protected abstract bool ActivateQueue(ushort index, Virtqueue queue);

    /// <summary>Rings the doorbell for queue <paramref name="index"/> (orders ring writes first).</summary>
    public abstract void NotifyQueue(ushort index);

    /// <summary>
    /// Reads and acknowledges the ISR status. Level-signaled transports
    /// deassert the interrupt line here; returns 0 when nothing is pending.
    /// </summary>
    public abstract uint ReadAndAckIsr();

    /// <summary>
    /// Arranges for <paramref name="handler"/> to run (from interrupt context,
    /// interrupt already acknowledged) when the device signals. Returns false
    /// if the platform offers no interrupt path for this device (polled mode).
    /// </summary>
    public abstract bool EnableInterrupt(VirtioInterruptHandler handler);

    public abstract byte ReadDeviceConfig8(uint offset);
    public abstract ushort ReadDeviceConfig16(uint offset);
    public abstract void WriteDeviceConfig8(uint offset, byte value);
}
