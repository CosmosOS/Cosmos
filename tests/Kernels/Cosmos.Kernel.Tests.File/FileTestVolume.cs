namespace Cosmos.Kernel.Tests.File;

// global:: because a plain "System" binds to Cosmos.Kernel.System from
// inside this namespace.
using global::System;
using Cosmos.Kernel.System.FileSystem.Fat;

/// <summary>
/// Builds the clean FAT16 volume the System.IO tests run on, using the same
/// formatter path real kernel code would use to mkfs a partition. Geometry
/// matches the Fat suite's FAT16 profile: 32 MiB / SPC=8 / 32-sector FAT
/// keeps the cluster count inside the FAT16 band.
/// </summary>
internal static class FileTestVolume
{
    public const ulong BlockSize = 512;

    private const ulong DiskSizeBytes = 32UL * 1024 * 1024;
    public const ulong BlockCount = DiskSizeBytes / BlockSize;

    /// <summary>Sectors per cluster (fatgen103 BPB_SecPerClus) — 4 KiB clusters.</summary>
    private const byte SectorsPerCluster = 8;

    /// <summary>Boot sector only, the classic FAT12/16 layout (fatgen103 BPB_RsvdSecCnt).</summary>
    private const ushort ReservedSectorCount = 1;

    /// <summary>Fixed root-directory entry count (fatgen103 BPB_RootEntCnt).</summary>
    private const ushort RootEntryCount = 512;

    /// <summary>Sectors per FAT copy — 32 sectors of 256 two-byte entries (fatgen103 BPB_FATSz16).</summary>
    private const uint FatSectorCount = 32;

    /// <summary>The standard redundant FAT pair (fatgen103 BPB_NumFATs).</summary>
    private const byte FatCopyCount = 2;

    public static MemoryBlockDevice Create(string name)
    {
        MemoryBlockDevice device = new(name, BlockSize, BlockCount);
        FatFileSystemType driver = new(device);
        FatFormatOptions options = new()
        {
            Type = FatType.Fat16,
            SectorsPerCluster = SectorsPerCluster,
            ReservedSectorCount = ReservedSectorCount,
            NumberOfFats = FatCopyCount,
            RootEntryCount = RootEntryCount,
            FatSectorCount = FatSectorCount,
            VolumeLabel = "COSMOSFILE ",
        };
        if (!driver.TryFormat(default, options))
        {
            throw new InvalidOperationException("FAT16 format failed");
        }

        return device;
    }
}
