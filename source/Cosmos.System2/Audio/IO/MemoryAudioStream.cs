using Cosmos.Core;
using Cosmos.HAL.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Audio.IO {
    /// <summary>
    /// Represents an audio stream that reads from a byte array in memory.
    /// </summary>
    public class MemoryAudioStream : SeekableAudioStream {
        readonly SampleFormat format;
        readonly uint sampleRate;
        readonly int sampleCount;
        readonly byte[] data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAudioStream"/> class.
        /// </summary>
        /// <param name="format">The format of the provided audio data.</param>
        /// <param name="data">The audio data.</param>
        public MemoryAudioStream(SampleFormat format, uint sampleRate, byte[] data)
        {
            this.format = format;
            this.sampleRate = sampleRate;
            this.data = data;
            sampleCount = data.Length / format.Size;
        }

        #region Standard AudioStream Members
        public SampleFormat Format
        {
            get => format;
            set
            {
                throw new NotSupportedException("Cannot change the format of a MemoryAudioStream");
            }
        }

        public override uint SampleRate {
            get => sampleRate;
            set => throw new InvalidOperationException("Cannot change the sample rate of a MemoryAudioStream.");
        }

        public override uint Position { get; set; }

        public override uint Length => (uint)sampleCount;

        public override bool Depleted => sampleCount <= Position;
        #endregion

        /// <summary>
        /// Creates a <see cref="MemoryAudioStream"/> from a PCM-encoded WAVE (.wav)
        /// file. The file has to have a valid header.
        /// </summary>
        /// <param name="waveFile">The target wave file.</param>
        public unsafe static MemoryAudioStream FromWave(byte[] waveFile)
        {
            if(!ValidateValues(waveFile, 0, 0x52, 0x49, 0x46, 0x46)) {
                throw new ArgumentException("Invalid WAVE file - expected a RIFF header.", nameof(waveFile));
            }

            // chunkSize at offset 4 of size 4

            if (!ValidateValues(waveFile, 8, 0x57, 0x41, 0x56, 0x45)) {
                throw new ArgumentException("Invalid WAVE file - expected a WAVE format in the RIFF header.", nameof(waveFile));
            }

            if (!ValidateValues(waveFile, 12, 0x66, 0x6D, 0x74, 0x20)) {
                throw new ArgumentException("The first subchunk is expected to be the sample format.", nameof(waveFile));
            }

            // This is usually 16 for standard PCM audio, but some old encoding software will
            // include extra, non-standard metadata entries at the end, changing the size.
            // Normally, this is fine, if the metadata is at the end of the chunk and we can safely ignore it.
            int metadataSize = BitConverter.ToInt32(waveFile, 16);

            if (!ValidateValues(waveFile, 20, 0x01, 0x00)) {
                throw new ArgumentException("WAVE compression is not supported.", nameof(waveFile));
            }

            ushort numChannels = BitConverter.ToUInt16(waveFile, 22);

            if (numChannels > byte.MaxValue) {
                throw new ArgumentException($"The maximum amount of channels for an AudioStream is 255 - the input contains {numChannels}.", nameof(waveFile));
            }

            uint sampleRate = BitConverter.ToUInt32(waveFile, 24);
            // byteRate at offset 28 of size 4
            // blockAlign at offset 32 of size 2
            ushort bitsPerSample = BitConverter.ToUInt16(waveFile, 34);

            var bitDepth = bitsPerSample switch {
                8 => AudioBitDepth.Bits8,
                16 => AudioBitDepth.Bits16,
                24 => AudioBitDepth.Bits24,
                32 => AudioBitDepth.Bits32,
                _ => throw new ArgumentException($"{bitsPerSample}-bit WAVE files are not supported.", nameof(waveFile)),
            };

            // ExtraParamSize and ExtraParams would be here for encodings different than PCM
            int dataStart = 20 + metadataSize;

            if (!ValidateValues(waveFile, dataStart, 0x64, 0x61, 0x74, 0x61)) {
                throw new ArgumentException("Expected a 'data' block");
            }

            uint dataSize = BitConverter.ToUInt32(waveFile, dataStart + 4);
            byte[] data = new byte[dataSize];

            fixed (byte* inputPtr = waveFile, outputPtr = data) {
                MemoryOperations.Copy(outputPtr, inputPtr + dataStart + 8, data.Length);
            }

            SampleFormat format = new(
                bitDepth,
                (byte)numChannels,
                bitDepth != AudioBitDepth.Bits8 // only 8-bit PCM is unsigned
            );

            return new MemoryAudioStream(format, sampleRate, data);
        }

        private static bool ValidateValues(byte[] target, int offset, params byte[] values)
        {
            for (int i = 0; i < values.Length; i++) {
                if(target[i + offset] != values[i]) {
                    return false;
                }
            }

            return true;
        }

        // We cache these values, because the heap allocation for these objects is costly.
        AudioBufferWriter cachedWriter;
        AudioBuffer cachedBuffer;

        public unsafe override void Read(AudioBuffer buffer)
        {
            int samplesLeft = sampleCount - (int)Position;
            int actualCopySize = Math.Min(samplesLeft, buffer.Size);

            if(cachedBuffer != buffer)
            {
                cachedWriter = new(buffer, format);
                cachedBuffer = buffer;
            }

            fixed (byte* dataPtr = data) {
                for (int i = 0; i < actualCopySize; i++) {
                    cachedWriter.Write(dataPtr + i * format.Size + Position * format.Size, i);
                }
            }

            uint delta = (uint)buffer.Size / buffer.Format.Channels;

            if (actualCopySize < buffer.Size)
            {
                int start = actualCopySize * buffer.Format.Size;
                int size = buffer.RawData.Length - start;

                // Fill the rest of the buffer with 0s
                fixed (byte* rawDataPtr = buffer.RawData)
                {
                    MemoryOperations.Fill(rawDataPtr + start, 0, size);
                }

                Position = Math.Min(Position + delta, Length);
            }
            else {
                Position += delta;
            }

            ApplyPostProcessing(buffer);
        }
    }
}
