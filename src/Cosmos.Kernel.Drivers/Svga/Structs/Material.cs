// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dMaterial: the fixed-function material of a SETMATERIAL, 68 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dMaterial
{
    /// <summary>The diffuse colour.</summary>
    public Vector4 diffuse;
    /// <summary>The ambient colour.</summary>
    public Vector4 ambient;
    /// <summary>The specular colour.</summary>
    public Vector4 specular;
    /// <summary>The emissive colour.</summary>
    public Vector4 emissive;
    /// <summary>The specular power.</summary>
    public float shininess;
}
