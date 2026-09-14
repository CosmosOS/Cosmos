using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdDrawPrimitives
{
    public uint cid;
    public uint numVertexDecls;
    public uint numRanges;
}
