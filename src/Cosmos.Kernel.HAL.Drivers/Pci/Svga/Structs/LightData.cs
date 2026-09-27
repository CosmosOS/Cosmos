using System.Diagnostics.CodeAnalysis;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Experimental("COSMOS0003")]
public struct SVGA3dLightData
{
    public LightType type;
    public uint inWorldSpace;
    public Vector4 diffuse;
    public Vector4 specular;
    public Vector4 ambient;
    public Vector4 position;
    public Vector4 direction;
    public float range;
    public float falloff;
    public Vector3 attenuation;
    public float theta;
    public float phi;
}
