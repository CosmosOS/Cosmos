//#define COSMOSDEBUG
using System;
using System.Collections;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    internal class SingleByteEncoding : Encoding
    {
        private static Debugger mDebugger = new Debugger("System", "SingleByteEncoding");

        private char[] _CodePageTable;
        internal char[] CodePageTable
        {
            get => _CodePageTable;
            set { _CodePageTable = value; PopulateChar2ByteTable(); }
        }

        private const byte ReplacementChar = (byte)'?';
        private Hashtable Char2ByteTable = new Hashtable();
        public override bool IsSingleByte => true;

        /*
         * Populates the Hashtable Char2ByteTable starting from the array CodePageTable
         * this avoids to do binary search using for() in GetByte. 
         */
        internal void PopulateChar2ByteTable()
        {
            mDebugger.SendInternal("Populating Char2Byte Hashtable...");

            for (int i = 0; i < CodePageTable.Length; i++)
            {
                Char2ByteTable.Add(CodePageTable[i], i + 128);
            }

            mDebugger.SendInternal("Done...");
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            mDebugger.SendInternal($"GetByteCount of chars {new string(chars)} index {index} count {count}");
            // Validate input parameters
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars), "Null Array");
            }

            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"), "negative number");
            }

            if (chars.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(chars), "count more that what is in array");
            }

            // If no input, return 0, avoid fixed empty array problem
            if (count == 0)
            {
                return 0;
            }

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (count == 0)
            {
                return 0;
            }

            //return chars.Length - index - count;
            return count - index;
        }

        private byte GetByte(char ch)
        {
            /* ch is in reality an ASCII character? */
            if (ch < 127)
            {
                mDebugger.SendInternal($"ch {ch} is ASCII");
                return (byte)ch;
            }

            // This will be nicer if we could use Nullable<byte> here
            object val = Char2ByteTable[ch];
            return val != null ? (byte)val : ReplacementChar;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            mDebugger.SendInternal($"GetBytes of chars {new string(chars)} index {charIndex} count {charCount}");
            // Validate input parameters
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars), "Null Array");
            }

            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0 ? "charIndex" : "charCount"), "negative number");
            }

            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException(nameof(chars), "count more that what is in array");
            }

            mDebugger.SendInternal($"Converting to CodePageTable: {new string(chars)}");

            for (var i = charIndex; i < charCount; i++)
            {
                bytes[byteIndex + i] = GetByte(chars[i]);
            }

            mDebugger.SendInternal($"So as bytes we have {BitConverter.ToString(bytes)}");
            return bytes.Length;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            mDebugger.SendInternal($"GetCharCount of bytes {BitConverter.ToString(bytes)} index {index} count {count}");
            // Validate Parameters
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Null Array");
            }

            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"), "negative number");
            }

            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "count more that what is in array");
            }

            // If no input just return 0, fixed doesn't like 0 length arrays
            return count == 0 ? 0 : count - index;
        }

        private char GetChar(byte b)
        {
            mDebugger.SendInternal($"Converting to UTF16: {b}...");
            /* Ascii? Simply cast it then... */
            if (b < 127)
            {
                mDebugger.SendInternal($"b {b} is ASCII");
                return (char)b;
            }

            mDebugger.SendInternal($"b in Extended ASCII");
            return CodePageTable[b - 128];
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            mDebugger.SendInternal($"Converting to UTF16: {BitConverter.ToString(bytes)}...");
            // Validate Parameters
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Null Array");
            }

            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0 ? "byteIndex" : "byteCount"), "negative number");
            }

            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "count more that what is in array");
            }

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (byteCount == 0)
            {
                return 0;
            }

            for (var i = byteIndex; i < byteCount; i++)
            {
                chars[charIndex + i] = GetChar(bytes[i]);
            }

            mDebugger.SendInternal($"So as chars we have {new string(chars)}");

            return chars.Length;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(charCount), "negative number");
            }

            // Characters would be # of characters + 1 in case high surrogate is ? * max fallback
            return charCount + 1;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            // Just return length, SBCS stay the same length because they don't map to surrogate
            return byteCount;
        }
    }
}
