// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Audio;

/// <summary>
/// What the reader needs out of a RIFF/WAVE file's chunk list: the frame
/// layout, the rate, and where the frames themselves begin and end. A
/// <c>.wav</c> is a chunk list rather than a fixed header, so the chunks
/// between <c>fmt </c> and <c>data</c> are walked rather than assumed away:
/// a file carrying a <c>LIST</c> or <c>fact</c> chunk reads like any other.
/// </summary>
public readonly struct WaveHeader
{
    /// <summary>Bytes of a chunk header: the four-character id and the length.</summary>
    internal const int ChunkHeaderBytes = 8;

    /// <summary>Offset of the first chunk, past <c>RIFF</c>, the length and <c>WAVE</c>.</summary>
    internal const int FirstChunkOffset = 12;

    /// <summary>Smallest <c>fmt </c> chunk the reader accepts: the PCM fields.</summary>
    internal const int MinimumFormatChunkBytes = 16;

    /// <summary>The format tag of uncompressed LPCM.</summary>
    private const ushort FormatPcm = 0x0001;

    /// <summary>The format tag of WAVE_FORMAT_EXTENSIBLE, whose sub-format is LPCM in practice.</summary>
    private const ushort FormatExtensible = 0xFFFE;

    /// <summary>Records a parsed header.</summary>
    /// <param name="format">The frame layout.</param>
    /// <param name="sampleRate">Frames per second.</param>
    /// <param name="dataOffset">Byte offset of the first frame in the file.</param>
    /// <param name="dataLength">Bytes of frames, a whole number of them.</param>
    internal WaveHeader(AudioFormat format, uint sampleRate, long dataOffset, long dataLength)
    {
        Format = format;
        SampleRate = sampleRate;
        DataOffset = dataOffset;
        DataLength = dataLength;
    }

    /// <summary>The frame layout the file's samples are in.</summary>
    public AudioFormat Format { get; }

    /// <summary>Frames per second the file declares.</summary>
    public uint SampleRate { get; }

    /// <summary>Byte offset of the first frame in the file.</summary>
    public long DataOffset { get; }

    /// <summary>Bytes of frames, trimmed to a whole number of them.</summary>
    public long DataLength { get; }

    /// <summary>How many frames the file holds.</summary>
    public uint FrameCount => Format.IsEmpty ? 0 : (uint)(DataLength / Format.FrameSize);

    /// <summary>Whether the four bytes at <paramref name="offset"/> are the given ASCII chunk id.</summary>
    /// <param name="bytes">The buffer to look in.</param>
    /// <param name="offset">Where the id should be.</param>
    /// <param name="id">The four-character id.</param>
    internal static bool Tagged(ReadOnlySpan<byte> bytes, int offset, string id)
    {
        if (offset < 0 || offset + id.Length > bytes.Length)
        {
            return false;
        }

        for (int i = 0; i < id.Length; i++)
        {
            if (bytes[offset + i] != (byte)id[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Whether a buffer opens with the <c>RIFF</c>/<c>WAVE</c> form.</summary>
    /// <param name="prefix">At least <see cref="FirstChunkOffset"/> bytes from the file's start.</param>
    internal static bool IsRiffWave(ReadOnlySpan<byte> prefix) =>
        prefix.Length >= FirstChunkOffset && Tagged(prefix, 0, "RIFF") && Tagged(prefix, 8, "WAVE");

    /// <summary>
    /// Decodes the PCM fields of a <c>fmt </c> chunk body.
    /// </summary>
    /// <param name="body">The chunk's bytes, past its header.</param>
    /// <param name="format">The frame layout the chunk declares.</param>
    /// <param name="sampleRate">Frames per second the chunk declares.</param>
    /// <returns>False when the chunk is short, compressed, or names a width this reader has no layout for.</returns>
    internal static bool TryReadFormatChunk(ReadOnlySpan<byte> body, out AudioFormat format, out uint sampleRate)
    {
        format = default;
        sampleRate = 0;

        if (body.Length < MinimumFormatChunkBytes)
        {
            return false;
        }

        ushort tag = BitConverter.ToUInt16(body[..2]);
        if (tag != FormatPcm && tag != FormatExtensible)
        {
            return false;
        }

        ushort channels = BitConverter.ToUInt16(body.Slice(2, 2));
        if (channels == 0 || channels > byte.MaxValue)
        {
            return false;
        }

        sampleRate = BitConverter.ToUInt32(body.Slice(4, 4));
        ushort bitsPerSample = BitConverter.ToUInt16(body.Slice(14, 2));
        AudioBitDepth depth = bitsPerSample switch
        {
            8 => AudioBitDepth.Bits8,
            16 => AudioBitDepth.Bits16,
            24 => AudioBitDepth.Bits24,
            32 => AudioBitDepth.Bits32,
            _ => default,
        };

        if (depth == default)
        {
            return false;
        }

        // Only 8-bit PCM is unsigned, by the format's own convention.
        format = new AudioFormat(depth, (byte)channels, depth != AudioBitDepth.Bits8);
        return true;
    }

    /// <summary>
    /// Walks the chunk list of a whole file already in memory.
    /// </summary>
    /// <param name="file">The whole <c>.wav</c> file.</param>
    /// <param name="header">What the chunks declared.</param>
    /// <returns>False when the buffer is not a PCM RIFF/WAVE this reader understands.</returns>
    public static bool TryParse(ReadOnlySpan<byte> file, out WaveHeader header)
    {
        header = default;

        if (!IsRiffWave(file))
        {
            return false;
        }

        AudioFormat format = default;
        uint sampleRate = 0;
        long dataOffset = -1;
        long dataLength = 0;

        int offset = FirstChunkOffset;
        while (offset + ChunkHeaderBytes <= file.Length)
        {
            uint declared = BitConverter.ToUInt32(file.Slice(offset + 4, 4));
            int body = offset + ChunkHeaderBytes;
            int available = file.Length - body;
            int length = declared > (uint)available ? available : (int)declared;

            if (Tagged(file, offset, "fmt "))
            {
                if (!TryReadFormatChunk(file.Slice(body, length), out format, out sampleRate))
                {
                    return false;
                }
            }
            else if (Tagged(file, offset, "data"))
            {
                dataOffset = body;
                dataLength = length;
            }

            offset = body + length + (length & 1);
        }

        if (format.IsEmpty || dataOffset < 0)
        {
            return false;
        }

        // Trim a trailing partial frame rather than play half of one.
        dataLength -= dataLength % format.FrameSize;
        header = new WaveHeader(format, sampleRate, dataOffset, dataLength);
        return true;
    }
}
