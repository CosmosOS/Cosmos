// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="VirtioGpuDriver"/> holds for one bound device,
/// hung off <see cref="DeviceBinding.DriverState"/>, and the display it
/// publishes: the access, the control queue, the scratch DMA block the
/// commands and responses go through, the framebuffer the host scans out
/// from, the kit lock and event, and the counters the Graphic suite reads
/// through <c>DisplayDevice.TryGetFacet</c>. Commands are synchronous and
/// one is in flight at a time: <see cref="SendCommand{T}"/> takes the
/// <see cref="DeviceLock"/> only around the ring operations and never
/// across a wait, so <see cref="Flush"/> may be entered from any thread;
/// <see cref="OnInterrupt"/> runs in interrupt context and only counts and
/// signals the event. Every struct goes through the scratch span with
/// <c>MemoryMarshal</c>, never a pointer.
/// </summary>
public sealed class VirtioGpuState : IDisplay
{
    // --- Constants ---

    /// <summary>The control queue's index (controlq).</summary>
    internal const ushort ControlQueue = 0;

    /// <summary>The cursor queue's index (cursorq).</summary>
    internal const ushort CursorQueue = 1;

    /// <summary>Bytes of one page: the scratch block's size and the framebuffer's alignment. The kit exposes no page size.</summary>
    internal const int PageBytes = 4096;

    /// <summary>Offset of the command buffer in the scratch block.</summary>
    internal const int CommandOffset = 0;

    /// <summary>Bytes reserved for a command; the largest 2D command is TRANSFER_TO_HOST_2D at 56.</summary>
    internal const int CommandBytes = 64;

    /// <summary>Offset of the memory entry a RESOURCE_ATTACH_BACKING chain carries.</summary>
    internal const int MemoryEntryOffset = 64;

    /// <summary>Offset of the response buffer, sized for the display info response.</summary>
    internal const int ResponseOffset = 128;

    /// <summary>The id of the one host resource, the scanout's; 0 is VIRTIO_GPU_INVALID_RES_ID.</summary>
    internal const uint ScanoutResourceId = 1;

    /// <summary>The scanout the resource is bound to.</summary>
    internal const uint ScanoutId = 0;

    /// <summary>Bytes per pixel of the B8G8R8X8 resource.</summary>
    internal const int BytesPerPixel = 4;

    /// <summary>The largest scanout side taken from the device: a rectangle beyond it is not a display but a bogus answer, and the bound keeps the frame's byte count in an int (16384 squared times 4 is 1 GiB).</summary>
    internal const int MaxDimension = 16384;

    /// <summary>Bits per pixel the display reports.</summary>
    internal const int BitsPerPixel = 32;

    /// <summary>How long a command may take before the device is declared faulted.</summary>
    internal const uint CommandTimeoutMilliseconds = 1000;

    /// <summary>Pause between two used ring reads when the queue has no interrupt, and between two tries for the command slot.</summary>
    internal const uint PollMicroseconds = 10;

    /// <summary>Milliseconds in a second, for the deadline arithmetic.</summary>
    private const long MillisecondsPerSecond = 1000;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly VirtioAccess _access;
    private readonly Virtqueue _controlQueue;
    private readonly Virtqueue? _cursorQueue;
    private readonly DmaBuffer _scratch;
    private DmaBuffer? _framebuffer;
    private DeviceLock? _lock;
    private DeviceEvent? _event;
    private DisplaySink? _sink;
    private int _width;
    private int _height;
    private int _scanoutCount;

    /// <summary>Set under the lock while a command's chain is on the ring.</summary>
    private bool _busy;

    /// <summary>The in-flight chain's memory entry descriptor, valid when <see cref="_inFlightHasEntry"/>.</summary>
    private ushort _inFlightEntry;

    /// <summary>The in-flight chain's response descriptor.</summary>
    private ushort _inFlightResponse;

    /// <summary>True when the in-flight chain has a memory entry between the command and the response.</summary>
    private bool _inFlightHasEntry;

    private volatile bool _faulted;
    private volatile bool _faultLogged;
    private volatile bool _unexpectedLogged;
    private volatile bool _hasInterrupt;
    private volatile bool _isPolling;
    private volatile int _commandsSent;
    private volatile int _flushCount;
    private volatile int _interruptCount;

    // --- Constructor ---

