using System;
using Cosmos.HAL.Audio;

namespace Cosmos.System.Audio.IO {
    /// <summary>
    /// Represents a read-only data view of an <see cref="AudioBuffer"/>.
    /// </summary>
    public class AudioBufferReader : AudioBufferReadWriteBase {
        readonly byte[] buffer;

        /// <summary>
        /// Creates a new read-only data view for the specified audio buffer.
        /// </summary>
        /// <param name="target">The target buffer.</param>
        public AudioBufferReader(AudioBuffer target)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            buffer = new byte[4 * target.format.channels];
        }

        private unsafe void EnsureBufferFormat(byte* bufferPtr, AudioBitDepth bitDepth, bool signed, byte channels)
        {
            if (target.format.bitDepth != bitDepth)
                ChangeBitDepth(
                    bufferPtr,
                    channels,
                    target.format.bitDepth,
                    bitDepth
                );

            if (target.format.signed && !signed)
                MakeUnsigned(bufferPtr, bitDepth, channels);
            else if (!target.format.signed && signed)
                MakeSigned(bufferPtr, bitDepth, channels);
        }

        /// <summary>
        /// Reads a single channel from the specified sample in the specified format
        /// and returns a pointer to the first byte of the read sample.
        /// </summary>
        private unsafe byte* ReadChannel(int index, int channel, bool signed, AudioBitDepth bitDepth)
        {
            target.ReadSampleChannel(index, channel, buffer);

            fixed (byte* bufferPtr = buffer) {
                EnsureBufferFormat(bufferPtr, bitDepth, signed, 1);
                return bufferPtr;
            }
        }

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 32-bit signed value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe int ReadChannelInt32(int index, int channel)
            => *(int*)ReadChannel(index, channel, true, AudioBitDepth.Bits32);

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 32-bit unsigned value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe uint ReadChannelUInt32(int index, int channel)
            => *(uint*)ReadChannel(index, channel, false, AudioBitDepth.Bits32);

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 16-bit signed value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe short ReadChannelInt16(int index, int channel)
            => *(short*)ReadChannel(index, channel, true, AudioBitDepth.Bits16);

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 16-bit unsigned value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe ushort ReadChannelUInt16(int index, int channel)
            => *(ushort*)ReadChannel(index, channel, false, AudioBitDepth.Bits16);

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 8-bit signed value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe sbyte ReadChannelInt8(int index, int channel)
            => *(sbyte*)ReadChannel(index, channel, true, AudioBitDepth.Bits8);

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 8-bit unsigned value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe byte ReadChannelUInt8(int index, int channel)
            => *ReadChannel(index, channel, false, AudioBitDepth.Bits8);

        /// <summary>
        /// Reads a single channel from the specified sample and returns the
        /// result as an 32-bit floating-point value.
        /// </summary>
        /// <param name="index">The index of the target sample.</param>
        /// <param name="channel">The channel to read.</param>
        public unsafe float ReadChannelFloat(int index, int channel)
        {
            target.ReadSampleChannel(index, channel, buffer);

            fixed (byte* bufferPtr = buffer) {
                if (!target.format.signed) {
                    MakeSigned(bufferPtr, target.format.bitDepth, 1);
                }

                switch (target.format.bitDepth) {
                    case AudioBitDepth.Bits8:
                        sbyte i8 = unchecked((sbyte)buffer[0]);
                        return i8 * (1f / 128f);
                    case AudioBitDepth.Bits16:
                        short i16 = *(short*)bufferPtr;
                        return i16 * (1f / 32768f);
                    case AudioBitDepth.Bits24:
                        int i24 = ToInt24(bufferPtr, 0);
                        return i24 * (1f / 8388608f);
                    case AudioBitDepth.Bits32:
                        int i32 = *(int*)bufferPtr;
                        return i32 * (1f / 2147483648f);
                    default:
                        throw new NotImplementedException($"Cannot convert to a floating-point value from bit-depth {target.format.bitDepth}");
                }
            }
        }
    }
}
