using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFParser
    {
        #region Dep classes
        public class Dimension
        {
            public int width, height;
            public void setSize(int w, int h)
            {
                width = w;
                height = h;
            }
        }

        public class Rectangle
        {
            public int x, y, width, height;

            public Rectangle() { }
            public Rectangle(Rectangle rect)
            {
                setBounds(rect.x, rect.y, rect.width, rect.height);
            }

            public void setBounds(int x, int y, int w, int h)
            {
                this.x = x;
                this.y = y;
                this.width = w;
                this.height = h;
            }
        }

        public class Version
        {
            private int major;
            private int minor;

            public Version()
            {
            }

            public void setVersion(int major, int minor)
            {
                this.major = major;
                this.minor = minor;
            }

            public int getMajor()
            {
                return major;
            }

            public int getMinor()
            {
                return minor;
            }

            public String toString()
            {
                return major + "." + minor;
            }
        }
        #endregion

        StreamReader inStream;

        #region Actual Implementation
        public BDFFontContainer CreateFont()
        {
            return LoadFont();
        }

        public BDFFontContainer LoadFont()
        {
            BDFFontContainer font = null;
            int[] version = null;
            List<String> comments = new List<string>();
            String[] fontName = null;
            int[] size = null;
            int[] bBox = null;
            int contentVersion = 0;
            int metrics = 0;
            List<String> properties = new List<string>();
            List<BDFGlyph> chars = new List<BDFGlyph>();
            FontStyle style = FontStyle.Normal;
            string s = "";

            s = inStream.ReadLine();
            if (s.Substring(0, 9).ToLower() == "startfont")
            {
                version = new int[2];
                version[0] = int.Parse(s.Substring(10, 1));
                version[1] = int.Parse(s.Substring(12, 1));
            }
            else
            {
                throw new Exception("Unable to find start of the font");
            }
            while (!inStream.EndOfStream)
            {
                System.GC.Collect();
                s = inStream.ReadLine();
                if (s.Length > 4)
                {
                    if (s.Substring(0, 4).ToLower() == "size")
                    {
                        string[] s2 = s.Substring(5).Split(new char[] { ' ' });
                        size = new int[4];
                        if (s2.Length == 3)
                        {
                            size[3] = 1;
                        }
                        else
                        {
                            size[3] = int.Parse(s2[3]);
                        }
                        size[0] = int.Parse(s2[0]);
                        size[1] = int.Parse(s2[1]);
                        size[2] = int.Parse(s2[2]);
                        continue;
                    }
                    else if (s.Substring(0, 4).ToLower() == "font")
                    {
                        if (s.Substring(4, 1) == " ") // font command
                        {
                            fontName = new string[14];
                            String[] split = s.Trim().Split(new char[] { '-' });
                            Array.Copy(split, 1, fontName, 0, split.Length - 2);
                            continue;
                        }
                        else if (s.Length > 15)
                        {
                            if (s.Substring(0, 15).ToLower() == "fontboundingbox")
                            {
                                string[] s2 = s.Substring(16).Split(new char[] { ' ' });
                                bBox = new int[4];
                                if (s2.Length != 4)
                                {
                                    throw new Exception("Unknown formatting!");
                                }
                                bBox[0] = int.Parse(s2[0]);
                                bBox[1] = int.Parse(s2[1]);
                                bBox[2] = int.Parse(s2[2]);
                                bBox[3] = int.Parse(s2[3]);
                                continue;
                            }
                        }
                    }
                    else if (s.Length > 5)
                    {
                        if (s.Substring(0, 5).ToLower() == "chars")
                        {
                            int chrs = int.Parse(s.Substring(6));
                            StringBuilder sb;

                            for (uint i = 0; i < chrs; i++)
                            {
                                System.GC.Collect();
                                s = inStream.ReadLine();
                                BDFGlyph g = null;
                                
                                if (s.Length < 9 || s.Substring(0, 9).ToLower() != "startchar")
                                {
                                    throw new Exception("Expected StartChar, but didn't get it");
                                }
                                g = new BDFGlyph(s.Substring(10));
                                while (true)
                                {
                                    s = inStream.ReadLine();
                                    if (s.Length > 3)
                                    {
                                        if (s.Substring(0, 3).ToLower() == "bbx")
                                        {
                                            string[] strs = s.Substring(4).Split(new char[] { ' ' });
                                            if (strs.Length != 4)
                                            {
                                                throw new Exception("Error when loading bbx!");
                                            }
                                            g.setBBX(int.Parse(strs[2]), int.Parse(strs[3]), int.Parse(strs[0]), int.Parse(strs[1]));
                                            continue;
                                        }
                                        else if (s.Length >= 6)
                                        {
                                            if (s.Substring(0, 6).ToLower() == "swidth")
                                            {
                                                string[] s2 = s.Substring(7).Split(new char[] { ' ' });
                                                if (s2.Length != 2)
                                                {
                                                    throw new Exception("Error when loading swidth!");
                                                }
                                                g.setSWidth(int.Parse(s2[0]), int.Parse(s2[1]));
                                                continue;
                                            }
                                            else if (s.Substring(0, 6).ToLower() == "dwidth")
                                            {
                                                string[] s2 = s.Substring(7).Split(new char[] { ' ' });
                                                if (s2.Length != 2)
                                                {
                                                    throw new Exception("Error when loading dwidth!");
                                                }
                                                g.setDWidth(int.Parse(s2[0]), int.Parse(s2[1]));
                                                continue;
                                            }
                                            else if (s.Substring(0, 6).ToLower() == "bitmap")
                                            {
                                                s = inStream.ReadLine();
                                                sb = new StringBuilder(200);
                                                if (s.Length == 2)
                                                {
                                                    while (s.Length == 2)
                                                    {
                                                        sb.Append(s);
                                                        s = inStream.ReadLine();
                                                    }
                                                }
                                                else if (s.Length == 4)
                                                {
                                                    while (s.Length == 4)
                                                    {
                                                        sb.Append(s);
                                                        s = inStream.ReadLine();
                                                    }
                                                }
                                                else
                                                {
                                                    throw new Exception("Unknown depth");
                                                }

                                                g.setRawData(sb.ToString());
                                                if (s.ToLower() != "endchar")
                                                {
                                                    throw new Exception("Unknown end to the bitmap");
                                                }
                                                break;
                                            }
                                            else if (s.Length > 7)
                                            {
                                                if (s.Substring(0, 7).ToLower() == "vvector")
                                                {
                                                    // We don't take this into account.
                                                    continue; 
                                                }
                                                else if (s.Substring(0, 7).ToLower() == "dwidth1")
                                                {
                                                    // We don't take this into account.
                                                    continue; 
                                                }
                                                else if (s.Substring(0, 7).ToLower() == "swidth1")
                                                {
                                                    // We don't take this into account.
                                                    continue; 
                                                }
                                                else if (s.Substring(0, 7).ToLower() == "endchar")
                                                {
                                                    throw new Exception("ERROR: Character didn't include a bitmap!");
                                                }
                                                else if (s.Length > 8)
                                                {
                                                    if (s.Substring(0, 8).ToLower() == "encoding")
                                                    {
                                                        g.encoding = int.Parse(s.Substring(9));
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    throw new Exception("Unknown Property");
                                }
                                chars.Add(g);
                            }

                            if (inStream.ReadLine().ToLower() != "endfont")
                            {
                                throw new Exception("Error when loading chars, missing endfont message.");
                            }
                            break;
                        }
                        else if (s.Length > 7)
                        {
                            if (s.Substring(0, 7).ToLower() == "comment")
                            {
                                comments.Add(s.Substring(9, (s.Length - 10)));
                                continue;
                            }
                            else if (s.Length > 10)
                            {
                                if (s.Substring(0, 10).ToLower() == "metricset")
                                {
                                    metrics = int.Parse(s.Substring(11,1));
                                    continue;
                                }
                                else if (s.Length > 14)
                                {
                                    if (s.Substring(0, 14).ToLower() == "contentversion")
                                    {
                                        contentVersion = int.Parse(s.Substring(15));
                                        continue;
                                    }
                                    else if (s.Length > 15)
                                    {
                                        if (s.Substring(0, 15).ToLower() == "startproperties")
                                        {
                                            int props = int.Parse(s.Substring(16));
                                            for (uint i = 0; i < props; i++)
                                            {
                                                properties.Add(inStream.ReadLine());
                                            }
                                            s = inStream.ReadLine();
                                            if (s.ToLower() != "endproperties")
                                            {
                                                throw new Exception("An error occured while loading properties!");
                                            }
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                throw new Exception("Unknown command");
            }

            if ("i".Equals(fontName[BDFFontContainer.SLANT], StringComparison.InvariantCultureIgnoreCase) ||
                "o".Equals(fontName[BDFFontContainer.SLANT], StringComparison.InvariantCultureIgnoreCase))
            {
                style = FontStyle.Italic;
            }
            if ("bold".Equals(fontName[BDFFontContainer.WEIGHT], StringComparison.InvariantCultureIgnoreCase))
                style |= FontStyle.Bold;

            font = new BDFFontContainer(fontName, style, size[0]);
            font.setBoundingBox(bBox[0], bBox[1], bBox[2], bBox[3]);
            font.setResolution(size[1], size[2]);
            font.setComments(comments.ToArray());
            font.setProperties(properties.ToArray());

            if (size != null && size.Length == 4)
                font.setDepth(size[3]);

            font.setGlyphs(chars.ToArray());
            return font;
        }
        #endregion


        public BDFParser(System.IO.Stream stream)
        {
            inStream = new StreamReader(stream, System.Text.ASCIIEncoding.ASCII);
        }
    }
}
