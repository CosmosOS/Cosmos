using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics
{
    /// <summary>
    /// A Multi-Image Image, 
    /// This clas was created specifically for 
    /// The Tiff File Format.
    /// </summary>
    public class MiImage : Shapes.Shape
    {
        private Image[] Images;
        private UInt32 CurIndx;
        /// <summary>
        /// The Active index of the MiImage,
        /// This is the index of the Image
        /// that gets drawn when Draw() is called.
        /// </summary>
        public UInt32 ActiveIndex
        {
            get
            {
                return CurIndx;
            }
            set
            {
                SetActiveIndex(value);
            }
        }

        /// <summary>
        /// The default contructor for a MiImage.
        /// </summary>
        /// <param name="init">The First Image to add to the MiImage.</param>
        public MiImage(Image init)
        {
            Images = new Image[] { init };
            CurIndx = 0;
        }

        /// <summary>
        /// Create a new MiImage from an Image Array.
        /// </summary>
        /// <param name="images">The image array to create the MiImage from.</param>
        public MiImage(Image[] images)
        {
            Images = new Image[images.Length];
            Array.Copy(images, Images, images.Length);
            CurIndx = 0;
        }

        /// <summary>
        /// Get the image at the specified index.
        /// </summary>
        /// <param name="indx">The index of the image to get.</param>
        /// <returns>The image at the specified Index.</returns>
        public Image GetImage(UInt32 indx)
        {
            if (indx < Images.Length)
                return Images[indx];
            throw new Exception("No Image at specified Index!");
        }

        /// <summary>
        /// Set's the active index to the specified value.
        /// </summary>
        /// <param name="indx">What to set the active index to.</param>
        public void SetActiveIndex(UInt32 indx)
        {
            if (indx < Images.Length)
                CurIndx = indx;
            throw new Exception("No Image at specified Index!");
        }

        /// <summary>
        /// Adds the specified image at the end of the MiImage.
        /// </summary>
        /// <param name="i">The image to add.</param>
        public void AppendImage(Image i)
        {
            Image[] tmp = new Image[Images.Length + 1];
            Array.Copy(Images, tmp, Images.Length);
            tmp[tmp.Length - 1] = i;
            Images = tmp;
        }

        public override void Draw()
        {
            Parent.DrawImage(new Vec2(base.X, base.Y), Images[CurIndx]);
        }
    }
}
