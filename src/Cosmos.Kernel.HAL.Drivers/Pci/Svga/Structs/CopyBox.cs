using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public struct SVGA3dCopyBox
{
    public uint x;
    public uint y;
    public uint z;
    public uint w;
    public uint h;
    public uint d;
    public uint srcx;
    public uint srcy;
    public uint srcz;
}
