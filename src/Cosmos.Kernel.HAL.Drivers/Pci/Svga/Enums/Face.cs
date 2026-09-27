using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

/// <summary>
/// ID values.
/// </summary>
[Experimental("COSMOS0003")]
public enum Face : uint
{
    SVGA3D_FACE_INVALID = 0,
    SVGA3D_FACE_NONE = 1,
    SVGA3D_FACE_FRONT = 2,
    SVGA3D_FACE_BACK = 3,
    SVGA3D_FACE_FRONT_BACK = 4,
    SVGA3D_FACE_MAX
}
