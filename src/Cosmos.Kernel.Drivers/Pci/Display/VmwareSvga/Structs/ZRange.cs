// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs;

/// <summary>SVGA3dZRange: the depth range of a SETZRANGE, 8 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dZRange
{
    /// <summary>The near depth.</summary>
    public float min;
    /// <summary>The far depth.</summary>
    public float max;
}
