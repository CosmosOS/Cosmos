// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

/// <summary>SVGA3dPrimitiveType: how a primitive range assembles its indices.</summary>
internal enum SVGA3dPrimitiveType
{
    SVGA3D_PRIMITIVE_INVALID = 0,
    SVGA3D_PRIMITIVE_TRIANGLELIST = 1,
    SVGA3D_PRIMITIVE_POINTLIST = 2,
    SVGA3D_PRIMITIVE_LINELIST = 3,
    SVGA3D_PRIMITIVE_LINESTRIP = 4,
    SVGA3D_PRIMITIVE_TRIANGLESTRIP = 5,
    SVGA3D_PRIMITIVE_TRIANGLEFAN = 6,
    SVGA3D_PRIMITIVE_MAX
}
