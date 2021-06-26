using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Color depth. Available:
    /// <list>
    /// <item>4-bit color depth  - 16 total colors</item>
    /// <item>8-bit color depth  - 255 total colors</item>
    /// <item>16-bit color depth - 65535 total colorss</item>
    /// <item>24-bit color depth - 16,777,216 total colorss</item>
    /// <item>32-bit color depth - 16,777,216 of colors with transparency</item>
    /// </list>
    /// </summary>
    public enum ColorDepth
    {
        /// <summary>
        /// EGA 16 colors.
        /// </summary>
        ColorDepth4 = 4,   /* EGA 16 colors */

        /// <summary>
        /// VGA 256 colors.
        /// </summary>
        ColorDepth8 = 8,   /* VGA 256 colors */

        /// <summary>
        /// 65535 colors.
        /// </summary>
        ColorDepth16 = 16, /* 65535 colors */

        /// <summary>
        /// 16,777,216 of colors.
        /// </summary>
        ColorDepth24 = 24, /* 16,777,216 of colors */

        /// <summary>
        /// 16,777,216 of colors (with transparency).
        /// </summary>
        ColorDepth32 = 32  /* 16,777,216 of colors (with transparency). Use this when in doubt :-) */
    }
}
