// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

/// <summary>
/// ID values.
/// </summary>
internal enum Face : uint
{
    SVGA3D_FACE_INVALID = 0,
    SVGA3D_FACE_NONE = 1,
    SVGA3D_FACE_FRONT = 2,
    SVGA3D_FACE_BACK = 3,
    SVGA3D_FACE_FRONT_BACK = 4,
    SVGA3D_FACE_MAX
}
