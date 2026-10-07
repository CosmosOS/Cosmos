using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LunarLabs.Fonts
{
    internal class GlyphBitmap
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Pixels;

        public GlyphBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new byte[width * height];
        }

        public void Draw(GlyphBitmap other, int x, int y)
        {
            for (int j = 0; j < other.Height; j++)
            {
                for (int i = 0; i < other.Width; i++)
                {
                    var srcOfs = i + j * other.Width;
                    var destOfs = (x + i) + (y + j) * this.Width;
                    Pixels[destOfs] = other.Pixels[srcOfs];
                }
            }
        }
    }

    internal class FontGlyph
    {
        public GlyphBitmap? Image { get; internal set; }
        public int xOfs { get; internal set; }
        public int yOfs { get; internal set; }
        public int xAdvance { get; internal set; }
    }

    internal struct Edge
    {
        public float x0;
        public float y0;
        public float x1;
        public float y1;
        public bool invert;
    }

    internal struct Vertex
    {
        public short x;
        public short y;
        public short cx;
        public short cy;
        public byte vertexType;

        public Vertex(byte vertexType, short x, short y, short cx, short cy)
        {
            this.vertexType = vertexType;
            this.x = x;
            this.y = y;
            this.cx = cx;
            this.cy = cy;
        }
    }

    internal struct Point
    {
        public float x;
        public float y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    internal class ActiveEdge
    {
        public int x;
        public int dx;
        public float ey;
        public ActiveEdge? next;
        public int valid;
    }

    internal class Font
    {
        private int _glyphCount;
        private byte[] _data;              // pointer to .ttf file

        private uint _loca;
        private uint _head;
        private uint _glyf;
        private uint _hhea;
        private uint _hmtx;
        private uint _kern; // table locations as offset from start of .ttf

        private uint _indexMap;                // a cmap mapping for our chosen character encoding
        private int _indexToLocFormat;         // format needed to map from glyph index to glyph

        private const byte PLATFORM_ID_UNICODE = 0;
        private const byte PLATFORM_ID_MAC = 1;
        private const byte PLATFORM_ID_ISO = 2;
        private const byte PLATFORM_ID_MICROSOFT = 3;

        private const byte MS_EID_SYMBOL = 0;
        private const byte MS_EID_UNICODE_BMP = 1;
        private const byte MS_EID_SHIFTJIS = 2;
        private const byte MS_EID_UNICODE_FULL = 10;

        private const int FIXSHIFT = 10;
        private const int FIX = (1 << FIXSHIFT);
        private const int FIXMASK = (FIX - 1);

        private const byte VMOVE = 1;
        private const byte VLINE = 2;
        private const byte VCURVE = 3;

        public Font(byte[] bytes)
        {
            _data = bytes;

            var cmap = FindTable("cmap");
            _loca = FindTable("loca");
            _head = FindTable("head");
            _glyf = FindTable("glyf");
            _hhea = FindTable("hhea");
            _hmtx = FindTable("hmtx");
            _kern = FindTable("kern");

            if (cmap == 0 || _loca == 0 || _head == 0 || _glyf == 0 || _hhea == 0 || _hmtx == 0)
            {
                throw new System.Exception("'Invalid font file");
            }

            var t = FindTable("maxp");

            if (t != 0)
                _glyphCount = ReadU16(t + 4);
            else
                _glyphCount = -1;

            // find a cmap encoding table we understand *now* to avoid searching later. (todo: could make this installable)
            var numTables = (int)(ReadU16(cmap + 2));
            this._indexMap = 0;

            for (int i = 0; i < numTables; i++)
            {
                uint encodingRecord = (uint)(cmap + 4 + 8 * i);

                // find an encoding we understand:
                switch (ReadU16(encodingRecord))
                {
                    case PLATFORM_ID_MICROSOFT:
                        switch (ReadU16(encodingRecord + 2))
                        {
                            case MS_EID_UNICODE_BMP:
                            case MS_EID_UNICODE_FULL:
                                // MS/Unicode
                                _indexMap = (uint)(cmap + ReadU32(encodingRecord + 4));
                                break;
                        }
                        break;
                }
            }

            if (_indexMap == 0)
            {
                throw new System.Exception("Could not find font index map");
            }
            _indexToLocFormat = ReadU16(_head + 50);
        }


        private byte Read8(uint offset)
        {
            if (offset >= _data.Length)
                return 0;
            else
                return _data[offset];
        }

        private ushort ReadU16(uint offset)
        {
            if (offset >= _data.Length)
                return 0;
            else
                return (ushort)((_data[offset] << 8) + _data[offset + 1]);
        }

        private short ReadS16(uint offset)
        {
            if (offset >= _data.Length)
                return 0;
            else
                return (short)((_data[offset] << 8) + _data[offset + 1]);
        }

        private uint ReadU32(uint offset)
        {
            if (offset >= _data.Length)
                return 0;
            else
                return (uint)((_data[offset] << 24) + (_data[offset + 1] << 16) + (_data[offset + 2] << 8) + _data[offset + 3]);
        }

        private int ReadS32(uint offset)
        {
            if (offset >= _data.Length)
                return 0;
            else
                return (int)((_data[offset] << 24) + (_data[offset + 1] << 16) + (_data[offset + 2] << 8) + _data[offset + 3]);
        }

        private bool HasTag(uint offset, string tag)
        {
            if (offset >= _data.Length)
                return false;
            else
            {
                var bytes = Encoding.ASCII.GetBytes(tag);
                return _data[offset + 0] == bytes[0] && _data[offset + 1] == bytes[1] && _data[offset + 2] == bytes[2] && _data[offset + 3] == bytes[3];
            }
        }

        // TODO OPTIMIZE: binary search
        private uint FindTable(string tag)
        {
            var num_tables = ReadU16(4);

            if (num_tables <= 0)
            {
                return 0;
            }

            uint tabledir = 12;

            for (uint i = 0; i < num_tables; i++)
            {
                uint loc = tabledir + 16 * i;
                if (HasTag(loc, tag))
                {
                    return ReadU32(loc + 8);
                }

            }
            return 0;
        }

        private void GetGlyphHMetrics(int glyphIndex, out int advanceWidth, out int leftSideBearing)
        {
            uint numOfLongHorMetrics = ReadU16(_hhea + 34);
            if (glyphIndex < numOfLongHorMetrics)
            {
                advanceWidth = ReadS16((uint)(_hmtx + 4 * glyphIndex));
                leftSideBearing = ReadS16((uint)(_hmtx + 4 * glyphIndex + 2));
            }
            else
            {
                advanceWidth = ReadS16(_hmtx + 4 * (numOfLongHorMetrics - 1));
                leftSideBearing = ReadS16((uint)(_hmtx + 4 * numOfLongHorMetrics + 2 * (glyphIndex - numOfLongHorMetrics)));
            }
        }

        private int GetGlyphKernAdvance(int glyph1, int glyph2)
        {
            if (this._kern <= 0)
            {
                return 0;
            }

            // we only look at the first table. it must be 'horizontal' and format 0.
            if (ReadU16(this._kern + 2) < 1) // number of tables
            {
                return 0;
            }

            if (ReadU16(this._kern + 8) != 1) // horizontal flag, format
            {
                return 0;
            }

            int l = 0;
            int r = ReadU16(this._kern + 10) - 1;
            uint needle = (uint)((glyph1 << 16) + glyph2);
            while (l <= r)
            {
                var m = (l + r) >> 1;
                var straw = ReadU32((uint)(this._kern + 18 + (m * 6))); // note: unaligned read
                if (needle < straw)
                    r = m - 1;
                else
                    if (needle > straw)
                        l = m + 1;
                    else
                    {
                        return ReadS16((uint)(this._kern + 22 + (m * 6)));
                    }
            }

            return 0;
        }

        private int GetCodepointKernAdvance(char ch1, char ch2)
        {
            if (_kern <= 0)
                return 0;
            else
                return GetGlyphKernAdvance(FindGlyphIndex(ch1), FindGlyphIndex(ch2));
        }

        public void GetCodepointHMetrics(char codepoint, out int advanceWidth, out int leftSideBearing)
        {
            GetGlyphHMetrics(FindGlyphIndex(codepoint), out advanceWidth, out leftSideBearing);
        }

        // ascent is the coordinate above the baseline the font extends;
        // descent is the coordinate below the baseline the font extends (i.e. it is typically negative)
        // lineGap is the spacing between one row's descent and the next row's ascent...
        // you should advance the vertical position by "*ascent - *descent + *lineGap"
        // these are expressed in unscaled coordinates, so you must multiply by the scale factor for a given size
        public void GetFontVMetrics(out int ascent, out int descent, out int lineGap)
        {
            ascent = ReadS16(_hhea + 4);
            descent = ReadS16(_hhea + 6);
            lineGap = ReadS16(_hhea + 8);
        }

        public float ScaleInEm(float ems)
        {
            return ScaleInPixels(ems * 16.0f);
        }

        public float ScaleInPixels(float pixelHeight)
        {
            var ascent = ReadS16(_hhea + 4);
            var descent = ReadS16(_hhea + 6);
            float fHeight = ascent - descent;
            return pixelHeight / fHeight;
        }

        private GlyphBitmap GetCodepointBitmap(float scaleX, float scaleY, char codepoint, out int xoff, out int yoff)
        {
            return GetGlyphBitmap(scaleX, scaleY, 0, 0, FindGlyphIndex(codepoint), out xoff, out yoff);
        }

        private GlyphBitmap GetGlyphBitmap(float scale_x, float scale_y, float shift_x, float shift_y, int glyph, out int xoff, out int yoff)
        {
            var vertices = GetGlyphShape(glyph)
                ?? throw new Exception($"Vertices can not be null");

            if (scale_x == 0)
                scale_x = scale_y;

            if (scale_y == 0)
            {
                if (scale_x == 0)
                {
                    throw new Exception("invalid scale");
                }

                scale_y = scale_x;
            }

            int ix0, iy0, ix1, iy1;

            GetGlyphBitmapBox(glyph, scale_x, scale_y, shift_x, shift_y, out ix0, out iy0, out ix1, out iy1);

            int w = (ix1 - ix0);
            int h = (iy1 - iy0);

            if (w <= 0 || h <= 0)
            {
                throw new Exception("invalid glyph size");
            }

            // now we get the size
            var result = new GlyphBitmap(w, h);
            xoff = ix0;
            yoff = iy0;
            Rasterize(result, 0.35f, vertices, scale_x, scale_y, shift_x, shift_y, ix0, iy0, true);
            return result;
        }

        private ushort FindGlyphIndex(char unicodeCodepoint)
        {
            var format = ReadU16(_indexMap);

            switch (format)
            {
                // apple byte encoding
                case 0:
                {
                    var bytes = ReadU16(_indexMap + 2);
                    if (unicodeCodepoint < bytes - 6)
                    {
                        return _data[_indexMap + 6 + unicodeCodepoint];
                    }
                    else
                    {
                        return 0;
                    }
                }

                case 6:
                {
                    var first = ReadU16(_indexMap + 6);
                    var count = ReadU16(_indexMap + 8);
                    if (unicodeCodepoint >= first && unicodeCodepoint < first + count)
                    {
                        return ReadU16((uint)(_indexMap + 10 + (unicodeCodepoint - first) * 2));
                    }
                    else
                    {
                        return 0;
                    }
                }

                // TODO: high-byte mapping for japanese/chinese/korean
                case 2:
                {
                    return 0;
                }

                // standard mapping for windows fonts: binary search collection of ranges
                case 4:
                {
                    var segcount = ReadU16(_indexMap + 6) >> 1;
                    var searchRange = ReadU16(_indexMap + 8) >> 1;
                    var entrySelector = ReadU16(_indexMap + 10);
                    var rangeShift = ReadU16(_indexMap + 12) >> 1;

                    // do a binary search of the segments
                    var endCount = _indexMap + 14;
                    var search = endCount;

                    if (unicodeCodepoint > 0xFFFF)
                    {
                        return 0;
                    }

                    // they lie from endCount .. endCount + segCount
                    // but searchRange is the nearest power of two, so...
                    if (unicodeCodepoint >= ReadU16((uint)(search + rangeShift * 2)))
                    {
                        search = (uint)(search + rangeShift * 2);
                    }

                    // now decrement to bias correctly to find smallest
                    search -= 2;

                    while (entrySelector != 0)
                    {
                        //stbtt_uint16 start, end;
                        searchRange = searchRange >> 1;
                        var startValue2 = ReadU16((uint)(search + 2 + segcount * 2 + 2));
                        var endValue2 = ReadU16(search + 2);
                        startValue2 = ReadU16((uint)(search + searchRange * 2 + segcount * 2 + 2));
                        endValue2 = ReadU16((uint)(search + searchRange * 2));

                        if (unicodeCodepoint > endValue2)
                        {
                            search = (uint)(search + searchRange * 2);
                        }

                        entrySelector--;
                    }


                    search += 2;

                    var item = (ushort)((search - endCount) >> 1);

                    //STBTT_assert(unicode_codepoint <= ttUSHORT(data + endCount + 2*item));
                    var startValue = ReadU16((uint)(_indexMap + 14 + segcount * 2 + 2 + 2 * item));
                    var endValue = ReadU16((uint)(_indexMap + 14 + 2 + 2 * item));
                    if (unicodeCodepoint < startValue)
                    {
                        //IntToString(unicode_codepoint); //BOO
                        return 0;
                    }

                    var offset = ReadU16((uint)(_indexMap + 14 + segcount * 6 + 2 + 2 * item));
                    if (offset == 0)
                    {
                        var n = ReadS16((uint)(_indexMap + 14 + segcount * 4 + 2 + 2 * item));
                        return (ushort)(unicodeCodepoint + n);
                    }

                    return ReadU16((uint)(offset + (unicodeCodepoint - startValue) * 2 + _indexMap + 14 + segcount * 6 + 2 + 2 * item));
                }

                case 12:
                {
                    int ngroups = ReadU16(_indexMap + 6);
                    int low = 0;
                    int high = ngroups;

                    // Binary search the right group.
                    while (low <= high)
                    {
                        var mid = low + ((high - low) >> 1); // rounds down, so low <= mid < high
                        var start_char = ReadU32((uint)(_indexMap + 16 + mid * 12));
                        var end_char = ReadU32((uint)(_indexMap + 16 + mid * 12 + 4));
                        if (unicodeCodepoint < start_char)
                            high = mid - 1;
                        else
                            if (unicodeCodepoint > end_char)
                                low = mid + 1;
                            else
                            {
                                uint start_glyph = ReadU32((uint)(_indexMap + 16 + mid * 12 + 8));
                                return (ushort)(start_glyph + unicodeCodepoint - start_char);
                            }
                    }

                    return 0; // not found
                }

                // TODO
                default:
                {
                    return 0;
                }
            }
        }

        private int GetGlyfOffset(int glyphIndex)
        {
            if (glyphIndex >= _glyphCount)
            {
                return -1; // glyph index out of range
            }

            if (_indexToLocFormat >= 2)
            {
                return -1; // unknown index->glyph map format
            }

            int g1, g2;
            if (_indexToLocFormat == 0)
            {
                g1 = (int)(_glyf + ReadU16((uint)(_loca + glyphIndex * 2)) * 2);
                g2 = (int)(_glyf + ReadU16((uint)(_loca + glyphIndex * 2 + 2)) * 2);
            }
            else
            {
                g1 = (int)(_glyf + ReadU32((uint)(_loca + glyphIndex * 4)));
                g2 = (int)(_glyf + ReadU32((uint)(_loca + glyphIndex * 4 + 4)));
            }

            if (g1 == g2)
            {
                return -1; // if length is 0, return -1
            }
            else
            {
                return g1;
            }
        }

        private bool GetGlyphBox(int glyphIndex, out int x0, out int y0, out int x1, out int y1)
        {
            var g = GetGlyfOffset(glyphIndex);
            if (g < 0)
            {
                x0 = 0;
                x1 = 0;
                y0 = 0;
                y1 = 0;
                return false;
            }

            x0 = ReadS16((uint)g + 2);
            y0 = ReadS16((uint)g + 4);
            x1 = ReadS16((uint)g + 6);
            y1 = ReadS16((uint)g + 8);

            return true;
        }

        private bool GetCodepointBox(char codepoint, out int x0, out int y0, out int x1, out int y1)
        {
            return GetGlyphBox(FindGlyphIndex(codepoint), out x0, out y0, out x1, out y1);
        }

        private List<Vertex>? GetGlyphShape(int glyphIndex)
        {
            var g = GetGlyfOffset(glyphIndex);

            if (g < 0)
            {
                return null;
            }

            var result = new List<Vertex>();
            var numberOfContours = ReadS16((uint)g);

            if (numberOfContours > 0)
            {
                byte flags = 0;
                int j = 0;
                bool was_off = false;
                uint endPtsOfContours = (uint)(g + 10);
                int ins = ReadU16((uint)(g + 10 + numberOfContours * 2));
                uint pointOffset = (uint)(g + 10 + numberOfContours * 2 + 2 + ins);

                int n = 1 + ReadU16((uint)(endPtsOfContours + numberOfContours * 2 - 2));

                int m = n + numberOfContours;  // a loose bound on how many vertices we might need

                //Result.Count := M;
                //SetLength(Result.List, Result.Count);
                var vertices = new Vertex[m];

                int next_move = 0;
                byte flagCount = 0;

                // in first pass, we load uninterpreted data into the allocated array
                // above, shifted to the end of the array so we won't overwrite it when
                // we create our final data starting from the front

                int off = m - n; // starting offset for uninterpreted data, regardless of how m ends up being calculated

                // first load flags

                int pointIndex = 0;
                for (var i = 0; i < n; i++)
                {
                    if (flagCount == 0)
                    {
                        flags = _data[pointOffset + pointIndex];
                        pointIndex++;
                        if ((flags & 8) != 0)
                        {
                            flagCount = _data[pointOffset + pointIndex];
                            pointIndex++;
                        }
                    }
                    else
                    {
                        flagCount--;
                    }

                    vertices[off + i].vertexType = flags;
                }

                // now load x coordinates
                short x = 0;
                for (var i = 0; i < n; i++)
                {
                    flags = vertices[off + i].vertexType;
                    if ((flags & 2) != 0)
                    {
                        short dx = _data[pointOffset + pointIndex];
                        pointIndex++;

                        if ((flags & 16) != 0)
                        {
                            x += dx;
                        }
                        else
                        {
                            x -= dx; // ???
                        }
                    }
                    else
                    {
                        if ((flags & 16) == 0)
                        {
                            x += ReadS16((uint)(pointOffset + pointIndex)); // PORT
                            pointIndex += 2;
                        }
                    }
                    vertices[off + i].x = x;
                }

                // now load y coordinates
                short y = 0;
                for (int i = 0; i < n; i++)
                {
                    flags = vertices[off + i].vertexType;
                    if ((flags & 4) != 0)
                    {
                        short dy = _data[pointOffset + pointIndex];
                        pointIndex++;

                        if ((flags & 32) != 0)
                        {
                            y += dy;
                        }
                        else
                        {
                            y -= dy; // ???
                        }
                    }
                    else
                    {
                        if ((flags & 32) == 0)
                        {
                            y += ReadS16((uint)(pointOffset + pointIndex)); // PORT
                            pointIndex += 2;
                        }
                    }
                    vertices[off + i].y = y;
                }

                // now convert them to our format
                short sx = 0;
                short sy = 0;
                short cx = 0;
                short cy = 0;
                var index = 0;
                while (index < n)
                {
                    flags = vertices[off + index].vertexType;
                    x = vertices[off + index].x;
                    y = vertices[off + index].y;
                    if (next_move == index)
                    {
                        // when we get to the end, we have to close the shape explicitly
                        if (index != 0)
                        {
                            if (was_off)
                            {
                                result.Add(new Vertex(VCURVE, sx, sy, cx, cy));
                            }
                            else
                            {
                                result.Add(new Vertex(VLINE, sx, sy, 0, 0));
                            }
                        }

                        // now start the new one
                        result.Add(new Vertex(VMOVE, x, y, 0, 0));

                        next_move = 1 + ReadU16((uint)(endPtsOfContours + j * 2));
                        j++;
                        was_off = false;
                        sx = x;
                        sy = y;
                    }
                    else
                    {
                        if ((flags & 1) == 0) // if it's a curve
                        {
                            if (was_off) // two off-curve control points in a row means interpolate an on-curve midpoint
                            {
                                result.Add(new Vertex(VCURVE, (short)((cx + x) >> 1), (short)((cy + y) >> 1), cx, cy));
                            }
                            cx = x;
                            cy = y;
                            was_off = true;
                        }
                        else
                        {
                            if (was_off)
                            {
                                result.Add(new Vertex(VCURVE, x, y, cx, cy));
                            }
                            else
                            {
                                result.Add(new Vertex(VLINE, x, y, 0, 0));
                            }
                            was_off = false;
                        }
                    }
                    index++;
                }

                if (index != 0)
                {
                    if (was_off)
                        result.Add(new Vertex(VCURVE, sx, sy, cx, cy));
                    else
                        result.Add(new Vertex(VLINE, sx, sy, 0, 0));
                }
            }
            else
                if (numberOfContours == -1)
                {
                    // Compound shapes.
                    bool more = true;
                    var comp2 = (uint)(g + 10);

                    var mtx = new float[6];

                    while (more)
                    {
                        mtx[0] = 1;
                        mtx[1] = 0;
                        mtx[2] = 0;
                        mtx[3] = 1;
                        mtx[4] = 0;
                        mtx[5] = 0;

                        byte flags = (byte)ReadS16(comp2);
                        comp2 += 2;
                        short gidx = ReadS16(comp2);
                        comp2 += 2;

                        if ((flags & 2) != 0)// XY values
                        {
                            if ((flags & 1) != 0)// shorts
                            {
                                mtx[4] = ReadS16(comp2);
                                comp2 += 2;
                                mtx[5] = ReadS16(comp2);
                                comp2 += 2;
                            }
                            else
                            {
                                mtx[4] = Read8(comp2);
                                comp2++;
                                mtx[5] = Read8(comp2);
                                comp2++;
                            }
                        }
                        else
                        {
                            // TODO handle matching point
                            throw new NotImplementedException("matching point");
                        }

                        if ((flags & (1 << 3)) != 0) // WE_HAVE_A_SCALE
                        {
                            mtx[0] = ReadS16(comp2) / 16384.0f;
                            mtx[1] = 0;
                            mtx[2] = 0;
                            mtx[3] = ReadS16(comp2) / 16384.0f;
                            comp2 += 2;
                        }
                        else
                            if ((flags & (1 << 6)) != 0)// WE_HAVE_AN_X_AND_YSCALE
                            {
                                mtx[0] = ReadS16(comp2) / 16384.0f;
                                comp2 += 2;
                                mtx[1] = 0;
                                mtx[2] = 0;
                                mtx[3] = ReadS16(comp2) / 16384.0f;
                                comp2 += 2;
                            }
                            else
                                if ((flags & (1 << 7)) != 0) // WE_HAVE_A_TWO_BY_TWO
                                {
                                    mtx[0] = ReadS16(comp2) / 16384.0f;
                                    comp2 += 2;
                                    mtx[1] = ReadS16(comp2) / 16384.0f;
                                    comp2 += 2;
                                    mtx[2] = ReadS16(comp2) / 16384.0f;
                                    comp2 += 2;
                                    mtx[3] = ReadS16(comp2) / 16384.0f;
                                    comp2 += 2;
                                }

                        // Find transformation scales.
                        var ms = (float)Math.Sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
                        var ns = (float)Math.Sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);

                        // Get indexed glyph.
                        var comp_verts = GetGlyphShape(gidx);
                        if (comp_verts?.Count > 0)
                        {
                            // Transform vertices.
                            for (int i = 0; i < comp_verts.Count; i++)

                            {
                                var vert = comp_verts[i];

                                var xx = vert.x;
                                var yy = vert.y;

                                vert.x = (short)(Math.Round(ms * (mtx[0] * xx + mtx[2] * yy + mtx[4])));
                                vert.y = (short)(Math.Round(ns * (mtx[1] * xx + mtx[3] * yy + mtx[5])));

                                xx = vert.cx;
                                yy = vert.cy;

                                vert.cx = (short)(Math.Round(ms * (mtx[0] * xx + mtx[2] * yy + mtx[4])));
                                vert.cy = (short)(Math.Round(ns * (mtx[1] * xx + mtx[3] * yy + mtx[5])));

                                // Append vertices.
                                result.Add(vert);
                            }
                        }

                        // More components ?
                        more = (flags & (1 << 5)) != 0;
                    }
                }
                else
                    if (numberOfContours < 0)
                    {
                        // TODO other compound variations?
                        throw new NotImplementedException("compound variation");
                    }
                    else
                    {
                        // numberOfCounters == 0, do nothing
                    }

            return result;
        }

        // antialiasing software rasterizer
        private void GetGlyphBitmapBox(int glyph, float scale_x, float scale_y, float shift_x, float shift_y, out int ix0, out int iy0, out int ix1, out int iy1)
        {
            int x0, y0, x1, y1;

            if (!GetGlyphBox(glyph, out x0, out y0, out x1, out y1))
            {
                x0 = 0;
                y0 = 0;
                x1 = 0;
                y1 = 0; // e.g. space character
            }

            // now move to integral bboxes (treating pixels as little squares, what pixels get touched)?
            ix0 = (int)Math.Floor(x0 * scale_x + shift_x);
            iy0 = -(int)Math.Ceiling(y1 * scale_y + shift_y);
            ix1 = (int)Math.Ceiling(x1 * scale_x + shift_x);
            iy1 = -(int)Math.Floor(y0 * scale_y + shift_y);
        }

        // tesselate until threshhold p is happy... TODO warped to compensate for non-linear stretching
        private void TesselateCurve(List<Point>? points, ref int numPoints, float x0, float y0, float x1, float y1, float x2, float y2, float objspaceFlatnessSquared, int n)
        //  mx, my, dx, dy: Single;
        {
            // midpoint
            float mx = (x0 + 2 * x1 + x2) / 4.0f;
            float my = (y0 + 2 * y1 + y2) / 4.0f;
            // versus directly drawn line
            float dx = (x0 + x2) / 2 - mx;
            float dy = (y0 + y2) / 2 - my;
            if (n > 16)// 65536 segments on one curve better be enough!
            {
                return;
            }

            if (dx * dx + dy * dy > objspaceFlatnessSquared)// half-pixel error allowed... need to be smaller if AA
            {
                TesselateCurve(points, ref numPoints, x0, y0, (x0 + x1) / 2.0f, (y0 + y1) / 2.0f, mx, my, objspaceFlatnessSquared, n + 1);
                TesselateCurve(points, ref numPoints, mx, my, (x1 + x2) / 2.0f, (y1 + y2) / 2.0f, x2, y2, objspaceFlatnessSquared, n + 1);
            }
            else
            {
                points?.Add(new Point(x2, y2));
                numPoints++;
            }
        }

        // returns number of contours
        private bool FlattenCurves(List<Vertex> vertices, float objSpaceFlatness,
            [NotNullWhen(true)] out int[]? contours, [NotNullWhen(true)] out List<Point>? windings)
        {
            float objspace_flatness_squared = objSpaceFlatness * objSpaceFlatness;
            int n = 0;

            // count how many "moves" there are to get the contour count
            for (int i = 0; i < vertices.Count; i++)
                if (vertices[i].vertexType == VMOVE)
                {
                    n++;
                }

            windings = null;
            contours = null;
            if (n == 0)
            {
                return false;
            }

            int numPoints = 0;
            int start = 0;

            contours = new int[n];

            // make two passes through the points so we don't need to realloc
            for (int pass = 0; pass <= 1; pass++)
            {
                float x = 0;
                float y = 0;
                if (pass == 1)
                {
                    contours = new int[numPoints * 2];
                    windings = new List<Point>(numPoints);
                }

                numPoints = 0;
                n = -1;

                for (int i = 0; i < vertices.Count; i++)
                {
                    switch (vertices[i].vertexType)
                    {
                        case VMOVE:
                        {
                            // start the next contour
                            if (n >= 0)
                            {
                                contours[n] = numPoints - start;
                            }
                            n++;
                            start = numPoints;

                            x = vertices[i].x;
                            y = vertices[i].y;
                            windings?.Add(new Point(x, y));
                            numPoints++;
                            break;
                        }

                        case VLINE:
                        {
                            x = vertices[i].x;
                            y = vertices[i].y;

                            windings?.Add(new Point(x, y));

                            numPoints++;
                            break;
                        }

                        case VCURVE:
                        {
                            TesselateCurve(windings, ref numPoints, x, y,
                                                     vertices[i].cx, vertices[i].cy,
                                                     vertices[i].x, vertices[i].y,
                                                     objspace_flatness_squared, 0);
                            x = vertices[i].x;
                            y = vertices[i].y;
                            break;
                        }
                    }
                }

                contours[n] = numPoints - start;
            }

            return windings is not null;
        }

        private void Rasterize(GlyphBitmap bitmap, float flatnessInPixels, List<Vertex> vertices, float scaleX, float scaleY, float shiftX, float shiftY, int XOff, int YOff, bool Invert)
        {
            float scale = scaleX < scaleY ? scaleX : scaleY;

            if (FlattenCurves(vertices, flatnessInPixels / scale, out var windingLengths, out var windings) && windings.Count > 0)
            {
                Rasterize(bitmap, windings, windingLengths, scaleX, scaleY, shiftX, shiftY, XOff, YOff, Invert);
            }
        }

        private void Rasterize(GlyphBitmap bitmap, List<Point> points, int[] windings, float scaleX, float scaleY, float shiftX, float shiftY, int XOff, int YOff, bool invert)
        {
            int ptOfs = 0;

            float yScaleInv = invert ? -scaleY : scaleY;

            // this value should divide 255 evenly; otherwise we won't reach full opacity
            int vSubSamples = (bitmap.Height < 8) ? 15 : 5;

            var edgeList = new List<Edge>(16);
            int m = 0;

            for (int i = 0; i < windings.Length; i++)
            {
                ptOfs = m;

                m += windings[i];
                int j = windings[i] - 1;
                int k = 0;

                while (k < windings[i])
                {

                    int a = k;
                    int b = j;

                    var en = new Edge();

                    // skip the edge if horizontal
                    if (points[ptOfs + j].y != points[ptOfs + k].y)
                    {
                        // add edge from j to k to the list
                        en.invert = false;

                        if ((invert && points[ptOfs + j].y > points[ptOfs + k].y) || (!invert && points[ptOfs + j].y < points[ptOfs + k].y))
                        {
                            en.invert = true;
                            a = j;
                            b = k;
                        }

                        en.x0 = points[ptOfs + a].x * scaleX + shiftX;
                        en.y0 = points[ptOfs + a].y * yScaleInv * vSubSamples + shiftY;
                        en.x1 = points[ptOfs + b].x * scaleX + shiftX;
                        en.y1 = points[ptOfs + b].y * yScaleInv * vSubSamples + shiftY;

                        edgeList.Add(en);
                    }

                    j = k;
                    k++;
                }
            }

            points.Clear();

            // now sort the edges by their highest point (should snap to integer, and then by x)
            edgeList.Sort((x, y) => EdgeCompare(x, y));

            var temp = new Edge();
            temp.y0 = 10000000;
            edgeList.Add(temp);

            // now, traverse the scanlines and find the intersections on each scanline, use xor winding rule
            RasterizeSortedEdges(bitmap, edgeList, vSubSamples, XOff, YOff);
        }

        private int EdgeCompare(Edge pa, Edge pb)
        {
            if (pa.y0 < pb.y0)
                return -1;

            if (pa.y0 > pb.y0)
                return 1;

            return 0;
        }

        private ActiveEdge CreateActiveEdge(Edge edge, int offX, float startPoint)
        {
            var z = new ActiveEdge(); // TODO: make a pool of these!!!

            float dxdy = (edge.x1 - edge.x0) / (edge.y1 - edge.y0);
            //STBTT_assert(e->y0 <= start_point);

            // round dx down to avoid going too far
            if (dxdy < 0)
                z.dx = -(int)Math.Floor(FIX * -dxdy);
            else
                z.dx = (int)Math.Floor(FIX * dxdy);

            z.x = (int)Math.Floor(FIX * (edge.x0 + dxdy * (startPoint - edge.y0)));
            z.x -= offX * FIX;
            z.ey = edge.y1;
            z.next = null;

            if (edge.invert)
                z.valid = 1;
            else
                z.valid = -1;

            return z;
        }

        private void RasterizeSortedEdges(GlyphBitmap bitmap, List<Edge> e, int vSubSamples, int offX, int off_y)
        {
            int eIndex = 0;

            ActiveEdge? active = null;
            int max_weight = 255 / vSubSamples;  // weight per vertical scanline

            int y = off_y * vSubSamples;

            int n = e.Count - 1;
            var tempEdge = e[n];
            tempEdge.y0 = (off_y + bitmap.Height) * vSubSamples + 1;
            e[n] = tempEdge;

            var scanline = new byte[bitmap.Width];

            float scanY = 0;

            int j = 0;
            while (j < bitmap.Height)
            {
                for (int iii = 0; iii < bitmap.Width; iii++)
                {
                    scanline[iii] = 0;
                }

                for (int s = 0; s < vSubSamples; s++)
                {
                    // find center of pixel for this scanline
                    scanY = y + 0.5f;

                    // update all active edges;
                    // remove all active edges that terminate before the center of this scanline
                    var curr = active;
                    ActiveEdge? prev = null;
                    while (curr != null)
                    {
                        if (curr.ey <= scanY)
                        {
                            // delete from list
                            if (prev != null)
                                prev.next = curr.next;
                            else
                                active = curr.next;

                            curr = curr.next;
                        }

                        else
                        {
                            curr.x += curr.dx; // advance to position for current scanline

                            prev = curr;
                            curr = curr.next; // advance through list
                        }
                    }

                    // resort the list if needed
                    bool changed;
                    do
                    {
                        changed = false;

                        curr = active;
                        prev = null;
                        while (curr != null && curr.next != null)
                        {
                            var prox = curr.next;
                            if (curr.x > prox.x)
                            {
                                if (prev == null)
                                {
                                    active = prox;
                                }
                                else
                                {
                                    prev.next = prox;
                                }

                                curr.next = prox.next;
                                prox.next = curr;
                                changed = true;
                            }

                            prev = curr;
                            curr = curr.next; // advance through list
                        }

                    } while (changed);

                    // insert all edges that start before the center of this scanline -- omit ones that also end on this scanline
                    while (e[eIndex].y0 <= scanY)
                    {
                        if (e[eIndex].y1 > scanY)
                        {
                            var z = CreateActiveEdge(e[eIndex], offX, scanY);
                            // find insertion point
                            if (active == null)
                                active = z;
                            else
                                if (z.x < active.x) // insert at front
                                {
                                    z.next = active;
                                    active = z;
                                }
                                else
                                {
                                    // find thing to insert AFTER
                                    var p = active;
                                    while (p.next != null && p.next.x < z.x)
                                    {
                                        p = p.next;
                                    }

                                    // at this point, p->next->x is NOT < z->x
                                    z.next = p.next;
                                    p.next = z;
                                }
                        }

                        eIndex++;
                    }

                    // now process all active edges in XOR fashion
                    if (active != null)
                    {
                        FillActiveEdges(scanline, bitmap.Width, active, max_weight);
                    }

                    y++;
                }

                for (int iii = 0; iii < bitmap.Width; iii++)
                    if (scanline[iii] > 0) // OPTIMIZATION?
                    {
                        int ofs = iii + j * bitmap.Width;
                        bitmap.Pixels[ofs] = scanline[iii];
                    }

                j++;
            }
        }

        // note: this routine clips fills that extend off the edges...
        // ideally this wouldn't happen, but it could happen if the truetype glyph bounding boxes are wrong, or if the user supplies a too-small bitmap
        private void FillActiveEdges(byte[] scanline, int len, ActiveEdge? e, int max_weight)
        {
            // non-zero winding fill
            int x0 = 0;
            int w = 0;

            int x1;

            while (e != null)
            {
                if (w == 0)
                {
                    // if we're currently at zero, we need to record the edge start point
                    x0 = e.x;
                    w += e.valid;
                }
                else
                {
                    x1 = e.x;
                    w += e.valid;
                    // if we went to zero, we need to draw
                    if (w == 0)
                    {
                        int i = x0 >> FIXSHIFT;
                        int j = x1 >> FIXSHIFT;

                        if (i < len && j >= 0)
                        {
                            if (i == j)
                            {
                                // x0,x1 are the same pixel, so compute combined coverage
                                scanline[i] = (byte)(scanline[i] + (byte)(((x1 - x0) * max_weight) >> FIXSHIFT));
                            }
                            else
                            {
                                if (i >= 0)// add antialiasing for x0
                                    scanline[i] = (byte)(scanline[i] + (byte)(((FIX - (x0 & FIXMASK)) * max_weight) >> FIXSHIFT));
                                else
                                    i = -1; // clip

                                if (j < len) // add antialiasing for x1
                                    scanline[j] = (byte)(scanline[j] + (byte)(((x1 & FIXMASK) * max_weight) >> FIXSHIFT));
                                else
                                    j = len; // clip

                                i++;
                                while (i < j) // fill pixels between x0 and x1
                                {
                                    scanline[i] = (byte)(scanline[i] + (byte)max_weight);
                                    i++;
                                }
                            }
                        }
                    }
                }
                e = e.next;
            }
        }

        public int GetKerning(char current, char next, float scale)
        {
            return (int)Math.Floor(GetCodepointKernAdvance(current, next) * scale);
        }

        public bool HasGlyph(char ID)
        {
            var P = FindGlyphIndex(ID);
            return (P > 0);
        }

        public FontGlyph? RenderGlyph(char ID, float scale)
        {
            if (!HasGlyph(ID))
            {
                return null;
            }

            var glyphTarget = new FontGlyph();

            int xOfs, yOfs;

            if (ID == ' ')
            {
                ID = '_';
                GetCodepointBitmap(scale, scale, ID, out xOfs, out yOfs);
                glyphTarget.Image = new GlyphBitmap(4, 4);
            }
            else
            {
                if (!HasGlyph(ID))
                {
                    if (char.IsLetter(ID))
                    {
                        if (char.IsUpper(ID))
                        {
                            ID = char.ToLowerInvariant(ID);
                        }
                        else
                        {
                            ID = char.ToUpperInvariant(ID);
                        }
                    }
                }

                glyphTarget.Image = GetCodepointBitmap(scale, scale, ID, out xOfs, out yOfs);
            }

            glyphTarget.xOfs = xOfs;
            glyphTarget.yOfs = yOfs;

            int xAdv, lsb;
            GetCodepointHMetrics(ID, out xAdv, out lsb);
            glyphTarget.xAdvance = (int)Math.Floor(xAdv * scale);

            return glyphTarget;
        }
    }
}
