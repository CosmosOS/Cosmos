// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dSize: the size of one mip level, 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dSize
{
    /// <summary>Width in pixels, or bytes for a buffer.</summary>
    public uint width;
    /// <summary>Height in pixels, or 1 for a buffer.</summary>
    public uint height;
    /// <summary>Depth in slices, 1 for a 2D surface.</summary>
    public uint depth;
}
