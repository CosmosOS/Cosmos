namespace BigGustave
{
    internal enum FilterType
    {
        /// <summary>
        /// The raw byte is unaltered.
        /// </summary>
        None = 0,
        /// <summary>
        /// The byte to the left.
        /// </summary>
        Sub = 1,
        /// <summary>
        /// The byte above.
        /// </summary>
        Up = 2,
        /// <summary>
        /// The mean of bytes left and above, rounded down.
        /// </summary>
        Average = 3,
        /// <summary>
        /// Byte to the left, above or top-left based on Paeth's algorithm.
        /// </summary>
        Paeth = 4
    }
}
