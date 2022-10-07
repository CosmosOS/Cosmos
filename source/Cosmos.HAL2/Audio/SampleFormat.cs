using System;

namespace Cosmos.HAL.Audio {
    /// <summary>
    /// Represents the format of an audio sample.
    /// </summary>
    public class SampleFormat {
        /// <summary>
        /// The audio bit depth, i.e. how many bits are used per a single
        /// channel in a sample.
        /// </summary>
        public AudioBitDepth BitDepth;

        /// <summary>
        /// The amount of channels per a single sample.
        /// </summary>
        public readonly byte Channels;

        /// <summary>
        /// A value indicating whether the audio samples are signed or
        /// unsigned. By convention, only 8-bit audio samples are unsigned.
        /// </summary>
        public readonly bool Signed;

        /// <summary>
        /// The size of a single audio sample (frame).
        /// </summary>
        public readonly uint Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleFormat"/> structure.
        /// </summary>
        /// <param name="bitDepth">The bit-depth (bits per a single sample in a channel).</param>
        /// <param name="channels">The amount of channels.</param>
        /// <param name="signed">Whether the audio is signed, that is, whether the sample can store negative values. By convention, only 8-bit audio samples are unsigned.</param>
        public SampleFormat(AudioBitDepth bitDepth, byte channels, bool signed)
        {
            BitDepth = bitDepth;
            Channels = channels;
            Signed = signed;
            Size = ChannelSize * channels;
        }

        /// <summary>
        /// The amount of bytes per a single audio channel.
        /// </summary>
        public int ChannelSize => (int)BitDepth;

        /// <summary>
        /// Returns a copy of this <see cref="SampleFormat"/> that represents
        /// a single channel of a sample.
        /// </summary>
        public SampleFormat AsSingleChannel()
        {
            return new SampleFormat(BitDepth, 1, Signed);
        }

        public static bool operator ==(SampleFormat lhs, SampleFormat rhs)
        {
            return lhs.BitDepth == rhs.BitDepth &&
                   lhs.Signed   == rhs.Signed   &&
                   lhs.Channels == rhs.Channels;
        }

        public static bool operator !=(SampleFormat lhs, SampleFormat rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            if(obj is SampleFormat other) {
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BitDepth, Channels, Signed);
        }

        public override string ToString()
        {
            string bitDepthEnumName = BitDepth switch
            {
                AudioBitDepth.Bits8 => "8-bit",
                AudioBitDepth.Bits16 => "16-bit",
                AudioBitDepth.Bits24 => "24-bit",
                AudioBitDepth.Bits32 => "32-bit",
                _ => $"{(int)BitDepth * 4}-bit"
            };

            string channelAmountName = Channels switch
            {
                0 => "no output",
                1 => "Mono",
                2 => "Stereo",
                _ => $"{Channels} channels"
            };

            return $"{(Signed ? "Signed" : "Unsigned")} {bitDepthEnumName}, {channelAmountName}";
        }
    }
}
