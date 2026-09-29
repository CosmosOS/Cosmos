// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dArray: a buffer surface, an offset and a stride, 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dArray
{
    /// <summary>The buffer surface holding the array.</summary>
    public uint surfaceId;
    /// <summary>Byte offset of the first element in the surface.</summary>
    public uint offset;
    /// <summary>Bytes from one element to the next.</summary>
    public uint stride;
}
