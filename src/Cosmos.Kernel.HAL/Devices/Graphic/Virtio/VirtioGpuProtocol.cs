// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.Virtio;

/// <summary>
/// virtio-gpu command opcodes and on-wire structs (virtio 1.x spec section 5.7).
/// Layouts are [StructLayout.Sequential, Pack=1] so the in-memory shape matches
/// the wire format the device DMA's into; fields are little-endian on every
/// supported architecture (x64 / ARM64-little).
/// </summary>
internal static class VirtioGpuCmd
{
    // 2D resource commands (virtio 1.x §5.7.5)
    public const uint VIRTIO_GPU_CMD_GET_DISPLAY_INFO    = 0x0100;
    public const uint VIRTIO_GPU_CMD_RESOURCE_CREATE_2D = 0x0101;
    public const uint VIRTIO_GPU_CMD_RESOURCE_UNREF      = 0x0102;
    public const uint VIRTIO_GPU_CMD_SET_SCANOUT         = 0x0103;
    public const uint VIRTIO_GPU_CMD_RESOURCE_FLUSH      = 0x0104;
    public const uint VIRTIO_GPU_CMD_TRANSFER_TO_HOST_2D = 0x0105;
    public const uint VIRTIO_GPU_CMD_RESOURCE_ATTACH_BACKING = 0x0106;
    public const uint VIRTIO_GPU_CMD_RESOURCE_DETACH_BACKING = 0x0107;
    public const uint VIRTIO_GPU_CMD_GET_CAPSET_INFO     = 0x0108;
    public const uint VIRTIO_GPU_CMD_GET_CAPSET          = 0x0109;
    public const uint VIRTIO_GPU_CMD_GET_EDID            = 0x010A;

    // Cursor commands go on the cursor queue (queue 1).
    public const uint VIRTIO_GPU_CMD_UPDATE_CURSOR = 0x0300;
    public const uint VIRTIO_GPU_CMD_MOVE_CURSOR   = 0x0301;

    // Response types returned on the control queue's used ring.
    public const uint VIRTIO_GPU_RESP_OK_NODATA        = 0x1100;
    public const uint VIRTIO_GPU_RESP_OK_DISPLAY_INFO  = 0x1101;
    public const uint VIRTIO_GPU_RESP_OK_CAPSET_INFO   = 0x1102;
    public const uint VIRTIO_GPU_RESP_OK_CAPSET        = 0x1103;
    public const uint VIRTIO_GPU_RESP_OK_EDID          = 0x1104;
    public const uint VIRTIO_GPU_RESP_ERR_UNSPEC       = 0x1200;
    public const uint VIRTIO_GPU_RESP_ERR_OUT_OF_MEMORY = 0x1201;
    public const uint VIRTIO_GPU_RESP_ERR_INVALID_SCANOUT_ID = 0x1202;
    public const uint VIRTIO_GPU_RESP_ERR_INVALID_RESOURCE_ID = 0x1203;
    public const uint VIRTIO_GPU_RESP_ERR_INVALID_CONTEXT_ID = 0x1204;
    public const uint VIRTIO_GPU_RESP_ERR_INVALID_PARAMETER = 0x1205;

    // Feature bits (virtio 1.x §5.7.4)
    public const uint VIRTIO_GPU_F_VIRGL          = 1u << 0;
    public const uint VIRTIO_GPU_F_EDID           = 1u << 1;
    public const uint VIRTIO_GPU_F_RESOURCE_UUID = 1u << 2;
    public const uint VIRTIO_GPU_F_RESOURCE_BLOB = 1u << 3;

    // virtio_gpu_ctrl_hdr.flags
    public const uint VIRTIO_GPU_FLAG_FENCE = 1u << 0;

