//#define COSMOSDEBUG
using System;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    internal class SingleByteEncoding : Encoding
    {
        private static Debugger mDebugger = new Debugger("System", "SingleByteEncoding");

        internal char[] CodePageTable { get; set; }
        private const byte ReplacementChar = (byte)'?';

        public override bool IsSingleByte => true;

        public override int GetByteCount(char[] chars, int index, int count)
        {
            mDebugger.SendInternal($"GetByteCount of chars {new string(chars)} index {index} count {count}");
            // Validate input parameters
            if (chars == null)
                throw new ArgumentNullException("chars", "Null Array");

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"), "negative number");

            if (chars.Length - index < count)
                throw new ArgumentOutOfRangeException("chars", "count more that what is in array");

            // If no input, return 0, avoid fixed empty array problem
            if (count == 0)
                return 0;

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (count == 0)
                return 0;

            //return chars.Length - index - count;
            return count - index;
        }

        private int GetCodePageIdxFromChr(char ch)
        {
            int idx;

            /* IL2CPU bug again with interfaces :-( let's do it manually... */
            //idx = Array.IndexOf<char>(CodePageTable, ch);

            for (idx = 0; idx < CodePageTable.Length; idx++)
            {
                if (CodePageTable[idx] == ch)
                    break;
            }

            // All CodePageTable searched, nothing found!
            if (idx == CodePageTable.Length)
                return -1;

            return idx + 128;
        }

        private byte GetByte(char ch)
        {
            /* ch is in reality an ASCII character? */
            if (ch < 127)
            {
                mDebugger.SendInternal($"ch {ch} is ASCII");
                return (byte)ch;
            }

            int idx = GetCodePageIdxFromChr(ch);
            if (idx == -1)
            {
                mDebugger.SendInternal($"ch {ch} not in CodePageTable replaced with {(char)ReplacementChar}");
                return ReplacementChar;
            }

            mDebugger.SendInternal($"ch {ch} is CodePageTable {idx}");
            return (byte)idx;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            mDebugger.SendInternal($"GetBytes of chars {new string(chars)} index {charIndex} count {charCount}");
            // Validate input parameters
            if (chars == null)
                throw new ArgumentNullException("chars", "Null Array");

            if (charIndex < 0 || charCount < 0)
                throw new ArgumentOutOfRangeException((charIndex < 0 ? "charIndex" : "charCount"), "negative number");

            if (chars.Length - charIndex < charCount)
                throw new ArgumentOutOfRangeException("chars", "count more that what is in array");

            mDebugger.SendInternal($"Converting to CodePageTable: {new String(chars)}");

            for (int i = charIndex; i < charCount; i++)
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
                throw new ArgumentNullException("bytes", "Null Array");

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"), "negative number");

            if (bytes.Length - index < count)
                throw new ArgumentOutOfRangeException("bytes", "count more that what is in array");

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (count == 0)
                return 0;

            return count - index;
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
                throw new ArgumentNullException("bytes", "Null Array");

            if (byteIndex < 0 || byteCount < 0)
                throw new ArgumentOutOfRangeException((byteIndex < 0 ? "byteIndex" : "byteCount"), "negative number");

            if (bytes.Length - byteIndex < byteCount)
                throw new ArgumentOutOfRangeException("bytes", "count more that what is in array");

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (byteCount == 0)
                return 0;

            for (int i = byteIndex; i < byteCount; i++)
            {
                chars[charIndex + i] = GetChar(bytes[i]);
            }

            mDebugger.SendInternal($"So as chars we have {new String(chars)}");

            return chars.Length;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "negative number");

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