    /// <summary>
    /// Takes the access, the queues and the scratch block the probe
    /// acquired; the framebuffer comes later through
    /// <see cref="SetGeometry"/> once the display's size is known. Thread
    /// context, from the probe.
    /// </summary>
    /// <param name="binding">The device's binding, for the log, the delays and the waits.</param>
    /// <param name="access">The kit's access to the device.</param>
    /// <param name="controlQueue">Queue 0, every descriptor free.</param>
    /// <param name="cursorQueue">Queue 1, or null when the device has none; not driven.</param>
    /// <param name="scratch">One page: the command at <see cref="CommandOffset"/>, the memory entry at <see cref="MemoryEntryOffset"/>, the response at <see cref="ResponseOffset"/>.</param>
    internal VirtioGpuState(DeviceBinding binding, VirtioAccess access, Virtqueue controlQueue, Virtqueue? cursorQueue, DmaBuffer scratch)
    {
        _binding = binding;
        _access = access;
        _controlQueue = controlQueue;
        _cursorQueue = cursorQueue;
        _scratch = scratch;
    }

    // --- IDisplay ---

    /// <inheritdoc/>
    public string Name => "virtio-gpu";

    /// <inheritdoc/>
    public DisplayMode Mode => new(_width, _height, _width * BytesPerPixel, BitsPerPixel);

    /// <inheritdoc/>
    public DeviceRegion? Framebuffer => _framebuffer?.Region;

    /// <summary>
    /// Clips the rectangle to the mode, then copies it into the host
    /// resource (TRANSFER_TO_HOST_2D) and makes it visible (RESOURCE_FLUSH),
    /// both synchronous. Returns at once, after logging once, when the
    /// device stopped answering. Thread context; any thread.
    /// </summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public void Flush(int x, int y, int width, int height)
    {
        int modeWidth = _width;
        int modeHeight = _height;
        if (x < 0)
        {
            width += x;
            x = 0;
        }

        if (y < 0)
        {
            height += y;
            y = 0;
        }

        if (x >= modeWidth || y >= modeHeight)
        {
            return;
        }

        width = Math.Min(width, modeWidth - x);
        height = Math.Min(height, modeHeight - y);
        if (width <= 0 || height <= 0 || _framebuffer is null)
        {
            return;
        }

        if (_faulted)
        {
            if (!_faultLogged)
            {
                _faultLogged = true;
                _binding.Log("the device stopped answering; flushes are dropped");
            }

            return;
        }

        _flushCount++;
        VirtioGpuRect rect = new()
        {
            X = (uint)x,
            Y = (uint)y,
            Width = (uint)width,
            Height = (uint)height,
        };
        if (!TransferToHost2D(in rect))
        {
            return;
        }

        ResourceFlush(in rect);
    }

    // --- Properties the suite reads ---

    /// <summary>Width of the scanout in pixels. Any context.</summary>
    public int Width => _width;

    /// <summary>Height of the scanout in pixels. Any context.</summary>
    public int Height => _height;

    /// <summary>How many scanouts the configuration space reports; 0 read as 1. Any context.</summary>
    public int ScanoutCount
    {
        get => _scanoutCount;
        internal set => _scanoutCount = value;
    }

    /// <summary>True when the control queue's source is connected to <see cref="OnInterrupt"/>, so a command waits on the event. Any context.</summary>
    public bool HasInterrupt
    {
        get => _hasInterrupt;
        internal set => _hasInterrupt = value;
    }

    /// <summary>True when the control queue has no interrupt, so a command polls the used ring. Any context.</summary>
    public bool IsPolling
    {
        get => _isPolling;
        internal set => _isPolling = value;
    }

    /// <summary>True once a command went unanswered for <see cref="CommandTimeoutMilliseconds"/>; every later command is dropped. Any context.</summary>
    public bool IsFaulted => _faulted;

    /// <summary>How many commands were put on the control queue. Any context.</summary>
    public int CommandsSent => _commandsSent;

    /// <summary>How many non-empty flushes reached the device. Any context.</summary>
    public int FlushCount => _flushCount;

    /// <summary>How many times the control queue's source ran <see cref="OnInterrupt"/>. Any context.</summary>
    public int InterruptCount => _interruptCount;

    /// <summary>True when the kit negotiated VIRTIO_F_VERSION_1 with the device. Any context.</summary>
    public bool Version1Negotiated => _access.Version1Negotiated;

    // --- Internal properties the probe sets ---

    /// <summary>The lock every ring operation runs under; set by the probe before the first command.</summary>
    internal DeviceLock? Lock
    {
        get => _lock;
        set => _lock = value;
    }

    /// <summary>The event the handler signals and a command waits on; set by the probe before the source is connected.</summary>
    internal DeviceEvent? Event
    {
        get => _event;
        set => _event = value;
    }

