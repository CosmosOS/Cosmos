// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdPresent: the fixed part of a PRESENT, 4 bytes; the rectangles follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdPresent
{
    /// <summary>The surface to present; the copy rectangles follow.</summary>
    public uint sid;
}
