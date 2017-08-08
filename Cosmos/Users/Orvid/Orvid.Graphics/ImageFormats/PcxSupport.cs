using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class PcxImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            PcxInternals.Save(i, dest);
        }

        public override Image Load(Stream s)
        {
            return PcxInternals.Load(s);
        }


        // Please note, everything below this
        // point was originally from a plugin 
        // for Paint.Net, the plugin is available here:
        // http://forums.getpaint.net/index.php?/topic/2135-pcx-plug-in/
        //
        //
        // The source has been modified for use in this library.
        // 
        // This disclaimer was last
        // modified on August 9, 2011.


        #region Internals
        private static class PcxInternals
        {

            #region REALLLLYYY Doesn't Work :P
            //private class pcx_t
            //{
            //    public char manufacturer;
            //    public char version;
            //    public char encoding;
            //    public char bits_per_pixel;
            //    public short xmin, ymin, xmax, ymax;
            //    public short hres, vres;
            //    public int[] palette = new int[48];
            //    public char reserved;
            //    public char color_planes;
            //    public short bytes_per_line;
            //    public short palette_type;
            //    public byte[] filler = new byte[58];
            //    public byte[] data;

            //    public pcx_t(byte[] raw)
            //    {
            //        manufacturer = (char)raw[0];
            //        version = (char)raw[1];
            //        encoding = (char)raw[2];
            //        bits_per_pixel = (char)raw[3];
            //        xmin = (short)((raw[4] + (raw[5] << 8)) & 0xff);
            //        ymin = (short)((raw[6] + (raw[7] << 8)) & 0xff);
            //        xmax = (short)((raw[8] + (raw[9] << 8)) & 0xff);
            //        ymax = (short)((raw[10] + (raw[11] << 8)) & 0xff);
            //        hres = (short)((raw[12] + (raw[13] << 8)) & 0xff);
            //        vres = (short)((raw[14] + (raw[15] << 8)) & 0xff);
            //        for (int i = 0; i < 48; i++)
            //            palette[i] = (raw[16 + i] & 0xff);
            //        reserved = (char)raw[64];
            //        color_planes = (char)raw[65];
            //        bytes_per_line = (short)((raw[66] + (raw[67] << 8)) & 0xff);
            //        palette_type = (short)((raw[68] + (raw[69] << 8)) & 0xff);
            //        for (int i = 0; i < 58; i++)
            //            filler[i] = raw[70 + i];
            //        data = new byte[raw.Length - 128];
            //        for (int i = 0; i < raw.Length - 128; i++)
            //            data[i] = raw[128 + i];
            //    }
            //}

            //private static byte[] imageData;
            //private static int imageWidth, imageHeight;
            //public static Image Load(Stream s)
            //{
            //    int[] palette, pic, ot, pix;
            //    int k, x, y, len = 0, dataByte, runLength;
            //    byte[] raw;
            //    pcx_t pcx;

            //    raw = new byte[s.Length];
            //    s.Read(raw, 0, (int)s.Length);


            //    pcx = new pcx_t(raw);
            //    raw = pcx.data;

            //    if (pcx.manufacturer != 0x0a || pcx.version != 5 || pcx.encoding != 1 || pcx.bits_per_pixel != 8 || pcx.xmax >= 640 || pcx.ymax >= 480)
            //    {
            //        throw new Exception("Bad Pcx Data!");
            //    }

            //    palette = new int[768];
            //    for (int i = 0; i < 768; i++)
            //    {
            //        if ((uint)(len - 128 - 768 + i) < pcx.data.Length)
            //        {
            //            palette[i] = pcx.data[len - 128 - 768 + i] & 0xff;
            //        }
            //    }

            //    imageWidth = pcx.xmax + 1;
            //    imageHeight = pcx.ymax + 1;

            //    ot = new int[(pcx.ymax + 1) * (pcx.xmax + 1)];
            //    pic = ot;
            //    pix = ot;
            //    int pixcount = 0;
            //    int rawcount = 0;

            //    for (y = 0; y <= pcx.ymax; y++, pixcount += pcx.xmax + 1)
            //    {
            //        for (x = 0; x <= pcx.xmax; )
            //        {
            //            dataByte = raw[rawcount++];

            //            if ((dataByte & 0xC0) == 0xC0)
            //            {
            //                runLength = dataByte & 0x3F;
            //                dataByte = raw[rawcount++];
            //            }
            //            else
            //                runLength = 1;

            //            while (runLength-- > 0)
            //                pix[pixcount + x++] = dataByte & 0xff;
            //        }
            //    }

            //    Image im = new Image(imageWidth, imageHeight);
            //    imageData = new byte[imageWidth * imageHeight * 4];

            //    //convert to rgb format
            //    for (k = 0; k < (imageWidth * imageHeight); k++)
            //    {
            //        imageData[k * 4] = (byte)palette[pic[k] * 3];
            //        imageData[k * 4 + 1] = (byte)palette[pic[k] * 3 + 1];
            //        imageData[k * 4 + 2] = (byte)palette[pic[k] * 3 + 2];
            //        imageData[k * 4 + 3] = 0xff;
            //    }
            //    uint ndx = 0;
            //    for (uint yloc = 0; yloc < imageHeight; yloc++)
            //    {
            //        for (uint xloc = 0; xloc < imageWidth; xloc++)
            //        {
            //            im.SetPixel(xloc, yloc, new Pixel(imageData[ndx], imageData[ndx + 1], imageData[ndx + 2], imageData[ndx + 3]));
            //            ndx += 4;
            //        }
            //    }

            //    return im;
            //}
            #endregion


            #region Load
            public static Image Load(Stream input)
            {
                PcxPalette palette = null;
                uint[] numArray = null;
                Image im;
                PcxHeader header = new PcxHeader(input);

                #region Checks
                if (header.id != PcxId.ZSoftPCX)
                {
                    throw new FormatException("Not a PCX file.");
                }
                if (((header.version != PcxVersion.Version3_0) && (header.version != PcxVersion.Version2_8_Palette)) && ((header.version != PcxVersion.Version2_8_DefaultPalette) && (header.version != PcxVersion.Version2_5)))
                {
                    throw new FormatException("Unsupported PCX version: " + header.version.ToString());
                }
                if (((header.bitsPerPixel != 1) && (header.bitsPerPixel != 2)) && ((header.bitsPerPixel != 4) && (header.bitsPerPixel != 8)))
                {
                    throw new FormatException("Unsupported PCX bits per pixel: " + header.bitsPerPixel.ToString() + " bits per pixel");
                }
                int width = (header.xMax - header.xMin) + 1;
                int height = (header.yMax - header.yMin) + 1;
                if (((width < 0) || (height < 0)) || ((width > 0xffff) || (height > 0xffff)))
                {
                    throw new FormatException("Invalid image dimensions: (" + header.xMin.ToString() + "," + header.yMin.ToString() + ")-(" + header.xMax.ToString() + "," + header.yMax.ToString() + ")");
                }
                int num3 = (header.bytesPerLine * 8) / header.bitsPerPixel;
                int BitDepth = header.bitsPerPixel * header.nPlanes;
                if ((((BitDepth != 1) && (BitDepth != 2)) && ((BitDepth != 4) && (BitDepth != 8))) && (BitDepth != 0x18))
                {
                    throw new FormatException("Unsupported PCX bit depth: " + BitDepth.ToString());
                }
                #endregion

                #region Load Palette
                while (true)
                {
                    if (BitDepth == 1)
                    {
                        palette = PcxPalette.FromEgaPalette(PcxPalette.MONO_PALETTE);
                        break;
                    }
                    if (BitDepth >= 8)
                    {
                        if (BitDepth == 8)
                        {
                            long position = input.Position;
                            input.Seek(-769L, SeekOrigin.End);
                            if (input.ReadByte() != 12)
                            {
                                throw new FormatException("PCX palette marker not present in file");
                            }
                            palette = new PcxPalette(input, 0x100);
                            input.Seek(position, SeekOrigin.Begin);
                        }
                        else
                        {
                            palette = new PcxPalette(0x100);
                        }
                        break;
                    }
                    switch (header.version)
                    {
                        case PcxVersion.Version2_5:
                        case PcxVersion.Version2_8_DefaultPalette:
                            if (BitDepth == 2)
                            {
                                numArray = PcxPalette.CGA_PALETTE;
                                palette = PcxPalette.FromEgaPalette(numArray);
                            }
                            break;

                        default:
                            numArray = new uint[0];
                            palette = PcxPalette.FromColorMap(header.colorMap);
                            break;
                    }
                    if (numArray == null)
                    {
                        numArray = PcxPalette.EGA_PALETTE;
                    }
                    break;
                }
                #endregion

                im = new Image(width, height);
                uint[] array = new uint[width];
                for (int y = 0; y < height; y++)
                {
                    PcxByteReader byteReader = (header.encoding == PcxEncoding.RunLengthEncoded) ? ((PcxByteReader)new PcxRleByteReader(input)) : ((PcxByteReader)new PcxRawByteReader(input));
                    PcxIndexReader indxReader = new PcxIndexReader(byteReader, header.bitsPerPixel);
                    for (int j = 0; j < header.nPlanes; j++)
                    {
                        for (int m = 0; m < num3; m++)
                        {
                            uint num10 = indxReader.ReadIndex();
                            if (m < width)
                            {
                                array[m] |= num10 << (j * header.bitsPerPixel);
                            }
                        }
                    }
                    for (int x = 0; x < width; x++)
                    {
                        Pixel bgra;
                        uint TempC = array[x];
                        if (BitDepth == 24)
                        {
                            byte r = (byte)(TempC & 0xff);
                            byte g = (byte)((TempC >> 8) & 0xff);
                            byte b = (byte)((TempC >> 16) & 0xff);
                            bgra = new Pixel(r, g, b, 255);
                        }
                        else
                        {
                            bgra = palette[TempC];
                        }
                        im.SetPixel((uint)x, (uint)y, bgra);
                    }
                }
                return im;
            }
            #endregion

            #region Save
            public static void Save(Image bitmap, Stream output)
            {
                //if ((bitmap.Palette.Entries.Length == 0) || (bitmap.Palette.Entries.Length > 0x100))
                //{
                //    throw new FormatException("Unsupported palette size");
                //}
                //PcxPalette palette = new PcxPalette(bitmap.Palette);
                //PcxHeader header = new PcxHeader
                //{
                //    version = PcxVersion.Version3_0,
                //    encoding = token.RleCompress ? PcxEncoding.RunLengthEncoded : PcxEncoding.None,
                //    bitsPerPixel = (palette.Size == 0x10) ? ((byte)4) : ((palette.Size == 0x100) ? ((byte)8) : ((byte)0)),
                //    xMin = 0,
                //    yMin = 0,
                //    xMax = (ushort)(bitmap.Width - 1),
                //    yMax = (ushort)(bitmap.Height - 1),
                //    hDpi = (ushort)bitmap.HorizontalResolution,
                //    vDpi = (ushort)bitmap.VerticalResolution,
                //    nPlanes = 1,
                //    bytesPerLine = (ushort)(bitmap.Width + (((bitmap.Width % 2) == 1) ? 1 : 0)),
                //    paletteInfo = PcxPaletteType.Indexed
                //};
                //if (palette.Size == 0x10)
                //{
                //    header.colorMap = palette.ToColorMap();
                //}
                //header.Write(output);
                //int num = (header.bytesPerLine * 8) / header.bitsPerPixel;
                //    for (int i = 0; i < bitmap.Height; i++)
                //    {
                //        byte[] destination = new byte[bitmap.Width];
                //        byte* numPtr = (byte*)(bitmapdata.Scan0.ToPointer() + (i * bitmapdata.Stride));
                //        Marshal.Copy(new IntPtr((void*)numPtr), destination, 0, bitmap.Width);
                //        PcxByteWriter writer = (header.encoding == PcxEncoding.RunLengthEncoded) ? ((PcxByteWriter)new PcxRleByteWriter(output)) : ((PcxByteWriter)new PcxRawByteWriter(output));
                //        PcxIndexWriter writer2 = new PcxIndexWriter(writer, header.bitsPerPixel);
                //        for (int j = 0; j < num; j++)
                //        {
                //            if (j < bitmap.Width)
                //            {
                //                writer2.WriteIndex(destination[j]);
                //            }
                //            else
                //            {
                //                writer2.WriteIndex(0);
                //            }
                //        }
                //        writer2.Flush();
                //        writer.Flush();
                //    }
                //if (palette.Size == 0x100)
                //{
                //    output.WriteByte(12);
                //    palette.Write(output);
                //}
            }
            #endregion


            #region PcxByteReader
            private abstract class PcxByteReader
            {
                protected PcxByteReader()
                {
                }

                public abstract byte ReadByte();
            }
            #endregion

            #region PcxByteWriter
            private abstract class PcxByteWriter
            {
                protected PcxByteWriter()
                {
                }

                public abstract void Flush();
                public abstract void WriteByte(byte value);
            }
            #endregion

            #region PcxHeader
            private class PcxHeader
            {
                public byte bitsPerPixel;
                public ushort bytesPerLine;
                public byte[] colorMap;
                public PcxEncoding encoding;
                public byte[] filler;
                public ushort hDpi;
                public PcxId id;
                public byte nPlanes;
                public PcxPaletteType paletteInfo;
                public byte reserved;
                public ushort vDpi;
                public PcxVersion version;
                public ushort xMax;
                public ushort xMin;
                public ushort yMax;
                public ushort yMin;

                public PcxHeader()
                {
                    this.id = PcxId.ZSoftPCX;
                    this.version = PcxVersion.Version3_0;
                    this.encoding = PcxEncoding.RunLengthEncoded;
                    this.colorMap = new byte[0x30];
                    this.filler = new byte[0x3a];
                }

                public PcxHeader(Stream input)
                {
                    this.id = PcxId.ZSoftPCX;
                    this.version = PcxVersion.Version3_0;
                    this.encoding = PcxEncoding.RunLengthEncoded;
                    this.colorMap = new byte[0x30];
                    this.filler = new byte[0x3a];
                    this.id = (PcxId)this.ReadByte(input);
                    this.version = (PcxVersion)this.ReadByte(input);
                    this.encoding = (PcxEncoding)this.ReadByte(input);
                    this.bitsPerPixel = this.ReadByte(input);
                    this.xMin = this.ReadUInt16(input);
                    this.yMin = this.ReadUInt16(input);
                    this.xMax = this.ReadUInt16(input);
                    this.yMax = this.ReadUInt16(input);
                    this.hDpi = this.ReadUInt16(input);
                    this.vDpi = this.ReadUInt16(input);
                    for (int i = 0; i < this.colorMap.Length; i++)
                    {
                        this.colorMap[i] = this.ReadByte(input);
                    }
                    this.reserved = this.ReadByte(input);
                    this.nPlanes = this.ReadByte(input);
                    this.bytesPerLine = this.ReadUInt16(input);
                    this.paletteInfo = (PcxPaletteType)((byte)this.ReadUInt16(input));
                    for (int j = 0; j < this.filler.Length; j++)
                    {
                        this.filler[j] = this.ReadByte(input);
                    }
                }

                private byte ReadByte(Stream input)
                {
                    int num = input.ReadByte();
                    if (num == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    return (byte)num;
                }

                private ushort ReadUInt16(Stream input)
                {
                    int num = BitConverter.ToUInt16(new byte[] { (byte)input.ReadByte(), (byte)input.ReadByte() }, 0);
                    if (num == -1)
                    {
                        throw new EndOfStreamException();
                    }
                    return (ushort)num;
                }

                public void Write(Stream output)
                {
                    output.WriteByte((byte)this.id);
                    output.WriteByte((byte)this.version);
                    output.WriteByte((byte)this.encoding);
                    output.WriteByte(this.bitsPerPixel);
                    output.Write(BitConverter.GetBytes((ushort)this.xMin), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)this.yMin), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)this.xMax), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)this.yMax), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)this.hDpi), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)this.vDpi), 0, 2);
                    for (int i = 0; i < this.colorMap.Length; i++)
                    {
                        output.WriteByte(this.colorMap[i]);
                    }
                    output.WriteByte(this.reserved);
                    output.WriteByte(this.nPlanes);
                    output.Write(BitConverter.GetBytes((ushort)this.bytesPerLine), 0, 2);
                    output.Write(BitConverter.GetBytes((ushort)1), 0, 2);
                    for (int j = 0; j < this.filler.Length; j++)
                    {
                        output.WriteByte(this.filler[j]);
                    }
                }
            }
            #endregion

            #region PcxIndexReader
            private class PcxIndexReader
            {
                private uint m_bitMask;
                private uint m_bitsPerPixel;
                private uint m_bitsRemaining;
                private uint m_byteRead;
                private PcxByteReader m_reader;

                public PcxIndexReader(PcxByteReader reader, uint bitsPerPixel)
                {
                    if (((bitsPerPixel != 1) && (bitsPerPixel != 2)) && ((bitsPerPixel != 4) && (bitsPerPixel != 8)))
                    {
                        throw new ArgumentException("bitsPerPixel must be 1, 2, 4 or 8", "bitsPerPixel");
                    }
                    this.m_reader = reader;
                    this.m_bitsPerPixel = bitsPerPixel;
                    this.m_bitMask = 1;
                    for (uint i = this.m_bitsPerPixel; i > 0; i--)
                    {
                        this.m_bitMask = this.m_bitMask << 1;
                    }
                    this.m_bitMask--;
                }

                public uint ReadIndex()
                {
                    if (this.m_bitsRemaining == 0)
                    {
                        this.m_byteRead = this.m_reader.ReadByte();
                        this.m_bitsRemaining = 8;
                    }
                    uint num = (this.m_byteRead >> (int)(8 - this.m_bitsPerPixel)) & this.m_bitMask;
                    this.m_byteRead = this.m_byteRead << (int)this.m_bitsPerPixel;
                    this.m_bitsRemaining -= this.m_bitsPerPixel;
                    return num;
                }
            }
            #endregion

            #region PcxIndexWriter
            private class PcxIndexWriter
            {
                private uint m_bitMask;
                private uint m_bitsPerPixel;
                private uint m_bitsUsed;
                private uint m_byteAccumulated;
                private PcxByteWriter m_writer;

                public PcxIndexWriter(PcxByteWriter writer, uint bitsPerPixel)
                {
                    if (((bitsPerPixel != 1) && (bitsPerPixel != 2)) && ((bitsPerPixel != 4) && (bitsPerPixel != 8)))
                    {
                        throw new ArgumentException("bitsPerPixel must be 1, 2, 4 or 8", "bitsPerPixel");
                    }
                    this.m_writer = writer;
                    this.m_bitsPerPixel = bitsPerPixel;
                    this.m_bitMask = 1;
                    for (uint i = this.m_bitsPerPixel; i > 0; i--)
                    {
                        this.m_bitMask = this.m_bitMask << 1;
                    }
                    this.m_bitMask--;
                }

                public void Flush()
                {
                    if (this.m_bitsUsed > 0)
                    {
                        this.m_writer.WriteByte((byte)this.m_byteAccumulated);
                        this.m_bitsUsed = 0;
                    }
                }

                public void WriteIndex(uint index)
                {
                    this.m_byteAccumulated = (this.m_byteAccumulated << (int)this.m_bitsPerPixel) | (index & this.m_bitMask);
                    this.m_bitsUsed += this.m_bitsPerPixel;
                    if (this.m_bitsUsed == 8)
                    {
                        this.Flush();
                    }
                }
            }
            #endregion

            #region PcxPalette
            private class PcxPalette
            {
                public static readonly uint[] CGA_PALETTE = new uint[] 
                {
                    0, 0xaaaa, 0xaa00aa, 0xaaaaaa, 
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0 
                };
                public static readonly uint[] EGA_PALETTE = new uint[] 
                { 
                    0, 0xa8, 0xa800, 0xa8a8,
                    0xa80000, 0xa800a8, 0xa85400, 0xa8a8a8,
                    0x545454, 0x5454fe, 0x54fe54, 0x54fefe,
                    0xfe5454, 0xfe54fe, 0xfefe54, 0xfefefe 
                };
                private Pixel[] m_palette;
                public static readonly uint[] MONO_PALETTE;

                static PcxPalette()
                {
                    uint[] numArray = new uint[0x10];
                    numArray[1] = 0xffffff;
                    MONO_PALETTE = numArray;
                }

                public PcxPalette(uint size)
                {
                    if ((size != 0x10) && (size != 0x100))
                    {
                        throw new FormatException("Unsupported palette size");
                    }
                    this.m_palette = new Pixel[size];
                }

                public PcxPalette(Stream input, int size)
                {
                    if ((size != 0x10) && (size != 0x100))
                    {
                        throw new FormatException("Unsupported palette size");
                    }
                    this.m_palette = new Pixel[size];
                    for (int i = 0; i < this.m_palette.Length; i++)
                    {
                        int num2 = input.ReadByte();
                        if (num2 == -1)
                        {
                            throw new EndOfStreamException();
                        }
                        int num3 = input.ReadByte();
                        if (num3 == -1)
                        {
                            throw new EndOfStreamException();
                        }
                        int num4 = input.ReadByte();
                        if (num4 == -1)
                        {
                            throw new EndOfStreamException();
                        }
                        this.m_palette[i] = new Pixel((byte)num2, (byte)num3, (byte)num4, 0xff);
                    }
                }

                public static PcxPalette FromColorMap(byte[] colorMap)
                {
                    if (colorMap == null)
                    {
                        throw new ArgumentNullException("colorMap");
                    }
                    if (colorMap.Length != 48)
                    {
                        throw new FormatException("Trying to read an unsupported palette size from a header ColorMap");
                    }
                    PcxPalette palette = new PcxPalette(16);
                    uint num = 0;
                    for (uint i = 0; i < 16; i++)
                    {
                        byte r = colorMap[num];
                        num++;
                        byte g = colorMap[num];
                        num++;
                        byte b = colorMap[num];
                        num++;
                        palette[i] = new Pixel(r, g, b, 255);
                    }
                    return palette;
                }

                public static PcxPalette FromEgaPalette(uint[] egaPalette)
                {
                    if (egaPalette == null)
                    {
                        throw new ArgumentNullException("egaPalette");
                    }
                    if (egaPalette.Length != 0x10)
                    {
                        throw new FormatException("Trying to read an unsupported palette size from a header ColorMap");
                    }
                    PcxPalette palette = new PcxPalette(0x10);
                    for (uint i = 0; i < 0x10; i++)
                    {
                        byte r = (byte)((egaPalette[i] >> 16) & 0xff);
                        byte g = (byte)((egaPalette[i] >> 8) & 0xff);
                        byte b = (byte)(egaPalette[i] & 0xff);
                        palette[i] = new Pixel(r, g, b, 255);
                    }
                    return palette;
                }

                public byte[] ToColorMap()
                {
                    if (this.m_palette.Length != 0x10)
                    {
                        throw new FormatException("Trying to write an unsupported palette size to a header ColorMap");
                    }
                    byte[] buffer = new byte[0x30];
                    uint num = 0;
                    for (int i = 0; i < 0x10; i++)
                    {
                        Pixel bgra = this.m_palette[i];
                        buffer[num++] = bgra.R;
                        buffer[num++] = bgra.G;
                        buffer[num++] = bgra.B;
                    }
                    return buffer;
                }

                public void Write(Stream output)
                {
                    for (int i = 0; i < this.m_palette.Length; i++)
                    {
                        Pixel bgra = this.m_palette[i];
                        output.WriteByte(bgra.R);
                        output.WriteByte(bgra.G);
                        output.WriteByte(bgra.B);
                    }
                }

                public Pixel this[uint index]
                {
                    get
                    {
                        return this.m_palette[index];
                    }
                    set
                    {
                        this.m_palette[index] = value;
                    }
                }

                public uint Size
                {
                    get
                    {
                        return (uint)this.m_palette.Length;
                    }
                }
            }
            #endregion

            #region PcxRawByteReader
            private class PcxRawByteReader : PcxByteReader
            {
                private Stream m_stream;

                public PcxRawByteReader(Stream stream)
                {
                    this.m_stream = stream;
                }

                public override byte ReadByte()
                {
                    return (byte)this.m_stream.ReadByte();
                }
            }
            #endregion

            #region PcxRawByteWriter
            private class PcxRawByteWriter : PcxByteWriter
            {
                private Stream m_stream;

                public PcxRawByteWriter(Stream stream)
                {
                    this.m_stream = stream;
                }

                public override void Flush()
                {
                }

                public override void WriteByte(byte value)
                {
                    this.m_stream.WriteByte(value);
                }
            }
            #endregion

            #region PcxRleByteReader
            private class PcxRleByteReader : PcxByteReader
            {
                private uint m_count;
                private byte m_rleValue;
                private Stream m_stream;

                public PcxRleByteReader(Stream input)
                {
                    this.m_stream = input;
                }

                public override byte ReadByte()
                {
                    if (this.m_count > 0)
                    {
                        this.m_count--;
                        return this.m_rleValue;
                    }
                    byte num = (byte)this.m_stream.ReadByte();
                    if ((num & 192) == 192)
                    {
                        this.m_count = (uint)(num & 63);
                        this.m_rleValue = (byte)this.m_stream.ReadByte();
                        this.m_count--;
                        return this.m_rleValue;
                    }
                    return num;
                }
            }
            #endregion

            #region PcxRleByteWriter
            private class PcxRleByteWriter : PcxByteWriter
            {
                private uint m_count;
                private byte m_lastValue;
                private Stream m_stream;

                public PcxRleByteWriter(Stream output)
                {
                    this.m_stream = output;
                }

                public override void Flush()
                {
                    if (this.m_count != 0)
                    {
                        if ((this.m_count > 1) || ((this.m_count == 1) && ((this.m_lastValue & 0xc0) == 0xc0)))
                        {
                            this.m_stream.WriteByte((byte)(0xc0 | this.m_count));
                            this.m_stream.WriteByte(this.m_lastValue);
                            this.m_count = 0;
                        }
                        else
                        {
                            this.m_stream.WriteByte(this.m_lastValue);
                            this.m_count = 0;
                        }
                    }
                }

                public override void WriteByte(byte value)
                {
                    if (((this.m_count == 0) || (this.m_count == 0x3f)) || (value != this.m_lastValue))
                    {
                        this.Flush();
                        this.m_lastValue = value;
                        this.m_count = 1;
                    }
                    else
                    {
                        this.m_count++;
                    }
                }
            }
            #endregion

            #region PcxVersion
            private enum PcxVersion : byte
            {
                Version2_5 = 0,
                Version2_8_DefaultPalette = 3,
                Version2_8_Palette = 2,
                Version3_0 = 5
            }
            #endregion

            #region PcxPaletteType
            private enum PcxPaletteType : byte
            {
                Grayscale = 2,
                Indexed = 1
            }
            #endregion

            #region PcxEncoding
            private enum PcxEncoding : byte
            {
                None = 0,
                RunLengthEncoded = 1
            }
            #endregion

            #region PcxId
            private enum PcxId : byte
            {
                ZSoftPCX = 10
            }
            #endregion

        }
        #endregion
    }
}
