// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>
/// The virtio-gpu 3D / virgl command opcodes and on-wire structs (virtio
/// specification section 5.7.6), for <see cref="VirtioGpu3DState"/>. Layouts
/// are sequential with no padding so a <c>MemoryMarshal.Write</c> into DMA
/// memory is the wire format; every field is little-endian on the
/// architectures the kernel runs on. The 2D opcodes live in
/// <see cref="VirtioGpuCmd"/>; the response types are shared. Constants only;
/// any context.
/// </summary>
internal static class VirtioGpuCmd3D
{
    /// <summary>VIRTIO_GPU_CMD_CTX_CREATE: a virgl (or Venus) context.</summary>
    public const uint CtxCreate = 0x0200;

    /// <summary>VIRTIO_GPU_CMD_CTX_DESTROY: drops the context and its objects.</summary>
    public const uint CtxDestroy = 0x0201;

    /// <summary>VIRTIO_GPU_CMD_CTX_ATTACH_RESOURCE: puts a resource into the context's table.</summary>
    public const uint CtxAttachResource = 0x0202;

    /// <summary>VIRTIO_GPU_CMD_CTX_DETACH_RESOURCE: takes a resource out of the context's table.</summary>
    public const uint CtxDetachResource = 0x0203;

    /// <summary>VIRTIO_GPU_CMD_RESOURCE_CREATE_3D: a host resource in virgl terms.</summary>
    public const uint ResourceCreate3D = 0x0204;

    /// <summary>VIRTIO_GPU_CMD_TRANSFER_TO_HOST_3D: guest backing into the host copy.</summary>
    public const uint TransferToHost3D = 0x0205;

    /// <summary>VIRTIO_GPU_CMD_TRANSFER_FROM_HOST_3D: the host copy into the guest backing.</summary>
    public const uint TransferFromHost3D = 0x0206;

    /// <summary>VIRTIO_GPU_CMD_SUBMIT_3D: a virgl command stream for the context.</summary>
    public const uint Submit3D = 0x0207;

    /// <summary>VIRTIO_GPU_CONTEXT_INIT_CAPSET_ID_VIRGL: the context_init bit selecting the plain virgl capset.</summary>
    public const uint ContextInitVirgl = 1u << 0;
}

/// <summary>
/// virtio_gpu_ctx_create (virtio 5.7.6.2): the context id rides the header's
/// CtxId; <see cref="ContextInit"/> selects the host context subtype and
/// <see cref="DebugName"/> is advisory, <see cref="NumLen"/> counting its
/// valid bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuCtxCreate
{
    /// <summary>Bytes the debug name field holds.</summary>
    public const int NameSize = 64;

    /// <summary>The header, type CTX_CREATE, CtxId = the context.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>How many bytes of <see cref="DebugName"/> are valid.</summary>
    public uint NumLen;

    /// <summary>VIRTIO_GPU_CONTEXT_INIT_CAPSET_ID_* bits; 0 for the plain virgl context.</summary>
    public uint ContextInit;

    /// <summary>The advisory debug name, NUL-terminated.</summary>
    public VirtioGpuDebugName DebugName;
}

/// <summary>The <see cref="VirtioGpuCtxCreate.NameSize"/> bytes of a context create's debug name, in place of a fixed buffer.</summary>
[InlineArray(VirtioGpuCtxCreate.NameSize)]
internal struct VirtioGpuDebugName
{
    /// <summary>The first byte; the attribute repeats it.</summary>
    private byte _element0;
}

/// <summary>
/// virtio_gpu_ctx_destroy (virtio 5.7.6.3): the context id rides the
/// header's CtxId.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuCtxDestroy
{
    /// <summary>The header, type CTX_DESTROY, CtxId = the context.</summary>
    public VirtioGpuCtrlHdr Hdr;
}

