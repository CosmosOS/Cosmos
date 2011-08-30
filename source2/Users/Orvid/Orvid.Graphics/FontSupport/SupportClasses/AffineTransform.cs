using System;

namespace Orvid.Graphics.FontSupport.SupportClasses
{
    /// <summary>
    /// A struct representing an affine transform.
    /// </summary>
    public class AffineTransform
    {
        /// <summary>
        /// Scale X.
        /// </summary>
        public double ScaleX;
        /// <summary>
        /// Scale Y.
        /// </summary>
        public double ScaleY;
        /// <summary>
        /// Shear X.
        /// </summary>
        public double ShearX;
        /// <summary>
        /// Shear Y.
        /// </summary>
        public double ShearY;
        /// <summary>
        /// Translate X.
        /// </summary>
        public double TranslateX;
        /// <summary>
        /// Translate Y.
        /// </summary>
        public double TranslateY;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="scaleX">Scale X.</param>
        /// <param name="shearY">Shear Y.</param>
        /// <param name="shearX">Shear X.</param>
        /// <param name="scaleY">Scale Y.</param>
        /// <param name="translateX">Translate X.</param>
        /// <param name="translateY">Translate Y.</param>
        public AffineTransform(double scaleX, double shearY, double shearX, double scaleY, double translateX, double translateY)
        {
            this.ScaleX = scaleX;
            this.ScaleY = scaleY;
            this.ShearX = shearX;
            this.ShearY = shearY;
            this.TranslateX = translateX;
            this.TranslateY = translateY;
        }
    }
}
