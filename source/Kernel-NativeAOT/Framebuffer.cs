using System;
using System.Runtime.InteropServices;

public class FrameBuffer
{
    public enum PixelFormat
    {
        Unknown,
        R8G8B8,
        B8G8R8,
    }

    public readonly IntPtr _ptr;
    public readonly ulong _len;

    public readonly uint Width;
    public readonly uint Height;
    public readonly PixelFormat Format;

    public FrameBuffer(IntPtr pointer, ulong length, uint width, uint height, PixelFormat format)
    {
        _ptr = pointer;
        _len = length;
        Width = width;
        Height = height;
        Format = format;
    }

    public unsafe uint this[uint index]
    {
        get => ((uint*)_ptr)[index];
        set { ((uint*)_ptr)[index] = value; }
    }

    public unsafe uint this[int x, int y]
    {
        get => ((uint*)_ptr)[y * Width + x];
        set { ((uint*)_ptr)[y * Width + x] = value; }
    }


    public unsafe void Clear()
    {
        var count = _len / 2;
        var rem = _len % 2;

        for (var i = 0u; i < count; i++)
            ((ulong*)_ptr)[i] = 0;

        if (rem == 1)
            ((uint*)_ptr)[_len - 1] = 0;
    }

    public void Fill(uint value)
    {
        for (var i = 0u; i < _len / 4; i++)
            this[i] = value;
    }

    public uint MakePixel(byte r, byte g, byte b)
    {
        uint pixel = 0;

        switch (Format)
        {
            case PixelFormat.R8G8B8:
                pixel = r + ((uint)g << 8) + ((uint)b << 16);
                break;
            case PixelFormat.B8G8R8:
                pixel = b + ((uint)g << 8) + ((uint)r << 16);
                break;
        }

        return pixel;
    }
}
