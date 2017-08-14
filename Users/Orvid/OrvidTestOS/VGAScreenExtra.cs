using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Orvid.Graphics;

namespace GuessKernel
{
    public class VGAScreenExtra
    {
        #region InternalVGAScreen
        internal class InternalVGAScreen
        {
            private const byte NumSeqRegs = 5;
            private const byte NumCRTCRegs = 25;
            private const byte NumGCRegs = 9;
            private const byte NumACRegs = 21;
            internal readonly Cosmos.Core.IOGroup.VGA mIO = new Cosmos.Core.IOGroup.VGA();
            public int Height { private set; get; }
            public int Width { private set; get; }
            public int Colors { private set; get; }

            public void Clear(int color)
            {
                for (uint i = 0; i < Height * Width; i++)
                {
                    mIO.VGAMemoryBlock[i] = (byte)(color & 0xFF);
                }
            }

            #region WriteVGARegisters
            private void WriteVGARegisters(byte[] registers)
            {
                int xIdx = 0;
                byte i;

                /* write MISCELLANEOUS reg */
                mIO.MiscellaneousOutput_Write.Byte = registers[xIdx];
                xIdx++;
                /* write SEQUENCER regs */
                for (i = 0; i < NumSeqRegs; i++)
                {
                    mIO.Sequencer_Index.Byte = i;
                    mIO.Sequencer_Data.Byte = registers[xIdx];
                    xIdx++;
                }
                /* unlock CRTC registers */
                mIO.CRTController_Index.Byte = 0x03;
                mIO.CRTController_Data.Byte = (byte)(mIO.CRTController_Data.Byte | 0x80);
                mIO.CRTController_Index.Byte = 0x11;
                mIO.CRTController_Data.Byte = (byte)(mIO.CRTController_Data.Byte & 0x7F);

                /* make sure they remain unlocked */
                registers[0x03] |= 0x80;
                registers[0x11] &= 0x7f;

                /* write CRTC regs */
                for (i = 0; i < NumCRTCRegs; i++)
                {
                    mIO.CRTController_Index.Byte = i;
                    mIO.CRTController_Data.Byte = registers[xIdx];
                    xIdx++;
                }
                /* write GRAPHICS CONTROLLER regs */
                for (i = 0; i < NumGCRegs; i++)
                {
                    mIO.GraphicsController_Index.Byte = i;
                    mIO.GraphicsController_Data.Byte = registers[xIdx];
                    xIdx++;
                }
                /* write ATTRIBUTE CONTROLLER regs */
                for (i = 0; i < NumACRegs; i++)
                {
                    var xDoSomething = mIO.Instat_Read.Byte;
                    mIO.AttributeController_Index.Byte = i;
                    mIO.AttributeController_Write.Byte = registers[xIdx];
                    xIdx++;
                }
                /* lock 16-color palette and unblank display */
                var xNothing = mIO.Instat_Read.Byte;
                mIO.AttributeController_Index.Byte = 0x20;
            }
            #endregion

            #region 320x200x8
            public void SetMode320x200x8()
            {
                WriteVGARegisters(g_320x200x256);
                Height = 200;
                Width = 320;
                Colors = 256;
            }

            private static byte[] g_320x200x256 = new byte[]
            {
                /* MISC */
                0x63,
                /* SEQ */
                0x03, 0x01, 0x0F, 0x00, 0x0E,
                /* CRTC */
                0x5F, 0x4F, 0x50, 0x82, 0x54, 0x80, 0xBF, 0x1F,
                0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x9C, 0x0E, 0x8F, 0x28,	0x40, 0x96, 0xB9, 0xA3,
                0xFF,
                /* GC */
                0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x05, 0x0F,
                0xFF,
                /* AC */
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x41, 0x00, 0x0F, 0x00,	0x00
            };
            #endregion

            #region Palette Operations
            public void SetPaletteEntry(int index, System.Drawing.Color color)
            {
                SetPaletteEntry(index, color.R, color.G, color.B);
            }

            public void SetPalette(int index, byte[] pallete)
            {
                mIO.DACIndex_Write.Byte = (byte)index;
                for (int i = 0; i < pallete.Length; i++)
                    mIO.DAC_Data.Byte = (byte)(pallete[i] >> 2);
            }

            public void SetPaletteEntry(int index, byte r, byte g, byte b)
            {
                mIO.DACIndex_Write.Byte = (byte)index;
                mIO.DAC_Data.Byte = (byte)(r >> 2);
                mIO.DAC_Data.Byte = (byte)(g >> 2);
                mIO.DAC_Data.Byte = (byte)(b >> 2);
            }
            #endregion

        }
        #endregion

