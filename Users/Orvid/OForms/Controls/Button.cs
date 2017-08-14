using System;
using Orvid.Graphics;

namespace OForms.Controls
{
    /// <summary>
    /// A button.
    /// </summary>
    internal class Button : Control
    {
        /// <summary>
        /// The internal size of the button. This is slighly smaller
        /// than the size that was specified, as the buffer has a base of 0,
        /// where-as the size has a base of 1.
        /// </summary>
        private Vec2 iSize;

        /// <summary>
        /// The default constructor for a button.
        /// </summary>
        /// <param name="loc">The location of the button in the parent control.</param>
        /// <param name="size">The size of the button.</param>
        public Button(Vec2 loc, Vec2 size)
            : base(loc, size)
        {
            if (size.X == 0 || size.Y == 0)
            {
                throw new Exception("No dimention of size can be zero!");
            }
            this.X = loc.X;
            this.Y = loc.Y;
            this.Size = size;
            this.iSize = new Vec2(size.X - 1, size.Y - 1);
            this.Bounds = new BoundingBox(this.X, this.X + Size.X, this.Y + Size.Y, this.Y);
            MouseEnter += new MouseEvent(this.ButtonEnter);
            MouseLeave += new MouseEvent(this.ButtonLeave);
            MouseDown += new MouseEvent(this.ButtonMouseDown);
            MouseUp += new MouseEvent(this.ButtonMouseUp);
            Buffer = new Image(size);
            this.DrawDefault();
        }

        /// <summary>
        /// Draws the button on the specified image.
        /// </summary>
        /// <param name="i">The image to draw the button on.</param>
        public override void Draw(Image i)
        {
            i.DrawImage(new Vec2(X, Y), Buffer);
        }

        #region Event Methods

        private void ButtonMouseUp(Vec2 loc, MouseButtons buttons)
        {
            if (Bounds.IsInBounds(loc))
            {
                this.DrawMouseOver();
            }
            else
            {
                this.DrawDefault();
            }
        }

        private void ButtonMouseDown(Vec2 loc, MouseButtons buttons)
        {
            this.DrawMouseDown();
        }

        private void ButtonEnter(Vec2 loc, MouseButtons buttons)
        {
            if (IsMouseDown)
            {
                this.DrawMouseDown();
            }
            else
            {
                this.DrawMouseOver();
            }
        }

        private void ButtonLeave(Vec2 loc, MouseButtons buttons)
        {
            this.DrawDefault();
        }
        #endregion

        #region Draw Methods
        /// <summary>
        /// The outline of the button in the 'Over' state.
        /// </summary>
        public Pixel OverOutline = Colors.Aquamarine;
        /// <summary>
        /// The fill of the button in the 'Over' state.
        /// </summary>
        public Pixel OverFill = Colors.Crimson;
        /// <summary>
        /// The outline of the button in the 'Down' state.
        /// </summary>
        public Pixel DownOutline = Colors.Red;
        /// <summary>
        /// The fill of the button in the 'Down' state.
        /// </summary>
        public Pixel DownFill = Colors.Blue;
        /// <summary>
        /// The outline of the button in it's default state.
        /// </summary>
        public Pixel NormalOutline = Colors.Blue;
        /// <summary>
        /// The fill of the button in it's default state.
        /// </summary>
        public Pixel NormalFill = Colors.Red;

        /// <summary>
        /// Draws the button in the 'Down' state.
        /// </summary>
        private void DrawMouseDown()
        {
            Buffer.DrawRectangle(Vec2.Zero, new Vec2(iSize.X, Size.Y), DownFill);
            Buffer.DrawPolygonOutline(new Vec2[] { 
                Vec2.Zero,
                new Vec2(iSize.X,0),
                new Vec2(iSize.X,iSize.Y),
                new Vec2(0,iSize.Y) }, DownOutline);
        }

        /// <summary>
        /// Draws the button in the 'Over' state.
        /// </summary>
        private void DrawMouseOver()
        {
            Buffer.DrawRectangle(new Vec2(0, 0), new Vec2(iSize.X, Size.Y), OverFill);
            Buffer.DrawPolygonOutline(new Vec2[] { 
                new Vec2(0,0),
                new Vec2(iSize.X,0),
                new Vec2(iSize.X,iSize.Y),
                new Vec2(0,iSize.Y) }, OverOutline);
        }

        /// <summary>
        /// Draws the button in it's default state.
        /// </summary>
        private void DrawDefault()
        {
            Buffer.DrawRectangle(new Vec2(0, 0), new Vec2(iSize.X, iSize.Y), NormalFill);
            Buffer.DrawPolygonOutline(new Vec2[] { 
                new Vec2(0,0),
                new Vec2(iSize.X,0),
                new Vec2(iSize.X,iSize.Y),
                new Vec2(0,iSize.Y) }, NormalOutline);
        }
        #endregion

    }
}
