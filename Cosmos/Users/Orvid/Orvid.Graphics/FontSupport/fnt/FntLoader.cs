// Reference: http://support.microsoft.com/kb/65123
using System;
using System.IO;
using System.Collections.Generic;

namespace Orvid.Graphics.FontSupport.fnt
{
    internal class FntLoader
    {
        #region Fields
        /// <summary>
        /// 2 bytes specifying the version (0200H or 0300H) of
        /// the file.
        /// </summary>
        public ushort Version;
        /// <summary>
        /// 4 bytes specifying the total size of the file in
        /// bytes.
        /// </summary>
        public uint FileSize;
        /// <summary>
        /// 60 bytes specifying copyright information.
        /// </summary>
        public string Copyright;
        /// <summary>
        /// 2 bytes specifying the type of font file.
        /// 
        /// The low-order byte is exclusively for GDI use. If the
        /// low-order bit of the WORD is zero, it is a bitmap
        /// (raster) font file. If the low-order bit is 1, it is a
        /// vector font file. The second bit is reserved and must
        /// be zero. If no bits follow in the file and the bits are
        /// located in memory at a fixed address specified in
        /// dfBitsOffset, the third bit is set to 1; otherwise, the
        /// bit is set to 0 (zero). The high-order bit of the low
        /// byte is set if the font was realized by a device. The
        /// remaining bits in the low byte are reserved and set to
        /// zero.
        /// 
        /// The high byte is reserved for device use and will
        /// always be set to zero for GDI-realized standard fonts
        /// Physical fonts with the high-order bit of the low byte
        /// set may use this byte to describe themselves. GDI will
        /// never inspect the high byte.
        /// </summary>
        public ushort Type;
        /// <summary>
        /// 2 bytes specifying the nominal point size at which
        /// this character set looks best.
        /// </summary>
        public ushort PointSize;
        /// <summary>
        /// 2 bytes specifying the nominal vertical resolution
        /// (dots-per-inch) at which this character set was
        /// digitized.
        /// </summary>
        public ushort VertDpi;
        /// <summary>
        /// 2 bytes specifying the nominal horizontal resolution
        /// (dots-per-inch) at which this character set was
        /// digitized.
        /// </summary>
        public ushort HorizDpi;
        /// <summary>
        /// 2 bytes specifying the distance from the top of a
        /// character definition cell to the baseline of the
        /// typographical font. It is useful for aligning the
        /// baselines of fonts of different heights.
        /// </summary>
        public ushort Ascent;
        /// <summary>
        /// Specifies the amount of leading inside the bounds set
        /// by dfPixHeight. Accent marks may occur in this area.
        /// This may be zero at the designer's option.
        /// </summary>
        public ushort InternalLeading;
        /// <summary>
        /// Specifies the amount of extra leading that the designer
        /// requests the application add between rows. Since this
        /// area is outside of the font proper, it contains no
        /// marks and will not be altered by text output calls in
        /// either the OPAQUE or TRANSPARENT mode. This may be zero
        /// at the designer's option.
        /// </summary>
        public ushort ExternalLeading;
        public bool IsItalic;
        public bool IsUnderline;
        public bool IsStrikeOut;
        /// <summary>
        /// 2 bytes specifying the weight of the characters in the
        /// character definition data, on a scale of 1 to 1000. A
        /// dfWeight of 400 specifies a regular weight.
        /// </summary>
        public ushort Weight;
        /// <summary>
        /// 1 byte specifying the character set defined by this
        /// font.
        /// </summary>
        public byte CharSet;
        /// <summary>
        /// 2 bytes specifing the width.
        /// </summary>
        public ushort PixWidth;
        /// <summary>
        /// 2 bytes specifying the height of the character bitmap
        /// (raster fonts), or the height of the grid on which a
        /// vector font was digitized.
        /// </summary>
        public ushort PixHeight;
        /// <summary>
        /// Specifies the pitch and font family. The low bit is set
        /// if the font is variable pitch. The high four bits give
        /// the family name of the font. Font families describe in
        /// a general way the look of a font. They are intended for
        /// specifying fonts when the exact face name desired is
        /// not available. The families are as follows:
        /// 
        ///   Family               Description
        ///   ------               -----------
        ///   FF_DONTCARE (0<<4)   Don't care or don't know.
        ///   FF_ROMAN (1<<4)      Proportionally spaced fonts with serifs.
        ///   FF_SWISS (2<<4)      Proportionally spaced fonts without serifs.
        ///   FF_MODERN (3<<4)     Fixed-pitch fonts.
        ///   FF_SCRIPT (4<<4)
        ///   FF_DECORATIVE (5<<4)
        /// </summary>
        public byte PitchAndFamily;
        /// <summary>
        /// 2 bytes specifying the width of characters in the font.
        /// For fixed-pitch fonts, this is the same as dfPixWidth.
        /// For variable-pitch fonts, this is the width of the
        /// character "X."
        /// </summary>
        public ushort AvgWidth;
        /// <summary>
        /// 2 bytes specifying the maximum pixel width of any
        /// character in the font. For fixed-pitch fonts, this is
        /// simply dfPixWidth.
        /// </summary>
        public ushort MaxWidth;
        /// <summary>
        /// 1 byte specifying the first character code defined by
        /// this font. Character definitions are stored only for
        /// the characters actually present in a font. Therefore,
        /// use this field when calculating indexes into either
        /// dfBits or dfCharOffset.
        /// </summary>
        public byte FirstChar;
        /// <summary>
        /// 1 byte specifying the last character code defined by
        /// this font. Note that all the characters with codes
        /// between dfFirstChar and dfLastChar must be present in
        /// the font character definitions.
        /// </summary>
        public byte LastChar;
        /// <summary>
        /// 1 byte specifying the character to substitute
        /// whenever a string contains a character out of the
        /// range. The character is given relative to dfFirstChar
        /// so that dfDefaultChar is the actual value of the
        /// character, less dfFirstChar. The dfDefaultChar should
        /// indicate a special character that is not a space.
        /// </summary>
        public byte DefaultChar;
        /// <summary>
        /// 1 byte specifying the character that will define word
        /// breaks. This character defines word breaks for word
        /// wrapping and word spacing justification. The character
        /// is given relative to dfFirstChar so that dfBreakChar is
        /// the actual value of the character, less that of
        /// dfFirstChar. The dfBreakChar is normally (32 -
        /// dfFirstChar), which is an ASCII space.
        /// </summary>
        public byte BreakChar;
        /// <summary>
        /// 2 bytes specifying the number of bytes in each row of
        /// the bitmap. This is always even, so that the rows start
        /// on WORD boundaries. For vector fonts, this field has no
        /// meaning.
        /// </summary>
        public ushort WidthBytes;
        /// <summary>
        /// 4 bytes specifying the offset in the file to the
        /// null-terminated string giving the device name. 
        /// For a generic font, this value is zero.
        /// </summary>
        public uint Device;
        /// <summary>
        /// 4 bytes specifying the offset in the file to the
        /// null-terminated string that names the face.
        /// </summary>
        public uint Face;
        /// <summary>
        /// 4 bytes specifying the absolute machine address of
        /// the bitmap. This is set by GDI at load time. The
        /// dfBitsPointer is guaranteed to be even.
        /// </summary>
        public uint BitsPointer;
        /// <summary>
        /// 4 bytes specifying the offset in the file to the
        /// beginning of the bitmap information. If the 04H bit in
        /// the dfType is set, then dfBitsOffset is an absolute
        /// address of the bitmap (probably in ROM).
        /// 
        /// For raster fonts, dfBitsOffset points to a sequence of
        /// bytes that make up the bitmap of the font, whose height
        /// is the height of the font, and whose width is the sum
        /// of the widths of the characters in the font rounded up
        /// to the next WORD boundary.
        /// 
        /// For vector fonts, it points to a string of bytes or
        /// words (depending on the size of the grid on which the
        /// font was digitized) that specify the strokes for each
        /// character of the font. The dfBitsOffset field must be
        /// even.
        /// </summary>
        public uint BitsOffset;
        /// <summary>
        /// 4 bytes specifying the bits flags, which are additional
        /// flags that define the format of the Glyph bitmap, as
        /// follows:
        /// 
        /// DFF_FIXED            equ  0001h ; font is fixed pitch
        /// DFF_PROPORTIONAL     equ  0002h ; font is proportional pitch
        /// DFF_ABCFIXED         equ  0004h ; font is an ABC fixed font
        /// DFF_ABCPROPORTIONAL  equ  0008h ; font is an ABC pro-portional font
        /// DFF_1COLOR           equ  0010h ; font is one color
        /// DFF_16COLOR          equ  0020h ; font is 16 color
        /// DFF_256COLOR         equ  0040h ; font is 256 color
        /// DFF_RGBCOLOR         equ  0080h ; font is RGB color
        /// </summary>
        public uint Flags;
        /// <summary>
        /// 2 bytes specifying the global A space, if any. The
        /// dfAspace is the distance from the current position to
        /// the left edge of the bitmap.
        /// </summary>
        public ushort ASpace;
        /// <summary>
        /// 2 bytes specifying the global B space, if any. The
        /// dfBspace is the width of the character.
        /// </summary>
        public ushort BSpace;
        /// <summary>
        /// 2 bytes specifying the global C space, if any. The
        /// dfCspace is the distance from the right edge of the
        /// bitmap to the new current position. The increment of a
        /// character is the sum of the three spaces. These apply
        /// to all glyphs and is the case for DFF_ABCFIXED.
        /// </summary>
        public ushort CSpace;
        /// <summary>
        /// 4 bytes specifying the offset to the color table for
        /// color fonts, if any. The format of the bits is similar
        /// to a DIB, but without the header. That is, the
        /// characters are not split up into disjoint bytes.
        /// Instead, they are left intact. If no color table is
        /// needed, this entry is NULL.
        /// </summary>
        public uint ColorPointer;
        
