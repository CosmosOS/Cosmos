// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dPrimitiveRange: one indexed primitive range of a DRAW_PRIMITIVES, 28 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dPrimitiveRange
{
    /// <summary>The primitive type.</summary>
    public SVGA3dPrimitiveType primType;
    /// <summary>How many primitives.</summary>
    public uint primitiveCount;
    /// <summary>The index buffer; a surface id of 0 draws unindexed.</summary>
    public SVGA3dArray indexArray;
    /// <summary>Bytes per index, 2 or 4.</summary>
    public uint indexWidth;
    /// <summary>Added to every index.</summary>
    public int indexBias;
}
