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

using System;

using BitMiracle.LibTiff.Classic;

namespace BitMiracle.Tiff2Rgba
{
    class Converter
    {
        public Compression m_compression = Compression.PACKBITS;
        public int m_rowsPerStrip = -1;
        public bool m_processByBlock;
        public bool m_noAlpha;
        public bool m_testFriendly;

        public bool tiffcvt(Tiff inImage, Tiff outImage)
        {
            FieldValue[] result = inImage.GetField(TiffTag.IMAGEWIDTH);
            if (result == null)
                return false;
            int width = result[0].ToInt();

            result = inImage.GetField(TiffTag.IMAGELENGTH);
            if (result == null)
                return false;
            int height = result[0].ToInt();

            copyField(inImage, outImage, TiffTag.SUBFILETYPE);
            outImage.SetField(TiffTag.IMAGEWIDTH, width);
            outImage.SetField(TiffTag.IMAGELENGTH, height);
            outImage.SetField(TiffTag.BITSPERSAMPLE, 8);
            outImage.SetField(TiffTag.COMPRESSION, m_compression);
            outImage.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);

            copyField(inImage, outImage, TiffTag.FILLORDER);
            outImage.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);

            if (m_noAlpha)
                outImage.SetField(TiffTag.SAMPLESPERPIXEL, 3);
            else
                outImage.SetField(TiffTag.SAMPLESPERPIXEL, 4);

            if (!m_noAlpha)
            {
                short[] v = new short[1];
                v[0] = (short)ExtraSample.ASSOCALPHA;
                outImage.SetField(TiffTag.EXTRASAMPLES, 1, v);
            }

            copyField(inImage, outImage, TiffTag.XRESOLUTION);
            copyField(inImage, outImage, TiffTag.YRESOLUTION);
            copyField(inImage, outImage, TiffTag.RESOLUTIONUNIT);
            outImage.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

            if (!m_testFriendly)
                outImage.SetField(TiffTag.SOFTWARE, Tiff.GetVersion());

            copyField(inImage, outImage, TiffTag.DOCUMENTNAME);
            
            if (m_processByBlock && inImage.IsTiled())
                return cvt_by_tile(inImage, outImage, width, height);
            else if (m_processByBlock)
                return cvt_by_strip(inImage, outImage, width, height);

