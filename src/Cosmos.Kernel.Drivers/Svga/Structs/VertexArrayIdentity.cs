// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dVertexArrayIdentity: the type, method, usage and usage index of a vertex stream, 16 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dVertexArrayIdentity
{
    /// <summary>The element type.</summary>
    public SVGA3dDeclType type;
    /// <summary>The tessellation method.</summary>
    public SVGA3dDeclMethod method;
    /// <summary>What the element is for.</summary>
    public SVGA3dDeclUsage usage;
    /// <summary>Which of several elements of the same usage.</summary>
    public uint usageIndex;
}
