using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dVertexArrayIdentity
{
    public SVGA3dDeclType type;
    public SVGA3dDeclMethod method;
    public SVGA3dDeclUsage usage;
    public uint usageIndex;
}