            return cvt_whole_image(inImage, outImage, width, height);
        }

        private static void copyField(Tiff inImage, Tiff outImage, TiffTag tag)
        {
            FieldValue[] result = inImage.GetField(tag);
            if (result != null)
                outImage.SetField(tag, result[0]);
        }

        private static int multiply(int x, int y)
        {
            long res = (long)x * (long)y;
            if (res > int.MaxValue)
                return 0;

            return (int)res;
        }

        static bool cvt_by_tile(Tiff inImage, Tiff outImage, int width, int height)
        {
            int tile_width = 0;
            int tile_height = 0;

            FieldValue[] result = inImage.GetField(TiffTag.TILEWIDTH);
            if (result != null)
            {
                tile_width = result[0].ToInt();

                result = inImage.GetField(TiffTag.TILELENGTH);
                if (result != null)
                    tile_height = result[0].ToInt();
            }

            if (result == null)
            {
                Tiff.Error(inImage.FileName(), "Source image not tiled");
                return false;
            }

            outImage.SetField(TiffTag.TILEWIDTH, tile_width);
            outImage.SetField(TiffTag.TILELENGTH, tile_height);

            // Allocate tile buffer
            int raster_size = multiply(tile_width, tile_height);
            int rasterByteSize = multiply(raster_size, sizeof(int));
            if (raster_size == 0 || rasterByteSize == 0)
            {
                Tiff.Error(inImage.FileName(),
                    "Can't allocate buffer for raster of size {0}x{1}", tile_width, tile_height);
                return false;
            }

            int[] raster = new int[raster_size];
            byte[] rasterBytes = new byte[rasterByteSize];

            // Allocate a scanline buffer for swapping during the vertical mirroring pass.
            // (Request can't overflow given prior checks.)
            int[] wrk_line = new int[tile_width];

            // Loop over the tiles.
            for (int row = 0; row < height; row += tile_height)
            {
                for (int col = 0; col < width; col += tile_width)
                {
                    // Read the tile into an RGBA array
                    if (!inImage.ReadRGBATile(col, row, raster))
                        return false;

                    // For some reason the ReadRGBATile() function chooses the lower left corner
                    // as the origin. Vertically mirror scanlines.
                    for (int i_row = 0; i_row < tile_height / 2; i_row++)
                    {
                        int topIndex = tile_width * i_row * sizeof(int);
                        int bottomIndex = tile_width * (tile_height - i_row - 1) * sizeof(int);

                        Buffer.BlockCopy(raster, topIndex, wrk_line, 0, tile_width * sizeof(int));
                        Buffer.BlockCopy(raster, bottomIndex, raster, topIndex, tile_width * sizeof(int));
                        Buffer.BlockCopy(wrk_line, 0, raster, bottomIndex, tile_width * sizeof(int));
                    }

                    // Write out the result in a tile.
                    int tile = outImage.ComputeTile(col, row, 0, 0);
                    Buffer.BlockCopy(raster, 0, rasterBytes, 0, rasterByteSize);
                    if (outImage.WriteEncodedTile(tile, rasterBytes, rasterByteSize) == -1)
                        return false;
                }
            }

            return true;
        }

        private bool cvt_by_strip(Tiff inImage, Tiff outImage, int width, int height)
        {
            FieldValue[] result = inImage.GetField(TiffTag.ROWSPERSTRIP);
            if (result == null)
            {
                Tiff.Error(inImage.FileName(), "Source image not in strips");
                return false;
            }

            m_rowsPerStrip = result[0].ToInt();
            outImage.SetField(TiffTag.ROWSPERSTRIP, m_rowsPerStrip);

            // Allocate strip buffer
            int raster_size = multiply(width, m_rowsPerStrip);
            int rasterByteSize = multiply(raster_size, sizeof(int));
            if (raster_size == 0 || rasterByteSize == 0)
            {
                Tiff.Error(inImage.FileName(),
                    "Can't allocate buffer for raster of size {0}x{1}", width, m_rowsPerStrip);
                return false;
            }

            int[] raster = new int[raster_size];
            byte[] rasterBytes = new byte[rasterByteSize];

            // Allocate a scanline buffer for swapping during the vertical mirroring pass.
            // (Request can't overflow given prior checks.)
            int[] wrk_line = new int[width];

            // Loop over the strips.
            for (int row = 0; row < height; row += m_rowsPerStrip)
            {
                // Read the strip into an RGBA array
                if (!inImage.ReadRGBAStrip(row, raster))
                    return false;

                // Figure out the number of scanlines actually in this strip.
                int rows_to_write;
                if (row + m_rowsPerStrip > height)
                    rows_to_write = height - row;
                else
                    rows_to_write = m_rowsPerStrip;

                // For some reason the TIFFReadRGBAStrip() function chooses the lower left corner
                // as the origin. Vertically mirror scanlines.
                for (int i_row = 0; i_row < rows_to_write / 2; i_row++)
                {
                    int topIndex = width * i_row * sizeof(int);
                    int bottomIndex = width * (rows_to_write - i_row - 1) * sizeof(int);

                    Buffer.BlockCopy(raster, topIndex, wrk_line, 0, width * sizeof(int));
                    Buffer.BlockCopy(raster, bottomIndex, raster, topIndex, width * sizeof(int));
                    Buffer.BlockCopy(wrk_line, 0, raster, bottomIndex, width * sizeof(int));
                }

                // Write out the result in a strip
                int bytesToWrite = rows_to_write * width * sizeof(int);
                Buffer.BlockCopy(raster, 0, rasterBytes, 0, bytesToWrite);
                if (outImage.WriteEncodedStrip(row / m_rowsPerStrip, rasterBytes, bytesToWrite) == -1)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Read the whole image into one big RGBA buffer and then write out
        /// strips from that. This is using the traditional TIFFReadRGBAImage()
        /// API that we trust.
        /// </summary>
        private bool cvt_whole_image(Tiff inImage, Tiff outImage, int width, int height)
        {
            int pixel_count = width * height;

            /* XXX: Check the integer overflow. */
            if (width == 0 || height == 0 || (pixel_count / width) != height)
            {
                Tiff.Error(inImage.FileName(),
                    "Malformed input file; can't allocate buffer for raster of {0}x{1} size",
                    width, height);
                return false;
            }

            m_rowsPerStrip = outImage.DefaultStripSize(m_rowsPerStrip);
            outImage.SetField(TiffTag.ROWSPERSTRIP, m_rowsPerStrip);

            int[] raster = new int[pixel_count];

            /* Read the image in one chunk into an RGBA array */
            if (!inImage.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT, false))
                return false;

            /*
             * Do we want to strip away alpha components?
             */
            byte[] rasterBytes;
            int rasterByteSize;
            if (m_noAlpha)
            {
                rasterByteSize = pixel_count * 3;
                rasterBytes = new byte[rasterByteSize];

                for (int i = 0, rasterBytesPos = 0; i < pixel_count; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(raster[i]);
                    rasterBytes[rasterBytesPos++] = bytes[0];
                    rasterBytes[rasterBytesPos++] = bytes[1];
                    rasterBytes[rasterBytesPos++] = bytes[2];
                }
            }
            else
            {
                rasterByteSize = pixel_count * 4;
                rasterBytes = new byte[rasterByteSize];
                Buffer.BlockCopy(raster, 0, rasterBytes, 0, rasterByteSize);
            }

            /*
             * Write out the result in strips
             */
            for (int row = 0; row < height; row += m_rowsPerStrip)
            {
                int bytes_per_pixel;
                if (m_noAlpha)
                    bytes_per_pixel = 3;
                else
                    bytes_per_pixel = 4;

                int rows_to_write;
                if (row + m_rowsPerStrip > height)
                    rows_to_write = height - row;
                else
                    rows_to_write = m_rowsPerStrip;

                int offset = bytes_per_pixel * row * width;
                int count = bytes_per_pixel * rows_to_write * width;
                if (outImage.WriteEncodedStrip(row / m_rowsPerStrip, rasterBytes, offset, count) == -1)
                    return false;
            }

            return true;
        }
    }
}
