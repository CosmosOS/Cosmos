// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using System.Numerics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Rendering3D;

namespace Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu;

/// <summary>
/// The <see cref="Canvas3D"/> of a virtio-gpu device on the virgl path,
/// over the ring's buffer and the driver's 3D state: 2D drawing is the
/// base's, into its buffer, and <see cref="Canvas.Display"/> presents the
/// rendered frame when a scene is open — the engine reads the color target
/// back into the scanout framebuffer — and copies the buffer to the frame
/// otherwise. The canvas owns one <see cref="Virtio3DEngine"/>, which it
/// installs as its <see cref="Canvas3D.Engine"/> from the constructor so
/// 3D code reaches the device through the canvas alone, and the engine
/// builds its context, targets and built-in shaders there, recreated when
/// the mode changes and destroyed by <see cref="Canvas.Disable"/>. Only
/// meaningful when the device kept the VIRGL bit; the engine reports not
/// ready otherwise and the canvas degrades to a 2D one. Thread context.
/// </summary>
internal sealed class Virtio3DCanvas : Canvas3D
{
    // --- Constants ---

    /// <summary>The near plane of the projection, in world units from the camera.</summary>
    private const float NearPlane = 0.1f;

    /// <summary>The far plane of the projection, in world units from the camera.</summary>
    private const float FarPlane = 1000f;

    // --- Private fields ---

    private readonly VirtioGpu3DState _state;
    private readonly Virtio3DEngine _engine;
    private bool _sceneOpen;
    private bool _displaying3D;
    private bool _cameraApplied;

    // --- Constructor ---

    /// <summary>
    /// Creates the canvas over the display the state published, and the
    /// engine behind it, after the base sized the buffer. Thread context.
    /// </summary>
    /// <param name="state">The virgl state the engine submits through.</param>
    /// <param name="display">The display the state published.</param>
    internal Virtio3DCanvas(VirtioGpu3DState state, DisplayDevice display)
        : base(display)
    {
        _state = state;
        _engine = new Virtio3DEngine(state);
        Engine = _engine;
    }

    // --- Public methods ---

    /// <summary>Presents the rendered frame when a scene is open, else copies the buffer to the frame. Thread context.</summary>
    public override void Display()
    {
        if (_sceneOpen)
        {
            _engine.Present();
            _sceneOpen = false;
            _displaying3D = true;
            return;
        }

        base.Display();
        _displaying3D = false;
    }

    /// <summary>
    /// Reads a rectangle of pixels back: from the rendered color target
    /// while a scene is open or the last presented frame was one, else from
    /// the buffer like every other canvas. Thread context.
    /// </summary>
    /// <param name="x">The starting X coordinate of the region to copy.</param>
    /// <param name="y">The starting Y coordinate of the region to copy.</param>
    /// <param name="width">The width of the region to copy.</param>
    /// <param name="height">The height of the region to copy.</param>
    /// <returns>A new <see cref="Bitmap"/> containing the copied region.</returns>
    public override Bitmap GetImage(int x, int y, int width, int height)
    {
        if (!_sceneOpen && !_displaying3D)
        {
            return base.GetImage(x, y, width, height);
        }

        Bitmap bitmap = new(width, height, ColorDepth.ColorDepth32);
        if (width > 0 && height > 0 && x >= 0 && y >= 0)
        {
            _engine.ReadPixels(x, y, width, height, bitmap.RawData);
        }

        return bitmap;
    }

    /// <inheritdoc/>
    public override void ClearScene(Color color, float depth = 1f)
    {
        EnsureCamera();
        _engine.ClearScene(color, depth);
        _sceneOpen = true;
    }

    /// <inheritdoc/>
    public override void DrawMesh(Mesh mesh, in Matrix4x4 world)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        if (mesh.Owner != this || mesh.IsDisposed)
        {
            throw new ArgumentException("The mesh was not created by this canvas or has been disposed.", nameof(mesh));
        }

        if (mesh.Texture is { IsDisposed: true })
        {
            throw new ArgumentException("The mesh maps a texture that has been disposed.", nameof(mesh));
        }

        if (DriverDataOf(mesh) is not VirtioMeshData)
        {
            throw new ArgumentException("The mesh holds no device resources.", nameof(mesh));
        }

        EnsureCamera();
        _engine.DrawMesh(DriverDataOf(mesh)!, world);
        _sceneOpen = true;
    }

    /// <inheritdoc/>
    public override Texture CreateTexture(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        object data = _engine.CreateTextureData(image);
        Texture texture = CreateTextureHandle(image.Width, image.Height, data);
        _engine.BindTexture(texture, data);
        return texture;
    }

    // --- Protected methods ---

    /// <summary>Destroys the engine's context and objects; the canvas is dead afterwards. Thread context.</summary>
    protected override void Disable()
    {
        _engine.Shutdown();
        _sceneOpen = false;
        _displaying3D = false;
        base.Disable();
    }

    /// <summary>Recreates the engine's render targets at the new size and drops the open scene. Thread context.</summary>
    protected override void OnModeChanged()
    {
        base.OnModeChanged();
        _engine.RecreateRenderTargets();
        _sceneOpen = false;
        _displaying3D = false;
        _cameraApplied = false;
    }

    /// <inheritdoc/>
    protected override Mesh CreateMeshCore(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<uint> colors,
        ReadOnlySpan<Vector2> uvs,
        Texture? texture,
        ReadOnlySpan<ushort> indices,
        MeshTopology topology)
    {
        object data = _engine.CreateMeshData(positions, colors, uvs, texture, indices, topology);
        return CreateMeshHandle(positions.Length, indices.Length, texture, topology, data);
    }

    /// <summary>Destroys the mesh's buffer resources and its vertex elements object. Thread context.</summary>
    /// <param name="mesh">The mesh being disposed.</param>
    protected override void DestroyMesh(Mesh mesh)
    {
        _engine.DestroyMeshData(DriverDataOf(mesh)!);
        SetDriverData(mesh, null);
    }

    /// <summary>Destroys the texture's resource and its sampler view. Thread context.</summary>
    /// <param name="texture">The texture being disposed.</param>
    protected override void DestroyTexture(Texture texture)
    {
        _engine.DestroyTextureData(DriverDataOf(texture)!);
        SetDriverData(texture, null);
    }

    /// <summary>Marks the view and projection for re-emission on the next draw. Thread context.</summary>
    protected override void OnCameraChanged()
    {
        base.OnCameraChanged();
        _cameraApplied = false;
    }

    // --- Private methods ---

    /// <summary>Emits the view and projection transforms from the camera unless they are current.</summary>
    private void EnsureCamera()
    {
        if (_cameraApplied)
        {
            return;
        }

        Camera3D camera = Camera;
        Matrix4x4 view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FovY * (MathF.PI / 180f), Width / (float)Height, NearPlane, FarPlane);

        _engine.SetCamera(view, projection);
        _cameraApplied = true;
    }
}
