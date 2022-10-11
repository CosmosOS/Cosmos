using Cosmos.System.Graphics.Extensions;
using Cosmos.System.Graphics.Fonts;
using Cosmos.HAL.Drivers.Video;
using System.Drawing;
using Cosmos.HAL;
using System;

namespace Cosmos.System.Graphics
{
    public unsafe class Canvas
    {
        /// <summary>
        /// Create new instance of <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="aWidth">Image width (greater then 0).</param>
        /// <param name="aHeight">Image height (greater then 0).</param>
        /// <param name="aColorDepth">Color depth.</param>
        public Canvas(uint aWidth, uint aHeight, ColorDepth aColorDepth)
        {
            Driver.Width = aWidth;
            Driver.Height = aHeight;
            Driver.Depth = (byte)aColorDepth;
        }
        /// <summary>
        /// Create new instance of <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="aMode">Image mode</param>
        public Canvas(Mode aMode)
        {
            Driver = new(aMode.Width, aMode.Height, (byte)aMode.ColorDepth);
        }
        /// <summary>
        /// Create new instance of <see cref="Canvas"/> class.
        /// </summary>
        public Canvas()
        {
            Driver.Width = GetDefaultMode().Width;
            Driver.Height = GetDefaultMode().Height;
            Driver.Depth = (byte)GetDefaultMode().ColorDepth;
        }

        #region Fields

        /// <summary>
        /// The video driver, Contains the mode of the canvas.
        /// </summary>
        public VideoDriver Driver;

        #endregion

        #region Pixel Methods

        /// <summary>
        /// Sets a pixel at the specified location.
        /// Will use alpha-blending if the canvas depth is 32 bit.
        /// </summary>
        /// <param name="aX">X position to set the pixel.</param>
        /// <param name="aY">Y position to set the pixel.</param>
        /// <param name="aColor">Color for the pixel.</param>
        public virtual void SetPixel(int aX, int aY, Color aColor)
        {
            if (aX > -1 && aY > -1 && aX < Driver.Width && aY < Driver.Height)
            {
                if (Driver.Depth == (byte)ColorDepth.ColorDepth32)
                {
                    aColor = ColorEx.AlphaBlend(Color.FromArgb((int)GetIndex(aX, aY)), aColor);
                }

                Driver.Buffer[GetIndex(aX, aY)] = (uint)aColor.ToArgb();
            }
        }
        /// <summary>
        /// Gets the color of a pixel at a location.
        /// </summary>
        /// <param name="aX">Source X.</param>
        /// <param name="aY">Source Y.</param>
        /// <returns>Color of the pixel at point (X, Y)</returns>
        public virtual Color GetPixel(int aX, int aY)
        {
            if (aX > -1 && aY > -1 && aX < Driver.Width && aY < Driver.Height)
            {
                return Color.FromArgb((int)Driver.Buffer[GetIndex(aX, aY)]);
            }
            return Color.Black;
        }
        /// <summary>
        /// Gets the linear index of an X and Y value.
        /// </summary>
        /// <param name="aX">X position.</param>
        /// <param name="aY">Y position.</param>
        /// <returns>Index of X and Y in the linear buffer.</returns>
        public uint GetIndex(int aX, int aY)
        {
            return (uint)(Driver.Width * aY + aX);
        }

        #endregion

        #region Line Methods

