// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Per-superblock callbacks.
/// </summary>
public interface ISuperblockOperations
{
    /// <summary>
    /// Flush all dirty filesystem state to the backing store so completed
    /// writes are durable (POSIX <c>sync</c>).
    /// </summary>
    /// <param name="superblock">Mount to synchronize.</param>
    /// <returns>true on success.</returns>
    bool Sync(IVfsSuperblock superblock);

    /// <summary>
    /// Read filesystem-level statistics (POSIX <c>statfs</c>).
    /// </summary>
    /// <param name="superblock">Mount to query.</param>
    /// <param name="statFs">Populated statistics on success.</param>
    /// <returns>true on success.</returns>
    bool StatFs(IVfsSuperblock superblock, out VfsStatFs statFs);

    /// <summary>
    /// Tear down this mount (unmount); analogous to <c>kill_sb</c> / final put.
    /// </summary>
    void Drop(IVfsSuperblock superblock);
}
