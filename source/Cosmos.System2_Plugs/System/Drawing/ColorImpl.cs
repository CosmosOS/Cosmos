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
                case "":
                    throw new ArgumentException("Color Name must be passed to FromName");
                    break;
                default:
                    return Color.Black;
                break;
            }
        }
    }
}
