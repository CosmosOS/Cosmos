// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs;

/// <summary>SVGA3dSurfaceImageId: a surface, a face and a mip level, 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dSurfaceImageId
{
    /// <summary>The surface.</summary>
    public uint sid;
    /// <summary>The cube face, 0 for a 2D surface.</summary>
    public uint face;
    /// <summary>The mip level.</summary>
    public uint mipmap;
}
