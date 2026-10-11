// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>
/// The virtio-gpu command opcodes, response types, feature bits, pixel
/// formats and configuration space offsets (virtio specification section
/// 5.7), for <see cref="VirtioGpuDriver"/> and <see cref="VirtioGpuState"/>.
/// The on-wire structs follow in this file, laid out sequentially with no
/// padding so a <c>MemoryMarshal.Write</c> into DMA memory is the wire
/// format; every field is little-endian on the architectures the kernel
/// runs on. Constants only; any context.
/// </summary>
internal static class VirtioGpuCmd
{
    /// <summary>VIRTIO_GPU_CMD_GET_DISPLAY_INFO: the scanouts and their rectangles.</summary>
    public const uint GetDisplayInfo = 0x0100;

    /// <summary>VIRTIO_GPU_CMD_RESOURCE_CREATE_2D: a host 2D resource of a size and a format.</summary>
    public const uint ResourceCreate2D = 0x0101;

    /// <summary>VIRTIO_GPU_CMD_RESOURCE_UNREF: destroys a host resource.</summary>
    public const uint ResourceUnref = 0x0102;

    /// <summary>VIRTIO_GPU_CMD_SET_SCANOUT: binds a resource rectangle to a scanout.</summary>
    public const uint SetScanout = 0x0103;

    /// <summary>VIRTIO_GPU_CMD_RESOURCE_FLUSH: makes a resource rectangle visible on its scanouts.</summary>
    public const uint ResourceFlush = 0x0104;

    /// <summary>VIRTIO_GPU_CMD_TRANSFER_TO_HOST_2D: copies a rectangle from the guest backing into the host resource.</summary>
    public const uint TransferToHost2D = 0x0105;

    /// <summary>VIRTIO_GPU_CMD_RESOURCE_ATTACH_BACKING: gives a resource its guest memory.</summary>
    public const uint ResourceAttachBacking = 0x0106;

    /// <summary>VIRTIO_GPU_CMD_RESOURCE_DETACH_BACKING: takes the guest memory away again.</summary>
    public const uint ResourceDetachBacking = 0x0107;

    /// <summary>VIRTIO_GPU_CMD_GET_CAPSET_INFO: a capability set's id and size.</summary>
    public const uint GetCapsetInfo = 0x0108;

    /// <summary>VIRTIO_GPU_CMD_GET_CAPSET: a capability set's contents.</summary>
    public const uint GetCapset = 0x0109;

    /// <summary>VIRTIO_GPU_CMD_GET_EDID: a scanout's EDID blob.</summary>
    public const uint GetEdid = 0x010A;

    /// <summary>VIRTIO_GPU_CMD_UPDATE_CURSOR, on the cursor queue: a new cursor image and position.</summary>
    public const uint UpdateCursor = 0x0300;

    /// <summary>VIRTIO_GPU_CMD_MOVE_CURSOR, on the cursor queue: a new cursor position.</summary>
    public const uint MoveCursor = 0x0301;

    /// <summary>VIRTIO_GPU_RESP_OK_NODATA: the command succeeded and the response is the header alone.</summary>
    public const uint RespOkNoData = 0x1100;

    /// <summary>VIRTIO_GPU_RESP_OK_DISPLAY_INFO: the response to GET_DISPLAY_INFO.</summary>
    public const uint RespOkDisplayInfo = 0x1101;

    /// <summary>VIRTIO_GPU_RESP_OK_CAPSET_INFO: the response to GET_CAPSET_INFO.</summary>
    public const uint RespOkCapsetInfo = 0x1102;

    /// <summary>VIRTIO_GPU_RESP_OK_CAPSET: the response to GET_CAPSET.</summary>
    public const uint RespOkCapset = 0x1103;

    /// <summary>VIRTIO_GPU_RESP_OK_EDID: the response to GET_EDID.</summary>
    public const uint RespOkEdid = 0x1104;

    /// <summary>VIRTIO_GPU_RESP_ERR_UNSPEC: the command failed for an unstated reason.</summary>
    public const uint RespErrUnspec = 0x1200;

    /// <summary>VIRTIO_GPU_RESP_ERR_OUT_OF_MEMORY: the host is out of memory.</summary>
    public const uint RespErrOutOfMemory = 0x1201;

