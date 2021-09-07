using System;
using System.Collections.Generic;
using System.Text;
using ZLibrary.Constants;
using ZLibrary.Story;

namespace ZLibrary.Machine
{
    public class ZInput : IZInput
    {
        private class Token
        {
            public readonly byte StartPos;
            public readonly byte Length;

            public Token(byte startPos, byte length)
            {
                StartPos = startPos;
                Length = length;
            }
        }

        private readonly ZStory _story;

        private readonly IZScreen _screen;

        private readonly ZMemory _memory;

        public ZInput(ZStory aStory, IZScreen aScreen, ZMemory aMemory)
        {
            _story = aStory;
            _screen = aScreen;
            _memory = aMemory;
        }

        public bool HandleInputTimer(ushort aRoutine)
        {
            int result = _memory.Stack.Pop();
            return (result != 0);
        }

        public ushort Read(ushort aCharacterBufferAddress, ushort aTokenBufferAddress, ushort aTimeout, ushort aTimeoutCallback)
        {
            byte initlen;

            _memory.GetByte(aCharacterBufferAddress, out byte max);
            if (_story.Header.Version <= FileVersion.V4)
            {
                max--;
            }

            if (max >= 200)
            {
                max = 200 - 1;
            }

            if (_story.Header.Version >= FileVersion.V5)
            {
                aCharacterBufferAddress++;
                _memory.GetByte(aCharacterBufferAddress, out initlen);
            }
            else
            {
                initlen = 0;
            }

            if (_story.Header.Version >= FileVersion.V3)
            {
                _screen.ShowStatus();
            }

            string initial = string.Empty;
            if (initlen > 0)
            {
                // we never get here for V1-4
                StringBuilder sb = new StringBuilder(initlen);
                for (int i = 0; i < initlen; i++)
                {
                    _memory.GetByte((aCharacterBufferAddress + 2 + i), out byte value);
                    sb.Append(ZText.CharFromZSCII(value));
                }
                initial = sb.ToString();
            }

            string s = _screen.ReadLine(initial, aTimeout, aTimeoutCallback, ZText.terminatingChars, out byte terminator);
            byte[] chars = ZText.StringToZSCII(s.ToLower());
            if (_story.Header.Version <= FileVersion.V4)
            {
                for (int i = 0; i < Math.Min(chars.Length, max); i++)
                {
                    _memory.SetByte((aCharacterBufferAddress + 1 + i), chars[i]);
                }
                _memory.SetByte((aCharacterBufferAddress + 1 + Math.Min(chars.Length, max)), 0);
            }
            else
            {
                _memory.SetByte((aCharacterBufferAddress + 1), (byte)chars.Length);
                for (int i = 0; i < Math.Min(chars.Length, max); i++)
                {
                    _memory.SetByte((aCharacterBufferAddress + 2 + i), chars[i]);
                }
            }

            if (aTokenBufferAddress != 0)
            {
                Tokenize(aCharacterBufferAddress, aTokenBufferAddress, 0, false);
            }

            return terminator;
        }

        public short FilterInput(short aChar)
        {
            // only allow characters that are defined for input: section 3.8
            if (aChar < 32 && (aChar != 8 && aChar != 13 && aChar != 27))
            {
                return 0;
            }

            if (aChar >= 127 && (aChar <= 128 || aChar >= 255))
            {
                return 0;
            }

            return aChar;
        }

        private bool IsWhitespace(byte aChar)
        {
            return (aChar == 9) || (aChar == 32);
        }

        private bool IsWordSeparator(byte aChar, ushort aUserDictionaryAddress)
        {
            byte[] seps;

            if (aUserDictionaryAddress == 0)
            {
                seps = ZText.wordSeparators;
            }
            else
            {
                _memory.GetByte(aUserDictionaryAddress, out byte n);
                _memory.GetBytes(aUserDictionaryAddress + 1, n, out seps);
            }

            return (Array.IndexOf(seps, aChar) >= 0);
        }

        private List<Token> SplitTokens(byte[] aBuffer, ushort aUserDictionaryAddress)
        {
            var result = new List<Token>();

            int xStart = -1;

            for (int i = 0; i < aBuffer.Length; i++)
            {
                // End of string
                if (aBuffer[i] == 0 && xStart >=0)
                {
                    result.Add(new Token((byte)xStart, (byte)(i - xStart)));
                    break;
                }

                // Skip whitespace
                if (IsWhitespace(aBuffer[i]) && xStart <0)
                {
                    continue;
                }

                if (IsWordSeparator(aBuffer[i], aUserDictionaryAddress))
                {
                    result.Add(new Token((byte)i, 1));
                    xStart = -1;
                }
                else
                {
                    if (xStart < 0)
                    {
                        xStart = i;
                    }

                    // find the end of the word
                    if (i < aBuffer.Length && !IsWhitespace(aBuffer[i]) && !IsWordSeparator(aBuffer[i], aUserDictionaryAddress))
                    {
                        continue;
                    }

                    // add it to the list
                    result.Add(new Token((byte) xStart, (byte)(i - xStart)));
                    xStart = -1;
                }
            }

            return result;
        }