        public FntGlyph[] Glyphs = new FntGlyph[255];
        public string DeviceName;
        public string FaceName;
        public FntGlyphType GlyphType;
        public FntGlyph NotDefChar;

        private const byte FixedFlagMask = 1;
        private const byte ProportionalFlagMask = 2;
        private const byte ABCFixedFlagMask = 4;
        private const byte ABCProportionalFlagMask = 8;
        private const byte Color1FlagMask = 16;
        private const byte Color16FlagMask = 32;
        private const byte Color256FlagMask = 64;
        private const byte ColorRGBFlagMask = 128;
        #endregion


        public FntLoader()
        {
        }

        public string GetFamily()
        {
        	if (PitchAndFamily == 0)
        	{
        		return "None";
        	}
        	else if (PitchAndFamily == (1<<4))
        	{
        		return "Roman";
        	}
        	else if (PitchAndFamily == (2<<4))
        	{
        		return "Swiss";
        	}
        	else if (PitchAndFamily == (3<<4))
        	{
        		return "Modern";
        	}
        	else if (PitchAndFamily == (4<<4))
        	{
        		return "Script";
        	}
        	else if (PitchAndFamily == (5<<4))
        	{
        		return "Decorative";
        	}
        	else
        	{
        		return "Unknown";
        	}
        }
        
