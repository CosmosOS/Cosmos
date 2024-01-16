using System;
using System.Text;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Cosmos.System
{
    //Modified to be used on cosmos
    public partial class Console
    {

        internal sealed class ConsoleEncoding : Encoding
        {
            private readonly Encoding _encoding;

            internal ConsoleEncoding(Encoding encoding)
            {
                _encoding = encoding;
            }

            public override byte[] GetPreamble()
            {
                return Array.Empty<byte>();
            }

            public override int CodePage => _encoding.CodePage;

            public override bool IsSingleByte => _encoding.IsSingleByte;

            public override string EncodingName => _encoding.EncodingName;

            public override string WebName => _encoding.WebName;

            public override int GetByteCount(char[] chars) => _encoding.GetByteCount(chars);

            public override unsafe int GetByteCount(char* chars, int count) => _encoding.GetByteCount(chars, count);

            public override int GetByteCount(char[] chars, int index, int count) => _encoding.GetByteCount(chars, index, count);

            public override int GetByteCount(string s) => _encoding.GetByteCount(s);

            public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount) => _encoding.GetBytes(chars, charCount, bytes, byteCount);

            public override byte[] GetBytes(char[] chars) => _encoding.GetBytes(chars);

            public override byte[] GetBytes(char[] chars, int index, int count) => _encoding.GetBytes(chars, index, count);

            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                if (chars is null)
                {
                    throw new ArgumentNullException(nameof(chars));
                }

                if (bytes is null)
                {
                    throw new ArgumentNullException(nameof(bytes));
                }

                return _encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            }

            public override byte[] GetBytes(string s) => _encoding.GetBytes(s);

            public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                if (string.IsNullOrEmpty(s))
                {
                    throw new ArgumentException($"'{nameof(s)}' cannot be null or empty.", nameof(s));
                }

                if (bytes is null)
                {
                    throw new ArgumentNullException(nameof(bytes));
                }

                return _encoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
            }

            public override unsafe int GetCharCount(byte* bytes, int count) => _encoding.GetCharCount(bytes, count);

            public override int GetCharCount(byte[] bytes) => _encoding.GetCharCount(bytes);

            public override int GetCharCount(byte[] bytes, int index, int count) => _encoding.GetCharCount(bytes, index, count);

            public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount) => _encoding.GetChars(bytes, byteCount, chars, charCount);

            public override char[] GetChars(byte[] bytes) => _encoding.GetChars(bytes);

            public override char[] GetChars(byte[] bytes, int index, int count) => _encoding.GetChars(bytes, index, count);

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) => _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

            public override Decoder GetDecoder() => _encoding.GetDecoder();

            public override Encoder GetEncoder() => _encoding.GetEncoder();

            public override int GetMaxByteCount(int charCount) => _encoding.GetMaxByteCount(charCount);

            public override int GetMaxCharCount(int byteCount) => _encoding.GetMaxCharCount(byteCount);

            public override string GetString(byte[] bytes)
            {
                ArgumentNullException.ThrowIfNull(bytes, nameof(bytes));
                return _encoding.GetString(bytes);
            }

            public override string GetString(byte[] bytes, int index, int count) => _encoding.GetString(bytes, index, count);
        }
    }
}
