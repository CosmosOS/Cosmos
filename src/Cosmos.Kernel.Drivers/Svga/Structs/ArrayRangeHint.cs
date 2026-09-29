// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dArrayRangeHint: the first and last vertex a stream is read at, 8 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dArrayRangeHint
{
    /// <summary>Index of the first element the draw touches.</summary>
    public uint first;
    /// <summary>Index of the last element the draw touches.</summary>
    public uint last;
}
