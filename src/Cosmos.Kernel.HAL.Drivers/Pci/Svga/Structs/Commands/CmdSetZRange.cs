using System.Diagnostics.CodeAnalysis;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public struct SVGA3dCmdSetZRange
{
    public uint cid;
    public SVGA3dZRange range;
}