        private void Tokenize(ushort aCharacterBufferAddress, ushort aTokenBufferAddress, ushort aUserDictionaryAddress, bool aSkipUnrecognized)
        {
            byte bufLen;
            int tokenOffset;

            _memory.SetByte(aTokenBufferAddress + 1, 0);

            if (_story.Header.Version >= FileVersion.V4)
            {
                bufLen = 0;
                tokenOffset = 1;

                for (int i = aCharacterBufferAddress + 1; i < _story.Header.DynamicSize; i++)
                {
                    _memory.GetByte(i, out byte value);
                    if (value == 0)
                    {
                        bufLen = (byte)(i - aCharacterBufferAddress - 1);
                        break;
                    }
                }
            }
            else
            {
                _memory.GetByte((aCharacterBufferAddress + 1), out bufLen);
                tokenOffset = 1;
            }

            _memory.GetByte((aTokenBufferAddress + 0), out byte max);
            byte count = 0;

            _memory.GetBytes(aCharacterBufferAddress + tokenOffset, bufLen, out byte[] myBuffer);
            List<Token> tokens = SplitTokens(myBuffer, aUserDictionaryAddress);

            foreach (Token tok in tokens)
            {
                ushort word = LookUpWord(myBuffer, tok.StartPos, tok.Length, aUserDictionaryAddress);
                if (word == 0 && aSkipUnrecognized)
                {
                    continue;
                }

                _memory.SetByte((aTokenBufferAddress + 1), (byte)(count + 1));
                _memory.SetByte((aTokenBufferAddress + tokenOffset + 4 * count + 1), (byte) (word >> 8));
                _memory.SetByte((aTokenBufferAddress + tokenOffset + 4 * count + 2), (byte) (word & 0xff));
                _memory.SetByte((aTokenBufferAddress + tokenOffset + 4 * count + 3), tok.Length);
                _memory.SetByte((aTokenBufferAddress + tokenOffset + 4 * count + 4), (byte)(tokenOffset + tok.StartPos));
                count++;

                if (count == max)
                {
                    break;
                }
            }
        }

        private ushort LookUpWord(byte[] aBuffer, int aPosition, int aLength, ushort aUserDictionaryAddress)
        {
            int dictStart;

            byte[] word = ZText.EncodeText(aBuffer, aPosition, aLength, ZText.DictionaryWordSize);

            if (aUserDictionaryAddress != 0)
            {
                _memory.GetByte(aUserDictionaryAddress, out byte n);
                dictStart = aUserDictionaryAddress + 1 + n;
            }
            else
            {
                dictStart = _story.Header.DictionaryOffset + 1 + ZText.wordSeparators.Length;
            }

            _memory.GetByte(dictStart++, out byte entryLength);

            ushort entries;
            if (aUserDictionaryAddress == 0)
            {
                _memory.GetWord(dictStart, out entries);
            }
            else
            {
                _memory.GetWord(dictStart, out entries);
            }

            dictStart += 2;

            if ((short)entries < 0)
            {
                // use linear search for unsorted user dictionary
                for (int i = 0; i < entries; i++)
                {
                    int addr = dictStart + i * entryLength;
                    if (CompareWords(word, addr) == 0)
                    {
                        return (ushort)addr;
                    }
                }
            }
            else
            {
                // use binary search
                int start = 0, end = entries;
                while (start < end)
                {
                    int mid = (start + end) / 2;
                    int addr = dictStart + mid * entryLength;
                    int cmp = CompareWords(word, addr);
                    if (cmp == 0)
                    {
                        return (ushort)addr;
                    }

                    if (cmp < 0)
                    {
                        end = mid;
                    }
                    else
                    {
                        start = mid + 1;
                    }
                }
            }

            return 0;
        }

        private int CompareWords(byte[] aWord, int aAddress)
        {
            for (int i = 0; i < aWord.Length; i++)
            {
                _memory.GetByte(aAddress + i, out byte value);
                int cmp = aWord[i] - value;
                if (cmp != 0)
                {
                    return cmp;
                }
            }

            return 0;
        }
    }
}