        internal InternalVGAScreen s = new InternalVGAScreen();
        public Image BackBuffer = new Image(320, 200);
        private Monitor m = new Monitor(320, 200);

        public void Update()
        {
            //for (int i = 0; i < BackBuffer.Modified.Count; i++)
            //{
            //    Vec2 v = BackBuffer.Modified[i];
            //    uint color = BackBuffer.GetPixel((uint)v.X, (uint)v.Y);
            //    mIO.VGAMemoryBlock[(uint)(v.Y * 320) + (uint)v.X] = (byte)(BackBuffer.Data[((v.Y * BackBuffer.Width) + v.X)] & 0xFF);
            //}
            DrawCursor();
            m.CurrentDriver.Update(BackBuffer);
            // Now we need to restore what was behind the mouse.
            Vec2 v = new Vec2((int)GuessOS.MouseX, (int)GuessOS.MouseY);
            DrawImage(v, behindMouseImage);
        }

        #region DrawCursor
        /// <summary>
        /// This image contains everything that is behind the mouse.
        /// </summary>
        private Image behindMouseImage = new Image(4, 4); // This means max mouse size is 4x4
        private Image Mouse = new Image(4, 4);
        private void DrawCursor()
        {
            #region SaveBehindMouse
            behindMouseImage.SetPixel(0, 0, BackBuffer.GetPixel(GuessOS.MouseX, GuessOS.MouseY));
            behindMouseImage.SetPixel(1, 0, BackBuffer.GetPixel(GuessOS.MouseX + 1, GuessOS.MouseY));
            behindMouseImage.SetPixel(2, 0, BackBuffer.GetPixel(GuessOS.MouseX + 2, GuessOS.MouseY));
            behindMouseImage.SetPixel(3, 0, BackBuffer.GetPixel(GuessOS.MouseX + 3, GuessOS.MouseY));
            behindMouseImage.SetPixel(0, 1, BackBuffer.GetPixel(GuessOS.MouseX, GuessOS.MouseY + 1));
            behindMouseImage.SetPixel(1, 1, BackBuffer.GetPixel(GuessOS.MouseX + 1, GuessOS.MouseY + 1));
            behindMouseImage.SetPixel(2, 1, BackBuffer.GetPixel(GuessOS.MouseX + 2, GuessOS.MouseY + 1));
            behindMouseImage.SetPixel(3, 1, BackBuffer.GetPixel(GuessOS.MouseX + 3, GuessOS.MouseY + 1));
            behindMouseImage.SetPixel(0, 2, BackBuffer.GetPixel(GuessOS.MouseX, GuessOS.MouseY + 2));
            behindMouseImage.SetPixel(1, 2, BackBuffer.GetPixel(GuessOS.MouseX + 1, GuessOS.MouseY + 2));
            behindMouseImage.SetPixel(2, 2, BackBuffer.GetPixel(GuessOS.MouseX + 2, GuessOS.MouseY + 2));
            behindMouseImage.SetPixel(3, 2, BackBuffer.GetPixel(GuessOS.MouseX + 3, GuessOS.MouseY + 2));
            behindMouseImage.SetPixel(0, 3, BackBuffer.GetPixel(GuessOS.MouseX, GuessOS.MouseY + 3));
            behindMouseImage.SetPixel(1, 3, BackBuffer.GetPixel(GuessOS.MouseX + 1, GuessOS.MouseY + 3));
            behindMouseImage.SetPixel(2, 3, BackBuffer.GetPixel(GuessOS.MouseX + 2, GuessOS.MouseY + 3));
            behindMouseImage.SetPixel(3, 3, BackBuffer.GetPixel(GuessOS.MouseX + 3, GuessOS.MouseY + 3));
            #endregion

            #region Draw Mouse
            Vec2 v = new Vec2((int)GuessOS.MouseX, (int)GuessOS.MouseY);
            DrawImage(v, Mouse);
            #endregion
        }
        #endregion

        public VGAScreenExtra()
        {
            s.Clear(1);
            

            // Now we need to setup the mouse
            // The color 255 is black, and 1 is white.
            Mouse.SetPixel(0, 0, 255);
            Mouse.SetPixel(1, 0, 255);
            Mouse.SetPixel(2, 0, 255);
            Mouse.SetPixel(3, 0, 255);
            Mouse.SetPixel(0, 1, 255);
            Mouse.SetPixel(1, 1, 1);
            Mouse.SetPixel(2, 1, 1);
            Mouse.SetPixel(3, 1, 255);
            Mouse.SetPixel(0, 2, 255);
            Mouse.SetPixel(1, 2, 1);
            Mouse.SetPixel(2, 2, 1);
            Mouse.SetPixel(3, 2, 255);
            Mouse.SetPixel(0, 3, 255);
            Mouse.SetPixel(1, 3, 255);
            Mouse.SetPixel(2, 3, 255);
            Mouse.SetPixel(3, 3, 255);

            //this.DrawCircleOutline(new Vec2(150, 100), 20, 8);
            //this.DrawTriangle(new Vec2(50, 50), new Vec2(150,50), new Vec2(100,150), 8);
        }

