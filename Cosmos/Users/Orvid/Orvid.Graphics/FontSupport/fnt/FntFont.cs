using System;

namespace Orvid.Graphics.FontSupport.fnt
{
	internal class FntFont : Font
    {
        private FntLoader ldr;
        private FntFontMetrics metrics;

        /// <summary>
        /// The constructor for the loader.
        /// </summary>
        public FntFont() : base(true) { }

        public FntFont(FntLoader loadr) 
            : base(loadr.GetFamily(), loadr.GetStyle(), loadr.GetSize())
        {
            ldr = loadr;
            metrics = new FntFontMetrics(this);
        }

        public FntLoader GetLoader()
        {
            return ldr;
        }

        public Rectangle GetBounds()
        {
            Rectangle r = new Rectangle();
            r.SetBounds(0, ldr.Ascent, ldr.MaxWidth, ldr.PixHeight);
            return r;
        }

        public override FontMetrics GetFontMetrics()
        {
            return metrics;
        }
        
		public override ITextRenderer GetTextRenderer()
		{
			return new FntTextRenderer(ldr);
		}
		
		public override bool IsSupportedType(Font f)
		{
			return (f is FntFont);
		}
		
		public override Font LoadFont(System.IO.Stream s)
		{
			FntLoader l = new FntLoader();
			l.Load(s);
			return new FntFont(l);
		}
		
		public override void Render(Image i, BoundingBox clip, AffineTransform trans, string text, Vec2 loc, Pixel color)
		{
			GetTextRenderer().Render(i,clip,trans,text,loc,color);
		}
		
		public override string ProviderName
		{
			get 
			{
				return "Default Fnt Font Provider";
			}
		}
		
		public override System.Collections.Generic.List<Font> DefaultFonts 
		{
			get 
			{
				return new System.Collections.Generic.List<Font>();
			}
		}
    }
}
