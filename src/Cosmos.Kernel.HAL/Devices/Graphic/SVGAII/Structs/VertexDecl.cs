using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dVertexDecl
{
    public SVGA3dVertexArrayIdentity identity;
    public SVGA3dArray array;
    public SVGA3dArrayRangeHint rangeHint;
}
