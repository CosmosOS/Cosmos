using Cosmos.HAL.Audio;
using Cosmos.System.Audio.DSP.Processing;
using System;
using System.Collections.Generic;

namespace Cosmos.System.Audio {
    /// <summary>
    /// Represents a single audio stream. An audio stream is responsible for
    /// providing audio samples of a certain format to various audio buffers.
    /// </summary>
    public abstract class AudioStream {
        /// <summary>
        /// The sample rate, i.e. how many samples correspond to one second of audio.
        /// No class should attempt to set the sample rate of an abstract AudioStream
        /// without a known type, as it may not support this operation.
        /// </summary>
        /// <exception cref="NotSupportedException">The given audio stream does not support changing the</exception>
        public abstract uint SampleRate { get; set; }

        /// <summary>
        /// Reads samples into the specified buffer and advances the position
        /// by the size of the buffer. An <see cref="AudioStream"/> instance should write the correct type of
        /// audio data, depending on the provided buffer.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        public abstract void Read(AudioBuffer buffer);

        /// <summary>
        /// Applies all post-processors in the <see cref="PostProcessors"/> list.
        /// All inherited classes should call this method after writing data to a buffer.
        /// </summary>
        /// <param name="buffer">The target buffer.</param>
        protected void ApplyPostProcessing(AudioBuffer buffer)
        {
            for (int i = 0; i < PostProcessors.Count; i++)
            {
                PostProcessors[i].Process(buffer);
            }
        }

        /// <summary>
        /// The post-processors to use when reading the stream.
        /// </summary>
        public List<AudioPostProcessor> PostProcessors { get; } = new List<AudioPostProcessor>();

        /// <summary>
        /// Gets the first post processor of the given type in the effect chain. 
        /// </summary>
        /// <typeparam name="T">The type of the post processor to return.</typeparam>
        /// <returns>The found post processor. If no post processor of the given type was found, this method returns <see langword="null"/>.</returns>
        public T GetPostProcessor<T>() where T : AudioPostProcessor
        {
            for (int i = 0; i < PostProcessors.Count; i++)
            {
                if (PostProcessors[i] is T t)
                    return t;
            }

            return null;
        }

        /// <summary>
        /// A value indicating whether the audio stream contains no further audio data.
        /// </summary>
        public abstract bool Depleted { get; }
    }
}
