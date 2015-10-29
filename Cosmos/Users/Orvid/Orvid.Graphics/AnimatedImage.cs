using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics
{
    public class AnimatedImage : Shapes.Shape, IDisposable
    {
        public Image[] Frames;
        private UInt32 curFrameIndex;
        private bool loop = false;
        public int Height { get { return Frames[0].Height; } }
        public int Width { get { return Frames[0].Width; } }

        /// <summary>
        /// This is in Milli-Seconds
        /// </summary>
        public int TimePerFrame { get; set; }

        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                loop = value;
            }
        }

        public UInt32 CurFrameIndex
        {
            get
            {
                return curFrameIndex;
            }
            set
            {
                SetCurrrentFrameIndex(value);
            }
        }

        public Image this[UInt32 val]
        {
            get
            {
                return GetFrame(val);
            }
            set
            {
                SetFrame(val, value);
            }
        }

        public void SetFrame(UInt32 indx, Image i)
        {
            if (indx < Frames.Length)
                Frames[indx] = i;
            throw new Exception("Specified Frame Doesn't Exist!");
        }

        public AnimatedImage()
        {
            Frames = new Image[0];
        }

        public AnimatedImage(Image[] images)
        {
            Frames = new Image[images.Length];
            Array.Copy(images, Frames, images.Length);
            curFrameIndex = 0;
        }

        public Image GetFrame(UInt32 indx)
        {
            if (indx < Frames.Length)
                return Frames[indx];
            throw new Exception("Specified Frame Doesn't Exist!");
        }

        public void AddFrame(Image i)
        {
            Image[] tmp = new Image[Frames.Length + 1];
            Array.Copy(Frames, tmp, Frames.Length);
            tmp[tmp.Length - 1] = i;
            Frames = tmp;
            curFrameIndex = 1;
        }

        public void SetCurrrentFrameIndex(uint v)
        {
            if (v < Frames.Length - 1)
                curFrameIndex = v;
            else
                throw new Exception("Specified Frame Non-Existant!");
        }

        public override void Draw()
        {
            Parent.Clear(new Pixel(true));
            Parent.DrawImage(new Vec2(this.X, this.Y), Frames[curFrameIndex]);

            if (curFrameIndex + 2 <= Frames.Length - 1)
            {
                curFrameIndex++;
                curFrameIndex++;
            }
            else// if (loop)
            {
                curFrameIndex = 0;
            }

            this.Modified = true;
        }

        public void Dispose()
        {
            this.Frames = null;
        }
    }
}
