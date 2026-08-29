// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Vfs;

namespace Cosmos.Kernel.System.Vfs;

/// <summary>
/// Managed handle for an open file with position and byte I/O. Disposal is
/// the one state the two halves of this interface report differently: every
/// <c>Try</c> member answers <see langword="false"/> for a disposed handle,
/// because that is what its bool already means, while
/// <see cref="Read(Span{byte})"/> and <see cref="Write(ReadOnlySpan{byte})"/>
/// throw, having no count that means "the handle is gone" rather than "no
/// bytes moved".
/// </summary>
public interface IVfsFileHandle : IVfsNodeHandle
{
    /// <summary>The current byte offset of the file cursor.</summary>
    long Position { get; }

    /// <summary>
    /// Reads bytes at the current position, advancing it by the amount read.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <returns>The number of bytes read; 0 at end of file, and also when the
    /// driver cannot resolve the file's storage.</returns>
    /// <exception cref="ObjectDisposedException">The handle has been disposed.</exception>
    long Read(Span<byte> buffer);

    /// <summary>
    /// Writes bytes at the current position, advancing it by the amount written.
    /// </summary>
    /// <param name="buffer">The bytes to write.</param>
    /// <returns>The number of bytes written, which is short of
    /// <paramref name="buffer"/> when the volume runs out of room.</returns>
    /// <exception cref="ObjectDisposedException">The handle has been disposed.</exception>
    long Write(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Moves the file cursor.
    /// </summary>
    /// <param name="offset">The offset relative to <paramref name="whence"/>.</param>
    /// <param name="whence">The origin the offset is applied from.</param>
    /// <returns><see langword="true"/> when the resulting position is valid;
    /// <see langword="false"/> for a disposed handle.</returns>
    bool TrySeek(long offset, SeekWhence whence);

    /// <summary>
    /// Flushes buffered writes to the underlying device.
    /// </summary>
    /// <returns><see langword="true"/> when the driver flushed successfully;
    /// <see langword="false"/> for a disposed handle.</returns>
    bool TryFlush();

    /// <summary>
    /// Updates the open file's metadata. Setting
    /// <see cref="SetAttrFlags.Size"/> is how a file is truncated or extended.
    /// </summary>
    /// <param name="flags">Which fields of <paramref name="attributes"/> to apply.</param>
    /// <param name="attributes">The new attribute values.</param>
    /// <returns><see langword="true"/> when the driver applied the change;
    /// <see langword="false"/> for a disposed handle.</returns>
    bool TrySetAttr(SetAttrFlags flags, in VfsStat attributes);
}
