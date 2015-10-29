using System;

namespace Orvid.Graphics
{
    public class GraphicsMode
    {
        public readonly Resolution Resolution;
        public readonly ColorDepth ColorDepth;
        public GraphicsMode(Resolution resolution, ColorDepth depth)
        {
            this.Resolution = resolution;
            this.ColorDepth = depth;
        }

        public static bool operator !=(GraphicsMode a, GraphicsMode b)
        {
            if (a.ColorDepth != b.ColorDepth || a.Resolution != b.Resolution)
                return true;
            return false;
        }

        public static bool operator ==(GraphicsMode a,GraphicsMode b)
        {
            if (a.ColorDepth != b.ColorDepth || a.Resolution != b.Resolution)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            return (this == (GraphicsMode)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    #region Resolution
    /// <summary>
    /// The enum that contains all of the possible display resolutions.
    /// NOTE: 
    /// Just because they're here, doesn't mean they are implemented.
    /// </summary>
    public enum Resolution
    {
#pragma warning disable 1591
        // That's to keep the compiler balking about the missing xml comments

        Size160x120,
        Size240x160,
        Size320x200,
        Size320x240,
        Size320x400,
        Size360x480,
        Size400x600,
        Size432x240,
        Size480x272,
        Size480x320,
        Size480x360,
        Size640x240,
        Size640x350,
        Size640x360,
        Size640x400,
        Size640x480,
        Size800x480,
        Size800x600,
        Size854x480,
        Size960x540,
        Size1024x576,
        Size1024x600,
        Size1024x768,
        Size1152x864,
        Size1280x720,
        Size1280x768,
        Size1280x800,
        Size1280x960,
        Size1280x1024,
        Size1360x768,
        Size1366x768,
        Size1400x1050,
        Size1440x900,
        Size1600x768,
        Size1600x1200,
        Size1680x1050,
        Size1920x1200,
        Size2048x1152,
        Size2048x1536,
        Size2560x1080,
        Size2560x1440,
        Size2560x1600,
        Size2560x2048,
        Size3200x2048,
        Size3200x2400,
        Size3840x2160,
        Size3840x2400,
        Size4096x3072,
        Size5120x3200,
        Size5120x4096,
        Size6400x4096,
        Size6400x4800,
        Size7680x4800,

        // And turn the warning back on for the rest of the file.
#pragma warning restore 1591
    }
    #endregion

    #region Color Depth
    /// <summary>
    /// The enum that contains all of the possible color depths.
    /// </summary>
    public enum ColorDepth
    {
        /// <summary>
        /// This is black and white.
        /// (well it can actually be any 2 colors of your choice, but still)
        /// </summary>
        Bit2,
        /// <summary>
        /// This is the 4-bit color depth.
        /// </summary>
        Bit4,
        /// <summary>
        /// This is the 8-bit color depth, also known as 256 colors.
        /// </summary>
        Bit8,
        /// <summary>
        /// This is the 16-bit depth.
        /// It's enough to be able to view photo's without losing much visually.
        /// </summary>
        Bit16,
        /// <summary>
        /// This is the 32-bit depth. It is the highest available depth.
        /// </summary>
        Bit32,
        /// <summary>
        /// This color depth isn't ever used, but is here for future possibilities.
        /// </summary>
        Bit64
    }
    #endregion
}
