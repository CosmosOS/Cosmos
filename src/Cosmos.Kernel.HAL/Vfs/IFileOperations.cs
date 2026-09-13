// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Vfs;

/// <summary>
/// Open-file byte I/O.
/// </summary>
public interface IFileOperations
{
    /// <summary>Bytes read; <c>0</c> indicates end-of-file.</summary>
    long Read(IVfsOpenFile openFile, Span<byte> buffer);

    /// <summary>Bytes written.</summary>
    long Write(IVfsOpenFile openFile, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Compute and apply a new file position from <paramref name="offset"/>
    /// relative to <paramref name="whence"/>. Positions beyond end-of-file
    /// are valid; a later write zero-fills the gap.
    /// </summary>
    /// <param name="openFile">Open file whose position is moved.</param>
    /// <param name="offset">Byte offset relative to <paramref name="whence"/>.</param>
    /// <param name="whence">Origin the offset is applied to.</param>
    /// <param name="newPosition">Resulting absolute position; 0 on failure.</param>
    /// <returns>true on success; false when the resulting position would be negative.</returns>
    bool Seek(IVfsOpenFile openFile, long offset, SeekWhence whence, out long newPosition);

    /// <summary>
    /// Flush the file's data and metadata to the backing store so completed
    /// writes are durable (POSIX <c>fsync</c>).
    /// </summary>
    /// <param name="openFile">Open file to synchronize.</param>
    /// <returns>true on success.</returns>
    bool Fsync(IVfsOpenFile openFile);

    /// <summary>
    /// Release driver-side state when the open file is closed.
    /// Implementations typically flush pending writes first.
    /// </summary>
    /// <param name="openFile">Open file being closed.</param>
    void Release(IVfsOpenFile openFile);
}
