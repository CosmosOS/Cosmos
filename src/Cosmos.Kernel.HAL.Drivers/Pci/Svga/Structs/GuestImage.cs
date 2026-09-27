using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public struct SVGA3dGuestImage
{
    public SVGAGuestPtr ptr;
    public float pitch;
}
