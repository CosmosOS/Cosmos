using System;
using IL2CPU.API.Attribs;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;

namespace Cosmos.System_Plugs.System.Drawing
{
    [Plug(Target = typeof(Color))]
    class ColorImpl
    {
        public static void Ctor(ref Color athis, KnownColor knownColor,
            [FieldAccess(Name = "System.String System.Drawing.Color.name")] ref string aName,
            [FieldAccess(Name = "System.Int64 System.Drawing.Color.value")] ref long aValue,
            [FieldAccess(Name = "System.Int16 System.Drawing.Color.knownColor")] ref short aKnownColor,
            [FieldAccess(Name = "System.Int16 System.Drawing.Color.state")] ref short aState)
        {
            switch (knownColor)
            {
                case KnownColor.AliceBlue:
                    aValue = 0xFFF0F8FF;
                    break;
                case KnownColor.AntiqueWhite:
                    aValue = 0xFFFAEBD7;
                    break;
                case KnownColor.Aqua:
                    aValue = 0xFF00FFFF;
                    break;
                case KnownColor.Aquamarine:
                    aValue = 0xFF7FFFD4;
                    break;
                case KnownColor.Azure:
                    aValue = 0xFFF0FFFF;
                    break;
                case KnownColor.Beige:
                    aValue = 0xFFF5F5DC;
                    break;
                case KnownColor.Bisque:
                    aValue = 0xFFFFE4C4;
                    break;
                case KnownColor.Black:
                    aValue = 0xFF000000;
                    break;
                case KnownColor.BlanchedAlmond:
                    aValue = 0xFFFFEBCD;
                    break;
                case KnownColor.Blue:
                    aValue = 0xFF0000FF;
                    break;
                case KnownColor.BlueViolet:
                    aValue = 0xFF8A2BE2;
                    break;
                case KnownColor.Brown:
                    aValue = 0xFFA52A2A;
                    break;
                case KnownColor.BurlyWood:
                    aValue = 0xFFDEB887;
                    break;
                case KnownColor.CadetBlue:
                    aValue = 0xFF5F9EA0;
                    break;
                case KnownColor.Chartreuse:
                    aValue = 0xFF7FFF00;
                    break;
                case KnownColor.Chocolate:
                    aValue = 0xFFD2691E;
                    break;
                case KnownColor.Coral:
                    aValue = 0xFFFF7F50;
                    break;
                case KnownColor.CornflowerBlue:
                    aValue = 0xFF6495ED;
                    break;
                case KnownColor.Cornsilk:
                    aValue = 0xFFFFF8DC;
                    break;
                case KnownColor.Crimson:
                    aValue = 0xFFDC143C;
                    break;
                case KnownColor.Cyan:
                    aValue = 0xFF00FFFF;
                    break;
                case KnownColor.DarkBlue:
                    aValue = 0xFF00008B;
                    break;
                case KnownColor.DarkCyan:
                    aValue = 0xFF008B8B;
                    break;
                case KnownColor.DarkGoldenrod:
                    aValue = 0xFFB8860B;
                    break;
                case KnownColor.DarkGray:
                    aValue = 0xFFA9A9A9;
                    break;
                case KnownColor.DarkGreen:
                    aValue = 0xFF006400;
                    break;
                case KnownColor.DarkKhaki:
                    aValue = 0xFFBDB76B;
                    break;
                case KnownColor.DarkMagenta:
                    aValue = 0xFF8B008B;
                    break;
                case KnownColor.DarkOliveGreen:
                    aValue = 0xFF556B2F;
                    break;
                case KnownColor.DarkOrange:
                    aValue = 0xFFFF8C00;
                    break;
                case KnownColor.DarkOrchid:
                    aValue = 0xFF9932CC;
                    break;
                case KnownColor.DarkRed:
                    aValue = 0xFF8B0000;
                    break;
                case KnownColor.DarkSalmon:
                    aValue = 0xFFE9967A;
                    break;
                case KnownColor.DarkSeaGreen:
                    aValue = 0xFF8FBC8F;
                    break;
                case KnownColor.DarkSlateBlue:
                    aValue = 0xFF483D8B;
                    break;
                case KnownColor.DarkSlateGray:
                    aValue = 0xFF2F4F4F;
                    break;
                case KnownColor.DarkTurquoise:
                    aValue = 0xFF00CED1;
                    break;
                case KnownColor.DarkViolet:
                    aValue = 0xFF9400D3;
                    break;
                case KnownColor.DeepPink:
                    aValue = 0xFFFF1493;
                    break;
                case KnownColor.DeepSkyBlue:
                    aValue = 0xFF00BFFF;
                    break;
                case KnownColor.DimGray:
                    aValue = 0xFF696969;
                    break;
                case KnownColor.DodgerBlue:
                    aValue = 0xFF1E90FF;
                    break;
                case KnownColor.Firebrick:
                    aValue = 0xFFB22222;
                    break;
                case KnownColor.FloralWhite:
                    aValue = 0xFFFFFAF0;
                    break;
                case KnownColor.ForestGreen:
                    aValue = 0xFF228B22;
                    break;
                case KnownColor.Fuchsia:
                    aValue = 0xFFFF00FF;
                    break;
                case KnownColor.Gainsboro:
                    aValue = 0xFFDCDCDC;
                    break;
                case KnownColor.GhostWhite:
                    aValue = 0xFFF8F8FF;
                    break;
                case KnownColor.Gold:
                    aValue = 0xFFFFD700;
                    break;
                case KnownColor.Goldenrod:
                    aValue = 0xFFDAA520;
                    break;
                case KnownColor.Gray:
                    aValue = 0xFF808080;
                    break;
                case KnownColor.Green:
                    aValue = 0xFF008000;
                    break;
                case KnownColor.GreenYellow:
                    aValue = 0xFFADFF2F;
                    break;
                case KnownColor.Honeydew:
                    aValue = 0xFFF0FFF0;
                    break;
                case KnownColor.HotPink:
                    aValue = 0xFFFF69B4;
                    break;
                case KnownColor.IndianRed:
                    aValue = 0xFFCD5C5C;
                    break;
                case KnownColor.Indigo:
                    aValue = 0xFF4B0082;
                    break;
                case KnownColor.Ivory:
                    aValue = 0xFFFFFFF0;
                    break;
                case KnownColor.Khaki:
                    aValue = 0xFFF0E68C;
                    break;
                case KnownColor.Lavender:
                    aValue = 0xFFE6E6FA;
                    break;
                case KnownColor.LavenderBlush:
                    aValue = 0xFFFFF0F5;
                    break;
                case KnownColor.LawnGreen:
                    aValue = 0xFF7CFC00;
                    break;
                case KnownColor.LemonChiffon:
                    aValue = 0xFFFFFACD;
                    break;
                case KnownColor.LightBlue:
                    aValue = 0xFFADD8E6;
                    break;
                case KnownColor.LightCoral:
                    aValue = 0xFFF08080;
                    break;
                case KnownColor.LightCyan:
                    aValue = 0xFFE0FFFF;
                    break;
                case KnownColor.LightGoldenrodYellow:
                    aValue = 0xFFFAFAD2;
                    break;
                case KnownColor.LightGreen:
                    aValue = 0xFF90EE90;
                    break;
                case KnownColor.LightGray:
                    aValue = 0xFFD3D3D3;
                    break;
                case KnownColor.LightPink:
                    aValue = 0xFFFFB6C1;
                    break;
                case KnownColor.LightSalmon:
                    aValue = 0xFFFFA07A;
                    break;
                case KnownColor.LightSeaGreen:
                    aValue = 0xFF20B2AA;
                    break;
                case KnownColor.LightSkyBlue:
                    aValue = 0xFF87CEFA;
                    break;
                case KnownColor.LightSlateGray:
                    aValue = 0xFF778899;
                    break;
                case KnownColor.LightSteelBlue:
                    aValue = 0xFFB0C4DE;
                    break;
                case KnownColor.LightYellow:
                    aValue = 0xFFFFFFE0;
                    break;
                case KnownColor.Lime:
                    aValue = 0xFF00FF00;
                    break;
                case KnownColor.LimeGreen:
                    aValue = 0xFF32CD32;
                    break;
                case KnownColor.Linen:
                    aValue = 0xFFFAF0E6;
                    break;
                case KnownColor.Magenta:
                    aValue = 0xFFFF00FF;
                    break;
                case KnownColor.Maroon:
                    aValue = 0xFF800000;
                    break;
                case KnownColor.MediumAquamarine:
                    aValue = 0xFF66CDAA;
                    break;
                case KnownColor.MediumBlue:
                    aValue = 0xFF0000CD;
                    break;
                case KnownColor.MediumOrchid:
                    aValue = 0xFFBA55D3;
                    break;
                case KnownColor.MediumPurple:
                    aValue = 0xFF9370DB;
                    break;
                case KnownColor.MediumSeaGreen:
                    aValue = 0xFF3CB371;
                    break;
                case KnownColor.MediumSlateBlue:
                    aValue = 0xFF7B68EE;
                    break;
                case KnownColor.MediumSpringGreen:
                    aValue = 0xFF00FA9A;
                    break;
                case KnownColor.MediumTurquoise:
                    aValue = 0xFF48D1CC;
                    break;
                case KnownColor.MediumVioletRed:
                    aValue = 0xFFC71585;
                    break;
                case KnownColor.MidnightBlue:
                    aValue = 0xFF191970;
                    break;
                case KnownColor.MintCream:
                    aValue = 0xFFF5FFFA;
                    break;
                case KnownColor.MistyRose:
                    aValue = 0xFFFFE4E1;
                    break;
                case KnownColor.Moccasin:
                    aValue = 0xFFFFE4B5;
                    break;
                case KnownColor.NavajoWhite:
                    aValue = 0xFFFFDEAD;
                    break;
                case KnownColor.Navy:
                    aValue = 0xFF000080;
                    break;
                case KnownColor.OldLace:
                    aValue = 0xFFFDF5E6;
                    break;
                case KnownColor.Olive:
                    aValue = 0xFF808000;
                    break;
                case KnownColor.OliveDrab:
                    aValue = 0xFF6B8E23;
                    break;
                case KnownColor.Orange:
                    aValue = 0xFFFFA500;
                    break;
                case KnownColor.OrangeRed:
                    aValue = 0xFFFF4500;
                    break;
                case KnownColor.Orchid:
                    aValue = 0xFFDA70D6;
                    break;
                case KnownColor.PaleGoldenrod:
                    aValue = 0xFFEEE8AA;
                    break;
                case KnownColor.PaleGreen:
                    aValue = 0xFF98FB98;
                    break;
                case KnownColor.PaleTurquoise:
                    aValue = 0xFFAFEEEE;
                    break;
                case KnownColor.PaleVioletRed:
                    aValue = 0xFFDB7093;
                    break;
                case KnownColor.PapayaWhip:
                    aValue = 0xFFFFEFD5;
                    break;
                case KnownColor.PeachPuff:
                    aValue = 0xFFFFDAB9;
                    break;
                case KnownColor.Peru:
                    aValue = 0xFFCD853F;
                    break;
                case KnownColor.Pink:
                    aValue = 0xFFFFC0CB;
                    break;
                case KnownColor.Plum:
                    aValue = 0xFFDDA0DD;
                    break;
                case KnownColor.PowderBlue:
                    aValue = 0xFFB0E0E6;
                    break;
                case KnownColor.Purple:
                    aValue = 0xFF800080;
                    break;
                case KnownColor.Red:
                    aValue = 0xFFFF0000;
                    break;
                case KnownColor.RosyBrown:
                    aValue = 0xFFBC8F8F;
                    break;
                case KnownColor.RoyalBlue:
                    aValue = 0xFF4169E1;
                    break;
                case KnownColor.SaddleBrown:
                    aValue = 0xFF8B4513;
                    break;
                case KnownColor.Salmon:
                    aValue = 0xFFFA8072;
                    break;
                case KnownColor.SandyBrown:
                    aValue = 0xFFF4A460;
                    break;
                case KnownColor.SeaGreen:
                    aValue = 0xFF2E8B57;
                    break;
                case KnownColor.Sienna:
                    aValue = 0xFFA0522D;
                    break;
                case KnownColor.Silver:
                    aValue = 0xFFC0C0C0;
                    break;
                case KnownColor.SkyBlue:
                    aValue = 0xFF87CEEB;
                    break;
                case KnownColor.SlateBlue:
                    aValue = 0xFF6A5ACD;
                    break;
                case KnownColor.SlateGray:
                    aValue = 0xFF708090;
                    break;
                case KnownColor.Snow:
                    aValue = 0xFFFFFAFA;
                    break;
                case KnownColor.SpringGreen:
                    aValue = 0xFF00FF7F;
                    break;
                case KnownColor.SteelBlue:
                    aValue = 0xFF4682B4;
                    break;
                case KnownColor.Tan:
                    aValue = 0xFFD2B48C;
                    break;
                case KnownColor.Teal:
                    aValue = 0xFF008080;
                    break;
                case KnownColor.Thistle:
                    aValue = 0xFFD8BFD8;
                    break;
                case KnownColor.Tomato:
                    aValue = 0xFFFF6347;
                    break;
                case KnownColor.Transparent:
                    aValue = 0x00FFFFFF;
                    break;
                case KnownColor.Turquoise:
                    aValue = 0xFF40E0D0;
                    break;
                case KnownColor.Violet:
                    aValue = 0xFFEE82EE;
                    break;
                case KnownColor.Wheat:
                    aValue = 0xFFF5DEB3;
                    break;
                case KnownColor.White:
                    aValue = 0xFFFFFFFF;
                    break;
                case KnownColor.WhiteSmoke:
                    aValue = 0xFFF5F5F5;
                    break;
                case KnownColor.Yellow:
                    aValue = 0xFFFFFF00;
                    break;
                case KnownColor.YellowGreen:
                    aValue = 0xFF9ACD32;
                    break;
                default:
                    aValue = 0xFF000000;
                    break;
            }

            aState = 1;
            aName = null;
            aKnownColor = (short)knownColor;
        }

