using System;

namespace Orvid.Graphics.FontSupport
{
    public abstract class FontMetrics
    {
        protected Font font;

        protected FontMetrics(Font font)
        {
            this.font = font;
        }

        public Font GetFont()
        {
            return font;
        }

        public int GetLeading()
        {
            return 0;
        }

        public int GetAscent()
        {
            return font.Size;
        }

        public int GetDescent()
        {
            return 0;
        }

        public int getHeight()
        {
            return GetLeading() + GetAscent() + GetDescent();
        }

        public int GetMaxAscent()
        {
            return GetAscent();
        }

        public int GetMaxDescent()
        {
            return GetDescent();
        }

        public int GetMaxAdvance()
        {
            return -1;
        }


        public int CharWidth(char ch)
        {
            if (ch < 256)
            {
                return GetWidths()[ch];
            }
            char[] data = { ch };
            return CharsWidth(data, 0, 1);
        }

        public int StringWidth(String str)
        {
            int len = str.Length;
            char[] data = new char[len];
            Array.Copy(str.ToCharArray(), 0, data, 0,len);
            return CharsWidth(data, 0, len);
        }

        public int CharsWidth(char[] data, int off, int len)
        {
            return StringWidth(new String(data, off, len));
        }

        public int[] GetWidths()
        {
            int[] widths = new int[256];
            for (char ch = (char)0; ch < 256; ch++)
            {
                widths[ch] = CharWidth(ch);
            }
            return widths;
        }
    }
}
