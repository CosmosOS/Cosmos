namespace Cosmos.System.Audio.IO
{
    /// <summary>
    /// Represents a finite audio stream that supports seeking operations.
    /// </summary>
    public abstract class SeekableAudioStream : AudioStream
    {
        /// <summary>
        /// The position of the audio stream, in samples.
        /// </summary>
        public abstract uint Position { get; set; }

        /// <summary>
        /// The length of the audio stream, in samples.
        /// </summary>
        public abstract uint Length { get; }
    }
}