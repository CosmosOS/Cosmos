using System;
using System.Drawing;

namespace Cosmos.System.Graphics
{
    public class Pen
    {
        /// <summary>
        /// Pen color
        /// </summary>
        public Color Color;

        /// <summary>
        /// Pen width
        /// </summary>
        public int Width;

        /// <summary>
        /// Create a new pen
        /// </summary>
        /// <param name="color"></param>
        /// <param name="width"></param>
        public Pen(Color color, int width = 1)
        {
            // set properties
            Color = color;
            Width = width;

            // lowest width is 1
            if (Width < 1) { Width = 1; }

        }

        /// <summary>
        /// Convert pen properties into readable string
        /// </summary>
        /// <returns>Color and width as string</returns>
        public override string ToString()
        {
            return Color.ToString() + ", " + Width.ToString();
        }
    }

    /// <summary>
    /// List of named pens
    /// </summary>
    public static class Pens
    {
        public static readonly Pen Transparent = new Pen(Color.Transparent, 1);
        public static readonly Pen AliceBlue = new Pen(Color.AliceBlue, 1);
        public static readonly Pen AntiqueWhite = new Pen(Color.AntiqueWhite, 1);
        public static readonly Pen Aqua = new Pen(Color.Aqua, 1);
        public static readonly Pen Aquamarine = new Pen(Color.Aquamarine, 1);
        public static readonly Pen Azure = new Pen(Color.Azure, 1);
        public static readonly Pen Beige = new Pen(Color.Beige, 1);
        public static readonly Pen Bisque = new Pen(Color.Bisque, 1);
        public static readonly Pen Black = new Pen(Color.Black, 1);
        public static readonly Pen BlanchedAlmond = new Pen(Color.BlanchedAlmond, 1);
        public static readonly Pen Blue = new Pen(Color.Blue, 1);
        public static readonly Pen BlueViolet = new Pen(Color.BlueViolet, 1);
        public static readonly Pen Brown = new Pen(Color.Brown, 1);
        public static readonly Pen BurlyWood = new Pen(Color.BurlyWood, 1);
        public static readonly Pen CadetBlue = new Pen(Color.CadetBlue, 1);
        public static readonly Pen Chartreuse = new Pen(Color.Chartreuse, 1);
        public static readonly Pen Chocolate = new Pen(Color.Chocolate, 1);
        public static readonly Pen Coral = new Pen(Color.Coral, 1);
        public static readonly Pen CornflowerBlue = new Pen(Color.CornflowerBlue, 1);
        public static readonly Pen Cornsilk = new Pen(Color.Cornsilk, 1);
        public static readonly Pen Crimson = new Pen(Color.Crimson, 1);
        public static readonly Pen Cyan = new Pen(Color.Cyan, 1);
        public static readonly Pen DarkBlue = new Pen(Color.DarkBlue, 1);
        public static readonly Pen DarkCyan = new Pen(Color.DarkCyan, 1);
        public static readonly Pen DarkGoldenrod = new Pen(Color.DarkGoldenrod, 1);
        public static readonly Pen DarkGray = new Pen(Color.DarkGray, 1);
        public static readonly Pen DarkGreen = new Pen(Color.DarkGreen, 1);
        public static readonly Pen DarkKhaki = new Pen(Color.DarkKhaki, 1);
        public static readonly Pen DarkMagenta = new Pen(Color.DarkMagenta, 1);
        public static readonly Pen DarkOliveGreen = new Pen(Color.DarkOliveGreen, 1);
        public static readonly Pen DarkOrange = new Pen(Color.DarkOrange, 1);
        public static readonly Pen DarkOrchid = new Pen(Color.DarkOrchid, 1);
        public static readonly Pen DarkRed = new Pen(Color.DarkRed, 1);
        public static readonly Pen DarkSalmon = new Pen(Color.DarkSalmon, 1);
        public static readonly Pen DarkSeaGreen = new Pen(Color.DarkSeaGreen, 1);
        public static readonly Pen DarkSlateBlue = new Pen(Color.DarkSlateBlue, 1);
        public static readonly Pen DarkSlateGray = new Pen(Color.DarkSlateGray, 1);
        public static readonly Pen DarkTurquoise = new Pen(Color.DarkTurquoise, 1);
        public static readonly Pen DarkViolet = new Pen(Color.DarkViolet, 1);
        public static readonly Pen DeepPink = new Pen(Color.DeepPink, 1);
        public static readonly Pen DeepSkyBlue = new Pen(Color.DeepSkyBlue, 1);
        public static readonly Pen DimGray = new Pen(Color.DimGray, 1);
        public static readonly Pen DodgerBlue = new Pen(Color.DodgerBlue, 1);
        public static readonly Pen Firebrick = new Pen(Color.Firebrick, 1);
        public static readonly Pen FloralWhite = new Pen(Color.FloralWhite, 1);
        public static readonly Pen ForestGreen = new Pen(Color.ForestGreen, 1);
        public static readonly Pen Fuchsia = new Pen(Color.Fuchsia, 1);
        public static readonly Pen Gainsboro = new Pen(Color.Gainsboro, 1);
        public static readonly Pen GhostWhite = new Pen(Color.GhostWhite, 1);
        public static readonly Pen Gold = new Pen(Color.Gold, 1);
        public static readonly Pen Goldenrod = new Pen(Color.Goldenrod, 1);
        public static readonly Pen Gray = new Pen(Color.Gray, 1);
        public static readonly Pen Green = new Pen(Color.Green, 1);
        public static readonly Pen GreenYellow = new Pen(Color.GreenYellow, 1);
        public static readonly Pen Honeydew = new Pen(Color.Honeydew, 1);
        public static readonly Pen HotPink = new Pen(Color.HotPink, 1);
        public static readonly Pen IndianRed = new Pen(Color.IndianRed, 1);
        public static readonly Pen Indigo = new Pen(Color.Indigo, 1);
        public static readonly Pen Ivory = new Pen(Color.Ivory, 1);
        public static readonly Pen Khaki = new Pen(Color.Khaki, 1);
        public static readonly Pen Lavender = new Pen(Color.Lavender, 1);
        public static readonly Pen LavenderBlush = new Pen(Color.LavenderBlush, 1);
        public static readonly Pen LawnGreen = new Pen(Color.LawnGreen, 1);
        public static readonly Pen LemonChiffon = new Pen(Color.LemonChiffon, 1);
        public static readonly Pen LightBlue = new Pen(Color.LightBlue, 1);
        public static readonly Pen LightCoral = new Pen(Color.LightCoral, 1);
        public static readonly Pen LightCyan = new Pen(Color.LightCyan, 1);
        public static readonly Pen LightGoldenrodYellow = new Pen(Color.LightGoldenrodYellow, 1);
        public static readonly Pen LightGreen = new Pen(Color.LightGreen, 1);
        public static readonly Pen LightGray = new Pen(Color.LightGray, 1);
        public static readonly Pen LightPink = new Pen(Color.LightPink, 1);
        public static readonly Pen LightSalmon = new Pen(Color.LightSalmon, 1);
        public static readonly Pen LightSeaGreen = new Pen(Color.LightSeaGreen, 1);
        public static readonly Pen LightSkyBlue = new Pen(Color.LightSkyBlue, 1);
        public static readonly Pen LightSlateGray = new Pen(Color.LightSlateGray, 1);
        public static readonly Pen LightSteelBlue = new Pen(Color.LightSteelBlue, 1);
        public static readonly Pen LightYellow = new Pen(Color.LightYellow, 1);
        public static readonly Pen Lime = new Pen(Color.Lime, 1);
        public static readonly Pen LimeGreen = new Pen(Color.LimeGreen, 1);
        public static readonly Pen Linen = new Pen(Color.Linen, 1);
        public static readonly Pen Magenta = new Pen(Color.Magenta, 1);
        public static readonly Pen Maroon = new Pen(Color.Maroon, 1);
        public static readonly Pen MediumAquamarine = new Pen(Color.MediumAquamarine, 1);
        public static readonly Pen MediumBlue = new Pen(Color.MediumBlue, 1);
        public static readonly Pen MediumOrchid = new Pen(Color.MediumOrchid, 1);
        public static readonly Pen MediumPurple = new Pen(Color.MediumPurple, 1);
        public static readonly Pen MediumSeaGreen = new Pen(Color.MediumSeaGreen, 1);
        public static readonly Pen MediumSlateBlue = new Pen(Color.MediumSlateBlue, 1);
        public static readonly Pen MediumSpringGreen = new Pen(Color.MediumSpringGreen, 1);
        public static readonly Pen MediumTurquoise = new Pen(Color.MediumTurquoise, 1);
        public static readonly Pen MediumVioletRed = new Pen(Color.MediumVioletRed, 1);
        public static readonly Pen MidnightBlue = new Pen(Color.MidnightBlue, 1);
        public static readonly Pen MintCream = new Pen(Color.MintCream, 1);
        public static readonly Pen MistyRose = new Pen(Color.MistyRose, 1);
        public static readonly Pen Moccasin = new Pen(Color.Moccasin, 1);
        public static readonly Pen NavajoWhite = new Pen(Color.NavajoWhite, 1);
        public static readonly Pen Navy = new Pen(Color.Navy, 1);
        public static readonly Pen OldLace = new Pen(Color.OldLace, 1);
        public static readonly Pen Olive = new Pen(Color.Olive, 1);
        public static readonly Pen OliveDrab = new Pen(Color.OliveDrab, 1);
        public static readonly Pen Orange = new Pen(Color.Orange, 1);
        public static readonly Pen OrangeRed = new Pen(Color.OrangeRed, 1);
        public static readonly Pen Orchid = new Pen(Color.Orchid, 1);
        public static readonly Pen PaleGoldenrod = new Pen(Color.PaleGoldenrod, 1);
        public static readonly Pen PaleGreen = new Pen(Color.PaleGreen, 1);
        public static readonly Pen PaleTurquoise = new Pen(Color.PaleTurquoise, 1);
        public static readonly Pen PaleVioletRed = new Pen(Color.PaleVioletRed, 1);
        public static readonly Pen PapayaWhip = new Pen(Color.PapayaWhip, 1);
        public static readonly Pen PeachPuff = new Pen(Color.PeachPuff, 1);
        public static readonly Pen Peru = new Pen(Color.Peru, 1);
        public static readonly Pen Pink = new Pen(Color.Pink, 1);
        public static readonly Pen Plum = new Pen(Color.Plum, 1);
        public static readonly Pen PowderBlue = new Pen(Color.PowderBlue, 1);
        public static readonly Pen Purple = new Pen(Color.Purple, 1);
        public static readonly Pen Red = new Pen(Color.Red, 1);
        public static readonly Pen RosyBrown = new Pen(Color.RosyBrown, 1);
        public static readonly Pen RoyalBlue = new Pen(Color.RoyalBlue, 1);
        public static readonly Pen SaddleBrown = new Pen(Color.SaddleBrown, 1);
        public static readonly Pen Salmon = new Pen(Color.Salmon, 1);
        public static readonly Pen SandyBrown = new Pen(Color.SandyBrown, 1);
        public static readonly Pen SeaGreen = new Pen(Color.SeaGreen, 1);
        public static readonly Pen SeaShell = new Pen(Color.SeaShell, 1);
        public static readonly Pen Sienna = new Pen(Color.Sienna, 1);
        public static readonly Pen Silver = new Pen(Color.Silver, 1);
        public static readonly Pen SkyBlue = new Pen(Color.SkyBlue, 1);
        public static readonly Pen SlateBlue = new Pen(Color.SlateBlue, 1);
        public static readonly Pen SlateGray = new Pen(Color.SlateGray, 1);
        public static readonly Pen Snow = new Pen(Color.Snow, 1);
        public static readonly Pen SpringGreen = new Pen(Color.SpringGreen, 1);
        public static readonly Pen SteelBlue = new Pen(Color.SteelBlue, 1);
        public static readonly Pen Tan = new Pen(Color.Tan, 1);
        public static readonly Pen Teal = new Pen(Color.Teal, 1);
        public static readonly Pen Thistle = new Pen(Color.Thistle, 1);
        public static readonly Pen Tomato = new Pen(Color.Tomato, 1);
        public static readonly Pen Turquoise = new Pen(Color.Turquoise, 1);
        public static readonly Pen Violet = new Pen(Color.Violet, 1);
        public static readonly Pen Wheat = new Pen(Color.Wheat, 1);
        public static readonly Pen White = new Pen(Color.White, 1);
        public static readonly Pen WhiteSmoke = new Pen(Color.WhiteSmoke, 1);
        public static readonly Pen Yellow = new Pen(Color.Yellow, 1);
        public static readonly Pen YellowGreen = new Pen(Color.YellowGreen, 1);

    }
}
