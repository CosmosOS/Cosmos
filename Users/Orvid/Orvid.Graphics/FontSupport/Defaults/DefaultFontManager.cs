using System;
using System.Collections.Generic;
using System.IO;

namespace Orvid.Graphics.FontSupport
{
    /// <summary>
    /// The default FontManager.
    /// </summary>
    public class DefaultFontManager : FontManager
    {
        
    	List<Font> Providers = new List<Font>();


        public DefaultFontManager()
        {
            Providers.Add(new bdf.BDFFont());
            //Providers.Add(2, new fnt.FntFont());
        }


        public override string Name
        {
            get { return "Default Font Manager"; }
        }

        public override FontMetrics GetFontMetrics(Font font)
        {
        	return font.GetFontMetrics();
        }

        public override void DrawText(Image i, BoundingBox clip, AffineTransform trans, string s, Font f, Vec2 Loc, Pixel p)
        {
            f.Render(i, clip, trans, s, Loc, p);
        }

        public override Font LoadFont(int format, Stream s)
        {
        	if (format > Providers.Count)
        	{
        		throw new Exception("Unknown format!");
        	}
        	
        	return Providers[format].LoadFont(s);

            throw new Exception("can't create font with format #'" + format.ToString() + "'");
        }

        public override Font[] Fonts
        {
            get
            {
                List<Font> all = new List<Font>();
                foreach (Font prv in Providers)
                {
                    all.AddRange(prv.DefaultFonts);
                }
                return all.ToArray();
            }
        }
    }
}