/// <summary>
/// virtio_gpu_ctx_attach_resource / ctx_detach_resource (virtio 5.7.6.4):
/// the context id rides the header's CtxId, the resource follows.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuCtxResource
{
    /// <summary>The header, type CTX_ATTACH_RESOURCE or CTX_DETACH_RESOURCE, CtxId = the context.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The resource to bind into or drop from the context's table.</summary>
    public uint ResourceId;

    /// <summary>Reserved.</summary>
    public uint Padding;
}

/// <summary>
/// virtio_gpu_resource_create_3d (virtio 5.7.6.5): a host-side 3D resource;
/// the target, format and bind fields use the virgl wire enums
/// (<see cref="Virgl"/>). The guest hands over no memory here;
/// RESOURCE_ATTACH_BACKING follows before data crosses.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceCreate3D
{
    /// <summary>The header, type RESOURCE_CREATE_3D.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The resource id the driver chose; 0 is reserved.</summary>
    public uint ResourceId;

    /// <summary>One of the PIPE_TEXTURE_* targets.</summary>
    public uint Target;

    /// <summary>One of the virgl_formats values.</summary>
    public uint Format;

    /// <summary>PIPE_BIND_* bits.</summary>
    public uint Bind;

    /// <summary>Width in pixels, or bytes for a buffer.</summary>
    public uint Width;

    /// <summary>Height in pixels; 1 for a buffer.</summary>
    public uint Height;

    /// <summary>Depth in pixels; 1 outside 3D textures.</summary>
    public uint Depth;

    /// <summary>Array size; 1 outside arrays.</summary>
    public uint ArraySize;

    /// <summary>The last mip level; 0 for a single-level resource.</summary>
    public uint LastLevel;

    /// <summary>Samples per pixel; 1 for unsampled resources.</summary>
    public uint NrSamples;

    /// <summary>Reserved.</summary>
    public uint Flags;

    /// <summary>Reserved; the device struct carries one dword more.</summary>
    public uint Padding;
}

/// <summary>
/// virtio_gpu_box (virtio 5.7.6.6): the 3D transfer region, x/y/z origin
/// and width/height/depth extent.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuBox
{
    /// <summary>Left edge.</summary>
    public uint X;

    /// <summary>Top edge.</summary>
    public uint Y;

    /// <summary>Front edge.</summary>
    public uint Z;

    /// <summary>Width.</summary>
    public uint Width;

    /// <summary>Height.</summary>
    public uint Height;

    /// <summary>Depth.</summary>
    public uint Depth;
}

/// <summary>
/// virtio_gpu_transfer_host_3d (virtio 5.7.6.7): copies a box between the
/// guest backing of a 3D resource and the host copy; the direction is the
/// command id, TO_HOST_3D up and FROM_HOST_3D down.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuTransferHost3D
{
    /// <summary>The header, type TRANSFER_TO_HOST_3D or TRANSFER_FROM_HOST_3D.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The region to copy.</summary>
    public VirtioGpuBox Box;

    /// <summary>Byte offset of the box's origin in the backing.</summary>
    public ulong Offset;

    /// <summary>The resource.</summary>
    public uint ResourceId;

    /// <summary>The mip level; 0 outside mipmapped resources.</summary>
    public uint Level;

    /// <summary>Bytes per row in the backing; 0 lets the host take the resource's stride.</summary>
    public uint Stride;

    /// <summary>Bytes per layer; 0 lets the host take the resource's stride.</summary>
    public uint LayerStride;
}

/// <summary>
/// virtio_gpu_cmd_submit (virtio 5.7.6.9): pushes <see cref="Size"/> bytes
/// of virgl command stream dwords into the context named by Hdr.CtxId; the
/// stream follows this struct on the same descriptor chain.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuCmdSubmit
{
    /// <summary>The header, type SUBMIT_3D, CtxId = the context.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The stream's length in bytes, a multiple of 4.</summary>
    public uint Size;

    /// <summary>Reserved.</summary>
    public uint Padding;
}
