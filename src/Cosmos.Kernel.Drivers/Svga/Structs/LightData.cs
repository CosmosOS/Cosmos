// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>SVGA3dLightData: the fixed-function light of a SETLIGHTDATA, 116 bytes.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SVGA3dLightData
{
    /// <summary>The light type.</summary>
    public LightType type;
    /// <summary>1 when the position and direction are in world space.</summary>
    public uint inWorldSpace;
    /// <summary>The diffuse colour.</summary>
    public Vector4 diffuse;
    /// <summary>The specular colour.</summary>
    public Vector4 specular;
    /// <summary>The ambient colour.</summary>
    public Vector4 ambient;
    /// <summary>The position, for point and spot lights.</summary>
    public Vector4 position;
    /// <summary>The direction, for directional and spot lights.</summary>
    public Vector4 direction;
    /// <summary>The distance the light reaches.</summary>
    public float range;
    /// <summary>The falloff between the cones of a spot light.</summary>
    public float falloff;
    /// <summary>The constant, linear and quadratic attenuation.</summary>
    public Vector3 attenuation;
    /// <summary>The inner cone angle of a spot light.</summary>
    public float theta;
    /// <summary>The outer cone angle of a spot light.</summary>
    public float phi;
}
