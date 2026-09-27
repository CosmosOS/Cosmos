using System.Diagnostics.CodeAnalysis;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public unsafe struct SVGA3dCmdSetTransform
{
    public uint cid;
    public SVGA3dTransformType type;
    public fixed float matrix[16];
}
