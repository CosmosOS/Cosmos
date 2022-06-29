using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Color depth. Available:
    /// <list type="bullet">
    /// <item>ColorDepth4 - EGA 16 colors</item>
    /// <item>ColorDepth8 - VGA 256 colors</item>
    /// <item>ColorDepth16 - 65535 colors</item>
    /// <item>ColorDepth24 - 16,777,216 of colors</item>
    /// <item>ColorDepth32 - 16,777,216 of colors (with transparency)</item>
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
