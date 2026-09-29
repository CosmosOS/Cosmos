// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCopyBox: one box of a SURFACE_DMA, 36 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCopyBox
{
    /// <summary>Left edge in the surface.</summary>
    public uint x;
    /// <summary>Top edge in the surface.</summary>
    public uint y;
    /// <summary>Front slice in the surface.</summary>
    public uint z;
    /// <summary>Width in pixels.</summary>
    public uint w;
    /// <summary>Height in pixels.</summary>
    public uint h;
    /// <summary>Depth in slices.</summary>
    public uint d;
    /// <summary>Left edge in the guest memory.</summary>
    public uint srcx;
    /// <summary>Top edge in the guest memory.</summary>
    public uint srcy;
    /// <summary>Front slice in the guest memory.</summary>
    public uint srcz;
}
