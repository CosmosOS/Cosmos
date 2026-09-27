using System.Diagnostics.CodeAnalysis;
using System;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[Experimental("COSMOS0003")]
public enum SVGA3dShaderConstType
{
    SVGA3D_CONST_TYPE_FLOAT = 0,
    SVGA3D_CONST_TYPE_INT = 1,
    SVGA3D_CONST_TYPE_BOOL = 2,
}
