using System;

namespace Orvid.Graphics.FontSupport.bdf
{
    public class BDFGlyph
    {
        public String name;
        public BDFParser.Rectangle bbx = new BDFParser.Rectangle();
        public int[] data;
        public int encoding;
        public String rawData;
        private BDFFontContainer font;
        public int[] decode(int depth, String data)
        {
            int[] result = new int[(data.Length / 2) * (8 / depth)];


            for (int i = 0, offset = 0; i < data.Length; i += 2, offset += (8 / depth))
            {
                String cut = data.Substring(i, 2);
                int value = int.Parse(cut, System.Globalization.NumberStyles.AllowHexSpecifier);
                int[] bits = null;
                switch (depth)
                {
                    case 1:
                        bits = new int[] {
                    (value & 0x80) >> 7,
                    (value & 0x40) >> 6,
                    (value & 0x20) >> 5,
                    (value & 0x10) >> 4,
                    (value & 0x08) >> 3,
                    (value & 0x04) >> 2,
                    (value & 0x02) >> 1,
                    (value & 0x01),
                };
                        break;
                    case 2:
                        bits = new int[] {
                    (value & 0xC0) >> 6,
                    (value & 0x30) >> 4,
                    (value & 0x0C) >> 2,
                    (value & 0x03),
                };
                        break;
                    case 4:
                        bits = new int[] {
                    (value & 0xF0) >> 4,
                    (value & 0x0F),
                };
                        break;
                    case 8:
                        bits = new int[] {
                    value & 0xFF,
                };
                        break;
                }

                for (int k = 0; k < bits.Length; k++)
                    result[offset + k] = bits[k];
            }

            return result;
        }

        public BDFParser.Rectangle getBbx()
        {
            return new BDFParser.Rectangle(bbx);
        }

        public BDFParser.Rectangle getBbx(BDFParser.Rectangle rec)
        {
            rec.x = bbx.x;
            rec.y = bbx.y;
            rec.width = bbx.width;
            rec.height = bbx.height;
            return rec;
        }

        public void init(BDFFontContainer font)
        {
            this.font = font;
            this.data = decode(getFont().getDepth(), rawData);
            rawData = null;
        }

        public void setBbx(BDFParser.Rectangle bbx)
        {
            this.bbx = bbx;
        }

        public int[] getData()
        {
            return data;
        }

        public void setRawData(String rawData)
        {
            this.rawData = rawData;
        }

        public String getName()
        {
            return name;
        }

        public void setName(String name)
        {
            this.name = name;
        }

        public BDFFontContainer getFont()
        {
            return font;
        }

        public BDFGlyph(String name)
        {
            this.name = name;
        }

        public void setSWidth(int swx0, int swy0)
        {
        }

        private BDFParser.Dimension dsize = new BDFParser.Dimension();

        public void setDWidth(int dwx0, int dwy0)
        {
            dsize.setSize(dwx0, dwy0);
        }

        public BDFParser.Dimension getDWidth()
        {
            return dsize;
        }

        public void setBBX(int x, int y, int width, int height)
        {
            bbx.setBounds(x, y, width, height);
        }

        public override String ToString()
        {
            return "BDFGlyph[name=" + name + ", bbx=" + bbx + ", dsize=" + dsize + "]";
        }
    }
}
