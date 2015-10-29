using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class XpmImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            XpmInternals.Save(i, dest);
        }

        public override Image Load(Stream s)
        {
            return XpmInternals.Load(s);
        }


        // Please note, everything below this
        // point was originally from a plugin 
        // for Paint.Net, the plugin is available here:
        // http://forums.getpaint.net/index.php?/topic/14512-xpm-file-type-plugin/
        //
        //
        // The source has been modified for use in this library.
        // 
        // This disclaimer was last
        // modified on August 9, 2011.


        #region Internals
        private static class XpmInternals
        {

            #region Support Methods
            private static int ColorHash(Pixel colour)
            {
                int num = 0;
                if (colour.A > 0)
                {
                    num = (((((0xff - colour.R) * 0x100) * 0x100) + ((0xff - colour.G) * 0x100)) + (0xff - colour.B)) + 1;
                }
                return num;
            }

            private static void IncrementChars(byte[] ascii)
            {
                ascii[0] = (byte)(ascii[0] + 1);
                for (int i = 0; i < ascii.Length; i++)
                {
                    if (ascii[i] > 0x7e)
                    {
                        ascii[i + 1] = (byte)(ascii[i + 1] + 1);
                        ascii[i] = 0x20;
                    }
                }
                for (int j = 0; j < ascii.Length; j++)
                {
                    if ((ascii[j] == 0x22) || (ascii[j] == 0x5c))
                    {
                        ascii[j] = (byte)(ascii[j] + 1);
                    }
                }
            }

            private static bool ReadLine(Stream input, out string line)
            {
                byte[] bytes = new byte[512];
                for (int i = 0; i < 512; i++)
                {
                    int num2 = input.ReadByte();
                    switch (num2)
                    {
                        case -1:
                            line = "";
                            return true;

                        case 10:
                        case 13:
                            line = Encoding.ASCII.GetString(bytes);
                            return false;
                    }
                    bytes[i] = (byte)num2;
                }
                line = Encoding.ASCII.GetString(bytes);
                return false;
            }

            private static void WriteASCII(string text, Stream output)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                output.Write(bytes, 0, bytes.Length);
                output.Flush();
            }
            #endregion

            #region Load
            public static Image Load(Stream input)
            {
                string str;
                if (ReadLine(input, out str) || (str.Trim().Substring(0, 6).CompareTo("/* XPM") != 0))
                {
                    throw new Exception("No valid XPM header found");
                }
                do
                {
                    if (ReadLine(input, out str))
                    {
                        throw new Exception("Cannot find the values section of the file");
                    }
                }
                while (str.Trim().Substring(0, 1).CompareTo("\"") != 0);
                char[] separator = new char[] { ' ', '\t', '"' };
                string[] strArray = str.Split(separator);
                if (strArray.Length < 6)
                {
                    throw new Exception("Invalid values section of the file");
                }
                int width = Convert.ToInt32(strArray[1]);
                int height = Convert.ToInt32(strArray[2]);
                int num3 = Convert.ToInt32(strArray[3]);
                int length = Convert.ToInt32(strArray[4]);
                Image image = new Image(width, height);
                do
                {
                    if (ReadLine(input, out str))
                    {
                        throw new Exception("Cannot find the color section of the file");
                    }
                }
                while (str.Trim().Substring(0, 1).CompareTo("\"") != 0);
                Dictionary<string, Pixel> dictionary = new Dictionary<string, Pixel>();
                for (int i = 0; i < num3; i++)
                {
                    string str2 = str.Trim();
                    string key = str2.Substring(1, length);
                    strArray = str2.Substring(length + 1).Split(separator);
                    if (strArray.Length < 4)
                    {
                        throw new Exception("Invalid color entry");
                    }
                    if (strArray[1].CompareTo("c") != 0)
                    {
                        throw new Exception("Non color type found, unhandled");
                    }
                    if (strArray[2].CompareTo("None") == 0)
                    {
                        Pixel color = new Pixel(true);
                        dictionary.Add(key, color);
                    }
                    else
                    {
                        if (strArray[2].Substring(0, 1).CompareTo("#") != 0)
                        {
                            throw new Exception("Non RGB color type found, unhandled");
                        }
                        byte red = (byte)Convert.ToInt32(strArray[2].Substring(1, 2), 0x10);
                        byte green = (byte)Convert.ToInt32(strArray[2].Substring(3, 2), 0x10);
                        byte blue = (byte)Convert.ToInt32(strArray[2].Substring(5, 2), 0x10);
                        Pixel color2 = new Pixel(red, green, blue, 255);
                        dictionary.Add(key, color2);
                    }
                    if (ReadLine(input, out str))
                    {
                        throw new Exception("Corrupt color section in the file");
                    }
                }
                do
                {
                    if (ReadLine(input, out str))
                    {
                        throw new Exception("Cannot find the pixel section of the file");
                    }
                }
                while (str.Trim().Substring(0, 1).CompareTo("\"") != 0);
                for (int j = 0; j < height; j++)
                {
                    string str4 = str.Trim();
                    if (str4.Substring(1).IndexOf('"') != (width * length))
                    {
                        throw new Exception("Corrupt pixel entry in the file");
                    }
                    int startIndex = 1;
                    int x = 0;
                    while (x < width)
                    {
                        Pixel color3;
                        string str5 = str4.Substring(startIndex, length);
                        if (!dictionary.TryGetValue(str5, out color3))
                        {
                            throw new Exception("Unknown pixel value - weird");
                        }
                        image.SetPixel((uint)x, (uint)j, color3);
                        x++;
                        startIndex += length;
                    }
                    if (ReadLine(input, out str))
                    {
                        throw new Exception("Corrupt pixel section in the file");
                    }
                }
                return image;
            }
            #endregion

            #region Save
            public static void Save(Image img, Stream dest)
            {
                WriteASCII("/* XPM */\nstatic char * pixmap[] = {\n", dest);
                int height = img.Height;
                int width = img.Width;
                SortedList<int, Pixel> list = new SortedList<int, Pixel>();
                int num3 = 0;
                for (int i = 0; i < width; i++)
                {
                    for (int m = 0; m < width; m++)
                    {
                        Pixel pixel = img.GetPixel((uint)i, (uint)m);
                        int key = ColorHash(pixel);
                        if (!list.ContainsKey(key))
                        {
                            list.Add(key, pixel);
                            num3++;
                        }
                    }
                }
                int num7 = 0;
                int num8 = 0;
                while (num8 < list.Count)
                {
                    num8 += 93;
                    num7++;
                }
                WriteASCII("/* width height num_colors chars_per_pixel */\n", dest);
                WriteASCII("\"" + width.ToString() + " " + height.ToString() + " " + num3.ToString() + " " + num7.ToString() + "\",\n", dest);
                Dictionary<int, string> dictionary = new Dictionary<int, string>();
                byte[] ascii = new byte[num7];
                for (int j = 0; j < num7; j++)
                {
                    ascii[j] = 0x20;
                }
                ascii[0] = 0x1f;
                WriteASCII("/* colors */\n", dest);
                foreach (KeyValuePair<int, Pixel> pair in list)
                {
                    IncrementChars(ascii);
                    string str2 = Encoding.ASCII.GetString(ascii);
                    Pixel color2 = pair.Value;
                    if (color2.A > 0)
                    {
                        WriteASCII("\"" + str2 + " c #" + color2.R.ToString("x").PadLeft(2, '0') + color2.G.ToString("x").PadLeft(2, '0') + color2.B.ToString("x").PadLeft(2, '0') + "\",\n", dest);
                    }
                    else
                    {
                        WriteASCII("\"" + str2 + " c None\",\n", dest);
                    }
                    dictionary.Add(pair.Key, str2);
                }
                WriteASCII("/* pixels */\n", dest);
                for (int k = 0; k < height; k++)
                {
                    string str5 = "\"";
                    for (int n = 0; n < width; n++)
                    {
                        string str6;
                        int num13 = ColorHash(img.GetPixel((uint)n, (uint)k));
                        if (!dictionary.TryGetValue(num13, out str6))
                        {
                            throw new Exception("Colour missing from the stringList, weird");
                        }
                        str5 = str5 + str6;
                    }
                    if (k == (height - 1))
                    {
                        str5 = str5 + "\"\n";
                    }
                    else
                    {
                        str5 = str5 + "\",\n";
                    }
                    WriteASCII(str5, dest);
                }
                WriteASCII("};\n", dest);
            }
            #endregion

        }
        #endregion
    }
}
