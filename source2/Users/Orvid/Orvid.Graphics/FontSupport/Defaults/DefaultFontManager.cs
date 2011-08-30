using System;
using System.Collections.Generic;
using Orvid.Graphics.FontSupport.SupportClasses;
using System.IO;

namespace Orvid.Graphics.FontSupport
{
    /// <summary>
    /// The default FontManager.
    /// </summary>
    public class DefaultFontManager : FontManager
    {
        List<AbstractFontProvider<Font, object>> Providers = new List<AbstractFontProvider<Font, object>>();
        Dictionary<int, string> FontTypeToProviderNameMap = new Dictionary<int, string>();


        public DefaultFontManager()
        {
            Providers.Add(new bdf.BDFFontProvider());
            FontTypeToProviderNameMap.Add(1, "bdf");
        }


        public override string Name
        {
            get { return "Default Font Manager"; }
        }

        public override FontMetrics GetFontMetrics(Font font)
        {
            return GetProvider(font).GetFontMetrics(font);
        }

        public override void DrawText(Image i, BoundingBox clip, AffineTransform trans, string s, Font f, Vec2 Loc, Pixel p)
        {
            ITextRenderer renderer = GetProvider(f).GetTextRenderer(f);
            renderer.Render(i, clip, trans, s, Loc, p);
        }

        public override Font CreateFont(int format, Stream s)
        {
            String name = FontTypeToProviderNameMap[format];

            if (name == null)
            {
                throw new ArgumentException("unknown format " + format.ToString());
            }

            foreach (IFontProvider<Font> prv in Providers)
            {
                if (prv.Name == name)
                {
                    return prv.LoadFont(s);
                }
            }

            throw new Exception("can't create font with format " + name);
        }

        public override Font[] Fonts
        {
            get
            {
                List<Font> all = new List<Font>();
                foreach (IFontProvider<Font> prv in Providers)
                {
                    all.AddRange(prv.Fonts);
                }
                return all.ToArray();
            }
        }

        private IFontProvider<Font> GetProvider(Font font)
        {
            foreach (IFontProvider<Font> prv in Providers)
            {
                if (prv.SupportsFormat(font))
                {
                    return prv;
                }
            }

            return null;
        }
    }
}
