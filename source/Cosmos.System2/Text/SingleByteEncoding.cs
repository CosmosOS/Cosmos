//#define COSMOSDEBUG
using System;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.ExtendedASCII
{
    /// <summary>
    /// Represents a single byte encoding.
    /// </summary>
    internal class SingleByteEncoding : Encoding
    {
        static readonly Debugger mDebugger = new("System", "SingleByteEncoding");

        /// <summary>
        /// The code page table.
        /// </summary>
        internal char[] CodePageTable { get; set; }

        private const byte ReplacementChar = (byte)'?';

        public override bool IsSingleByte => true;

        public override int GetByteCount(char[] chars, int index, int count)
        {
            // Validate input parameters
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count), "negative number");
            }

            if (chars.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "The 'count' parameter is greater than the length of the 'chars' array.");
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
                if (CodePageTable[idx] == ch) {
                    break;
                }
            }

            // All CodePageTable searched, nothing found!
            return idx == CodePageTable.Length ? -1 : idx + 128;
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
                return (byte)ch;
            }

            int idx = GetCodePageIdxFromChr(ch);
            if (idx == -1)
            {
                return ReplacementChar;
            }

            return (byte)idx;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            mDebugger.SendInternal($"GetBytes of chars {new string(chars)} index {charIndex} count {charCount}");
            // Validate input parameters
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }

            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException(charIndex < 0 ? nameof(charIndex) : nameof(charCount), "A negative number was given.");
            }

            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException(nameof(chars), "'count' is greater than the length of the array.");
            }

            for (int i = charIndex; i < charCount; i++)
            {
                bytes[byteIndex + i] = GetByte(chars[i]);
            }

            return bytes.Length;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            // Validate Parameters
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(index < 0 ? nameof(index) : nameof(count), "The given number was negative.");
            }

            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "count more that what is in array");
            }

            // If no input just return 0, fixed doesn't like 0 length arrays
            return count == 0 ? 0 : count - index;
        }

        /// <summary>
        /// Convert byte to char.
        /// </summary>
        /// <param name="b">byte to convert.</param>
        /// <returns>char value.</returns>
        private char GetChar(byte b)
        {
            /* Ascii? Simply cast it then... */
            if (b < 127)
            {
                return (char)b;
            }

            return CodePageTable[b - 128];
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            // Validate Parameters
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException(byteIndex < 0 ? nameof(byteIndex) : nameof(byteCount), "The given number was negative.");
            }

            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "'count' is greater than the length of the array.");
            }

            // If no input just return 0, fixed doesn't like 0 length arrays
            if (byteCount == 0)
            {
                return 0;
            }

            for (int i = byteIndex; i < byteCount; i++)
            {
                chars[charIndex + i] = GetChar(bytes[i]);
            }

            return chars.Length;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(charCount), "The given number was negative.");
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
