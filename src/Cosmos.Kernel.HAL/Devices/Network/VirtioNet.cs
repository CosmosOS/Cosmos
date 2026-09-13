// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Devices.Network;

/// <summary>
/// VirtIO network device driver. Transport-agnostic: works over virtio MMIO
/// (QEMU virt virtio-net-device) and virtio PCI (virtio-net-pci) alike.
/// </summary>
internal unsafe class VirtioNet : INetworkDevice
{
    // --- Constants ---

    // Virtio-net feature bits
    private const uint VIRTIO_NET_F_MAC = 1 << 5;
    private const uint VIRTIO_NET_F_STATUS = 1 << 16;
    private const uint VIRTIO_NET_S_LINK_UP = 1;

    // Queue indices
    private const ushort RX_QUEUE = 0;
    private const ushort TX_QUEUE = 1;

    // Without VIRTIO_F_VERSION_1 the net header is 10 bytes; virtio 1.x always
    // includes the num_buffers field, growing it to 12 (spec 5.1.6.1).
    private const int LegacyHeaderSize = 10;
    private const int ModernHeaderSize = 12;

    private const uint QUEUE_SIZE = 128;
    private const int RX_BUFFER_SIZE = 2048;

    // Device config space layout: u8 mac[6], u16 status (spec 5.1.4).
    private const uint MacConfigOffset = 0;
    private const uint StatusConfigOffset = 6;

    // --- Private fields ---

    private readonly VirtioTransport _transport;
    private int _headerSize = LegacyHeaderSize;

    // Guards the virtqueue free lists and used-ring cursors, which are
    // touched from both thread context (Send) and interrupt context
    // (OnDeviceInterrupt). Never held across OnPacketReceived — the network
    // stack sends replies from that callback, which would self-deadlock.
    private SchedSpinLock _queueLock;

    private MACAddress _macAddress;
    private bool _networkInitialized;
    private bool _linkUp;
    private bool _enabled;

    private Virtqueue? _rxQueue;
    private Virtqueue? _txQueue;

    private byte** _rxBuffers;
    private byte** _txBuffers;

    // --- Properties ---

    /// <summary>The transport this device was bound over (MMIO or PCI).</summary>
    public VirtioTransport Transport => _transport;

    public PacketReceivedHandler? OnPacketReceived { get; set; }
    string INetworkDevice.Name => "VirtioNet";
    public MACAddress MacAddress => _macAddress;
    public bool LinkUp => _linkUp;
    public bool Ready => _networkInitialized;

    // --- Constructor ---

    internal VirtioNet(VirtioTransport transport)
    {
        _transport = transport;
        _macAddress = MACAddress.None;
        _networkInitialized = false;
        _linkUp = false;
        _enabled = false;
    }

    // --- Public methods ---

    public void Initialize()
    {
        InitializeNetwork();
    }

    public bool Send(byte[] data, int length)
    {
        if (!_networkInitialized || !_enabled || _txQueue == null || _txBuffers == null || data == null)
        {
            return false;
        }

        using IrqLockScope scope = _queueLock.AcquireIrqSafe();

        ReclaimTxLocked();

        int descIdx = _txQueue.AllocDescriptor();
        if (descIdx < 0)
        {
            Serial.Write("[VirtioNet] No TX descriptors available\n");
            return false;
        }

        if (length > RX_BUFFER_SIZE - _headerSize)
        {
            length = RX_BUFFER_SIZE - _headerSize;
        }

        byte* buf = _txBuffers[descIdx];

        // Clear virtio-net header
        for (int i = 0; i < _headerSize; i++)
        {
            buf[i] = 0;
        }

        // Copy packet data
        for (int i = 0; i < length; i++)
        {
            buf[_headerSize + i] = data[i];
        }

        _txQueue.SetupDescriptor(descIdx, VirtioDma.VirtToPhys((ulong)buf), (uint)(_headerSize + length), 0, 0);
        _txQueue.AddAvailable((ushort)descIdx);

        // Notify device
        _transport.NotifyQueue(TX_QUEUE);

        return true;
    }

    public void Enable() => _enabled = true;
    public void Disable() => _enabled = false;

    // --- Private methods ---

    private void InitializeNetwork()
    {
        if (_networkInitialized)
        {
            return;
        }

        Serial.Write("[VirtioNet] Initializing (");
        Serial.Write(_transport.TransportName);
        Serial.Write(" transport)...\n");

        _transport.BeginInit();

        if (!_transport.NegotiateFeatures(VIRTIO_NET_F_MAC | VIRTIO_NET_F_STATUS, out uint features))
        {
            Serial.Write("[VirtioNet] ERROR: Feature negotiation failed\n");
            _transport.Fail();
            return;
        }

        _headerSize = _transport.Version1Negotiated ? ModernHeaderSize : LegacyHeaderSize;

        Serial.Write("[VirtioNet] Negotiated features: 0x");
        Serial.WriteHex(features);
        Serial.Write("\n");

        // Setup RX and TX queues
        _rxQueue = _transport.CreateQueue(RX_QUEUE, QUEUE_SIZE);
        _txQueue = _transport.CreateQueue(TX_QUEUE, QUEUE_SIZE);
        if (_rxQueue == null || _txQueue == null)
        {
            Serial.Write("[VirtioNet] ERROR: Failed to setup queues\n");
            _transport.Fail();
            return;
        }

        InitializeRxBuffers();
        InitializeTxBuffers();

        // Read MAC address from device config space
        if ((features & VIRTIO_NET_F_MAC) != 0)
        {
            byte[] mac = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                mac[i] = _transport.ReadDeviceConfig8(MacConfigOffset + (uint)i);
            }
            _macAddress = new MACAddress(mac);
            Serial.Write("[VirtioNet] MAC address: ");
            Serial.WriteString(_macAddress.ToString());
            Serial.Write("\n");
        }