        /// <summary>Implements System.Drawing.Color.FromName
        /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color.fromname?view=net-6.0">See also</seealso>
        /// </summary>
        public static Color FromName(string name)
        {
            return name switch
            {
                "AliceBlue" => Color.AliceBlue,
                "AntiqueWhite" => Color.AntiqueWhite,
                "Aqua" => Color.Aqua,
                "Aquamarine" => Color.Aquamarine,
                "Azure" => Color.Azure,
                "Beige" => Color.Beige,
                "Bisque" => Color.Bisque,
                "Black" => Color.Black,
                "BlanchedAlmond" => Color.BlanchedAlmond,
                "Blue" => Color.Blue,
                "BlueViolet" => Color.BlueViolet,
                "Brown" => Color.Brown,
                "BurlyWood" => Color.BurlyWood,
                "CadetBlue" => Color.CadetBlue,
                "Chartreuse" => Color.Chartreuse,
                "Chocolate" => Color.Chocolate,
                "Coral" => Color.Coral,
                "CornflowerBlue" => Color.CornflowerBlue,
                "Cornsilk" => Color.Cornsilk,
                "Crimson" => Color.Crimson,
                "Cyan" => Color.Cyan,
                "DarkBlue" => Color.DarkBlue,
                "DarkCyan" => Color.DarkCyan,
                "DarkGoldenrod" => Color.DarkGoldenrod,
                "DarkGray" => Color.DarkGray,
                "DarkGreen" => Color.DarkGreen,
                "DarkKhaki" => Color.DarkKhaki,
                "DarkMagenta" => Color.DarkMagenta,
                "DarkOliveGreen" => Color.DarkOliveGreen,
                "AliceOrange" => Color.DarkOrange,
                "DarkOrchid" => Color.DarkOrchid,
                "DarkRed" => Color.DarkRed,
                "DarkSalmon" => Color.DarkSalmon,
                "DarkSeaGreen" => Color.DarkSeaGreen,
                "DarkSlateBlue" => Color.DarkSlateBlue,
                "DarkSlateGray" => Color.DarkSlateGray,
                "DarkTurquoise" => Color.DarkTurquoise,
                "DarkViolet" => Color.DarkViolet,
                "DeepPink" => Color.DeepPink,
                "DeepSkyBlue" => Color.DeepSkyBlue,
                "DimGray" => Color.DimGray,
                "DodgerBlue" => Color.DodgerBlue,
                "Firebrick" => Color.Firebrick,
                "FloralWhite" => Color.FloralWhite,
                "ForestGreen" => Color.ForestGreen,
                "Fuchsia" => Color.Fuchsia,
                "Gainsboro" => Color.Gainsboro,
                "GhostWhite" => Color.GhostWhite,
                "Gold" => Color.Gold,
                "Goldenrod" => Color.Goldenrod,
                "Gray" => Color.Gray,
                "Green" => Color.Green,
                "GreenYellow" => Color.GreenYellow,
                "Honeydew" => Color.Honeydew,
                "HotPink" => Color.HotPink,
                "IndianRed" => Color.IndianRed,
                "Indigo" => Color.Indigo,
                "Ivory" => Color.Ivory,
                "Khaki" => Color.Khaki,
                "Lavender" => Color.Lavender,
                "LavenderBlush" => Color.LavenderBlush,
                "LawnGreen" => Color.LawnGreen,
                "LemonChiffon" => Color.LemonChiffon,
                "LightBlue" => Color.LightBlue,
                "LightCoral" => Color.LightCoral,
                "LightCyan" => Color.LightCyan,
                "LightGoldenrodYellow" => Color.LightGoldenrodYellow,
                "LightGreen" => Color.LightGreen,
                "LightGray" => Color.LightGray,
                "LightPink" => Color.LightPink,
                "LightSalmon" => Color.LightSalmon,
                "LightSeaGreen" => Color.LightSeaGreen,
                "LightSkyBlue" => Color.LightSkyBlue,
                "LightSlateGray" => Color.LightSlateGray,
                "LightSteelBlue" => Color.LightSteelBlue,
                "LightYellow" => Color.LightYellow,
                "Lime" => Color.Lime,
                "LimeGreen" => Color.LimeGreen,
                "Linen" => Color.Linen,
                "Magenta" => Color.Magenta,
                "Maroon" => Color.Maroon,
                "MediumAquamarine" => Color.MediumAquamarine,
                "MediumBlue" => Color.MediumBlue,
                "MediumOrchid" => Color.MediumOrchid,
                "MediumPurple" => Color.MediumPurple,
                "MediumSeaGreen" => Color.MediumSeaGreen,
                "MediumSlateBlue" => Color.MediumSlateBlue,
                "MediumSpringGreen" => Color.MediumSpringGreen,
                "MediumTurquoise" => Color.MediumTurquoise,
                "MediumVioletRed" => Color.MediumVioletRed,
                "MidnightBlue" => Color.MidnightBlue,
                "MintCream" => Color.MintCream,
                "MistyRose" => Color.MistyRose,
                "Moccasin" => Color.Moccasin,
                "NavajoWhite" => Color.NavajoWhite,
                "Navy" => Color.Navy,
                "OldLace" => Color.OldLace,
                "Olive" => Color.Olive,
                "OliveDrab" => Color.OliveDrab,
                "Orange" => Color.Orange,
                "OrangeRed" => Color.OrangeRed,
                "Orchid" => Color.Orchid,
                "PaleGoldenrod" => Color.PaleGoldenrod,
                "PaleGreen" => Color.PaleGreen,
                "PaleTurquoise" => Color.PaleTurquoise,
                "PaleVioletRed" => Color.PaleVioletRed,
                "PapayaWhip" => Color.PapayaWhip,
                "PeachPuff" => Color.PeachPuff,
                "Peru" => Color.Peru,
                "Pink" => Color.Pink,
                "Plum" => Color.Plum,
                "PowderBlue" => Color.PowderBlue,
                "Purple" => Color.Purple,
                "Red" => Color.Red,
                "RosyBrown" => Color.RosyBrown,
                "RoyalBlue" => Color.RoyalBlue,
                "SaddleBrown" => Color.SaddleBrown,
                "Salmon" => Color.Salmon,
                "SandyBrown" => Color.SandyBrown,
                "SeaGreen" => Color.SeaGreen,
                "Sienna" => Color.Sienna,
                "Silver" => Color.Silver,
                "SkyBlue" => Color.SkyBlue,
                "SlateBlue" => Color.SlateBlue,
                "SlateGray" => Color.SlateGray,
                "Snow" => Color.Snow,
                "SpringGreen" => Color.SpringGreen,
                "SteelBlue" => Color.SteelBlue,
                "Tan" => Color.Tan,
                "Teal" => Color.Teal,
                "Thistle" => Color.Thistle,
                "Tomato" => Color.Tomato,
                "Transparent" => Color.Transparent,
                "Turquoise" => Color.Turquoise,
                "Violet" => Color.Violet,
                "Wheat" => Color.Wheat,
                "White" => Color.White,
                "WhiteSmoke" => Color.WhiteSmoke,
                "Yellow" => Color.Yellow,
                "YellowGreen" => Color.YellowGreen,
                "" => throw new ArgumentException("Color Name must be passed to the 'System.Drawing.Color.FromName' method."),
                _ => throw new ArgumentException("{0} is not a valid color Name", name),
            };
        }

