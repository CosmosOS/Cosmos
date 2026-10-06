// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSetLightData: the body of a SETLIGHTDATA, 124 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetLightData
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The light index.</summary>
    public uint index;
    /// <summary>The light.</summary>
    public SVGA3dLightData data;
}
