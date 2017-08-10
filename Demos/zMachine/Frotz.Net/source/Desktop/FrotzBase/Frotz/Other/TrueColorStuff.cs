using zword = System.UInt16;
using zbyte = System.Byte;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Frotz.Generic;
using Frotz.Constants;

namespace FrotzNet.Frotz.Other
{
    public static class TrueColorStuff
    {

        static zword NON_STD_COLS = 238;

        static long[] m_colours;
        static long[] m_nonStdColours;

        static zword m_nonStdIndex = 0;


        static long m_defaultFore = -1;
        static long m_defaultBack = -1;

        static TrueColorStuff()
        {
            m_colours = new long[11];
            m_nonStdColours = new long[NON_STD_COLS];

            // TODO Pass in the real default colors
            m_defaultFore = RGB(0xFF, 0xFF, 0xFF);
            m_defaultBack = RGB(0x00, 0x00, 0x80);

            m_colours[0] = RGB5ToTrue(0x0000); // black
            m_colours[1] = RGB5ToTrue(0x001D); // red
            m_colours[2] = RGB5ToTrue(0x0340); // green
            m_colours[3] = RGB5ToTrue(0x03BD); // yellow
            m_colours[4] = RGB5ToTrue(0x59A0); // blue
            m_colours[5] = RGB5ToTrue(0x7C1F); // magenta
            m_colours[6] = RGB5ToTrue(0x77A0); // cyan
            m_colours[7] = RGB5ToTrue(0x7FFF); // white
            m_colours[8] = RGB5ToTrue(0x5AD6); // light grey
            m_colours[9] = RGB5ToTrue(0x4631); // medium grey
            m_colours[10] = RGB5ToTrue(0x2D6B); // dark grey
        }

        internal static int RGB5ToTrue(zword five)
        {
            byte r = (byte)(five & 0x001F);
            byte g = (byte)((five & 0x03E0) >> 5);
            byte b = (byte)((five & 0x7C00) >> 10);
            return RGB(
                (byte)((r << 3) | (r >> 2)),
                (byte)((g << 3) | (g >> 2)),
                (byte)((b << 3) | (b >> 2)));
        }

        internal static int RGB(byte r, byte g, byte b)
        {
            return r | g << 8 | b << 16;

            // return (int)((int)(((byte)(r) | ((short)((byte)(g)) << 8)) | (((int)(byte)(b)) << 16)));
        }

        // Convert from a true colour to 5-bit RGB
        internal static zword TrueToRGB5(long colour)
        {
            int r = GetRValue(colour) >> 3;
            int g = GetGValue(colour) >> 3;
            int b = GetBValue(colour) >> 3;
            return (zword)(r | (g << 5) | (b << 10));
        }

        public static byte GetRValue(long rgb)
        {
            return LOBYTE(rgb);
        }
        
        public static byte GetGValue(long rgb)
        {
            return LOBYTE(rgb >> 8);
        }
        
        public static byte GetBValue(long rgb)
        {
            return LOBYTE(rgb >> 16);
        }

        internal static byte LOBYTE(long w)
        {
            return (byte)(w & 0xff);
        }

        // Get an index for a non-standard colour
        internal static zword GetColourIndex(long colour)
        {
            // Is this a standard colour?
            for (int i = 0; i < 11; i++)
            {
                if (m_colours[i] == colour)
                    return (zword)(i + ZColor.BLACK_COLOUR);
            }

            // Is this a default colour?
            if (m_defaultFore == colour)
                return 16;
            if (m_defaultBack == colour)
                return 17;

            // Is this colour already in the table?
            for (int i = 0; i < NON_STD_COLS; i++)
            {
                if (m_nonStdColours[i] == colour)
                    return (zword)(i + 18);
            }

            // Find a free colour index
            int index = -1;
            while (index == -1)
            {
                if (Screen.colour_in_use(
                    (zword)( m_nonStdIndex + 18 )) == 0)
                {
                    m_nonStdColours[m_nonStdIndex] = colour;
                    index = m_nonStdIndex + 18;
                }

                m_nonStdIndex++;
                if (m_nonStdIndex >= NON_STD_COLS)
                    m_nonStdIndex = 0;
            }
            return (zword) index;
        }


        // Get a colour
        public static long GetColour(int colour)
        {
            // Standard colours
            if ((colour >= ZColor.BLACK_COLOUR) && (colour <= ZColor.DARKGREY_COLOUR))
                return m_colours[colour - ZColor.BLACK_COLOUR];

            // Default colours
            if (colour == 16)
                return m_defaultFore;
            if (colour == 17)
                return m_defaultBack;

            // Non standard colours
            if ((colour >= 18) && (colour < 256))
            {
                if (m_nonStdColours[colour - 18] != 0xFFFFFFFF)
                    return m_nonStdColours[colour - 18];
            }
            return m_colours[0];
        }
    }
}
