// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Fat;

/// <summary>
/// FAT family identifier; selected from cluster count per the FAT32 spec.
/// </summary>
public enum FatType
{
    /// <summary>The volume is not a recognized FAT variant.</summary>
    Unknown,
    /// <summary>FAT12: fewer than 4085 clusters.</summary>
    Fat12,
    /// <summary>FAT16: 4085 to 65524 clusters.</summary>
    Fat16,
    /// <summary>FAT32: 65525 clusters or more.</summary>
    Fat32,
}
