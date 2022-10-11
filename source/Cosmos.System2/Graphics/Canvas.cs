using Cosmos.System.Graphics.Extensions;
using Cosmos.HAL.Drivers.Video;
using System.Drawing;
using System;

namespace Cosmos.System.Graphics
{
    public class Canvas
    {
        /// <summary>
        /// Create new instance of <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="aWidth">Image width (greater then 0).</param>
        /// <param name="aHeight">Image height (greater then 0).</param>
        /// <param name="aColorDepth">Color depth.</param>
        public Canvas(uint aWidth, uint aHeight, ColorDepth aColorDepth)
        {
            Buffer = new uint[aWidth * aHeight];
            Mode = new(aWidth, aHeight, aColorDepth);
        }
        /// <summary>
        /// Create new instance of <see cref="Canvas"/> class.
        /// </summary>
        /// <param name="aMode">Image mode</param>
        public Canvas(Mode aMode)
        {
            Buffer = new uint[aMode.Width * aMode.Height];
            Mode = aMode;
        }
        /// <summary>
        /// Create new instance of <see cref="Canvas"/> class.
        /// </summary>
        public Canvas()
        {
            Mode = DefaultMode;
        }

        #region Fields

        /// <summary>
        /// Default video mode. 1024x768x32.
        /// </summary>
        public static readonly Mode DefaultMode = new(1024, 768, ColorDepth.ColorDepth32);
        /// <summary>
        /// The video driver.
        /// </summary>
        public VideoDriver Driver;
        /// <summary>
        /// Current mode set for the canvas.
        /// </summary>
        public Mode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                Scale(value.Width, value.Height);
                _Mode = value;
            }
        }
        private Mode _Mode;

        #endregion

        #region Pixel Methods

        public virtual void SetPixel(int aX, int aY, Color aColor)
        {
            if (aX > -1 && aY > -1 && aX < Mode.Width && aY < Mode.Height)
            {
                if (Mode.ColorDepth == ColorDepth.ColorDepth32)
                {
                    aColor = ColorEx.AlphaBlend(Color.FromArgb((int)GetIndex(aX, aY)), aColor);
                }

                Buffer[GetIndex(aX, aY)] = (uint)aColor.ToArgb();
            }
        }
        public virtual Color GetPixel(int aX, int aY)
        {
            if (aX > -1 && aY > -1 && aX < Mode.Width && aY < Mode.Height)
            {
                return Color.FromArgb((int)Buffer[GetIndex(aX, aY)]);
            }
            return Color.Black;
        }
        public uint GetIndex(int aX, int aY)
        {
            return (uint)(Mode.Width * aY + aX);
        }

        #endregion

        #region Line Methods

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

        public virtual void DrawFilledRectangle(int aX, int aY, uint aWidth, uint aHeight, Color aColor)
        {
            if (X == 0 && Y == 0 && Width == this.Width && Height == this.Height && Radius == 0 && Color.A == 255)
            {
                Clear(Color);
                return;
            }
            if (Radius == 0 && Color.A == 255)
            {
                if (X < 0)
                {
                    Width -= (uint)Math.Abs(X);
                    X = 0;
                }
                if (Y < 0)
                {
                    Height -= (uint)Math.Abs(Y);
                    Y = 0;
                }
                if (X + Width >= this.Width)
                {
                    Width -= (uint)X;
                }
                if (Y + Height >= this.Height)
                {
                    Height -= (uint)Y;
                }
                for (int IY = 0; IY < Mode.Height; IY++)
                {
                    Driver.Fill((uint)((aY + IY) * Mode.Width + aX), (uint)aColor.ToArgb(), aWidth);
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
        public virtual void DrawRectangle(int aX, int aY, uint aWidth, uint aHeight, Color aColor)
        {
            DrawLine(aX, aY, aX + (int)aWidth, aY, aColor); // Top
            DrawLine(aX, aY, aX, aY + (int)aHeight, aColor); // Left
            DrawLine(aX + (int)aWidth, aY + (int)aHeight, aX, aY + (int)aHeight, aColor); // Bottom
            DrawLine(aX + (int)aWidth, aY + (int)aHeight, aX + (int)aWidth, aY, aColor); // Right
        }

        #endregion

        #region Image

        public virtual void DrawImage(int aX, int aY, Canvas aImage)
        {
            if (aX == 0 && aY == 0 && aImage.Mode == Mode && aImage.Mode.ColorDepth == ColorDepth.ColorDepth24)
            {
                for (uint I = 0; I < aImage.Mode.Size; I++)
                {
                    Buffer[I] = aImage.Buffer[I];
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

        #region Misc

        // These are methods that need to be overridden by a base class.
        public virtual void Display()
        {
            throw new NotImplementedException();
        }
        public virtual void Disable()
        {
            throw new NotImplementedException();
        }
        public virtual void Update()
        {
            throw new NotImplementedException();
        }

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
                    FB.Buffer[FB.GetIndex((int)X, (int)Y)] = Buffer[GetIndex(IX, IY)];
                }
            }
            return FB;
        }
        public virtual void Clear(Color aColor)
        {
            Driver.Fill((uint)aColor.ToArgb());
        }

        #endregion
    }
}
