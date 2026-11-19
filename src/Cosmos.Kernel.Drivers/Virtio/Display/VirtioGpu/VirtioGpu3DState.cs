// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Virtio;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Rendering3D;

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>
/// The <see cref="VirtioGpuState"/> published when the device kept the
/// VIRTIO_GPU_F_VIRGL bit: the same display, plus the ring's
/// <see cref="ICanvas3DFactory"/>, so <see cref="Canvas.GetFullScreen()"/>
/// hands out the virgl canvas instead of the framework one. Adds the 3D
/// half of the device on top of the 2D state: a virgl context on the
/// device's context table, host resources backed by driver-owned DMA
/// memory, the box transfers that move bytes between the two, and
/// SUBMIT_3D for the command streams the engine composes. The payloads
/// dwarf the 2D scratch block's command area, so a dedicated 256 KiB
/// buffer carries every 3D command; the response area, the command slot
/// and the fault tracking are the base's, so the one-command-at-a-time
/// protocol holds across the 2D and the 3D path. Thread context; one
/// caller at a time, as the ring's canvas is.
/// </summary>
internal sealed class VirtioGpu3DState : VirtioGpuState, ICanvas3DFactory
{
    // --- Constants ---

    /// <summary>Bytes of the 3D command buffer: sized for a full frame of virgl commands (shaders, state setup, clears, draws) plus headroom.</summary>
    internal const int Command3DBufferBytes = 256 * 1024;

    /// <summary>The first 3D resource id, above the 2D scanout resource, so the two paths cannot collide on the device's shared resource table.</summary>
    internal const uint First3DResourceId = 10;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private DmaBuffer? _commandBuffer;
    private uint _nextResourceId = First3DResourceId;

    // --- Constructor ---

    /// <summary>
    /// Takes the access, the queues and the scratch block the probe
    /// acquired, as the base does; the framebuffer comes later through the
    /// base's <see cref="VirtioGpuState.SetGeometry"/>. Thread context,
    /// from the probe.
    /// </summary>
    /// <param name="binding">The device's binding, for the log, the delays and the waits.</param>
    /// <param name="access">The kit's access to the device.</param>
    /// <param name="controlQueue">Queue 0, every descriptor free.</param>
    /// <param name="cursorQueue">Queue 1, or null when the device has none; not driven.</param>
    /// <param name="scratch">One page, the base's command and response area.</param>
    internal VirtioGpu3DState(DeviceBinding binding, VirtioAccess access, Virtqueue controlQueue, Virtqueue? cursorQueue, DmaBuffer scratch)
        : base(binding, access, controlQueue, cursorQueue, scratch)
    {
        _binding = binding;
    }

    // --- ICanvas3DFactory ---

    /// <summary>Creates the 3D canvas over <paramref name="display"/>, which is the display this state published. Thread context.</summary>
    /// <param name="display">The display the facet was found on.</param>
    /// <returns>The 3D canvas, which draws on <paramref name="display"/>.</returns>
    public Canvas3D CreateCanvas3D(DisplayDevice display)
    {
        return new Virtio3DCanvas(this, display);
    }

    // --- Internal methods for the engine ---

    /// <summary>Writes a line into the binding's log. Thread context.</summary>
    internal void Log(string message)
    {
        _binding.Log(message);
    }

    /// <summary>Allocates the next free 3D resource id. Thread context.</summary>
    internal uint ReserveResourceId() => _nextResourceId++;

    /// <summary>Allocates physically contiguous DMA memory on the binding's ledger. Thread context.</summary>
    internal DmaBuffer AllocateBacking(int bytes, int alignment)
    {
        return _binding.AllocateDma(bytes, alignment);
    }

