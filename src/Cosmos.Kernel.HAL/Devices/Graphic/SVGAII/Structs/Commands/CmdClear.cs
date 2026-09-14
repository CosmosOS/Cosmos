using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdClear
{
    public uint cid;
    public ClearFlags flag;
    public uint color;
    public float depth;
    public uint stencil;
}
