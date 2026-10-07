// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdDrawPrimitives: the fixed part of a DRAW_PRIMITIVES, 12 bytes; the declarations and ranges follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdDrawPrimitives
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>How many vertex declarations follow the body.</summary>
    public uint numVertexDecls;
    /// <summary>How many primitive ranges follow the declarations.</summary>
    public uint numRanges;
}
