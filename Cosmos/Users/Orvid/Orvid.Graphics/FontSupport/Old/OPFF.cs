/* This file will eventually hold an implementation 
 * of the Orvid Precompiled Font Format. The use of
 * this format is to provide a much easier to 
 * implement format for loading fonts. It is meant 
 * to eliminate the Pre-Rendering step required 
 * for most current font formats. It will achieve
 * this by using a bit-based format, where each bit
 * will represent a single pixel. It will also merge
 * the multiple files required for Bold, Italic, and
 * other such formatting, support, into a single
 * file.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace Orvid.Graphics.FontSupport.Old
{
    public class OPFF : Font
    {
        private class BinaryReader : System.IO.BinaryReader
        {
            private bool[] curByte = new bool[8];
            private byte curBitIndx = 0;
            private BitArray ba;

            public BinaryReader(Stream s) : base(s)
            {
                ba = new BitArray(new byte[] { base.ReadByte() });
                ba.CopyTo(curByte, 0);
                ba = null;
            }

            public override bool ReadBoolean()
            {
                if (curBitIndx == 8)
                {
                    ba = new BitArray(new byte[] { base.ReadByte() });
                    ba.CopyTo(curByte, 0);
                    ba = null;
                    this.curBitIndx = 0;
                }

                bool b = curByte[curBitIndx];
                curBitIndx++;
                return b;
            }

            public override byte ReadByte()
            {
                bool[] bar = new bool[8];
                byte i;
                for (i = 0; i < 8; i++)
                {
                    bar[i] = this.ReadBoolean();
                }

                byte b = 0;
                byte bitIndex = 0;
                for (i = 0; i < 8; i++)
                {
                    if (bar[i])
                    {
                        b |= (byte)(((byte)1) << bitIndex);
                    }
                    bitIndex++;
                }
                return b;
            }

            public override byte[] ReadBytes(int count)
            {
                byte[] bytes = new byte[count];
                for (int i = 0; i < count; i++)
                {
                    bytes[i] = this.ReadByte();
                }
                return bytes;
            }

            public override ushort ReadUInt16()
            {
                byte[] bytes = ReadBytes(2);
                return BitConverter.ToUInt16(bytes, 0);
            }

            public override uint ReadUInt32()
            {
                byte[] bytes = ReadBytes(4);
                return BitConverter.ToUInt32(bytes, 0);
            }

            public override ulong ReadUInt64()
            {
                byte[] bytes = ReadBytes(8);
                return BitConverter.ToUInt64(bytes, 0);
            }
        }

        private string name;
        public override string Name
        {
            get { return name; }
        }

        private UInt16 ver;
        public UInt16 FileFormatVersion
        {
            get { return ver; }
        }

        FontCharacterSet foundChars = new FontCharacterSet();

        public OPFF(byte[] data)
        {
            Load(data);
        }

        private void Load(byte[] data)
        {
            if (data[0] == 0xFF) // this means it's been compressed in LZMA format.
            {
                byte[] tmp = new byte[data.Length - 1];
                Array.Copy(data, 1, tmp, 0, tmp.Length);
                data = Orvid.Compression.LZMA.Decompress(tmp);
                tmp = null;
            }
            MemoryStream m = new MemoryStream(data);
            BinaryReader br = new BinaryReader(m);
            br.ReadBytes(8); // There are 8 empty bytes at the start of the header.
            byte[] datarr;
            ver = br.ReadUInt16();
            if (ver > 47)
            {
                throw new Exception("Format version is to high!");
            }

            datarr = br.ReadBytes(256);
            name = new String(ASCIIEncoding.ASCII.GetChars(datarr)).Replace("\0","");

            UInt64 charsToRead = br.ReadUInt64();

            UInt32 prevCharNumber = 0;
            byte height, width;
            FontFlag flags;
            int bits, len;
            Image im;
            for (UInt64 i = 0; i < charsToRead; i++)
            {
                // Check if the character number is incremented from the last item.
                if (br.ReadByte() == 255) // this means increment it.
                {
                    //throw new Exception();
                    prevCharNumber++;
                    flags = (FontFlag)br.ReadByte();
                    height = br.ReadByte();
                    width = br.ReadByte();
                    bits = (int)br.ReadUInt32();
                    len = (Int32)Math.Ceiling((double)(bits / 8));
                    datarr = br.ReadBytes(len);
                    im = LoadFromBinary(datarr, height, width, bits);
                    foundChars.AddCharacter((int)prevCharNumber, im, flags);
                }
                else
                {
                    prevCharNumber = br.ReadUInt32();
                    flags = (FontFlag)br.ReadByte();
                    height = br.ReadByte();
                    width = br.ReadByte();
                    bits = (int)br.ReadUInt32();
                    len = (Int32)Math.Ceiling((double)(bits / 8));
                    datarr = br.ReadBytes(len);
                    im = LoadFromBinary(datarr, height, width, bits);
                    foundChars.AddCharacter((int)prevCharNumber, im, flags);
                }
            }
        }

        private Image LoadFromBinary(byte[] data, byte height, byte width, int bits)
        {
            MemoryStream m = new MemoryStream(data);
            BinaryReader br = new BinaryReader(m);
            Image i = new Image(width, height);

            for (uint x = 0; x < width; x++)
            //for (uint y = 0; y < height; y++)
            {
                for (uint y = 0; y < height; y++)
                //for (uint x = 0; x < width; x++)
                {
                    //if (br.ReadBoolean())
                    //{
                    //    if (br.ReadBoolean())
                    //    {
                    //        i.SetPixel(x, y, Colors.Black); // Color the pixel black
                    //    }
                    //    else
                    //    {
                            byte greyscale = br.ReadByte();
                            if (greyscale != 255)
                            {
                                i.SetPixel(x, y, new Pixel(greyscale, greyscale, greyscale, 255)); // Color the pixel as greyscale
                            }
                    //    }
                    //}
                    //else
                    //{
                    //    i.SetPixel(x, y, Colors.White); // Color the pixel white
                    //}
                }
            }
            data = null;
            br.Dispose();
            m.Dispose();
            return i;
        }

        public override Image GetCharacter(Int32 charNumber, FontFlag flags)
        {
            return foundChars.GetCharacter(charNumber, flags);
        }
    }
}
