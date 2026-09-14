// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Bitmask selecting which inode fields apply in <c>setattr</c>.
/// </summary>
[Flags]
public enum SetAttrFlags : uint
{
    /// <summary>No fields selected.</summary>
    None = 0,
    /// <summary>Apply <see cref="VfsStat.Mode"/> (permission bits; the file type is not changed).</summary>
    Mode = 1 << 0,
    /// <summary>Apply <see cref="VfsStat.Uid"/>.</summary>
    Uid = 1 << 1,
    /// <summary>Apply <see cref="VfsStat.Gid"/>.</summary>
    Gid = 1 << 2,
    /// <summary>Apply <see cref="VfsStat.Size"/>: truncate or zero-extend the file.</summary>
    Size = 1 << 3,
    /// <summary>Apply <see cref="VfsStat.Atime"/>.</summary>
    Atime = 1 << 4,
    /// <summary>Apply <see cref="VfsStat.Mtime"/>.</summary>
    Mtime = 1 << 5,
    /// <summary>Apply <see cref="VfsStat.Ctime"/>.</summary>
    Ctime = 1 << 6,
}
