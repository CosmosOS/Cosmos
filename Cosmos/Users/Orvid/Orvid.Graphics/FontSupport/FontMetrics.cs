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

        public virtual int GetLeading()
        {
            return 0;
        }

        public virtual int GetAscent()
        {
            return (int)font.Size;
        }

        public virtual int GetDescent()
        {
            return 0;
        }

        public virtual int GetHeight()
        {
            return GetLeading() + GetAscent() + GetDescent();
        }

        public virtual int GetMaxAscent()
        {
            return GetAscent();
        }

        public virtual int GetMaxDescent()
        {
            return GetDescent();
        }

        public virtual int GetMaxAdvance()
        {
            return -1;
        }


        public virtual int CharWidth(char ch)
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

        public abstract int[] CharsWidths(char[] chars, int start, int len);

        public virtual int CharsWidth(char[] data, int off, int len)
        {
            return StringWidth(new String(data, off, len));
        }

        public virtual int[] GetWidths()
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
