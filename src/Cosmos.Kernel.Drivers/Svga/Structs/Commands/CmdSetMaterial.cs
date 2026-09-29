// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdSetMaterial: the body of a SETMATERIAL, 76 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetMaterial
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>Which faces the material applies to.</summary>
    public Face face;
    /// <summary>The material.</summary>
    public SVGA3dMaterial material;
}
