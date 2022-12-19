using Cosmos.Core;
using Cosmos.HAL.Audio;
using System;

namespace Cosmos.HAL.Audio {
    /// <summary>
    /// Represents a buffer of channel-interleaved audio samples.
    /// </summary>
    public class AudioBuffer {
        int size;

        /// <summary>
        /// The audio format of the buffer.
        /// </summary>
        public readonly SampleFormat Format;

        /// <summary>
        /// Creates an empty <see cref="AudioBuffer"/>.
        /// </summary>
        /// <param name="size">The size of the buffer.</param>
        /// <param name="format">The target format of the audio buffer.</param>
        public AudioBuffer(int size, SampleFormat format)
        {
            Format = format;
            this.size = size;
            RawData = new byte[format.Size * Size];
            SampleAmount = size * format.Channels;
        }

        /// <summary>
        /// Provides access to the raw buffer data.
        /// </summary>
        public byte[] RawData { get; private set; }

        /// <summary>
        /// The size of the audio buffer, in audio samples. Setting this property
        /// to a different value causes the audio buffer to flush.
        /// </summary>
        public int Size {
            get => size;
            set {
                if(value != size)
                {
                    size = value;
                    RawData = new byte[Format.Size * Size];
                }
            }
        }

        /// <summary>
        /// The amount of sample values the buffer contains.
        /// </summary>
        public int SampleAmount { get; }

        /// <summary>
        /// Flushes (empties) the audio buffer, setting all of its sample data to zero.
        /// </summary>
        public void Flush()
        {
            // Set all of the bytes of the buffer to 0.
            MemoryOperations.Fill(RawData, 0);
        }

        /// <summary>
        /// Reads a sample from the audio buffer to the target array.
        /// </summary>
        /// <param name="index">The index of the audio sample to read.</param>
        /// <param name="dest">The array to write the sample to.</param>
        /// <param name="destOffset">The offset in the array to write data to.</param>
        /// <exception cref="ArgumentException">Thrown when the provided destination array is not large enough to hold all of the bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent sample.</exception>
        public unsafe void ReadSample(int index, byte[] dest, int destOffset = 0)
        {
            if (dest.Length - destOffset - Format.Size < 0) {
                throw new ArgumentException("The provided destination array is not large enough to hold all of the bytes.", nameof(dest));
            }

            fixed (byte* destPtr = dest) {
                ReadSample(index, destPtr + destOffset);
            }
        }

        /// <summary>
        /// Reads a sample from the audio buffer to the target pointer in memory.
        /// </summary>
        /// <param name="index">The index of the audio sample to read.</param>
        /// <param name="dest">The part of memory to write the sample to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent sample.</exception>
        public unsafe void ReadSample(int index, byte* dest)
        {
            if (index >= Size) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int bufferOffset = index * Format.Size;

            fixed(byte* bufferPtr = RawData) {
                MemoryOperations.Copy(dest, bufferPtr + bufferOffset, Format.Size);
            }
        }

        /// <summary>
        /// Reads a single channel of a sample to the target array.
        /// </summary>
        /// <param name="index">The index of the audio sample to read the channel of.</param>
        /// <param name="channel">The target channel to read.</param>
        /// <param name="dest">The array to write the sample to.</param>
        /// <param name="destOffset">The offset in the array to write data to.</param>
        /// <exception cref="ArgumentException">Thrown when the provided source array is not large enough to hold all of the bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent channel or sample.</exception>
        public unsafe void ReadSampleChannel(int index, int channel, byte[] dest, int destOffset = 0)
        {
            fixed (byte* destPtr = dest) {
                ReadSampleChannel(index, channel, destPtr + destOffset);
            }
        }

