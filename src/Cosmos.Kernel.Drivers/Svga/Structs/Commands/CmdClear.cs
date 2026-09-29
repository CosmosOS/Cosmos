// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdClear: the fixed part of a CLEAR, 20 bytes; the rectangles follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdClear
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>Which buffers to clear.</summary>
    public ClearFlags flag;
    /// <summary>The colour, raw ARGB.</summary>
    public uint color;
    /// <summary>The depth value.</summary>
    public float depth;
    /// <summary>The stencil value.</summary>
    public uint stencil;
}
