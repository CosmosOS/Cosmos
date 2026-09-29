// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dGuestImage: a guest pointer and a pitch (0 for tightly packed), 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dGuestImage
{
    /// <summary>Where the image sits in guest memory.</summary>
    public SVGAGuestPtr ptr;
    /// <summary>Bytes per row; 0 for tightly packed rows.</summary>
    public float pitch;
}
