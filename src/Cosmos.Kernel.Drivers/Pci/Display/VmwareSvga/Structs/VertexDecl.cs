// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs;

/// <summary>SVGA3dVertexDecl: one vertex stream of a DRAW_PRIMITIVES, 36 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dVertexDecl
{
    /// <summary>What the stream holds.</summary>
    public SVGA3dVertexArrayIdentity identity;
    /// <summary>Where the stream is.</summary>
    public SVGA3dArray array;
    /// <summary>Which elements the draw touches.</summary>
    public SVGA3dArrayRangeHint rangeHint;
}
