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

            if(target.format == writeFormat) {
                mode = OperationMode.DirectCopy;
            } else
            {
                if(target.format.bitDepth == writeFormat.bitDepth && target.format.signed == writeFormat.signed)
                {
                    mode = OperationMode.ChannelCopy;
                } else
                {
                    shouldMakeSigned = !writeFormat.signed && target.format.signed;     // whether we should make the samples in 'src' signed
                    shouldChangeSign = shouldMakeSigned || (!target.format.signed && writeFormat.signed);  // whether we should change the sign of the samples

                    buffer = new byte[
                        Math.Max(writeFormat.ChannelSize, target.format.ChannelSize) * Math.Max(writeFormat.channels, target.format.channels)
                    ];
                }     
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
                int sampleStart = index * target.format.size;

                fixed(byte* destPtr = target.RawData)
                {
                    MemoryOperations.Copy(destPtr + sampleStart, sample, target.format.size);
                }
            } else
            {
                // Read the sample to the buffer
                for (int j = 0; j < writeFormat.size; j++)
                {
                    buffer[j] = sample[j];
                }

                // Pass 1: Upmix/downmix channels
                if (writeFormat.channels < target.format.channels)
                {
                    UpmixChannels(
                        buffer,
                        writeFormat.channels,
                        target.format.channels,
                        writeFormat
                    );
                }

                fixed (byte* bufferPtr = buffer, destPtr = target.RawData)
                {
                    // Pass 2: Change the bit-depth
                    if (writeFormat.bitDepth != target.format.bitDepth)
                    {
                        ChangeBitDepth(
                            bufferPtr,
                            target.format.channels,
                            writeFormat.bitDepth,
                            target.format.bitDepth
                        );
                    }

                    // Pass 3: Change the sign
                    if (shouldChangeSign)
                    {
                        if (shouldMakeSigned)
                            MakeSigned(bufferPtr, target.format);
                        else
                            MakeUnsigned(bufferPtr, target.format);
                    }

                    int sampleStart = index * target.format.size;
                    MemoryOperations.Copy(destPtr + sampleStart, bufferPtr, target.format.size);
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
            if (sample.Length < writeFormat.size)
                throw new ArgumentException("The provided sample array is not large enough to provide all data necessary.", nameof(sample));

            fixed(byte* samplePtr = sample) {
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
            if(sample.Length - offset < writeFormat.size)
                throw new ArgumentException("The provided sample array is not large enough to provide all data necessary.", nameof(sample));

            fixed(byte* samplePtr = sample) {
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
                int sampleStart = index * target.format.size + channel * target.format.ChannelSize;

                fixed (byte* destPtr = target.RawData)
                {
                    MemoryOperations.Copy(destPtr + sampleStart, sample, target.format.ChannelSize);
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
                    if (writeFormat.bitDepth != target.format.bitDepth)
                    {
                        ChangeBitDepth(
                            bufferPtr,
                            1, // one channel
                            writeFormat.bitDepth,
                            target.format.bitDepth
                        );
                    }

                    // Pass 2: Change the sign
                    if (shouldChangeSign)
                    {
                        if (shouldMakeSigned)
                            MakeSigned(bufferPtr, target.format);
                        else
                            MakeUnsigned(bufferPtr, target.format);
                    }

                    int sampleStart = index * target.format.size + channel * target.format.ChannelSize;

                    MemoryOperations.Copy(destPtr + sampleStart, bufferPtr, target.format.ChannelSize);
                }
            }
        }
    }
}