        // Set DRIVER_OK to complete initialization
        _transport.FinishInit();

        // Check link status
        if ((features & VIRTIO_NET_F_STATUS) != 0)
        {
            ushort linkStatus = _transport.ReadDeviceConfig16(StatusConfigOffset);
            _linkUp = (linkStatus & VIRTIO_NET_S_LINK_UP) != 0;
            Serial.Write("[VirtioNet] Link status: ");
            Serial.WriteString(_linkUp ? "UP" : "DOWN");
            Serial.Write("\n");
        }
        else
        {
            _linkUp = true;
        }

        _networkInitialized = true;
        _enabled = true;

        // Register the interrupt handler AFTER the device is fully
        // initialized: with level-triggered lines the interrupt fires as soon
        // as it is enabled if the line is already asserted, and the handler
        // must be able to process it (requires _networkInitialized = true).
        if (!_transport.EnableInterrupt(OnDeviceInterrupt))
        {
            // Nothing in the system pumps a network device (there is no Poll
            // path), so without an interrupt source received packets would
            // sit in the used ring forever. Fail loudly instead of
            // advertising a NIC whose RX silently never completes.
            Serial.Write("[VirtioNet] ERROR: No interrupt path available; disabling device\n");
            _networkInitialized = false;
            _enabled = false;
            _linkUp = false;
            _transport.Fail();
            return;
        }

        Serial.Write("[VirtioNet] Initialization complete\n");
    }

    private void InitializeRxBuffers()
    {
        if (_rxQueue == null)
        {
            return;
        }

        Serial.Write("[VirtioNet] Initializing RX buffers...\n");

        _rxBuffers = (byte**)MemoryOp.Alloc((uint)(_rxQueue.QueueSize * sizeof(byte*)));
        for (int i = 0; i < _rxQueue.QueueSize; i++)
        {
            _rxBuffers[i] = (byte*)MemoryOp.Alloc(RX_BUFFER_SIZE);
            int descIdx = _rxQueue.AllocDescriptor();
            if (descIdx < 0)
            {
                break;
            }

            _rxQueue.SetupDescriptor(descIdx, VirtioDma.VirtToPhys((ulong)_rxBuffers[i]), RX_BUFFER_SIZE,
                Virtqueue.VRING_DESC_F_WRITE, 0);
            _rxQueue.AddAvailable((ushort)descIdx);
        }

        // Notify device that RX buffers are available
        _transport.NotifyQueue(RX_QUEUE);

        Serial.Write("[VirtioNet] RX buffers initialized\n");
    }

    private void InitializeTxBuffers()
    {
        if (_txQueue == null)
        {
            return;
        }

        Serial.Write("[VirtioNet] Initializing TX buffers...\n");

        _txBuffers = (byte**)MemoryOp.Alloc((uint)(_txQueue.QueueSize * sizeof(byte*)));
        for (int i = 0; i < _txQueue.QueueSize; i++)
        {
            _txBuffers[i] = (byte*)MemoryOp.Alloc(RX_BUFFER_SIZE);
        }

        Serial.Write("[VirtioNet] TX buffers initialized\n");
    }

    private void OnDeviceInterrupt(uint isrStatus)
    {
        if (!_networkInitialized)
        {
            return;
        }

        // Process used buffers
        if ((isrStatus & VirtioTransport.IsrQueue) != 0)
        {
            ProcessRx();

            using IrqLockScope scope = _queueLock.AcquireIrqSafe();
            ReclaimTxLocked();
        }
    }

    private void ProcessRx()
    {
        if (_rxQueue == null || _rxBuffers == null)
        {
            return;
        }

        bool received = false;
        while (true)
        {
            // Pop and recycle one used buffer under the queue lock, but
            // deliver the packet outside it: the network stack sends replies
            // (ARP, TCP ACKs) from OnPacketReceived, and Send takes the lock.
            byte[]? packet = null;
            using (IrqLockScope scope = _queueLock.AcquireIrqSafe())
            {
                if (!_rxQueue.GetUsedBuffer(out uint id, out uint len))
                {
                    break;
                }

                if (id < _rxQueue.QueueSize && len > _headerSize)
                {
                    uint dataLen = len - (uint)_headerSize;
                    packet = new byte[dataLen];
                    byte* src = _rxBuffers[id] + _headerSize;
                    for (int i = 0; i < dataLen; i++)
                    {
                        packet[i] = src[i];
                    }
                }

                // Return buffer to available ring
                if (id < _rxQueue.QueueSize)
                {
                    _rxQueue.SetupDescriptor((int)id, VirtioDma.VirtToPhys((ulong)_rxBuffers[id]), RX_BUFFER_SIZE,
                        Virtqueue.VRING_DESC_F_WRITE, 0);
                    _rxQueue.AddAvailable((ushort)id);
                }

                received = true;
            }

            if (packet != null)
            {
                OnPacketReceived?.Invoke(packet, packet.Length);
            }
        }

        if (received)
        {
            // Notify device that new RX buffers are available
            _transport.NotifyQueue(RX_QUEUE);
        }
    }

    /// <summary>Reclaims completed TX descriptors. Caller must hold _queueLock.</summary>
    private void ReclaimTxLocked()
    {
        if (_txQueue == null)
        {
            return;
        }

        while (_txQueue.GetUsedBuffer(out uint id, out uint len))
        {
            if (id < _txQueue.QueueSize)
            {
                _txQueue.FreeDescriptor((int)id);
            }
        }
    }
}
