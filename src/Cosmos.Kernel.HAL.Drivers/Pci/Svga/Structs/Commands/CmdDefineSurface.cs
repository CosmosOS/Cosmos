using System.Diagnostics.CodeAnalysis;
using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public unsafe struct SVGA3dCmdDefineSurface
{
    public uint sid;
    public SVGA3dSurfaceFlags flags;
    public SVGA3dSurfaceFormat format;
    public fixed uint face[6];
}
