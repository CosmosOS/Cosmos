// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Describes the order in which the colors are stored in each pixel of an image.
/// </summary>
public enum ColorOrder
{
    /// <summary>
    /// The color order is as follows: Red, Green, Blue.
    /// </summary>
    RGB,

    /// <summary>
    /// The color order is as follows: Blue, Green, Red.
    /// </summary>
    BGR
}
