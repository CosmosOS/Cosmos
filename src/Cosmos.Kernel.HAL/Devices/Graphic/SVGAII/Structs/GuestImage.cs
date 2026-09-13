using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dGuestImage
{
    public SVGAGuestPtr ptr;
    public float pitch;
}
