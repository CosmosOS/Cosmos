using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct SVGA3dCmdDefineSurface
{
    public uint sid;
    public SVGA3dSurfaceFlags flags;
    public SVGA3dSurfaceFormat format;
    public fixed uint face[6];
}
