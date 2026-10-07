namespace BigGustave
{
    using System.Collections.Generic;

    internal static class Adam7
    {
        /// <summary>
        /// For a given pass number (1 indexed) the scanline indexes of the lines included in that pass in the 8x8 grid.
        /// </summary>
        private static readonly IReadOnlyDictionary<int, int[]> PassToScanlineGridIndex = new Dictionary<int, int[]>
        {
            { 1, new []{ 0 } },
            { 2, new []{ 0 } },
            { 3, new []{ 4 } },
            { 4, new []{ 0, 4 } },
            { 5, new []{ 2, 6 } },
            { 6, new[] { 0, 2, 4, 6 } },
            { 7, new[] { 1, 3, 5, 7 } }
        };

        private static readonly IReadOnlyDictionary<int, int[]> PassToScanlineColumnIndex = new Dictionary<int, int[]>
        {
            { 1, new []{ 0 } },
            { 2, new []{ 4 } },
            { 3, new []{ 0, 4 } },
            { 4, new []{ 2, 6 } },
            { 5, new []{ 0, 2, 4, 6 } },
            { 6, new []{ 1, 3, 5, 7 } },
            { 7, new []{ 0, 1, 2, 3, 4, 5, 6, 7 } }
        };

        /*
         * To go from raw image data to interlaced:
         *
         * An 8x8 grid is repeated over the image. There are 7 passes and the indexes in this grid correspond to the
         * pass number including that pixel. Each row in the grid corresponds to a scanline.
         *
         * 1 6 4 6 2 6 4 6 - Scanline 0: pass 1 has pixel 0, 8, 16, etc. pass 2 has pixel 4, 12, 20, etc.
         * 7 7 7 7 7 7 7 7
         * 5 6 5 6 5 6 5 6
         * 7 7 7 7 7 7 7 7
         * 3 6 4 6 3 6 4 6
         * 7 7 7 7 7 7 7 7
         * 5 6 5 6 5 6 5 6
         * 7 7 7 7 7 7 7 7
         *
         *
         *
         */

        public static int GetNumberOfScanlinesInPass(ImageHeader header, int pass)
        {
            var indices = PassToScanlineGridIndex[pass + 1];

            var mod = header.Height % 8;

            var fitsExactly = mod == 0;

            if (fitsExactly)
            {
                return indices.Length * (header.Height / 8);
            }

            var additionalLines = 0;
            for (var i = 0; i < indices.Length; i++)
            {
                if (indices[i] < mod)
                {
                    additionalLines++;
                }
            }

            return (indices.Length * (header.Height / 8)) + additionalLines;
        }

        public static int GetPixelsPerScanlineInPass(ImageHeader header, int pass)
        {
            var indices = PassToScanlineColumnIndex[pass + 1];

            var mod = header.Width % 8;

            var fitsExactly = mod == 0;

            if (fitsExactly)
            {
                return indices.Length * (header.Width / 8);
            }

            var additionalColumns = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] < mod)
                {
                    additionalColumns++;
                }
            }

            return (indices.Length * (header.Width / 8)) + additionalColumns;
        }

        public static (int x, int y) GetPixelIndexForScanlineInPass(ImageHeader header, int pass, int scanlineIndex, int indexInScanline)
        {
            var columnIndices = PassToScanlineColumnIndex[pass + 1];
            var rows = PassToScanlineGridIndex[pass + 1];

            var actualRow = scanlineIndex % rows.Length;
            var actualCol = indexInScanline % columnIndices.Length;
            var precedingRows = 8 * (scanlineIndex / rows.Length);
            var precedingCols = 8 * (indexInScanline / columnIndices.Length);

            return (precedingCols + columnIndices[actualCol], precedingRows + rows[actualRow]);
        }
    }
}
