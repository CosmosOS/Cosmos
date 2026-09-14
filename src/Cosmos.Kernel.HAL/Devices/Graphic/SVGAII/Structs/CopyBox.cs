using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCopyBox
{
    public uint x;
    public uint y;
    public uint z;
    public uint w;
    public uint h;
    public uint d;
    public uint srcx;
    public uint srcy;
    public uint srcz;
}
