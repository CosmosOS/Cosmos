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
                    
                case "AntiqueWhite":
                    return Color.AntiqueWhite;
                    
                case "Aqua":
                    return Color.Aqua;
                    
                case "Aquamarine":
                    return Color.Aquamarine;
                    
                case "Azure":
                    return Color.Azure;
                    
                case "Beige":
                    return Color.Beige;
                    
                case "Bisque":
                    return Color.Bisque;
                    
                case "Black":
                    return Color.Black;
                    
                case "BlueViolet":
                    return Color.BlueViolet;
                    
                case "Brown":
                    return Color.Brown;
                    
                case "BurlyWood":
                    return Color.BurlyWood;
                    
                case "CadetBlue":
                    return Color.CadetBlue;
                    
                case "Chartreuse":
                    return Color.Chartreuse;
                    
                case "Chocolate":
                    return Color.Chocolate;
                    
                case "Coral":
                    return Color.Coral;
                    
                case "CornflowerBlue":
                    return Color.CornflowerBlue;
                    
                case "Cornsilk":
                    return Color.Cornsilk;
                    
                case "Crimson":
                    return Color.Crimson;
                    
                case "Cyan":
                    return Color.Cyan;
                    
                case "DarkBlue":
                    return Color.DarkBlue;
                    
                case "DarkCyan":
                    return Color.DarkCyan;
                    
                case "DarkGoldenrod":
                    return Color.DarkGoldenrod;
                    
                case "DarkGray":
                    return Color.DarkGray;
                    
                case "DarkGreen":
                    return Color.DarkGreen;
                    
                case "DarkKhaki":
                    return Color.DarkKhaki;
                    
                case "DarkMagenta":
                    return Color.DarkMagenta;
                    
                case "DarkOliveGreen":
                    return Color.DarkOliveGreen;
                    
                case "AliceOrange":
                    return Color.DarkOrange;
                    
                case "DarkOrchid":
                    return Color.DarkOrchid;
                    
                case "DarkRed":
                    return Color.DarkRed;
                    
                case "DarkSalmon":
                    return Color.DarkSalmon;
                    
                case "DarkSeaGreen":
                    return Color.DarkSeaGreen;
                    
                case "DarkSlateBlue":
                    return Color.DarkSlateBlue;
                    
                case "DarkSlateGray":
                    return Color.DarkSlateGray;
                    
                case "DarkTurquoise":
                    return Color.DarkTurquoise;
                    
                case "DarkViolet":
                    return Color.DarkViolet;
                    
                case "DeepPink":
                    return Color.DeepPink;
                    
                case "DeepSkyBlue":
                    return Color.DeepSkyBlue;
                    
                case "DimGray":
                    return Color.DimGray;
                    
                case "DodgerBlue":
                    return Color.DodgerBlue;
                    
                case "Firebrick":
                    return Color.Firebrick;
                    
                case "FloralWhite":
                    return Color.FloralWhite;
                    
                case "ForestGreen":
                    return Color.ForestGreen;
                    
                case "Fuchsia":
                    return Color.Fuchsia;
                    
                case "Gainsboro":
                    return Color.Gainsboro;
                    
                case "GhostWhite":
                    return Color.GhostWhite;
                    
                case "Gold":
                    return Color.Gold;
                    
                case "Goldenrod":
                    return Color.Goldenrod;
                    
                case "Gray":
                    return Color.Gray;
                    
                case "Green":
                    return Color.Green;
                    
                case "GreenYellow":
                    return Color.GreenYellow;
                    
                case "Honeydew":
                    return Color.Honeydew;
                    
                case "HotPink":
                    return Color.HotPink;
                    
                case "IndianRed":
                    return Color.IndianRed;
                    
                case "Indigo":
                    return Color.Indigo;
                    
                case "Ivory":
                    return Color.Ivory;
                    
                case "Khaki":
                    return Color.Khaki;
                    
                case "Lavender":
                    return Color.Lavender;
                    
                case "LavenderBlush":
                    return Color.LavenderBlush;
                    
                case "LawnGreen":
                    return Color.LawnGreen;
                    
                case "LemonChiffon":
                    return Color.LemonChiffon;
                    
                case "LightBlue":
                    return Color.LightBlue;
                    
                case "LightCoral":
                    return Color.LightCoral;
                    
                case "LightCyan":
                    return Color.LightCyan;
                    
                case "LightGoldenrodYellow":
                    return Color.LightGoldenrodYellow;
                    
                case "LightGreen":
                    return Color.LightGreen;
                    
                case "LightGray":
                    return Color.LightGray;
                    
                case "LightPink":
                    return Color.LightPink;
                    
                case "LightSalmon":
                    return Color.LightSalmon;
                    
                case "LightSeaGreen":
                    return Color.LightSeaGreen;
                    
                case "LightSkyBlue":
                    return Color.LightSkyBlue;
                    
                case "LightSlateGray":
                    return Color.LightSlateGray;
                    
                case "LightSteelBlue":
                    return Color.LightSteelBlue;
                    
                case "LightYellow":
                    return Color.LightYellow;
                    
                case "Lime":
                    return Color.Lime;
                    
                case "LimeGreen":
                    return Color.LimeGreen;
                    
                case "Linen":
                    return Color.Linen;
                    
                case "Magenta":
                    return Color.Magenta;
                    
                case "Maroon":
                    return Color.Maroon;
                    
                case "MediumAquamarine":
                    return Color.MediumAquamarine ;
                    
                case "MediumBlue":
                    return Color.MediumBlue;
                    
                case "MediumOrchid":
                    return Color.MediumOrchid;
                    
                case "MediumPurple":
                    return Color.MediumPurple;
                    
                case "MediumSeaGreen":
                    return Color.MediumSeaGreen;
                    
                case "MediumSlateBlue":
                    return Color.MediumSlateBlue;
                    
                case "MediumSpringGreen":
                    return Color.MediumSpringGreen;
                    
                case "MediumTurquoise":
                    return Color.MediumTurquoise;
                    
                case "MediumVioletRed":
                    return Color.MediumVioletRed;
                    
                case "MidnightBlue":
                    return Color.MidnightBlue;
                    
                case "MintCream":
                    return Color.MintCream;
                    
                case "MistyRose":
                    return Color.MistyRose;
                    
                case "Moccasin":
                    return Color.Moccasin;
                    
                case "NavajoWhite":
                    return Color.NavajoWhite;
                    
                case "Navy":
                    return Color.Navy;
                    
                case "OldLace":
                    return Color.OldLace;
                    
                case "Olive":
                    return Color.Olive;
                    
                case "OliveDrab":
                    return Color.OliveDrab;
                    
                case "Orange":
                    return Color.Orange;
                    
                case "OrangeRed":
                    return Color.OrangeRed;
                    
                case "Orchid":
                    return Color.Orchid;
                    
                case "PaleGoldenrod":
                    return Color.PaleGoldenrod;
                    
                case "PaleGreen":
                    return Color.PaleGreen;
                    
                case "PaleTurquoise":
                    return Color.PaleTurquoise;
                    
                case "PaleVioletRed":
                    return Color.PaleVioletRed;
                    
                case "PapayaWhip":
                    return Color.PapayaWhip;
                    
                case "PeachPuff":
                    return Color.PeachPuff;
                    
                case "Peru":
                    return Color.Peru;
                    
                case "Pink":
                    return Color.Pink;
                    
                case "Plum":
                    return Color.Plum;
                    
                case "PowderBlue":
                    return Color.PowderBlue;
                    
                case "Purple":
                    return Color.Purple;
                    
                case "Red":
                    return Color.Red;
                    
                case "RosyBrown":
                    return Color.RosyBrown;
                    
                case "RoyalBlue":
                    return Color.RoyalBlue;
                    
                case "SaddleBrown":
                    return Color.SaddleBrown;
                    
                case "Salmon":
                    return Color.Salmon;
                    
                case "SandyBrown":
                    return Color.SandyBrown;
                    
                case "SeaGreen":
                    return Color.SeaGreen;
                    
                case "Sienna":
                    return Color.Sienna;
                    
                case "Silver":
                    return Color.Silver;
                    
                case "SkyBlue":
                    return Color.SkyBlue;
                    
                case "SlateBlue":
                    return Color.SlateBlue;
                    
                case "SlateGray":
                    return Color.SlateGray;
                    
                case "Snow":
                    return Color.Snow;
                    
                case "SpringGreen":
                    return Color.SpringGreen;
                    
                case "SteelBlue":
                    return Color.SteelBlue;
                    
                case "Tan":
                    return Color.Tan;
                    
                case "Thistle":
                    return Color.Thistle;
                    
                case "Tomato":
                    return Color.Tomato;
                    
                case "Transparent":
                    return Color.Transparent;
                    
                case "Turquoise":
                    return Color.Turquoise;
                    
                case "Violet":
                    return Color.Violet;
                    
                case "Wheat":
                    return Color.Wheat;
                    
                case "White":
                    return Color.White;
                    
                case "WhiteSmoke":
                    return Color.WhiteSmoke;
                    
                case "Yellow":
                    return Color.Yellow;
                    
                case "YellowGreen":
                    return Color.YellowGreen;
                    
                case "":
                    throw new ArgumentException("Color Name must be passed to FromName");
                    
                default:
                    throw new ArgumentException("{0} is not a valid Color Name", name);
                
            }
        }
    }
}
