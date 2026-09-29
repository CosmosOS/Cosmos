// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdSetShader: the body of a SET_SHADER, 12 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetShader
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>Vertex or pixel.</summary>
    public SVGA3dShaderType type;
    /// <summary>The shader id.</summary>
    public uint shid;
}
