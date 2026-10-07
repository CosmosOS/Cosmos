// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Represents the color depth of the pixels of a graphics surface.
/// </summary>
public enum ColorDepth
{
    /// <summary>
    /// 4 bits per pixel; 16 colors.
    /// </summary>
    ColorDepth4 = 4,

    /// <summary>
    /// 8 bits per pixel; 256 colors.
    /// </summary>
    ColorDepth8 = 8,

    /// <summary>
    /// 16 bits per pixel; 65,536 colors.
    /// </summary>
    ColorDepth16 = 16,

    /// <summary>
    /// 24 bits per pixel; 16,777,216 colors.
    /// </summary>
    ColorDepth24 = 24,

    /// <summary>
    /// 32 bits per pixel; 16,777,216 colors with transparency (alpha values).
    /// </summary>
    ColorDepth32 = 32
}
