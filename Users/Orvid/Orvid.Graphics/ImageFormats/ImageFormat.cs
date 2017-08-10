using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    /// <summary>
    /// This class represents any Image format, 
    /// and should be the base type for all drivers.
    /// </summary>
    public abstract class ImageFormat
    {
        /// <summary>
        /// Save an image to the specified stream.
        /// </summary>
        /// <param name="i">The image to save.</param>
        /// <param name="dest">The Stream to write to.</param>
        public abstract void Save(Image i, Stream dest);
        /// <summary>
        /// Load an image from the specified stream.
        /// </summary>
        /// <param name="s">The Stream to load from.</param>
        /// <returns>The loaded Image.</returns>
        public abstract Image Load(Stream s);
    }
}
