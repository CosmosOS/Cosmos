// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSetRenderTarget: the body of a SETRENDERTARGET, 20 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetRenderTarget
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The slot.</summary>
    public SVGA3dRenderTargetType type;
    /// <summary>The surface image bound to the slot.</summary>
    public SVGA3dSurfaceImageId target;
}
