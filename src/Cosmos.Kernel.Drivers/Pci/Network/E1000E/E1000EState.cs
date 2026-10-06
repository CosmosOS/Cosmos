// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.Drivers.Pci.Network.E1000E;

/// <summary>
/// Everything <see cref="E1000EDriver"/> holds for one bound controller,
/// hung off <see cref="DeviceBinding.DriverState"/>, and the network
/// interface it publishes: the register window, the two descriptor rings
/// and their buffers, the kit lock, the sink, the drain work item and the
/// counters. <see cref="Transmit"/> is entered by the ring from its own
/// thread and again from the drain's synchronous replies, so it runs under
/// the <see cref="DeviceLock"/>; <see cref="Drain"/> runs on the kit worker
/// and takes the lock only around its own ring index updates, never across
/// a sink call; <see cref="OnInterrupt"/> runs in interrupt context and
/// only reads the cause register and schedules the drain.
/// </summary>
public sealed class E1000EState : INetworkInterface
{
    /// <summary>Descriptors per ring; the controller wants a multiple of 8.</summary>
    internal const int DescriptorCount = 32;

    /// <summary>Bytes per legacy descriptor.</summary>
    internal const int DescriptorBytes = 16;

    /// <summary>Bytes per ring.</summary>
    internal const int RingBytes = DescriptorCount * DescriptorBytes;

    /// <summary>Alignment of a ring: the controller needs 16 bytes.</summary>
    internal const int RingAlignment = 16;

    /// <summary>Bytes per frame buffer, the RCTL buffer size and the transmit length cap.</summary>
    internal const int BufferBytes = 2048;

    /// <summary>Alignment of the buffer area: one page.</summary>
    internal const int BufferAlignment = 4096;

    /// <summary>Bytes of one buffer area: one buffer per descriptor.</summary>
    internal const int BuffersBytes = DescriptorCount * BufferBytes;

    /// <summary>The last descriptor index, where the receive tail starts.</summary>
    private const int LastDescriptor = DescriptorCount - 1;

    private readonly DeviceBinding _binding;
    private readonly RegisterWindow _registers;
    private readonly MACAddress _macAddress;
    private readonly DmaBuffer _receiveRing;
    private readonly DmaBuffer _transmitRing;
    private readonly DmaBuffer _receiveBuffers;
    private readonly DmaBuffer _transmitBuffers;
    private DeviceLock? _lock;
    private NetworkSink? _sink;
    private WorkItem? _drainWork;
    private int _receiveTail;
    private int _transmitTail;
    private volatile bool _linkUp;
    private volatile bool _isPolling;
    private volatile bool _hasLine;
    private volatile int _framesReceived;
    private volatile int _framesTransmitted;
    private volatile int _linkChanges;
    private volatile int _interruptCount;

    /// <summary>
    /// Takes the binding, the window, the address and the four DMA buffers
    /// the probe acquired; the rings are already programmed. The receive
    /// tail starts on the last descriptor, the transmit tail on the first,
    /// as the registers were set. Thread context, from the probe.
    /// </summary>
    internal E1000EState(DeviceBinding binding, RegisterWindow registers, MACAddress macAddress, DmaBuffer receiveRing, DmaBuffer transmitRing, DmaBuffer receiveBuffers, DmaBuffer transmitBuffers)
    {
        _binding = binding;
        _registers = registers;
        _macAddress = macAddress;
        _receiveRing = receiveRing;
        _transmitRing = transmitRing;
        _receiveBuffers = receiveBuffers;
        _transmitBuffers = transmitBuffers;
        _receiveTail = LastDescriptor;
        _transmitTail = 0;
    }

    /// <inheritdoc/>
    public string Name => "e1000e";

    /// <inheritdoc/>
    public MACAddress MacAddress => _macAddress;

    /// <summary>True while the link is up, as the probe read it and the drain keeps it. Any context.</summary>
    public bool LinkUp
    {
        get => _linkUp;
        internal set => _linkUp = value;
    }

    /// <summary>How many frames the drain handed to the ring. Any context.</summary>
    public int FramesReceived => _framesReceived;

    /// <summary>How many frames <see cref="Transmit"/> queued. Any context.</summary>
    public int FramesTransmitted => _framesTransmitted;

    /// <summary>How many times the drain saw the link change state. Any context.</summary>
    public int LinkChanges => _linkChanges;

    /// <summary>How many times the line handler ran. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>True when the kit runs the drain periodically as well. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True when the function's legacy line is connected to <see cref="OnInterrupt"/>. Any context.</summary>
    public bool HasLine
    {
        get => _hasLine;
        internal set => _hasLine = value;
    }

    /// <summary>The BAR0 window, for the driver's detach hook.</summary>
    internal RegisterWindow Registers => _registers;

    /// <summary>The lock <see cref="Transmit"/> and the drain's index updates run under; set by the probe before the interface is published.</summary>
    internal DeviceLock? Lock
    {
        get => _lock;
        set => _lock = value;
    }

    /// <summary>The sink frames and link changes go to; set by the probe when the interface is published.</summary>
    internal NetworkSink? Sink
    {
        get => _sink;
        set => _sink = value;
    }

    /// <summary>The work item running <see cref="Drain"/>, which the handler schedules; set by the probe before the line is connected.</summary>
    internal WorkItem? DrainWork
    {
        get => _drainWork;
        set => _drainWork = value;
    }