        /// <summary>
        /// Draws a line from point A (aX1, aY1) to point B (aX2, aY2).
        /// </summary>
        /// <param name="aX1">X position 1.</param>
        /// <param name="aY1">Y position 1.</param>
        /// <param name="aX2">X position 2.</param>
        /// <param name="aY2">Y position 2.</param>
        /// <param name="aColor">Color to draw with.</param>
        /// <param name="aUseAntiAlias">Whether to use anti-aliasing.</param>
        public void DrawLine(int aX1, int aY1, int aX2, int aY2, Color aColor, bool aUseAntiAlias = false)
        {
            int DX = Math.Abs(aX2 - aX1), SX = aX1 < aX2 ? 1 : -1;
            int DY = Math.Abs(aY2 - aY1), SY = aY1 < aY2 ? 1 : -1;
            int err = (DX > DY ? DX : -DY) / 2;

            while (aX1 != aX2 || aY1 != aY2)
            {
                SetPixel(aX1, aY1, aColor);
                if (aUseAntiAlias)
                {
                    if (aX1 + aX2 > aY1 + aY2)
                    {
                        SetPixel(aX1 + 1, aY1, Color.FromArgb((byte)(aColor.A / 2), aColor.R, aColor.G, aColor.B));
                        SetPixel(aX1 + 1, aY1, Color.FromArgb((byte)(aColor.A / 2), aColor.R, aColor.G, aColor.B));
                    }
                    else
                    {
                        SetPixel(aX1, aY1 + 1, Color.FromArgb((byte)(aColor.A / 2), aColor.R, aColor.G, aColor.B));
                        SetPixel(aX1, aY1 - 1, Color.FromArgb((byte)(aColor.A / 2), aColor.R, aColor.G, aColor.B));
                    }
                }
                int e2 = err;
                if (e2 > -DX) { err -= DY; aX1 += SX; }
                if (e2 < DY) { err += DX; aY1 += SY; }
            }
        }

        #endregion

        #region Rectangle Methods

        /// <summary>
        /// Draws a filled rectangle at the specified coordinates.
        /// </summary>
        /// <param name="aX">X position of the rectangle.</param>
        /// <param name="aY">Y position of the rectangle.</param>
        /// <param name="aWidth">Width of the rectangle.</param>
        /// <param name="aHeight">Height of the rectangle.</param>
        /// <param name="aColor">Color used to draw the rectangle.</param>
        public virtual void DrawFilledRectangle(int aX, int aY, uint aWidth, uint aHeight, Color aColor)
        {
            if (aX == 0 && aY == 0 && aWidth == Driver.Width && aHeight == Driver.Height && aColor.A == 255)
            {
                Clear(aColor);
                return;
            }
            if (aColor.A == 255)
            {
                if (aX < 0)
                {
                    aWidth -= (uint)Math.Abs(aX);
                    aX = 0;
                }
                if (aY < 0)
                {
                    aHeight -= (uint)Math.Abs(aY);
                    aY = 0;
                }
                if (aX + aWidth >= Driver.Width)
                {
                    aWidth -= (uint)aX;
                }
                if (aY + aHeight >= Driver.Height)
                {
                    aHeight -= (uint)aY;
                }
                for (int IY = 0; IY < aHeight; IY++)
                {
                    Driver.Fill((uint)((aY + IY) * Driver.Width + aX), (uint)aColor.ToArgb(), aWidth);
                }
                return;
            }
            for (int IX = aX; IX < aX + aWidth; IX++)
            {
                for (int IY = aY; IY < aY + aHeight; IY++)
                {
                    SetPixel(IX, IY, aColor);
                }
            }
        }
        /// <summary>
        /// Draws a non-filled rectangle at the specified coordinates.
        /// </summary>
        /// <param name="aX">X position of the rectangle.</param>
        /// <param name="aY">Y position of the rectangle.</param>
        /// <param name="aWidth">Width of the rectangle.</param>
        /// <param name="aHeight">Height of the rectangle.</param>
        /// <param name="aColor">Color used to draw the rectangle.</param>
        public virtual void DrawRectangle(int aX, int aY, uint aWidth, uint aHeight, Color aColor)
        {
            DrawLine(aX, aY, aX + (int)aWidth, aY, aColor); // Top
            DrawLine(aX, aY, aX, aY + (int)aHeight, aColor); // Left
            DrawLine(aX + (int)aWidth, aY + (int)aHeight, aX, aY + (int)aHeight, aColor); // Bottom
            DrawLine(aX + (int)aWidth, aY + (int)aHeight, aX + (int)aWidth, aY, aColor); // Right
        }

        #endregion

        #region Image

