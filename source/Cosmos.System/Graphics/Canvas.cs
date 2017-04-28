//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Cosmos.System.Graphics
{
    public abstract class Canvas
    {
        protected Mode mode;
        protected List<Mode> aviableModes;
        static protected Mode defaultGraphicMode;

        protected Canvas(Mode mode)
        {
            //Global.mDebugger.SendInternal($"Creating a new Canvas with Mode ${mode}");

            aviableModes = getAviableModes();
            defaultGraphicMode = getDefaultGraphicMode();
            this.mode = mode;
        }

        protected Canvas()
        {
            Global.mDebugger.SendInternal($"Creating a new Canvas with default graphic Mode");

            aviableModes = getAviableModes();
            defaultGraphicMode = getDefaultGraphicMode();
            this.mode = defaultGraphicMode;
        }

        abstract public List<Mode> getAviableModes();

        abstract protected Mode getDefaultGraphicMode();

        public List<Mode> AviableModes
        {
            get
            {
                return aviableModes;
            }
        }

        public Mode DefaultGraphicMode
        {
            get
            {
                return defaultGraphicMode;
            }
        }

        public abstract Mode Mode
        {
            get;
            set;
        }

        /* Clear all the Canvas with the Black color */
        public void Clear()
        {
            Clear(Color.Black);
        }

        /*
         * Clear all the Canvas with the specified color. Please note that it is a very naïve implementation and any
         * driver should replace it (or with an hardware command or if not possible with a block copy on the IoMemoryBlock)
         */
        public virtual void Clear(Color color)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {color}");
            //if (color == null)
            //    throw new ArgumentNullException(nameof(color));

            Pen pen = new Pen(color);
            for (int x = 0; x < mode.Rows; x++)
            {
                for (int y = 0; y < mode.Columns; y++)
                {
                    DrawPoint(pen, x, y);
                }
            }
        }

        public abstract void DrawPoint(Pen pen, int x, int y);

        public abstract void DrawPoint(Pen pen, float x, float y);

        private void DrawHorizontalLine(Pen pen, int dx, int x1, int y1)
        {
            int i;

            for (i = 0; i < dx; i++)
                DrawPoint(pen, x1 + i, y1);
        }

        private void DrawVerthicalLine(Pen pen, int dy, int x1, int y1)
        {
            int i;

            for (i = 0; i < dy; i++)
                DrawPoint(pen, x1, y1 + i);
        }

        /*
         * To draw a diagonal line we use the fast version of the Bresenham's algorithm.
         * See http://www.brackeen.com/vga/shapes.html#4 for more informations.
         */
        private void DrawDiagonalLine(Pen pen, int dx, int dy, int x1, int y1)
        {
            int i, sdx, sdy, dxabs, dyabs, x, y, px, py;

            dxabs = Math.Abs(dx);
            dyabs = Math.Abs(dy);
            sdx = Math.Sign(dx);
            sdy = Math.Sign(dy);
            x = dyabs >> 1;
            y = dxabs >> 1;
            px = x1;
            py = y1;

            if (dxabs >= dyabs) /* the line is more horizontal than vertical */
            {
                for (i = 0; i < dxabs; i++)
                {
                    y += dyabs;
                    if (y >= dxabs)
                    {
                        y -= dxabs;
                        py += sdy;
                    }
                    px += sdx;
                    DrawPoint(pen, px, py);
                }
            }
            else /* the line is more vertical than horizontal */
            {
                for (i = 0; i < dyabs; i++)
                {
                    x += dxabs;
                    if (x >= dyabs)
                    {
                        x -= dyabs;
                        px += sdx;
                    }
                    py += sdy;
                    DrawPoint(pen, px, py);
                }
            }
        }

        /*
         * DrawLine throw if the line goes out of the boundary of the Canvas, probably will be better to draw only the part
         * of line visibile. This is too "smart" to do here better do it in a future Window Manager.
         */
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            if (pen == null)
                throw new ArgumentOutOfRangeException(nameof(pen));

            ThrowIfCoordNotValid(x1, y1);

            ThrowIfCoordNotValid(x2, y2);

            int dx, dy;

            dx = x2 - x1;      /* the horizontal distance of the line */
            dy = y2 - y1;      /* the vertical distance of the line */

            if (dy == 0) /* The line is horizontal */
            {
                DrawHorizontalLine(pen, dx, x1, y1);
                return;
            }

            if (dx == 0) /* the line is vertical */
            {
                DrawVerthicalLine(pen, dy, x1, y1);
                return;
            }

            /* the line is neither horizontal neither vertical, is diagonal then! */
            DrawDiagonalLine(pen, dx, dy, x1, y1);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            throw new NotImplementedException();
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            /*
             * we must draw four lines connecting any vertex of our rectangle to do this we first obtain the position of these
             * vertex (we call these vertexes A, B, C, D as for geometric convention)
             */
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

            /* The check of the validity of x and y are done in DrawLine() */

            /* The vertex A is where x,y are */
            int xa = x;
            int ya = y;

            /* The vertex B has the same y coordinate of A but x is moved of width pixels */
            int xb = x + width;
            int yb = y;

            /* The vertex C has the same x coordiate of A but this time is y that is moved of height pixels */
            int xc = x;
            int yc = y + height;

            /* The Vertex D has x moved of width pixels and y moved of height pixels */
            int xd = x + width;
            int yd = y + height;

            /* Draw a line betwen A and B */
            DrawLine(pen, xa, ya, xb, yb);

            /* Draw a line between A and C */
            DrawLine(pen, xa, ya, xc, yc);

            /* Draw a line between B and D */
            DrawLine(pen, xb, yb, xd, yd);

            /* Draw a line between C and D */
            DrawLine(pen, xc, yc, xd, yd);
        }

        public void DrawRectangle(Pen pen, float x_start, float y_start, float width, float height)
        {
            throw new NotImplementedException();
        }

        public void DrawImage(Image image, int x, int y)
        {
            throw new NotImplementedException();
        }

        public void DrawString(String str, Font aFont, Brush brush, int x, int y)
        {
            throw new NotImplementedException();
        }

        protected bool CheckIfModeIsValid(Mode mode)
        {
            Global.mDebugger.SendInternal($"CheckIfModeIsValid");

            if (mode == null)
                return false;

            foreach (var elem in aviableModes)
            {
                if (elem == mode)
                {
                    return true; // All OK mode does exists in aviableModes
                }
            }

            return false;
        }

        protected void ThrowIfModeIsNotValid(Mode mode)
        {
            if (mode == null)
            {
                Global.mDebugger.SendInternal($"mode is null raising exception!");
                throw new ArgumentNullException(nameof(mode));
            }
#if false
            /* This would have been the more "modern" version but LINQ is not working */
            if (!aviableModes.Exists(element => element == mode))
                throw new ArgumentOutOfRangeException($"Mode {mode} is not supported by this Driver");
#endif

            foreach (var elem in aviableModes)
            {
                if (elem == mode)
                {
                    Global.mDebugger.SendInternal($"mode {mode} found");
                    return; // All OK mode does exists in aviableModes
                }
            }

            Global.mDebugger.SendInternal($"foreach ended mode is not found! Raising exception...");
            /* 'mode' was not in the 'aviableModes' List ==> 'mode' in NOT Valid */
            throw new ArgumentOutOfRangeException(nameof(mode), $"Mode {mode} is not supported by this Driver");
        }

        protected void ThrowIfCoordNotValid(int x, int y)
        {
            if (x < 0 || x >= Mode.Columns)
            {
                throw new ArgumentOutOfRangeException(nameof(x),$"x ({x}) is not between 0 and {Mode.Columns}");
            }

            if (y < 0 || y >= Mode.Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"y ({y}) is not between 0 and {Mode.Rows}");
            }
        }
    }
}
