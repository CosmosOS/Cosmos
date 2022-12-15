//#define COSMOSDEBUG
using System;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// SingleByteEncoding class. Used to represent a single byte encoding.
    /// </summary>
    internal class SingleByteEncoding : Encoding
    {
        /// <summary>
        /// Debugger instance of the "System" ring with the "SingleByteEncoding" tag.
        /// </summary>
        private static Debugger mDebugger = new Debugger("System", "SingleByteEncoding");

        /// <summary>
        /// Get and set codepage table.
        /// </summary>
        internal char[] CodePageTable { get; set; }
        /// <summary>
        /// Replacement char.
        /// </summary>
        private const byte ReplacementChar = (byte)'?';

        /// <summary>
        /// Check if this is single byte.
        /// </summary>
        public override bool IsSingleByte => true;

        /// <summary>
        /// Get count of bytes in chars array.
        /// </summary>
        /// <param name="chars">Chars array.</param>
        /// <param name="index">Starting index in chars array.</param>
        /// <param name="count">Number of chars to check.</param>
        /// <returns>int value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if chars is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if index or count are invalid.</exception>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            mDebugger.SendInternal($"GetByteCount of chars {new string(chars)} index {index} count {count}");
            // Validate input parameters
            if (chars == null)
                throw new ArgumentNullException("chars", "Null Array");

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count", "negative number");

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

        /// <summary>
        /// Get codepage index relative to given char.
        /// </summary>
        /// <param name="ch">Char to get the codepage of.</param>
        /// <returns>int value.</returns>
        /// <exception cref="OverflowException">Thrown if number of entrys in the codepage table is greater than Int32.MaxValue.</exception>
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

        /// <summary>
        /// Convert char to byte.
        /// </summary>
        /// <param name="ch">Char to convert.</param>
        /// <returns>byte value.</returns>
        /// <exception cref="OverflowException">Thrown if number of entrys in the codepage table is greater than Int32.MaxValue.</exception>
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

        /// <summary>
        /// Get bytes array out of chars array, and bytes count.
        /// </summary>
        /// <param name="chars">Chars array.</param>
        /// <param name="charIndex">Stating index in chars array.</param>
        /// <param name="charCount">Number of chars to convert.</param>
        /// <param name="bytes">Output bytes array.</param>
        /// <param name="byteIndex">Starting index in bytes array.</param>
        /// <returns>int value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if chars or bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if charIndex or charCount are invalid.</exception>
        /// <exception cref="OverflowException">Thrown if bytes array length or codepage table length is greater than Int32.MaxValue.</exception>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            mDebugger.SendInternal($"GetBytes of chars {new string(chars)} index {charIndex} count {charCount}");
            // Validate input parameters
            if (chars == null)
                throw new ArgumentNullException("chars", "Null Array");

            if (charIndex < 0 || charCount < 0)
                throw new ArgumentOutOfRangeException(charIndex < 0 ? "charIndex" : "charCount", "negative number");

            if (chars.Length - charIndex < charCount)
                throw new ArgumentOutOfRangeException("chars", "count more that what is in array");

            mDebugger.SendInternal($"Converting to CodePageTable: {new string(chars)}");

            for (int i = charIndex; i < charCount; i++)
            {
                bytes[byteIndex + i] = GetByte(chars[i]);
            }

            mDebugger.SendInternal($"So as bytes we have {BitConverter.ToString(bytes)}");
            return bytes.Length;
        }

        /// <summary>
        /// Get char count in bytes array.
        /// </summary>
        /// <param name="bytes">Bytes array to count the chars in.</param>
        /// <param name="index">Starting index.</param>
        /// <param name="count">Number of bytes to check.</param>
        /// <returns>int value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if index or count are invalid.</exception>
        /// <exception cref="OverflowException">Thrown if bytes array length is greater than Int32.MaxValue.</exception>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            mDebugger.SendInternal($"GetCharCount of bytes {BitConverter.ToString(bytes)} index {index} count {count}");
            // Validate Parameters
            if (bytes == null)
                throw new ArgumentNullException("bytes", "Null Array");

            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count", "negative number");

            if (bytes.Length - index < count)
                throw new ArgumentOutOfRangeException("bytes", "count more that what is in array");

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (count == 0)
                return 0;

            return count - index;
        }

        /// <summary>
        /// Convert byte to char.
        /// </summary>
        /// <param name="b">byte to convert.</param>
        /// <returns>char value.</returns>
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

        /// <summary>
        /// Convert bytes array to chars array, and get number of chars in bytes array.
        /// </summary>
        /// <param name="bytes">Bytes array to count chars in.</param>
        /// <param name="byteIndex">Starting index in bytes array.</param>
        /// <param name="byteCount">Number of bytes to convert.</param>
        /// <param name="chars">Output array, in which the bytes that are char would be stored in.</param>
        /// <param name="charIndex">Starting index in chars array</param>
        /// <returns>int value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if byteCount or byteIndex are invalid</exception>
        /// <exception cref="OverflowException">Thrown if number of chars is greater than Int32.MaxValue.</exception>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            mDebugger.SendInternal($"Converting to UTF16: {BitConverter.ToString(bytes)}...");
            // Validate Parameters
            if (bytes == null)
                throw new ArgumentNullException("bytes", "Null Array");

            if (byteIndex < 0 || byteCount < 0)
                throw new ArgumentOutOfRangeException(byteIndex < 0 ? "byteIndex" : "byteCount", "negative number");

            if (bytes.Length - byteIndex < byteCount)
                throw new ArgumentOutOfRangeException("bytes", "count more that what is in array");

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (byteCount == 0)
                return 0;

            for (int i = byteIndex; i < byteCount; i++)
            {
                chars[charIndex + i] = GetChar(bytes[i]);
            }

            mDebugger.SendInternal($"So as chars we have {new string(chars)}");

            return chars.Length;
        }

        /// <summary>
        /// Get max char count.
        /// </summary>
        /// <param name="charCount">char count.</param>
        /// <returns>int value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if charCount is less than 0.</exception>
        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException(nameof(charCount), "negative number");

            // Characters would be # of characters + 1 in case high surrogate is ? * max fallback
            return charCount + 1;
        }

        /// <summary>
        /// Get max char count.
        /// </summary>
        /// <remarks>returns byteCount.</remarks>
        /// <param name="byteCount">byte count</param>
        /// <returns>int value.</returns>
        public override int GetMaxCharCount(int byteCount)
        {
            // Just return length, SBCS stay the same length because they don't map to surrogate
            return byteCount;
        }
    }
}
