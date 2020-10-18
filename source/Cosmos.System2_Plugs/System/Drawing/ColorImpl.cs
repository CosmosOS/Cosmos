//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Drawing
{
    [Plug(Target = typeof(global::System.Drawing.Color))]
    class ColorImpl
    {
        /// <summary>Implements System.Drawing.Color.FromName
        /// <para>See https://docs.microsoft.com/de-de/dotnet/api/system.drawing.color.fromname?view=netcore-3.1 for Usage Explanation</para>
        /// </summary>
        public static Color FromName(string name)
        {
           switch(name)
            {
                case "AliceBlue":
                    return Color.AliceBlue;
                    break;
                case "AntiqueWhite":
                    return Color.AntiqueWhite;
                    break;
                case "Aqua":
                    return Color.Aqua;
                    break;
                case "Aquamarine":
                    return Color.Aquamarine;
                    break;
                case "Azure":
                    return Color.Azure;
                    break;
                case "Beige":
                    return Color.Beige;
                    break;
                case "Bisque":
                    return Color.Bisque;
                    break;
                case "Black":
                    return Color.Black;
                    break;
                case "BlueViolet":
                    return Color.BlueViolet;
                    break;
                case "Brown":
                    return Color.Brown;
                    break;
                case "BurlyWood":
                    return Color.BurlyWood;
                    break;
                case "CadetBlue":
                    return Color.CadetBlue;
                    break;
                case "Chartreuse":
                    return Color.Chartreuse;
                    break;
                case "Chocolate":
                    return Color.Chocolate;
                    break;
                case "Coral":
                    return Color.Coral;
                    break;
                case "CornflowerBlue":
                    return Color.CornflowerBlue;
                    break;
                case "Cornsilk":
                    return Color.Cornsilk;
                    break;
                case "Crimson":
                    return Color.Crimson;
                    break;
                case "Cyan":
                    return Color.Cyan;
                    break;
                case "DarkBlue":
                    return Color.DarkBlue;
                    break;
                case "DarkCyan":
                    return Color.DarkCyan;
                    break;
                case "DarkGoldenrod":
                    return Color.DarkGoldenrod;
                    break;
                case "DarkGray":
                    return Color.DarkGray;
                    break;
                case "DarkGreen":
                    return Color.DarkGreen;
                    break;
                case "DarkKhaki":
                    return Color.DarkKhaki;
                    break;
                case "DarkMagenta":
                    return Color.DarkMagenta;
                    break;
                case "DarkOliveGreen":
                    return Color.DarkOliveGreen;
                    break;
                case "AliceOrange":
                    return Color.DarkOrange;
                    break;
                case "DarkOrchid":
                    return Color.DarkOrchid;
                    break;
                case "DarkRed":
                    return Color.DarkRed;
                    break;
                case "DarkSalmon":
                    return Color.DarkSalmon;
                    break;
                case "DarkSeaGreen":
                    return Color.DarkSeaGreen;
                    break;
                case "DarkSlateBlue":
                    return Color.DarkSlateBlue;
                    break;
                case "DarkSlateGray":
                    return Color.DarkSlateGray;
                    break;
                case "DarkTurquoise":
                    return Color.DarkTurquoise;
                    break;
                case "DarkViolet":
                    return Color.DarkViolet;
                    break;
                case "DeepPink":
                    return Color.DeepPink;
                    break;
                case "DeepSkyBlue":
                    return Color.DeepSkyBlue;
                    break;
                case "DimGray":
                    return Color.DimGray;
                    break;
                case "DodgerBlue":
                    return Color.DodgerBlue;
                    break;
                case "Firebrick":
                    return Color.Firebrick;
                    break;
                case "FloralWhite":
                    return Color.FloralWhite;
                    break;
                case "ForestGreen":
                    return Color.ForestGreen;
                    break;
                case "Fuchsia":
                    return Color.Fuchsia;
                    break;
                case "Gainsboro":
                    return Color.Gainsboro;
                    break;
                case "GhostWhite":
                    return Color.GhostWhite;
                    break;
                case "Gold":
                    return Color.Gold;
                    break;
                case "Goldenrod":
                    return Color.Goldenrod;
                    break;
                case "Gray":
                    return Color.Gray;
                    break;
                case "Green":
                    return Color.Green;
                    break;
                case "GreenYellow":
                    return Color.GreenYellow;
                    break;
                case "Honeydew":
                    return Color.Honeydew;
                    break;
                case "HotPink":
                    return Color.HotPink;
                    break;
                case "IndianRed":
                    return Color.IndianRed;
                    break;
                case "Indigo":
                    return Color.Indigo;
                    break;
                case "Ivory":
                    return Color.Ivory;
                    break;
                case "Khaki":
                    return Color.Khaki;
                    break;
                case "Lavender":
                    return Color.Lavender;
                    break;
                case "LavenderBlush":
                    return Color.LavenderBlush;
                    break;
                case "LawnGreen":
                    return Color.LawnGreen;
                    break;
                case "LemonChiffon":
                    return Color.LemonChiffon;
                    break;
                case "LightBlue":
                    return Color.LightBlue;
                    break;
                case "LightCoral":
                    return Color.LightCoral;
                    break;
                case "LightCyan":
                    return Color.LightCyan;
                    break;
                case "LightGoldenrodYellow":
                    return Color.LightGoldenrodYellow;
                    break;
                case "LightGreen":
                    return Color.LightGreen;
                    break;
                case "LightGray":
                    return Color.LightGray;
                    break;
                case "LightPink":
                    return Color.LightPink;
                    break;
                case "LightSalmon":
                    return Color.LightSalmon;
                    break;
                case "LightSeaGreen":
                    return Color.LightSeaGreen;
                    break;
                case "LightSkyBlue":
                    return Color.LightSkyBlue;
                    break;
                case "LightSlateGray":
                    return Color.LightSlateGray;
                    break;
                case "LightSteelBlue":
                    return Color.LightSteelBlue;
                    break;
                case "LightYellow":
                    return Color.LightYellow;
                    break;
                case "Lime":
                    return Color.Lime;
                    break;
                case "LimeGreen":
                    return Color.LimeGreen;
                    break;
                case "Linen":
                    return Color.Linen;
                    break;
                case "Magenta":
                    return Color.Magenta;
                    break;
                case "Maroon":
                    return Color.Maroon;
                    break;
                case "MediumAquamarine":
                    return Color.MediumAquamarine ;
                    break;
                case "MediumBlue":
                    return Color.MediumBlue;
                    break;
                case "MediumOrchid":
                    return Color.MediumOrchid;
                    break;
                case "MediumPurple":
                    return Color.MediumPurple;
                    break;
                case "MediumSeaGreen":
                    return Color.MediumSeaGreen;
                    break;
                case "MediumSlateBlue":
                    return Color.MediumSlateBlue;
                    break;
                case "MediumSpringGreen":
                    return Color.MediumSpringGreen;
                    break;
                case "MediumTurquoise":
                    return Color.MediumTurquoise;
                    break;
                case "MediumVioletRed":
                    return Color.MediumVioletRed;
                    break;
                case "MidnightBlue":
                    return Color.MidnightBlue;
                    break;
                case "MintCream":
                    return Color.MintCream;
                    break;
                case "MistyRose":
                    return Color.MistyRose;
                    break;
                case "Moccasin":
                    return Color.Moccasin;
                    break;
                case "NavajoWhite":
                    return Color.NavajoWhite;
                    break;
                case "Navy":
                    return Color.Navy;
                    break;
                case "OldLace":
                    return Color.OldLace;
                    break;
                case "Olive":
                    return Color.Olive;
                    break;
                case "OliveDrab":
                    return Color.OliveDrab;
                    break;
                case "Orange":
                    return Color.Orange;
                    break;
                case "OrangeRed":
                    return Color.OrangeRed;
                    break;
                case "Orchid":
                    return Color.Orchid;
                    break;
                case "PaleGoldenrod":
                    return Color.PaleGoldenrod;
                    break;
                case "PaleGreen":
                    return Color.PaleGreen;
                    break;
                case "PaleTurquoise":
                    return Color.PaleTurquoise;
                    break;
                case "PaleVioletRed":
                    return Color.PaleVioletRed;
                    break;
                case "PapayaWhip":
                    return Color.PapayaWhip;
                    break;
                case "PeachPuff":
                    return Color.PeachPuff;
                    break;
                case "Peru":
                    return Color.Peru;
                    break;
                case "Pink":
                    return Color.Pink;
                    break;
                case "Plum":
                    return Color.Plum;
                    break;
                case "PowderBlue":
                    return Color.PowderBlue;
                    break;
                case "Purple":
                    return Color.Purple;
                    break;
                case "Red":
                    return Color.Red;
                    break;
                case "RosyBrown":
                    return Color.RosyBrown;
                    break;
                case "RoyalBlue":
                    return Color.RoyalBlue;
                    break;
                case "SaddleBrown":
                    return Color.SaddleBrown;
                    break;
                case "Salmon":
                    return Color.Salmon;
                    break;
                case "SandyBrown":
                    return Color.SandyBrown;
                    break;
                case "SeaGreen":
                    return Color.SeaGreen;
                    break;
                case "Sienna":
                    return Color.Sienna;
                    break;
                case "Silver":
                    return Color.Silver;
                    break;
                case "SkyBlue":
                    return Color.SkyBlue;
                    break;
                case "SlateBlue":
                    return Color.SlateBlue;
                    break;
                case "SlateGray":
                    return Color.SlateGray;
                    break;
                case "Snow":
                    return Color.Snow;
                    break;
                case "SpringGreen":
                    return Color.SpringGreen;
                    break;
                case "SteelBlue":
                    return Color.SteelBlue;
                    break;
                case "Tan":
                    return Color.Tan;
                    break;
                case "Thistle":
                    return Color.Thistle;
                    break;
                case "Tomato":
                    return Color.Tomato;
                    break;
                case "Transparent":
                    return Color.Transparent;
                    break;
                case "Turquoise":
                    return Color.Turquoise;
                    break;
                case "Violet":
                    return Color.Violet;
                    break;
                case "Wheat":
                    return Color.Wheat;
                    break;
                case "White":
                    return Color.White;
                    break;
                case "WhiteSmoke":
                    return Color.WhiteSmoke;
                    break;
                case "Yellow":
                    return Color.Yellow;
                    break;
                case "YellowGreen":
                    return Color.YellowGreen;
                    break;
                case "":
                    throw new ArgumentException("Color Name must be passed to FromName");
                    break;
                default:
                    throw new ArgumentException("{0} is not a valid Color Name", name);
                break;
            }
        }
    }
}
