/*
 * DDSReader
 * Copyright 2006 Michael Farrell
 
    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;

namespace au.id.micolous.libs.DDSReader
{
    /// <summary>
    /// The brand-spankingly new and revamped DDSReader library.
    /// 
    /// Now with 100% native .NET goodness.
    /// 
    /// This loads a DDS image into an object.  Not much more than that.  At the moment,
    /// it only supports DXT1 compressed images.  It doesn't support uncompressed
    /// images yet.
    /// </summary>
    public class DDSImage
    {
        private enum PixelFormat
        {
            /// <summary>
            /// 32-bit image, with 8-bit red, green, blue and alpha.
            /// </summary>
            ARGB,
            /// <summary>
            /// 24-bit image with 8-bit red, green, blue.
            /// </summary>
            RGB,
            /// <summary>
            /// 16-bit DXT-1 compression, 1-bit alpha.
            /// </summary>
            DXT1,
            /// <summary>
            /// DXT-2 Compression
            /// </summary>
            DXT2,
            /// <summary>
            /// DXT-3 Compression
            /// </summary>
            DXT3,
            /// <summary>
            /// DXT-4 Compression
            /// </summary>
            DXT4,
            /// <summary>
            /// DXT-5 Compression
            /// </summary>
            DTX5,
            /// <summary>
            /// 3DC Compression
            /// </summary>
            THREEDC,
            /// <summary>
            /// ATI1n Compression
            /// </summary>
            ATI1N,
            LUMINANCE,
            LUMINANCE_ALPHA,
            RXGB,
            A16B16G16R16,
            R16F,
            G16R16F,
            A16B16G16R16F,
            R32F,
            G32R32F,
            A32B32G32R32F,
            /// <summary>
            /// Unknown pixel format.
            /// </summary>
            UNKNOWN
        }

        /*
         * This class is based on parts of DevIL.net, specifically;
         * /DevIL-1.6.8/src-IL/src/il_dds.c
         *
         * All ported to c#/.net.
         * 
         * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/directx9_c/Opaque_and_1_Bit_Alpha_Textures.asp
         */

        /// <summary>
        /// A space-seperated list of supported image encoders.
        /// </summary>
        public const String SUPPORTED_ENCODERS = "DXT1 DXT3";

        private static byte[] DDS_HEADER = Convert.FromBase64String("RERTIA=="); // "DDS "

        // fourccs
        private const uint FOURCC_DXT1 = 827611204;
        private const uint FOURCC_DXT2 = 844388420;
        private const uint FOURCC_DXT3 = 861165636;
        private const uint FOURCC_DXT4 = 877942852;
        private const uint FOURCC_DXT5 = 894720068;
        private const uint FOURCC_ATI1 = 826889281;
        private const uint FOURCC_ATI2 = 843666497;
        private const uint FOURCC_RXGB = 1111971922;
        private const uint FOURCC_DOLLARNULL = 36;
        private const uint FOURCC_oNULL = 111;
        private const uint FOURCC_pNULL = 112;
        private const uint FOURCC_qNULL = 113;
        private const uint FOURCC_rNULL = 114;
        private const uint FOURCC_sNULL = 115;
        private const uint FOURCC_tNULL = 116;


        // other defines
        private const uint DDS_LINEARSIZE = 524288;
        private const uint DDS_PITCH = 8;
        private const uint DDS_FOURCC = 4;
        private const uint DDS_LUMINANCE = 131072;
        private const uint DDS_ALPHAPIXELS = 1;

        // headers 
        // DDSURFACEDESC2 structure
        private byte[] signature;
        private uint size1;
        private uint flags1;
        private uint height;
        private uint width;
        private uint linearsize;
        private uint depth;
        private uint mipmapcount;
        private uint alphabitdepth;
        // DDPIXELFORMAT structure
        private uint size2;
        private uint flags2;
        private uint fourcc;
        private uint rgbbitcount;
        private uint rbitmask;
        private uint bbitmask;
        private uint gbitmask;
        private uint alphabitmask;

        // DDCAPS2 structure
        private uint ddscaps1;
        private uint ddscaps2;
        private uint ddscaps3;
        private uint ddscaps4;
        // end DDCAPS2 structure
        private uint texturestage;
        // end DDSURFACEDESC2 structure

        private PixelFormat CompFormat;
        private uint blocksize;

        private uint bpp;
        private uint bps;
        private uint sizeofplane;
        private uint compsize;
        private byte[] compdata;
        private byte[] rawidata;
        private BinaryReader br;
        private Bitmap img;

        /// <summary>
        /// Returns a System.Imaging.Bitmap containing the DDS image.
        /// </summary>
        public Bitmap BitmapImage { get { return this.img; } }

        /// <summary>
        /// Constructs a new DDSImage object using the given byte array, which
        /// contains the raw DDS file.
        /// </summary>
        /// <param name="ddsimage">A byte[] containing the DDS file.</param>
        public DDSImage(Stream ms)
        {
            br = new BinaryReader(ms);
            this.signature = br.ReadBytes(4);

            if (!IsByteArrayEqual(this.signature, DDS_HEADER))
            {
                System.Console.WriteLine("Got header of '" + ASCIIEncoding.ASCII.GetString(this.signature, 0, this.signature.Length) + "'.");

                throw new Exception("Not a dds File!");
            }

            //System.Console.WriteLine("Got dds header okay");

            // now read in the rest
            this.size1 = br.ReadUInt32();
            this.flags1 = br.ReadUInt32();
            this.height = br.ReadUInt32();
            this.width = br.ReadUInt32();
            this.linearsize = br.ReadUInt32();
            this.depth = br.ReadUInt32();
            this.mipmapcount = br.ReadUInt32();
            this.alphabitdepth = br.ReadUInt32();

            // skip next 10 uints
            for (int x = 0; x < 10; x++)
            {
                br.ReadUInt32();
            }

            this.size2 = br.ReadUInt32();
            this.flags2 = br.ReadUInt32();
            this.fourcc = br.ReadUInt32();
            this.rgbbitcount = br.ReadUInt32();
            this.rbitmask = br.ReadUInt32();
            this.gbitmask = br.ReadUInt32();
            this.bbitmask = br.ReadUInt32();
            this.alphabitmask = br.ReadUInt32();
            this.ddscaps1 = br.ReadUInt32();
            this.ddscaps2 = br.ReadUInt32();
            this.ddscaps3 = br.ReadUInt32();
            this.ddscaps4 = br.ReadUInt32();
            this.texturestage = br.ReadUInt32();


            // patches for stuff
            if (this.depth == 0)
            {
                this.depth = 1;
            }

            if ((this.flags2 & DDS_FOURCC) > 0)
            {
                blocksize = ((this.width + 3) / 4) * ((this.height + 3) / 4) * this.depth;

                switch (this.fourcc)
                {
                    case FOURCC_DXT1:
                        CompFormat = PixelFormat.DXT1;
                        blocksize *= 8;
                        break;

                    case FOURCC_DXT2:
                        CompFormat = PixelFormat.DXT2;
                        blocksize *= 16;
                        break;

                    case FOURCC_DXT3:
                        CompFormat = PixelFormat.DXT3;
                        blocksize *= 16;
                        break;

                    case FOURCC_DXT4:
                        CompFormat = PixelFormat.DXT4;
                        blocksize *= 16;
                        break;

                    case FOURCC_DXT5:
                        CompFormat = PixelFormat.DTX5;
                        blocksize *= 16;
                        break;

                    case FOURCC_ATI1:
                        CompFormat = PixelFormat.ATI1N;
                        blocksize *= 8;
                        break;

                    case FOURCC_ATI2:
                        CompFormat = PixelFormat.THREEDC;
                        blocksize *= 16;
                        break;

                    case FOURCC_RXGB:
                        CompFormat = PixelFormat.RXGB;
                        blocksize *= 16;
                        break;

                    case FOURCC_DOLLARNULL:
                        CompFormat = PixelFormat.A16B16G16R16;
                        blocksize = this.width * this.height * this.depth * 8;
                        break;

                    case FOURCC_oNULL:
                        CompFormat = PixelFormat.R16F;
                        blocksize = this.width * this.height * this.depth * 2;
                        break;

                    case FOURCC_pNULL:
                        CompFormat = PixelFormat.G16R16F;
                        blocksize = this.width * this.height * this.depth * 4;
                        break;

                    case FOURCC_qNULL:
                        CompFormat = PixelFormat.A16B16G16R16F;
                        blocksize = this.width * this.height * this.depth * 8;
                        break;

                    case FOURCC_rNULL:
                        CompFormat = PixelFormat.R32F;
                        blocksize = this.width * this.height * this.depth * 4;
                        break;

                    case FOURCC_sNULL:
                        CompFormat = PixelFormat.G32R32F;
                        blocksize = this.width * this.height * this.depth * 8;
                        break;

                    case FOURCC_tNULL:
                        CompFormat = PixelFormat.A32B32G32R32F;
                        blocksize = this.width * this.height * this.depth * 16;
                        break;

                    default:
                        CompFormat = PixelFormat.UNKNOWN;
                        blocksize *= 16;
                        break;
                } // switch
            }
            else
            {
                // uncompressed image
                if ((this.flags2 & DDS_LUMINANCE) > 0)
                {
                    if ((this.flags2 & DDS_ALPHAPIXELS) > 0)
                    {
                        CompFormat = PixelFormat.LUMINANCE_ALPHA;
                    }
                    else
                    {
                        CompFormat = PixelFormat.LUMINANCE;
                    }
                }
                else
                {
                    if ((this.flags2 & DDS_ALPHAPIXELS) > 0)
                    {
                        CompFormat = PixelFormat.ARGB;
                    }
                    else
                    {
                        CompFormat = PixelFormat.RGB;
                    }
                }

                blocksize = (this.width * this.height * this.depth * (this.rgbbitcount >> 3));
            }

            if (CompFormat == PixelFormat.UNKNOWN)
            {
                throw new Exception("Invalid Header Format!");
            }

            if ((this.flags1 & (DDS_LINEARSIZE | DDS_PITCH)) == 0
                || this.linearsize == 0)
            {
                this.flags1 |= DDS_LINEARSIZE;
                this.linearsize = blocksize;
            }


            // get image data
            this.ReadData();

            // allocate bitmap
            this.bpp = this.PixelFormatToBpp(this.CompFormat);
            this.bps = this.width * this.bpp * this.PixelFormatToBpc(this.CompFormat);
            this.sizeofplane = this.bps * this.height;
            this.rawidata = new byte[this.depth * this.sizeofplane + this.height * this.bps + this.width * this.bpp];

            // decompress
            switch (this.CompFormat)
            {
                case PixelFormat.ARGB:
                case PixelFormat.RGB:
                case PixelFormat.LUMINANCE:
                case PixelFormat.LUMINANCE_ALPHA:
                    this.DecompressARGB();
                    break;

                case PixelFormat.DXT1:
                    this.DecompressDXT1();
                    break;

                case PixelFormat.DXT3:
                    this.DecompressDXT3();
                    break;

                default:
                    throw new Exception("Unknown file format!");
            }

            this.img = new Bitmap((int)this.width, (int)this.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // now fill bitmap with raw image datas.  this is really slow.
            // but only on windows/microsoft's .net clr.  it's fast in mono.
            // should find a better way to do this.

            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    // draw
                    ulong pos = (ulong)(((y * this.width) + x) * 4);
                    this.img.SetPixel(x, y, Color.FromArgb(this.rawidata[pos + 3], this.rawidata[pos], this.rawidata[pos + 1], this.rawidata[pos + 2]));
                }
            }


            // cleanup
            this.rawidata = null;
            this.compdata = null;

        }

        private static bool IsByteArrayEqual(byte[] arg0, byte[] arg1)
        {
            if (arg0.Length != arg1.Length)
            {
                return false;
            }

            for (int x = 0; x < arg0.Length; x++)
            {
                if (arg0[x] != arg1[x])
                {
                    return false;
                }
            }

            return true;
        }

        // iCompFormatToBpp
        private uint PixelFormatToBpp(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.LUMINANCE:
                case PixelFormat.LUMINANCE_ALPHA:
                case PixelFormat.ARGB:
                    return this.rgbbitcount / 8;

                case PixelFormat.RGB:
                case PixelFormat.THREEDC:
                case PixelFormat.RXGB:
                    return 3;

                case PixelFormat.ATI1N:
                    return 1;

                case PixelFormat.R16F:
                    return 2;

                case PixelFormat.A16B16G16R16:
                case PixelFormat.A16B16G16R16F:
                case PixelFormat.G32R32F:
                    return 8;

                case PixelFormat.A32B32G32R32F:
                    return 16;

                default:
                    return 4;
            }
        }

        // iCompFormatToBpc
        private uint PixelFormatToBpc(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.R16F:
                case PixelFormat.G16R16F:
                case PixelFormat.A16B16G16R16F:
                    return 4;

                case PixelFormat.R32F:
                case PixelFormat.G32R32F:
                case PixelFormat.A32B32G32R32F:
                    return 4;

                case PixelFormat.A16B16G16R16:
                    return 2;

                default:
                    return 1;
            }
        }

        // iCompFormatToChannelCount
        private uint PixelFormatToChannelCount(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.RGB:
                case PixelFormat.THREEDC:
                case PixelFormat.RXGB:
                    return 3;

                case PixelFormat.LUMINANCE:
                case PixelFormat.R16F:
                case PixelFormat.R32F:
                case PixelFormat.ATI1N:
                    return 1;

                case PixelFormat.LUMINANCE_ALPHA:
                case PixelFormat.G16R16F:
                case PixelFormat.G32R32F:
                    return 2;

                default:
                    return 4;
            }
        }

        private void ReadData()
        {
            this.compdata = null;

            if ((this.flags1 & DDS_LINEARSIZE) > 1)
            {
                this.compdata = this.br.ReadBytes((int)this.linearsize);
                this.compsize = (uint)this.compdata.Length;
            }
            else
            {
                uint bps = this.width * this.rgbbitcount / 8;
                this.compsize = bps * this.height * this.depth;
                this.compdata = new byte[this.compsize];

                MemoryStream mem = new MemoryStream((int)this.compsize);


                byte[] temp;
                for (int z = 0; z < this.depth; z++)
                {
                    for (int y = 0; y < this.height; y++)
                    {
                        temp = this.br.ReadBytes((int)this.bps);
                        mem.Write(temp, 0, temp.Length);
                    }
                }
                mem.Seek(0, SeekOrigin.Begin);

                mem.Read(this.compdata, 0, this.compdata.Length);
                mem.Close();
            }
        }

        private void DecompressARGB()
        {
            // not done
            throw new Exception("Un-compressed images not yet supported!");
        }

        private void DecompressDXT1()
        {
            // DXT1 decompressor
            Pixel[] colours = new Pixel[4];


            ushort colour0, colour1;
            uint bitmask, offset;
            int i, j, k, x, y, z, Select;

            MemoryStream mem = new MemoryStream(this.compdata.Length);
            mem.Write(this.compdata, 0, this.compdata.Length);
            mem.Seek(0, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(mem);

            colours[0].a = 255;
            colours[1].a = 255;
            colours[2].a = 255;

            for (z = 0; z < this.depth; z++)
            {
                for (y = 0; y < this.height; y += 4)
                {
                    for (x = 0; x < this.width; x += 4)
                    {
                        colour0 = r.ReadUInt16();
                        colour1 = r.ReadUInt16();

                        this.ReadColour(colour0, ref colours[0]);
                        this.ReadColour(colour1, ref colours[1]);

                        bitmask = r.ReadUInt32();

                        if (colour0 > colour1)
                        {
                            // Four-color block: derive the other two colors.
                            // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                            // These 2-bit codes correspond to the 2-bit fields
                            // stored in the 64-bit block.
                            colours[2].b = (byte)((2 * colours[0].b + colours[1].b + 1) / 3);
                            colours[2].g = (byte)((2 * colours[0].g + colours[1].g + 1) / 3);
                            colours[2].r = (byte)((2 * colours[0].r + colours[1].r + 1) / 3);

                            colours[3].b = (byte)((colours[0].b + 2 * colours[1].b + 1) / 3);
                            colours[3].g = (byte)((colours[0].g + 2 * colours[1].g + 1) / 3);
                            colours[3].r = (byte)((colours[0].r + 2 * colours[1].r + 1) / 3);
                            colours[3].a = 0xFF;
                        }
                        else
                        {
                            // Three-color block: derive the other color.
                            // 00 = color_0,  01 = color_1,  10 = color_2,
                            // 11 = transparent.
                            // These 2-bit codes correspond to the 2-bit fields 
                            // stored in the 64-bit block. 
                            colours[2].b = (byte)((colours[0].b + colours[1].b) / 2);
                            colours[2].g = (byte)((colours[0].g + colours[1].g) / 2);
                            colours[2].r = (byte)((colours[0].r + colours[1].r) / 2);

                            colours[3].b = 0;
                            colours[3].g = 0;
                            colours[3].r = 0;
                            colours[3].a = 0;
                        }


                        for (j = 0, k = 0; j < 4; j++)
                        {
                            for (i = 0; i < 4; i++, k++)
                            {
                                Select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    offset = (uint)(z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp);
                                    this.rawidata[offset] = (byte)colours[Select].r;
                                    this.rawidata[offset + 1] = (byte)colours[Select].g;
                                    this.rawidata[offset + 2] = (byte)colours[Select].b;
                                    this.rawidata[offset + 3] = (byte)colours[Select].a;
                                }
                            }
                        }



                    }
                }
            }

        }

        private void DecompressDXT3()
        {
            Pixel[] colours = new Pixel[4];
            uint bitmask, offset;
            int i, j, k, x, y, z, Select;
            ushort word, colour0, colour1;
            byte[] alpha; //temp;

            MemoryStream mem = new MemoryStream(this.compdata.Length);
            mem.Write(this.compdata, 0, this.compdata.Length);
            mem.Seek(0, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(mem);

            for (z = 0; z < this.depth; z++)
            {
                for (y = 0; y < this.height; y += 4)
                {
                    for (x = 0; x < this.width; x += 4)
                    {
                        alpha = r.ReadBytes(8);

                        colour0 = r.ReadUInt16();
                        colour1 = r.ReadUInt16();
                        this.ReadColour(colour0, ref colours[0]);
                        this.ReadColour(colour1, ref colours[1]);

                        bitmask = r.ReadUInt32();

                        colours[2].b = (byte)((2 * colours[0].b + colours[1].b + 1) / 3);
                        colours[2].g = (byte)((2 * colours[0].g + colours[1].g + 1) / 3);
                        colours[2].r = (byte)((2 * colours[0].r + colours[1].r + 1) / 3);

                        colours[3].b = (byte)((colours[0].b + 2 * colours[1].b + 1) / 3);
                        colours[3].g = (byte)((colours[0].g + 2 * colours[1].g + 1) / 3);
                        colours[3].r = (byte)((colours[0].r + 2 * colours[1].r + 1) / 3);

                        for (j = 0, k = 0; j < 4; j++)
                        {
                            for (i = 0; i < 4; k++, i++)
                            {
                                Select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);

                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    offset = (uint)(z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp);
                                    this.rawidata[offset] = (byte)colours[Select].r;
                                    this.rawidata[offset + 1] = (byte)colours[Select].g;
                                    this.rawidata[offset + 2] = (byte)colours[Select].b;
                                }
                            }
                        }

                        for (j = 0; j < 4; j++)
                        {
                            word = (ushort)(alpha[2 * j] + 256 * alpha[2 * j + 1]);
                            for (i = 0; i < 4; i++)
                            {
                                if (((x + i) < this.width) && ((y + j) < this.height))
                                {
                                    offset = (uint)(z * this.sizeofplane + (y + j) * this.bps + (x + i) * this.bpp + 3);
                                    this.rawidata[offset] = (byte)(word & 0x0F);
                                    this.rawidata[offset] = (byte)(this.rawidata[offset] | (this.rawidata[offset] << 4));
                                }
                                word >>= 4;
                            }
                        }
                    }
                }
            }
        }

        private void ReadColour(ushort Data, ref Pixel op)
        {
            byte r, g, b;

            b = (byte)(Data & 0x1f);
            g = (byte)((Data & 0x7E0) >> 5);
            r = (byte)((Data & 0xF800) >> 11);

            op.r = (byte)(r * 255 / 31);
            op.g = (byte)(g * 255 / 63);
            op.b = (byte)(b * 255 / 31);
        }

        public struct Pixel
        {
            /// <summary>
            /// The byte that describes the amount of Red in the pixel.
            /// </summary>
            public byte r;
            /// <summary>
            /// The byte that describes the amount of Green in the pixel.
            /// </summary>
            public byte g;
            /// <summary>
            /// The byte that describes the amount of Blue in the pixel.
            /// </summary>
            public byte b;
            /// <summary>
            /// The byte that describes the transparency of the pixel.
            /// </summary>
            public byte a;
        }
    }
}
