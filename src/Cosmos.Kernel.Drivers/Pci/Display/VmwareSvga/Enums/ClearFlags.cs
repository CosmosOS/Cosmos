// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

/// <summary>SVGA3dClearFlag bits: which buffers a CLEAR resets.</summary>
[Flags]
internal enum ClearFlags : uint
{
    Color = 0x1,
    Depth = 0x2,
    Stencil = 0x4
}