    /// <summary>The sink mode changes would go to; set by the probe when the display is published. The mode never changes.</summary>
    internal DisplaySink? Sink
    {
        get => _sink;
        set => _sink = value;
    }

    /// <summary>True when the device offered a cursor queue and the probe created it.</summary>
    internal bool HasCursorQueue => _cursorQueue is not null;

    // --- Internal methods for the probe ---

    /// <summary>Records the display's size and the DMA memory the host scans out from. Thread context, from the probe, before the display is published.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="framebuffer">Page-aligned DMA memory of at least width times height times <see cref="BytesPerPixel"/> bytes.</param>
    internal void SetGeometry(int width, int height, DmaBuffer framebuffer)
    {
        _width = width;
        _height = height;
        _framebuffer = framebuffer;
    }

    /// <summary>
    /// GET_DISPLAY_INFO: the first scanout's rectangle. Thread context, from
    /// the probe.
    /// </summary>
    /// <param name="width">The scanout's width, when the answer is usable.</param>
    /// <param name="height">The scanout's height, when the answer is usable.</param>
    /// <returns>False when the command failed, the scanout is disabled, its rectangle is empty or a side exceeds <see cref="MaxDimension"/>; <see cref="IsFaulted"/> tells a fault from an unusable answer.</returns>
    internal bool TryGetDisplayInfo(out int width, out int height)
    {
        width = 0;
        height = 0;
        VirtioGpuCtrlHdr command = new()
        {
            Type = VirtioGpuCmd.GetDisplayInfo,
        };
        if (!SendCommand(in command, VirtioGpuCmd.RespOkDisplayInfo, "GET_DISPLAY_INFO"))
        {
            return false;
        }

        VirtioGpuRespDisplayInfo info = MemoryMarshal.Read<VirtioGpuRespDisplayInfo>(_scratch.Span.Slice(ResponseOffset));
        VirtioGpuDisplayOne first = info.PModes[0];
        if (first.Enabled == 0 || first.Rect.Width == 0 || first.Rect.Height == 0)
        {
            return false;
        }

        if (first.Rect.Width > MaxDimension || first.Rect.Height > MaxDimension)
        {
            return false;
        }

        width = (int)first.Rect.Width;
        height = (int)first.Rect.Height;
        return true;
    }

    /// <summary>RESOURCE_CREATE_2D: the scanout resource, B8G8R8X8, sized as <see cref="SetGeometry"/> recorded. Thread context, from the probe.</summary>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool CreateScanoutResource()
    {
        VirtioGpuResourceCreate2D command = new()
        {
            Hdr = new VirtioGpuCtrlHdr { Type = VirtioGpuCmd.ResourceCreate2D },
            ResourceId = ScanoutResourceId,
            Format = VirtioGpuCmd.FormatB8G8R8X8Unorm,
            Width = (uint)_width,
            Height = (uint)_height,
        };
        return SendCommand(in command, VirtioGpuCmd.RespOkNoData, "RESOURCE_CREATE_2D");
    }

    /// <summary>RESOURCE_ATTACH_BACKING: the framebuffer as the resource's one memory entry, a three-descriptor chain. Thread context, from the probe.</summary>
    /// <returns>False when the device refused or did not answer, or no framebuffer was recorded.</returns>
    internal bool AttachFramebuffer()
    {
        DmaBuffer? framebuffer = _framebuffer;
        if (framebuffer is null)
        {
            return false;
        }

        VirtioGpuResourceAttachBacking command = new()
        {
            Hdr = new VirtioGpuCtrlHdr { Type = VirtioGpuCmd.ResourceAttachBacking },
            ResourceId = ScanoutResourceId,
            NrEntries = 1,
        };
        VirtioGpuMemEntry entry = new()
        {
            Addr = framebuffer.PhysicalAddress,
            Length = (uint)((long)_width * _height * BytesPerPixel),
            Padding = 0,
        };
        return SendCommandCore(in command, true, in entry, VirtioGpuCmd.RespOkNoData, "RESOURCE_ATTACH_BACKING");
    }

    /// <summary>SET_SCANOUT: scanout 0 shows the whole resource. Thread context, from the probe.</summary>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool SetScanout()
    {
        VirtioGpuSetScanout command = new()
        {
            Hdr = new VirtioGpuCtrlHdr { Type = VirtioGpuCmd.SetScanout },
            Rect = new VirtioGpuRect { X = 0, Y = 0, Width = (uint)_width, Height = (uint)_height },
            ScanoutId = ScanoutId,
            ResourceId = ScanoutResourceId,
        };
        return SendCommand(in command, VirtioGpuCmd.RespOkNoData, "SET_SCANOUT");
    }

