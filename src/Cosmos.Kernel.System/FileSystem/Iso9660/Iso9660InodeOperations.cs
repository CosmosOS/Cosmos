// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Directory and metadata operations for an ISO 9660 volume. Names match
/// exactly on a Rock Ridge volume, where they are POSIX names, and without
/// regard to ASCII case on a plain one, where the standard's identifiers
/// are upper case by definition. Every mutation is refused: the volume is
/// read-only.
/// </summary>
internal sealed class Iso9660InodeOperations : IInodeOperations
{
    /// <summary>Unit of <see cref="VfsStat.Blocks"/>: 512-byte sectors, whatever the block size.</summary>
    private const uint SectorSize = 512;

    /// <summary>Mode of a directory on a volume without Rock Ridge: readable and searchable by all.</summary>
    private const VfsMode DefaultDirectoryMode = VfsMode.Directory
        | VfsMode.OwnerRead | VfsMode.OwnerExecute
        | VfsMode.GroupRead | VfsMode.GroupExecute
        | VfsMode.OtherRead | VfsMode.OtherExecute;

    /// <summary>Mode of a file on a volume without Rock Ridge: readable by all.</summary>
    private const VfsMode DefaultFileMode = VfsMode.RegularFile | VfsMode.OwnerRead | VfsMode.GroupRead | VfsMode.OtherRead;

    /// <summary>The write bits, cleared from every mode: nothing on the volume can be written.</summary>
    private const VfsMode WriteBits = VfsMode.OwnerWrite | VfsMode.GroupWrite | VfsMode.OtherWrite;

    private readonly Iso9660Superblock _superblock;

    /// <summary>
    /// Creates inode operations bound to a mounted volume.
    /// </summary>
    /// <param name="superblock">The mounted volume.</param>
    public Iso9660InodeOperations(Iso9660Superblock superblock)
    {
        _superblock = superblock;
    }

