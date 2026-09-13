namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

/// <summary>
/// ID values.
/// </summary>
internal enum LightType : uint
{
    SVGA3D_LIGHTTYPE_INVALID = 0,
    SVGA3D_LIGHTTYPE_POINT = 1,
    SVGA3D_LIGHTTYPE_SPOT1 = 2,
    SVGA3D_LIGHTTYPE_SPOT2 = 3,
    SVGA3D_LIGHTTYPE_DIRECTIONAL = 4,
    SVGA3D_LIGHTTYPE_MAX
}
