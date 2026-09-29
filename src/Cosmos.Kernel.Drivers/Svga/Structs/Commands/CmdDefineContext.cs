// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdDefineContext: the body of a CONTEXT_DEFINE, 4 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdDefineContext
{
    /// <summary>The context id to define.</summary>
    public uint cid;
}
