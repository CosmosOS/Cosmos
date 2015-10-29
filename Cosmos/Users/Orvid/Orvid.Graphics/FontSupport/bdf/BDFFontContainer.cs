using System;
using System.Collections.Generic;
using System.IO;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFFontContainer
    {
        public const int FOUNDRY = 0;
        public const int FAMILY = 1;
        public const int WEIGHT = 2;
        public const int SLANT = 3;
        public const int SWIDTH = 4;
        public const int ADSTYL = 5;
        public const int PIXELSIZE = 6;
        public const int POINTSIZE = 7;
        public const int HORIZONTAL = 8;
        public const int VERTICAL = 9;
        public const int SPACING = 10;
        public const int AVERAGEWIDTH = 11;
        public const int REGISTRY = 12;
        public const int ENCODING = 13;

        private static Dictionary<string, string> charMapper = new Dictionary<string, string>();
        static BDFFontContainer()
        {

            #region Setup charMapper
            charMapper.Add(".undef", "\u0219");
            charMapper.Add(".null", "\u0000");
            charMapper.Add("space", "\u0020");
            charMapper.Add("exclam", "!");
            charMapper.Add("quotedbl", "\"");
            charMapper.Add("numbersign", "#");
            charMapper.Add("dollar", "$");
            charMapper.Add("percent", "%");
            charMapper.Add("ampersand", "&");
            charMapper.Add("quotesingle", "'");
            charMapper.Add("parenleft", "(");
            charMapper.Add("parenright", ")");
            charMapper.Add("asterisk", "*");
            charMapper.Add("plus", "+");
            charMapper.Add("comma", ",");
            charMapper.Add("hyphen", "-");
            charMapper.Add("period", ".");
            charMapper.Add("slash", "/");
            charMapper.Add("one", "1");
            charMapper.Add("two", "2");
            charMapper.Add("three", "3");
            charMapper.Add("four", "4");
            charMapper.Add("five", "5");
            charMapper.Add("six", "6");
            charMapper.Add("seven", "7");
            charMapper.Add("eight", "8");
            charMapper.Add("nine", "9");
            charMapper.Add("zero", "0");
            charMapper.Add("colon", ":");
            charMapper.Add("semicolon", ";");
            charMapper.Add("less", "<");
            charMapper.Add("equal", "=");
            charMapper.Add("greater", ">");
            charMapper.Add("question", "?");
            charMapper.Add("at", "@");
            charMapper.Add("bracketleft", "[");
            charMapper.Add("backslash", "\u005C");
            charMapper.Add("bracketright", "]");
            charMapper.Add("asciicircum", "^");
            charMapper.Add("underscore", "_");
            charMapper.Add("grave", "`");
            charMapper.Add("braceleft", "{");
            charMapper.Add("bar", "|");
            charMapper.Add("braceright", "}");
            charMapper.Add("asciitilde", "~");
            charMapper.Add("ltshade", "\u00B0");
            charMapper.Add("shade", "\u00B1");
            charMapper.Add("dkshade", "\u00B2");
            charMapper.Add("SF110000", "\u00B3");
            charMapper.Add("SF090000", "\u00B4");
            charMapper.Add("SF190000", "\u00B5");
            charMapper.Add("SF200000", "\u00B6");
            charMapper.Add("SF210000", "\u00B7");
            charMapper.Add("SF220000", "\u00B8");
            charMapper.Add("SF230000", "\u00B9");
            charMapper.Add("SF240000", "\u00BA");
            charMapper.Add("SF250000", "\u00BB");
            charMapper.Add("SF260000", "\u00BC");
            charMapper.Add("SF270000", "\u00BD");
            charMapper.Add("SF280000", "\u00BE");
            charMapper.Add("SF030000", "\u00BF");
            charMapper.Add("SF020000", "\u00C0");
            charMapper.Add("SF070000", "\u00C1");
            charMapper.Add("SF060000", "\u00C2");
            charMapper.Add("SF080000", "\u00C3");
            charMapper.Add("SF100000", "\u00C4");
            charMapper.Add("SF050000", "\u00C5");
            charMapper.Add("SF360000", "\u00C6");
            charMapper.Add("SF370000", "\u00C7");
            charMapper.Add("SF380000", "\u00C8");
            charMapper.Add("SF390000", "\u00C9");
            charMapper.Add("SF400000", "\u00CA");
            charMapper.Add("SF410000", "\u00CB");
            charMapper.Add("SF420000", "\u00CC");
            charMapper.Add("SF430000", "\u00CD");
            charMapper.Add("SF440000", "\u00CE");
            charMapper.Add("SF450000", "\u00CF");
            charMapper.Add("SF460000", "\u00D0");
            charMapper.Add("SF470000", "\u00D1");
            charMapper.Add("SF480000", "\u00D2");
            charMapper.Add("SF490000", "\u00D3");
            charMapper.Add("SF500000", "\u00D4");
            charMapper.Add("SF510000", "\u00D5");
            charMapper.Add("SF520000", "\u00D6");
            charMapper.Add("SF530000", "\u00D7");
            charMapper.Add("SF540000", "\u00D8");
            charMapper.Add("SF040000", "\u00D9");
            charMapper.Add("SF010000", "\u00DA");
            charMapper.Add("block", "\u00DB");
            charMapper.Add("dnblock", "\u00DC");
            charMapper.Add("lfblock", "\u00DD");
            charMapper.Add("rtblock", "\u00DE");
            charMapper.Add("upblock", "\u00DF");
            charMapper.Add("space0", "\u0000");
            charMapper.Add("vga1", "\u0001");
            charMapper.Add("vga2", "\u0002");
            charMapper.Add("vga3", "\u0003");
            charMapper.Add("vga4", "\u0004");
            charMapper.Add("vga5", "\u0005");
            charMapper.Add("vga6", "\u0006");
            charMapper.Add("vga7", "\u0007");
            charMapper.Add("vga8", "\u0008");
            charMapper.Add("vga9", "\u0009");
            charMapper.Add("vga10", "\u000A");
            charMapper.Add("vga11", "\u000B");
            charMapper.Add("vga12", "\u000C");
            charMapper.Add("vga13", "\u000D");
            charMapper.Add("vga14", "\u000E");
            charMapper.Add("vga15", "\u000F");
            charMapper.Add("righttriangle", "\u0010");
            charMapper.Add("lefttriangle", "\u0011");
            charMapper.Add("updownarrow", "\u0012");
            charMapper.Add("vga19", "\u0013");
            charMapper.Add("paragraph", "\u0014");
            charMapper.Add("section", "\u0015");
            charMapper.Add("vga22", "\u0016");
            charMapper.Add("vga23", "\u0017");
            charMapper.Add("uparrow", "\u0018");
            charMapper.Add("downarrow", "\u0019");
            charMapper.Add("vga26", "\u001A");
            charMapper.Add("vga27", "\u001B");
            charMapper.Add("vga28", "\u001C");
            charMapper.Add("vga29", "\u001D");
            charMapper.Add("vga30", "\u001E");
            charMapper.Add("vga31", "\u001F");
            charMapper.Add("apostrophe", "\u0027");
            charMapper.Add("minus", "\u002D");
            charMapper.Add("vga127", "\u007F");
            charMapper.Add("Ccedilla", "\u0080");
            charMapper.Add("udiaeresis", "\u0081");
            charMapper.Add("eacute", "\u0082");
            charMapper.Add("acircumflex", "\u0083");
            charMapper.Add("adiaeresis", "\u0084");
            charMapper.Add("agrave", "\u0085");
            charMapper.Add("aring", "\u0086");
            charMapper.Add("ccedilla", "\u0087");
            charMapper.Add("ecircumflex", "\u0088");
            charMapper.Add("ediaeresis", "\u0089");
            charMapper.Add("egrave", "\u008A");
            charMapper.Add("idiaeresis", "\u008B");
            charMapper.Add("icircumflex", "\u008C");
            charMapper.Add("igrave", "\u008D");
            charMapper.Add("Adiaeresis", "\u008E");
            charMapper.Add("Aring", "\u008F");
            charMapper.Add("Eacute", "\u0090");
            charMapper.Add("ae", "\u0091");
            charMapper.Add("AE", "\u0092");
            charMapper.Add("ocircumflex", "\u0093");
            charMapper.Add("odiaeresis", "\u0094");
            charMapper.Add("ograve", "\u0095");
            charMapper.Add("ucircumflex", "\u0096");
            charMapper.Add("ugrave", "\u0097");
            charMapper.Add("ydiaeresis", "\u0098");
            charMapper.Add("Odiaeresis", "\u0099");
            charMapper.Add("Udiaeresis", "\u009A");
            charMapper.Add("cent", "\u009B");
            charMapper.Add("sterling", "\u009C");
            charMapper.Add("yen", "\u009D");
            charMapper.Add("vga158", "\u009E");
            charMapper.Add("vga159", "\u009F");
            charMapper.Add("aacute", "\u00A0");
            charMapper.Add("iacute", "\u00A1");
            charMapper.Add("oacute", "\u00A2");
            charMapper.Add("uacute", "\u00A3");
            charMapper.Add("vga164", "\u00A4");
            charMapper.Add("vga165", "\u00A5");
            charMapper.Add("vga166", "\u00A6");
            charMapper.Add("vga167", "\u00A7");
            charMapper.Add("questiondown", "\u00A8");
            charMapper.Add("hook", "\u00A9");
            charMapper.Add("notsign", "\u00AA");
            charMapper.Add("onehalf", "\u00AB");
            charMapper.Add("onequarter", "\u00AC");
            charMapper.Add("exclamdown", "\u00AD");
            charMapper.Add("guillemotleft", "\u00AE");
            charMapper.Add("guillemotright", "\u00AF");
            charMapper.Add("raster1", "\u00B0");
            charMapper.Add("raster2", "\u00B1");
            charMapper.Add("raster3", "\u00B2");
            charMapper.Add("udline", "\u00B3");
            charMapper.Add("udlline", "\u00B4");
            charMapper.Add("udLline", "\u00B5");
            charMapper.Add("UDlline", "\u00B6");
            charMapper.Add("Dlline", "\u00B7");
            charMapper.Add("dLline", "\u00B8");
            charMapper.Add("UDLline", "\u00B9");
            charMapper.Add("UDline", "\u00BA");
            charMapper.Add("DLline", "\u00BB");
            charMapper.Add("ULline", "\u00BC");
            charMapper.Add("Ulline", "\u00BD");
            charMapper.Add("uLline", "\u00BE");
            charMapper.Add("dlline", "\u00BF");
            charMapper.Add("urlline", "\u00C1");
            charMapper.Add("rdlline", "\u00C2");
            charMapper.Add("urdline", "\u00C3");
            charMapper.Add("rlline", "\u00C4");
            charMapper.Add("urdlline", "\u00C5");
            charMapper.Add("uRdline", "\u00C6");
            charMapper.Add("UrDline", "\u00C7");
            charMapper.Add("URline", "\u00C8");
            charMapper.Add("RDline", "\u00C9");
            charMapper.Add("URLline", "\u00CA");
            charMapper.Add("RDLline", "\u00CB");
            charMapper.Add("URDline", "\u00CC");
            charMapper.Add("RLline", "\u00CD");
            charMapper.Add("URDLline", "\u00CE");
            charMapper.Add("uRLline", "\u00CF");
            charMapper.Add("Urlline", "\u00D0");
            charMapper.Add("RdLline", "\u00D1");
            charMapper.Add("rDlline", "\u00D2");
            charMapper.Add("Urline", "\u00D3");
            charMapper.Add("uRline", "\u00D4");
            charMapper.Add("Rdline", "\u00D5");
            charMapper.Add("rDline", "\u00D6");
            charMapper.Add("UrDlline", "\u00D7");
            charMapper.Add("uRdLline", "\u00D8");
            charMapper.Add("urline", "\u00D9");
            charMapper.Add("rdline", "\u00DA");
            charMapper.Add("fullblock", "\u00DB");
            charMapper.Add("bottomblock", "\u00DC");
            charMapper.Add("leftblock", "\u00DD");
            charMapper.Add("rightblock", "\u00DE");
            charMapper.Add("topblock", "\u00DF");
            charMapper.Add("vga224", "\u00E0");
            charMapper.Add("vga225", "\u00E1");
            charMapper.Add("vga226", "\u00E2");
            charMapper.Add("vga227", "\u00E3");
            charMapper.Add("vga228", "\u00E4");
            charMapper.Add("vga229", "\u00E5");
            charMapper.Add("vga230", "\u00E6");
            charMapper.Add("vga231", "\u00E7");
            charMapper.Add("vga232", "\u00E8");
            charMapper.Add("vga233", "\u00E9");
            charMapper.Add("vga234", "\u00EA");
            charMapper.Add("vga235", "\u00EB");
            charMapper.Add("vga236", "\u00EC");
            charMapper.Add("vga237", "\u00ED");
            charMapper.Add("vga238", "\u00EE");
            charMapper.Add("vga239", "\u00EF");
            charMapper.Add("isequal", "\u00F0");
            charMapper.Add("plusminus", "\u00F1");
            charMapper.Add("vga242", "\u00F2");
            charMapper.Add("vga243", "\u00F3");
            charMapper.Add("vga244", "\u00F4");
            charMapper.Add("vga245", "\u00F5");
            charMapper.Add("division", "\u00F6");
            charMapper.Add("vga247", "\u00F7");
            charMapper.Add("degree", "\u00F8");
            charMapper.Add("smalldot", "\u00F9");
            charMapper.Add("smallerdot", "\u00FA");
            charMapper.Add("vga251", "\u00FB");
            charMapper.Add("vga252", "\u00FC");
            charMapper.Add("twosuperior", "\u00FD");
            charMapper.Add("bullet", "\u00FE");
            charMapper.Add("space255", "\u00FF");
            charMapper.Add("currency", "\u00A4");
            charMapper.Add("brokenbar", "\u00A6");
            charMapper.Add("nonbreakingspace", "\u00A0");
            charMapper.Add("dieresis", "\u0308");
            charMapper.Add("copyright", "\u00A9");
            charMapper.Add("ordfeminine", "\u00AA");
            charMapper.Add("logicalnot", "\u00AC");
            #endregion

        }

        private FontStyle style = FontStyle.Normal;

        public static BDFFontContainer CreateFont(Stream r)
        {
            BDFParser parser = new BDFParser(r);
            return parser.CreateFont();
        }

        public static int fill(int num)
        {
            return (1 << num + 1) - 1;
        }



        private BDFParser.Rectangle boundingBox = new BDFParser.Rectangle();

        private String[] comments;

        private int contentVersion;

        private int depth = 1;

        private String[] fontName;

        private BDFGlyph[] glyphMapper = new BDFGlyph[ushort.MaxValue];

        private BDFGlyph NotDefGlyph;

        private Dictionary<String, BDFGlyph> UnknownGlyphs = new Dictionary<String, BDFGlyph>();

        private BDFGlyph[] glyphs;

        private String[] properties;

        private BDFParser.Dimension resolution = new BDFParser.Dimension();

        private BDFParser.Version version = new BDFParser.Version();

        private String postScriptName;

        private int size;

        private String family;

        public BDFFontContainer(String[] name, FontStyle style, int size)
        {
            postScriptName = GetPostScriptName(name);
            this.style = style;
            this.size = size;
            this.family = name[FAMILY];
            fontName = name;
        }

        public String getName()
        {
            return postScriptName;
        }

        public String getFamily()
        {
            return family;
        }

        public BDFParser.Rectangle getBoundingBox()
        {
            return boundingBox;
        }

        public FontStyle getStyle()
        {
            return style;
        }

        public String[] getComments()
        {
            return comments;
        }

        public int getContentVersion()
        {
            return contentVersion;
        }

        public int getDepth()
        {
            return depth;
        }

        public int getSize()
        {
            return size;
        }

        public BDFMetrics getFontMetrics()
        {
            return new BDFMetrics(this);
        }

        public BDFGlyph getGlyph(char ch)
        {
            BDFGlyph g = glyphMapper[(ushort)ch];
            if (g == null)
            {
                g = NotDefGlyph;
            }
            return g;
        }

        public BDFGlyph[] getGlyphs()
        {
            return glyphs;
        }

        public String[] getProperties()
        {
            return properties;
        }

        public BDFParser.Dimension getResolution()
        {
            return resolution;
        }

        public BDFParser.Version getVersion()
        {
            return version;
        }

        public void setBoundingBox(int x, int y, int width, int height)
        {
            boundingBox.setBounds(x, y, width, height);
        }

        public void setBoundingBox(BDFParser.Rectangle boundingBox)
        {
            this.boundingBox = boundingBox;
        }

        public void setCharCount(int count)
        {
            glyphs = new BDFGlyph[count];
        }

        public void setComments(String[] comments)
        {
            this.comments = comments;
        }

        public void setContentVersion(int version)
        {
            this.contentVersion = version;
        }

        public void setDepth(int depth)
        {
            this.depth = depth;
        }

        public void setGlyphs(BDFGlyph[] glyphs)
        {
            this.glyphs = glyphs;
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].init(this);
                if (glyphs[i].encoding != -1)
                {
                    glyphMapper[glyphs[i].encoding] = glyphs[i];
                }
                else
                {
                    if (glyphs[i].name.ToLower() == ".notdef")
                    {
                        NotDefGlyph = glyphs[i];
                    }
                    else
                    {
                        UnknownGlyphs.Add(glyphs[i].name, glyphs[i]);
                    }
                }
            }
        }

        public void setMetricsSet(int set)
        {
        }

        public void setProperties(String[] properties)
        {
            this.properties = properties;
        }

        public void setResolution(BDFParser.Dimension resolution)
        {
            this.resolution = resolution;
        }

        public void setResolution(int xres, int yres)
        {
            resolution.setSize(xres, yres);
        }

        public void setVersion(BDFParser.Version version)
        {
            this.version = version;
        }

        public void setVersion(int major, int minor)
        {
            version.setVersion(major, minor);
        }

        public static String GetPostScriptName(String[] str)
        {
            String strng = "";
            for (int i = 0; i < str.Length; i++)
            {
                strng += "-";
                strng += str[i];
            }
            return strng;
        }

        public override String ToString()
        {
            String styleStr = ((style & FontStyle.Bold) != 0 ? "Bold " : "") + ((style & FontStyle.Italic) != 0 ? "Italic" : "");

            if ("" == styleStr)
            {
                styleStr = "Normal";
            }

            return "BDFFontContainer [name=" + fontName[FAMILY] + ", style=" + styleStr + ", size=" + size + "pt, depth=" + depth + "bpp]";
        }
    }
}
