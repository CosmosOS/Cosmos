// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Vfs;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// A RIFF/WAVE stream read straight off a volume, a block at a time. The
/// chunk list is walked over the file handle rather than over a buffer, so
/// nothing larger than one chunk header is held to find the frames, and a
/// file of any size plays in a fixed amount of memory. The handle stays open
/// for the life of the stream and belongs to whoever opened it.
/// </summary>
public sealed class WaveAudioStream : SeekableAudioStream
{
    /// <summary>
    /// Chunks walked before the search gives up. A well-formed file has a
    /// handful; the bound stops a file whose lengths point in circles.
    /// </summary>
    private const int MaxChunks = 64;

    private readonly IVfsFileHandle _file;
    private readonly WaveHeader _header;
    private readonly uint _frameCount;

    /// <summary>Position in frames, kept next to the handle's byte cursor.</summary>
    private uint _position;

    private WaveAudioStream(IVfsFileHandle file, in WaveHeader header)
    {
        _file = file;
        _header = header;
        _frameCount = header.FrameCount;
    }

    /// <inheritdoc/>
    public override AudioFormat Format => _header.Format;

    /// <inheritdoc/>
    public override uint SampleRate => _header.SampleRate;

    /// <inheritdoc/>
    public override uint Length => _frameCount;

    /// <inheritdoc/>
    public override bool Depleted => _position >= _frameCount;

    /// <summary>The parsed chunk list, for a caller that wants to report what it is about to play.</summary>
    public WaveHeader Header => _header;

    /// <inheritdoc/>
    public override uint Position
    {
        get => _position;
        set
        {
            uint frame = value > _frameCount ? _frameCount : value;
            if (_file.TrySeek(_header.DataOffset + (frame * _header.Format.FrameSize), SeekWhence.Set))
            {
                _position = frame;
            }
        }
    }

    /// <summary>
    /// Walks the file's chunk list and, when it names PCM frames, leaves the
    /// handle positioned at the first one.
    /// </summary>
    /// <param name="file">An open handle, which the stream reads for its whole life.</param>
    /// <param name="stream">The stream over the file's frames.</param>
    /// <returns>False when the file is not a PCM RIFF/WAVE this reader understands.</returns>
    public static bool TryOpen(IVfsFileHandle file, [NotNullWhen(true)] out WaveAudioStream? stream)
    {
        ArgumentNullException.ThrowIfNull(file);
        stream = null;

        Span<byte> header = stackalloc byte[WaveHeader.FirstChunkOffset];
        if (!file.TrySeek(0, SeekWhence.Set) || file.Read(header) != header.Length)
        {
            return false;
        }

        if (!WaveHeader.IsRiffWave(header))
        {
            return false;
        }

        if (!TryWalkChunks(file, out WaveHeader parsed))
        {
            return false;
        }

        if (!file.TrySeek(parsed.DataOffset, SeekWhence.Set))
        {
            return false;
        }

        stream = new WaveAudioStream(file, parsed);
        return true;
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> destination)
    {
        int frameSize = _header.Format.FrameSize;
        long framesLeft = _frameCount - _position;
        int wanted = destination.Length / frameSize;
        int frames = (long)wanted < framesLeft ? wanted : (int)framesLeft;
        if (frames <= 0)
        {
            return 0;
        }

        // A short read from the volume is honoured rather than retried: the
        // caller pads the rest with silence and the next pass picks up where
        // the handle's cursor now is.
        long read = _file.Read(destination[..(frames * frameSize)]);
        if (read <= 0)
        {
            return 0;
        }

        read -= read % frameSize;
        _position += (uint)(read / frameSize);
        return (int)read;
    }

    /// <summary>
    /// Reads each chunk's header in turn, seeking over the bodies, until the
    /// format and the frames are both located.
    /// </summary>
    /// <param name="file">The open handle.</param>
    /// <param name="header">What the chunks declared.</param>
    /// <returns>False when either chunk is missing or the format is one this reader has no layout for.</returns>
    private static bool TryWalkChunks(IVfsFileHandle file, out WaveHeader header)
    {
        header = default;

        AudioFormat format = default;
        uint sampleRate = 0;
        long dataOffset = -1;
        long dataLength = 0;

        long offset = WaveHeader.FirstChunkOffset;
        Span<byte> chunk = stackalloc byte[WaveHeader.ChunkHeaderBytes];

        for (int walked = 0; walked < MaxChunks; walked++)
        {
            if (!file.TrySeek(offset, SeekWhence.Set) || file.Read(chunk) != chunk.Length)
            {
                break;
            }

            long length = BitConverter.ToUInt32(chunk[4..]);
            long body = offset + WaveHeader.ChunkHeaderBytes;

            if (WaveHeader.Tagged(chunk, 0, "fmt "))
            {
                int wanted = length > WaveHeader.MinimumFormatChunkBytes
                    ? WaveHeader.MinimumFormatChunkBytes
                    : (int)length;
                Span<byte> fields = stackalloc byte[WaveHeader.MinimumFormatChunkBytes];
                if (file.Read(fields[..wanted]) != wanted)
                {
                    return false;
                }

                if (!WaveHeader.TryReadFormatChunk(fields, out format, out sampleRate))
                {
                    return false;
                }
            }
            else if (WaveHeader.Tagged(chunk, 0, "data"))
            {
                dataOffset = body;
                dataLength = length;

                // The frames are the last thing that matters, and a data
                // chunk whose length runs past the file is clamped below.
                break;
            }

            offset = body + length + (length & 1);
        }

        if (format.IsEmpty || dataOffset < 0)
        {
            return false;
        }

        // Clamp a length that runs past the end, which is how a recording cut
        // short leaves its file, then trim a trailing partial frame.
        if (file.TryStat(out VfsStat stat) && (long)stat.Size > dataOffset)
        {
            long available = (long)stat.Size - dataOffset;
            if (dataLength > available)
            {
                dataLength = available;
            }
        }

        dataLength -= dataLength % format.FrameSize;
        if (dataLength <= 0)
        {
            return false;
        }

        header = new WaveHeader(format, sampleRate, dataOffset, dataLength);
        return true;
    }
}