    /// <summary>VIRTIO_GPU_RESP_ERR_INVALID_SCANOUT_ID: the scanout does not exist.</summary>
    public const uint RespErrInvalidScanoutId = 0x1202;

    /// <summary>VIRTIO_GPU_RESP_ERR_INVALID_RESOURCE_ID: the resource does not exist.</summary>
    public const uint RespErrInvalidResourceId = 0x1203;

    /// <summary>VIRTIO_GPU_RESP_ERR_INVALID_CONTEXT_ID: the context does not exist.</summary>
    public const uint RespErrInvalidContextId = 0x1204;

    /// <summary>VIRTIO_GPU_RESP_ERR_INVALID_PARAMETER: a parameter is out of range.</summary>
    public const uint RespErrInvalidParameter = 0x1205;

    /// <summary>VIRTIO_GPU_F_VIRGL: the device renders 3D through virgl; not requested, the 2D path is the whole driver.</summary>
    public const uint FeatureVirgl = 1u << 0;

    /// <summary>VIRTIO_GPU_F_EDID: the device answers GET_EDID.</summary>
    public const uint FeatureEdid = 1u << 1;

    /// <summary>VIRTIO_GPU_F_RESOURCE_UUID: resources carry a UUID.</summary>
    public const uint FeatureResourceUuid = 1u << 2;

    /// <summary>VIRTIO_GPU_F_RESOURCE_BLOB: blob resources exist.</summary>
    public const uint FeatureResourceBlob = 1u << 3;

    /// <summary>VIRTIO_GPU_FLAG_FENCE in a header's flags: the device fences the command.</summary>
    public const uint FlagFence = 1u << 0;

    /// <summary>VIRTIO_GPU_FORMAT_B8G8R8A8_UNORM.</summary>
    public const uint FormatB8G8R8A8Unorm = 1;

    /// <summary>VIRTIO_GPU_FORMAT_B8G8R8X8_UNORM: the scanout resource's format, the canvas's ARGB dword with the alpha byte ignored.</summary>
    public const uint FormatB8G8R8X8Unorm = 2;

    /// <summary>VIRTIO_GPU_FORMAT_A8R8G8B8_UNORM.</summary>
    public const uint FormatA8R8G8B8Unorm = 3;

    /// <summary>VIRTIO_GPU_FORMAT_X8R8G8B8_UNORM.</summary>
    public const uint FormatX8R8G8B8Unorm = 4;

    /// <summary>VIRTIO_GPU_FORMAT_R8G8B8A8_UNORM.</summary>
    public const uint FormatR8G8B8A8Unorm = 67;

    /// <summary>VIRTIO_GPU_FORMAT_R8G8B8X8_UNORM.</summary>
    public const uint FormatR8G8B8X8Unorm = 68;

    /// <summary>Offset of events_read in the configuration space (virtio 5.7.4: events_read, events_clear, num_scanouts, num_capsets, each 32 bits).</summary>
    public const uint ConfigEventsReadOffset = 0;

    /// <summary>Offset of events_clear in the configuration space.</summary>
    public const uint ConfigEventsClearOffset = 4;

    /// <summary>Offset of num_scanouts in the configuration space.</summary>
    public const uint ConfigNumScanoutsOffset = 8;

    /// <summary>Offset of num_capsets in the configuration space.</summary>
    public const uint ConfigNumCapsetsOffset = 12;

    /// <summary>VIRTIO_GPU_MAX_SCANOUTS: how many entries a display info response carries.</summary>
    public const int MaxScanouts = 16;
}

/// <summary>
/// virtio_gpu_ctrl_hdr (virtio 5.7.6.1): the 24 bytes every control queue
/// command and response starts with. The hardware layout, so the fields
/// stay public.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuCtrlHdr
{
    /// <summary>The command or response type.</summary>
    public uint Type;

    /// <summary>VIRTIO_GPU_FLAG_FENCE or 0.</summary>
    public uint Flags;

    /// <summary>The fence id when fenced.</summary>
    public ulong FenceId;

    /// <summary>The 3D context id; 0 on the 2D path.</summary>
    public uint CtxId;

    /// <summary>Reserved.</summary>
    public uint Padding;
}

/// <summary>virtio_gpu_rect (virtio 5.7.6.2): a rectangle in pixels, 16 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuRect
{
    /// <summary>Left edge.</summary>
    public uint X;

    /// <summary>Top edge.</summary>
    public uint Y;

    /// <summary>Width.</summary>
    public uint Width;

    /// <summary>Height.</summary>
    public uint Height;
}

