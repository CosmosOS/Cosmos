using Cosmos.HAL.Audio;
using Cosmos.System.Audio.IO;
using System;
using System.Collections.Generic;

namespace Cosmos.System.Audio {
    /// <summary>
    /// An audio mixer is responsible for mixing several streams of audio together.
    /// </summary>
    /// <remarks>
    /// The <see cref="AudioMixer"/> class does not implement any kind of compressor or
    /// limiter, and as such, the resulting audio may introduce hard-clipping.
    /// </remarks>
    public class AudioMixer : AudioStream {
        AudioBuffer mixBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioMixer"/> class.
        /// </summary>
        /// <remarks>
        /// The internal mixing buffer will be set to <see langword="null"/> and
        /// will be initialized with the correct format and buffer size as soon
        /// as a read operation will be attempted.
        /// </remarks>
        public AudioMixer()
        {
            mixBuffer = null;
            streamReader = null;
        }

        /// <summary>
        /// The list of streams that the audio mixer should process.
        /// This list will be dynamically modified as streams get depleted.
        /// </summary>
        public List<AudioStream> Streams { get; private set; } = new List<AudioStream>();

        /// <summary>
        /// The size of the internal mixing buffer, in audio samples.
        /// </summary>
        public int BufferSize => mixBuffer.Size;

        #region Standard AudioStream Members
        public override uint SampleRate { get; set; }

        public override bool Depleted => false;
        #endregion

        AudioBufferWriter cachedOutputWriter;
        AudioBufferReader cachedOutputReader;

        // This will contain the current stream we are processing
        AudioBufferReader streamReader;

        AudioBuffer cachedBuffer;

        public unsafe override void Read(AudioBuffer buffer)
        {
            // Ensure our mixing buffer is the same format and size as the output buffer
            if(mixBuffer == null || mixBuffer.Size != buffer.Size || mixBuffer.Format != buffer.Format) {
                mixBuffer = new AudioBuffer(buffer.Size, buffer.Format);
                streamReader = new AudioBufferReader(mixBuffer);
            }

            if(cachedBuffer != buffer)
            {
                cachedBuffer = buffer;
                cachedOutputWriter = new(buffer, new SampleFormat(AudioBitDepth.Bits16, 1, true));
                cachedOutputReader = new(buffer);
            }

            if (Streams.Count == 1)
            {
                AudioStream stream = Streams[0];

                if (stream.Depleted)
                {
                    Streams.Clear();
                    buffer.Flush();
                }
                else
                {
                    Streams[0].Read(buffer);
                }
            }
            else
            {
                buffer.Flush();

                // Mix all streams together
                // A reverse for-loop so we can modify the list while iterating
                for (int i = Streams.Count - 1; i >= 0; i--)
                {
                    AudioStream stream = Streams[i];

                    // Do not process streams with no data
                    if (stream.Depleted)
                    {
                        Streams.RemoveAt(i);
                        continue;
                    }

                    // Read the stream to our mixing buffer - this will also
                    // convert it to the right format
                    stream.Read(mixBuffer);

                    // Iterate through every sample
                    for (int j = 0; j < mixBuffer.Size; j++)
                    {
                        // Iterate through each channel of the output buffer
                        // Our mixing buffer has the same amount of channels as the output buffer
                        for (int channel = 0; channel < buffer.Format.Channels; channel++)
                        {
                            short sum = SaturationAdd(
                                cachedOutputReader.ReadChannelInt16(j, channel),
                                streamReader.ReadChannelInt16(j, channel)
                            );

                            cachedOutputWriter.WriteChannel(
                                (byte*)&sum,
                                j, channel
                            );
                        }
                    }
                }
            }

            ApplyPostProcessing(buffer);
        }

        private static short SaturationAdd(short a, short b)
        {
            if(a > 0) {
                if (b > short.MaxValue - a)
                {
                    return short.MaxValue;
                }
            }
            else if (b < short.MinValue - a)
            {
                return short.MinValue;
            }

            return (short)(a + b);
        }
    }
}
