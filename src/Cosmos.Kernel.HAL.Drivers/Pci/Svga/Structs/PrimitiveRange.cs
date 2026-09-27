using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public struct SVGA3dPrimitiveRange
{
    public SVGA3dPrimitiveType primType;
    public uint primitiveCount;
    public SVGA3dArray indexArray;
    public uint indexWidth;
    public int indexBias;
}
