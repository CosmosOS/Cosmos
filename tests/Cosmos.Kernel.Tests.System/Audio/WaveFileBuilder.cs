// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;

namespace Cosmos.Kernel.Tests.System.Audio;

/// <summary>
/// Builds a RIFF/WAVE file chunk by chunk, so a test spells out the exact
/// layout it hands the reader: which chunks, in what order, and what length
/// each one declares.
/// </summary>
internal sealed class WaveFileBuilder
{
    /// <summary>The format tag of uncompressed LPCM.</summary>
    internal const ushort PcmTag = 0x0001;

    /// <summary>The format tag of WAVE_FORMAT_EXTENSIBLE.</summary>
    internal const ushort ExtensibleTag = 0xFFFE;

    /// <summary>Bytes before the first chunk: <c>RIFF</c>, the length and <c>WAVE</c>.</summary>
    internal const int RiffHeaderBytes = 12;

    /// <summary>Bytes of a chunk header: the id and the length.</summary>
    internal const int ChunkHeaderBytes = 8;

    /// <summary>Bytes of the PCM <c>fmt </c> chunk body.</summary>
    internal const int FormatBodyBytes = 16;

    /// <summary>Where the frames start in a file holding only <c>fmt </c> and <c>data</c>.</summary>
    internal const int PlainDataOffset = RiffHeaderBytes + ChunkHeaderBytes + FormatBodyBytes + ChunkHeaderBytes;

    private readonly MemoryStream _chunks = new();

    /// <summary>Appends a <c>fmt </c> chunk.</summary>
    internal WaveFileBuilder Format(ushort tag, ushort channels, uint sampleRate, ushort bitsPerSample)
    {
        ushort blockAlign = (ushort)(channels * bitsPerSample / 8);
        byte[] body = new byte[FormatBodyBytes];
        using (BinaryWriter writer = new(new MemoryStream(body)))
        {
            writer.Write(tag);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * blockAlign);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
        }

        return Chunk("fmt ", body);
    }

    /// <summary>Appends a <c>data</c> chunk.</summary>
    /// <param name="frames">The bytes the chunk carries.</param>
    /// <param name="declaredLength">The length the header claims, when it is not the body's.</param>
    internal WaveFileBuilder Data(byte[] frames, uint? declaredLength = null)
    {
        return Chunk("data", frames, declaredLength);
    }

    /// <summary>Appends a chunk, with the pad byte RIFF puts after an odd-length body.</summary>
    /// <param name="id">The four-character id.</param>
    /// <param name="body">The bytes the chunk carries.</param>
    /// <param name="declaredLength">The length the header claims, when it is not the body's.</param>
    internal WaveFileBuilder Chunk(string id, byte[] body, uint? declaredLength = null)
    {
        _chunks.Write(Encoding.ASCII.GetBytes(id));
        _chunks.Write(BitConverter.GetBytes(declaredLength ?? (uint)body.Length));
        _chunks.Write(body);
        if ((body.Length & 1) != 0)
        {
            _chunks.WriteByte(0);
        }

        return this;
    }

    /// <summary>The whole file: the RIFF header, then the chunks in the order they were appended.</summary>
    internal byte[] ToArray()
    {
        byte[] chunks = _chunks.ToArray();
        using MemoryStream file = new();
        file.Write(Encoding.ASCII.GetBytes("RIFF"));
        file.Write(BitConverter.GetBytes((uint)(4 + chunks.Length)));
        file.Write(Encoding.ASCII.GetBytes("WAVE"));
        file.Write(chunks);
        return file.ToArray();
    }

    /// <summary>A PCM file of signed 16-bit stereo at 48 kHz around the given frames.</summary>
    internal static byte[] Stereo16(byte[] frames)
    {
        return new WaveFileBuilder().Format(PcmTag, 2, 48000, 16).Data(frames).ToArray();
    }

    /// <summary>Distinct bytes, so a test can tell which frame it read: byte <c>i</c> is <c>i + 1</c>.</summary>
    internal static byte[] Counting(int length)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = (byte)(i + 1);
        }

        return bytes;
    }
}