/// <summary>virtio_gpu_resource_create_2d: a host resource of the given format and size.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceCreate2D
{
    /// <summary>The header, type RESOURCE_CREATE_2D.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The resource id the driver chose; 0 is reserved.</summary>
    public uint ResourceId;

    /// <summary>One of the VIRTIO_GPU_FORMAT values.</summary>
    public uint Format;

    /// <summary>Width in pixels.</summary>
    public uint Width;

    /// <summary>Height in pixels.</summary>
    public uint Height;
}

/// <summary>virtio_gpu_set_scanout: binds a rectangle of a resource to a scanout.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuSetScanout
{
    /// <summary>The header, type SET_SCANOUT.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The part of the resource the scanout shows.</summary>
    public VirtioGpuRect Rect;

    /// <summary>The scanout, 0 to num_scanouts - 1.</summary>
    public uint ScanoutId;

    /// <summary>The resource; 0 disables the scanout.</summary>
    public uint ResourceId;
}

/// <summary>virtio_gpu_resource_flush: makes a resource rectangle visible.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceFlush
{
    /// <summary>The header, type RESOURCE_FLUSH.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The dirty rectangle.</summary>
    public VirtioGpuRect Rect;

    /// <summary>The resource.</summary>
    public uint ResourceId;

    /// <summary>Reserved.</summary>
    public uint Padding;
}

/// <summary>virtio_gpu_transfer_to_host_2d: copies a rectangle from the guest backing into the host resource.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuTransferToHost2D
{
    /// <summary>The header, type TRANSFER_TO_HOST_2D.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The rectangle to copy.</summary>
    public VirtioGpuRect Rect;

    /// <summary>Byte offset of the rectangle's top-left pixel in the backing.</summary>
    public ulong Offset;

    /// <summary>The resource.</summary>
    public uint ResourceId;

    /// <summary>Reserved.</summary>
    public uint Padding;
}

/// <summary>virtio_gpu_resource_unref: destroys a host resource.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceUnref
{
    /// <summary>The header, type RESOURCE_UNREF.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The resource.</summary>
    public uint ResourceId;

    /// <summary>Reserved.</summary>
    public uint Padding;
}

/// <summary>
/// virtio_gpu_resource_attach_backing: gives a resource its guest memory;
/// <see cref="NrEntries"/> <see cref="VirtioGpuMemEntry"/> values follow on
/// the same descriptor chain.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceAttachBacking
{
    /// <summary>The header, type RESOURCE_ATTACH_BACKING.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The resource.</summary>
    public uint ResourceId;

    /// <summary>How many memory entries follow.</summary>
    public uint NrEntries;
}

/// <summary>virtio_gpu_mem_entry: one scatter-gather entry of a backing, 16 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuMemEntry
{
    /// <summary>Guest-physical address of the entry's first byte.</summary>
    public ulong Addr;

    /// <summary>Length in bytes.</summary>
    public uint Length;

    /// <summary>Reserved.</summary>
    public uint Padding;
}

/// <summary>virtio_gpu_display_one: one scanout of a display info response, 24 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuDisplayOne
{
    /// <summary>The scanout's rectangle on the host's display.</summary>
    public VirtioGpuRect Rect;

    /// <summary>1 when the scanout is enabled.</summary>
    public uint Enabled;

    /// <summary>Reserved.</summary>
    public uint Flags;
}

/// <summary>The <see cref="VirtioGpuCmd.MaxScanouts"/> entries of a display info response, in place of a fixed buffer.</summary>
[InlineArray(VirtioGpuCmd.MaxScanouts)]
internal struct VirtioGpuPModes
{
    /// <summary>The first entry; the attribute repeats it.</summary>
    private VirtioGpuDisplayOne _element0;
}

/// <summary>
/// virtio_gpu_resp_display_info (virtio 5.7.6.6.1): the header and one
/// entry per possible scanout, 408 bytes in all; a response buffer shorter
/// than that is truncated by the device.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuRespDisplayInfo
{
    /// <summary>The header, type RESP_OK_DISPLAY_INFO.</summary>
    public VirtioGpuCtrlHdr Hdr;

    /// <summary>The scanouts; only the first num_scanouts entries are meaningful.</summary>
    public VirtioGpuPModes PModes;
}
