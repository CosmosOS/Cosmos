using Cosmos.Core;
using Cosmos.HAL.Audio;
using System;

namespace Cosmos.System.Audio.IO {
    /// <summary>
    /// Represents a write-only data view of an <see cref="AudioBuffer"/>.
    /// The <see cref="AudioBufferWriter"/> class allows for performant
    /// writing of samples of different formats than the target audio buffer.
    /// </summary>
    public class AudioBufferWriter : AudioBufferReadWriteBase {
        /// <summary>
        /// Specifies the mode of operation for an <see cref="AudioBufferWriter"/>.
        /// </summary>
        private enum OperationMode
        {
            /// <summary>
            /// All samples are directly copied, without any conversion.
            /// </summary>
            DirectCopy,

            /// <summary>
            /// Channels are upmixed if necessary, and then copied, without
            /// any sort of bit modification.
            /// </summary>
            ChannelCopy,

            /// <summary>
            /// All samples are converted to the correct format before being
            /// written.
            /// </summary>
            Convert
        }

        readonly SampleFormat writeFormat;
        readonly bool shouldChangeSign;
        readonly bool shouldMakeSigned;
        readonly byte[] buffer;
        readonly OperationMode mode;

        /// <summary>
        /// Creates a new write-only data view for the specified audio buffer.
        /// </summary>
        /// <param name="target">The target buffer.</param>
        /// <param name="writeFormat">The format of the samples that will be written.</param>
        public AudioBufferWriter(AudioBuffer target, SampleFormat writeFormat)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.writeFormat = writeFormat;

            if(target.Format == writeFormat) {
                mode = OperationMode.DirectCopy;
            } else
            {
                if(target.Format.BitDepth == writeFormat.BitDepth && target.Format.Signed == writeFormat.Signed)
                {
                    mode = OperationMode.ChannelCopy;
                }
                else
                {
                    shouldMakeSigned = !writeFormat.Signed && target.Format.Signed;     // whether we should make the samples in 'src' signed
                    shouldChangeSign = shouldMakeSigned || (!target.Format.Signed && writeFormat.Signed);  // whether we should change the sign of the samples

                    mode = OperationMode.Convert;
                }

                buffer = new byte[
                    Math.Max(writeFormat.ChannelSize, target.Format.ChannelSize) * Math.Max(writeFormat.Channels, target.Format.Channels)
                ];
            }
        }

        /// <summary>
        /// Writes a sample to the buffer.
        /// </summary>
        /// <param name="sample">A pointer to the first byte of the sample write.</param>
        /// <param name="index">The index of the target sample to overwrite.</param>
        public unsafe void Write(byte* sample, int index)
        {
            if (mode == OperationMode.DirectCopy)
            {
                int sampleStart = index * target.Format.Size;

                fixed(byte* destPtr = target.RawData)
                {
                    MemoryOperations.Copy(destPtr + sampleStart, sample, target.Format.Size);
                }
            } else
            {
                // Read the sample to the buffer
                for (int j = 0; j < writeFormat.Size; j++)
                {
                    buffer[j] = sample[j];
                }

                // Pass 1: Upmix/downmix channels
                if (writeFormat.Channels < target.Format.Channels)
                {
                    UpmixChannels(
                        buffer,
                        writeFormat.Channels,
                        target.Format.Channels,
                        writeFormat
                    );
                }

                fixed (byte* bufferPtr = buffer, destPtr = target.RawData)
                {
                    // Pass 2: Change the bit-depth
                    if (writeFormat.BitDepth != target.Format.BitDepth)
                    {
                        ChangeBitDepth(
                            bufferPtr,
                            target.Format.Channels,
                            writeFormat.BitDepth,
                            target.Format.BitDepth
                        );
                    }

                    // Pass 3: Change the sign
                    if (shouldChangeSign)
                    {
                        if (shouldMakeSigned) {
                            MakeSigned(bufferPtr, target.Format);
                        }
                        else {
                            MakeUnsigned(bufferPtr, target.Format);
                        }
                    }

                    int sampleStart = index * target.Format.Size;
                    MemoryOperations.Copy(destPtr + sampleStart, bufferPtr, target.Format.Size);
                }
            }
        }

        /// <summary>
        /// Writes a sample to the buffer, reading from the given array.
        /// </summary>
        /// <param name="sample">The bytes of the sample to write.</param>
        /// <param name="index">The index of the target sample to overwrite.</param>
        /// <exception cref="ArgumentException">The target array is not big enough to contain the sample information.</exception>
        public unsafe void Write(byte[] sample, int index)
        {
            if (sample.Length < writeFormat.Size) {
                throw new ArgumentException("The provided sample array is not large enough to provide all data necessary.", nameof(sample));
            }

            fixed (byte* samplePtr = sample) {
                Write(samplePtr, index);
            }
        }

        /// <summary>
        /// Writes a sample to the buffer, reading from the given array, starting
        /// from the given offset.
        /// </summary>
        /// <param name="sample">The bytes of the sample to write.</param>
        /// <param name="index">The index of the target sample to overwrite.</param>
        /// <param name="offset">The starting index to read from.</param>
        /// <exception cref="ArgumentException">The target array is not big enough to provide the sample information with the given offset.</exception>
        public unsafe void Write(byte[] sample, int index, int offset)
        {
            if(sample.Length - offset < writeFormat.Size) {
                throw new ArgumentException("The provided sample array is not large enough to provide all data necessary.", nameof(sample));
            }

            fixed (byte* samplePtr = sample) {
                Write(samplePtr + offset, index);
            }
        }

        /// <summary>
        /// Overwrites a channel of a sample of the buffer.
        /// </summary>
        /// <param name="sample">A pointer to the first byte of the channel value to read from.</param>
        /// <param name="index">The index of the target sample to write to.</param>
        /// <param name="channel">The index of the channel to write to.</param>
        public unsafe void WriteChannel(byte* sample, int index, int channel)
        {
            if (mode != OperationMode.Convert)
            {
                int sampleStart = index * target.Format.Size + channel * target.Format.ChannelSize;

                fixed (byte* destPtr = target.RawData)
                {
                    MemoryOperations.Copy(destPtr + sampleStart, sample, target.Format.ChannelSize);
                }
            } else
            {
                // Read the sample to the buffer
                for (int j = 0; j < writeFormat.ChannelSize; j++)
                {
                    buffer[j] = sample[j];
                }

                fixed (byte* bufferPtr = buffer, destPtr = target.RawData)
                {
                    // Pass 1: Change the bit-depth
                    if (writeFormat.BitDepth != target.Format.BitDepth)
                    {
                        ChangeBitDepth(
                            bufferPtr,
                            1, // one channel
                            writeFormat.BitDepth,
                            target.Format.BitDepth
                        );
                    }

                    // Pass 2: Change the sign
                    if (shouldChangeSign)
                    {
                        if (shouldMakeSigned) {
                            MakeSigned(bufferPtr, target.Format);
                        }
                        else {
                            MakeUnsigned(bufferPtr, target.Format);
                        }
                    }

                    int sampleStart = index * target.Format.Size + channel * target.Format.ChannelSize;

                    MemoryOperations.Copy(destPtr + sampleStart, bufferPtr, target.Format.ChannelSize);
                }
            }
        }
    }
}
