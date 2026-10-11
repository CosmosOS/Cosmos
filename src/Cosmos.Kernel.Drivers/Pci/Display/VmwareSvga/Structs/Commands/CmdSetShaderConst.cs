// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Structs.Commands;

/// <summary>SVGA3dCmdSetShaderConst: the body of a SET_SHADER_CONST, 32 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetShaderConst
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>The constant register.</summary>
    public uint reg;
    /// <summary>Vertex or pixel.</summary>
    public SVGA3dShaderType type;
    /// <summary>The element type of the register.</summary>
    public SVGA3dShaderConstType ctype;
    /// <summary>The register's four dwords.</summary>
    public FourDwords values;
}

/// <summary>The four dwords of one shader constant register, in place of a fixed buffer.</summary>
[InlineArray(4)]
internal struct FourDwords
{
    /// <summary>The first element; the attribute repeats it.</summary>
    private uint _element0;
}
