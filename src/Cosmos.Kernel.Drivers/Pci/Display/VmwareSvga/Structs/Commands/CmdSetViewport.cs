// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSetViewport: the body of a SETVIEWPORT, 20 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetViewport
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The viewport.</summary>
    public SVGA3dRect rect;
}
