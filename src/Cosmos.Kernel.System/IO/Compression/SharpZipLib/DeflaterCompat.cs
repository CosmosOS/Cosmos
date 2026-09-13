// Minimal replacements for the two members of SharpZipLib's Deflater and
// DeflaterHuffman classes that the inflater references (see README.md) —
// the compression side itself is not vendored.
namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    /// <summary>
    /// Stand-in for SharpZipLib's Deflater exposing the single constant the
    /// inflater needs to validate a zlib header.
    /// </summary>
    internal static class Deflater
    {
        /// <summary>
        /// The DEFLATE compression method id used in the zlib header.
        /// </summary>
        public const int DEFLATED = 8;
    }

    /// <summary>
    /// Stand-in for SharpZipLib's DeflaterHuffman exposing the bit-reversal
    /// helper used when building Huffman decoding tables.
    /// </summary>
    internal static class DeflaterHuffman
    {
        private static readonly short[] s_bit4Reverse = { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };

        /// <summary>
        /// Reverses the bits of a 16-bit value.
        /// </summary>
        /// <param name="toReverse">Value to reverse bits.</param>
        /// <returns>Value with bits reversed.</returns>
        public static short BitReverse(int toReverse)
        {
            return (short)((s_bit4Reverse[toReverse & 0xF] << 12) |
                           (s_bit4Reverse[(toReverse >> 4) & 0xF] << 8) |
                           (s_bit4Reverse[(toReverse >> 8) & 0xF] << 4) |
                           s_bit4Reverse[toReverse >> 12]);
        }
    }
}
