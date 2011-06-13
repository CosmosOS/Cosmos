/* Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * This software is based in part on the work of the Sam Leffler, Silicon 
 * Graphics, Inc. and contributors.
 *
 * Copyright (c) 1988-1997 Sam Leffler
 * Copyright (c) 1991-1997 Silicon Graphics, Inc.
 * For conditions of distribution and use, see the accompanying README file.
 */

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// Field bits (flags) for tags.
    /// </summary>
    /// <remarks>Field bits used to indicate fields that have been set in a directory, and to
    /// reference fields when manipulating a directory.</remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    static class FieldBit
    {
        internal const int SetLongs = 4;

        /////////////////////////////////////////////////////////////////////////
        // multi-item fields

        internal const short ImageDimensions = 1;
        internal const short TileDimensions = 2;
        internal const short Resolution = 3;
        internal const short Position = 4;

        /////////////////////////////////////////////////////////////////////////
        // single-item fields

        internal const short SubFileType = 5;
        internal const short BitsPerSample = 6;
        internal const short Compression = 7;
        internal const short Photometric = 8;
        internal const short Thresholding = 9;
        internal const short FillOrder = 10;
        internal const short Orientation = 15;
        internal const short SamplesPerPixel = 16;
        internal const short RowsPerStrip = 17;
        internal const short MinSampleValue = 18;
        internal const short MaxSampleValue = 19;
        internal const short PlanarConfig = 20;
        internal const short ResolutionUnit = 22;
        internal const short PageNumber = 23;
        internal const short StripByteCounts = 24;
        internal const short StripOffsets = 25;
        internal const short ColorMap = 26;
        internal const short ExtraSamples = 31;
        internal const short SampleFormat = 32;
        internal const short SMinSampleValue = 33;
        internal const short SMaxSampleValue = 34;
        internal const short ImageDepth = 35;
        internal const short TileDepth = 36;
        internal const short HalftoneHints = 37;
        internal const short YCbCrSubsampling = 39;
        internal const short YCbCrPositioning = 40;
        internal const short RefBlackWhite = 41;
        internal const short TransferFunction = 44;
        internal const short InkNames = 46;
        internal const short SubIFD = 49;

        /////////////////////////////////////////////////////////////////////////
        // end of support for well-known tags; codec-private tags follow

        /// <summary>
        /// This value is used to signify tags that are to be processed
        /// but otherwise ignored.<br/>
        /// This permits antiquated tags to be quietly read and discarded. Note that
        /// a bit <b>is</b> allocated for ignored tags; this is understood by the
        /// directory reading logic which uses this fact to avoid special-case handling.
        /// </summary>
        public const short Ignore = 0;

        /// <summary>
        /// This value is used to signify pseudo-tags.<br/>
        /// Pseudo-tags don't normally need field bits since they are not
        /// written to an output file (by definition). The library also has
        /// express logic to always query a codec for a pseudo-tag so allocating
        /// a field bit for one is a waste. If codec wants to promote the notion
        /// of a pseudo-tag being <i>set</i> or <i>unset</i> then it can do using
        /// internal state flags without polluting the field bit space defined
        /// for real tags.
        /// </summary>
        public const short Pseudo = 0;

        /// <summary>
        /// This value is used to signify custom tags.
        /// </summary>
        public const short Custom = 65;

        /// <summary>
        /// This value is used as a base (starting) value for codec-private tags.
        /// </summary>
        public const short Codec = 66;

        /// <summary>
        /// Last usable value for field bit. All tags values should be less than this value.
        /// </summary>
        public const short Last = (32 * SetLongs - 1);        
    }
}
