using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    internal class BDFFontMetrics : FontMetrics
    {
        private readonly BDFMetrics metrics;

        public BDFFontMetrics(BDFFont font)
            : base(font)
        {
            this.metrics = font.getContainer().getFontMetrics();
        }

        public override int GetHeight()
        {
            return metrics.getHeight();
        }

        public override int GetAscent()
        {
            return metrics.getAscent();
        }

        public override int GetDescent()
        {
            return metrics.getDescent();
        }

        public override int GetLeading()
        {
            return metrics.getLeading();
        }

        public override int GetMaxAdvance()
        {
            return metrics.getMaxAdvance();
        }

        public override int CharWidth(char ch)
        {
            return metrics.charWidth(ch);
        }

        public override int[] CharsWidths(char[] chars, int start, int len)
        {
            return metrics.charsWidths(chars, start, len);
        }

        public override int CharsWidth(char[] chars, int start, int len)
        {
            return metrics.charsWidth(chars, start, len);
        }
    }
}
