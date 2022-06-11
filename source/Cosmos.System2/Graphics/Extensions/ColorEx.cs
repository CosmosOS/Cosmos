using System.Drawing;

namespace Cosmos.System.Graphics.Extensions;

public static class ColorEx
{
    public static Pen ToPen(this Color color, int width) => new(color, width);
}
