// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Vfs;

/// <summary>
/// Common surface for VFS nodes (files or directories). Every handle owns
/// driver state and must be released, so the base interface is disposable and
/// both handle kinds work in <c>using</c> blocks.
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
