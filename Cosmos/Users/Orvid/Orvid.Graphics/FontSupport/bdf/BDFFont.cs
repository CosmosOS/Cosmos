using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    internal class BDFFont : Font
    {
        private BDFFontContainer bdfContainer;
        private BDFFontMetrics metrics;

        /// <summary>
        /// The internal constructor for initializing this
        /// instance as a loader.
        /// </summary>
        internal BDFFont() : base(true) { }

        internal BDFFont(BDFFontContainer container) 
            : base(container.getFamily(), container.getStyle(), container.getSize())
        {
            bdfContainer = container;
            metrics = new BDFFontMetrics(this);
        }

        internal BDFFontContainer getContainer()
        {
            return bdfContainer;
        }
        
		public override System.Collections.Generic.List<Font> DefaultFonts 
		{
			get 
			{
				return 	new System.Collections.Generic.List<Font>();
			}
		}
        
		public override string ProviderName
		{
			get 
			{
				return "Default BDF Font Provider";
			}
		}

        public override FontMetrics GetFontMetrics()
        {
            return metrics;
        }
        
		public override ITextRenderer GetTextRenderer()
		{
            ITextRenderer renderer = new BDFTextRenderer(this.getContainer());
            return renderer;
		}
        
		public override bool IsSupportedType(Font f)
		{
			return (f is BDFFont);
		}
		
		public override void Render(Image i, BoundingBox clip, AffineTransform trans, string text, Vec2 loc, Pixel color)
		{
			GetTextRenderer().Render(i,clip,trans,text,loc,color);
		}
        
        public override Font LoadFont(System.IO.Stream s)
        {
            BDFFontContainer container = BDFFontContainer.CreateFont(s);
            return new BDFFont(container);
        }
    }
}
