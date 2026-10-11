// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

/// <summary>SVGA3dTransferType: the direction of a SURFACE_DMA, guest memory to the host surface or back.</summary>
internal enum SVGA3dTransferType
{
    SVGA3D_WRITE_HOST_VRAM = 1,
    SVGA3D_READ_HOST_VRAM = 2,
}
