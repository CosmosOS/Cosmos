using System;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// An image uploaded to a 3D device, ready to be mapped onto meshes. Created
/// with <see cref="Canvas3D.CreateTexture"/>; dispose it to release the
/// device memory it occupies.
/// </summary>
public sealed class Texture : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// The canvas that created this texture; only its meshes can map it.
    /// </summary>
    public Canvas3D Owner { get; }

    /// <summary>
    /// Backend-specific resource data, owned by the canvas that created the
    /// texture, which reaches it through <see cref="Canvas3D.DriverDataOf(Texture)"/>
    /// and <see cref="Canvas3D.SetDriverData(Texture, object?)"/>. The canvas
    /// clears it when it releases the device resource, so the slot is set
    /// exactly while that resource exists.
    /// </summary>
    internal object? DriverData { get; set; }

    /// <summary>
    /// Creates a texture handle owned by <paramref name="owner"/>; the canvas
    /// calls this through <see cref="Canvas3D.CreateTextureHandle"/>.
    /// </summary>
    /// <param name="owner">The canvas that uploaded the texture.</param>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <param name="driverData">The backend's state for the texture.</param>
    internal Texture(Canvas3D owner, int width, int height, object? driverData)
    {
        Owner = owner;
        Width = width;
        Height = height;
        DriverData = driverData;
    }

    /// <summary>
    /// The width of the texture in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the texture in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Whether <see cref="Dispose"/> ran: a mesh mapping a disposed texture can no longer be drawn.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Releases the device memory held by this texture. A mesh that still
    /// maps it can no longer be drawn:
    /// <see cref="Canvas3D.DrawMesh(Mesh, in global::System.Numerics.Matrix4x4)"/>
    /// rejects it.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Owner.DestroyTexture(this);
    }
}