    /// <inheritdoc />
    public bool Lookup(IVfsInode dir, ReadOnlySpan<char> name, [NotNullWhen(true)] out IVfsInode? child)
    {
        child = null;
        if (dir is not Iso9660Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        if (name.Length == 1 && name[0] == '.')
        {
            child = parent;
            return true;
        }

        if (name.Length == 2 && name[0] == '.' && name[1] == '.')
        {
            child = parent.Parent ?? parent;
            return true;
        }

        Iso9660Inode[] children = GetChildren(parent);
        for (int i = 0; i < children.Length; i++)
        {
            if (NamesEqual(children[i].Name, name))
            {
                child = children[i];
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public bool ReadDir(IVfsInode dir, out IReadOnlyList<IVfsInode> entries)
    {
        entries = [];
        if (dir is not Iso9660Inode parent || !parent.IsDirectory)
        {
            return false;
        }

        Iso9660Inode[] children = GetChildren(parent);

        // A copy, not the cached array itself: an array is covariantly an
        // IVfsInode[], so handing it out would let a caller's downcast
        // rewrite the directory cache every later lookup reads.
        List<IVfsInode> result = new(children.Length);
        for (int i = 0; i < children.Length; i++)
        {
            result.Add(children[i]);
        }

        entries = result;
        return true;
    }

    /// <inheritdoc />
    public bool Create(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        return false;
    }

    /// <inheritdoc />
    public bool Mkdir(IVfsInode dir, ReadOnlySpan<char> name, VfsMode mode, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        return false;
    }

    /// <inheritdoc />
    public bool Symlink(IVfsInode dir, ReadOnlySpan<char> name, ReadOnlySpan<char> target, [NotNullWhen(true)] out IVfsInode? inode)
    {
        inode = null;
        return false;
    }

    /// <inheritdoc />
    public bool Unlink(IVfsInode dir, ReadOnlySpan<char> name)
    {
        return false;
    }

    /// <inheritdoc />
    public bool Rmdir(IVfsInode dir, ReadOnlySpan<char> name)
    {
        return false;
    }

    /// <inheritdoc />
    public bool Rename(IVfsInode oldParent, ReadOnlySpan<char> oldName, IVfsInode newParent, ReadOnlySpan<char> newName)
    {
        return false;
    }

    /// <inheritdoc />
    public bool GetAttr(IVfsInode inode, out VfsStat stat)
    {
        stat = default;
        if (inode is not Iso9660Inode node)
        {
            return false;
        }

        RockRidgeAttributes rockRidge = node.RockRidge;
        stat.InodeNumber = node.InodeNumber;
        stat.Mode = ToVfsMode(node);
        stat.LinkCount = rockRidge.HasPosixAttributes && rockRidge.LinkCount > 0 ? rockRidge.LinkCount : 1;
        stat.Uid = rockRidge.HasPosixAttributes ? rockRidge.Uid : 0;
        stat.Gid = rockRidge.HasPosixAttributes ? rockRidge.Gid : 0;
        stat.DeviceId = 0;
        stat.Size = node.DataLength;
        stat.PreferredBlockSize = _superblock.BlockSize;
        uint blockSize = _superblock.BlockSize;
        ulong logicalBlocks = ((ulong)node.DataLength + blockSize - 1) / blockSize;
        stat.Blocks = logicalBlocks * (blockSize / SectorSize);
        stat.ModificationTime = new VfsTimespec(rockRidge.HasModificationTime ? rockRidge.ModificationTime : node.RecordingTime, 0);
        stat.AccessTime = new VfsTimespec(rockRidge.HasAccessTime ? rockRidge.AccessTime : node.RecordingTime, 0);
        stat.ChangeTime = new VfsTimespec(rockRidge.HasChangeTime ? rockRidge.ChangeTime : node.RecordingTime, 0);
        return true;
    }

    /// <inheritdoc />
    public bool SetAttr(IVfsInode inode, SetAttrFlags flags, in VfsStat attributes)
    {
        return false;
    }

    /// <inheritdoc />
    public bool TryReadLink(IVfsInode symlink, [NotNullWhen(true)] out string? target)
    {
        target = null;
        if (symlink is not Iso9660Inode node)
        {
            return false;
        }

        target = node.RockRidge.SymbolicLinkTarget;
        return target is not null;
    }

    /// <summary>
    /// The directory's entries, parsed on the first call and kept: the
    /// medium does not change under the mount.
    /// </summary>
    private Iso9660Inode[] GetChildren(Iso9660Inode directory)
    {
        return directory.Children ??= Iso9660DirectoryReader.ReadChildren(_superblock, directory);
    }

    private bool NamesEqual(string entryName, ReadOnlySpan<char> name)
    {
        if (_superblock.HasRockRidge)
        {
            return name.SequenceEqual(entryName);
        }

        if (entryName.Length != name.Length)
        {
            return false;
        }

        for (int i = 0; i < name.Length; i++)
        {
            if (ToUpperAscii(entryName[i]) != ToUpperAscii(name[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static char ToUpperAscii(char c)
    {
        return c >= 'a' && c <= 'z' ? (char)(c - ('a' - 'A')) : c;
    }

    /// <summary>
    /// The mode a reader sees: the Rock Ridge mode with its write bits
    /// cleared, so that <c>FileInfo.IsReadOnly</c> is true on a CD, or the
    /// defaults of a plain volume. The type is the record's own: a
    /// placeholder is a directory whatever its <c>PX</c> says.
    /// </summary>
    private static VfsMode ToVfsMode(Iso9660Inode node)
    {
        if (!node.RockRidge.HasPosixAttributes)
        {
            return node.IsDirectory ? DefaultDirectoryMode : DefaultFileMode;
        }

        uint posixMode = node.RockRidge.Mode;
        VfsMode type = (posixMode & Iso9660Layout.PosixTypeMask) switch
        {
            Iso9660Layout.PosixDirectory => VfsMode.Directory,
            Iso9660Layout.PosixSymbolicLink => VfsMode.SymbolicLink,
            _ => VfsMode.RegularFile,
        };

        if (node.IsDirectory)
        {
            type = VfsMode.Directory;
        }
        else if (node.IsSymbolicLink)
        {
            type = VfsMode.SymbolicLink;
        }

        // The permission bits occupy the same low 12 bits in both encodings.
        VfsMode permissions = (VfsMode)(posixMode & Iso9660Layout.PosixPermissionMask) & ~WriteBits;
        return type | permissions;
    }
}
