using System;
using System.Drawing;
using System.Numerics;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// A canvas backed by a device that can render 3D primitives. On top of the
/// 2D <see cref="Canvas"/> surface, a 3D canvas draws device-owned meshes
/// (<see cref="CreateMesh(ReadOnlySpan{Vector3}, ReadOnlySpan{uint}, ReadOnlySpan{ushort})"/>,
/// <see cref="DrawMesh(Mesh, in Matrix4x4)"/>) as seen through
/// <see cref="Camera"/>. A frame is composed by calling
/// <see cref="ClearScene"/>, issuing draw calls, and presenting with
/// <see cref="Canvas.Display"/>.
/// </summary>
/// <remarks>
/// Instances are obtained from
/// <see cref="Canvas.GetFullScreen()"/> and tested with <c>is Canvas3D</c>;
/// the canvas only reports the type when the primary display implements
/// <see cref="ICanvas3DFactory"/>, which a display driver does when its
/// device renders 3D. A driver package derives from this class: it builds
/// its render targets from <see cref="Canvas.Width"/> and
/// <see cref="Canvas.Height"/> in its constructor, recreates them in
/// <see cref="Canvas.OnModeChanged"/>, presents the scene in an override of
/// <see cref="Canvas.Display"/> (falling back to the base for a 2D frame),
/// releases everything in <see cref="Canvas.Disable"/>, and keeps its
/// per-resource state in the handles through <see cref="CreateMeshHandle"/>,
/// <see cref="CreateTextureHandle"/>, <see cref="DriverDataOf(Mesh)"/> and
/// <see cref="SetDriverData(Mesh, object?)"/>. Depth testing is enabled with
/// a depth range of 0.1 to 1000 world units from the camera; faces are not
/// culled.
/// </remarks>
public abstract class Canvas3D : Canvas
{
    private Camera3D _camera = new(new Vector3(0f, 0f, 5f), Vector3.Zero);

    private Mesh? _cubeMesh;
    private uint _cubeColor;
    private Mesh? _lineMesh;
    private uint _lineColor;

    /// <summary>
    /// Initializes a 3D canvas over a published display, in the display's
    /// current mode or its default mode when it reports none. The base has
    /// sized the buffer when the subclass constructor runs and calls no
    /// virtual member, so a subclass creates its render targets from
    /// <see cref="Canvas.Width"/> and <see cref="Canvas.Height"/> there and
    /// relies on <see cref="Canvas.OnModeChanged"/> only for later switches.
    /// Thread context.
    /// </summary>
    /// <param name="display">The display to draw on.</param>
    protected Canvas3D(DisplayDevice display)
        : base(display)
    {
    }

    /// <summary>
    /// The camera every 3D draw call is seen through. The aspect ratio comes
    /// from the canvas <see cref="Canvas.Mode"/>.
    /// </summary>
    public Camera3D Camera
    {
        get => _camera;
        set
        {
            _camera = value;
            OnCameraChanged();
        }
    }

    /// <summary>
    /// Called after <see cref="Camera"/> is assigned so the backend can
    /// invalidate any device state derived from it. Thread context.
    /// </summary>
    protected virtual void OnCameraChanged()
    {
    }

    /// <summary>
    /// Creates a per-vertex-colored triangle mesh on the device.
    /// </summary>
    /// <param name="positions">The vertex positions in model space.</param>
    /// <param name="colors">One raw ARGB color per vertex.</param>
    /// <param name="indices">Vertex indices; every three form a triangle.</param>
    /// <returns>The mesh, ready to be drawn on this canvas.</returns>
    /// <exception cref="ArgumentException">The mesh data is empty, mismatched or refers to vertices that do not exist.</exception>
    public Mesh CreateMesh(ReadOnlySpan<Vector3> positions, ReadOnlySpan<uint> colors, ReadOnlySpan<ushort> indices)
    {
        ThrowIfMeshDataNotValid(positions, indices, 3);

        if (colors.Length != positions.Length)
        {
            throw new ArgumentException("A mesh needs exactly one color per vertex.", nameof(colors));
        }

        return CreateMeshCore(positions, colors, default, null, indices, MeshTopology.Triangles);
    }