        public void SetPixel(uint x, uint y, uint c)
        {
            BackBuffer.SetPixel(x, y, c);
        }

        public void GetPixel(uint x, uint y)
        {
            BackBuffer.GetPixel(x, y);
        }

        public void Clear(uint c)
        {
            BackBuffer.Clear(c);
        }

        //public void WriteToConsole(string s)
        //{
        //    BackBuffer.WriteToConsole(s);
        //}

        #region DrawPolygon
        /// <summary>
        /// Draws a polygon with the specified points.
        /// </summary>
        /// <param name="points">The points of the polygon.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawPolygon(Vec2[] points, uint color)
        {
            BackBuffer.DrawPolygon(points, color);
        }
        #endregion

        #region DrawPolygonOutline
        /// <summary>
        /// Draws the outline of a polygon.
        /// The last point connects to the first point.
        /// </summary>
        /// <param name="points">An array containing the points to draw.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawPolygonOutline(Vec2[] points, uint color)
        {
            BackBuffer.DrawPolygonOutline(points, color);
        }
        #endregion

        #region DrawImage
        /// <summary>
        /// Draws the specified image, at the specified point.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="i"></param>
        public void DrawImage(Vec2 loc, Image i)
        {
            BackBuffer.DrawImage(loc, i);
        }
        #endregion

        #region DrawTriangle
        /// <summary>
        /// Draws a triangle and fills it in.
        /// </summary>
        /// <param name="p1">The first point of the triangle.</param>
        /// <param name="p2">The second point of the triangle.</param>
        /// <param name="p3">The third point of the triangle.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawTriangle(Vec2 p1, Vec2 p2, Vec2 p3, uint color)
        {
            BackBuffer.DrawTriangle(p1, p2, p3, color);
        }

        /// <summary>
        /// Draws a triangle with the specified fill color, and the specified border color.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="FillColor">The color to fill the tirangle with.</param>
        /// <param name="BorderColor">The color to draw the border of the triangle.</param>
        public void DrawTriangle(Vec2 p1, Vec2 p2, Vec2 p3, uint FillColor, uint BorderColor)
        {
            BackBuffer.DrawTriangle(p1, p2, p3, FillColor);
            BackBuffer.DrawTriangleOutline(p1, p2, p3, BorderColor);
        }
        #endregion

        #region DrawTriangleOutline
        /// <summary>
        /// Draw a triangle's outline.
        /// </summary>
        /// <param name="p1">The first point of the triangle.</param>
        /// <param name="p2">The second point of the triangle.</param>
        /// <param name="p3">The third point of the triangle.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawTriangleOutline(Vec2 p1, Vec2 p2, Vec2 p3, uint color)
        {
            BackBuffer.DrawLines(new Vec2[] { p1, p2, p3, p1 }, color);
        }
        #endregion

        #region DrawElipse
        /// <summary>
        /// Draws and fills an elipse.
        /// </summary>
        /// <param name="CenterPoint">The center of the elipse</param>
        /// <param name="height">The height of the elipse.</param>
        /// <param name="width">The width of the elipse.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawElipse(Vec2 CenterPoint, int height, int width, uint color)
        {
            BackBuffer.DrawElipse(CenterPoint, height, width, color);
        }

        /// <summary>
        /// Draws and fills an elipse.
        /// </summary>
        /// <param name="CenterPoint">The center of the elipse</param>
        /// <param name="height">The height of the elipse.</param>
        /// <param name="width">The width of the elipse.</param>
        /// <param name="fillColor">The color to fill in.</param>
        /// <param name="borderColor">The color to draw the border in.</param>
        public void DrawElipse(Vec2 CenterPoint, int height, int width, uint fillColor, uint borderColor)
        {
            BackBuffer.DrawElipse(CenterPoint, height, width, fillColor);
            BackBuffer.DrawElipseOutline(CenterPoint, height, width, borderColor);
        }
        #endregion

