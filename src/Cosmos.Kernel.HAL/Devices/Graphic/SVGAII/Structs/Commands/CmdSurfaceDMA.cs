using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSurfaceDMA
{
    public SVGA3dGuestImage guest;
    public SVGA3dSurfaceImageId host;
    public SVGA3dTransferType transfer;
}
