using System;
using Orvid.Graphics;
using Forms = System.Windows.Forms;

namespace TestBed
{

    internal class ExitButton
    {
        #region Events
        public event ObjectClick Click;
        public event ObjectClick MouseEnter;
        public event ObjectClick MouseLeave;
        public event ObjectClick MouseDown;
        public event ObjectClick MouseUp;
        #endregion

        public int X;
        public int Y;
        public Vec2 Size;
        private Vec2 iSize;
        private Image i;
        public Image parent;
        public Forms.Form parForm;
        public ObjectEvents evnts;
        public BoundingBox bounds;

        public ExitButton(Vec2 loc, Vec2 size, Image parent, Forms.Form parf)
        {
            if (size.X == 0 || size.Y == 0)
            {
                throw new Exception("No dimention of size can be zero!");
            }
            this.X = loc.X;
            this.Y = loc.Y;
            this.Size = size;
            this.iSize = new Vec2(size.X - 1, size.Y - 1);
            this.parent = parent;
            this.parForm = parf;
            this.bounds = new BoundingBox(this.X, this.X + Size.X, this.Y + Size.Y, this.Y);
            Click = new ObjectClick(this.ExitButtonClicked);
            MouseEnter = new ObjectClick(this.ExitButtonEnter);
            MouseLeave = new ObjectClick(this.ExitButtonLeave);
            MouseDown = new ObjectClick(this.ExitButtonMouseDown);
            MouseUp = new ObjectClick(this.ExitButtonMouseUp);
            evnts = new ObjectEvents(
                new ObjectClick(Click),
                new ObjectClick(MouseEnter),
                new ObjectClick(MouseLeave),
                new ObjectClick(MouseDown),
                new ObjectClick(MouseUp),
                new DrawMethod(Draw),
                bounds);
            i = new Image(size);
            this.DrawDefault();
        }




        public void Draw()
        {
            parent.DrawImage(new Vec2(X, Y), i);
        }

        #region Event Methods

        private void ExitButtonMouseUp(object sender, Forms.MouseEventArgs e)
        {
            if (bounds.IsInBounds(new Vec2(e.X, e.Y)))
            {
                this.DrawMouseOver();
            }
            else
            {
                this.DrawDefault();
            }
        }

        private void ExitButtonMouseDown(object sender, Forms.MouseEventArgs e)
        {
            this.DrawMouseDown();
        }

        private void ExitButtonClicked(object sender, Forms.MouseEventArgs e)
        {
            parForm.Close();
        }

        private void ExitButtonEnter(object sender, Forms.MouseEventArgs e)
        {
            this.DrawMouseOver();
        }

        private void ExitButtonLeave(object sender, Forms.MouseEventArgs e)
        {
            this.DrawDefault();
        }
        #endregion

        #region Draw Methods
        public Pixel OverOutline = Colors.Green;
        public Pixel OverFill = Colors.Beige;
        public Pixel DownOutline = Colors.Green;
        public Pixel DownFill = Colors.Blue;
        public Pixel NormalOutline = Colors.Green;
        public Pixel NormalFill = Colors.Red;

        private void DrawMouseDown()
        {
            i.DrawRectangle(new Vec2(0, 0), new Vec2(iSize.X, Size.Y), DownFill);
            i.DrawPolygonOutline(new Vec2[] { 
                new Vec2(0,0),
                new Vec2(iSize.X,0),
                new Vec2(iSize.X,iSize.Y),
                new Vec2(0,iSize.Y) }, DownOutline);
            i.DrawLine(new Vec2(0, 0), new Vec2(iSize.X, iSize.Y), DownOutline);
            i.DrawLine(new Vec2(0, iSize.Y), new Vec2(iSize.X, 0), DownOutline);
            this.Draw();
        }

        private void DrawMouseOver()
        {
            i.DrawRectangle(new Vec2(0, 0), new Vec2(iSize.X, Size.Y), OverFill);
            i.DrawPolygonOutline(new Vec2[] { 
                new Vec2(0,0),
                new Vec2(iSize.X,0),
                new Vec2(iSize.X,iSize.Y),
                new Vec2(0,iSize.Y) }, OverOutline);
            i.DrawLine(new Vec2(0, 0), new Vec2(iSize.X, iSize.Y), OverOutline);
            i.DrawLine(new Vec2(0, iSize.Y), new Vec2(iSize.X, 0), OverOutline);
            this.Draw();
        }

        private void DrawDefault()
        {
            i.DrawRectangle(new Vec2(0, 0), new Vec2(iSize.X, iSize.Y), NormalFill);
            i.DrawPolygonOutline(new Vec2[] { 
                new Vec2(0,0),
                new Vec2(iSize.X,0),
                new Vec2(iSize.X,iSize.Y),
                new Vec2(0,iSize.Y) }, NormalOutline);
            i.DrawLine(new Vec2(0, 0), new Vec2(iSize.X, iSize.Y), NormalOutline);
            i.DrawLine(new Vec2(0, iSize.Y), new Vec2(iSize.X, 0), NormalOutline);
            this.Draw();
        }
        #endregion

    }

}
