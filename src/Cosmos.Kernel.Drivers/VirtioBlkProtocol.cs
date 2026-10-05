// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The constants of the virtio block device (virtio 1.x section 5.2) as the
/// Linux UAPI header <c>include/uapi/linux/virtio_blk.h</c> fixes them: the
/// feature bits of the low word, the offsets of
/// <c>struct virtio_blk_config</c>, the request types, the status values and
/// the sizes of the three parts of a request. Any context.
/// </summary>
internal static class VirtioBlkProtocol
{
    // --- Feature bits (the low word VirtioAccess.NegotiateFeatures takes) ---

    /// <summary>VIRTIO_BLK_F_SIZE_MAX: the configuration space carries the largest segment the device takes.</summary>
    internal const uint FeatureSizeMax = 1u << 1;

    /// <summary>VIRTIO_BLK_F_SEG_MAX: the configuration space carries the most segments a request may have.</summary>
    internal const uint FeatureSegMax = 1u << 2;

    /// <summary>VIRTIO_BLK_F_GEOMETRY: the configuration space carries a legacy cylinder, head and sector geometry.</summary>
    internal const uint FeatureGeometry = 1u << 4;

    /// <summary>VIRTIO_BLK_F_RO: the device is read-only.</summary>
    internal const uint FeatureReadOnly = 1u << 5;

    /// <summary>VIRTIO_BLK_F_BLK_SIZE: the configuration space carries the logical block size.</summary>
    internal const uint FeatureBlockSize = 1u << 6;

    /// <summary>VIRTIO_BLK_F_FLUSH: the device has a volatile cache and takes the flush request.</summary>
    internal const uint FeatureFlush = 1u << 9;

    /// <summary>VIRTIO_BLK_F_TOPOLOGY: the configuration space carries the physical block topology.</summary>
    internal const uint FeatureTopology = 1u << 10;

    /// <summary>VIRTIO_BLK_F_CONFIG_WCE: the cache mode is writable through the writeback field.</summary>
    internal const uint FeatureConfigWce = 1u << 11;

    /// <summary>VIRTIO_BLK_F_MQ: the device has more than one request queue.</summary>
    internal const uint FeatureMultiQueue = 1u << 12;

    /// <summary>VIRTIO_F_ANY_LAYOUT: the device accepts any descriptor layout.</summary>
    internal const uint FeatureAnyLayout = 1u << 27;

    // --- Configuration space offsets (struct virtio_blk_config, packed) ---

    /// <summary>The low dword of <c>capacity</c>, a u64 in 512-byte sectors whatever <c>blk_size</c> says.</summary>
    internal const uint CapacityLowOffset = 0;

    /// <summary>The high dword of <c>capacity</c>; the kit has no 64-bit read, so the field is two dword reads.</summary>
    internal const uint CapacityHighOffset = 4;

    /// <summary><c>size_max</c>, read only with <see cref="FeatureSizeMax"/>.</summary>
    internal const uint SizeMaxOffset = 8;

    /// <summary><c>seg_max</c>, read only with <see cref="FeatureSegMax"/>.</summary>
    internal const uint SegMaxOffset = 12;

    /// <summary><c>blk_size</c>, read only with <see cref="FeatureBlockSize"/>.</summary>
    internal const uint BlockSizeOffset = 20;

    /// <summary><c>writeback</c>, read only with <see cref="FeatureConfigWce"/>.</summary>
    internal const uint WritebackOffset = 32;

    // --- Requests ---

    /// <summary>VIRTIO_BLK_T_IN: a read.</summary>
    internal const uint RequestIn = 0u;

    /// <summary>VIRTIO_BLK_T_OUT: a write.</summary>
    internal const uint RequestOut = 1u;

    /// <summary>VIRTIO_BLK_T_FLUSH: a cache flush, with no data.</summary>
    internal const uint RequestFlush = 4u;

    /// <summary>VIRTIO_BLK_T_GET_ID: the device's serial; named, not used.</summary>
    internal const uint RequestGetId = 8u;

    /// <summary>VIRTIO_BLK_S_OK: the request completed.</summary>
    internal const byte StatusOk = 0;

    /// <summary>VIRTIO_BLK_S_IOERR: the request failed on the device.</summary>
    internal const byte StatusIoError = 1;

    /// <summary>VIRTIO_BLK_S_UNSUPP: the device does not take the request.</summary>
    internal const byte StatusUnsupported = 2;

    /// <summary>The value the driver writes into the status byte before submitting, which the device never writes.</summary>
    internal const byte StatusPending = 0xFF;

    /// <summary>Bytes of a sector: the unit of <c>capacity</c> and of the request's <c>sector</c> field.</summary>
    internal const int SectorBytes = 512;

    /// <summary>Bytes of the request header, <see cref="VirtioBlkRequestHeader"/>.</summary>
    internal const int HeaderBytes = 16;

    /// <summary>Bytes of the status the device writes at the end of a request.</summary>
    internal const int StatusBytes = 1;

    /// <summary>The index of the one request queue the driver creates.</summary>
    internal const ushort RequestQueue = 0;
}

/// <summary>
/// The header of a request (<c>struct virtio_blk_outhdr</c>): the type, a
/// reserved word (ioprio on the legacy interface) and the first sector, 16
/// bytes, every field little-endian as the device reads it on both
/// machines. Written into the bounce block with <c>MemoryMarshal</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VirtioBlkRequestHeader
{
    /// <summary>The request type, one of the <c>Request</c> constants of <see cref="VirtioBlkProtocol"/>.</summary>
    public uint Type;

    /// <summary>Reserved; zero.</summary>
    public uint Reserved;

    /// <summary>The first sector, in 512-byte units whatever the block size.</summary>
    public ulong Sector;
}
