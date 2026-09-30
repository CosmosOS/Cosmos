// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// A finite stream over frames already in memory, and the RIFF/WAVE reader
/// that builds one from a <c>.wav</c> file. Only uncompressed LPCM is read,
/// which is what a <c>.wav</c> almost always carries.
/// </summary>
public sealed class MemoryAudioStream : SeekableAudioStream
{
    private readonly byte[] _data;
    private readonly int _frameCount;

    /// <summary>Wraps frames already in memory.</summary>
    /// <param name="format">The layout the bytes are in.</param>
    /// <param name="sampleRate">Frames per second.</param>
    /// <param name="data">The interleaved frames.</param>
    /// <exception cref="ArgumentException"><paramref name="format"/> names no layout.</exception>
    public MemoryAudioStream(AudioFormat format, uint sampleRate, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (format.IsEmpty)
        {
            throw new ArgumentException("The format names no layout.", nameof(format));
        }

        Format = format;
        SampleRate = sampleRate;
        _data = data;
        _frameCount = data.Length / format.FrameSize;
    }

    /// <inheritdoc/>
    public override AudioFormat Format { get; }

    /// <inheritdoc/>
    public override uint SampleRate { get; }

    /// <inheritdoc/>
    public override uint Position { get; set; }

    /// <inheritdoc/>
    public override uint Length => (uint)_frameCount;

    /// <inheritdoc/>
    public override bool Depleted => Position >= (uint)_frameCount;

    /// <summary>
    /// Reads a PCM-encoded RIFF/WAVE file that is already in memory. For a
    /// file on a volume, <see cref="WaveAudioStream"/> streams it instead of
    /// holding all of it at once.
    /// </summary>
    /// <param name="file">The whole <c>.wav</c> file.</param>
    /// <returns>A stream over the file's frames.</returns>
    /// <exception cref="ArgumentException">The file is not a PCM RIFF/WAVE the reader understands.</exception>
    public static MemoryAudioStream FromWave(byte[] file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!WaveHeader.TryParse(file, out WaveHeader header))
        {
            throw new ArgumentException("Not a PCM RIFF/WAVE file this reader understands.", nameof(file));
        }

        byte[] frames = new byte[header.DataLength];
        Array.Copy(file, header.DataOffset, frames, 0, frames.Length);
        return new MemoryAudioStream(header.Format, header.SampleRate, frames);
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> destination)
    {
        int frameSize = Format.FrameSize;
        int wanted = destination.Length / frameSize;
        int left = _frameCount - (int)Position;
        int frames = wanted < left ? wanted : left;
        if (frames <= 0)
        {
            return 0;
        }

        int bytes = frames * frameSize;
        _data.AsSpan((int)Position * frameSize, bytes).CopyTo(destination);
        Position += (uint)frames;
        return bytes;
    }
}
