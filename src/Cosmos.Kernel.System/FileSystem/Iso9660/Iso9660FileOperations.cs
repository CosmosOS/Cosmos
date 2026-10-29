// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Iso9660;

/// <summary>
/// Open-file byte I/O for ISO 9660 files: a read is one range of the file's
/// extent, handed to the superblock, which reads the whole device blocks in
/// one command and bounces only the partial ones at either end. Position
/// advancement is the caller's (the Linux VFS convention). Writes return 0:
/// the VFS refuses them before they get here.
/// </summary>
internal sealed class Iso9660FileOperations : IFileOperations
{
    private readonly Iso9660Superblock _superblock;

    /// <summary>
    /// Creates file operations bound to a mounted volume.
    /// </summary>
    /// <param name="superblock">The mounted volume.</param>
    public Iso9660FileOperations(Iso9660Superblock superblock)
    {
        _superblock = superblock;
    }

    /// <inheritdoc />
    public long Read(IVfsOpenFile openFile, Span<byte> buffer)
    {
        if (openFile.Inode is not Iso9660Inode inode || inode.IsDirectory)
        {
            return 0;
        }

        long position = openFile.Position;
        if (position < 0 || position >= inode.DataLength || buffer.Length == 0)
        {
            return 0;
        }

        int toRead = (int)Math.Min(buffer.Length, inode.DataLength - position);
        ulong volumeOffset = ((ulong)inode.Extent * _superblock.BlockSize) + (ulong)position;
        _superblock.ReadBytes(volumeOffset, buffer.Slice(0, toRead));
        return toRead;
    }

    /// <inheritdoc />
    public long Write(IVfsOpenFile openFile, ReadOnlySpan<byte> buffer)
    {
        return 0;
    }

    /// <inheritdoc />
    public bool Seek(IVfsOpenFile openFile, long offset, SeekWhence whence, out long newPosition)
    {
        newPosition = 0;
        if (openFile.Inode is not Iso9660Inode inode)
        {
            return false;
        }

        long basePosition = whence switch
        {
            SeekWhence.Set => 0,
            SeekWhence.Cur => openFile.Position,
            SeekWhence.End => inode.DataLength,
            _ => -1,
        };

        if (basePosition < 0)
        {
            return false;
        }

        long target = basePosition + offset;
        if (target < 0)
        {
            return false;
        }

        openFile.Position = target;
        newPosition = target;
        return true;
    }

    /// <inheritdoc />
    public bool Fsync(IVfsOpenFile openFile)
    {
        // Nothing is ever dirty on a read-only volume.
        return true;
    }

    /// <inheritdoc />
    public void Release(IVfsOpenFile openFile)
    {
        // An open file holds no driver state beyond its inode.
    }
}
