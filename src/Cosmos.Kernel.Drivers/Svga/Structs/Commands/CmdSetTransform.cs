// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dCmdSetTransform: the body of a SETTRANSFORM, 72 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dCmdSetTransform
{
    /// <summary>The context.</summary>
    public uint cid;
    /// <summary>Which matrix.</summary>
    public SVGA3dTransformType type;
    /// <summary>The matrix, 16 floats row-major.</summary>
    public Matrix4x4Floats matrix;
}

/// <summary>The 16 floats of a 4x4 matrix, row-major, in place of a fixed buffer.</summary>
[InlineArray(16)]
internal struct Matrix4x4Floats
{
    /// <summary>The first element; the attribute repeats it.</summary>
    private float _element0;
}
