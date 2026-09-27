using System.Diagnostics.CodeAnalysis;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public unsafe struct SVGA3dCmdSetShaderConst
{
    public uint cid;
    public uint reg;
    public SVGA3dShaderType type;
    public SVGA3dShaderConstType ctype;
    public fixed uint values[4];
}