    /// <summary>
    /// A source's handler: counts the interrupt and wakes the command
    /// waiting on the event. Interrupt context; allocation-free.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        _interruptCount++;
        DeviceEvent? evt = _event;
        if (evt is not null)
        {
            context.Signal(evt);
        }
    }

    /// <summary>
    /// Sends one command and waits for its response: a two-descriptor
    /// chain, the command out and the response in. Thread context; any
    /// thread; never called with the lock held.
    /// </summary>
    /// <typeparam name="T">The command struct, a header followed by its payload.</typeparam>
    /// <param name="command">The command; copied into the scratch block.</param>
    /// <param name="expectedResponseType">The response type that means success.</param>
    /// <param name="name">The command's name for the log.</param>
    /// <returns>True when the device answered with <paramref name="expectedResponseType"/>.</returns>
    internal bool SendCommand<T>(in T command, uint expectedResponseType, string name) where T : unmanaged
    {
        VirtioGpuMemEntry none = default;
        return SendCommandCore(in command, false, in none, expectedResponseType, name);
    }

    // --- Private methods ---

    /// <summary>TRANSFER_TO_HOST_2D of one rectangle of the scanout resource. Thread context.</summary>
    private bool TransferToHost2D(in VirtioGpuRect rect)
    {
        VirtioGpuTransferToHost2D command = new()
        {
            Hdr = new VirtioGpuCtrlHdr { Type = VirtioGpuCmd.TransferToHost2D },
            Rect = rect,
            Offset = (ulong)rect.Y * (ulong)(_width * BytesPerPixel) + (ulong)rect.X * BytesPerPixel,
            ResourceId = ScanoutResourceId,
            Padding = 0,
        };
        return SendCommand(in command, VirtioGpuCmd.RespOkNoData, "TRANSFER_TO_HOST_2D");
    }

    /// <summary>RESOURCE_FLUSH of one rectangle of the scanout resource. Thread context.</summary>
    private bool ResourceFlush(in VirtioGpuRect rect)
    {
        VirtioGpuResourceFlush command = new()
        {
            Hdr = new VirtioGpuCtrlHdr { Type = VirtioGpuCmd.ResourceFlush },
            Rect = rect,
            ResourceId = ScanoutResourceId,
            Padding = 0,
        };
        return SendCommand(in command, VirtioGpuCmd.RespOkNoData, "RESOURCE_FLUSH");
    }

    /// <summary>
    /// The one command path. Takes the slot: under the lock, when no command
    /// is in flight, marks the state busy, writes the command (and the
    /// memory entry when <paramref name="withEntry"/>) into the scratch
    /// block, fills the chain, submits and notifies; when a command is in
    /// flight, releases the lock, pauses and tries again. Then waits without
    /// the lock, up to <see cref="CommandTimeoutMilliseconds"/>, taking the
    /// used ring under the lock each round: the chain's head ends the wait,
    /// a foreign id is a protocol fault (only one command is ever in
    /// flight), nothing used waits on the event with an interrupt or pauses
    /// without one. A timeout or a foreign id marks the state faulted. Only
    /// after the used element is taken is the response header read. Thread
    /// context; any thread.
    /// </summary>
    private bool SendCommandCore<T>(in T command, bool withEntry, in VirtioGpuMemEntry entry, uint expectedResponseType, string name) where T : unmanaged
    {
        DeviceLock? deviceLock = _lock;
        if (deviceLock is null || _faulted)
        {
            return false;
        }

        if (Unsafe.SizeOf<T>() > CommandBytes)
        {
            _binding.Log($"{name}: the command does not fit the scratch buffer");
            return false;
        }

        // 1. The slot and the chain, in one scope.
        ushort head = 0;
        bool submitted = false;
        bool exhausted = false;
        while (!submitted)
        {
            using (deviceLock.Acquire())
            {
                if (!_busy)
                {
                    _busy = true;
                    submitted = TrySubmitLocked(in command, withEntry, in entry, out head);
                    if (!submitted)
                    {
                        _busy = false;
                        exhausted = true;
                    }
                }
            }

            if (exhausted)
            {
                _binding.Log($"{name}: no free descriptor on the control queue");
                return false;
            }

            if (!submitted)
            {
                _binding.Delay(PollMicroseconds);
            }
        }

        _commandsSent++;

        // 2. The wait, without the lock.
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * CommandTimeoutMilliseconds;
        bool taken = false;
        bool foreign = false;
        while (true)
        {
            using (deviceLock.Acquire())
            {
                if (_controlQueue.TryTakeUsed(out ushort id, out _))
                {
                    if (id == head)
                    {
                        FreeChainLocked(head);
                        _busy = false;
                        taken = true;
                    }
                    else
                    {
                        _controlQueue.FreeDescriptor(id);
                        foreign = true;
                    }
                }
            }

            if (taken || foreign)
            {
                break;
            }

            long now = Stopwatch.GetTimestamp();
            if (now >= deadline)
            {
                break;
            }

            DeviceEvent? evt = _event;
            if (_hasInterrupt && evt is not null)
            {
                long remaining = (deadline - now) * MillisecondsPerSecond / Stopwatch.Frequency;
                _binding.Wait(evt, (uint)Math.Max(1, remaining));
            }
            else
            {
                _binding.Delay(PollMicroseconds);
            }
        }

        if (!taken)
        {
            using (deviceLock.Acquire())
            {
                _busy = false;
                _faulted = true;
            }

            if (foreign && !_unexpectedLogged)
            {
                _unexpectedLogged = true;
                _binding.Log($"{name}: unexpected used element, the device is faulted");
            }

            return false;
        }

        // 3. The response.
        VirtioGpuCtrlHdr response = MemoryMarshal.Read<VirtioGpuCtrlHdr>(_scratch.Span.Slice(ResponseOffset));
        if (response.Type != expectedResponseType)
        {
            _binding.Log($"{name}: response type 0x{response.Type:X} instead of 0x{expectedResponseType:X}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Allocates the chain, writes the command, the entry and a cleared
    /// response area into the scratch block, fills the descriptors, submits
    /// the head and notifies. Under the lock; allocation-free.
    /// </summary>
    /// <returns>False when the queue has too few free descriptors; nothing was submitted.</returns>
    private bool TrySubmitLocked<T>(in T command, bool withEntry, in VirtioGpuMemEntry entry, out ushort head) where T : unmanaged
    {
        head = 0;
        if (!_controlQueue.TryAllocateDescriptor(out ushort commandIndex))
        {
            return false;
        }

        ushort entryIndex = 0;
        if (withEntry && !_controlQueue.TryAllocateDescriptor(out entryIndex))
        {
            _controlQueue.FreeDescriptor(commandIndex);
            return false;
        }

        if (!_controlQueue.TryAllocateDescriptor(out ushort responseIndex))
        {
            _controlQueue.FreeDescriptor(commandIndex);
            if (withEntry)
            {
                _controlQueue.FreeDescriptor(entryIndex);
            }

            return false;
        }

        int responseBytes = Unsafe.SizeOf<VirtioGpuRespDisplayInfo>();
        Span<byte> scratch = _scratch.Span;
        scratch.Slice(CommandOffset, CommandBytes).Clear();
        MemoryMarshal.Write(scratch.Slice(CommandOffset), in command);
        if (withEntry)
        {
            MemoryMarshal.Write(scratch.Slice(MemoryEntryOffset), in entry);
        }

        scratch.Slice(ResponseOffset, responseBytes).Clear();

        ulong physical = _scratch.PhysicalAddress;
        ushort afterCommand = withEntry ? entryIndex : responseIndex;
        _controlQueue.SetDescriptor(commandIndex, physical + CommandOffset, (uint)Unsafe.SizeOf<T>(), VirtqueueDescriptorFlags.Next, afterCommand);
        if (withEntry)
        {
            _controlQueue.SetDescriptor(entryIndex, physical + MemoryEntryOffset, (uint)Unsafe.SizeOf<VirtioGpuMemEntry>(), VirtqueueDescriptorFlags.Next, responseIndex);
        }

        _controlQueue.SetDescriptor(responseIndex, physical + ResponseOffset, (uint)responseBytes, VirtqueueDescriptorFlags.Write);
        _inFlightHasEntry = withEntry;
        _inFlightEntry = entryIndex;
        _inFlightResponse = responseIndex;
        _controlQueue.Submit(commandIndex);
        _controlQueue.Notify();
        head = commandIndex;
        return true;
    }

    /// <summary>Frees the in-flight chain's descriptors. Under the lock; allocation-free.</summary>
    private void FreeChainLocked(ushort head)
    {
        _controlQueue.FreeDescriptor(head);
        if (_inFlightHasEntry)
        {
            _controlQueue.FreeDescriptor(_inFlightEntry);
        }

        _controlQueue.FreeDescriptor(_inFlightResponse);
        _inFlightHasEntry = false;
    }
}
