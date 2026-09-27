// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// One display a canvas can drive, whatever put it there: a bus driver the
/// driver kit bound and published, or a framebuffer firmware handed over.
/// Public for the same reason <see cref="IBlockDevice"/> is — a driver
/// outside this assembly publishes one through the kit — and, like that one,
/// every member runs on whichever thread asked for the screen.
/// </summary>
public interface IGraphicDevice
{
    /// <summary>Width of the visible framebuffer in pixels.</summary>
    uint Width { get; }

    /// <summary>Height of the visible framebuffer in pixels.</summary>
    uint Height { get; }

    /// <summary>Bytes from the start of one scanline to the start of the next, which is not always the width times the pixel size.</summary>
    uint Pitch { get; }

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
    /// Fills <paramref name="count"/> pixels of video memory from
    /// <paramref name="startByteOffset"/> with <paramref name="value"/>, the
    /// bulk clear a scanline-at-a-time canvas needs.
    /// </summary>
    /// <param name="startByteOffset">Byte offset in the frame buffer.</param>
    /// <param name="count">Number of pixels to write.</param>
    /// <param name="value">ARGB color value.</param>
    void ClearVRAM(int startByteOffset, int count, int value);

    /// <summary>
    /// Swap the back buffer to the screen.
    /// </summary>
    void Swap();

    /// <summary>
    /// Gives the screen back, so the device stops scanning out this canvas's
    /// framebuffer and a later acquisition builds a fresh one against it.
    /// </summary>
    void Disable();
}
