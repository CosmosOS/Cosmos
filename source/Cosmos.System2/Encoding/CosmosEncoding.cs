using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System2.Encoding
{
    public abstract class CosmosEncoding
    {
        public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex);

        //public abstract String GetString(Byte[] bytes);

        public abstract int GetMaxByteCount(int ByteCount);

        public virtual byte[] GetBytes(string s)
        {
            byte[] bytes = new byte[GetMaxByteCount(s.Length)];
            char[] textToEncode = s.ToCharArray();
            int nBytes;

            nBytes = GetBytes(textToEncode, 0, textToEncode.Length, bytes, 0);

            /*
             * This could be not the fastest method (it creates a new array and then does a copy of the old
             * until 'nBytes') but the alternative way was to call a version of GetBytes() that only counts
             * the encoded bytes and allocate the array using the correct size and then call GetBytes().
             * This is the approach used by the real Encoding class I'm unsure it is faster sincerely...
             * in the end is doing the encoding two times!
             *
             * Remeber - in any case - that this is a temporary solution we should plug the real Encoding class...
             */
            Array.Resize(ref bytes, nBytes);
            return bytes;
        }

        public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);

        public string GetString(byte[] bytes)
        {
            int numChar;
            char[] chars = new char[bytes.Length];

            numChar = GetChars(bytes, 0, bytes.Length, chars, 0);

            Array.Resize(ref chars, numChar);

            return new string(chars);
        }
    }
}
