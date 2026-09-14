// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Per-open file state (position, etc.).
/// </summary>
public interface IVfsOpenFile
{
    /// <summary>Inode this open file refers to.</summary>
    IVfsInode Inode { get; }

    /// <summary>Byte I/O operations that service reads and writes on this open file.</summary>
    IFileOperations Operations { get; }

    /// <summary>
    /// Current byte offset for the next read or write. The VFS layer
    /// advances it by the return value of <see cref="IFileOperations.Read"/>
    /// and <see cref="IFileOperations.Write"/>; implementations of those do not.
    /// </summary>
    long Position { get; set; }
}
