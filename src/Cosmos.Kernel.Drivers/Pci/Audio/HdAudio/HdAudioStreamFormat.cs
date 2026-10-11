// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Audio;

namespace Cosmos.Kernel.Drivers.Pci.Audio.HdAudio;

/// <summary>
/// The stream format word an HD Audio stream descriptor and its converter
/// both take: a rate expressed as one of two base clocks with a multiplier
/// and a divisor, a sample width, and a channel count. The rates below are
/// the ones the specification derives from 48 kHz and 44.1 kHz; a rate that
/// is not among them cannot be expressed and is refused rather than rounded,
/// because a converter programmed for the wrong rate plays at the wrong
/// pitch instead of failing.
/// </summary>
internal static class HdAudioStreamFormat
{
    /// <summary>The rate a freshly bound controller is programmed for.</summary>
    internal const uint DefaultSampleRate = 48000;

    /// <summary>The 44.1 kHz base clock; clear for the 48 kHz family.</summary>
    private const ushort Base44100 = 1 << 14;

    /// <summary>Shift of the multiplier field.</summary>
    private const int MultiplierShift = 11;

    /// <summary>Shift of the divisor field.</summary>
    private const int DivisorShift = 8;

    /// <summary>Shift of the sample width field.</summary>
    private const int BitsShift = 4;

    /// <summary>Sample width field: 16 bits.</summary>
    private const ushort Bits16 = 0x1;

    /// <summary>The highest channel count the four-bit field can hold.</summary>
    private const int MaxChannels = 16;

    /// <summary>
    /// Encodes <paramref name="format"/> at <paramref name="sampleRate"/>.
    /// </summary>
    /// <param name="format">The frame layout; only signed 16-bit is encoded here.</param>
    /// <param name="sampleRate">Frames per second.</param>
    /// <param name="encoded">The format word for the stream descriptor and the converter.</param>
    /// <returns>False when the rate is not one the link can derive, or the layout is not 16-bit.</returns>
    internal static bool TryEncode(AudioFormat format, uint sampleRate, out ushort encoded)
    {
        encoded = 0;

        if (format.BitDepth != AudioBitDepth.Bits16 || !format.Signed)
        {
            return false;
        }

        if (format.Channels == 0 || format.Channels > MaxChannels)
        {
            return false;
        }

        if (!TryEncodeRate(sampleRate, out ushort rate))
        {
            return false;
        }

        encoded = (ushort)(rate | (Bits16 << BitsShift) | (ushort)(format.Channels - 1));
        return true;
    }

    /// <summary>Encodes the base, multiplier and divisor of one of the derivable rates.</summary>
    /// <param name="sampleRate">Frames per second.</param>
    /// <param name="encoded">The base, multiplier and divisor bits.</param>
    /// <returns>False for a rate the link cannot derive.</returns>
    private static bool TryEncodeRate(uint sampleRate, out ushort encoded)
    {
        // Multiplier and divisor are encoded as one less than the factor, so
        // x1 and /1 are both zero.
        (ushort Base, int Multiplier, int Divisor) parts = sampleRate switch
        {
            48000 => (0, 1, 1),
            44100 => (Base44100, 1, 1),
            96000 => (0, 2, 1),
            88200 => (Base44100, 2, 1),
            144000 => (0, 3, 1),
            192000 => (0, 4, 1),
            176400 => (Base44100, 4, 1),
            24000 => (0, 1, 2),
            22050 => (Base44100, 1, 2),
            16000 => (0, 1, 3),
            12000 => (0, 1, 4),
            11025 => (Base44100, 1, 4),
            9600 => (0, 1, 5),
            8000 => (0, 1, 6),
            32000 => (0, 2, 3),
            _ => (ushort.MaxValue, 0, 0),
        };

        if (parts.Base == ushort.MaxValue)
        {
            encoded = 0;
            return false;
        }

        encoded = (ushort)(parts.Base
            | ((parts.Multiplier - 1) << MultiplierShift)
            | ((parts.Divisor - 1) << DivisorShift));
        return true;
    }
}