        public static long get_Value(ref Color aThis,
            [FieldAccess(Name = "System.Int64 System.Drawing.Color.value")] ref long aValue)
        {
            return aValue;
        }

        public static int ToArgb(ref Color aThis,
            [FieldAccess(Name = "System.Int64 System.Drawing.Color.value")] ref long aValue)
        {
            return unchecked((int)aValue);
        }

        public static string ToString(ref Color aThis,
            [FieldAccess(Name = "System.Int64 System.Drawing.Color.value")] ref long aValue,
            [FieldAccess(Name = "System.String System.Drawing.Color.name")] ref string aName,
            [FieldAccess(Name = "System.Int16 System.Drawing.Color.knownColor")] ref short aKnownColor)
        {
            if (aName != null)
            {
                return aName;
            }

            switch (aKnownColor)
            {
                case (short)KnownColor.AliceBlue:
                    return "Color [AliceBlue]";
                case (short)KnownColor.AntiqueWhite:
                    return "Color [AntiqueWhite]";
                case (short)KnownColor.Aqua:
                    return "Color [Aqua]";
                case (short)KnownColor.Aquamarine:
                    return "Color [Aquamarine]";
                case (short)KnownColor.Azure:
                    return "Color [Azure]";
                case (short)KnownColor.Beige:
                    return "Color [Beige]";
                case (short)KnownColor.Bisque:
                    return "Color [Bisque]";
                case (short)KnownColor.Black:
                    return "Color [Black]";
                case (short)KnownColor.BlanchedAlmond:
                    return "Color [BlanchedAlmond]";
                case (short)KnownColor.Blue:
                    return "Color [Blue]";
                case (short)KnownColor.BlueViolet:
                    return "Color [BlueViolet]";
                case (short)KnownColor.Brown:
                    return "Color [Brown]";
                case (short)KnownColor.BurlyWood:
                    return "Color [BurlyWood]";
                case (short)KnownColor.CadetBlue:
                    return "Color [CadetBlue]";
                case (short)KnownColor.Chartreuse:
                    return "Color [Chartreuse]";
                case (short)KnownColor.Chocolate:
                    return "Color [Chocolate]";
                case (short)KnownColor.Coral:
                    return "Color [Coral]";
                case (short)KnownColor.CornflowerBlue:
                    return "Color [CornflowerBlue]";
                case (short)KnownColor.Cornsilk:
                    return "Color [Cornsilk]";
                case (short)KnownColor.Crimson:
                    return "Color [Crimson]";
                case (short)KnownColor.Cyan:
                    return "Color [Cyan]";
                case (short)KnownColor.DarkBlue:
                    return "Color [DarkBlue]";
                case (short)KnownColor.DarkCyan:
                    return "Color [DarkCyan]";
                case (short)KnownColor.DarkGoldenrod:
                    return "Color [DarkGoldenrod]";
                case (short)KnownColor.DarkGray:
                    return "Color [DarkGray]";
                case (short)KnownColor.DarkGreen:
                    return "Color [DarkGreen]";
                case (short)KnownColor.DarkKhaki:
                    return "Color [DarkKhaki]";
                case (short)KnownColor.DarkMagenta:
                    return "Color [DarkMagenta]";
                case (short)KnownColor.DarkOliveGreen:
                    return "Color [DarkOliveGreen]";
                case (short)KnownColor.DarkOrange:
                    return "Color [DarkOrange]";
                case (short)KnownColor.DarkOrchid:
                    return "Color [DarkOrchid]";
                case (short)KnownColor.DarkRed:
                    return "Color [DarkRed]";
                case (short)KnownColor.DarkSalmon:
                    return "Color [DarkSalmon]";
                case (short)KnownColor.DarkSeaGreen:
                    return "Color [DarkSeaGreen]";
                case (short)KnownColor.DarkSlateBlue:
                    return "Color [DarkSlateBlue]";
                case (short)KnownColor.DarkSlateGray:
                    return "Color [DarkSlateGray]";
                case (short)KnownColor.DarkTurquoise:
                    return "Color [DarkTurquoise]";
                case (short)KnownColor.DarkViolet:
                    return "Color [DarkViolet]";
                case (short)KnownColor.DeepPink:
                    return "Color [DeepPink]";
                case (short)KnownColor.DeepSkyBlue:
                    return "Color [DeepSkyBlue]";
                case (short)KnownColor.DimGray:
                    return "Color [DimGray]";
                case (short)KnownColor.DodgerBlue:
                    return "Color [DodgerBlue]";
                case (short)KnownColor.Firebrick:
                    return "Color [Firebrick]";
                case (short)KnownColor.FloralWhite:
                    return "Color [FloralWhite]";
                case (short)KnownColor.ForestGreen:
                    return "Color [ForestGreen]";
                case (short)KnownColor.Fuchsia:
                    return "Color [Fuchsia]";
                case (short)KnownColor.Gainsboro:
                    return "Color [Gainsboro]";
                case (short)KnownColor.GhostWhite:
                    return "Color [GhostWhite]";
                case (short)KnownColor.Gold:
                    return "Color [Gold]";
                case (short)KnownColor.Goldenrod:
                    return "Color [Goldenrod]";
                case (short)KnownColor.Gray:
                    return "Color [Gray]";
                case (short)KnownColor.Green:
                    return "Color [Green]";
                case (short)KnownColor.GreenYellow:
                    return "Color [GreenYellow]";
                case (short)KnownColor.Honeydew:
                    return "Color [Honeydew]";
                case (short)KnownColor.HotPink:
                    return "Color [HotPink]";
                case (short)KnownColor.IndianRed:
                    return "Color [IndianRed]";
                case (short)KnownColor.Indigo:
                    return "Color [Indigo]";
                case (short)KnownColor.Ivory:
                    return "Color [Ivory]";
                case (short)KnownColor.Khaki:
                    return "Color [Khaki]";
                case (short)KnownColor.Lavender:
                    return "Color [Lavender]";
                case (short)KnownColor.LavenderBlush:
                    return "Color [LavenderBlush]";
                case (short)KnownColor.LawnGreen:
                    return "Color [LawnGreen]";
                case (short)KnownColor.LemonChiffon:
                    return "Color [LemonChiffon]";
                case (short)KnownColor.LightBlue:
                    return "Color [LightBlue]";
                case (short)KnownColor.LightCoral:
                    return "Color [LightCoral]";
                case (short)KnownColor.LightCyan:
                    return "Color [LightCyan]";
                case (short)KnownColor.LightGoldenrodYellow:
                    return "Color [LightGoldenrodYellow]";
                case (short)KnownColor.LightGreen:
                    return "Color [LightGreen]";
                case (short)KnownColor.LightGray:
                    return "Color [LightGray]";
                case (short)KnownColor.LightPink:
                    return "Color [LightPink]";
                case (short)KnownColor.LightSalmon:
                    return "Color [LightSalmon]";
                case (short)KnownColor.LightSeaGreen:
                    return "Color [LightSeaGreen]";
                case (short)KnownColor.LightSkyBlue:
                    return "Color [LightSkyBlue]";
                case (short)KnownColor.LightSlateGray:
                    return "Color [LightSlateGray]";
                case (short)KnownColor.LightSteelBlue:
                    return "Color [LightSteelBlue]";
                case (short)KnownColor.LightYellow:
                    return "Color [LightYellow]";
                case (short)KnownColor.Lime:
                    return "Color [Lime]";
                case (short)KnownColor.LimeGreen:
                    return "Color [LimeGreen]";
                case (short)KnownColor.Linen:
                    return "Color [Linen]";
                case (short)KnownColor.Magenta:
                    return "Color [Magenta]";
                case (short)KnownColor.Maroon:
                    return "Color [Maroon]";
                case (short)KnownColor.MediumAquamarine:
                    return "Color [MediumAquamarine]";
                case (short)KnownColor.MediumBlue:
                    return "Color [MediumBlue]";
                case (short)KnownColor.MediumOrchid:
                    return "Color [MediumOrchid]";
                case (short)KnownColor.MediumPurple:
                    return "Color [MediumPurple]";
                case (short)KnownColor.MediumSeaGreen:
                    return "Color [MediumSeaGreen]";
                case (short)KnownColor.MediumSlateBlue:
                    return "Color [MediumSlateBlue]";
                case (short)KnownColor.MediumSpringGreen:
                    return "Color [MediumSpringGreen]";
                case (short)KnownColor.MediumTurquoise:
                    return "Color [MediumTurquoise]";
                case (short)KnownColor.MediumVioletRed:
                    return "Color [MediumVioletRed]";
                case (short)KnownColor.MidnightBlue:
                    return "Color [MidnightBlue]";
                case (short)KnownColor.MintCream:
                    return "Color [MintCream]";
                case (short)KnownColor.MistyRose:
                    return "Color [MistyRose]";
                case (short)KnownColor.Moccasin:
                    return "Color [Moccasin]";
                case (short)KnownColor.NavajoWhite:
                    return "Color [NavajoWhite]";
                case (short)KnownColor.Navy:
                    return "Color [Navy]";
                case (short)KnownColor.OldLace:
                    return "Color [OldLace]";
                case (short)KnownColor.Olive:
                    return "Color [Olive]";
                case (short)KnownColor.OliveDrab:
                    return "Color [OliveDrab]";
                case (short)KnownColor.Orange:
                    return "Color [Orange]";
                case (short)KnownColor.OrangeRed:
                    return "Color [OrangeRed]";
                case (short)KnownColor.Orchid:
                    return "Color [Orchid]";
                case (short)KnownColor.PaleGoldenrod:
                    return "Color [PaleGoldenrod]";
                case (short)KnownColor.PaleGreen:
                    return "Color [PaleGreen]";
                case (short)KnownColor.PaleTurquoise:
                    return "Color [PaleTurquoise]";
                case (short)KnownColor.PaleVioletRed:
                    return "Color [PaleVioletRed]";
                case (short)KnownColor.PapayaWhip:
                    return "Color [PapayaWhip]";
                case (short)KnownColor.PeachPuff:
                    return "Color [PeachPuff]";
                case (short)KnownColor.Peru:
                    return "Color [Peru]";
                case (short)KnownColor.Pink:
                    return "Color [Pink]";
                case (short)KnownColor.Plum:
                    return "Color [Plum]";
                case (short)KnownColor.PowderBlue:
                    return "Color [PowderBlue]";
                case (short)KnownColor.Purple:
                    return "Color [Purple]";
                case (short)KnownColor.Red:
                    return "Color [Red]";
                case (short)KnownColor.RosyBrown:
                    return "Color [RosyBrown]";
                case (short)KnownColor.RoyalBlue:
                    return "Color [RoyalBlue]";
                case (short)KnownColor.SaddleBrown:
                    return "Color [SaddleBrown]";
                case (short)KnownColor.Salmon:
                    return "Color [Salmon]";
                case (short)KnownColor.SandyBrown:
                    return "Color [SandyBrown]";
                case (short)KnownColor.SeaGreen:
                    return "Color [SeaGreen]";
                case (short)KnownColor.Sienna:
                    return "Color [Sienna]";
                case (short)KnownColor.Silver:
                    return "Color [Silver]";
                case (short)KnownColor.SkyBlue:
                    return "Color [SkyBlue]";
                case (short)KnownColor.SlateBlue:
                    return "Color [SlateBlue]";
                case (short)KnownColor.SlateGray:
                    return "Color [SlateGray]";
                case (short)KnownColor.Snow:
                    return "Color [Snow]";
                case (short)KnownColor.SpringGreen:
                    return "Color [SpringGreen]";
                case (short)KnownColor.SteelBlue:
                    return "Color [SteelBlue]";
                case (short)KnownColor.Tan:
                    return "Color [Tan]";
                case (short)KnownColor.Teal:
                    return "Color [Teal]";
                case (short)KnownColor.Thistle:
                    return "Color [Thistle]";
                case (short)KnownColor.Tomato:
                    return "Color [Tomato]";
                case (short)KnownColor.Transparent:
                    return "Color [Transparent]";
                case (short)KnownColor.Turquoise:
                    return "Color [Turquoise]";
                case (short)KnownColor.Violet:
                    return "Color [Violet]";
                case (short)KnownColor.Wheat:
                    return "Color [Wheat]";
                case (short)KnownColor.White:
                    return "Color [White]";
                case (short)KnownColor.WhiteSmoke:
                    return "Color [WhiteSmoke]";
                case (short)KnownColor.Yellow:
                    return "Color [Yellow]";
                case (short)KnownColor.YellowGreen:
                    return "Color [YellowGreen]";
                default:
                    int alpha = (int)(aValue >> 24) & 0xFF;
                    int red = (int)(aValue >> 16) & 0xFF;
                    int green = (int)(aValue >> 8) & 0xFF;
                    int blue = (int)aValue & 0xFF;
                    return $"Color [A={alpha}, R={red}, G={green}, B={blue}]";
            }
        }

        public static bool op_Equality(Color left, Color right) => left.ToArgb() == right.ToArgb();

        public static bool Equals(ref Color aThis, Color other) => aThis == other;
    }
}
