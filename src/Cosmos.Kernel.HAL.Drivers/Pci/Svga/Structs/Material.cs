using System.Diagnostics.CodeAnalysis;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public struct SVGA3dMaterial
{
    public Vector4 diffuse;
    public Vector4 ambient;
    public Vector4 specular;
    public Vector4 emissive;
    public float shininess;
}
