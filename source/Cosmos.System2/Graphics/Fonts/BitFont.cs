using Cosmos.System.Graphics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Cosmos.System.Graphics.Fonts
{
    public struct BitFontDescriptor
    {
        public string Charset;
        public MemoryStream MS;
        public int Size;
        public BitFontDescriptor(string aCharset, MemoryStream aMS, int aSize)
        {
            this.Charset = aCharset;
            this.MS = aMS;
            this.Size = aSize;
        }
    }

    /// <summary>
    /// For More https://github.com/nifanfa/BitFont
    /// </summary>
    static class BitFont
    {
        public static Dictionary<string, BitFontDescriptor> RegisteredBitFont = new Dictionary<string, BitFontDescriptor>();

        /// <summary>
        /// The BitFont Should Be Left Aligned
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="bitFontDescriptor"></param>
        public static void RegisterBitFont(string Name, BitFontDescriptor bitFontDescriptor)
        {
            RegisteredBitFont.Add(Name, bitFontDescriptor);
        }

        /// <summary>
        /// Draw BitFont String
        /// </summary>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <param name="canvas"></param>
        /// <param name="FontName"></param>
        /// <param name="color"></param>
        /// <param name="Text"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Devide"></param>
        public static void DrawBitFontString(this Canvas canvas, string FontName, Color color, string Text, int X, int Y, int Devide = 2, bool DisableAntiAliasing = false)
        {
            if (!RegisteredBitFont.ContainsKey(FontName))
            {
                throw new KeyNotFoundException($"\"{FontName}\" is not registered yet");
            }

            BitFontDescriptor bitFontDescriptor = RegisteredBitFont[FontName];
            string[] Lines = Text.Split('\n');
            for (int l = 0; l < Lines.Length; l++)
            {
                int UsedX = 0;
                for (int i = 0; i < Lines[l].Length; i++)
                {
                    char c = Lines[l][i];
                    UsedX += DrawBitFontChar(canvas, bitFontDescriptor.MS, bitFontDescriptor.Size, color, bitFontDescriptor.Charset.Impl_Str_IndexOf(c), UsedX + X, Y + bitFontDescriptor.Size * l, !DisableAntiAliasing) + Devide;
                }
            }
        }

        public static int Impl_Str_IndexOf(this string s, char c)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char aC = s[i];
                if (aC == c)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Return Font Used Width
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="MemoryStream"></param>
        /// <param name="Size"></param>
        /// <param name="Color"></param>
        /// <param name="Index"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static int DrawBitFontChar(Canvas canvas, MemoryStream MemoryStream, int Size, Color Color, int Index, int X, int Y, bool UseAntiAliasing)
        {
            if (Index == -1) return Size / 2;

            int MaxX = 0;

            bool LastPixelIsNotDrawn = false;

            int SizePerFont = Size * (Size / 8);
            byte[] Font = new byte[SizePerFont];
            MemoryStream.Seek(SizePerFont * Index, SeekOrigin.Begin);
            MemoryStream.Read(Font, 0, Font.Length);

            for (int h = 0; h < Size; h++)
            {
                for (int aw = 0; aw < Size / 8; aw++)
                {

                    for (int ww = 0; ww < 8; ww++)
                    {
                        if ((Font[(h * (Size / 8)) + aw] & (0x80 >> ww)) != 0)
                        {
                            canvas.DrawPoint(new Pen(Color), X + (aw * 8) + ww, Y + h);

                            if ((aw * 8) + ww > MaxX)
                            {
                                MaxX = (aw * 8) + ww;
                            }

                            if (LastPixelIsNotDrawn)
                            {
                                if (UseAntiAliasing)
                                {
                                    canvas.DrawPoint(new Pen(Color.FromArgb(Color.R / 2, Color.G / 2, Color.B / 2)), X + (aw * 8) + ww - 1, Y + h);
                                }

                                LastPixelIsNotDrawn = false;
                            }
                        }
                        else
                        {
                            LastPixelIsNotDrawn = true;
                        }
                    }
                }
            }

            return MaxX;
        }
    }
}