        /// <summary>
        /// Draw image on the canvas.
        /// </summary>
        /// <param name="aX">X position to start drawing.</param>
        /// <param name="aY">Y position to start drawing.</param>
        /// <param name="aImage">Image to draw.</param>
        public virtual void DrawImage(int aX, int aY, Canvas aImage)
        {
            if (aX == 0 && aY == 0 && aImage.Mode == Mode && aImage.Mode.ColorDepth == ColorDepth.ColorDepth24)
            {
                for (uint I = 0; I < aImage.Mode.Size; I++)
                {
                    Driver.Buffer[I] = aImage.Driver.Buffer[I];
                }
            }

            for (uint X = 0; X < aImage.Mode.Width; X++)
            {
                for (uint Y = 0; Y < aImage.Mode.Height; Y++)
                {
                    SetPixel((int)(aX + X), (int)(aY + Y), aImage.GetPixel((int)X, (int)Y));
                }
            }
        }

        #endregion

        #region Text

        /// <summary>
        /// Draw string.
        /// </summary>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <param name="aText">string to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="aColor">Color.</param>
        public virtual void DrawString(int aX, int aY, string aText, Font aFont, Color aColor)
        {
            for (int i = 0; i < aText.Length; i++)
            {
                DrawChar(aX, aY, aText[i], aFont, aColor);
                aX += aFont.Width;
            }
        }

        /// <summary>
        /// Draw char.
        /// </summary>
        /// <param name="str">char to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="pen">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public virtual void DrawChar(int aX, int aY, char aChar, Font aFont, Color aColor)
        {
            int p = aFont.Height * (byte)aChar;

            for (int cy = 0; cy < aFont.Height; cy++)
            {
                for (byte cx = 0; cx < aFont.Width; cx++)
                {
                    if (aFont.ConvertByteToBitAddres(aFont.Data[p + cy], cx + 1))
                    {
                        SetPixel((ushort)(aX + (aFont.Width - cx)), (ushort)(aY + cy), aColor);
                    }
                }
            }
        }

        #endregion

        #region Misc

        /// <summary>
        /// Resizes the canvas, useful for images or in the canvas where needed.
        /// </summary>
        /// <param name="aWidth">New width to resize to.</param>
        /// <param name="aHeight">New height to resize to.</param>
        /// <returns>Resized version of the current canvas.</returns>
        public virtual Canvas Scale(uint aWidth, uint aHeight)
        {
            if (aWidth <= 0 || aHeight <= 0 || aWidth == Mode.Width || aHeight == Mode.Height)
            {
                return this;
            }

            Canvas FB = new(new Mode(aWidth, aHeight, Mode.ColorDepth));
            for (int IX = 0; IX < Mode.Width; IX++)
            {
                for (int IY = 0; IY < Mode.Height; IY++)
                {
                    long X = IX / (Mode.Width / aWidth);
                    long Y = IY / (Mode.Height / aHeight);
                    FB.Driver.Buffer[FB.GetIndex((int)X, (int)Y)] = Driver.Buffer[GetIndex(IX, IY)];
                }
            }
            return FB;
        }
        /// <summary>
        /// Gets the correct screen driver for the device.
        /// </summary>
        /// <param name="aMode">Video mode to set.</param>
        /// <returns>Correct video canvas.</returns>
        public static Canvas GetVideoDriver(Mode aMode)
        {
            if (PCI.Exists(VendorID.VMWare, DeviceID.SVGAIIAdapter))
            {
                return new SVGAIICanvas(aMode);
            }
            else if (VBEDriver.ISAModeAvailable())
            {
                return new VBECanvas(aMode);
            }
            else
            {
                return new VGACanvas(aMode);
            }
        }
        /// <summary>
        /// Update the canvas.
        /// </summary>
        public virtual void Update()
        {
            Driver.Update();
        }
        /// <summary>
        /// Clears the canvas with the specified color.
        /// </summary>
        /// <param name="aColor"></param>
        public virtual void Clear(Color aColor)
        {
            Driver.Fill((uint)aColor.ToArgb());
        }
        /// <summary>
        /// Gets the default mode of the canvas.
        /// </summary>
        /// <returns>Mode with 1024 width, 768 height, 32 bit depth.</returns>
        public virtual Mode GetDefaultMode()
        {
            return new(1024, 768, ColorDepth.ColorDepth32);
        }
        /// <summary>
        /// Return the name of the canvas version.
        /// </summary>
        /// <returns>Canvas name.</returns>
        public virtual string GetName()
        {
            return "Canvas";
        }

        #endregion
    }
}