        public Rectangle GetBounds()
        {
        	Rectangle r = new Rectangle();
        	r.SetBounds(this.CSpace, this.Ascent, this.MaxWidth, this.PixHeight);
        	return r;
        }
        
        public int GetDepth()
        {
        	switch (GlyphType)
        	{
        		case FntGlyphType.Version2:
        		case FntGlyphType.Color1:
        			return 1;
        		default:
        			return 900;
        	}
        }
        
        public FontStyle GetStyle()
        {
        	FontStyle f = FontStyle.Normal;
        	if (IsItalic)
        		f |= FontStyle.Italic;
        	if (IsStrikeOut)
        		f |= FontStyle.Strikeout;
        	if (IsUnderline)
        		f |= FontStyle.Underline;
        	if (Weight >= 800)
        		f |= FontStyle.Bold;
        	return f;
        }
        
        public FntGlyph GetGlyph(char c)
        {
        	if (((int)c) > 255)
        		return NotDefChar;
        	FntGlyph g = Glyphs[((int)c) - 1];
            if (g == null)
                return NotDefChar;
            return g;
        }
        
        public int GetSize()
        {
        	return PointSize;
        }
        
        public void Load(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            byte[] buf;


            Version = br.ReadUInt16();
            if (Version != 0x0300 && Version != 0x0200)
            {
                throw new Exception("Unknown format version!");
            }

            FileSize = br.ReadUInt32();
            if (FileSize != s.Length)
            {
                throw new Exception("FileSize incorrect! Possible corrupt file?");
            }

            buf = br.ReadBytes(60);
            Copyright = System.Text.ASCIIEncoding.ASCII.GetString(buf).Replace("\0", "");
            buf = null;

            Type = br.ReadUInt16();
            if ((Type & 1) == 1)
            {
                throw new Exception("Can't handle Vector FNT files.");
            }
            PointSize = br.ReadUInt16();
            VertDpi = br.ReadUInt16();
            HorizDpi = br.ReadUInt16();
            Ascent = br.ReadUInt16();
            InternalLeading = br.ReadUInt16();
            ExternalLeading = br.ReadUInt16();

            if (br.ReadByte() != 0)
            {
                IsItalic = true;
            }
            else
            {
                IsItalic = false;
            }

            if (br.ReadByte() != 0)
            {
                IsUnderline = true;
            }
            else
            {
                IsUnderline = false;
            }

            if (br.ReadByte() != 0)
            {
                IsStrikeOut = true;
            }
            else
            {
                IsStrikeOut = false;
            }

            Weight = br.ReadUInt16();
            CharSet = br.ReadByte();
            PixWidth = br.ReadUInt16();
            PixHeight = br.ReadUInt16();
            PitchAndFamily = br.ReadByte();
            AvgWidth = br.ReadUInt16();
            MaxWidth = br.ReadUInt16();
            FirstChar = br.ReadByte();
            LastChar = br.ReadByte();
            DefaultChar = br.ReadByte();
            BreakChar = br.ReadByte();
            WidthBytes = br.ReadUInt16();
            
            long position;
            string deviceName = "";
            char curChar = ' ';
            
            #region Read Device Name
            Device = br.ReadUInt32();
            position = br.BaseStream.Position;
            br.BaseStream.Position = Device;
            br.BaseStream.Flush();
            while (curChar != '\u0000')
            {
            	deviceName += curChar;
            	curChar = (char)br.ReadByte();
            }
            br.BaseStream.Position = position;
            br.BaseStream.Flush();
            DeviceName = deviceName.Substring(1);
            #endregion
                    
            #region Read Face Name
            Face = br.ReadUInt32();
            deviceName = ""; // re-use the variable.
            curChar = ' ';
            position = br.BaseStream.Position;
            br.BaseStream.Position = Face;
            br.BaseStream.Flush();
            // loop while it's not the null terminator.
            while (curChar != '\u0000')
            {
            	deviceName += curChar;
            	curChar = (char)br.ReadByte();
            }
            br.BaseStream.Position = position;
            br.BaseStream.Flush();
            // account for the extra char we added.
            FaceName = deviceName.Substring(1);
            #endregion
            
            BitsPointer = br.ReadUInt32();
            BitsOffset = br.ReadUInt32();

            br.ReadByte(); // Not used.

            if (Version == 0x0300)
            {
                Flags = br.ReadUInt32();
                ASpace = br.ReadUInt16();
                BSpace = br.ReadUInt16();
                CSpace = br.ReadUInt16();
                ColorPointer = br.ReadUInt32();

                br.ReadBytes(16); // Not used.
            }

            // Next is the character table.
            List<FntGlyph> chars = new List<FntGlyph>();
            FntGlyph g;
            uint nChars = (uint)((LastChar - FirstChar) + 2);
            position = 0;
            long loc;
            int bytesToRead;
            if (Version == 0x0200)
            {
            	this.GlyphType = FntGlyphType.Version2;
                for (uint c = 0; c < nChars; c++)
                {
                    g = new FntGlyph();
                    g.Width = br.ReadUInt16();
                    g.Type = FntGlyphType.Version2;
                    g.Height = PixHeight;
                    g.Font = this;

                    #region Read Data
                    loc = br.ReadUInt16();
                    position = br.BaseStream.Position;
                    br.BaseStream.Position = loc;
                    br.BaseStream.Flush();
                    bytesToRead = (int)(((g.Width + 7) >> 3) * (PixHeight));
                    g.RawData = br.ReadBytes(bytesToRead);
                    br.BaseStream.Position = position;
                    br.BaseStream.Flush();
                    #endregion

                    chars.Add(g);

                }
            }

            for (int i = 0; i < chars.Count; i++)
            {
                chars[i].Initialize();
            }
            Array.Copy(chars.ToArray(), 0, Glyphs, FirstChar - 1, chars.Count - 1);
            //Glyphs = chars;
            NotDefChar = chars[chars.Count - 1];
            //throw new Exception();

            //else // version == 0x0300
            //{
            //    if ((Flags & FixedFlagMask) == FixedFlagMask || (Flags & ProportionalFlagMask) == ProportionalFlagMask)
            //    {
            //        FntGlyphType t;
            //        if ((Flags & FixedFlagMask) == FixedFlagMask)
            //        {
            //            t = FntGlyphType.Fixed;
            //        }
            //        else
            //        {
            //            t = FntGlyphType.Proportional;
            //        }
            //        for (uint c = 0; c < nChars; c++)
            //        {
            //            g = new FntGlyph();
            //            g.Type = t;

            //        }
            //    }
            //    else if ((Flags & ABCFixedFlagMask) == ABCFixedFlagMask || (Flags & ABCProportionalFlagMask) == ABCProportionalFlagMask)
            //    {
            //        for (uint c = 0; c < nChars; c++)
            //        {

            //        }
            //    }
            //    else // A color flag.
            //    {
            //        for (uint c = 0; c < nChars; c++)
            //        {

            //        }
            //    }
            //}

        }

    }
}
