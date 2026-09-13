// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Vfs;

/// <summary>
/// Common surface for VFS nodes (files or directories). The interface is
/// disposable so both handle kinds work in <c>using</c> blocks, but only a
/// file handle has anything to release: it owns the open-file state the
/// driver keeps for a read or write position. Disposing a directory handle
/// does nothing, and a directory handle stays usable afterwards.
/// </summary>
public interface IVfsNodeHandle : IDisposable
{
    /// <summary>The name of the node inside its parent directory.</summary>
    string Name { get; }

    /// <summary>The underlying HAL VFS inode.</summary>
    IVfsInode Inode { get; }

    /// <summary>
    /// Reads the node's metadata (size, timestamps, mode).
    /// </summary>
    /// <param name="stat">The node's metadata when the call succeeds.</param>
    /// <returns><see langword="true"/> when the driver produced the metadata;
    /// <see langword="false"/> for a disposed handle.</returns>
    bool TryStat(out VfsStat stat);
}