    /// <summary>
    /// Queues one frame on the transmit ring. False before the probe created
    /// the lock; otherwise, under the lock: false when the frame is empty or
    /// longer than a buffer, or when the ring is full (the next slot is the
    /// controller's head); else the frame is copied into the tail slot, its
    /// descriptor written, the stores ordered, and the tail register
    /// advanced. Any context; called by the ring from its own thread and
    /// from the drain's synchronous replies.
    /// </summary>
    /// <param name="frame">The frame, without checksum.</param>
    /// <returns>True when the frame was queued.</returns>
    public bool Transmit(ReadOnlySpan<byte> frame)
    {
        DeviceLock? deviceLock = _lock;
        if (deviceLock is null || frame.Length == 0 || frame.Length > BufferBytes)
        {
            return false;
        }

        using (deviceLock.Acquire())
        {
            int slot = _transmitTail;
            int next = (slot + 1) % DescriptorCount;
            uint head = _registers.Read32(E1000ERegisters.TransmitDescriptorHead);
            if (next == (int)head)
            {
                return false;
            }

            frame.CopyTo(_transmitBuffers.Span.Slice(slot * BufferBytes, frame.Length));
            Span<E1000ETransmitDescriptor> ring = MemoryMarshal.Cast<byte, E1000ETransmitDescriptor>(_transmitRing.Span);
            ref E1000ETransmitDescriptor descriptor = ref ring[slot];
            descriptor.BufferAddress = _transmitBuffers.PhysicalAddress + (ulong)(slot * BufferBytes);
            descriptor.Length = (ushort)frame.Length;
            descriptor.ChecksumOffset = 0;
            descriptor.Command = E1000ERegisters.TransmitCommandEndOfPacket | E1000ERegisters.TransmitCommandInsertFcs | E1000ERegisters.TransmitCommandReportStatus;
            descriptor.Status = 0;
            descriptor.ChecksumStart = 0;
            descriptor.Special = 0;
            DmaBuffer.WriteBarrier();
            _transmitTail = next;
            _registers.Write32(E1000ERegisters.TransmitDescriptorTail, (uint)next);
            _framesTransmitted++;
            return true;
        }
    }

    /// <summary>
    /// The line handler: reads the interrupt cause register, which clears
    /// it, counts the interrupt and schedules the drain when a frame
    /// arrived, the ring ran low or the link changed. Interrupt context;
    /// allocation-free.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        uint cause = _registers.Read32(E1000ERegisters.InterruptCauseRead);
        _interruptCount++;
        const uint DrainCauses = E1000ERegisters.InterruptReceiveTimer
            | E1000ERegisters.InterruptReceiveDescriptorMinimumThreshold
            | E1000ERegisters.InterruptLinkStatusChange;
        WorkItem? drain = _drainWork;
        if ((cause & DrainCauses) != 0 && drain is not null)
        {
            context.Schedule(drain);
        }
    }

    /// <summary>
    /// The drain: reports a link change, then walks the receive ring from
    /// the tail, handing every complete, error-free frame to the sink and
    /// returning its descriptor to the controller. Idempotent, so the line
    /// handler and the periodic schedule can both run it; a run that finds
    /// nothing costs a few register reads. Thread context on the kit
    /// worker; the lock is held only around the tail update.
    /// </summary>
    internal void Drain()
    {
        NetworkSink? sink = _sink;
        bool up = (_registers.Read32(E1000ERegisters.Status) & E1000ERegisters.StatusLinkUp) != 0;
        if (up != _linkUp)
        {
            _linkUp = up;
            _linkChanges++;
            sink?.LinkChanged(up);
        }

        while (true)
        {
            int next = (_receiveTail + 1) % DescriptorCount;
            Span<E1000EReceiveDescriptor> ring = MemoryMarshal.Cast<byte, E1000EReceiveDescriptor>(_receiveRing.Span);
            byte status = ring[next].Status;
            if ((status & E1000ERegisters.ReceiveStatusDescriptorDone) == 0)
            {
                return;
            }

            // The done flag was observed; the length and the payload the
            // controller wrote before it are read only after the barrier.
            DmaBuffer.ReadBarrier();
            int length = Math.Min((int)ring[next].Length, BufferBytes);
            byte errors = ring[next].Errors;
            if ((status & E1000ERegisters.ReceiveStatusEndOfPacket) != 0 && errors == 0)
            {
                ReadOnlySpan<byte> payload = _receiveBuffers.Span.Slice(next * BufferBytes, length);
                Deliver(sink, payload);
                _framesReceived++;
            }

            ring = MemoryMarshal.Cast<byte, E1000EReceiveDescriptor>(_receiveRing.Span);
            ring[next].Status = 0;
            DmaBuffer.WriteBarrier();
            AdvanceReceiveTail(next);
        }
    }

    /// <summary>
    /// Hands one frame to the sink, fencing whatever the ring does with it:
    /// the kit cancels a work item that throws, which would stop every later
    /// drain and leave the descriptor with the controller for good, so a
    /// delivery that throws is logged and the walk goes on. Thread context
    /// on the kit worker, outside the lock.
    /// </summary>
    private void Deliver(NetworkSink? sink, ReadOnlySpan<byte> payload)
    {
        if (sink is null)
        {
            return;
        }

        try
        {
            sink.Receive(payload);
        }
        catch (Exception exception)
        {
            _binding.Log($"a frame's delivery threw: {exception.Message}");
        }
    }

    /// <summary>Records the new receive tail and hands the descriptor back, under the lock when the probe created it. Thread context on the kit worker.</summary>
    private void AdvanceReceiveTail(int next)
    {
        DeviceLock? deviceLock = _lock;
        if (deviceLock is null)
        {
            _receiveTail = next;
            _registers.Write32(E1000ERegisters.ReceiveDescriptorTail, (uint)next);
            return;
        }

        using (deviceLock.Acquire())
        {
            _receiveTail = next;
            _registers.Write32(E1000ERegisters.ReceiveDescriptorTail, (uint)next);
        }
    }
}
