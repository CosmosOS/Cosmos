using System;
using System.Collections.Generic;

namespace Orvid.Graphics.FontSupport
{
    public abstract class AbstractFontProvider<F, FD> : IFontProvider<F> where F : Font
    {

        private Dictionary<Font, ITextRenderer> renderers = new Dictionary<Font, ITextRenderer>();
        private Dictionary<Font, FontMetrics> metrics = new Dictionary<Font, FontMetrics>();
        private Dictionary<String, F> fontsByName = new Dictionary<String, F>();
        private bool fontsLoaded = false;
        private Image renderCache;
        private String name;
        private List<FD> userFontDatas = new List<FD>();
        private Dictionary<FD, Size> maxCharBounds = new Dictionary<FD, Size>();


        protected AbstractFontProvider(String name)
        {
            this.name = name;
        }


        public override String Name
        {
            get
            {
                return name;
            }
        }


        public override bool SupportsFormat(Font font)
        {
            if (font == null) 
                return false; // don't provide default (null) fonts
            if (font is F)
                return true;

            loadFonts();
            Font f;
            return GetCompatibleFont(font, out f);
        }

        public override List<Font> Fonts
        {
            get
            {
                loadFonts();
                List<Font> fnts = new List<Font>();
                foreach (var f in fontsByName)
                {
                    fnts.Add(f.Value);
                }
                return fnts;
            }
        }

        public override ITextRenderer GetTextRenderer(Font font)
        {
            ITextRenderer r;
            renderers.TryGetValue(font, out r);
            if (r == null)
            {
                r = CreateTextRenderer(renderCache, font);
                renderers.Add(font, r);
            }
            return r;
        }

        public override FontMetrics GetFontMetrics(Font font)
        {
            FontMetrics fm = metrics[font];

            if (fm == null)
            {
                try
                {
                    fm = CreateFontMetrics(font);
                    metrics.Add(font, fm);
                }
                catch (System.IO.IOException ex)
                {
                    throw new Exception("Cannot create font metrics for " + font, ex);
                }
            }

            return fm;
        }

        private void loadFonts()
        {
            if (!fontsLoaded)
            {
                LoadFontsImpl();
                fontsLoaded = true;
            }
        }

        protected abstract FontMetrics CreateFontMetrics(Font font);
        protected abstract void LoadFontsImpl();
        protected abstract ITextRenderer CreateTextRenderer(Image renderCache, Font font);
        protected abstract Size GetMaxCharSize(FD fontData);


        public override bool GetCompatibleFont(Font font, out Font fnt)
        {
            if (font is F)
            {
                fnt = (F)font;
                return true;
            }

            foreach (KeyValuePair<string, F> i in fontsByName)
            {
                if (i.Value.Name == font.Name)
                {
                    fnt = (F)i.Value;
                    return true;
                }
            }


#warning TODO: Implement Conversion between formats.
            fnt = null;
            return false;
        }

        protected void addUserFontData(FD data)
        {
            userFontDatas.Add(data);
        }

        protected List<FD> getUserFontDatas()
        {
            return userFontDatas;
        }

        protected void addFont(F font)
        {
            fontsByName.Add(font.Name, font); 
        }


        public BoundingBox getMaxCharBounds(FD container)
        {
            Size size = maxCharBounds[container];

            if (size == null)
            {
                size = GetMaxCharSize(container);
                maxCharBounds.Add(container, size);
            }

            return new BoundingBox(0, size.maxCharWidth, size.maxCharHeight, 0);
        }

        public class Size
        {
            public int maxCharWidth = 0;
            public int maxCharHeight = 0;
        }
    }
}
