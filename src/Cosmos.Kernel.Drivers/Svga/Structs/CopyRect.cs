// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCopyRect: one rectangle of a PRESENT, 24 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCopyRect
{
    /// <summary>Left edge on the destination.</summary>
    public uint x;
    /// <summary>Top edge on the destination.</summary>
    public uint y;
    /// <summary>Width in pixels.</summary>
    public uint w;
    /// <summary>Height in pixels.</summary>
    public uint h;
    /// <summary>Left edge in the surface.</summary>
    public uint srcx;
    /// <summary>Top edge in the surface.</summary>
    public uint srcy;
}
