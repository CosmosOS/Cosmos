using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFMetrics
    {
        private BDFFontContainer font;
        private BDFParser.Rectangle box;

        public BDFMetrics(BDFFontContainer font)
        {
            this.font = font;
            this.box = font.getBoundingBox();
        }

        public int getAscent()
        {
            return box.height - getDescent();
        }

        public int getDescent()
        {
            return Math.Abs(box.y);
        }

        public int getLeading()
        {
            return Math.Abs(box.x);
        }

        public int getMaxAdvance()
        {
            return box.width;
        }

        public int getHeight()
        {
            return box.height;
        }

        public int charWidth(char ch)
        {
            BDFGlyph g = font.getGlyph(ch);
            if (g != null)
            {
                BDFParser.Rectangle r = g.getBbx();
                r.width = (r.width + 1) & ~1;
                return r.width;
            }
            return 0;
        }

        public int[] charsWidths(char[] chars, int start, int length)
        {
            int[] advances = new int[length];
            int adv_idx = 0;

            for (int i = start; i < start + length; i++)
            {
                BDFGlyph glyph = font.getGlyph(chars[i]);
                if (adv_idx == 0)
                {
                    advances[adv_idx++] = glyph.getDWidth().width;
                }
                else
                {
                    advances[adv_idx++] = advances[adv_idx - 1] + glyph.getDWidth().width;
                }
            }

            return advances;
        }

        public int charsWidth(char[] chars, int start, int end)
        {
            int total = 0;
            int[] lengths = charsWidths(chars, start, end);
            for (int i = 0; i < lengths.Length; i++)
                total += lengths[i];

            return total;
        }
    }
}
