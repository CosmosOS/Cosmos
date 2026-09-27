using System.Diagnostics.CodeAnalysis;
using System;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[Experimental("COSMOS0003")]
public enum SVGA3dTransferType
{
    SVGA3D_WRITE_HOST_VRAM = 1,
    SVGA3D_READ_HOST_VRAM = 2,
}
