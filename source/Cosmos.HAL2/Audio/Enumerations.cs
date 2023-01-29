namespace Cosmos.HAL.Audio {
    /// <summary>
    /// Represents the bit depth of an audio sample or buffer.
    /// </summary>
    public enum AudioBitDepth : byte {
        /// <summary>
        /// 8 bits per sample.
        /// </summary>
        Bits8 = 1,

        /// <summary>
        /// 16 bits per sample.
        /// </summary>
        Bits16 = 2,

        /// <summary>
        /// 24 bits per sample.
        /// </summary>
        Bits24 = 3,

        /// <summary>
        /// 32 bits per sample.
        /// </summary>
        Bits32 = 4,
    }
}
