// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Directory and inode metadata operations.
/// </summary>
public interface IInodeOperations
{
    /// <summary>
    /// Resolve a child entry by name within a directory.
    /// </summary>
    /// <param name="dir">Directory to search.</param>
    /// <param name="name">Child name, a single path component.</param>
    /// <param name="child">Resolved child inode, or null when not found.</param>
    /// <returns>true when the name exists in <paramref name="dir"/>.</returns>
    bool Lookup(IVfsInode dir, ReadOnlySpan<char> name, [NotNullWhen(true)] out IVfsInode? child);

    /// <summary>
    /// List the directory's children. <paramref name="entries"/> is
    /// intentionally non-nullable (empty on failure) and read-only:
    /// implementations return fixed or immutable collections; callers
    /// that need to mutate must copy.
    /// </summary>
    bool ReadDir(IVfsInode dir, out IReadOnlyList<IVfsInode> entries);

    /// <summary>
    /// Create an empty regular file in a directory.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the new file, a single path component.</param>
    /// <param name="mode">Permission bits for the new inode.</param>
    /// <param name="inode">Created inode on success; null on failure.</param>
    /// <returns>true on success; false when the name already exists or allocation fails.</returns>
    bool Create(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode);

    /// <summary>
    /// Create an empty subdirectory in a directory.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the new directory, a single path component.</param>
    /// <param name="mode">Permission bits for the new inode.</param>
    /// <param name="inode">Created inode on success; null on failure.</param>
    /// <returns>true on success; false when the name already exists or allocation fails.</returns>
    bool Mkdir(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode);

    /// <summary>
    /// Create a symbolic link pointing at <paramref name="target"/>.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the new link, a single path component.</param>
    /// <param name="target">Path the link points to; stored verbatim, not resolved.</param>
    /// <param name="inode">Created inode on success; null on failure.</param>
    /// <returns>true on success; false when the filesystem does not support symbolic links.</returns>
    bool Symlink(
        IVfsInode dir,
        ReadOnlySpan<char> name,
        ReadOnlySpan<char> target,
        [NotNullWhen(true)] out IVfsInode? inode);

    /// <summary>
    /// Remove the directory entry for a non-directory child and free its storage.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the entry to remove.</param>
    /// <returns>true on success; false when the name does not exist or names a directory.</returns>
    bool Unlink(IVfsInode dir, ReadOnlySpan<char> name);

    /// <summary>
    /// Remove an empty child directory.
    /// </summary>
    /// <param name="dir">Parent directory.</param>
    /// <param name="name">Name of the directory to remove.</param>
    /// <returns>true on success; false when the name does not exist, is not a directory, or is not empty.</returns>
    bool Rmdir(IVfsInode dir, ReadOnlySpan<char> name);

    /// <summary>
    /// Move an entry to a new parent and/or name. Both directories belong
    /// to the same filesystem instance.
    /// </summary>
    /// <param name="oldParent">Directory currently holding the entry.</param>
    /// <param name="oldName">Current name of the entry.</param>
    /// <param name="newParent">Destination directory; may equal <paramref name="oldParent"/>.</param>
    /// <param name="newName">New name for the entry.</param>
    /// <returns>true when the entry was moved.</returns>
    bool Rename(
        IVfsInode oldParent,
        ReadOnlySpan<char> oldName,
        IVfsInode newParent,
        ReadOnlySpan<char> newName);

    /// <summary>
    /// Read the inode's attributes (POSIX <c>stat</c>). The file-type nibble of
    /// <see cref="VfsStat.Mode"/> is mandatory: the VFS reads it to tell a
    /// directory from a file, and a driver that leaves it zero has every
    /// directory open rejected.
    /// </summary>
    /// <param name="inode">Inode to query.</param>
    /// <param name="stat">Populated attributes on success.</param>
    /// <returns>true on success.</returns>
    bool GetAttr(IVfsInode inode, out VfsStat stat);

    /// <summary>
    /// Update the inode attributes selected by <paramref name="flags"/>.
    /// Selecting <see cref="SetAttrFlags.Size"/> truncates or zero-extends
    /// the file; fields without a corresponding flag are ignored.
    /// </summary>
    /// <param name="inode">Inode to modify.</param>
    /// <param name="flags">Which fields of <paramref name="attributes"/> to apply.</param>
    /// <param name="attributes">Source values for the selected fields.</param>
    /// <returns>true on success; false when a selected change is not supported.</returns>
    bool SetAttr(IVfsInode inode, SetAttrFlags flags, in VfsStat attributes);
}
