using System.Diagnostics.CodeAnalysis;
using System;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[Experimental("COSMOS0003")]
public enum SVGA3dRenderTargetType : uint
{
    Color = 2,
    Depth = 0,
}
