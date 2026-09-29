// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dShaderConstType: the element type of a shader constant register.</summary>
internal enum SVGA3dShaderConstType
{
    SVGA3D_CONST_TYPE_FLOAT = 0,
    SVGA3D_CONST_TYPE_INT = 1,
    SVGA3D_CONST_TYPE_BOOL = 2,
}
