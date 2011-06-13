/****************************************************************************
 * 
 * LibJpeg.Net
 * Copyright (c) 2008-2011, Bit Miracle
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met: 
 * Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer. 
 * 
 * Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the 
 * documentation and/or other materials provided with the distribution. 
 * 
 * Neither the name of the Bit Miracle nor the names of its contributors 
 * may be used to endorse or promote products derived from this software 
 * without specific prior written permission. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS 
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL BIT MIRACLE BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
 * THE POSSIBILITY OF SUCH DAMAGE. 
 ****************************************************************************
 * 
 * Copyright (C) 1994-1996, Thomas G. Lane.
 * This file is part of the Independent JPEG Group's software.
 *
 ****************************************************************************
 *
 * This file contains routines to read input images in Microsoft "BMP"
 * format (MS Windows 3.x, OS/2 1.x, and OS/2 2.x flavors).
 * Currently, only 8-bit and 24-bit images are supported, not 1-bit or
 * 4-bit (feeding such low-depth images into JPEG would be silly anyway).
 * Also, we don't support RLE-compressed files.
 *
 * Original code was contributed by James Arthur Boucher.
 * This file contains routines to write output images in Microsoft "BMP"
 * format (MS Windows 3.x and OS/2 1.x flavors).
 * Either 8-bit colormapped or 24-bit full-color format can be written.
 * No compression is supported.
 *
 * These routines may need modification for non-Unix environments or
 * specialized applications.  As they stand, they assume output to
 * an ordinary stdio stream.
 *
 * This code contributed by James Arthur Boucher.
 *
 * To support 12-bit JPEG data, we'd have to scale output down to 8 bits.
 * This is not yet implemented.
 *
 * Since BMP stores scanlines bottom-to-top, we have to invert the image
 * from JPEG's top-to-bottom order.  To do this, we save the outgoing data
 * in a virtual array during put_pixel_row calls, then actually emit the
 * BMP file during finish_output.  The virtual array contains one byte per
 * pixel if the output is grayscale or colormapped, three if it is full color.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BitMiracle.LibJpeg;

namespace GuessKernel.ImageFormats
{

    #region BmpCompressor
    class BmpCompressor : JpegCompressor
    {
        /* Target file spec; filled in by djpeg.c after object is created. */
        public Stream output_file;

        /* Output pixel-row buffer.  Created by module init or start_output.
         * Width is cinfo.output_width * cinfo.output_components;
         * height is buffer_height.
         */
        public byte[][] buffer;
        public int buffer_height;

        private JpegDecompressor cinfo;
        private bool m_putGrayRows;
        private bool is_os2;        /* saves the OS2 format request flag */

        private JpegVirtualArray<byte> whole_image;  /* needed to reverse row order */
        private int data_width;  /* bytes per row */
        private int row_width;       /* physical width of one row in the BMP file */
        private int pad_bytes;      /* number of padding bytes needed per row */
        private int cur_output_row;  /* next row# to write to virtual array */

        public BmpCompressor(JpegDecompressor cinfo, bool is_os2)
        {
            this.cinfo = cinfo;
            this.is_os2 = is_os2;

            if (cinfo.Out_color_space == ColorSpace.Grayscale)
            {
                m_putGrayRows = true;
            }
            else if (cinfo.Out_color_space == ColorSpace.RGB)
            {
                if (cinfo.Quantize_colors)
                    m_putGrayRows = true;
                else
                    m_putGrayRows = false;
            }
            else
            {
                throw new Exception("BMP output must be grayscale or RGB");
            }

            /* Calculate output image dimensions so we can allocate space */
            cinfo.jpeg_calc_output_dimensions();

            /* Determine width of rows in the BMP file (padded to 4-byte boundary). */
            row_width = cinfo.Output_width * cinfo.Output_components;
            data_width = row_width;
            while ((row_width & 3) != 0)
                row_width++;

            pad_bytes = row_width - data_width;

            /* Allocate space for inversion array, prepare for write pass */
            whole_image = JpegCommonBase.CreateSamplesArray(row_width, cinfo.Output_height);
            whole_image.ErrorProcessor = cinfo;

            cur_output_row = 0;

            /* Create decompressor output buffer. */
            buffer = JpegCommonBase.AllocJpegSamples(row_width, 1);
            buffer_height = 1;
        }

        public BmpCompressor(JpegDecompressor cinfo, Stream output, bool is_os2) : this(cinfo, is_os2)
        {
            this.output_file = output;
            
        }

        /// <summary>
        /// Startup: normally writes the file header.
        /// In this module we may as well postpone everything until finish_output.
        /// </summary>
        public void start_output()
        {
            /* no work here */
        }

        /// <summary>
        /// Write some pixel data.
        /// In this module rows_supplied will always be 1.
        /// </summary>
        public void put_pixel_rows(int rows_supplied)
        {
            if (m_putGrayRows)
                put_gray_rows(rows_supplied);
            else
                put_24bit_rows(rows_supplied);
        }

        /// <summary>
        /// Finish up at the end of the file.
        /// Here is where we really output the BMP file.
        /// </summary>
        public void finish_output()
        {
            /* Write the header and colormap */
            if (is_os2)
                write_os2_header();
            else
                write_bmp_header();


            /* Write the file body from our virtual array */
            for (int row = cinfo.Output_height; row > 0; row--)
            {
                byte[][] image_ptr = whole_image.Access(row - 1, 1);
                int imageIndex = 0;
                for (int col = row_width; col > 0; col--)
                {
                    output_file.WriteByte(image_ptr[0][imageIndex]);
                    imageIndex++;
                }
            }

            /* Make sure we wrote the output file OK */
            output_file.Flush();
        }

        /// <summary>
        /// Write some pixel data.
        /// In this module rows_supplied will always be 1.
        /// 
        /// This version is for writing 24-bit pixels
        /// </summary>
        private void put_24bit_rows(int rows_supplied)
        {
            /* Access next row in virtual array */
            byte[][] image_ptr = whole_image.Access(cur_output_row, 1);
            cur_output_row++;

            /* Transfer data.  Note destination values must be in BGR order
             * (even though Microsoft's own documents say the opposite).
             */
            int bufferIndex = 0;
            int imageIndex = 0;
            for (int col = cinfo.Output_width; col > 0; col--)
            {
                image_ptr[0][imageIndex + 2] = buffer[0][bufferIndex];   /* can omit GETJSAMPLE() safely */
                bufferIndex++;
                image_ptr[0][imageIndex + 1] = buffer[0][bufferIndex];
                bufferIndex++;
                image_ptr[0][imageIndex] = buffer[0][bufferIndex];
                bufferIndex++;
                imageIndex += 3;
            }

            /* Zero out the pad bytes. */
            int pad = pad_bytes;
            while (--pad >= 0)
            {
                image_ptr[0][imageIndex] = 0;
                imageIndex++;
            }
        }

        /// <summary>
        /// Write some pixel data.
        /// In this module rows_supplied will always be 1.
        /// 
        /// This version is for grayscale OR quantized color output
        /// </summary>
        private void put_gray_rows(int rows_supplied)
        {
            /* Access next row in virtual array */
            byte[][] image_ptr = whole_image.Access(cur_output_row, 1);
            cur_output_row++;

            /* Transfer data. */
            int index = 0;
            for (int col = cinfo.Output_width; col > 0; col--)
            {
                image_ptr[0][index] = buffer[0][index];/* can omit GETJSAMPLE() safely */
                index++;
            }

            /* Zero out the pad bytes. */
            int pad = pad_bytes;
            while (--pad >= 0)
            {
                image_ptr[0][index] = 0;
                index++;
            }
        }

        /// <summary>
        /// Write a Windows-style BMP file header, including colormap if needed
        /// </summary>
        private void write_bmp_header()
        {
            int bits_per_pixel;
            int cmap_entries;

            /* Compute colormap size and total file size */
            if (cinfo.Out_color_space == ColorSpace.RGB)
            {
                if (cinfo.Quantize_colors)
                {
                    /* Colormapped RGB */
                    bits_per_pixel = 8;
                    cmap_entries = 256;
                }
                else
                {
                    /* Unquantized, full color RGB */
                    bits_per_pixel = 24;
                    cmap_entries = 0;
                }
            }
            else
            {
                /* Grayscale output.  We need to fake a 256-entry colormap. */
                bits_per_pixel = 8;
                cmap_entries = 256;
            }

            /* File size */
            int headersize = 14 + 40 + cmap_entries * 4; /* Header and colormap */
            int bfSize = headersize + row_width * cinfo.Output_height;

            /* Set unused fields of header to 0 */
            byte[] bmpfileheader = new byte[14];
            byte[] bmpinfoheader = new byte[40];

            /* Fill the file header */
            bmpfileheader[0] = 0x42;    /* first 2 bytes are ASCII 'B', 'M' */
            bmpfileheader[1] = 0x4D;
            PUT_4B(bmpfileheader, 2, bfSize); /* bfSize */
            /* we leave bfReserved1 & bfReserved2 = 0 */
            PUT_4B(bmpfileheader, 10, headersize); /* bfOffBits */

            /* Fill the info header (Microsoft calls this a BITMAPINFOHEADER) */
            PUT_2B(bmpinfoheader, 0, 40);   /* biSize */
            PUT_4B(bmpinfoheader, 4, cinfo.Output_width); /* biWidth */
            PUT_4B(bmpinfoheader, 8, cinfo.Output_height); /* biHeight */
            PUT_2B(bmpinfoheader, 12, 1);   /* biPlanes - must be 1 */
            PUT_2B(bmpinfoheader, 14, bits_per_pixel); /* biBitCount */
            /* we leave biCompression = 0, for none */
            /* we leave biSizeImage = 0; this is correct for uncompressed data */

            if (cinfo.Density_unit == DensityUnit.DotsCm)
            {
                /* if have density in dots/cm, then */
                PUT_4B(bmpinfoheader, 24, (int)(cinfo.X_density * 100)); /* XPels/M */
                PUT_4B(bmpinfoheader, 28, (int)(cinfo.Y_density * 100)); /* XPels/M */
            }
            PUT_2B(bmpinfoheader, 32, cmap_entries); /* biClrUsed */
            /* we leave biClrImportant = 0 */

            try
            {
                output_file.Write(bmpfileheader, 0, 14);
            }
            catch
            {
                throw new Exception("An error occurred when writing the output file. Perhaps your out of disk space?");
            }

            try
            {
                output_file.Write(bmpinfoheader, 0, 40);
            }
            catch
            {
                throw new Exception("An error occurred when writing the output file. Perhaps your out of disk space?");
            }

            if (cmap_entries > 0)
                write_colormap(cmap_entries, 4);
        }

        /// <summary>
        /// Write an OS2-style BMP file header, including colormap if needed
        /// </summary>
        private void write_os2_header()
        {
            int bits_per_pixel;
            int cmap_entries;

            /* Compute colormap size and total file size */
            if (cinfo.Out_color_space == ColorSpace.RGB)
            {
                if (cinfo.Quantize_colors)
                {
                    /* Colormapped RGB */
                    bits_per_pixel = 8;
                    cmap_entries = 256;
                }
                else
                {
                    /* Unquantized, full color RGB */
                    bits_per_pixel = 24;
                    cmap_entries = 0;
                }
            }
            else
            {
                /* Grayscale output.  We need to fake a 256-entry colormap. */
                bits_per_pixel = 8;
                cmap_entries = 256;
            }

            /* File size */
            int headersize = 14 + 12 + cmap_entries * 3; /* Header and colormap */
            int bfSize = headersize + row_width * cinfo.Output_height;

            /* Set unused fields of header to 0 */
            byte[] bmpfileheader = new byte[14];
            byte[] bmpcoreheader = new byte[12];

            /* Fill the file header */
            bmpfileheader[0] = 0x42;    /* first 2 bytes are ASCII 'B', 'M' */
            bmpfileheader[1] = 0x4D;
            PUT_4B(bmpfileheader, 2, bfSize); /* bfSize */
            /* we leave bfReserved1 & bfReserved2 = 0 */
            PUT_4B(bmpfileheader, 10, headersize); /* bfOffBits */

            /* Fill the info header (Microsoft calls this a BITMAPCOREHEADER) */
            PUT_2B(bmpcoreheader, 0, 12);   /* bcSize */
            PUT_2B(bmpcoreheader, 4, cinfo.Output_width); /* bcWidth */
            PUT_2B(bmpcoreheader, 6, cinfo.Output_height); /* bcHeight */
            PUT_2B(bmpcoreheader, 8, 1);    /* bcPlanes - must be 1 */
            PUT_2B(bmpcoreheader, 10, bits_per_pixel); /* bcBitCount */

            try
            {
                output_file.Write(bmpfileheader, 0, 14);
            }
            catch
            {
                throw new Exception("An error occurred when writing the output file. Perhaps your out of disk space?");
            }

            try
            {
                output_file.Write(bmpcoreheader, 0, 12);
            }
            catch
            {
                throw new Exception("An error occurred when writing the output file. Perhaps your out of disk space?");
            }

            if (cmap_entries > 0)
                write_colormap(cmap_entries, 3);
        }

        /// <summary>
        /// Write the colormap.
        /// Windows uses BGR0 map entries; OS/2 uses BGR entries.
        /// </summary>
        private void write_colormap(int map_colors, int map_entry_size)
        {
            byte[][] colormap = cinfo.Colormap;
            int num_colors = cinfo.Actual_number_of_colors;

            int i = 0;
            if (colormap != null)
            {
                if (cinfo.Out_color_components == 3)
                {
                    /* Normal case with RGB colormap */
                    for (i = 0; i < num_colors; i++)
                    {
                        output_file.WriteByte(colormap[2][i]);
                        output_file.WriteByte(colormap[1][i]);
                        output_file.WriteByte(colormap[0][i]);
                        if (map_entry_size == 4)
                            output_file.WriteByte(0);
                    }
                }
                else
                {
                    /* Grayscale colormap (only happens with grayscale quantization) */
                    for (i = 0; i < num_colors; i++)
                    {
                        output_file.WriteByte(colormap[0][i]);
                        output_file.WriteByte(colormap[0][i]);
                        output_file.WriteByte(colormap[0][i]);
                        if (map_entry_size == 4)
                            output_file.WriteByte(0);
                    }
                }
            }
            else
            {
                /* If no colormap, must be grayscale data.  Generate a linear "map". */
                for (i = 0; i < 256; i++)
                {
                    output_file.WriteByte((byte)i);
                    output_file.WriteByte((byte)i);
                    output_file.WriteByte((byte)i);
                    if (map_entry_size == 4)
                        output_file.WriteByte(0);
                }
            }

            /* Pad colormap with zeros to ensure specified number of colormap entries */
            if (i > map_colors)
                throw new Exception(string.Format("Output file format cannot handle %d colormap entries", i));

            for (; i < map_colors; i++)
            {
                output_file.WriteByte(0);
                output_file.WriteByte(0);
                output_file.WriteByte(0);
                if (map_entry_size == 4)
                    output_file.WriteByte(0);
            }
        }

        private static void PUT_2B(byte[] array, int offset, int value)
        {
            array[offset] = (byte)((value) & 0xFF);
            array[offset + 1] = (byte)(((value) >> 8) & 0xFF);
        }

        private static void PUT_4B(byte[] array, int offset, int value)
        {
            array[offset] = (byte)((value) & 0xFF);
            array[offset + 1] = (byte)(((value) >> 8) & 0xFF);
            array[offset + 2] = (byte)(((value) >> 16) & 0xFF);
            array[offset + 3] = (byte)(((value) >> 24) & 0xFF);
        }
    }
    #endregion

    #region JpegDecompressor
    class BmpDecompressor : JpegDecompressor
    {
        private enum PixelRowsMethod
        {
            preload,
            use8bit,
            use24bit
        }

        public Stream input_file;
        public byte[][] buffer;
        public uint buffer_height;

        private JpegCompressor cinfo;
        private PixelRowsMethod m_pixelRowsMethod;

        // BMP colormap (converted to my format)
        private byte[][] colormap;

        // Needed to reverse row order
        private JpegVirtualArray<byte> whole_image;

        // Current source row number
        private int source_row;

        // Physical width of scanlines in file
        private int row_width;

        // remembers 8- or 24-bit format
        private int bits_per_pixel;

        public BmpDecompressor(JpegCompressor cinfo)
        {
            this.cinfo = cinfo;
        }

        public BmpDecompressor(JpegCompressor cinfo, Stream input)
        {
            this.cinfo = cinfo;
            this.input_file = input;
        }

        /// <summary>
        /// Read the file header; detects image size and component count.
        /// </summary>
        public void start_input()
        {
            byte[] bmpfileheader = new byte[14];
            /* Read and verify the bitmap file header */
            if (!ReadOK(input_file, bmpfileheader, 0, 14))
                throw new Exception("The file ended pre-maturely. Perhaps the file is corrupt?");

            if (GET_2B(bmpfileheader, 0) != 0x4D42) /* 'BM' */
                throw new Exception("Not a BMP file. File header doesn't start with BM");

            int bfOffBits = GET_4B(bmpfileheader, 10);
            /* We ignore the remaining file header fields */

            /* The info header might be 12 bytes (OS/2 1.x), 40 bytes (Windows),
             * or 64 bytes (OS/2 2.x).  Check the first 4 bytes to find out which.
             */
            byte[] bmpinfoheader = new byte[64];
            if (!ReadOK(input_file, bmpinfoheader, 0, 4))
                throw new Exception("The file ended pre-maturely. Perhaps the file is corrupt?");

            int headerSize = GET_4B(bmpinfoheader, 0);
            if (headerSize < 12 || headerSize > 64)
                throw new Exception("Invalid BMP file: Bad Header Length.");

            if (!ReadOK(input_file, bmpinfoheader, 4, headerSize - 4))
                throw new Exception("The file ended pre-maturely. Perhaps the file is corrupt?");

            int biWidth = 0;      /* initialize to avoid compiler warning */
            int biHeight = 0;
            int biPlanes;
            int biCompression;
            int biXPelsPerMeter;
            int biYPelsPerMeter;
            int biClrUsed = 0;
            int mapentrysize = 0;       /* 0 indicates no colormap */
            switch (headerSize)
            {
                case 12:
                    /* Decode OS/2 1.x header (Microsoft calls this a BITMAPCOREHEADER) */
                    biWidth = GET_2B(bmpinfoheader, 4);
                    biHeight = GET_2B(bmpinfoheader, 6);
                    biPlanes = GET_2B(bmpinfoheader, 8);
                    bits_per_pixel = GET_2B(bmpinfoheader, 10);

                    switch (bits_per_pixel)
                    {
                        case 8:
                            /* colormapped image */
                            mapentrysize = 3;       /* OS/2 uses RGBTRIPLE colormap */
                            break;
                        case 24:
                            /* RGB image */
                            break;
                        default:
                            throw new Exception("Bad Color Depth: Only 8-bit and 24-bit BMP files are supported.");
                    }

                    if (biPlanes != 1)
                        throw new Exception("Invalid BMP file: biPlanes is not equal to 1");
                    break;
                case 40:
                case 64:
                    /* Decode Windows 3.x header (Microsoft calls this a BITMAPINFOHEADER) */
                    /* or OS/2 2.x header, which has additional fields that we ignore */
                    biWidth = GET_4B(bmpinfoheader, 4);
                    biHeight = GET_4B(bmpinfoheader, 8);
                    biPlanes = GET_2B(bmpinfoheader, 12);
                    bits_per_pixel = GET_2B(bmpinfoheader, 14);
                    biCompression = GET_4B(bmpinfoheader, 16);
                    biXPelsPerMeter = GET_4B(bmpinfoheader, 24);
                    biYPelsPerMeter = GET_4B(bmpinfoheader, 28);
                    biClrUsed = GET_4B(bmpinfoheader, 32);
                    /* biSizeImage, biClrImportant fields are ignored */

                    switch (bits_per_pixel)
                    {
                        case 8:
                            /* colormapped image */
                            mapentrysize = 4;       /* Windows uses RGBQUAD colormap */
                            break;
                        case 24:
                            /* RGB image */
                            break;
                        default:
                            throw new Exception("Bad Color Depth: Only 8-bit and 24-bit BMP files are supported.");
                    }

                    if (biPlanes != 1)
                        throw new Exception("Invalid BMP file: biPlanes is not equal to 1");

                    if (biCompression != 0)
                        throw new Exception("Sorry, compressed BMPs not yet supported.");

                    if (biXPelsPerMeter > 0 && biYPelsPerMeter > 0)
                    {
                        /* Set JFIF density parameters from the BMP data */
                        cinfo.X_density = (short)(biXPelsPerMeter / 100); /* 100 cm per meter */
                        cinfo.Y_density = (short)(biYPelsPerMeter / 100);
                        cinfo.Density_unit = DensityUnit.DotsCm;
                    }
                    break;
                default:
                    throw new Exception("Invalid BMP file: Bad Header Length.");
            }

            /* Compute distance to bitmap data --- will adjust for colormap below */
            int bPad = bfOffBits - (headerSize + 14);

            /* Read the colormap, if any */
            if (mapentrysize > 0)
            {
                if (biClrUsed <= 0)
                    biClrUsed = 256;        /* assume it's 256 */
                else if (biClrUsed > 256)
                    throw new Exception("Unsupported BMP colormap format.");

                /* Allocate space to store the colormap */
                colormap = JpegCommonBase.AllocJpegSamples(biClrUsed, 3);
                /* and read it from the file */
                read_colormap(biClrUsed, mapentrysize);
                /* account for size of colormap */
                bPad -= biClrUsed * mapentrysize;
            }

            /* Skip any remaining pad bytes */
            if (bPad < 0)           /* incorrect bfOffBits value? */
                throw new Exception("Invalid BMP file: Bad Header Length.");

            while (--bPad >= 0)
                read_byte();

            /* Compute row width in file, including padding to 4-byte boundary */
            if (bits_per_pixel == 24)
                row_width = biWidth * 3;
            else
                row_width = biWidth;

            while ((row_width & 3) != 0)
                row_width++;

            /* Allocate space for inversion array, prepare for preload pass */
            whole_image = JpegCommonBase.CreateSamplesArray(row_width, biHeight);
            whole_image.ErrorProcessor = cinfo;
            m_pixelRowsMethod = PixelRowsMethod.preload;

            /* Allocate one-row buffer for returned data */
            buffer = JpegCommonBase.AllocJpegSamples(biWidth * 3, 1);
            buffer_height = 1;

            cinfo.In_color_space = ColorSpace.RGB;
            cinfo.Input_components = 3;
            cinfo.Data_precision = 8;
            cinfo.Image_width = biWidth;
            cinfo.Image_height = biHeight;
        }

        public int get_pixel_rows()
        {
            if (m_pixelRowsMethod == PixelRowsMethod.preload)
                return preload_image();
            else if (m_pixelRowsMethod == PixelRowsMethod.use8bit)
                return get_8bit_row();

            return get_24bit_row();
        }

        // Finish up at the end of the file.
        public void finish_input()
        {
            // no work
        }


        #region 8-Bit Support
        /// <summary>
        /// Read one row of pixels. 
        /// The image has been read into the whole_image array, but is otherwise
        /// unprocessed.  We must read it out in top-to-bottom row order, and if
        /// it is an 8-bit image, we must expand colormapped pixels to 24bit format.
        /// This version is for reading 8-bit colormap indexes.
        /// </summary>
        private int get_8bit_row()
        {
            /* Fetch next row from virtual array */
            source_row--;

            byte[][] image_ptr = whole_image.Access(source_row, 1);

            /* Expand the colormap indexes to real data */
            int imageIndex = 0;
            int outIndex = 0;
            for (int col = cinfo.Image_width; col > 0; col--)
            {
                int t = image_ptr[0][imageIndex];
                imageIndex++;

                buffer[0][outIndex] = colormap[0][t]; /* can omit GETbyte() safely */
                outIndex++;
                buffer[0][outIndex] = colormap[1][t];
                outIndex++;
                buffer[0][outIndex] = colormap[2][t];
                outIndex++;
            }

            return 1;
        }
        #endregion

        #region 24-Bit Support
        /// <summary>
        /// Read one row of pixels. 
        /// The image has been read into the whole_image array, but is otherwise
        /// unprocessed.  We must read it out in top-to-bottom row order, and if
        /// it is an 8-bit image, we must expand colormapped pixels to 24bit format.
        /// This version is for reading 24-bit pixels.
        /// </summary>
        private int get_24bit_row()
        {
            /* Fetch next row from virtual array */
            source_row--;
            byte[][] image_ptr = whole_image.Access(source_row, 1);

            /* Transfer data.  Note source values are in BGR order
             * (even though Microsoft's own documents say the opposite).
             */
            int imageIndex = 0;
            int outIndex = 0;

            for (int col = cinfo.Image_width; col > 0; col--)
            {
                buffer[0][outIndex + 2] = image_ptr[0][imageIndex];   /* can omit GETbyte() safely */
                imageIndex++;
                buffer[0][outIndex + 1] = image_ptr[0][imageIndex];
                imageIndex++;
                buffer[0][outIndex] = image_ptr[0][imageIndex];
                imageIndex++;
                outIndex += 3;
            }

            return 1;
        }
        #endregion


        /// <summary>
        /// This method loads the image into whole_image during the first call on
        /// get_pixel_rows. 
        /// </summary>
        private int preload_image()
        {
            /* Read the data into a virtual array in input-file row order. */
            for (int row = 0; row < cinfo.Image_height; row++)
            {

                byte[][] image_ptr = whole_image.Access(row, 1);
                int imageIndex = 0;
                for (int col = row_width; col > 0; col--)
                {
                    /* inline copy of read_byte() for speed */
                    int c = input_file.ReadByte();
                    if (c == -1)
                        throw new Exception("The file ended pre-maturely. Perhaps the file is corrupt?");

                    image_ptr[0][imageIndex] = (byte)c;
                    imageIndex++;
                }
            }

            /* Set up to read from the virtual array in top-to-bottom order */
            switch (bits_per_pixel)
            {
                case 8:
                    m_pixelRowsMethod = PixelRowsMethod.use8bit;
                    break;
                case 24:
                    m_pixelRowsMethod = PixelRowsMethod.use24bit;
                    break;
                default:
                    throw new Exception("Bad Color Depth: Only 8-bit and 24-bit BMP files are supported.");
            }

            source_row = cinfo.Image_height;

            /* And read the first row */
            return get_pixel_rows();
        }

        // Read next byte from BMP file
        private int read_byte()
        {
            int c = input_file.ReadByte();
            if (c == -1)
                throw new Exception("The file ended pre-maturely. Perhaps the file is corrupt?");

            return c;
        }

        // Read the colormap from a BMP file
        private void read_colormap(int cmaplen, int mapentrysize)
        {
            switch (mapentrysize)
            {
                case 3:
                    /* BGR format (occurs in OS/2 files) */
                    for (int i = 0; i < cmaplen; i++)
                    {
                        colormap[2][i] = (byte)read_byte();
                        colormap[1][i] = (byte)read_byte();
                        colormap[0][i] = (byte)read_byte();
                    }
                    break;
                case 4:
                    /* BGR0 format (occurs in MS Windows files) */
                    for (int i = 0; i < cmaplen; i++)
                    {
                        colormap[2][i] = (byte)read_byte();
                        colormap[1][i] = (byte)read_byte();
                        colormap[0][i] = (byte)read_byte();
                        read_byte();
                    }
                    break;
                default:
                    throw new Exception("Unsupported BMP colormap format.");
            }
        }

        private static bool ReadOK(Stream file, byte[] buffer, int offset, int len)
        {
            int read = file.Read(buffer, offset, len);
            return (read == len);
        }

        private static int GET_2B(byte[] array, int offset)
        {
            return (int)array[offset] + ((int)array[offset + 1] << 8);
        }

        private static int GET_4B(byte[] array, int offset)
        {
            return (int)array[offset] + ((int)array[offset + 1] << 8) + ((int)array[offset + 2] << 16) + ((int)array[offset + 3] << 24);
        }
    }
    #endregion

}
