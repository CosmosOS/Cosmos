using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct SVGA3dCmdSetTransform
{
    public uint cid;
    public SVGA3dTransformType type;
    public fixed float matrix[16];
}