        #region DrawElipseOutline
        /// <summary>
        /// Draws an elipse outline.
        /// </summary>
        /// <param name="CenterPoint">The center of the elipse</param>
        /// <param name="height">The height of the elipse.</param>
        /// <param name="width">The width of the elipse.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawElipseOutline(Vec2 CenterPoint, int height, int width, uint color)
        {
            BackBuffer.DrawElipticalArc(CenterPoint, height, width, 0, 360, color);
        }
        #endregion

        #region DrawElipticalArc
        /// <summary>
        /// Draws an eliptical arc.
        /// </summary>
        /// <param name="CenterPoint">The center-point of the elipse to use.</param>
        /// <param name="height">The height of the elipse to use.</param>
        /// <param name="width">The width of the elipse to use.</param>
        /// <param name="startAngle">The angle to start drawing at.</param>
        /// <param name="endAngle">The angle to stop drawing at.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawElipticalArc(Vec2 CenterPoint, int height, int width, int startAngle, int endAngle, uint color)
        {
            BackBuffer.DrawElipticalArc(CenterPoint, height, width, startAngle, endAngle, color);
        }
        #endregion

        #region DrawCircle
        /// <summary>
        /// Draws a filled circle.
        /// </summary>
        /// <param name="Center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawCircle(Vec2 Center, int radius, uint color)
        {
            BackBuffer.DrawCircle(Center, radius, color);
        }
        #endregion

        #region DrawCircleOutline
        /// <summary>
        /// Draws the outline of a circle.
        /// </summary>
        /// <param name="Center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color to draw with.</param>
        public void DrawCircleOutline(Vec2 Center, int radius, uint color)
        {
            BackBuffer.DrawCircleOutline(Center, radius, color);
        }
        #endregion

        #region DrawLine
        /// <summary>
        /// Draws a line between 2 points.
        /// </summary>
        /// <param name="Point1">The first point.</param>
        /// <param name="Point2">The first point.</param>
        /// <param name="color">The color to draw.</param>
        public void DrawLine(Vec2 Point1, Vec2 Point2, uint color)
        {
            BackBuffer.DrawLine(Point1, Point2, color);
        }
        #endregion

        #region DrawLines
        /// <summary>
        /// Draws a set of connected lines.
        /// </summary>
        /// <param name="Points">
        /// An array of points to draw, in the order they need drawing.
        /// </param>
        /// <param name="color">The color to draw.</param>
        public void DrawLines(Vec2[] Points, uint color)
        {
            BackBuffer.DrawLines(Points, color);
        }
        #endregion

        #region DrawRectangle
        /// <summary>
        /// Draws a rectangle with the specified points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="p4">The fourth point.</param>
        /// <param name="FillColor">The color to fill in the rectangle with.</param>
        /// <param name="BorderColor">The color to draw the border in.</param>
        public void DrawRectangle(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4, uint FillColor, uint BorderColor)
        {
            BackBuffer.DrawPolygon(new Vec2[] { p1, p2, p3, p4 }, FillColor);
            BackBuffer.DrawPolygonOutline(new Vec2[] { p1, p2, p3, p4 }, BorderColor);
        }

        /// <summary>
        /// Draws a rectangle with the specified points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="p4">The fourth point.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawRectangle(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4, uint color)
        {
            BackBuffer.DrawPolygon(new Vec2[] { p1, p2, p3, p4 }, color);
        }

        /// <summary>
        /// This method fills in the space between
        /// the 2 points specified in a rectangle.
        /// </summary>
        /// <param name="TopLeftCorner">
        /// The point that specifies the top left 
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="BottomRightCorner">
        /// The point that specifies the bottom right
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="color">The color to draw.</param>
        public void DrawRectangle(Vec2 TopLeftCorner, Vec2 BottomRightCorner, uint color)
        {
            BackBuffer.DrawRectangle(TopLeftCorner, new Vec2(BottomRightCorner.X, TopLeftCorner.Y), BottomRightCorner, new Vec2(TopLeftCorner.X, BottomRightCorner.Y), color);
        }
        #endregion

        #region DrawReverseRectangle
        /// <summary>
        /// This method fills in the space between
        /// the 2 points specified in a rectangle.
        /// </summary>
        /// <param name="TopRightCorner">
        /// The point that specifies the top right 
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="BottomLeftCorner">
        /// The point that specifies the bottom left
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="color">The color to draw.</param>
        public void ReverseDrawRectangle(Vec2 TopRightCorner, Vec2 BottomLeftCorner, uint color)
        {
            BackBuffer.DrawRectangle(new Vec2(BottomLeftCorner.X, TopRightCorner.Y), TopRightCorner, new Vec2(TopRightCorner.X, BottomLeftCorner.Y), BottomLeftCorner, color);
        }
        #endregion

    }
}
