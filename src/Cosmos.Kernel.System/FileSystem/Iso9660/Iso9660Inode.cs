// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// VFS inode over one ISO 9660 directory record: the record's extent, length
/// and flags, the name a reader sees, the recording date, what Rock Ridge
/// added, and for a directory the children parsed on first listing and kept
/// for the mount's lifetime, since the medium does not change under it.
/// </summary>
internal sealed class Iso9660Inode : IVfsInode
{
    /// <summary>The mounted volume owning this inode.</summary>
    public Iso9660Superblock Superblock { get; }

    /// <summary>The directory holding this entry; null for the root.</summary>
    public Iso9660Inode? Parent { get; }

    /// <summary>
    /// The byte offset of the directory record on the volume, which is
    /// unique and stable: an extent is not, since every empty file and every
    /// symbolic link shares one.
    /// </summary>
    public ulong InodeNumber { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>First logical block of the entry's extent; for a relocated directory, the real one.</summary>
    public uint Extent { get; }

    /// <summary>Byte length of the extent.</summary>
    public uint DataLength { get; }

    /// <summary>True when the entry is a directory.</summary>
    public bool IsDirectory { get; }

    /// <summary>True when the entry is a symbolic link: Rock Ridge gave it a target.</summary>
    public bool IsSymbolicLink => RockRidge.SymbolicLinkTarget is not null;

    /// <summary>The 7-byte recording date as Unix seconds, the stamp a volume without Rock Ridge has.</summary>
    public long RecordingTime { get; }

    /// <summary>What Rock Ridge recorded for the entry.</summary>
    public RockRidgeAttributes RockRidge { get; }

    /// <summary>The children of a directory once listed; null until then, and for a file.</summary>
    internal Iso9660Inode[]? Children { get; set; }

    /// <inheritdoc />
    public IInodeOperations InodeOperations => Superblock.InodeOps;

    /// <inheritdoc />
    public IFileOperations? FileOperations => IsDirectory || IsSymbolicLink ? null : Superblock.FileOps;

    /// <summary>
    /// Creates an inode.
    /// </summary>
    /// <param name="superblock">The mounted volume.</param>
    /// <param name="parent">The directory holding the entry; null for the root.</param>
    /// <param name="inodeNumber">The byte offset of the directory record on the volume.</param>
    /// <param name="name">The name a reader sees.</param>
    /// <param name="extent">First logical block of the extent.</param>
    /// <param name="dataLength">Byte length of the extent.</param>
    /// <param name="isDirectory">True for a directory.</param>
    /// <param name="recordingTime">The recording date as Unix seconds.</param>
    /// <param name="rockRidge">What Rock Ridge recorded.</param>
    public Iso9660Inode(
        Iso9660Superblock superblock,
        Iso9660Inode? parent,
        ulong inodeNumber,
        string name,
        uint extent,
        uint dataLength,
        bool isDirectory,
        long recordingTime,
        RockRidgeAttributes rockRidge)
    {
        Superblock = superblock;
        Parent = parent;
        InodeNumber = inodeNumber;
        Name = name;
        Extent = extent;
        DataLength = dataLength;
        IsDirectory = isDirectory;
        RecordingTime = recordingTime;
        RockRidge = rockRidge;
    }
}
