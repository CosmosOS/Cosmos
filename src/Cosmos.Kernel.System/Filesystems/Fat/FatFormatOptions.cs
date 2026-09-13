// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Filesystems.Fat;

/// <summary>
/// Parameters for <see cref="FatFilesystemType.TryFormat(global::System.ReadOnlySpan{char}, IVfsFormatOptions)"/>.
/// All <c>0</c> / null fields mean "let the formatter pick a sensible default
/// for the requested <see cref="FatType"/> and the device size."
/// </summary>
public sealed class FatFormatOptions : IVfsFormatOptions
{
    /// <summary>FAT family to format. <see cref="FatType.Unknown"/> picks
    /// the smallest type that fits the device's cluster count.</summary>
    public FatType Type { get; init; } = FatType.Unknown;

    /// <summary>Sectors per cluster. <c>0</c> = auto-pick a power of two.</summary>
    public byte SectorsPerCluster { get; init; }

    /// <summary>Reserved sector count. <c>0</c> = 1 for FAT12/16, 32 for FAT32.</summary>
    public ushort ReservedSectorCount { get; init; }

    /// <summary>Number of FAT copies. <c>0</c> = 2 mirrored copies (fatgen103 §3.1).</summary>
    public byte NumberOfFats { get; init; } = FatFormatter.DefaultNumberOfFats;

    /// <summary>Root entry count (FAT12/16 only). <c>0</c> = 512.</summary>
    public ushort RootEntryCount { get; init; }

    /// <summary>
    /// FAT sector count. <c>0</c> = compute from cluster count. On
    /// FAT12/16 the BPB stores this in a 16-bit field, so values above
    /// 65535 are rejected by the formatter for those types.
    /// </summary>
    public uint FatSectorCount { get; init; }

    /// <summary>Root cluster (FAT32 only). <c>0</c> = 2.</summary>
    public uint RootCluster { get; init; }

    /// <summary>11-byte volume label, padded with spaces. Empty = "NO NAME    ".</summary>
    public string? VolumeLabel { get; init; }

    /// <summary>32-bit volume serial number. <c>0</c> = leave a deterministic default.</summary>
    public uint VolumeSerial { get; init; }
}
