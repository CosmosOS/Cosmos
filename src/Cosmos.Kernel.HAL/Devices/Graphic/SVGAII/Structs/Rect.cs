using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dRect
{
    public uint x;
    public uint y;
    public uint w;
    public uint h;

    public SVGA3dRect(uint x, uint y, uint w, uint h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
}
