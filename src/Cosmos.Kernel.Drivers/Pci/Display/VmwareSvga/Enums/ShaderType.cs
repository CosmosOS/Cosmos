// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

/// <summary>SVGA3dShaderType: vertex or pixel shader.</summary>
internal enum SVGA3dShaderType
{
    SVGA3D_SHADERTYPE_VS = 1,
    SVGA3D_SHADERTYPE_PS = 2,
    SVGA3D_SHADERTYPE_MAX
}