    // 2D pixel formats (virtio 1.x §5.7.7)
    public const uint VIRTIO_GPU_FORMAT_B8G8R8A8_UNORM  = 1;
    public const uint VIRTIO_GPU_FORMAT_B8G8R8X8_UNORM  = 2;
    public const uint VIRTIO_GPU_FORMAT_A8R8G8B8_UNORM  = 3;
    public const uint VIRTIO_GPU_FORMAT_X8R8G8B8_UNORM  = 4;
    public const uint VIRTIO_GPU_FORMAT_R8G8B8A8_UNORM  = 67;
    public const uint VIRTIO_GPU_FORMAT_R8G8B8X8_UNORM  = 68;

    // Device config space layout (virtio 1.x §5.7.2): u32 num_scanouts, u32 num_capsets, u32 events_read.
    public const uint ConfigNumScanoutsOffset  = 0;
    public const uint ConfigNumCapsetsOffset   = 4;
    public const uint ConfigEventsReadOffset   = 8;
}

/// <summary>
/// virtio_gpu_ctrl_hdr (virtio 1.x §5.7.5.1): 24 bytes, every controlq command
/// and response starts with one.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuCtrlHdr
{
    public uint Type;
    public uint Flags;
    public ulong FenceId;
    public uint CtxId;
    public uint Padding;
}

/// <summary>
/// virtio_gpu_rect (virtio 1.x §5.7.5.2): 16 bytes, used by SET_SCANOUT /
/// RESOURCE_FLUSH / TRANSFER_TO_HOST_2D payloads.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuRect
{
    public uint X;
    public uint Y;
    public uint Width;
    public uint Height;
}

/// <summary>
/// virtio_gpu_resource_create_2d payload (after the ctrl_hdr).
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceCreate2D
{
    public VirtioGpuCtrlHdr Hdr;
    public uint ResourceId;
    public uint Format;
    public uint Width;
    public uint Height;
}

/// <summary>
/// virtio_gpu_set_scanout payload. ScanoutId is the display pipe index (0..num_scanouts-1).
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuSetScanout
{
    public VirtioGpuCtrlHdr Hdr;
    public VirtioGpuRect Rect;
    public uint ScanoutId;
    public uint ResourceId;
}

/// <summary>
/// virtio_gpu_resource_flush payload: dirty rect on a scanout.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceFlush
{
    public VirtioGpuCtrlHdr Hdr;
    public VirtioGpuRect Rect;
    public uint ResourceId;
    public uint Padding;
}

/// <summary>
/// virtio_gpu_transfer_to_host_2d payload: copy guest backing rect → host 2D resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuTransferToHost2D
{
    public VirtioGpuCtrlHdr Hdr;
    public VirtioGpuRect Rect;
    public ulong Offset;
    public uint ResourceId;
    public uint Padding;
}

/// <summary>
/// virtio_gpu_resource_unref payload.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceUnref
{
    public VirtioGpuCtrlHdr Hdr;
    public uint ResourceId;
    public uint Padding;
}

/// <summary>
/// virtio_gpu_resource_attach_backing payload: bind a guest mem region as backing
/// for a host resource. Followed by N virtio_gpu_mem_entry on the same chain.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuResourceAttachBacking
{
    public VirtioGpuCtrlHdr Hdr;
    public uint ResourceId;
    public uint NrEntries;
}

/// <summary>
/// virtio_gpu_mem_entry: one scatter-gather entry in an attach-backing chain.
/// Addr is a guest-physical address; the device DMA's from it.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuMemEntry
{
    public ulong Addr;
    public uint Length;
    public uint Padding;
}

/// <summary>
/// virtio_gpu_resp_display_info (virtio 1.x §5.7.5.6.1): one entry per scanout,
/// enabled flag in the high bit of r.rect.x.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuDisplayOne
{
    public VirtioGpuRect Rect;
    public uint Enabled;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioGpuRespDisplayInfo
{
    public VirtioGpuCtrlHdr Hdr;
    // The device writes one VirtioGpuDisplayOne per scanout, up to 16. The
    // common single-scanout case reads just the first; the rest is reserved.
    public VirtioGpuDisplayOne PModes;
}
