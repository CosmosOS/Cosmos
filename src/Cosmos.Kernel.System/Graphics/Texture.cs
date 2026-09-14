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
    /// The canvas that created this texture.
    /// </summary>
    internal Canvas3D Owner { get; }

    /// <summary>
    /// Backend-specific resource data, owned by the canvas that created the
    /// texture. The canvas clears it when it releases the device resource,
    /// so the slot is set exactly while that resource exists.
    /// </summary>
    internal object? DriverData { get; set; }

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

    internal bool IsDisposed => _disposed;

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
