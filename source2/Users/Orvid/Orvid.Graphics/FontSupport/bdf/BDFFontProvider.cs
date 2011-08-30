using System;
using System.Collections.Generic;
using System.IO;


namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFFontProvider : AbstractFontProvider<Font, object>
    {
        private List<BDFFontContainer> containers = new List<BDFFontContainer>();

        public BDFFontProvider()
            : base("bdf")
        {
        }

        /// <summary>
        /// The built-in fonts.
        /// </summary>
        private static List<byte[]> BuiltInFonts = new List<byte[]>
        {
#warning TODO: Add some fonts here.
        };

        protected override FontMetrics CreateFontMetrics(Font font)
        {
            Font bdfFont;
            GetCompatibleFont(font, out bdfFont);
            return ((BDFFont)bdfFont).getFontMetrics();
        }

        protected override void LoadFontsImpl()
        {
            foreach (BDFFontContainer container in getContainers())
            {
                addFont(new BDFFont(container));
            }
        }
        private List<BDFFontContainer> getContainers()
        {
            if (containers == null)
            {
                containers = new List<BDFFontContainer>();

                foreach (byte[] fontResource in BuiltInFonts)
                {
                    MemoryStream m = new MemoryStream(fontResource);
                    containers.Add(((BDFFont)LoadFont(m)).getContainer());
                }
            }
            return containers;
        }

        protected override ITextRenderer CreateTextRenderer(Image renderCache, Font font)
        {
            BDFFont bdfFont = (BDFFont)font;
            ITextRenderer renderer = new BDFTextRenderer(bdfFont.getContainer());
            return renderer;
        }

        protected override AbstractFontProvider<Font, object>.Size GetMaxCharSize(object fontData)
        {
            BDFFontContainer container = (BDFFontContainer)fontData;
            Size size = new Size();

            foreach (BDFGlyph g in container.getGlyphs())
            {
                if (g != null)
                {
                    size.maxCharWidth += g.getDWidth().width;
                    size.maxCharHeight = Math.Max(g.getDWidth().height, size.maxCharHeight);
                }
            }

            return size;
        }

        public override Font LoadFont(System.IO.Stream s)
        {
            BDFFontContainer container = BDFFontContainer.CreateFont(s);
            addUserFontData(container);
            return new BDFFont(container);
        }
    }
}
