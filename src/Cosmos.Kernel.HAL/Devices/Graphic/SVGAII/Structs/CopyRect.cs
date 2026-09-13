using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCopyRect
{
    public uint x;
    public uint y;
    public uint w;
    public uint h;
    public uint srcx;
    public uint srcy;
}
