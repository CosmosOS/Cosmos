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

namespace Orvid.Graphics.FontSupport
{
    public class OPFF : Font
    {
        private string name;
        public override string Name
        {
            get { return name; }
        }

        FontCharacterSet foundChars = new FontCharacterSet();

        public OPFF(byte[] data)
        {
            Load(data);
        }

        private UInt64 ReadUInt64(byte[] data)
        {
            UInt64 r = 0;

            r += data[0];
            r <<= 8;
            r += data[1];
            r <<= 8;
            r += data[2];
            r <<= 8;
            r += data[3];
            r <<= 8;
            r += data[4];
            r <<= 8;
            r += data[5];
            r <<= 8;
            r += data[6];
            r <<= 8;
            r += data[7];

            return r;
        }

        private Int32 ReadInt32(byte[] data)
        {
            Int32 r = 0;

            r += data[0];
            r <<= 8;
            r += data[1];
            r <<= 8;
            r += data[2];
            r <<= 8;
            r += data[3];
            r <<= 8;

            return r;
        }

        private void Load(byte[] data)
        {
            int curloc = 8; // There are 8 empty bytes at the start of the header.

            byte[] datarr = new byte[256];
            Array.Copy(data, curloc, datarr, 0, 256);
            curloc += 256;
            name = new String(ASCIIEncoding.ASCII.GetChars(datarr));

            datarr = new byte[8];
            Array.Copy(data, curloc, datarr, 0, 8);
            curloc += 8;
            UInt64 charsToRead = ReadUInt64(datarr);

            Int32 prevCharNumber = 0;
            for (UInt64 i = 0; i < charsToRead; i++)
            {
                // Check if the character number is incremented from the last item.
                if ((data[curloc] & 1) == 1) // this means increment it.
                {
                    curloc++;
                    prevCharNumber++;
                    FontFlag flags = (FontFlag)data[curloc];
                    curloc++;
                    byte height = data[curloc];
                    curloc++;
                    byte width = data[curloc];
                    curloc++;
                    int len = (Int32)Math.Ceiling((double)((width * height) / 8));
                    datarr = new byte[len];
                    Array.Copy(data, curloc, datarr, 0, len);
                    curloc += len;
                    Image im = LoadFromBinary(datarr, height, width);
                    foundChars.AddCharacter(prevCharNumber, im, flags);
                }
                else
                {
                    curloc++;
                    datarr = new byte[4];
                    Array.Copy(data, curloc, datarr, 0, 4);
                    curloc += 4;
                    prevCharNumber = ReadInt32(datarr);
                    FontFlag flags = (FontFlag)data[curloc];
                    curloc++;
                    byte height = data[curloc];
                    curloc++;
                    byte width = data[curloc];
                    curloc++;
                    int len = (Int32)Math.Ceiling((double)((width * height) / 8));
                    datarr = new byte[len];
                    Array.Copy(data, curloc, datarr, 0, len);
                    curloc += len;
                    Image im = LoadFromBinary(datarr, height, width);
                    foundChars.AddCharacter(prevCharNumber, im, flags);
                }
            }
        }

        private Image LoadFromBinary(byte[] data, byte height, byte width)
        {
            #region LoadData
            bool[] idata = new bool[height * width];
            int bitnum = 0;
            for (int inc = 0; inc < data.Length; inc++)
            {
                if ((data[inc] & 1) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 2) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 3) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 4) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 5) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 6) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 7) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
                if ((data[inc] & 8) == 1)
                {
                    idata[bitnum] = true;
                }
                bitnum++;
            }
            #endregion

            bitnum = 0;
            Image i = new Image(width, height);

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    if (idata[bitnum])
                    {
                        i.SetPixel(x, y, 1); // Color the pixel white
                    }
                    else
                    {
                        i.SetPixel(x, y, 255); // Color the pixel black
                    }
                    bitnum++;
                }
            }

            return i;
        }

        public override Image GetCharacter(Int32 charNumber, FontFlag flags)
        {
            return foundChars.GetCharacter(charNumber, flags);
        }
    }
}
