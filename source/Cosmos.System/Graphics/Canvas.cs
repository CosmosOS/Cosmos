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
            this.mode = mode;
            aviableModes = getAviableModes();
            defaultGraphicMode = getDefaultGraphicMode();
        }

        protected Canvas() : this(defaultGraphicMode)
        {
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
        public void Clear(Color color)
        {
            for (uint x = 0; x < mode.Rows; x++)
            {
                for (uint y = 0; y < mode.Columns; y++)
                {
                    DrawPoint(new Pen(color), x, y);
                }
            }
        }

        public abstract void DrawPoint(Pen pen, int x, int y);

        public abstract void DrawPoint(Pen pen, float x, float y);

        public void DrawLine(Pen pen, int x_start, int y_start, int x_end, int y_end)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Pen pen, float x_start, float y_start, float x_end, float y_end)
        {
            throw new NotImplementedException();
        }

        public void DrawRectangle(Pen pen, int x_start, int y_start, int width, int height)
        {
            throw new NotImplementedException();
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

        protected bool IsModeValid(Mode mode)
        {
            /* This would have been the more "modern" version but LINQ is not working */
            //return aviableModes.Exists(element => element == mode);

            foreach(var elem in aviableModes)
            {
                if (elem == mode)
                    return true;
            }

            /* 'mode' was not in the 'aviableModes' List ==> 'mode' in NOT Valid */
            return false;
        }
    }
}
