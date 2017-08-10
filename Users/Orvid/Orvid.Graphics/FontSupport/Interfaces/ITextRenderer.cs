using System;

namespace Orvid.Graphics.FontSupport
{
    public abstract class ITextRenderer
    {
        public abstract void Render(Image i, BoundingBox clip, AffineTransform trans, string text, Vec2 loc, Pixel color);
    }
}
