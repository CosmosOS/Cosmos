// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdDefineShader: the fixed part of a SHADER_DEFINE, 12 bytes; the bytecode follows.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdDefineShader
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The shader id.</summary>
    public uint shid;
    /// <summary>Vertex or pixel.</summary>
    public SVGA3dShaderType type;
}
