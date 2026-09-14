// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Devices.Graphic;

/// <summary>
/// UEFI GOP Video Driver.
/// Provides video output via UEFI framebuffer.
/// </summary>
internal unsafe class GopDriver : GraphicDevice
{
    /// <summary>
    /// Returns true if the device was successfully initialized.
    /// </summary>
    public bool IsInitialized => _initialized;

    /// <summary>
    /// Frame buffer memory block.
    /// </summary>
    public MemoryBlock LinearFrameBuffer;
    //public MemoryBlock LinearFrameBuffer = new MemoryBlock(0xE0000000, 1024 * 768 * 4);

    protected readonly ManagedMemoryBlock lastbuffer;

    public uint Width;
    public uint Height;
    public uint Pitch;
    public uint Stride;
    private bool _initialized;

    public GopDriver(uint* baseAddress, uint width, uint height, uint pitch)
    {
        LinearFrameBuffer = new MemoryBlock((ulong)baseAddress, height * pitch);
        lastbuffer = new ManagedMemoryBlock(height * pitch);
        Width = width;
        Height = height;
        Pitch = pitch;
        Stride = 4; // Assuming 32bpp
    }

    private uint GetPointOffset(int x, int y)
    {
        return (uint)(x * Stride) + (uint)(y * Pitch);
    }

    /// <summary>
    /// Initializes the UEFI video device.
    /// </summary>
    public override void Initialize()
    {
        _initialized = true;
    }

    public override void DrawPixel(uint color, int x, int y)
    {
        uint offset = GetPointOffset(x, y);

        lastbuffer[offset] = (byte)(color & 0xFF);         // B
        lastbuffer[offset + 1] = (byte)((color >> 8) & 0xFF);  // G
        lastbuffer[offset + 2] = (byte)((color >> 16) & 0xFF); // R
        lastbuffer[offset + 3] = (byte)((color >> 24) & 0xFF); // A
    }

    public override uint GetPixel(int x, int y)
    {
        uint offset = GetPointOffset(x, y);

        byte b = lastbuffer[offset];
        byte g = lastbuffer[offset + 1];
        byte r = lastbuffer[offset + 2];
        byte a = lastbuffer[offset + 3];

        return (uint)(b | (g << 8) | (r << 16) | (a << 24));
    }

    public override void GetVRAM(int sourceByteOffset, int[] dest, int destIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            uint offset = (uint)(sourceByteOffset + i * 4);
            byte b = lastbuffer[offset];
            byte g = lastbuffer[offset + 1];
            byte r = lastbuffer[offset + 2];
            byte a = lastbuffer[offset + 3];
            dest[destIndex + i] = (int)(uint)(b | (g << 8) | (r << 16) | (a << 24));
        }
    }

    public void ClearVRAM(int aStart, int aCount, int value)
    {
        lastbuffer.Fill(aStart, aCount, value);
    }

    public override void ClearScreen(uint color)
    {
        lastbuffer.Fill(color);
    }

    /// <summary>
    /// Swap back buffer to video memory
    /// </summary>
    public override void Swap()
    {
        LinearFrameBuffer.Copy(lastbuffer);
    }

    /// <summary>
    /// Copy a buffer of pixels to a rectangular region.
    /// </summary>
    public override void CopyBuffer(ReadOnlyMemory<uint> pixels, int x, int y, int width, int height)
    {
        // Clamp to screen bounds
        if (x < 0 || y < 0 || width <= 0 || height <= 0)
        {
            return;
        }

        if (x >= (int)Width || y >= (int)Height)
        {
            return;
        }

        int clampedWidth = Math.Min(width, (int)Width - x);
        int clampedHeight = Math.Min(height, (int)Height - y);

        var span = pixels.Span;
        for (int row = 0; row < clampedHeight; row++)
        {
            int srcOffset = row * width;
            int dstByteOffset = (int)((y + row) * Pitch + x * Stride);

            // Copy one row at a time
            var rowPixels = span.Slice(srcOffset, clampedWidth);
            lastbuffer.Copy(dstByteOffset, rowPixels);
        }
    }

    /// <summary>
    /// Copy a buffer of pixels to a rectangular region (int version for image data).
    /// </summary>
    public override void CopyBuffer(ReadOnlyMemory<int> pixels, int x, int y, int width, int height)
    {
        // Clamp to screen bounds
        if (x < 0 || y < 0 || width <= 0 || height <= 0)
        {
            return;
        }

        if (x >= (int)Width || y >= (int)Height)
        {
            return;
        }

        int clampedWidth = Math.Min(width, (int)Width - x);
        int clampedHeight = Math.Min(height, (int)Height - y);

        // Reinterpret int as uint span (same memory layout)
        var span = global::System.Runtime.InteropServices.MemoryMarshal.Cast<int, uint>(pixels.Span);
        for (int row = 0; row < clampedHeight; row++)
        {
            int srcOffset = row * width;
            int dstByteOffset = (int)((y + row) * Pitch + x * Stride);

            var rowPixels = span.Slice(srcOffset, clampedWidth);
            lastbuffer.Copy(dstByteOffset, rowPixels);
        }
    }
}
