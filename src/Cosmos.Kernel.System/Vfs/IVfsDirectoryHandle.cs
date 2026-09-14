// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Vfs;

/// <summary>
/// Managed handle for a directory node supporting lookup and mutation operations.
/// </summary>
public interface IVfsDirectoryHandle : IVfsNodeHandle
{
    /// <summary>
    /// Lists the directory's entries.
    /// </summary>
    /// <param name="entries">The child inodes when the call succeeds.</param>
    /// <returns><see langword="true"/> when the driver produced the listing.</returns>
    bool TryReadDir(out IReadOnlyList<IVfsInode> entries);

    /// <summary>
    /// Resolves a child entry by name.
    /// </summary>
    /// <param name="name">The child's name.</param>
    /// <param name="child">A handle on the child when it exists.</param>
    /// <returns><see langword="true"/> when the entry exists.</returns>
    bool TryLookup(ReadOnlySpan<char> name, [NotNullWhen(true)] out IVfsNodeHandle? child);

    /// <summary>
    /// Creates a file entry in this directory.
    /// </summary>
    /// <param name="name">The new file's name.</param>
    /// <param name="mode">The mode bits of the new file.</param>
    /// <param name="child">A handle on the created file.</param>
    /// <returns><see langword="true"/> when the file was created.</returns>
    bool TryCreateFile(ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsNodeHandle? child);

    /// <summary>
    /// Creates a subdirectory in this directory.
    /// </summary>
    /// <param name="name">The new directory's name.</param>
    /// <param name="mode">The mode bits of the new directory.</param>
    /// <param name="child">A handle on the created directory.</param>
    /// <returns><see langword="true"/> when the directory was created.</returns>
    bool TryCreateDirectory(ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsDirectoryHandle? child);

    /// <summary>
    /// Creates a symbolic link in this directory.
    /// </summary>
    /// <param name="name">The link's name.</param>
    /// <param name="target">The path the link points to.</param>
    /// <param name="child">A handle on the created link.</param>
    /// <returns><see langword="true"/> when the link was created.</returns>
    bool TrySymlink(ReadOnlySpan<char> name, ReadOnlySpan<char> target, [NotNullWhen(true)] out IVfsNodeHandle? child);

    /// <summary>
    /// Removes a file entry from this directory.
    /// </summary>
    /// <param name="name">The entry's name.</param>
    /// <returns><see langword="true"/> when the entry was removed.</returns>
    bool TryUnlink(ReadOnlySpan<char> name);

    /// <summary>
    /// Removes an empty subdirectory from this directory.
    /// </summary>
    /// <param name="name">The subdirectory's name.</param>
    /// <returns><see langword="true"/> when the directory was removed.</returns>
    bool TryRemoveDirectory(ReadOnlySpan<char> name);

    /// <summary>
    /// Moves or renames an entry of this directory into <paramref name="newParent"/>.
    /// </summary>
    /// <param name="oldName">The entry's current name in this directory.</param>
    /// <param name="newParent">The directory receiving the entry; may be this handle.</param>
    /// <param name="newName">The entry's new name.</param>
    /// <returns><see langword="true"/> when the entry was moved.</returns>
    bool TryRename(ReadOnlySpan<char> oldName, IVfsDirectoryHandle newParent, ReadOnlySpan<char> newName);

    /// <summary>
    /// Updates this directory's metadata.
    /// </summary>
    /// <param name="flags">Which fields of <paramref name="attributes"/> to apply.</param>
    /// <param name="attributes">The new attribute values.</param>
    /// <returns><see langword="true"/> when the driver applied the change.</returns>
    bool TrySetAttr(SetAttrFlags flags, in VfsStat attributes);
}
