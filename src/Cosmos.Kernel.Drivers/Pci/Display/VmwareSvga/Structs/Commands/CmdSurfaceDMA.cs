// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSurfaceDMA: the fixed part of a SURFACE_DMA, 28 bytes; the boxes follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSurfaceDMA
{
    /// <summary>The guest memory: a pointer and a pitch.</summary>
    public SVGA3dGuestImage guest;
    /// <summary>The surface image.</summary>
    public SVGA3dSurfaceImageId host;
    /// <summary>The direction; the copy boxes follow.</summary>
    public SVGA3dTransferType transfer;
}