        /// <summary>
        /// Reads a single channel of a sample to the target pointer in memory.
        /// </summary>
        /// <param name="index">The index of the audio sample to read the channel of.</param>
        /// <param name="channel">The target channel to read.</param>
        /// <param name="dest">The part of memory to write the sample to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent channel or sample.</exception>
        public unsafe void ReadSampleChannel(int index, int channel, byte* dest)
        {
            int channelByteSize = Format.ChannelSize;
            int bufferOffset = index * Format.Size + channelByteSize * channel;

            fixed (byte* bufferPtr = RawData) {
                MemoryOperations.Copy(
                    dest,
                    bufferPtr + bufferOffset,
                    channelByteSize
                );
            }
        }

        /// <summary>
        /// Writes a sample to the audio buffer, reading the sample data from
        /// the specified array beginning at <paramref name="srcOffset"/>.
        /// </summary>
        /// <param name="index">The index of the audio sample to overwrite.</param>
        /// <param name="src">The array to read the sample from.</param>
        /// <param name="srcOffset">The offset in the array to read data from.</param>
        /// <exception cref="ArgumentException">Thrown when the provided source array is not large enough to provide all of the bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent sample.</exception>
        public unsafe void WriteSample(int index, byte[] src, int srcOffset = 0)
        {
            if (src.Length - srcOffset - Format.Size < 0) {
                throw new ArgumentException("The provided source array is not large enough to provide all of the bytes.", nameof(src));
            }

            fixed (byte* destPtr = src) {
                ReadSample(index, destPtr + srcOffset);
            }
        }

        /// <summary>
        /// Writes a sample from the target pointer in memory to the buffer.
        /// The amount of bytes that will be read is equal to <see cref="SampleSize"/>.
        /// </summary>
        /// <param name="index">The index of the audio sample to overwrite.</param>
        /// <param name="src">The part of memory to read the data from.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent sample.</exception>
        public unsafe void WriteSample(int index, byte* src)
        {
            if (index >= Size) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int bufferOffset = index * Format.Size;

            fixed(byte* bufferPtr = RawData) {
                for (int i = 0; i < Format.Size; i++) {
                    *(bufferPtr + i) = *(src + i);
                }
            }
        }

        /// <summary>
        /// Changes the value of a single channel of the specified sample to
        /// the value that's located at index <paramref name="srcOffset"/> of the given
        /// array. The amount of bytes that will be read is equal to the value of <see cref="BitDepth"/>.
        /// </summary>
        /// <param name="index">The index of the audio sample to overwrite the channel of.</param>
        /// <param name="channel">The channel to overwrite.</param>
        /// <param name="src">The array to read the data from.</param>
        /// <param name="srcOffset">The starting index to read the data from <paramref name="src"/>.</param>
        /// <exception cref="ArgumentException">Thrown when the provided source array is not large enough to provide all of the bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent channel or sample.</exception>
        public unsafe void WriteSampleChannel(int index, int channel, byte[] src, int srcOffset = 0)
        {
            if (src.Length - srcOffset - (int)Format.BitDepth < 0) {
                throw new ArgumentException("The provided source array is not large enough to provide all of the bytes.", nameof(src));
            }

            fixed (byte* destPtr = src) {
                WriteSampleChannel(index, channel, destPtr + srcOffset);
            }
        }

        /// <summary>
        /// Changes the value of a single channel of the specified sample to
        /// the value that's located in memory at the specified pointer.
        /// The amount of bytes that will be read is equal to the value of <see cref="BitDepth"/>.
        /// </summary>
        /// <param name="index">The index of the audio sample to overwrite the channel of.</param>
        /// <param name="channel">The channel to overwrite.</param>
        /// <param name="src">The part of memory to read the data from.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to write to a non-existent channel or sample.</exception>
        public unsafe void WriteSampleChannel(int index, int channel, byte* src)
        {
            if (index >= Size) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (channel > Format.Channels) {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            int channelByteSize = (int)Format.BitDepth;
            int bufferOffset = index * Format.Size + channelByteSize * channel;

            fixed (byte* bufferPtr = RawData) {
                for (int i = 0; i < channelByteSize; i++) {
                    *(bufferPtr + i) = *(src + i);
                }
            }
        }
    }
}
