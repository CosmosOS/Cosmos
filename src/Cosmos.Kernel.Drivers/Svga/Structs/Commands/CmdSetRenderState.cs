// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdSetRenderState: the fixed part of a SETRENDERSTATE, 4 bytes; the states follow.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetRenderState
{
    /// <summary>The context; the render states follow.</summary>
    public uint cid;
}
