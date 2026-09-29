// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdDefineSurface: the fixed part of a SURFACE_DEFINE, 36 bytes; the mip sizes follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdDefineSurface
{
    /// <summary>The surface id.</summary>
    public uint sid;
    /// <summary>The surface flags.</summary>
    public SVGA3dSurfaceFlags flags;
    /// <summary>The surface format.</summary>
    public SVGA3dSurfaceFormat format;
    /// <summary>The mip level count of each of the six faces; one face with one mip for the surfaces the driver defines.</summary>
    public SixFaces face;
}

/// <summary>The mip level count of each of the six faces of a surface, in place of a fixed buffer.</summary>
[InlineArray(6)]
internal struct SixFaces
{
    /// <summary>The first element; the attribute repeats it.</summary>
    private uint _element0;
}
