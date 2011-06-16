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
        


        public OPFF(byte[] data)
        {
            Load(data);
        }

        private UInt64 FromByteArray(byte[] data)
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
            UInt64 charsToRead = FromByteArray(datarr);

            for (UInt64 i = 0; i < charsToRead; i++)
            {

            }
        }


        public override Image GetCharacter(ulong charNumber, FontFlag flags)
        {
            throw new NotImplementedException();
        }
    }
}
