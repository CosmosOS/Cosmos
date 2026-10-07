// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdHeader: the id and body size in front of every 3D command, 8 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdHeader
{
    /// <summary>The command id, an SVGA_3D_CMD value.</summary>
    public uint id;
    /// <summary>Bytes of the body that follows the header.</summary>
    public uint size;
}
