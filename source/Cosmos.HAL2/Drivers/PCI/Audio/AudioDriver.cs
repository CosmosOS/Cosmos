using System;
using Cosmos.HAL.Audio;

namespace Cosmos.HAL.Audio
{
    /// <summary>
    /// Represents an audio driver.
    /// </summary>
    public abstract class AudioDriver {
        /// <summary>
        /// The buffer provider to use.
        /// </summary>
        public abstract IAudioBufferProvider BufferProvider { get; set; }

        /// <summary>
        /// Gets the supported audio sample formats by the audio driver.
        /// </summary>
        /// <returns>An array containing all supported sample formats.</returns>
        public abstract SampleFormat[] GetSupportedSampleFormats();

        /// <summary>
        /// Sets the used sample format by the audio driver. The sample format
        /// must be supported by the audio driver in order to be accepted;
        /// otherwise, the method will throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <remarks>
        /// An audio driver can supply a list of supported formats with the
        /// <see cref="GetSupportedSampleFormats"/> method.
        /// </remarks>
        /// <param name="sampleFormat">The target sample format to use.</param>
        public abstract void SetSampleFormat(SampleFormat sampleFormat);

        /// <summary>
        /// Whether the audio device controlled by this audio driver
        /// is currently enabled and outputing/capturing audio.
        /// </summary>
        public abstract bool Enabled { get; }

        /// <summary>
        /// Starts outputting audio.
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// Stops any audio output.
        /// </summary>
        public abstract void Disable();
    }
}
