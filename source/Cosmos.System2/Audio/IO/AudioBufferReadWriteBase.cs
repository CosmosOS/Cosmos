using Cosmos.Core;
using Cosmos.HAL.Audio;

namespace Cosmos.System.Audio.IO {
    /// <summary>
    /// Represents the base class for <see cref="AudioBufferReader"/> and
    /// <see cref="AudioBufferWriter"/> instances.
    /// </summary>
    public abstract class AudioBufferReadWriteBase {
        /// <summary>
        /// The audio buffer this reader/writer is working on.
        /// </summary>
        protected AudioBuffer target;

        /// <summary>
        /// Converts 8-bit samples to the the specified format, using the specified
        /// buffer. If <paramref name="target"/> is equal to <see cref="AudioBitDepth.Bits8"/>,
        /// no action is performed.
        /// </summary>
        protected static unsafe void ChangeBitDepth(byte* outputPtr, AudioBitDepth target, byte channels, byte* inputPtr)
        {
            switch (target) {
                case AudioBitDepth.Bits16: {
                    // 8bit -> 16bit
                    short* opBufSamplePtr = (short*)outputPtr;
                    for (int j = 0; j < channels; j++) {
                        *(opBufSamplePtr + j) = (short)(*(inputPtr + j) << 8);
                    }
                    break;
                }
                case AudioBitDepth.Bits24:
                    // 8bit -> 24bit
                    for (int j = 0; j < channels; j++) {
                        WriteInt24ToBuffer(outputPtr, j * 3, *(inputPtr + j) << 16);
                    }
                    break;
                case AudioBitDepth.Bits32: {
                    // 8bit -> 32bit
                    int* opBufSamplePtr = (int*)outputPtr;
                    for (int j = 0; j < channels; j++) {
                        *(opBufSamplePtr + j) = *(inputPtr + j) << 24;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Converts 16-bit samples to the specified format, using the specified
        /// buffer. If <paramref name="target"/> is equal to <see cref="AudioBitDepth.Bits16"/>,
        /// no action is performed.
        /// </summary>
        protected static unsafe void ChangeBitDepth(byte* outputPtr, AudioBitDepth target, byte channels, short* inputPtr)
        {
            switch (target) {
                case AudioBitDepth.Bits8: {
                    // 16bit -> 8bit
                    for (int j = 0; j < channels; j++) {
                        *(outputPtr + j) = (byte)(*(inputPtr + j) >> 8);
                    }
                    break;
                }
                case AudioBitDepth.Bits24: {
                    // 16bit -> 24bit
                    for (int j = 0; j < channels; j++) {
                        WriteInt24ToBuffer(outputPtr, j * 3, *(inputPtr + j) << 8);
                    }
                    break;
                }
                case AudioBitDepth.Bits32: {
                    // 16bit -> 32bit
                    int* opBufSamplePtr = (int*)outputPtr;
                    for (int j = 0; j < channels; j++) {
                        *(opBufSamplePtr + j) = *(inputPtr + j) << 16;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Converts 24-bit samples to the specified format, using the specified
        /// buffer. Each <see langword="int"/> in the specified buffer will be
        /// interpreted as 24-bit values, and any extra bits will be ignored.
        /// If <paramref name="target"/> is equal to <see cref="AudioBitDepth.Bits24"/>,
        /// no action is performed.
        /// </summary>
        protected static unsafe void ChangeBitDepth24(byte* outputPtr, AudioBitDepth target, byte channels, int* inputPtr)
        {
            switch (target) {
                case AudioBitDepth.Bits8: {
                    // 24bit -> 8bit
                    for (int j = 0; j < channels; j++) {
                        *(outputPtr + j) = (byte)(*(inputPtr + j) >> 16);
                    }
                    break;
                }
                case AudioBitDepth.Bits16: {
                    // 24bit -> 16bit
                    short* opBufSamplePtr = (short*)outputPtr;
                    for (int j = 0; j < channels; j++) {
                        *(opBufSamplePtr + j) = (short)(*(inputPtr + j) >> 8);
                    }
                    break;
                }
                case AudioBitDepth.Bits32: {
                    // 24bit -> 32bit
                    int* opBufSamplePtr = (int*)outputPtr;
                    for (int j = 0; j < channels; j++) {
                        *(opBufSamplePtr + j) = *(inputPtr + j) << 8;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Converts 32-bit samples to the specified format, using the specified
        /// buffer. If <paramref name="target"/> is equal to <see cref="AudioBitDepth.Bits32"/>,
        /// no action is performed.
        /// </summary>
        protected static unsafe void ChangeBitDepth(byte* outputPtr, AudioBitDepth target, byte channels, int* inputPtr)
        {
            switch (target) {
                case AudioBitDepth.Bits8: {
                    // 32bit -> 8bit
                    for (int j = 0; j < channels; j++) {
                        *(outputPtr + j) = (byte)(*(inputPtr + j) >> 24);
                    }
                    break;
                }
                case AudioBitDepth.Bits16:
                    // 32bit -> 16bit
                    short* opBufSamplePtr = (short*)outputPtr;
                    for (int j = 0; j < channels; j++) {
                        *(opBufSamplePtr + j) = (short)(*(inputPtr + j) >> 16);
                    }
                    break;
                case AudioBitDepth.Bits24:
                    // 32bit -> 24bit
                    for (int j = 0; j < channels; j++) {
                        WriteInt24ToBuffer(outputPtr, j * 3, *(inputPtr + j) >> 8);
                    }
                    break;
            }
        }

        /// <summary>
        /// Converts the specified samples to the specified target format.
        /// The specified buffer pointer must be large enough to hold both
        /// the input samples, and the converted output samples.
        /// </summary>
        protected static unsafe void ChangeBitDepth(byte* bufferPtr, byte channels, AudioBitDepth inputBitDepth, AudioBitDepth outputBitDepth)
        {
            // Convert the bit-depth
            //     We have to re-normalize the samples; from the source data type's min and max value,
            //     to the target data type's min and max values - this operation can be represented by
            //     the following formula:
            //        (value / max source data type value) * max target data type value
            //     For example, to convert a 16-bit sample 0x0E to a 32-bit sample:
            //        (14 / 32767) * 2147483647
            //     However, this approach, while the most obvious from a mathematical standpoint,
            //     uses floating-point math. This can be much slower than direct bit-manipulation.
            //     The code below will do the equivalent to the provided formula by using
            //     bit-manipulation operators, making the method much more performant.
            //
            //     By using bit-shifting, we do not have to change the sign of the samples
            //     which is useful for us as we convert the samples in several different passes.

            switch (inputBitDepth) {
                case AudioBitDepth.Bits8: {
                    // 8 bit -> target bit depth
                    byte[] samples = new byte[channels];

                    fixed (byte* samplesPtr = samples) {
                        MemoryOperations.Copy(samplesPtr, bufferPtr, channels);
                        ChangeBitDepth(bufferPtr, outputBitDepth, channels, samplesPtr);
                    }
                    break;
                }
                case AudioBitDepth.Bits16: {
                    // 16 bit -> target bit depth
                    short[] samples = new short[channels];

                    fixed (short* samplesPtr = samples) {
                        MemoryOperations.Copy((byte*)samplesPtr, bufferPtr, channels * sizeof(short));
                        ChangeBitDepth(bufferPtr, outputBitDepth, channels, samplesPtr);
                    }
                    break;
                }
                case AudioBitDepth.Bits24: {
                    // 24 bit -> target bit depth
                    int[] samples = new int[channels];

                    for (int j = 0; j < channels; j++) {
                        samples[j] = ToInt24(bufferPtr, j * 3);
                    }

                    fixed (int* samplesPtr = samples) {
                        ChangeBitDepth24(bufferPtr, outputBitDepth, channels, samplesPtr);
                    }
                    break;
                }
                case AudioBitDepth.Bits32: {
                    // 32 bit -> target bit depth
                    int[] samples = new int[channels];

                    fixed (int* samplesPtr = samples) {
                        MemoryOperations.Copy((byte*)samplesPtr, bufferPtr, channels * sizeof(int));
                        ChangeBitDepth(bufferPtr, outputBitDepth, channels, samplesPtr);
                    }
                    break;
                }
            }
        }

        protected static unsafe void MakeUnsigned(byte* sourcePtr, SampleFormat format)
            => MakeUnsigned(sourcePtr, format.BitDepth, format.Channels);

        /// <summary>
        /// Makes a single signed audio sample unsigned.
        /// </summary>
        /// <param name="sourcePtr">The pointer to the target sample.</param>
        /// <param name="format">The audio format of the sample.</param>
        protected static unsafe void MakeUnsigned(byte* sourcePtr, AudioBitDepth bitDepth, byte channels)
        {
            // To change the sign of a signed value: value + ((data type max value / 2) + 1)
            switch (bitDepth) {
                case AudioBitDepth.Bits8:
                    for (int j = 0; j < channels; j++) {
                        byte rawSample = sourcePtr[j];

                        // Signed 8-bit -> Unsigned 8-bit
                        byte sample = (byte)(unchecked((sbyte)rawSample) + 128);
                        sourcePtr[j] = sample;
                    }
                    break;
                case AudioBitDepth.Bits16:
                    for (int j = 0; j < channels; j++) {
                        // Signed 16-bit -> Unsigned 16-bit
                        short origSample = *((short*)sourcePtr + j);
                        ushort sample = (ushort)(origSample + 32768);
                        *((ushort*)sourcePtr + j) = sample;
                    }
                    break;
                case AudioBitDepth.Bits24:
                    for (int j = 0; j < channels; j++) {
                        int byteAlignment = j * 3;
                        int origSample = ToInt24(sourcePtr, byteAlignment);
                        int sample = origSample + 8388608;

                        WriteInt24ToBuffer(sourcePtr, byteAlignment, sample);
                    }
                    break;
                case AudioBitDepth.Bits32:
                    for (int j = 0; j < channels; j++) {
                        // Signed 32-bit -> Unsigned 32-bit
                        int origSample = *((int*)sourcePtr + j);
                        uint sample = (uint)(origSample + 2147483648);
                        *((uint*)sourcePtr + j) = sample;
                    }
                    break;
            }
        }

        protected unsafe static void MakeSigned(byte* sourcePtr, SampleFormat format)
            => MakeSigned(sourcePtr, format.BitDepth, format.Channels);

        /// <summary>
        /// Makes a single unsigned audio sample signed.
        /// </summary>
        /// <param name="sourcePtr">The pointer to the target sample.</param>
        /// <param name="format">The audio format of the sample.</param>
        protected unsafe static void MakeSigned(byte* sourcePtr, AudioBitDepth bitDepth, byte channels)
        {
            // To change the sign of an unsigned value: value - ((data type max value / 2) + 1)
            switch (bitDepth) {
                case AudioBitDepth.Bits8:
                    for (int j = 0; j < channels; j++) {
                        byte rawSample = sourcePtr[j];

                        // Unsigned 8-bit -> Signed 8-bit
                        sbyte sample = (sbyte)(rawSample - 128);
                        rawSample = unchecked((byte)sample);
                        sourcePtr[j] = rawSample;
                    }
                    break;
                case AudioBitDepth.Bits16:
                    for (int j = 0; j < channels; j++) {
                        // Unsigned 16-bit -> Signed 16-bit
                        ushort origSample = *((ushort*)sourcePtr + j);
                        short sample = (short)(origSample - 32768);
                        *((short*)sourcePtr + j) = sample;
                    }
                    break;
                case AudioBitDepth.Bits24:
                    for (int j = 0; j < channels; j++) {
                        // Unsigned 24-bit -> Signed 24-bit
                        int byteAlignment = j * 3;
                        int origSample = ToInt24(sourcePtr, byteAlignment);
                        int sample = origSample - 8388608;

                        WriteInt24ToBuffer(sourcePtr, byteAlignment, sample);
                    }
                    break;
                case AudioBitDepth.Bits32:
                    for (int j = 0; j < channels; j++) {
                        // Unsigned 32-bit -> Signed 32-bit
                        uint origSample = *((uint*)sourcePtr + j);
                        int sample = (int)(origSample - 2147483648);
                        *((int*)sourcePtr + j) = sample;
                    }
                    break;
            }
        }

        /// <summary>
        /// Up-mixes an audio sample by duplicating channels.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        /// <param name="srcChannels">The amount of channels available in the buffer.</param>
        /// <param name="destChannels">The destination amount of channels.</param>
        /// <param name="format">The format of the samples.</param>
        protected unsafe static void UpmixChannels(byte[] buffer, byte srcChannels, byte destChannels, SampleFormat format)
        {
            // we have to duplicate the channels in operationBuffer
            for (int j = 0; j < destChannels - srcChannels; j++) {
                // iterate over each byte of the iterated channel
                for (int k = 0; k < format.ChannelSize; k++) {
                    // srcSampleSize points to the index right after our sample information
                    // offsetting that by srcChannelSize will point where the last channel is
                    // we are adding k to get the appropriate byte (as the channel can be 16-bit, 32-bit, etc)
                    byte channelByte = buffer[format.Size - format.ChannelSize + k];
                    buffer[format.Size + j * format.ChannelSize + k] = channelByte;
                }
            }
        }

        /// <summary>
        /// Directly writes a 24-bit integer value to a buffer. Only the first 3 bytes
        /// of the provided <see cref="int"/> are used.
        /// </summary>
        /// <param name="bufferPtr">The pointer to the target buffer.</param>
        /// <param name="offset">The location in the provided buffer the integer should be written to.</param>
        /// <param name="i24">The target 24-bit integer to write. The last byte is ignored.</param>
        protected static unsafe void WriteInt24ToBuffer(byte* bufferPtr, int offset, int i24)
        {
            byte* i24Ptr = (byte*)&i24;

            *(bufferPtr + offset) = *i24Ptr;
            *(bufferPtr + offset + 1) = *(i24Ptr + 1);
            *(bufferPtr + offset + 2) = *(i24Ptr + 2);
        }

        /// <summary>
        /// Returns a 24-bit signed integer converted from three bytes at a
        /// specified position in a byte array pointer.
        /// </summary>
        /// <param name="arrayPtr">A pointer to an array of bytes that includes the three bytes to convert.</param>
        /// <param name="startIndex">The starting position within <paramref name="arrayPtr"/>.</param>
        /// <returns>A 32-bit signed integer formed by three bytes beginning at <paramref name="startIndex"/>.</returns>
        protected static unsafe int ToInt24(byte* arrayPtr, int startIndex)
        {
            return arrayPtr[startIndex] | arrayPtr[startIndex + 1] << 8 | (sbyte)arrayPtr[startIndex + 2] << 16;
        }
    }
}
