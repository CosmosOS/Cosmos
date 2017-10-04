using System;

namespace Orvid.Graphics.FontSupport.fnt
{
    internal enum FntGlyphType
	{
		Fixed,
		Proportional,
		ABCFixed,
		ABCProportional,
		Color1,
		Color16,
		Color256,
		ColorRGB,
		Version2,
	}

    internal class FntGlyph
	{
		private Rectangle bounds;
		public FntLoader Font;
		public int Height;
		public int Width;
		public byte[] RawData;
		public int[] Data;
		public FntGlyphType Type;

		public Rectangle GetBounds()
		{
			return new Rectangle(bounds);
		}
		
		/// <summary>
		/// Processes the raw data into a usable form.
		/// </summary>
		public void Initialize()
		{
			bounds = new Rectangle();
			bounds.SetBounds(0,0,Width,Height);
            switch (Type)
            {
                case FntGlyphType.Version2:
                case FntGlyphType.Color1:
                    uint RowBytes = (uint)((Width + 7) >> 3);
                    uint RowBits = (uint)Math.Floor((double)Width * .125) + (uint)(Width % 8);
                    Data = new int[Height * Width];
                    byte value;
                    for (int y = 0, im = 0; y < Height; y++)
                    {
                        for (int x = 0, xbits = 0; x < RowBytes; x++)
                        {
                            value = RawData[(y * RowBytes) + x];
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x80) >> 7);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x40) >> 6);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x20) >> 5);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x10) >> 4);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x08) >> 3);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x04) >> 2);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x02) >> 1);
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                            if (xbits < RowBits)
                            {
                                Data[im] = (byte)((value & 0x01)     );
                                xbits++;
                                im++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                //case 2:
                //    bits = new int[] {
                //    (value & 0xC0) >> 6,
                //    (value & 0x30) >> 4,
                //    (value & 0x0C) >> 2,
                //    (value & 0x03),
                //};
                //    break;
                //case 4:
                //    bits = new int[] {
                //    (value & 0xF0) >> 4,
                //    (value & 0x0F),
                //};
                //    break;
                //case 8:
                //    bits = new int[] {
                //    value & 0xFF,
                //};
                //    break;
            }
            RawData = null;
		}	
	}
}
