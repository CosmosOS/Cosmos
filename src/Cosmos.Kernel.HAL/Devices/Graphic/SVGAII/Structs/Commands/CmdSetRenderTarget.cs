using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetRenderTarget
{
    public uint cid;
    public SVGA3dRenderTargetType type;
    public SVGA3dSurfaceImageId target;
}
