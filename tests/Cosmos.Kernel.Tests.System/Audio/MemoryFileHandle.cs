// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.FileSystem;

namespace Cosmos.Kernel.Tests.System.Audio;

/// <summary>
/// An open, read-only file over bytes in memory: the cursor, the seeks and
/// the size a stream reading off a volume relies on, and nothing a stream
/// never touches.
/// </summary>
internal sealed class MemoryFileHandle : IVfsFileHandle
{
    private readonly byte[] _content;

    /// <summary>True once the handle was disposed.</summary>
    internal bool Disposed { get; private set; }

    public string Name => "test.wav";

    public IVfsInode Inode => throw new NotSupportedException("A memory file has no inode.");

    public long Position { get; private set; }

    internal MemoryFileHandle(byte[] content)
    {
        _content = content;
    }

    public long Read(Span<byte> buffer)
    {
        long available = _content.Length - Position;
        int count = available < buffer.Length ? (int)available : buffer.Length;
        if (count <= 0)
        {
            return 0;
        }

        _content.AsSpan((int)Position, count).CopyTo(buffer);
        Position += count;
        return count;
    }

    public long Write(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException("A memory file is read-only.");
    }

    public bool TrySeek(long offset, SeekWhence whence)
    {
        long target = whence switch
        {
            SeekWhence.Set => offset,
            SeekWhence.Cur => Position + offset,
            SeekWhence.End => _content.Length + offset,
            _ => -1,
        };

        if (target < 0)
        {
            return false;
        }

        Position = target;
        return true;
    }

    public bool TryFlush()
    {
        return true;
    }

    public bool TrySetAttr(SetAttrFlags flags, in VfsStat attributes)
    {
        return false;
    }

    public bool TryStat(out VfsStat stat)
    {
        stat = default;
        stat.Size = (ulong)_content.Length;
        stat.Mode = VfsMode.RegularFile;
        return true;
    }

    public void Dispose()
    {
        Disposed = true;
    }
}
