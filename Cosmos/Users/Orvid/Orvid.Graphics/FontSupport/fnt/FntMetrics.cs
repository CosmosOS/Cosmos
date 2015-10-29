using System;

namespace Orvid.Graphics.FontSupport.fnt
{
    internal class FntFontMetrics : FontMetrics
    {
    	private Rectangle box;
    	
        public FntFontMetrics(FntFont font)
            : base(font)
        {
        	this.box = font.GetBounds();
        }

        public override int GetHeight()
        {
            return box.Height;
        }

        public override int GetAscent()
        {
            return Math.Abs(box.Y);
        }

        public override int GetDescent()
        {
            return box.Height + Math.Abs(box.Y);
        }

        public override int GetLeading()
        {
            return Math.Abs(box.X);
        }

        public override int GetMaxAdvance()
        {
            return box.Width;
        }

        public override int CharWidth(char ch)
        {
            FntGlyph g = ((FntFont)(font)).GetLoader().GetGlyph(ch);
            if (g != null)
            {
                return (int)((g.Width + 1) & ~1);
            }
            return 0;
        }

        public override int[] CharsWidths(char[] chars, int start, int len)
        {
            int[] advances = new int[len];
            int adv_idx = 0;

            for (int i = start; i < start + len; i++)
            {
                FntGlyph glyph = ((FntFont)(font)).GetLoader().GetGlyph(chars[i]);
                if (adv_idx == 0)
                {
                    advances[adv_idx++] = glyph.Width;
                }
                else
                {
                    advances[adv_idx++] = advances[adv_idx - 1] + glyph.Width;
                }
            }

            return advances;
        }

        public override int CharsWidth(char[] chars, int start, int len)
        {
            int total = 0;
            int[] lengths = CharsWidths(chars, start, len);
            for (int i = 0; i < lengths.Length; i++)
                total += lengths[i];

            return total;
        }
    }
}
