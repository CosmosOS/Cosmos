namespace BigGustave
{
    using System;

    /// <summary>
    /// Describes the interpretation of the image data.
    /// </summary>
    [Flags]
    internal enum ColorType : byte
    {
        /// <summary>
        /// Grayscale.
        /// </summary>
        None = 0,
        /// <summary>
        /// Colors are stored in a palette rather than directly in the data.
        /// </summary>
        PaletteUsed = 1,
        /// <summary>
        /// The image uses color.
        /// </summary>
        ColorUsed = 2,
        /// <summary>
        /// The image has an alpha channel.
        /// </summary>
        AlphaChannelUsed = 4
    }
}
