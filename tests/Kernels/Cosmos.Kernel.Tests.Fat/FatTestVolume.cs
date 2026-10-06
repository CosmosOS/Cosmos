using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.FileSystem.Fat;

namespace Cosmos.Kernel.Tests.Fat;

/// <summary>
/// Builds clean FAT16 / FAT32 volumes on a <see cref="MemoryBlockDevice"/> by
/// calling <see cref="FatFileSystemType.TryFormat(global::System.ReadOnlySpan{char}, IVfsFormatOptions?)"/>
/// — the same path real kernel code would use to mkfs a partition. The
/// volume sizes and explicit overrides are chosen so the resulting cluster
/// counts land squarely in the FAT family the test asks for.
/// </summary>
internal static class FatTestVolume
{
    public const ulong BlockSize = 512;

    /// <summary>Number of FAT copies on every test volume — the standard redundant pair (fatgen103 BPB_NumFATs); shared with the format cells in <see cref="Kernel"/>.</summary>
    internal const byte FatCopyCount = 2;

    // FAT16 geometry: 32 MiB / SPC=8 / 32-sector FAT keeps cluster count in the
    // [4085, 65525) band even with the formatter's auto-pick branches.
    private const ulong Fat16DiskSizeBytes = 32UL * 1024 * 1024;
    public const ulong Fat16BlockCount = Fat16DiskSizeBytes / BlockSize;

    /// <summary>Sectors per cluster on the FAT16 volume; keeps the 32 MiB disk's cluster count inside the FAT16 band (fatgen103 BPB_SecPerClus).</summary>
    private const byte Fat16SectorsPerCluster = 8;

    /// <summary>Reserved sectors before the first FAT on the FAT16 volume — boot sector only, the classic FAT12/16 layout (fatgen103 BPB_RsvdSecCnt).</summary>
    private const ushort Fat16ReservedSectorCount = 1;

    /// <summary>Fixed root-directory entry count on the FAT16 volume — the fatgen103 BPB_RootEntCnt default of 512 32-byte entries.</summary>
    private const ushort Fat16RootEntryCount = 512;

    /// <summary>Sectors per FAT copy on the FAT16 volume; 32 sectors of 256 two-byte entries cover the ~8k clusters (fatgen103 BPB_FATSz16).</summary>
    private const uint Fat16FatSectorCount = 32;

    // FAT32 geometry: 33 MiB / SPC=1 / 520-sector FAT yields cluster count
    // > 65525 while one FAT copy still covers it (fatgen103 requires
    // clusterCount + 2 <= FatSectorCount * 128; 512 sectors fell 994
    // entries short and out-of-FAT accesses would corrupt copy #2).
    private const ulong Fat32DiskSizeBytes = 33UL * 1024 * 1024;
    public const ulong Fat32BlockCount = Fat32DiskSizeBytes / BlockSize;

    /// <summary>Sectors per cluster on the FAT32 volume; 1 maximizes the cluster count so it clears the 65525 FAT32 threshold (fatgen103 BPB_SecPerClus).</summary>
    private const byte Fat32SectorsPerCluster = 1;

    /// <summary>Reserved sectors before the first FAT on the FAT32 volume — the fatgen103 BPB_RsvdSecCnt default of 32 for FAT32.</summary>
    private const ushort Fat32ReservedSectorCount = 32;

    /// <summary>Sectors per FAT copy on the FAT32 volume; 520 sectors of 128 four-byte entries cover clusterCount + 2 where 512 fell short (fatgen103 BPB_FATSz32).</summary>
    private const uint Fat32FatSectorCount = 520;

    public static MemoryBlockDevice CreateFat16(string name)
    {
        return FormatFat16(new MemoryBlockDevice(name, BlockSize, Fat16BlockCount));
    }

    /// <summary>Formats an existing device (sized <see cref="Fat16BlockCount"/>) with the FAT16 geometry above.</summary>
    public static MemoryBlockDevice FormatFat16(MemoryBlockDevice device)
    {
        FatFileSystemType driver = new(device);
        FatFormatOptions options = new()
        {
            Type = FatType.Fat16,
            SectorsPerCluster = Fat16SectorsPerCluster,
            ReservedSectorCount = Fat16ReservedSectorCount,
            NumberOfFats = FatCopyCount,
            RootEntryCount = Fat16RootEntryCount,
            FatSectorCount = Fat16FatSectorCount,
            VolumeLabel = "COSMOSFAT  ",
        };
        if (!driver.TryFormat(default, options))
        {
            throw new InvalidOperationException("FAT16 format failed");
        }
        return device;
    }

    public static MemoryBlockDevice CreateFat32(string name)
    {
        return FormatFat32(new MemoryBlockDevice(name, BlockSize, Fat32BlockCount));
    }

    /// <summary>Formats an existing device (sized <see cref="Fat32BlockCount"/>) with the FAT32 geometry above.</summary>
    public static MemoryBlockDevice FormatFat32(MemoryBlockDevice device)
    {
        FatFileSystemType driver = new(device);
        FatFormatOptions options = new()
        {
            Type = FatType.Fat32,
            SectorsPerCluster = Fat32SectorsPerCluster,
            ReservedSectorCount = Fat32ReservedSectorCount,
            NumberOfFats = FatCopyCount,
            FatSectorCount = Fat32FatSectorCount,
            RootCluster = FatTable.FirstDataCluster,
            VolumeLabel = "COSMOSFAT32",
        };
        if (!driver.TryFormat(default, options))
        {
            throw new InvalidOperationException("FAT32 format failed");
        }
        return device;
    }
}
