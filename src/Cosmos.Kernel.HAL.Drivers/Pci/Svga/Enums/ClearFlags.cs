using System.Diagnostics.CodeAnalysis;
using System;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[Flags]
[Experimental("COSMOS0003")]
public enum ClearFlags : uint
{
    Color = 0x1,
    Depth = 0x2,
    Stencil = 0x4
}
