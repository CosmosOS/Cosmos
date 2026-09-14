// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Interface for graphic devices.
/// </summary>
internal interface IGraphicDevice
{
    /// <summary>
    /// Initialize the graphic device.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Clear the screen with a solid color.
    /// </summary>
    /// <param name="color">ARGB color value.</param>
    void ClearScreen(uint color);

    /// <summary>
    /// Draw a single pixel.
    /// </summary>
    /// <param name="color">ARGB color value.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    void DrawPixel(uint color, int x, int y);

    /// <summary>
    /// Get the color of a single pixel.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>ARGB color value.</returns>
    uint GetPixel(int x, int y);

    /// <summary>
    /// Reads a block of pixels from video memory.
    /// </summary>
    /// <param name="sourceByteOffset">Byte offset in the frame buffer.</param>
    /// <param name="dest">Destination array.</param>
    /// <param name="destIndex">Index in destination array to start writing.</param>
    /// <param name="count">Number of pixels to read.</param>
    void GetVRAM(int sourceByteOffset, int[] dest, int destIndex, int count);

    /// <summary>
    /// Copy a buffer of pixels to a rectangular region.
    /// </summary>
    /// <param name="pixels">Pixel data as ARGB values.</param>
    /// <param name="x">Destination X coordinate.</param>
    /// <param name="y">Destination Y coordinate.</param>
    /// <param name="width">Width of the region in pixels.</param>
    /// <param name="height">Height of the region in pixels.</param>
    void CopyBuffer(ReadOnlyMemory<uint> pixels, int x, int y, int width, int height);

    /// <summary>
    /// Copy a buffer of pixels to a rectangular region.
    /// </summary>
    /// <param name="pixels">Pixel data as ARGB values (as int, common for image data).</param>
    /// <param name="x">Destination X coordinate.</param>
    /// <param name="y">Destination Y coordinate.</param>
    /// <param name="width">Width of the region in pixels.</param>
    /// <param name="height">Height of the region in pixels.</param>
    void CopyBuffer(ReadOnlyMemory<int> pixels, int x, int y, int width, int height);

    /// <summary>
    /// Swap the back buffer to the screen.
    /// </summary>
    void Swap();
}
