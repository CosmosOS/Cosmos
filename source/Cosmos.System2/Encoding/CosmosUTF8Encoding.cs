#define COSMOSDEBUG
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System2.Encoding
{
    public class CosmosUTF8Encoding : CosmosEncoding
    {
        private const uint UNI_REPLACEMENT_CHAR = 0x0000FFFD;
        private const uint UNI_SUR_HIGH_START = 0xD800;
        private const uint UNI_SUR_HIGH_END = 0xDBFF;
        private const uint UNI_SUR_LOW_START = 0xDC00;
        private const uint UNI_SUR_LOW_END = 0xDFFF;
        private const uint UNI_MAX_BMP = 0x0000FFFF;
        private const uint UNI_MAX_UTF16 = 0x0010FFFF;
        private const int  halfShift = 10;
        private const int  halfBase = 0x0010000;
        private const uint halfMask = 0x3FF;

        /*
         * Index into the table below with the first byte of a UTF-8 sequence to
         * get the number of trailing bytes that are supposed to follow it.
         * Note that *legal* UTF-8 values can't have 4 or 5-bytes. The table is
         * left as-is for anyone who may want to do such conversion, which was
         * allowed in earlier algorithms.
         */
        private static int[] trailingBytesForUTF8 = new int[] {
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1, 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
                2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2, 3,3,3,3,3,3,3,3,4,4,4,4,5,5,5,5
         };

        /*
         * Magic values subtracted from a buffer value during UTF8 conversion.
         * This table contains as many values as there might be trailing bytes
         * in a UTF-8 sequence.
         */
        static uint[] offsetsFromUTF8 = new uint[] { 0x00000000, 0x00003080, 0x000E2080, 0x03C82080, 0xFA082080,
                                                     0x82082080 };

        private static int GetCharBytes(uint ch, byte[] bytes, int byteIndex, int bytePos)
        {
            int bytesToWrite;

            // Filter out byte order marks and invalid character 0xFFFF
            if ((ch == 0xFEFF) || (ch == 0xFFFE) || (ch == 0xFFFF))
            {
                return bytePos;
            }

            /* Figure out how many bytes the result will require */
            if (ch < 0x80) /* 0XXX XXXX one byte */
                bytesToWrite = 1;
            else if (ch < 0x800) /* 110X XXXX two bytes */
                bytesToWrite = 2;
            else if (ch < 0x10000) /* 1110 XXXX three bytes */
                bytesToWrite = 3;
            else if (ch < 0x110000) /* 1111 0XXX four bytes */
                bytesToWrite = 4;
            else /* Invalid Unicode sequence Encode it as UNI_REPLACEMENT_CHAR */
            {
                ch = UNI_REPLACEMENT_CHAR;
                return GetCharBytes(ch, bytes, byteIndex, bytePos);
            }

            /* Check if there is sufficient space on bytes before writing on it */
            if (bytes.Length - (byteIndex + bytePos) < bytesToWrite)
                throw new ArgumentException("bytes has no sufficient space");

            switch (bytesToWrite)
            {
                case 1:
                    bytes[byteIndex + bytePos + 0] = (byte)ch;
                    break;

                case 2:
                    bytes[byteIndex + bytePos + 0] = (byte)(0xC0 | (ch >> 6));
                    bytes[byteIndex + bytePos + 1] = (byte)(0x80 | (ch & 0x3F));
                    break;

                case 3:
                    bytes[byteIndex + bytePos + 0] = (byte)(0xE0 | (ch >> 12));
                    bytes[byteIndex + bytePos + 1] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                    bytes[byteIndex + bytePos + 2] = (byte)(0x80 | (ch & 0x3F));
                    break;

                case 4:
                    bytes[byteIndex + bytePos + 0] = (byte)(0xF0 | (ch >> 18));
                    bytes[byteIndex + bytePos + 1] = (byte)(0x80 | ((ch >> 12) & 0x3F));
                    bytes[byteIndex + bytePos + 2] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                    bytes[byteIndex + bytePos + 3] = (byte)(0x80 | (ch & 0x3F));
                    break;
            }

            //bytePos += bytesToWrite;
            return bytesToWrite;
        }

        private static uint HandleSurrogatePairs(uint SurrFirst, uint SurrSecond)
        {
            if (SurrSecond >= UNI_SUR_LOW_START && SurrSecond <= UNI_SUR_LOW_END)
            {
                return ((SurrFirst - UNI_SUR_HIGH_START) << halfShift)
                      + (SurrSecond - UNI_SUR_LOW_START) + halfBase;
            }
            else /* it's an unpaired high surrogate */
            {
                throw new ArgumentException("Source contains unpaired surrogate");
            }
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null)
            {
                Global.mFileSystemDebugger.SendInternal($"chars is null returning 0");
                return 0;
            }

            if (charIndex == 0 && charCount == 0)
            {
                Global.mFileSystemDebugger.SendInternal($"charIndex and charCount both 0 returning 0");
                return 0;
            }

            int bytePos = 0;

            for (int i = charIndex; i < charCount; i++)
            {
                uint ch = chars[i];
                /* If we have a surrogate pair, convert to UTF32 first. */
                if (ch >= UNI_SUR_HIGH_START && ch <= UNI_SUR_HIGH_END)
                {
                    /* There is the next part of the surrogate? */
                    if (chars.Length >= i + 1)
                    {
                        i++;
                        ch = HandleSurrogatePairs(ch, chars[i]);
                    }
                    else
                        throw new ArgumentException("Source contains unpaired surrogate");
                }

                bytePos += GetCharBytes(ch, bytes, byteIndex, bytePos);
            }

            return bytePos;
        }

        /* Some UFT-8 "character" can occupy 4 bytes */
        public override int GetMaxByteCount(int ByteCount) => 4 * ByteCount;

        private static uint GetCharFromUFT8(byte[] bytes, out int bytesConsumed, int bytePos)
        {
            //uint ch = bytes[bytePos];
            uint ch = 0;

            int UtfTrailingBytes = trailingBytesForUTF8[bytes[bytePos]];
            int Uft8CharLen = UtfTrailingBytes + 1;
            bytesConsumed = Uft8CharLen;

            int i = bytePos;
            /* We "consume" the bytes and do the needed bitmasking to obtain the corrisponding codepoint */
            do
            {
                ch += bytes[i];
                i++;
                --Uft8CharLen;
                if (Uft8CharLen != 0)
                    ch <<= 6;
            } while (Uft8CharLen > 0);
            ch -= offsetsFromUTF8[UtfTrailingBytes];

            /* Target is a character <= 0xFFFF */
            if (ch <= UNI_MAX_BMP)
            {
                /* Invalid surrugates */
                if (ch >= UNI_SUR_HIGH_START && ch <= UNI_SUR_LOW_END)
                    return UNI_REPLACEMENT_CHAR;
                /* normal case */
                else
                    return (char)ch;
            }
            else if (ch > UNI_MAX_UTF16)
            {
                return UNI_REPLACEMENT_CHAR;
            }
            /* surrogate pairs */
            else
            {
                ushort lo = 0;
                ushort hi = 0;
                ch -= halfBase;
                hi = (ushort)((ch >> halfShift) + UNI_SUR_HIGH_START);
                lo = (ushort)((ch & halfMask) + UNI_SUR_LOW_START);
                /* 
                 * We pack the two halves of the pair in an uint sadly we need to unpack them later
                 * the alternative was to make this function return an array of character that will be really
                 * used only in this case :-(
                 */
                ch = (uint)((uint)hi << 16 | (uint)lo);

                return ch;
            }
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            uint ch = 0;
            int bytesConsumed = 0;
            //for (i = byteIndex; i < byteCount; i++)
            int numChar = 0;
            int bytePos = byteIndex;
            while (byteCount != 0)
            {
                ch = GetCharFromUFT8(bytes, out bytesConsumed, bytePos);
                /* check that chars has sufficient space */
                if (chars.Length < (charIndex + numChar))
                    throw new ArgumentException("chars has no sufficient space");

                if (ch < UNI_SUR_HIGH_START)
                    chars[charIndex + numChar] = (char)ch;
                else
                {
                    /* Unpach the uint in the two paired surrugates */
                    char chHigh = (char)(ch >> 16);
                    char chLow = (char)(ch & 0xFFFF);
                    chars[charIndex + numChar] = chHigh;
                    chars[charIndex + numChar + 1] = chLow;
                    numChar++;
                }

                /* skip the part of 'bytes' we have already consumed */
                byteCount -= bytesConsumed;
                bytePos += bytesConsumed;
                numChar++;
            }

            return numChar;
        }
    }
}
