// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs;

/// <summary>SVGA3dRect: a rectangle in pixels, 16 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dRect
{
    /// <summary>Left edge.</summary>
    public uint x;
    /// <summary>Top edge.</summary>
    public uint y;
    /// <summary>Width in pixels.</summary>
    public uint w;
    /// <summary>Height in pixels.</summary>
    public uint h;

    /// <summary>A rectangle from its edges and size.</summary>
    public SVGA3dRect(uint x, uint y, uint w, uint h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
}