    /// <summary>
    /// CTX_CREATE: a plain virgl context on the device's context table. All
    /// SUBMIT_3D streams of the canvas carry its id in the header's CtxId
    /// field. Thread context.
    /// </summary>
    /// <param name="ctxId">The context id, nonzero.</param>
    /// <param name="debugName">The advisory name the host logs.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool CreateContext(uint ctxId, string debugName)
    {
        VirtioGpuCtxCreate command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd3D.CtxCreate,
                CtxId = ctxId,
            },
            NumLen = 0,
            ContextInit = 0,
        };

        // The debug name is advisory; keep it short enough for the 64-byte
        // device-side field and NUL-terminate.
        uint count = 0;
        while (count < VirtioGpuCtxCreate.NameSize - 1 && count < debugName.Length)
        {
            command.DebugName[(int)count] = (byte)debugName[(int)count];
            count++;
        }
        command.DebugName[(int)count] = 0;
        command.NumLen = count;

        return Send3D(in command, "CTX_CREATE");
    }

    /// <summary>CTX_DESTROY: drops the context and every object it held. Thread context.</summary>
    /// <param name="ctxId">The context id.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool DestroyContext(uint ctxId)
    {
        VirtioGpuCtxDestroy command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd3D.CtxDestroy,
                CtxId = ctxId,
            },
        };
        return Send3D(in command, "CTX_DESTROY");
    }

    /// <summary>
    /// RESOURCE_CREATE_3D: a host resource in virgl terms; the guest hands
    /// over no memory here, <see cref="AttachBacking"/> follows before data
    /// crosses. Thread context.
    /// </summary>
    /// <param name="resourceId">The resource id, from <see cref="ReserveResourceId"/>.</param>
    /// <param name="target">One of the PIPE_TEXTURE_* targets.</param>
    /// <param name="format">One of the virgl_formats values.</param>
    /// <param name="bind">PIPE_BIND_* bits.</param>
    /// <param name="width">Width in pixels, or bytes for a buffer.</param>
    /// <param name="height">Height in pixels; 1 for a buffer.</param>
    /// <param name="depth">Depth in pixels; 1 outside 3D textures.</param>
    /// <param name="arraySize">Array size; 1 outside arrays.</param>
    /// <param name="lastLevel">The last mip level; 0 for a single-level resource.</param>
    /// <param name="nrSamples">Samples per pixel; 1 for unsampled resources.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool Create3DResource(uint resourceId, uint target, uint format, uint bind,
                                   uint width, uint height, uint depth, uint arraySize,
                                   uint lastLevel, uint nrSamples)
    {
        VirtioGpuResourceCreate3D command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd3D.ResourceCreate3D,
            },
            ResourceId = resourceId,
            Target = target,
            Format = format,
            Bind = bind,
            Width = width,
            Height = height,
            Depth = depth,
            ArraySize = arraySize,
            LastLevel = lastLevel,
            NrSamples = nrSamples,
            Flags = 0,
        };

        // 68 bytes: one past the 2D scratch block's command area, so the
        // command buffer carries it.
        return Send3D(in command, "RESOURCE_CREATE_3D");
    }

    /// <summary>RESOURCE_UNREF: destroys a host resource. Thread context.</summary>
    /// <param name="resourceId">The resource id.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool Destroy3DResource(uint resourceId)
    {
        VirtioGpuResourceUnref command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd.ResourceUnref,
            },
            ResourceId = resourceId,
            Padding = 0,
        };
        return Send3D(in command, "RESOURCE_UNREF");
    }

    /// <summary>
    /// RESOURCE_ATTACH_BACKING: binds a physically contiguous DMA buffer as
    /// the resource's one memory entry, a three-descriptor chain. Thread
    /// context.
    /// </summary>
    /// <param name="resourceId">The resource id.</param>
    /// <param name="backing">The DMA memory the host DMA's from and into.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool AttachBacking(uint resourceId, DmaBuffer backing)
    {
        VirtioGpuResourceAttachBacking command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd.ResourceAttachBacking,
            },
            ResourceId = resourceId,
            NrEntries = 1,
        };
        VirtioGpuMemEntry entry = new()
        {
            Addr = backing.PhysicalAddress,
            Length = (uint)backing.Length,
            Padding = 0,
        };
        return SendCommandWithEntry(in command, in entry, VirtioGpuCmd.RespOkNoData, "RESOURCE_ATTACH_BACKING");
    }

    /// <summary>CTX_ATTACH_RESOURCE: puts a resource into the context's table so SUBMIT_3D streams can reference it. Thread context.</summary>
    /// <param name="ctxId">The context id.</param>
    /// <param name="resourceId">The resource id.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool AttachResourceToContext(uint ctxId, uint resourceId)
    {
        VirtioGpuCtxResource command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd3D.CtxAttachResource,
                CtxId = ctxId,
            },
            ResourceId = resourceId,
            Padding = 0,
        };
        return Send3D(in command, "CTX_ATTACH_RESOURCE");
    }

    /// <summary>CTX_DETACH_RESOURCE: takes a resource out of the context's table. Thread context.</summary>
    /// <param name="ctxId">The context id.</param>
    /// <param name="resourceId">The resource id.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool DetachResourceFromContext(uint ctxId, uint resourceId)
    {
        VirtioGpuCtxResource command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd3D.CtxDetachResource,
                CtxId = ctxId,
            },
            ResourceId = resourceId,
            Padding = 0,
        };
        return Send3D(in command, "CTX_DETACH_RESOURCE");
    }

    /// <summary>
    /// TRANSFER_TO_HOST_3D / TRANSFER_FROM_HOST_3D: copies a box between
    /// the guest backing of a resource and the host copy, up with
    /// <paramref name="toHost"/> and down without it; the present path
    /// reads the rendered color target back with the argument false.
    /// Thread context.
    /// </summary>
    /// <param name="resourceId">The resource id.</param>
    /// <param name="toHost">True to copy the backing into the host copy, false to read the host copy into the backing.</param>
    /// <param name="offset">Byte offset of the box's origin in the backing.</param>
    /// <param name="x">Left edge of the box.</param>
    /// <param name="y">Top edge of the box.</param>
    /// <param name="width">Width of the box.</param>
    /// <param name="height">Height of the box.</param>
    /// <param name="stride">Bytes per row in the backing; 0 lets the host take the resource's stride.</param>
    /// <returns>False when the device refused or did not answer.</returns>
    internal bool TransferBox3D(uint resourceId, bool toHost, ulong offset,
                                uint x, uint y, uint width, uint height, uint stride)
    {
        VirtioGpuTransferHost3D command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = toHost ? VirtioGpuCmd3D.TransferToHost3D : VirtioGpuCmd3D.TransferFromHost3D,
            },
            Box = new VirtioGpuBox
            {
                X = x,
                Y = y,
                Z = 0,
                Width = width,
                Height = height,
                Depth = 1,
            },
            Offset = offset,
            ResourceId = resourceId,
            Level = 0,
            Stride = stride,
            LayerStride = 0,
        };

        // 72 bytes: one past the 2D scratch block's command area, so the
        // command buffer carries it.
        return Send3D(in command, toHost ? "TRANSFER_TO_HOST_3D" : "TRANSFER_FROM_HOST_3D");
    }

    /// <summary>
    /// SUBMIT_3D: pushes a virgl command stream, the dwords the engine
    /// composed, into the context. The device consumes the stream
    /// synchronously before answering, so the caller can read back
    /// transferred resources right after this returns. Thread context.
    /// </summary>
    /// <param name="ctxId">The context id.</param>
    /// <param name="stream">The command stream dwords.</param>
    /// <returns>False when the stream is empty, does not fit the command buffer, or the device refused or did not answer.</returns>
    internal bool Submit3D(uint ctxId, ReadOnlySpan<uint> stream)
    {
        if (stream.Length == 0)
        {
            return true;
        }

        DmaBuffer? commandBuffer = _commandBuffer;
        if (commandBuffer is null)
        {
            _binding.Log("SUBMIT_3D: no command buffer");
            return false;
        }

        int submitBytes = Unsafe.SizeOf<VirtioGpuCmdSubmit>();
        int streamBytes = stream.Length * 4;
        if (streamBytes > commandBuffer.Length - submitBytes)
        {
            _binding.Log("SUBMIT_3D: the stream does not fit the command buffer");
            return false;
        }

        VirtioGpuCmdSubmit command = new()
        {
            Hdr = new VirtioGpuCtrlHdr
            {
                Type = VirtioGpuCmd3D.Submit3D,
                CtxId = ctxId,
            },
            Size = (uint)streamBytes,
            Padding = 0,
        };

        MemoryMarshal.Write(commandBuffer.Span, in command);
        stream.CopyTo(MemoryMarshal.Cast<byte, uint>(commandBuffer.Span.Slice(submitBytes)));
        return SendBufferCommand(commandBuffer, submitBytes + streamBytes, VirtioGpuCmd.RespOkNoData, "SUBMIT_3D");
    }

    // --- Private methods ---

    /// <summary>
    /// Allocates the 3D command buffer on the first use, then sends one
    /// command whose bytes sit at its offset 0. Thread context.
    /// </summary>
    /// <typeparam name="T">The command struct, a header followed by its payload.</typeparam>
    /// <param name="command">The command; copied into the command buffer.</param>
    /// <param name="name">The command's name for the log.</param>
    /// <returns>False when the buffer cannot be allocated or the device refused or did not answer.</returns>
    private bool Send3D<T>(in T command, string name) where T : unmanaged
    {
        DmaBuffer? commandBuffer = _commandBuffer;
        if (commandBuffer is null)
        {
            commandBuffer = _binding.AllocateDma(Command3DBufferBytes, VirtioGpuState.PageBytes);
            _commandBuffer = commandBuffer;
        }

        int commandBytes = Unsafe.SizeOf<T>();
        if (commandBytes > commandBuffer.Length)
        {
            _binding.Log($"{name}: the command does not fit the command buffer");
            return false;
        }

        MemoryMarshal.Write(commandBuffer.Span, in command);
        return SendBufferCommand(commandBuffer, commandBytes, VirtioGpuCmd.RespOkNoData, name);
    }
}
