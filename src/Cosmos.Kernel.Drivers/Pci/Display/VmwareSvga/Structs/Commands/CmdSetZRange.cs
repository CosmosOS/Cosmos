// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSetZRange: the body of a SETZRANGE, 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetZRange
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The near and far depths.</summary>
    public SVGA3dZRange range;
}
