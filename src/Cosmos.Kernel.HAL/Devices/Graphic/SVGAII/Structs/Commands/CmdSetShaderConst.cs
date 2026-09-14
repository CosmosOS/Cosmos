using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct SVGA3dCmdSetShaderConst
{
    public uint cid;
    public uint reg;
    public SVGA3dShaderType type;
    public SVGA3dShaderConstType ctype;
    public fixed uint values[4];
}
