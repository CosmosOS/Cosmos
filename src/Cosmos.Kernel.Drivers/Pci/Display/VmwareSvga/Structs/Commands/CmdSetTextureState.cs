// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSetTextureState: the fixed part of a SETTEXTURESTATE, 4 bytes; the states follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetTextureState
{
    /// <summary>The context; the texture states follow.</summary>
    public uint cid;
}
