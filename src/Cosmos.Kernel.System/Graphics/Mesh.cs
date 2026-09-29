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
    /// The canvas that created this mesh; only it can draw the mesh.
    /// </summary>
    public Canvas3D Owner { get; }

    /// <summary>
    /// How the indices of this mesh assemble into primitives.
    /// </summary>
    public MeshTopology Topology { get; }

    /// <summary>
    /// Backend-specific resource data, owned by the canvas that created the
    /// mesh, which reaches it through <see cref="Canvas3D.DriverDataOf(Mesh)"/>
    /// and <see cref="Canvas3D.SetDriverData(Mesh, object?)"/>.
    /// </summary>
    internal object? DriverData { get; set; }

    /// <summary>
    /// Creates a mesh handle owned by <paramref name="owner"/>; the canvas
    /// calls this through <see cref="Canvas3D.CreateMeshHandle"/>.
    /// </summary>
    /// <param name="owner">The canvas that uploaded the mesh.</param>
    /// <param name="vertexCount">The number of vertices in the mesh.</param>
    /// <param name="indexCount">The number of indices in the mesh.</param>
    /// <param name="texture">The texture mapped onto the mesh, or null.</param>
    /// <param name="topology">How the indices assemble into primitives.</param>
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

    /// <summary>
    /// Whether <see cref="Dispose"/> ran: a disposed mesh can no longer be drawn.
    /// </summary>
    public bool IsDisposed => _disposed;

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
public enum MeshTopology
{
    /// <summary>Every three indices form a triangle.</summary>
    Triangles,

    /// <summary>Every two indices form a line segment.</summary>
    Lines,
}