    /// <summary>
    /// Creates a textured triangle mesh on the device.
    /// </summary>
    /// <param name="positions">The vertex positions in model space.</param>
    /// <param name="uvs">One texture coordinate per vertex, in the 0-1 range.</param>
    /// <param name="texture">The texture mapped onto the mesh.</param>
    /// <param name="indices">Vertex indices; every three form a triangle.</param>
    /// <returns>The mesh, ready to be drawn on this canvas.</returns>
    /// <exception cref="ArgumentException">The mesh data is empty, mismatched or refers to vertices that do not exist.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="texture"/> is null.</exception>
    public Mesh CreateMesh(ReadOnlySpan<Vector3> positions, ReadOnlySpan<Vector2> uvs, Texture texture, ReadOnlySpan<ushort> indices)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ThrowIfMeshDataNotValid(positions, indices, 3);

        if (texture.Owner != this || texture.IsDisposed)
        {
            throw new ArgumentException("The texture was not created by this canvas or has been disposed.", nameof(texture));
        }

        if (uvs.Length != positions.Length)
        {
            throw new ArgumentException("A mesh needs exactly one texture coordinate per vertex.", nameof(uvs));
        }

        return CreateMeshCore(positions, default, uvs, texture, indices, MeshTopology.Triangles);
    }

    /// <summary>
    /// Uploads an image to the device so it can be mapped onto meshes.
    /// </summary>
    /// <param name="image">The image to upload.</param>
    /// <returns>The texture, ready to be referenced by meshes on this canvas.</returns>
    public abstract Texture CreateTexture(Image image);

    /// <summary>
    /// Clears the 3D scene to the given color and resets the depth buffer.
    /// Call at the start of every 3D frame.
    /// </summary>
    /// <param name="color">The background color.</param>
    /// <param name="depth">The depth value to reset the depth buffer to, in the 0-1 range.</param>
    public abstract void ClearScene(Color color, float depth = 1f);

    /// <summary>
    /// Draws a mesh transformed by <paramref name="world"/>.
    /// </summary>
    /// <param name="mesh">The mesh to draw; must have been created by this canvas.</param>
    /// <param name="world">The model-to-world transform of the mesh.</param>
    /// <exception cref="ArgumentNullException"><paramref name="mesh"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="mesh"/> was not created by this canvas, has been disposed, or maps a texture that has been disposed.</exception>
    public abstract void DrawMesh(Mesh mesh, in Matrix4x4 world);

    /// <summary>
    /// Draws a mesh at the world origin.
    /// </summary>
    /// <param name="mesh">The mesh to draw; must have been created by this canvas.</param>
    public void DrawMesh(Mesh mesh)
    {
        DrawMesh(mesh, Matrix4x4.Identity);
    }

    /// <summary>
    /// Draws a solid axis-aligned cube.
    /// </summary>
    /// <param name="position">The center of the cube in world space.</param>
    /// <param name="size">The edge lengths of the cube along each axis.</param>
    /// <param name="color">The color of the cube.</param>
    public void DrawCube(Vector3 position, Vector3 size, Color color)
    {
        uint argb = (uint)color.ToArgb();

        if (_cubeMesh is null || _cubeColor != argb)
        {
            _cubeMesh?.Dispose();
            _cubeMesh = CreateUnitCube(argb);
            _cubeColor = argb;
        }

        DrawMesh(_cubeMesh, Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(position));
    }

    /// <summary>
    /// Draws a line between two points in world space.
    /// </summary>
    /// <param name="start">The start of the line.</param>
    /// <param name="end">The end of the line.</param>
    /// <param name="color">The color of the line.</param>
    public void DrawLine3D(Vector3 start, Vector3 end, Color color)
    {
        uint argb = (uint)color.ToArgb();

        if (_lineMesh is null || _lineColor != argb)
        {
            _lineMesh?.Dispose();
            _lineMesh = CreateUnitLine(argb);
            _lineColor = argb;
        }

        // The mesh is the unit X segment; a matrix whose first row is the
        // line direction and whose translation is the start point maps it
        // onto start-end (only row X and the translation are ever multiplied
        // by its vertices).
        Vector3 direction = end - start;
        Matrix4x4 world = new(
            direction.X, direction.Y, direction.Z, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            start.X, start.Y, start.Z, 1f);

        DrawMesh(_lineMesh, world);
    }

    /// <summary>
    /// Draws a square grid of lines on the XZ plane, centered on the world
    /// origin.
    /// </summary>
    /// <param name="slices">The number of cells along each side of the grid.</param>
    /// <param name="spacing">The edge length of one cell.</param>
    /// <param name="color">The color of the grid lines.</param>
    public void DrawGrid(int slices, float spacing, Color color)
    {
        float extent = slices * spacing * 0.5f;

        for (int i = 0; i <= slices; i++)
        {
            float offset = (i * spacing) - extent;
            DrawLine3D(new Vector3(offset, 0f, -extent), new Vector3(offset, 0f, extent), color);
            DrawLine3D(new Vector3(-extent, 0f, offset), new Vector3(extent, 0f, offset), color);
        }
    }

    /// <summary>
    /// Creates a mesh on the device. Inputs are pre-validated: positions and
    /// indices are non-empty, <paramref name="colors"/> or <paramref name="uvs"/>
    /// are either empty or one entry per vertex, and every index is in range.
    /// The implementation uploads the data and returns the handle from
    /// <see cref="CreateMeshHandle"/> with its device state as driver data.
    /// Thread context.
    /// </summary>
    /// <param name="positions">The vertex positions in model space.</param>
    /// <param name="colors">One raw ARGB color per vertex, or empty for a textured mesh.</param>
    /// <param name="uvs">One texture coordinate per vertex, or empty for a colored mesh.</param>
    /// <param name="texture">The texture mapped onto the mesh, or null for a colored mesh.</param>
    /// <param name="indices">Vertex indices, assembled by <paramref name="topology"/>.</param>
    /// <param name="topology">How the indices assemble into primitives.</param>
    protected abstract Mesh CreateMeshCore(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<uint> colors,
        ReadOnlySpan<Vector2> uvs,
        Texture? texture,
        ReadOnlySpan<ushort> indices,
        MeshTopology topology);

    /// <summary>
    /// Releases the device resources of a mesh created by this canvas.
    /// Called once, from <see cref="Mesh.Dispose"/>; an override in a driver
    /// package is declared <c>protected override</c>. Thread context.
    /// </summary>
    /// <param name="mesh">The mesh being disposed.</param>
    protected internal abstract void DestroyMesh(Mesh mesh);

    /// <summary>
    /// Releases the device resources of a texture created by this canvas.
    /// Called once, from <see cref="Texture.Dispose"/>; an override in a
    /// driver package is declared <c>protected override</c>. Thread context.
    /// </summary>
    /// <param name="texture">The texture being disposed.</param>
    protected internal abstract void DestroyTexture(Texture texture);

    /// <summary>
    /// Creates the handle for a mesh this canvas uploaded, owned by this
    /// canvas, with the backend's state attached as driver data. For
    /// <see cref="CreateMeshCore"/> in a driver package. Thread context.
    /// </summary>
    /// <param name="vertexCount">The number of vertices in the mesh.</param>
    /// <param name="indexCount">The number of indices in the mesh.</param>
    /// <param name="texture">The texture mapped onto the mesh, or null.</param>
    /// <param name="topology">How the indices assemble into primitives.</param>
    /// <param name="driverData">The backend's state for the mesh, read back with <see cref="DriverDataOf(Mesh)"/>.</param>
    /// <returns>The mesh handle.</returns>
    protected Mesh CreateMeshHandle(int vertexCount, int indexCount, Texture? texture, MeshTopology topology, object? driverData)
    {
        return new Mesh(this, vertexCount, indexCount, texture, topology)
        {
            DriverData = driverData,
        };
    }

    /// <summary>
    /// Creates the handle for a texture this canvas uploaded, owned by this
    /// canvas, with the backend's state attached as driver data. For
    /// <see cref="CreateTexture"/> in a driver package. Thread context.
    /// </summary>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <param name="driverData">The backend's state for the texture, read back with <see cref="DriverDataOf(Texture)"/>.</param>
    /// <returns>The texture handle.</returns>
    protected Texture CreateTextureHandle(int width, int height, object? driverData)
    {
        return new Texture(this, width, height, driverData);
    }

    /// <summary>
    /// The backend's state attached to a mesh by <see cref="CreateMeshHandle"/>
    /// or <see cref="SetDriverData(Mesh, object?)"/>. Any context.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    protected static object? DriverDataOf(Mesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        return mesh.DriverData;
    }

    /// <summary>
    /// The backend's state attached to a texture by <see cref="CreateTextureHandle"/>
    /// or <see cref="SetDriverData(Texture, object?)"/>. Any context.
    /// </summary>
    /// <param name="texture">The texture.</param>
    protected static object? DriverDataOf(Texture texture)
    {
        ArgumentNullException.ThrowIfNull(texture);

        return texture.DriverData;
    }

    /// <summary>
    /// Attaches the backend's state to a mesh, or clears it with null once
    /// the device resource is released. Any context.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="data">The state, or null.</param>
    protected static void SetDriverData(Mesh mesh, object? data)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        mesh.DriverData = data;
    }

    /// <summary>
    /// Attaches the backend's state to a texture, or clears it with null once
    /// the device resource is released. Any context.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="data">The state, or null.</param>
    protected static void SetDriverData(Texture texture, object? data)
    {
        ArgumentNullException.ThrowIfNull(texture);

        texture.DriverData = data;
    }

    private Mesh CreateUnitCube(uint argb)
    {
        ReadOnlySpan<Vector3> positions =
        [
            new(-0.5f, -0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f),
            new(-0.5f, 0.5f, -0.5f),
            new(-0.5f, 0.5f, 0.5f),
            new(0.5f, -0.5f, -0.5f),
            new(0.5f, -0.5f, 0.5f),
            new(0.5f, 0.5f, -0.5f),
            new(0.5f, 0.5f, 0.5f),
        ];

        ReadOnlySpan<ushort> indices =
        [
            0, 1, 3, 3, 2, 0, // -X
            4, 5, 7, 7, 6, 4, // +X
            0, 1, 5, 5, 4, 0, // -Y
            2, 3, 7, 7, 6, 2, // +Y
            0, 2, 6, 6, 4, 0, // -Z
            1, 3, 7, 7, 5, 1, // +Z
        ];

        Span<uint> colors = stackalloc uint[positions.Length];
        colors.Fill(argb);

        return CreateMeshCore(positions, colors, default, null, indices, MeshTopology.Triangles);
    }

    private Mesh CreateUnitLine(uint argb)
    {
        ReadOnlySpan<Vector3> positions = [Vector3.Zero, Vector3.UnitX];
        ReadOnlySpan<uint> colors = [argb, argb];
        ReadOnlySpan<ushort> indices = [0, 1];

        return CreateMeshCore(positions, colors, default, null, indices, MeshTopology.Lines);
    }

    private static void ThrowIfMeshDataNotValid(ReadOnlySpan<Vector3> positions, ReadOnlySpan<ushort> indices, int indicesPerPrimitive)
    {
        if (positions.IsEmpty)
        {
            throw new ArgumentException("A mesh needs at least one vertex.", nameof(positions));
        }

        if (indices.IsEmpty || indices.Length % indicesPerPrimitive != 0)
        {
            throw new ArgumentException($"A mesh needs a non-empty index list with a multiple of {indicesPerPrimitive} entries.", nameof(indices));
        }

        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] >= positions.Length)
            {
                throw new ArgumentException($"Index {i} refers to vertex {indices[i]}, but the mesh only has {positions.Length} vertices.", nameof(indices));
            }
        }
    }
}
