using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFFont : Font
    {
        private BDFFontContainer bdfContainer;
        private BDFFontMetrics metrics;

        public BDFFont(BDFFontContainer container) 
            : base(container.getFamily(), container.getStyle(), container.getSize())
        {
            bdfContainer = container;
            metrics = new BDFFontMetrics(this);
        }

        public BDFFontContainer getContainer()
        {
            return bdfContainer;
        }

        public BDFFontMetrics getFontMetrics()
        {
            return metrics;
        }
    }
}
