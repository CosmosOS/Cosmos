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
        public AudioBitDepth bitDepth;

        /// <summary>
        /// The amount of channels per a single sample.
        /// </summary>
        public readonly byte channels;

        /// <summary>
        /// A value indicating whether the audio samples are signed or
        /// unsigned. By convention, only 8-bit audio samples are unsigned.
        /// </summary>
        public readonly bool signed;

        /// <summary>
        /// The size of a single audio sample (frame).
        /// </summary>
        public readonly int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleFormat"/> structure.
        /// </summary>
        /// <param name="bitDepth">The bit-depth (bits per a single sample in a channel).</param>
        /// <param name="channels">The amount of channels.</param>
        /// <param name="signed">Whether the audio is signed, that is, whether the sample can store negative values. By convention, only 8-bit audio samples are unsigned.</param>
        public SampleFormat(AudioBitDepth bitDepth, byte channels, bool signed)
        {
            this.bitDepth = bitDepth;
            this.channels = channels;
            this.signed = signed;
            size = ChannelSize * channels;
        }

        /// <summary>
        /// The amount of bytes per a single audio channel.
        /// </summary>
        public int ChannelSize => (int)bitDepth;

        /// <summary>
        /// Returns a copy of this <see cref="SampleFormat"/> that represents
        /// a single channel of a sample.
        /// </summary>
        public SampleFormat AsSingleChannel()
        {
            return new SampleFormat(bitDepth, 1, signed);
        }

        public static bool operator ==(SampleFormat lhs, SampleFormat rhs)
        {
            return lhs.bitDepth == rhs.bitDepth &&
                   lhs.signed   == rhs.signed   &&
                   lhs.channels == rhs.channels;
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
            return HashCode.Combine(bitDepth, channels, signed);
        }

        public override string ToString()
        {
            string bitDepthEnumName = bitDepth switch
            {
                AudioBitDepth.Bits8 => "8-bit",
                AudioBitDepth.Bits16 => "16-bit",
                AudioBitDepth.Bits24 => "24-bit",
                AudioBitDepth.Bits32 => "32-bit",
                _ => $"{(int)bitDepth * 4}-bit"
            };

            string channelAmountName = channels switch
            {
                0 => "no output",
                1 => "Mono",
                2 => "Stereo",
                _ => $"{channels} channels"
            };

            return $"{(signed ? "Signed" : "Unsigned")} {bitDepthEnumName}, {channelAmountName}";
        }
    }
}
