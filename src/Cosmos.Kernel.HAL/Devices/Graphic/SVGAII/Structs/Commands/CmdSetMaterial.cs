using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetMaterial
{
    public uint cid;
    public Face face;
    public SVGA3dMaterial material;
}
