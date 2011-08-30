using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFFontMetrics : FontMetrics
    {
        private readonly BDFMetrics metrics;

        public BDFFontMetrics(BDFFont font)
            : base(font)
        {
            this.metrics = font.getContainer().getFontMetrics();
        }

        public int getHeight()
        {
            return metrics.getHeight();
        }

        public int getAscent()
        {
            return metrics.getAscent();
        }

        public int getDescent()
        {
            return metrics.getDescent();
        }

        public int getLeading()
        {
            return metrics.getLeading();
        }

        public int getMaxAdvance()
        {
            return metrics.getMaxAdvance();
        }

        public int charWidth(char ch)
        {
            return metrics.charWidth(ch);
        }

        public int[] charsWidths(char[] chars, int start, int len)
        {
            return metrics.charsWidths(chars, start, len);
        }

        public int charsWidth(char[] chars, int start, int len)
        {
            return metrics.charsWidth(chars, start, len);
        }
    }
}
