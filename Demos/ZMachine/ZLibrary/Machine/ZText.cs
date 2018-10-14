using System;
using System.Collections.Generic;
using System.Text;
using ZLibrary.Constants;

namespace ZLibrary.Machine
{
    public static class ZText
    {
        public static int DictionaryWordSize;

        public static char[] alphabet0;
        public static char[] alphabet1;
        public static char[] alphabet2;

        public static byte[] wordSeparators;
        public static byte[] terminatingChars;
        private static char[] extraChars;

        // default alphabets (S 3.5.3)
        private static readonly char[] defaultAlphabet0 =
        { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        private static readonly char[] defaultAlphabet1 =
        { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private static readonly char[] defaultAlphabet2 =
        { ' ', '\n', '0', '1', '2', '3',  '4', '5', '6',  '7', '8', '9', '.',
            ',', '!',  '?', '_', '#', '\'', '"', '/', '\\', '-', ':', '(', ')' };

        // default Unicode translations (S 3.8.5.3)
        private static readonly char[] defaultExtraChars =
        {
            '\u00e4', '\u00f6', '\u00fc', '\u00c4', '\u00d6', '\u00dc', '\u00df', '\u00bb', '\u00ab', '\u00eb',
            '\u00ef', '\u00ff', '\u00cb', '\u00cf', '\u00e1', '\u00e9', '\u00ed', '\u00f3', '\u00fa', '\u00fd',
            '\u00c1', '\u00c9', '\u00cd', '\u00d3', '\u00da', '\u00dd', '\u00e0', '\u00e8', '\u00ec', '\u00f2',
            '\u00f9', '\u00c0', '\u00c8', '\u00cc', '\u00d2', '\u00d9', '\u00e2', '\u00ea', '\u00ee', '\u00f4',
            '\u00fb', '\u00c2', '\u00ca', '\u00ce', '\u00d4', '\u00db', '\u00e5', '\u00c5', '\u00f8', '\u00d8',
            '\u00e3', '\u00f1', '\u00f5', '\u00c3', '\u00d1', '\u00d5', '\u00e6', '\u00c6', '\u00e7', '\u00c7',
            '\u00fe', '\u00f0', '\u00de', '\u00d0', '\u00a3', '\u0153', '\u0152', '\u00a1', '\u00bf'
        };

        private static ZMachine _machine;

        public static void Initialize(ZMachine aMachine)
        {
            _machine = aMachine;
            DictionaryWordSize =  aMachine.Story.Header.Version >= FileVersion.V4 ? 9 : 6;
            LoadAlphabets();
            LoadExtraChars();
            LoadTerminatingChars();
            LoadWordSeparators();
        }

        private static void LoadAlphabets()
        {
            _machine.Memory.GetWord(0x34, out ushort userAlphabets);
            if (userAlphabets == 0)
            {
                alphabet0 = defaultAlphabet0;
                alphabet1 = defaultAlphabet1;
                alphabet2 = defaultAlphabet2;
            }
            else
            {
                alphabet0 = new char[26];
                for (int i = 0; i < 26; i++)
                {
                    ushort addr = (ushort) (userAlphabets + i);
                    _machine.Memory.GetByte(addr, out byte value);
                    alphabet0[i] = CharFromZSCII(value);
                }

                alphabet1 = new char[26];
                for (int i = 0; i < 26; i++)
                {
                    ushort addr = (ushort) (userAlphabets + 26 + i);
                    _machine.Memory.GetByte(addr, out byte value);
                    alphabet1[i] = CharFromZSCII(value);
                }

                alphabet2 = new char[26];
                alphabet2[0] = ' '; // escape code
                alphabet2[1] = '\n'; // new line
                for (int i = 2; i < 26; i++)
                {
                    ushort addr = (ushort) (userAlphabets + 52 + i);
                    _machine.Memory.GetByte(addr, out byte value);
                    alphabet2[i] = CharFromZSCII(value);
                }
            }
        }

        private static void LoadExtraChars()
        {
            extraChars = defaultExtraChars;
        }

        private static void LoadTerminatingChars()
        {
            _machine.Memory.GetWord(0x2E, out ushort terminatingTable);
            if (terminatingTable == 0)
            {
                terminatingChars = new byte[0];
            }
            else
            {
                List<byte> temp = new List<byte>();
                _machine.Memory.GetByte(terminatingTable, out byte b);
                while (b != 0)
                {
                    if (b == 255)
                    {
                        // 255 means every possible terminator, so don't bother with the rest of the list
                        temp.Clear();
                        temp.Add(255);
                        break;
                    }

                    temp.Add(b);
                    _machine.Memory.GetByte(++terminatingTable, out b);
                }
                terminatingChars = temp.ToArray();
            }
        }

        private static void LoadWordSeparators()
        {
            // read word separators
            _machine.Memory.GetByte(_machine.Story.Header.DictionaryOffset, out byte n);
            wordSeparators = new byte[n];
            for (int i = 0; i < n; i++)
            {
                _machine.Memory.GetByte((ushort) (_machine.Story.Header.DictionaryOffset + 1 + i), out wordSeparators[i]);
            }
        }

        /// <summary>
        /// Encodes a section of text, optionally truncating or padding the output to a fixed size.
        /// </summary>
        /// <param name="input">The buffer containing the plain text.</param>
        /// <param name="start">The index within <paramref name="input"/> where the
        /// plain text starts.</param>
        /// <param name="length">The length of the plain text.</param>
        /// <param name="numZchars">The number of 5-bit characters that the output should be
        /// truncated or padded to, which must be a multiple of 3; or 0 to allow variable size
        /// output (padded up to a multiple of 2 bytes, if necessary).</param>
        /// <returns>The encoded text, with th.</returns>
        public static byte[] EncodeText(byte[] input, int start, int length, int numZchars)
        {
            List<byte> zchars;
            if (numZchars == 0)
            {
                zchars = new List<byte>(length);
            }
            else
            {
                if (numZchars < 0 || numZchars % 3 != 0)
                {
                    throw new ArgumentException("Output size must be a multiple of 3", "numZchars");
                }

                zchars = new List<byte>(numZchars);
            }

            for (int i = 0; i < length; i++)
            {
                byte zc = input[start + i];
                char ch = CharFromZSCII(zc);

                if (ch == ' ')
                {
                    zchars.Add(0);
                }
                else
                {
                    int alpha;
                    if ((alpha = Array.IndexOf(alphabet0, ch)) >= 0)
                    {
                        zchars.Add((byte)(alpha + 6));
                    }
                    else if ((alpha = Array.IndexOf(alphabet1, ch)) >= 0)
                    {
                        zchars.Add(4);
                        zchars.Add((byte)(alpha + 6));
                    }
                    else if ((alpha = Array.IndexOf(alphabet2, ch)) >= 0)
                    {
                        zchars.Add(5);
                        zchars.Add((byte)(alpha + 6));
                    }
                    else
                    {
                        zchars.Add(5);
                        zchars.Add(6);
                        zchars.Add((byte)(zc >> 5));
                        zchars.Add((byte)(zc & 31));
                    }
                }
            }

            int resultBytes;
            if (numZchars == 0)
            {
                // pad up to a multiple of 3
                while (zchars.Count % 3 != 0)
                {
                    zchars.Add(5);
                }

                resultBytes = zchars.Count * 2 / 3;
            }
            else
            {
                // pad up to the fixed size
                while (zchars.Count < numZchars)
                {
                    zchars.Add(5);
                }

                resultBytes = numZchars * 2 / 3;
            }

            byte[] result = new byte[resultBytes];
            int zi = 0, ri = 0;
            while (ri < resultBytes)
            {
                result[ri] = (byte)((zchars[zi] << 2) | (zchars[zi + 1] >> 3));
                result[ri + 1] = (byte)((zchars[zi + 1] << 5) | zchars[zi + 2]);
                ri += 2;
                zi += 3;
            }

            result[resultBytes - 2] |= 128;
            return result;
        }

        public static string DecodeString(ushort address)
        {
            int dummy;
            return DecodeStringWithLen(address, out dummy);
        }

        public static string DecodeStringWithLen(long address, out int len)
        {
            len = 0;

            int alphabet = 0;
            int abbrevMode = 0;
            ushort word;
            StringBuilder sb = new StringBuilder();

            do
            {
                _machine.Memory.GetWord(address, out word);
                address += 2;
                len += 2;

                DecodeChar((word >> 10) & 0x1F, ref alphabet, ref abbrevMode, sb);
                DecodeChar((word >> 5) & 0x1F, ref alphabet, ref abbrevMode, sb);
                DecodeChar((word) & 0x1F, ref alphabet, ref abbrevMode, sb);
            } while ((word & 0x8000) == 0);

            return sb.ToString();
        }

        public static void DecodeChar(int zchar, ref int alphabet, ref int abbrevMode, StringBuilder sb)
        {
            switch (abbrevMode)
            {
                case 1:
                case 2:
                case 3:
                    sb.Append(GetAbbreviation((short)(32 * (abbrevMode - 1) + zchar)));
                    abbrevMode = 0;
                    return;

                case 4:
                    abbrevMode = 5;
                    alphabet = zchar;
                    return;
                case 5:
                    abbrevMode = 0;
                    sb.Append(CharFromZSCII((short)((alphabet << 5) + zchar)));
                    alphabet = 0;
                    return;
            }

            switch (zchar)
            {
                case 0:
                    sb.Append(' ');
                    return;

                case 1:
                case 2:
                case 3:
                    abbrevMode = zchar;
                    return;

                case 4:
                    alphabet = 1;
                    return;
                case 5:
                    alphabet = 2;
                    return;
            }

            zchar -= 6;
            switch (alphabet)
            {
                case 0:
                    sb.Append(alphabet0[zchar]);
                    return;

                case 1:
                    sb.Append(alphabet1[zchar]);
                    alphabet = 0;
                    return;

                case 2:
                    if (zchar == 0)
                    {
                        abbrevMode = 4;
                    }
                    else
                    {
                        sb.Append(alphabet2[zchar]);
                    }

                    alphabet = 0;
                    return;
            }
        }

        private static string GetAbbreviation(int num)
        {
            ushort abbreviationsOffset = (ushort)(_machine.Story.Header.AbbreviationsOffset + num * 2);
            _machine.Memory.GetWord(abbreviationsOffset, out ushort address);
            return DecodeString((ushort)(address * 2)); // word address, not byte address!
        }

        public static char CharFromZSCII(short ch)
        {
            switch (ch)
            {
                case 13:
                    return '\n';

                default:
                    if (ch >= 155 && ch < 155 + extraChars.Length)
                    {
                        return extraChars[ch - 155];
                    }
                    else
                    {
                        return (char)ch;
                    }
            }
        }

        public static short CharToZSCII(char ch)
        {
            switch (ch)
            {
                case '\n':
                    return 13;

                default:
                    int idx = Array.IndexOf(extraChars, ch);
                    if (idx >= 0)
                    {
                        return (short)(155 + idx);
                    }
                    else
                    {
                        return (short)ch;
                    }
            }
        }

        public static byte[] StringToZSCII(string str)
        {
            byte[] result = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                result[i] = (byte)CharToZSCII(str[i]);
            }

            return result;
        }
    }
}
