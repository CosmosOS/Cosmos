// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Graphics.Rendering3D;

/// <summary>
/// How the indices of a mesh assemble into primitives.
/// </summary>
public enum MeshTopology
{
    /// <summary>Every three indices form a triangle.</summary>
    Triangles,

    /// <summary>Every two indices form a line segment.</summary>
    Lines,
}
