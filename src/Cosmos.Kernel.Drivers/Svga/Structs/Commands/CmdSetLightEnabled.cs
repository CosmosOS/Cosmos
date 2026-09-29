// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdSetLightEnabled: the body of a SETLIGHTENABLED, 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetLightEnabled
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The light index.</summary>
    public uint index;
    /// <summary>1 to turn the light on, 0 to turn it off.</summary>
    public uint enabled;
}
