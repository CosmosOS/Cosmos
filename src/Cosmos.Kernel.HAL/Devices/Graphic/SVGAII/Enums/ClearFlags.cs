using System;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

[Flags]
internal enum ClearFlags : uint
{
    Color = 0x1,
    Depth = 0x2,
    Stencil = 0x4
}
