using System;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// A set of 3D primitives uploaded to a 3D device. Created with one of the
/// <see cref="Canvas3D"/> <c>CreateMesh</c> overloads and drawn with
/// <see cref="Canvas3D.DrawMesh(Mesh, in global::System.Numerics.Matrix4x4)"/>;
/// dispose it to release the device memory it occupies.
/// </summary>
public sealed class Mesh : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// The canvas that created this mesh.
    /// </summary>
    internal Canvas3D Owner { get; }

    /// <summary>
    /// How the indices of this mesh assemble into primitives.
    /// </summary>
    internal MeshTopology Topology { get; }

    /// <summary>
    /// Backend-specific resource data, owned by the canvas that created the
    /// mesh.
    /// </summary>
    internal object? DriverData { get; set; }

    internal Mesh(Canvas3D owner, int vertexCount, int indexCount, Texture? texture, MeshTopology topology)
    {
        Owner = owner;
        VertexCount = vertexCount;
        IndexCount = indexCount;
        Texture = texture;
        Topology = topology;
    }

    /// <summary>
    /// The number of vertices in the mesh.
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// The number of indices in the mesh.
    /// </summary>
    public int IndexCount { get; }

    /// <summary>
    /// The texture mapped onto the mesh, or <see langword="null"/> when the
    /// mesh is colored per vertex.
    /// </summary>
    public Texture? Texture { get; }

    internal bool IsDisposed => _disposed;

    /// <summary>
    /// Releases the device memory held by this mesh. Any <see cref="Texture"/>
    /// it references is not disposed with it.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Owner.DestroyMesh(this);
    }
}

/// <summary>
/// How the indices of a mesh assemble into primitives.
/// </summary>
internal enum MeshTopology
{
    /// <summary>Every three indices form a triangle.</summary>
    Triangles,

    /// <summary>Every two indices form a line segment.</summary>
    Lines,
}
