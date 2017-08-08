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
 * THIS Software IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS 
 * IS" AND Any EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED 
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL BIT MIRACLE BE 
 * LIABLE FOR Any DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON Any THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN Any WAY OUT OF THE USE OF THIS Software, EVEN IF ADVISED OF 
 * THE POSSIBILITY OF SUCH DAMAGE. 
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Collections.ObjectModel;

namespace BitMiracle.LibJpeg
{
    #region JpegImage
    public sealed class JpegImage : IDisposable
    {
        private bool m_alreadyDisposed;
        private List<SampleRow> m_rows = new List<SampleRow>();
        private int m_width;
        private int m_height;
        private byte m_bitsPerComponent;
        private byte m_componentsPerSample;
        private ColorSpace m_colorspace;
        private MemoryStream m_compressedData;
        private CompressionParameters m_compressionParameters;
        private MemoryStream m_decompressedData;
        private Bitmap m_bitmap;
        public JpegImage(System.Drawing.Bitmap bitmap)
        {
            createFromBitmap(bitmap);
        }
        public JpegImage(Stream imageData)
        {
            createFromStream(imageData);
        }
        public JpegImage(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            using (FileStream input = new FileStream(fileName, FileMode.Open))
                createFromStream(input);
        }
        public JpegImage(SampleRow[] sampleData, ColorSpace colorspace)
        {
            if (sampleData == null)
                throw new ArgumentNullException("sampleData");

            if (sampleData.Length == 0)
                throw new ArgumentException("sampleData must not be empty");

            if (colorspace == ColorSpace.Unknown)
                throw new ArgumentException("Unknown colorspace");

            m_rows = new List<SampleRow>(sampleData);

            SampleRow firstRow = m_rows[0];
            m_width = firstRow.Length;
            m_height = m_rows.Count;

            Sample firstSample = firstRow[0];
            m_bitsPerComponent = firstSample.BitsPerComponent;
            m_componentsPerSample = firstSample.ComponentCount;
            m_colorspace = colorspace;
        }
        public static JpegImage FromBitmap(Bitmap bitmap)
        {
            return new JpegImage(bitmap);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!m_alreadyDisposed)
            {
                if (disposing)
                {
                    if (m_compressedData != null)
                        m_compressedData.Dispose();

                    if (m_decompressedData != null)
                        m_decompressedData.Dispose();
                    if (m_bitmap != null)
                        m_bitmap.Dispose();
                }
                m_compressionParameters = null;
                m_compressedData = null;
                m_decompressedData = null;
                m_bitmap = null;
                m_rows = null;
                m_alreadyDisposed = true;
            }
        }

        public int Width
        {
            get
            {
                return m_width;
            }
            internal set
            {
                m_width = value;
            }
        }
        public int Height
        {
            get
            {
                return m_height;
            }
            internal set
            {
                m_height = value;
            }
        }
        public byte ComponentsPerSample
        {
            get
            {
                return m_componentsPerSample;
            }
            internal set
            {
                m_componentsPerSample = value;
            }
        }
        public byte BitsPerComponent
        {
            get
            {
                return m_bitsPerComponent;
            }
            internal set
            {
                m_bitsPerComponent = value;
            }
        }
        public ColorSpace Colorspace
        {
            get
            {
                return m_colorspace;
            }
            internal set
            {
                m_colorspace = value;
            }
        }
        public SampleRow GetRow(int rowNumber)
        {
            return m_rows[rowNumber];
        }
        public void WriteJpeg(Stream output)
        {
            WriteJpeg(output, new CompressionParameters());
        }
        public void WriteJpeg(Stream output, CompressionParameters parameters)
        {
            compress(parameters);
            compressedData.WriteTo(output);
        }
        public void WriteBitmap(Stream output)
        {
            decompressedData.WriteTo(output);
        }
        public Bitmap ToBitmap()
        {
            return bitmap.Clone() as Bitmap;
        }
        private MemoryStream compressedData
        {
            get
            {
                if (m_compressedData == null)
                    compress(new CompressionParameters());
                return m_compressedData;
            }
        }
        private MemoryStream decompressedData
        {
            get
            {
                if (m_decompressedData == null)
                    fillDecompressedData();
                return m_decompressedData;
            }
        }
        private Bitmap bitmap
        {
            get
            {
                if (m_bitmap == null)
                {
                    long position = compressedData.Position;
                    m_bitmap = new Bitmap(compressedData);
                    compressedData.Seek(position, SeekOrigin.Begin);
                }

                return m_bitmap;
            }
        }
        internal void addSampleRow(SampleRow row)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            m_rows.Add(row);
        }
        private static bool isCompressed(Stream imageData)
        {
            if (imageData == null)
                return false;

            if (imageData.Length <= 2)
                return false;

            imageData.Seek(0, SeekOrigin.Begin);
            int first = imageData.ReadByte();
            int second = imageData.ReadByte();
            return (first == 0xFF && second == (int)JpegMarkerType.SOI);
        }
        private void createFromBitmap(Bitmap bitmap)
        {
            initializeFromBitmap(bitmap);
            compress(new CompressionParameters());
        }
        private void createFromStream(Stream imageData)
        {
            if (imageData == null)
                throw new ArgumentNullException("imageData");

            if (isCompressed(imageData))
            {
                m_compressedData = Utils.CopyStream(imageData);
                decompress();
            }
            else
            {
                createFromBitmap(new Bitmap(imageData));
            }
        }
        private void initializeFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            m_bitmap = bitmap;
            m_width = m_bitmap.Width;
            m_height = m_bitmap.Height;
            processPixelFormat(bitmap.PixelFormat);
            fillSamplesFromBitmap();
        }
        private void compress(CompressionParameters parameters)
        {

            if (m_rows == null)
                throw new Exception("'m_rows' Cannot be null!");

            if (m_rows.Count == 0)
                throw new ArgumentException("'m_rows' Cannot be empty!");
            

            RawImage source = new RawImage(m_rows, m_colorspace);
            compress(source, parameters);
        }
        private void compress(IRawImage source, CompressionParameters parameters)
        {
            if (source == null)
                throw new ArgumentNullException("'source' Cannot be null!");

            if (!needCompressWith(parameters))
                return;

            m_compressedData = new MemoryStream();
            m_compressionParameters = new CompressionParameters(parameters);

            Jpeg jpeg = new Jpeg();
            jpeg.CompressionParameters = m_compressionParameters;
            jpeg.Compress(source, m_compressedData);
        }
        private bool needCompressWith(CompressionParameters parameters)
        {
            return m_compressedData == null ||
                   m_compressionParameters == null ||
                   !m_compressionParameters.Equals(parameters);
        }
        private void decompress()
        {
            Jpeg jpeg = new Jpeg();
            jpeg.Decompress(compressedData, new DecompressorToJpegImage(this));
        }
        private void fillDecompressedData()
        {
            if (m_decompressedData != null)
                throw new Exception("'m_decompressedData' Is not null!");

            m_decompressedData = new MemoryStream();
            BitmapDestination dest = new BitmapDestination(m_decompressedData);

            Jpeg jpeg = new Jpeg();
            jpeg.Decompress(compressedData, dest);
        }

        private void processPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            if (pixelFormat == System.Drawing.Imaging.PixelFormat.Format16bppGrayScale)
            {
                m_bitsPerComponent = 16;
                m_componentsPerSample = 1;
                m_colorspace = ColorSpace.Grayscale;
                return;
            }

            byte formatIndexByte = (byte)((int)pixelFormat & 0x000000FF);
            byte pixelSizeByte = (byte)((int)pixelFormat & 0x0000FF00);

            if (pixelSizeByte == 32 && formatIndexByte == 15)
            {
                m_bitsPerComponent = 8;
                m_componentsPerSample = 4;
                m_colorspace = ColorSpace.CMYK;
                return;
            }

            m_bitsPerComponent = 8;
            m_componentsPerSample = 3;
            m_colorspace = ColorSpace.RGB;

            if (pixelSizeByte == 16)
                m_bitsPerComponent = 6;
            else if (pixelSizeByte == 24 || pixelSizeByte == 32)
                m_bitsPerComponent = 8;
            else if (pixelSizeByte == 48 || pixelSizeByte == 64)
                m_bitsPerComponent = 16;
        }
        private void fillSamplesFromBitmap()
        {
            if (m_bitmap == null)
                throw new Exception("Field 'm_bitmap' Cannot be null!");

            for (int y = 0; y < Height; ++y)
            {
                short[] samples = new short[Width * 3];
                for (int x = 0; x < Width; ++x)
                {
                    Color color = m_bitmap.GetPixel(x, y);
                    samples[x * 3] = color.R;
                    samples[x * 3 + 1] = color.G;
                    samples[x * 3 + 2] = color.B;
                }
                m_rows.Add(new SampleRow(samples, m_bitsPerComponent, m_componentsPerSample));
            }
        }
    }
    #endregion

    #region BitmapDestination
    class BitmapDestination : IDecompressor
    {
        private Stream m_output;
        private byte[][] m_pixels;
        private int m_rowWidth;
        private int m_currentRow;
        private LoadedImageAttributes m_parameters;

        public BitmapDestination(Stream output)
        {
            m_output = output;
        }

        public override Stream Output
        {
            get
            {
                return m_output;
            }
        }

        public override void SetImageAttributes(LoadedImageAttributes parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            m_parameters = parameters;
        }
        public override void BeginWrite()
        {
            m_rowWidth = m_parameters.Width * m_parameters.Components;
            while (m_rowWidth % 4 != 0)
                m_rowWidth++;

            m_pixels = new byte[m_rowWidth][];
            for (int i = 0; i < m_rowWidth; i++)
                m_pixels[i] = new byte[m_parameters.Height];

            m_currentRow = 0;
        }
        public override void ProcessPixelsRow(byte[] row)
        {
            if (m_parameters.Colorspace == ColorSpace.Grayscale || m_parameters.QuantizeColors)
            {
                putGrayRow(row);
            }
            else
            {
                if (m_parameters.Colorspace == ColorSpace.CMYK)
                    putCmykRow(row);
                else
                    putRgbRow(row);
            }

            ++m_currentRow;
        }
        public override void EndWrite()
        {
            writeHeader();
            writePixels();
            m_output.Flush();
        }
        private void putGrayRow(byte[] row)
        {
            for (int i = 0; i < m_parameters.Width; ++i)
                m_pixels[i][m_currentRow] = row[i];
        }
        private void putRgbRow(byte[] row)
        {
            for (int i = 0; i < m_parameters.Width; ++i)
            {
                int firstComponent = i * 3;
                byte red = row[firstComponent];
                byte green = row[firstComponent + 1];
                byte blue = row[firstComponent + 2];
                m_pixels[firstComponent][m_currentRow] = blue;
                m_pixels[firstComponent + 1][m_currentRow] = green;
                m_pixels[firstComponent + 2][m_currentRow] = red;
            }
        }
        private void putCmykRow(byte[] row)
        {
            for (int i = 0; i < m_parameters.Width; ++i)
            {
                int firstComponent = i * 4;
                m_pixels[firstComponent][m_currentRow] = row[firstComponent + 2];
                m_pixels[firstComponent + 1][m_currentRow] = row[firstComponent + 1];
                m_pixels[firstComponent + 2][m_currentRow] = row[firstComponent + 0];
                m_pixels[firstComponent + 3][m_currentRow] = row[firstComponent + 3];
            }
        }
        private void writeHeader()
        {
            int bits_per_pixel;
            int cmap_entries;

            if (m_parameters.Colorspace == ColorSpace.Grayscale || m_parameters.QuantizeColors)
            {
                bits_per_pixel = 8;
                cmap_entries = 256;
            }
            else
            {
                cmap_entries = 0;

                if (m_parameters.Colorspace == ColorSpace.RGB)
                    bits_per_pixel = 24;
                else if (m_parameters.Colorspace == ColorSpace.CMYK)
                    bits_per_pixel = 32;
                else
                    throw new InvalidOperationException();
            }

            byte[] infoHeader = null;
            if (m_parameters.Colorspace == ColorSpace.RGB)
                infoHeader = createBitmapInfoHeader(bits_per_pixel, cmap_entries);
            else
                infoHeader = createBitmapV4InfoHeader(bits_per_pixel);

            const int fileHeaderSize = 14;
            int infoHeaderSize = infoHeader.Length;
            int paletteSize = cmap_entries * 4;
            int offsetToPixels = fileHeaderSize + infoHeaderSize + paletteSize;
            int fileSize = offsetToPixels + m_rowWidth * m_parameters.Height;

            byte[] fileHeader = createBitmapFileHeader(offsetToPixels, fileSize);

            m_output.Write(fileHeader, 0, fileHeader.Length);
            m_output.Write(infoHeader, 0, infoHeader.Length);

            if (cmap_entries > 0)
                writeColormap(cmap_entries, 4);
        }

        private static byte[] createBitmapFileHeader(int offsetToPixels, int fileSize)
        {
            byte[] bmpfileheader = new byte[14];
            bmpfileheader[0] = 0x42;
            bmpfileheader[1] = 0x4D;
            PUT_4B(bmpfileheader, 2, fileSize);
            PUT_4B(bmpfileheader, 10, offsetToPixels);
            return bmpfileheader;
        }

        private byte[] createBitmapInfoHeader(int bits_per_pixel, int cmap_entries)
        {
            byte[] bmpinfoheader = new byte[40];
            fillBitmapInfoHeader(bits_per_pixel, cmap_entries, bmpinfoheader);
            return bmpinfoheader;
        }

        private void fillBitmapInfoHeader(int bitsPerPixel, int cmap_entries, byte[] infoHeader)
        {
            PUT_2B(infoHeader, 0, infoHeader.Length);
            PUT_4B(infoHeader, 4, m_parameters.Width);
            PUT_4B(infoHeader, 8, m_parameters.Height);
            PUT_2B(infoHeader, 12, 1);
            PUT_2B(infoHeader, 14, bitsPerPixel);

            if (m_parameters.DensityUnit == DensityUnit.DotsCm)
            {
                PUT_4B(infoHeader, 24, m_parameters.DensityX * 100);
                PUT_4B(infoHeader, 28, m_parameters.DensityY * 100);
            }
            PUT_2B(infoHeader, 32, cmap_entries);
        }

        private byte[] createBitmapV4InfoHeader(int bitsPerPixel)
        {
            byte[] infoHeader = new byte[40 + 68];
            fillBitmapInfoHeader(bitsPerPixel, 0, infoHeader);

            PUT_4B(infoHeader, 56, 0x02);

            return infoHeader;
        }

        private void writeColormap(int map_colors, int map_entry_size)
        {
            byte[][] colormap = m_parameters.Colormap;
            int num_colors = m_parameters.ActualNumberOfColors;

            int i = 0;
            if (colormap != null)
            {
                if (m_parameters.ComponentsPerSample == 3)
                {
                    for (i = 0; i < num_colors; i++)
                    {
                        m_output.WriteByte(colormap[2][i]);
                        m_output.WriteByte(colormap[1][i]);
                        m_output.WriteByte(colormap[0][i]);
                        if (map_entry_size == 4)
                            m_output.WriteByte(0);
                    }
                }
                else
                {
                    for (i = 0; i < num_colors; i++)
                    {
                        m_output.WriteByte(colormap[0][i]);
                        m_output.WriteByte(colormap[0][i]);
                        m_output.WriteByte(colormap[0][i]);
                        if (map_entry_size == 4)
                            m_output.WriteByte(0);
                    }
                }
            }
            else
            {
                for (i = 0; i < 256; i++)
                {
                    m_output.WriteByte((byte)i);
                    m_output.WriteByte((byte)i);
                    m_output.WriteByte((byte)i);
                    if (map_entry_size == 4)
                        m_output.WriteByte(0);
                }
            }

            if (i > map_colors)
                throw new InvalidOperationException("Too many colors");

            for (; i < map_colors; i++)
            {
                m_output.WriteByte(0);
                m_output.WriteByte(0);
                m_output.WriteByte(0);
                if (map_entry_size == 4)
                    m_output.WriteByte(0);
            }
        }
        private void writePixels()
        {
            for (int row = m_parameters.Height - 1; row >= 0; --row)
                for (int col = 0; col < m_rowWidth; ++col)
                    m_output.WriteByte(m_pixels[col][row]);
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

    #region BitStream
    class BitStream : IDisposable
    {
        private bool m_alreadyDisposed;

        private const int bitsInByte = 8;
        private Stream m_stream;
        private int m_positionInByte;

        private int m_size;

        public BitStream()
        {
            m_stream = new MemoryStream();
        }

        public BitStream(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            m_stream = new MemoryStream(buffer);
            m_size = bitsAllocated();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_alreadyDisposed)
            {
                if (disposing)
                {
                    if (m_stream != null)
                        m_stream.Dispose();
                }

                m_stream = null;
                m_alreadyDisposed = true;
            }
        }

        public int Size()
        {
            return m_size;
        }

        public Stream UnderlyingStream
        {
            get
            {
                return m_stream;
            }
        }

        public virtual int Read(int bitCount)
        {
            if (Tell() + bitCount > bitsAllocated())
                throw new ArgumentOutOfRangeException("bitCount");

            return read(bitCount);
        }

        public int Write(int bitStorage, int bitCount)
        {
            if (bitCount == 0)
                return 0;

            const int maxBitsInStorage = sizeof(int) * bitsInByte;
            if (bitCount > maxBitsInStorage)
                throw new ArgumentOutOfRangeException("bitCount");

            for (int i = 0; i < bitCount; ++i)
            {
                byte bit = (byte)((bitStorage << (maxBitsInStorage - (bitCount - i))) >> (maxBitsInStorage - 1));
                if (!writeBit(bit))
                    return i;
            }

            return bitCount;
        }

        public void Seek(int pos, SeekOrigin mode)
        {
            switch (mode)
            {
                case SeekOrigin.Begin:
                    seekSet(pos);
                    break;

                case SeekOrigin.Current:
                    seekCurrent(pos);
                    break;

                case SeekOrigin.End:
                    seekSet(Size() + pos);
                    break;
            }
        }

        public int Tell()
        {
            return (int)m_stream.Position * bitsInByte + m_positionInByte;
        }

        private int bitsAllocated()
        {
            return (int)m_stream.Length * bitsInByte;
        }

        private int read(int bitsCount)
        {
            if (bitsCount < 0 || bitsCount > 32)
                throw new ArgumentOutOfRangeException("bitsCount");

            if (bitsCount == 0)
                return 0;

            int bitsRead = 0;
            int result = 0;
            byte[] bt = new byte[1];
            while (bitsRead == 0 || (bitsRead - m_positionInByte < bitsCount))
            {
                m_stream.Read(bt, 0, 1);

                result = (result << bitsInByte);
                result += bt[0];

                bitsRead += 8;
            }

            m_positionInByte = (m_positionInByte + bitsCount) % 8;
            if (m_positionInByte != 0)
            {
                result = (result >> (bitsInByte - m_positionInByte));

                m_stream.Seek(-1, SeekOrigin.Current);
            }

            if (bitsCount < 32)
            {
                int mask = ((1 << bitsCount) - 1);
                result = result & mask;
            }

            return result;
        }

        private bool writeBit(byte bit)
        {
            if (m_stream.Position == m_stream.Length)
            {
                byte[] bt = { (byte)(bit << (bitsInByte - 1)) };
                m_stream.Write(bt, 0, 1);
                m_stream.Seek(-1, SeekOrigin.Current);
            }
            else
            {
                byte[] bt = { 0 };
                m_stream.Read(bt, 0, 1);
                m_stream.Seek(-1, SeekOrigin.Current);

                int shift = (bitsInByte - m_positionInByte - 1) % bitsInByte;
                byte maskByte = (byte)(bit << shift);

                bt[0] |= maskByte;
                m_stream.Write(bt, 0, 1);
                m_stream.Seek(-1, SeekOrigin.Current);
            }

            Seek(1, SeekOrigin.Current);

            int currentPosition = Tell();
            if (currentPosition > m_size)
                m_size = currentPosition;

            return true;
        }

        private void seekSet(int pos)
        {
            if (pos < 0)
                throw new ArgumentOutOfRangeException("pos");

            int byteDisplacement = pos / bitsInByte;
            m_stream.Seek(byteDisplacement, SeekOrigin.Begin);

            int shiftInByte = pos - byteDisplacement * bitsInByte;
            m_positionInByte = shiftInByte;
        }

        private void seekCurrent(int pos)
        {
            int result = Tell() + pos;
            if (result < 0 || result > bitsAllocated())
                throw new ArgumentOutOfRangeException("pos");

            seekSet(result);
        }
    }
    #endregion

    #region CoefControllerImpl
    class CoefControllerImpl : JpegCompressorCoefController
    {
        private BufferMode m_passModeSetByLastStartPass;
        private JpegCompressor m_cinfo;
        private int m_iMCU_row_num;
        private int m_mcu_ctr;
        private int m_MCU_vert_offset;
        private int m_MCU_rows_per_iMCU_row;
        private JpegBlock[][] m_MCU_buffer = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU][];

        private JpegVirtualArray<JpegBlock>[] m_whole_image = new JpegVirtualArray<JpegBlock>[JpegConstants.MaxComponents];

        public CoefControllerImpl(JpegCompressor cinfo, bool need_full_buffer)
        {
            m_cinfo = cinfo;
            if (need_full_buffer)
            {
                for (int ci = 0; ci < cinfo.m_num_components; ci++)
                {
                    m_whole_image[ci] = JpegCommonBase.CreateBlocksArray(
                        JpegUtils.jround_up(cinfo.Component_info[ci].Width_in_blocks, cinfo.Component_info[ci].H_samp_factor),
                        JpegUtils.jround_up(cinfo.Component_info[ci].height_in_blocks, cinfo.Component_info[ci].V_samp_factor));
                    m_whole_image[ci].ErrorProcessor = cinfo;
                }
            }
            else
            {
                JpegBlock[] buffer = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU];
                for (int i = 0; i < JpegConstants.CompressorMaxBlocksInMCU; i++)
                    buffer[i] = new JpegBlock();

                for (int i = 0; i < JpegConstants.CompressorMaxBlocksInMCU; i++)
                {
                    m_MCU_buffer[i] = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU - i];
                    for (int j = i; j < JpegConstants.CompressorMaxBlocksInMCU; j++)
                        m_MCU_buffer[i][j - i] = buffer[j];
                }
                m_whole_image[0] = null;
            }
        }
        public virtual void start_pass(BufferMode pass_mode)
        {
            m_iMCU_row_num = 0;
            start_iMCU_row();

            switch (pass_mode)
            {
                case BufferMode.PassThru:
                    if (m_whole_image[0] != null)
                        throw new Exception("Bogus buffer control mode");
                    break;

                case BufferMode.SaveAndPass:
                    if (m_whole_image[0] == null)
                        throw new Exception("Bogus buffer control mode");
                    break;

                case BufferMode.CrankDest:
                    if (m_whole_image[0] == null)
                        throw new Exception("Bogus buffer control mode");
                    break;

                default:
                    throw new Exception("Bogus buffer control mode");
            }

            m_passModeSetByLastStartPass = pass_mode;
        }

        public virtual bool compress_data(byte[][][] input_buf)
        {
            switch (m_passModeSetByLastStartPass)
            {
                case BufferMode.PassThru:
                    return compressDataImpl(input_buf);

                case BufferMode.SaveAndPass:
                    return compressFirstPass(input_buf);

                case BufferMode.CrankDest:
                    return compressOutput();
            }

            return false;
        }
        private bool compressDataImpl(byte[][][] input_buf)
        {
            int last_MCU_col = m_cinfo.m_MCUs_per_row - 1;
            int last_iMCU_row = m_cinfo.m_total_iMCU_rows - 1;
            for (int yoffset = m_MCU_vert_offset; yoffset < m_MCU_rows_per_iMCU_row; yoffset++)
            {
                for (int MCU_col_num = m_mcu_ctr; MCU_col_num <= last_MCU_col; MCU_col_num++)
                {
                    int blkn = 0;
                    for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                    {
                        JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];
                        int blockcnt = (MCU_col_num < last_MCU_col) ? componentInfo.MCU_width : componentInfo.last_col_width;
                        int xpos = MCU_col_num * componentInfo.MCU_sample_width;
                        int ypos = yoffset * JpegConstants.DCTSize;

                        for (int yindex = 0; yindex < componentInfo.MCU_height; yindex++)
                        {
                            if (m_iMCU_row_num < last_iMCU_row || yoffset + yindex < componentInfo.last_row_height)
                            {
                                m_cinfo.m_fdct.forward_DCT(componentInfo.Quant_tbl_no, input_buf[componentInfo.Component_index],
                                    m_MCU_buffer[blkn], ypos, xpos, blockcnt);

                                if (blockcnt < componentInfo.MCU_width)
                                {
                                    for (int i = 0; i < (componentInfo.MCU_width - blockcnt); i++)
                                        Array.Clear(m_MCU_buffer[blkn + blockcnt][i].data, 0, m_MCU_buffer[blkn + blockcnt][i].data.Length);

                                    for (int bi = blockcnt; bi < componentInfo.MCU_width; bi++)
                                        m_MCU_buffer[blkn + bi][0][0] = m_MCU_buffer[blkn + bi - 1][0][0];
                                }
                            }
                            else
                            {
                                for (int i = 0; i < componentInfo.MCU_width; i++)
                                    Array.Clear(m_MCU_buffer[blkn][i].data, 0, m_MCU_buffer[blkn][i].data.Length);

                                for (int bi = 0; bi < componentInfo.MCU_width; bi++)
                                    m_MCU_buffer[blkn + bi][0][0] = m_MCU_buffer[blkn - 1][0][0];
                            }

                            blkn += componentInfo.MCU_width;
                            ypos += JpegConstants.DCTSize;
                        }
                    }
                    if (!m_cinfo.m_entropy.encode_mcu(m_MCU_buffer))
                    {
                        m_MCU_vert_offset = yoffset;
                        m_mcu_ctr = MCU_col_num;
                        return false;
                    }
                }
                m_mcu_ctr = 0;
            }
            m_iMCU_row_num++;
            start_iMCU_row();
            return true;
        }
        private bool compressFirstPass(byte[][][] input_buf)
        {
            int last_iMCU_row = m_cinfo.m_total_iMCU_rows - 1;

            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Component_info[ci];
                JpegBlock[][] buffer = m_whole_image[ci].Access(m_iMCU_row_num * componentInfo.V_samp_factor, componentInfo.V_samp_factor);
                int block_rows;
                if (m_iMCU_row_num < last_iMCU_row)
                {
                    block_rows = componentInfo.V_samp_factor;
                }
                else
                {
                    block_rows = componentInfo.height_in_blocks % componentInfo.V_samp_factor;
                    if (block_rows == 0)
                        block_rows = componentInfo.V_samp_factor;
                }

                int blocks_across = componentInfo.Width_in_blocks;
                int h_samp_factor = componentInfo.H_samp_factor;
                int ndummy = blocks_across % h_samp_factor;
                if (ndummy > 0)
                    ndummy = h_samp_factor - ndummy;
                for (int block_row = 0; block_row < block_rows; block_row++)
                {
                    m_cinfo.m_fdct.forward_DCT(componentInfo.Quant_tbl_no, input_buf[ci],
                        buffer[block_row], block_row * JpegConstants.DCTSize, 0, blocks_across);

                    if (ndummy > 0)
                    {
                        Array.Clear(buffer[block_row][blocks_across].data, 0, buffer[block_row][blocks_across].data.Length);

                        short lastDC = buffer[block_row][blocks_across - 1][0];
                        for (int bi = 0; bi < ndummy; bi++)
                            buffer[block_row][blocks_across + bi][0] = lastDC;
                    }
                }

                if (m_iMCU_row_num == last_iMCU_row)
                {
                    blocks_across += ndummy;
                    int MCUs_across = blocks_across / h_samp_factor;
                    for (int block_row = block_rows; block_row < componentInfo.V_samp_factor; block_row++)
                    {
                        for (int i = 0; i < blocks_across; i++)
                            Array.Clear(buffer[block_row][i].data, 0, buffer[block_row][i].data.Length);

                        int thisOffset = 0;
                        int lastOffset = 0;
                        for (int MCUindex = 0; MCUindex < MCUs_across; MCUindex++)
                        {
                            short lastDC = buffer[block_row - 1][lastOffset + h_samp_factor - 1][0];
                            for (int bi = 0; bi < h_samp_factor; bi++)
                                buffer[block_row][thisOffset + bi][0] = lastDC;

                            thisOffset += h_samp_factor;
                            lastOffset += h_samp_factor;
                        }
                    }
                }
            }
            return compressOutput();
        }
        private bool compressOutput()
        {
            JpegBlock[][][] buffer = new JpegBlock[JpegConstants.MaxComponentsInScan][][];
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];
                buffer[ci] = m_whole_image[componentInfo.Component_index].Access(
                    m_iMCU_row_num * componentInfo.V_samp_factor, componentInfo.V_samp_factor);
            }
            for (int yoffset = m_MCU_vert_offset; yoffset < m_MCU_rows_per_iMCU_row; yoffset++)
            {
                for (int MCU_col_num = m_mcu_ctr; MCU_col_num < m_cinfo.m_MCUs_per_row; MCU_col_num++)
                {
                    int blkn = 0;
                    for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                    {
                        JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];
                        int start_col = MCU_col_num * componentInfo.MCU_width;
                        for (int yindex = 0; yindex < componentInfo.MCU_height; yindex++)
                        {
                            for (int xindex = 0; xindex < componentInfo.MCU_width; xindex++)
                            {
                                int bufLength = buffer[ci][yindex + yoffset].Length;
                                int start = start_col + xindex;
                                m_MCU_buffer[blkn] = new JpegBlock[bufLength - start];
                                for (int j = start; j < bufLength; j++)
                                    m_MCU_buffer[blkn][j - start] = buffer[ci][yindex + yoffset][j];

                                blkn++;
                            }
                        }
                    }
                    if (!m_cinfo.m_entropy.encode_mcu(m_MCU_buffer))
                    {
                        m_MCU_vert_offset = yoffset;
                        m_mcu_ctr = MCU_col_num;
                        return false;
                    }
                }
                m_mcu_ctr = 0;
            }
            m_iMCU_row_num++;
            start_iMCU_row();
            return true;
        }
        private void start_iMCU_row()
        {
            if (m_cinfo.m_comps_in_scan > 1)
            {
                m_MCU_rows_per_iMCU_row = 1;
            }
            else
            {
                if (m_iMCU_row_num < (m_cinfo.m_total_iMCU_rows - 1))
                    m_MCU_rows_per_iMCU_row = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[0]].V_samp_factor;
                else
                    m_MCU_rows_per_iMCU_row = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[0]].last_row_height;
            }

            m_mcu_ctr = 0;
            m_MCU_vert_offset = 0;
        }
    }
    #endregion

    #region ColorConverter
    class ColorConverter
    {
        private const int SCALEBITS = 16;
        private const int CBCR_OFFSET = JpegConstants.MediumSampleValue << SCALEBITS;
        private const int ONE_HALF = 1 << (SCALEBITS - 1);
        private const int R_Y_OFF = 0;
        private const int G_Y_OFF = (1 * (JpegConstants.MaxSampleValue + 1));
        private const int B_Y_OFF = (2 * (JpegConstants.MaxSampleValue + 1));
        private const int R_CB_OFF = (3 * (JpegConstants.MaxSampleValue + 1));
        private const int G_CB_OFF = (4 * (JpegConstants.MaxSampleValue + 1));
        private const int B_CB_OFF = (5 * (JpegConstants.MaxSampleValue + 1));
        private const int R_CR_OFF = B_CB_OFF;
        private const int G_CR_OFF = (6 * (JpegConstants.MaxSampleValue + 1));
        private const int B_CR_OFF = (7 * (JpegConstants.MaxSampleValue + 1));
        private const int TABLE_SIZE = (8 * (JpegConstants.MaxSampleValue + 1));
        private JpegCompressor m_cinfo;
        private bool m_useNullStart;
        private bool m_useCmykYcckConvert;
        private bool m_useGrayscaleConvert;
        private bool m_useNullConvert;
        private bool m_useRgbGrayConvert;
        private bool m_useRgbYccConvert;
        private int[] m_rgb_ycc_tab;

        public ColorConverter(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;
            m_useNullStart = true;
            switch (cinfo.m_in_color_space)
            {
                case ColorSpace.Grayscale:
                    if (cinfo.m_input_components != 1)
                        throw new Exception("Bogus input colorspace!");
                    break;

                case ColorSpace.RGB:
                case ColorSpace.YCbCr:
                    if (cinfo.m_input_components != 3)
                        throw new Exception("Bogus input colorspace!");
                    break;

                case ColorSpace.CMYK:
                case ColorSpace.YCCK:
                    if (cinfo.m_input_components != 4)
                        throw new Exception("Bogus input colorspace!");
                    break;

                default:
                    if (cinfo.m_input_components < 1)
                        throw new Exception("Bogus input colorspace!");
                    break;
            }
            clearConvertFlags();
            switch (cinfo.m_jpeg_color_space)
            {
                case ColorSpace.Grayscale:
                    {
                        if (cinfo.m_num_components != 1)
                            throw new Exception("Bogus Jpeg colorspace!");


                        if (cinfo.m_in_color_space == ColorSpace.Grayscale)
                        {
                            m_useGrayscaleConvert = true;
                        }
                        else if (cinfo.m_in_color_space == ColorSpace.RGB)
                        {
                            m_useNullStart = false;
                            m_useRgbGrayConvert = true;
                        }
                        else if (cinfo.m_in_color_space == ColorSpace.YCbCr)
                        {
                            m_useGrayscaleConvert = true;
                        }
                        else
                        {
                            throw new Exception("Unsupported color conversion request.");
                        }

                        break;
                    }
                case ColorSpace.RGB:
                    {
                        if (cinfo.m_num_components != 3)
                            throw new Exception("Bogus Jpeg colorspace!");


                        if (cinfo.m_in_color_space == ColorSpace.RGB)
                        {
                            m_useNullConvert = true;
                        }
                        else
                        {
                            throw new Exception("Unsupported color conversion request.");
                        }

                        break;
                    }
                case ColorSpace.YCbCr:
                    {
                        if (cinfo.m_num_components != 3)
                            throw new Exception("Bogus Jpeg colorspace!");


                        if (cinfo.m_in_color_space == ColorSpace.RGB)
                        {
                            m_useNullStart = false;
                            m_useRgbYccConvert = true;
                        }
                        else if (cinfo.m_in_color_space == ColorSpace.YCbCr)
                        {
                            m_useNullConvert = true;
                        }
                        else
                        {
                            throw new Exception("Unsupported color conversion request.");
                        }

                        break;
                    }
                case ColorSpace.CMYK:
                    {
                        if (cinfo.m_num_components != 4)
                            throw new Exception("Bogus Jpeg colorspace!");


                        if (cinfo.m_in_color_space == ColorSpace.CMYK)
                        {
                            m_useNullConvert = true;
                        }
                        else
                        {
                            throw new Exception("Unsupported color conversion request.");
                        }

                        break;
                    }
                case ColorSpace.YCCK:
                    {
                        if (cinfo.m_num_components != 4)
                            throw new Exception("Bogus Jpeg colorspace!");


                        if (cinfo.m_in_color_space == ColorSpace.CMYK)
                        {
                            m_useNullStart = false;
                            m_useCmykYcckConvert = true;
                        }
                        else if (cinfo.m_in_color_space == ColorSpace.YCCK)
                        {
                            m_useNullConvert = true;
                        }
                        else
                        {
                            throw new Exception("Unsupported color conversion request.");
                        }

                        break;
                    }

                default:
                    if (cinfo.m_jpeg_color_space != cinfo.m_in_color_space || cinfo.m_num_components != cinfo.m_input_components)
                        throw new Exception("Unsupported color conversion request.");

                    m_useNullConvert = true;
                    break;
            }
        }

        public void start_pass()
        {
            if (!m_useNullStart)
                rgb_ycc_start();
        }
        public void color_convert(byte[][] input_buf, int input_row, byte[][][] output_buf, int output_row, int num_rows)
        {
            if (m_useCmykYcckConvert)
                cmyk_ycck_convert(input_buf, input_row, output_buf, output_row, num_rows);
            else if (m_useGrayscaleConvert)
                grayscale_convert(input_buf, input_row, output_buf, output_row, num_rows);
            else if (m_useRgbGrayConvert)
                rgb_gray_convert(input_buf, input_row, output_buf, output_row, num_rows);
            else if (m_useRgbYccConvert)
                rgb_ycc_convert(input_buf, input_row, output_buf, output_row, num_rows);
            else if (m_useNullConvert)
                null_convert(input_buf, input_row, output_buf, output_row, num_rows);
            else
                throw new Exception("Unsupported color conversion request.");
        }

        private void clearConvertFlags()
        {
            m_useCmykYcckConvert = false;
            m_useGrayscaleConvert = false;
            m_useNullConvert = false;
            m_useRgbGrayConvert = false;
            m_useRgbYccConvert = false;
        }

        private static int FIX(double x)
        {
            return (int)(x * (1L << SCALEBITS) + 0.5);
        }

        #region RGB to YCC
        private void rgb_ycc_start()
        {
            m_rgb_ycc_tab = new int[TABLE_SIZE];

            for (int i = 0; i <= JpegConstants.MaxSampleValue; i++)
            {
                m_rgb_ycc_tab[i + R_Y_OFF] = FIX(0.29900) * i;
                m_rgb_ycc_tab[i + G_Y_OFF] = FIX(0.58700) * i;
                m_rgb_ycc_tab[i + B_Y_OFF] = FIX(0.11400) * i + ONE_HALF;
                m_rgb_ycc_tab[i + R_CB_OFF] = (-FIX(0.16874)) * i;
                m_rgb_ycc_tab[i + G_CB_OFF] = (-FIX(0.33126)) * i;
                m_rgb_ycc_tab[i + B_CB_OFF] = FIX(0.50000) * i + CBCR_OFFSET + ONE_HALF - 1;
                m_rgb_ycc_tab[i + G_CR_OFF] = (-FIX(0.41869)) * i;
                m_rgb_ycc_tab[i + B_CR_OFF] = (-FIX(0.08131)) * i;
            }
        }
        private void rgb_ycc_convert(byte[][] input_buf, int input_row, byte[][][] output_buf, int output_row, int num_rows)
        {
            int num_cols = m_cinfo.m_image_width;
            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < num_cols; col++)
                {
                    int r = input_buf[input_row + row][columnOffset + JpegConstants.Offset_RGB_Red];
                    int g = input_buf[input_row + row][columnOffset + JpegConstants.Offset_RGB_Green];
                    int b = input_buf[input_row + row][columnOffset + JpegConstants.Offset_RGB_Blue];
                    columnOffset += JpegConstants.RGB_PixelLength;
                    output_buf[0][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_Y_OFF] + m_rgb_ycc_tab[g + G_Y_OFF] + m_rgb_ycc_tab[b + B_Y_OFF]) >> SCALEBITS);
                    output_buf[1][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_CB_OFF] + m_rgb_ycc_tab[g + G_CB_OFF] + m_rgb_ycc_tab[b + B_CB_OFF]) >> SCALEBITS);
                    output_buf[2][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_CR_OFF] + m_rgb_ycc_tab[g + G_CR_OFF] + m_rgb_ycc_tab[b + B_CR_OFF]) >> SCALEBITS);
                }
                output_row++;
            }
        }
        #endregion

        #region RGB to Grayscale
        private void rgb_gray_convert(byte[][] input_buf, int input_row, byte[][][] output_buf, int output_row, int num_rows)
        {
            int num_cols = m_cinfo.m_image_width;
            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < num_cols; col++)
                {
                    int r = input_buf[input_row + row][columnOffset + JpegConstants.Offset_RGB_Red];
                    int g = input_buf[input_row + row][columnOffset + JpegConstants.Offset_RGB_Green];
                    int b = input_buf[input_row + row][columnOffset + JpegConstants.Offset_RGB_Blue];
                    columnOffset += JpegConstants.RGB_PixelLength;
                    output_buf[0][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_Y_OFF] + m_rgb_ycc_tab[g + G_Y_OFF] + m_rgb_ycc_tab[b + B_Y_OFF]) >> SCALEBITS);
                }
                output_row++;
            }
        }
        #endregion

        #region CMYK to YCCK
        private void cmyk_ycck_convert(byte[][] input_buf, int input_row, byte[][][] output_buf, int output_row, int num_rows)
        {
            int num_cols = m_cinfo.m_image_width;
            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < num_cols; col++)
                {
                    int r = JpegConstants.MaxSampleValue - input_buf[input_row + row][columnOffset];
                    int g = JpegConstants.MaxSampleValue - input_buf[input_row + row][columnOffset + 1];
                    int b = JpegConstants.MaxSampleValue - input_buf[input_row + row][columnOffset + 2];
                    output_buf[3][output_row][col] = input_buf[input_row + row][columnOffset + 3];
                    columnOffset += 4;
                    output_buf[0][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_Y_OFF] + m_rgb_ycc_tab[g + G_Y_OFF] + m_rgb_ycc_tab[b + B_Y_OFF]) >> SCALEBITS);
                    output_buf[1][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_CB_OFF] + m_rgb_ycc_tab[g + G_CB_OFF] + m_rgb_ycc_tab[b + B_CB_OFF]) >> SCALEBITS);
                    output_buf[2][output_row][col] = (byte)((m_rgb_ycc_tab[r + R_CR_OFF] + m_rgb_ycc_tab[g + G_CR_OFF] + m_rgb_ycc_tab[b + B_CR_OFF]) >> SCALEBITS);
                }
                output_row++;
            }
        }
        #endregion

        #region Grayscale Conversion
        private void grayscale_convert(byte[][] input_buf, int input_row, byte[][][] output_buf, int output_row, int num_rows)
        {
            int num_cols = m_cinfo.m_image_width;
            int instride = m_cinfo.m_input_components;

            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < num_cols; col++)
                {
                    output_buf[0][output_row][col] = input_buf[input_row + row][columnOffset];
                    columnOffset += instride;
                }
                output_row++;
            }
        }
        #endregion

        #region Null Conversion
        private void null_convert(byte[][] input_buf, int input_row, byte[][][] output_buf, int output_row, int num_rows)
        {
            int nc = m_cinfo.m_num_components;
            int num_cols = m_cinfo.m_image_width;

            for (int row = 0; row < num_rows; row++)
            {
                for (int ci = 0; ci < nc; ci++)
                {
                    int columnOffset = 0;
                    for (int col = 0; col < num_cols; col++)
                    {
                        output_buf[ci][output_row][col] = input_buf[input_row + row][columnOffset + ci];
                        columnOffset += nc;
                    }
                }

                output_row++;
            }
        }
        #endregion
    }
    #endregion

    #region ColorDeconverter
    class ColorDeconverter
    {
        private const int SCALEBITS = 16;
        private const int ONE_HALF = 1 << (SCALEBITS - 1);

        private enum ColorConverter
        {
            grayscale_converter,
            ycc_rgb_converter,
            gray_rgb_converter,
            null_converter,
            ycck_cmyk_converter
        }

        private ColorConverter m_converter;
        private JpegDecompressor m_cinfo;

        private int[] m_perComponentOffsets;
        private int[] m_Cr_r_tab;
        private int[] m_Cb_b_tab;
        private int[] m_Cr_g_tab;
        private int[] m_Cb_g_tab;
        public ColorDeconverter(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;
            switch (cinfo.m_jpeg_color_space)
            {
                case ColorSpace.Grayscale:
                    if (cinfo.m_num_components != 1)
                        throw new Exception("Bogus Jpeg colorspace!");
                    break;

                case ColorSpace.RGB:
                case ColorSpace.YCbCr:
                    if (cinfo.m_num_components != 3)
                        throw new Exception("Bogus Jpeg colorspace!");
                    break;

                case ColorSpace.CMYK:
                case ColorSpace.YCCK:
                    if (cinfo.m_num_components != 4)
                        throw new Exception("Bogus Jpeg colorspace!");
                    break;

                default:
                    if (cinfo.m_num_components < 1)
                        throw new Exception("Bogus Jpeg colorspace!");
                    break;
            }

            switch (cinfo.m_out_color_space)
            {
                case ColorSpace.Grayscale:
                    cinfo.m_out_color_components = 1;
                    if (cinfo.m_jpeg_color_space == ColorSpace.Grayscale || cinfo.m_jpeg_color_space == ColorSpace.YCbCr)
                    {
                        m_converter = ColorConverter.grayscale_converter;
                        for (int ci = 1; ci < cinfo.m_num_components; ci++)
                            cinfo.Comp_info[ci].component_needed = false;
                    }
                    else
                        throw new Exception("Unsupported color conversion request.");
                    break;

                case ColorSpace.RGB:
                    cinfo.m_out_color_components = JpegConstants.RGB_PixelLength;
                    if (cinfo.m_jpeg_color_space == ColorSpace.YCbCr)
                    {
                        m_converter = ColorConverter.ycc_rgb_converter;
                        build_ycc_rgb_table();
                    }
                    else if (cinfo.m_jpeg_color_space == ColorSpace.Grayscale)
                        m_converter = ColorConverter.gray_rgb_converter;
                    else if (cinfo.m_jpeg_color_space == ColorSpace.RGB)
                        m_converter = ColorConverter.null_converter;
                    else
                        throw new Exception("Unsupported color conversion request.");
                    break;

                case ColorSpace.CMYK:
                    cinfo.m_out_color_components = 4;
                    if (cinfo.m_jpeg_color_space == ColorSpace.YCCK)
                    {
                        m_converter = ColorConverter.ycck_cmyk_converter;
                        build_ycc_rgb_table();
                    }
                    else if (cinfo.m_jpeg_color_space == ColorSpace.CMYK)
                        m_converter = ColorConverter.null_converter;
                    else
                        throw new Exception("Unsupported color conversion request.");
                    break;

                default:
                    if (cinfo.m_out_color_space == cinfo.m_jpeg_color_space)
                    {
                        cinfo.m_out_color_components = cinfo.m_num_components;
                        m_converter = ColorConverter.null_converter;
                    }
                    else
                    {
                        throw new Exception("Unsupported color conversion request.");
                    }
                    break;
            }

            if (cinfo.m_quantize_colors)
                cinfo.m_output_components = 1;
            else
                cinfo.m_output_components = cinfo.m_out_color_components;
        }
        public void color_convert(ComponentBuffer[] input_buf, int[] perComponentOffsets, int input_row, byte[][] output_buf, int output_row, int num_rows)
        {
            m_perComponentOffsets = perComponentOffsets;

            switch (m_converter)
            {
                case ColorConverter.grayscale_converter:
                    grayscale_convert(input_buf, input_row, output_buf, output_row, num_rows);
                    break;
                case ColorConverter.ycc_rgb_converter:
                    ycc_rgb_convert(input_buf, input_row, output_buf, output_row, num_rows);
                    break;
                case ColorConverter.gray_rgb_converter:
                    gray_rgb_convert(input_buf, input_row, output_buf, output_row, num_rows);
                    break;
                case ColorConverter.null_converter:
                    null_convert(input_buf, input_row, output_buf, output_row, num_rows);
                    break;
                case ColorConverter.ycck_cmyk_converter:
                    ycck_cmyk_convert(input_buf, input_row, output_buf, output_row, num_rows);
                    break;
                default:
                    throw new Exception("Unsupported color conversion request.");
            }
        }

        private static int FIX(double x)
        {
            return (int)(x * (1L << SCALEBITS) + 0.5);
        }


        #region YCbCr to RGB
        private void build_ycc_rgb_table()
        {
            m_Cr_r_tab = new int[JpegConstants.MaxSampleValue + 1];
            m_Cb_b_tab = new int[JpegConstants.MaxSampleValue + 1];
            m_Cr_g_tab = new int[JpegConstants.MaxSampleValue + 1];
            m_Cb_g_tab = new int[JpegConstants.MaxSampleValue + 1];

            for (int i = 0, x = -JpegConstants.MediumSampleValue; i <= JpegConstants.MaxSampleValue; i++, x++)
            {
                m_Cr_r_tab[i] = JpegUtils.RIGHT_SHIFT(FIX(1.40200) * x + ONE_HALF, SCALEBITS);
                m_Cb_b_tab[i] = JpegUtils.RIGHT_SHIFT(FIX(1.77200) * x + ONE_HALF, SCALEBITS);
                m_Cr_g_tab[i] = (-FIX(0.71414)) * x;
                m_Cb_g_tab[i] = (-FIX(0.34414)) * x + ONE_HALF;
            }
        }

        private void ycc_rgb_convert(ComponentBuffer[] input_buf, int input_row, byte[][] output_buf, int output_row, int num_rows)
        {
            int component0RowOffset = m_perComponentOffsets[0];
            int component1RowOffset = m_perComponentOffsets[1];
            int component2RowOffset = m_perComponentOffsets[2];
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset;
            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < m_cinfo.m_output_width; col++)
                {
                    int y = input_buf[0][input_row + component0RowOffset][col];
                    int cb = input_buf[1][input_row + component1RowOffset][col];
                    int cr = input_buf[2][input_row + component2RowOffset][col];
                    output_buf[output_row + row][columnOffset + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + m_Cr_r_tab[cr]];
                    output_buf[output_row + row][columnOffset + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + JpegUtils.RIGHT_SHIFT(m_Cb_g_tab[cb] + m_Cr_g_tab[cr], SCALEBITS)];
                    output_buf[output_row + row][columnOffset + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + m_Cb_b_tab[cb]];
                    columnOffset += JpegConstants.RGB_PixelLength;
                }
                input_row++;
            }
        }
        #endregion

        #region YCCK to CMYK
        private void ycck_cmyk_convert(ComponentBuffer[] input_buf, int input_row, byte[][] output_buf, int output_row, int num_rows)
        {
            int component0RowOffset = m_perComponentOffsets[0];
            int component1RowOffset = m_perComponentOffsets[1];
            int component2RowOffset = m_perComponentOffsets[2];
            int component3RowOffset = m_perComponentOffsets[3];
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset;
            int num_cols = m_cinfo.m_output_width;
            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < num_cols; col++)
                {
                    int y = input_buf[0][input_row + component0RowOffset][col];
                    int cb = input_buf[1][input_row + component1RowOffset][col];
                    int cr = input_buf[2][input_row + component2RowOffset][col];
                    output_buf[output_row + row][columnOffset] = limit[limitOffset + JpegConstants.MaxSampleValue - (y + m_Cr_r_tab[cr])];
                    output_buf[output_row + row][columnOffset + 1] = limit[limitOffset + JpegConstants.MaxSampleValue - (y + JpegUtils.RIGHT_SHIFT(m_Cb_g_tab[cb] + m_Cr_g_tab[cr], SCALEBITS))];
                    output_buf[output_row + row][columnOffset + 2] = limit[limitOffset + JpegConstants.MaxSampleValue - (y + m_Cb_b_tab[cb])];
                    output_buf[output_row + row][columnOffset + 3] = input_buf[3][input_row + component3RowOffset][col];
                    columnOffset += 4;
                }
                input_row++;
            }
        }
        #endregion

        #region Grayscale to RGB
        private void gray_rgb_convert(ComponentBuffer[] input_buf, int input_row, byte[][] output_buf, int output_row, int num_rows)
        {
            int component0RowOffset = m_perComponentOffsets[0];
            int component1RowOffset = m_perComponentOffsets[1];
            int component2RowOffset = m_perComponentOffsets[2];
            int num_cols = m_cinfo.m_output_width;
            for (int row = 0; row < num_rows; row++)
            {
                int columnOffset = 0;
                for (int col = 0; col < num_cols; col++)
                {
                    output_buf[output_row + row][columnOffset + JpegConstants.Offset_RGB_Red] = input_buf[0][input_row + component0RowOffset][col];
                    output_buf[output_row + row][columnOffset + JpegConstants.Offset_RGB_Green] = input_buf[0][input_row + component1RowOffset][col];
                    output_buf[output_row + row][columnOffset + JpegConstants.Offset_RGB_Blue] = input_buf[0][input_row + component2RowOffset][col];
                    columnOffset += JpegConstants.RGB_PixelLength;
                }
                input_row++;
            }
        }
        #endregion

        #region Grayscale Conversion
        private void grayscale_convert(ComponentBuffer[] input_buf, int input_row, byte[][] output_buf, int output_row, int num_rows)
        {
            JpegUtils.jcopy_sample_rows(input_buf[0], input_row + m_perComponentOffsets[0], output_buf, output_row, num_rows, m_cinfo.m_output_width);
        }
        #endregion

        #region Null Conversion
        private void null_convert(ComponentBuffer[] input_buf, int input_row, byte[][] output_buf, int output_row, int num_rows)
        {
            for (int row = 0; row < num_rows; row++)
            {
                for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                {
                    int columnIndex = 0;
                    int componentOffset = 0;
                    int perComponentOffset = m_perComponentOffsets[ci];

                    for (int count = m_cinfo.m_output_width; count > 0; count--)
                    {
                        output_buf[output_row + row][ci + componentOffset] = input_buf[ci][input_row + perComponentOffset][columnIndex];
                        componentOffset += m_cinfo.m_num_components;
                        columnIndex++;
                    }
                }
                input_row++;
            }
        }
        #endregion
    }
    #endregion

    #region ColorQuantizer
    interface ColorQuantizer
    {
        void start_pass(bool is_pre_scan);

        void color_quantize(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows);

        void finish_pass();
        void new_color_map();
    }
    #endregion

    #region ComponentBuffer
    class ComponentBuffer
    {
        private byte[][] m_buffer;
        private int[] m_funnyIndices;
        private int m_funnyOffset;

        public ComponentBuffer()
        {
        }

        public ComponentBuffer(byte[][] buf, int[] funnyIndices, int funnyOffset)
        {
            SetBuffer(buf, funnyIndices, funnyOffset);
        }

        public void SetBuffer(byte[][] buf, int[] funnyIndices, int funnyOffset)
        {
            m_buffer = buf;
            m_funnyIndices = funnyIndices;
            m_funnyOffset = funnyOffset;
        }

        public byte[] this[int i]
        {
            get
            {
                if (m_funnyIndices == null)
                    return m_buffer[i];

                return m_buffer[m_funnyIndices[i + m_funnyOffset]];
            }
        }
    }
    #endregion

    #region CompressionParameters
    public class CompressionParameters
    {
        private int m_quality = 75;
        private int m_smoothingFactor;
        private bool m_simpleProgressive;
        public CompressionParameters()
        {
        }

        internal CompressionParameters(CompressionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            m_quality = parameters.m_quality;
            m_smoothingFactor = parameters.m_smoothingFactor;
            m_simpleProgressive = parameters.m_simpleProgressive;
        }
        public override bool Equals(object obj)
        {
            CompressionParameters parameters = obj as CompressionParameters;
            if (parameters == null)
                return false;

            return (m_quality == parameters.m_quality &&
                    m_smoothingFactor == parameters.m_smoothingFactor &&
                    m_simpleProgressive == parameters.m_simpleProgressive);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public int Quality
        {
            get { return m_quality; }
            set { m_quality = value; }
        }
        public int SmoothingFactor
        {
            get { return m_smoothingFactor; }
            set { m_smoothingFactor = value; }
        }
        public bool SimpleProgressive
        {
            get { return m_simpleProgressive; }
            set { m_simpleProgressive = value; }
        }
    }
    #endregion

    #region DecompressionParameters
    class DecompressionParameters
    {
        private ColorSpace m_outColorspace = ColorSpace.Unknown;
        private int m_scaleNumerator = 1;
        private int m_scaleDenominator = 1;
        private bool m_bufferedImage;
        private bool m_rawDataOut;
        private DCTMethod m_dctMethod = (DCTMethod)JpegConstants.DefaultDCTMethod;
        private DitherMode m_ditherMode = DitherMode.FloydStein;
        private bool m_doFancyUpsampling = true;
        private bool m_doBlockSmoothing = true;
        private bool m_quantizeColors;
        private bool m_twoPassQuantize = true;
        private int m_desiredNumberOfColors = 256;
        private bool m_enableOnePassQuantizer;
        private bool m_enableExternalQuant;
        private bool m_enableTwoPassQuantizer;
        private int m_traceLevel;

        public int TraceLevel
        {
            get
            {
                return m_traceLevel;
            }
            set
            {
                m_traceLevel = value;
            }
        }
        public ColorSpace OutColorspace
        {
            get
            {
                return m_outColorspace;
            }
            set
            {
                m_outColorspace = value;
            }
        }
        public int ScaleNumerator
        {
            get
            {
                return m_scaleNumerator;
            }
            set
            {
                m_scaleNumerator = value;
            }
        }

        public int ScaleDenominator
        {
            get
            {
                return m_scaleDenominator;
            }
            set
            {
                m_scaleDenominator = value;
            }
        }
        public bool BufferedImage
        {
            get
            {
                return m_bufferedImage;
            }
            set
            {
                m_bufferedImage = value;
            }
        }
        public bool RawDataOut
        {
            get
            {
                return m_rawDataOut;
            }
            set
            {
                m_rawDataOut = value;
            }
        }
        public DCTMethod DCTMethod
        {
            get
            {
                return m_dctMethod;
            }
            set
            {
                m_dctMethod = value;
            }
        }
        public bool DoFancyUpsampling
        {
            get
            {
                return m_doFancyUpsampling;
            }
            set
            {
                m_doFancyUpsampling = value;
            }
        }
        public bool DoBlockSmoothing
        {
            get
            {
                return m_doBlockSmoothing;
            }
            set
            {
                m_doBlockSmoothing = value;
            }
        }
        public bool QuantizeColors
        {
            get
            {
                return m_quantizeColors;
            }
            set
            {
                m_quantizeColors = value;
            }
        }
        public DitherMode DitherMode
        {
            get
            {
                return m_ditherMode;
            }
            set
            {
                m_ditherMode = value;
            }
        }
        public bool TwoPassQuantize
        {
            get
            {
                return m_twoPassQuantize;
            }
            set
            {
                m_twoPassQuantize = value;
            }
        }
        public int DesiredNumberOfColors
        {
            get
            {
                return m_desiredNumberOfColors;
            }
            set
            {
                m_desiredNumberOfColors = value;
            }
        }
        public bool EnableOnePassQuantizer
        {
            get
            {
                return m_enableOnePassQuantizer;
            }
            set
            {
                m_enableOnePassQuantizer = value;
            }
        }
        public bool EnableExternalQuant
        {
            get
            {
                return m_enableExternalQuant;
            }
            set
            {
                m_enableExternalQuant = value;
            }
        }
        public bool EnableTwoPassQuantizer
        {
            get
            {
                return m_enableTwoPassQuantizer;
            }
            set
            {
                m_enableTwoPassQuantizer = value;
            }
        }
    }
    #endregion

    #region DecompressorToJpegImage
    class DecompressorToJpegImage : IDecompressor
    {
        private JpegImage m_jpegImage;

        internal DecompressorToJpegImage(JpegImage jpegImage)
        {
            m_jpegImage = jpegImage;
        }

        public override Stream Output
        {
            get
            {
                return null;
            }
        }

        public override void SetImageAttributes(LoadedImageAttributes parameters)
        {
            m_jpegImage.Width = parameters.Width;
            m_jpegImage.Height = parameters.Height;
            m_jpegImage.BitsPerComponent = 8;
            m_jpegImage.ComponentsPerSample = (byte)parameters.ComponentsPerSample;
            m_jpegImage.Colorspace = parameters.Colorspace;
        }

        public override void BeginWrite()
        {
        }

        public override void ProcessPixelsRow(byte[] row)
        {
            SampleRow samplesRow = new SampleRow(row, m_jpegImage.Width, m_jpegImage.BitsPerComponent, m_jpegImage.ComponentsPerSample);
            m_jpegImage.addSampleRow(samplesRow);
        }

        public override void EndWrite()
        {
        }
    }
    #endregion

    #region DerivedTable
    class DerivedTable
    {
        public int[] maxcode = new int[18];
        public int[] valoffset = new int[17];
        public JpegHuffmanTable pub;
        public int[] look_nbits = new int[1 << JpegConstants.HuffmanLookaheadDistance];
        public byte[] look_sym = new byte[1 << JpegConstants.HuffmanLookaheadDistance];
    }
    #endregion

    #region DestinationManager
    public abstract class DestinationManager
    {
        private byte[] m_buffer;
        private int m_position;
        private int m_free_in_buffer;
        public abstract void init_destination();
        public abstract bool empty_output_buffer();
        public abstract void term_destination();
        public virtual bool emit_byte(int val)
        {
            m_buffer[m_position] = (byte)val;
            m_position++;

            if (--m_free_in_buffer == 0)
            {
                if (!empty_output_buffer())
                    return false;
            }

            return true;
        }
        protected void initInternalBuffer(byte[] buffer, int offset)
        {
            m_buffer = buffer;
            m_free_in_buffer = buffer.Length - offset;
            m_position = offset;
        }
        protected int freeInBuffer
        {
            get
            {
                return m_free_in_buffer;
            }
        }
    }
    #endregion

    #region DestinationManagerImpl
    class DestinationManagerImpl : DestinationManager
    {
        private const int OUTPUT_BUF_SIZE = 4096;
        private JpegCompressor m_cinfo;
        private Stream m_outfile;
        private byte[] m_buffer;

        public DestinationManagerImpl(JpegCompressor cinfo, Stream alreadyOpenFile)
        {
            m_cinfo = cinfo;
            m_outfile = alreadyOpenFile;
        }
        public override void init_destination()
        {
            m_buffer = new byte[OUTPUT_BUF_SIZE];
            initInternalBuffer(m_buffer, 0);
        }
        public override bool empty_output_buffer()
        {
            writeBuffer(m_buffer.Length);
            initInternalBuffer(m_buffer, 0);
            return true;
        }
        public override void term_destination()
        {
            int datacount = m_buffer.Length - freeInBuffer;
            if (datacount > 0)
                writeBuffer(datacount);

            m_outfile.Flush();
        }

        private void writeBuffer(int dataCount)
        {
            try
            {
                m_outfile.Write(m_buffer, 0, dataCount);
            }
#pragma warning disable 168
            catch (IOException e)
            {
                throw new Exception("Output file write error --- out of disk space?");
            }
            catch (NotSupportedException e)
            {
                throw new Exception("Output file write error --- out of disk space?");
            }
            catch (ObjectDisposedException e)
            {
                throw new Exception("Output file write error --- out of disk space?");
            }
#pragma warning restore 168
        }
    }
    #endregion

    #region Enumerations
    enum BufferMode
    {
        PassThru,
        SaveSource,
        CrankDest,
        SaveAndPass
    }
    public enum DensityUnit
    {
        Unknown = 0,
        DotsInch = 1,
        DotsCm = 2
    }
    public enum DitherMode
    {
        None,
        Ordered,
        FloydStein
    }
    public enum ReadResult
    {
        Suspended = 0,
        Header_Ok = 1,
        Header_Tables_Only = 2,
        Reached_SOS = 3,
        Reached_EOI = 4,
        Row_Completed = 5,
        Scan_Completed = 6
    }
    public enum ColorSpace
    {
        Unknown,
        Grayscale,
        RGB,
        YCbCr,
        CMYK,
        YCCK
    }
    public enum DCTMethod
    {
        IntSlow,
        IntFast,
        Float
    }
    #endregion

    #region HuffEntropyDecoder
    class HuffEntropyDecoder : JpegEntropyDecoder
    {
        private class savable_state
        {
            public int[] last_dc_val = new int[JpegConstants.MaxComponentsInScan]; /* last DC coef for each component */

            public void Assign(savable_state ss)
            {
                Buffer.BlockCopy(ss.last_dc_val, 0, last_dc_val, 0, last_dc_val.Length * sizeof(int));
            }
        }
        private SavedBitreadState m_bitstate;
        private savable_state m_saved = new savable_state();
        private int m_restarts_to_go;
        private DerivedTable[] m_dc_derived_tbls = new DerivedTable[JpegConstants.NumberOfHuffmanTables];
        private DerivedTable[] m_ac_derived_tbls = new DerivedTable[JpegConstants.NumberOfHuffmanTables];
        private DerivedTable[] m_dc_cur_tbls = new DerivedTable[JpegConstants.DecompressorMaxBlocksInMCU];
        private DerivedTable[] m_ac_cur_tbls = new DerivedTable[JpegConstants.DecompressorMaxBlocksInMCU];
        private bool[] m_dc_needed = new bool[JpegConstants.DecompressorMaxBlocksInMCU];
        private bool[] m_ac_needed = new bool[JpegConstants.DecompressorMaxBlocksInMCU];

        public HuffEntropyDecoder(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;
            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
                m_dc_derived_tbls[i] = m_ac_derived_tbls[i] = null;
        }
        public override void start_pass()
        {
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];
                int dctbl = componentInfo.Dc_tbl_no;
                int actbl = componentInfo.Ac_tbl_no;
                jpeg_make_d_derived_tbl(true, dctbl, ref m_dc_derived_tbls[dctbl]);
                jpeg_make_d_derived_tbl(false, actbl, ref m_ac_derived_tbls[actbl]);
                m_saved.last_dc_val[ci] = 0;
            }
            for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
            {
                int ci = m_cinfo.m_MCU_membership[blkn];
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];
                m_dc_cur_tbls[blkn] = m_dc_derived_tbls[componentInfo.Dc_tbl_no];
                m_ac_cur_tbls[blkn] = m_ac_derived_tbls[componentInfo.Ac_tbl_no];
                if (componentInfo.component_needed)
                {
                    m_dc_needed[blkn] = true;
                    m_ac_needed[blkn] = (componentInfo.DCT_scaled_size > 1);
                }
                else
                {
                    m_dc_needed[blkn] = m_ac_needed[blkn] = false;
                }
            }
            m_bitstate.bits_left = 0;
            m_bitstate.get_buffer = 0;
            m_insufficient_data = false;
            m_restarts_to_go = m_cinfo.m_restart_interval;
        }
        public override bool decode_mcu(JpegBlock[] MCU_data)
        {
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    if (!process_restart())
                        return false;
                }
            }
            if (!m_insufficient_data)
            {
                int get_buffer;
                int bits_left;
                WorkingBitreadState br_state = new WorkingBitreadState();
                BITREAD_LOAD_STATE(m_bitstate, out get_buffer, out bits_left, ref br_state);
                savable_state state = new savable_state();
                state.Assign(m_saved);
                for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
                {
                    int s;
                    if (!HUFF_DECODE(out s, ref br_state, m_dc_cur_tbls[blkn], ref get_buffer, ref bits_left))
                        return false;

                    if (s != 0)
                    {
                        if (!CHECK_BIT_BUFFER(ref br_state, s, ref get_buffer, ref bits_left))
                            return false;

                        int r = GET_BITS(s, get_buffer, ref bits_left);
                        s = HUFF_EXTEND(r, s);
                    }

                    if (m_dc_needed[blkn])
                    {
                        int ci = m_cinfo.m_MCU_membership[blkn];
                        s += state.last_dc_val[ci];
                        state.last_dc_val[ci] = s;
                        MCU_data[blkn][0] = (short)s;
                    }

                    if (m_ac_needed[blkn])
                    {
                        for (int k = 1; k < JpegConstants.DCTSize2; k++)
                        {
                            if (!HUFF_DECODE(out s, ref br_state, m_ac_cur_tbls[blkn], ref get_buffer, ref bits_left))
                                return false;

                            int r = s >> 4;
                            s &= 15;

                            if (s != 0)
                            {
                                k += r;
                                if (!CHECK_BIT_BUFFER(ref br_state, s, ref get_buffer, ref bits_left))
                                    return false;
                                r = GET_BITS(s, get_buffer, ref bits_left);
                                s = HUFF_EXTEND(r, s);
                                MCU_data[blkn][JpegUtils.jpeg_natural_order[k]] = (short)s;
                            }
                            else
                            {
                                if (r != 15)
                                    break;

                                k += 15;
                            }
                        }
                    }
                    else
                    {
                        for (int k = 1; k < JpegConstants.DCTSize2; k++)
                        {
                            if (!HUFF_DECODE(out s, ref br_state, m_ac_cur_tbls[blkn], ref get_buffer, ref bits_left))
                                return false;

                            int r = s >> 4;
                            s &= 15;

                            if (s != 0)
                            {
                                k += r;
                                if (!CHECK_BIT_BUFFER(ref br_state, s, ref get_buffer, ref bits_left))
                                    return false;

                                DROP_BITS(s, ref bits_left);
                            }
                            else
                            {
                                if (r != 15)
                                    break;

                                k += 15;
                            }
                        }
                    }
                }
                BITREAD_SAVE_STATE(ref m_bitstate, get_buffer, bits_left);
                m_saved.Assign(state);
            }
            m_restarts_to_go--;

            return true;

        }
        private bool process_restart()
        {
            m_cinfo.m_marker.SkipBytes(m_bitstate.bits_left / 8);
            m_bitstate.bits_left = 0;
            if (!m_cinfo.m_marker.read_restart_marker())
                return false;
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                m_saved.last_dc_val[ci] = 0;
            m_restarts_to_go = m_cinfo.m_restart_interval;
            if (m_cinfo.m_unread_marker == 0)
                m_insufficient_data = false;

            return true;
        }
    }
    #endregion

    #region HuffEntropyEncoder
    class HuffEntropyEncoder : JpegEntropyEncoder
    {
        private class savable_state
        {
            public int put_buffer;
            public int put_bits;
            public int[] last_dc_val = new int[JpegConstants.MaxComponentsInScan];
        }
        private bool m_gather_statistics;
        private savable_state m_saved = new savable_state();
        private int m_restarts_to_go;
        private int m_next_restart_num;
        private c_derived_tbl[] m_dc_derived_tbls = new c_derived_tbl[JpegConstants.NumberOfHuffmanTables];
        private c_derived_tbl[] m_ac_derived_tbls = new c_derived_tbl[JpegConstants.NumberOfHuffmanTables];
        private long[][] m_dc_count_ptrs = new long[JpegConstants.NumberOfHuffmanTables][];
        private long[][] m_ac_count_ptrs = new long[JpegConstants.NumberOfHuffmanTables][];

        public HuffEntropyEncoder(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;
            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
            {
                m_dc_derived_tbls[i] = m_ac_derived_tbls[i] = null;
                m_dc_count_ptrs[i] = m_ac_count_ptrs[i] = null;
            }
        }
        public override void start_pass(bool gather_statistics)
        {
            m_gather_statistics = gather_statistics;

            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                int dctbl = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Dc_tbl_no;
                int actbl = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Ac_tbl_no;
                if (m_gather_statistics)
                {
                    if (dctbl < 0 || dctbl >= JpegConstants.NumberOfHuffmanTables)
                        throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", dctbl));

                    if (actbl < 0 || actbl >= JpegConstants.NumberOfHuffmanTables)
                        throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", actbl));
                    if (m_dc_count_ptrs[dctbl] == null)
                        m_dc_count_ptrs[dctbl] = new long[257];

                    Array.Clear(m_dc_count_ptrs[dctbl], 0, m_dc_count_ptrs[dctbl].Length);

                    if (m_ac_count_ptrs[actbl] == null)
                        m_ac_count_ptrs[actbl] = new long[257];

                    Array.Clear(m_ac_count_ptrs[actbl], 0, m_ac_count_ptrs[actbl].Length);
                }
                else
                {
                    jpeg_make_c_derived_tbl(true, dctbl, ref m_dc_derived_tbls[dctbl]);
                    jpeg_make_c_derived_tbl(false, actbl, ref m_ac_derived_tbls[actbl]);
                }
                m_saved.last_dc_val[ci] = 0;
            }
            m_saved.put_buffer = 0;
            m_saved.put_bits = 0;
            m_restarts_to_go = m_cinfo.m_restart_interval;
            m_next_restart_num = 0;
        }

        public override bool encode_mcu(JpegBlock[][] MCU_data)
        {
            if (m_gather_statistics)
                return encode_mcu_gather(MCU_data);

            return encode_mcu_huff(MCU_data);
        }

        public override void finish_pass()
        {
            if (m_gather_statistics)
                finish_pass_gather();
            else
                finish_pass_huff();
        }
        private bool encode_mcu_huff(JpegBlock[][] MCU_data)
        {
            savable_state state;
            state = m_saved;
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    if (!emit_restart(state, m_next_restart_num))
                        return false;
                }
            }
            for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
            {
                int ci = m_cinfo.m_MCU_membership[blkn];
                if (!encode_one_block(state, MCU_data[blkn][0].data, state.last_dc_val[ci],
                    m_dc_derived_tbls[m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Dc_tbl_no],
                    m_ac_derived_tbls[m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Ac_tbl_no]))
                {
                    return false;
                }
                state.last_dc_val[ci] = MCU_data[blkn][0][0];
            }
            m_saved = state;
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    m_restarts_to_go = m_cinfo.m_restart_interval;
                    m_next_restart_num++;
                    m_next_restart_num &= 7;
                }

                m_restarts_to_go--;
            }

            return true;
        }
        private void finish_pass_huff()
        {
            savable_state state;
            state = m_saved;
            if (!flush_bits(state))
                throw new Exception("Suspension not allowed here!");
            m_saved = state;
        }
        private bool encode_mcu_gather(JpegBlock[][] MCU_data)
        {
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                        m_saved.last_dc_val[ci] = 0;
                    m_restarts_to_go = m_cinfo.m_restart_interval;
                }

                m_restarts_to_go--;
            }

            for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
            {
                int ci = m_cinfo.m_MCU_membership[blkn];
                htest_one_block(MCU_data[blkn][0].data, m_saved.last_dc_val[ci],
                    m_dc_count_ptrs[m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Dc_tbl_no],
                    m_ac_count_ptrs[m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Ac_tbl_no]);
                m_saved.last_dc_val[ci] = MCU_data[blkn][0][0];
            }

            return true;
        }
        private void finish_pass_gather()
        {
            bool[] did_dc = new bool[JpegConstants.NumberOfHuffmanTables];
            bool[] did_ac = new bool[JpegConstants.NumberOfHuffmanTables];

            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                int dctbl = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Dc_tbl_no;
                if (!did_dc[dctbl])
                {
                    if (m_cinfo.m_dc_huff_tbl_ptrs[dctbl] == null)
                        m_cinfo.m_dc_huff_tbl_ptrs[dctbl] = new JpegHuffmanTable();

                    jpeg_gen_optimal_table(m_cinfo.m_dc_huff_tbl_ptrs[dctbl], m_dc_count_ptrs[dctbl]);
                    did_dc[dctbl] = true;
                }

                int actbl = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Ac_tbl_no;
                if (!did_ac[actbl])
                {
                    if (m_cinfo.m_ac_huff_tbl_ptrs[actbl] == null)
                        m_cinfo.m_ac_huff_tbl_ptrs[actbl] = new JpegHuffmanTable();

                    jpeg_gen_optimal_table(m_cinfo.m_ac_huff_tbl_ptrs[actbl], m_ac_count_ptrs[actbl]);
                    did_ac[actbl] = true;
                }
            }
        }
        private bool encode_one_block(savable_state state, short[] block, int last_dc_val, c_derived_tbl dctbl, c_derived_tbl actbl)
        {
            int temp = block[0] - last_dc_val;
            int temp2 = temp;
            if (temp < 0)
            {
                temp = -temp;
                temp2--;
            }

            int nbits = 0;
            while (temp != 0)
            {
                nbits++;
                temp >>= 1;
            }
            if (nbits > MAX_HUFFMAN_COEF_BITS + 1)
                throw new Exception("DCT coefficient is out of range!");
            if (!emit_bits(state, dctbl.ehufco[nbits], dctbl.ehufsi[nbits]))
                return false;
            if (nbits != 0)
            {
                if (!emit_bits(state, temp2, nbits))
                    return false;
            }
            int r = 0;
            for (int k = 1; k < JpegConstants.DCTSize2; k++)
            {
                temp = block[JpegUtils.jpeg_natural_order[k]];
                if (temp == 0)
                {
                    r++;
                }
                else
                {
                    while (r > 15)
                    {
                        if (!emit_bits(state, actbl.ehufco[0xF0], actbl.ehufsi[0xF0]))
                            return false;
                        r -= 16;
                    }

                    temp2 = temp;
                    if (temp < 0)
                    {
                        temp = -temp;
                        temp2--;
                    }
                    nbits = 1;
                    while ((temp >>= 1) != 0)
                        nbits++;
                    if (nbits > MAX_HUFFMAN_COEF_BITS)
                        throw new Exception("DCT coefficient is out of range!");
                    int i = (r << 4) + nbits;
                    if (!emit_bits(state, actbl.ehufco[i], actbl.ehufsi[i]))
                        return false;
                    if (!emit_bits(state, temp2, nbits))
                        return false;

                    r = 0;
                }
            }
            if (r > 0)
            {
                if (!emit_bits(state, actbl.ehufco[0], actbl.ehufsi[0]))
                    return false;
            }

            return true;
        }
        private void htest_one_block(short[] block, int last_dc_val, long[] dc_counts, long[] ac_counts)
        {
            int temp = block[0] - last_dc_val;
            if (temp < 0)
                temp = -temp;
            int nbits = 0;
            while (temp != 0)
            {
                nbits++;
                temp >>= 1;
            }
            if (nbits > MAX_HUFFMAN_COEF_BITS + 1)
                throw new Exception("DCT coefficient is out of range!");
            dc_counts[nbits]++;
            int r = 0;
            for (int k = 1; k < JpegConstants.DCTSize2; k++)
            {
                temp = block[JpegUtils.jpeg_natural_order[k]];
                if (temp == 0)
                {
                    r++;
                }
                else
                {
                    while (r > 15)
                    {
                        ac_counts[0xF0]++;
                        r -= 16;
                    }
                    if (temp < 0)
                        temp = -temp;
                    nbits = 1;
                    while ((temp >>= 1) != 0)
                        nbits++;
                    if (nbits > MAX_HUFFMAN_COEF_BITS)
                        throw new Exception("DCT coefficient is out of range!");
                    ac_counts[(r << 4) + nbits]++;

                    r = 0;
                }
            }
            if (r > 0)
                ac_counts[0]++;
        }

        private bool emit_byte(int val)
        {
            return m_cinfo.m_dest.emit_byte(val);
        }
        private bool emit_bits(savable_state state, int code, int size)
        {
            int put_buffer = code;
            int put_bits = state.put_bits;
            if (size == 0)
                throw new Exception("Missing Huffman code table entry");

            put_buffer &= (1 << size) - 1;
            put_bits += size;
            put_buffer <<= 24 - put_bits;
            put_buffer |= state.put_buffer;

            while (put_bits >= 8)
            {
                int c = (put_buffer >> 16) & 0xFF;
                if (!emit_byte(c))
                    return false;

                if (c == 0xFF)
                {
                    if (!emit_byte(0))
                        return false;
                }

                put_buffer <<= 8;
                put_bits -= 8;
            }

            state.put_buffer = put_buffer;
            state.put_bits = put_bits;

            return true;
        }

        private bool flush_bits(savable_state state)
        {
            if (!emit_bits(state, 0x7F, 7))
                return false;

            state.put_buffer = 0;
            state.put_bits = 0;
            return true;
        }
        private bool emit_restart(savable_state state, int restart_num)
        {
            if (!flush_bits(state))
                return false;

            if (!emit_byte(0xFF))
                return false;

            if (!emit_byte((int)(JpegMarkerType.RST0 + restart_num)))
                return false;
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                state.last_dc_val[ci] = 0;
            return true;
        }
    }
    #endregion

    #region IDecompressDestination
    abstract class IDecompressor
    {
        public abstract Stream Output { get; }
        public abstract void SetImageAttributes(LoadedImageAttributes parameters);
        public abstract void BeginWrite();
        public abstract void ProcessPixelsRow(byte[] row);
        public abstract void EndWrite();
    }
    class LoadedImageAttributes
    {
        private ColorSpace m_colorspace;
        private bool m_quantizeColors;
        private int m_width;
        private int m_height;
        private int m_componentsPerSample;
        private int m_components;
        private int m_actualNumberOfColors;
        private byte[][] m_colormap;
        private DensityUnit m_densityUnit;
        private int m_densityX;
        private int m_densityY;
        public ColorSpace Colorspace
        {
            get
            {
                return m_colorspace;
            }
            internal set
            {
                m_colorspace = value;
            }
        }
        public bool QuantizeColors
        {
            get
            {
                return m_quantizeColors;
            }
            internal set
            {
                m_quantizeColors = value;
            }
        }
        public int Width
        {
            get
            {
                return m_width;
            }
            internal set
            {
                m_width = value;
            }
        }
        public int Height
        {
            get
            {
                return m_height;
            }
            internal set
            {
                m_height = value;
            }
        }
        public int ComponentsPerSample
        {
            get
            {
                return m_componentsPerSample;
            }
            internal set
            {
                m_componentsPerSample = value;
            }
        }
        public int Components
        {
            get
            {
                return m_components;
            }
            internal set
            {
                m_components = value;
            }
        }
        public int ActualNumberOfColors
        {
            get
            {
                return m_actualNumberOfColors;
            }
            internal set
            {
                m_actualNumberOfColors = value;
            }
        }
        public byte[][] Colormap
        {
            get
            {
                return m_colormap;
            }
            internal set
            {
                m_colormap = value;
            }
        }
        public DensityUnit DensityUnit
        {
            get
            {
                return m_densityUnit;
            }
            internal set
            {
                m_densityUnit = value;
            }
        }
        public int DensityX
        {
            get
            {
                return m_densityX;
            }
            internal set
            {
                m_densityX = value;
            }
        }
        public int DensityY
        {
            get
            {
                return m_densityY;
            }
            internal set
            {
                m_densityY = value;
            }
        }
    }
    #endregion

    #region IRawImage
    abstract class IRawImage
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract ColorSpace Colorspace { get; }
        public abstract int ComponentsPerPixel { get; }

        public abstract void BeginRead();
        public abstract byte[] GetPixelRow();
        public abstract void EndRead();
    }
    #endregion

    #region Jpeg
    class Jpeg
    {
        private JpegCompressor m_compressor = new JpegCompressor();
        private JpegDecompressor m_decompressor = new JpegDecompressor();
        private CompressionParameters m_compressionParameters = new CompressionParameters();
        private DecompressionParameters m_decompressionParameters = new DecompressionParameters();
        public CompressionParameters CompressionParameters
        {
            get
            {
                return m_compressionParameters;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_compressionParameters = value;
            }
        }
        public DecompressionParameters DecompressionParameters
        {
            get
            {
                return m_decompressionParameters;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_decompressionParameters = value;
            }
        }
        public void Compress(IRawImage source, Stream output)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (output == null)
                throw new ArgumentNullException("output");

            m_compressor.Image_width = source.Width;
            m_compressor.Image_height = source.Height;
            m_compressor.In_color_space = (ColorSpace)source.Colorspace;
            m_compressor.Input_components = source.ComponentsPerPixel;
            m_compressor.jpeg_set_defaults();
            applyParameters(m_compressionParameters);
            m_compressor.jpeg_stdio_dest(output);
            m_compressor.jpeg_start_compress(true);
            source.BeginRead();
            while (m_compressor.Next_scanline < m_compressor.Image_height)
            {
                byte[] row = source.GetPixelRow();
                if (row == null)
                {
                    throw new InvalidDataException("Row of pixels is null");
                }

                byte[][] rowForDecompressor = new byte[1][];
                rowForDecompressor[0] = row;
                m_compressor.jpeg_write_scanlines(rowForDecompressor, 1);
            }
            source.EndRead();
            m_compressor.jpeg_finish_compress();
        }
        public void Decompress(Stream jpeg, IDecompressor destination)
        {
            if (jpeg == null)
                throw new ArgumentNullException("jpeg");

            if (destination == null)
                throw new ArgumentNullException("destination");

            beforeDecompress(jpeg);
            m_decompressor.jpeg_start_decompress();

            LoadedImageAttributes parameters = getImageParametersFromDecompressor();
            destination.SetImageAttributes(parameters);
            destination.BeginWrite();
            while (m_decompressor.Output_scanline < m_decompressor.Output_height)
            {
                byte[][] row = JpegCommonBase.AllocJpegSamples(m_decompressor.Output_width * m_decompressor.Output_components, 1);
                m_decompressor.jpeg_read_scanlines(row, 1);
                destination.ProcessPixelsRow(row[0]);
            }

            destination.EndWrite();
            m_decompressor.jpeg_finish_decompress();
        }
        private void beforeDecompress(Stream jpeg)
        {
            m_decompressor.jpeg_stdio_src(jpeg);
            m_decompressor.jpeg_read_header(true);
            applyParameters(m_decompressionParameters);
            m_decompressor.jpeg_calc_output_dimensions();
        }

        private LoadedImageAttributes getImageParametersFromDecompressor()
        {
            LoadedImageAttributes result = new LoadedImageAttributes();
            result.Colorspace = (ColorSpace)m_decompressor.Out_color_space;
            result.QuantizeColors = m_decompressor.Quantize_colors;
            result.Width = m_decompressor.Output_width;
            result.Height = m_decompressor.Output_height;
            result.ComponentsPerSample = m_decompressor.Out_color_components;
            result.Components = m_decompressor.Output_components;
            result.ActualNumberOfColors = m_decompressor.Actual_number_of_colors;
            result.Colormap = m_decompressor.Colormap;
            result.DensityUnit = m_decompressor.Density_unit;
            result.DensityX = m_decompressor.X_density;
            result.DensityY = m_decompressor.Y_density;
            return result;
        }

        public JpegCompressor ClassicCompressor
        {
            get
            {
                return m_compressor;
            }
        }

        public JpegDecompressor ClassicDecompressor
        {
            get
            {
                return m_decompressor;
            }
        }
        public delegate bool MarkerParser(Jpeg decompressor);
        public void SetMarkerProcessor(int markerCode, MarkerParser routine)
        {
            JpegDecompressor.jpeg_marker_parser_method f = delegate { return routine(this); };
            m_decompressor.jpeg_set_marker_processor(markerCode, f);
        }

        private void applyParameters(DecompressionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("'parameters' Cannot be null!");

            if (parameters.OutColorspace != ColorSpace.Unknown)
                m_decompressor.Out_color_space = (ColorSpace)parameters.OutColorspace;

            m_decompressor.Scale_num = parameters.ScaleNumerator;
            m_decompressor.Scale_denom = parameters.ScaleDenominator;
            m_decompressor.Buffered_image = parameters.BufferedImage;
            m_decompressor.Raw_data_out = parameters.RawDataOut;
            m_decompressor.Dct_method = (DCTMethod)parameters.DCTMethod;
            m_decompressor.Dither_mode = (DitherMode)parameters.DitherMode;
            m_decompressor.Do_fancy_upsampling = parameters.DoFancyUpsampling;
            m_decompressor.Do_block_smoothing = parameters.DoBlockSmoothing;
            m_decompressor.Quantize_colors = parameters.QuantizeColors;
            m_decompressor.Two_pass_quantize = parameters.TwoPassQuantize;
            m_decompressor.Desired_number_of_colors = parameters.DesiredNumberOfColors;
            m_decompressor.Enable_1pass_quant = parameters.EnableOnePassQuantizer;
            m_decompressor.Enable_external_quant = parameters.EnableExternalQuant;
            m_decompressor.Enable_2pass_quant = parameters.EnableTwoPassQuantizer;
        }

        private void applyParameters(CompressionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("'parameters' Cannot be null!");

            m_compressor.Smoothing_factor = parameters.SmoothingFactor;
            m_compressor.jpeg_set_quality(parameters.Quality, true);
            if (parameters.SimpleProgressive)
                m_compressor.jpeg_simple_progression();
        }
    }
    #endregion

    #region JpegBlock
    public class JpegBlock
    {
        internal short[] data = new short[JpegConstants.DCTSize2];
        public short this[int index]
        {
            get
            {
                return data[index];
            }
            set
            {
                data[index] = value;
            }
        }
    }
    #endregion

    #region JpegCompressor
    public class JpegCompressor : JpegCommonBase
    {
        private static int[] std_luminance_quant_tbl = { 
            16, 11, 10, 16, 24, 40, 51, 61, 12, 12, 14, 19, 26,
            58, 60, 55, 14, 13, 16, 24, 40, 57, 69, 56, 14, 17,
            22, 29, 51, 87, 80, 62, 18, 22, 37, 56, 68, 109,
            103, 77, 24, 35, 55, 64, 81, 104, 113, 92, 49, 64,
            78, 87, 103, 121, 120, 101, 72, 92, 95, 98, 112,
            100, 103, 99 };

        private static int[] std_chrominance_quant_tbl = {
            17, 18, 24, 47, 99, 99, 99, 99, 18, 21, 26, 66,
            99, 99, 99, 99, 24, 26, 56, 99, 99, 99, 99, 99,
            47, 66, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99 };
        private static byte[] bits_dc_luminance = { 0, 0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 };

        private static byte[] val_dc_luminance = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        private static byte[] bits_dc_chrominance = { 0, 0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };

        private static byte[] val_dc_chrominance = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        private static byte[] bits_ac_luminance = { 0, 0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 0x7d };

        private static byte[] val_ac_luminance = 
            { 0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
              0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08,
              0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0, 0x24, 0x33, 0x62, 0x72,
              0x82, 0x09, 0x0a, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28,
              0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45,
              0x46, 0x47, 0x48, 0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
              0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x73, 0x74, 0x75,
              0x76, 0x77, 0x78, 0x79, 0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
              0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3,
              0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6,
              0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9,
              0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
              0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf1, 0xf2, 0xf3, 0xf4,
              0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa };

        private static byte[] bits_ac_chrominance = { 0, 0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 0x77 };

        private static byte[] val_ac_chrominance = 
            { 0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, 0x31, 0x06, 0x12, 0x41,
              0x51, 0x07, 0x61, 0x71, 0x13, 0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91,
              0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0, 0x15, 0x62, 0x72, 0xd1,
              0x0a, 0x16, 0x24, 0x34, 0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26,
              0x27, 0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44,
              0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
              0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x73, 0x74,
              0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
              0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a,
              0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4,
              0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7,
              0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
              0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf2, 0xf3, 0xf4,
              0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa };

        internal DestinationManager m_dest;
        internal int m_image_width;
        internal int m_image_height;
        internal int m_input_components;
        internal ColorSpace m_in_color_space;
        internal int m_data_precision;
        internal int m_num_components;
        internal ColorSpace m_jpeg_color_space;
        private JpegComponent[] m_comp_info;
        internal JpegQuantizationTable[] m_quant_tbl_ptrs = new JpegQuantizationTable[JpegConstants.NumberOfQuantTables];
        internal JpegHuffmanTable[] m_dc_huff_tbl_ptrs = new JpegHuffmanTable[JpegConstants.NumberOfHuffmanTables];
        internal JpegHuffmanTable[] m_ac_huff_tbl_ptrs = new JpegHuffmanTable[JpegConstants.NumberOfHuffmanTables];
        internal int m_num_scans;
        internal JpegScanInfo[] m_scan_info;
        internal bool m_raw_data_in;
        internal bool m_optimize_coding;
        internal bool m_CCIR601_sampling;
        internal int m_smoothing_factor;
        internal DCTMethod m_dct_method;
        internal int m_restart_interval;
        internal int m_restart_in_rows;
        internal bool m_write_JFIF_header;
        internal byte m_JFIF_major_version;
        internal byte m_JFIF_minor_version;
        internal DensityUnit m_density_unit;
        internal short m_X_density;
        internal short m_Y_density;
        internal bool m_write_Adobe_marker;
        internal int m_next_scanline;
        internal bool m_progressive_mode;
        internal int m_max_h_samp_factor;
        internal int m_max_v_samp_factor;
        internal int m_total_iMCU_rows;
        internal int m_comps_in_scan;
        internal int[] m_cur_comp_info = new int[JpegConstants.MaxComponentsInScan];
        internal int m_MCUs_per_row;
        internal int m_MCU_rows_in_scan;
        internal int m_blocks_in_MCU;
        internal int[] m_MCU_membership = new int[JpegConstants.CompressorMaxBlocksInMCU];
        internal int m_Ss;
        internal int m_Se;
        internal int m_Ah;
        internal int m_Al;
        internal JpegCompressorMaster m_master;
        internal JpegCompressorMainController m_main;
        internal JpegCompressorPrepController m_prep;
        internal JpegCompressorCoefController m_coef;
        internal JpegMarkerWriter m_marker;
        internal ColorConverter m_cconvert;
        internal JpegDownsampler m_downsample;
        internal JpegFowardDCT m_fdct;
        internal JpegEntropyEncoder m_entropy;
        internal JpegScanInfo[] m_script_space;
        internal int m_script_space_size;
        public JpegCompressor()
            : base()
        {
            initialize();
        }
        public override bool IsDecompressor
        {
            get { return false; }
        }
        public LibJpeg.DestinationManager Dest
        {
            get { return m_dest; }
            set { m_dest = value; }
        }
        public int Image_width
        {
            get { return m_image_width; }
            set { m_image_width = value; }
        }
        public int Image_height
        {
            get { return m_image_height; }
            set { m_image_height = value; }
        }
        public int Input_components
        {
            get { return m_input_components; }
            set { m_input_components = value; }
        }
        public LibJpeg.ColorSpace In_color_space
        {
            get { return m_in_color_space; }
            set { m_in_color_space = value; }
        }
        public int Data_precision
        {
            get { return m_data_precision; }
            set { m_data_precision = value; }
        }
        public int Num_components
        {
            get { return m_num_components; }
            set { m_num_components = value; }
        }
        public ColorSpace Jpeg_color_space
        {
            get { return m_jpeg_color_space; }
            set { m_jpeg_color_space = value; }
        }
        public bool Raw_data_in
        {
            get { return m_raw_data_in; }
            set { m_raw_data_in = value; }
        }
        public bool Optimize_coding
        {
            get { return m_optimize_coding; }
            set { m_optimize_coding = value; }
        }
        public bool CCIR601_sampling
        {
            get { return m_CCIR601_sampling; }
            set { m_CCIR601_sampling = value; }
        }
        public int Smoothing_factor
        {
            get { return m_smoothing_factor; }
            set { m_smoothing_factor = value; }
        }
        public DCTMethod Dct_method
        {
            get { return m_dct_method; }
            set { m_dct_method = value; }
        }
        public int Restart_interval
        {
            get { return m_restart_interval; }
            set { m_restart_interval = value; }
        }
        public int Restart_in_rows
        {
            get { return m_restart_in_rows; }
            set { m_restart_in_rows = value; }
        }
        public bool Write_JFIF_header
        {
            get { return m_write_JFIF_header; }
            set { m_write_JFIF_header = value; }
        }
        public byte JFIF_major_version
        {
            get { return m_JFIF_major_version; }
            set { m_JFIF_major_version = value; }
        }
        public byte JFIF_minor_version
        {
            get { return m_JFIF_minor_version; }
            set { m_JFIF_minor_version = value; }
        }
        public DensityUnit Density_unit
        {
            get { return m_density_unit; }
            set { m_density_unit = value; }
        }
        public short X_density
        {
            get { return m_X_density; }
            set { m_X_density = value; }
        }
        public short Y_density
        {
            get { return m_Y_density; }
            set { m_Y_density = value; }
        }
        public bool Write_Adobe_marker
        {
            get { return m_write_Adobe_marker; }
            set { m_write_Adobe_marker = value; }
        }
        public int Max_v_samp_factor
        {
            get { return m_max_v_samp_factor; }
        }
        public JpegComponent[] Component_info
        {
            get { return m_comp_info; }
        }
        public JpegQuantizationTable[] Quant_tbl_ptrs
        {
            get { return m_quant_tbl_ptrs; }
        }
        public JpegHuffmanTable[] Dc_huff_tbl_ptrs
        {
            get { return m_dc_huff_tbl_ptrs; }
        }
        public JpegHuffmanTable[] Ac_huff_tbl_ptrs
        {
            get { return m_ac_huff_tbl_ptrs; }
        }
        public int Next_scanline
        {
            get { return m_next_scanline; }
        }
        public void jpeg_abort_compress()
        {
            jpeg_abort();
        }
        public void jpeg_suppress_tables(bool suppress)
        {
            for (int i = 0; i < JpegConstants.NumberOfQuantTables; i++)
            {
                if (m_quant_tbl_ptrs[i] != null)
                    m_quant_tbl_ptrs[i].Sent_table = suppress;
            }

            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
            {
                if (m_dc_huff_tbl_ptrs[i] != null)
                    m_dc_huff_tbl_ptrs[i].Sent_table = suppress;

                if (m_ac_huff_tbl_ptrs[i] != null)
                    m_ac_huff_tbl_ptrs[i].Sent_table = suppress;
            }
        }
        public void jpeg_finish_compress()
        {
            int iMCU_row;

            if (m_global_state == JpegState.CSTATE_SCANNING || m_global_state == JpegState.CSTATE_RAW_OK)
            {
                if (m_next_scanline < m_image_height)
                    throw new Exception("Application transferred too few scanlines");
                m_master.finish_pass();
            }
            else if (m_global_state != JpegState.CSTATE_WRCOEFS)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            while (!m_master.IsLastPass())
            {
                m_master.prepare_for_pass();
                for (iMCU_row = 0; iMCU_row < m_total_iMCU_rows; iMCU_row++)
                {
                    if (!m_coef.compress_data(null))
                        throw new Exception("Suspension not allowed here");
                }

                m_master.finish_pass();
            }
            m_marker.write_file_trailer();
            m_dest.term_destination();
            jpeg_abort();
        }
        public void jpeg_write_marker(int marker, byte[] data)
        {
            if (m_next_scanline != 0 || (m_global_state != JpegState.CSTATE_SCANNING && m_global_state != JpegState.CSTATE_RAW_OK && m_global_state != JpegState.CSTATE_WRCOEFS))
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            m_marker.write_marker_header(marker, data.Length);

            for (int i = 0; i < data.Length; i++)
                m_marker.write_marker_byte(data[i]);
        }
        public void jpeg_write_m_header(int marker, int datalen)
        {
            if (m_next_scanline != 0 || (m_global_state != JpegState.CSTATE_SCANNING && m_global_state != JpegState.CSTATE_RAW_OK && m_global_state != JpegState.CSTATE_WRCOEFS))
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            m_marker.write_marker_header(marker, datalen);
        }
        public void jpeg_write_m_byte(byte val)
        {
            m_marker.write_marker_byte(val);
        }
        public void jpeg_write_tables()
        {
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            m_dest.init_destination();
            m_marker = new JpegMarkerWriter(this);
            m_marker.write_tables_only();
            m_dest.term_destination();
        }
        public void jpeg_stdio_dest(Stream outfile)
        {
            m_dest = new DestinationManagerImpl(this, outfile);
        }
        public void jpeg_set_defaults()
        {
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));
            if (m_comp_info == null)
            {
                m_comp_info = JpegComponent.createArrayOfComponents(JpegConstants.MaxComponents);
            }
            m_data_precision = JpegConstants.BitsInSample;
            jpeg_set_quality(75, true);
            std_huff_tables();
            m_scan_info = null;
            m_num_scans = 0;
            m_raw_data_in = false;
            m_optimize_coding = false;
            if (m_data_precision > 8)
                m_optimize_coding = true;
            m_CCIR601_sampling = false;
            m_smoothing_factor = 0;
            m_dct_method = JpegConstants.DefaultDCTMethod;
            m_restart_interval = 0;
            m_restart_in_rows = 0;
            m_JFIF_major_version = 1;
            m_JFIF_minor_version = 1;
            m_density_unit = DensityUnit.Unknown;
            m_X_density = 1;
            m_Y_density = 1;
            jpeg_default_colorspace();
        }
        public void jpeg_set_colorspace(ColorSpace colorspace)
        {
            int ci;
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            m_jpeg_color_space = colorspace;
            m_write_JFIF_header = false;
            m_write_Adobe_marker = false;

            switch (colorspace)
            {
                case ColorSpace.Grayscale:
                    m_write_JFIF_header = true;
                    m_num_components = 1;
                    jpeg_set_colorspace_SET_COMP(0, 1, 1, 1, 0, 0, 0);
                    break;
                case ColorSpace.RGB:
                    m_write_Adobe_marker = true;
                    m_num_components = 3;
                    jpeg_set_colorspace_SET_COMP(0, 0x52, 1, 1, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(1, 0x47, 1, 1, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(2, 0x42, 1, 1, 0, 0, 0);
                    break;
                case ColorSpace.YCbCr:
                    m_write_JFIF_header = true;
                    m_num_components = 3;
                    jpeg_set_colorspace_SET_COMP(0, 1, 2, 2, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(1, 2, 1, 1, 1, 1, 1);
                    jpeg_set_colorspace_SET_COMP(2, 3, 1, 1, 1, 1, 1);
                    break;
                case ColorSpace.CMYK:
                    m_write_Adobe_marker = true;
                    m_num_components = 4;
                    jpeg_set_colorspace_SET_COMP(0, 0x43, 1, 1, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(1, 0x4D, 1, 1, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(2, 0x59, 1, 1, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(3, 0x4B, 1, 1, 0, 0, 0);
                    break;
                case ColorSpace.YCCK:
                    m_write_Adobe_marker = true;
                    m_num_components = 4;
                    jpeg_set_colorspace_SET_COMP(0, 1, 2, 2, 0, 0, 0);
                    jpeg_set_colorspace_SET_COMP(1, 2, 1, 1, 1, 1, 1);
                    jpeg_set_colorspace_SET_COMP(2, 3, 1, 1, 1, 1, 1);
                    jpeg_set_colorspace_SET_COMP(3, 4, 2, 2, 0, 0, 0);
                    break;
                case ColorSpace.Unknown:
                    m_num_components = m_input_components;
                    if (m_num_components < 1 || m_num_components > JpegConstants.MaxComponents)
                        throw new Exception(String.Format("Too many color components: {0}, max {1}", m_num_components, JpegConstants.MaxComponents));
                    for (ci = 0; ci < m_num_components; ci++)
                    {
                        jpeg_set_colorspace_SET_COMP(ci, ci, 1, 1, 0, 0, 0);
                    }
                    break;
                default:
                    throw new Exception("Bad Jpeg ColorSpace.");
            }
        }
        public void jpeg_default_colorspace()
        {
            switch (m_in_color_space)
            {
                case ColorSpace.Grayscale:
                    jpeg_set_colorspace(ColorSpace.Grayscale);
                    break;
                case ColorSpace.RGB:
                    jpeg_set_colorspace(ColorSpace.YCbCr);
                    break;
                case ColorSpace.YCbCr:
                    jpeg_set_colorspace(ColorSpace.YCbCr);
                    break;
                case ColorSpace.CMYK:
                    jpeg_set_colorspace(ColorSpace.CMYK);
                    break;
                case ColorSpace.YCCK:
                    jpeg_set_colorspace(ColorSpace.YCCK);
                    break;
                case ColorSpace.Unknown:
                    jpeg_set_colorspace(ColorSpace.Unknown);
                    break;
                default:
                    throw new Exception("Bad input colorspace!");
            }
        }
        public void jpeg_set_quality(int quality, bool force_baseline)
        {
            quality = jpeg_quality_scaling(quality);
            jpeg_set_linear_quality(quality, force_baseline);
        }
        public void jpeg_set_linear_quality(int scale_factor, bool force_baseline)
        {
            jpeg_add_quant_table(0, std_luminance_quant_tbl, scale_factor, force_baseline);
            jpeg_add_quant_table(1, std_chrominance_quant_tbl, scale_factor, force_baseline);
        }
        public void jpeg_add_quant_table(int which_tbl, int[] basic_table, int scale_factor, bool force_baseline)
        {
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            if (which_tbl < 0 || which_tbl >= JpegConstants.NumberOfQuantTables)
                throw new Exception(String.Format("Bogus DQT index {0}", which_tbl));

            if (m_quant_tbl_ptrs[which_tbl] == null)
                m_quant_tbl_ptrs[which_tbl] = new JpegQuantizationTable();

            for (int i = 0; i < JpegConstants.DCTSize2; i++)
            {
                int temp = (basic_table[i] * scale_factor + 50) / 100;
                if (temp <= 0)
                    temp = 1;
                if (temp > 32767)
                    temp = 32767;
                if (force_baseline && temp > 255)
                    temp = 255;

                m_quant_tbl_ptrs[which_tbl].quantval[i] = (short)temp;
            }
            m_quant_tbl_ptrs[which_tbl].Sent_table = false;
        }
        public static int jpeg_quality_scaling(int quality)
        {
            if (quality <= 0)
                quality = 1;

            if (quality > 100)
                quality = 100;

            if (quality < 50)
                quality = 5000 / quality;
            else
                quality = 200 - quality * 2;

            return quality;
        }
        public void jpeg_simple_progression()
        {
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            int nscans;
            if (m_num_components == 3 && m_jpeg_color_space == ColorSpace.YCbCr)
            {
                nscans = 10;
            }
            else
            {
                if (m_num_components > JpegConstants.MaxComponentsInScan)
                {
                    nscans = 6 * m_num_components;
                }
                else
                {
                    nscans = 2 + 4 * m_num_components;
                }
            }
            if (m_script_space == null || m_script_space_size < nscans)
            {
                m_script_space_size = Math.Max(nscans, 10);
                m_script_space = new JpegScanInfo[m_script_space_size];
                for (int i = 0; i < m_script_space_size; i++)
                    m_script_space[i] = new JpegScanInfo();
            }

            m_scan_info = m_script_space;
            m_num_scans = nscans;

            int scanIndex = 0;
            if (m_num_components == 3 && m_jpeg_color_space == ColorSpace.YCbCr)
            {
                fill_dc_scans(ref scanIndex, m_num_components, 0, 1);
                fill_a_scan(ref scanIndex, 0, 1, 5, 0, 2);
                fill_a_scan(ref scanIndex, 2, 1, 63, 0, 1);
                fill_a_scan(ref scanIndex, 1, 1, 63, 0, 1);
                fill_a_scan(ref scanIndex, 0, 6, 63, 0, 2);
                fill_a_scan(ref scanIndex, 0, 1, 63, 2, 1);
                fill_dc_scans(ref scanIndex, m_num_components, 1, 0);
                fill_a_scan(ref scanIndex, 2, 1, 63, 1, 0);
                fill_a_scan(ref scanIndex, 1, 1, 63, 1, 0);
                fill_a_scan(ref scanIndex, 0, 1, 63, 1, 0);
            }
            else
            {
                fill_dc_scans(ref scanIndex, m_num_components, 0, 1);
                fill_scans(ref scanIndex, m_num_components, 1, 5, 0, 2);
                fill_scans(ref scanIndex, m_num_components, 6, 63, 0, 2);
                fill_scans(ref scanIndex, m_num_components, 1, 63, 2, 1);
                fill_dc_scans(ref scanIndex, m_num_components, 1, 0);
                fill_scans(ref scanIndex, m_num_components, 1, 63, 1, 0);
            }
        }
        public void jpeg_start_compress(bool write_all_tables)
        {
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            if (write_all_tables)
                jpeg_suppress_tables(false);
            m_dest.init_destination();
            jinit_compress_master();
            m_master.prepare_for_pass();
            m_next_scanline = 0;
            m_global_state = (m_raw_data_in ? JpegState.CSTATE_RAW_OK : JpegState.CSTATE_SCANNING);
        }
        public int jpeg_write_scanlines(byte[][] scanlines, int num_lines)
        {
            if (m_global_state != JpegState.CSTATE_SCANNING)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            if (m_master.MustCallPassStartup())
                m_master.pass_startup();

            int rows_left = m_image_height - m_next_scanline;
            if (num_lines > rows_left)
                num_lines = rows_left;

            int row_ctr = 0;
            m_main.process_data(scanlines, ref row_ctr, num_lines);
            m_next_scanline += row_ctr;
            return row_ctr;
        }
        public int jpeg_write_raw_data(byte[][][] data, int num_lines)
        {
            if (m_global_state != JpegState.CSTATE_RAW_OK)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            if (m_next_scanline >= m_image_height)
            {
                return 0;
            }
            if (m_master.MustCallPassStartup())
                m_master.pass_startup();


            int lines_per_iMCU_row = m_max_v_samp_factor * JpegConstants.DCTSize;
            if (num_lines < lines_per_iMCU_row)
                throw new Exception("Buffer passed to JPEG library is too small");

            if (!m_coef.compress_data(data))
            {
                return 0;
            }

            m_next_scanline += lines_per_iMCU_row;
            return lines_per_iMCU_row;
        }
        public void jpeg_write_coefficients(JpegVirtualArray<JpegBlock>[] coef_arrays)
        {
            if (m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            jpeg_suppress_tables(false);
            m_dest.init_destination();
            transencode_master_selection(coef_arrays);
            m_next_scanline = 0;
            m_global_state = JpegState.CSTATE_WRCOEFS;
        }
        private void initialize()
        {
            m_dest = null;
            m_comp_info = null;

            for (int i = 0; i < JpegConstants.NumberOfQuantTables; i++)
                m_quant_tbl_ptrs[i] = null;

            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
            {
                m_dc_huff_tbl_ptrs[i] = null;
                m_ac_huff_tbl_ptrs[i] = null;
            }
            m_script_space = null;
            m_global_state = JpegState.CSTATE_START;
        }
        private void jinit_compress_master()
        {
            jinit_c_master_control(false);
            if (!m_raw_data_in)
            {
                m_cconvert = new ColorConverter(this);
                m_downsample = new JpegDownsampler(this);
                m_prep = new JpegCompressorPrepController(this);
            }
            m_fdct = new JpegFowardDCT(this);
            if (m_progressive_mode)
                m_entropy = new ProgressiveHuffmanEncoder(this);
            else
                m_entropy = new HuffEntropyEncoder(this);
            m_coef = new CoefControllerImpl(this, (bool)(m_num_scans > 1 || m_optimize_coding));
            jinit_c_main_controller(false);
            m_marker = new JpegMarkerWriter(this);
            m_marker.write_file_header();
        }
        private void jinit_c_master_control(bool transcode_only)
        {
            initial_setup();

            if (m_scan_info != null)
            {
                validate_script();
            }
            else
            {
                m_progressive_mode = false;
                m_num_scans = 1;
            }

            if (m_progressive_mode)
                m_optimize_coding = true;

            m_master = new JpegCompressorMaster(this, transcode_only);
        }
        private void jinit_c_main_controller(bool need_full_buffer)
        {
            if (m_raw_data_in)
                return;
            if (need_full_buffer)
                throw new Exception("Bogus buffer control mode");
            else
                m_main = new JpegCompressorMainController(this);
        }
        private void transencode_master_selection(JpegVirtualArray<JpegBlock>[] coef_arrays)
        {
            m_input_components = 1;
            jinit_c_master_control(true);
            if (m_progressive_mode)
                m_entropy = new ProgressiveHuffmanEncoder(this);
            else
                m_entropy = new HuffEntropyEncoder(this);
            m_coef = new TransCoefControllerImpl(this, coef_arrays);
            m_marker = new JpegMarkerWriter(this);
            m_marker.write_file_header();
        }
        private void initial_setup()
        {
            if (m_image_height <= 0 || m_image_width <= 0 || m_num_components <= 0 || m_input_components <= 0)
                throw new Exception("Empty JPEG image (DNL not supported)");

            if (m_image_height > JpegConstants.JpegMaxDimention || m_image_width > JpegConstants.JpegMaxDimention)
                throw new Exception(String.Format("Maximum supported image dimension is {0} pixels", (int)JpegConstants.JpegMaxDimention));

            long samplesperrow = m_image_width * m_input_components;
            int jd_samplesperrow = (int)samplesperrow;
            if ((long)jd_samplesperrow != samplesperrow)
                throw new Exception("Image too wide for this implementation");

            if (m_data_precision != JpegConstants.BitsInSample)
                throw new Exception(String.Format("Unsupported JPEG data precision {0}", m_data_precision));

            if (m_num_components > JpegConstants.MaxComponents)
                throw new Exception(String.Format("Too many color components: {0}, max {1}", m_num_components, JpegConstants.MaxComponents));

            m_max_h_samp_factor = 1;
            m_max_v_samp_factor = 1;
            for (int ci = 0; ci < m_num_components; ci++)
            {
                if (m_comp_info[ci].H_samp_factor <= 0 || m_comp_info[ci].H_samp_factor > JpegConstants.MaxSamplingFactor ||
                    m_comp_info[ci].V_samp_factor <= 0 || m_comp_info[ci].V_samp_factor > JpegConstants.MaxSamplingFactor)
                {
                    throw new Exception("Bogus sampling factors");
                }

                m_max_h_samp_factor = Math.Max(m_max_h_samp_factor, m_comp_info[ci].H_samp_factor);
                m_max_v_samp_factor = Math.Max(m_max_v_samp_factor, m_comp_info[ci].V_samp_factor);
            }

            for (int ci = 0; ci < m_num_components; ci++)
            {
                m_comp_info[ci].Component_index = ci;
                m_comp_info[ci].DCT_scaled_size = JpegConstants.DCTSize;
                m_comp_info[ci].Width_in_blocks = JpegUtils.jdiv_round_up(
                    m_image_width * m_comp_info[ci].H_samp_factor, m_max_h_samp_factor * JpegConstants.DCTSize);
                m_comp_info[ci].height_in_blocks = JpegUtils.jdiv_round_up(
                    m_image_height * m_comp_info[ci].V_samp_factor, m_max_v_samp_factor * JpegConstants.DCTSize);
                m_comp_info[ci].downsampled_width = JpegUtils.jdiv_round_up(
                    m_image_width * m_comp_info[ci].H_samp_factor, m_max_h_samp_factor);
                m_comp_info[ci].downsampled_height = JpegUtils.jdiv_round_up(
                    m_image_height * m_comp_info[ci].V_samp_factor, m_max_v_samp_factor);
                m_comp_info[ci].component_needed = true;
            }
            m_total_iMCU_rows = JpegUtils.jdiv_round_up(m_image_height, m_max_v_samp_factor * JpegConstants.DCTSize);
        }
        private void validate_script()
        {
            if (m_num_scans <= 0)
                throw new Exception(String.Format("Invalid scan script at entry {0}", 0));

            int[][] last_bitpos = new int[JpegConstants.MaxComponents][];
            for (int i = 0; i < JpegConstants.MaxComponents; i++)
                last_bitpos[i] = new int[JpegConstants.DCTSize2];

            bool[] component_sent = new bool[JpegConstants.MaxComponents];
            if (m_scan_info[0].Ss != 0 || m_scan_info[0].Se != JpegConstants.DCTSize2 - 1)
            {
                m_progressive_mode = true;
                for (int ci = 0; ci < m_num_components; ci++)
                {
                    for (int coefi = 0; coefi < JpegConstants.DCTSize2; coefi++)
                        last_bitpos[ci][coefi] = -1;
                }
            }
            else
            {
                m_progressive_mode = false;
                for (int ci = 0; ci < m_num_components; ci++)
                    component_sent[ci] = false;
            }

            for (int scanno = 1; scanno <= m_num_scans; scanno++)
            {
                JpegScanInfo scanInfo = m_scan_info[scanno - 1];

                int ncomps = scanInfo.comps_in_scan;
                if (ncomps <= 0 || ncomps > JpegConstants.MaxComponentsInScan)
                    throw new Exception(String.Format("Too many color components: {0}, max {1}", ncomps, JpegConstants.MaxComponentsInScan));

                for (int ci = 0; ci < ncomps; ci++)
                {
                    int thisi = scanInfo.component_index[ci];
                    if (thisi < 0 || thisi >= m_num_components)
                        throw new Exception(String.Format("Invalid scan script at entry {0}", scanno));

                    if (ci > 0 && thisi <= scanInfo.component_index[ci - 1])
                        throw new Exception(String.Format("Invalid scan script at entry {0}", scanno));
                }

                int Ss = scanInfo.Ss;
                int Se = scanInfo.Se;
                int Ah = scanInfo.Ah;
                int Al = scanInfo.Al;
                if (m_progressive_mode)
                {
                    const int MAX_AH_AL = 10;
                    if (Ss < 0 || Ss >= JpegConstants.DCTSize2 || Se < Ss || Se >= JpegConstants.DCTSize2 ||
                        Ah < 0 || Ah > MAX_AH_AL || Al < 0 || Al > MAX_AH_AL)
                    {
                        throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));
                    }

                    if (Ss == 0)
                    {
                        if (Se != 0)
                            throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));
                    }
                    else
                    {
                        if (ncomps != 1)
                            throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));
                    }

                    for (int ci = 0; ci < ncomps; ci++)
                    {
                        int lastBitComponentIndex = scanInfo.component_index[ci];
                        if (Ss != 0 && last_bitpos[lastBitComponentIndex][0] < 0)
                            throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));

                        for (int coefi = Ss; coefi <= Se; coefi++)
                        {
                            if (last_bitpos[lastBitComponentIndex][coefi] < 0)
                            {
                                if (Ah != 0)
                                    throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));
                            }
                            else
                            {
                                if (Ah != last_bitpos[lastBitComponentIndex][coefi] || Al != Ah - 1)
                                    throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));
                            }

                            last_bitpos[lastBitComponentIndex][coefi] = Al;
                        }
                    }
                }
                else
                {
                    if (Ss != 0 || Se != JpegConstants.DCTSize2 - 1 || Ah != 0 || Al != 0)
                        throw new Exception(String.Format("Invalid progressive parameters at scan script entry {0}", scanno));

                    for (int ci = 0; ci < ncomps; ci++)
                    {
                        int thisi = scanInfo.component_index[ci];
                        if (component_sent[thisi])
                            throw new Exception(String.Format("Invalid scan script at entry {0}", scanno));

                        component_sent[thisi] = true;
                    }
                }
            }

            if (m_progressive_mode)
            {
                for (int ci = 0; ci < m_num_components; ci++)
                {
                    if (last_bitpos[ci][0] < 0)
                        throw new Exception("Scan script does not transmit all data");
                }
            }
            else
            {
                for (int ci = 0; ci < m_num_components; ci++)
                {
                    if (!component_sent[ci])
                        throw new Exception("Scan script does not transmit all data");
                }
            }
        }
        private void std_huff_tables()
        {
            add_huff_table(ref m_dc_huff_tbl_ptrs[0], bits_dc_luminance, val_dc_luminance);
            add_huff_table(ref m_ac_huff_tbl_ptrs[0], bits_ac_luminance, val_ac_luminance);
            add_huff_table(ref m_dc_huff_tbl_ptrs[1], bits_dc_chrominance, val_dc_chrominance);
            add_huff_table(ref m_ac_huff_tbl_ptrs[1], bits_ac_chrominance, val_ac_chrominance);
        }
        private void add_huff_table(ref JpegHuffmanTable htblptr, byte[] bits, byte[] val)
        {
            if (htblptr == null)
                htblptr = new JpegHuffmanTable();
            Buffer.BlockCopy(bits, 0, htblptr.Bits, 0, htblptr.Bits.Length);
            int nsymbols = 0;
            for (int len = 1; len <= 16; len++)
                nsymbols += bits[len];

            if (nsymbols < 1 || nsymbols > 256)
                throw new Exception("Bogus Huffman table definition");

            Buffer.BlockCopy(val, 0, htblptr.Huffval, 0, nsymbols);
            htblptr.Sent_table = false;
        }
        private void fill_a_scan(ref int scanIndex, int ci, int Ss, int Se, int Ah, int Al)
        {
            m_script_space[scanIndex].comps_in_scan = 1;
            m_script_space[scanIndex].component_index[0] = ci;
            m_script_space[scanIndex].Ss = Ss;
            m_script_space[scanIndex].Se = Se;
            m_script_space[scanIndex].Ah = Ah;
            m_script_space[scanIndex].Al = Al;
            scanIndex++;
        }
        private void fill_dc_scans(ref int scanIndex, int ncomps, int Ah, int Al)
        {
            if (ncomps <= JpegConstants.MaxComponentsInScan)
            {
                m_script_space[scanIndex].comps_in_scan = ncomps;
                for (int ci = 0; ci < ncomps; ci++)
                    m_script_space[scanIndex].component_index[ci] = ci;

                m_script_space[scanIndex].Ss = 0;
                m_script_space[scanIndex].Se = 0;
                m_script_space[scanIndex].Ah = Ah;
                m_script_space[scanIndex].Al = Al;
                scanIndex++;
            }
            else
            {
                fill_scans(ref scanIndex, ncomps, 0, 0, Ah, Al);
            }
        }
        private void fill_scans(ref int scanIndex, int ncomps, int Ss, int Se, int Ah, int Al)
        {
            for (int ci = 0; ci < ncomps; ci++)
            {
                m_script_space[scanIndex].comps_in_scan = 1;
                m_script_space[scanIndex].component_index[0] = ci;
                m_script_space[scanIndex].Ss = Ss;
                m_script_space[scanIndex].Se = Se;
                m_script_space[scanIndex].Ah = Ah;
                m_script_space[scanIndex].Al = Al;
                scanIndex++;
            }
        }

        private void jpeg_set_colorspace_SET_COMP(int index, int id, int hsamp, int vsamp, int quant, int dctbl, int actbl)
        {
            m_comp_info[index].Component_id = id;
            m_comp_info[index].H_samp_factor = hsamp;
            m_comp_info[index].V_samp_factor = vsamp;
            m_comp_info[index].Quant_tbl_no = quant;
            m_comp_info[index].Dc_tbl_no = dctbl;
            m_comp_info[index].Ac_tbl_no = actbl;
        }
    }
    #endregion



    // STOPPED REMOVING COMMENTS HERE.



    #region JpegCompressorCoefController
    /// <summary>
    /// Coefficient buffer control
    /// </summary>
    interface JpegCompressorCoefController
    {
        void start_pass(BufferMode pass_mode);
        bool compress_data(byte[][][] input_buf);
    }
    #endregion

    #region JpegCompressorMainController
    /// <summary>
    /// Main buffer control (downsampled-data buffer)
    /// </summary>
    class JpegCompressorMainController
    {
        private JpegCompressor m_cinfo;

        private int m_cur_iMCU_row;    /* number of current iMCU row */
        private int m_rowgroup_ctr;    /* counts row groups received in iMCU row */
        private bool m_suspended;     /* remember if we suspended output */

        /* If using just a strip buffer, this points to the entire set of buffers
        * (we allocate one for each component).  In the full-image case, this
        * points to the currently accessible strips of the virtual arrays.
        */
        private byte[][][] m_buffer = new byte[JpegConstants.MaxComponents][][];

        public JpegCompressorMainController(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Allocate a strip buffer for each component */
            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                m_buffer[ci] = JpegCommonBase.AllocJpegSamples(
                    cinfo.Component_info[ci].Width_in_blocks * JpegConstants.DCTSize,
                    cinfo.Component_info[ci].V_samp_factor * JpegConstants.DCTSize);
            }
        }

        // Initialize for a processing pass.
        public void start_pass(BufferMode pass_mode)
        {
            /* Do nothing in raw-data mode. */
            if (m_cinfo.m_raw_data_in)
                return;

            m_cur_iMCU_row = 0; /* initialize counters */
            m_rowgroup_ctr = 0;
            m_suspended = false;

            if (pass_mode != BufferMode.PassThru)
                throw new Exception("Bogus buffer control mode!");
        }

        /// <summary>
        /// Process some data.
        /// This routine handles the simple pass-through mode,
        /// where we have only a strip buffer.
        /// </summary>
        public void process_data(byte[][] input_buf, ref int in_row_ctr, int in_rows_avail)
        {
            while (m_cur_iMCU_row < m_cinfo.m_total_iMCU_rows)
            {
                /* Read input data if we haven't filled the main buffer yet */
                if (m_rowgroup_ctr < JpegConstants.DCTSize)
                    m_cinfo.m_prep.pre_process_data(input_buf, ref in_row_ctr, in_rows_avail, m_buffer, ref m_rowgroup_ctr, JpegConstants.DCTSize);

                /* If we don't have a full iMCU row buffered, return to application for
                 * more data.  Note that preprocessor will always pad to fill the iMCU row
                 * at the bottom of the image.
                 */
                if (m_rowgroup_ctr != JpegConstants.DCTSize)
                    return;

                /* Send the completed row to the compressor */
                if (!m_cinfo.m_coef.compress_data(m_buffer))
                {
                    /* If compressor did not consume the whole row, then we must need to
                     * suspend processing and return to the application.  In this situation
                     * we pretend we didn't yet consume the last input row; otherwise, if
                     * it happened to be the last row of the image, the application would
                     * think we were done.
                     */
                    if (!m_suspended)
                    {
                        in_row_ctr--;
                        m_suspended = true;
                    }

                    return;
                }

                /* We did finish the row.  Undo our little suspension hack if a previous
                 * call suspended; then mark the main buffer empty.
                 */
                if (m_suspended)
                {
                    in_row_ctr++;
                    m_suspended = false;
                }

                m_rowgroup_ctr = 0;
                m_cur_iMCU_row++;
            }
        }
    }
    #endregion

    #region JpegCompressorMaster
    /// <summary>
    /// Master control module
    /// </summary>
    class JpegCompressorMaster
    {
        private enum c_pass_type
        {
            main_pass,      /* input data, also do first output step */
            huff_opt_pass,  /* Huffman code optimization pass */
            output_pass     /* data output pass */
        }

        private JpegCompressor m_cinfo;

        private bool m_call_pass_startup; /* True if pass_startup must be called */
        private bool m_is_last_pass;      /* True during last pass */

        private c_pass_type m_pass_type;  /* the type of the current pass */

        private int m_pass_number;        /* # of passes completed */
        private int m_total_passes;       /* total # of passes needed */

        private int m_scan_number;        /* current index in scan_info[] */

        public JpegCompressorMaster(JpegCompressor cinfo, bool transcode_only)
        {
            m_cinfo = cinfo;

            if (transcode_only)
            {
                /* no main pass in transcoding */
                if (cinfo.m_optimize_coding)
                    m_pass_type = c_pass_type.huff_opt_pass;
                else
                    m_pass_type = c_pass_type.output_pass;
            }
            else
            {
                /* for normal compression, first pass is always this type: */
                m_pass_type = c_pass_type.main_pass;
            }

            if (cinfo.m_optimize_coding)
                m_total_passes = cinfo.m_num_scans * 2;
            else
                m_total_passes = cinfo.m_num_scans;
        }

        /// <summary>
        /// Per-pass setup.
        /// 
        /// This is called at the beginning of each pass.  We determine which 
        /// modules will be active during this pass and give them appropriate 
        /// start_pass calls. 
        /// We also set is_last_pass to indicate whether any more passes will 
        /// be required.
        /// </summary>
        public void prepare_for_pass()
        {
            switch (m_pass_type)
            {
                case c_pass_type.main_pass:
                    prepare_for_main_pass();
                    break;
                case c_pass_type.huff_opt_pass:
                    if (!prepare_for_huff_opt_pass())
                        break;
                    prepare_for_output_pass();
                    break;
                case c_pass_type.output_pass:
                    prepare_for_output_pass();
                    break;
            }

            m_is_last_pass = (m_pass_number == m_total_passes - 1);
        }

        /// <summary>
        /// Special start-of-pass hook.
        /// 
        /// This is called by jpeg_write_scanlines if call_pass_startup is true.
        /// In single-pass processing, we need this hook because we don't want to
        /// write frame/scan headers during jpeg_start_compress; we want to let the
        /// application write COM markers etc. between jpeg_start_compress and the
        /// jpeg_write_scanlines loop.
        /// In multi-pass processing, this routine is not used.
        /// </summary>
        public void pass_startup()
        {
            m_cinfo.m_master.m_call_pass_startup = false; /* reset flag so call only once */

            m_cinfo.m_marker.write_frame_header();
            m_cinfo.m_marker.write_scan_header();
        }

        /// <summary>
        /// Finish up at end of pass.
        /// </summary>
        public void finish_pass()
        {
            /* The entropy coder always needs an end-of-pass call,
            * either to analyze statistics or to flush its output buffer.
            */
            m_cinfo.m_entropy.finish_pass();

            /* Update state for next pass */
            switch (m_pass_type)
            {
                case c_pass_type.main_pass:
                    /* next pass is either output of scan 0 (after optimization)
                    * or output of scan 1 (if no optimization).
                    */
                    m_pass_type = c_pass_type.output_pass;
                    if (!m_cinfo.m_optimize_coding)
                        m_scan_number++;
                    break;
                case c_pass_type.huff_opt_pass:
                    /* next pass is always output of current scan */
                    m_pass_type = c_pass_type.output_pass;
                    break;
                case c_pass_type.output_pass:
                    /* next pass is either optimization or output of next scan */
                    if (m_cinfo.m_optimize_coding)
                        m_pass_type = c_pass_type.huff_opt_pass;
                    m_scan_number++;
                    break;
            }

            m_pass_number++;
        }

        public bool IsLastPass()
        {
            return m_is_last_pass;
        }

        public bool MustCallPassStartup()
        {
            return m_call_pass_startup;
        }

        private void prepare_for_main_pass()
        {
            /* Initial pass: will collect input data, and do either Huffman
            * optimization or data output for the first scan.
            */
            select_scan_parameters();
            per_scan_setup();

            if (!m_cinfo.m_raw_data_in)
            {
                m_cinfo.m_cconvert.start_pass();
                m_cinfo.m_prep.start_pass(BufferMode.PassThru);
            }

            m_cinfo.m_fdct.start_pass();
            m_cinfo.m_entropy.start_pass(m_cinfo.m_optimize_coding);
            m_cinfo.m_coef.start_pass((m_total_passes > 1 ? BufferMode.SaveAndPass : BufferMode.PassThru));
            m_cinfo.m_main.start_pass(BufferMode.PassThru);

            if (m_cinfo.m_optimize_coding)
            {
                /* No immediate data output; postpone writing frame/scan headers */
                m_call_pass_startup = false;
            }
            else
            {
                /* Will write frame/scan headers at first jpeg_write_scanlines call */
                m_call_pass_startup = true;
            }
        }

        private bool prepare_for_huff_opt_pass()
        {
            /* Do Huffman optimization for a scan after the first one. */
            select_scan_parameters();
            per_scan_setup();

            if (m_cinfo.m_Ss != 0 || m_cinfo.m_Ah == 0)
            {
                m_cinfo.m_entropy.start_pass(true);
                m_cinfo.m_coef.start_pass(BufferMode.CrankDest);
                m_call_pass_startup = false;
                return false;
            }

            /* Special case: Huffman DC refinement scans need no Huffman table
            * and therefore we can skip the optimization pass for them.
            */
            m_pass_type = c_pass_type.output_pass;
            m_pass_number++;
            return true;
        }

        private void prepare_for_output_pass()
        {
            /* Do a data-output pass. */
            /* We need not repeat per-scan setup if prior optimization pass did it. */
            if (!m_cinfo.m_optimize_coding)
            {
                select_scan_parameters();
                per_scan_setup();
            }

            m_cinfo.m_entropy.start_pass(false);
            m_cinfo.m_coef.start_pass(BufferMode.CrankDest);

            /* We emit frame/scan headers now */
            if (m_scan_number == 0)
                m_cinfo.m_marker.write_frame_header();

            m_cinfo.m_marker.write_scan_header();
            m_call_pass_startup = false;
        }

        // Set up the scan parameters for the current scan
        private void select_scan_parameters()
        {
            if (m_cinfo.m_scan_info != null)
            {
                /* Prepare for current scan --- the script is already validated */
                JpegScanInfo scanInfo = m_cinfo.m_scan_info[m_scan_number];

                m_cinfo.m_comps_in_scan = scanInfo.comps_in_scan;
                for (int ci = 0; ci < scanInfo.comps_in_scan; ci++)
                    m_cinfo.m_cur_comp_info[ci] = scanInfo.component_index[ci];

                m_cinfo.m_Ss = scanInfo.Ss;
                m_cinfo.m_Se = scanInfo.Se;
                m_cinfo.m_Ah = scanInfo.Ah;
                m_cinfo.m_Al = scanInfo.Al;
            }
            else
            {
                /* Prepare for single sequential-JPEG scan containing all components */
                if (m_cinfo.m_num_components > JpegConstants.MaxComponentsInScan)
                    throw new Exception(String.Format("Too many color components: {0}, max {1}", m_cinfo.m_num_components, JpegConstants.MaxComponentsInScan));

                m_cinfo.m_comps_in_scan = m_cinfo.m_num_components;
                for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                    m_cinfo.m_cur_comp_info[ci] = ci;

                m_cinfo.m_Ss = 0;
                m_cinfo.m_Se = JpegConstants.DCTSize2 - 1;
                m_cinfo.m_Ah = 0;
                m_cinfo.m_Al = 0;
            }
        }

        /// <summary>
        /// Do computations that are needed before processing a JPEG scan
        /// cinfo.comps_in_scan and cinfo.cur_comp_info[] are already set
        /// </summary>
        private void per_scan_setup()
        {
            if (m_cinfo.m_comps_in_scan == 1)
            {
                /* Non-interleaved (single-component) scan */
                int compIndex = m_cinfo.m_cur_comp_info[0];

                /* Overall image size in MCUs */
                m_cinfo.m_MCUs_per_row = m_cinfo.Component_info[compIndex].Width_in_blocks;
                m_cinfo.m_MCU_rows_in_scan = m_cinfo.Component_info[compIndex].height_in_blocks;

                /* For non-interleaved scan, always one block per MCU */
                m_cinfo.Component_info[compIndex].MCU_width = 1;
                m_cinfo.Component_info[compIndex].MCU_height = 1;
                m_cinfo.Component_info[compIndex].MCU_blocks = 1;
                m_cinfo.Component_info[compIndex].MCU_sample_width = JpegConstants.DCTSize;
                m_cinfo.Component_info[compIndex].last_col_width = 1;

                /* For non-interleaved scans, it is convenient to define last_row_height
                * as the number of block rows present in the last iMCU row.
                */
                int tmp = m_cinfo.Component_info[compIndex].height_in_blocks % m_cinfo.Component_info[compIndex].V_samp_factor;
                if (tmp == 0)
                    tmp = m_cinfo.Component_info[compIndex].V_samp_factor;
                m_cinfo.Component_info[compIndex].last_row_height = tmp;

                /* Prepare array describing MCU composition */
                m_cinfo.m_blocks_in_MCU = 1;
                m_cinfo.m_MCU_membership[0] = 0;
            }
            else
            {
                /* Interleaved (multi-component) scan */
                if (m_cinfo.m_comps_in_scan <= 0 || m_cinfo.m_comps_in_scan > JpegConstants.MaxComponentsInScan)
                    throw new Exception(String.Format("Too many color components: {0}, max {1}", m_cinfo.m_comps_in_scan, JpegConstants.MaxComponentsInScan));

                /* Overall image size in MCUs */
                m_cinfo.m_MCUs_per_row = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width, m_cinfo.m_max_h_samp_factor * JpegConstants.DCTSize);

                m_cinfo.m_MCU_rows_in_scan = JpegUtils.jdiv_round_up(m_cinfo.m_image_height,
                    m_cinfo.m_max_v_samp_factor * JpegConstants.DCTSize);

                m_cinfo.m_blocks_in_MCU = 0;

                for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                {
                    int compIndex = m_cinfo.m_cur_comp_info[ci];

                    /* Sampling factors give # of blocks of component in each MCU */
                    m_cinfo.Component_info[compIndex].MCU_width = m_cinfo.Component_info[compIndex].H_samp_factor;
                    m_cinfo.Component_info[compIndex].MCU_height = m_cinfo.Component_info[compIndex].V_samp_factor;
                    m_cinfo.Component_info[compIndex].MCU_blocks = m_cinfo.Component_info[compIndex].MCU_width * m_cinfo.Component_info[compIndex].MCU_height;
                    m_cinfo.Component_info[compIndex].MCU_sample_width = m_cinfo.Component_info[compIndex].MCU_width * JpegConstants.DCTSize;

                    /* Figure number of non-dummy blocks in last MCU column & row */
                    int tmp = m_cinfo.Component_info[compIndex].Width_in_blocks % m_cinfo.Component_info[compIndex].MCU_width;
                    if (tmp == 0)
                        tmp = m_cinfo.Component_info[compIndex].MCU_width;
                    m_cinfo.Component_info[compIndex].last_col_width = tmp;

                    tmp = m_cinfo.Component_info[compIndex].height_in_blocks % m_cinfo.Component_info[compIndex].MCU_height;
                    if (tmp == 0)
                        tmp = m_cinfo.Component_info[compIndex].MCU_height;
                    m_cinfo.Component_info[compIndex].last_row_height = tmp;

                    /* Prepare array describing MCU composition */
                    int mcublks = m_cinfo.Component_info[compIndex].MCU_blocks;
                    if (m_cinfo.m_blocks_in_MCU + mcublks > JpegConstants.CompressorMaxBlocksInMCU)
                        throw new Exception("Sampling factors too large for interleaved scan");

                    while (mcublks-- > 0)
                        m_cinfo.m_MCU_membership[m_cinfo.m_blocks_in_MCU++] = ci;
                }
            }

            /* Convert restart specified in rows to actual MCU count. */
            /* Note that count must fit in 16 bits, so we provide limiting. */
            if (m_cinfo.m_restart_in_rows > 0)
            {
                int nominal = m_cinfo.m_restart_in_rows * m_cinfo.m_MCUs_per_row;
                m_cinfo.m_restart_interval = Math.Min(nominal, 65535);
            }
        }
    }
    #endregion

    #region JpegCompressorPrepController
    /// <summary>
    /// Compression preprocessing (downsampling input buffer control).
    /// 
    /// For the simple (no-context-row) case, we just need to buffer one
    /// row group's worth of pixels for the downsampling step.  At the bottom of
    /// the image, we pad to a full row group by replicating the last pixel row.
    /// The downsampler's last output row is then replicated if needed to pad
    /// out to a full iMCU row.
    /// 
    /// When providing context rows, we must buffer three row groups' worth of
    /// pixels.  Three row groups are physically allocated, but the row pointer
    /// arrays are made five row groups high, with the extra pointers above and
    /// below "wrapping around" to point to the last and first real row groups.
    /// This allows the downsampler to access the proper context rows.
    /// At the top and bottom of the image, we create dummy context rows by
    /// copying the first or last real pixel row.  This copying could be avoided
    /// by pointer hacking as is done in jdmainct.c, but it doesn't seem worth the
    /// trouble on the compression side.
    /// </summary>
    class JpegCompressorPrepController
    {
        private JpegCompressor m_cinfo;

        /* Downsampling input buffer.  This buffer holds color-converted data
        * until we have enough to do a downsample step.
        */
        private byte[][][] m_color_buf = new byte[JpegConstants.MaxComponents][][];
        private int m_colorBufRowsOffset;

        private int m_rows_to_go;  /* counts rows remaining in source image */
        private int m_next_buf_row;       /* index of next row to store in color_buf */

        private int m_this_row_group;     /* starting row index of group to process */
        private int m_next_buf_stop;      /* downsample when we reach this index */

        public JpegCompressorPrepController(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Allocate the color conversion buffer.
            * We make the buffer wide enough to allow the downsampler to edge-expand
            * horizontally within the buffer, if it so chooses.
            */
            if (cinfo.m_downsample.NeedContextRows())
            {
                /* Set up to provide context rows */
                create_context_buffer();
            }
            else
            {
                /* No context, just make it tall enough for one row group */
                for (int ci = 0; ci < cinfo.m_num_components; ci++)
                {
                    m_colorBufRowsOffset = 0;
                    m_color_buf[ci] = JpegCompressor.AllocJpegSamples(
                        (cinfo.Component_info[ci].Width_in_blocks * JpegConstants.DCTSize * cinfo.m_max_h_samp_factor) / cinfo.Component_info[ci].H_samp_factor,
                        cinfo.m_max_v_samp_factor);
                }
            }
        }

        /// <summary>
        /// Initialize for a processing pass.
        /// </summary>
        public void start_pass(BufferMode pass_mode)
        {
            if (pass_mode != BufferMode.PassThru)
                throw new Exception("Bogus buffer control mode!");

            /* Initialize total-height counter for detecting bottom of image */
            m_rows_to_go = m_cinfo.m_image_height;

            /* Mark the conversion buffer empty */
            m_next_buf_row = 0;

            /* Preset additional state variables for context mode.
             * These aren't used in non-context mode, so we needn't test which mode.
             */
            m_this_row_group = 0;

            /* Set next_buf_stop to stop after two row groups have been read in. */
            m_next_buf_stop = 2 * m_cinfo.m_max_v_samp_factor;
        }

        public void pre_process_data(byte[][] input_buf, ref int in_row_ctr, int in_rows_avail, byte[][][] output_buf, ref int out_row_group_ctr, int out_row_groups_avail)
        {
            if (m_cinfo.m_downsample.NeedContextRows())
                pre_process_context(input_buf, ref in_row_ctr, in_rows_avail, output_buf, ref out_row_group_ctr, out_row_groups_avail);
            else
                pre_process_WithoutContext(input_buf, ref in_row_ctr, in_rows_avail, output_buf, ref out_row_group_ctr, out_row_groups_avail);
        }

        /// <summary>
        /// Create the wrapped-around downsampling input buffer needed for context mode.
        /// </summary>
        private void create_context_buffer()
        {
            int rgroup_height = m_cinfo.m_max_v_samp_factor;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                int samplesPerRow = (m_cinfo.Component_info[ci].Width_in_blocks * JpegConstants.DCTSize * m_cinfo.m_max_h_samp_factor) / m_cinfo.Component_info[ci].H_samp_factor;

                byte[][] fake_buffer = new byte[5 * rgroup_height][];
                for (int i = 1; i < 4 * rgroup_height; i++)
                    fake_buffer[i] = new byte[samplesPerRow];

                /* Allocate the actual buffer space (3 row groups) for this component.
                 * We make the buffer wide enough to allow the downsampler to edge-expand
                 * horizontally within the buffer, if it so chooses.
                 */
                byte[][] true_buffer = JpegCommonBase.AllocJpegSamples(samplesPerRow, 3 * rgroup_height);

                /* Copy true buffer row pointers into the middle of the fake row array */
                for (int i = 0; i < 3 * rgroup_height; i++)
                    fake_buffer[rgroup_height + i] = true_buffer[i];

                /* Fill in the above and below wraparound pointers */
                for (int i = 0; i < rgroup_height; i++)
                {
                    fake_buffer[i] = true_buffer[2 * rgroup_height + i];
                    fake_buffer[4 * rgroup_height + i] = true_buffer[i];
                }

                m_color_buf[ci] = fake_buffer;
                m_colorBufRowsOffset = rgroup_height;
            }
        }

        /// <summary>
        /// Process some data in the simple no-context case.
        /// 
        /// Preprocessor output data is counted in "row groups".  A row group
        /// is defined to be v_samp_factor sample rows of each component.
        /// Downsampling will produce this much data from each max_v_samp_factor
        /// input rows.
        /// </summary>
        private void pre_process_WithoutContext(byte[][] input_buf, ref int in_row_ctr, int in_rows_avail, byte[][][] output_buf, ref int out_row_group_ctr, int out_row_groups_avail)
        {
            while (in_row_ctr < in_rows_avail && out_row_group_ctr < out_row_groups_avail)
            {
                /* Do color conversion to fill the conversion buffer. */
                int inrows = in_rows_avail - in_row_ctr;
                int numrows = m_cinfo.m_max_v_samp_factor - m_next_buf_row;
                numrows = Math.Min(numrows, inrows);
                m_cinfo.m_cconvert.color_convert(input_buf, in_row_ctr, m_color_buf, m_colorBufRowsOffset + m_next_buf_row, numrows);
                in_row_ctr += numrows;
                m_next_buf_row += numrows;
                m_rows_to_go -= numrows;

                /* If at bottom of image, pad to fill the conversion buffer. */
                if (m_rows_to_go == 0 && m_next_buf_row < m_cinfo.m_max_v_samp_factor)
                {
                    for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                        expand_bottom_edge(m_color_buf[ci], m_colorBufRowsOffset, m_cinfo.m_image_width, m_next_buf_row, m_cinfo.m_max_v_samp_factor);

                    m_next_buf_row = m_cinfo.m_max_v_samp_factor;
                }

                /* If we've filled the conversion buffer, empty it. */
                if (m_next_buf_row == m_cinfo.m_max_v_samp_factor)
                {
                    m_cinfo.m_downsample.downsample(m_color_buf, m_colorBufRowsOffset, output_buf, out_row_group_ctr);
                    m_next_buf_row = 0;
                    out_row_group_ctr++;
                }

                /* If at bottom of image, pad the output to a full iMCU height.
                 * Note we assume the caller is providing a one-iMCU-height output buffer!
                 */
                if (m_rows_to_go == 0 && out_row_group_ctr < out_row_groups_avail)
                {
                    for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                    {
                        JpegComponent componentInfo = m_cinfo.Component_info[ci];
                        expand_bottom_edge(output_buf[ci], 0, componentInfo.Width_in_blocks * JpegConstants.DCTSize,
                            out_row_group_ctr * componentInfo.V_samp_factor,
                            out_row_groups_avail * componentInfo.V_samp_factor);
                    }

                    out_row_group_ctr = out_row_groups_avail;
                    break;          /* can exit outer loop without test */
                }
            }
        }

        /// <summary>
        /// Process some data in the context case.
        /// </summary>
        private void pre_process_context(byte[][] input_buf, ref int in_row_ctr, int in_rows_avail, byte[][][] output_buf, ref int out_row_group_ctr, int out_row_groups_avail)
        {
            while (out_row_group_ctr < out_row_groups_avail)
            {
                if (in_row_ctr < in_rows_avail)
                {
                    /* Do color conversion to fill the conversion buffer. */
                    int inrows = in_rows_avail - in_row_ctr;
                    int numrows = m_next_buf_stop - m_next_buf_row;
                    numrows = Math.Min(numrows, inrows);
                    m_cinfo.m_cconvert.color_convert(input_buf, in_row_ctr, m_color_buf, m_colorBufRowsOffset + m_next_buf_row, numrows);

                    /* Pad at top of image, if first time through */
                    if (m_rows_to_go == m_cinfo.m_image_height)
                    {
                        for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                        {
                            for (int row = 1; row <= m_cinfo.m_max_v_samp_factor; row++)
                                JpegUtils.jcopy_sample_rows(m_color_buf[ci], m_colorBufRowsOffset, m_color_buf[ci], m_colorBufRowsOffset - row, 1, m_cinfo.m_image_width);
                        }
                    }

                    in_row_ctr += numrows;
                    m_next_buf_row += numrows;
                    m_rows_to_go -= numrows;
                }
                else
                {
                    /* Return for more data, unless we are at the bottom of the image. */
                    if (m_rows_to_go != 0)
                        break;

                    /* When at bottom of image, pad to fill the conversion buffer. */
                    if (m_next_buf_row < m_next_buf_stop)
                    {
                        for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                            expand_bottom_edge(m_color_buf[ci], m_colorBufRowsOffset, m_cinfo.m_image_width, m_next_buf_row, m_next_buf_stop);

                        m_next_buf_row = m_next_buf_stop;
                    }
                }

                /* If we've gotten enough data, downsample a row group. */
                if (m_next_buf_row == m_next_buf_stop)
                {
                    m_cinfo.m_downsample.downsample(m_color_buf, m_colorBufRowsOffset + m_this_row_group, output_buf, out_row_group_ctr);
                    out_row_group_ctr++;

                    /* Advance pointers with wraparound as necessary. */
                    m_this_row_group += m_cinfo.m_max_v_samp_factor;
                    int buf_height = m_cinfo.m_max_v_samp_factor * 3;

                    if (m_this_row_group >= buf_height)
                        m_this_row_group = 0;

                    if (m_next_buf_row >= buf_height)
                        m_next_buf_row = 0;

                    m_next_buf_stop = m_next_buf_row + m_cinfo.m_max_v_samp_factor;
                }
            }
        }

        /// <summary>
        /// Expand an image vertically from height input_rows to height output_rows,
        /// by duplicating the bottom row.
        /// </summary>
        private static void expand_bottom_edge(byte[][] image_data, int rowsOffset, int num_cols, int input_rows, int output_rows)
        {
            for (int row = input_rows; row < output_rows; row++)
                JpegUtils.jcopy_sample_rows(image_data, rowsOffset + input_rows - 1, image_data, row, 1, num_cols);
        }
    }
    #endregion

    #region JpegDecompressor
    /// <summary>
    /// JPEG decompression routine.
    /// </summary>
    /// <seealso cref="JpegCompressor"/>
    public class JpegDecompressor : JpegCommonBase
    {
        /// <summary>
        /// The delegate for application-supplied marker processing methods.
        /// </summary>
        /// <param name="cinfo">Decompressor.</param>
        /// <returns>Return <c>true</c> to indicate success. <c>false</c> should be returned only 
        /// if you are using a suspending data source and it tells you to suspend.
        /// </returns>
        /// <remarks>Although the marker code is not explicitly passed, the routine can find it 
        /// in the <see cref="JpegDecompressor.Unread_marker"/>. At the time of call, 
        /// the marker proper has been read from the data source module. The processor routine 
        /// is responsible for reading the marker length word and the remaining parameter bytes, if any.
        /// </remarks>
        public delegate bool jpeg_marker_parser_method(JpegDecompressor cinfo);

        /* Source of compressed data */
        internal Jpeg_Source m_src;

        internal int m_image_width; /* nominal image width (from SOF marker) */
        internal int m_image_height;    /* nominal image height */
        internal int m_num_components;     /* # of color components in JPEG image */
        internal ColorSpace m_jpeg_color_space; /* colorspace of JPEG image */

        internal ColorSpace m_out_color_space; /* colorspace for output */
        internal int m_scale_num;
        internal int m_scale_denom; /* fraction by which to scale image */
        internal bool m_buffered_image;    /* true=multiple output passes */
        internal bool m_raw_data_out;      /* true=downsampled data wanted */
        internal DCTMethod m_dct_method;    /* IDCT algorithm selector */
        internal bool m_do_fancy_upsampling;   /* true=apply fancy up-sampling */
        internal bool m_do_block_smoothing;    /* true=apply inter-block smoothing */
        internal bool m_quantize_colors;   /* true=colormapped output wanted */
        internal DitherMode m_dither_mode;  /* type of color dithering to use */
        internal bool m_two_pass_quantize; /* true=use two-pass color quantization */
        internal int m_desired_number_of_colors;   /* max # colors to use in created colormap */
        internal bool m_enable_1pass_quant;    /* enable future use of 1-pass quantizer */
        internal bool m_enable_external_quant;/* enable future use of external colormap */
        internal bool m_enable_2pass_quant;    /* enable future use of 2-pass quantizer */

        internal int m_output_width;    /* scaled image width */
        internal int m_output_height;   /* scaled image height */
        internal int m_out_color_components;   /* # of color components in out_color_space */
        /* # of color components returned
         * output_components is 1 (a colormap index) when quantizing colors;
         * otherwise it equals out_color_components.
         */
        internal int m_output_components;

        internal int m_rec_outbuf_height;  /* min recommended height of scanline buffer */

        internal int m_actual_number_of_colors;    /* number of entries in use */
        internal byte[][] m_colormap;     /* The color map as a 2-D pixel array */

        internal int m_output_scanline; /* 0 .. output_height-1  */

        internal int m_input_scan_number;  /* Number of SOS markers seen so far */
        internal int m_input_iMCU_row;  /* Number of iMCU rows completed */

        internal int m_output_scan_number; /* Nominal scan number being displayed */
        internal int m_output_iMCU_row; /* Number of iMCU rows read */

        internal int[][] m_coef_bits; /* -1 or current Al value for each coef */

        /* Internal JPEG parameters --- the application usually need not look at
         * these fields.  Note that the decompressor output side may not use
         * any parameters that can change between scans.
         */

        /* Quantization and Huffman tables are carried forward across input
         * data-streams when processing abbreviated JPEG data-streams.
         */

        internal JpegQuantizationTable[] m_quant_tbl_ptrs = new JpegQuantizationTable[JpegConstants.NumberOfQuantTables];
        /* ptrs to coefficient quantization tables, or null if not defined */

        internal JpegHuffmanTable[] m_dc_huff_tbl_ptrs = new JpegHuffmanTable[JpegConstants.NumberOfHuffmanTables];
        internal JpegHuffmanTable[] m_ac_huff_tbl_ptrs = new JpegHuffmanTable[JpegConstants.NumberOfHuffmanTables];
        /* ptrs to Huffman coding tables, or null if not defined */

        /* These parameters are never carried across data-streams, since they
         * are given in SOF/SOS markers or defined to be reset by SOI.
         */

        internal int m_data_precision;     /* bits of precision in image data */

        /* m_comp_info[i] describes component that appears i'th in SOF */
        private JpegComponent[] m_comp_info;

        internal bool m_progressive_mode;  /* true if SOFn specifies progressive mode */

        internal int m_restart_interval; /* MCUs per restart interval, or 0 for no restart */

        /* These fields record data obtained from optional markers recognized by
         * the JPEG library.
         */
        internal bool m_saw_JFIF_marker;   /* true iff a JFIF APP0 marker was found */
        /* Data copied from JFIF marker; only valid if saw_JFIF_marker is true: */
        internal byte m_JFIF_major_version;   /* JFIF version number */
        internal byte m_JFIF_minor_version;

        internal DensityUnit m_density_unit;     /* JFIF code for pixel size units */
        internal short m_X_density;       /* Horizontal pixel density */
        internal short m_Y_density;       /* Vertical pixel density */

        internal bool m_saw_Adobe_marker;  /* true iff an Adobe APP14 marker was found */
        internal byte m_Adobe_transform;  /* Color transform code from Adobe marker */

        internal bool m_CCIR601_sampling;  /* true=first samples are co-sited */

        internal List<JpegMarker> m_marker_list; /* Head of list of saved markers */

        /* Remaining fields are known throughout decompressor, but generally
         * should not be touched by a surrounding application.
         */

        /*
         * These fields are computed during decompression startup
         */
        internal int m_max_h_samp_factor;  /* largest h_samp_factor */
        internal int m_max_v_samp_factor;  /* largest v_samp_factor */

        internal int m_min_DCT_scaled_size;    /* smallest DCT_scaled_size of any component */

        internal int m_total_iMCU_rows; /* # of iMCU rows in image */
        /* The coefficient controller's input and output progress is measured in
         * units of "iMCU" (interleaved MCU) rows.  These are the same as MCU rows
         * in fully interleaved JPEG scans, but are used whether the scan is
         * interleaved or not.  We define an iMCU row as v_samp_factor DCT block
         * rows of each component.  Therefore, the IDCT output contains
         * v_samp_factor*DCT_scaled_size sample rows of a component per iMCU row.
         */

        internal byte[] m_sample_range_limit; /* table for fast range-limiting */
        internal int m_sampleRangeLimitOffset;

        /*
         * These fields are valid during any one scan.
         * They describe the components and MCUs actually appearing in the scan.
         * Note that the decompressor output side must not use these fields.
         */
        internal int m_comps_in_scan;      /* # of JPEG components in this scan */
        internal int[] m_cur_comp_info = new int[JpegConstants.MaxComponentsInScan];
        /* *cur_comp_info[i] describes component that appears i'th in SOS */

        internal int m_MCUs_per_row;    /* # of MCUs across the image */
        internal int m_MCU_rows_in_scan;    /* # of MCU rows in the image */

        internal int m_blocks_in_MCU;      /* # of DCT blocks per MCU */
        internal int[] m_MCU_membership = new int[JpegConstants.DecompressorMaxBlocksInMCU];
        /* MCU_membership[i] is index in cur_comp_info of component owning */
        /* i'th block in an MCU */

        /* progressive JPEG parameters for scan */
        internal int m_Ss;
        internal int m_Se;
        internal int m_Ah;
        internal int m_Al;

        /* This field is shared between entropy decoder and marker parser.
         * It is either zero or the code of a JPEG marker that has been
         * read from the data source, but has not yet been processed.
         */
        internal int m_unread_marker;

        /*
         * Links to decompression sub-objects (methods, private variables of modules)
         */
        internal JpegDecompressorMaster m_master;
        internal JpegDecompressorMainController m_main;
        internal JpegDecompressorCoefController m_coef;
        internal JpegDecompressorPostController m_post;
        internal JpegInputController m_inputctl;
        internal JpegMarkerReader m_marker;
        internal JpegEntropyDecoder m_entropy;
        internal JpegInverseDCT m_idct;
        internal JpegUpsampler m_upsample;
        internal ColorDeconverter m_cconvert;
        internal ColorQuantizer m_cquantize;

        /// <summary>
        /// Initializes a new instance of the <see cref="JpegDecompressor"/> class.
        /// </summary>
        /// <seealso cref="JpegCompressor"/>
        public JpegDecompressor()
            : base()
        {
            initialize();
        }

        /// <summary>
        /// Retrieves <c>true</c> because this is a decompressor.
        /// </summary>
        /// <value><c>true</c></value>
        public override bool IsDecompressor
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the source for decompression.
        /// </summary>
        /// <value>The source for decompression.</value>
        public LibJpeg.Jpeg_Source Src
        {
            get { return m_src; }
            set { m_src = value; }
        }

        /* Basic description of image --- filled in by jpeg_read_header(). */
        /* Application may inspect these values to decide how to process image. */

        /// <summary>
        /// Gets the width of image, set by <see cref="JpegDecompressor.jpeg_read_header"/>
        /// </summary>
        /// <value>The width of image.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Image_width
        {
            get { return m_image_width; }
        }

        /// <summary>
        /// Gets the height of image, set by <see cref="JpegDecompressor.jpeg_read_header"/>
        /// </summary>
        /// <value>The height of image.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Image_height
        {
            get { return m_image_height; }
        }

        /// <summary>
        /// Gets the number of color components in JPEG image.
        /// </summary>
        /// <value>The number of color components.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Num_components
        {
            get { return m_num_components; }
        }

        /// <summary>
        /// Gets or sets the colorspace of JPEG image.
        /// </summary>
        /// <value>The colorspace of JPEG image.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.ColorSpace Jpeg_color_space
        {
            get { return m_jpeg_color_space; }
            set { m_jpeg_color_space = value; }
        }

        /// <summary>
        /// Gets the list of loaded special markers.
        /// </summary>
        /// <remarks>All the special markers in the file appear in this list, in order of 
        /// their occurrence in the file (but omitting any markers of types you didn't ask for)
        /// </remarks>
        /// <value>The list of loaded special markers.</value>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public ReadOnlyCollection<JpegMarker> Marker_list
        {
            get
            {
                return m_marker_list.AsReadOnly();
            }
        }

        /* Decompression processing parameters --- these fields must be set before
         * calling jpeg_start_decompress().  Note that jpeg_read_header() initializes
         * them to default values.
         */

        /// <summary>
        /// Gets or sets the output color space.
        /// </summary>
        /// <value>The output color space.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.ColorSpace Out_color_space
        {
            get { return m_out_color_space; }
            set { m_out_color_space = value; }
        }

        /// <summary>
        /// Gets or sets the numerator of the fraction of image scaling.
        /// </summary>
        /// <value>Scale the image by the fraction Scale_num/<see cref="JpegDecompressor.Scale_denom">Scale_denom</see>. 
        /// Default is 1/1, or no scaling. Currently, the only supported scaling ratios are 1/1, 1/2, 1/4, and 1/8.
        /// (The library design allows for arbitrary scaling ratios but this is not likely to be implemented any time soon.)
        /// </value>
        /// <remarks>Smaller scaling ratios permit significantly faster decoding since fewer pixels 
        /// need to be processed and a simpler <see cref="DCTMethod">DCT method</see> can be used.</remarks>
        /// <seealso cref="JpegDecompressor.Scale_denom"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Scale_num
        {
            get { return m_scale_num; }
            set { m_scale_num = value; }
        }

        /// <summary>
        /// Gets or sets the denominator of the fraction of image scaling.
        /// </summary>
        /// <value>Scale the image by the fraction <see cref="JpegDecompressor.Scale_num">Scale_num</see>/Scale_denom. 
        /// Default is 1/1, or no scaling. Currently, the only supported scaling ratios are 1/1, 1/2, 1/4, and 1/8.
        /// (The library design allows for arbitrary scaling ratios but this is not likely to be implemented any time soon.)
        /// </value>
        /// <remarks>Smaller scaling ratios permit significantly faster decoding since fewer pixels 
        /// need to be processed and a simpler <see cref="DCTMethod">DCT method</see> can be used.</remarks>
        /// <seealso cref="JpegDecompressor.Scale_num"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Scale_denom
        {
            get { return m_scale_denom; }
            set { m_scale_denom = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use buffered-image mode.
        /// </summary>
        /// <value><c>true</c> if buffered-image mode is turned on; otherwise, <c>false</c>.</value>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Buffered_image
        {
            get { return m_buffered_image; }
            set { m_buffered_image = value; }
        }

        /// <summary>
        /// Enable or disable raw data output.
        /// </summary>
        /// <value><c>true</c> if raw data output is enabled; otherwise, <c>false</c>.</value>
        /// <remarks>Default value: <c>false</c><br/>
        /// Set this to true before <see cref="JpegDecompressor.jpeg_start_decompress"/> 
        /// if you need to obtain raw data output.
        /// </remarks>
        /// <seealso cref="jpeg_read_raw_data"/>
        public bool Raw_data_out
        {
            get { return m_raw_data_out; }
            set { m_raw_data_out = value; }
        }

        /// <summary>
        /// Gets or sets the algorithm used for the DCT step.
        /// </summary>
        /// <value>The algorithm used for the DCT step.</value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.DCTMethod Dct_method
        {
            get { return m_dct_method; }
            set { m_dct_method = value; }
        }

        /// <summary>
        /// Enable or disable up-sampling of chroma components.
        /// </summary>
        /// <value>If <c>true</c>, do careful up-sampling of chroma components. 
        /// If <c>false</c>, a faster but sloppier method is used. 
        /// The visual impact of the sloppier method is often very small.
        /// </value>
        /// <remarks>Default value: <c>true</c></remarks>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Do_fancy_upsampling
        {
            get { return m_do_fancy_upsampling; }
            set { m_do_fancy_upsampling = value; }
        }

        /// <summary>
        /// Apply inter-block smoothing in early stages of decoding progressive JPEG files.
        /// </summary>
        /// <value>If <c>true</c>, inter-block smoothing is applied in early stages of decoding progressive JPEG files; 
        /// if <c>false</c>, not. Early progression stages look "fuzzy" with smoothing, "blocky" without.</value>
        /// <remarks>Default value: <c>true</c><br/>
        /// In any case, block smoothing ceases to be applied after the first few AC coefficients are 
        /// known to full accuracy, so it is relevant only when using 
        /// <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see> for progressive images.
        /// </remarks>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Do_block_smoothing
        {
            get { return m_do_block_smoothing; }
            set { m_do_block_smoothing = value; }
        }

        /// <summary>
        /// Colors quantization.
        /// </summary>
        /// <value>If set <c>true</c>, colormapped output will be delivered.<br/>
        /// Default value: <c>false</c>, meaning that full-color output will be delivered.
        /// </value>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Quantize_colors
        {
            get { return m_quantize_colors; }
            set { m_quantize_colors = value; }
        }

        /* the following are ignored if not quantize_colors: */

        /// <summary>
        /// Selects color dithering method.
        /// </summary>
        /// <value>Default value: <see cref="DitherMode.FloydStein"/>.</value>
        /// <remarks>Ignored if <see cref="JpegDecompressor.Quantize_colors"/> is <c>false</c>.<br/>
        /// At present, ordered dither is implemented only in the single-pass, standard-colormap case. 
        /// If you ask for ordered dither when <see cref="JpegDecompressor.Two_pass_quantize"/> is <c>true</c>
        /// or when you supply an external color map, you'll get F-S dithering.
        /// </remarks>
        /// <seealso cref="JpegDecompressor.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public LibJpeg.DitherMode Dither_mode
        {
            get { return m_dither_mode; }
            set { m_dither_mode = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use two-pass color quantization.
        /// </summary>
        /// <value>If <c>true</c>, an extra pass over the image is made to select a custom color map for the image.
        /// This usually looks a lot better than the one-size-fits-all colormap that is used otherwise.
        /// Ignored when the application supplies its own color map.<br/>
        /// 
        /// Default value: <c>true</c>
        /// </value>
        /// <remarks>Ignored if <see cref="JpegDecompressor.Quantize_colors"/> is <c>false</c>.<br/>
        /// </remarks>
        /// <seealso cref="JpegDecompressor.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public bool Two_pass_quantize
        {
            get { return m_two_pass_quantize; }
            set { m_two_pass_quantize = value; }
        }

        /// <summary>
        /// Maximum number of colors to use in generating a library-supplied color map.
        /// </summary>
        /// <value>Default value: 256.</value>
        /// <remarks>Ignored if <see cref="JpegDecompressor.Quantize_colors"/> is <c>false</c>.<br/>
        /// The actual number of colors is returned in a <see cref="JpegDecompressor.Actual_number_of_colors"/>.
        /// </remarks>
        /// <seealso cref="JpegDecompressor.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Desired_number_of_colors
        {
            get { return m_desired_number_of_colors; }
            set { m_desired_number_of_colors = value; }
        }

        /* these are significant only in buffered-image mode: */

        /// <summary>
        /// Enable future use of 1-pass quantizer.
        /// </summary>
        /// <value>Default value: <c>false</c></value>
        /// <remarks>Significant only in buffered-image mode.</remarks>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Enable_1pass_quant
        {
            get { return m_enable_1pass_quant; }
            set { m_enable_1pass_quant = value; }
        }

        /// <summary>
        /// Enable future use of external colormap.
        /// </summary>
        /// <value>Default value: <c>false</c></value>
        /// <remarks>Significant only in buffered-image mode.</remarks>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Enable_external_quant
        {
            get { return m_enable_external_quant; }
            set { m_enable_external_quant = value; }
        }

        /// <summary>
        /// Enable future use of 2-pass quantizer.
        /// </summary>
        /// <value>Default value: <c>false</c></value>
        /// <remarks>Significant only in buffered-image mode.</remarks>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool Enable_2pass_quant
        {
            get { return m_enable_2pass_quant; }
            set { m_enable_2pass_quant = value; }
        }

        /* Description of actual output image that will be returned to application.
         * These fields are computed by jpeg_start_decompress().
         * You can also use jpeg_calc_output_dimensions() to determine these values
         * in advance of calling jpeg_start_decompress().
         */

        /// <summary>
        /// Gets the actual width of output image.
        /// </summary>
        /// <value>The width of output image.</value>
        /// <remarks>Computed by <see cref="JpegDecompressor.jpeg_start_decompress"/>.
        /// You can also use <see cref="JpegDecompressor.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="JpegDecompressor.jpeg_start_decompress"/>.</remarks>
        /// <seealso cref="JpegDecompressor.Output_height"/>
        public int Output_width
        {
            get { return m_output_width; }
        }

        /// <summary>
        /// Gets the actual height of output image.
        /// </summary>
        /// <value>The height of output image.</value>
        /// <remarks>Computed by <see cref="JpegDecompressor.jpeg_start_decompress"/>.
        /// You can also use <see cref="JpegDecompressor.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="JpegDecompressor.jpeg_start_decompress"/>.</remarks>
        /// <seealso cref="JpegDecompressor.Output_width"/>
        public int Output_height
        {
            get { return m_output_height; }
        }

        /// <summary>
        /// Gets the number of color components in <see cref="JpegDecompressor.Out_color_space"/>.
        /// </summary>
        /// <remarks>Computed by <see cref="JpegDecompressor.jpeg_start_decompress"/>.
        /// You can also use <see cref="JpegDecompressor.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="JpegDecompressor.jpeg_start_decompress"/>.</remarks>
        /// <value>The number of color components.</value>
        /// <seealso cref="JpegDecompressor.Out_color_space"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Out_color_components
        {
            get { return m_out_color_components; }
        }

        /// <summary>
        /// Gets the number of color components returned.
        /// </summary>
        /// <remarks>Computed by <see cref="JpegDecompressor.jpeg_start_decompress"/>.
        /// You can also use <see cref="JpegDecompressor.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="JpegDecompressor.jpeg_start_decompress"/>.</remarks>
        /// <value>When <see cref="JpegDecompressor.Quantize_colors">quantizing colors</see>, 
        /// <c>Output_components</c> is 1, indicating a single color map index per pixel. 
        /// Otherwise it equals to <see cref="JpegDecompressor.Out_color_components"/>.
        /// </value>
        /// <seealso cref="JpegDecompressor.Out_color_space"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Output_components
        {
            get { return m_output_components; }
        }

        /// <summary>
        /// Gets the recommended height of scanline buffer.
        /// </summary>
        /// <value>In high-quality modes, <c>Rec_outbuf_height</c> is always 1, but some faster, 
        /// lower-quality modes set it to larger values (typically 2 to 4).</value>
        /// <remarks>Computed by <see cref="JpegDecompressor.jpeg_start_decompress"/>.
        /// You can also use <see cref="JpegDecompressor.jpeg_calc_output_dimensions"/> to determine this value
        /// in advance of calling <see cref="JpegDecompressor.jpeg_start_decompress"/>.<br/>
        /// 
        /// <c>Rec_outbuf_height</c> is the recommended minimum height (in scanlines) 
        /// of the buffer passed to <see cref="JpegDecompressor.jpeg_read_scanlines"/>.
        /// If the buffer is smaller, the library will still work, but time will be wasted due 
        /// to unnecessary data copying. If you are going to ask for a high-speed processing mode, 
        /// you may as well go to the trouble of honoring <c>Rec_outbuf_height</c> so as to avoid data copying.
        /// (An output buffer larger than <c>Rec_outbuf_height</c> lines is OK, but won't provide 
        /// any material speed improvement over that height.)
        /// </remarks>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Rec_outbuf_height
        {
            get { return m_rec_outbuf_height; }
        }

        /* When quantizing colors, the output colormap is described by these fields.
         * The application can supply a colormap by setting colormap non-null before
         * calling jpeg_start_decompress; otherwise a colormap is created during
         * jpeg_start_decompress or jpeg_start_output.
         * The map has out_color_components rows and actual_number_of_colors columns.
         */

        /// <summary>
        /// The number of colors in the color map.
        /// </summary>
        /// <value>The number of colors in the color map.</value>
        /// <seealso cref="JpegDecompressor.Colormap"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public int Actual_number_of_colors
        {
            get { return m_actual_number_of_colors; }
            set { m_actual_number_of_colors = value; }
        }

        /// <summary>
        /// The color map, represented as a 2-D pixel array of <see cref="JpegDecompressor.Out_color_components"/> rows 
        /// and <see cref="JpegDecompressor.Actual_number_of_colors"/> columns.
        /// </summary>
        /// <value>Colormap is set to <c>null</c> by <see cref="JpegDecompressor.jpeg_read_header"/>.
        /// The application can supply a color map by setting <c>Colormap</c> non-null and setting 
        /// <see cref="JpegDecompressor.Actual_number_of_colors"/> to the map size.
        /// </value>
        /// <remarks>Ignored if not quantizing.<br/>
        /// Implementation restriction: at present, an externally supplied <c>Colormap</c>
        /// is only accepted for 3-component output color spaces.
        /// </remarks>
        /// <seealso cref="JpegDecompressor.Actual_number_of_colors"/>
        /// <seealso cref="JpegDecompressor.Quantize_colors"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public byte[][] Colormap
        {
            get { return m_colormap; }
            set { m_colormap = value; }
        }

        /* State variables: these variables indicate the progress of decompression.
         * The application may examine these but must not modify them.
         */

        /* Row index of next scanline to be read from jpeg_read_scanlines().
         * Application may use this to control its processing loop, e.g.,
         * "while (output_scanline < output_height)".
         */

        /// <summary>
        /// Gets the number of scanlines returned so far.
        /// </summary>
        /// <value>The output_scanline.</value>
        /// <remarks>Usually you can just use this variable as the loop counter, 
        /// so that the loop test looks like 
        /// <c>while (cinfo.Output_scanline &lt; cinfo.Output_height)</c></remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public int Output_scanline
        {
            get { return m_output_scanline; }
        }

        /* Current input scan number and number of iMCU rows completed in scan.
         * These indicate the progress of the decompressor input side.
         */

        /// <summary>
        /// Gets the number of SOS markers seen so far.
        /// </summary>
        /// <value>The number of SOS markers seen so far.</value>
        /// <remarks>Indicates the progress of the decompressor input side.</remarks>
        public int Input_scan_number
        {
            get { return m_input_scan_number; }
        }

        /// <summary>
        /// Gets the number of iMCU rows completed.
        /// </summary>
        /// <value>The number of iMCU rows completed.</value>
        /// <remarks>Indicates the progress of the decompressor input side.</remarks>
        public int Input_iMCU_row
        {
            get { return m_input_iMCU_row; }
        }

        /* The "output scan number" is the notional scan being displayed by the
         * output side.  The decompressor will not allow output scan/row number
         * to get ahead of input scan/row, but it can fall arbitrarily far behind.
         */

        /// <summary>
        /// Gets the nominal scan number being displayed.
        /// </summary>
        /// <value>The nominal scan number being displayed.</value>
        public int Output_scan_number
        {
            get { return m_output_scan_number; }
        }

        /// <summary>
        /// Gets the number of iMCU rows read.
        /// </summary>
        /// <value>The number of iMCU rows read.</value>
        public int Output_iMCU_row
        {
            get { return m_output_iMCU_row; }
        }

        /* Current progression status.  coef_bits[c][i] indicates the precision
         * with which component c's DCT coefficient i (in zigzag order) is known.
         * It is -1 when no data has yet been received, otherwise it is the point
         * transform (shift) value for the most recent scan of the coefficient
         * (thus, 0 at completion of the progression).
         * This is null when reading a non-progressive file.
         */

        /// <summary>
        /// Gets the current progression status..
        /// </summary>
        /// <value><c>Coef_bits[c][i]</c> indicates the precision with 
        /// which component c's DCT coefficient i (in zigzag order) is known. 
        /// It is <c>-1</c> when no data has yet been received, otherwise 
        /// it is the point transform (shift) value for the most recent scan of the coefficient 
        /// (thus, 0 at completion of the progression). This is null when reading a non-progressive file.
        /// </value>
        /// <seealso href="bda5b19b-88e0-44bf-97de-cd30fc61bb65.htm" target="_self">Progressive JPEG support</seealso>
        public int[][] Coef_bits
        {
            get { return m_coef_bits; }
        }

        // These fields record data obtained from optional markers 
        // recognized by the JPEG library.

        /// <summary>
        /// Gets the resolution information from JFIF marker.
        /// </summary>
        /// <value>The information from JFIF marker.</value>
        /// <seealso cref="JpegDecompressor.X_density"/>
        /// <seealso cref="JpegDecompressor.Y_density"/>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public DensityUnit Density_unit
        {
            get { return m_density_unit; }
        }

        /// <summary>
        /// Gets the horizontal component of pixel ratio.
        /// </summary>
        /// <value>The horizontal component of pixel ratio.</value>
        /// <seealso cref="JpegDecompressor.Y_density"/>
        /// <seealso cref="JpegDecompressor.Density_unit"/>
        public short X_density
        {
            get { return m_X_density; }
        }

        /// <summary>
        /// Gets the vertical component of pixel ratio.
        /// </summary>
        /// <value>The vertical component of pixel ratio.</value>
        /// <seealso cref="JpegDecompressor.X_density"/>
        /// <seealso cref="JpegDecompressor.Density_unit"/>
        public short Y_density
        {
            get { return m_Y_density; }
        }

        /// <summary>
        /// Gets the data precision.
        /// </summary>
        /// <value>The data precision.</value>
        public int Data_precision
        {
            get { return m_data_precision; }
            //set { m_data_precision = value; }
        }

        /// <summary>
        /// Gets the largest vertical sample factor.
        /// </summary>
        /// <value>The largest vertical sample factor.</value>
        public int Max_v_samp_factor
        {
            get { return m_max_v_samp_factor; }
            //set { m_max_v_samp_factor = value; }
        }

        /// <summary>
        /// Gets the last read and unprocessed JPEG marker.
        /// </summary>
        /// <value>It is either zero or the code of a JPEG marker that has been
        /// read from the data source, but has not yet been processed.
        /// </value>
        /// <seealso cref="JpegDecompressor.jpeg_set_marker_processor"/>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public int Unread_marker
        {
            get { return m_unread_marker; }
        }

        /// <summary>
        /// Comp_info[i] describes component that appears i'th in SOF
        /// </summary>
        /// <value>The components in SOF.</value>
        /// <seealso cref="JpegComponent"/>
        public JpegComponent[] Comp_info
        {
            get { return m_comp_info; }
            internal set { m_comp_info = value; }
        }

        /// <summary>
        /// Sets input stream.
        /// </summary>
        /// <param name="infile">The input stream.</param>
        /// <remarks>
        /// The caller must have already opened the stream, and is responsible
        /// for closing it after finishing decompression.
        /// </remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public void jpeg_stdio_src(Stream infile)
        {
            /* The source object and input buffer are made permanent so that a series
            * of JPEG images can be read from the same file by calling jpeg_stdio_src
            * only before the first one.  (If we discarded the buffer at the end of
            * one image, we'd likely lose the start of the next one.)
            * This makes it unsafe to use this manager and a different source
            * manager serially with the same JPEG object.  Caveat programmer.
            */
            if (m_src == null)
            {
                /* first time for this JPEG object? */
                m_src = new SourceManagerImpl(this);
            }

            SourceManagerImpl m = m_src as SourceManagerImpl;
            if (m != null)
                m.Attach(infile);
        }

        /// <summary>
        /// Decompression startup: this will read the source datastream header markers, up to the beginning of the compressed data proper.
        /// </summary>
        /// <param name="require_image">Read a description of <b>Return Value</b>.</param>
        /// <returns>
        /// If you pass <c>require_image=true</c> (normal case), you need not check for a
        /// <see cref="ReadResult.Header_Tables_Only"/> return code; an abbreviated file will cause
        /// an error exit. <see cref="ReadResult.Suspended"/> is only possible if you use a data source
        /// module that can give a suspension return.<br/><br/>
        /// 
        /// This method will read as far as the first SOS marker (ie, actual start of compressed data),
        /// and will save all tables and parameters in the JPEG object. It will also initialize the
        /// decompression parameters to default values, and finally return <see cref="ReadResult.Header_Ok"/>.
        /// On return, the application may adjust the decompression parameters and then call
        /// <see cref="JpegDecompressor.jpeg_start_decompress"/>. (Or, if the application only wanted to
        /// determine the image parameters, the data need not be decompressed. In that case, call
        /// <see cref="JpegCommonBase.jpeg_abort"/> to release any temporary space.)<br/><br/>
        /// 
        /// If an abbreviated (tables only) datastream is presented, the routine will return
        /// <see cref="ReadResult.Header_Tables_Only"/> upon reaching EOI. The application may then re-use
        /// the JPEG object to read the abbreviated image datastream(s). It is unnecessary (but OK) to call
        /// <see cref="JpegCommonBase.jpeg_abort">jpeg_abort</see> in this case.
        /// The <see cref="ReadResult.Suspended"/> return code only occurs if the data source module
        /// requests suspension of the decompressor. In this case the application should load more source
        /// data and then re-call <c>jpeg_read_header</c> to resume processing.<br/><br/>
        /// 
        /// If a non-suspending data source is used and <c>require_image</c> is <c>true</c>,
        /// then the return code need not be inspected since only <see cref="ReadResult.Header_Ok"/> is possible.
        /// </returns>
        /// <remarks>Need only initialize JPEG object and supply a data source before calling.<br/>
        /// On return, the image dimensions and other info have been stored in the JPEG object.
        /// The application may wish to consult this information before selecting decompression parameters.<br/>
        /// This routine is now just a front end to <see cref="jpeg_consume_input"/>, with some extra error checking.
        /// </remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        /// <seealso href="0955150c-4ee7-4b0f-a716-4bda2e85652b.htm" target="_self">Decompression parameter selection</seealso>
        public ReadResult jpeg_read_header(bool require_image)
        {
            if (m_global_state != JpegState.DSTATE_START && m_global_state != JpegState.DSTATE_INHEADER)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            ReadResult retcode = jpeg_consume_input();

            switch (retcode)
            {
                case ReadResult.Reached_SOS:
                    return ReadResult.Header_Ok;
                case ReadResult.Reached_EOI:
                    if (require_image)      /* Complain if application wanted an image */
                        throw new Exception("JPEG datastream contains no image");
                    /* Reset to start state; it would be safer to require the application to
                    * call jpeg_abort, but we can't change it now for compatibility reasons.
                    * A side effect is to free any temporary memory (there shouldn't be any).
                    */
                    jpeg_abort(); /* sets state = DSTATE_START */
                    return ReadResult.Header_Tables_Only;

                case ReadResult.Suspended:
                    /* no work */
                    break;
            }

            return ReadResult.Suspended;
        }

        //////////////////////////////////////////////////////////////////////////
        // Main entry points for decompression

        /// <summary>
        /// Decompression initialization.
        /// </summary>
        /// <returns>Returns <c>false</c> if suspended. The return value need be inspected 
        /// only if a suspending data source is used.
        /// </returns>
        /// <remarks><see cref="JpegDecompressor.jpeg_read_header">jpeg_read_header</see> must be completed before calling this.<br/>
        /// 
        /// If a multipass operating mode was selected, this will do all but the last pass, and thus may take a great deal of time.
        /// </remarks>
        /// <seealso cref="JpegDecompressor.jpeg_finish_decompress"/>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public bool jpeg_start_decompress()
        {
            if (m_global_state == JpegState.DSTATE_READY)
            {
                /* First call: initialize master control, select active modules */
                m_master = new JpegDecompressorMaster(this);
                if (m_buffered_image)
                {
                    /* No more work here; expecting jpeg_start_output next */
                    m_global_state = JpegState.DSTATE_BUFIMAGE;
                    return true;
                }
                m_global_state = JpegState.DSTATE_PRELOAD;
            }

            if (m_global_state == JpegState.DSTATE_PRELOAD)
            {
                /* If file has multiple scans, absorb them all into the coef buffer */
                if (m_inputctl.HasMultipleScans())
                {
                    for (; ; )
                    {
                        ReadResult retcode;

                        /* Absorb some more input */
                        retcode = m_inputctl.consume_input();
                        if (retcode == ReadResult.Suspended)
                            return false;

                        if (retcode == ReadResult.Reached_EOI)
                            break;
                    }
                }

                m_output_scan_number = m_input_scan_number;
            }
            else if (m_global_state != JpegState.DSTATE_PRESCAN)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            /* Perform any dummy output passes, and set up for the final pass */
            return output_pass_setup();
        }

        /// <summary>
        /// Read some scanlines of data from the JPEG decompressor.
        /// </summary>
        /// <param name="scanlines">Buffer for filling.</param>
        /// <param name="max_lines">Required number of lines.</param>
        /// <returns>The return value will be the number of lines actually read. 
        /// This may be less than the number requested in several cases, including 
        /// bottom of image, data source suspension, and operating modes that emit multiple scanlines at a time.
        /// </returns>
        /// <remarks>We warn about excess calls to <c>jpeg_read_scanlines</c> since this likely signals an 
        /// application programmer error. However, an oversize buffer <c>(max_lines > scanlines remaining)</c> 
        /// is not an error.
        /// </remarks>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public int jpeg_read_scanlines(byte[][] scanlines, int max_lines)
        {
            if (m_global_state != JpegState.DSTATE_SCANNING)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            if (m_output_scanline >= m_output_height)
            {
                return 0;
            }

            /* Process some data */
            int row_ctr = 0;
            m_main.process_data(scanlines, ref row_ctr, max_lines);
            m_output_scanline += row_ctr;
            return row_ctr;
        }

        /// <summary>
        /// Finish JPEG decompression.
        /// </summary>
        /// <returns>Returns <c>false</c> if suspended. The return value need be inspected 
        /// only if a suspending data source is used.
        /// </returns>
        /// <remarks>This will normally just verify the file trailer and release temp storage.</remarks>
        /// <seealso cref="JpegDecompressor.jpeg_start_decompress"/>
        /// <seealso href="9d052723-a7f9-42de-8747-0bd9896f8157.htm" target="_self">Decompression details</seealso>
        public bool jpeg_finish_decompress()
        {
            if ((m_global_state == JpegState.DSTATE_SCANNING || m_global_state == JpegState.DSTATE_RAW_OK) && !m_buffered_image)
            {
                /* Terminate final pass of non-buffered mode */
                if (m_output_scanline < m_output_height)
                    throw new Exception("Application transferred too few scanlines");

                m_master.finish_output_pass();
                m_global_state = JpegState.DSTATE_STOPPING;
            }
            else if (m_global_state == JpegState.DSTATE_BUFIMAGE)
            {
                /* Finishing after a buffered-image operation */
                m_global_state = JpegState.DSTATE_STOPPING;
            }
            else if (m_global_state != JpegState.DSTATE_STOPPING)
            {
                /* STOPPING = repeat call after a suspension, anything else is error */
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));
            }

            /* Read until EOI */
            while (!m_inputctl.EOIReached())
            {
                if (m_inputctl.consume_input() == ReadResult.Suspended)
                {
                    /* Suspend, come back later */
                    return false;
                }
            }

            /* Do final cleanup */
            m_src.term_source();

            /* We can use jpeg_abort to release memory and reset global_state */
            jpeg_abort();
            return true;
        }

        /// <summary>
        /// Alternate entry point to read raw data.
        /// </summary>
        /// <param name="data">The raw data.</param>
        /// <param name="max_lines">The number of scanlines for reading.</param>
        /// <returns>The number of lines actually read.</returns>
        /// <remarks>Replaces <see cref="JpegDecompressor.jpeg_read_scanlines">jpeg_read_scanlines</see> 
        /// when reading raw downsampled data. Processes exactly one iMCU row per call, unless suspended.
        /// </remarks>
        public int jpeg_read_raw_data(byte[][][] data, int max_lines)
        {
            if (m_global_state != JpegState.DSTATE_RAW_OK)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            if (m_output_scanline >= m_output_height)
            {
                return 0;
            }

            /* Verify that at least one iMCU row can be returned. */
            int lines_per_iMCU_row = m_max_v_samp_factor * m_min_DCT_scaled_size;
            if (max_lines < lines_per_iMCU_row)
                throw new Exception("Buffer passed to JPEG library is too small");

            int componentCount = data.Length; // maybe we should use max_lines here
            ComponentBuffer[] cb = new ComponentBuffer[componentCount];
            for (int i = 0; i < componentCount; i++)
            {
                cb[i] = new ComponentBuffer();
                cb[i].SetBuffer(data[i], null, 0);
            }

            /* Decompress directly into user's buffer. */
            if (m_coef.decompress_data(cb) == ReadResult.Suspended)
            {
                /* suspension forced, can do nothing more */
                return 0;
            }

            /* OK, we processed one iMCU row. */
            m_output_scanline += lines_per_iMCU_row;
            return lines_per_iMCU_row;
        }

        //////////////////////////////////////////////////////////////////////////
        // Additional entry points for buffered-image mode.

        /// <summary>
        /// Is there more than one scan?
        /// </summary>
        /// <returns><c>true</c> if image has more than one scan; otherwise, <c>false</c></returns>
        /// <remarks>If you are concerned about maximum performance on baseline JPEG files,
        /// you should use <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see> only
        /// when the incoming file actually has multiple scans. This can be tested by calling this method.
        /// </remarks>
        public bool jpeg_has_multiple_scans()
        {
            /* Only valid after jpeg_read_header completes */
            if (m_global_state < JpegState.DSTATE_READY || m_global_state > JpegState.DSTATE_STOPPING)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            return m_inputctl.HasMultipleScans();
        }

        /// <summary>
        /// Initialize for an output pass in <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see>.
        /// </summary>
        /// <param name="scan_number">Indicates which scan of the input file is to be displayed; 
        /// the scans are numbered starting at 1 for this purpose.</param>
        /// <returns><c>true</c> if done; <c>false</c> if suspended</returns>
        /// <seealso cref="JpegDecompressor.jpeg_finish_output"/>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool jpeg_start_output(int scan_number)
        {
            if (m_global_state != JpegState.DSTATE_BUFIMAGE && m_global_state != JpegState.DSTATE_PRESCAN)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            /* Limit scan number to valid range */
            if (scan_number <= 0)
                scan_number = 1;

            if (m_inputctl.EOIReached() && scan_number > m_input_scan_number)
                scan_number = m_input_scan_number;

            m_output_scan_number = scan_number;
            /* Perform any dummy output passes, and set up for the real pass */
            return output_pass_setup();
        }

        /// <summary>
        /// Finish up after an output pass in <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see>.
        /// </summary>
        /// <returns>Returns <c>false</c> if suspended. The return value need be inspected only if a suspending data source is used.</returns>
        /// <seealso cref="JpegDecompressor.jpeg_start_output"/>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool jpeg_finish_output()
        {
            if ((m_global_state == JpegState.DSTATE_SCANNING || m_global_state == JpegState.DSTATE_RAW_OK) && m_buffered_image)
            {
                /* Terminate this pass. */
                /* We do not require the whole pass to have been completed. */
                m_master.finish_output_pass();
                m_global_state = JpegState.DSTATE_BUFPOST;
            }
            else if (m_global_state != JpegState.DSTATE_BUFPOST)
            {
                /* BUFPOST = repeat call after a suspension, anything else is error */
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));
            }

            /* Read markers looking for SOS or EOI */
            while (m_input_scan_number <= m_output_scan_number && !m_inputctl.EOIReached())
            {
                if (m_inputctl.consume_input() == ReadResult.Suspended)
                {
                    /* Suspend, come back later */
                    return false;
                }
            }

            m_global_state = JpegState.DSTATE_BUFIMAGE;
            return true;
        }

        /// <summary>
        /// Indicates if we have finished reading the input file.
        /// </summary>
        /// <returns><c>true</c> if we have finished reading the input file.</returns>
        /// <seealso href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">Buffered-image mode</seealso>
        public bool jpeg_input_complete()
        {
            /* Check for valid jpeg object */
            if (m_global_state < JpegState.DSTATE_START || m_global_state > JpegState.DSTATE_STOPPING)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            return m_inputctl.EOIReached();
        }

        /// <summary>
        /// Consume data in advance of what the decompressor requires.
        /// </summary>
        /// <returns>The result of data consumption.</returns>
        /// <remarks>This routine can be called at any time after initializing the JPEG object.
        /// It reads some additional data and returns when one of the indicated significant events
        /// occurs. If called after the EOI marker is reached, it will immediately return
        /// <see cref="ReadResult.Reached_EOI"/> without attempting to read more data.</remarks>
        public ReadResult jpeg_consume_input()
        {
            ReadResult retcode = ReadResult.Suspended;

            /* NB: every possible DSTATE value should be listed in this switch */
            switch (m_global_state)
            {
                case JpegState.DSTATE_START:
                    jpeg_consume_input_start();
                    retcode = jpeg_consume_input_inHeader();
                    break;
                case JpegState.DSTATE_INHEADER:
                    retcode = jpeg_consume_input_inHeader();
                    break;
                case JpegState.DSTATE_READY:
                    /* Can't advance past first SOS until start_decompress is called */
                    retcode = ReadResult.Reached_SOS;
                    break;
                case JpegState.DSTATE_PRELOAD:
                case JpegState.DSTATE_PRESCAN:
                case JpegState.DSTATE_SCANNING:
                case JpegState.DSTATE_RAW_OK:
                case JpegState.DSTATE_BUFIMAGE:
                case JpegState.DSTATE_BUFPOST:
                case JpegState.DSTATE_STOPPING:
                    retcode = m_inputctl.consume_input();
                    break;
                default:
                    throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));
            }
            return retcode;
        }

        /// <summary>
        /// Pre-calculate output image dimensions and related values for current decompression parameters.
        /// </summary>
        /// <remarks>This is allowed for possible use by application. Hence it mustn't do anything 
        /// that can't be done twice. Also note that it may be called before the master module is initialized!
        /// </remarks>
        public void jpeg_calc_output_dimensions()
        {
            // Do computations that are needed before master selection phase
            /* Prevent application from calling me at wrong times */
            if (m_global_state != JpegState.DSTATE_READY)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            /* Compute actual output image dimensions and DCT scaling choices. */
            if (m_scale_num * 8 <= m_scale_denom)
            {
                /* Provide 1/8 scaling */
                m_output_width = JpegUtils.jdiv_round_up(m_image_width, 8);
                m_output_height = JpegUtils.jdiv_round_up(m_image_height, 8);
                m_min_DCT_scaled_size = 1;
            }
            else if (m_scale_num * 4 <= m_scale_denom)
            {
                /* Provide 1/4 scaling */
                m_output_width = JpegUtils.jdiv_round_up(m_image_width, 4);
                m_output_height = JpegUtils.jdiv_round_up(m_image_height, 4);
                m_min_DCT_scaled_size = 2;
            }
            else if (m_scale_num * 2 <= m_scale_denom)
            {
                /* Provide 1/2 scaling */
                m_output_width = JpegUtils.jdiv_round_up(m_image_width, 2);
                m_output_height = JpegUtils.jdiv_round_up(m_image_height, 2);
                m_min_DCT_scaled_size = 4;
            }
            else
            {
                /* Provide 1/1 scaling */
                m_output_width = m_image_width;
                m_output_height = m_image_height;
                m_min_DCT_scaled_size = JpegConstants.DCTSize;
            }

            /* In selecting the actual DCT scaling for each component, we try to
            * scale up the chroma components via IDCT scaling rather than upsampling.
            * This saves time if the upsampler gets to use 1:1 scaling.
            * Note this code assumes that the supported DCT scalings are powers of 2.
            */
            for (int ci = 0; ci < m_num_components; ci++)
            {
                int ssize = m_min_DCT_scaled_size;
                while (ssize < JpegConstants.DCTSize &&
                    (m_comp_info[ci].H_samp_factor * ssize * 2 <= m_max_h_samp_factor * m_min_DCT_scaled_size) &&
                    (m_comp_info[ci].V_samp_factor * ssize * 2 <= m_max_v_samp_factor * m_min_DCT_scaled_size))
                {
                    ssize = ssize * 2;
                }

                m_comp_info[ci].DCT_scaled_size = ssize;
            }

            /* Recompute downsampled dimensions of components;
            * application needs to know these if using raw downsampled data.
            */
            for (int ci = 0; ci < m_num_components; ci++)
            {
                /* Size in samples, after IDCT scaling */
                m_comp_info[ci].downsampled_width = JpegUtils.jdiv_round_up(
                    m_image_width * m_comp_info[ci].H_samp_factor * m_comp_info[ci].DCT_scaled_size,
                    m_max_h_samp_factor * JpegConstants.DCTSize);

                m_comp_info[ci].downsampled_height = JpegUtils.jdiv_round_up(
                    m_image_height * m_comp_info[ci].V_samp_factor * m_comp_info[ci].DCT_scaled_size,
                    m_max_v_samp_factor * JpegConstants.DCTSize);
            }

            /* Report number of components in selected colorspace. */
            /* Probably this should be in the color conversion module... */
            switch (m_out_color_space)
            {
                case ColorSpace.Grayscale:
                    m_out_color_components = 1;
                    break;
                case ColorSpace.RGB:
                case ColorSpace.YCbCr:
                    m_out_color_components = 3;
                    break;
                case ColorSpace.CMYK:
                case ColorSpace.YCCK:
                    m_out_color_components = 4;
                    break;
                default:
                    /* else must be same colorspace as in file */
                    m_out_color_components = m_num_components;
                    break;
            }

            m_output_components = (m_quantize_colors ? 1 : m_out_color_components);

            /* See if up-sampler will want to emit more than one row at a time */
            if (use_merged_upsample())
                m_rec_outbuf_height = m_max_v_samp_factor;
            else
                m_rec_outbuf_height = 1;
        }

        /// <summary>
        /// Read or write the raw DCT coefficient arrays from a JPEG file (useful for lossless transcoding).
        /// </summary>
        /// <returns>Returns <c>null</c> if suspended. This case need be checked only 
        /// if a suspending data source is used.
        /// </returns>
        /// <remarks>
        /// <see cref="JpegDecompressor.jpeg_read_header">jpeg_read_header</see> must be completed before calling this.<br/>
        /// 
        /// The entire image is read into a set of virtual coefficient-block arrays, one per component.
        /// The return value is an array of virtual-array descriptors.<br/>
        /// 
        /// An alternative usage is to simply obtain access to the coefficient arrays during a 
        /// <see href="6dba59c5-d32e-4dfc-87fe-f9eff7004146.htm" target="_self">buffered-image mode</see> decompression operation. This is allowed after any 
        /// <see cref="JpegDecompressor.jpeg_finish_output">jpeg_finish_output</see> call. The arrays can be accessed 
        /// until <see cref="JpegDecompressor.jpeg_finish_decompress">jpeg_finish_decompress</see> is called. 
        /// Note that any call to the library may reposition the arrays, 
        /// so don't rely on <see cref="JpegVirtualArray{T}.Access"/> results to stay valid across library calls.
        /// </remarks>
        public JpegVirtualArray<JpegBlock>[] jpeg_read_coefficients()
        {
            if (m_global_state == JpegState.DSTATE_READY)
            {
                /* First call: initialize active modules */
                transdecode_master_selection();
                m_global_state = JpegState.DSTATE_RDCOEFS;
            }

            if (m_global_state == JpegState.DSTATE_RDCOEFS)
            {
                /* Absorb whole file into the coef buffer */
                for (; ; )
                {
                    ReadResult retcode;

                    /* Absorb some more input */
                    retcode = m_inputctl.consume_input();
                    if (retcode == ReadResult.Suspended)
                        return null;

                    if (retcode == ReadResult.Reached_EOI)
                        break;
                }

                /* Set state so that jpeg_finish_decompress does the right thing */
                m_global_state = JpegState.DSTATE_STOPPING;
            }

            /* At this point we should be in state DSTATE_STOPPING if being used
            * standalone, or in state DSTATE_BUFIMAGE if being invoked to get access
            * to the coefficients during a full buffered-image-mode decompression.
            */
            if ((m_global_state == JpegState.DSTATE_STOPPING || m_global_state == JpegState.DSTATE_BUFIMAGE) && m_buffered_image)
                return m_coef.GetCoefArrays();

            /* Oops, improper usage */
            throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));
        }

        /// <summary>
        /// Initializes the compression object with default parameters, then copy from the source object 
        /// all parameters needed for lossless transcoding.
        /// </summary>
        /// <param name="dstinfo">Target JPEG compression object.</param>
        /// <remarks>Parameters that can be varied without loss (such as scan script and 
        /// Huffman optimization) are left in their default states.</remarks>
        public void jpeg_copy_critical_parameters(JpegCompressor dstinfo)
        {
            /* Safety check to ensure start_compress not called yet. */
            if (dstinfo.m_global_state != JpegState.CSTATE_START)
                throw new Exception(String.Format("Improper call to JPEG library in state {0}", (int)m_global_state));

            /* Copy fundamental image dimensions */
            dstinfo.m_image_width = m_image_width;
            dstinfo.m_image_height = m_image_height;
            dstinfo.m_input_components = m_num_components;
            dstinfo.m_in_color_space = m_jpeg_color_space;

            /* Initialize all parameters to default values */
            dstinfo.jpeg_set_defaults();

            /* jpeg_set_defaults may choose wrong colorspace, eg YCbCr if input is RGB.
            * Fix it to get the right header markers for the image colorspace.
            */
            dstinfo.jpeg_set_colorspace(m_jpeg_color_space);
            dstinfo.m_data_precision = m_data_precision;
            dstinfo.m_CCIR601_sampling = m_CCIR601_sampling;

            /* Copy the source's quantization tables. */
            for (int tblno = 0; tblno < JpegConstants.NumberOfQuantTables; tblno++)
            {
                if (m_quant_tbl_ptrs[tblno] != null)
                {
                    if (dstinfo.m_quant_tbl_ptrs[tblno] == null)
                        dstinfo.m_quant_tbl_ptrs[tblno] = new JpegQuantizationTable();

                    Buffer.BlockCopy(m_quant_tbl_ptrs[tblno].quantval, 0,
                        dstinfo.m_quant_tbl_ptrs[tblno].quantval, 0,
                        dstinfo.m_quant_tbl_ptrs[tblno].quantval.Length * sizeof(short));

                    dstinfo.m_quant_tbl_ptrs[tblno].Sent_table = false;
                }
            }

            /* Copy the source's per-component info.
            * Note we assume jpeg_set_defaults has allocated the dest comp_info array.
            */
            dstinfo.m_num_components = m_num_components;
            if (dstinfo.m_num_components < 1 || dstinfo.m_num_components > JpegConstants.MaxComponents)
                throw new Exception(String.Format("Too many color components: {0}, max {1}", dstinfo.m_num_components, JpegConstants.MaxComponents));

            for (int ci = 0; ci < dstinfo.m_num_components; ci++)
            {
                dstinfo.Component_info[ci].Component_id = m_comp_info[ci].Component_id;
                dstinfo.Component_info[ci].H_samp_factor = m_comp_info[ci].H_samp_factor;
                dstinfo.Component_info[ci].V_samp_factor = m_comp_info[ci].V_samp_factor;
                dstinfo.Component_info[ci].Quant_tbl_no = m_comp_info[ci].Quant_tbl_no;

                /* Make sure saved quantization table for component matches the qtable
                * slot.  If not, the input file re-used this qtable slot.
                * IJG encoder currently cannot duplicate this.
                */
                int tblno = dstinfo.Component_info[ci].Quant_tbl_no;
                if (tblno < 0 || tblno >= JpegConstants.NumberOfQuantTables || m_quant_tbl_ptrs[tblno] == null)
                    throw new Exception(String.Format("Quantization table 0x{0:X2} was not defined", tblno));

                JpegQuantizationTable c_quant = m_comp_info[ci].quant_table;
                if (c_quant != null)
                {
                    JpegQuantizationTable slot_quant = m_quant_tbl_ptrs[tblno];
                    for (int coefi = 0; coefi < JpegConstants.DCTSize2; coefi++)
                    {
                        if (c_quant.quantval[coefi] != slot_quant.quantval[coefi])
                            throw new Exception(String.Format("Cannot transcode due to multiple use of quantization table {0}", tblno));
                    }
                }
                /* Note: we do not copy the source's Huffman table assignments;
                * instead we rely on jpeg_set_colorspace to have made a suitable choice.
                */
            }

            /* Also copy JFIF version and resolution information, if available.
            * Strictly speaking this isn't "critical" info, but it's nearly
            * always appropriate to copy it if available.  In particular,
            * if the application chooses to copy JFIF 1.02 extension markers from
            * the source file, we need to copy the version to make sure we don't
            * emit a file that has 1.02 extensions but a claimed version of 1.01.
            * We will *not*, however, copy version info from mislabeled "2.01" files.
            */
            if (m_saw_JFIF_marker)
            {
                if (m_JFIF_major_version == 1)
                {
                    dstinfo.m_JFIF_major_version = m_JFIF_major_version;
                    dstinfo.m_JFIF_minor_version = m_JFIF_minor_version;
                }

                dstinfo.m_density_unit = m_density_unit;
                dstinfo.m_X_density = (short)m_X_density;
                dstinfo.m_Y_density = (short)m_Y_density;
            }
        }

        /// <summary>
        /// Aborts processing of a JPEG decompression operation.
        /// </summary>
        /// <seealso cref="JpegCommonBase.jpeg_abort"/>
        public void jpeg_abort_decompress()
        {
            jpeg_abort();
        }

        /// <summary>
        /// Sets processor for special marker.
        /// </summary>
        /// <param name="marker_code">The marker code.</param>
        /// <param name="routine">The processor.</param>
        /// <remarks>Allows you to supply your own routine to process 
        /// COM and/or APPn markers on-the-fly as they are read.
        /// </remarks>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public void jpeg_set_marker_processor(int marker_code, jpeg_marker_parser_method routine)
        {
            m_marker.jpeg_set_marker_processor(marker_code, routine);
        }

        /// <summary>
        /// Control saving of COM and APPn markers into <see cref="JpegDecompressor.Marker_list">Marker_list</see>.
        /// </summary>
        /// <param name="marker_code">The marker type to save (see JpegMarkerType enumeration).<br/>
        /// To arrange to save all the special marker types, you need to call this 
        /// routine 17 times, for COM and APP0-APP15 markers.</param>
        /// <param name="length_limit">If the incoming marker is longer than <c>length_limit</c> data bytes, 
        /// only <c>length_limit</c> bytes will be saved; this parameter allows you to avoid chewing up memory 
        /// when you only need to see the first few bytes of a potentially large marker. If you want to save 
        /// all the data, set <c>length_limit</c> to 0xFFFF; that is enough since marker lengths are only 16 bits. 
        /// As a special case, setting <c>length_limit</c> to 0 prevents that marker type from being saved at all. 
        /// (That is the default behavior, in fact.)
        /// </param>
        /// <seealso cref="JpegDecompressor.Marker_list"/>
        /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
        public void jpeg_save_markers(int marker_code, int length_limit)
        {
            m_marker.jpeg_save_markers(marker_code, length_limit);
        }

        /// <summary>
        /// Determine whether merged upsample/color conversion should be used.
        /// CRUCIAL: this must match the actual capabilities of merged upsampler!
        /// </summary>
        internal bool use_merged_upsample()
        {
            /* Merging is the equivalent of plain box-filter upsampling */
            if (m_do_fancy_upsampling || m_CCIR601_sampling)
                return false;

            /* UpsamplerImpl only supports YCC=>RGB color conversion */
            if (m_jpeg_color_space != ColorSpace.YCbCr || m_num_components != 3 ||
                m_out_color_space != ColorSpace.RGB || m_out_color_components != JpegConstants.RGB_PixelLength)
            {
                return false;
            }

            /* and it only handles 2h1v or 2h2v sampling ratios */
            if (m_comp_info[0].H_samp_factor != 2 || m_comp_info[1].H_samp_factor != 1 ||
                m_comp_info[2].H_samp_factor != 1 || m_comp_info[0].V_samp_factor > 2 ||
                m_comp_info[1].V_samp_factor != 1 || m_comp_info[2].V_samp_factor != 1)
            {
                return false;
            }

            /* furthermore, it doesn't work if we've scaled the IDCTs differently */
            if (m_comp_info[0].DCT_scaled_size != m_min_DCT_scaled_size ||
                m_comp_info[1].DCT_scaled_size != m_min_DCT_scaled_size ||
                m_comp_info[2].DCT_scaled_size != m_min_DCT_scaled_size)
            {
                return false;
            }

            /* ??? also need to test for upsample-time rescaling, when & if supported */
            /* by golly, it'll work... */
            return true;
        }

        /// <summary>
        /// Initialization of JPEG compression objects.
        /// The error manager must already be set up (in case memory manager fails).
        /// </summary>
        private void initialize()
        {
            /* Zero out pointers to permanent structures. */
            m_src = null;

            for (int i = 0; i < JpegConstants.NumberOfQuantTables; i++)
                m_quant_tbl_ptrs[i] = null;

            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
            {
                m_dc_huff_tbl_ptrs[i] = null;
                m_ac_huff_tbl_ptrs[i] = null;
            }

            /* Initialize marker processor so application can override methods
            * for COM, APPn markers before calling jpeg_read_header.
            */
            m_marker_list = new List<JpegMarker>();
            m_marker = new JpegMarkerReader(this);

            /* And initialize the overall input controller. */
            m_inputctl = new JpegInputController(this);

            /* OK, I'm ready */
            m_global_state = JpegState.DSTATE_START;
        }

        /// <summary>
        /// Master selection of decompression modules for transcoding (that is, reading 
        /// raw DCT coefficient arrays from an input JPEG file.)
        /// This substitutes for initialization of the full decompressor.
        /// </summary>
        private void transdecode_master_selection()
        {
            /* This is effectively a buffered-image operation. */
            m_buffered_image = true;

            if (m_progressive_mode)
                m_entropy = new ProgressiveHuffmanDecoder(this);
            else
                m_entropy = new HuffEntropyDecoder(this);

            /* Always get a full-image coefficient buffer. */
            m_coef = new JpegDecompressorCoefController(this, true);

            /* Initialize input side of decompressor to consume first scan. */
            m_inputctl.start_input_pass();
        }

        /// <summary>
        /// Set up for an output pass, and perform any dummy pass(es) needed.
        /// Common subroutine for jpeg_start_decompress and jpeg_start_output.
        /// Entry: global_state = DSTATE_PRESCAN only if previously suspended.
        /// Exit: If done, returns true and sets global_state for proper output mode.
        ///       If suspended, returns false and sets global_state = DSTATE_PRESCAN.
        /// </summary>
        private bool output_pass_setup()
        {
            if (m_global_state != JpegState.DSTATE_PRESCAN)
            {
                /* First call: do pass setup */
                m_master.prepare_for_output_pass();
                m_output_scanline = 0;
                m_global_state = JpegState.DSTATE_PRESCAN;
            }

            /* Loop over any required dummy passes */
            while (m_master.IsDummyPass())
            {
                /* Crank through the dummy pass */
                while (m_output_scanline < m_output_height)
                {
                    int last_scanline;

                    /* Process some data */
                    last_scanline = m_output_scanline;
                    m_main.process_data(null, ref m_output_scanline, 0);
                    if (m_output_scanline == last_scanline)
                    {
                        /* No progress made, must suspend */
                        return false;
                    }
                }

                /* Finish up dummy pass, and set up for another one */
                m_master.finish_output_pass();
                m_master.prepare_for_output_pass();
                m_output_scanline = 0;
            }

            /* Ready for application to drive output pass through
            * jpeg_read_scanlines or jpeg_read_raw_data.
            */
            m_global_state = m_raw_data_out ? JpegState.DSTATE_RAW_OK : JpegState.DSTATE_SCANNING;
            return true;
        }

        /// <summary>
        /// Set default decompression parameters.
        /// </summary>
        private void default_decompress_parms()
        {
            /* Guess the input colorspace, and set output colorspace accordingly. */
            /* (Wish JPEG committee had provided a real way to specify this...) */
            /* Note application may override our guesses. */
            switch (m_num_components)
            {
                case 1:
                    m_jpeg_color_space = ColorSpace.Grayscale;
                    m_out_color_space = ColorSpace.Grayscale;
                    break;

                case 3:
                    if (m_saw_JFIF_marker)
                    {
                        /* JFIF implies YCbCr */
                        m_jpeg_color_space = ColorSpace.YCbCr;
                    }
                    else if (m_saw_Adobe_marker)
                    {
                        switch (m_Adobe_transform)
                        {
                            case 0:
                                m_jpeg_color_space = ColorSpace.RGB;
                                break;
                            case 1:
                                m_jpeg_color_space = ColorSpace.YCbCr;
                                break;
                            default:
                                m_jpeg_color_space = ColorSpace.YCbCr; /* assume it's YCbCr */
                                break;
                        }
                    }
                    else
                    {
                        /* Saw no special markers, try to guess from the component IDs */
                        int cid0 = m_comp_info[0].Component_id;
                        int cid1 = m_comp_info[1].Component_id;
                        int cid2 = m_comp_info[2].Component_id;

                        if (cid0 == 1 && cid1 == 2 && cid2 == 3)
                        {
                            /* assume JFIF w/out marker */
                            m_jpeg_color_space = ColorSpace.YCbCr;
                        }
                        else if (cid0 == 82 && cid1 == 71 && cid2 == 66)
                        {
                            /* ASCII 'R', 'G', 'B' */
                            m_jpeg_color_space = ColorSpace.RGB;
                        }
                        else
                        {
                            /* assume it's YCbCr */
                            m_jpeg_color_space = ColorSpace.YCbCr;
                        }
                    }
                    /* Always guess RGB is proper output colorspace. */
                    m_out_color_space = ColorSpace.RGB;
                    break;

                case 4:
                    if (m_saw_Adobe_marker)
                    {
                        switch (m_Adobe_transform)
                        {
                            case 0:
                                m_jpeg_color_space = ColorSpace.CMYK;
                                break;
                            case 2:
                                m_jpeg_color_space = ColorSpace.YCCK;
                                break;
                            default:
                                /* assume it's YCCK */
                                m_jpeg_color_space = ColorSpace.YCCK;
                                break;
                        }
                    }
                    else
                    {
                        /* No special markers, assume straight CMYK. */
                        m_jpeg_color_space = ColorSpace.CMYK;
                    }

                    m_out_color_space = ColorSpace.CMYK;
                    break;

                default:
                    m_jpeg_color_space = ColorSpace.Unknown;
                    m_out_color_space = ColorSpace.Unknown;
                    break;
            }

            /* Set defaults for other decompression parameters. */
            m_scale_num = 1;       /* 1:1 scaling */
            m_scale_denom = 1;
            m_buffered_image = false;
            m_raw_data_out = false;
            m_dct_method = JpegConstants.DefaultDCTMethod;
            m_do_fancy_upsampling = true;
            m_do_block_smoothing = true;
            m_quantize_colors = false;

            /* We set these in case application only sets quantize_colors. */
            m_dither_mode = DitherMode.FloydStein;
            m_two_pass_quantize = true;
            m_desired_number_of_colors = 256;
            m_colormap = null;

            /* Initialize for no mode change in buffered-image mode. */
            m_enable_1pass_quant = false;
            m_enable_external_quant = false;
            m_enable_2pass_quant = false;
        }

        private void jpeg_consume_input_start()
        {
            /* Start-of-datastream actions: reset appropriate modules */
            m_inputctl.reset_input_controller();

            /* Initialize application's data source module */
            m_src.init_source();
            m_global_state = JpegState.DSTATE_INHEADER;
        }

        private ReadResult jpeg_consume_input_inHeader()
        {
            ReadResult retcode = m_inputctl.consume_input();
            if (retcode == ReadResult.Reached_SOS)
            {
                /* Found SOS, prepare to decompress */
                /* Set up default parameters based on header data */
                default_decompress_parms();

                /* Set global state: ready for start_decompress */
                m_global_state = JpegState.DSTATE_READY;
            }

            return retcode;
        }
    }
    #endregion

    #region JpegDecompressorCoefController
    /// <summary>
    /// Coefficient buffer control
    /// 
    /// This code applies interblock smoothing as described by section K.8
    /// of the JPEG standard: the first 5 AC coefficients are estimated from
    /// the DC values of a DCT block and its 8 neighboring blocks.
    /// We apply smoothing only for progressive JPEG decoding, and only if
    /// the coefficients it can estimate are not yet known to full precision.
    /// </summary>
    class JpegDecompressorCoefController
    {
        private const int SAVED_COEFS = 6; /* we save coef_bits[0..5] */

        /* Natural-order array positions of the first 5 zigzag-order coefficients */
        private const int Q01_POS = 1;
        private const int Q10_POS = 8;
        private const int Q20_POS = 16;
        private const int Q11_POS = 9;
        private const int Q02_POS = 2;

        private enum DecompressorType
        {
            Ordinary,
            Smooth,
            OnePass
        }

        private JpegDecompressor m_cinfo;
        private bool m_useDummyConsumeData;
        private DecompressorType m_decompressor;

        /* These variables keep track of the current location of the input side. */
        /* cinfo.input_iMCU_row is also used for this. */
        private int m_MCU_ctr;     /* counts MCUs processed in current row */
        private int m_MCU_vert_offset;        /* counts MCU rows within iMCU row */
        private int m_MCU_rows_per_iMCU_row;  /* number of such rows needed */

        /* The output side's location is represented by cinfo.output_iMCU_row. */

        /* In single-pass modes, it's sufficient to buffer just one MCU.
        * We allocate a workspace of DecompressorMaxBlocksInMCU coefficient blocks,
        * and let the entropy decoder write into that workspace each time.
        * (On 80x86, the workspace is FAR even though it's not really very big;
        * this is to keep the module interfaces unchanged when a large coefficient
        * buffer is necessary.)
        * In multi-pass modes, this array points to the current MCU's blocks
        * within the virtual arrays; it is used only by the input side.
        */
        private JpegBlock[] m_MCU_buffer = new JpegBlock[JpegConstants.DecompressorMaxBlocksInMCU];

        /* In multi-pass modes, we need a virtual block array for each component. */
        private JpegVirtualArray<JpegBlock>[] m_whole_image = new JpegVirtualArray<JpegBlock>[JpegConstants.MaxComponents];
        private JpegVirtualArray<JpegBlock>[] m_coef_arrays;

        /* When doing block smoothing, we latch coefficient Al values here */
        private int[] m_coef_bits_latch;
        private int m_coef_bits_savedOffset;

        public JpegDecompressorCoefController(JpegDecompressor cinfo, bool need_full_buffer)
        {
            m_cinfo = cinfo;

            /* Create the coefficient buffer. */
            if (need_full_buffer)
            {
                /* Allocate a full-image virtual array for each component, */
                /* padded to a multiple of samp_factor DCT blocks in each direction. */
                /* Note we ask for a pre-zeroed array. */
                for (int ci = 0; ci < cinfo.m_num_components; ci++)
                {
                    m_whole_image[ci] = JpegCommonBase.CreateBlocksArray(
                        JpegUtils.jround_up(cinfo.Comp_info[ci].Width_in_blocks, cinfo.Comp_info[ci].H_samp_factor),
                        JpegUtils.jround_up(cinfo.Comp_info[ci].height_in_blocks, cinfo.Comp_info[ci].V_samp_factor));
                    m_whole_image[ci].ErrorProcessor = cinfo;
                }

                m_useDummyConsumeData = false;
                m_decompressor = DecompressorType.Ordinary;
                m_coef_arrays = m_whole_image; /* link to virtual arrays */
            }
            else
            {
                /* We only need a single-MCU buffer. */
                JpegBlock[] buffer = new JpegBlock[JpegConstants.DecompressorMaxBlocksInMCU];
                for (int i = 0; i < JpegConstants.DecompressorMaxBlocksInMCU; i++)
                {
                    buffer[i] = new JpegBlock();
                    for (int ii = 0; ii < buffer[i].data.Length; ii++)
                        buffer[i].data[ii] = -12851;

                    m_MCU_buffer[i] = buffer[i];
                }

                m_useDummyConsumeData = true;
                m_decompressor = DecompressorType.OnePass;
                m_coef_arrays = null; /* flag for no virtual arrays */
            }
        }

        /// <summary>
        /// Initialize for an input processing pass.
        /// </summary>
        public void start_input_pass()
        {
            m_cinfo.m_input_iMCU_row = 0;
            start_iMCU_row();
        }

        /// <summary>
        /// Consume input data and store it in the full-image coefficient buffer.
        /// We read as much as one fully interleaved MCU row ("iMCU" row) per call,
        /// ie, v_samp_factor block rows for each component in the scan.
        /// </summary>
        public ReadResult consume_data()
        {
            if (m_useDummyConsumeData)
                return ReadResult.Suspended;  /* Always indicate nothing was done */

            JpegBlock[][][] buffer = new JpegBlock[JpegConstants.MaxComponentsInScan][][];

            /* Align the virtual buffers for the components used in this scan. */
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];

                buffer[ci] = m_whole_image[componentInfo.Component_index].Access(
                    m_cinfo.m_input_iMCU_row * componentInfo.V_samp_factor, componentInfo.V_samp_factor);

                /* Note: entropy decoder expects buffer to be zeroed,
                 * but this is handled automatically by the memory manager
                 * because we requested a pre-zeroed array.
                 */
            }

            /* Loop to process one whole iMCU row */
            for (int yoffset = m_MCU_vert_offset; yoffset < m_MCU_rows_per_iMCU_row; yoffset++)
            {
                for (int MCU_col_num = m_MCU_ctr; MCU_col_num < m_cinfo.m_MCUs_per_row; MCU_col_num++)
                {
                    /* Construct list of pointers to DCT blocks belonging to this MCU */
                    int blkn = 0;           /* index of current DCT block within MCU */
                    for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                    {
                        JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];
                        int start_col = MCU_col_num * componentInfo.MCU_width;
                        for (int yindex = 0; yindex < componentInfo.MCU_height; yindex++)
                        {
                            for (int xindex = 0; xindex < componentInfo.MCU_width; xindex++)
                            {
                                m_MCU_buffer[blkn] = buffer[ci][yindex + yoffset][start_col + xindex];
                                blkn++;
                            }
                        }
                    }

                    /* Try to fetch the MCU. */
                    if (!m_cinfo.m_entropy.decode_mcu(m_MCU_buffer))
                    {
                        /* Suspension forced; update state counters and exit */
                        m_MCU_vert_offset = yoffset;
                        m_MCU_ctr = MCU_col_num;
                        return ReadResult.Suspended;
                    }
                }

                /* Completed an MCU row, but perhaps not an iMCU row */
                m_MCU_ctr = 0;
            }

            /* Completed the iMCU row, advance counters for next one */
            m_cinfo.m_input_iMCU_row++;
            if (m_cinfo.m_input_iMCU_row < m_cinfo.m_total_iMCU_rows)
            {
                start_iMCU_row();
                return ReadResult.Row_Completed;
            }

            /* Completed the scan */
            m_cinfo.m_inputctl.finish_input_pass();
            return ReadResult.Scan_Completed;
        }

        /// <summary>
        /// Initialize for an output processing pass.
        /// </summary>
        public void start_output_pass()
        {
            /* If multipass, check to see whether to use block smoothing on this pass */
            if (m_coef_arrays != null)
            {
                if (m_cinfo.m_do_block_smoothing && smoothing_ok())
                    m_decompressor = DecompressorType.Smooth;
                else
                    m_decompressor = DecompressorType.Ordinary;
            }

            m_cinfo.m_output_iMCU_row = 0;
        }

        public ReadResult decompress_data(ComponentBuffer[] output_buf)
        {
            switch (m_decompressor)
            {
                case DecompressorType.Ordinary:
                    return decompress_data_ordinary(output_buf);

                case DecompressorType.Smooth:
                    return decompress_smooth_data(output_buf);

                case DecompressorType.OnePass:
                    return decompress_onepass(output_buf);
            }

            throw new Exception("Not implemented yet");
        }

        /* Pointer to array of coefficient virtual arrays, or null if none */
        public JpegVirtualArray<JpegBlock>[] GetCoefArrays()
        {
            return m_coef_arrays;
        }

        /// <summary>
        /// Decompress and return some data in the single-pass case.
        /// Always attempts to emit one fully interleaved MCU row ("iMCU" row).
        /// Input and output must run in lockstep since we have only a one-MCU buffer.
        /// Return value is JPEG_ROW_COMPLETED, JPEG_SCAN_COMPLETED, or JPEG_SUSPENDED.
        /// 
        /// NB: output_buf contains a plane for each component in image,
        /// which we index according to the component's SOF position.
        /// </summary>
        private ReadResult decompress_onepass(ComponentBuffer[] output_buf)
        {
            int last_MCU_col = m_cinfo.m_MCUs_per_row - 1;
            int last_iMCU_row = m_cinfo.m_total_iMCU_rows - 1;

            /* Loop to process as much as one whole iMCU row */
            for (int yoffset = m_MCU_vert_offset; yoffset < m_MCU_rows_per_iMCU_row; yoffset++)
            {
                for (int MCU_col_num = m_MCU_ctr; MCU_col_num <= last_MCU_col; MCU_col_num++)
                {
                    /* Try to fetch an MCU.  Entropy decoder expects buffer to be zeroed. */
                    for (int i = 0; i < m_cinfo.m_blocks_in_MCU; i++)
                        Array.Clear(m_MCU_buffer[i].data, 0, m_MCU_buffer[i].data.Length);

                    if (!m_cinfo.m_entropy.decode_mcu(m_MCU_buffer))
                    {
                        /* Suspension forced; update state counters and exit */
                        m_MCU_vert_offset = yoffset;
                        m_MCU_ctr = MCU_col_num;
                        return ReadResult.Suspended;
                    }

                    /* Determine where data should go in output_buf and do the IDCT thing.
                     * We skip dummy blocks at the right and bottom edges (but blkn gets
                     * incremented past them!).  Note the inner loop relies on having
                     * allocated the MCU_buffer[] blocks sequentially.
                     */
                    int blkn = 0;           /* index of current DCT block within MCU */
                    for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                    {
                        JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];

                        /* Don't bother to IDCT an uninteresting component. */
                        if (!componentInfo.component_needed)
                        {
                            blkn += componentInfo.MCU_blocks;
                            continue;
                        }

                        int useful_width = (MCU_col_num < last_MCU_col) ? componentInfo.MCU_width : componentInfo.last_col_width;
                        int outputIndex = yoffset * componentInfo.DCT_scaled_size;
                        int start_col = MCU_col_num * componentInfo.MCU_sample_width;
                        for (int yindex = 0; yindex < componentInfo.MCU_height; yindex++)
                        {
                            if (m_cinfo.m_input_iMCU_row < last_iMCU_row || yoffset + yindex < componentInfo.last_row_height)
                            {
                                int output_col = start_col;
                                for (int xindex = 0; xindex < useful_width; xindex++)
                                {
                                    m_cinfo.m_idct.inverse(componentInfo.Component_index,
                                        m_MCU_buffer[blkn + xindex].data, output_buf[componentInfo.Component_index],
                                        outputIndex, output_col);

                                    output_col += componentInfo.DCT_scaled_size;
                                }
                            }

                            blkn += componentInfo.MCU_width;
                            outputIndex += componentInfo.DCT_scaled_size;
                        }
                    }
                }

                /* Completed an MCU row, but perhaps not an iMCU row */
                m_MCU_ctr = 0;
            }

            /* Completed the iMCU row, advance counters for next one */
            m_cinfo.m_output_iMCU_row++;
            m_cinfo.m_input_iMCU_row++;
            if (m_cinfo.m_input_iMCU_row < m_cinfo.m_total_iMCU_rows)
            {
                start_iMCU_row();
                return ReadResult.Row_Completed;
            }

            /* Completed the scan */
            m_cinfo.m_inputctl.finish_input_pass();
            return ReadResult.Scan_Completed;
        }

        /// <summary>
        /// Decompress and return some data in the multi-pass case.
        /// Always attempts to emit one fully interleaved MCU row ("iMCU" row).
        /// Return value is JPEG_ROW_COMPLETED, JPEG_SCAN_COMPLETED, or JPEG_SUSPENDED.
        /// 
        /// NB: output_buf contains a plane for each component in image.
        /// </summary>
        private ReadResult decompress_data_ordinary(ComponentBuffer[] output_buf)
        {
            /* Force some input to be done if we are getting ahead of the input. */
            while (m_cinfo.m_input_scan_number < m_cinfo.m_output_scan_number ||
                   (m_cinfo.m_input_scan_number == m_cinfo.m_output_scan_number &&
                    m_cinfo.m_input_iMCU_row <= m_cinfo.m_output_iMCU_row))
            {
                if (m_cinfo.m_inputctl.consume_input() == ReadResult.Suspended)
                    return ReadResult.Suspended;
            }

            int last_iMCU_row = m_cinfo.m_total_iMCU_rows - 1;

            /* OK, output from the virtual arrays. */
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[ci];

                /* Don't bother to IDCT an uninteresting component. */
                if (!componentInfo.component_needed)
                    continue;

                /* Align the virtual buffer for this component. */
                JpegBlock[][] buffer = m_whole_image[ci].Access(m_cinfo.m_output_iMCU_row * componentInfo.V_samp_factor,
                    componentInfo.V_samp_factor);

                /* Count non-dummy DCT block rows in this iMCU row. */
                int block_rows;
                if (m_cinfo.m_output_iMCU_row < last_iMCU_row)
                    block_rows = componentInfo.V_samp_factor;
                else
                {
                    /* NB: can't use last_row_height here; it is input-side-dependent! */
                    block_rows = componentInfo.height_in_blocks % componentInfo.V_samp_factor;
                    if (block_rows == 0)
                        block_rows = componentInfo.V_samp_factor;
                }

                /* Loop over all DCT blocks to be processed. */
                int rowIndex = 0;
                for (int block_row = 0; block_row < block_rows; block_row++)
                {
                    int output_col = 0;
                    for (int block_num = 0; block_num < componentInfo.Width_in_blocks; block_num++)
                    {
                        m_cinfo.m_idct.inverse(componentInfo.Component_index,
                            buffer[block_row][block_num].data, output_buf[ci], rowIndex, output_col);

                        output_col += componentInfo.DCT_scaled_size;
                    }

                    rowIndex += componentInfo.DCT_scaled_size;
                }
            }

            m_cinfo.m_output_iMCU_row++;
            if (m_cinfo.m_output_iMCU_row < m_cinfo.m_total_iMCU_rows)
                return ReadResult.Row_Completed;

            return ReadResult.Scan_Completed;
        }

        /// <summary>
        /// Variant of decompress_data for use when doing block smoothing.
        /// </summary>
        private ReadResult decompress_smooth_data(ComponentBuffer[] output_buf)
        {
            /* Force some input to be done if we are getting ahead of the input. */
            while (m_cinfo.m_input_scan_number <= m_cinfo.m_output_scan_number && !m_cinfo.m_inputctl.EOIReached())
            {
                if (m_cinfo.m_input_scan_number == m_cinfo.m_output_scan_number)
                {
                    /* If input is working on current scan, we ordinarily want it to
                     * have completed the current row.  But if input scan is DC,
                     * we want it to keep one row ahead so that next block row's DC
                     * values are up to date.
                     */
                    int delta = (m_cinfo.m_Ss == 0) ? 1 : 0;
                    if (m_cinfo.m_input_iMCU_row > m_cinfo.m_output_iMCU_row + delta)
                        break;
                }

                if (m_cinfo.m_inputctl.consume_input() == ReadResult.Suspended)
                    return ReadResult.Suspended;
            }

            int last_iMCU_row = m_cinfo.m_total_iMCU_rows - 1;

            /* OK, output from the virtual arrays. */
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[ci];

                /* Don't bother to IDCT an uninteresting component. */
                if (!componentInfo.component_needed)
                    continue;

                int block_rows;
                int access_rows;
                bool last_row;
                /* Count non-dummy DCT block rows in this iMCU row. */
                if (m_cinfo.m_output_iMCU_row < last_iMCU_row)
                {
                    block_rows = componentInfo.V_samp_factor;
                    access_rows = block_rows * 2; /* this and next iMCU row */
                    last_row = false;
                }
                else
                {
                    /* NB: can't use last_row_height here; it is input-side-dependent! */
                    block_rows = componentInfo.height_in_blocks % componentInfo.V_samp_factor;
                    if (block_rows == 0)
                        block_rows = componentInfo.V_samp_factor;
                    access_rows = block_rows; /* this iMCU row only */
                    last_row = true;
                }

                /* Align the virtual buffer for this component. */
                JpegBlock[][] buffer = null;
                bool first_row;
                int bufferRowOffset = 0;
                if (m_cinfo.m_output_iMCU_row > 0)
                {
                    access_rows += componentInfo.V_samp_factor; /* prior iMCU row too */
                    buffer = m_whole_image[ci].Access((m_cinfo.m_output_iMCU_row - 1) * componentInfo.V_samp_factor, access_rows);
                    bufferRowOffset = componentInfo.V_samp_factor; /* point to current iMCU row */
                    first_row = false;
                }
                else
                {
                    buffer = m_whole_image[ci].Access(0, access_rows);
                    first_row = true;
                }

                /* Fetch component-dependent info */
                int coefBitsOffset = ci * SAVED_COEFS;
                int Q00 = componentInfo.quant_table.quantval[0];
                int Q01 = componentInfo.quant_table.quantval[Q01_POS];
                int Q10 = componentInfo.quant_table.quantval[Q10_POS];
                int Q20 = componentInfo.quant_table.quantval[Q20_POS];
                int Q11 = componentInfo.quant_table.quantval[Q11_POS];
                int Q02 = componentInfo.quant_table.quantval[Q02_POS];
                int outputIndex = ci;

                /* Loop over all DCT blocks to be processed. */
                for (int block_row = 0; block_row < block_rows; block_row++)
                {
                    int bufferIndex = bufferRowOffset + block_row;

                    int prev_block_row = 0;
                    if (first_row && block_row == 0)
                        prev_block_row = bufferIndex;
                    else
                        prev_block_row = bufferIndex - 1;

                    int next_block_row = 0;
                    if (last_row && block_row == block_rows - 1)
                        next_block_row = bufferIndex;
                    else
                        next_block_row = bufferIndex + 1;

                    /* We fetch the surrounding DC values using a sliding-register approach.
                     * Initialize all nine here so as to do the right thing on narrow pics.
                     */
                    int DC1 = buffer[prev_block_row][0][0];
                    int DC2 = DC1;
                    int DC3 = DC1;

                    int DC4 = buffer[bufferIndex][0][0];
                    int DC5 = DC4;
                    int DC6 = DC4;

                    int DC7 = buffer[next_block_row][0][0];
                    int DC8 = DC7;
                    int DC9 = DC7;

                    int output_col = 0;
                    int last_block_column = componentInfo.Width_in_blocks - 1;
                    for (int block_num = 0; block_num <= last_block_column; block_num++)
                    {
                        /* Fetch current DCT block into workspace so we can modify it. */
                        JpegBlock workspace = new JpegBlock();
                        Buffer.BlockCopy(buffer[bufferIndex][0].data, 0, workspace.data, 0, workspace.data.Length * sizeof(short));

                        /* Update DC values */
                        if (block_num < last_block_column)
                        {
                            DC3 = buffer[prev_block_row][1][0];
                            DC6 = buffer[bufferIndex][1][0];
                            DC9 = buffer[next_block_row][1][0];
                        }

                        /* Compute coefficient estimates per K.8.
                         * An estimate is applied only if coefficient is still zero,
                         * and is not known to be fully accurate.
                         */
                        /* AC01 */
                        int Al = m_coef_bits_latch[m_coef_bits_savedOffset + coefBitsOffset + 1];
                        if (Al != 0 && workspace[1] == 0)
                        {
                            int pred;
                            int num = 36 * Q00 * (DC4 - DC6);
                            if (num >= 0)
                            {
                                pred = ((Q01 << 7) + num) / (Q01 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                            }
                            else
                            {
                                pred = ((Q01 << 7) - num) / (Q01 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                                pred = -pred;
                            }
                            workspace[1] = (short)pred;
                        }

                        /* AC10 */
                        Al = m_coef_bits_latch[m_coef_bits_savedOffset + coefBitsOffset + 2];
                        if (Al != 0 && workspace[8] == 0)
                        {
                            int pred;
                            int num = 36 * Q00 * (DC2 - DC8);
                            if (num >= 0)
                            {
                                pred = ((Q10 << 7) + num) / (Q10 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                            }
                            else
                            {
                                pred = ((Q10 << 7) - num) / (Q10 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                                pred = -pred;
                            }
                            workspace[8] = (short)pred;
                        }

                        /* AC20 */
                        Al = m_coef_bits_latch[m_coef_bits_savedOffset + coefBitsOffset + 3];
                        if (Al != 0 && workspace[16] == 0)
                        {
                            int pred;
                            int num = 9 * Q00 * (DC2 + DC8 - 2 * DC5);
                            if (num >= 0)
                            {
                                pred = ((Q20 << 7) + num) / (Q20 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                            }
                            else
                            {
                                pred = ((Q20 << 7) - num) / (Q20 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                                pred = -pred;
                            }
                            workspace[16] = (short)pred;
                        }

                        /* AC11 */
                        Al = m_coef_bits_latch[m_coef_bits_savedOffset + coefBitsOffset + 4];
                        if (Al != 0 && workspace[9] == 0)
                        {
                            int pred;
                            int num = 5 * Q00 * (DC1 - DC3 - DC7 + DC9);
                            if (num >= 0)
                            {
                                pred = ((Q11 << 7) + num) / (Q11 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                            }
                            else
                            {
                                pred = ((Q11 << 7) - num) / (Q11 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                                pred = -pred;
                            }
                            workspace[9] = (short)pred;
                        }

                        /* AC02 */
                        Al = m_coef_bits_latch[m_coef_bits_savedOffset + coefBitsOffset + 5];
                        if (Al != 0 && workspace[2] == 0)
                        {
                            int pred;
                            int num = 9 * Q00 * (DC4 + DC6 - 2 * DC5);
                            if (num >= 0)
                            {
                                pred = ((Q02 << 7) + num) / (Q02 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                            }
                            else
                            {
                                pred = ((Q02 << 7) - num) / (Q02 << 8);
                                if (Al > 0 && pred >= (1 << Al))
                                    pred = (1 << Al) - 1;
                                pred = -pred;
                            }
                            workspace[2] = (short)pred;
                        }

                        /* OK, do the IDCT */
                        m_cinfo.m_idct.inverse(componentInfo.Component_index, workspace.data, output_buf[outputIndex], 0, output_col);

                        /* Advance for next column */
                        DC1 = DC2;
                        DC2 = DC3;
                        DC4 = DC5;
                        DC5 = DC6;
                        DC7 = DC8;
                        DC8 = DC9;

                        bufferIndex++;
                        prev_block_row++;
                        next_block_row++;

                        output_col += componentInfo.DCT_scaled_size;
                    }

                    outputIndex += componentInfo.DCT_scaled_size;
                }
            }

            m_cinfo.m_output_iMCU_row++;
            if (m_cinfo.m_output_iMCU_row < m_cinfo.m_total_iMCU_rows)
                return ReadResult.Row_Completed;

            return ReadResult.Scan_Completed;
        }

        /// <summary>
        /// Determine whether block smoothing is applicable and safe.
        /// We also latch the current states of the coef_bits[] entries for the
        /// AC coefficients; otherwise, if the input side of the decompressor
        /// advances into a new scan, we might think the coefficients are known
        /// more accurately than they really are.
        /// </summary>
        private bool smoothing_ok()
        {
            if (!m_cinfo.m_progressive_mode || m_cinfo.m_coef_bits == null)
                return false;

            /* Allocate latch area if not already done */
            if (m_coef_bits_latch == null)
            {
                m_coef_bits_latch = new int[m_cinfo.m_num_components * SAVED_COEFS];
                m_coef_bits_savedOffset = 0;
            }

            bool smoothing_useful = false;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                /* All components' quantization values must already be latched. */
                JpegQuantizationTable qtable = m_cinfo.Comp_info[ci].quant_table;
                if (qtable == null)
                    return false;

                /* Verify DC & first 5 AC quantizers are nonzero to avoid zero-divide. */
                if (qtable.quantval[0] == 0 || qtable.quantval[Q01_POS] == 0 ||
                    qtable.quantval[Q10_POS] == 0 || qtable.quantval[Q20_POS] == 0 ||
                    qtable.quantval[Q11_POS] == 0 || qtable.quantval[Q02_POS] == 0)
                {
                    return false;
                }

                /* DC values must be at least partly known for all components. */
                if (m_cinfo.m_coef_bits[ci][0] < 0)
                    return false;

                /* Block smoothing is helpful if some AC coefficients remain inaccurate. */
                for (int coefi = 1; coefi <= 5; coefi++)
                {
                    m_coef_bits_latch[m_coef_bits_savedOffset + coefi] = m_cinfo.m_coef_bits[ci][coefi];
                    if (m_cinfo.m_coef_bits[ci][coefi] != 0)
                        smoothing_useful = true;
                }

                m_coef_bits_savedOffset += SAVED_COEFS;
            }

            return smoothing_useful;
        }

        /// <summary>
        /// Reset within-iMCU-row counters for a new row (input side)
        /// </summary>
        private void start_iMCU_row()
        {
            /* In an interleaved scan, an MCU row is the same as an iMCU row.
             * In a noninterleaved scan, an iMCU row has v_samp_factor MCU rows.
             * But at the bottom of the image, process only what's left.
             */
            if (m_cinfo.m_comps_in_scan > 1)
            {
                m_MCU_rows_per_iMCU_row = 1;
            }
            else
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[0]];

                if (m_cinfo.m_input_iMCU_row < (m_cinfo.m_total_iMCU_rows - 1))
                    m_MCU_rows_per_iMCU_row = componentInfo.V_samp_factor;
                else
                    m_MCU_rows_per_iMCU_row = componentInfo.last_row_height;
            }

            m_MCU_ctr = 0;
            m_MCU_vert_offset = 0;
        }
    }
    #endregion

    #region JpegDecompressorMainController
    /// <summary>
    /// Main buffer control (downsampled-data buffer)
    /// 
    /// In the current system design, the main buffer need never be a full-image
    /// buffer; any full-height buffers will be found inside the coefficient or
    /// postprocessing controllers.  Nonetheless, the main controller is not
    /// trivial.  Its responsibility is to provide context rows for upsampling/
    /// rescaling, and doing this in an efficient fashion is a bit tricky.
    /// 
    /// Postprocessor input data is counted in "row groups".  A row group
    /// is defined to be (v_samp_factor * DCT_scaled_size / min_DCT_scaled_size)
    /// sample rows of each component.  (We require DCT_scaled_size values to be
    /// chosen such that these numbers are integers.  In practice DCT_scaled_size
    /// values will likely be powers of two, so we actually have the stronger
    /// condition that DCT_scaled_size / min_DCT_scaled_size is an integer.)
    /// Upsampling will typically produce max_v_samp_factor pixel rows from each
    /// row group (times any additional scale factor that the upsampler is
    /// applying).
    /// 
    /// The coefficient controller will deliver data to us one iMCU row at a time;
    /// each iMCU row contains v_samp_factor * DCT_scaled_size sample rows, or
    /// exactly min_DCT_scaled_size row groups.  (This amount of data corresponds
    /// to one row of MCUs when the image is fully interleaved.)  Note that the
    /// number of sample rows varies across components, but the number of row
    /// groups does not.  Some garbage sample rows may be included in the last iMCU
    /// row at the bottom of the image.
    /// 
    /// Depending on the vertical scaling algorithm used, the upsampler may need
    /// access to the sample row(s) above and below its current input row group.
    /// The upsampler is required to set need_context_rows true at global selection
    /// time if so.  When need_context_rows is false, this controller can simply
    /// obtain one iMCU row at a time from the coefficient controller and dole it
    /// out as row groups to the postprocessor.
    /// 
    /// When need_context_rows is true, this controller guarantees that the buffer
    /// passed to postprocessing contains at least one row group's worth of samples
    /// above and below the row group(s) being processed.  Note that the context
    /// rows "above" the first passed row group appear at negative row offsets in
    /// the passed buffer.  At the top and bottom of the image, the required
    /// context rows are manufactured by duplicating the first or last real sample
    /// row; this avoids having special cases in the upsampling inner loops.
    /// 
    /// The amount of context is fixed at one row group just because that's a
    /// convenient number for this controller to work with.  The existing
    /// upsamplers really only need one sample row of context.  An upsampler
    /// supporting arbitrary output rescaling might wish for more than one row
    /// group of context when shrinking the image; tough, we don't handle that.
    /// (This is justified by the assumption that downsizing will be handled mostly
    /// by adjusting the DCT_scaled_size values, so that the actual scale factor at
    /// the upsample step needn't be much less than one.)
    /// 
    /// To provide the desired context, we have to retain the last two row groups
    /// of one iMCU row while reading in the next iMCU row.  (The last row group
    /// can't be processed until we have another row group for its below-context,
    /// and so we have to save the next-to-last group too for its above-context.)
    /// We could do this most simply by copying data around in our buffer, but
    /// that'd be very slow.  We can avoid copying any data by creating a rather
    /// strange pointer structure.  Here's how it works.  We allocate a workspace
    /// consisting of M+2 row groups (where M = min_DCT_scaled_size is the number
    /// of row groups per iMCU row).  We create two sets of redundant pointers to
    /// the workspace.  Labeling the physical row groups 0 to M+1, the synthesized
    /// pointer lists look like this:
    ///                   M+1                          M-1
    ///                   master pointer --> 0         master pointer --> 0
    ///                   1                            1
    ///                   ...                          ...
    ///                   M-3                          M-3
    ///                   M-2                           M
    ///                   M-1                          M+1
    ///                    M                           M-2
    ///                   M+1                          M-1
    ///                    0                            0
    /// We read alternate iMCU rows using each master pointer; thus the last two
    /// row groups of the previous iMCU row remain un-overwritten in the workspace.
    /// The pointer lists are set up so that the required context rows appear to
    /// be adjacent to the proper places when we pass the pointer lists to the
    /// upsampler.
    /// 
    /// The above pictures describe the normal state of the pointer lists.
    /// At top and bottom of the image, we diddle the pointer lists to duplicate
    /// the first or last sample row as necessary (this is cheaper than copying
    /// sample rows around).
    /// 
    /// This scheme breaks down if M less than 2, ie, min_DCT_scaled_size is 1.  In that
    /// situation each iMCU row provides only one row group so the buffering logic
    /// must be different (eg, we must read two iMCU rows before we can emit the
    /// first row group).  For now, we simply do not support providing context
    /// rows when min_DCT_scaled_size is 1.  That combination seems unlikely to
    /// be worth providing --- if someone wants a 1/8th-size preview, they probably
    /// want it quick and dirty, so a context-free upsampler is sufficient.
    /// </summary>
    class JpegDecompressorMainController
    {
        private enum DataProcessor
        {
            context_main,
            simple_main,
            crank_post
        }

        /* context_state values: */
        private const int CTX_PREPARE_FOR_IMCU = 0;   /* need to prepare for MCU row */
        private const int CTX_PROCESS_IMCU = 1;   /* feeding iMCU to postprocessor */
        private const int CTX_POSTPONED_ROW = 2;   /* feeding postponed row group */

        private DataProcessor m_dataProcessor;
        private JpegDecompressor m_cinfo;

        /* Pointer to allocated workspace (M or M+2 row groups). */
        private byte[][][] m_buffer = new byte[JpegConstants.MaxComponents][][];

        private bool m_buffer_full;       /* Have we gotten an iMCU row from decoder? */
        private int m_rowgroup_ctr;    /* counts row groups output to postprocessor */

        /* Remaining fields are only used in the context case. */

        private int[][][] m_funnyIndices = new int[2][][] { new int[JpegConstants.MaxComponents][], new int[JpegConstants.MaxComponents][] };
        private int[] m_funnyOffsets = new int[JpegConstants.MaxComponents];
        private int m_whichFunny;           /* indicates which funny indices set is now in use */

        private int m_context_state;      /* process_data state machine status */
        private int m_rowgroups_avail; /* row groups available to postprocessor */
        private int m_iMCU_row_ctr;    /* counts iMCU rows to detect image top/bot */

        public JpegDecompressorMainController(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Allocate the workspace.
            * ngroups is the number of row groups we need.
            */
            int ngroups = cinfo.m_min_DCT_scaled_size;
            if (cinfo.m_upsample.NeedContextRows())
            {
                if (cinfo.m_min_DCT_scaled_size < 2) /* unsupported, see comments above */
                    throw new Exception("Not implemented yet");

                alloc_funny_pointers(); /* Alloc space for xbuffer[] lists */
                ngroups = cinfo.m_min_DCT_scaled_size + 2;
            }

            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                /* height of a row group of component */
                int rgroup = (cinfo.Comp_info[ci].V_samp_factor * cinfo.Comp_info[ci].DCT_scaled_size) / cinfo.m_min_DCT_scaled_size;

                m_buffer[ci] = JpegCommonBase.AllocJpegSamples(
                    cinfo.Comp_info[ci].Width_in_blocks * cinfo.Comp_info[ci].DCT_scaled_size,
                    rgroup * ngroups);
            }
        }

        /// <summary>
        /// Initialize for a processing pass.
        /// </summary>
        public void start_pass(BufferMode pass_mode)
        {
            switch (pass_mode)
            {
                case BufferMode.PassThru:
                    if (m_cinfo.m_upsample.NeedContextRows())
                    {
                        m_dataProcessor = DataProcessor.context_main;
                        make_funny_pointers(); /* Create the xbuffer[] lists */
                        m_whichFunny = 0; /* Read first iMCU row into xbuffer[0] */
                        m_context_state = CTX_PREPARE_FOR_IMCU;
                        m_iMCU_row_ctr = 0;
                    }
                    else
                    {
                        /* Simple case with no context needed */
                        m_dataProcessor = DataProcessor.simple_main;
                    }
                    m_buffer_full = false;  /* Mark buffer empty */
                    m_rowgroup_ctr = 0;
                    break;
                case BufferMode.CrankDest:
                    /* For last pass of 2-pass quantization, just crank the postprocessor */
                    m_dataProcessor = DataProcessor.crank_post;
                    break;
                default:
                    throw new Exception("Bogus buffer control mode");
            }
        }

        public void process_data(byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            switch (m_dataProcessor)
            {
                case DataProcessor.simple_main:
                    process_data_simple_main(output_buf, ref out_row_ctr, out_rows_avail);
                    break;

                case DataProcessor.context_main:
                    process_data_context_main(output_buf, ref out_row_ctr, out_rows_avail);
                    break;

                case DataProcessor.crank_post:
                    process_data_crank_post(output_buf, ref out_row_ctr, out_rows_avail);
                    break;

                default:
                    throw new Exception("Not implemented yet");
            }
        }

        /// <summary>
        /// Process some data.
        /// This handles the simple case where no context is required.
        /// </summary>
        private void process_data_simple_main(byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            ComponentBuffer[] cb = new ComponentBuffer[JpegConstants.MaxComponents];
            for (int i = 0; i < JpegConstants.MaxComponents; i++)
            {
                cb[i] = new ComponentBuffer();
                cb[i].SetBuffer(m_buffer[i], null, 0);
            }

            /* Read input data if we haven't filled the main buffer yet */
            if (!m_buffer_full)
            {
                if (m_cinfo.m_coef.decompress_data(cb) == ReadResult.Suspended)
                {
                    /* suspension forced, can do nothing more */
                    return;
                }

                /* OK, we have an iMCU row to work with */
                m_buffer_full = true;
            }

            /* There are always min_DCT_scaled_size row groups in an iMCU row. */
            int rowgroups_avail = m_cinfo.m_min_DCT_scaled_size;

            /* Note: at the bottom of the image, we may pass extra garbage row groups
             * to the postprocessor.  The postprocessor has to check for bottom
             * of image anyway (at row resolution), so no point in us doing it too.
             */

            /* Feed the postprocessor */
            m_cinfo.m_post.post_process_data(cb, ref m_rowgroup_ctr, rowgroups_avail, output_buf, ref out_row_ctr, out_rows_avail);

            /* Has postprocessor consumed all the data yet? If so, mark buffer empty */
            if (m_rowgroup_ctr >= rowgroups_avail)
            {
                m_buffer_full = false;
                m_rowgroup_ctr = 0;
            }
        }

        /// <summary>
        /// Process some data.
        /// This handles the case where context rows must be provided.
        /// </summary>
        private void process_data_context_main(byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            ComponentBuffer[] cb = new ComponentBuffer[m_cinfo.m_num_components];
            for (int i = 0; i < m_cinfo.m_num_components; i++)
            {
                cb[i] = new ComponentBuffer();
                cb[i].SetBuffer(m_buffer[i], m_funnyIndices[m_whichFunny][i], m_funnyOffsets[i]);
            }

            /* Read input data if we haven't filled the main buffer yet */
            if (!m_buffer_full)
            {
                if (m_cinfo.m_coef.decompress_data(cb) == ReadResult.Suspended)
                {
                    /* suspension forced, can do nothing more */
                    return;
                }

                /* OK, we have an iMCU row to work with */
                m_buffer_full = true;

                /* count rows received */
                m_iMCU_row_ctr++;
            }

            /* Postprocessor typically will not swallow all the input data it is handed
             * in one call (due to filling the output buffer first).  Must be prepared
             * to exit and restart.
     
     
             This switch lets us keep track of how far we got.
             * Note that each case falls through to the next on successful completion.
             */
            if (m_context_state == CTX_POSTPONED_ROW)
            {
                /* Call postprocessor using previously set pointers for postponed row */
                m_cinfo.m_post.post_process_data(cb, ref m_rowgroup_ctr,
                    m_rowgroups_avail, output_buf, ref out_row_ctr, out_rows_avail);

                if (m_rowgroup_ctr < m_rowgroups_avail)
                {
                    /* Need to suspend */
                    return;
                }

                m_context_state = CTX_PREPARE_FOR_IMCU;

                if (out_row_ctr >= out_rows_avail)
                {
                    /* Postprocessor exactly filled output buf */
                    return;
                }
            }

            if (m_context_state == CTX_PREPARE_FOR_IMCU)
            {
                /* Prepare to process first M-1 row groups of this iMCU row */
                m_rowgroup_ctr = 0;
                m_rowgroups_avail = m_cinfo.m_min_DCT_scaled_size - 1;

                /* Check for bottom of image: if so, tweak pointers to "duplicate"
                 * the last sample row, and adjust rowgroups_avail to ignore padding rows.
                 */
                if (m_iMCU_row_ctr == m_cinfo.m_total_iMCU_rows)
                    set_bottom_pointers();

                m_context_state = CTX_PROCESS_IMCU;
            }

            if (m_context_state == CTX_PROCESS_IMCU)
            {
                /* Call postprocessor using previously set pointers */
                m_cinfo.m_post.post_process_data(cb, ref m_rowgroup_ctr,
                    m_rowgroups_avail, output_buf, ref out_row_ctr, out_rows_avail);

                if (m_rowgroup_ctr < m_rowgroups_avail)
                {
                    /* Need to suspend */
                    return;
                }

                /* After the first iMCU, change wraparound pointers to normal state */
                if (m_iMCU_row_ctr == 1)
                    set_wraparound_pointers();

                /* Prepare to load new iMCU row using other xbuffer list */
                m_whichFunny ^= 1;    /* 0=>1 or 1=>0 */
                m_buffer_full = false;

                /* Still need to process last row group of this iMCU row, */
                /* which is saved at index M+1 of the other xbuffer */
                m_rowgroup_ctr = m_cinfo.m_min_DCT_scaled_size + 1;
                m_rowgroups_avail = m_cinfo.m_min_DCT_scaled_size + 2;
                m_context_state = CTX_POSTPONED_ROW;
            }
        }

        /// <summary>
        /// Process some data.
        /// Final pass of two-pass quantization: just call the postprocessor.
        /// Source data will be the postprocessor controller's internal buffer.
        /// </summary>
        private void process_data_crank_post(byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            int dummy = 0;
            m_cinfo.m_post.post_process_data(null, ref dummy, 0, output_buf, ref out_row_ctr, out_rows_avail);
        }

        /// <summary>
        /// Allocate space for the funny pointer lists.
        /// This is done only once, not once per pass.
        /// </summary>
        private void alloc_funny_pointers()
        {
            int M = m_cinfo.m_min_DCT_scaled_size;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                /* height of a row group of component */
                int rgroup = (m_cinfo.Comp_info[ci].V_samp_factor * m_cinfo.Comp_info[ci].DCT_scaled_size) / m_cinfo.m_min_DCT_scaled_size;

                /* Get space for pointer lists --- M+4 row groups in each list.
                 */
                m_funnyIndices[0][ci] = new int[rgroup * (M + 4)];
                m_funnyIndices[1][ci] = new int[rgroup * (M + 4)];
                m_funnyOffsets[ci] = rgroup;
            }
        }

        /// <summary>
        /// Create the funny pointer lists discussed in the comments above.
        /// The actual workspace is already allocated (in main.buffer),
        /// and the space for the pointer lists is allocated too.
        /// This routine just fills in the curiously ordered lists.
        /// This will be repeated at the beginning of each pass.
        /// </summary>
        private void make_funny_pointers()
        {
            int M = m_cinfo.m_min_DCT_scaled_size;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                /* height of a row group of component */
                int rgroup = (m_cinfo.Comp_info[ci].V_samp_factor * m_cinfo.Comp_info[ci].DCT_scaled_size) / m_cinfo.m_min_DCT_scaled_size;

                int[] ind0 = m_funnyIndices[0][ci];
                int[] ind1 = m_funnyIndices[1][ci];

                /* First copy the workspace pointers as-is */
                for (int i = 0; i < rgroup * (M + 2); i++)
                {
                    ind0[i + rgroup] = i;
                    ind1[i + rgroup] = i;
                }

                /* In the second list, put the last four row groups in swapped order */
                for (int i = 0; i < rgroup * 2; i++)
                {
                    ind1[rgroup * (M - 1) + i] = rgroup * M + i;
                    ind1[rgroup * (M + 1) + i] = rgroup * (M - 2) + i;
                }

                /* The wraparound pointers at top and bottom will be filled later
                 * (see set_wraparound_pointers, below).  Initially we want the "above"
                 * pointers to duplicate the first actual data line.  This only needs
                 * to happen in xbuffer[0].
                 */
                for (int i = 0; i < rgroup; i++)
                    ind0[i] = ind0[rgroup];
            }
        }

        /// <summary>
        /// Set up the "wraparound" pointers at top and bottom of the pointer lists.
        /// This changes the pointer list state from top-of-image to the normal state.
        /// </summary>
        private void set_wraparound_pointers()
        {
            int M = m_cinfo.m_min_DCT_scaled_size;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                /* height of a row group of component */
                int rgroup = (m_cinfo.Comp_info[ci].V_samp_factor * m_cinfo.Comp_info[ci].DCT_scaled_size) / m_cinfo.m_min_DCT_scaled_size;

                int[] ind0 = m_funnyIndices[0][ci];
                int[] ind1 = m_funnyIndices[1][ci];

                for (int i = 0; i < rgroup; i++)
                {
                    ind0[i] = ind0[rgroup * (M + 2) + i];
                    ind1[i] = ind1[rgroup * (M + 2) + i];

                    ind0[rgroup * (M + 3) + i] = ind0[i + rgroup];
                    ind1[rgroup * (M + 3) + i] = ind1[i + rgroup];
                }
            }
        }

        /// <summary>
        /// Change the pointer lists to duplicate the last sample row at the bottom
        /// of the image.  m_whichFunny indicates which m_funnyIndices holds the final iMCU row.
        /// Also sets rowgroups_avail to indicate number of nondummy row groups in row.
        /// </summary>
        private void set_bottom_pointers()
        {
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                /* Count sample rows in one iMCU row and in one row group */
                int iMCUheight = m_cinfo.Comp_info[ci].V_samp_factor * m_cinfo.Comp_info[ci].DCT_scaled_size;
                int rgroup = iMCUheight / m_cinfo.m_min_DCT_scaled_size;

                /* Count nondummy sample rows remaining for this component */
                int rows_left = m_cinfo.Comp_info[ci].downsampled_height % iMCUheight;
                if (rows_left == 0)
                    rows_left = iMCUheight;

                /* Count nondummy row groups.  Should get same answer for each component,
                 * so we need only do it once.
                 */
                if (ci == 0)
                    m_rowgroups_avail = (rows_left - 1) / rgroup + 1;

                /* Duplicate the last real sample row rgroup*2 times; this pads out the
                 * last partial rowgroup and ensures at least one full rowgroup of context.
                 */
                for (int i = 0; i < rgroup * 2; i++)
                    m_funnyIndices[m_whichFunny][ci][rows_left + i + rgroup] = m_funnyIndices[m_whichFunny][ci][rows_left - 1 + rgroup];
            }
        }
    }
    #endregion

    #region JpegDecompressorMaster
    /// <summary>
    /// Master control module
    /// </summary>
    class JpegDecompressorMaster
    {
        private JpegDecompressor m_cinfo;

        private int m_pass_number;        /* # of passes completed */
        private bool m_is_dummy_pass; /* True during 1st pass for 2-pass quant */

        private bool m_using_merged_upsample; /* true if using merged upsample/cconvert */

        /* Saved references to initialized quantizer modules,
        * in case we need to switch modes.
        */
        private ColorQuantizer m_quantizer_1pass;
        private ColorQuantizer m_quantizer_2pass;

        public JpegDecompressorMaster(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;
            master_selection();
        }

        /// <summary>
        /// Per-pass setup.
        /// This is called at the beginning of each output pass.  We determine which
        /// modules will be active during this pass and give them appropriate
        /// start_pass calls.  We also set is_dummy_pass to indicate whether this
        /// is a "real" output pass or a dummy pass for color quantization.
        /// (In the latter case, we will crank the pass to completion.)
        /// </summary>
        public void prepare_for_output_pass()
        {
            if (m_is_dummy_pass)
            {
                /* Final pass of 2-pass quantization */
                m_is_dummy_pass = false;
                m_cinfo.m_cquantize.start_pass(false);
                m_cinfo.m_post.start_pass(BufferMode.CrankDest);
                m_cinfo.m_main.start_pass(BufferMode.CrankDest);
            }
            else
            {
                if (m_cinfo.m_quantize_colors && m_cinfo.m_colormap == null)
                {
                    /* Select new quantization method */
                    if (m_cinfo.m_two_pass_quantize && m_cinfo.m_enable_2pass_quant)
                    {
                        m_cinfo.m_cquantize = m_quantizer_2pass;
                        m_is_dummy_pass = true;
                    }
                    else if (m_cinfo.m_enable_1pass_quant)
                        m_cinfo.m_cquantize = m_quantizer_1pass;
                    else
                        throw new Exception("Invalid color quantization mode change");
                }

                m_cinfo.m_idct.start_pass();
                m_cinfo.m_coef.start_output_pass();

                if (!m_cinfo.m_raw_data_out)
                {
                    m_cinfo.m_upsample.start_pass();

                    if (m_cinfo.m_quantize_colors)
                        m_cinfo.m_cquantize.start_pass(m_is_dummy_pass);

                    m_cinfo.m_post.start_pass((m_is_dummy_pass ? BufferMode.SaveAndPass : BufferMode.PassThru));
                    m_cinfo.m_main.start_pass(BufferMode.PassThru);
                }
            }
        }

        /// <summary>
        /// Finish up at end of an output pass.
        /// </summary>
        public void finish_output_pass()
        {
            if (m_cinfo.m_quantize_colors)
                m_cinfo.m_cquantize.finish_pass();

            m_pass_number++;
        }

        public bool IsDummyPass()
        {
            return m_is_dummy_pass;
        }

        /// <summary>
        /// Master selection of decompression modules.
        /// This is done once at jpeg_start_decompress time.  We determine
        /// which modules will be used and give them appropriate initialization calls.
        /// We also initialize the decompressor input side to begin consuming data.
        /// 
        /// Since jpeg_read_header has finished, we know what is in the SOF
        /// and (first) SOS markers.  We also have all the application parameter
        /// settings.
        /// </summary>
        private void master_selection()
        {
            /* Initialize dimensions and other stuff */
            m_cinfo.jpeg_calc_output_dimensions();
            prepare_range_limit_table();

            /* Width of an output scanline must be representable as int. */
            long samplesperrow = m_cinfo.m_output_width * m_cinfo.m_out_color_components;
            int jd_samplesperrow = (int)samplesperrow;
            if ((long)jd_samplesperrow != samplesperrow)
                throw new Exception("Image too wide for this implementation");

            /* Initialize my private state */
            m_pass_number = 0;
            m_using_merged_upsample = m_cinfo.use_merged_upsample();

            /* Color quantizer selection */
            m_quantizer_1pass = null;
            m_quantizer_2pass = null;

            /* No mode changes if not using buffered-image mode. */
            if (!m_cinfo.m_quantize_colors || !m_cinfo.m_buffered_image)
            {
                m_cinfo.m_enable_1pass_quant = false;
                m_cinfo.m_enable_external_quant = false;
                m_cinfo.m_enable_2pass_quant = false;
            }

            if (m_cinfo.m_quantize_colors)
            {
                if (m_cinfo.m_raw_data_out)
                    throw new Exception("Not implemented yet");

                /* 2-pass quantizer only works in 3-component color space. */
                if (m_cinfo.m_out_color_components != 3)
                {
                    m_cinfo.m_enable_1pass_quant = true;
                    m_cinfo.m_enable_external_quant = false;
                    m_cinfo.m_enable_2pass_quant = false;
                    m_cinfo.m_colormap = null;
                }
                else if (m_cinfo.m_colormap != null)
                    m_cinfo.m_enable_external_quant = true;
                else if (m_cinfo.m_two_pass_quantize)
                    m_cinfo.m_enable_2pass_quant = true;
                else
                    m_cinfo.m_enable_1pass_quant = true;

                if (m_cinfo.m_enable_1pass_quant)
                {
                    m_cinfo.m_cquantize = new Pass1ColorQuantizer(m_cinfo);
                    m_quantizer_1pass = m_cinfo.m_cquantize;
                }

                /* We use the 2-pass code to map to external colormaps. */
                if (m_cinfo.m_enable_2pass_quant || m_cinfo.m_enable_external_quant)
                {
                    m_cinfo.m_cquantize = new Pass2ColorQuantizer(m_cinfo);
                    m_quantizer_2pass = m_cinfo.m_cquantize;
                }
                /* If both quantizers are initialized, the 2-pass one is left active;
                 * this is necessary for starting with quantization to an external map.
                 */
            }

            /* Post-processing: in particular, color conversion first */
            if (!m_cinfo.m_raw_data_out)
            {
                if (m_using_merged_upsample)
                {
                    /* does color conversion too */
                    m_cinfo.m_upsample = new MergedUpsampler(m_cinfo);
                }
                else
                {
                    m_cinfo.m_cconvert = new ColorDeconverter(m_cinfo);
                    m_cinfo.m_upsample = new UpsamplerImpl(m_cinfo);
                }

                m_cinfo.m_post = new JpegDecompressorPostController(m_cinfo, m_cinfo.m_enable_2pass_quant);
            }

            /* Inverse DCT */
            m_cinfo.m_idct = new JpegInverseDCT(m_cinfo);

            if (m_cinfo.m_progressive_mode)
                m_cinfo.m_entropy = new ProgressiveHuffmanDecoder(m_cinfo);
            else
                m_cinfo.m_entropy = new HuffEntropyDecoder(m_cinfo);

            /* Initialize principal buffer controllers. */
            bool use_c_buffer = m_cinfo.m_inputctl.HasMultipleScans() || m_cinfo.m_buffered_image;
            m_cinfo.m_coef = new JpegDecompressorCoefController(m_cinfo, use_c_buffer);

            if (!m_cinfo.m_raw_data_out)
                m_cinfo.m_main = new JpegDecompressorMainController(m_cinfo);

            /* Initialize input side of decompressor to consume first scan. */
            m_cinfo.m_inputctl.start_input_pass();
        }

        /// <summary>
        /// Allocate and fill in the sample_range_limit table.
        /// 
        /// Several decompression processes need to range-limit values to the range
        /// 0..MaxSampleValue; the input value may fall somewhat outside this range
        /// due to noise introduced by quantization, roundoff error, etc. These
        /// processes are inner loops and need to be as fast as possible. On most
        /// machines, particularly CPUs with pipelines or instruction prefetch,
        /// a (subscript-check-less) C table lookup
        ///     x = sample_range_limit[x];
        /// is faster than explicit tests
        /// <c>
        ///     if (x &amp; 0)
        ///        x = 0;
        ///     else if (x > MaxSampleValue)
        ///        x = MaxSampleValue;
        /// </c>
        /// These processes all use a common table prepared by the routine below.
        /// 
        /// For most steps we can mathematically guarantee that the initial value
        /// of x is within MaxSampleValue + 1 of the legal range, so a table running from
        /// -(MaxSampleValue + 1) to 2 * MaxSampleValue + 1 is sufficient.  But for the initial
        /// limiting step (just after the IDCT), a wildly out-of-range value is 
        /// possible if the input data is corrupt.  To avoid any chance of indexing
        /// off the end of memory and getting a bad-pointer trap, we perform the
        /// post-IDCT limiting thus: <c>x = range_limit[x &amp; Mask];</c>
        /// where Mask is 2 bits wider than legal sample data, ie 10 bits for 8-bit
        /// samples.  Under normal circumstances this is more than enough range and
        /// a correct output will be generated; with bogus input data the mask will
        /// cause wraparound, and we will safely generate a bogus-but-in-range output.
        /// For the post-IDCT step, we want to convert the data from signed to unsigned
        /// representation by adding MediumSampleValue at the same time that we limit it.
        /// So the post-IDCT limiting table ends up looking like this:
        /// <pre>
        ///     MediumSampleValue, MediumSampleValue + 1, ..., MaxSampleValue,
        ///     MaxSampleValue (repeat 2 * (MaxSampleValue + 1) - MediumSampleValue times),
        ///     0          (repeat 2 * (MaxSampleValue + 1) - MediumSampleValue times),
        ///     0, 1, ..., MediumSampleValue - 1
        /// </pre>
        /// Negative inputs select values from the upper half of the table after
        /// masking.
        /// 
        /// We can save some space by overlapping the start of the post-IDCT table
        /// with the simpler range limiting table.  The post-IDCT table begins at
        /// sample_range_limit + MediumSampleValue.
        /// 
        /// Note that the table is allocated in near data space on PCs; it's small
        /// enough and used often enough to justify this.
        /// </summary>
        private void prepare_range_limit_table()
        {
            byte[] table = new byte[5 * (JpegConstants.MaxSampleValue + 1) + JpegConstants.MediumSampleValue];

            /* allow negative subscripts of simple table */
            int tableOffset = JpegConstants.MaxSampleValue + 1;
            m_cinfo.m_sample_range_limit = table;
            m_cinfo.m_sampleRangeLimitOffset = tableOffset;

            /* First segment of "simple" table: limit[x] = 0 for x < 0 */
            Array.Clear(table, 0, JpegConstants.MaxSampleValue + 1);

            /* Main part of "simple" table: limit[x] = x */
            for (int i = 0; i <= JpegConstants.MaxSampleValue; i++)
                table[tableOffset + i] = (byte)i;

            tableOffset += JpegConstants.MediumSampleValue; /* Point to where post-IDCT table starts */

            /* End of simple table, rest of first half of post-IDCT table */
            for (int i = JpegConstants.MediumSampleValue; i < 2 * (JpegConstants.MaxSampleValue + 1); i++)
                table[tableOffset + i] = JpegConstants.MaxSampleValue;

            /* Second half of post-IDCT table */
            Array.Clear(table, tableOffset + 2 * (JpegConstants.MaxSampleValue + 1),
                2 * (JpegConstants.MaxSampleValue + 1) - JpegConstants.MediumSampleValue);

            Buffer.BlockCopy(m_cinfo.m_sample_range_limit, 0, table,
                tableOffset + 4 * (JpegConstants.MaxSampleValue + 1) - JpegConstants.MediumSampleValue, JpegConstants.MediumSampleValue);
        }
    }
    #endregion

    #region JpegDecompressorPostController
    /// <summary>
    /// Decompression postprocessing (color quantization buffer control)
    /// </summary>
    class JpegDecompressorPostController
    {
        private enum ProcessorType
        {
            OnePass,
            PrePass,
            Upsample,
            SecondPass
        }

        private ProcessorType m_processor;

        private JpegDecompressor m_cinfo;

        /* Color quantization source buffer: this holds output data from
        * the upsample/color conversion step to be passed to the quantizer.
        * For two-pass color quantization, we need a full-image buffer;
        * for one-pass operation, a strip buffer is sufficient.
        */
        private JpegVirtualArray<byte> m_whole_image;  /* virtual array, or null if one-pass */
        private byte[][] m_buffer;       /* strip buffer, or current strip of virtual */
        private int m_strip_height;    /* buffer size in rows */
        /* for two-pass mode only: */
        private int m_starting_row;    /* row # of first row in current strip */
        private int m_next_row;        /* index of next row to fill/empty in strip */

        /// <summary>
        /// Initialize postprocessing controller.
        /// </summary>
        public JpegDecompressorPostController(JpegDecompressor cinfo, bool need_full_buffer)
        {
            m_cinfo = cinfo;

            /* Create the quantization buffer, if needed */
            if (cinfo.m_quantize_colors)
            {
                /* The buffer strip height is max_v_samp_factor, which is typically
                * an efficient number of rows for upsampling to return.
                * (In the presence of output rescaling, we might want to be smarter?)
                */
                m_strip_height = cinfo.m_max_v_samp_factor;

                if (need_full_buffer)
                {
                    /* Two-pass color quantization: need full-image storage. */
                    /* We round up the number of rows to a multiple of the strip height. */
                    m_whole_image = JpegCommonBase.CreateSamplesArray(
                        cinfo.m_output_width * cinfo.m_out_color_components,
                        JpegUtils.jround_up(cinfo.m_output_height, m_strip_height));
                    m_whole_image.ErrorProcessor = cinfo;
                }
                else
                {
                    /* One-pass color quantization: just make a strip buffer. */
                    m_buffer = JpegCommonBase.AllocJpegSamples(
                        cinfo.m_output_width * cinfo.m_out_color_components, m_strip_height);
                }
            }
        }

        /// <summary>
        /// Initialize for a processing pass.
        /// </summary>
        public void start_pass(BufferMode pass_mode)
        {
            switch (pass_mode)
            {
                case BufferMode.PassThru:
                    if (m_cinfo.m_quantize_colors)
                    {
                        /* Single-pass processing with color quantization. */
                        m_processor = ProcessorType.OnePass;
                        /* We could be doing buffered-image output before starting a 2-pass
                         * color quantization; in that case, jinit_d_post_controller did not
                         * allocate a strip buffer.  Use the virtual-array buffer as workspace.
                         */
                        if (m_buffer == null)
                            m_buffer = m_whole_image.Access(0, m_strip_height);
                    }
                    else
                    {
                        /* For single-pass processing without color quantization,
                         * I have no work to do; just call the upsampler directly.
                         */
                        m_processor = ProcessorType.Upsample;
                    }
                    break;
                case BufferMode.SaveAndPass:
                    /* First pass of 2-pass quantization */
                    if (m_whole_image == null)
                        throw new Exception("Bogus buffer control mode");

                    m_processor = ProcessorType.PrePass;
                    break;
                case BufferMode.CrankDest:
                    /* Second pass of 2-pass quantization */
                    if (m_whole_image == null)
                        throw new Exception("Bogus buffer control mode");

                    m_processor = ProcessorType.SecondPass;
                    break;
                default:
                    throw new Exception("Bogus buffer control mode");
            }
            m_starting_row = m_next_row = 0;
        }

        public void post_process_data(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            switch (m_processor)
            {
                case ProcessorType.OnePass:
                    post_process_1pass(input_buf, ref in_row_group_ctr, in_row_groups_avail, output_buf, ref out_row_ctr, out_rows_avail);
                    break;
                case ProcessorType.PrePass:
                    post_process_prepass(input_buf, ref in_row_group_ctr, in_row_groups_avail, ref out_row_ctr);
                    break;
                case ProcessorType.Upsample:
                    m_cinfo.m_upsample.upsample(input_buf, ref in_row_group_ctr, in_row_groups_avail, output_buf, ref out_row_ctr, out_rows_avail);
                    break;
                case ProcessorType.SecondPass:
                    post_process_2pass(output_buf, ref out_row_ctr, out_rows_avail);
                    break;
                default:
                    throw new Exception("Not implemented yet");
            }
        }

        /// <summary>
        /// Process some data in the one-pass (strip buffer) case.
        /// This is used for color precision reduction as well as one-pass quantization.
        /// </summary>
        private void post_process_1pass(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            /* Fill the buffer, but not more than what we can dump out in one go. */
            /* Note we rely on the upsampler to detect bottom of image. */
            int max_rows = out_rows_avail - out_row_ctr;
            if (max_rows > m_strip_height)
                max_rows = m_strip_height;

            int num_rows = 0;
            m_cinfo.m_upsample.upsample(input_buf, ref in_row_group_ctr, in_row_groups_avail, m_buffer, ref num_rows, max_rows);

            /* Quantize and emit data. */
            m_cinfo.m_cquantize.color_quantize(m_buffer, 0, output_buf, out_row_ctr, num_rows);
            out_row_ctr += num_rows;
        }

        /// <summary>
        /// Process some data in the first pass of 2-pass quantization.
        /// </summary>
        private void post_process_prepass(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, ref int out_row_ctr)
        {
            int old_next_row, num_rows;

            /* Reposition virtual buffer if at start of strip. */
            if (m_next_row == 0)
                m_buffer = m_whole_image.Access(m_starting_row, m_strip_height);

            /* Upsample some data (up to a strip height's worth). */
            old_next_row = m_next_row;
            m_cinfo.m_upsample.upsample(input_buf, ref in_row_group_ctr, in_row_groups_avail, m_buffer, ref m_next_row, m_strip_height);

            /* Allow quantizer to scan new data.  No data is emitted, */
            /* but we advance out_row_ctr so outer loop can tell when we're done. */
            if (m_next_row > old_next_row)
            {
                num_rows = m_next_row - old_next_row;
                m_cinfo.m_cquantize.color_quantize(m_buffer, old_next_row, null, 0, num_rows);
                out_row_ctr += num_rows;
            }

            /* Advance if we filled the strip. */
            if (m_next_row >= m_strip_height)
            {
                m_starting_row += m_strip_height;
                m_next_row = 0;
            }
        }

        /// <summary>
        /// Process some data in the second pass of 2-pass quantization.
        /// </summary>
        private void post_process_2pass(byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            int num_rows, max_rows;

            /* Reposition virtual buffer if at start of strip. */
            if (m_next_row == 0)
                m_buffer = m_whole_image.Access(m_starting_row, m_strip_height);

            /* Determine number of rows to emit. */
            num_rows = m_strip_height - m_next_row; /* available in strip */
            max_rows = out_rows_avail - out_row_ctr; /* available in output area */
            if (num_rows > max_rows)
                num_rows = max_rows;

            /* We have to check bottom of image here, can't depend on upsampler. */
            max_rows = m_cinfo.m_output_height - m_starting_row;
            if (num_rows > max_rows)
                num_rows = max_rows;

            /* Quantize and emit data. */
            m_cinfo.m_cquantize.color_quantize(m_buffer, m_next_row, output_buf, out_row_ctr, num_rows);
            out_row_ctr += num_rows;

            /* Advance if we filled the strip. */
            m_next_row += num_rows;
            if (m_next_row >= m_strip_height)
            {
                m_starting_row += m_strip_height;
                m_next_row = 0;
            }
        }
    }
    #endregion

    #region JpegCommonBase
    /// <summary>Base class for both JPEG compressor and decompresor.</summary>
    /// <remarks>
    /// Routines that are to be used by both halves of the library are declared
    /// to receive an instance of this class. There are no actual instances of 
    /// <see cref="JpegCommonBase"/>, only of <see cref="JpegCompressor"/> 
    /// and <see cref="JpegDecompressor"/>
    /// </remarks>
    public abstract class JpegCommonBase
    {
        internal enum JpegState
        {
            DESTROYED = 0,
            CSTATE_START = 100,     /* after create_compress */
            CSTATE_SCANNING = 101,  /* start_compress done, write_scanlines OK */
            CSTATE_RAW_OK = 102,    /* start_compress done, write_raw_data OK */
            CSTATE_WRCOEFS = 103,   /* jpeg_write_coefficients done */
            DSTATE_START = 200,     /* after create_decompress */
            DSTATE_INHEADER = 201,  /* reading header markers, no SOS yet */
            DSTATE_READY = 202,     /* found SOS, ready for start_decompress */
            DSTATE_PRELOAD = 203,   /* reading multi-scan file in start_decompress*/
            DSTATE_PRESCAN = 204,   /* performing dummy pass for 2-pass quant */
            DSTATE_SCANNING = 205,  /* start_decompress done, read_scanlines OK */
            DSTATE_RAW_OK = 206,    /* start_decompress done, read_raw_data OK */
            DSTATE_BUFIMAGE = 207,  /* expecting jpeg_start_output */
            DSTATE_BUFPOST = 208,   /* looking for SOS/EOI in jpeg_finish_output */
            DSTATE_RDCOEFS = 209,   /* reading file in jpeg_read_coefficients */
            DSTATE_STOPPING = 210   /* looking for EOI in jpeg_finish_decompress */
        }

        internal JpegState m_global_state;     /* For checking call sequence validity */

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <seealso cref="JpegCompressor"/>
        /// <seealso cref="JpegDecompressor"/>
        public JpegCommonBase()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance is Jpeg decompressor.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this is Jpeg decompressor; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsDecompressor
        {
            get;
        }

        /// <summary>
        /// Gets the version of LibJpeg.
        /// </summary>
        /// <value>The version of LibJpeg.</value>
        public static string Version
        {
            get
            {
                return "Special Compilation for Cosmos. Based on version 1.2.300.0 of LibJpeg.Net";
            }
        }

        /// <summary>
        /// Gets the LibJpeg's copyright.
        /// </summary>
        /// <value>The copyright.</value>
        public static string Copyright
        {
            get
            {
                return "Copyright (C) 2008-2011, Bit Miracle";
            }
        }

        /// <summary>
        /// Creates the array of samples.
        /// </summary>
        /// <param name="samplesPerRow">The number of samples in row.</param>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <returns>The array of samples.</returns>
        public static JpegVirtualArray<byte> CreateSamplesArray(int samplesPerRow, int numberOfRows)
        {
            return new JpegVirtualArray<byte>(samplesPerRow, numberOfRows, AllocJpegSamples);
        }

        /// <summary>
        /// Creates the array of blocks.
        /// </summary>
        /// <param name="blocksPerRow">The number of blocks in row.</param>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <returns>The array of blocks.</returns>
        /// <seealso cref="JpegBlock"/>
        public static JpegVirtualArray<JpegBlock> CreateBlocksArray(int blocksPerRow, int numberOfRows)
        {
            return new JpegVirtualArray<JpegBlock>(blocksPerRow, numberOfRows, allocJpegBlocks);
        }

        /// <summary>
        /// Creates 2-D sample array.
        /// </summary>
        /// <param name="samplesPerRow">The number of samples per row.</param>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <returns>The array of samples.</returns>
        public static byte[][] AllocJpegSamples(int samplesPerRow, int numberOfRows)
        {
            byte[][] result = new byte[numberOfRows][];
            for (int i = 0; i < numberOfRows; ++i)
                result[i] = new byte[samplesPerRow];

            return result;
        }

        // Creation of 2-D block arrays.
        private static JpegBlock[][] allocJpegBlocks(int blocksPerRow, int numberOfRows)
        {
            JpegBlock[][] result = new JpegBlock[numberOfRows][];
            for (int i = 0; i < numberOfRows; ++i)
            {
                result[i] = new JpegBlock[blocksPerRow];
                for (int j = 0; j < blocksPerRow; ++j)
                    result[i][j] = new JpegBlock();
            }
            return result;
        }

        // Generic versions of jpeg_abort and jpeg_destroy that work on either
        // flavor of JPEG object.  These may be more convenient in some places.

        /// <summary>
        /// Abort processing of a JPEG compression or decompression operation,
        /// but don't destroy the object itself.
        /// 
        /// Closing a data source or destination, if necessary, is the 
        /// application's responsibility.
        /// </summary>
        public void jpeg_abort()
        {
            /* Reset overall state for possible reuse of object */
            if (IsDecompressor)
            {
                m_global_state = JpegState.DSTATE_START;

                /* Try to keep application from accessing now-deleted marker list.
                 * A bit kludgy to do it here, but this is the most central place.
                 */
                JpegDecompressor s = this as JpegDecompressor;
                if (s != null)
                    s.m_marker_list = null;
            }
            else
            {
                m_global_state = JpegState.CSTATE_START;
            }
        }

        /// <summary>
        /// Destruction of a JPEG object. 
        /// 
        /// Closing a data source or destination, if necessary, is the 
        /// application's responsibility.
        /// </summary>
        public void jpeg_destroy()
        {
            // mark it destroyed
            m_global_state = JpegState.DESTROYED;
        }
    }
    #endregion

    #region JpegComponent
    /// <summary>
    /// Basic info about one component (color channel).
    /// </summary>
    public class JpegComponent
    {
        /* These values are fixed over the whole image. */
        /* For compression, they must be supplied by parameter setup; */
        /* for decompression, they are read from the SOF marker. */

        private int component_id;
        private int component_index;
        private int h_samp_factor;
        private int v_samp_factor;
        private int quant_tbl_no;

        /* These values may vary between scans. */
        /* For compression, they must be supplied by parameter setup; */
        /* for decompression, they are read from the SOS marker. */
        /* The decompressor output side may not use these variables. */
        private int dc_tbl_no;
        private int ac_tbl_no;

        /* Remaining fields should be treated as private by applications. */

        /* These values are computed during compression or decompression startup: */
        /* Component's size in DCT blocks.
         * Any dummy blocks added to complete an MCU are not counted; therefore
         * these values do not depend on whether a scan is interleaved or not.
         */
        private int width_in_blocks;
        internal int height_in_blocks;
        /* Size of a DCT block in samples.  Always DCTSize for compression.
         * For decompression this is the size of the output from one DCT block,
         * reflecting any scaling we choose to apply during the IDCT step.
         * Values of 1,2,4,8 are likely to be supported.  Note that different
         * components may receive different IDCT scalings.
         */
        internal int DCT_scaled_size;
        /* The downsampled dimensions are the component's actual, unpadded number
         * of samples at the main buffer (preprocessing/compression interface), thus
         * downsampled_width = ceil(image_width * Hi/Hmax)
         * and similarly for height.  For decompression, IDCT scaling is included, so
         * downsampled_width = ceil(image_width * Hi/Hmax * DCT_scaled_size/DCTSize)
         */
        internal int downsampled_width;    /* actual width in samples */

        internal int downsampled_height; /* actual height in samples */
        /* This flag is used only for decompression.  In cases where some of the
         * components will be ignored (eg grayscale output from YCbCr image),
         * we can skip most computations for the unused components.
         */
        internal bool component_needed;  /* do we need the value of this component? */

        /* These values are computed before starting a scan of the component. */
        /* The decompressor output side may not use these variables. */
        internal int MCU_width;      /* number of blocks per MCU, horizontally */
        internal int MCU_height;     /* number of blocks per MCU, vertically */
        internal int MCU_blocks;     /* MCU_width * MCU_height */
        internal int MCU_sample_width;       /* MCU width in samples, MCU_width*DCT_scaled_size */
        internal int last_col_width;     /* # of non-dummy blocks across in last MCU */
        internal int last_row_height;        /* # of non-dummy blocks down in last MCU */

        /* Saved quantization table for component; null if none yet saved.
         * See JpegInputController comments about the need for this information.
         * This field is currently used only for decompression.
         */
        internal JpegQuantizationTable quant_table;

        internal JpegComponent()
        {
        }

        internal void Assign(JpegComponent ci)
        {
            component_id = ci.component_id;
            component_index = ci.component_index;
            h_samp_factor = ci.h_samp_factor;
            v_samp_factor = ci.v_samp_factor;
            quant_tbl_no = ci.quant_tbl_no;
            dc_tbl_no = ci.dc_tbl_no;
            ac_tbl_no = ci.ac_tbl_no;
            width_in_blocks = ci.width_in_blocks;
            height_in_blocks = ci.height_in_blocks;
            DCT_scaled_size = ci.DCT_scaled_size;
            downsampled_width = ci.downsampled_width;
            downsampled_height = ci.downsampled_height;
            component_needed = ci.component_needed;
            MCU_width = ci.MCU_width;
            MCU_height = ci.MCU_height;
            MCU_blocks = ci.MCU_blocks;
            MCU_sample_width = ci.MCU_sample_width;
            last_col_width = ci.last_col_width;
            last_row_height = ci.last_row_height;
            quant_table = ci.quant_table;
        }

        /// <summary>
        /// Identifier for this component (0..255)
        /// </summary>
        /// <value>The component ID.</value>
        public int Component_id
        {
            get { return component_id; }
            set { component_id = value; }
        }

        /// <summary>
        /// Its index in SOF or <see cref="JpegDecompressor.Comp_info"/>.
        /// </summary>
        /// <value>The component index.</value>
        public int Component_index
        {
            get { return component_index; }
            set { component_index = value; }
        }

        /// <summary>
        /// Horizontal sampling factor (1..4)
        /// </summary>
        /// <value>The horizontal sampling factor.</value>
        public int H_samp_factor
        {
            get { return h_samp_factor; }
            set { h_samp_factor = value; }
        }

        /// <summary>
        /// Vertical sampling factor (1..4)
        /// </summary>
        /// <value>The vertical sampling factor.</value>
        public int V_samp_factor
        {
            get { return v_samp_factor; }
            set { v_samp_factor = value; }
        }

        /// <summary>
        /// Quantization table selector (0..3)
        /// </summary>
        /// <value>The quantization table selector.</value>
        public int Quant_tbl_no
        {
            get { return quant_tbl_no; }
            set { quant_tbl_no = value; }
        }

        /// <summary>
        /// DC entropy table selector (0..3)
        /// </summary>
        /// <value>The DC entropy table selector.</value>
        public int Dc_tbl_no
        {
            get { return dc_tbl_no; }
            set { dc_tbl_no = value; }
        }

        /// <summary>
        /// AC entropy table selector (0..3)
        /// </summary>
        /// <value>The AC entropy table selector.</value>
        public int Ac_tbl_no
        {
            get { return ac_tbl_no; }
            set { ac_tbl_no = value; }
        }

        /// <summary>
        /// Gets or sets the width in blocks.
        /// </summary>
        /// <value>The width in blocks.</value>
        public int Width_in_blocks
        {
            get { return width_in_blocks; }
            set { width_in_blocks = value; }
        }

        /// <summary>
        /// Gets the downsampled width.
        /// </summary>
        /// <value>The downsampled width.</value>
        public int Downsampled_width
        {
            get { return downsampled_width; }
        }

        internal static JpegComponent[] createArrayOfComponents(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            JpegComponent[] result = new JpegComponent[length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = new JpegComponent();

            return result;
        }
    }
    #endregion

    #region JpegConstants
    /// <summary>
    /// Defines some JPEG constants.
    /// </summary>
    public static class JpegConstants
    {
        //////////////////////////////////////////////////////////////////////////
        // All of these are specified by the JPEG standard, so don't change them
        // if you want to be compatible.
        //

        /// <summary>
        /// The basic DCT block is 8x8 samples
        /// </summary>
        public const int DCTSize = 8;

        /// <summary>
        /// DCTSize squared; the number of elements in a block. 
        /// </summary>
        public const int DCTSize2 = DCTSize * DCTSize;

        /// <summary>
        /// Quantization tables are numbered 0..3 
        /// </summary>
        public const int NumberOfQuantTables = 4;

        /// <summary>
        /// Huffman tables are numbered 0..3
        /// </summary>
        public const int NumberOfHuffmanTables = 4;

        /// <summary>
        /// JPEG limit on the number of components in one scan.
        /// </summary>
        public const int MaxComponentsInScan = 4;

        // compressor's limit on blocks per MCU
        //
        // Unfortunately, some bozo at Adobe saw no reason to be bound by the standard;
        // the PostScript DCT filter can emit files with many more than 10 blocks/MCU.
        // If you happen to run across such a file, you can up DecompressorMaxBlocksInMCU
        // to handle it.  We even let you do this from the jconfig.h file. However,
        // we strongly discourage changing CompressorMaxBlocksInMCU; just because Adobe
        // sometimes emits noncompliant files doesn't mean you should too.

        /// <summary>
        /// Compressor's limit on blocks per MCU.
        /// </summary>
        public const int CompressorMaxBlocksInMCU = 10;

        /// <summary>
        /// Decompressor's limit on blocks per MCU.
        /// </summary>
        public const int DecompressorMaxBlocksInMCU = 10;

        /// <summary>
        /// JPEG limit on sampling factors.
        /// </summary>
        public const int MaxSamplingFactor = 4;


        //////////////////////////////////////////////////////////////////////////
        // implementation-specific constants
        //

        // Maximum number of components (color channels) allowed in JPEG image.
        // To meet the letter of the JPEG spec, set this to 255.  However, darn
        // few applications need more than 4 channels (maybe 5 for CMYK + alpha
        // mask).  We recommend 10 as a reasonable compromise; use 4 if you are
        // really short on memory.  (Each allowed component costs a hundred or so
        // bytes of storage, whether actually used in an image or not.)

        /// <summary>
        /// Maximum number of color channels allowed in JPEG image.
        /// </summary>
        public const int MaxComponents = 10;



        /// <summary>
        /// The size of sample.
        /// </summary>
        /// <remarks>Is either:
        /// 8 - for 8-bit sample values (the usual setting)<br/>
        /// 12 - for 12-bit sample values (not supported by this version)<br/>
        /// Only 8 and 12 are legal data precisions for lossy JPEG according to the JPEG standard.
        /// Although original IJG code claims it supports 12 bit images, our code does not support 
        /// anything except 8-bit images.</remarks>
        public const int BitsInSample = 8;

        /// <summary>
        /// DCT method used by default.
        /// </summary>
        public static DCTMethod DefaultDCTMethod = DCTMethod.IntSlow;

        /// <summary>
        /// Fastest DCT method.
        /// </summary>
        public static DCTMethod FastestDCTMethod = DCTMethod.IntFast;

        /// <summary>
        /// A tad under 64K to prevent overflows. 
        /// </summary>
        public const int JpegMaxDimention = 65500;

        /// <summary>
        /// The maximum sample value.
        /// </summary>
        public const int MaxSampleValue = 255;

        /// <summary>
        /// The medium sample value.
        /// </summary>
        public const int MediumSampleValue = 128;

        // Ordering of RGB data in scanlines passed to or from the application.
        // RESTRICTIONS:
        // 1. These macros only affect RGB<=>YCbCr color conversion, so they are not
        // useful if you are using JPEG color spaces other than YCbCr or grayscale.
        // 2. The color quantizer modules will not behave desirably if RGB_PixelLength
        // is not 3 (they don't understand about dummy color components!).  So you
        // can't use color quantization if you change that value.

        /// <summary>
        /// Offset of Red in an RGB scanline element. 
        /// </summary>
        public const int Offset_RGB_Red = 0;

        /// <summary>
        /// Offset of Green in an RGB scanline element. 
        /// </summary>
        public const int Offset_RGB_Green = 1;

        /// <summary>
        /// Offset of Blue in an RGB scanline element. 
        /// </summary>
        public const int Offset_RGB_Blue = 2;

        /// <summary>
        /// Bytes per RGB scanline element.
        /// </summary>
        public const int RGB_PixelLength = 3;

        /// <summary>
        /// The number of bits of lookahead.
        /// </summary>
        public const int HuffmanLookaheadDistance = 8;
    }
    #endregion

    #region JpegDownsampler
    /// <summary>
    /// Downsampling
    /// </summary>
    class JpegDownsampler
    {
        private enum downSampleMethod
        {
            fullsize_smooth_downsampler,
            fullsize_downsampler,
            h2v1_downsampler,
            h2v2_smooth_downsampler,
            h2v2_downsampler,
            int_downsampler
        };

        /* Downsamplers, one per component */
        private downSampleMethod[] m_downSamplers = new downSampleMethod[JpegConstants.MaxComponents];

        private JpegCompressor m_cinfo;
        private bool m_need_context_rows; /* true if need rows above & below */

        public JpegDownsampler(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;
            m_need_context_rows = false;

            if (cinfo.m_CCIR601_sampling)
                throw new Exception("CCIR601 sampling not implemented yet");

            /* Verify we can handle the sampling factors, and set up method pointers */
            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = cinfo.Component_info[ci];

                if (componentInfo.H_samp_factor == cinfo.m_max_h_samp_factor &&
                    componentInfo.V_samp_factor == cinfo.m_max_v_samp_factor)
                {
                    if (cinfo.m_smoothing_factor != 0)
                    {
                        m_downSamplers[ci] = downSampleMethod.fullsize_smooth_downsampler;
                        m_need_context_rows = true;
                    }
                    else
                    {
                        m_downSamplers[ci] = downSampleMethod.fullsize_downsampler;
                    }
                }
                else if (componentInfo.H_samp_factor * 2 == cinfo.m_max_h_samp_factor &&
                         componentInfo.V_samp_factor == cinfo.m_max_v_samp_factor)
                {
                    m_downSamplers[ci] = downSampleMethod.h2v1_downsampler;
                }
                else if (componentInfo.H_samp_factor * 2 == cinfo.m_max_h_samp_factor &&
                         componentInfo.V_samp_factor * 2 == cinfo.m_max_v_samp_factor)
                {
                    if (cinfo.m_smoothing_factor != 0)
                    {
                        m_downSamplers[ci] = downSampleMethod.h2v2_smooth_downsampler;
                        m_need_context_rows = true;
                    }
                    else
                    {
                        m_downSamplers[ci] = downSampleMethod.h2v2_downsampler;
                    }
                }
                else if ((cinfo.m_max_h_samp_factor % componentInfo.H_samp_factor) == 0 &&
                         (cinfo.m_max_v_samp_factor % componentInfo.V_samp_factor) == 0)
                {
                    m_downSamplers[ci] = downSampleMethod.int_downsampler;
                }
                else
                    throw new Exception("Fractional sampling not implemented yet");
            }
        }

        /// <summary>
        /// Do downsampling for a whole row group (all components).
        /// 
        /// In this version we simply downsample each component independently.
        /// </summary>
        public void downsample(byte[][][] input_buf, int in_row_index, byte[][][] output_buf, int out_row_group_index)
        {
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                int outIndex = out_row_group_index * m_cinfo.Component_info[ci].V_samp_factor;
                switch (m_downSamplers[ci])
                {
                    case downSampleMethod.fullsize_smooth_downsampler:
                        fullsize_smooth_downsample(ci, input_buf[ci], in_row_index, output_buf[ci], outIndex);
                        break;

                    case downSampleMethod.fullsize_downsampler:
                        fullsize_downsample(ci, input_buf[ci], in_row_index, output_buf[ci], outIndex);
                        break;

                    case downSampleMethod.h2v1_downsampler:
                        h2v1_downsample(ci, input_buf[ci], in_row_index, output_buf[ci], outIndex);
                        break;

                    case downSampleMethod.h2v2_smooth_downsampler:
                        h2v2_smooth_downsample(ci, input_buf[ci], in_row_index, output_buf[ci], outIndex);
                        break;

                    case downSampleMethod.h2v2_downsampler:
                        h2v2_downsample(ci, input_buf[ci], in_row_index, output_buf[ci], outIndex);
                        break;

                    case downSampleMethod.int_downsampler:
                        int_downsample(ci, input_buf[ci], in_row_index, output_buf[ci], outIndex);
                        break;
                };
            }
        }

        public bool NeedContextRows()
        {
            return m_need_context_rows;
        }

        /// <summary>
        /// Downsample pixel values of a single component.
        /// One row group is processed per call.
        /// This version handles arbitrary integral sampling ratios, without smoothing.
        /// Note that this version is not actually used for customary sampling ratios.
        /// </summary>
        private void int_downsample(int componentIndex, byte[][] input_data, int startInputRow, byte[][] output_data, int startOutRow)
        {
            /* Expand input data enough to let all the output samples be generated
             * by the standard loop.  Special-casing padded output would be more
             * efficient.
             */
            int output_cols = m_cinfo.Component_info[componentIndex].Width_in_blocks * JpegConstants.DCTSize;
            int h_expand = m_cinfo.m_max_h_samp_factor / m_cinfo.Component_info[componentIndex].H_samp_factor;
            expand_right_edge(input_data, startInputRow, m_cinfo.m_max_v_samp_factor, m_cinfo.m_image_width, output_cols * h_expand);

            int v_expand = m_cinfo.m_max_v_samp_factor / m_cinfo.Component_info[componentIndex].V_samp_factor;
            int numpix = h_expand * v_expand;
            int numpix2 = numpix / 2;
            int inrow = 0;
            for (int outrow = 0; outrow < m_cinfo.Component_info[componentIndex].V_samp_factor; outrow++)
            {
                for (int outcol = 0, outcol_h = 0; outcol < output_cols; outcol++, outcol_h += h_expand)
                {
                    int outvalue = 0;
                    for (int v = 0; v < v_expand; v++)
                    {
                        for (int h = 0; h < h_expand; h++)
                            outvalue += input_data[startInputRow + inrow + v][outcol_h + h];
                    }

                    output_data[startOutRow + outrow][outcol] = (byte)((outvalue + numpix2) / numpix);
                }

                inrow += v_expand;
            }
        }

        /// <summary>
        /// Downsample pixel values of a single component.
        /// This version handles the special case of a full-size component,
        /// without smoothing.
        /// </summary>
        private void fullsize_downsample(int componentIndex, byte[][] input_data, int startInputRow, byte[][] output_data, int startOutRow)
        {
            /* Copy the data */
            JpegUtils.jcopy_sample_rows(input_data, startInputRow, output_data, startOutRow, m_cinfo.m_max_v_samp_factor, m_cinfo.m_image_width);

            /* Edge-expand */
            expand_right_edge(output_data, startOutRow, m_cinfo.m_max_v_samp_factor, m_cinfo.m_image_width, m_cinfo.Component_info[componentIndex].Width_in_blocks * JpegConstants.DCTSize);
        }

        /// <summary>
        /// Downsample pixel values of a single component.
        /// This version handles the common case of 2:1 horizontal and 1:1 vertical,
        /// without smoothing.
        /// 
        /// A note about the "bias" calculations: when rounding fractional values to
        /// integer, we do not want to always round 0.5 up to the next integer.
        /// If we did that, we'd introduce a noticeable bias towards larger values.
        /// Instead, this code is arranged so that 0.5 will be rounded up or down at
        /// alternate pixel locations (a simple ordered dither pattern).
        /// </summary>
        private void h2v1_downsample(int componentIndex, byte[][] input_data, int startInputRow, byte[][] output_data, int startOutRow)
        {
            /* Expand input data enough to let all the output samples be generated
             * by the standard loop.  Special-casing padded output would be more
             * efficient.
             */
            int output_cols = m_cinfo.Component_info[componentIndex].Width_in_blocks * JpegConstants.DCTSize;
            expand_right_edge(input_data, startInputRow, m_cinfo.m_max_v_samp_factor, m_cinfo.m_image_width, output_cols * 2);

            for (int outrow = 0; outrow < m_cinfo.Component_info[componentIndex].V_samp_factor; outrow++)
            {
                /* bias = 0,1,0,1,... for successive samples */
                int bias = 0;
                int inputColumn = 0;
                for (int outcol = 0; outcol < output_cols; outcol++)
                {
                    output_data[startOutRow + outrow][outcol] = (byte)(
                        ((int)input_data[startInputRow + outrow][inputColumn] +
                        (int)input_data[startInputRow + outrow][inputColumn + 1] + bias) >> 1);

                    bias ^= 1;      /* 0=>1, 1=>0 */
                    inputColumn += 2;
                }
            }
        }

        /// <summary>
        /// Downsample pixel values of a single component.
        /// This version handles the standard case of 2:1 horizontal and 2:1 vertical,
        /// without smoothing.
        /// </summary>
        private void h2v2_downsample(int componentIndex, byte[][] input_data, int startInputRow, byte[][] output_data, int startOutRow)
        {
            /* Expand input data enough to let all the output samples be generated
             * by the standard loop.  Special-casing padded output would be more
             * efficient.
             */
            int output_cols = m_cinfo.Component_info[componentIndex].Width_in_blocks * JpegConstants.DCTSize;
            expand_right_edge(input_data, startInputRow, m_cinfo.m_max_v_samp_factor, m_cinfo.m_image_width, output_cols * 2);

            int inrow = 0;
            for (int outrow = 0; outrow < m_cinfo.Component_info[componentIndex].V_samp_factor; outrow++)
            {
                /* bias = 1,2,1,2,... for successive samples */
                int bias = 1;
                int inputColumn = 0;
                for (int outcol = 0; outcol < output_cols; outcol++)
                {
                    output_data[startOutRow + outrow][outcol] = (byte)((
                        (int)input_data[startInputRow + inrow][inputColumn] +
                        (int)input_data[startInputRow + inrow][inputColumn + 1] +
                        (int)input_data[startInputRow + inrow + 1][inputColumn] +
                        (int)input_data[startInputRow + inrow + 1][inputColumn + 1] + bias) >> 2);

                    bias ^= 3;      /* 1=>2, 2=>1 */
                    inputColumn += 2;
                }

                inrow += 2;
            }
        }

        /// <summary>
        /// Downsample pixel values of a single component.
        /// This version handles the standard case of 2:1 horizontal and 2:1 vertical,
        /// with smoothing.  One row of context is required.
        /// </summary>
        private void h2v2_smooth_downsample(int componentIndex, byte[][] input_data, int startInputRow, byte[][] output_data, int startOutRow)
        {
            /* Expand input data enough to let all the output samples be generated
             * by the standard loop.  Special-casing padded output would be more
             * efficient.
             */
            int output_cols = m_cinfo.Component_info[componentIndex].Width_in_blocks * JpegConstants.DCTSize;
            expand_right_edge(input_data, startInputRow - 1, m_cinfo.m_max_v_samp_factor + 2, m_cinfo.m_image_width, output_cols * 2);

            /* We don't bother to form the individual "smoothed" input pixel values;
             * we can directly compute the output which is the average of the four
             * smoothed values.  Each of the four member pixels contributes a fraction
             * (1-8*SF) to its own smoothed image and a fraction SF to each of the three
             * other smoothed pixels, therefore a total fraction (1-5*SF)/4 to the final
             * output.  The four corner-adjacent neighbor pixels contribute a fraction
             * SF to just one smoothed pixel, or SF/4 to the final output; while the
             * eight edge-adjacent neighbors contribute SF to each of two smoothed
             * pixels, or SF/2 overall.  In order to use integer arithmetic, these
             * factors are scaled by 2^16 = 65536.
             * Also recall that SF = smoothing_factor / 1024.
             */

            int memberscale = 16384 - m_cinfo.m_smoothing_factor * 80; /* scaled (1-5*SF)/4 */
            int neighscale = m_cinfo.m_smoothing_factor * 16; /* scaled SF/4 */

            for (int inrow = 0, outrow = 0; outrow < m_cinfo.Component_info[componentIndex].V_samp_factor; outrow++)
            {
                int outIndex = 0;
                int inIndex0 = 0;
                int inIndex1 = 0;
                int aboveIndex = 0;
                int belowIndex = 0;

                /* Special case for first column: pretend column -1 is same as column 0 */
                int membersum = input_data[startInputRow + inrow][inIndex0] +
                    input_data[startInputRow + inrow][inIndex0 + 1] +
                    input_data[startInputRow + inrow + 1][inIndex1] +
                    input_data[startInputRow + inrow + 1][inIndex1 + 1];

                int neighsum = input_data[startInputRow + inrow - 1][aboveIndex] +
                    input_data[startInputRow + inrow - 1][aboveIndex + 1] +
                    input_data[startInputRow + inrow + 2][belowIndex] +
                    input_data[startInputRow + inrow + 2][belowIndex + 1] +
                    input_data[startInputRow + inrow][inIndex0] +
                    input_data[startInputRow + inrow][inIndex0 + 2] +
                    input_data[startInputRow + inrow + 1][inIndex1] +
                    input_data[startInputRow + inrow + 1][inIndex1 + 2];

                neighsum += neighsum;
                neighsum += input_data[startInputRow + inrow - 1][aboveIndex] +
                    input_data[startInputRow + inrow - 1][aboveIndex + 2] +
                    input_data[startInputRow + inrow + 2][belowIndex] +
                    input_data[startInputRow + inrow + 2][belowIndex + 2];

                membersum = membersum * memberscale + neighsum * neighscale;
                output_data[startOutRow + outrow][outIndex] = (byte)((membersum + 32768) >> 16);
                outIndex++;

                inIndex0 += 2;
                inIndex1 += 2;
                aboveIndex += 2;
                belowIndex += 2;

                for (int colctr = output_cols - 2; colctr > 0; colctr--)
                {
                    /* sum of pixels directly mapped to this output element */
                    membersum = input_data[startInputRow + inrow][inIndex0] +
                        input_data[startInputRow + inrow][inIndex0 + 1] +
                        input_data[startInputRow + inrow + 1][inIndex1] +
                        input_data[startInputRow + inrow + 1][inIndex1 + 1];

                    /* sum of edge-neighbor pixels */
                    neighsum = input_data[startInputRow + inrow - 1][aboveIndex] +
                        input_data[startInputRow + inrow - 1][aboveIndex + 1] +
                        input_data[startInputRow + inrow + 2][belowIndex] +
                        input_data[startInputRow + inrow + 2][belowIndex + 1] +
                        input_data[startInputRow + inrow][inIndex0 - 1] +
                        input_data[startInputRow + inrow][inIndex0 + 2] +
                        input_data[startInputRow + inrow + 1][inIndex1 - 1] +
                        input_data[startInputRow + inrow + 1][inIndex1 + 2];

                    /* The edge-neighbors count twice as much as corner-neighbors */
                    neighsum += neighsum;

                    /* Add in the corner-neighbors */
                    neighsum += input_data[startInputRow + inrow - 1][aboveIndex - 1] +
                        input_data[startInputRow + inrow - 1][aboveIndex + 2] +
                        input_data[startInputRow + inrow + 2][belowIndex - 1] +
                        input_data[startInputRow + inrow + 2][belowIndex + 2];

                    /* form final output scaled up by 2^16 */
                    membersum = membersum * memberscale + neighsum * neighscale;

                    /* round, descale and output it */
                    output_data[startOutRow + outrow][outIndex] = (byte)((membersum + 32768) >> 16);
                    outIndex++;

                    inIndex0 += 2;
                    inIndex1 += 2;
                    aboveIndex += 2;
                    belowIndex += 2;
                }

                /* Special case for last column */
                membersum = input_data[startInputRow + inrow][inIndex0] +
                    input_data[startInputRow + inrow][inIndex0 + 1] +
                    input_data[startInputRow + inrow + 1][inIndex1] +
                    input_data[startInputRow + inrow + 1][inIndex1 + 1];

                neighsum = input_data[startInputRow + inrow - 1][aboveIndex] +
                    input_data[startInputRow + inrow - 1][aboveIndex + 1] +
                    input_data[startInputRow + inrow + 2][belowIndex] +
                    input_data[startInputRow + inrow + 2][belowIndex + 1] +
                    input_data[startInputRow + inrow][inIndex0 - 1] +
                    input_data[startInputRow + inrow][inIndex0 + 1] +
                    input_data[startInputRow + inrow + 1][inIndex1 - 1] +
                    input_data[startInputRow + inrow + 1][inIndex1 + 1];

                neighsum += neighsum;
                neighsum += input_data[startInputRow + inrow - 1][aboveIndex - 1] +
                    input_data[startInputRow + inrow - 1][aboveIndex + 1] +
                    input_data[startInputRow + inrow + 2][belowIndex - 1] +
                    input_data[startInputRow + inrow + 2][belowIndex + 1];

                membersum = membersum * memberscale + neighsum * neighscale;
                output_data[startOutRow + outrow][outIndex] = (byte)((membersum + 32768) >> 16);

                inrow += 2;
            }
        }

        /// <summary>
        /// Downsample pixel values of a single component.
        /// This version handles the special case of a full-size component,
        /// with smoothing.  One row of context is required.
        /// </summary>
        private void fullsize_smooth_downsample(int componentIndex, byte[][] input_data, int startInputRow, byte[][] output_data, int startOutRow)
        {
            /* Expand input data enough to let all the output samples be generated
             * by the standard loop.  Special-casing padded output would be more
             * efficient.
             */
            int output_cols = m_cinfo.Component_info[componentIndex].Width_in_blocks * JpegConstants.DCTSize;
            expand_right_edge(input_data, startInputRow - 1, m_cinfo.m_max_v_samp_factor + 2, m_cinfo.m_image_width, output_cols);

            /* Each of the eight neighbor pixels contributes a fraction SF to the
             * smoothed pixel, while the main pixel contributes (1-8*SF).  In order
             * to use integer arithmetic, these factors are multiplied by 2^16 = 65536.
             * Also recall that SF = smoothing_factor / 1024.
             */

            int memberscale = 65536 - m_cinfo.m_smoothing_factor * 512; /* scaled 1-8*SF */
            int neighscale = m_cinfo.m_smoothing_factor * 64; /* scaled SF */

            for (int outrow = 0; outrow < m_cinfo.Component_info[componentIndex].V_samp_factor; outrow++)
            {
                int outIndex = 0;
                int inIndex = 0;
                int aboveIndex = 0;
                int belowIndex = 0;

                /* Special case for first column */
                int colsum = input_data[startInputRow + outrow - 1][aboveIndex] +
                    input_data[startInputRow + outrow + 1][belowIndex] +
                    input_data[startInputRow + outrow][inIndex];

                aboveIndex++;
                belowIndex++;

                int membersum = input_data[startInputRow + outrow][inIndex];
                inIndex++;

                int nextcolsum = input_data[startInputRow + outrow - 1][aboveIndex] +
                    input_data[startInputRow + outrow + 1][belowIndex] +
                    input_data[startInputRow + outrow][inIndex];

                int neighsum = colsum + (colsum - membersum) + nextcolsum;

                membersum = membersum * memberscale + neighsum * neighscale;
                output_data[startOutRow + outrow][outIndex] = (byte)((membersum + 32768) >> 16);
                outIndex++;

                int lastcolsum = colsum;
                colsum = nextcolsum;

                for (int colctr = output_cols - 2; colctr > 0; colctr--)
                {
                    membersum = input_data[startInputRow + outrow][inIndex];

                    inIndex++;
                    aboveIndex++;
                    belowIndex++;

                    nextcolsum = input_data[startInputRow + outrow - 1][aboveIndex] +
                        input_data[startInputRow + outrow + 1][belowIndex] +
                        input_data[startInputRow + outrow][inIndex];

                    neighsum = lastcolsum + (colsum - membersum) + nextcolsum;
                    membersum = membersum * memberscale + neighsum * neighscale;

                    output_data[startOutRow + outrow][outIndex] = (byte)((membersum + 32768) >> 16);
                    outIndex++;

                    lastcolsum = colsum;
                    colsum = nextcolsum;
                }

                /* Special case for last column */
                membersum = input_data[startInputRow + outrow][inIndex];
                neighsum = lastcolsum + (colsum - membersum) + colsum;
                membersum = membersum * memberscale + neighsum * neighscale;
                output_data[startOutRow + outrow][outIndex] = (byte)((membersum + 32768) >> 16);
            }
        }

        /// <summary>
        /// Expand a component horizontally from width input_cols to width output_cols,
        /// by duplicating the rightmost samples.
        /// </summary>
        private static void expand_right_edge(byte[][] image_data, int startInputRow, int num_rows, int input_cols, int output_cols)
        {
            int numcols = output_cols - input_cols;
            if (numcols > 0)
            {
                for (int row = startInputRow; row < (startInputRow + num_rows); row++)
                {
                    /* don't need GETJSAMPLE() here */
                    byte pixval = image_data[row][input_cols - 1];
                    for (int count = 0; count < numcols; count++)
                        image_data[row][input_cols + count] = pixval;
                }
            }
        }
    }
    #endregion

    #region JpegEntropyDecoder
    /// <summary>
    /// Entropy decoding
    /// </summary>
    abstract class JpegEntropyDecoder
    {
        // Figure F.12: extend sign bit.
        // entry n is 2**(n-1)
        private static int[] extend_test = 
        { 
            0, 0x0001, 0x0002, 0x0004, 0x0008, 0x0010, 0x0020, 
            0x0040, 0x0080, 0x0100, 0x0200, 0x0400, 0x0800, 
            0x1000, 0x2000, 0x4000 
        };

        // entry n is (-1 << n) + 1
        private static int[] extend_offset = 
        { 
            0, (-1 << 1) + 1, (-1 << 2) + 1, 
            (-1 << 3) + 1, (-1 << 4) + 1, (-1 << 5) + 1,
            (-1 << 6) + 1, (-1 << 7) + 1, (-1 << 8) + 1,
            (-1 << 9) + 1, (-1 << 10) + 1,
            (-1 << 11) + 1, (-1 << 12) + 1,
            (-1 << 13) + 1, (-1 << 14) + 1,
            (-1 << 15) + 1 
        };

        /* Fetching the next N bits from the input stream is a time-critical operation
        * for the Huffman decoders.  We implement it with a combination of inline
        * macros and out-of-line subroutines.  Note that N (the number of bits
        * demanded at one time) never exceeds 15 for JPEG use.
        *
        * We read source bytes into get_buffer and dole out bits as needed.
        * If get_buffer already contains enough bits, they are fetched in-line
        * by the macros CHECK_BIT_BUFFER and GET_BITS.  When there aren't enough
        * bits, jpeg_fill_bit_buffer is called; it will attempt to fill get_buffer
        * as full as possible (not just to the number of bits needed; this
        * prefetching reduces the overhead cost of calling jpeg_fill_bit_buffer).
        * Note that jpeg_fill_bit_buffer may return false to indicate suspension.
        * On true return, jpeg_fill_bit_buffer guarantees that get_buffer contains
        * at least the requested number of bits --- dummy zeroes are inserted if
        * necessary.
        */
        protected const int BIT_BUF_SIZE = 32;    /* size of buffer in bits */

        /*
        * Out-of-line code for bit fetching (shared with jdphuff.c).
        * See jdhuff.h for info about usage.
        * Note: current values of get_buffer and bits_left are passed as parameters,
        * but are returned in the corresponding fields of the state struct.
        *
        * On most machines MIN_GET_BITS should be 25 to allow the full 32-bit width
        * of get_buffer to be used.  (On machines with wider words, an even larger
        * buffer could be used.)  However, on some machines 32-bit shifts are
        * quite slow and take time proportional to the number of places shifted.
        * (This is true with most PC compilers, for instance.)  In this case it may
        * be a win to set MIN_GET_BITS to the minimum value of 15.  This reduces the
        * average shift distance at the cost of more calls to jpeg_fill_bit_buffer.
        */

        protected const int MIN_GET_BITS = BIT_BUF_SIZE - 7;

        protected JpegDecompressor m_cinfo;

        /* This is here to share code between baseline and progressive decoders; */
        /* other modules probably should not use it */
        protected bool m_insufficient_data; /* set true after emitting warning */

        public abstract void start_pass();
        public abstract bool decode_mcu(JpegBlock[] MCU_data);

        protected static int HUFF_EXTEND(int x, int s)
        {
            return ((x) < extend_test[s] ? (x) + extend_offset[s] : (x));
        }

        protected void BITREAD_LOAD_STATE(SavedBitreadState bitstate, out int get_buffer, out int bits_left, ref WorkingBitreadState br_state)
        {
            br_state.cinfo = m_cinfo;
            get_buffer = bitstate.get_buffer;
            bits_left = bitstate.bits_left;
        }

        protected static void BITREAD_SAVE_STATE(ref SavedBitreadState bitstate, int get_buffer, int bits_left)
        {
            bitstate.get_buffer = get_buffer;
            bitstate.bits_left = bits_left;
        }

        /// <summary>
        /// Expand a Huffman table definition into the derived format
        /// This routine also performs some validation checks on the table.
        /// </summary>
        protected void jpeg_make_d_derived_tbl(bool isDC, int tblno, ref DerivedTable dtbl)
        {
            /* Note that huffsize[] and huffcode[] are filled in code-length order,
            * paralleling the order of the symbols themselves in htbl.huffval[].
            */

            /* Find the input Huffman table */
            if (tblno < 0 || tblno >= JpegConstants.NumberOfHuffmanTables)
                throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", tblno));

            JpegHuffmanTable htbl = isDC ? m_cinfo.m_dc_huff_tbl_ptrs[tblno] : m_cinfo.m_ac_huff_tbl_ptrs[tblno];
            if (htbl == null)
                throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", tblno));

            /* Allocate a workspace if we haven't already done so. */
            if (dtbl == null)
                dtbl = new DerivedTable();

            dtbl.pub = htbl;       /* fill in back link */

            /* Figure C.1: make table of Huffman code length for each symbol */

            int p = 0;
            char[] huffsize = new char[257];
            for (int l = 1; l <= 16; l++)
            {
                int i = htbl.Bits[l];
                if (i < 0 || p + i > 256)    /* protect against table overrun */
                    throw new Exception("Bogus Huffman table definition");

                while ((i--) != 0)
                    huffsize[p++] = (char)l;
            }
            huffsize[p] = (char)0;
            int numsymbols = p;

            /* Figure C.2: generate the codes themselves */
            /* We also validate that the counts represent a legal Huffman code tree. */

            int code = 0;
            int si = huffsize[0];
            int[] huffcode = new int[257];
            p = 0;
            while (huffsize[p] != 0)
            {
                while (((int)huffsize[p]) == si)
                {
                    huffcode[p++] = code;
                    code++;
                }

                /* code is now 1 more than the last code used for code-length si; but
                * it must still fit in si bits, since no code is allowed to be all ones.
                */
                if (code >= (1 << si))
                    throw new Exception("Bogus Huffman table definition");
                code <<= 1;
                si++;
            }

            /* Figure F.15: generate decoding tables for bit-sequential decoding */

            p = 0;
            for (int l = 1; l <= 16; l++)
            {
                if (htbl.Bits[l] != 0)
                {
                    /* valoffset[l] = huffval[] index of 1st symbol of code length l,
                    * minus the minimum code of length l
                    */
                    dtbl.valoffset[l] = p - huffcode[p];
                    p += htbl.Bits[l];
                    dtbl.maxcode[l] = huffcode[p - 1]; /* maximum code of length l */
                }
                else
                {
                    /* -1 if no codes of this length */
                    dtbl.maxcode[l] = -1;
                }
            }
            dtbl.maxcode[17] = 0xFFFFF; /* ensures jpeg_huff_decode terminates */

            /* Compute lookahead tables to speed up decoding.
            * First we set all the table entries to 0, indicating "too long";
            * then we iterate through the Huffman codes that are short enough and
            * fill in all the entries that correspond to bit sequences starting
            * with that code.
            */

            Array.Clear(dtbl.look_nbits, 0, dtbl.look_nbits.Length);
            p = 0;
            for (int l = 1; l <= JpegConstants.HuffmanLookaheadDistance; l++)
            {
                for (int i = 1; i <= htbl.Bits[l]; i++, p++)
                {
                    /* l = current code's length, p = its index in huffcode[] & huffval[]. */
                    /* Generate left-justified code followed by all possible bit sequences */
                    int lookbits = huffcode[p] << (JpegConstants.HuffmanLookaheadDistance - l);
                    for (int ctr = 1 << (JpegConstants.HuffmanLookaheadDistance - l); ctr > 0; ctr--)
                    {
                        dtbl.look_nbits[lookbits] = l;
                        dtbl.look_sym[lookbits] = htbl.Huffval[p];
                        lookbits++;
                    }
                }
            }

            /* Validate symbols as being reasonable.
            * For AC tables, we make no check, but accept all byte values 0..255.
            * For DC tables, we require the symbols to be in range 0..15.
            * (Tighter bounds could be applied depending on the data depth and mode,
            * but this is sufficient to ensure safe decoding.)
            */
            if (isDC)
            {
                for (int i = 0; i < numsymbols; i++)
                {
                    int sym = htbl.Huffval[i];
                    if (sym < 0 || sym > 15)
                        throw new Exception("Bogus Huffman table definition");
                }
            }
        }

        /*
        * These methods provide the in-line portion of bit fetching.
        * Use CHECK_BIT_BUFFER to ensure there are N bits in get_buffer
        * before using GET_BITS, PEEK_BITS, or DROP_BITS.
        * The variables get_buffer and bits_left are assumed to be locals,
        * but the state struct might not be (jpeg_huff_decode needs this).
        *  CHECK_BIT_BUFFER(state,n,action);
        *      Ensure there are N bits in get_buffer; if suspend, take action.
        *      val = GET_BITS(n);
        *      Fetch next N bits.
        *      val = PEEK_BITS(n);
        *      Fetch next N bits without removing them from the buffer.
        *  DROP_BITS(n);
        *      Discard next N bits.
        * The value N should be a simple variable, not an expression, because it
        * is evaluated multiple times.
        */

        protected static bool CHECK_BIT_BUFFER(ref WorkingBitreadState state, int nbits, ref int get_buffer, ref int bits_left)
        {
            if (bits_left < nbits)
            {
                if (!jpeg_fill_bit_buffer(ref state, get_buffer, bits_left, nbits))
                    return false;

                get_buffer = state.get_buffer;
                bits_left = state.bits_left;
            }

            return true;
        }

        protected static int GET_BITS(int nbits, int get_buffer, ref int bits_left)
        {
            return (((int)(get_buffer >> (bits_left -= nbits))) & ((1 << nbits) - 1));
        }

        protected static int PEEK_BITS(int nbits, int get_buffer, int bits_left)
        {
            return (((int)(get_buffer >> (bits_left - nbits))) & ((1 << nbits) - 1));
        }

        protected static void DROP_BITS(int nbits, ref int bits_left)
        {
            bits_left -= nbits;
        }

        /* Load up the bit buffer to a depth of at least nbits */
        protected static bool jpeg_fill_bit_buffer(ref WorkingBitreadState state, int get_buffer, int bits_left, int nbits)
        {
            /* Attempt to load at least MIN_GET_BITS bits into get_buffer. */
            /* (It is assumed that no request will be for more than that many bits.) */
            /* We fail to do so only if we hit a marker or are forced to suspend. */

            bool noMoreBytes = false;

            if (state.cinfo.m_unread_marker == 0)
            {
                /* cannot advance past a marker */
                while (bits_left < MIN_GET_BITS)
                {
                    int c;
                    state.cinfo.m_src.GetByte(out c);

                    /* If it's 0xFF, check and discard stuffed zero byte */
                    if (c == 0xFF)
                    {
                        /* Loop here to discard any padding FF's on terminating marker,
                        * so that we can save a valid unread_marker value.  NOTE: we will
                        * accept multiple FF's followed by a 0 as meaning a single FF data
                        * byte.  This data pattern is not valid according to the standard.
                        */
                        do
                        {
                            state.cinfo.m_src.GetByte(out c);
                        }
                        while (c == 0xFF);

                        if (c == 0)
                        {
                            /* Found FF/00, which represents an FF data byte */
                            c = 0xFF;
                        }
                        else
                        {
                            /* Oops, it's actually a marker indicating end of compressed data.
                            * Save the marker code for later use.
                            * Fine point: it might appear that we should save the marker into
                            * bitread working state, not straight into permanent state.  But
                            * once we have hit a marker, we cannot need to suspend within the
                            * current MCU, because we will read no more bytes from the data
                            * source.  So it is OK to update permanent state right away.
                            */
                            state.cinfo.m_unread_marker = c;
                            /* See if we need to insert some fake zero bits. */
                            noMoreBytes = true;
                            break;
                        }
                    }

                    /* OK, load c into get_buffer */
                    get_buffer = (get_buffer << 8) | c;
                    bits_left += 8;
                } /* end while */
            }
            else
                noMoreBytes = true;

            if (noMoreBytes)
            {
                /* We get here if we've read the marker that terminates the compressed
                * data segment.  There should be enough bits in the buffer register
                * to satisfy the request; if so, no problem.
                */
                if (nbits > bits_left)
                {
                    /* Uh-oh.  Report corrupted data to user and stuff zeroes into
                    * the data stream, so that we can produce some kind of image.
                    * We use a nonvolatile flag to ensure that only one warning message
                    * appears per data segment.
                    */
                    if (!state.cinfo.m_entropy.m_insufficient_data)
                    {
                        state.cinfo.m_entropy.m_insufficient_data = true;
                    }

                    /* Fill the buffer with zero bits */
                    get_buffer <<= MIN_GET_BITS - bits_left;
                    bits_left = MIN_GET_BITS;
                }
            }

            /* Unload the local registers */
            state.get_buffer = get_buffer;
            state.bits_left = bits_left;

            return true;
        }

        /*
        * Code for extracting next Huffman-coded symbol from input bit stream.
        * Again, this is time-critical and we make the main paths be macros.
        *
        * We use a lookahead table to process codes of up to HuffmanLookaheadDistance bits
        * without looping.  Usually, more than 95% of the Huffman codes will be 8
        * or fewer bits long.  The few overlength codes are handled with a loop,
        * which need not be inline code.
        *
        * Notes about the HUFF_DECODE macro:
        * 1. Near the end of the data segment, we may fail to get enough bits
        *    for a lookahead.  In that case, we do it the hard way.
        * 2. If the lookahead table contains no entry, the next code must be
        *    more than HuffmanLookaheadDistance bits long.
        * 3. jpeg_huff_decode returns -1 if forced to suspend.
        */
        protected static bool HUFF_DECODE(out int result, ref WorkingBitreadState state, DerivedTable htbl, ref int get_buffer, ref int bits_left)
        {
            int nb = 0;
            bool doSlow = false;

            if (bits_left < JpegConstants.HuffmanLookaheadDistance)
            {
                if (!jpeg_fill_bit_buffer(ref state, get_buffer, bits_left, 0))
                {
                    result = -1;
                    return false;
                }

                get_buffer = state.get_buffer;
                bits_left = state.bits_left;
                if (bits_left < JpegConstants.HuffmanLookaheadDistance)
                {
                    nb = 1;
                    doSlow = true;
                }
            }

            if (!doSlow)
            {
                int look = PEEK_BITS(JpegConstants.HuffmanLookaheadDistance, get_buffer, bits_left);
                if ((nb = htbl.look_nbits[look]) != 0)
                {
                    DROP_BITS(nb, ref bits_left);
                    result = htbl.look_sym[look];
                    return true;
                }

                nb = JpegConstants.HuffmanLookaheadDistance + 1;
            }

            result = jpeg_huff_decode(ref state, get_buffer, bits_left, htbl, nb);
            if (result < 0)
                return false;

            get_buffer = state.get_buffer;
            bits_left = state.bits_left;

            return true;
        }

        /* Out-of-line case for Huffman code fetching */
        protected static int jpeg_huff_decode(ref WorkingBitreadState state, int get_buffer, int bits_left, DerivedTable htbl, int min_bits)
        {
            /* HUFF_DECODE has determined that the code is at least min_bits */
            /* bits long, so fetch that many bits in one swoop. */
            int l = min_bits;
            if (!CHECK_BIT_BUFFER(ref state, l, ref get_buffer, ref bits_left))
                return -1;

            int code = GET_BITS(l, get_buffer, ref bits_left);

            /* Collect the rest of the Huffman code one bit at a time. */
            /* This is per Figure F.16 in the JPEG spec. */

            while (code > htbl.maxcode[l])
            {
                code <<= 1;
                if (!CHECK_BIT_BUFFER(ref state, 1, ref get_buffer, ref bits_left))
                    return -1;

                code |= GET_BITS(1, get_buffer, ref bits_left);
                l++;
            }

            /* Unload the local registers */
            state.get_buffer = get_buffer;
            state.bits_left = bits_left;

            /* With garbage input we may reach the sentinel value l = 17. */

            if (l > 16)
            {
                /* fake a zero as the safest result */
                return 0;
            }

            return htbl.pub.Huffval[code + htbl.valoffset[l]];
        }
    }
    #endregion

    #region JpegEntropyEncoder
    /// <summary>
    /// Entropy encoding
    /// </summary>
    abstract class JpegEntropyEncoder
    {
        /* Derived data constructed for each Huffman table */
        protected class c_derived_tbl
        {
            public int[] ehufco = new int[256];   /* code for each symbol */
            public char[] ehufsi = new char[256];       /* length of code for each symbol */
            /* If no code has been allocated for a symbol S, ehufsi[S] contains 0 */
        }

        /* The legal range of a DCT coefficient is
        *  -1024 .. +1023  for 8-bit data;
        * -16384 .. +16383 for 12-bit data.
        * Hence the magnitude should always fit in 10 or 14 bits respectively.
        */
        protected static int MAX_HUFFMAN_COEF_BITS = 10;
        private static int MAX_CLEN = 32;     /* assumed maximum initial code length */

        protected JpegCompressor m_cinfo;

        public abstract void start_pass(bool gather_statistics);
        public abstract bool encode_mcu(JpegBlock[][] MCU_data);
        public abstract void finish_pass();

        /// <summary>
        /// Expand a Huffman table definition into the derived format
        /// Compute the derived values for a Huffman table.
        /// This routine also performs some validation checks on the table.
        /// </summary>
        protected void jpeg_make_c_derived_tbl(bool isDC, int tblno, ref c_derived_tbl dtbl)
        {
            /* Note that huffsize[] and huffcode[] are filled in code-length order,
            * paralleling the order of the symbols themselves in htbl.huffval[].
            */

            /* Find the input Huffman table */
            if (tblno < 0 || tblno >= JpegConstants.NumberOfHuffmanTables)
                throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", tblno));

            JpegHuffmanTable htbl = isDC ? m_cinfo.m_dc_huff_tbl_ptrs[tblno] : m_cinfo.m_ac_huff_tbl_ptrs[tblno];
            if (htbl == null)
                throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", tblno));

            /* Allocate a workspace if we haven't already done so. */
            if (dtbl == null)
                dtbl = new c_derived_tbl();

            /* Figure C.1: make table of Huffman code length for each symbol */

            int p = 0;
            char[] huffsize = new char[257];
            for (int l = 1; l <= 16; l++)
            {
                int i = htbl.Bits[l];
                if (i < 0 || p + i > 256)    /* protect against table overrun */
                    throw new Exception("Bogus Huffman table definition");

                while ((i--) != 0)
                    huffsize[p++] = (char)l;
            }
            huffsize[p] = (char)0;
            int lastp = p;

            /* Figure C.2: generate the codes themselves */
            /* We also validate that the counts represent a legal Huffman code tree. */

            int code = 0;
            int si = huffsize[0];
            p = 0;
            int[] huffcode = new int[257];
            while (huffsize[p] != 0)
            {
                while (((int)huffsize[p]) == si)
                {
                    huffcode[p++] = code;
                    code++;
                }
                /* code is now 1 more than the last code used for codelength si; but
                * it must still fit in si bits, since no code is allowed to be all ones.
                */
                if (code >= (1 << si))
                    throw new Exception("Bogus Huffman table definition");
                code <<= 1;
                si++;
            }

            /* Figure C.3: generate encoding tables */
            /* These are code and size indexed by symbol value */

            /* Set all codeless symbols to have code length 0;
            * this lets us detect duplicate VAL entries here, and later
            * allows emit_bits to detect any attempt to emit such symbols.
            */
            Array.Clear(dtbl.ehufsi, 0, dtbl.ehufsi.Length);

            /* This is also a convenient place to check for out-of-range
            * and duplicated VAL entries.  We allow 0..255 for AC symbols
            * but only 0..15 for DC.  (We could constrain them further
            * based on data depth and mode, but this seems enough.)
            */
            int maxsymbol = isDC ? 15 : 255;

            for (p = 0; p < lastp; p++)
            {
                int i = htbl.Huffval[p];
                if (i < 0 || i > maxsymbol || dtbl.ehufsi[i] != 0)
                    throw new Exception("Bogus Huffman table definition");

                dtbl.ehufco[i] = huffcode[p];
                dtbl.ehufsi[i] = huffsize[p];
            }
        }

        /// <summary>
        /// Generate the best Huffman code table for the given counts, fill htbl.
        /// 
        /// The JPEG standard requires that no symbol be assigned a codeword of all
        /// one bits (so that padding bits added at the end of a compressed segment
        /// can't look like a valid code).  Because of the canonical ordering of
        /// codewords, this just means that there must be an unused slot in the
        /// longest codeword length category.  Section K.2 of the JPEG spec suggests
        /// reserving such a slot by pretending that symbol 256 is a valid symbol
        /// with count 1.  In theory that's not optimal; giving it count zero but
        /// including it in the symbol set anyway should give a better Huffman code.
        /// But the theoretically better code actually seems to come out worse in
        /// practice, because it produces more all-ones bytes (which incur stuffed
        /// zero bytes in the final file).  In any case the difference is tiny.
        /// 
        /// The JPEG standard requires Huffman codes to be no more than 16 bits long.
        /// If some symbols have a very small but nonzero probability, the Huffman tree
        /// must be adjusted to meet the code length restriction.  We currently use
        /// the adjustment method suggested in JPEG section K.2.  This method is *not*
        /// optimal; it may not choose the best possible limited-length code.  But
        /// typically only very-low-frequency symbols will be given less-than-optimal
        /// lengths, so the code is almost optimal.  Experimental comparisons against
        /// an optimal limited-length-code algorithm indicate that the difference is
        /// microscopic --- usually less than a hundredth of a percent of total size.
        /// So the extra complexity of an optimal algorithm doesn't seem worthwhile.
        /// </summary>
        protected void jpeg_gen_optimal_table(JpegHuffmanTable htbl, long[] freq)
        {
            byte[] bits = new byte[MAX_CLEN + 1];   /* bits[k] = # of symbols with code length k */
            int[] codesize = new int[257];      /* codesize[k] = code length of symbol k */
            int[] others = new int[257];        /* next symbol in current branch of tree */
            int c1, c2;
            int p, i, j;
            long v;

            /* This algorithm is explained in section K.2 of the JPEG standard */
            for (i = 0; i < 257; i++)
                others[i] = -1;     /* init links to empty */

            freq[256] = 1;      /* make sure 256 has a nonzero count */
            /* Including the pseudo-symbol 256 in the Huffman procedure guarantees
            * that no real symbol is given code-value of all ones, because 256
            * will be placed last in the largest codeword category.
            */

            /* Huffman's basic algorithm to assign optimal code lengths to symbols */

            for (; ; )
            {
                /* Find the smallest nonzero frequency, set c1 = its symbol */
                /* In case of ties, take the larger symbol number */
                c1 = -1;
                v = 1000000000L;
                for (i = 0; i <= 256; i++)
                {
                    if (freq[i] != 0 && freq[i] <= v)
                    {
                        v = freq[i];
                        c1 = i;
                    }
                }

                /* Find the next smallest nonzero frequency, set c2 = its symbol */
                /* In case of ties, take the larger symbol number */
                c2 = -1;
                v = 1000000000L;
                for (i = 0; i <= 256; i++)
                {
                    if (freq[i] != 0 && freq[i] <= v && i != c1)
                    {
                        v = freq[i];
                        c2 = i;
                    }
                }

                /* Done if we've merged everything into one frequency */
                if (c2 < 0)
                    break;

                /* Else merge the two counts/trees */
                freq[c1] += freq[c2];
                freq[c2] = 0;

                /* Increment the codesize of everything in c1's tree branch */
                codesize[c1]++;
                while (others[c1] >= 0)
                {
                    c1 = others[c1];
                    codesize[c1]++;
                }

                others[c1] = c2;        /* chain c2 onto c1's tree branch */

                /* Increment the codesize of everything in c2's tree branch */
                codesize[c2]++;
                while (others[c2] >= 0)
                {
                    c2 = others[c2];
                    codesize[c2]++;
                }
            }

            /* Now count the number of symbols of each code length */
            for (i = 0; i <= 256; i++)
            {
                if (codesize[i] != 0)
                {
                    /* The JPEG standard seems to think that this can't happen, */
                    /* but I'm paranoid... */
                    if (codesize[i] > MAX_CLEN)
                        throw new Exception("Huffman code size table overflow");

                    bits[codesize[i]]++;
                }
            }

            /* JPEG doesn't allow symbols with code lengths over 16 bits, so if the pure
            * Huffman procedure assigned any such lengths, we must adjust the coding.
            * Here is what the JPEG spec says about how this next bit works:
            * Since symbols are paired for the longest Huffman code, the symbols are
            * removed from this length category two at a time.  The prefix for the pair
            * (which is one bit shorter) is allocated to one of the pair; then,
            * skipping the BITS entry for that prefix length, a code word from the next
            * shortest nonzero BITS entry is converted into a prefix for two code words
            * one bit longer.
            */

            for (i = MAX_CLEN; i > 16; i--)
            {
                while (bits[i] > 0)
                {
                    j = i - 2;      /* find length of new prefix to be used */
                    while (bits[j] == 0)
                        j--;

                    bits[i] -= 2;       /* remove two symbols */
                    bits[i - 1]++;      /* one goes in this length */
                    bits[j + 1] += 2;       /* two new symbols in this length */
                    bits[j]--;      /* symbol of this length is now a prefix */
                }
            }

            /* Remove the count for the pseudo-symbol 256 from the largest codelength */
            while (bits[i] == 0)        /* find largest codelength still in use */
                i--;
            bits[i]--;

            /* Return final symbol counts (only for lengths 0..16) */
            Buffer.BlockCopy(bits, 0, htbl.Bits, 0, htbl.Bits.Length);

            /* Return a list of the symbols sorted by code length */
            /* It's not real clear to me why we don't need to consider the codelength
            * changes made above, but the JPEG spec seems to think this works.
            */
            p = 0;
            for (i = 1; i <= MAX_CLEN; i++)
            {
                for (j = 0; j <= 255; j++)
                {
                    if (codesize[j] == i)
                    {
                        htbl.Huffval[p] = (byte)j;
                        p++;
                    }
                }
            }

            /* Set sent_table false so updated table will be written to JPEG file. */
            htbl.Sent_table = false;
        }
    }
    #endregion

    #region JpegFowardDCT
    /// <summary>
    /// Forward DCT (also controls coefficient quantization)
    /// 
    /// A forward DCT routine is given a pointer to a work area of type DCTELEM[];
    /// the DCT is to be performed in-place in that buffer.  Type DCTELEM is int
    /// for 8-bit samples, int for 12-bit samples.  (NOTE: Floating-point DCT
    /// implementations use an array of type float, instead.)
    /// The DCT inputs are expected to be signed (range +-MediumSampleValue).
    /// The DCT outputs are returned scaled up by a factor of 8; they therefore
    /// have a range of +-8K for 8-bit data, +-128K for 12-bit data. This
    /// convention improves accuracy in integer implementations and saves some
    /// work in floating-point ones.
    /// 
    /// Each IDCT routine has its own ideas about the best dct_table element type.
    /// </summary>
    class JpegFowardDCT
    {
        private const int FAST_INTEGER_CONST_BITS = 8;

        /* We use the following pre-calculated constants.
        * If you change FAST_INTEGER_CONST_BITS you may want to add appropriate values.
        * 
        * Convert a positive real constant to an integer scaled by CONST_SCALE.
        * static int FAST_INTEGER_FIX(double x)
        *{
        *    return ((int) ((x) * (((int) 1) << FAST_INTEGER_CONST_BITS) + 0.5));
        *}
        */
        private const int FAST_INTEGER_FIX_0_382683433 = 98;        /* FIX(0.382683433) */
        private const int FAST_INTEGER_FIX_0_541196100 = 139;       /* FIX(0.541196100) */
        private const int FAST_INTEGER_FIX_0_707106781 = 181;       /* FIX(0.707106781) */
        private const int FAST_INTEGER_FIX_1_306562965 = 334;       /* FIX(1.306562965) */

        private const int SLOW_INTEGER_CONST_BITS = 13;
        private const int SLOW_INTEGER_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change SLOW_INTEGER_CONST_BITS you may want to add appropriate values.
        * 
        * Convert a positive real constant to an integer scaled by CONST_SCALE.
        *
        * static int SLOW_INTEGER_FIX(double x)
        * {
        *     return ((int) ((x) * (((int) 1) << SLOW_INTEGER_CONST_BITS) + 0.5));
        * }
        */
        private const int SLOW_INTEGER_FIX_0_298631336 = 2446;   /* FIX(0.298631336) */
        private const int SLOW_INTEGER_FIX_0_390180644 = 3196;   /* FIX(0.390180644) */
        private const int SLOW_INTEGER_FIX_0_541196100 = 4433;   /* FIX(0.541196100) */
        private const int SLOW_INTEGER_FIX_0_765366865 = 6270;   /* FIX(0.765366865) */
        private const int SLOW_INTEGER_FIX_0_899976223 = 7373;   /* FIX(0.899976223) */
        private const int SLOW_INTEGER_FIX_1_175875602 = 9633;   /* FIX(1.175875602) */
        private const int SLOW_INTEGER_FIX_1_501321110 = 12299;  /* FIX(1.501321110) */
        private const int SLOW_INTEGER_FIX_1_847759065 = 15137;  /* FIX(1.847759065) */
        private const int SLOW_INTEGER_FIX_1_961570560 = 16069;  /* FIX(1.961570560) */
        private const int SLOW_INTEGER_FIX_2_053119869 = 16819;  /* FIX(2.053119869) */
        private const int SLOW_INTEGER_FIX_2_562915447 = 20995;  /* FIX(2.562915447) */
        private const int SLOW_INTEGER_FIX_3_072711026 = 25172;  /* FIX(3.072711026) */

        /* For AA&N IDCT method, divisors are equal to quantization
         * coefficients scaled by scalefactor[row]*scalefactor[col], where
         *   scalefactor[0] = 1
         *   scalefactor[k] = cos(k*PI/16) * sqrt(2)    for k=1..7
         * We apply a further scale factor of 8.
         */
        private const int CONST_BITS = 14;

        /* precomputed values scaled up by 14 bits */
        private static short[] aanscales = { 
            16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 22725, 31521, 29692, 26722, 22725, 17855,
            12299, 6270, 21407, 29692, 27969, 25172, 21407, 16819, 11585,
            5906, 19266, 26722, 25172, 22654, 19266, 15137, 10426, 5315,
            16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 12873,
            17855, 16819, 15137, 12873, 10114, 6967, 3552, 8867, 12299,
            11585, 10426, 8867, 6967, 4799, 2446, 4520, 6270, 5906, 5315,
            4520, 3552, 2446, 1247 };

        /* For float AA&N IDCT method, divisors are equal to quantization
         * coefficients scaled by scalefactor[row]*scalefactor[col], where
         *   scalefactor[0] = 1
         *   scalefactor[k] = cos(k*PI/16) * sqrt(2)    for k=1..7
         * We apply a further scale factor of 8.
         * What's actually stored is 1/divisor so that the inner loop can
         * use a multiplication rather than a division.
         */
        private static double[] aanscalefactor = { 
            1.0, 1.387039845, 1.306562965, 1.175875602, 1.0,
            0.785694958, 0.541196100, 0.275899379 };

        private JpegCompressor m_cinfo;
        private bool m_useSlowMethod;
        private bool m_useFloatMethod;

        /* The actual post-DCT divisors --- not identical to the quant table
        * entries, because of scaling (especially for an unnormalized DCT).
        * Each table is given in normal array order.
        */
        private int[][] m_divisors = new int[JpegConstants.NumberOfQuantTables][];

        /* Same as above for the floating-point case. */
        private float[][] m_float_divisors = new float[JpegConstants.NumberOfQuantTables][];

        public JpegFowardDCT(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;

            switch (cinfo.m_dct_method)
            {
                case DCTMethod.IntSlow:
                    m_useFloatMethod = false;
                    m_useSlowMethod = true;
                    break;
                case DCTMethod.IntFast:
                    m_useFloatMethod = false;
                    m_useSlowMethod = false;
                    break;
                case DCTMethod.Float:
                    m_useFloatMethod = true;
                    break;
                default:
                    throw new Exception("Unknown dct method!");
            }

            /* Mark divisor tables unallocated */
            for (int i = 0; i < JpegConstants.NumberOfQuantTables; i++)
            {
                m_divisors[i] = null;
                m_float_divisors[i] = null;
            }
        }

        /// <summary>
        /// Initialize for a processing pass.
        /// Verify that all referenced Q-tables are present, and set up
        /// the divisor table for each one.
        /// In the current implementation, DCT of all components is done during
        /// the first pass, even if only some components will be output in the
        /// first scan.  Hence all components should be examined here.
        /// </summary>
        public virtual void start_pass()
        {
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                int qtblno = m_cinfo.Component_info[ci].Quant_tbl_no;

                /* Make sure specified quantization table is present */
                if (qtblno < 0 || qtblno >= JpegConstants.NumberOfQuantTables || m_cinfo.m_quant_tbl_ptrs[qtblno] == null)
                    throw new Exception(String.Format("Quantization table 0x{0:X2} was not defined", qtblno));

                JpegQuantizationTable qtbl = m_cinfo.m_quant_tbl_ptrs[qtblno];

                /* Compute divisors for this quant table */
                /* We may do this more than once for same table, but it's not a big deal */
                int i = 0;
                switch (m_cinfo.m_dct_method)
                {
                    case DCTMethod.IntSlow:
                        /* For LL&M IDCT method, divisors are equal to raw quantization
                         * coefficients multiplied by 8 (to counteract scaling).
                         */
                        if (m_divisors[qtblno] == null)
                            m_divisors[qtblno] = new int[JpegConstants.DCTSize2];

                        for (i = 0; i < JpegConstants.DCTSize2; i++)
                            m_divisors[qtblno][i] = ((int)qtbl.quantval[i]) << 3;

                        break;
                    case DCTMethod.IntFast:
                        if (m_divisors[qtblno] == null)
                            m_divisors[qtblno] = new int[JpegConstants.DCTSize2];

                        for (i = 0; i < JpegConstants.DCTSize2; i++)
                            m_divisors[qtblno][i] = JpegUtils.DESCALE((int)qtbl.quantval[i] * (int)aanscales[i], CONST_BITS - 3);
                        break;
                    case DCTMethod.Float:
                        if (m_float_divisors[qtblno] == null)
                            m_float_divisors[qtblno] = new float[JpegConstants.DCTSize2];

                        float[] fdtbl = m_float_divisors[qtblno];
                        i = 0;
                        for (int row = 0; row < JpegConstants.DCTSize; row++)
                        {
                            for (int col = 0; col < JpegConstants.DCTSize; col++)
                            {
                                fdtbl[i] = (float)(1.0 / (((double)qtbl.quantval[i] * aanscalefactor[row] * aanscalefactor[col] * 8.0)));
                                i++;
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unknown dct method!");
                }
            }
        }

        /// <summary>
        /// Perform forward DCT on one or more blocks of a component.
        /// 
        /// The input samples are taken from the sample_data[] array starting at
        /// position start_row/start_col, and moving to the right for any additional
        /// blocks. The quantized coefficients are returned in coef_blocks[].
        /// </summary>
        public virtual void forward_DCT(int quant_tbl_no, byte[][] sample_data, JpegBlock[] coef_blocks, int start_row, int start_col, int num_blocks)
        {
            if (m_useFloatMethod)
                forwardDCTFloatImpl(quant_tbl_no, sample_data, coef_blocks, start_row, start_col, num_blocks);
            else
                forwardDCTImpl(quant_tbl_no, sample_data, coef_blocks, start_row, start_col, num_blocks);
        }

        // This version is used for integer DCT implementations.
        private void forwardDCTImpl(int quant_tbl_no, byte[][] sample_data, JpegBlock[] coef_blocks, int start_row, int start_col, int num_blocks)
        {
            /* This routine is heavily used, so it's worth coding it tightly. */
            int[] workspace = new int[JpegConstants.DCTSize2];    /* work area for FDCT subroutine */
            for (int bi = 0; bi < num_blocks; bi++, start_col += JpegConstants.DCTSize)
            {
                /* Load data into workspace, applying unsigned->signed conversion */
                int workspaceIndex = 0;
                for (int elemr = 0; elemr < JpegConstants.DCTSize; elemr++)
                {
                    for (int column = 0; column < JpegConstants.DCTSize; column++)
                    {
                        workspace[workspaceIndex] = (int)sample_data[start_row + elemr][start_col + column] - JpegConstants.MediumSampleValue;
                        workspaceIndex++;
                    }
                }

                /* Perform the DCT */
                if (m_useSlowMethod)
                    jpeg_fdct_islow(workspace);
                else
                    jpeg_fdct_ifast(workspace);

                /* Quantize/descale the coefficients, and store into coef_blocks[] */
                for (int i = 0; i < JpegConstants.DCTSize2; i++)
                {
                    int qval = m_divisors[quant_tbl_no][i];
                    int temp = workspace[i];

                    if (temp < 0)
                    {
                        temp = -temp;
                        temp += qval >> 1;  /* for rounding */

                        if (temp >= qval)
                            temp /= qval;
                        else
                            temp = 0;

                        temp = -temp;
                    }
                    else
                    {
                        temp += qval >> 1;  /* for rounding */

                        if (temp >= qval)
                            temp /= qval;
                        else
                            temp = 0;
                    }

                    coef_blocks[bi][i] = (short)temp;
                }
            }
        }

        // This version is used for floating-point DCT implementations.
        private void forwardDCTFloatImpl(int quant_tbl_no, byte[][] sample_data, JpegBlock[] coef_blocks, int start_row, int start_col, int num_blocks)
        {
            /* This routine is heavily used, so it's worth coding it tightly. */
            float[] workspace = new float[JpegConstants.DCTSize2]; /* work area for FDCT subroutine */
            for (int bi = 0; bi < num_blocks; bi++, start_col += JpegConstants.DCTSize)
            {
                /* Load data into workspace, applying unsigned->signed conversion */
                int workspaceIndex = 0;
                for (int elemr = 0; elemr < JpegConstants.DCTSize; elemr++)
                {
                    for (int column = 0; column < JpegConstants.DCTSize; column++)
                    {
                        workspace[workspaceIndex] = (float)((int)sample_data[start_row + elemr][start_col + column] - JpegConstants.MediumSampleValue);
                        workspaceIndex++;
                    }
                }

                /* Perform the DCT */
                jpeg_fdct_float(workspace);

                /* Quantize/descale the coefficients, and store into coef_blocks[] */
                for (int i = 0; i < JpegConstants.DCTSize2; i++)
                {
                    /* Apply the quantization and scaling factor */
                    float temp = workspace[i] * m_float_divisors[quant_tbl_no][i];

                    /* Round to nearest integer.
                     * Since C does not specify the direction of rounding for negative
                     * quotients, we have to force the dividend positive for portability.
                     * The maximum coefficient size is +-16K (for 12-bit data), so this
                     * code should work for either 16-bit or 32-bit ints.
                     */
                    coef_blocks[bi][i] = (short)((int)(temp + (float)16384.5) - 16384);
                }
            }
        }

        /// <summary>
        /// Perform the forward DCT on one block of samples.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// A floating-point implementation of the 
        /// forward DCT (Discrete Cosine Transform).
        /// 
        /// This implementation should be more accurate than either of the integer
        /// DCT implementations.  However, it may not give the same results on all
        /// machines because of differences in roundoff behavior.  Speed will depend
        /// on the hardware's floating point capacity.
        /// 
        /// A 2-D DCT can be done by 1-D DCT on each row followed by 1-D DCT
        /// on each column.  Direct algorithms are also available, but they are
        /// much more complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on Arai, Agui, and Nakajima's algorithm for
        /// scaled DCT.  Their original paper (Trans. IEICE E-71(11):1095) is in
        /// Japanese, but the algorithm is described in the Pennebaker &amp; Mitchell
        /// JPEG textbook (see REFERENCES section in file README).  The following code
        /// is based directly on figure 4-8 in P&amp;M.
        /// While an 8-point DCT cannot be done in less than 11 multiplies, it is
        /// possible to arrange the computation so that many of the multiplies are
        /// simple scalings of the final outputs.  These multiplies can then be
        /// folded into the multiplications or divisions by the JPEG quantization
        /// table entries.  The AA&amp;N method leaves only 5 multiplies and 29 adds
        /// to be done in the DCT itself.
        /// The primary disadvantage of this method is that with a fixed-point
        /// implementation, accuracy is lost due to imprecise representation of the
        /// scaled quantization values.  However, that problem does not arise if
        /// we use floating point arithmetic.
        /// </summary>
        private static void jpeg_fdct_float(float[] data)
        {
            /* Pass 1: process rows. */
            int dataIndex = 0;
            for (int ctr = JpegConstants.DCTSize - 1; ctr >= 0; ctr--)
            {
                float tmp0 = data[dataIndex + 0] + data[dataIndex + 7];
                float tmp7 = data[dataIndex + 0] - data[dataIndex + 7];
                float tmp1 = data[dataIndex + 1] + data[dataIndex + 6];
                float tmp6 = data[dataIndex + 1] - data[dataIndex + 6];
                float tmp2 = data[dataIndex + 2] + data[dataIndex + 5];
                float tmp5 = data[dataIndex + 2] - data[dataIndex + 5];
                float tmp3 = data[dataIndex + 3] + data[dataIndex + 4];
                float tmp4 = data[dataIndex + 3] - data[dataIndex + 4];

                /* Even part */

                float tmp10 = tmp0 + tmp3;    /* phase 2 */
                float tmp13 = tmp0 - tmp3;
                float tmp11 = tmp1 + tmp2;
                float tmp12 = tmp1 - tmp2;

                data[dataIndex + 0] = tmp10 + tmp11; /* phase 3 */
                data[dataIndex + 4] = tmp10 - tmp11;

                float z1 = (tmp12 + tmp13) * ((float)0.707106781); /* c4 */
                data[dataIndex + 2] = tmp13 + z1;    /* phase 5 */
                data[dataIndex + 6] = tmp13 - z1;

                /* Odd part */

                tmp10 = tmp4 + tmp5;    /* phase 2 */
                tmp11 = tmp5 + tmp6;
                tmp12 = tmp6 + tmp7;

                /* The rotator is modified from fig 4-8 to avoid extra negations. */
                float z5 = (tmp10 - tmp12) * ((float)0.382683433); /* c6 */
                float z2 = ((float)0.541196100) * tmp10 + z5; /* c2-c6 */
                float z4 = ((float)1.306562965) * tmp12 + z5; /* c2+c6 */
                float z3 = tmp11 * ((float)0.707106781); /* c4 */

                float z11 = tmp7 + z3;        /* phase 5 */
                float z13 = tmp7 - z3;

                data[dataIndex + 5] = z13 + z2;  /* phase 6 */
                data[dataIndex + 3] = z13 - z2;
                data[dataIndex + 1] = z11 + z4;
                data[dataIndex + 7] = z11 - z4;

                dataIndex += JpegConstants.DCTSize;     /* advance pointer to next row */
            }

            /* Pass 2: process columns. */

            dataIndex = 0;
            for (int ctr = JpegConstants.DCTSize - 1; ctr >= 0; ctr--)
            {
                float tmp0 = data[dataIndex + JpegConstants.DCTSize * 0] + data[dataIndex + JpegConstants.DCTSize * 7];
                float tmp7 = data[dataIndex + JpegConstants.DCTSize * 0] - data[dataIndex + JpegConstants.DCTSize * 7];
                float tmp1 = data[dataIndex + JpegConstants.DCTSize * 1] + data[dataIndex + JpegConstants.DCTSize * 6];
                float tmp6 = data[dataIndex + JpegConstants.DCTSize * 1] - data[dataIndex + JpegConstants.DCTSize * 6];
                float tmp2 = data[dataIndex + JpegConstants.DCTSize * 2] + data[dataIndex + JpegConstants.DCTSize * 5];
                float tmp5 = data[dataIndex + JpegConstants.DCTSize * 2] - data[dataIndex + JpegConstants.DCTSize * 5];
                float tmp3 = data[dataIndex + JpegConstants.DCTSize * 3] + data[dataIndex + JpegConstants.DCTSize * 4];
                float tmp4 = data[dataIndex + JpegConstants.DCTSize * 3] - data[dataIndex + JpegConstants.DCTSize * 4];

                /* Even part */

                float tmp10 = tmp0 + tmp3;    /* phase 2 */
                float tmp13 = tmp0 - tmp3;
                float tmp11 = tmp1 + tmp2;
                float tmp12 = tmp1 - tmp2;

                data[dataIndex + JpegConstants.DCTSize * 0] = tmp10 + tmp11; /* phase 3 */
                data[dataIndex + JpegConstants.DCTSize * 4] = tmp10 - tmp11;

                float z1 = (tmp12 + tmp13) * ((float)0.707106781); /* c4 */
                data[dataIndex + JpegConstants.DCTSize * 2] = tmp13 + z1; /* phase 5 */
                data[dataIndex + JpegConstants.DCTSize * 6] = tmp13 - z1;

                /* Odd part */

                tmp10 = tmp4 + tmp5;    /* phase 2 */
                tmp11 = tmp5 + tmp6;
                tmp12 = tmp6 + tmp7;

                /* The rotator is modified from fig 4-8 to avoid extra negations. */
                float z5 = (tmp10 - tmp12) * ((float)0.382683433); /* c6 */
                float z2 = ((float)0.541196100) * tmp10 + z5; /* c2-c6 */
                float z4 = ((float)1.306562965) * tmp12 + z5; /* c2+c6 */
                float z3 = tmp11 * ((float)0.707106781); /* c4 */

                float z11 = tmp7 + z3;        /* phase 5 */
                float z13 = tmp7 - z3;

                data[dataIndex + JpegConstants.DCTSize * 5] = z13 + z2; /* phase 6 */
                data[dataIndex + JpegConstants.DCTSize * 3] = z13 - z2;
                data[dataIndex + JpegConstants.DCTSize * 1] = z11 + z4;
                data[dataIndex + JpegConstants.DCTSize * 7] = z11 - z4;

                dataIndex++;          /* advance pointer to next column */
            }
        }

        /// <summary>
        /// Perform the forward DCT on one block of samples.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// This file contains a fast, not so accurate integer implementation of the
        /// forward DCT (Discrete Cosine Transform).
        /// 
        /// A 2-D DCT can be done by 1-D DCT on each row followed by 1-D DCT
        /// on each column.  Direct algorithms are also available, but they are
        /// much more complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on Arai, Agui, and Nakajima's algorithm for
        /// scaled DCT.  Their original paper (Trans. IEICE E-71(11):1095) is in
        /// Japanese, but the algorithm is described in the Pennebaker &amp; Mitchell
        /// JPEG textbook (see REFERENCES section in file README).  The following code
        /// is based directly on figure 4-8 in P&amp;M.
        /// While an 8-point DCT cannot be done in less than 11 multiplies, it is
        /// possible to arrange the computation so that many of the multiplies are
        /// simple scalings of the final outputs.  These multiplies can then be
        /// folded into the multiplications or divisions by the JPEG quantization
        /// table entries.  The AA&amp;N method leaves only 5 multiplies and 29 adds
        /// to be done in the DCT itself.
        /// The primary disadvantage of this method is that with fixed-point math,
        /// accuracy is lost due to imprecise representation of the scaled
        /// quantization values.  The smaller the quantization table entry, the less
        /// precise the scaled value, so this implementation does worse with high-
        /// quality-setting files than with low-quality ones.
        /// 
        /// Scaling decisions are generally the same as in the LL&amp;M algorithm;
        /// see jpeg_fdct_islow for more details.  However, we choose to descale
        /// (right shift) multiplication products as soon as they are formed,
        /// rather than carrying additional fractional bits into subsequent additions.
        /// This compromises accuracy slightly, but it lets us save a few shifts.
        /// More importantly, 16-bit arithmetic is then adequate (for 8-bit samples)
        /// everywhere except in the multiplications proper; this saves a good deal
        /// of work on 16-bit-int machines.
        /// 
        /// Again to save a few shifts, the intermediate results between pass 1 and
        /// pass 2 are not upscaled, but are represented only to integral precision.
        /// 
        /// A final compromise is to represent the multiplicative constants to only
        /// 8 fractional bits, rather than 13.  This saves some shifting work on some
        /// machines, and may also reduce the cost of multiplication (since there
        /// are fewer one-bits in the constants).
        /// </summary>
        private static void jpeg_fdct_ifast(int[] data)
        {
            /* Pass 1: process rows. */
            int dataIndex = 0;
            for (int ctr = JpegConstants.DCTSize - 1; ctr >= 0; ctr--)
            {
                int tmp0 = data[dataIndex + 0] + data[dataIndex + 7];
                int tmp7 = data[dataIndex + 0] - data[dataIndex + 7];
                int tmp1 = data[dataIndex + 1] + data[dataIndex + 6];
                int tmp6 = data[dataIndex + 1] - data[dataIndex + 6];
                int tmp2 = data[dataIndex + 2] + data[dataIndex + 5];
                int tmp5 = data[dataIndex + 2] - data[dataIndex + 5];
                int tmp3 = data[dataIndex + 3] + data[dataIndex + 4];
                int tmp4 = data[dataIndex + 3] - data[dataIndex + 4];

                /* Even part */

                int tmp10 = tmp0 + tmp3;    /* phase 2 */
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                data[dataIndex + 0] = tmp10 + tmp11; /* phase 3 */
                data[dataIndex + 4] = tmp10 - tmp11;

                int z1 = FAST_INTEGER_MULTIPLY(tmp12 + tmp13, FAST_INTEGER_FIX_0_707106781); /* c4 */
                data[dataIndex + 2] = tmp13 + z1;    /* phase 5 */
                data[dataIndex + 6] = tmp13 - z1;

                /* Odd part */

                tmp10 = tmp4 + tmp5;    /* phase 2 */
                tmp11 = tmp5 + tmp6;
                tmp12 = tmp6 + tmp7;

                /* The rotator is modified from fig 4-8 to avoid extra negations. */
                int z5 = FAST_INTEGER_MULTIPLY(tmp10 - tmp12, FAST_INTEGER_FIX_0_382683433); /* c6 */
                int z2 = FAST_INTEGER_MULTIPLY(tmp10, FAST_INTEGER_FIX_0_541196100) + z5; /* c2-c6 */
                int z4 = FAST_INTEGER_MULTIPLY(tmp12, FAST_INTEGER_FIX_1_306562965) + z5; /* c2+c6 */
                int z3 = FAST_INTEGER_MULTIPLY(tmp11, FAST_INTEGER_FIX_0_707106781); /* c4 */

                int z11 = tmp7 + z3;        /* phase 5 */
                int z13 = tmp7 - z3;

                data[dataIndex + 5] = z13 + z2;  /* phase 6 */
                data[dataIndex + 3] = z13 - z2;
                data[dataIndex + 1] = z11 + z4;
                data[dataIndex + 7] = z11 - z4;

                dataIndex += JpegConstants.DCTSize;     /* advance pointer to next row */
            }

            /* Pass 2: process columns. */

            dataIndex = 0;
            for (int ctr = JpegConstants.DCTSize - 1; ctr >= 0; ctr--)
            {
                int tmp0 = data[dataIndex + JpegConstants.DCTSize * 0] + data[dataIndex + JpegConstants.DCTSize * 7];
                int tmp7 = data[dataIndex + JpegConstants.DCTSize * 0] - data[dataIndex + JpegConstants.DCTSize * 7];
                int tmp1 = data[dataIndex + JpegConstants.DCTSize * 1] + data[dataIndex + JpegConstants.DCTSize * 6];
                int tmp6 = data[dataIndex + JpegConstants.DCTSize * 1] - data[dataIndex + JpegConstants.DCTSize * 6];
                int tmp2 = data[dataIndex + JpegConstants.DCTSize * 2] + data[dataIndex + JpegConstants.DCTSize * 5];
                int tmp5 = data[dataIndex + JpegConstants.DCTSize * 2] - data[dataIndex + JpegConstants.DCTSize * 5];
                int tmp3 = data[dataIndex + JpegConstants.DCTSize * 3] + data[dataIndex + JpegConstants.DCTSize * 4];
                int tmp4 = data[dataIndex + JpegConstants.DCTSize * 3] - data[dataIndex + JpegConstants.DCTSize * 4];

                /* Even part */

                int tmp10 = tmp0 + tmp3;    /* phase 2 */
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                data[dataIndex + JpegConstants.DCTSize * 0] = tmp10 + tmp11; /* phase 3 */
                data[dataIndex + JpegConstants.DCTSize * 4] = tmp10 - tmp11;

                int z1 = FAST_INTEGER_MULTIPLY(tmp12 + tmp13, FAST_INTEGER_FIX_0_707106781); /* c4 */
                data[dataIndex + JpegConstants.DCTSize * 2] = tmp13 + z1; /* phase 5 */
                data[dataIndex + JpegConstants.DCTSize * 6] = tmp13 - z1;

                /* Odd part */

                tmp10 = tmp4 + tmp5;    /* phase 2 */
                tmp11 = tmp5 + tmp6;
                tmp12 = tmp6 + tmp7;

                /* The rotator is modified from fig 4-8 to avoid extra negations. */
                int z5 = FAST_INTEGER_MULTIPLY(tmp10 - tmp12, FAST_INTEGER_FIX_0_382683433); /* c6 */
                int z2 = FAST_INTEGER_MULTIPLY(tmp10, FAST_INTEGER_FIX_0_541196100) + z5; /* c2-c6 */
                int z4 = FAST_INTEGER_MULTIPLY(tmp12, FAST_INTEGER_FIX_1_306562965) + z5; /* c2+c6 */
                int z3 = FAST_INTEGER_MULTIPLY(tmp11, FAST_INTEGER_FIX_0_707106781); /* c4 */

                int z11 = tmp7 + z3;        /* phase 5 */
                int z13 = tmp7 - z3;

                data[dataIndex + JpegConstants.DCTSize * 5] = z13 + z2; /* phase 6 */
                data[dataIndex + JpegConstants.DCTSize * 3] = z13 - z2;
                data[dataIndex + JpegConstants.DCTSize * 1] = z11 + z4;
                data[dataIndex + JpegConstants.DCTSize * 7] = z11 - z4;

                dataIndex++;          /* advance pointer to next column */
            }
        }

        /// <summary>
        /// Perform the forward DCT on one block of samples.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// A slow-but-accurate integer implementation of the
        /// forward DCT (Discrete Cosine Transform).
        /// 
        /// A 2-D DCT can be done by 1-D DCT on each row followed by 1-D DCT
        /// on each column.  Direct algorithms are also available, but they are
        /// much more complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on an algorithm described in
        /// C. Loeffler, A. Ligtenberg and G. Moschytz, "Practical Fast 1-D DCT
        /// Algorithms with 11 Multiplications", Proc. Int'l. Conf. on Acoustics,
        /// Speech, and Signal Processing 1989 (ICASSP '89), pp. 988-991.
        /// The primary algorithm described there uses 11 multiplies and 29 adds.
        /// We use their alternate method with 12 multiplies and 32 adds.
        /// The advantage of this method is that no data path contains more than one
        /// multiplication; this allows a very simple and accurate implementation in
        /// scaled fixed-point arithmetic, with a minimal number of shifts.
        /// 
        /// The poop on this scaling stuff is as follows:
        /// 
        /// Each 1-D DCT step produces outputs which are a factor of sqrt(N)
        /// larger than the true DCT outputs.  The final outputs are therefore
        /// a factor of N larger than desired; since N=8 this can be cured by
        /// a simple right shift at the end of the algorithm.  The advantage of
        /// this arrangement is that we save two multiplications per 1-D DCT,
        /// because the y0 and y4 outputs need not be divided by sqrt(N).
        /// In the IJG code, this factor of 8 is removed by the quantization 
        /// step, NOT here.
        /// 
        /// We have to do addition and subtraction of the integer inputs, which
        /// is no problem, and multiplication by fractional constants, which is
        /// a problem to do in integer arithmetic.  We multiply all the constants
        /// by CONST_SCALE and convert them to integer constants (thus retaining
        /// SLOW_INTEGER_CONST_BITS bits of precision in the constants).  After doing a
        /// multiplication we have to divide the product by CONST_SCALE, with proper
        /// rounding, to produce the correct output.  This division can be done
        /// cheaply as a right shift of SLOW_INTEGER_CONST_BITS bits.  We postpone shifting
        /// as long as possible so that partial sums can be added together with
        /// full fractional precision.
        /// 
        /// The outputs of the first pass are scaled up by SLOW_INTEGER_PASS1_BITS bits so that
        /// they are represented to better-than-integral precision.  These outputs
        /// require BitsInSample + SLOW_INTEGER_PASS1_BITS + 3 bits; this fits in a 16-bit word
        /// with the recommended scaling.  (For 12-bit sample data, the intermediate
        /// array is int anyway.)
        /// 
        /// To avoid overflow of the 32-bit intermediate results in pass 2, we must
        /// have BitsInSample + SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS &lt;= 26.  Error analysis
        /// shows that the values given below are the most effective.
        /// </summary>
        private static void jpeg_fdct_islow(int[] data)
        {
            /* Pass 1: process rows. */
            /* Note results are scaled up by sqrt(8) compared to a true DCT; */
            /* furthermore, we scale the results by 2**SLOW_INTEGER_PASS1_BITS. */
            int dataIndex = 0;
            for (int ctr = JpegConstants.DCTSize - 1; ctr >= 0; ctr--)
            {
                int tmp0 = data[dataIndex + 0] + data[dataIndex + 7];
                int tmp7 = data[dataIndex + 0] - data[dataIndex + 7];
                int tmp1 = data[dataIndex + 1] + data[dataIndex + 6];
                int tmp6 = data[dataIndex + 1] - data[dataIndex + 6];
                int tmp2 = data[dataIndex + 2] + data[dataIndex + 5];
                int tmp5 = data[dataIndex + 2] - data[dataIndex + 5];
                int tmp3 = data[dataIndex + 3] + data[dataIndex + 4];
                int tmp4 = data[dataIndex + 3] - data[dataIndex + 4];

                /* Even part per LL&M figure 1 --- note that published figure is faulty;
                * rotator "sqrt(2)*c1" should be "sqrt(2)*c6".
                */

                int tmp10 = tmp0 + tmp3;
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                data[dataIndex + 0] = (tmp10 + tmp11) << SLOW_INTEGER_PASS1_BITS;
                data[dataIndex + 4] = (tmp10 - tmp11) << SLOW_INTEGER_PASS1_BITS;

                int z1 = (tmp12 + tmp13) * SLOW_INTEGER_FIX_0_541196100;
                data[dataIndex + 2] = JpegUtils.DESCALE(z1 + tmp13 * SLOW_INTEGER_FIX_0_765366865,
                                                SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + 6] = JpegUtils.DESCALE(z1 + tmp12 * (-SLOW_INTEGER_FIX_1_847759065),
                                                SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);

                /* Odd part per figure 8 --- note paper omits factor of sqrt(2).
                * cK represents cos(K*pi/16).
                * i0..i3 in the paper are tmp4..tmp7 here.
                */

                z1 = tmp4 + tmp7;
                int z2 = tmp5 + tmp6;
                int z3 = tmp4 + tmp6;
                int z4 = tmp5 + tmp7;
                int z5 = (z3 + z4) * SLOW_INTEGER_FIX_1_175875602; /* sqrt(2) * c3 */

                tmp4 = tmp4 * SLOW_INTEGER_FIX_0_298631336; /* sqrt(2) * (-c1+c3+c5-c7) */
                tmp5 = tmp5 * SLOW_INTEGER_FIX_2_053119869; /* sqrt(2) * ( c1+c3-c5+c7) */
                tmp6 = tmp6 * SLOW_INTEGER_FIX_3_072711026; /* sqrt(2) * ( c1+c3+c5-c7) */
                tmp7 = tmp7 * SLOW_INTEGER_FIX_1_501321110; /* sqrt(2) * ( c1+c3-c5-c7) */
                z1 = z1 * (-SLOW_INTEGER_FIX_0_899976223); /* sqrt(2) * (c7-c3) */
                z2 = z2 * (-SLOW_INTEGER_FIX_2_562915447); /* sqrt(2) * (-c1-c3) */
                z3 = z3 * (-SLOW_INTEGER_FIX_1_961570560); /* sqrt(2) * (-c3-c5) */
                z4 = z4 * (-SLOW_INTEGER_FIX_0_390180644); /* sqrt(2) * (c5-c3) */

                z3 += z5;
                z4 += z5;

                data[dataIndex + 7] = JpegUtils.DESCALE(tmp4 + z1 + z3, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + 5] = JpegUtils.DESCALE(tmp5 + z2 + z4, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + 3] = JpegUtils.DESCALE(tmp6 + z2 + z3, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + 1] = JpegUtils.DESCALE(tmp7 + z1 + z4, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);

                dataIndex += JpegConstants.DCTSize;     /* advance pointer to next row */
            }

            /* Pass 2: process columns.
            * We remove the SLOW_INTEGER_PASS1_BITS scaling, but leave the results scaled up
            * by an overall factor of 8.
            */

            dataIndex = 0;
            for (int ctr = JpegConstants.DCTSize - 1; ctr >= 0; ctr--)
            {
                int tmp0 = data[dataIndex + JpegConstants.DCTSize * 0] + data[dataIndex + JpegConstants.DCTSize * 7];
                int tmp7 = data[dataIndex + JpegConstants.DCTSize * 0] - data[dataIndex + JpegConstants.DCTSize * 7];
                int tmp1 = data[dataIndex + JpegConstants.DCTSize * 1] + data[dataIndex + JpegConstants.DCTSize * 6];
                int tmp6 = data[dataIndex + JpegConstants.DCTSize * 1] - data[dataIndex + JpegConstants.DCTSize * 6];
                int tmp2 = data[dataIndex + JpegConstants.DCTSize * 2] + data[dataIndex + JpegConstants.DCTSize * 5];
                int tmp5 = data[dataIndex + JpegConstants.DCTSize * 2] - data[dataIndex + JpegConstants.DCTSize * 5];
                int tmp3 = data[dataIndex + JpegConstants.DCTSize * 3] + data[dataIndex + JpegConstants.DCTSize * 4];
                int tmp4 = data[dataIndex + JpegConstants.DCTSize * 3] - data[dataIndex + JpegConstants.DCTSize * 4];

                /* Even part per LL&M figure 1 --- note that published figure is faulty;
                * rotator "sqrt(2)*c1" should be "sqrt(2)*c6".
                */

                int tmp10 = tmp0 + tmp3;
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                data[dataIndex + JpegConstants.DCTSize * 0] = JpegUtils.DESCALE(tmp10 + tmp11, SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + JpegConstants.DCTSize * 4] = JpegUtils.DESCALE(tmp10 - tmp11, SLOW_INTEGER_PASS1_BITS);

                int z1 = (tmp12 + tmp13) * SLOW_INTEGER_FIX_0_541196100;
                data[dataIndex + JpegConstants.DCTSize * 2] = JpegUtils.DESCALE(z1 + tmp13 * SLOW_INTEGER_FIX_0_765366865,
                                                          SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + JpegConstants.DCTSize * 6] = JpegUtils.DESCALE(z1 + tmp12 * (-SLOW_INTEGER_FIX_1_847759065),
                                                          SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS);

                /* Odd part per figure 8 --- note paper omits factor of sqrt(2).
                * cK represents cos(K*pi/16).
                * i0..i3 in the paper are tmp4..tmp7 here.
                */

                z1 = tmp4 + tmp7;
                int z2 = tmp5 + tmp6;
                int z3 = tmp4 + tmp6;
                int z4 = tmp5 + tmp7;
                int z5 = (z3 + z4) * SLOW_INTEGER_FIX_1_175875602; /* sqrt(2) * c3 */

                tmp4 = tmp4 * SLOW_INTEGER_FIX_0_298631336; /* sqrt(2) * (-c1+c3+c5-c7) */
                tmp5 = tmp5 * SLOW_INTEGER_FIX_2_053119869; /* sqrt(2) * ( c1+c3-c5+c7) */
                tmp6 = tmp6 * SLOW_INTEGER_FIX_3_072711026; /* sqrt(2) * ( c1+c3+c5-c7) */
                tmp7 = tmp7 * SLOW_INTEGER_FIX_1_501321110; /* sqrt(2) * ( c1+c3-c5-c7) */
                z1 = z1 * (-SLOW_INTEGER_FIX_0_899976223); /* sqrt(2) * (c7-c3) */
                z2 = z2 * (-SLOW_INTEGER_FIX_2_562915447); /* sqrt(2) * (-c1-c3) */
                z3 = z3 * (-SLOW_INTEGER_FIX_1_961570560); /* sqrt(2) * (-c3-c5) */
                z4 = z4 * (-SLOW_INTEGER_FIX_0_390180644); /* sqrt(2) * (c5-c3) */

                z3 += z5;
                z4 += z5;

                data[dataIndex + JpegConstants.DCTSize * 7] = JpegUtils.DESCALE(tmp4 + z1 + z3, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + JpegConstants.DCTSize * 5] = JpegUtils.DESCALE(tmp5 + z2 + z4, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + JpegConstants.DCTSize * 3] = JpegUtils.DESCALE(tmp6 + z2 + z3, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS);
                data[dataIndex + JpegConstants.DCTSize * 1] = JpegUtils.DESCALE(tmp7 + z1 + z4, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS);

                dataIndex++;          /* advance pointer to next column */
            }
        }

        /// <summary>
        /// Multiply a DCTELEM variable by an int constant, and immediately
        /// descale to yield a DCTELEM result.
        /// </summary>
        private static int FAST_INTEGER_MULTIPLY(int var, int c)
        {
            return (JpegUtils.DESCALE((var) * (c), FAST_INTEGER_CONST_BITS));
        }
    }
    #endregion

    #region JpegHuffmanTable
    /// <summary>
    /// Huffman coding table.
    /// </summary>
    public class JpegHuffmanTable
    {
        /* These two fields directly represent the contents of a JPEG DHT marker */
        private readonly byte[] m_bits = new byte[17];     /* bits[k] = # of symbols with codes of */

        /* length k bits; bits[0] is unused */
        private readonly byte[] m_huffval = new byte[256];     /* The symbols, in order of incr code length */

        private bool m_sent_table;        /* true when table has been output */


        internal JpegHuffmanTable()
        {
        }

        internal byte[] Bits
        {
            get { return m_bits; }
        }

        internal byte[] Huffval
        {
            get { return m_huffval; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the table has been output to file.
        /// </summary>
        /// <value>It's initialized <c>false</c> when the table is created, and set 
        /// <c>true</c> when it's been output to the file. You could suppress output 
        /// of a table by setting this to <c>true</c>.
        /// </value>
        /// <remarks>This property is used only during compression. It's initialized
        /// <c>false</c> when the table is created, and set <c>true</c> when it's been
        /// output to the file. You could suppress output of a table by setting this to
        /// <c>true</c>. (See jpeg_suppress_tables for an example.)</remarks>
        /// <seealso cref="JpegCompressor.jpeg_suppress_tables"/>
        public bool Sent_table
        {
            get { return m_sent_table; }
            set { m_sent_table = value; }
        }
    }
    #endregion

    #region JpegInputController
    /// <summary>
    /// Input control module
    /// </summary>
    class JpegInputController
    {
        private JpegDecompressor m_cinfo;
        private bool m_consumeData;
        private bool m_inheaders;     /* true until first SOS is reached */
        private bool m_has_multiple_scans;    /* True if file has multiple scans */
        private bool m_eoi_reached;       /* True when EOI has been consumed */

        /// <summary>
        /// Initialize the input controller module.
        /// This is called only once, when the decompression object is created.
        /// </summary>
        public JpegInputController(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Initialize state: can't use reset_input_controller since we don't
            * want to try to reset other modules yet.
            */
            m_inheaders = true;
        }

        public ReadResult consume_input()
        {
            if (m_consumeData)
                return m_cinfo.m_coef.consume_data();

            return consume_markers();
        }

        /// <summary>
        /// Reset state to begin a fresh datastream.
        /// </summary>
        public void reset_input_controller()
        {
            m_consumeData = false;
            m_has_multiple_scans = false; /* "unknown" would be better */
            m_eoi_reached = false;
            m_inheaders = true;

            /* Reset other modules */
            m_cinfo.m_marker.reset_marker_reader();

            /* Reset progression state -- would be cleaner if entropy decoder did this */
            m_cinfo.m_coef_bits = null;
        }

        /// <summary>
        /// Initialize the input modules to read a scan of compressed data.
        /// The first call to this is done after initializing
        /// the entire decompressor (during jpeg_start_decompress).
        /// Subsequent calls come from consume_markers, below.
        /// </summary>
        public void start_input_pass()
        {
            per_scan_setup();
            latch_quant_tables();
            m_cinfo.m_entropy.start_pass();
            m_cinfo.m_coef.start_input_pass();
            m_consumeData = true;
        }

        /// <summary>
        /// Finish up after inputting a compressed-data scan.
        /// This is called by the coefficient controller after it's read all
        /// the expected data of the scan.
        /// </summary>
        public void finish_input_pass()
        {
            m_consumeData = false;
        }

        public bool HasMultipleScans()
        {
            return m_has_multiple_scans;
        }

        public bool EOIReached()
        {
            return m_eoi_reached;
        }

        /// <summary>
        /// Read JPEG markers before, between, or after compressed-data scans.
        /// Change state as necessary when a new scan is reached.
        /// Return value is JPEG_SUSPENDED, JPEG_REACHED_SOS, or JPEG_REACHED_EOI.
        /// 
        /// The consume_input method pointer points either here or to the
        /// coefficient controller's consume_data routine, depending on whether
        /// we are reading a compressed data segment or inter-segment markers.
        /// </summary>
        private ReadResult consume_markers()
        {
            ReadResult val;

            if (m_eoi_reached) /* After hitting EOI, read no further */
                return ReadResult.Reached_EOI;

            val = m_cinfo.m_marker.read_markers();

            switch (val)
            {
                case ReadResult.Reached_SOS:
                    /* Found SOS */
                    if (m_inheaders)
                    {
                        /* 1st SOS */
                        initial_setup();
                        m_inheaders = false;
                        /* Note: start_input_pass must be called by JpegDecompressorMaster
                         * before any more input can be consumed.
                         */
                    }
                    else
                    {
                        /* 2nd or later SOS marker */
                        if (!m_has_multiple_scans)
                        {
                            /* Oops, I wasn't expecting this! */
                            throw new Exception("Didn't expect more than one scan");
                        }

                        m_cinfo.m_inputctl.start_input_pass();
                    }
                    break;
                case ReadResult.Reached_EOI:
                    /* Found EOI */
                    m_eoi_reached = true;
                    if (m_inheaders)
                    {
                        /* Tables-only data-stream, apparently */
                        if (m_cinfo.m_marker.SawSOF())
                            throw new Exception("Invalid JPEG file structure: missing SOS marker");
                    }
                    else
                    {
                        /* Prevent infinite loop in coef ctlr's decompress_data routine
                         * if user set output_scan_number larger than number of scans.
                         */
                        if (m_cinfo.m_output_scan_number > m_cinfo.m_input_scan_number)
                            m_cinfo.m_output_scan_number = m_cinfo.m_input_scan_number;
                    }
                    break;
                case ReadResult.Suspended:
                    break;
            }

            return val;
        }

        /// <summary>
        /// Routines to calculate various quantities related to the size of the image.
        /// Called once, when first SOS marker is reached
        /// </summary>
        private void initial_setup()
        {
            /* Make sure image isn't bigger than I can handle */
            if (m_cinfo.m_image_height > JpegConstants.JpegMaxDimention ||
                m_cinfo.m_image_width > JpegConstants.JpegMaxDimention)
            {
                throw new Exception(String.Format("Maximum supported image dimension is {0} pixels", (int)JpegConstants.JpegMaxDimention));

            }

            /* For now, precision must match compiled-in value... */
            if (m_cinfo.m_data_precision != JpegConstants.BitsInSample)
                throw new Exception(String.Format("Unsupported JPEG data precision {0}", m_cinfo.m_data_precision));

            /* Check that number of components won't exceed internal array sizes */
            if (m_cinfo.m_num_components > JpegConstants.MaxComponents)
                throw new Exception(String.Format("Too many color components: {0}, max {1}", m_cinfo.m_num_components, JpegConstants.MaxComponents));

            /* Compute maximum sampling factors; check factor validity */
            m_cinfo.m_max_h_samp_factor = 1;
            m_cinfo.m_max_v_samp_factor = 1;

            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                if (m_cinfo.Comp_info[ci].H_samp_factor <= 0 || m_cinfo.Comp_info[ci].H_samp_factor > JpegConstants.MaxSamplingFactor ||
                    m_cinfo.Comp_info[ci].V_samp_factor <= 0 || m_cinfo.Comp_info[ci].V_samp_factor > JpegConstants.MaxSamplingFactor)
                {
                    throw new Exception("Bogus sampling factors");
                }

                m_cinfo.m_max_h_samp_factor = Math.Max(m_cinfo.m_max_h_samp_factor, m_cinfo.Comp_info[ci].H_samp_factor);
                m_cinfo.m_max_v_samp_factor = Math.Max(m_cinfo.m_max_v_samp_factor, m_cinfo.Comp_info[ci].V_samp_factor);
            }

            /* We initialize DCT_scaled_size and min_DCT_scaled_size to DCTSize.
             * In the full decompressor, this will be overridden JpegDecompressorMaster;
             * but in the transcoder, JpegDecompressorMaster is not used, so we must do it here.
             */
            m_cinfo.m_min_DCT_scaled_size = JpegConstants.DCTSize;

            /* Compute dimensions of components */
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                m_cinfo.Comp_info[ci].DCT_scaled_size = JpegConstants.DCTSize;

                /* Size in DCT blocks */
                m_cinfo.Comp_info[ci].Width_in_blocks = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width * m_cinfo.Comp_info[ci].H_samp_factor,
                    m_cinfo.m_max_h_samp_factor * JpegConstants.DCTSize);

                m_cinfo.Comp_info[ci].height_in_blocks = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_height * m_cinfo.Comp_info[ci].V_samp_factor,
                    m_cinfo.m_max_v_samp_factor * JpegConstants.DCTSize);

                /* downsampled_width and downsampled_height will also be overridden by
                 * JpegDecompressorMaster if we are doing full decompression.  The transcoder library
                 * doesn't use these values, but the calling application might.
                 */
                /* Size in samples */
                m_cinfo.Comp_info[ci].downsampled_width = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width * m_cinfo.Comp_info[ci].H_samp_factor,
                    m_cinfo.m_max_h_samp_factor);

                m_cinfo.Comp_info[ci].downsampled_height = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_height * m_cinfo.Comp_info[ci].V_samp_factor,
                    m_cinfo.m_max_v_samp_factor);

                /* Mark component needed, until color conversion says otherwise */
                m_cinfo.Comp_info[ci].component_needed = true;

                /* Mark no quantization table yet saved for component */
                m_cinfo.Comp_info[ci].quant_table = null;
            }

            /* Compute number of fully interleaved MCU rows. */
            m_cinfo.m_total_iMCU_rows = JpegUtils.jdiv_round_up(
                m_cinfo.m_image_height, m_cinfo.m_max_v_samp_factor * JpegConstants.DCTSize);

            /* Decide whether file contains multiple scans */
            if (m_cinfo.m_comps_in_scan < m_cinfo.m_num_components || m_cinfo.m_progressive_mode)
                m_cinfo.m_inputctl.m_has_multiple_scans = true;
            else
                m_cinfo.m_inputctl.m_has_multiple_scans = false;
        }

        /// <summary>
        /// Save away a copy of the Q-table referenced by each component present
        /// in the current scan, unless already saved during a prior scan.
        /// 
        /// In a multiple-scan JPEG file, the encoder could assign different components
        /// the same Q-table slot number, but change table definitions between scans
        /// so that each component uses a different Q-table.  (The IJG encoder is not
        /// currently capable of doing this, but other encoders might.)  Since we want
        /// to be able to de-quantize all the components at the end of the file, this
        /// means that we have to save away the table actually used for each component.
        /// We do this by copying the table at the start of the first scan containing
        /// the component.
        /// The JPEG spec prohibits the encoder from changing the contents of a Q-table
        /// slot between scans of a component using that slot.  If the encoder does so
        /// anyway, this decoder will simply use the Q-table values that were current
        /// at the start of the first scan for the component.
        /// 
        /// The decompressor output side looks only at the saved quant tables,
        /// not at the current Q-table slots.
        /// </summary>
        private void latch_quant_tables()
        {
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];

                /* No work if we already saved Q-table for this component */
                if (componentInfo.quant_table != null)
                    continue;

                /* Make sure specified quantization table is present */
                int qtblno = componentInfo.Quant_tbl_no;
                if (qtblno < 0 || qtblno >= JpegConstants.NumberOfQuantTables || m_cinfo.m_quant_tbl_ptrs[qtblno] == null)
                    throw new Exception(String.Format("Quantization table 0x{0:X2} was not defined", qtblno));

                /* OK, save away the quantization table */
                JpegQuantizationTable qtbl = new JpegQuantizationTable();
                Buffer.BlockCopy(m_cinfo.m_quant_tbl_ptrs[qtblno].quantval, 0,
                    qtbl.quantval, 0, qtbl.quantval.Length * sizeof(short));
                qtbl.Sent_table = m_cinfo.m_quant_tbl_ptrs[qtblno].Sent_table;
                componentInfo.quant_table = qtbl;
                m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]] = componentInfo;
            }
        }

        /// <summary>
        /// Do computations that are needed before processing a JPEG scan
        /// cinfo.comps_in_scan and cinfo.cur_comp_info[] were set from SOS marker
        /// </summary>
        private void per_scan_setup()
        {
            if (m_cinfo.m_comps_in_scan == 1)
            {
                /* Non-interleaved (single-component) scan */
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[0]];

                /* Overall image size in MCUs */
                m_cinfo.m_MCUs_per_row = componentInfo.Width_in_blocks;
                m_cinfo.m_MCU_rows_in_scan = componentInfo.height_in_blocks;

                /* For non-interleaved scan, always one block per MCU */
                componentInfo.MCU_width = 1;
                componentInfo.MCU_height = 1;
                componentInfo.MCU_blocks = 1;
                componentInfo.MCU_sample_width = componentInfo.DCT_scaled_size;
                componentInfo.last_col_width = 1;

                /* For non-interleaved scans, it is convenient to define last_row_height
                 * as the number of block rows present in the last iMCU row.
                 */
                int tmp = componentInfo.height_in_blocks % componentInfo.V_samp_factor;
                if (tmp == 0)
                    tmp = componentInfo.V_samp_factor;
                componentInfo.last_row_height = tmp;
                m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[0]] = componentInfo;

                /* Prepare array describing MCU composition */
                m_cinfo.m_blocks_in_MCU = 1;
                m_cinfo.m_MCU_membership[0] = 0;
            }
            else
            {
                /* Interleaved (multi-component) scan */
                if (m_cinfo.m_comps_in_scan <= 0 || m_cinfo.m_comps_in_scan > JpegConstants.MaxComponentsInScan)
                    throw new Exception(String.Format("Too many color components: {0}, max {1}", m_cinfo.m_comps_in_scan, JpegConstants.MaxComponentsInScan));

                /* Overall image size in MCUs */
                m_cinfo.m_MCUs_per_row = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_width, m_cinfo.m_max_h_samp_factor * JpegConstants.DCTSize);

                m_cinfo.m_MCU_rows_in_scan = JpegUtils.jdiv_round_up(
                    m_cinfo.m_image_height, m_cinfo.m_max_v_samp_factor * JpegConstants.DCTSize);

                m_cinfo.m_blocks_in_MCU = 0;

                for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                {
                    JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];

                    /* Sampling factors give # of blocks of component in each MCU */
                    componentInfo.MCU_width = componentInfo.H_samp_factor;
                    componentInfo.MCU_height = componentInfo.V_samp_factor;
                    componentInfo.MCU_blocks = componentInfo.MCU_width * componentInfo.MCU_height;
                    componentInfo.MCU_sample_width = componentInfo.MCU_width * componentInfo.DCT_scaled_size;

                    /* Figure number of non-dummy blocks in last MCU column & row */
                    int tmp = componentInfo.Width_in_blocks % componentInfo.MCU_width;
                    if (tmp == 0)
                        tmp = componentInfo.MCU_width;
                    componentInfo.last_col_width = tmp;

                    tmp = componentInfo.height_in_blocks % componentInfo.MCU_height;
                    if (tmp == 0)
                        tmp = componentInfo.MCU_height;
                    componentInfo.last_row_height = tmp;

                    /* Prepare array describing MCU composition */
                    int mcublks = componentInfo.MCU_blocks;
                    if (m_cinfo.m_blocks_in_MCU + mcublks > JpegConstants.DecompressorMaxBlocksInMCU)
                        throw new Exception("Sampling factors too large for interleaved scan");

                    m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]] = componentInfo;

                    while (mcublks-- > 0)
                        m_cinfo.m_MCU_membership[m_cinfo.m_blocks_in_MCU++] = ci;
                }
            }
        }
    }
    #endregion

    #region JpegInverseDCT
    /// <summary>
    /// An inverse DCT routine is given a pointer to the input JpegBlock and a pointer
    /// to an output sample array.  The routine must dequantize the input data as
    /// well as perform the IDCT; for dequantization, it uses the multiplier table
    /// pointed to by componentInfo.dct_table.  The output data is to be placed into the
    /// sample array starting at a specified column. (Any row offset needed will
    /// be applied to the array pointer before it is passed to the IDCT code)
    /// Note that the number of samples emitted by the IDCT routine is
    /// DCT_scaled_size * DCT_scaled_size.
    /// 
    /// Each IDCT routine has its own ideas about the best dct_table element type.
    /// 
    /// The decompressor input side saves away the appropriate
    /// quantization table for each component at the start of the first scan
    /// involving that component.  (This is necessary in order to correctly
    /// decode files that reuse Q-table slots.)
    /// When we are ready to make an output pass, the saved Q-table is converted
    /// to a multiplier table that will actually be used by the IDCT routine.
    /// The multiplier table contents are IDCT-method-dependent.  To support
    /// application changes in IDCT method between scans, we can remake the
    /// multiplier tables if necessary.
    /// In buffered-image mode, the first output pass may occur before any data
    /// has been seen for some components, and thus before their Q-tables have
    /// been saved away.  To handle this case, multiplier tables are preset
    /// to zeroes; the result of the IDCT will be a neutral gray level.
    /// </summary>
    class JpegInverseDCT
    {
        private const int IFAST_SCALE_BITS = 2; /* fractional bits in scale factors */

        /*
        * Each IDCT routine is responsible for range-limiting its results and
        * converting them to unsigned form (0..MaxSampleValue).  The raw outputs could
        * be quite far out of range if the input data is corrupt, so a bulletproof
        * range-limiting step is required.  We use a mask-and-table-lookup method
        * to do the combined operations quickly.  See the comments with
        * prepare_range_limit_table (in jdmaster.c) for more info.
        */
        private const int RANGE_MASK = (JpegConstants.MaxSampleValue * 4 + 3); /* 2 bits wider than legal samples */

        private const int SLOW_INTEGER_CONST_BITS = 13;
        private const int SLOW_INTEGER_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change SLOW_INTEGER_CONST_BITS you may want to add appropriate values.
        * 
        * Convert a positive real constant to an integer scaled by CONST_SCALE.
        * static int SLOW_INTEGER_FIX(double x)
        * {
        *  return ((int) ((x) * (((int) 1) << SLOW_INTEGER_CONST_BITS) + 0.5));
        * }
        */

        private const int SLOW_INTEGER_FIX_0_298631336 = 2446;   /* SLOW_INTEGER_FIX(0.298631336) */
        private const int SLOW_INTEGER_FIX_0_390180644 = 3196;   /* SLOW_INTEGER_FIX(0.390180644) */
        private const int SLOW_INTEGER_FIX_0_541196100 = 4433;   /* SLOW_INTEGER_FIX(0.541196100) */
        private const int SLOW_INTEGER_FIX_0_765366865 = 6270;   /* SLOW_INTEGER_FIX(0.765366865) */
        private const int SLOW_INTEGER_FIX_0_899976223 = 7373;   /* SLOW_INTEGER_FIX(0.899976223) */
        private const int SLOW_INTEGER_FIX_1_175875602 = 9633;   /* SLOW_INTEGER_FIX(1.175875602) */
        private const int SLOW_INTEGER_FIX_1_501321110 = 12299;  /* SLOW_INTEGER_FIX(1.501321110) */
        private const int SLOW_INTEGER_FIX_1_847759065 = 15137;  /* SLOW_INTEGER_FIX(1.847759065) */
        private const int SLOW_INTEGER_FIX_1_961570560 = 16069;  /* SLOW_INTEGER_FIX(1.961570560) */
        private const int SLOW_INTEGER_FIX_2_053119869 = 16819;  /* SLOW_INTEGER_FIX(2.053119869) */
        private const int SLOW_INTEGER_FIX_2_562915447 = 20995;  /* SLOW_INTEGER_FIX(2.562915447) */
        private const int SLOW_INTEGER_FIX_3_072711026 = 25172;  /* SLOW_INTEGER_FIX(3.072711026) */

        private const int FAST_INTEGER_CONST_BITS = 8;
        private const int FAST_INTEGER_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change FAST_INTEGER_CONST_BITS you may want to add appropriate values.
        */
        private const int FAST_INTEGER_FIX_1_082392200 = 277;        /* FAST_INTEGER_FIX(1.082392200) */
        private const int FAST_INTEGER_FIX_1_414213562 = 362;        /* FAST_INTEGER_FIX(1.414213562) */
        private const int FAST_INTEGER_FIX_1_847759065 = 473;        /* FAST_INTEGER_FIX(1.847759065) */
        private const int FAST_INTEGER_FIX_2_613125930 = 669;        /* FAST_INTEGER_FIX(2.613125930) */

        private const int REDUCED_CONST_BITS = 13;
        private const int REDUCED_PASS1_BITS = 2;

        /* We use the following pre-calculated constants.
        * If you change REDUCED_CONST_BITS you may want to add appropriate values.
        * Convert a positive real constant to an integer scaled by CONST_SCALE.
        * static int REDUCED_FIX(double x)
        * {
        *   return ((int) ((x) * (((int) 1) << REDUCED_CONST_BITS) + 0.5));
        * }
        */

        private const int REDUCED_FIX_0_211164243 = 1730;    /* REDUCED_FIX(0.211164243) */
        private const int REDUCED_FIX_0_509795579 = 4176;    /* REDUCED_FIX(0.509795579) */
        private const int REDUCED_FIX_0_601344887 = 4926;    /* REDUCED_FIX(0.601344887) */
        private const int REDUCED_FIX_0_720959822 = 5906;    /* REDUCED_FIX(0.720959822) */
        private const int REDUCED_FIX_0_765366865 = 6270;    /* REDUCED_FIX(0.765366865) */
        private const int REDUCED_FIX_0_850430095 = 6967;    /* REDUCED_FIX(0.850430095) */
        private const int REDUCED_FIX_0_899976223 = 7373;    /* REDUCED_FIX(0.899976223) */
        private const int REDUCED_FIX_1_061594337 = 8697;    /* REDUCED_FIX(1.061594337) */
        private const int REDUCED_FIX_1_272758580 = 10426;   /* REDUCED_FIX(1.272758580) */
        private const int REDUCED_FIX_1_451774981 = 11893;   /* REDUCED_FIX(1.451774981) */
        private const int REDUCED_FIX_1_847759065 = 15137;   /* REDUCED_FIX(1.847759065) */
        private const int REDUCED_FIX_2_172734803 = 17799;   /* REDUCED_FIX(2.172734803) */
        private const int REDUCED_FIX_2_562915447 = 20995;   /* REDUCED_FIX(2.562915447) */
        private const int REDUCED_FIX_3_624509785 = 29692;   /* REDUCED_FIX(3.624509785) */

        /* precomputed values scaled up by 14 bits */
        private static short[] aanscales = 
        {
            16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 22725, 31521, 29692, 26722, 22725, 17855,
            12299, 6270, 21407, 29692, 27969, 25172, 21407, 16819, 11585,
            5906, 19266, 26722, 25172, 22654, 19266, 15137, 10426, 5315,
            16384, 22725, 21407, 19266, 16384, 12873, 8867, 4520, 12873,
            17855, 16819, 15137, 12873, 10114, 6967, 3552, 8867, 12299,
            11585, 10426, 8867, 6967, 4799, 2446, 4520, 6270, 5906, 5315,
            4520, 3552, 2446, 1247 
        };

        private const int CONST_BITS = 14;

        private static double[] aanscalefactor = 
        { 
            1.0, 1.387039845, 1.306562965, 1.175875602, 1.0,
            0.785694958, 0.541196100, 0.275899379 
        };

        private enum InverseMethod
        {
            Unknown,
            idct_1x1_method,
            idct_2x2_method,
            idct_4x4_method,
            idct_islow_method,
            idct_ifast_method,
            idct_float_method
        }

        /* It is useful to allow each component to have a separate IDCT method. */
        private InverseMethod[] m_inverse_DCT_method = new InverseMethod[JpegConstants.MaxComponents];

        /* Allocated multiplier tables: big enough for any supported variant */
        private class multiplier_table
        {
            public int[] int_array = new int[JpegConstants.DCTSize2];
            public float[] float_array = new float[JpegConstants.DCTSize2];
        };

        private multiplier_table[] m_dctTables;

        private JpegDecompressor m_cinfo;

        /* This array contains the IDCT method code that each multiplier table
        * is currently set up for, or -1 if it's not yet set up.
        * The actual multiplier tables are pointed to by dct_table in the
        * per-component comp_info structures.
        */
        private int[] m_cur_method = new int[JpegConstants.MaxComponents];

        private ComponentBuffer m_componentBuffer;

        public JpegInverseDCT(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            m_dctTables = new multiplier_table[cinfo.m_num_components];
            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                /* Allocate and pre-zero a multiplier table for each component */
                m_dctTables[ci] = new multiplier_table();

                /* Mark multiplier table not yet set up for any method */
                m_cur_method[ci] = -1;
            }
        }

        /// <summary>
        /// Prepare for an output pass.
        /// Here we select the proper IDCT routine for each component and build
        /// a matching multiplier table.
        /// </summary>
        public void start_pass()
        {
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[ci];

                InverseMethod im = InverseMethod.Unknown;
                int method = 0;
                /* Select the proper IDCT routine for this component's scaling */
                switch (componentInfo.DCT_scaled_size)
                {
                    case 1:
                        im = InverseMethod.idct_1x1_method;
                        method = (int)DCTMethod.IntSlow;    /* jidctred uses islow-style table */
                        break;
                    case 2:
                        im = InverseMethod.idct_2x2_method;
                        method = (int)DCTMethod.IntSlow;    /* jidctred uses islow-style table */
                        break;
                    case 4:
                        im = InverseMethod.idct_4x4_method;
                        method = (int)DCTMethod.IntSlow;    /* jidctred uses islow-style table */
                        break;
                    case JpegConstants.DCTSize:
                        switch (m_cinfo.m_dct_method)
                        {
                            case DCTMethod.IntSlow:
                                im = InverseMethod.idct_islow_method;
                                method = (int)DCTMethod.IntSlow;
                                break;
                            case DCTMethod.IntFast:
                                im = InverseMethod.idct_ifast_method;
                                method = (int)DCTMethod.IntFast;
                                break;
                            case DCTMethod.Float:
                                im = InverseMethod.idct_float_method;
                                method = (int)DCTMethod.Float;
                                break;
                            default:
                                throw new Exception("Unknown DCT Method!");
                        }
                        break;
                    default:
                        throw new Exception(String.Format("IDCT output block size {0} not supported", componentInfo.DCT_scaled_size));
                }

                m_inverse_DCT_method[ci] = im;

                /* Create multiplier table from quant table.
                 * However, we can skip this if the component is uninteresting
                 * or if we already built the table.  Also, if no quant table
                 * has yet been saved for the component, we leave the
                 * multiplier table all-zero; we'll be reading zeroes from the
                 * coefficient controller's buffer anyway.
                 */
                if (!componentInfo.component_needed || m_cur_method[ci] == method)
                    continue;

                if (componentInfo.quant_table == null)
                {
                    /* happens if no data yet for component */
                    continue;
                }

                m_cur_method[ci] = method;
                switch ((DCTMethod)method)
                {
                    case DCTMethod.IntSlow:
                        /* For LL&M IDCT method, multipliers are equal to raw quantization
                         * coefficients, but are stored as ints to ensure access efficiency.
                         */
                        int[] ismtbl = m_dctTables[ci].int_array;
                        for (int i = 0; i < JpegConstants.DCTSize2; i++)
                            ismtbl[i] = componentInfo.quant_table.quantval[i];
                        break;

                    case DCTMethod.IntFast:
                        /* For AA&N IDCT method, multipliers are equal to quantization
                         * coefficients scaled by scalefactor[row]*scalefactor[col], where
                         *   scalefactor[0] = 1
                         *   scalefactor[k] = cos(k*PI/16) * sqrt(2)    for k=1..7
                         * For integer operation, the multiplier table is to be scaled by
                         * IFAST_SCALE_BITS.
                         */
                        int[] ifmtbl = m_dctTables[ci].int_array;

                        for (int i = 0; i < JpegConstants.DCTSize2; i++)
                        {
                            ifmtbl[i] = JpegUtils.DESCALE((int)componentInfo.quant_table.quantval[i] * (int)aanscales[i], CONST_BITS - IFAST_SCALE_BITS);
                        }
                        break;

                    case DCTMethod.Float:
                        /* For float AA&N IDCT method, multipliers are equal to quantization
                         * coefficients scaled by scalefactor[row]*scalefactor[col], where
                         *   scalefactor[0] = 1
                         *   scalefactor[k] = cos(k*PI/16) * sqrt(2)    for k=1..7
                         */
                        float[] fmtbl = m_dctTables[ci].float_array;
                        int ii = 0;
                        for (int row = 0; row < JpegConstants.DCTSize; row++)
                        {
                            for (int col = 0; col < JpegConstants.DCTSize; col++)
                            {
                                fmtbl[ii] = (float)((double)componentInfo.quant_table.quantval[ii] * aanscalefactor[row] * aanscalefactor[col]);
                                ii++;
                            }
                        }
                        break;

                    default:
                        throw new Exception("Unknown DCT Method!");
                }
            }
        }

        /* Inverse DCT (also performs de-quantization) */
        public void inverse(int component_index, short[] coef_block, ComponentBuffer output_buf, int output_row, int output_col)
        {
            m_componentBuffer = output_buf;
            switch (m_inverse_DCT_method[component_index])
            {
                case InverseMethod.idct_1x1_method:
                    jpeg_idct_1x1(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_2x2_method:
                    jpeg_idct_2x2(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_4x4_method:
                    jpeg_idct_4x4(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_islow_method:
                    jpeg_idct_islow(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_ifast_method:
                    jpeg_idct_ifast(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.idct_float_method:
                    jpeg_idct_float(component_index, coef_block, output_row, output_col);
                    break;
                case InverseMethod.Unknown:
                default:
                    throw new Exception("Unknown Inverse Method!");
            }
        }

        /// <summary>
        /// Perform de-quantization and inverse DCT on one block of coefficients.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// A slow-but-accurate integer implementation of the
        /// inverse DCT (Discrete Cosine Transform).  In the IJG code, this routine
        /// must also perform de-quantization of the input coefficients.
        /// 
        /// A 2-D IDCT can be done by 1-D IDCT on each column followed by 1-D IDCT
        /// on each row (or vice versa, but it's more convenient to emit a row at
        /// a time).  Direct algorithms are also available, but they are much more
        /// complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on an algorithm described in
        /// C. Loeffler, A. Ligtenberg and G. Moschytz, "Practical Fast 1-D DCT
        /// Algorithms with 11 Multiplications", Proc. Int'l. Conf. on Acoustics,
        /// Speech, and Signal Processing 1989 (ICASSP '89), pp. 988-991.
        /// The primary algorithm described there uses 11 multiplies and 29 adds.
        /// We use their alternate method with 12 multiplies and 32 adds.
        /// The advantage of this method is that no data path contains more than one
        /// multiplication; this allows a very simple and accurate implementation in
        /// scaled fixed-point arithmetic, with a minimal number of shifts.
        /// 
        /// The poop on this scaling stuff is as follows:
        /// 
        /// Each 1-D IDCT step produces outputs which are a factor of sqrt(N)
        /// larger than the true IDCT outputs.  The final outputs are therefore
        /// a factor of N larger than desired; since N=8 this can be cured by
        /// a simple right shift at the end of the algorithm.  The advantage of
        /// this arrangement is that we save two multiplications per 1-D IDCT,
        /// because the y0 and y4 inputs need not be divided by sqrt(N).
        /// 
        /// We have to do addition and subtraction of the integer inputs, which
        /// is no problem, and multiplication by fractional constants, which is
        /// a problem to do in integer arithmetic.  We multiply all the constants
        /// by CONST_SCALE and convert them to integer constants (thus retaining
        /// SLOW_INTEGER_CONST_BITS bits of precision in the constants).  After doing a
        /// multiplication we have to divide the product by CONST_SCALE, with proper
        /// rounding, to produce the correct output.  This division can be done
        /// cheaply as a right shift of SLOW_INTEGER_CONST_BITS bits.  We postpone shifting
        /// as long as possible so that partial sums can be added together with
        /// full fractional precision.
        /// 
        /// The outputs of the first pass are scaled up by SLOW_INTEGER_PASS1_BITS bits so that
        /// they are represented to better-than-integral precision.  These outputs
        /// require BitsInSample + SLOW_INTEGER_PASS1_BITS + 3 bits; this fits in a 16-bit word
        /// with the recommended scaling.  (To scale up 12-bit sample data further, an
        /// intermediate int array would be needed.)
        /// 
        /// To avoid overflow of the 32-bit intermediate results in pass 2, we must
        /// have BitsInSample + SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS &lt;= 26.  Error analysis
        /// shows that the values given below are the most effective.
        /// </summary>
        private void jpeg_idct_islow(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSize2];

            /* Pass 1: process columns from input, store into work array. */
            /* Note results are scaled up by sqrt(8) compared to a true IDCT; */
            /* furthermore, we scale the results by 2**SLOW_INTEGER_PASS1_BITS. */

            int coefBlockIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            int workspaceIndex = 0;

            for (int ctr = JpegConstants.DCTSize; ctr > 0; ctr--)
            {
                /* Due to quantization, we will usually find that many of the input
                * coefficients are zero, especially the AC terms.  We can exploit this
                * by short-circuiting the IDCT calculation for any column in which all
                * the AC terms are zero.  In that case each output is equal to the
                * DC coefficient (with scale factor as needed).
                * With typical images and quantization tables, half or more of the
                * column DCT calculations can be simplified this way.
                */

                if (coef_block[coefBlockIndex + JpegConstants.DCTSize * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 4] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 7] == 0)
                {
                    /* AC terms all zero */
                    int dcval = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                        quantTable[quantTableIndex + JpegConstants.DCTSize * 0]) << SLOW_INTEGER_PASS1_BITS;

                    workspace[workspaceIndex + JpegConstants.DCTSize * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 3] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 4] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 5] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 6] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 7] = dcval;

                    /* advance pointers to next column */
                    coefBlockIndex++;
                    quantTableIndex++;
                    workspaceIndex++;
                    continue;
                }

                /* Even part: reverse the even part of the forward DCT. */
                /* The rotator is sqrt(2)*c(-6). */

                int z2 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 2],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 2]);
                int z3 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 6],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 6]);

                int z1 = (z2 + z3) * SLOW_INTEGER_FIX_0_541196100;
                int tmp2 = z1 + z3 * (-SLOW_INTEGER_FIX_1_847759065);
                int tmp3 = z1 + z2 * SLOW_INTEGER_FIX_0_765366865;

                z2 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);
                z3 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 4],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 4]);

                int tmp0 = (z2 + z3) << SLOW_INTEGER_CONST_BITS;
                int tmp1 = (z2 - z3) << SLOW_INTEGER_CONST_BITS;

                int tmp10 = tmp0 + tmp3;
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                /* Odd part per figure 8; the matrix is unitary and hence its
                * transpose is its inverse.  i0..i3 are y7,y5,y3,y1 respectively.
                */

                tmp0 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 7],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 7]);
                tmp1 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 5],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 5]);
                tmp2 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 3],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 3]);
                tmp3 = SLOW_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 1],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 1]);

                z1 = tmp0 + tmp3;
                z2 = tmp1 + tmp2;
                z3 = tmp0 + tmp2;
                int z4 = tmp1 + tmp3;
                int z5 = (z3 + z4) * SLOW_INTEGER_FIX_1_175875602; /* sqrt(2) * c3 */

                tmp0 = tmp0 * SLOW_INTEGER_FIX_0_298631336; /* sqrt(2) * (-c1+c3+c5-c7) */
                tmp1 = tmp1 * SLOW_INTEGER_FIX_2_053119869; /* sqrt(2) * ( c1+c3-c5+c7) */
                tmp2 = tmp2 * SLOW_INTEGER_FIX_3_072711026; /* sqrt(2) * ( c1+c3+c5-c7) */
                tmp3 = tmp3 * SLOW_INTEGER_FIX_1_501321110; /* sqrt(2) * ( c1+c3-c5-c7) */
                z1 = z1 * (-SLOW_INTEGER_FIX_0_899976223); /* sqrt(2) * (c7-c3) */
                z2 = z2 * (-SLOW_INTEGER_FIX_2_562915447); /* sqrt(2) * (-c1-c3) */
                z3 = z3 * (-SLOW_INTEGER_FIX_1_961570560); /* sqrt(2) * (-c3-c5) */
                z4 = z4 * (-SLOW_INTEGER_FIX_0_390180644); /* sqrt(2) * (c5-c3) */

                z3 += z5;
                z4 += z5;

                tmp0 += z1 + z3;
                tmp1 += z2 + z4;
                tmp2 += z2 + z3;
                tmp3 += z1 + z4;

                /* Final output stage: inputs are tmp10..tmp13, tmp0..tmp3 */

                workspace[workspaceIndex + JpegConstants.DCTSize * 0] = JpegUtils.DESCALE(tmp10 + tmp3, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 7] = JpegUtils.DESCALE(tmp10 - tmp3, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 1] = JpegUtils.DESCALE(tmp11 + tmp2, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 6] = JpegUtils.DESCALE(tmp11 - tmp2, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 2] = JpegUtils.DESCALE(tmp12 + tmp1, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 5] = JpegUtils.DESCALE(tmp12 - tmp1, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 3] = JpegUtils.DESCALE(tmp13 + tmp0, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);
                workspace[workspaceIndex + JpegConstants.DCTSize * 4] = JpegUtils.DESCALE(tmp13 - tmp0, SLOW_INTEGER_CONST_BITS - SLOW_INTEGER_PASS1_BITS);

                /* advance pointers to next column */
                coefBlockIndex++;
                quantTableIndex++;
                workspaceIndex++;
            }

            /* Pass 2: process rows from work array, store into output array. */
            /* Note that we must descale the results by a factor of 8 == 2**3, */
            /* and also undo the SLOW_INTEGER_PASS1_BITS scaling. */

            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.MediumSampleValue;

            for (int ctr = 0; ctr < JpegConstants.DCTSize; ctr++)
            {
                /* Rows of zeroes can be exploited in the same way as we did with columns.
                * However, the column calculation has created many nonzero AC terms, so
                * the simplification applies less often (typically 5% to 10% of the time).
                * On machines with very fast multiplication, it's possible that the
                * test takes more time than it's worth.  In that case this section
                * may be commented out.
                */
                int currentOutRow = output_row + ctr;
                if (workspace[workspaceIndex + 1] == 0 &&
                    workspace[workspaceIndex + 2] == 0 &&
                    workspace[workspaceIndex + 3] == 0 &&
                    workspace[workspaceIndex + 4] == 0 &&
                    workspace[workspaceIndex + 5] == 0 &&
                    workspace[workspaceIndex + 6] == 0 &&
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + JpegUtils.DESCALE(workspace[workspaceIndex + 0], SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 2] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 3] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 4] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 5] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 6] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 7] = dcval;

                    workspaceIndex += JpegConstants.DCTSize;       /* advance pointer to next row */
                    continue;
                }

                /* Even part: reverse the even part of the forward DCT. */
                /* The rotator is sqrt(2)*c(-6). */

                int z2 = workspace[workspaceIndex + 2];
                int z3 = workspace[workspaceIndex + 6];

                int z1 = (z2 + z3) * SLOW_INTEGER_FIX_0_541196100;
                int tmp2 = z1 + z3 * (-SLOW_INTEGER_FIX_1_847759065);
                int tmp3 = z1 + z2 * SLOW_INTEGER_FIX_0_765366865;

                int tmp0 = (workspace[workspaceIndex + 0] + workspace[workspaceIndex + 4]) << SLOW_INTEGER_CONST_BITS;
                int tmp1 = (workspace[workspaceIndex + 0] - workspace[workspaceIndex + 4]) << SLOW_INTEGER_CONST_BITS;

                int tmp10 = tmp0 + tmp3;
                int tmp13 = tmp0 - tmp3;
                int tmp11 = tmp1 + tmp2;
                int tmp12 = tmp1 - tmp2;

                /* Odd part per figure 8; the matrix is unitary and hence its
                * transpose is its inverse.  i0..i3 are y7,y5,y3,y1 respectively.
                */

                tmp0 = workspace[workspaceIndex + 7];
                tmp1 = workspace[workspaceIndex + 5];
                tmp2 = workspace[workspaceIndex + 3];
                tmp3 = workspace[workspaceIndex + 1];

                z1 = tmp0 + tmp3;
                z2 = tmp1 + tmp2;
                z3 = tmp0 + tmp2;
                int z4 = tmp1 + tmp3;
                int z5 = (z3 + z4) * SLOW_INTEGER_FIX_1_175875602; /* sqrt(2) * c3 */

                tmp0 = tmp0 * SLOW_INTEGER_FIX_0_298631336; /* sqrt(2) * (-c1+c3+c5-c7) */
                tmp1 = tmp1 * SLOW_INTEGER_FIX_2_053119869; /* sqrt(2) * ( c1+c3-c5+c7) */
                tmp2 = tmp2 * SLOW_INTEGER_FIX_3_072711026; /* sqrt(2) * ( c1+c3+c5-c7) */
                tmp3 = tmp3 * SLOW_INTEGER_FIX_1_501321110; /* sqrt(2) * ( c1+c3-c5-c7) */
                z1 = z1 * (-SLOW_INTEGER_FIX_0_899976223); /* sqrt(2) * (c7-c3) */
                z2 = z2 * (-SLOW_INTEGER_FIX_2_562915447); /* sqrt(2) * (-c1-c3) */
                z3 = z3 * (-SLOW_INTEGER_FIX_1_961570560); /* sqrt(2) * (-c3-c5) */
                z4 = z4 * (-SLOW_INTEGER_FIX_0_390180644); /* sqrt(2) * (c5-c3) */

                z3 += z5;
                z4 += z5;

                tmp0 += z1 + z3;
                tmp1 += z2 + z4;
                tmp2 += z2 + z3;
                tmp3 += z1 + z4;

                /* Final output stage: inputs are tmp10..tmp13, tmp0..tmp3 */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE(tmp10 + tmp3, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 7] = limit[limitOffset + JpegUtils.DESCALE(tmp10 - tmp3, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE(tmp11 + tmp2, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 6] = limit[limitOffset + JpegUtils.DESCALE(tmp11 - tmp2, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + JpegUtils.DESCALE(tmp12 + tmp1, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 5] = limit[limitOffset + JpegUtils.DESCALE(tmp12 - tmp1, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + JpegUtils.DESCALE(tmp13 + tmp0, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 4] = limit[limitOffset + JpegUtils.DESCALE(tmp13 - tmp0, SLOW_INTEGER_CONST_BITS + SLOW_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                /* advance pointer to next row */
                workspaceIndex += JpegConstants.DCTSize;
            }
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce an int result.  In this module, both inputs and result
        /// are 16 bits or less, so either int or short multiply will work.
        /// </summary>
        private static int SLOW_INTEGER_DEQUANTIZE(int coef, int quantval)
        {
            return (coef * quantval);
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// A fast, not so accurate integer implementation of the
        /// inverse DCT (Discrete Cosine Transform).  In the IJG code, this routine
        /// must also perform dequantization of the input coefficients.
        /// 
        /// A 2-D IDCT can be done by 1-D IDCT on each column followed by 1-D IDCT
        /// on each row (or vice versa, but it's more convenient to emit a row at
        /// a time).  Direct algorithms are also available, but they are much more
        /// complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on Arai, Agui, and Nakajima's algorithm for
        /// scaled DCT.  Their original paper (Trans. IEICE E-71(11):1095) is in
        /// Japanese, but the algorithm is described in the Pennebaker &amp; Mitchell
        /// JPEG textbook (see REFERENCES section in file README).  The following code
        /// is based directly on figure 4-8 in P&amp;M.
        /// While an 8-point DCT cannot be done in less than 11 multiplies, it is
        /// possible to arrange the computation so that many of the multiplies are
        /// simple scalings of the final outputs.  These multiplies can then be
        /// folded into the multiplications or divisions by the JPEG quantization
        /// table entries.  The AA&amp;N method leaves only 5 multiplies and 29 adds
        /// to be done in the DCT itself.
        /// The primary disadvantage of this method is that with fixed-point math,
        /// accuracy is lost due to imprecise representation of the scaled
        /// quantization values.  The smaller the quantization table entry, the less
        /// precise the scaled value, so this implementation does worse with high-
        /// quality-setting files than with low-quality ones.
        /// 
        /// Scaling decisions are generally the same as in the LL&amp;M algorithm;
        /// However, we choose to descale
        /// (right shift) multiplication products as soon as they are formed,
        /// rather than carrying additional fractional bits into subsequent additions.
        /// This compromises accuracy slightly, but it lets us save a few shifts.
        /// More importantly, 16-bit arithmetic is then adequate (for 8-bit samples)
        /// everywhere except in the multiplications proper; this saves a good deal
        /// of work on 16-bit-int machines.
        /// 
        /// The dequantized coefficients are not integers because the AA&amp;N scaling
        /// factors have been incorporated.  We represent them scaled up by FAST_INTEGER_PASS1_BITS,
        /// so that the first and second IDCT rounds have the same input scaling.
        /// For 8-bit JSAMPLEs, we choose IFAST_SCALE_BITS = FAST_INTEGER_PASS1_BITS so as to
        /// avoid a descaling shift; this compromises accuracy rather drastically
        /// for small quantization table entries, but it saves a lot of shifts.
        /// For 12-bit JSAMPLEs, there's no hope of using 16x16 multiplies anyway,
        /// so we use a much larger scaling factor to preserve accuracy.
        /// 
        /// A final compromise is to represent the multiplicative constants to only
        /// 8 fractional bits, rather than 13.  This saves some shifting work on some
        /// machines, and may also reduce the cost of multiplication (since there
        /// are fewer one-bits in the constants).
        /// </summary>
        private void jpeg_idct_ifast(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSize2];

            /* Pass 1: process columns from input, store into work array. */

            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSize; ctr > 0; ctr--)
            {
                /* Due to quantization, we will usually find that many of the input
                * coefficients are zero, especially the AC terms.  We can exploit this
                * by short-circuiting the IDCT calculation for any column in which all
                * the AC terms are zero.  In that case each output is equal to the
                * DC coefficient (with scale factor as needed).
                * With typical images and quantization tables, half or more of the
                * column DCT calculations can be simplified this way.
                */

                if (coef_block[coefBlockIndex + JpegConstants.DCTSize * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 4] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 7] == 0)
                {
                    /* AC terms all zero */
                    int dcval = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                        quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);

                    workspace[workspaceIndex + JpegConstants.DCTSize * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 3] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 4] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 5] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 6] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 7] = dcval;

                    /* advance pointers to next column */
                    coefBlockIndex++;
                    quantTableIndex++;
                    workspaceIndex++;
                    continue;
                }

                /* Even part */

                int tmp0 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);
                int tmp1 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 2],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 2]);
                int tmp2 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 4],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 4]);
                int tmp3 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 6],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 6]);

                int tmp10 = tmp0 + tmp2;    /* phase 3 */
                int tmp11 = tmp0 - tmp2;

                int tmp13 = tmp1 + tmp3;    /* phases 5-3 */
                int tmp12 = FAST_INTEGER_MULTIPLY(tmp1 - tmp3, FAST_INTEGER_FIX_1_414213562) - tmp13; /* 2*c4 */

                tmp0 = tmp10 + tmp13;   /* phase 2 */
                tmp3 = tmp10 - tmp13;
                tmp1 = tmp11 + tmp12;
                tmp2 = tmp11 - tmp12;

                /* Odd part */

                int tmp4 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 1],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 1]);
                int tmp5 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 3],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 3]);
                int tmp6 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 5],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 5]);
                int tmp7 = FAST_INTEGER_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 7],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 7]);

                int z13 = tmp6 + tmp5;      /* phase 6 */
                int z10 = tmp6 - tmp5;
                int z11 = tmp4 + tmp7;
                int z12 = tmp4 - tmp7;

                tmp7 = z11 + z13;       /* phase 5 */
                tmp11 = FAST_INTEGER_MULTIPLY(z11 - z13, FAST_INTEGER_FIX_1_414213562); /* 2*c4 */

                int z5 = FAST_INTEGER_MULTIPLY(z10 + z12, FAST_INTEGER_FIX_1_847759065); /* 2*c2 */
                tmp10 = FAST_INTEGER_MULTIPLY(z12, FAST_INTEGER_FIX_1_082392200) - z5; /* 2*(c2-c6) */
                tmp12 = FAST_INTEGER_MULTIPLY(z10, -FAST_INTEGER_FIX_2_613125930) + z5; /* -2*(c2+c6) */

                tmp6 = tmp12 - tmp7;    /* phase 2 */
                tmp5 = tmp11 - tmp6;
                tmp4 = tmp10 + tmp5;

                workspace[workspaceIndex + JpegConstants.DCTSize * 0] = tmp0 + tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSize * 7] = tmp0 - tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSize * 1] = tmp1 + tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSize * 6] = tmp1 - tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSize * 2] = tmp2 + tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSize * 5] = tmp2 - tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSize * 4] = tmp3 + tmp4;
                workspace[workspaceIndex + JpegConstants.DCTSize * 3] = tmp3 - tmp4;

                /* advance pointers to next column */
                coefBlockIndex++;
                quantTableIndex++;
                workspaceIndex++;
            }

            /* Pass 2: process rows from work array, store into output array. */
            /* Note that we must descale the results by a factor of 8 == 2**3, */
            /* and also undo the FAST_INTEGER_PASS1_BITS scaling. */

            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.MediumSampleValue;

            for (int ctr = 0; ctr < JpegConstants.DCTSize; ctr++)
            {
                int currentOutRow = output_row + ctr;

                /* Rows of zeroes can be exploited in the same way as we did with columns.
                * However, the column calculation has created many nonzero AC terms, so
                * the simplification applies less often (typically 5% to 10% of the time).
                * On machines with very fast multiplication, it's possible that the
                * test takes more time than it's worth.  In that case this section
                * may be commented out.
                */

                if (workspace[workspaceIndex + 1] == 0 &&
                    workspace[workspaceIndex + 2] == 0 &&
                    workspace[workspaceIndex + 3] == 0 &&
                    workspace[workspaceIndex + 4] == 0 &&
                    workspace[workspaceIndex + 5] == 0 &&
                    workspace[workspaceIndex + 6] == 0 &&
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + FAST_INTEGER_IDESCALE(workspace[workspaceIndex + 0], FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 2] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 3] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 4] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 5] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 6] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 7] = dcval;

                    /* advance pointer to next row */
                    workspaceIndex += JpegConstants.DCTSize;
                    continue;
                }

                /* Even part */

                int tmp10 = workspace[workspaceIndex + 0] + workspace[workspaceIndex + 4];
                int tmp11 = workspace[workspaceIndex + 0] - workspace[workspaceIndex + 4];

                int tmp13 = workspace[workspaceIndex + 2] + workspace[workspaceIndex + 6];
                int tmp12 = FAST_INTEGER_MULTIPLY(workspace[workspaceIndex + 2] - workspace[workspaceIndex + 6], FAST_INTEGER_FIX_1_414213562) - tmp13;

                int tmp0 = tmp10 + tmp13;
                int tmp3 = tmp10 - tmp13;
                int tmp1 = tmp11 + tmp12;
                int tmp2 = tmp11 - tmp12;

                /* Odd part */

                int z13 = workspace[workspaceIndex + 5] + workspace[workspaceIndex + 3];
                int z10 = workspace[workspaceIndex + 5] - workspace[workspaceIndex + 3];
                int z11 = workspace[workspaceIndex + 1] + workspace[workspaceIndex + 7];
                int z12 = workspace[workspaceIndex + 1] - workspace[workspaceIndex + 7];

                int tmp7 = z11 + z13;       /* phase 5 */
                tmp11 = FAST_INTEGER_MULTIPLY(z11 - z13, FAST_INTEGER_FIX_1_414213562); /* 2*c4 */

                int z5 = FAST_INTEGER_MULTIPLY(z10 + z12, FAST_INTEGER_FIX_1_847759065); /* 2*c2 */
                tmp10 = FAST_INTEGER_MULTIPLY(z12, FAST_INTEGER_FIX_1_082392200) - z5; /* 2*(c2-c6) */
                tmp12 = FAST_INTEGER_MULTIPLY(z10, -FAST_INTEGER_FIX_2_613125930) + z5; /* -2*(c2+c6) */

                int tmp6 = tmp12 - tmp7;    /* phase 2 */
                int tmp5 = tmp11 - tmp6;
                int tmp4 = tmp10 + tmp5;

                /* Final output stage: scale down by a factor of 8 and range-limit */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp0 + tmp7, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 7] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp0 - tmp7, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp1 + tmp6, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 6] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp1 - tmp6, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp2 + tmp5, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 5] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp2 - tmp5, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 4] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp3 + tmp4, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + FAST_INTEGER_IDESCALE(tmp3 - tmp4, FAST_INTEGER_PASS1_BITS + 3) & RANGE_MASK];

                /* advance pointer to next row */
                workspaceIndex += JpegConstants.DCTSize;
            }
        }

        /// <summary>
        /// Multiply a DCTELEM variable by an int constant, and immediately
        /// descale to yield a DCTELEM result.
        /// </summary>
        private static int FAST_INTEGER_MULTIPLY(int var, int c)
        {
            return (JpegUtils.DESCALE(var * c, FAST_INTEGER_CONST_BITS));
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce a DCTELEM result.  For 8-bit data a 16x16->16
        /// multiplication will do.  For 12-bit data, the multiplier table is
        /// declared int, so a 32-bit multiply will be used.
        /// </summary>
        private static int FAST_INTEGER_DEQUANTIZE(short coef, int quantval)
        {
            return ((int)coef * quantval);
        }

        /// <summary>
        /// Like DESCALE, but applies to a DCTELEM and produces an int.
        /// We assume that int right shift is unsigned if int right shift is.
        /// </summary>
        private static int FAST_INTEGER_IRIGHT_SHIFT(int x, int shft)
        {
            return (x >> shft);
        }

        private static int FAST_INTEGER_IDESCALE(int x, int n)
        {
            return (FAST_INTEGER_IRIGHT_SHIFT((x) + (1 << ((n) - 1)), n));
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients.
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// A floating-point implementation of the
        /// inverse DCT (Discrete Cosine Transform).  In the IJG code, this routine
        /// must also perform dequantization of the input coefficients.
        /// 
        /// This implementation should be more accurate than either of the integer
        /// IDCT implementations.  However, it may not give the same results on all
        /// machines because of differences in roundoff behavior.  Speed will depend
        /// on the hardware's floating point capacity.
        /// 
        /// A 2-D IDCT can be done by 1-D IDCT on each column followed by 1-D IDCT
        /// on each row (or vice versa, but it's more convenient to emit a row at
        /// a time).  Direct algorithms are also available, but they are much more
        /// complex and seem not to be any faster when reduced to code.
        /// 
        /// This implementation is based on Arai, Agui, and Nakajima's algorithm for
        /// scaled DCT.  Their original paper (Trans. IEICE E-71(11):1095) is in
        /// Japanese, but the algorithm is described in the Pennebaker &amp; Mitchell
        /// JPEG textbook (see REFERENCES section in file README).  The following code
        /// is based directly on figure 4-8 in P&amp;M.
        /// While an 8-point DCT cannot be done in less than 11 multiplies, it is
        /// possible to arrange the computation so that many of the multiplies are
        /// simple scalings of the final outputs.  These multiplies can then be
        /// folded into the multiplications or divisions by the JPEG quantization
        /// table entries.  The AA&amp;N method leaves only 5 multiplies and 29 adds
        /// to be done in the DCT itself.
        /// The primary disadvantage of this method is that with a fixed-point
        /// implementation, accuracy is lost due to imprecise representation of the
        /// scaled quantization values.  However, that problem does not arise if
        /// we use floating point arithmetic.
        /// </summary>
        private void jpeg_idct_float(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            float[] workspace = new float[JpegConstants.DCTSize2];

            /* Pass 1: process columns from input, store into work array. */

            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            float[] quantTable = m_dctTables[component_index].float_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSize; ctr > 0; ctr--)
            {
                /* Due to quantization, we will usually find that many of the input
                * coefficients are zero, especially the AC terms.  We can exploit this
                * by short-circuiting the IDCT calculation for any column in which all
                * the AC terms are zero.  In that case each output is equal to the
                * DC coefficient (with scale factor as needed).
                * With typical images and quantization tables, half or more of the
                * column DCT calculations can be simplified this way.
                */

                if (coef_block[coefBlockIndex + JpegConstants.DCTSize * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 4] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 7] == 0)
                {
                    /* AC terms all zero */
                    float dcval = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                        quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);

                    workspace[workspaceIndex + JpegConstants.DCTSize * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 3] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 4] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 5] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 6] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 7] = dcval;

                    coefBlockIndex++;            /* advance pointers to next column */
                    quantTableIndex++;
                    workspaceIndex++;
                    continue;
                }

                /* Even part */

                float tmp0 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);
                float tmp1 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 2],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 2]);
                float tmp2 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 4],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 4]);
                float tmp3 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 6],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 6]);

                float tmp10 = tmp0 + tmp2;    /* phase 3 */
                float tmp11 = tmp0 - tmp2;

                float tmp13 = tmp1 + tmp3;    /* phases 5-3 */
                float tmp12 = (tmp1 - tmp3) * 1.414213562f - tmp13; /* 2*c4 */

                tmp0 = tmp10 + tmp13;   /* phase 2 */
                tmp3 = tmp10 - tmp13;
                tmp1 = tmp11 + tmp12;
                tmp2 = tmp11 - tmp12;

                /* Odd part */

                float tmp4 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 1],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 1]);
                float tmp5 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 3],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 3]);
                float tmp6 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 5],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 5]);
                float tmp7 = FLOAT_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 7],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 7]);

                float z13 = tmp6 + tmp5;      /* phase 6 */
                float z10 = tmp6 - tmp5;
                float z11 = tmp4 + tmp7;
                float z12 = tmp4 - tmp7;

                tmp7 = z11 + z13;       /* phase 5 */
                tmp11 = (z11 - z13) * 1.414213562f; /* 2*c4 */

                float z5 = (z10 + z12) * 1.847759065f; /* 2*c2 */
                tmp10 = 1.082392200f * z12 - z5; /* 2*(c2-c6) */
                tmp12 = -2.613125930f * z10 + z5; /* -2*(c2+c6) */

                tmp6 = tmp12 - tmp7;    /* phase 2 */
                tmp5 = tmp11 - tmp6;
                tmp4 = tmp10 + tmp5;

                workspace[workspaceIndex + JpegConstants.DCTSize * 0] = tmp0 + tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSize * 7] = tmp0 - tmp7;
                workspace[workspaceIndex + JpegConstants.DCTSize * 1] = tmp1 + tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSize * 6] = tmp1 - tmp6;
                workspace[workspaceIndex + JpegConstants.DCTSize * 2] = tmp2 + tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSize * 5] = tmp2 - tmp5;
                workspace[workspaceIndex + JpegConstants.DCTSize * 4] = tmp3 + tmp4;
                workspace[workspaceIndex + JpegConstants.DCTSize * 3] = tmp3 - tmp4;

                coefBlockIndex++;            /* advance pointers to next column */
                quantTableIndex++;
                workspaceIndex++;
            }

            /* Pass 2: process rows from work array, store into output array. */
            /* Note that we must descale the results by a factor of 8 == 2**3. */
            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.MediumSampleValue;

            for (int ctr = 0; ctr < JpegConstants.DCTSize; ctr++)
            {
                /* Rows of zeroes can be exploited in the same way as we did with columns.
                * However, the column calculation has created many nonzero AC terms, so
                * the simplification applies less often (typically 5% to 10% of the time).
                * And testing floats for zero is relatively expensive, so we don't bother.
                */

                /* Even part */

                float tmp10 = workspace[workspaceIndex + 0] + workspace[workspaceIndex + 4];
                float tmp11 = workspace[workspaceIndex + 0] - workspace[workspaceIndex + 4];

                float tmp13 = workspace[workspaceIndex + 2] + workspace[workspaceIndex + 6];
                float tmp12 = (workspace[workspaceIndex + 2] - workspace[workspaceIndex + 6]) * 1.414213562f - tmp13;

                float tmp0 = tmp10 + tmp13;
                float tmp3 = tmp10 - tmp13;
                float tmp1 = tmp11 + tmp12;
                float tmp2 = tmp11 - tmp12;

                /* Odd part */

                float z13 = workspace[workspaceIndex + 5] + workspace[workspaceIndex + 3];
                float z10 = workspace[workspaceIndex + 5] - workspace[workspaceIndex + 3];
                float z11 = workspace[workspaceIndex + 1] + workspace[workspaceIndex + 7];
                float z12 = workspace[workspaceIndex + 1] - workspace[workspaceIndex + 7];

                float tmp7 = z11 + z13;
                tmp11 = (z11 - z13) * 1.414213562f;

                float z5 = (z10 + z12) * 1.847759065f; /* 2*c2 */
                tmp10 = 1.082392200f * z12 - z5; /* 2*(c2-c6) */
                tmp12 = -2.613125930f * z10 + z5; /* -2*(c2+c6) */

                float tmp6 = tmp12 - tmp7;
                float tmp5 = tmp11 - tmp6;
                float tmp4 = tmp10 + tmp5;

                /* Final output stage: scale down by a factor of 8 and range-limit */
                int currentOutRow = output_row + ctr;
                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp0 + tmp7), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 7] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp0 - tmp7), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp1 + tmp6), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 6] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp1 - tmp6), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp2 + tmp5), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 5] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp2 - tmp5), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 4] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp3 + tmp4), 3) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + JpegUtils.DESCALE((int)(tmp3 - tmp4), 3) & RANGE_MASK];

                workspaceIndex += JpegConstants.DCTSize;       /* advance pointer to next row */
            }
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce a float result.
        /// </summary>
        private static float FLOAT_DEQUANTIZE(short coef, float quantval)
        {
            return (((float)(coef)) * (quantval));
        }

        /// <summary>
        /// Inverse-DCT routines that produce reduced-size output:
        /// either 4x4, 2x2, or 1x1 pixels from an 8x8 DCT block.
        /// 
        /// NOTE: this code only copes with 8x8 DCTs.
        /// 
        /// The implementation is based on the Loeffler, Ligtenberg and Moschytz (LL&amp;M)
        /// algorithm. We simply replace each 8-to-8 1-D IDCT step
        /// with an 8-to-4 step that produces the four averages of two adjacent outputs
        /// (or an 8-to-2 step producing two averages of four outputs, for 2x2 output).
        /// These steps were derived by computing the corresponding values at the end
        /// of the normal LL&amp;M code, then simplifying as much as possible.
        /// 
        /// 1x1 is trivial: just take the DC coefficient divided by 8.
        /// 
        /// Perform dequantization and inverse DCT on one block of coefficients,
        /// producing a reduced-size 4x4 output block.
        /// </summary>
        private void jpeg_idct_4x4(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSize * 4];

            /* Pass 1: process columns from input, store into work array. */
            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSize; ctr > 0; coefBlockIndex++, quantTableIndex++, workspaceIndex++, ctr--)
            {
                /* Don't bother to process column 4, because second pass won't use it */
                if (ctr == JpegConstants.DCTSize - 4)
                    continue;

                if (coef_block[coefBlockIndex + JpegConstants.DCTSize * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 2] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 6] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 7] == 0)
                {
                    /* AC terms all zero; we need not examine term 4 for 4x4 output */
                    int dcval = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                        quantTable[quantTableIndex + JpegConstants.DCTSize * 0]) << REDUCED_PASS1_BITS;

                    workspace[workspaceIndex + JpegConstants.DCTSize * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 1] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 2] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 3] = dcval;

                    continue;
                }

                /* Even part */

                int tmp0 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);
                tmp0 <<= (REDUCED_CONST_BITS + 1);

                int z2 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 2],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 2]);
                int z3 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 6],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 6]);

                int tmp2 = z2 * REDUCED_FIX_1_847759065 + z3 * (-REDUCED_FIX_0_765366865);

                int tmp10 = tmp0 + tmp2;
                int tmp12 = tmp0 - tmp2;

                /* Odd part */

                int z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 7],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 7]);
                z2 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 5],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 5]);
                z3 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 3],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 3]);
                int z4 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 1],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 1]);

                tmp0 = z1 * (-REDUCED_FIX_0_211164243) /* sqrt(2) * (c3-c1) */ +
                       z2 * REDUCED_FIX_1_451774981 /* sqrt(2) * (c3+c7) */ +
                       z3 * (-REDUCED_FIX_2_172734803) /* sqrt(2) * (-c1-c5) */ +
                       z4 * REDUCED_FIX_1_061594337; /* sqrt(2) * (c5+c7) */

                tmp2 = z1 * (-REDUCED_FIX_0_509795579) /* sqrt(2) * (c7-c5) */ +
                       z2 * (-REDUCED_FIX_0_601344887) /* sqrt(2) * (c5-c1) */ +
                       z3 * REDUCED_FIX_0_899976223 /* sqrt(2) * (c3-c7) */ +
                       z4 * REDUCED_FIX_2_562915447; /* sqrt(2) * (c1+c3) */

                /* Final output stage */

                workspace[workspaceIndex + JpegConstants.DCTSize * 0] = JpegUtils.DESCALE(tmp10 + tmp2, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
                workspace[workspaceIndex + JpegConstants.DCTSize * 3] = JpegUtils.DESCALE(tmp10 - tmp2, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
                workspace[workspaceIndex + JpegConstants.DCTSize * 1] = JpegUtils.DESCALE(tmp12 + tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
                workspace[workspaceIndex + JpegConstants.DCTSize * 2] = JpegUtils.DESCALE(tmp12 - tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 1);
            }

            /* Pass 2: process 4 rows from work array, store into output array. */
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.MediumSampleValue;

            workspaceIndex = 0;
            for (int ctr = 0; ctr < 4; ctr++)
            {
                int currentOutRow = output_row + ctr;
                /* It's not clear whether a zero row test is worthwhile here ... */

                if (workspace[workspaceIndex + 1] == 0 &&
                    workspace[workspaceIndex + 2] == 0 &&
                    workspace[workspaceIndex + 3] == 0 &&
                    workspace[workspaceIndex + 5] == 0 &&
                    workspace[workspaceIndex + 6] == 0 &&
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + JpegUtils.DESCALE(workspace[workspaceIndex + 0], REDUCED_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 2] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 3] = dcval;

                    workspaceIndex += JpegConstants.DCTSize;       /* advance pointer to next row */
                    continue;
                }

                /* Even part */

                int tmp0 = (workspace[workspaceIndex + 0]) << (REDUCED_CONST_BITS + 1);

                int tmp2 = workspace[workspaceIndex + 2] * REDUCED_FIX_1_847759065 + workspace[workspaceIndex + 6] * (-REDUCED_FIX_0_765366865);

                int tmp10 = tmp0 + tmp2;
                int tmp12 = tmp0 - tmp2;

                /* Odd part */

                int z1 = workspace[workspaceIndex + 7];
                int z2 = workspace[workspaceIndex + 5];
                int z3 = workspace[workspaceIndex + 3];
                int z4 = workspace[workspaceIndex + 1];

                tmp0 = z1 * (-REDUCED_FIX_0_211164243) /* sqrt(2) * (c3-c1) */ +
                       z2 * REDUCED_FIX_1_451774981 /* sqrt(2) * (c3+c7) */ +
                       z3 * (-REDUCED_FIX_2_172734803) /* sqrt(2) * (-c1-c5) */ +
                       z4 * REDUCED_FIX_1_061594337; /* sqrt(2) * (c5+c7) */

                tmp2 = z1 * (-REDUCED_FIX_0_509795579) /* sqrt(2) * (c7-c5) */ +
                       z2 * (-REDUCED_FIX_0_601344887) /* sqrt(2) * (c5-c1) */ +
                       z3 * REDUCED_FIX_0_899976223 /* sqrt(2) * (c3-c7) */ +
                       z4 * REDUCED_FIX_2_562915447; /* sqrt(2) * (c1+c3) */

                /* Final output stage */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE(tmp10 + tmp2, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 3] = limit[limitOffset + JpegUtils.DESCALE(tmp10 - tmp2, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE(tmp12 + tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 2] = limit[limitOffset + JpegUtils.DESCALE(tmp12 - tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 1) & RANGE_MASK];

                workspaceIndex += JpegConstants.DCTSize;       /* advance pointer to next row */
            }
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients,
        /// producing a reduced-size 2x2 output block.
        /// </summary>
        private void jpeg_idct_2x2(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* buffers data between passes */
            int[] workspace = new int[JpegConstants.DCTSize * 2];

            /* Pass 1: process columns from input, store into work array. */
            int coefBlockIndex = 0;
            int workspaceIndex = 0;

            int[] quantTable = m_dctTables[component_index].int_array;
            int quantTableIndex = 0;

            for (int ctr = JpegConstants.DCTSize; ctr > 0; coefBlockIndex++, quantTableIndex++, workspaceIndex++, ctr--)
            {
                /* Don't bother to process columns 2,4,6 */
                if (ctr == JpegConstants.DCTSize - 2 || ctr == JpegConstants.DCTSize - 4 || ctr == JpegConstants.DCTSize - 6)
                    continue;

                if (coef_block[coefBlockIndex + JpegConstants.DCTSize * 1] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 3] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 5] == 0 &&
                    coef_block[coefBlockIndex + JpegConstants.DCTSize * 7] == 0)
                {
                    /* AC terms all zero; we need not examine terms 2,4,6 for 2x2 output */
                    int dcval = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                        quantTable[quantTableIndex + JpegConstants.DCTSize * 0]) << REDUCED_PASS1_BITS;

                    workspace[workspaceIndex + JpegConstants.DCTSize * 0] = dcval;
                    workspace[workspaceIndex + JpegConstants.DCTSize * 1] = dcval;

                    continue;
                }

                /* Even part */

                int z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 0],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 0]);
                int tmp10 = z1 << (REDUCED_CONST_BITS + 2);

                /* Odd part */

                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 7],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 7]);
                int tmp0 = z1 * -REDUCED_FIX_0_720959822; /* sqrt(2) * (c7-c5+c3-c1) */
                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 5],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 5]);
                tmp0 += z1 * REDUCED_FIX_0_850430095; /* sqrt(2) * (-c1+c3+c5+c7) */
                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 3],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 3]);
                tmp0 += z1 * (-REDUCED_FIX_1_272758580); /* sqrt(2) * (-c1+c3-c5-c7) */
                z1 = REDUCED_DEQUANTIZE(coef_block[coefBlockIndex + JpegConstants.DCTSize * 1],
                    quantTable[quantTableIndex + JpegConstants.DCTSize * 1]);
                tmp0 += z1 * REDUCED_FIX_3_624509785; /* sqrt(2) * (c1+c3+c5+c7) */

                /* Final output stage */

                workspace[workspaceIndex + JpegConstants.DCTSize * 0] = JpegUtils.DESCALE(tmp10 + tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 2);
                workspace[workspaceIndex + JpegConstants.DCTSize * 1] = JpegUtils.DESCALE(tmp10 - tmp0, REDUCED_CONST_BITS - REDUCED_PASS1_BITS + 2);
            }

            /* Pass 2: process 2 rows from work array, store into output array. */
            workspaceIndex = 0;
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.MediumSampleValue;

            for (int ctr = 0; ctr < 2; ctr++)
            {
                int currentOutRow = output_row + ctr;
                /* It's not clear whether a zero row test is worthwhile here ... */

                if (workspace[workspaceIndex + 1] == 0 &&
                    workspace[workspaceIndex + 3] == 0 &&
                    workspace[workspaceIndex + 5] == 0 &&
                    workspace[workspaceIndex + 7] == 0)
                {
                    /* AC terms all zero */
                    byte dcval = limit[limitOffset + JpegUtils.DESCALE(workspace[workspaceIndex + 0], REDUCED_PASS1_BITS + 3) & RANGE_MASK];

                    m_componentBuffer[currentOutRow][output_col + 0] = dcval;
                    m_componentBuffer[currentOutRow][output_col + 1] = dcval;

                    workspaceIndex += JpegConstants.DCTSize;       /* advance pointer to next row */
                    continue;
                }

                /* Even part */

                int tmp10 = (workspace[workspaceIndex + 0]) << (REDUCED_CONST_BITS + 2);

                /* Odd part */

                int tmp0 = workspace[workspaceIndex + 7] * (-REDUCED_FIX_0_720959822) /* sqrt(2) * (c7-c5+c3-c1) */ +
                       workspace[workspaceIndex + 5] * REDUCED_FIX_0_850430095 /* sqrt(2) * (-c1+c3+c5+c7) */ +
                       workspace[workspaceIndex + 3] * (-REDUCED_FIX_1_272758580) /* sqrt(2) * (-c1+c3-c5-c7) */ +
                       workspace[workspaceIndex + 1] * REDUCED_FIX_3_624509785; /* sqrt(2) * (c1+c3+c5+c7) */

                /* Final output stage */

                m_componentBuffer[currentOutRow][output_col + 0] = limit[limitOffset + JpegUtils.DESCALE(tmp10 + tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 2) & RANGE_MASK];
                m_componentBuffer[currentOutRow][output_col + 1] = limit[limitOffset + JpegUtils.DESCALE(tmp10 - tmp0, REDUCED_CONST_BITS + REDUCED_PASS1_BITS + 3 + 2) & RANGE_MASK];

                workspaceIndex += JpegConstants.DCTSize;       /* advance pointer to next row */
            }
        }

        /// <summary>
        /// Perform dequantization and inverse DCT on one block of coefficients,
        /// producing a reduced-size 1x1 output block.
        /// </summary>
        private void jpeg_idct_1x1(int component_index, short[] coef_block, int output_row, int output_col)
        {
            /* We hardly need an inverse DCT routine for this: just take the
            * average pixel value, which is one-eighth of the DC coefficient.
            */
            int[] quantptr = m_dctTables[component_index].int_array;
            int dcval = REDUCED_DEQUANTIZE(coef_block[0], quantptr[0]);
            dcval = JpegUtils.DESCALE(dcval, 3);

            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset + JpegConstants.MediumSampleValue;

            m_componentBuffer[output_row + 0][output_col] = limit[limitOffset + dcval & RANGE_MASK];
        }

        /// <summary>
        /// Dequantize a coefficient by multiplying it by the multiplier-table
        /// entry; produce an int result.  In this module, both inputs and result
        /// are 16 bits or less, so either int or short multiply will work.
        /// </summary>
        private static int REDUCED_DEQUANTIZE(short coef, int quantval)
        {
            return ((int)coef * quantval);
        }
    }
    #endregion

    #region JpegMarker
    /// <summary>
    /// Representation of special JPEG marker.
    /// </summary>
    /// <remarks>You can't create instance of this class manually.
    /// Concrete objects are instantiated by library and you can get them
    /// through <see cref="JpegDecompressor.Marker_list">Marker_list</see> property.
    /// </remarks>
    /// <seealso cref="JpegDecompressor.Marker_list"/>
    /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
    public class JpegMarker
    {
        /// <summary>
        /// marker code: JPEG_COM, or JPEG_APP0+n
        /// </summary>
        private byte m_marker;
        /// <summary>
        /// # bytes of data in the file
        /// </summary>
        private int m_originalLength;
        /// <summary>
        /// the data contained in the marker
        /// </summary>
        private byte[] m_data;

        internal JpegMarker(byte marker, int originalDataLength, int lengthLimit)
        {
            m_marker = marker;
            m_originalLength = originalDataLength;
            m_data = new byte[lengthLimit];
        }

        /// <summary>
        /// Gets the special marker.
        /// </summary>
        /// <value>The marker value.</value>
        public byte Marker
        {
            get
            {
                return m_marker;
            }
        }

        /// <summary>
        /// Gets the full length of original data associated with the marker.
        /// </summary>
        /// <value>The length of original data associated with the marker.</value>
        /// <remarks>This length excludes the marker length word, whereas the stored representation 
        /// within the JPEG file includes it. (Hence the maximum data length is really only 65533.)
        /// </remarks>
        public int OriginalLength
        {
            get
            {
                return m_originalLength;
            }
        }

        /// <summary>
        /// Gets the data associated with the marker.
        /// </summary>
        /// <value>The data associated with the marker.</value>
        /// <remarks>The length of this array doesn't exceed <c>length_limit</c> for the particular marker type.
        /// Note that this length excludes the marker length word, whereas the stored representation 
        /// within the JPEG file includes it. (Hence the maximum data length is really only 65533.)
        /// </remarks>
        public byte[] Data
        {
            get
            {
                return m_data;
            }
        }
    }
    #endregion

    #region JpegMarkerReader
    /// <summary>
    /// Marker reading and parsing
    /// </summary>
    class JpegMarkerReader
    {
        private const int APP0_DATA_LEN = 14;  /* Length of interesting data in APP0 */
        private const int APP14_DATA_LEN = 12;  /* Length of interesting data in APP14 */
        private const int APPN_DATA_LEN = 14;  /* Must be the largest of the above!! */

        private JpegDecompressor m_cinfo;

        /* Application-overridable marker processing methods */
        private JpegDecompressor.jpeg_marker_parser_method m_process_COM;
        private JpegDecompressor.jpeg_marker_parser_method[] m_process_APPn = new JpegDecompressor.jpeg_marker_parser_method[16];

        /* Limit on marker data length to save for each marker type */
        private int m_length_limit_COM;
        private int[] m_length_limit_APPn = new int[16];

        private bool m_saw_SOI;       /* found SOI? */
        private bool m_saw_SOF;       /* found SOF? */
        private int m_next_restart_num;       /* next restart number expected (0-7) */
        private int m_discarded_bytes;   /* # of bytes skipped looking for a marker */

        /* Status of COM/APPn marker saving */
        private JpegMarker m_cur_marker; /* null if not processing a marker */
        private int m_bytes_read;        /* data bytes read so far in marker */
        /* Note: cur_marker is not linked into marker_list until it's all read. */

        /// <summary>
        /// Initialize the marker reader module.
        /// This is called only once, when the decompression object is created.
        /// </summary>
        public JpegMarkerReader(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Initialize COM/APPn processing.
            * By default, we examine and then discard APP0 and APP14,
            * but simply discard COM and all other APPn.
            */
            m_process_COM = skip_variable;

            for (int i = 0; i < 16; i++)
            {
                m_process_APPn[i] = skip_variable;
                m_length_limit_APPn[i] = 0;
            }

            m_process_APPn[0] = get_interesting_appn;
            m_process_APPn[14] = get_interesting_appn;

            /* Reset marker processing state */
            reset_marker_reader();
        }

        /// <summary>
        /// Reset marker processing state to begin a fresh datastream.
        /// </summary>
        public void reset_marker_reader()
        {
            m_cinfo.Comp_info = null;        /* until allocated by get_sof */
            m_cinfo.m_input_scan_number = 0;       /* no SOS seen yet */
            m_cinfo.m_unread_marker = 0;       /* no pending marker */
            m_saw_SOI = false;        /* set internal state too */
            m_saw_SOF = false;
            m_discarded_bytes = 0;
            m_cur_marker = null;
        }

        /// <summary>
        /// Read markers until SOS or EOI.
        /// 
        /// Returns same codes as are defined for jpeg_consume_input:
        /// JPEG_SUSPENDED, JPEG_REACHED_SOS, or JPEG_REACHED_EOI.
        /// </summary>
        public ReadResult read_markers()
        {
            /* Outer loop repeats once for each marker. */
            for (; ; )
            {
                /* Collect the marker proper, unless we already did. */
                /* NB: first_marker() enforces the requirement that SOI appear first. */
                if (m_cinfo.m_unread_marker == 0)
                {
                    if (!m_cinfo.m_marker.m_saw_SOI)
                    {
                        if (!first_marker())
                            return ReadResult.Suspended;
                    }
                    else
                    {
                        if (!next_marker())
                            return ReadResult.Suspended;
                    }
                }

                /* At this point m_cinfo.unread_marker contains the marker code and the
                 * input point is just past the marker proper, but before any parameters.
                 * A suspension will cause us to return with this state still true.
                 */
                switch ((JpegMarkerType)m_cinfo.m_unread_marker)
                {
                    case JpegMarkerType.SOI:
                        if (!get_soi())
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.SOF0:
                    /* Baseline */
                    case JpegMarkerType.SOF1:
                        /* Extended sequential, Huffman */
                        if (!get_sof(false))
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.SOF2:
                        /* Progressive, Huffman */
                        if (!get_sof(true))
                            return ReadResult.Suspended;
                        break;

                    /* Currently unsupported SOFn types */
                    case JpegMarkerType.SOF3:
                    /* Lossless, Huffman */
                    case JpegMarkerType.SOF5:
                    /* Differential sequential, Huffman */
                    case JpegMarkerType.SOF6:
                    /* Differential progressive, Huffman */
                    case JpegMarkerType.SOF7:
                    /* Differential lossless, Huffman */
                    case JpegMarkerType.SOF9:
                    /* Extended sequential, arithmetic */
                    case JpegMarkerType.SOF10:
                    /* Progressive, arithmetic */
                    case JpegMarkerType.JPG:
                    /* Reserved for JPEG extensions */
                    case JpegMarkerType.SOF11:
                    /* Lossless, arithmetic */
                    case JpegMarkerType.SOF13:
                    /* Differential sequential, arithmetic */
                    case JpegMarkerType.SOF14:
                    /* Differential progressive, arithmetic */
                    case JpegMarkerType.SOF15:
                        /* Differential lossless, arithmetic */
                        throw new Exception(String.Format("Unsupported JPEG process: SOF type 0x{0:X2}", m_cinfo.m_unread_marker));

                    case JpegMarkerType.SOS:
                        if (!get_sos())
                            return ReadResult.Suspended;
                        m_cinfo.m_unread_marker = 0;   /* processed the marker */
                        return ReadResult.Reached_SOS;

                    case JpegMarkerType.EOI:
                        m_cinfo.m_unread_marker = 0;   /* processed the marker */
                        return ReadResult.Reached_EOI;

                    case JpegMarkerType.DAC:
                        if (!skip_variable(m_cinfo))
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.DHT:
                        if (!get_dht())
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.DQT:
                        if (!get_dqt())
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.DRI:
                        if (!get_dri())
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.APP0:
                    case JpegMarkerType.APP1:
                    case JpegMarkerType.APP2:
                    case JpegMarkerType.APP3:
                    case JpegMarkerType.APP4:
                    case JpegMarkerType.APP5:
                    case JpegMarkerType.APP6:
                    case JpegMarkerType.APP7:
                    case JpegMarkerType.APP8:
                    case JpegMarkerType.APP9:
                    case JpegMarkerType.APP10:
                    case JpegMarkerType.APP11:
                    case JpegMarkerType.APP12:
                    case JpegMarkerType.APP13:
                    case JpegMarkerType.APP14:
                    case JpegMarkerType.APP15:
                        if (!m_cinfo.m_marker.m_process_APPn[m_cinfo.m_unread_marker - (int)JpegMarkerType.APP0](m_cinfo))
                            return ReadResult.Suspended;
                        break;

                    case JpegMarkerType.COM:
                        if (!m_cinfo.m_marker.m_process_COM(m_cinfo))
                            return ReadResult.Suspended;
                        break;

                    /* these are all parameter-less */
                    case JpegMarkerType.RST0:
                    case JpegMarkerType.RST1:
                    case JpegMarkerType.RST2:
                    case JpegMarkerType.RST3:
                    case JpegMarkerType.RST4:
                    case JpegMarkerType.RST5:
                    case JpegMarkerType.RST6:
                    case JpegMarkerType.RST7:
                    case JpegMarkerType.TEM:
                        break;

                    case JpegMarkerType.DNL:
                        /* Ignore DNL ... perhaps the wrong thing */
                        if (!skip_variable(m_cinfo))
                            return ReadResult.Suspended;
                        break;

                    default:
                        /* must be DHP, EXP, JPGn, or RESn */
                        /* For now, we treat the reserved markers as fatal errors since they are
                         * likely to be used to signal incompatible JPEG Part 3 extensions.
                         * Once the JPEG 3 version-number marker is well defined, this code
                         * ought to change!
                         */
                        throw new Exception(String.Format("Unsupported marker type 0x{0:X2}", m_cinfo.m_unread_marker));
                }

                /* Successfully processed marker, so reset state variable */
                m_cinfo.m_unread_marker = 0;
            } /* end loop */
        }

        /// <summary>
        /// Read a restart marker, which is expected to appear next in the data-stream;
        /// if the marker is not there, take appropriate recovery action.
        /// Returns false if suspension is required.
        /// 
        /// Made public for use by entropy decoder only
        /// 
        /// This is called by the entropy decoder after it has read an appropriate
        /// number of MCUs.  cinfo.unread_marker may be nonzero if the entropy decoder
        /// has already read a marker from the data source.  Under normal conditions
        /// cinfo.unread_marker will be reset to 0 before returning; if not reset,
        /// it holds a marker which the decoder will be unable to read past.
        /// </summary>
        public bool read_restart_marker()
        {
            /* Obtain a marker unless we already did. */
            /* Note that next_marker will complain if it skips any data. */
            if (m_cinfo.m_unread_marker == 0)
            {
                if (!next_marker())
                    return false;
            }

            if (m_cinfo.m_unread_marker == ((int)JpegMarkerType.RST0 + m_cinfo.m_marker.m_next_restart_num))
            {
                /* Normal case --- swallow the marker and let entropy decoder continue */
                m_cinfo.m_unread_marker = 0;
            }
            else
            {
                /* Uh-oh, the restart markers have been messed up. */
                /* Let the data source manager determine how to re-sync. */
                if (!m_cinfo.m_src.resync_to_restart(m_cinfo, m_cinfo.m_marker.m_next_restart_num))
                    return false;
            }

            /* Update next-restart state */
            m_cinfo.m_marker.m_next_restart_num = (m_cinfo.m_marker.m_next_restart_num + 1) & 7;

            return true;
        }

        /// <summary>
        /// Find the next JPEG marker, save it in cinfo.unread_marker.
        /// Returns false if had to suspend before reaching a marker;
        /// in that case cinfo.unread_marker is unchanged.
        /// 
        /// Note that the result might not be a valid marker code,
        /// but it will never be 0 or FF.
        /// </summary>
        public bool next_marker()
        {
            int c;
            for (; ; )
            {
                if (!m_cinfo.m_src.GetByte(out c))
                    return false;

                /* Skip any non-FF bytes.
                 * This may look a bit inefficient, but it will not occur in a valid file.
                 * We sync after each discarded byte so that a suspending data source
                 * can discard the byte from its buffer.
                 */
                while (c != 0xFF)
                {
                    m_cinfo.m_marker.m_discarded_bytes++;
                    if (!m_cinfo.m_src.GetByte(out c))
                        return false;
                }

                /* This loop swallows any duplicate FF bytes.  Extra FFs are legal as
                 * pad bytes, so don't count them in discarded_bytes.  We assume there
                 * will not be so many consecutive FF bytes as to overflow a suspending
                 * data source's input buffer.
                 */
                do
                {
                    if (!m_cinfo.m_src.GetByte(out c))
                        return false;
                }
                while (c == 0xFF);

                if (c != 0)
                {
                    /* found a valid marker, exit loop */
                    break;
                }

                /* Reach here if we found a stuffed-zero data sequence (FF/00).
                 * Discard it and loop back to try again.
                 */
                m_cinfo.m_marker.m_discarded_bytes += 2;
            }

            if (m_cinfo.m_marker.m_discarded_bytes != 0)
            {
                m_cinfo.m_marker.m_discarded_bytes = 0;
            }

            m_cinfo.m_unread_marker = c;
            return true;
        }

        /// <summary>
        /// Install a special processing method for COM or APPn markers.
        /// </summary>
        public void jpeg_set_marker_processor(int marker_code, JpegDecompressor.jpeg_marker_parser_method routine)
        {
            if (marker_code == (int)JpegMarkerType.COM)
                m_process_COM = routine;
            else if (marker_code >= (int)JpegMarkerType.APP0 && marker_code <= (int)JpegMarkerType.APP15)
                m_process_APPn[marker_code - (int)JpegMarkerType.APP0] = routine;
            else
                throw new Exception(String.Format("Unsupported marker type 0x{0:X2}", marker_code));
        }

        public void jpeg_save_markers(int marker_code, int length_limit)
        {
            /* Choose processor routine to use.
             * APP0/APP14 have special requirements.
             */
            JpegDecompressor.jpeg_marker_parser_method processor;
            if (length_limit != 0)
            {
                processor = save_marker;
                /* If saving APP0/APP14, save at least enough for our internal use. */
                if (marker_code == (int)JpegMarkerType.APP0 && length_limit < APP0_DATA_LEN)
                    length_limit = APP0_DATA_LEN;
                else if (marker_code == (int)JpegMarkerType.APP14 && length_limit < APP14_DATA_LEN)
                    length_limit = APP14_DATA_LEN;
            }
            else
            {
                processor = skip_variable;
                /* If discarding APP0/APP14, use our regular on-the-fly processor. */
                if (marker_code == (int)JpegMarkerType.APP0 || marker_code == (int)JpegMarkerType.APP14)
                    processor = get_interesting_appn;
            }

            if (marker_code == (int)JpegMarkerType.COM)
            {
                m_process_COM = processor;
                m_length_limit_COM = length_limit;
            }
            else if (marker_code >= (int)JpegMarkerType.APP0 && marker_code <= (int)JpegMarkerType.APP15)
            {
                m_process_APPn[marker_code - (int)JpegMarkerType.APP0] = processor;
                m_length_limit_APPn[marker_code - (int)JpegMarkerType.APP0] = length_limit;
            }
            else
                throw new Exception(String.Format("Unsupported marker type 0x{0:X2}", marker_code));
        }

        /* State of marker reader, applications
        * supplying COM or APPn handlers might like to know the state.
        */
        public bool SawSOI()
        {
            return m_saw_SOI;
        }

        public bool SawSOF()
        {
            return m_saw_SOF;
        }

        public int NextRestartNumber()
        {
            return m_next_restart_num;
        }

        public int DiscardedByteCount()
        {
            return m_discarded_bytes;
        }

        public void SkipBytes(int count)
        {
            m_discarded_bytes += count;
        }

        /// <summary>
        /// Save an APPn or COM marker into the marker list
        /// </summary>
        private static bool save_marker(JpegDecompressor cinfo)
        {
            JpegMarker cur_marker = cinfo.m_marker.m_cur_marker;

            byte[] data = null;
            int length = 0;
            int bytes_read;
            int data_length;
            int dataOffset = 0;

            if (cur_marker == null)
            {
                /* begin reading a marker */
                if (!cinfo.m_src.GetTwoBytes(out length))
                    return false;

                length -= 2;
                if (length >= 0)
                {
                    /* watch out for bogus length word */
                    /* figure out how much we want to save */
                    int limit;
                    if (cinfo.m_unread_marker == (int)JpegMarkerType.COM)
                        limit = cinfo.m_marker.m_length_limit_COM;
                    else
                        limit = cinfo.m_marker.m_length_limit_APPn[cinfo.m_unread_marker - (int)JpegMarkerType.APP0];

                    if (length < limit)
                        limit = length;

                    /* allocate and initialize the marker item */
                    cur_marker = new JpegMarker((byte)cinfo.m_unread_marker, length, limit);

                    /* data area is just beyond the JpegMarker */
                    data = cur_marker.Data;
                    cinfo.m_marker.m_cur_marker = cur_marker;
                    cinfo.m_marker.m_bytes_read = 0;
                    bytes_read = 0;
                    data_length = limit;
                }
                else
                {
                    /* deal with bogus length word */
                    bytes_read = data_length = 0;
                    data = null;
                }
            }
            else
            {
                /* resume reading a marker */
                bytes_read = cinfo.m_marker.m_bytes_read;
                data_length = cur_marker.Data.Length;
                data = cur_marker.Data;
                dataOffset = bytes_read;
            }

            byte[] tempData = null;
            if (data_length != 0)
                tempData = new byte[data.Length];

            while (bytes_read < data_length)
            {
                /* move the restart point to here */
                cinfo.m_marker.m_bytes_read = bytes_read;

                /* If there's not at least one byte in buffer, suspend */
                if (!cinfo.m_src.MakeByteAvailable())
                    return false;

                /* Copy bytes with reasonable rapidity */
                int read = cinfo.m_src.GetBytes(tempData, data_length - bytes_read);
                Buffer.BlockCopy(tempData, 0, data, dataOffset, data_length - bytes_read);
                bytes_read += read;
            }

            /* Done reading what we want to read */
            if (cur_marker != null)
            {
                /* will be null if bogus length word */
                /* Add new marker to end of list */
                cinfo.m_marker_list.Add(cur_marker);

                /* Reset pointer & calc remaining data length */
                data = cur_marker.Data;
                dataOffset = 0;
                length = cur_marker.OriginalLength - data_length;
            }

            /* Reset to initial state for next marker */
            cinfo.m_marker.m_cur_marker = null;

            JpegMarkerType currentMarker = (JpegMarkerType)cinfo.m_unread_marker;
            if (data_length != 0 && (currentMarker == JpegMarkerType.APP0 || currentMarker == JpegMarkerType.APP14))
            {
                tempData = new byte[data.Length];
                Buffer.BlockCopy(data, dataOffset, tempData, 0, data.Length - dataOffset);
            }

            /* Process the marker if interesting; else just make a generic trace msg */
            switch ((JpegMarkerType)cinfo.m_unread_marker)
            {
                case JpegMarkerType.APP0:
                    examine_app0(cinfo, tempData, data_length, length);
                    break;
                case JpegMarkerType.APP14:
                    examine_app14(cinfo, tempData, data_length, length);
                    break;
                default:
                    break;
            }

            /* skip any remaining data -- could be lots */
            if (length > 0)
                cinfo.m_src.skip_input_data(length);

            return true;
        }

        /// <summary>
        /// Skip over an unknown or uninteresting variable-length marker
        /// </summary>
        private static bool skip_variable(JpegDecompressor cinfo)
        {
            int length;
            if (!cinfo.m_src.GetTwoBytes(out length))
                return false;

            length -= 2;

            if (length > 0)
                cinfo.m_src.skip_input_data(length);

            return true;
        }

        /// <summary>
        /// Process an APP0 or APP14 marker without saving it
        /// </summary>
        private static bool get_interesting_appn(JpegDecompressor cinfo)
        {
            int length;
            if (!cinfo.m_src.GetTwoBytes(out length))
                return false;

            length -= 2;

            /* get the interesting part of the marker data */
            int numtoread = 0;
            if (length >= APPN_DATA_LEN)
                numtoread = APPN_DATA_LEN;
            else if (length > 0)
                numtoread = length;

            byte[] b = new byte[APPN_DATA_LEN];
            for (int i = 0; i < numtoread; i++)
            {
                int temp = 0;
                if (!cinfo.m_src.GetByte(out temp))
                    return false;

                b[i] = (byte)temp;
            }

            length -= numtoread;

            /* process it */
            switch ((JpegMarkerType)cinfo.m_unread_marker)
            {
                case JpegMarkerType.APP0:
                    examine_app0(cinfo, b, numtoread, length);
                    break;
                case JpegMarkerType.APP14:
                    examine_app14(cinfo, b, numtoread, length);
                    break;
                default:
                    /* can't get here unless jpeg_save_markers chooses wrong processor */
                    throw new Exception(String.Format("Unsupported marker type 0x{0:X2}", cinfo.m_unread_marker));
            }

            /* skip any remaining data -- could be lots */
            if (length > 0)
                cinfo.m_src.skip_input_data(length);

            return true;
        }

        /*
         * Routines for processing APPn and COM markers.
         * These are either saved in memory or discarded, per application request.
         * APP0 and APP14 are specially checked to see if they are
         * JFIF and Adobe markers, respectively.
         */

        /// <summary>
        /// Examine first few bytes from an APP0.
        /// Take appropriate action if it is a JFIF marker.
        /// datalen is # of bytes at data[], remaining is length of rest of marker data.
        /// </summary>
        private static void examine_app0(JpegDecompressor cinfo, byte[] data, int datalen, int remaining)
        {
            int totallen = datalen + remaining;

            if (datalen >= APP0_DATA_LEN &&
                data[0] == 0x4A &&
                data[1] == 0x46 &&
                data[2] == 0x49 &&
                data[3] == 0x46 &&
                data[4] == 0)
            {
                /* Found JFIF APP0 marker: save info */
                cinfo.m_saw_JFIF_marker = true;
                cinfo.m_JFIF_major_version = data[5];
                cinfo.m_JFIF_minor_version = data[6];
                cinfo.m_density_unit = (DensityUnit)data[7];
                cinfo.m_X_density = (short)((data[8] << 8) + data[9]);
                cinfo.m_Y_density = (short)((data[10] << 8) + data[11]);
            }
            else if (datalen >= 6 && data[0] == 0x4A && data[1] == 0x46 && data[2] == 0x58 && data[3] == 0x58 && data[4] == 0)
            {
                /* Found JFIF "JFXX" extension APP0 marker */
                /* The library doesn't actually do anything with these.
                 */
            }
            else
            {
                /* Start of APP0 does not match "JFIF" or "JFXX", or too short */
            }
        }

        /// <summary>
        /// Examine first few bytes from an APP14.
        /// Take appropriate action if it is an Adobe marker.
        /// datalen is # of bytes at data[], remaining is length of rest of marker data.
        /// </summary>
        private static void examine_app14(JpegDecompressor cinfo, byte[] data, int datalen, int remaining)
        {
            if (datalen >= APP14_DATA_LEN &&
                data[0] == 0x41 &&
                data[1] == 0x64 &&
                data[2] == 0x6F &&
                data[3] == 0x62 &&
                data[4] == 0x65)
            {
                /* Found Adobe APP14 marker */
                int version = (data[5] << 8) + data[6];
                int flags0 = (data[7] << 8) + data[8];
                int flags1 = (data[9] << 8) + data[10];
                int transform = data[11];
                cinfo.m_saw_Adobe_marker = true;
                cinfo.m_Adobe_transform = (byte)transform;
            }
            else
            {
                /* Start of APP14 does not match "Adobe", or too short */
            }
        }

        /*
         * Routines to process JPEG markers.
         *
         * Entry condition: JPEG marker itself has been read and its code saved
         *   in cinfo.unread_marker; input restart point is just after the marker.
         *
         * Exit: if return true, have read and processed any parameters, and have
         *   updated the restart point to point after the parameters.
         *   If return false, was forced to suspend before reaching end of
         *   marker parameters; restart point has not been moved.  Same routine
         *   will be called again after application supplies more input data.
         *
         * This approach to suspension assumes that all of a marker's parameters
         * can fit into a single input buffer-load.  This should hold for "normal"
         * markers.  Some COM/APPn markers might have large parameter segments
         * that might not fit.  If we are simply dropping such a marker, we use
         * skip_input_data to get past it, and thereby put the problem on the
         * source manager's shoulders.  If we are saving the marker's contents
         * into memory, we use a slightly different convention: when forced to
         * suspend, the marker processor updates the restart point to the end of
         * what it's consumed (ie, the end of the buffer) before returning false.
         * On resumption, cinfo.unread_marker still contains the marker code,
         * but the data source will point to the next chunk of marker data.
         * The marker processor must retain internal state to deal with this.
         *
         * Note that we don't bother to avoid duplicate trace messages if a
         * suspension occurs within marker parameters.  Other side effects
         * require more care.
         */


        /// <summary>
        /// Process an SOI marker
        /// </summary>
        private bool get_soi()
        {
            if (m_cinfo.m_marker.m_saw_SOI)
                throw new Exception("Invalid JPEG file structure: two SOI markers");

            /* Reset all parameters that are defined to be reset by SOI */
            m_cinfo.m_restart_interval = 0;

            /* Set initial assumptions for colorspace etc */

            m_cinfo.m_jpeg_color_space = ColorSpace.Unknown;
            m_cinfo.m_CCIR601_sampling = false; /* Assume non-CCIR sampling??? */

            m_cinfo.m_saw_JFIF_marker = false;
            m_cinfo.m_JFIF_major_version = 1; /* set default JFIF APP0 values */
            m_cinfo.m_JFIF_minor_version = 1;
            m_cinfo.m_density_unit = DensityUnit.Unknown;
            m_cinfo.m_X_density = 1;
            m_cinfo.m_Y_density = 1;
            m_cinfo.m_saw_Adobe_marker = false;
            m_cinfo.m_Adobe_transform = 0;

            m_cinfo.m_marker.m_saw_SOI = true;

            return true;
        }

        /// <summary>
        /// Process a SOFn marker
        /// </summary>
        private bool get_sof(bool is_prog)
        {
            m_cinfo.m_progressive_mode = is_prog;

            int length;
            if (!m_cinfo.m_src.GetTwoBytes(out length))
                return false;

            if (!m_cinfo.m_src.GetByte(out m_cinfo.m_data_precision))
                return false;

            int temp = 0;
            if (!m_cinfo.m_src.GetTwoBytes(out temp))
                return false;
            m_cinfo.m_image_height = temp;

            if (!m_cinfo.m_src.GetTwoBytes(out temp))
                return false;
            m_cinfo.m_image_width = temp;

            if (!m_cinfo.m_src.GetByte(out m_cinfo.m_num_components))
                return false;

            length -= 8;

            if (m_cinfo.m_marker.m_saw_SOF)
                throw new Exception("Invalid JPEG file structure: two SOI markers");

            /* We don't support files in which the image height is initially specified */
            /* as 0 and is later redefined by DNL.  As long as we have to check that,  */
            /* might as well have a general sanity check. */
            if (m_cinfo.m_image_height <= 0 || m_cinfo.m_image_width <= 0 || m_cinfo.m_num_components <= 0)
                throw new Exception("Empty JPEG image (DNL not supported)");

            if (length != (m_cinfo.m_num_components * 3))
                throw new Exception("Bogus marker length");

            if (m_cinfo.Comp_info == null)
            {
                /* do only once, even if suspend */
                m_cinfo.Comp_info = JpegComponent.createArrayOfComponents(m_cinfo.m_num_components);
            }

            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                m_cinfo.Comp_info[ci].Component_index = ci;

                int component_id;
                if (!m_cinfo.m_src.GetByte(out component_id))
                    return false;

                m_cinfo.Comp_info[ci].Component_id = component_id;

                int c;
                if (!m_cinfo.m_src.GetByte(out c))
                    return false;

                m_cinfo.Comp_info[ci].H_samp_factor = (c >> 4) & 15;
                m_cinfo.Comp_info[ci].V_samp_factor = (c) & 15;

                int quant_tbl_no;
                if (!m_cinfo.m_src.GetByte(out quant_tbl_no))
                    return false;

                m_cinfo.Comp_info[ci].Quant_tbl_no = quant_tbl_no;
            }

            m_cinfo.m_marker.m_saw_SOF = true;
            return true;
        }

        /// <summary>
        /// Process a SOS marker
        /// </summary>
        private bool get_sos()
        {
            if (!m_cinfo.m_marker.m_saw_SOF)
                throw new Exception("Invalid JPEG file structure: SOS before SOF");

            int length;
            if (!m_cinfo.m_src.GetTwoBytes(out length))
                return false;

            /* Number of components */
            int n;
            if (!m_cinfo.m_src.GetByte(out n))
                return false;

            if (length != (n * 2 + 6) || n < 1 || n > JpegConstants.MaxComponentsInScan)
                throw new Exception("Bogus marker length");

            m_cinfo.m_comps_in_scan = n;

            /* Collect the component-spec parameters */

            for (int i = 0; i < n; i++)
            {
                int cc;
                if (!m_cinfo.m_src.GetByte(out cc))
                    return false;

                int c;
                if (!m_cinfo.m_src.GetByte(out c))
                    return false;

                bool idFound = false;
                int foundIndex = -1;
                for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                {
                    if (cc == m_cinfo.Comp_info[ci].Component_id)
                    {
                        foundIndex = ci;
                        idFound = true;
                        break;
                    }
                }

                if (!idFound)
                    throw new Exception(String.Format("Invalid component ID {0} in SOS", cc));

                m_cinfo.m_cur_comp_info[i] = foundIndex;
                m_cinfo.Comp_info[foundIndex].Dc_tbl_no = (c >> 4) & 15;
                m_cinfo.Comp_info[foundIndex].Ac_tbl_no = (c) & 15;
            }

            /* Collect the additional scan parameters Ss, Se, Ah/Al. */
            int temp;
            if (!m_cinfo.m_src.GetByte(out temp))
                return false;

            m_cinfo.m_Ss = temp;
            if (!m_cinfo.m_src.GetByte(out temp))
                return false;

            m_cinfo.m_Se = temp;
            if (!m_cinfo.m_src.GetByte(out temp))
                return false;

            m_cinfo.m_Ah = (temp >> 4) & 15;
            m_cinfo.m_Al = (temp) & 15;

            /* Prepare to scan data & restart markers */
            m_cinfo.m_marker.m_next_restart_num = 0;

            /* Count another SOS marker */
            m_cinfo.m_input_scan_number++;
            return true;
        }

        /// <summary>
        /// Process a DHT marker
        /// </summary>
        private bool get_dht()
        {
            int length;
            if (!m_cinfo.m_src.GetTwoBytes(out length))
                return false;

            length -= 2;

            byte[] bits = new byte[17];
            byte[] huffval = new byte[256];
            while (length > 16)
            {
                int index;
                if (!m_cinfo.m_src.GetByte(out index))
                    return false;

                bits[0] = 0;
                int count = 0;
                for (int i = 1; i <= 16; i++)
                {
                    int temp = 0;
                    if (!m_cinfo.m_src.GetByte(out temp))
                        return false;

                    bits[i] = (byte)temp;
                    count += bits[i];
                }

                length -= 1 + 16;

                /* Here we just do minimal validation of the counts to avoid walking
                 * off the end of our table space. HuffEntropyDecoder will check more carefully.
                 */
                if (count > 256 || count > length)
                    throw new Exception("Bogus Huffman table definition");

                for (int i = 0; i < count; i++)
                {
                    int temp = 0;
                    if (!m_cinfo.m_src.GetByte(out temp))
                        return false;

                    huffval[i] = (byte)temp;
                }

                length -= count;

                JpegHuffmanTable htblptr = null;
                if ((index & 0x10) != 0)
                {
                    /* AC table definition */
                    index -= 0x10;
                    if (m_cinfo.m_ac_huff_tbl_ptrs[index] == null)
                        m_cinfo.m_ac_huff_tbl_ptrs[index] = new JpegHuffmanTable();

                    htblptr = m_cinfo.m_ac_huff_tbl_ptrs[index];
                }
                else
                {
                    /* DC table definition */
                    if (m_cinfo.m_dc_huff_tbl_ptrs[index] == null)
                        m_cinfo.m_dc_huff_tbl_ptrs[index] = new JpegHuffmanTable();

                    htblptr = m_cinfo.m_dc_huff_tbl_ptrs[index];
                }

                if (index < 0 || index >= JpegConstants.NumberOfHuffmanTables)
                    throw new Exception(String.Format("Bogus DHT index {0}", index));

                Buffer.BlockCopy(bits, 0, htblptr.Bits, 0, htblptr.Bits.Length);
                Buffer.BlockCopy(huffval, 0, htblptr.Huffval, 0, htblptr.Huffval.Length);
            }

            if (length != 0)
                throw new Exception("Bogus marker length");

            return true;
        }

        /// <summary>
        /// Process a DQT marker
        /// </summary>
        private bool get_dqt()
        {
            int length;
            if (!m_cinfo.m_src.GetTwoBytes(out length))
                return false;

            length -= 2;
            while (length > 0)
            {
                int n;
                if (!m_cinfo.m_src.GetByte(out n))
                    return false;

                int prec = n >> 4;
                n &= 0x0F;

                if (n >= JpegConstants.NumberOfQuantTables)
                    throw new Exception(String.Format("Bogus DQT index {0}", n));

                if (m_cinfo.m_quant_tbl_ptrs[n] == null)
                    m_cinfo.m_quant_tbl_ptrs[n] = new JpegQuantizationTable();

                JpegQuantizationTable quant_ptr = m_cinfo.m_quant_tbl_ptrs[n];

                for (int i = 0; i < JpegConstants.DCTSize2; i++)
                {
                    int tmp;
                    if (prec != 0)
                    {
                        int temp = 0;
                        if (!m_cinfo.m_src.GetTwoBytes(out temp))
                            return false;

                        tmp = temp;
                    }
                    else
                    {
                        int temp = 0;
                        if (!m_cinfo.m_src.GetByte(out temp))
                            return false;

                        tmp = temp;
                    }

                    /* We convert the zigzag-order table to natural array order. */
                    quant_ptr.quantval[JpegUtils.jpeg_natural_order[i]] = (short)tmp;
                }

                length -= JpegConstants.DCTSize2 + 1;
                if (prec != 0)
                    length -= JpegConstants.DCTSize2;
            }

            if (length != 0)
                throw new Exception("Bogus marker length");

            return true;
        }

        /// <summary>
        /// Process a DRI marker
        /// </summary>
        private bool get_dri()
        {
            int length;
            if (!m_cinfo.m_src.GetTwoBytes(out length))
                return false;

            if (length != 4)
                throw new Exception("Bogus marker length");

            int temp = 0;
            if (!m_cinfo.m_src.GetTwoBytes(out temp))
                return false;

            int tmp = temp;
            m_cinfo.m_restart_interval = tmp;

            return true;
        }

        /// <summary>
        /// Like next_marker, but used to obtain the initial SOI marker.
        /// For this marker, we do not allow preceding garbage or fill; otherwise,
        /// we might well scan an entire input file before realizing it's not JPEG.
        /// If an application wants to process non-JFIF files, it must seek to the
        /// SOI before calling the JPEG library.
        /// </summary>
        private bool first_marker()
        {
            int c;
            if (!m_cinfo.m_src.GetByte(out c))
                return false;

            int c2;
            if (!m_cinfo.m_src.GetByte(out c2))
                return false;

            if (c != 0xFF || c2 != (int)JpegMarkerType.SOI)
                throw new Exception(String.Format("Not a JPEG file: starts with 0x{0:X2} 0x{1:X2}", c, c2));

            m_cinfo.m_unread_marker = c2;
            return true;
        }
    }
    #endregion

    #region JpegMarkerType
    /// <summary>
    /// JPEG marker codes.
    /// </summary>
    /// <seealso href="81c88818-a5d7-4550-9ce5-024a768f7b1e.htm" target="_self">Special markers</seealso>
    public enum JpegMarkerType
    {
        /// <summary>
        /// 
        /// </summary>
        SOF0 = 0xc0,
        /// <summary>
        /// 
        /// </summary>
        SOF1 = 0xc1,
        /// <summary>
        /// 
        /// </summary>
        SOF2 = 0xc2,
        /// <summary>
        /// 
        /// </summary>
        SOF3 = 0xc3,
        /// <summary>
        /// 
        /// </summary>
        SOF5 = 0xc5,
        /// <summary>
        /// 
        /// </summary>
        SOF6 = 0xc6,
        /// <summary>
        /// 
        /// </summary>
        SOF7 = 0xc7,
        /// <summary>
        /// 
        /// </summary>
        JPG = 0xc8,
        /// <summary>
        /// 
        /// </summary>
        SOF9 = 0xc9,
        /// <summary>
        /// 
        /// </summary>
        SOF10 = 0xca,
        /// <summary>
        /// 
        /// </summary>
        SOF11 = 0xcb,
        /// <summary>
        /// 
        /// </summary>
        SOF13 = 0xcd,
        /// <summary>
        /// 
        /// </summary>
        SOF14 = 0xce,
        /// <summary>
        /// 
        /// </summary>
        SOF15 = 0xcf,
        /// <summary>
        /// 
        /// </summary>
        DHT = 0xc4,
        /// <summary>
        /// 
        /// </summary>
        DAC = 0xcc,
        /// <summary>
        /// 
        /// </summary>
        RST0 = 0xd0,
        /// <summary>
        /// 
        /// </summary>
        RST1 = 0xd1,
        /// <summary>
        /// 
        /// </summary>
        RST2 = 0xd2,
        /// <summary>
        /// 
        /// </summary>
        RST3 = 0xd3,
        /// <summary>
        /// 
        /// </summary>
        RST4 = 0xd4,
        /// <summary>
        /// 
        /// </summary>
        RST5 = 0xd5,
        /// <summary>
        /// 
        /// </summary>
        RST6 = 0xd6,
        /// <summary>
        /// 
        /// </summary>
        RST7 = 0xd7,
        /// <summary>
        /// 
        /// </summary>
        SOI = 0xd8,
        /// <summary>
        /// 
        /// </summary>
        EOI = 0xd9,
        /// <summary>
        /// 
        /// </summary>
        SOS = 0xda,
        /// <summary>
        /// 
        /// </summary>
        DQT = 0xdb,
        /// <summary>
        /// 
        /// </summary>
        DNL = 0xdc,
        /// <summary>
        /// 
        /// </summary>
        DRI = 0xdd,
        /// <summary>
        /// 
        /// </summary>
        DHP = 0xde,
        /// <summary>
        /// 
        /// </summary>
        EXP = 0xdf,
        /// <summary>
        /// 
        /// </summary>
        APP0 = 0xe0,
        /// <summary>
        /// 
        /// </summary>
        APP1 = 0xe1,
        /// <summary>
        /// 
        /// </summary>
        APP2 = 0xe2,
        /// <summary>
        /// 
        /// </summary>
        APP3 = 0xe3,
        /// <summary>
        /// 
        /// </summary>
        APP4 = 0xe4,
        /// <summary>
        /// 
        /// </summary>
        APP5 = 0xe5,
        /// <summary>
        /// 
        /// </summary>
        APP6 = 0xe6,
        /// <summary>
        /// 
        /// </summary>
        APP7 = 0xe7,
        /// <summary>
        /// 
        /// </summary>
        APP8 = 0xe8,
        /// <summary>
        /// 
        /// </summary>
        APP9 = 0xe9,
        /// <summary>
        /// 
        /// </summary>
        APP10 = 0xea,
        /// <summary>
        /// 
        /// </summary>
        APP11 = 0xeb,
        /// <summary>
        /// 
        /// </summary>
        APP12 = 0xec,
        /// <summary>
        /// 
        /// </summary>
        APP13 = 0xed,
        /// <summary>
        /// 
        /// </summary>
        APP14 = 0xee,
        /// <summary>
        /// 
        /// </summary>
        APP15 = 0xef,
        /// <summary>
        /// 
        /// </summary>
        JPG0 = 0xf0,
        /// <summary>
        /// 
        /// </summary>
        JPG13 = 0xfd,
        /// <summary>
        /// 
        /// </summary>
        COM = 0xfe,
        /// <summary>
        /// 
        /// </summary>
        TEM = 0x01,
        /// <summary>
        /// 
        /// </summary>
        ERROR = 0x100
    }
    #endregion

    #region JpegMarkerWriter
    /// <summary>
    /// Marker writing
    /// </summary>
    class JpegMarkerWriter
    {
        private JpegCompressor m_cinfo;
        private int m_last_restart_interval; /* last DRI value emitted; 0 after SOI */

        public JpegMarkerWriter(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;
        }

        /// <summary>
        /// Write datastream header.
        /// This consists of an SOI and optional APPn markers.
        /// We recommend use of the JFIF marker, but not the Adobe marker,
        /// when using YCbCr or grayscale data.  The JFIF marker should NOT
        /// be used for any other JPEG colorspace.  The Adobe marker is helpful
        /// to distinguish RGB, CMYK, and YCCK colorspaces.
        /// Note that an application can write additional header markers after
        /// jpeg_start_compress returns.
        /// </summary>
        public void write_file_header()
        {
            emit_marker(JpegMarkerType.SOI);  /* first the SOI */

            /* SOI is defined to reset restart interval to 0 */
            m_last_restart_interval = 0;

            if (m_cinfo.m_write_JFIF_header)   /* next an optional JFIF APP0 */
                emit_jfif_app0();
            if (m_cinfo.m_write_Adobe_marker) /* next an optional Adobe APP14 */
                emit_adobe_app14();
        }

        /// <summary>
        /// Write frame header.
        /// This consists of DQT and SOFn markers.
        /// Note that we do not emit the SOF until we have emitted the DQT(s).
        /// This avoids compatibility problems with incorrect implementations that
        /// try to error-check the quant table numbers as soon as they see the SOF.
        /// </summary>
        public void write_frame_header()
        {
            /* Emit DQT for each quantization table.
             * Note that emit_dqt() suppresses any duplicate tables.
             */
            int prec = 0;
            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                prec += emit_dqt(m_cinfo.Component_info[ci].Quant_tbl_no);

            /* now prec is nonzero iff there are any 16-bit quant tables. */

            /* Check for a non-baseline specification.
             * Note we assume that Huffman table numbers won't be changed later.
             */
            bool is_baseline;
            if (m_cinfo.m_progressive_mode || m_cinfo.m_data_precision != 8)
            {
                is_baseline = false;
            }
            else
            {
                is_baseline = true;
                for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                {
                    if (m_cinfo.Component_info[ci].Dc_tbl_no > 1 || m_cinfo.Component_info[ci].Ac_tbl_no > 1)
                        is_baseline = false;
                }

                if (prec != 0 && is_baseline)
                {
                    is_baseline = false;
                }
            }

            /* Emit the proper SOF marker */
            if (m_cinfo.m_progressive_mode)
                emit_sof(JpegMarkerType.SOF2);    /* SOF code for progressive Huffman */
            else if (is_baseline)
                emit_sof(JpegMarkerType.SOF0);    /* SOF code for baseline implementation */
            else
                emit_sof(JpegMarkerType.SOF1);    /* SOF code for non-baseline Huffman file */
        }

        /// <summary>
        /// Write scan header.
        /// This consists of DHT or DAC markers, optional DRI, and SOS.
        /// Compressed data will be written following the SOS.
        /// </summary>
        public void write_scan_header()
        {
            /* Emit Huffman tables.
             * Note that emit_dht() suppresses any duplicate tables.
             */
            for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
            {
                int ac_tbl_no = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[i]].Ac_tbl_no;
                int dc_tbl_no = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[i]].Dc_tbl_no;
                if (m_cinfo.m_progressive_mode)
                {
                    /* Progressive mode: only DC or only AC tables are used in one scan */
                    if (m_cinfo.m_Ss == 0)
                    {
                        if (m_cinfo.m_Ah == 0)
                        {
                            /* DC needs no table for refinement scan */
                            emit_dht(dc_tbl_no, false);
                        }
                    }
                    else
                    {
                        emit_dht(ac_tbl_no, true);
                    }
                }
                else
                {
                    /* Sequential mode: need both DC and AC tables */
                    emit_dht(dc_tbl_no, false);
                    emit_dht(ac_tbl_no, true);
                }
            }

            /* Emit DRI if required --- note that DRI value could change for each scan.
             * We avoid wasting space with unnecessary DRIs, however.
             */
            if (m_cinfo.m_restart_interval != m_last_restart_interval)
            {
                emit_dri();
                m_last_restart_interval = m_cinfo.m_restart_interval;
            }

            emit_sos();
        }

        /// <summary>
        /// Write datastream trailer.
        /// </summary>
        public void write_file_trailer()
        {
            emit_marker(JpegMarkerType.EOI);
        }

        /// <summary>
        /// Write an abbreviated table-specification datastream.
        /// This consists of SOI, DQT and DHT tables, and EOI.
        /// Any table that is defined and not marked sent_table = true will be
        /// emitted.  Note that all tables will be marked sent_table = true at exit.
        /// </summary>
        public void write_tables_only()
        {
            emit_marker(JpegMarkerType.SOI);

            for (int i = 0; i < JpegConstants.NumberOfQuantTables; i++)
            {
                if (m_cinfo.m_quant_tbl_ptrs[i] != null)
                    emit_dqt(i);
            }

            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
            {
                if (m_cinfo.m_dc_huff_tbl_ptrs[i] != null)
                    emit_dht(i, false);
                if (m_cinfo.m_ac_huff_tbl_ptrs[i] != null)
                    emit_dht(i, true);
            }

            emit_marker(JpegMarkerType.EOI);
        }

        //////////////////////////////////////////////////////////////////////////
        // These routines allow writing an arbitrary marker with parameters.
        // The only intended use is to emit COM or APPn markers after calling
        // write_file_header and before calling write_frame_header.
        // Other uses are not guaranteed to produce desirable results.
        // Counting the parameter bytes properly is the caller's responsibility.

        /// <summary>
        /// Emit an arbitrary marker header
        /// </summary>
        public void write_marker_header(int marker, int datalen)
        {
            if (datalen > 65533)     /* safety check */
                throw new Exception("Bogus marker length");

            emit_marker((JpegMarkerType)marker);

            emit_2bytes(datalen + 2);    /* total length */
        }

        /// <summary>
        /// Emit one byte of marker parameters following write_marker_header
        /// </summary>
        public void write_marker_byte(byte val)
        {
            emit_byte(val);
        }

        //////////////////////////////////////////////////////////////////////////
        // Routines to write specific marker types.
        //

        /// <summary>
        /// Emit a SOS marker
        /// </summary>
        private void emit_sos()
        {
            emit_marker(JpegMarkerType.SOS);

            emit_2bytes(2 * m_cinfo.m_comps_in_scan + 2 + 1 + 3); /* length */

            emit_byte(m_cinfo.m_comps_in_scan);

            for (int i = 0; i < m_cinfo.m_comps_in_scan; i++)
            {
                int componentIndex = m_cinfo.m_cur_comp_info[i];
                emit_byte(m_cinfo.Component_info[componentIndex].Component_id);

                int td = m_cinfo.Component_info[componentIndex].Dc_tbl_no;
                int ta = m_cinfo.Component_info[componentIndex].Ac_tbl_no;
                if (m_cinfo.m_progressive_mode)
                {
                    /* Progressive mode: only DC or only AC tables are used in one scan;
                     * furthermore, Huffman coding of DC refinement uses no table at all.
                     * We emit 0 for unused field(s); this is recommended by the P&M text
                     * but does not seem to be specified in the standard.
                     */
                    if (m_cinfo.m_Ss == 0)
                    {
                        /* DC scan */
                        ta = 0;
                        if (m_cinfo.m_Ah != 0)
                        {
                            /* no DC table either */
                            td = 0;
                        }
                    }
                    else
                    {
                        /* AC scan */
                        td = 0;
                    }
                }

                emit_byte((td << 4) + ta);
            }

            emit_byte(m_cinfo.m_Ss);
            emit_byte(m_cinfo.m_Se);
            emit_byte((m_cinfo.m_Ah << 4) + m_cinfo.m_Al);
        }

        /// <summary>
        /// Emit a SOF marker
        /// </summary>
        private void emit_sof(JpegMarkerType code)
        {
            emit_marker(code);

            emit_2bytes(3 * m_cinfo.m_num_components + 2 + 5 + 1); /* length */

            /* Make sure image isn't bigger than SOF field can handle */
            if (m_cinfo.m_image_height > 65535 || m_cinfo.m_image_width > 65535)
                throw new Exception(String.Format("Maximum supported image dimension is {0} pixels", 65535));

            emit_byte(m_cinfo.m_data_precision);
            emit_2bytes(m_cinfo.m_image_height);
            emit_2bytes(m_cinfo.m_image_width);

            emit_byte(m_cinfo.m_num_components);

            for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Component_info[ci];
                emit_byte(componentInfo.Component_id);
                emit_byte((componentInfo.H_samp_factor << 4) + componentInfo.V_samp_factor);
                emit_byte(componentInfo.Quant_tbl_no);
            }
        }

        /// <summary>
        /// Emit an Adobe APP14 marker
        /// </summary>
        private void emit_adobe_app14()
        {
            /*
             * Length of APP14 block    (2 bytes)
             * Block ID         (5 bytes - ASCII "Adobe")
             * Version Number       (2 bytes - currently 100)
             * Flags0           (2 bytes - currently 0)
             * Flags1           (2 bytes - currently 0)
             * Color transform      (1 byte)
             *
             * Although Adobe TN 5116 mentions Version = 101, all the Adobe files
             * now in circulation seem to use Version = 100, so that's what we write.
             *
             * We write the color transform byte as 1 if the JPEG color space is
             * YCbCr, 2 if it's YCCK, 0 otherwise.  Adobe's definition has to do with
             * whether the encoder performed a transformation, which is pretty useless.
             */

            emit_marker(JpegMarkerType.APP14);

            emit_2bytes(2 + 5 + 2 + 2 + 2 + 1); /* length */

            emit_byte(0x41); /* Identifier: ASCII "Adobe" */
            emit_byte(0x64);
            emit_byte(0x6F);
            emit_byte(0x62);
            emit_byte(0x65);
            emit_2bytes(100);    /* Version */
            emit_2bytes(0);  /* Flags0 */
            emit_2bytes(0);  /* Flags1 */
            switch (m_cinfo.m_jpeg_color_space)
            {
                case ColorSpace.YCbCr:
                    emit_byte(1);    /* Color transform = 1 */
                    break;
                case ColorSpace.YCCK:
                    emit_byte(2);    /* Color transform = 2 */
                    break;
                default:
                    emit_byte(0);    /* Color transform = 0 */
                    break;
            }
        }

        /// <summary>
        /// Emit a DRI marker
        /// </summary>
        private void emit_dri()
        {
            emit_marker(JpegMarkerType.DRI);

            emit_2bytes(4);  /* fixed length */

            emit_2bytes(m_cinfo.m_restart_interval);
        }

        /// <summary>
        /// Emit a DHT marker
        /// </summary>
        private void emit_dht(int index, bool is_ac)
        {
            JpegHuffmanTable htbl = m_cinfo.m_dc_huff_tbl_ptrs[index];
            if (is_ac)
            {
                htbl = m_cinfo.m_ac_huff_tbl_ptrs[index];
                index += 0x10; /* output index has AC bit set */
            }

            if (htbl == null)
                throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", index));

            if (!htbl.Sent_table)
            {
                emit_marker(JpegMarkerType.DHT);

                int length = 0;
                for (int i = 1; i <= 16; i++)
                    length += htbl.Bits[i];

                emit_2bytes(length + 2 + 1 + 16);
                emit_byte(index);

                for (int i = 1; i <= 16; i++)
                    emit_byte(htbl.Bits[i]);

                for (int i = 0; i < length; i++)
                    emit_byte(htbl.Huffval[i]);

                htbl.Sent_table = true;
            }
        }

        /// <summary>
        /// Emit a DQT marker
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>the precision used (0 = 8bits, 1 = 16bits) for baseline checking</returns>
        private int emit_dqt(int index)
        {
            JpegQuantizationTable qtbl = m_cinfo.m_quant_tbl_ptrs[index];
            if (qtbl == null)
                throw new Exception(String.Format("Quantization table 0x{0:X2} was not defined", index));

            int prec = 0;
            for (int i = 0; i < JpegConstants.DCTSize2; i++)
            {
                if (qtbl.quantval[i] > 255)
                    prec = 1;
            }

            if (!qtbl.Sent_table)
            {
                emit_marker(JpegMarkerType.DQT);

                emit_2bytes(prec != 0 ? JpegConstants.DCTSize2 * 2 + 1 + 2 : JpegConstants.DCTSize2 + 1 + 2);

                emit_byte(index + (prec << 4));

                for (int i = 0; i < JpegConstants.DCTSize2; i++)
                {
                    /* The table entries must be emitted in zigzag order. */
                    int qval = qtbl.quantval[JpegUtils.jpeg_natural_order[i]];

                    if (prec != 0)
                        emit_byte(qval >> 8);

                    emit_byte(qval & 0xFF);
                }

                qtbl.Sent_table = true;
            }

            return prec;
        }

        /// <summary>
        /// Emit a JFIF-compliant APP0 marker
        /// </summary>
        private void emit_jfif_app0()
        {
            /*
             * Length of APP0 block (2 bytes)
             * Block ID         (4 bytes - ASCII "JFIF")
             * Zero byte            (1 byte to terminate the ID string)
             * Version Major, Minor (2 bytes - major first)
             * Units            (1 byte - 0x00 = none, 0x01 = inch, 0x02 = cm)
             * Xdpu         (2 bytes - dots per unit horizontal)
             * Ydpu         (2 bytes - dots per unit vertical)
             * Thumbnail X size     (1 byte)
             * Thumbnail Y size     (1 byte)
             */

            emit_marker(JpegMarkerType.APP0);

            emit_2bytes(2 + 4 + 1 + 2 + 1 + 2 + 2 + 1 + 1); /* length */

            emit_byte(0x4A); /* Identifier: ASCII "JFIF" */
            emit_byte(0x46);
            emit_byte(0x49);
            emit_byte(0x46);
            emit_byte(0);
            emit_byte(m_cinfo.m_JFIF_major_version); /* Version fields */
            emit_byte(m_cinfo.m_JFIF_minor_version);
            emit_byte((int)m_cinfo.m_density_unit); /* Pixel size information */
            emit_2bytes(m_cinfo.m_X_density);
            emit_2bytes(m_cinfo.m_Y_density);
            emit_byte(0);        /* No thumbnail image */
            emit_byte(0);
        }

        //////////////////////////////////////////////////////////////////////////
        // Basic output routines.
        // 
        // Note that we do not support suspension while writing a marker.
        // Therefore, an application using suspension must ensure that there is
        // enough buffer space for the initial markers (typ. 600-700 bytes) before
        // calling jpeg_start_compress, and enough space to write the trailing EOI
        // (a few bytes) before calling jpeg_finish_compress.  Multi-pass compression
        // modes are not supported at all with suspension, so those two are the only
        // points where markers will be written.


        /// <summary>
        /// Emit a marker code
        /// </summary>
        private void emit_marker(JpegMarkerType mark)
        {
            emit_byte(0xFF);
            emit_byte((int)mark);
        }

        /// <summary>
        /// Emit a 2-byte integer; these are always MSB first in JPEG files
        /// </summary>
        private void emit_2bytes(int value)
        {
            emit_byte((value >> 8) & 0xFF);
            emit_byte(value & 0xFF);
        }

        /// <summary>
        /// Emit a byte
        /// </summary>
        private void emit_byte(int val)
        {
            if (!m_cinfo.m_dest.emit_byte(val))
                throw new Exception("Suspension not allowed here");
        }
    }
    #endregion

    #region JpegQuantizationTable
    /// <summary>
    /// DCT coefficient quantization table.
    /// </summary>
    public class JpegQuantizationTable
    {
        /// <summary>
        /// This field is used only during compression.  It's initialized false when
        /// the table is created, and set true when it's been output to the file.
        /// You could suppress output of a table by setting this to true.
        /// </summary>
        private bool m_sent_table;

        /// <summary>
        /// This array gives the coefficient quantizers in natural array order
        /// (not the zigzag order in which they are stored in a JPEG DQT marker).
        /// CAUTION: IJG versions prior to v6a kept this array in zigzag order.
        /// </summary>
        internal readonly short[] quantval = new short[JpegConstants.DCTSize2];

        internal JpegQuantizationTable()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the table has been output to file.
        /// </summary>
        /// <value>It's initialized <c>false</c> when the table is created, and set 
        /// <c>true</c> when it's been output to the file. You could suppress output of a table by setting this to <c>true</c>.
        /// </value>
        /// <remarks>This property is used only during compression.</remarks>
        /// <seealso cref="JpegCompressor.jpeg_suppress_tables"/>
        public bool Sent_table
        {
            get { return m_sent_table; }
            set { m_sent_table = value; }
        }
    }
    #endregion

    #region JpegScanInfo
    /// <summary>
    /// The script for encoding a multiple-scan file is an array of these:
    /// </summary>
    class JpegScanInfo
    {
        public int comps_in_scan;      /* number of components encoded in this scan */
        public int[] component_index = new int[JpegConstants.MaxComponentsInScan]; /* their SOF/comp_info[] indexes */
        public int Ss;
        public int Se;         /* progressive JPEG spectral selection parms */
        public int Ah;
        public int Al;         /* progressive JPEG successive approx. parms */
    }
    #endregion

    #region JpegSource
    /// <summary>
    /// Data source object for decompression.
    /// </summary>
    public abstract class Jpeg_Source
    {
        private byte[] m_next_input_byte;
        private int m_bytes_in_buffer; /* # of bytes remaining (unread) in buffer */
        private int m_position;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public abstract void init_source();

        /// <summary>
        /// Fills input buffer
        /// </summary>
        /// <returns><c>true</c> if operation succeed; otherwise, <c>false</c></returns>
        public abstract bool fill_input_buffer();

        /// <summary>
        /// Initializes the internal buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="size">The size.</param>
        protected void initInternalBuffer(byte[] buffer, int size)
        {
            m_bytes_in_buffer = size;
            m_next_input_byte = buffer;
            m_position = 0;
        }

        /// <summary>
        /// Skip data - used to skip over a potentially large amount of
        /// uninteresting data (such as an APPn marker).
        /// </summary>
        /// <param name="num_bytes">The number of bytes to skip.</param>
        /// <remarks>Writers of suspendable-input applications must note that skip_input_data
        /// is not granted the right to give a suspension return.  If the skip extends
        /// beyond the data currently in the buffer, the buffer can be marked empty so
        /// that the next read will cause a fill_input_buffer call that can suspend.
        /// Arranging for additional bytes to be discarded before reloading the input
        /// buffer is the application writer's problem.</remarks>
        public virtual void skip_input_data(int num_bytes)
        {
            /* Just a dumb implementation for now.  Could use fseek() except
            * it doesn't work on pipes.  Not clear that being smart is worth
            * any trouble anyway --- large skips are infrequent.
            */
            if (num_bytes > 0)
            {
                while (num_bytes > m_bytes_in_buffer)
                {
                    num_bytes -= m_bytes_in_buffer;
                    fill_input_buffer();
                    /* note we assume that fill_input_buffer will never return false,
                    * so suspension need not be handled.
                    */
                }

                m_position += num_bytes;
                m_bytes_in_buffer -= num_bytes;
            }
        }

        /// <summary>
        /// This is the default resync_to_restart method for data source 
        /// managers to use if they don't have any better approach.
        /// </summary>
        /// <param name="cinfo">An instance of <see cref="JpegDecompressor"/></param>
        /// <param name="desired">The desired</param>
        /// <returns><c>false</c> if suspension is required.</returns>
        /// <remarks>That method assumes that no backtracking is possible. 
        /// Some data source managers may be able to back up, or may have 
        /// additional knowledge about the data which permits a more 
        /// intelligent recovery strategy; such managers would
        /// presumably supply their own resync method.<br/><br/>
        /// 
        /// read_restart_marker calls resync_to_restart if it finds a marker other than
        /// the restart marker it was expecting.  (This code is *not* used unless
        /// a nonzero restart interval has been declared.)  cinfo.unread_marker is
        /// the marker code actually found (might be anything, except 0 or FF).
        /// The desired restart marker number (0..7) is passed as a parameter.<br/><br/>
        /// 
        /// This routine is supposed to apply whatever error recovery strategy seems
        /// appropriate in order to position the input stream to the next data segment.
        /// Note that cinfo.unread_marker is treated as a marker appearing before
        /// the current data-source input point; usually it should be reset to zero
        /// before returning.<br/><br/>
        /// 
        /// This implementation is substantially constrained by wanting to treat the
        /// input as a data stream; this means we can't back up.  Therefore, we have
        /// only the following actions to work with:<br/>
        /// 1. Simply discard the marker and let the entropy decoder resume at next
        /// byte of file.<br/>
        /// 2. Read forward until we find another marker, discarding intervening
        /// data.  (In theory we could look ahead within the current bufferload,
        /// without having to discard data if we don't find the desired marker.
        /// This idea is not implemented here, in part because it makes behavior
        /// dependent on buffer size and chance buffer-boundary positions.)<br/>
        /// 3. Leave the marker unread (by failing to zero cinfo.unread_marker).
        /// This will cause the entropy decoder to process an empty data segment,
        /// inserting dummy zeroes, and then we will reprocess the marker.<br/>
        /// 
        /// #2 is appropriate if we think the desired marker lies ahead, while #3 is
        /// appropriate if the found marker is a future restart marker (indicating
        /// that we have missed the desired restart marker, probably because it got
        /// corrupted).<br/>
        /// We apply #2 or #3 if the found marker is a restart marker no more than
        /// two counts behind or ahead of the expected one.  We also apply #2 if the
        /// found marker is not a legal JPEG marker code (it's certainly bogus data).
        /// If the found marker is a restart marker more than 2 counts away, we do #1
        /// (too much risk that the marker is erroneous; with luck we will be able to
        /// resync at some future point).<br/>
        /// For any valid non-restart JPEG marker, we apply #3.  This keeps us from
        /// overrunning the end of a scan.  An implementation limited to single-scan
        /// files might find it better to apply #2 for markers other than EOI, since
        /// any other marker would have to be bogus data in that case.</remarks>
        public virtual bool resync_to_restart(JpegDecompressor cinfo, int desired)
        {
            /* Outer loop handles repeated decision after scanning forward. */
            int action = 1;
            for (; ; )
            {
                if (cinfo.m_unread_marker < (int)JpegMarkerType.SOF0)
                {
                    /* invalid marker */
                    action = 2;
                }
                else if (cinfo.m_unread_marker < (int)JpegMarkerType.RST0 ||
                    cinfo.m_unread_marker > (int)JpegMarkerType.RST7)
                {
                    /* valid non-restart marker */
                    action = 3;
                }
                else
                {
                    if (cinfo.m_unread_marker == ((int)JpegMarkerType.RST0 + ((desired + 1) & 7))
                        || cinfo.m_unread_marker == ((int)JpegMarkerType.RST0 + ((desired + 2) & 7)))
                    {
                        /* one of the next two expected restarts */
                        action = 3;
                    }
                    else if (cinfo.m_unread_marker == ((int)JpegMarkerType.RST0 + ((desired - 1) & 7)) ||
                        cinfo.m_unread_marker == ((int)JpegMarkerType.RST0 + ((desired - 2) & 7)))
                    {
                        /* a prior restart, so advance */
                        action = 2;
                    }
                    else
                    {
                        /* desired restart or too far away */
                        action = 1;
                    }
                }

                switch (action)
                {
                    case 1:
                        /* Discard marker and let entropy decoder resume processing. */
                        cinfo.m_unread_marker = 0;
                        return true;
                    case 2:
                        /* Scan to the next marker, and repeat the decision loop. */
                        if (!cinfo.m_marker.next_marker())
                            return false;
                        break;
                    case 3:
                        /* Return without advancing past this marker. */
                        /* Entropy decoder will be forced to process an empty segment. */
                        return true;
                }
            }
        }

        /// <summary>
        /// Terminate source - called by jpeg_finish_decompress
        /// after all data has been read.  Often a no-op.
        /// </summary>
        /// <remarks>NB: <b>not</b> called by jpeg_abort or jpeg_destroy; surrounding
        /// application must deal with any cleanup that should happen even
        /// for error exit.</remarks>
        public virtual void term_source()
        {
        }

        /// <summary>
        /// Reads two bytes interpreted as an unsigned 16-bit integer.
        /// </summary>
        /// <param name="V">The result.</param>
        /// <returns><c>true</c> if operation succeed; otherwise, <c>false</c></returns>
        public virtual bool GetTwoBytes(out int V)
        {
            if (!MakeByteAvailable())
            {
                V = 0;
                return false;
            }

            m_bytes_in_buffer--;
            V = m_next_input_byte[m_position] << 8;
            m_position++;

            if (!MakeByteAvailable())
                return false;

            m_bytes_in_buffer--;
            V += m_next_input_byte[m_position];
            m_position++;
            return true;
        }

        /// <summary>
        /// Read a byte into variable V.
        /// If must suspend, take the specified action (typically "return false").
        /// </summary>
        /// <param name="V">The result.</param>
        /// <returns><c>true</c> if operation succeed; otherwise, <c>false</c></returns>
        public virtual bool GetByte(out int V)
        {
            if (!MakeByteAvailable())
            {
                V = 0;
                return false;
            }

            m_bytes_in_buffer--;
            V = m_next_input_byte[m_position];
            m_position++;
            return true;
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <param name="dest">The destination.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>The number of available bytes.</returns>
        public virtual int GetBytes(byte[] dest, int amount)
        {
            int avail = amount;
            if (avail > m_bytes_in_buffer)
                avail = m_bytes_in_buffer;

            for (int i = 0; i < avail; i++)
            {
                dest[i] = m_next_input_byte[m_position];
                m_position++;
                m_bytes_in_buffer--;
            }

            return avail;
        }

        /// <summary>
        /// Functions for fetching data from the data source module.
        /// </summary>
        /// <returns><c>true</c> if operation succeed; otherwise, <c>false</c></returns>
        /// <remarks>At all times, cinfo.src.next_input_byte and .bytes_in_buffer reflect
        /// the current restart point; we update them only when we have reached a
        /// suitable place to restart if a suspension occurs.</remarks>
        public virtual bool MakeByteAvailable()
        {
            if (m_bytes_in_buffer == 0)
            {
                if (!fill_input_buffer())
                    return false;
            }

            return true;
        }
    }
    #endregion

    #region JpegUpsampler
    /// <summary>
    /// Upsampling (note that upsampler must also call color converter)
    /// </summary>
    abstract class JpegUpsampler
    {
        protected bool m_need_context_rows; /* true if need rows above & below */

        public abstract void start_pass();
        public abstract void upsample(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail);

        public bool NeedContextRows()
        {
            return m_need_context_rows;
        }
    }
    #endregion

    #region JpegUtils
    class JpegUtils
    {
        /*
        * jpeg_natural_order[i] is the natural-order position of the i'th element
        * of zigzag order.
        *
        * When reading corrupted data, the Huffman decoders could attempt
        * to reference an entry beyond the end of this array (if the decoded
        * zero run length reaches past the end of the block).  To prevent
        * wild stores without adding an inner-loop test, we put some extra
        * "63"s after the real entries.  This will cause the extra coefficient
        * to be stored in location 63 of the block, not somewhere random.
        * The worst case would be a run-length of 15, which means we need 16
        * fake entries.
        */
        public static int[] jpeg_natural_order = 
        { 
            0, 1, 8, 16, 9, 2, 3, 10, 17, 24, 32, 25, 18, 11, 4, 5, 12,
            19, 26, 33, 40, 48, 41, 34, 27, 20, 13, 6, 7, 14, 21, 28, 35,
            42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62,
            63, 63, 63, 63, 63, 63, 63, 63, 63,
            /* extra entries for safety in decoder */
            63, 63, 63, 63, 63, 63, 63, 63 
        };

        /* We assume that right shift corresponds to signed division by 2 with
        * rounding towards minus infinity.  This is correct for typical "arithmetic
        * shift" instructions that shift in copies of the sign bit.
        * RIGHT_SHIFT provides a proper signed right shift of an int quantity.
        * It is only applied with constant shift counts.  SHIFT_TEMPS must be
        * included in the variables of any routine using RIGHT_SHIFT.
        */
        public static int RIGHT_SHIFT(int x, int shft)
        {
            return (x >> shft);
        }

        /* Descale and correctly round an int value that's scaled by N bits.
        * We assume RIGHT_SHIFT rounds towards minus infinity, so adding
        * the fudge factor is correct for either sign of X.
        */
        public static int DESCALE(int x, int n)
        {
            return RIGHT_SHIFT(x + (1 << (n - 1)), n);
        }

        //////////////////////////////////////////////////////////////////////////
        // Arithmetic utilities

        /// <summary>
        /// Compute a/b rounded up to next integer, ie, ceil(a/b)
        /// Assumes a >= 0, b > 0
        /// </summary>
        public static int jdiv_round_up(int a, int b)
        {
            return (a + b - 1) / b;
        }

        /// <summary>
        /// Compute a rounded up to next multiple of b, ie, ceil(a/b)*b
        /// Assumes a >= 0, b > 0
        /// </summary>
        public static int jround_up(int a, int b)
        {
            a += b - 1;
            return a - (a % b);
        }

        /// <summary>
        /// Copy some rows of samples from one place to another.
        /// num_rows rows are copied from input_array[source_row++]
        /// to output_array[dest_row++]; these areas may overlap for duplication.
        /// The source and destination arrays must be at least as wide as num_cols.
        /// </summary>
        public static void jcopy_sample_rows(ComponentBuffer input_array, int source_row, byte[][] output_array, int dest_row, int num_rows, int num_cols)
        {
            for (int row = 0; row < num_rows; row++)
                Buffer.BlockCopy(input_array[source_row + row], 0, output_array[dest_row + row], 0, num_cols);
        }

        public static void jcopy_sample_rows(ComponentBuffer input_array, int source_row, ComponentBuffer output_array, int dest_row, int num_rows, int num_cols)
        {
            for (int row = 0; row < num_rows; row++)
                Buffer.BlockCopy(input_array[source_row + row], 0, output_array[dest_row + row], 0, num_cols);
        }

        public static void jcopy_sample_rows(byte[][] input_array, int source_row, byte[][] output_array, int dest_row, int num_rows, int num_cols)
        {
            for (int row = 0; row < num_rows; row++)
                Buffer.BlockCopy(input_array[source_row++], 0, output_array[dest_row++], 0, num_cols);
        }
    }
    #endregion

    #region JpegVirtualArray
    /// <summary>
    /// JPEG virtual array.
    /// </summary>
    /// <typeparam name="T">The type of array's elements.</typeparam>
    /// <remarks>You can't create virtual array manually. For creation use methods
    /// <see cref="JpegCommonBase.CreateSamplesArray"/> and
    /// <see cref="JpegCommonBase.CreateBlocksArray"/>.
    /// </remarks>
    public class JpegVirtualArray<T>
    {
        internal delegate T[][] Allocator(int width, int height);

        private JpegCommonBase m_cinfo;

        /// <summary>
        /// the in-memory buffer
        /// </summary>
        private T[][] m_buffer;

        /// <summary>
        /// Request a virtual 2-D array
        /// </summary>
        /// <param name="width">Width of array</param>
        /// <param name="height">Total virtual array height</param>
        /// <param name="allocator">The allocator.</param>
        internal JpegVirtualArray(int width, int height, Allocator allocator)
        {
            m_cinfo = null;
            m_buffer = allocator(width, height);

            if (m_buffer == null)
                throw new Exception("Filling of 'm_buffer' Failed!");
        }

        /// <summary>
        /// Gets or sets the error processor.
        /// </summary>
        /// <value>The error processor.<br/>
        /// Default value: <c>null</c>
        /// </value>
        /// <remarks>Uses only for calling 
        /// <see cref="M:BitMiracle.LibJpeg.Classic.JpegCommonBase.ERREXIT(BitMiracle.LibJpeg.Classic.J_MESSAGE_CODE)">JpegCommonBase.ERREXIT</see>
        /// on error.</remarks>
        public JpegCommonBase ErrorProcessor
        {
            get { return m_cinfo; }
            set { m_cinfo = value; }
        }

        /// <summary>
        /// Access the part of a virtual array.
        /// </summary>
        /// <param name="startRow">The first row in required block.</param>
        /// <param name="numberOfRows">The number of required rows.</param>
        /// <returns>The required part of virtual array.</returns>
        public T[][] Access(int startRow, int numberOfRows)
        {
            /* debugging check */
            if (startRow + numberOfRows > m_buffer.Length)
            {
                throw new InvalidOperationException("Bogus virtual array access");
            }

            /* Return proper part of the buffer */
            T[][] ret = new T[numberOfRows][];
            for (int i = 0; i < numberOfRows; i++)
                ret[i] = m_buffer[startRow + i];

            return ret;
        }
    }
    #endregion

    #region MergedUpsampler
    class MergedUpsampler : JpegUpsampler
    {
        private const int SCALEBITS = 16;  /* speediest right-shift on some machines */
        private const int ONE_HALF = 1 << (SCALEBITS - 1);

        private JpegDecompressor m_cinfo;

        private bool m_use_2v_upsample;

        /* Private state for YCC->RGB conversion */
        private int[] m_Cr_r_tab;      /* => table for Cr to R conversion */
        private int[] m_Cb_b_tab;      /* => table for Cb to B conversion */
        private int[] m_Cr_g_tab;        /* => table for Cr to G conversion */
        private int[] m_Cb_g_tab;        /* => table for Cb to G conversion */

        /* For 2:1 vertical sampling, we produce two output rows at a time.
        * We need a "spare" row buffer to hold the second output row if the
        * application provides just a one-row buffer; we also use the spare
        * to discard the dummy last row if the image height is odd.
        */
        private byte[] m_spare_row;
        private bool m_spare_full;        /* T if spare buffer is occupied */

        private int m_out_row_width;   /* samples per output row */
        private int m_rows_to_go;  /* counts rows remaining in image */

        public MergedUpsampler(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;
            m_need_context_rows = false;

            m_out_row_width = cinfo.m_output_width * cinfo.m_out_color_components;

            if (cinfo.m_max_v_samp_factor == 2)
            {
                m_use_2v_upsample = true;
                /* Allocate a spare row buffer */
                m_spare_row = new byte[m_out_row_width];
            }
            else
            {
                m_use_2v_upsample = false;
            }

            build_ycc_rgb_table();
        }

        /// <summary>
        /// Initialize for an upsampling pass.
        /// </summary>
        public override void start_pass()
        {
            /* Mark the spare buffer empty */
            m_spare_full = false;

            /* Initialize total-height counter for detecting bottom of image */
            m_rows_to_go = m_cinfo.m_output_height;
        }

        public override void upsample(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            if (m_use_2v_upsample)
                merged_2v_upsample(input_buf, ref in_row_group_ctr, output_buf, ref out_row_ctr, out_rows_avail);
            else
                merged_1v_upsample(input_buf, ref in_row_group_ctr, output_buf, ref out_row_ctr);
        }

        /// <summary>
        /// Control routine to do upsampling (and color conversion).
        /// The control routine just handles the row buffering considerations.
        /// 1:1 vertical sampling case: much easier, never need a spare row.
        /// </summary>
        private void merged_1v_upsample(ComponentBuffer[] input_buf, ref int in_row_group_ctr, byte[][] output_buf, ref int out_row_ctr)
        {
            /* Just do the upsampling. */
            h2v1_merged_upsample(input_buf, in_row_group_ctr, output_buf, out_row_ctr);

            /* Adjust counts */
            out_row_ctr++;
            in_row_group_ctr++;
        }

        /// <summary>
        /// Control routine to do upsampling (and color conversion).
        /// The control routine just handles the row buffering considerations.
        /// 2:1 vertical sampling case: may need a spare row.
        /// </summary>
        private void merged_2v_upsample(ComponentBuffer[] input_buf, ref int in_row_group_ctr, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            int num_rows;        /* number of rows returned to caller */
            if (m_spare_full)
            {
                /* If we have a spare row saved from a previous cycle, just return it. */
                byte[][] temp = new byte[1][];
                temp[0] = m_spare_row;
                JpegUtils.jcopy_sample_rows(temp, 0, output_buf, out_row_ctr, 1, m_out_row_width);
                num_rows = 1;
                m_spare_full = false;
            }
            else
            {
                /* Figure number of rows to return to caller. */
                num_rows = 2;

                /* Not more than the distance to the end of the image. */
                if (num_rows > m_rows_to_go)
                    num_rows = m_rows_to_go;

                /* And not more than what the client can accept: */
                out_rows_avail -= out_row_ctr;
                if (num_rows > out_rows_avail)
                    num_rows = out_rows_avail;

                /* Create output pointer array for upsampler. */
                byte[][] work_ptrs = new byte[2][];
                work_ptrs[0] = output_buf[out_row_ctr];
                if (num_rows > 1)
                {
                    work_ptrs[1] = output_buf[out_row_ctr + 1];
                }
                else
                {
                    work_ptrs[1] = m_spare_row;
                    m_spare_full = true;
                }

                /* Now do the upsampling. */
                h2v2_merged_upsample(input_buf, in_row_group_ctr, work_ptrs);
            }

            /* Adjust counts */
            out_row_ctr += num_rows;
            m_rows_to_go -= num_rows;

            /* When the buffer is emptied, declare this input row group consumed */
            if (!m_spare_full)
                in_row_group_ctr++;
        }

        /*
         * These are the routines invoked by the control routines to do
         * the actual upsampling/conversion.  One row group is processed per call.
         *
         * Note: since we may be writing directly into application-supplied buffers,
         * we have to be honest about the output width; we can't assume the buffer
         * has been rounded up to an even width.
         */

        /// <summary>
        /// Upsample and color convert for the case of 2:1 horizontal and 1:1 vertical.
        /// </summary>
        private void h2v1_merged_upsample(ComponentBuffer[] input_buf, int in_row_group_ctr, byte[][] output_buf, int outRow)
        {
            int inputIndex0 = 0;
            int inputIndex1 = 0;
            int inputIndex2 = 0;
            int outputIndex = 0;

            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset;

            /* Loop for each pair of output pixels */
            for (int col = m_cinfo.m_output_width >> 1; col > 0; col--)
            {
                /* Do the chroma part of the calculation */
                int cb = input_buf[1][in_row_group_ctr][inputIndex1];
                inputIndex1++;

                int cr = input_buf[2][in_row_group_ctr][inputIndex2];
                inputIndex2++;

                int cred = m_Cr_r_tab[cr];
                int cgreen = JpegUtils.RIGHT_SHIFT(m_Cb_g_tab[cb] + m_Cr_g_tab[cr], SCALEBITS);
                int cblue = m_Cb_b_tab[cb];

                /* Fetch 2 Y values and emit 2 pixels */
                int y = input_buf[0][in_row_group_ctr][inputIndex0];
                inputIndex0++;

                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
                outputIndex += JpegConstants.RGB_PixelLength;

                y = input_buf[0][in_row_group_ctr][inputIndex0];
                inputIndex0++;

                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
                outputIndex += JpegConstants.RGB_PixelLength;
            }

            /* If image width is odd, do the last output column separately */
            if ((m_cinfo.m_output_width & 1) != 0)
            {
                int cb = input_buf[1][in_row_group_ctr][inputIndex1];
                int cr = input_buf[2][in_row_group_ctr][inputIndex2];
                int cred = m_Cr_r_tab[cr];
                int cgreen = JpegUtils.RIGHT_SHIFT(m_Cb_g_tab[cb] + m_Cr_g_tab[cr], SCALEBITS);
                int cblue = m_Cb_b_tab[cb];

                int y = input_buf[0][in_row_group_ctr][inputIndex0];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[outRow][outputIndex + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
            }
        }

        /// <summary>
        /// Upsample and color convert for the case of 2:1 horizontal and 2:1 vertical.
        /// </summary>
        private void h2v2_merged_upsample(ComponentBuffer[] input_buf, int in_row_group_ctr, byte[][] output_buf)
        {
            int inputRow00 = in_row_group_ctr * 2;
            int inputIndex00 = 0;

            int inputRow01 = in_row_group_ctr * 2 + 1;
            int inputIndex01 = 0;

            int inputIndex1 = 0;
            int inputIndex2 = 0;

            int outIndex0 = 0;
            int outIndex1 = 0;

            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset;

            /* Loop for each group of output pixels */
            for (int col = m_cinfo.m_output_width >> 1; col > 0; col--)
            {
                /* Do the chroma part of the calculation */
                int cb = input_buf[1][in_row_group_ctr][inputIndex1];
                inputIndex1++;

                int cr = input_buf[2][in_row_group_ctr][inputIndex2];
                inputIndex2++;

                int cred = m_Cr_r_tab[cr];
                int cgreen = JpegUtils.RIGHT_SHIFT(m_Cb_g_tab[cb] + m_Cr_g_tab[cr], SCALEBITS);
                int cblue = m_Cb_b_tab[cb];

                /* Fetch 4 Y values and emit 4 pixels */
                int y = input_buf[0][inputRow00][inputIndex00];
                inputIndex00++;

                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
                outIndex0 += JpegConstants.RGB_PixelLength;

                y = input_buf[0][inputRow00][inputIndex00];
                inputIndex00++;

                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
                outIndex0 += JpegConstants.RGB_PixelLength;

                y = input_buf[0][inputRow01][inputIndex01];
                inputIndex01++;

                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
                outIndex1 += JpegConstants.RGB_PixelLength;

                y = input_buf[0][inputRow01][inputIndex01];
                inputIndex01++;

                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
                outIndex1 += JpegConstants.RGB_PixelLength;
            }

            /* If image width is odd, do the last output column separately */
            if ((m_cinfo.m_output_width & 1) != 0)
            {
                int cb = input_buf[1][in_row_group_ctr][inputIndex1];
                int cr = input_buf[2][in_row_group_ctr][inputIndex2];
                int cred = m_Cr_r_tab[cr];
                int cgreen = JpegUtils.RIGHT_SHIFT(m_Cb_g_tab[cb] + m_Cr_g_tab[cr], SCALEBITS);
                int cblue = m_Cb_b_tab[cb];

                int y = input_buf[0][inputRow00][inputIndex00];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[0][outIndex0 + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];

                y = input_buf[0][inputRow01][inputIndex01];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Red] = limit[limitOffset + y + cred];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Green] = limit[limitOffset + y + cgreen];
                output_buf[1][outIndex1 + JpegConstants.Offset_RGB_Blue] = limit[limitOffset + y + cblue];
            }
        }

        /// <summary>
        /// Initialize tables for YCC->RGB colorspace conversion.
        /// This is taken directly from ColorDeconverter; see that file for more info.
        /// </summary>
        private void build_ycc_rgb_table()
        {
            m_Cr_r_tab = new int[JpegConstants.MaxSampleValue + 1];
            m_Cb_b_tab = new int[JpegConstants.MaxSampleValue + 1];
            m_Cr_g_tab = new int[JpegConstants.MaxSampleValue + 1];
            m_Cb_g_tab = new int[JpegConstants.MaxSampleValue + 1];

            for (int i = 0, x = -JpegConstants.MediumSampleValue; i <= JpegConstants.MaxSampleValue; i++, x++)
            {
                /* i is the actual input pixel value, in the range 0..MaxSampleValue */
                /* The Cb or Cr value we are thinking of is x = i - MediumSampleValue */
                /* Cr=>R value is nearest int to 1.40200 * x */
                m_Cr_r_tab[i] = JpegUtils.RIGHT_SHIFT(FIX(1.40200) * x + ONE_HALF, SCALEBITS);

                /* Cb=>B value is nearest int to 1.77200 * x */
                m_Cb_b_tab[i] = JpegUtils.RIGHT_SHIFT(FIX(1.77200) * x + ONE_HALF, SCALEBITS);

                /* Cr=>G value is scaled-up -0.71414 * x */
                m_Cr_g_tab[i] = (-FIX(0.71414)) * x;

                /* Cb=>G value is scaled-up -0.34414 * x */
                /* We also add in ONE_HALF so that need not do it in inner loop */
                m_Cb_g_tab[i] = (-FIX(0.34414)) * x + ONE_HALF;
            }
        }

        private static int FIX(double x)
        {
            return ((int)((x) * (1L << SCALEBITS) + 0.5));
        }
    }
    #endregion

    #region Pass1ColorQuantizer
    /// <summary>
    /// The main purpose of 1-pass quantization is to provide a fast, if not very
    /// high quality, colormapped output capability.  A 2-pass quantizer usually
    /// gives better visual quality; however, for quantized grayscale output this
    /// quantizer is perfectly adequate.  Dithering is highly recommended with this
    /// quantizer, though you can turn it off if you really want to.
    /// 
    /// In 1-pass quantization the colormap must be chosen in advance of seeing the
    /// image.  We use a map consisting of all combinations of Ncolors[i] color
    /// values for the i'th component.  The Ncolors[] values are chosen so that
    /// their product, the total number of colors, is no more than that requested.
    /// (In most cases, the product will be somewhat less.)
    /// 
    /// Since the colormap is orthogonal, the representative value for each color
    /// component can be determined without considering the other components;
    /// then these indexes can be combined into a colormap index by a standard
    /// N-dimensional-array-subscript calculation.  Most of the arithmetic involved
    /// can be precalculated and stored in the lookup table colorindex[].
    /// colorindex[i][j] maps pixel value j in component i to the nearest
    /// representative value (grid plane) for that component; this index is
    /// multiplied by the array stride for component i, so that the
    /// index of the colormap entry closest to a given pixel value is just
    ///     sum( colorindex[component-number][pixel-component-value] )
    /// Aside from being fast, this scheme allows for variable spacing between
    /// representative values with no additional lookup cost.
    /// 
    /// If gamma correction has been applied in color conversion, it might be wise
    /// to adjust the color grid spacing so that the representative colors are
    /// equidistant in linear space.  At this writing, gamma correction is not
    /// implemented, so nothing is done here.
    /// 
    /// 
    /// Declarations for Floyd-Steinberg dithering.
    /// 
    /// Errors are accumulated into the array fserrors[], at a resolution of
    /// 1/16th of a pixel count.  The error at a given pixel is propagated
    /// to its not-yet-processed neighbors using the standard F-S fractions,
    ///     ...	(here)	7/16
    ///    3/16	5/16	1/16
    /// We work left-to-right on even rows, right-to-left on odd rows.
    /// 
    /// We can get away with a single array (holding one row's worth of errors)
    /// by using it to store the current row's errors at pixel columns not yet
    /// processed, but the next row's errors at columns already processed.  We
    /// need only a few extra variables to hold the errors immediately around the
    /// current column.  (If we are lucky, those variables are in registers, but
    /// even if not, they're probably cheaper to access than array elements are.)
    /// 
    /// The fserrors[] array is indexed [component#][position].
    /// We provide (#columns + 2) entries per component; the extra entry at each
    /// end saves us from special-casing the first and last pixels.
    /// 
    /// 
    /// Declarations for ordered dithering.
    /// 
    /// We use a standard 16x16 ordered dither array.  The basic concept of ordered
    /// dithering is described in many references, for instance Dale Schumacher's
    /// chapter II.2 of Graphics Gems II (James Arvo, ed. Academic Press, 1991).
    /// In place of Schumacher's comparisons against a "threshold" value, we add a
    /// "dither" value to the input pixel and then round the result to the nearest
    /// output value.  The dither value is equivalent to (0.5 - threshold) times
    /// the distance between output values.  For ordered dithering, we assume that
    /// the output colors are equally spaced; if not, results will probably be
    /// worse, since the dither may be too much or too little at a given point.
    /// 
    /// The normal calculation would be to form pixel value + dither, range-limit
    /// this to 0..MaxSampleValue, and then index into the colorindex table as usual.
    /// We can skip the separate range-limiting step by extending the colorindex
    /// table in both directions.
    /// </summary>
    class Pass1ColorQuantizer : ColorQuantizer
    {
        private enum QuantizerType
        {
            color_quantizer3,
            color_quantizer,
            quantize3_ord_dither_quantizer,
            quantize_ord_dither_quantizer,
            quantize_fs_dither_quantizer
        }

        private static int[] RGB_order = { JpegConstants.Offset_RGB_Green, JpegConstants.Offset_RGB_Red, JpegConstants.Offset_RGB_Blue };
        private const int MAX_Q_COMPS = 4; /* max components I can handle */

        private const int ODITHER_SIZE = 16; /* dimension of dither matrix */

        /* NB: if ODITHER_SIZE is not a power of 2, ODITHER_MASK uses will break */
        private const int ODITHER_CELLS = (ODITHER_SIZE * ODITHER_SIZE); /* # cells in matrix */
        private const int ODITHER_MASK = (ODITHER_SIZE - 1); /* mask for wrapping around counters */

        /* Bayer's order-4 dither array.  Generated by the code given in
        * Stephen Hawley's article "Ordered Dithering" in Graphics Gems I.
        * The values in this array must range from 0 to ODITHER_CELLS-1.
        */
        private static byte[][] base_dither_matrix = new byte[][] 
        {
            new byte[] {   0,192, 48,240, 12,204, 60,252,  3,195, 51,243, 15,207, 63,255 },
            new byte[] { 128, 64,176,112,140, 76,188,124,131, 67,179,115,143, 79,191,127 },
            new byte[] {  32,224, 16,208, 44,236, 28,220, 35,227, 19,211, 47,239, 31,223 },
            new byte[] { 160, 96,144, 80,172,108,156, 92,163, 99,147, 83,175,111,159, 95 },
            new byte[] {   8,200, 56,248,  4,196, 52,244, 11,203, 59,251,  7,199, 55,247 },
            new byte[] { 136, 72,184,120,132, 68,180,116,139, 75,187,123,135, 71,183,119 },
            new byte[] {  40,232, 24,216, 36,228, 20,212, 43,235, 27,219, 39,231, 23,215 },
            new byte[] { 168,104,152, 88,164,100,148, 84,171,107,155, 91,167,103,151, 87 },
            new byte[] {   2,194, 50,242, 14,206, 62,254,  1,193, 49,241, 13,205, 61,253 },
            new byte[] { 130, 66,178,114,142, 78,190,126,129, 65,177,113,141, 77,189,125 },
            new byte[] {  34,226, 18,210, 46,238, 30,222, 33,225, 17,209, 45,237, 29,221 },
            new byte[] { 162, 98,146, 82,174,110,158, 94,161, 97,145, 81,173,109,157, 93 },
            new byte[] {  10,202, 58,250,  6,198, 54,246,  9,201, 57,249,  5,197, 53,245 },
            new byte[] { 138, 74,186,122,134, 70,182,118,137, 73,185,121,133, 69,181,117 },
            new byte[] {  42,234, 26,218, 38,230, 22,214, 41,233, 25,217, 37,229, 21,213 },
            new byte[] { 170,106,154, 90,166,102,150, 86,169,105,153, 89,165,101,149, 85 }
        };

        private QuantizerType m_quantizer;

        private JpegDecompressor m_cinfo;

        /* Initially allocated colormap is saved here */
        private byte[][] m_sv_colormap;	/* The color map as a 2-D pixel array */
        private int m_sv_actual;		/* number of entries in use */

        private byte[][] m_colorindex;	/* Precomputed mapping for speed */
        private int[] m_colorindexOffset;

        /* colorindex[i][j] = index of color closest to pixel value j in component i,
        * premultiplied as described above.  Since colormap indexes must fit into
        * bytes, the entries of this array will too.
        */
        private bool m_is_padded;		/* is the colorindex padded for odither? */

        private int[] m_Ncolors = new int[MAX_Q_COMPS];	/* # of values alloced to each component */

        /* Variables for ordered dithering */
        private int m_row_index;		/* cur row's vertical index in dither matrix */
        private int[][][] m_odither = new int[MAX_Q_COMPS][][]; /* one dither array per component */

        /* Variables for Floyd-Steinberg dithering */
        private short[][] m_fserrors = new short[MAX_Q_COMPS][]; /* accumulated errors */
        private bool m_on_odd_row;		/* flag to remember which row we are on */

        /// <summary>
        /// Module initialization routine for 1-pass color quantization.
        /// </summary>
        /// <param name="cinfo">The cinfo.</param>
        public Pass1ColorQuantizer(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            m_fserrors[0] = null; /* Flag FS workspace not allocated */
            m_odither[0] = null;    /* Also flag odither arrays not allocated */

            /* Make sure my internal arrays won't overflow */
            if (cinfo.m_out_color_components > MAX_Q_COMPS)
                throw new Exception(String.Format("Cannot quantize more than {0} color components", MAX_Q_COMPS));

            /* Make sure colormap indexes can be represented by JSAMPLEs */
            if (cinfo.m_desired_number_of_colors > (JpegConstants.MaxSampleValue + 1))
                throw new Exception(String.Format("Cannot quantize to more than {0} colors", JpegConstants.MaxSampleValue + 1));

            /* Create the colormap and color index table. */
            create_colormap();
            create_colorindex();

            /* Allocate Floyd-Steinberg workspace now if requested.
            * We do this now since it is FAR storage and may affect the memory
            * manager's space calculations.  If the user changes to FS dither
            * mode in a later pass, we will allocate the space then, and will
            * possibly overrun the max_memory_to_use setting.
            */
            if (cinfo.m_dither_mode == DitherMode.FloydStein)
                alloc_fs_workspace();
        }

        /// <summary>
        /// Initialize for one-pass color quantization.
        /// </summary>
        public virtual void start_pass(bool is_pre_scan)
        {
            /* Install my colormap. */
            m_cinfo.m_colormap = m_sv_colormap;
            m_cinfo.m_actual_number_of_colors = m_sv_actual;

            /* Initialize for desired dithering mode. */
            switch (m_cinfo.m_dither_mode)
            {
                case DitherMode.None:
                    if (m_cinfo.m_out_color_components == 3)
                        m_quantizer = QuantizerType.color_quantizer3;
                    else
                        m_quantizer = QuantizerType.color_quantizer;

                    break;
                case DitherMode.Ordered:
                    if (m_cinfo.m_out_color_components == 3)
                        m_quantizer = QuantizerType.quantize3_ord_dither_quantizer;
                    else
                        m_quantizer = QuantizerType.quantize3_ord_dither_quantizer;

                    /* initialize state for ordered dither */
                    m_row_index = 0;

                    /* If user changed to ordered dither from another mode,
                     * we must recreate the color index table with padding.
                     * This will cost extra space, but probably isn't very likely.
                     */
                    if (!m_is_padded)
                        create_colorindex();

                    /* Create ordered-dither tables if we didn't already. */
                    if (m_odither[0] == null)
                        create_odither_tables();

                    break;
                case DitherMode.FloydStein:
                    m_quantizer = QuantizerType.quantize_fs_dither_quantizer;

                    /* initialize state for F-S dither */
                    m_on_odd_row = false;

                    /* Allocate Floyd-Steinberg workspace if didn't already. */
                    if (m_fserrors[0] == null)
                        alloc_fs_workspace();

                    /* Initialize the propagated errors to zero. */
                    int arraysize = m_cinfo.m_output_width + 2;
                    for (int i = 0; i < m_cinfo.m_out_color_components; i++)
                        Array.Clear(m_fserrors[i], 0, arraysize);

                    break;
                default:
                    throw new Exception("Unknown Dither Mode");
            }
        }

        public virtual void color_quantize(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            switch (m_quantizer)
            {
                case QuantizerType.color_quantizer3:
                    quantize3(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                case QuantizerType.color_quantizer:
                    quantize(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                case QuantizerType.quantize3_ord_dither_quantizer:
                    quantize3_ord_dither(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                case QuantizerType.quantize_ord_dither_quantizer:
                    quantize_ord_dither(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                case QuantizerType.quantize_fs_dither_quantizer:
                    quantize_fs_dither(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                default:
                    throw new Exception("Not implemented yet");
            }
        }

        /// <summary>
        /// Finish up at the end of the pass.
        /// </summary>
        public virtual void finish_pass()
        {
            /* no work in 1-pass case */
        }

        /// <summary>
        /// Switch to a new external colormap between output passes.
        /// Shouldn't get to this!
        /// </summary>
        public virtual void new_color_map()
        {
            throw new Exception("Invalid mode change during color quantization");
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// General case, no dithering.
        /// </summary>
        private void quantize(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            int nc = m_cinfo.m_out_color_components;

            for (int row = 0; row < num_rows; row++)
            {
                int inIndex = 0;
                int inRow = in_row + row;

                int outIndex = 0;
                int outRow = out_row + row;

                for (int col = m_cinfo.m_output_width; col > 0; col--)
                {
                    int pixcode = 0;
                    for (int ci = 0; ci < nc; ci++)
                    {
                        pixcode += m_colorindex[ci][m_colorindexOffset[ci] + input_buf[inRow][inIndex]];
                        inIndex++;
                    }

                    output_buf[outRow][outIndex] = (byte)pixcode;
                    outIndex++;
                }
            }
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// Fast path for out_color_components==3, no dithering
        /// </summary>
        private void quantize3(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            int width = m_cinfo.m_output_width;

            for (int row = 0; row < num_rows; row++)
            {
                int inIndex = 0;
                int inRow = in_row + row;

                int outIndex = 0;
                int outRow = out_row + row;

                for (int col = width; col > 0; col--)
                {
                    int pixcode = m_colorindex[0][m_colorindexOffset[0] + input_buf[inRow][inIndex]];
                    inIndex++;

                    pixcode += m_colorindex[1][m_colorindexOffset[1] + input_buf[inRow][inIndex]];
                    inIndex++;

                    pixcode += m_colorindex[2][m_colorindexOffset[2] + input_buf[inRow][inIndex]];
                    inIndex++;

                    output_buf[outRow][outIndex] = (byte)pixcode;
                    outIndex++;
                }
            }
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// General case, with ordered dithering.
        /// </summary>
        private void quantize_ord_dither(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            int nc = m_cinfo.m_out_color_components;
            int width = m_cinfo.m_output_width;

            for (int row = 0; row < num_rows; row++)
            {
                /* Initialize output values to 0 so can process components separately */
                Array.Clear(output_buf[out_row + row], 0, width);

                int row_index = m_row_index;
                for (int ci = 0; ci < nc; ci++)
                {
                    int inputIndex = ci;
                    int outIndex = 0;
                    int outRow = out_row + row;

                    int col_index = 0;
                    for (int col = width; col > 0; col--)
                    {
                        /* Form pixel value + dither, range-limit to 0..MaxSampleValue,
                         * select output value, accumulate into output code for this pixel.
                         * Range-limiting need not be done explicitly, as we have extended
                         * the colorindex table to produce the right answers for out-of-range
                         * inputs.  The maximum dither is +- MaxSampleValue; this sets the
                         * required amount of padding.
                         */
                        output_buf[outRow][outIndex] += m_colorindex[ci][m_colorindexOffset[ci] + input_buf[in_row + row][inputIndex] + m_odither[ci][row_index][col_index]];
                        inputIndex += nc;
                        outIndex++;
                        col_index = (col_index + 1) & ODITHER_MASK;
                    }
                }

                /* Advance row index for next row */
                row_index = (row_index + 1) & ODITHER_MASK;
                m_row_index = row_index;
            }
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// Fast path for out_color_components==3, with ordered dithering
        /// </summary>
        private void quantize3_ord_dither(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            int width = m_cinfo.m_output_width;

            for (int row = 0; row < num_rows; row++)
            {
                int row_index = m_row_index;
                int inRow = in_row + row;
                int inIndex = 0;

                int outIndex = 0;
                int outRow = out_row + row;

                int col_index = 0;
                for (int col = width; col > 0; col--)
                {
                    int pixcode = m_colorindex[0][m_colorindexOffset[0] + input_buf[inRow][inIndex] + m_odither[0][row_index][col_index]];
                    inIndex++;

                    pixcode += m_colorindex[1][m_colorindexOffset[1] + input_buf[inRow][inIndex] + m_odither[1][row_index][col_index]];
                    inIndex++;

                    pixcode += m_colorindex[2][m_colorindexOffset[2] + input_buf[inRow][inIndex] + m_odither[2][row_index][col_index]];
                    inIndex++;

                    output_buf[outRow][outIndex] = (byte)pixcode;
                    outIndex++;

                    col_index = (col_index + 1) & ODITHER_MASK;
                }

                row_index = (row_index + 1) & ODITHER_MASK;
                m_row_index = row_index;
            }
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// General case, with Floyd-Steinberg dithering
        /// </summary>
        private void quantize_fs_dither(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            int nc = m_cinfo.m_out_color_components;
            int width = m_cinfo.m_output_width;

            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset;

            for (int row = 0; row < num_rows; row++)
            {
                /* Initialize output values to 0 so can process components separately */
                Array.Clear(output_buf[out_row + row], 0, width);

                for (int ci = 0; ci < nc; ci++)
                {
                    int inRow = in_row + row;
                    int inIndex = ci;

                    int outIndex = 0;
                    int outRow = out_row + row;

                    int errorIndex = 0;
                    int dir;            /* 1 for left-to-right, -1 for right-to-left */
                    if (m_on_odd_row)
                    {
                        /* work right to left in this row */
                        inIndex += (width - 1) * nc; /* so point to rightmost pixel */
                        outIndex += width - 1;
                        dir = -1;
                        errorIndex = width + 1; /* => entry after last column */
                    }
                    else
                    {
                        /* work left to right in this row */
                        dir = 1;
                        errorIndex = 0; /* => entry before first column */
                    }
                    int dirnc = dir * nc;

                    /* Preset error values: no error propagated to first pixel from left */
                    int cur = 0;
                    /* and no error propagated to row below yet */
                    int belowerr = 0;
                    int bpreverr = 0;

                    for (int col = width; col > 0; col--)
                    {
                        /* cur holds the error propagated from the previous pixel on the
                         * current line.  Add the error propagated from the previous line
                         * to form the complete error correction term for this pixel, and
                         * round the error term (which is expressed * 16) to an integer.
                         * RIGHT_SHIFT rounds towards minus infinity, so adding 8 is correct
                         * for either sign of the error value.
                         * Note: errorIndex is for *previous* column's array entry.
                         */
                        cur = JpegUtils.RIGHT_SHIFT(cur + m_fserrors[ci][errorIndex + dir] + 8, 4);

                        /* Form pixel value + error, and range-limit to 0..MaxSampleValue.
                         * The maximum error is +- MaxSampleValue; this sets the required size
                         * of the range_limit array.
                         */
                        cur += input_buf[inRow][inIndex];
                        cur = limit[limitOffset + cur];

                        /* Select output value, accumulate into output code for this pixel */
                        int pixcode = m_colorindex[ci][m_colorindexOffset[ci] + cur];
                        output_buf[outRow][outIndex] += (byte)pixcode;

                        /* Compute actual representation error at this pixel */
                        /* Note: we can do this even though we don't have the final */
                        /* pixel code, because the colormap is orthogonal. */
                        cur -= m_sv_colormap[ci][pixcode];

                        /* Compute error fractions to be propagated to adjacent pixels.
                         * Add these into the running sums, and simultaneously shift the
                         * next-line error sums left by 1 column.
                         */
                        int bnexterr = cur;
                        int delta = cur * 2;
                        cur += delta;       /* form error * 3 */
                        m_fserrors[ci][errorIndex + 0] = (short)(bpreverr + cur);
                        cur += delta;       /* form error * 5 */
                        bpreverr = belowerr + cur;
                        belowerr = bnexterr;
                        cur += delta;       /* form error * 7 */

                        /* At this point cur contains the 7/16 error value to be propagated
                         * to the next pixel on the current line, and all the errors for the
                         * next line have been shifted over. We are therefore ready to move on.
                         */
                        inIndex += dirnc; /* advance input to next column */
                        outIndex += dir;  /* advance output to next column */
                        errorIndex += dir;    /* advance errorIndex to current column */
                    }

                    /* Post-loop cleanup: we must unload the final error value into the
                     * final fserrors[] entry.  Note we need not unload belowerr because
                     * it is for the dummy column before or after the actual array.
                     */
                    m_fserrors[ci][errorIndex + 0] = (short)bpreverr; /* unload prev err into array */
                }

                m_on_odd_row = (m_on_odd_row ? false : true);
            }
        }

        /// <summary>
        /// Create the colormap.
        /// </summary>
        private void create_colormap()
        {
            /* Select number of colors for each component */
            int total_colors = select_ncolors(m_Ncolors);

            /* Allocate and fill in the colormap. */
            /* The colors are ordered in the map in standard row-major order, */
            /* i.e. rightmost (highest-indexed) color changes most rapidly. */
            byte[][] colormap = JpegCommonBase.AllocJpegSamples(total_colors, m_cinfo.m_out_color_components);

            /* blksize is number of adjacent repeated entries for a component */
            /* blkdist is distance between groups of identical entries for a component */
            int blkdist = total_colors;
            for (int i = 0; i < m_cinfo.m_out_color_components; i++)
            {
                /* fill in colormap entries for i'th color component */
                int nci = m_Ncolors[i]; /* # of distinct values for this color */
                int blksize = blkdist / nci;
                for (int j = 0; j < nci; j++)
                {
                    /* Compute j'th output value (out of nci) for component */
                    int val = output_value(j, nci - 1);

                    /* Fill in all colormap entries that have this value of this component */
                    for (int ptr = j * blksize; ptr < total_colors; ptr += blkdist)
                    {
                        /* fill in blksize entries beginning at ptr */
                        for (int k = 0; k < blksize; k++)
                            colormap[i][ptr + k] = (byte)val;
                    }
                }

                /* blksize of this color is blkdist of next */
                blkdist = blksize;
            }

            /* Save the colormap in private storage,
             * where it will survive color quantization mode changes.
             */
            m_sv_colormap = colormap;
            m_sv_actual = total_colors;
        }

        /// <summary>
        /// Create the color index table.
        /// </summary>
        private void create_colorindex()
        {
            /* For ordered dither, we pad the color index tables by MaxSampleValue in
             * each direction (input index values can be -MaxSampleValue .. 2*MaxSampleValue).
             * This is not necessary in the other dithering modes.  However, we
             * flag whether it was done in case user changes dithering mode.
             */
            int pad;
            if (m_cinfo.m_dither_mode == DitherMode.Ordered)
            {
                pad = JpegConstants.MaxSampleValue * 2;
                m_is_padded = true;
            }
            else
            {
                pad = 0;
                m_is_padded = false;
            }

            m_colorindex = JpegCommonBase.AllocJpegSamples(JpegConstants.MaxSampleValue + 1 + pad, m_cinfo.m_out_color_components);
            m_colorindexOffset = new int[m_cinfo.m_out_color_components];

            /* blksize is number of adjacent repeated entries for a component */
            int blksize = m_sv_actual;
            for (int i = 0; i < m_cinfo.m_out_color_components; i++)
            {
                /* fill in colorindex entries for i'th color component */
                int nci = m_Ncolors[i]; /* # of distinct values for this color */
                blksize = blksize / nci;

                /* adjust colorindex pointers to provide padding at negative indexes. */
                if (pad != 0)
                    m_colorindexOffset[i] += JpegConstants.MaxSampleValue;

                /* in loop, val = index of current output value, */
                /* and k = largest j that maps to current val */
                int val = 0;
                int k = largest_input_value(0, nci - 1);
                for (int j = 0; j <= JpegConstants.MaxSampleValue; j++)
                {
                    while (j > k)
                    {
                        /* advance val if past boundary */
                        k = largest_input_value(++val, nci - 1);
                    }

                    /* premultiply so that no multiplication needed in main processing */
                    m_colorindex[i][m_colorindexOffset[i] + j] = (byte)(val * blksize);
                }

                /* Pad at both ends if necessary */
                if (pad != 0)
                {
                    for (int j = 1; j <= JpegConstants.MaxSampleValue; j++)
                    {
                        m_colorindex[i][m_colorindexOffset[i] + -j] = m_colorindex[i][m_colorindexOffset[i]];
                        m_colorindex[i][m_colorindexOffset[i] + JpegConstants.MaxSampleValue + j] = m_colorindex[i][m_colorindexOffset[i] + JpegConstants.MaxSampleValue];
                    }
                }
            }
        }

        /// <summary>
        /// Create the ordered-dither tables.
        /// Components having the same number of representative colors may 
        /// share a dither table.
        /// </summary>
        private void create_odither_tables()
        {
            for (int i = 0; i < m_cinfo.m_out_color_components; i++)
            {
                int nci = m_Ncolors[i]; /* # of distinct values for this color */

                /* search for matching prior component */
                int foundPos = -1;
                for (int j = 0; j < i; j++)
                {
                    if (nci == m_Ncolors[j])
                    {
                        foundPos = j;
                        break;
                    }
                }

                if (foundPos == -1)
                {
                    /* need a new table? */
                    m_odither[i] = make_odither_array(nci);
                }
                else
                    m_odither[i] = m_odither[foundPos];
            }
        }

        /// <summary>
        /// Allocate workspace for Floyd-Steinberg errors.
        /// </summary>
        private void alloc_fs_workspace()
        {
            for (int i = 0; i < m_cinfo.m_out_color_components; i++)
                m_fserrors[i] = new short[m_cinfo.m_output_width + 2];
        }

        /* 
         * Policy-making subroutines for create_colormap and create_colorindex.
         * These routines determine the colormap to be used.  The rest of the module
         * only assumes that the colormap is orthogonal.
         *
         *  * select_ncolors decides how to divvy up the available colors
         *    among the components.
         *  * output_value defines the set of representative values for a component.
         *  * largest_input_value defines the mapping from input values to
         *    representative values for a component.
         * Note that the latter two routines may impose different policies for
         * different components, though this is not currently done.
         */

        /// <summary>
        /// Return largest input value that should map to j'th output value
        /// Must have largest(j=0) >= 0, and largest(j=maxj) >= MaxSampleValue
        /// </summary>
        private static int largest_input_value(int j, int maxj)
        {
            /* Breakpoints are halfway between values returned by output_value */
            return (int)(((2 * j + 1) * JpegConstants.MaxSampleValue + maxj) / (2 * maxj));
        }

        /// <summary>
        /// Return j'th output value, where j will range from 0 to maxj
        /// The output values must fall in 0..MaxSampleValue in increasing order
        /// </summary>
        private static int output_value(int j, int maxj)
        {
            /* We always provide values 0 and MaxSampleValue for each component;
             * any additional values are equally spaced between these limits.
             * (Forcing the upper and lower values to the limits ensures that
             * dithering can't produce a color outside the selected gamut.)
             */
            return (int)((j * JpegConstants.MaxSampleValue + maxj / 2) / maxj);
        }

        /// <summary>
        /// Determine allocation of desired colors to components,
        /// and fill in Ncolors[] array to indicate choice.
        /// Return value is total number of colors (product of Ncolors[] values).
        /// </summary>
        private int select_ncolors(int[] Ncolors)
        {
            int nc = m_cinfo.m_out_color_components; /* number of color components */
            int max_colors = m_cinfo.m_desired_number_of_colors;

            /* We can allocate at least the nc'th root of max_colors per component. */
            /* Compute floor(nc'th root of max_colors). */
            int iroot = 1;
            long temp = 0;
            do
            {
                iroot++;
                temp = iroot;       /* set temp = iroot ** nc */
                for (int i = 1; i < nc; i++)
                    temp *= iroot;
            }
            while (temp <= max_colors); /* repeat till iroot exceeds root */

            /* now iroot = floor(root) */
            iroot--;

            /* Must have at least 2 color values per component */
            if (iroot < 2)
                throw new Exception(String.Format("Cannot quantize to fewer than {0} colors", (int)temp));

            /* Initialize to iroot color values for each component */
            int total_colors = 1;
            for (int i = 0; i < nc; i++)
            {
                Ncolors[i] = iroot;
                total_colors *= iroot;
            }

            /* We may be able to increment the count for one or more components without
             * exceeding max_colors, though we know not all can be incremented.
             * Sometimes, the first component can be incremented more than once!
             * (Example: for 16 colors, we start at 2*2*2, go to 3*2*2, then 4*2*2.)
             * In RGB colorspace, try to increment G first, then R, then B.
             */
            bool changed = false;
            do
            {
                changed = false;
                for (int i = 0; i < nc; i++)
                {
                    int j = (m_cinfo.m_out_color_space == ColorSpace.RGB ? RGB_order[i] : i);
                    /* calculate new total_colors if Ncolors[j] is incremented */
                    temp = total_colors / Ncolors[j];
                    temp *= Ncolors[j] + 1; /* done in long arith to avoid oflo */

                    if (temp > max_colors)
                        break;          /* won't fit, done with this pass */

                    Ncolors[j]++;       /* OK, apply the increment */
                    total_colors = (int)temp;
                    changed = true;
                }
            }
            while (changed);

            return total_colors;
        }

        /// <summary>
        /// Create an ordered-dither array for a component having ncolors
        /// distinct output values.
        /// </summary>
        private static int[][] make_odither_array(int ncolors)
        {
            int[][] odither = new int[ODITHER_SIZE][];
            for (int i = 0; i < ODITHER_SIZE; i++)
                odither[i] = new int[ODITHER_SIZE];

            /* The inter-value distance for this color is MaxSampleValue/(ncolors-1).
             * Hence the dither value for the matrix cell with fill order f
             * (f=0..N-1) should be (N-1-2*f)/(2*N) * MaxSampleValue/(ncolors-1).
             * On 16-bit-int machine, be careful to avoid overflow.
             */
            int den = 2 * ODITHER_CELLS * (ncolors - 1);
            for (int j = 0; j < ODITHER_SIZE; j++)
            {
                for (int k = 0; k < ODITHER_SIZE; k++)
                {
                    int num = ((int)(ODITHER_CELLS - 1 - 2 * ((int)base_dither_matrix[j][k]))) * JpegConstants.MaxSampleValue;

                    /* Ensure round towards zero despite C's lack of consistency
                     * about rounding negative values in integer division...
                     */
                    odither[j][k] = num < 0 ? -((-num) / den) : num / den;
                }
            }

            return odither;
        }
    }
    #endregion

    #region Pass2ColorQuantizer
    /// <summary>
    /// This module implements the well-known Heckbert paradigm for color
    /// quantization.  Most of the ideas used here can be traced back to
    /// Heckbert's seminal paper
    /// Heckbert, Paul.  "Color Image Quantization for Frame Buffer Display",
    /// Proc. SIGGRAPH '82, Computer Graphics v.16 #3 (July 1982), pp 297-304.
    /// 
    /// In the first pass over the image, we accumulate a histogram showing the
    /// usage count of each possible color.  To keep the histogram to a reasonable
    /// size, we reduce the precision of the input; typical practice is to retain
    /// 5 or 6 bits per color, so that 8 or 4 different input values are counted
    /// in the same histogram cell.
    /// 
    /// Next, the color-selection step begins with a box representing the whole
    /// color space, and repeatedly splits the "largest" remaining box until we
    /// have as many boxes as desired colors.  Then the mean color in each
    /// remaining box becomes one of the possible output colors.
    /// 
    /// The second pass over the image maps each input pixel to the closest output
    /// color (optionally after applying a Floyd-Steinberg dithering correction).
    /// This mapping is logically trivial, but making it go fast enough requires
    /// considerable care.
    /// 
    /// Heckbert-style quantizers vary a good deal in their policies for choosing
    /// the "largest" box and deciding where to cut it.  The particular policies
    /// used here have proved out well in experimental comparisons, but better ones
    /// may yet be found.
    /// 
    /// In earlier versions of the IJG code, this module quantized in YCbCr color
    /// space, processing the raw upsampled data without a color conversion step.
    /// This allowed the color conversion math to be done only once per colormap
    /// entry, not once per pixel.  However, that optimization precluded other
    /// useful optimizations (such as merging color conversion with upsampling)
    /// and it also interfered with desired capabilities such as quantizing to an
    /// externally-supplied colormap.  We have therefore abandoned that approach.
    /// The present code works in the post-conversion color space, typically RGB.
    /// 
    /// To improve the visual quality of the results, we actually work in scaled
    /// RGB space, giving G distances more weight than R, and R in turn more than
    /// B.  To do everything in integer math, we must use integer scale factors.
    /// The 2/3/1 scale factors used here correspond loosely to the relative
    /// weights of the colors in the NTSC grayscale equation.
    /// If you want to use this code to quantize a non-RGB color space, you'll
    /// probably need to change these scale factors.
    /// 
    /// First we have the histogram data structure and routines for creating it.
    /// 
    /// The number of bits of precision can be adjusted by changing these symbols.
    /// We recommend keeping 6 bits for G and 5 each for R and B.
    /// If you have plenty of memory and cycles, 6 bits all around gives marginally
    /// better results; if you are short of memory, 5 bits all around will save
    /// some space but degrade the results.
    /// To maintain a fully accurate histogram, we'd need to allocate a "long"
    /// (preferably unsigned long) for each cell.  In practice this is overkill;
    /// we can get by with 16 bits per cell.  Few of the cell counts will overflow,
    /// and clamping those that do overflow to the maximum value will give close-
    /// enough results.  This reduces the recommended histogram size from 256Kb
    /// to 128Kb, which is a useful savings on PC-class machines.
    /// (In the second pass the histogram space is re-used for pixel mapping data;
    /// in that capacity, each cell must be able to store zero to the number of
    /// desired colors.  16 bits/cell is plenty for that too.)
    /// Since the JPEG code is intended to run in small memory model on 80x86
    /// machines, we can't just allocate the histogram in one chunk.  Instead
    /// of a true 3-D array, we use a row of pointers to 2-D arrays.  Each
    /// pointer corresponds to a C0 value (typically 2^5 = 32 pointers) and
    /// each 2-D array has 2^6*2^5 = 2048 or 2^6*2^6 = 4096 entries.  Note that
    /// on 80x86 machines, the pointer row is in near memory but the actual
    /// arrays are in far memory (same arrangement as we use for image arrays).
    /// 
    /// 
    /// Declarations for Floyd-Steinberg dithering.
    /// 
    /// Errors are accumulated into the array fserrors[], at a resolution of
    /// 1/16th of a pixel count.  The error at a given pixel is propagated
    /// to its not-yet-processed neighbors using the standard F-S fractions,
    ///     ... (here)  7/16
    /// 3/16    5/16    1/16
    /// We work left-to-right on even rows, right-to-left on odd rows.
    /// 
    /// We can get away with a single array (holding one row's worth of errors)
    /// by using it to store the current row's errors at pixel columns not yet
    /// processed, but the next row's errors at columns already processed.  We
    /// need only a few extra variables to hold the errors immediately around the
    /// current column.  (If we are lucky, those variables are in registers, but
    /// even if not, they're probably cheaper to access than array elements are.)
    /// 
    /// The fserrors[] array has (#columns + 2) entries; the extra entry at
    /// each end saves us from special-casing the first and last pixels.
    /// Each entry is three values long, one value for each color component.
    /// </summary>
    class Pass2ColorQuantizer : ColorQuantizer
    {
        private struct box
        {
            /* The bounds of the box (inclusive); expressed as histogram indexes */
            public int c0min;
            public int c0max;
            public int c1min;
            public int c1max;
            public int c2min;
            public int c2max;
            /* The volume (actually 2-norm) of the box */
            public int volume;
            /* The number of nonzero histogram cells within this box */
            public long colorcount;
        }

        private enum QuantizerType
        {
            prescan_quantizer,
            pass2_fs_dither_quantizer,
            pass2_no_dither_quantizer
        }

        private const int MAXNUMCOLORS = (JpegConstants.MaxSampleValue + 1); /* maximum size of colormap */

        /* These will do the right thing for either R,G,B or B,G,R color order,
        * but you may not like the results for other color orders.
        */
        private const int HIST_C0_BITS = 5;     /* bits of precision in R/B histogram */
        private const int HIST_C1_BITS = 6;     /* bits of precision in G histogram */
        private const int HIST_C2_BITS = 5;     /* bits of precision in B/R histogram */

        /* Number of elements along histogram axes. */
        private const int HIST_C0_ELEMS = (1 << HIST_C0_BITS);
        private const int HIST_C1_ELEMS = (1 << HIST_C1_BITS);
        private const int HIST_C2_ELEMS = (1 << HIST_C2_BITS);

        /* These are the amounts to shift an input value to get a histogram index. */
        private const int C0_SHIFT = (JpegConstants.BitsInSample - HIST_C0_BITS);
        private const int C1_SHIFT = (JpegConstants.BitsInSample - HIST_C1_BITS);
        private const int C2_SHIFT = (JpegConstants.BitsInSample - HIST_C2_BITS);

        private const int R_SCALE = 2;       /* scale R distances by this much */
        private const int G_SCALE = 3;       /* scale G distances by this much */
        private const int B_SCALE = 1;       /* and B by this much */

        /* log2(histogram cells in update box) for each axis; this can be adjusted */
        private const int BOX_C0_LOG = (HIST_C0_BITS - 3);
        private const int BOX_C1_LOG = (HIST_C1_BITS - 3);
        private const int BOX_C2_LOG = (HIST_C2_BITS - 3);

        private const int BOX_C0_ELEMS = (1 << BOX_C0_LOG); /* # of hist cells in update box */
        private const int BOX_C1_ELEMS = (1 << BOX_C1_LOG);
        private const int BOX_C2_ELEMS = (1 << BOX_C2_LOG);

        private const int BOX_C0_SHIFT = (C0_SHIFT + BOX_C0_LOG);
        private const int BOX_C1_SHIFT = (C1_SHIFT + BOX_C1_LOG);
        private const int BOX_C2_SHIFT = (C2_SHIFT + BOX_C2_LOG);

        private QuantizerType m_quantizer;

        private bool m_useFinishPass1;

        private JpegDecompressor m_cinfo;

        /* Space for the eventually created colormap is stashed here */
        private byte[][] m_sv_colormap;  /* colormap allocated at init time */
        private int m_desired;            /* desired # of colors = size of colormap */

        /* Variables for accumulating image statistics */
        private ushort[][] m_histogram;     /* pointer to the histogram */

        private bool m_needs_zeroed;      /* true if next pass must zero histogram */

        /* Variables for Floyd-Steinberg dithering */
        private short[] m_fserrors;      /* accumulated errors */
        private bool m_on_odd_row;        /* flag to remember which row we are on */
        private int[] m_error_limiter;     /* table for clamping the applied error */

        /// <summary>
        /// Module initialization routine for 2-pass color quantization.
        /// </summary>
        public Pass2ColorQuantizer(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Make sure jdmaster didn't give me a case I can't handle */
            if (cinfo.m_out_color_components != 3)
                throw new Exception("Unable to handle anything other than 3 color components!");

            /* Allocate the histogram/inverse colormap storage */
            m_histogram = new ushort[HIST_C0_ELEMS][];
            for (int i = 0; i < HIST_C0_ELEMS; i++)
                m_histogram[i] = new ushort[HIST_C1_ELEMS * HIST_C2_ELEMS];

            m_needs_zeroed = true; /* histogram is garbage now */

            /* Allocate storage for the completed colormap, if required.
            * We do this now since it is FAR storage and may affect
            * the memory manager's space calculations.
            */
            if (cinfo.m_enable_2pass_quant)
            {
                /* Make sure color count is acceptable */
                int desired_local = cinfo.m_desired_number_of_colors;

                /* Lower bound on # of colors ... somewhat arbitrary as long as > 0 */
                if (desired_local < 8)
                    throw new Exception("Cannot quantize to fewer than 8 colors");

                /* Make sure colormap indexes can be represented by JSAMPLEs */
                if (desired_local > MAXNUMCOLORS)
                    throw new Exception(String.Format("Cannot quantize to more than {0} colors", MAXNUMCOLORS));

                m_sv_colormap = JpegCommonBase.AllocJpegSamples(desired_local, 3);
                m_desired = desired_local;
            }

            /* Only F-S dithering or no dithering is supported. */
            /* If user asks for ordered dither, give him F-S. */
            if (cinfo.m_dither_mode != DitherMode.None)
                cinfo.m_dither_mode = DitherMode.FloydStein;

            /* Allocate Floyd-Steinberg workspace if necessary.
            * This isn't really needed until pass 2, but again it is FAR storage.
            * Although we will cope with a later change in dither_mode,
            * we do not promise to honor max_memory_to_use if dither_mode changes.
            */
            if (cinfo.m_dither_mode == DitherMode.FloydStein)
            {
                m_fserrors = new short[(cinfo.m_output_width + 2) * 3];

                /* Might as well create the error-limiting table too. */
                init_error_limit();
            }
        }

        /// <summary>
        /// Initialize for each processing pass.
        /// </summary>
        public virtual void start_pass(bool is_pre_scan)
        {
            /* Only F-S dithering or no dithering is supported. */
            /* If user asks for ordered dither, give him F-S. */
            if (m_cinfo.m_dither_mode != DitherMode.None)
                m_cinfo.m_dither_mode = DitherMode.FloydStein;

            if (is_pre_scan)
            {
                /* Set up method pointers */
                m_quantizer = QuantizerType.prescan_quantizer;
                m_useFinishPass1 = true;
                m_needs_zeroed = true; /* Always zero histogram */
            }
            else
            {
                /* Set up method pointers */
                if (m_cinfo.m_dither_mode == DitherMode.FloydStein)
                    m_quantizer = QuantizerType.pass2_fs_dither_quantizer;
                else
                    m_quantizer = QuantizerType.pass2_no_dither_quantizer;

                m_useFinishPass1 = false;

                /* Make sure color count is acceptable */
                int i = m_cinfo.m_actual_number_of_colors;
                if (i < 1)
                    throw new Exception("Cannot quantize to less than 1 color");

                if (i > MAXNUMCOLORS)
                    throw new Exception(String.Format("Cannot quantize to more than {0} colors", MAXNUMCOLORS));

                if (m_cinfo.m_dither_mode == DitherMode.FloydStein)
                {
                    /* Allocate Floyd-Steinberg workspace if we didn't already. */
                    if (m_fserrors == null)
                    {
                        int arraysize = (m_cinfo.m_output_width + 2) * 3;
                        m_fserrors = new short[arraysize];
                    }
                    else
                    {
                        /* Initialize the propagated errors to zero. */
                        Array.Clear(m_fserrors, 0, m_fserrors.Length);
                    }

                    /* Make the error-limit table if we didn't already. */
                    if (m_error_limiter == null)
                        init_error_limit();

                    m_on_odd_row = false;
                }
            }

            /* Zero the histogram or inverse color map, if necessary */
            if (m_needs_zeroed)
            {
                for (int i = 0; i < HIST_C0_ELEMS; i++)
                    Array.Clear(m_histogram[i], 0, m_histogram[i].Length);

                m_needs_zeroed = false;
            }
        }

        public virtual void color_quantize(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            switch (m_quantizer)
            {
                case QuantizerType.prescan_quantizer:
                    prescan_quantize(input_buf, in_row, num_rows);
                    break;
                case QuantizerType.pass2_fs_dither_quantizer:
                    pass2_fs_dither(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                case QuantizerType.pass2_no_dither_quantizer:
                    pass2_no_dither(input_buf, in_row, output_buf, out_row, num_rows);
                    break;
                default:
                    throw new Exception("Specified Quantizer Type not implemented");
            }
        }

        public virtual void finish_pass()
        {
            if (m_useFinishPass1)
                finish_pass1();
        }

        /// <summary>
        /// Switch to a new external colormap between output passes.
        /// </summary>
        public virtual void new_color_map()
        {
            /* Reset the inverse color map */
            m_needs_zeroed = true;
        }

        /// <summary>
        /// Prescan some rows of pixels.
        /// In this module the prescan simply updates the histogram, which has been
        /// initialized to zeroes by start_pass.
        /// An output_buf parameter is required by the method signature, but no data
        /// is actually output (in fact the buffer controller is probably passing a
        /// null pointer).
        /// </summary>
        private void prescan_quantize(byte[][] input_buf, int in_row, int num_rows)
        {
            for (int row = 0; row < num_rows; row++)
            {
                int inputIndex = 0;
                for (int col = m_cinfo.m_output_width; col > 0; col--)
                {
                    int rowIndex = (int)input_buf[in_row + row][inputIndex] >> C0_SHIFT;
                    int columnIndex = ((int)input_buf[in_row + row][inputIndex + 1] >> C1_SHIFT) * HIST_C2_ELEMS +
                        ((int)input_buf[in_row + row][inputIndex + 2] >> C2_SHIFT);

                    /* increment pixel value, check for overflow and undo increment if so. */
                    m_histogram[rowIndex][columnIndex]++;
                    if (m_histogram[rowIndex][columnIndex] <= 0)
                        m_histogram[rowIndex][columnIndex]--;

                    inputIndex += 3;
                }
            }
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// This version performs Floyd-Steinberg dithering
        /// </summary>
        private void pass2_fs_dither(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            byte[] limit = m_cinfo.m_sample_range_limit;
            int limitOffset = m_cinfo.m_sampleRangeLimitOffset;

            for (int row = 0; row < num_rows; row++)
            {
                int inputPixelIndex = 0;
                int outputPixelIndex = 0;
                int errorIndex = 0;
                int dir;            /* +1 or -1 depending on direction */
                int dir3;           /* 3*dir, for advancing inputIndex & errorIndex */
                if (m_on_odd_row)
                {
                    /* work right to left in this row */
                    inputPixelIndex += (m_cinfo.m_output_width - 1) * 3;   /* so point to rightmost pixel */
                    outputPixelIndex += m_cinfo.m_output_width - 1;
                    dir = -1;
                    dir3 = -3;
                    errorIndex = (m_cinfo.m_output_width + 1) * 3; /* => entry after last column */
                    m_on_odd_row = false; /* flip for next time */
                }
                else
                {
                    /* work left to right in this row */
                    dir = 1;
                    dir3 = 3;
                    errorIndex = 0; /* => entry before first real column */
                    m_on_odd_row = true; /* flip for next time */
                }

                /* Preset error values: no error propagated to first pixel from left */
                /* current error or pixel value */
                int cur0 = 0;
                int cur1 = 0;
                int cur2 = 0;
                /* and no error propagated to row below yet */
                /* error for pixel below cur */
                int belowerr0 = 0;
                int belowerr1 = 0;
                int belowerr2 = 0;
                /* error for below/prev col */
                int bpreverr0 = 0;
                int bpreverr1 = 0;
                int bpreverr2 = 0;

                for (int col = m_cinfo.m_output_width; col > 0; col--)
                {
                    /* curN holds the error propagated from the previous pixel on the
                     * current line.  Add the error propagated from the previous line
                     * to form the complete error correction term for this pixel, and
                     * round the error term (which is expressed * 16) to an integer.
                     * RIGHT_SHIFT rounds towards minus infinity, so adding 8 is correct
                     * for either sign of the error value.
                     * Note: errorIndex is for *previous* column's array entry.
                     */
                    cur0 = JpegUtils.RIGHT_SHIFT(cur0 + m_fserrors[errorIndex + dir3] + 8, 4);
                    cur1 = JpegUtils.RIGHT_SHIFT(cur1 + m_fserrors[errorIndex + dir3 + 1] + 8, 4);
                    cur2 = JpegUtils.RIGHT_SHIFT(cur2 + m_fserrors[errorIndex + dir3 + 2] + 8, 4);

                    /* Limit the error using transfer function set by init_error_limit.
                     * See comments with init_error_limit for rationale.
                     */
                    cur0 = m_error_limiter[JpegConstants.MaxSampleValue + cur0];
                    cur1 = m_error_limiter[JpegConstants.MaxSampleValue + cur1];
                    cur2 = m_error_limiter[JpegConstants.MaxSampleValue + cur2];

                    /* Form pixel value + error, and range-limit to 0..MaxSampleValue.
                     * The maximum error is +- MaxSampleValue (or less with error limiting);
                     * this sets the required size of the range_limit array.
                     */
                    cur0 += input_buf[in_row + row][inputPixelIndex];
                    cur1 += input_buf[in_row + row][inputPixelIndex + 1];
                    cur2 += input_buf[in_row + row][inputPixelIndex + 2];
                    cur0 = limit[limitOffset + cur0];
                    cur1 = limit[limitOffset + cur1];
                    cur2 = limit[limitOffset + cur2];

                    /* Index into the cache with adjusted pixel value */
                    int hRow = cur0 >> C0_SHIFT;
                    int hColumn = (cur1 >> C1_SHIFT) * HIST_C2_ELEMS + (cur2 >> C2_SHIFT);

                    /* If we have not seen this color before, find nearest colormap */
                    /* entry and update the cache */
                    if (m_histogram[hRow][hColumn] == 0)
                        fill_inverse_cmap(cur0 >> C0_SHIFT, cur1 >> C1_SHIFT, cur2 >> C2_SHIFT);

                    /* Now emit the colormap index for this cell */
                    int pixcode = m_histogram[hRow][hColumn] - 1;
                    output_buf[out_row + row][outputPixelIndex] = (byte)pixcode;

                    /* Compute representation error for this pixel */
                    cur0 -= m_cinfo.m_colormap[0][pixcode];
                    cur1 -= m_cinfo.m_colormap[1][pixcode];
                    cur2 -= m_cinfo.m_colormap[2][pixcode];

                    /* Compute error fractions to be propagated to adjacent pixels.
                     * Add these into the running sums, and simultaneously shift the
                     * next-line error sums left by 1 column.
                     */
                    int bnexterr = cur0;    /* Process component 0 */
                    int delta = cur0 * 2;
                    cur0 += delta;      /* form error * 3 */
                    m_fserrors[errorIndex] = (short)(bpreverr0 + cur0);
                    cur0 += delta;      /* form error * 5 */
                    bpreverr0 = belowerr0 + cur0;
                    belowerr0 = bnexterr;
                    cur0 += delta;      /* form error * 7 */
                    bnexterr = cur1;    /* Process component 1 */
                    delta = cur1 * 2;
                    cur1 += delta;      /* form error * 3 */
                    m_fserrors[errorIndex + 1] = (short)(bpreverr1 + cur1);
                    cur1 += delta;      /* form error * 5 */
                    bpreverr1 = belowerr1 + cur1;
                    belowerr1 = bnexterr;
                    cur1 += delta;      /* form error * 7 */
                    bnexterr = cur2;    /* Process component 2 */
                    delta = cur2 * 2;
                    cur2 += delta;      /* form error * 3 */
                    m_fserrors[errorIndex + 2] = (short)(bpreverr2 + cur2);
                    cur2 += delta;      /* form error * 5 */
                    bpreverr2 = belowerr2 + cur2;
                    belowerr2 = bnexterr;
                    cur2 += delta;      /* form error * 7 */

                    /* At this point curN contains the 7/16 error value to be propagated
                     * to the next pixel on the current line, and all the errors for the
                     * next line have been shifted over.  We are therefore ready to move on.
                     */
                    inputPixelIndex += dir3;      /* Advance pixel pointers to next column */
                    outputPixelIndex += dir;
                    errorIndex += dir3;       /* advance errorIndex to current column */
                }

                /* Post-loop cleanup: we must unload the final error values into the
                 * final fserrors[] entry.  Note we need not unload belowerrN because
                 * it is for the dummy column before or after the actual array.
                 */
                m_fserrors[errorIndex] = (short)bpreverr0; /* unload prev errs into array */
                m_fserrors[errorIndex + 1] = (short)bpreverr1;
                m_fserrors[errorIndex + 2] = (short)bpreverr2;
            }
        }

        /// <summary>
        /// Map some rows of pixels to the output colormapped representation.
        /// This version performs no dithering
        /// </summary>
        private void pass2_no_dither(byte[][] input_buf, int in_row, byte[][] output_buf, int out_row, int num_rows)
        {
            for (int row = 0; row < num_rows; row++)
            {
                int inRow = row + in_row;
                int inIndex = 0;
                int outIndex = 0;
                int outRow = out_row + row;
                for (int col = m_cinfo.m_output_width; col > 0; col--)
                {
                    /* get pixel value and index into the cache */
                    int c0 = (int)input_buf[inRow][inIndex] >> C0_SHIFT;
                    inIndex++;

                    int c1 = (int)input_buf[inRow][inIndex] >> C1_SHIFT;
                    inIndex++;

                    int c2 = (int)input_buf[inRow][inIndex] >> C2_SHIFT;
                    inIndex++;

                    int hRow = c0;
                    int hColumn = c1 * HIST_C2_ELEMS + c2;

                    /* If we have not seen this color before, find nearest colormap entry */
                    /* and update the cache */
                    if (m_histogram[hRow][hColumn] == 0)
                        fill_inverse_cmap(c0, c1, c2);

                    /* Now emit the colormap index for this cell */
                    output_buf[outRow][outIndex] = (byte)(m_histogram[hRow][hColumn] - 1);
                    outIndex++;
                }
            }
        }

        /// <summary>
        /// Finish up at the end of each pass.
        /// </summary>
        private void finish_pass1()
        {
            /* Select the representative colors and fill in cinfo.colormap */
            m_cinfo.m_colormap = m_sv_colormap;
            select_colors(m_desired);

            /* Force next pass to zero the color index table */
            m_needs_zeroed = true;
        }

        /// <summary>
        /// Compute representative color for a box, put it in colormap[icolor]
        /// </summary>
        private void compute_color(box[] boxlist, int boxIndex, int icolor)
        {
            /* Current algorithm: mean weighted by pixels (not colors) */
            /* Note it is important to get the rounding correct! */
            long total = 0;
            long c0total = 0;
            long c1total = 0;
            long c2total = 0;
            box curBox = boxlist[boxIndex];
            for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
            {
                for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++)
                {
                    int histogramIndex = c1 * HIST_C2_ELEMS + curBox.c2min;
                    for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++)
                    {
                        long count = m_histogram[c0][histogramIndex];
                        histogramIndex++;

                        if (count != 0)
                        {
                            total += count;
                            c0total += ((c0 << C0_SHIFT) + ((1 << C0_SHIFT) >> 1)) * count;
                            c1total += ((c1 << C1_SHIFT) + ((1 << C1_SHIFT) >> 1)) * count;
                            c2total += ((c2 << C2_SHIFT) + ((1 << C2_SHIFT) >> 1)) * count;
                        }
                    }
                }
            }

            m_cinfo.m_colormap[0][icolor] = (byte)((c0total + (total >> 1)) / total);
            m_cinfo.m_colormap[1][icolor] = (byte)((c1total + (total >> 1)) / total);
            m_cinfo.m_colormap[2][icolor] = (byte)((c2total + (total >> 1)) / total);
        }

        /// <summary>
        /// Master routine for color selection
        /// </summary>
        private void select_colors(int desired_colors)
        {
            /* Allocate workspace for box list */
            box[] boxlist = new box[desired_colors];

            /* Initialize one box containing whole space */
            int numboxes = 1;
            boxlist[0].c0min = 0;
            boxlist[0].c0max = JpegConstants.MaxSampleValue >> C0_SHIFT;
            boxlist[0].c1min = 0;
            boxlist[0].c1max = JpegConstants.MaxSampleValue >> C1_SHIFT;
            boxlist[0].c2min = 0;
            boxlist[0].c2max = JpegConstants.MaxSampleValue >> C2_SHIFT;

            /* Shrink it to actually-used volume and set its statistics */
            update_box(boxlist, 0);

            /* Perform median-cut to produce final box list */
            numboxes = median_cut(boxlist, numboxes, desired_colors);

            /* Compute the representative color for each box, fill colormap */
            for (int i = 0; i < numboxes; i++)
                compute_color(boxlist, i, i);

            m_cinfo.m_actual_number_of_colors = numboxes;
        }

        /// <summary>
        /// Repeatedly select and split the largest box until we have enough boxes
        /// </summary>
        private int median_cut(box[] boxlist, int numboxes, int desired_colors)
        {
            while (numboxes < desired_colors)
            {
                /* Select box to split.
                 * Current algorithm: by population for first half, then by volume.
                 */
                int foundIndex;
                if (numboxes * 2 <= desired_colors)
                    foundIndex = find_biggest_color_pop(boxlist, numboxes);
                else
                    foundIndex = find_biggest_volume(boxlist, numboxes);

                if (foundIndex == -1)     /* no splittable boxes left! */
                    break;

                /* Copy the color bounds to the new box. */
                boxlist[numboxes].c0max = boxlist[foundIndex].c0max;
                boxlist[numboxes].c1max = boxlist[foundIndex].c1max;
                boxlist[numboxes].c2max = boxlist[foundIndex].c2max;
                boxlist[numboxes].c0min = boxlist[foundIndex].c0min;
                boxlist[numboxes].c1min = boxlist[foundIndex].c1min;
                boxlist[numboxes].c2min = boxlist[foundIndex].c2min;

                /* Choose which axis to split the box on.
                 * Current algorithm: longest scaled axis.
                 * See notes in update_box about scaling distances.
                 */
                int c0 = ((boxlist[foundIndex].c0max - boxlist[foundIndex].c0min) << C0_SHIFT) * R_SCALE;
                int c1 = ((boxlist[foundIndex].c1max - boxlist[foundIndex].c1min) << C1_SHIFT) * G_SCALE;
                int c2 = ((boxlist[foundIndex].c2max - boxlist[foundIndex].c2min) << C2_SHIFT) * B_SCALE;

                /* We want to break any ties in favor of green, then red, blue last.
                 * This code does the right thing for R,G,B or B,G,R color orders only.
                 */
                int cmax = c1;
                int n = 1;

                if (c0 > cmax)
                {
                    cmax = c0;
                    n = 0;
                }

                if (c2 > cmax)
                {
                    n = 2;
                }

                /* Choose split point along selected axis, and update box bounds.
                 * Current algorithm: split at halfway point.
                 * (Since the box has been shrunk to minimum volume,
                 * any split will produce two nonempty subboxes.)
                 * Note that lb value is max for lower box, so must be < old max.
                 */
                int lb;
                switch (n)
                {
                    case 0:
                        lb = (boxlist[foundIndex].c0max + boxlist[foundIndex].c0min) / 2;
                        boxlist[foundIndex].c0max = lb;
                        boxlist[numboxes].c0min = lb + 1;
                        break;
                    case 1:
                        lb = (boxlist[foundIndex].c1max + boxlist[foundIndex].c1min) / 2;
                        boxlist[foundIndex].c1max = lb;
                        boxlist[numboxes].c1min = lb + 1;
                        break;
                    case 2:
                        lb = (boxlist[foundIndex].c2max + boxlist[foundIndex].c2min) / 2;
                        boxlist[foundIndex].c2max = lb;
                        boxlist[numboxes].c2min = lb + 1;
                        break;
                }

                /* Update stats for boxes */
                update_box(boxlist, foundIndex);
                update_box(boxlist, numboxes);
                numboxes++;
            }

            return numboxes;
        }

        /*
         * Next we have the really interesting routines: selection of a colormap
         * given the completed histogram.
         * These routines work with a list of "boxes", each representing a rectangular
         * subset of the input color space (to histogram precision).
         */

        /// <summary>
        /// Find the splittable box with the largest color population
        /// Returns null if no splittable boxes remain
        /// </summary>
        private static int find_biggest_color_pop(box[] boxlist, int numboxes)
        {
            long maxc = 0;
            int which = -1;
            for (int i = 0; i < numboxes; i++)
            {
                if (boxlist[i].colorcount > maxc && boxlist[i].volume > 0)
                {
                    which = i;
                    maxc = boxlist[i].colorcount;
                }
            }

            return which;
        }

        /// <summary>
        /// Find the splittable box with the largest (scaled) volume
        /// Returns null if no splittable boxes remain
        /// </summary>
        private static int find_biggest_volume(box[] boxlist, int numboxes)
        {
            int maxv = 0;
            int which = -1;
            for (int i = 0; i < numboxes; i++)
            {
                if (boxlist[i].volume > maxv)
                {
                    which = i;
                    maxv = boxlist[i].volume;
                }
            }

            return which;
        }

        /// <summary>
        /// Shrink the min/max bounds of a box to enclose only nonzero elements,
        /// and recompute its volume and population
        /// </summary>
        private void update_box(box[] boxlist, int boxIndex)
        {
            box curBox = boxlist[boxIndex];
            bool have_c0min = false;

            if (curBox.c0max > curBox.c0min)
            {
                for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
                {
                    for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++)
                    {
                        int histogramIndex = c1 * HIST_C2_ELEMS + curBox.c2min;
                        for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++)
                        {
                            if (m_histogram[c0][histogramIndex++] != 0)
                            {
                                curBox.c0min = c0;
                                have_c0min = true;
                                break;
                            }
                        }

                        if (have_c0min)
                            break;
                    }

                    if (have_c0min)
                        break;
                }
            }

            bool have_c0max = false;
            if (curBox.c0max > curBox.c0min)
            {
                for (int c0 = curBox.c0max; c0 >= curBox.c0min; c0--)
                {
                    for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++)
                    {
                        int histogramIndex = c1 * HIST_C2_ELEMS + curBox.c2min;
                        for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++)
                        {
                            if (m_histogram[c0][histogramIndex++] != 0)
                            {
                                curBox.c0max = c0;
                                have_c0max = true;
                                break;
                            }
                        }

                        if (have_c0max)
                            break;
                    }

                    if (have_c0max)
                        break;
                }
            }

            bool have_c1min = false;
            if (curBox.c1max > curBox.c1min)
            {
                for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++)
                {
                    for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
                    {
                        int histogramIndex = c1 * HIST_C2_ELEMS + curBox.c2min;
                        for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++)
                        {
                            if (m_histogram[c0][histogramIndex++] != 0)
                            {
                                curBox.c1min = c1;
                                have_c1min = true;
                                break;
                            }
                        }

                        if (have_c1min)
                            break;
                    }

                    if (have_c1min)
                        break;
                }
            }

            bool have_c1max = false;
            if (curBox.c1max > curBox.c1min)
            {
                for (int c1 = curBox.c1max; c1 >= curBox.c1min; c1--)
                {
                    for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
                    {
                        int histogramIndex = c1 * HIST_C2_ELEMS + curBox.c2min;
                        for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++)
                        {
                            if (m_histogram[c0][histogramIndex++] != 0)
                            {
                                curBox.c1max = c1;
                                have_c1max = true;
                                break;
                            }
                        }

                        if (have_c1max)
                            break;
                    }

                    if (have_c1max)
                        break;
                }
            }

            bool have_c2min = false;
            if (curBox.c2max > curBox.c2min)
            {
                for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++)
                {
                    for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
                    {
                        int histogramIndex = curBox.c1min * HIST_C2_ELEMS + c2;
                        for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++, histogramIndex += HIST_C2_ELEMS)
                        {
                            if (m_histogram[c0][histogramIndex] != 0)
                            {
                                curBox.c2min = c2;
                                have_c2min = true;
                                break;
                            }
                        }

                        if (have_c2min)
                            break;
                    }

                    if (have_c2min)
                        break;
                }
            }

            bool have_c2max = false;
            if (curBox.c2max > curBox.c2min)
            {
                for (int c2 = curBox.c2max; c2 >= curBox.c2min; c2--)
                {
                    for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
                    {
                        int histogramIndex = curBox.c1min * HIST_C2_ELEMS + c2;
                        for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++, histogramIndex += HIST_C2_ELEMS)
                        {
                            if (m_histogram[c0][histogramIndex] != 0)
                            {
                                curBox.c2max = c2;
                                have_c2max = true;
                                break;
                            }
                        }

                        if (have_c2max)
                            break;
                    }

                    if (have_c2max)
                        break;
                }
            }

            /* Update box volume.
             * We use 2-norm rather than real volume here; this biases the method
             * against making long narrow boxes, and it has the side benefit that
             * a box is splittable iff norm > 0.
             * Since the differences are expressed in histogram-cell units,
             * we have to shift back to byte units to get consistent distances;
             * after which, we scale according to the selected distance scale factors.
             */
            int dist0 = ((curBox.c0max - curBox.c0min) << C0_SHIFT) * R_SCALE;
            int dist1 = ((curBox.c1max - curBox.c1min) << C1_SHIFT) * G_SCALE;
            int dist2 = ((curBox.c2max - curBox.c2min) << C2_SHIFT) * B_SCALE;
            curBox.volume = dist0 * dist0 + dist1 * dist1 + dist2 * dist2;

            /* Now scan remaining volume of box and compute population */
            long ccount = 0;
            for (int c0 = curBox.c0min; c0 <= curBox.c0max; c0++)
            {
                for (int c1 = curBox.c1min; c1 <= curBox.c1max; c1++)
                {
                    int histogramIndex = c1 * HIST_C2_ELEMS + curBox.c2min;
                    for (int c2 = curBox.c2min; c2 <= curBox.c2max; c2++, histogramIndex++)
                    {
                        if (m_histogram[c0][histogramIndex] != 0)
                            ccount++;
                    }
                }
            }

            curBox.colorcount = ccount;
            boxlist[boxIndex] = curBox;
        }

        /// <summary>
        /// Initialize the error-limiting transfer function (lookup table).
        /// The raw F-S error computation can potentially compute error values of up to
        /// +- MaxSampleValue.  But we want the maximum correction applied to a pixel to be
        /// much less, otherwise obviously wrong pixels will be created.  (Typical
        /// effects include weird fringes at color-area boundaries, isolated bright
        /// pixels in a dark area, etc.)  The standard advice for avoiding this problem
        /// is to ensure that the "corners" of the color cube are allocated as output
        /// colors; then repeated errors in the same direction cannot cause cascading
        /// error buildup.  However, that only prevents the error from getting
        /// completely out of hand; Aaron Giles reports that error limiting improves
        /// the results even with corner colors allocated.
        /// A simple clamping of the error values to about +- MaxSampleValue/8 works pretty
        /// well, but the smoother transfer function used below is even better.  Thanks
        /// to Aaron Giles for this idea.
        /// </summary>
        private void init_error_limit()
        {
            m_error_limiter = new int[JpegConstants.MaxSampleValue * 2 + 1];
            int tableOffset = JpegConstants.MaxSampleValue;

            const int STEPSIZE = ((JpegConstants.MaxSampleValue + 1) / 16);

            /* Map errors 1:1 up to +- MaxSampleValue/16 */
            int output = 0;
            int input = 0;
            for (; input < STEPSIZE; input++, output++)
            {
                m_error_limiter[tableOffset + input] = output;
                m_error_limiter[tableOffset - input] = -output;
            }

            /* Map errors 1:2 up to +- 3*MaxSampleValue/16 */
            for (; input < STEPSIZE * 3; input++)
            {
                m_error_limiter[tableOffset + input] = output;
                m_error_limiter[tableOffset - input] = -output;
                output += (input & 1) != 0 ? 1 : 0;
            }

            /* Clamp the rest to final output value (which is (MaxSampleValue+1)/8) */
            for (; input <= JpegConstants.MaxSampleValue; input++)
            {
                m_error_limiter[tableOffset + input] = output;
                m_error_limiter[tableOffset - input] = -output;
            }
        }

        /*
         * These routines are concerned with the time-critical task of mapping input
         * colors to the nearest color in the selected colormap.
         *
         * We re-use the histogram space as an "inverse color map", essentially a
         * cache for the results of nearest-color searches.  All colors within a
         * histogram cell will be mapped to the same colormap entry, namely the one
         * closest to the cell's center.  This may not be quite the closest entry to
         * the actual input color, but it's almost as good.  A zero in the cache
         * indicates we haven't found the nearest color for that cell yet; the array
         * is cleared to zeroes before starting the mapping pass.  When we find the
         * nearest color for a cell, its colormap index plus one is recorded in the
         * cache for future use.  The pass2 scanning routines call fill_inverse_cmap
         * when they need to use an unfilled entry in the cache.
         *
         * Our method of efficiently finding nearest colors is based on the "locally
         * sorted search" idea described by Heckbert and on the incremental distance
         * calculation described by Spencer W. Thomas in chapter III.1 of Graphics
         * Gems II (James Arvo, ed.  Academic Press, 1991).  Thomas points out that
         * the distances from a given colormap entry to each cell of the histogram can
         * be computed quickly using an incremental method: the differences between
         * distances to adjacent cells themselves differ by a constant.  This allows a
         * fairly fast implementation of the "brute force" approach of computing the
         * distance from every colormap entry to every histogram cell.  Unfortunately,
         * it needs a work array to hold the best-distance-so-far for each histogram
         * cell (because the inner loop has to be over cells, not colormap entries).
         * The work array elements have to be ints, so the work array would need
         * 256Kb at our recommended precision.  This is not feasible in DOS machines.
         *
         * To get around these problems, we apply Thomas' method to compute the
         * nearest colors for only the cells within a small subbox of the histogram.
         * The work array need be only as big as the subbox, so the memory usage
         * problem is solved.  Furthermore, we need not fill subboxes that are never
         * referenced in pass2; many images use only part of the color gamut, so a
         * fair amount of work is saved.  An additional advantage of this
         * approach is that we can apply Heckbert's locality criterion to quickly
         * eliminate colormap entries that are far away from the subbox; typically
         * three-fourths of the colormap entries are rejected by Heckbert's criterion,
         * and we need not compute their distances to individual cells in the subbox.
         * The speed of this approach is heavily influenced by the subbox size: too
         * small means too much overhead, too big loses because Heckbert's criterion
         * can't eliminate as many colormap entries.  Empirically the best subbox
         * size seems to be about 1/512th of the histogram (1/8th in each direction).
         *
         * Thomas' article also describes a refined method which is asymptotically
         * faster than the brute-force method, but it is also far more complex and
         * cannot efficiently be applied to small subboxes.  It is therefore not
         * useful for programs intended to be portable to DOS machines.  On machines
         * with plenty of memory, filling the whole histogram in one shot with Thomas'
         * refined method might be faster than the present code --- but then again,
         * it might not be any faster, and it's certainly more complicated.
         */

        /*
         * The next three routines implement inverse colormap filling.  They could
         * all be folded into one big routine, but splitting them up this way saves
         * some stack space (the mindist[] and bestdist[] arrays need not coexist)
         * and may allow some compilers to produce better code by registerizing more
         * inner-loop variables.
         */

        /// <summary>
        /// Locate the colormap entries close enough to an update box to be candidates
        /// for the nearest entry to some cell(s) in the update box.  The update box
        /// is specified by the center coordinates of its first cell.  The number of
        /// candidate colormap entries is returned, and their colormap indexes are
        /// placed in colorlist[].
        /// This routine uses Heckbert's "locally sorted search" criterion to select
        /// the colors that need further consideration.
        /// </summary>
        private int find_nearby_colors(int minc0, int minc1, int minc2, byte[] colorlist)
        {
            /* Compute true coordinates of update box's upper corner and center.
             * Actually we compute the coordinates of the center of the upper-corner
             * histogram cell, which are the upper bounds of the volume we care about.
             * Note that since ">>" rounds down, the "center" values may be closer to
             * min than to max; hence comparisons to them must be "<=", not "<".
             */
            int maxc0 = minc0 + ((1 << BOX_C0_SHIFT) - (1 << C0_SHIFT));
            int centerc0 = (minc0 + maxc0) >> 1;

            int maxc1 = minc1 + ((1 << BOX_C1_SHIFT) - (1 << C1_SHIFT));
            int centerc1 = (minc1 + maxc1) >> 1;

            int maxc2 = minc2 + ((1 << BOX_C2_SHIFT) - (1 << C2_SHIFT));
            int centerc2 = (minc2 + maxc2) >> 1;

            /* For each color in colormap, find:
             *  1. its minimum squared-distance to any point in the update box
             *     (zero if color is within update box);
             *  2. its maximum squared-distance to any point in the update box.
             * Both of these can be found by considering only the corners of the box.
             * We save the minimum distance for each color in mindist[];
             * only the smallest maximum distance is of interest.
             */
            int minmaxdist = 0x7FFFFFFF;
            int[] mindist = new int[MAXNUMCOLORS];    /* min distance to colormap entry i */

            for (int i = 0; i < m_cinfo.m_actual_number_of_colors; i++)
            {
                /* We compute the squared-c0-distance term, then add in the other two. */
                int x = m_cinfo.m_colormap[0][i];
                int min_dist;
                int max_dist;

                if (x < minc0)
                {
                    int tdist = (x - minc0) * R_SCALE;
                    min_dist = tdist * tdist;
                    tdist = (x - maxc0) * R_SCALE;
                    max_dist = tdist * tdist;
                }
                else if (x > maxc0)
                {
                    int tdist = (x - maxc0) * R_SCALE;
                    min_dist = tdist * tdist;
                    tdist = (x - minc0) * R_SCALE;
                    max_dist = tdist * tdist;
                }
                else
                {
                    /* within cell range so no contribution to min_dist */
                    min_dist = 0;
                    if (x <= centerc0)
                    {
                        int tdist = (x - maxc0) * R_SCALE;
                        max_dist = tdist * tdist;
                    }
                    else
                    {
                        int tdist = (x - minc0) * R_SCALE;
                        max_dist = tdist * tdist;
                    }
                }

                x = m_cinfo.m_colormap[1][i];
                if (x < minc1)
                {
                    int tdist = (x - minc1) * G_SCALE;
                    min_dist += tdist * tdist;
                    tdist = (x - maxc1) * G_SCALE;
                    max_dist += tdist * tdist;
                }
                else if (x > maxc1)
                {
                    int tdist = (x - maxc1) * G_SCALE;
                    min_dist += tdist * tdist;
                    tdist = (x - minc1) * G_SCALE;
                    max_dist += tdist * tdist;
                }
                else
                {
                    /* within cell range so no contribution to min_dist */
                    if (x <= centerc1)
                    {
                        int tdist = (x - maxc1) * G_SCALE;
                        max_dist += tdist * tdist;
                    }
                    else
                    {
                        int tdist = (x - minc1) * G_SCALE;
                        max_dist += tdist * tdist;
                    }
                }

                x = m_cinfo.m_colormap[2][i];
                if (x < minc2)
                {
                    int tdist = (x - minc2) * B_SCALE;
                    min_dist += tdist * tdist;
                    tdist = (x - maxc2) * B_SCALE;
                    max_dist += tdist * tdist;
                }
                else if (x > maxc2)
                {
                    int tdist = (x - maxc2) * B_SCALE;
                    min_dist += tdist * tdist;
                    tdist = (x - minc2) * B_SCALE;
                    max_dist += tdist * tdist;
                }
                else
                {
                    /* within cell range so no contribution to min_dist */
                    if (x <= centerc2)
                    {
                        int tdist = (x - maxc2) * B_SCALE;
                        max_dist += tdist * tdist;
                    }
                    else
                    {
                        int tdist = (x - minc2) * B_SCALE;
                        max_dist += tdist * tdist;
                    }
                }

                mindist[i] = min_dist;  /* save away the results */
                if (max_dist < minmaxdist)
                    minmaxdist = max_dist;
            }

            /* Now we know that no cell in the update box is more than minmaxdist
             * away from some colormap entry.  Therefore, only colors that are
             * within minmaxdist of some part of the box need be considered.
             */
            int ncolors = 0;
            for (int i = 0; i < m_cinfo.m_actual_number_of_colors; i++)
            {
                if (mindist[i] <= minmaxdist)
                    colorlist[ncolors++] = (byte)i;
            }

            return ncolors;
        }

        /// <summary>
        /// Find the closest colormap entry for each cell in the update box,
        /// given the list of candidate colors prepared by find_nearby_colors.
        /// Return the indexes of the closest entries in the bestcolor[] array.
        /// This routine uses Thomas' incremental distance calculation method to
        /// find the distance from a colormap entry to successive cells in the box.
        /// </summary>
        private void find_best_colors(int minc0, int minc1, int minc2, int numcolors, byte[] colorlist, byte[] bestcolor)
        {
            /* Nominal steps between cell centers ("x" in Thomas article) */
            const int STEP_C0 = ((1 << C0_SHIFT) * R_SCALE);
            const int STEP_C1 = ((1 << C1_SHIFT) * G_SCALE);
            const int STEP_C2 = ((1 << C2_SHIFT) * B_SCALE);

            /* This array holds the distance to the nearest-so-far color for each cell */
            int[] bestdist = new int[BOX_C0_ELEMS * BOX_C1_ELEMS * BOX_C2_ELEMS];

            /* Initialize best-distance for each cell of the update box */
            int bestIndex = 0;
            for (int i = BOX_C0_ELEMS * BOX_C1_ELEMS * BOX_C2_ELEMS - 1; i >= 0; i--)
            {
                bestdist[bestIndex] = 0x7FFFFFFF;
                bestIndex++;
            }

            /* For each color selected by find_nearby_colors,
             * compute its distance to the center of each cell in the box.
             * If that's less than best-so-far, update best distance and color number.
             */
            for (int i = 0; i < numcolors; i++)
            {
                int icolor = colorlist[i];

                /* Compute (square of) distance from minc0/c1/c2 to this color */
                int inc0 = (minc0 - m_cinfo.m_colormap[0][icolor]) * R_SCALE;
                int dist0 = inc0 * inc0;

                int inc1 = (minc1 - m_cinfo.m_colormap[1][icolor]) * G_SCALE;
                dist0 += inc1 * inc1;

                int inc2 = (minc2 - m_cinfo.m_colormap[2][icolor]) * B_SCALE;
                dist0 += inc2 * inc2;

                /* Form the initial difference increments */
                inc0 = inc0 * (2 * STEP_C0) + STEP_C0 * STEP_C0;
                inc1 = inc1 * (2 * STEP_C1) + STEP_C1 * STEP_C1;
                inc2 = inc2 * (2 * STEP_C2) + STEP_C2 * STEP_C2;

                /* Now loop over all cells in box, updating distance per Thomas method */
                bestIndex = 0;
                int colorIndex = 0;
                int xx0 = inc0;
                for (int ic0 = BOX_C0_ELEMS - 1; ic0 >= 0; ic0--)
                {
                    int dist1 = dist0;
                    int xx1 = inc1;
                    for (int ic1 = BOX_C1_ELEMS - 1; ic1 >= 0; ic1--)
                    {
                        int dist2 = dist1;
                        int xx2 = inc2;
                        for (int ic2 = BOX_C2_ELEMS - 1; ic2 >= 0; ic2--)
                        {
                            if (dist2 < bestdist[bestIndex])
                            {
                                bestdist[bestIndex] = dist2;
                                bestcolor[colorIndex] = (byte)icolor;
                            }

                            dist2 += xx2;
                            xx2 += 2 * STEP_C2 * STEP_C2;
                            bestIndex++;
                            colorIndex++;
                        }

                        dist1 += xx1;
                        xx1 += 2 * STEP_C1 * STEP_C1;
                    }

                    dist0 += xx0;
                    xx0 += 2 * STEP_C0 * STEP_C0;
                }
            }
        }

        /// <summary>
        /// Fill the inverse-colormap entries in the update box that contains
        /// histogram cell c0/c1/c2.  (Only that one cell MUST be filled, but
        /// we can fill as many others as we wish.)
        /// </summary>
        private void fill_inverse_cmap(int c0, int c1, int c2)
        {
            /* Convert cell coordinates to update box ID */
            c0 >>= BOX_C0_LOG;
            c1 >>= BOX_C1_LOG;
            c2 >>= BOX_C2_LOG;

            /* Compute true coordinates of update box's origin corner.
             * Actually we compute the coordinates of the center of the corner
             * histogram cell, which are the lower bounds of the volume we care about.
             */
            int minc0 = (c0 << BOX_C0_SHIFT) + ((1 << C0_SHIFT) >> 1);
            int minc1 = (c1 << BOX_C1_SHIFT) + ((1 << C1_SHIFT) >> 1);
            int minc2 = (c2 << BOX_C2_SHIFT) + ((1 << C2_SHIFT) >> 1);

            /* Determine which colormap entries are close enough to be candidates
             * for the nearest entry to some cell in the update box.
             */
            /* This array lists the candidate colormap indexes. */
            byte[] colorlist = new byte[MAXNUMCOLORS];
            int numcolors = find_nearby_colors(minc0, minc1, minc2, colorlist);

            /* Determine the actually nearest colors. */
            /* This array holds the actually closest colormap index for each cell. */
            byte[] bestcolor = new byte[BOX_C0_ELEMS * BOX_C1_ELEMS * BOX_C2_ELEMS];
            find_best_colors(minc0, minc1, minc2, numcolors, colorlist, bestcolor);

            /* Save the best color numbers (plus 1) in the main cache array */
            c0 <<= BOX_C0_LOG;      /* convert ID back to base cell indexes */
            c1 <<= BOX_C1_LOG;
            c2 <<= BOX_C2_LOG;
            int bestcolorIndex = 0;
            for (int ic0 = 0; ic0 < BOX_C0_ELEMS; ic0++)
            {
                for (int ic1 = 0; ic1 < BOX_C1_ELEMS; ic1++)
                {
                    int histogramIndex = (c1 + ic1) * HIST_C2_ELEMS + c2;
                    for (int ic2 = 0; ic2 < BOX_C2_ELEMS; ic2++)
                    {
                        m_histogram[c0 + ic0][histogramIndex] = (ushort)((int)bestcolor[bestcolorIndex] + 1);
                        histogramIndex++;
                        bestcolorIndex++;
                    }
                }
            }
        }
    }
    #endregion

    #region ProgressiveHuffmanDecoder
    /// <summary>
    /// Expanded entropy decoder object for progressive Huffman decoding.
    /// 
    /// The savable_state sub-record contains fields that change within an MCU,
    /// but must not be updated permanently until we complete the MCU.
    /// </summary>
    class ProgressiveHuffmanDecoder : JpegEntropyDecoder
    {
        private class savable_state
        {
            //savable_state operator=(savable_state src);
            public int EOBRUN;            /* remaining EOBs in EOBRUN */
            public int[] last_dc_val = new int[JpegConstants.MaxComponentsInScan]; /* last DC coef for each component */

            public void Assign(savable_state ss)
            {
                EOBRUN = ss.EOBRUN;
                Buffer.BlockCopy(ss.last_dc_val, 0, last_dc_val, 0, last_dc_val.Length * sizeof(int));
            }
        }

        private enum MCUDecoder
        {
            mcu_DC_first_decoder,
            mcu_AC_first_decoder,
            mcu_DC_refine_decoder,
            mcu_AC_refine_decoder
        }

        private MCUDecoder m_decoder;

        /* These fields are loaded into local variables at start of each MCU.
        * In case of suspension, we exit WITHOUT updating them.
        */
        private SavedBitreadState m_bitstate;    /* Bit buffer at start of MCU */
        private savable_state m_saved = new savable_state();        /* Other state at start of MCU */

        /* These fields are NOT loaded into local working state. */
        private int m_restarts_to_go;    /* MCUs left in this restart interval */

        /* Pointers to derived tables (these workspaces have image lifespan) */
        private DerivedTable[] m_derived_tbls = new DerivedTable[JpegConstants.NumberOfHuffmanTables];

        private DerivedTable m_ac_derived_tbl; /* active table during an AC scan */

        public ProgressiveHuffmanDecoder(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Mark derived tables unallocated */
            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
                m_derived_tbls[i] = null;

            /* Create progression status table */
            cinfo.m_coef_bits = new int[cinfo.m_num_components][];
            for (int i = 0; i < cinfo.m_num_components; i++)
                cinfo.m_coef_bits[i] = new int[JpegConstants.DCTSize2];

            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                for (int i = 0; i < JpegConstants.DCTSize2; i++)
                    cinfo.m_coef_bits[ci][i] = -1;
            }
        }

        /// <summary>
        /// Initialize for a Huffman-compressed scan.
        /// </summary>
        public override void start_pass()
        {
            /* Validate scan parameters */
            bool bad = false;
            bool is_DC_band = (m_cinfo.m_Ss == 0);
            if (is_DC_band)
            {
                if (m_cinfo.m_Se != 0)
                    bad = true;
            }
            else
            {
                /* need not check Ss/Se < 0 since they came from unsigned bytes */
                if (m_cinfo.m_Ss > m_cinfo.m_Se || m_cinfo.m_Se >= JpegConstants.DCTSize2)
                    bad = true;

                /* AC scans may have only one component */
                if (m_cinfo.m_comps_in_scan != 1)
                    bad = true;
            }

            if (m_cinfo.m_Ah != 0)
            {
                /* Successive approximation refinement scan: must have Al = Ah-1. */
                if (m_cinfo.m_Al != m_cinfo.m_Ah - 1)
                    bad = true;
            }

            if (m_cinfo.m_Al > 13)
            {
                /* need not check for < 0 */
                bad = true;
            }

            /* Arguably the maximum Al value should be less than 13 for 8-bit precision,
             * but the spec doesn't say so, and we try to be liberal about what we
             * accept.  Note: large Al values could result in out-of-range DC
             * coefficients during early scans, leading to bizarre displays due to
             * overflows in the IDCT math.  But we won't crash.
             */
            if (bad)
                throw new Exception(String.Format("Invalid progressive parameters Ss={0} Se={1} Ah={2} Al={3}", m_cinfo.m_Ss, m_cinfo.m_Se, m_cinfo.m_Ah, m_cinfo.m_Al));

            /* Update progression status, and verify that scan order is legal.
             * Note that inter-scan inconsistencies are treated as warnings
             * not fatal errors ... not clear if this is right way to behave.
             */
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                int cindex = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]].Component_index;

                for (int coefi = m_cinfo.m_Ss; coefi <= m_cinfo.m_Se; coefi++)
                {
                    int expected = m_cinfo.m_coef_bits[cindex][coefi];
                    if (expected < 0)
                        expected = 0;

                    m_cinfo.m_coef_bits[cindex][coefi] = m_cinfo.m_Al;
                }
            }

            /* Select MCU decoding routine */
            if (m_cinfo.m_Ah == 0)
            {
                if (is_DC_band)
                    m_decoder = MCUDecoder.mcu_DC_first_decoder;
                else
                    m_decoder = MCUDecoder.mcu_AC_first_decoder;
            }
            else
            {
                if (is_DC_band)
                    m_decoder = MCUDecoder.mcu_DC_refine_decoder;
                else
                    m_decoder = MCUDecoder.mcu_AC_refine_decoder;
            }

            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]];
                /* Make sure requested tables are present, and compute derived tables.
                 * We may build same derived table more than once, but it's not expensive.
                 */
                if (is_DC_band)
                {
                    if (m_cinfo.m_Ah == 0)
                    {
                        /* DC refinement needs no table */
                        jpeg_make_d_derived_tbl(true, componentInfo.Dc_tbl_no, ref m_derived_tbls[componentInfo.Dc_tbl_no]);
                    }
                }
                else
                {
                    jpeg_make_d_derived_tbl(false, componentInfo.Ac_tbl_no, ref m_derived_tbls[componentInfo.Ac_tbl_no]);

                    /* remember the single active table */
                    m_ac_derived_tbl = m_derived_tbls[componentInfo.Ac_tbl_no];
                }

                /* Initialize DC predictions to 0 */
                m_saved.last_dc_val[ci] = 0;
            }

            /* Initialize bitread state variables */
            m_bitstate.bits_left = 0;
            m_bitstate.get_buffer = 0; /* unnecessary, but keeps Purify quiet */
            m_insufficient_data = false;

            /* Initialize private state variables */
            m_saved.EOBRUN = 0;

            /* Initialize restart counter */
            m_restarts_to_go = m_cinfo.m_restart_interval;
        }

        public override bool decode_mcu(JpegBlock[] MCU_data)
        {
            switch (m_decoder)
            {
                case MCUDecoder.mcu_DC_first_decoder:
                    return decode_mcu_DC_first(MCU_data);
                case MCUDecoder.mcu_AC_first_decoder:
                    return decode_mcu_AC_first(MCU_data);
                case MCUDecoder.mcu_DC_refine_decoder:
                    return decode_mcu_DC_refine(MCU_data);
                case MCUDecoder.mcu_AC_refine_decoder:
                    return decode_mcu_AC_refine(MCU_data);
            }

            throw new Exception("The specified MCUDecoder is not implemented!");
        }

        /*
         * Huffman MCU decoding.
         * Each of these routines decodes and returns one MCU's worth of
         * Huffman-compressed coefficients. 
         * The coefficients are reordered from zigzag order into natural array order,
         * but are not de-quantized.
         *
         * The i'th block of the MCU is stored into the block pointed to by
         * MCU_data[i].  WE ASSUME THIS AREA IS INITIALLY ZEROED BY THE CALLER.
         *
         * We return false if data source requested suspension.  In that case no
         * changes have been made to permanent state.  (Exception: some output
         * coefficients may already have been assigned.  This is harmless for
         * spectral selection, since we'll just re-assign them on the next call.
         * Successive approximation AC refinement has to be more careful, however.)
         */

        /// <summary>
        /// MCU decoding for DC initial scan (either spectral selection,
        /// or first pass of successive approximation).
        /// </summary>
        private bool decode_mcu_DC_first(JpegBlock[] MCU_data)
        {
            /* Process restart marker if needed; may have to suspend */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    if (!process_restart())
                        return false;
                }
            }

            /* If we've run out of data, just leave the MCU set to zeroes.
             * This way, we return uniform gray for the remainder of the segment.
             */
            if (!m_insufficient_data)
            {
                /* Load up working state */
                int get_buffer;
                int bits_left;
                WorkingBitreadState br_state = new WorkingBitreadState();
                BITREAD_LOAD_STATE(m_bitstate, out get_buffer, out bits_left, ref br_state);
                savable_state state = new savable_state();
                state.Assign(m_saved);

                /* Outer loop handles each block in the MCU */
                for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
                {
                    int ci = m_cinfo.m_MCU_membership[blkn];

                    /* Decode a single block's worth of coefficients */

                    /* Section F.2.2.1: decode the DC coefficient difference */
                    int s;
                    if (!HUFF_DECODE(out s, ref br_state, m_derived_tbls[m_cinfo.Comp_info[m_cinfo.m_cur_comp_info[ci]].Dc_tbl_no], ref get_buffer, ref bits_left))
                        return false;

                    if (s != 0)
                    {
                        if (!CHECK_BIT_BUFFER(ref br_state, s, ref get_buffer, ref bits_left))
                            return false;

                        int r = GET_BITS(s, get_buffer, ref bits_left);
                        s = HUFF_EXTEND(r, s);
                    }

                    /* Convert DC difference to actual value, update last_dc_val */
                    s += state.last_dc_val[ci];
                    state.last_dc_val[ci] = s;

                    /* Scale and output the coefficient (assumes jpeg_natural_order[0]=0) */
                    MCU_data[blkn][0] = (short)(s << m_cinfo.m_Al);
                }

                /* Completed MCU, so update state */
                BITREAD_SAVE_STATE(ref m_bitstate, get_buffer, bits_left);
                m_saved.Assign(state);
            }

            /* Account for restart interval (no-op if not using restarts) */
            m_restarts_to_go--;

            return true;
        }

        /// <summary>
        /// MCU decoding for AC initial scan (either spectral selection,
        /// or first pass of successive approximation).
        /// </summary>
        private bool decode_mcu_AC_first(JpegBlock[] MCU_data)
        {
            /* Process restart marker if needed; may have to suspend */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    if (!process_restart())
                        return false;
                }
            }

            /* If we've run out of data, just leave the MCU set to zeroes.
             * This way, we return uniform gray for the remainder of the segment.
             */
            if (!m_insufficient_data)
            {
                /* Load up working state.
                 * We can avoid loading/saving bitread state if in an EOB run.
                 */
                int EOBRUN = m_saved.EOBRUN; /* only part of saved state we need */

                /* There is always only one block per MCU */

                if (EOBRUN > 0)
                {
                    /* if it's a band of zeroes... */
                    /* ...process it now (we do nothing) */
                    EOBRUN--;
                }
                else
                {
                    int get_buffer;
                    int bits_left;
                    WorkingBitreadState br_state = new WorkingBitreadState();
                    BITREAD_LOAD_STATE(m_bitstate, out get_buffer, out bits_left, ref br_state);

                    for (int k = m_cinfo.m_Ss; k <= m_cinfo.m_Se; k++)
                    {
                        int s;
                        if (!HUFF_DECODE(out s, ref br_state, m_ac_derived_tbl, ref get_buffer, ref bits_left))
                            return false;

                        int r = s >> 4;
                        s &= 15;
                        if (s != 0)
                        {
                            k += r;

                            if (!CHECK_BIT_BUFFER(ref br_state, s, ref get_buffer, ref bits_left))
                                return false;

                            r = GET_BITS(s, get_buffer, ref bits_left);
                            s = HUFF_EXTEND(r, s);

                            /* Scale and output coefficient in natural (dezigzagged) order */
                            MCU_data[0][JpegUtils.jpeg_natural_order[k]] = (short)(s << m_cinfo.m_Al);
                        }
                        else
                        {
                            if (r == 15)
                            {
                                /* ZRL */
                                k += 15;        /* skip 15 zeroes in band */
                            }
                            else
                            {
                                /* EOBr, run length is 2^r + appended bits */
                                EOBRUN = 1 << r;
                                if (r != 0)
                                {
                                    /* EOBr, r > 0 */
                                    if (!CHECK_BIT_BUFFER(ref br_state, r, ref get_buffer, ref bits_left))
                                        return false;

                                    r = GET_BITS(r, get_buffer, ref bits_left);
                                    EOBRUN += r;
                                }

                                EOBRUN--;       /* this band is processed at this moment */
                                break;      /* force end-of-band */
                            }
                        }
                    }

                    BITREAD_SAVE_STATE(ref m_bitstate, get_buffer, bits_left);
                }

                /* Completed MCU, so update state */
                m_saved.EOBRUN = EOBRUN; /* only part of saved state we need */
            }

            /* Account for restart interval (no-op if not using restarts) */
            m_restarts_to_go--;

            return true;
        }

        /// <summary>
        /// MCU decoding for DC successive approximation refinement scan.
        /// Note: we assume such scans can be multi-component, although the spec
        /// is not very clear on the point.
        /// </summary>
        private bool decode_mcu_DC_refine(JpegBlock[] MCU_data)
        {
            /* Process restart marker if needed; may have to suspend */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    if (!process_restart())
                        return false;
                }
            }

            /* Not worth the cycles to check insufficient_data here,
             * since we will not change the data anyway if we read zeroes.
             */

            /* Load up working state */
            int get_buffer;
            int bits_left;
            WorkingBitreadState br_state = new WorkingBitreadState();
            BITREAD_LOAD_STATE(m_bitstate, out get_buffer, out bits_left, ref br_state);

            /* Outer loop handles each block in the MCU */

            for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
            {
                /* Encoded data is simply the next bit of the two's-complement DC value */
                if (!CHECK_BIT_BUFFER(ref br_state, 1, ref get_buffer, ref bits_left))
                    return false;

                if (GET_BITS(1, get_buffer, ref bits_left) != 0)
                {
                    /* 1 in the bit position being coded */
                    MCU_data[blkn][0] |= (short)(1 << m_cinfo.m_Al);
                }

                /* Note: since we use |=, repeating the assignment later is safe */
            }

            /* Completed MCU, so update state */
            BITREAD_SAVE_STATE(ref m_bitstate, get_buffer, bits_left);

            /* Account for restart interval (no-op if not using restarts) */
            m_restarts_to_go--;

            return true;
        }

        // There is always only one block per MCU
        private bool decode_mcu_AC_refine(JpegBlock[] MCU_data)
        {
            int p1 = 1 << m_cinfo.m_Al;    /* 1 in the bit position being coded */
            int m1 = -1 << m_cinfo.m_Al; /* -1 in the bit position being coded */

            /* Process restart marker if needed; may have to suspend */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    if (!process_restart())
                        return false;
                }
            }

            /* If we've run out of data, don't modify the MCU.
             */
            if (!m_insufficient_data)
            {
                /* Load up working state */
                int get_buffer;
                int bits_left;
                WorkingBitreadState br_state = new WorkingBitreadState();
                BITREAD_LOAD_STATE(m_bitstate, out get_buffer, out bits_left, ref br_state);
                int EOBRUN = m_saved.EOBRUN; /* only part of saved state we need */

                /* If we are forced to suspend, we must undo the assignments to any newly
                 * nonzero coefficients in the block, because otherwise we'd get confused
                 * next time about which coefficients were already nonzero.
                 * But we need not undo addition of bits to already-nonzero coefficients;
                 * instead, we can test the current bit to see if we already did it.
                 */
                int num_newnz = 0;
                int[] newnz_pos = new int[JpegConstants.DCTSize2];

                /* initialize coefficient loop counter to start of band */
                int k = m_cinfo.m_Ss;

                if (EOBRUN == 0)
                {
                    for (; k <= m_cinfo.m_Se; k++)
                    {
                        int s;
                        if (!HUFF_DECODE(out s, ref br_state, m_ac_derived_tbl, ref get_buffer, ref bits_left))
                        {
                            undo_decode_mcu_AC_refine(MCU_data, newnz_pos, num_newnz);
                            return false;
                        }

                        int r = s >> 4;
                        s &= 15;
                        if (s != 0)
                        {
                            if (!CHECK_BIT_BUFFER(ref br_state, 1, ref get_buffer, ref bits_left))
                            {
                                undo_decode_mcu_AC_refine(MCU_data, newnz_pos, num_newnz);
                                return false;
                            }

                            if (GET_BITS(1, get_buffer, ref bits_left) != 0)
                            {
                                /* newly nonzero coef is positive */
                                s = p1;
                            }
                            else
                            {
                                /* newly nonzero coef is negative */
                                s = m1;
                            }
                        }
                        else
                        {
                            if (r != 15)
                            {
                                EOBRUN = 1 << r;    /* EOBr, run length is 2^r + appended bits */
                                if (r != 0)
                                {
                                    if (!CHECK_BIT_BUFFER(ref br_state, r, ref get_buffer, ref bits_left))
                                    {
                                        undo_decode_mcu_AC_refine(MCU_data, newnz_pos, num_newnz);
                                        return false;
                                    }

                                    r = GET_BITS(r, get_buffer, ref bits_left);
                                    EOBRUN += r;
                                }
                                break;      /* rest of block is handled by EOB logic */
                            }
                            /* note s = 0 for processing ZRL */
                        }
                        /* Advance over already-nonzero coefs and r still-zero coefs,
                         * appending correction bits to the nonzeroes.  A correction bit is 1
                         * if the absolute value of the coefficient must be increased.
                         */
                        do
                        {
                            int blockIndex = JpegUtils.jpeg_natural_order[k];
                            short thiscoef = MCU_data[0][blockIndex];
                            if (thiscoef != 0)
                            {
                                if (!CHECK_BIT_BUFFER(ref br_state, 1, ref get_buffer, ref bits_left))
                                {
                                    undo_decode_mcu_AC_refine(MCU_data, newnz_pos, num_newnz);
                                    return false;
                                }

                                if (GET_BITS(1, get_buffer, ref bits_left) != 0)
                                {
                                    if ((thiscoef & p1) == 0)
                                    {
                                        /* do nothing if already set it */
                                        if (thiscoef >= 0)
                                            MCU_data[0][blockIndex] += (short)p1;
                                        else
                                            MCU_data[0][blockIndex] += (short)m1;
                                    }
                                }
                            }
                            else
                            {
                                if (--r < 0)
                                    break;      /* reached target zero coefficient */
                            }

                            k++;
                        }
                        while (k <= m_cinfo.m_Se);

                        if (s != 0)
                        {
                            int pos = JpegUtils.jpeg_natural_order[k];

                            /* Output newly nonzero coefficient */
                            MCU_data[0][pos] = (short)s;

                            /* Remember its position in case we have to suspend */
                            newnz_pos[num_newnz++] = pos;
                        }
                    }
                }

                if (EOBRUN > 0)
                {
                    /* Scan any remaining coefficient positions after the end-of-band
                     * (the last newly nonzero coefficient, if any).  Append a correction
                     * bit to each already-nonzero coefficient.  A correction bit is 1
                     * if the absolute value of the coefficient must be increased.
                     */
                    for (; k <= m_cinfo.m_Se; k++)
                    {
                        int blockIndex = JpegUtils.jpeg_natural_order[k];
                        short thiscoef = MCU_data[0][blockIndex];
                        if (thiscoef != 0)
                        {
                            if (!CHECK_BIT_BUFFER(ref br_state, 1, ref get_buffer, ref bits_left))
                            {
                                //undo_decode_mcu_AC_refine(MCU_data[0], newnz_pos, num_newnz);
                                undo_decode_mcu_AC_refine(MCU_data, newnz_pos, num_newnz);
                                return false;
                            }

                            if (GET_BITS(1, get_buffer, ref bits_left) != 0)
                            {
                                if ((thiscoef & p1) == 0)
                                {
                                    /* do nothing if already changed it */
                                    if (thiscoef >= 0)
                                        MCU_data[0][blockIndex] += (short)p1;
                                    else
                                        MCU_data[0][blockIndex] += (short)m1;
                                }
                            }
                        }
                    }

                    /* Count one block completed in EOB run */
                    EOBRUN--;
                }

                /* Completed MCU, so update state */
                BITREAD_SAVE_STATE(ref m_bitstate, get_buffer, bits_left);
                m_saved.EOBRUN = EOBRUN; /* only part of saved state we need */
            }

            /* Account for restart interval (no-op if not using restarts) */
            m_restarts_to_go--;

            return true;
        }

        /// <summary>
        /// Check for a restart marker and resynchronize decoder.
        /// Returns false if must suspend.
        /// </summary>
        private bool process_restart()
        {
            /* Throw away any unused bits remaining in bit buffer; */
            /* include any full bytes in next_marker's count of discarded bytes */
            m_cinfo.m_marker.SkipBytes(m_bitstate.bits_left / 8);
            m_bitstate.bits_left = 0;

            /* Advance past the RSTn marker */
            if (!m_cinfo.m_marker.read_restart_marker())
                return false;

            /* Re-initialize DC predictions to 0 */
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                m_saved.last_dc_val[ci] = 0;

            /* Re-init EOB run count, too */
            m_saved.EOBRUN = 0;

            /* Reset restart counter */
            m_restarts_to_go = m_cinfo.m_restart_interval;

            /* Reset out-of-data flag, unless read_restart_marker left us smack up
             * against a marker.  In that case we will end up treating the next data
             * segment as empty, and we can avoid producing bogus output pixels by
             * leaving the flag set.
             */
            if (m_cinfo.m_unread_marker == 0)
                m_insufficient_data = false;

            return true;
        }

        /// <summary>
        /// MCU decoding for AC successive approximation refinement scan.
        /// </summary>
        private static void undo_decode_mcu_AC_refine(JpegBlock[] block, int[] newnz_pos, int num_newnz)
        {
            /* Re-zero any output coefficients that we made newly nonzero */
            while (num_newnz > 0)
                block[0][newnz_pos[--num_newnz]] = 0;
        }
    }
    #endregion

    #region ProgressiveHuffmanEncoder
    /// <summary>
    /// Expanded entropy encoder object for progressive Huffman encoding.
    /// </summary>
    class ProgressiveHuffmanEncoder : JpegEntropyEncoder
    {
        private enum MCUEncoder
        {
            mcu_DC_first_encoder,
            mcu_AC_first_encoder,
            mcu_DC_refine_encoder,
            mcu_AC_refine_encoder
        }

        /* MAX_CORR_BITS is the number of bits the AC refinement correction-bit
         * buffer can hold.  Larger sizes may slightly improve compression, but
         * 1000 is already well into the realm of overkill.
         * The minimum safe size is 64 bits.
         */
        private const int MAX_CORR_BITS = 1000; /* Max # of correction bits I can buffer */

        private MCUEncoder m_MCUEncoder;

        /* Mode flag: true for optimization, false for actual data output */
        private bool m_gather_statistics;

        // Bit-level coding status.
        private int m_put_buffer;       /* current bit-accumulation buffer */
        private int m_put_bits;           /* # of bits now in it */

        /* Coding status for DC components */
        private int[] m_last_dc_val = new int[JpegConstants.MaxComponentsInScan]; /* last DC coef for each component */

        /* Coding status for AC components */
        private int m_ac_tbl_no;      /* the table number of the single component */
        private int m_EOBRUN;        /* run length of EOBs */
        private int m_BE;        /* # of buffered correction bits before MCU */
        private char[] m_bit_buffer;       /* buffer for correction bits (1 per char) */
        /* packing correction bits tightly would save some space but cost time... */

        private int m_restarts_to_go;    /* MCUs left in this restart interval */
        private int m_next_restart_num;       /* next restart number to write (0-7) */

        /* Pointers to derived tables (these workspaces have image lifespan).
        * Since any one scan codes only DC or only AC, we only need one set
        * of tables, not one for DC and one for AC.
        */
        private c_derived_tbl[] m_derived_tbls = new c_derived_tbl[JpegConstants.NumberOfHuffmanTables];

        /* Statistics tables for optimization; again, one set is enough */
        private long[][] m_count_ptrs = new long[JpegConstants.NumberOfHuffmanTables][];

        public ProgressiveHuffmanEncoder(JpegCompressor cinfo)
        {
            m_cinfo = cinfo;

            /* Mark tables unallocated */
            for (int i = 0; i < JpegConstants.NumberOfHuffmanTables; i++)
            {
                m_derived_tbls[i] = null;
                m_count_ptrs[i] = null;
            }
        }

        // Initialize for a Huffman-compressed scan using progressive JPEG.
        public override void start_pass(bool gather_statistics)
        {
            m_gather_statistics = gather_statistics;

            /* We assume the scan parameters are already validated. */

            /* Select execution routines */
            bool is_DC_band = (m_cinfo.m_Ss == 0);
            if (m_cinfo.m_Ah == 0)
            {
                if (is_DC_band)
                    m_MCUEncoder = MCUEncoder.mcu_DC_first_encoder;
                else
                    m_MCUEncoder = MCUEncoder.mcu_AC_first_encoder;
            }
            else
            {
                if (is_DC_band)
                {
                    m_MCUEncoder = MCUEncoder.mcu_DC_refine_encoder;
                }
                else
                {
                    m_MCUEncoder = MCUEncoder.mcu_AC_refine_encoder;

                    /* AC refinement needs a correction bit buffer */
                    if (m_bit_buffer == null)
                        m_bit_buffer = new char[MAX_CORR_BITS];
                }
            }

            /* Only DC coefficients may be interleaved, so m_cinfo.comps_in_scan = 1
             * for AC coefficients.
             */
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];

                /* Initialize DC predictions to 0 */
                m_last_dc_val[ci] = 0;

                /* Get table index */
                int tbl;
                if (is_DC_band)
                {
                    if (m_cinfo.m_Ah != 0) /* DC refinement needs no table */
                        continue;

                    tbl = componentInfo.Dc_tbl_no;
                }
                else
                {
                    m_ac_tbl_no = componentInfo.Ac_tbl_no;
                    tbl = componentInfo.Ac_tbl_no;
                }

                if (m_gather_statistics)
                {
                    /* Check for invalid table index */
                    /* (make_c_derived_tbl does this in the other path) */
                    if (tbl < 0 || tbl >= JpegConstants.NumberOfHuffmanTables)
                        throw new Exception(String.Format("Huffman table 0x{0:X2} was not defined", tbl));

                    /* Allocate and zero the statistics tables */
                    /* Note that jpeg_gen_optimal_table expects 257 entries in each table! */
                    if (m_count_ptrs[tbl] == null)
                        m_count_ptrs[tbl] = new long[257];

                    Array.Clear(m_count_ptrs[tbl], 0, 257);
                }
                else
                {
                    /* Compute derived values for Huffman table */
                    /* We may do this more than once for a table, but it's not expensive */
                    jpeg_make_c_derived_tbl(is_DC_band, tbl, ref m_derived_tbls[tbl]);
                }
            }

            /* Initialize AC stuff */
            m_EOBRUN = 0;
            m_BE = 0;

            /* Initialize bit buffer to empty */
            m_put_buffer = 0;
            m_put_bits = 0;

            /* Initialize restart stuff */
            m_restarts_to_go = m_cinfo.m_restart_interval;
            m_next_restart_num = 0;
        }

        public override bool encode_mcu(JpegBlock[][] MCU_data)
        {
            switch (m_MCUEncoder)
            {
                case MCUEncoder.mcu_DC_first_encoder:
                    return encode_mcu_DC_first(MCU_data);
                case MCUEncoder.mcu_AC_first_encoder:
                    return encode_mcu_AC_first(MCU_data);
                case MCUEncoder.mcu_DC_refine_encoder:
                    return encode_mcu_DC_refine(MCU_data);
                case MCUEncoder.mcu_AC_refine_encoder:
                    return encode_mcu_AC_refine(MCU_data);
            }

            throw new Exception("The specified MCUEncoder is not implemented.");
        }

        public override void finish_pass()
        {
            if (m_gather_statistics)
                finish_pass_gather_phuff();
            else
                finish_pass_phuff();
        }

        /// <summary>
        /// MCU encoding for DC initial scan (either spectral selection,
        /// or first pass of successive approximation).
        /// </summary>
        private bool encode_mcu_DC_first(JpegBlock[][] MCU_data)
        {
            /* Emit restart marker if needed */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                    emit_restart(m_next_restart_num);
            }

            /* Encode the MCU data blocks */
            for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
            {
                /* Compute the DC value after the required point transform by Al.
                 * This is simply an arithmetic right shift.
                 */
                int temp2 = IRIGHT_SHIFT(MCU_data[blkn][0][0], m_cinfo.m_Al);

                /* DC differences are figured on the point-transformed values. */
                int ci = m_cinfo.m_MCU_membership[blkn];
                int temp = temp2 - m_last_dc_val[ci];
                m_last_dc_val[ci] = temp2;

                /* Encode the DC coefficient difference per section G.1.2.1 */
                temp2 = temp;
                if (temp < 0)
                {
                    /* temp is abs value of input */
                    temp = -temp;

                    /* For a negative input, want temp2 = bitwise complement of abs(input) */
                    /* This code assumes we are on a two's complement machine */
                    temp2--;
                }

                /* Find the number of bits needed for the magnitude of the coefficient */
                int nbits = 0;
                while (temp != 0)
                {
                    nbits++;
                    temp >>= 1;
                }

                /* Check for out-of-range coefficient values.
                 * Since we're encoding a difference, the range limit is twice as much.
                 */
                if (nbits > MAX_HUFFMAN_COEF_BITS + 1)
                    throw new Exception("DCT coefficient out of range");

                /* Count/emit the Huffman-coded symbol for the number of bits */
                emit_symbol(m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]].Dc_tbl_no, nbits);

                /* Emit that number of bits of the value, if positive, */
                /* or the complement of its magnitude, if negative. */
                if (nbits != 0)
                {
                    /* emit_bits rejects calls with size 0 */
                    emit_bits(temp2, nbits);
                }
            }

            /* Update restart-interval state too */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    m_restarts_to_go = m_cinfo.m_restart_interval;
                    m_next_restart_num++;
                    m_next_restart_num &= 7;
                }

                m_restarts_to_go--;
            }

            return true;
        }

        /// <summary>
        /// MCU encoding for AC initial scan (either spectral selection,
        /// or first pass of successive approximation).
        /// </summary>
        private bool encode_mcu_AC_first(JpegBlock[][] MCU_data)
        {
            /* Emit restart marker if needed */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                    emit_restart(m_next_restart_num);
            }

            /* Encode the AC coefficients per section G.1.2.2, fig. G.3 */
            /* r = run length of zeros */
            int r = 0;
            for (int k = m_cinfo.m_Ss; k <= m_cinfo.m_Se; k++)
            {
                int temp = MCU_data[0][0][JpegUtils.jpeg_natural_order[k]];
                if (temp == 0)
                {
                    r++;
                    continue;
                }

                /* We must apply the point transform by Al.  For AC coefficients this
                 * is an integer division with rounding towards 0.  To do this portably
                 * in C, we shift after obtaining the absolute value; so the code is
                 * interwoven with finding the abs value (temp) and output bits (temp2).
                 */
                int temp2;
                if (temp < 0)
                {
                    temp = -temp;       /* temp is abs value of input */
                    temp >>= m_cinfo.m_Al;        /* apply the point transform */
                    /* For a negative coef, want temp2 = bitwise complement of abs(coef) */
                    temp2 = ~temp;
                }
                else
                {
                    temp >>= m_cinfo.m_Al;        /* apply the point transform */
                    temp2 = temp;
                }

                /* Watch out for case that nonzero coef is zero after point transform */
                if (temp == 0)
                {
                    r++;
                    continue;
                }

                /* Emit any pending EOBRUN */
                if (m_EOBRUN > 0)
                    emit_eobrun();

                /* if run length > 15, must emit special run-length-16 codes (0xF0) */
                while (r > 15)
                {
                    emit_symbol(m_ac_tbl_no, 0xF0);
                    r -= 16;
                }

                /* Find the number of bits needed for the magnitude of the coefficient */
                int nbits = 1;          /* there must be at least one 1 bit */
                while ((temp >>= 1) != 0)
                    nbits++;

                /* Check for out-of-range coefficient values */
                if (nbits > MAX_HUFFMAN_COEF_BITS)
                    throw new Exception("DCT coefficient out of range");

                /* Count/emit Huffman symbol for run length / number of bits */
                emit_symbol(m_ac_tbl_no, (r << 4) + nbits);

                /* Emit that number of bits of the value, if positive, */
                /* or the complement of its magnitude, if negative. */
                emit_bits(temp2, nbits);

                r = 0;          /* reset zero run length */
            }

            if (r > 0)
            {
                /* If there are trailing zeroes, */
                m_EOBRUN++;      /* count an EOB */
                if (m_EOBRUN == 0x7FFF)
                    emit_eobrun();   /* force it out to avoid overflow */
            }

            /* Update restart-interval state too */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    m_restarts_to_go = m_cinfo.m_restart_interval;
                    m_next_restart_num++;
                    m_next_restart_num &= 7;
                }
                m_restarts_to_go--;
            }

            return true;
        }

        /// <summary>
        /// MCU encoding for DC successive approximation refinement scan.
        /// Note: we assume such scans can be multi-component, although the spec
        /// is not very clear on the point.
        /// </summary>
        private bool encode_mcu_DC_refine(JpegBlock[][] MCU_data)
        {
            /* Emit restart marker if needed */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                    emit_restart(m_next_restart_num);
            }

            /* Encode the MCU data blocks */
            for (int blkn = 0; blkn < m_cinfo.m_blocks_in_MCU; blkn++)
            {
                /* We simply emit the Al'th bit of the DC coefficient value. */
                int temp = MCU_data[blkn][0][0];
                emit_bits(temp >> m_cinfo.m_Al, 1);
            }

            /* Update restart-interval state too */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    m_restarts_to_go = m_cinfo.m_restart_interval;
                    m_next_restart_num++;
                    m_next_restart_num &= 7;
                }
                m_restarts_to_go--;
            }

            return true;
        }

        /// <summary>
        /// MCU encoding for AC successive approximation refinement scan.
        /// </summary>
        private bool encode_mcu_AC_refine(JpegBlock[][] MCU_data)
        {
            /* Emit restart marker if needed */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                    emit_restart(m_next_restart_num);
            }

            /* Encode the MCU data block */

            /* It is convenient to make a pre-pass to determine the transformed
             * coefficients' absolute values and the EOB position.
             */
            int EOB = 0;
            int[] absvalues = new int[JpegConstants.DCTSize2];
            for (int k = m_cinfo.m_Ss; k <= m_cinfo.m_Se; k++)
            {
                int temp = MCU_data[0][0][JpegUtils.jpeg_natural_order[k]];

                /* We must apply the point transform by Al.  For AC coefficients this
                 * is an integer division with rounding towards 0.  To do this portably
                 * in C, we shift after obtaining the absolute value.
                 */
                if (temp < 0)
                    temp = -temp;       /* temp is abs value of input */

                temp >>= m_cinfo.m_Al;        /* apply the point transform */
                absvalues[k] = temp;    /* save abs value for main pass */

                if (temp == 1)
                {
                    /* EOB = index of last newly-nonzero coef */
                    EOB = k;
                }
            }

            /* Encode the AC coefficients per section G.1.2.3, fig. G.7 */

            int r = 0;          /* r = run length of zeros */
            int BR = 0;         /* BR = count of buffered bits added now */
            int bitBufferOffset = m_BE; /* Append bits to buffer */

            for (int k = m_cinfo.m_Ss; k <= m_cinfo.m_Se; k++)
            {
                int temp = absvalues[k];
                if (temp == 0)
                {
                    r++;
                    continue;
                }

                /* Emit any required ZRLs, but not if they can be folded into EOB */
                while (r > 15 && k <= EOB)
                {
                    /* emit any pending EOBRUN and the BE correction bits */
                    emit_eobrun();

                    /* Emit ZRL */
                    emit_symbol(m_ac_tbl_no, 0xF0);
                    r -= 16;

                    /* Emit buffered correction bits that must be associated with ZRL */
                    emit_buffered_bits(bitBufferOffset, BR);
                    bitBufferOffset = 0;/* BE bits are gone now */
                    BR = 0;
                }

                /* If the coef was previously nonzero, it only needs a correction bit.
                 * NOTE: a straight translation of the spec's figure G.7 would suggest
                 * that we also need to test r > 15.  But if r > 15, we can only get here
                 * if k > EOB, which implies that this coefficient is not 1.
                 */
                if (temp > 1)
                {
                    /* The correction bit is the next bit of the absolute value. */
                    m_bit_buffer[bitBufferOffset + BR] = (char)(temp & 1);
                    BR++;
                    continue;
                }

                /* Emit any pending EOBRUN and the BE correction bits */
                emit_eobrun();

                /* Count/emit Huffman symbol for run length / number of bits */
                emit_symbol(m_ac_tbl_no, (r << 4) + 1);

                /* Emit output bit for newly-nonzero coef */
                temp = (MCU_data[0][0][JpegUtils.jpeg_natural_order[k]] < 0) ? 0 : 1;
                emit_bits(temp, 1);

                /* Emit buffered correction bits that must be associated with this code */
                emit_buffered_bits(bitBufferOffset, BR);
                bitBufferOffset = 0;/* BE bits are gone now */
                BR = 0;
                r = 0;          /* reset zero run length */
            }

            if (r > 0 || BR > 0)
            {
                /* If there are trailing zeroes, */
                m_EOBRUN++;      /* count an EOB */
                m_BE += BR;      /* concat my correction bits to older ones */

                /* We force out the EOB if we risk either:
                 * 1. overflow of the EOB counter;
                 * 2. overflow of the correction bit buffer during the next MCU.
                 */
                if (m_EOBRUN == 0x7FFF || m_BE > (MAX_CORR_BITS - JpegConstants.DCTSize2 + 1))
                    emit_eobrun();
            }

            /* Update restart-interval state too */
            if (m_cinfo.m_restart_interval != 0)
            {
                if (m_restarts_to_go == 0)
                {
                    m_restarts_to_go = m_cinfo.m_restart_interval;
                    m_next_restart_num++;
                    m_next_restart_num &= 7;
                }
                m_restarts_to_go--;
            }

            return true;
        }

        /// <summary>
        /// Finish up at the end of a Huffman-compressed progressive scan.
        /// </summary>
        private void finish_pass_phuff()
        {
            /* Flush out any buffered data */
            emit_eobrun();
            flush_bits();
        }

        /// <summary>
        /// Finish up a statistics-gathering pass and create the new Huffman tables.
        /// </summary>
        private void finish_pass_gather_phuff()
        {
            /* Flush out buffered data (all we care about is counting the EOB symbol) */
            emit_eobrun();

            /* It's important not to apply jpeg_gen_optimal_table more than once
             * per table, because it clobbers the input frequency counts!
             */
            bool[] did = new bool[JpegConstants.NumberOfHuffmanTables];

            bool is_DC_band = (m_cinfo.m_Ss == 0);
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];
                int tbl = componentInfo.Ac_tbl_no;

                if (is_DC_band)
                {
                    if (m_cinfo.m_Ah != 0) /* DC refinement needs no table */
                        continue;

                    tbl = componentInfo.Dc_tbl_no;
                }

                if (!did[tbl])
                {
                    JpegHuffmanTable htblptr = null;
                    if (is_DC_band)
                    {
                        if (m_cinfo.m_dc_huff_tbl_ptrs[tbl] == null)
                            m_cinfo.m_dc_huff_tbl_ptrs[tbl] = new JpegHuffmanTable();

                        htblptr = m_cinfo.m_dc_huff_tbl_ptrs[tbl];
                    }
                    else
                    {
                        if (m_cinfo.m_ac_huff_tbl_ptrs[tbl] == null)
                            m_cinfo.m_ac_huff_tbl_ptrs[tbl] = new JpegHuffmanTable();

                        htblptr = m_cinfo.m_ac_huff_tbl_ptrs[tbl];
                    }

                    jpeg_gen_optimal_table(htblptr, m_count_ptrs[tbl]);
                    did[tbl] = true;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Outputting bytes to the file.
        // NB: these must be called only when actually outputting,
        // that is, entropy.gather_statistics == false.

        // Emit a byte
        private void emit_byte(int val)
        {
            m_cinfo.m_dest.emit_byte(val);
        }

        /// <summary>
        /// Outputting bits to the file
        /// 
        /// Only the right 24 bits of put_buffer are used; the valid bits are
        /// left-justified in this part.  At most 16 bits can be passed to emit_bits
        /// in one call, and we never retain more than 7 bits in put_buffer
        /// between calls, so 24 bits are sufficient.
        /// </summary>
        private void emit_bits(int code, int size)
        {
            // Emit some bits, unless we are in gather mode
            /* This routine is heavily used, so it's worth coding tightly. */
            int local_put_buffer = code;

            /* if size is 0, caller used an invalid Huffman table entry */
            if (size == 0)
                throw new Exception("Missing Huffman code table entry");

            if (m_gather_statistics)
            {
                /* do nothing if we're only getting stats */
                return;
            }

            local_put_buffer &= (1 << size) - 1; /* mask off any extra bits in code */

            m_put_bits += size;       /* new number of bits in buffer */

            local_put_buffer <<= 24 - m_put_bits; /* align incoming bits */

            local_put_buffer |= m_put_buffer; /* and merge with old buffer contents */

            while (m_put_bits >= 8)
            {
                int c = (local_put_buffer >> 16) & 0xFF;

                emit_byte(c);
                if (c == 0xFF)
                {
                    /* need to stuff a zero byte? */
                    emit_byte(0);
                }
                local_put_buffer <<= 8;
                m_put_bits -= 8;
            }

            m_put_buffer = local_put_buffer; /* update variables */
        }

        private void flush_bits()
        {
            emit_bits(0x7F, 7); /* fill any partial byte with ones */
            m_put_buffer = 0;     /* and reset bit-buffer to empty */
            m_put_bits = 0;
        }

        // Emit (or just count) a Huffman symbol.
        private void emit_symbol(int tbl_no, int symbol)
        {
            if (m_gather_statistics)
                m_count_ptrs[tbl_no][symbol]++;
            else
                emit_bits(m_derived_tbls[tbl_no].ehufco[symbol], m_derived_tbls[tbl_no].ehufsi[symbol]);
        }

        // Emit bits from a correction bit buffer.
        private void emit_buffered_bits(int offset, int nbits)
        {
            if (m_gather_statistics)
            {
                /* no real work */
                return;
            }

            for (int i = 0; i < nbits; i++)
                emit_bits(m_bit_buffer[offset + i], 1);
        }

        // Emit any pending EOBRUN symbol.
        private void emit_eobrun()
        {
            if (m_EOBRUN > 0)
            {
                /* if there is any pending EOBRUN */
                int temp = m_EOBRUN;
                int nbits = 0;
                while ((temp >>= 1) != 0)
                    nbits++;

                /* safety check: shouldn't happen given limited correction-bit buffer */
                if (nbits > 14)
                    throw new Exception("Missing Huffman code table entry");

                emit_symbol(m_ac_tbl_no, nbits << 4);
                if (nbits != 0)
                    emit_bits(m_EOBRUN, nbits);

                m_EOBRUN = 0;

                /* Emit any buffered correction bits */
                emit_buffered_bits(0, m_BE);
                m_BE = 0;
            }
        }

        // Emit a restart marker & resynchronize predictions.
        private void emit_restart(int restart_num)
        {
            emit_eobrun();

            if (!m_gather_statistics)
            {
                flush_bits();
                emit_byte(0xFF);
                emit_byte((int)(JpegMarkerType.RST0 + restart_num));
            }

            if (m_cinfo.m_Ss == 0)
            {
                /* Re-initialize DC predictions to 0 */
                for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                    m_last_dc_val[ci] = 0;
            }
            else
            {
                /* Re-initialize all AC-related fields to 0 */
                m_EOBRUN = 0;
                m_BE = 0;
            }
        }

        /// <summary>
        /// IRIGHT_SHIFT is like RIGHT_SHIFT, but works on int rather than int.
        /// We assume that int right shift is unsigned if int right shift is,
        /// which should be safe.
        /// </summary>
        private static int IRIGHT_SHIFT(int x, int shft)
        {
            return (x >> shft);
        }
    }
    #endregion

    #region RawImage
    class RawImage : IRawImage
    {
        private List<SampleRow> m_samples;
        private ColorSpace m_colorspace;

        private int m_currentRow = -1;

        internal RawImage(List<SampleRow> samples, ColorSpace colorspace)
        {
            if (samples == null)
                throw new ArgumentNullException("'samples' Cannot be null!");
            if (samples.Count <= 0)
                throw new Exception("No element specified in 'samples'");
            if (colorspace == ColorSpace.Unknown)
                throw new Exception("Unknown Color Space!");

            m_samples = samples;
            m_colorspace = colorspace;
        }

        public override int Width
        {
            get
            {
                return m_samples[0].Length;
            }
        }

        public override int Height
        {
            get
            {
                return m_samples.Count;
            }
        }

        public override ColorSpace Colorspace
        {
            get
            {
                return m_colorspace;
            }
        }

        public override int ComponentsPerPixel
        {
            get
            {
                return m_samples[0][0].ComponentCount;
            }
        }

        public override void BeginRead()
        {
            m_currentRow = 0;
        }

        public override byte[] GetPixelRow()
        {
            SampleRow row = m_samples[m_currentRow];
            List<byte> result = new List<byte>();
            for (int i = 0; i < row.Length; ++i)
            {
                Sample sample = row[i];
                for (int j = 0; j < sample.ComponentCount; ++j)
                    result.Add((byte)sample[j]);
            }
            ++m_currentRow;
            return result.ToArray();
        }

        public override void EndRead()
        {
        }
    }
    #endregion

    #region Sample
    /// <summary>
    /// Represents a "sample" (you can understand it as a "pixel") of image.
    /// </summary>
    /// <remarks>It's impossible to create an instance of this class directly, 
    /// but you can use existing samples through <see cref="SampleRow"/> collection. 
    /// Usual scenario is to get row of samples from the <see cref="JpegImage.GetRow"/> method.
    /// </remarks>
    public class Sample
    {
        private short[] m_components;
        private byte m_bitsPerComponent;

        internal Sample(BitStream bitStream, byte bitsPerComponent, byte componentCount)
        {
            if (bitStream == null)
                throw new ArgumentNullException("bitStream");

            if (bitsPerComponent <= 0 || bitsPerComponent > 16)
                throw new ArgumentOutOfRangeException("bitsPerComponent");

            if (componentCount <= 0 || componentCount > 5)
                throw new ArgumentOutOfRangeException("componentCount");

            m_bitsPerComponent = bitsPerComponent;

            m_components = new short[componentCount];
            for (short i = 0; i < componentCount; ++i)
                m_components[i] = (short)bitStream.Read(bitsPerComponent);
        }

        internal Sample(short[] components, byte bitsPerComponent)
        {
            if (components == null)
                throw new ArgumentNullException("components");

            if (components.Length == 0 || components.Length > 5)
                throw new ArgumentException("components must be not empty and contain less than 5 elements");

            if (bitsPerComponent <= 0 || bitsPerComponent > 16)
                throw new ArgumentOutOfRangeException("bitsPerComponent");

            m_bitsPerComponent = bitsPerComponent;

            m_components = new short[components.Length];
            Buffer.BlockCopy(components, 0, m_components, 0, components.Length * sizeof(short));
        }

        /// <summary>
        /// Gets the number of bits per color component.
        /// </summary>
        /// <value>The number of bits per color component.</value>
        public byte BitsPerComponent
        {
            get
            {
                return m_bitsPerComponent;
            }
        }

        /// <summary>
        /// Gets the number of color components.
        /// </summary>
        /// <value>The number of color components.</value>
        public byte ComponentCount
        {
            get
            {
                return (byte)m_components.Length;
            }
        }

        /// <summary>
        /// Gets the color component at the specified index.
        /// </summary>
        /// <param name="componentNumber">The number of color component.</param>
        /// <returns>Value of color component.</returns>
        public short this[int componentNumber]
        {
            get
            {
                return GetComponent(componentNumber);
            }
        }

        /// <summary>
        /// Gets the required color component.
        /// </summary>
        /// <param name="componentNumber">The number of color component.</param>
        /// <returns>Value of color component.</returns>
        public short GetComponent(int componentNumber)
        {
            return m_components[componentNumber];
        }
    }
    #endregion

    #region SampleRow
    /// <summary>
    /// Represents a row of image - collection of samples.
    /// </summary>
    public class SampleRow
    {
        private byte[] m_bytes;
        private Sample[] m_samples;

        /// <summary>
        /// Creates a row from raw samples data.
        /// </summary>
        /// <param name="row">Raw description of samples.<br/>
        /// You can pass collection with more than sampleCount samples - only sampleCount samples 
        /// will be parsed and all remaining bytes will be ignored.</param>
        /// <param name="sampleCount">The number of samples in row.</param>
        /// <param name="bitsPerComponent">The number of bits per component.</param>
        /// <param name="componentsPerSample">The number of components per sample.</param>
        public SampleRow(byte[] row, int sampleCount, byte bitsPerComponent, byte componentsPerSample)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            if (row.Length == 0)
                throw new ArgumentException("row is empty");

            if (sampleCount <= 0)
                throw new ArgumentOutOfRangeException("sampleCount");

            if (bitsPerComponent <= 0 || bitsPerComponent > 16)
                throw new ArgumentOutOfRangeException("bitsPerComponent");

            if (componentsPerSample <= 0 || componentsPerSample > 5)
                throw new ArgumentOutOfRangeException("componentsPerSample");

            m_bytes = row;

            using (BitStream bitStream = new BitStream(row))
            {
                m_samples = new Sample[sampleCount];
                for (int i = 0; i < sampleCount; ++i)
                    m_samples[i] = new Sample(bitStream, bitsPerComponent, componentsPerSample);
            }
        }

        /// <summary>
        /// Creates row from an array of components.
        /// </summary>
        /// <param name="sampleComponents">Array of color components.</param>
        /// <param name="bitsPerComponent">The number of bits per component.</param>
        /// <param name="componentsPerSample">The number of components per sample.</param>
        /// <remarks>The difference between this constructor and 
        /// <see cref="M:BitMiracle.LibJpeg.SampleRow.#ctor(System.Byte[],System.Int32,System.Byte,System.Byte)">another one</see> -
        /// this constructor accept an array of prepared color components whereas
        /// another constructor accept raw bytes and parse them.
        /// </remarks>
        internal SampleRow(short[] sampleComponents, byte bitsPerComponent, byte componentsPerSample)
        {
            if (sampleComponents == null)
                throw new ArgumentNullException("sampleComponents");

            if (sampleComponents.Length == 0)
                throw new ArgumentException("row is empty");

            if (bitsPerComponent <= 0 || bitsPerComponent > 16)
                throw new ArgumentOutOfRangeException("bitsPerComponent");

            if (componentsPerSample <= 0 || componentsPerSample > 5)
                throw new ArgumentOutOfRangeException("componentsPerSample");

            int sampleCount = sampleComponents.Length / componentsPerSample;
            m_samples = new Sample[sampleCount];
            for (int i = 0; i < sampleCount; ++i)
            {
                short[] components = new short[componentsPerSample];
                Buffer.BlockCopy(sampleComponents, i * componentsPerSample * sizeof(short), components, 0, componentsPerSample * sizeof(short));
                m_samples[i] = new Sample(components, bitsPerComponent);
            }

            using (BitStream bits = new BitStream())
            {
                for (int i = 0; i < sampleCount; ++i)
                {
                    for (int j = 0; j < componentsPerSample; ++j)
                        bits.Write(sampleComponents[i * componentsPerSample + j], bitsPerComponent);
                }

                m_bytes = new byte[bits.UnderlyingStream.Length];
                bits.UnderlyingStream.Seek(0, System.IO.SeekOrigin.Begin);
                bits.UnderlyingStream.Read(m_bytes, 0, (int)bits.UnderlyingStream.Length);
            }
        }


        /// <summary>
        /// Gets the number of samples in this row.
        /// </summary>
        /// <value>The number of samples.</value>
        public int Length
        {
            get
            {
                return m_samples.Length;
            }
        }


        /// <summary>
        /// Gets the sample at the specified index.
        /// </summary>
        /// <param name="sampleNumber">The number of sample.</param>
        /// <returns>The required sample.</returns>
        public Sample this[int sampleNumber]
        {
            get
            {
                return GetAt(sampleNumber);
            }
        }

        /// <summary>
        /// Gets the sample at the specified index.
        /// </summary>
        /// <param name="sampleNumber">The number of sample.</param>
        /// <returns>The required sample.</returns>
        public Sample GetAt(int sampleNumber)
        {
            return m_samples[sampleNumber];
        }

        /// <summary>
        /// Serializes this row to raw bytes.
        /// </summary>
        /// <returns>The row representation as array of bytes</returns>
        public byte[] ToBytes()
        {
            return m_bytes;
        }
    }
    #endregion

    #region SavedBitreadState
    /// <summary>
    /// Bitreading state saved across MCUs
    /// </summary>
    struct SavedBitreadState
    {
        public int get_buffer;    /* current bit-extraction buffer */
        public int bits_left;      /* # of unused bits in it */
    }
    #endregion

    #region SourceManagerImpl
    /// <summary>
    /// Expanded data source object for stdio input
    /// </summary>
    class SourceManagerImpl : Jpeg_Source
    {
        private const int INPUT_BUF_SIZE = 4096;

        private JpegDecompressor m_cinfo;

        private Stream m_infile;       /* source stream */
        private byte[] m_buffer;     /* start of buffer */
        private bool m_start_of_file; /* have we gotten any data yet? */

        /// <summary>
        /// Initialize source - called by jpeg_read_header
        /// before any data is actually read.
        /// </summary>
        public SourceManagerImpl(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;
            m_buffer = new byte[INPUT_BUF_SIZE];
        }

        public void Attach(Stream infile)
        {
            m_infile = infile;
            m_infile.Seek(0, SeekOrigin.Begin);
            initInternalBuffer(null, 0);
        }

        public override void init_source()
        {
            /* We reset the empty-input-file flag for each image,
             * but we don't clear the input buffer.
             * This is correct behavior for reading a series of images from one source.
             */
            m_start_of_file = true;
        }

        /// <summary>
        /// Fill the input buffer - called whenever buffer is emptied.
        /// 
        /// In typical applications, this should read fresh data into the buffer
        /// (ignoring the current state of next_input_byte and bytes_in_buffer),
        /// reset the pointer and count to the start of the buffer, and return true
        /// indicating that the buffer has been reloaded.  It is not necessary to
        /// fill the buffer entirely, only to obtain at least one more byte.
        /// 
        /// There is no such thing as an EOF return.  If the end of the file has been
        /// reached, the routine has a choice of ERREXIT() or inserting fake data into
        /// the buffer.  In most cases, generating a warning message and inserting a
        /// fake EOI marker is the best course of action --- this will allow the
        /// decompressor to output however much of the image is there.  However,
        /// the resulting error message is misleading if the real problem is an empty
        /// input file, so we handle that case specially.
        /// 
        /// In applications that need to be able to suspend compression due to input
        /// not being available yet, a false return indicates that no more data can be
        /// obtained right now, but more may be forthcoming later.  In this situation,
        /// the decompressor will return to its caller (with an indication of the
        /// number of scanlines it has read, if any).  The application should resume
        /// decompression after it has loaded more data into the input buffer.  Note
        /// that there are substantial restrictions on the use of suspension --- see
        /// the documentation.
        /// 
        /// When suspending, the decompressor will back up to a convenient restart point
        /// (typically the start of the current MCU). next_input_byte and bytes_in_buffer
        /// indicate where the restart point will be if the current call returns false.
        /// Data beyond this point must be rescanned after resumption, so move it to
        /// the front of the buffer rather than discarding it.
        /// </summary>
        public override bool fill_input_buffer()
        {
            int nbytes = m_infile.Read(m_buffer, 0, INPUT_BUF_SIZE);
            if (nbytes <= 0)
            {
                if (m_start_of_file) /* Treat empty input file as fatal error */
                    throw new Exception("The input file is empty!");

                /* Insert a fake EOI marker */
                m_buffer[0] = (byte)0xFF;
                m_buffer[1] = (byte)JpegMarkerType.EOI;
                nbytes = 2;
            }

            initInternalBuffer(m_buffer, nbytes);
            m_start_of_file = false;

            return true;
        }
    }
    #endregion

    #region TransCoefControllerImpl
    /// <summary>
    /// This is a special implementation of the coefficient
    /// buffer controller.  This is similar to jccoefct.c, but it handles only
    /// output from presupplied virtual arrays.  Furthermore, we generate any
    /// dummy padding blocks on-the-fly rather than expecting them to be present
    /// in the arrays.
    /// </summary>
    class TransCoefControllerImpl : JpegCompressorCoefController
    {
        private JpegCompressor m_cinfo;

        private int m_iMCU_row_num;    /* iMCU row # within image */
        private int m_mcu_ctr;     /* counts MCUs processed in current row */
        private int m_MCU_vert_offset;        /* counts MCU rows within iMCU row */
        private int m_MCU_rows_per_iMCU_row;  /* number of such rows needed */

        /* Virtual block array for each component. */
        private JpegVirtualArray<JpegBlock>[] m_whole_image;

        /* Workspace for constructing dummy blocks at right/bottom edges. */
        private JpegBlock[][] m_dummy_buffer = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU][];

        /// <summary>
        /// Initialize coefficient buffer controller.
        /// 
        /// Each passed coefficient array must be the right size for that
        /// coefficient: width_in_blocks wide and height_in_blocks high,
        /// with unit height at least v_samp_factor.
        /// </summary>
        public TransCoefControllerImpl(JpegCompressor cinfo, JpegVirtualArray<JpegBlock>[] coef_arrays)
        {
            m_cinfo = cinfo;

            /* Save pointer to virtual arrays */
            m_whole_image = coef_arrays;

            /* Allocate and pre-zero space for dummy DCT blocks. */
            JpegBlock[] buffer = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU];
            for (int i = 0; i < JpegConstants.CompressorMaxBlocksInMCU; i++)
                buffer[i] = new JpegBlock();

            for (int i = 0; i < JpegConstants.CompressorMaxBlocksInMCU; i++)
            {
                m_dummy_buffer[i] = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU - i];
                for (int j = i; j < JpegConstants.CompressorMaxBlocksInMCU; j++)
                    m_dummy_buffer[i][j - i] = buffer[j];
            }
        }

        /// <summary>
        /// Initialize for a processing pass.
        /// </summary>
        public virtual void start_pass(BufferMode pass_mode)
        {
            if (pass_mode != BufferMode.CrankDest)
                throw new Exception("Bogus buffer control mode");

            m_iMCU_row_num = 0;
            start_iMCU_row();
        }

        /// <summary>
        /// Process some data.
        /// We process the equivalent of one fully interleaved MCU row ("iMCU" row)
        /// per call, ie, v_samp_factor block rows for each component in the scan.
        /// The data is obtained from the virtual arrays and fed to the entropy coder.
        /// Returns true if the iMCU row is completed, false if suspended.
        /// 
        /// NB: input_buf is ignored; it is likely to be a null pointer.
        /// </summary>
        public virtual bool compress_data(byte[][][] input_buf)
        {
            /* Align the virtual buffers for the components used in this scan. */
            JpegBlock[][][] buffer = new JpegBlock[JpegConstants.MaxComponentsInScan][][];
            for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
            {
                JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];
                buffer[ci] = m_whole_image[componentInfo.Component_index].Access(
                    m_iMCU_row_num * componentInfo.V_samp_factor, componentInfo.V_samp_factor);
            }

            /* Loop to process one whole iMCU row */
            int last_MCU_col = m_cinfo.m_MCUs_per_row - 1;
            int last_iMCU_row = m_cinfo.m_total_iMCU_rows - 1;
            JpegBlock[][] MCU_buffer = new JpegBlock[JpegConstants.CompressorMaxBlocksInMCU][];
            for (int yoffset = m_MCU_vert_offset; yoffset < m_MCU_rows_per_iMCU_row; yoffset++)
            {
                for (int MCU_col_num = m_mcu_ctr; MCU_col_num < m_cinfo.m_MCUs_per_row; MCU_col_num++)
                {
                    /* Construct list of pointers to DCT blocks belonging to this MCU */
                    int blkn = 0;           /* index of current DCT block within MCU */
                    for (int ci = 0; ci < m_cinfo.m_comps_in_scan; ci++)
                    {
                        JpegComponent componentInfo = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[ci]];
                        int start_col = MCU_col_num * componentInfo.MCU_width;
                        int blockcnt = (MCU_col_num < last_MCU_col) ? componentInfo.MCU_width : componentInfo.last_col_width;
                        for (int yindex = 0; yindex < componentInfo.MCU_height; yindex++)
                        {
                            int xindex = 0;
                            if (m_iMCU_row_num < last_iMCU_row || yindex + yoffset < componentInfo.last_row_height)
                            {
                                /* Fill in pointers to real blocks in this row */
                                for (xindex = 0; xindex < blockcnt; xindex++)
                                {
                                    int bufLength = buffer[ci][yindex + yoffset].Length;
                                    int start = start_col + xindex;
                                    MCU_buffer[blkn] = new JpegBlock[bufLength - start];
                                    for (int j = start; j < bufLength; j++)
                                        MCU_buffer[blkn][j - start] = buffer[ci][yindex + yoffset][j];

                                    blkn++;
                                }
                            }
                            else
                            {
                                /* At bottom of image, need a whole row of dummy blocks */
                                xindex = 0;
                            }

                            /* Fill in any dummy blocks needed in this row.
                            * Dummy blocks are filled in the same way as in jccoefct.c:
                            * all zeroes in the AC entries, DC entries equal to previous
                            * block's DC value.  The init routine has already zeroed the
                            * AC entries, so we need only set the DC entries correctly.
                            */
                            for (; xindex < componentInfo.MCU_width; xindex++)
                            {
                                MCU_buffer[blkn] = m_dummy_buffer[blkn];
                                MCU_buffer[blkn][0][0] = MCU_buffer[blkn - 1][0][0];
                                blkn++;
                            }
                        }
                    }

                    /* Try to write the MCU. */
                    if (!m_cinfo.m_entropy.encode_mcu(MCU_buffer))
                    {
                        /* Suspension forced; update state counters and exit */
                        m_MCU_vert_offset = yoffset;
                        m_mcu_ctr = MCU_col_num;
                        return false;
                    }
                }

                /* Completed an MCU row, but perhaps not an iMCU row */
                m_mcu_ctr = 0;
            }

            /* Completed the iMCU row, advance counters for next one */
            m_iMCU_row_num++;
            start_iMCU_row();
            return true;
        }

        /// <summary>
        /// Reset within-iMCU-row counters for a new row
        /// </summary>
        private void start_iMCU_row()
        {
            /* In an interleaved scan, an MCU row is the same as an iMCU row.
            * In a noninterleaved scan, an iMCU row has v_samp_factor MCU rows.
            * But at the bottom of the image, process only what's left.
            */
            if (m_cinfo.m_comps_in_scan > 1)
            {
                m_MCU_rows_per_iMCU_row = 1;
            }
            else
            {
                if (m_iMCU_row_num < (m_cinfo.m_total_iMCU_rows - 1))
                    m_MCU_rows_per_iMCU_row = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[0]].V_samp_factor;
                else
                    m_MCU_rows_per_iMCU_row = m_cinfo.Component_info[m_cinfo.m_cur_comp_info[0]].last_row_height;
            }

            m_mcu_ctr = 0;
            m_MCU_vert_offset = 0;
        }
    }
    #endregion

    #region UpsamplerImpl
    class UpsamplerImpl : JpegUpsampler
    {
        private enum ComponentUpsampler
        {
            noop_upsampler,
            fullsize_upsampler,
            h2v1_fancy_upsampler,
            h2v1_upsampler,
            h2v2_fancy_upsampler,
            h2v2_upsampler,
            int_upsampler
        }

        private JpegDecompressor m_cinfo;

        /* Color conversion buffer.  When using separate upsampling and color
        * conversion steps, this buffer holds one upsampled row group until it
        * has been color converted and output.
        * Note: we do not allocate any storage for component(s) which are full-size,
        * ie do not need rescaling.  The corresponding entry of color_buf[] is
        * simply set to point to the input data array, thereby avoiding copying.
        */
        private ComponentBuffer[] m_color_buf = new ComponentBuffer[JpegConstants.MaxComponents];

        // used only for fullsize_upsampler mode
        private int[] m_perComponentOffsets = new int[JpegConstants.MaxComponents];

        /* Per-component upsampling method pointers */
        private ComponentUpsampler[] m_upsampleMethods = new ComponentUpsampler[JpegConstants.MaxComponents];
        private int m_currentComponent; // component being upsampled
        private int m_upsampleRowOffset;

        private int m_next_row_out;       /* counts rows emitted from color_buf */
        private int m_rows_to_go;  /* counts rows remaining in image */

        /* Height of an input row group for each component. */
        private int[] m_rowgroup_height = new int[JpegConstants.MaxComponents];

        /* These arrays save pixel expansion factors so that int_expand need not
        * re-compute them each time.  They are unused for other up-sampling methods.
        */
        private byte[] m_h_expand = new byte[JpegConstants.MaxComponents];
        private byte[] m_v_expand = new byte[JpegConstants.MaxComponents];

        public UpsamplerImpl(JpegDecompressor cinfo)
        {
            m_cinfo = cinfo;
            m_need_context_rows = false; /* until we find out differently */

            if (cinfo.m_CCIR601_sampling)    /* this isn't supported */
                throw new Exception("CCIR601 sampling not implemented yet");

            /* JpegDecompressorMainController doesn't support context rows when min_DCT_scaled_size = 1,
            * so don't ask for it.
            */
            bool do_fancy = cinfo.m_do_fancy_upsampling && cinfo.m_min_DCT_scaled_size > 1;

            /* Verify we can handle the sampling factors, select per-component methods,
            * and create storage as needed.
            */
            for (int ci = 0; ci < cinfo.m_num_components; ci++)
            {
                JpegComponent componentInfo = cinfo.Comp_info[ci];

                /* Compute size of an "input group" after IDCT scaling.  This many samples
                * are to be converted to max_h_samp_factor * max_v_samp_factor pixels.
                */
                int h_in_group = (componentInfo.H_samp_factor * componentInfo.DCT_scaled_size) / cinfo.m_min_DCT_scaled_size;
                int v_in_group = (componentInfo.V_samp_factor * componentInfo.DCT_scaled_size) / cinfo.m_min_DCT_scaled_size;
                int h_out_group = cinfo.m_max_h_samp_factor;
                int v_out_group = cinfo.m_max_v_samp_factor;

                /* save for use later */
                m_rowgroup_height[ci] = v_in_group;
                bool need_buffer = true;
                if (!componentInfo.component_needed)
                {
                    /* Don't bother to upsample an uninteresting component. */
                    m_upsampleMethods[ci] = ComponentUpsampler.noop_upsampler;
                    need_buffer = false;
                }
                else if (h_in_group == h_out_group && v_in_group == v_out_group)
                {
                    /* Fullsize components can be processed without any work. */
                    m_upsampleMethods[ci] = ComponentUpsampler.fullsize_upsampler;
                    need_buffer = false;
                }
                else if (h_in_group * 2 == h_out_group && v_in_group == v_out_group)
                {
                    /* Special cases for 2h1v upsampling */
                    if (do_fancy && componentInfo.downsampled_width > 2)
                        m_upsampleMethods[ci] = ComponentUpsampler.h2v1_fancy_upsampler;
                    else
                        m_upsampleMethods[ci] = ComponentUpsampler.h2v1_upsampler;
                }
                else if (h_in_group * 2 == h_out_group && v_in_group * 2 == v_out_group)
                {
                    /* Special cases for 2h2v upsampling */
                    if (do_fancy && componentInfo.downsampled_width > 2)
                    {
                        m_upsampleMethods[ci] = ComponentUpsampler.h2v2_fancy_upsampler;
                        m_need_context_rows = true;
                    }
                    else
                    {
                        m_upsampleMethods[ci] = ComponentUpsampler.h2v2_upsampler;
                    }
                }
                else if ((h_out_group % h_in_group) == 0 && (v_out_group % v_in_group) == 0)
                {
                    /* Generic integral-factors up-sampling method */
                    m_upsampleMethods[ci] = ComponentUpsampler.int_upsampler;
                    m_h_expand[ci] = (byte)(h_out_group / h_in_group);
                    m_v_expand[ci] = (byte)(v_out_group / v_in_group);
                }
                else
                    throw new Exception("Fractional sampling not implemented yet");

                if (need_buffer)
                {
                    ComponentBuffer cb = new ComponentBuffer();
                    cb.SetBuffer(JpegCommonBase.AllocJpegSamples(JpegUtils.jround_up(cinfo.m_output_width,
                        cinfo.m_max_h_samp_factor), cinfo.m_max_v_samp_factor), null, 0);

                    m_color_buf[ci] = cb;
                }
            }
        }

        /// <summary>
        /// Initialize for an upsampling pass.
        /// </summary>
        public override void start_pass()
        {
            /* Mark the conversion buffer empty */
            m_next_row_out = m_cinfo.m_max_v_samp_factor;

            /* Initialize total-height counter for detecting bottom of image */
            m_rows_to_go = m_cinfo.m_output_height;
        }

        /// <summary>
        /// Control routine to do upsampling (and color conversion).
        /// 
        /// In this version we upsample each component independently.
        /// We upsample one row group into the conversion buffer, then apply
        /// color conversion a row at a time.
        /// </summary>
        public override void upsample(ComponentBuffer[] input_buf, ref int in_row_group_ctr, int in_row_groups_avail, byte[][] output_buf, ref int out_row_ctr, int out_rows_avail)
        {
            /* Fill the conversion buffer, if it's empty */
            if (m_next_row_out >= m_cinfo.m_max_v_samp_factor)
            {
                for (int ci = 0; ci < m_cinfo.m_num_components; ci++)
                {
                    m_perComponentOffsets[ci] = 0;

                    /* Invoke per-component upsample method.*/
                    m_currentComponent = ci;
                    m_upsampleRowOffset = in_row_group_ctr * m_rowgroup_height[ci];
                    upsampleComponent(ref input_buf[ci]);
                }

                m_next_row_out = 0;
            }

            /* Color-convert and emit rows */

            /* How many we have in the buffer: */
            int num_rows = m_cinfo.m_max_v_samp_factor - m_next_row_out;

            /* Not more than the distance to the end of the image.  Need this test
             * in case the image height is not a multiple of max_v_samp_factor:
             */
            if (num_rows > m_rows_to_go)
                num_rows = m_rows_to_go;

            /* And not more than what the client can accept: */
            out_rows_avail -= out_row_ctr;
            if (num_rows > out_rows_avail)
                num_rows = out_rows_avail;

            m_cinfo.m_cconvert.color_convert(m_color_buf, m_perComponentOffsets, m_next_row_out, output_buf, out_row_ctr, num_rows);

            /* Adjust counts */
            out_row_ctr += num_rows;
            m_rows_to_go -= num_rows;
            m_next_row_out += num_rows;

            /* When the buffer is emptied, declare this input row group consumed */
            if (m_next_row_out >= m_cinfo.m_max_v_samp_factor)
                in_row_group_ctr++;
        }

        private void upsampleComponent(ref ComponentBuffer input_data)
        {
            switch (m_upsampleMethods[m_currentComponent])
            {
                case ComponentUpsampler.noop_upsampler:
                    noop_upsample();
                    break;
                case ComponentUpsampler.fullsize_upsampler:
                    fullsize_upsample(ref input_data);
                    break;
                case ComponentUpsampler.h2v1_fancy_upsampler:
                    h2v1_fancy_upsample(m_cinfo.Comp_info[m_currentComponent].downsampled_width, ref input_data);
                    break;
                case ComponentUpsampler.h2v1_upsampler:
                    h2v1_upsample(ref input_data);
                    break;
                case ComponentUpsampler.h2v2_fancy_upsampler:
                    h2v2_fancy_upsample(m_cinfo.Comp_info[m_currentComponent].downsampled_width, ref input_data);
                    break;
                case ComponentUpsampler.h2v2_upsampler:
                    h2v2_upsample(ref input_data);
                    break;
                case ComponentUpsampler.int_upsampler:
                    int_upsample(ref input_data);
                    break;
                default:
                    throw new Exception("The specified Component Up-sampler isn't implemented.");
            }
        }

        /*
         * These are the routines invoked to upsample pixel values
         * of a single component.  One row group is processed per call.
         */

        /// <summary>
        /// This is a no-op version used for "uninteresting" components.
        /// These components will not be referenced by color conversion.
        /// </summary>
        private static void noop_upsample()
        {
            // do nothing
        }

        /// <summary>
        /// For full-size components, we just make color_buf[ci] point at the
        /// input buffer, and thus avoid copying any data.  Note that this is
        /// safe only because sep_upsample doesn't declare the input row group
        /// "consumed" until we are done color converting and emitting it.
        /// </summary>
        private void fullsize_upsample(ref ComponentBuffer input_data)
        {
            m_color_buf[m_currentComponent] = input_data;
            m_perComponentOffsets[m_currentComponent] = m_upsampleRowOffset;
        }

        /// <summary>
        /// Fancy processing for the common case of 2:1 horizontal and 1:1 vertical.
        /// 
        /// The upsampling algorithm is linear interpolation between pixel centers,
        /// also known as a "triangle filter".  This is a good compromise between
        /// speed and visual quality.  The centers of the output pixels are 1/4 and 3/4
        /// of the way between input pixel centers.
        /// 
        /// A note about the "bias" calculations: when rounding fractional values to
        /// integer, we do not want to always round 0.5 up to the next integer.
        /// If we did that, we'd introduce a noticeable bias towards larger values.
        /// Instead, this code is arranged so that 0.5 will be rounded up or down at
        /// alternate pixel locations (a simple ordered dither pattern).
        /// </summary>
        private void h2v1_fancy_upsample(int downsampled_width, ref ComponentBuffer input_data)
        {
            ComponentBuffer output_data = m_color_buf[m_currentComponent];

            for (int inrow = 0; inrow < m_cinfo.m_max_v_samp_factor; inrow++)
            {
                int row = m_upsampleRowOffset + inrow;
                int inIndex = 0;

                int outIndex = 0;

                /* Special case for first column */
                int invalue = input_data[row][inIndex];
                inIndex++;

                output_data[inrow][outIndex] = (byte)invalue;
                outIndex++;
                output_data[inrow][outIndex] = (byte)((invalue * 3 + (int)input_data[row][inIndex] + 2) >> 2);
                outIndex++;

                for (int colctr = downsampled_width - 2; colctr > 0; colctr--)
                {
                    /* General case: 3/4 * nearer pixel + 1/4 * further pixel */
                    invalue = (int)input_data[row][inIndex] * 3;
                    inIndex++;

                    output_data[inrow][outIndex] = (byte)((invalue + (int)input_data[row][inIndex - 2] + 1) >> 2);
                    outIndex++;

                    output_data[inrow][outIndex] = (byte)((invalue + (int)input_data[row][inIndex] + 2) >> 2);
                    outIndex++;
                }

                /* Special case for last column */
                invalue = input_data[row][inIndex];
                output_data[inrow][outIndex] = (byte)((invalue * 3 + (int)input_data[row][inIndex - 1] + 1) >> 2);
                outIndex++;
                output_data[inrow][outIndex] = (byte)invalue;
                outIndex++;
            }
        }

        /// <summary>
        /// Fast processing for the common case of 2:1 horizontal and 1:1 vertical.
        /// It's still a box filter.
        /// </summary>
        private void h2v1_upsample(ref ComponentBuffer input_data)
        {
            ComponentBuffer output_data = m_color_buf[m_currentComponent];

            for (int inrow = 0; inrow < m_cinfo.m_max_v_samp_factor; inrow++)
            {
                int row = m_upsampleRowOffset + inrow;
                int outIndex = 0;

                for (int col = 0; col < m_cinfo.m_output_width; col++)
                {
                    byte invalue = input_data[row][col]; /* don't need GETJSAMPLE() here */
                    output_data[inrow][outIndex] = invalue;
                    outIndex++;
                    output_data[inrow][outIndex] = invalue;
                    outIndex++;
                }
            }
        }

        /// <summary>
        /// Fancy processing for the common case of 2:1 horizontal and 2:1 vertical.
        /// Again a triangle filter; see comments for h2v1 case, above.
        /// 
        /// It is OK for us to reference the adjacent input rows because we demanded
        /// context from the main buffer controller (see initialization code).
        /// </summary>
        private void h2v2_fancy_upsample(int downsampled_width, ref ComponentBuffer input_data)
        {
            ComponentBuffer output_data = m_color_buf[m_currentComponent];

            int inrow = m_upsampleRowOffset;
            int outrow = 0;
            while (outrow < m_cinfo.m_max_v_samp_factor)
            {
                for (int v = 0; v < 2; v++)
                {
                    // nearest input row index
                    int inIndex0 = 0;

                    //next nearest input row index
                    int inIndex1 = 0;
                    int inRow1 = -1;
                    if (v == 0)
                    {
                        /* next nearest is row above */
                        inRow1 = inrow - 1;
                    }
                    else
                    {
                        /* next nearest is row below */
                        inRow1 = inrow + 1;
                    }

                    int row = outrow;
                    int outIndex = 0;
                    outrow++;

                    /* Special case for first column */
                    int thiscolsum = (int)input_data[inrow][inIndex0] * 3 + (int)input_data[inRow1][inIndex1];
                    inIndex0++;
                    inIndex1++;

                    int nextcolsum = (int)input_data[inrow][inIndex0] * 3 + (int)input_data[inRow1][inIndex1];
                    inIndex0++;
                    inIndex1++;

                    output_data[row][outIndex] = (byte)((thiscolsum * 4 + 8) >> 4);
                    outIndex++;

                    output_data[row][outIndex] = (byte)((thiscolsum * 3 + nextcolsum + 7) >> 4);
                    outIndex++;

                    int lastcolsum = thiscolsum;
                    thiscolsum = nextcolsum;

                    for (int colctr = downsampled_width - 2; colctr > 0; colctr--)
                    {
                        /* General case: 3/4 * nearer pixel + 1/4 * further pixel in each */
                        /* dimension, thus 9/16, 3/16, 3/16, 1/16 overall */
                        nextcolsum = (int)input_data[inrow][inIndex0] * 3 + (int)input_data[inRow1][inIndex1];
                        inIndex0++;
                        inIndex1++;

                        output_data[row][outIndex] = (byte)((thiscolsum * 3 + lastcolsum + 8) >> 4);
                        outIndex++;

                        output_data[row][outIndex] = (byte)((thiscolsum * 3 + nextcolsum + 7) >> 4);
                        outIndex++;

                        lastcolsum = thiscolsum;
                        thiscolsum = nextcolsum;
                    }

                    /* Special case for last column */
                    output_data[row][outIndex] = (byte)((thiscolsum * 3 + lastcolsum + 8) >> 4);
                    outIndex++;
                    output_data[row][outIndex] = (byte)((thiscolsum * 4 + 7) >> 4);
                    outIndex++;
                }

                inrow++;
            }
        }

        /// <summary>
        /// Fast processing for the common case of 2:1 horizontal and 2:1 vertical.
        /// It's still a box filter.
        /// </summary>
        private void h2v2_upsample(ref ComponentBuffer input_data)
        {
            ComponentBuffer output_data = m_color_buf[m_currentComponent];

            int inrow = 0;
            int outrow = 0;
            while (outrow < m_cinfo.m_max_v_samp_factor)
            {
                int row = m_upsampleRowOffset + inrow;
                int outIndex = 0;

                for (int col = 0; col < m_cinfo.m_output_width; col++)
                {
                    byte invalue = input_data[row][col]; /* don't need GETJSAMPLE() here */
                    output_data[outrow][outIndex] = invalue;
                    outIndex++;
                    output_data[outrow][outIndex] = invalue;
                    outIndex++;
                }

                JpegUtils.jcopy_sample_rows(output_data, outrow, output_data, outrow + 1, 1, m_cinfo.m_output_width);
                inrow++;
                outrow += 2;
            }
        }

        /// <summary>
        /// This version handles any integral sampling ratios.
        /// This is not used for typical JPEG files, so it need not be fast.
        /// Nor, for that matter, is it particularly accurate: the algorithm is
        /// simple replication of the input pixel onto the corresponding output
        /// pixels.  The hi-falutin sampling literature refers to this as a
        /// "box filter".  A box filter tends to introduce visible artifacts,
        /// so if you are actually going to use 3:1 or 4:1 sampling ratios
        /// you would be well advised to improve this code.
        /// </summary>
        private void int_upsample(ref ComponentBuffer input_data)
        {
            ComponentBuffer output_data = m_color_buf[m_currentComponent];
            int h_expand = m_h_expand[m_currentComponent];
            int v_expand = m_v_expand[m_currentComponent];

            int inrow = 0;
            int outrow = 0;
            while (outrow < m_cinfo.m_max_v_samp_factor)
            {
                /* Generate one output row with proper horizontal expansion */
                int row = m_upsampleRowOffset + inrow;
                for (int col = 0; col < m_cinfo.m_output_width; col++)
                {
                    byte invalue = input_data[row][col]; /* don't need GETJSAMPLE() here */
                    int outIndex = 0;
                    for (int h = h_expand; h > 0; h--)
                    {
                        output_data[outrow][outIndex] = invalue;
                        outIndex++;
                    }
                }

                /* Generate any additional output rows by duplicating the first one */
                if (v_expand > 1)
                {
                    JpegUtils.jcopy_sample_rows(output_data, outrow, output_data,
                        outrow + 1, v_expand - 1, m_cinfo.m_output_width);
                }

                inrow++;
                outrow += v_expand;
            }
        }
    }
    #endregion

    #region Utils
    class Utils
    {
        public static MemoryStream CopyStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            long positionBefore = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);

            MemoryStream result = new MemoryStream((int)stream.Length);

            byte[] block = new byte[2048];
            for (; ; )
            {
                int bytesRead = stream.Read(block, 0, 2048);
                result.Write(block, 0, bytesRead);
                if (bytesRead < 2048)
                    break;
            }

            stream.Seek(positionBefore, SeekOrigin.Begin);
            return result;
        }

        public static void CMYK2RGB(byte c, byte m, byte y, byte k, out byte red, out byte green, out byte blue)
        {
            float C, M, Y, K;
            C = c / 255.0f;
            M = m / 255.0f;
            Y = y / 255.0f;
            K = k / 255.0f;

            float R, G, B;
            R = C * (1.0f - K) + K;
            G = M * (1.0f - K) + K;
            B = Y * (1.0f - K) + K;

            R = (1.0f - R) * 255.0f + 0.5f;
            G = (1.0f - G) * 255.0f + 0.5f;
            B = (1.0f - B) * 255.0f + 0.5f;

            red = (byte)(R * 255);
            green = (byte)(G * 255);
            blue = (byte)(B * 255);
        }
    }
    #endregion
    
    #region WorkingBitreadState
    struct WorkingBitreadState
    {
        public int get_buffer;
        public int bits_left;
        public JpegDecompressor cinfo;
    }
    #endregion
}
