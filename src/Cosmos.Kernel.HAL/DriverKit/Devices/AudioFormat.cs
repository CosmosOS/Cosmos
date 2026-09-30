// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// The layout of one audio frame: how many bits a channel holds, how many
/// channels a frame interleaves, and whether the samples are signed. A
/// format is not a rate: <see cref="IAudioOutput.SampleRate"/> carries that,
/// because the same layout plays at every rate the codec accepts.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct AudioFormat : IEquatable<AudioFormat>
{
    /// <summary>Creates a format.</summary>
    /// <param name="bitDepth">Bits per channel.</param>
    /// <param name="channels">Channels per frame, interleaved.</param>
    /// <param name="signed">Whether a sample can be negative. By convention only 8-bit audio is unsigned.</param>
    public AudioFormat(AudioBitDepth bitDepth, byte channels, bool signed)
    {
        BitDepth = bitDepth;
        Channels = channels;
        Signed = signed;
    }

    /// <summary>Signed 16-bit stereo: the format every output here supports and the one a WAV usually carries.</summary>
    public static AudioFormat Stereo16 => new(AudioBitDepth.Bits16, 2, true);

    /// <summary>Bits per channel.</summary>
    public AudioBitDepth BitDepth { get; }

    /// <summary>Channels per frame.</summary>
    public byte Channels { get; }

    /// <summary>Whether a sample can be negative.</summary>
    public bool Signed { get; }

    /// <summary>Bytes in one channel of a frame.</summary>
    public int ChannelSize => (int)BitDepth;

    /// <summary>Bytes in one whole frame: <see cref="ChannelSize"/> times <see cref="Channels"/>.</summary>
    public int FrameSize => (int)BitDepth * Channels;

    /// <summary>True for the default value, which names no layout.</summary>
    public bool IsEmpty => Channels == 0 || BitDepth == 0;

    /// <summary>Whether two formats name the same layout.</summary>
    /// <param name="left">The first format.</param>
    /// <param name="right">The second format.</param>
    public static bool operator ==(AudioFormat left, AudioFormat right) => left.Equals(right);

    /// <summary>Whether two formats name different layouts.</summary>
    /// <param name="left">The first format.</param>
    /// <param name="right">The second format.</param>
    public static bool operator !=(AudioFormat left, AudioFormat right) => !left.Equals(right);

    /// <inheritdoc/>
    public bool Equals(AudioFormat other) =>
        BitDepth == other.BitDepth && Channels == other.Channels && Signed == other.Signed;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AudioFormat other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => ((int)BitDepth << 16) | (Channels << 1) | (Signed ? 1 : 0);
}
