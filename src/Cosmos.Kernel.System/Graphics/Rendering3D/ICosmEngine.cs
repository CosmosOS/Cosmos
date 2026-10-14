// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;

namespace Cosmos.Kernel.System.Graphics.Rendering3D;

/// <summary>
/// The universal 3D rendering layer a canvas can carry. Drivers that can do
/// 3D (virtio-gpu via virgl, VMware SVGA II, a software rasterizer, ...)
/// implement this interface, and the specific canvas implementation installs
/// its engine from its constructor based on the driver it was built on.
/// After that the engine is an independent layer: it renders device-side and
/// presents its result onto whatever canvas owns it, so 3D code never talks
/// to a canvas type directly.
/// </summary>
/// <remarks>
/// Resource data flows through <see langword="object"/> handles so the
/// interface stays driver agnostic; the opaque values are created and
/// consumed by the same engine instance and must not be shared between
/// engines.
/// </remarks>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface ICosmEngine
{
    /// <summary>The engine name, used for logging and canvas naming.</summary>
    string Name { get; }

    /// <summary>Whether the engine initialized against its driver successfully.</summary>
    bool Ready { get; }

    /// <summary>
    /// Uploads a per-vertex-colored or textured mesh and returns the opaque
    /// handle to draw it with.
    /// </summary>
    object CreateMeshData(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<uint> colors,
        ReadOnlySpan<Vector2> uvs,
        Texture? texture,
        ReadOnlySpan<ushort> indices,
        MeshTopology topology);

    /// <summary>Releases the device resources of a mesh handle.</summary>
    void DestroyMeshData(object meshData);

    /// <summary>Uploads an image as a texture and returns the opaque handle.</summary>
    object CreateTextureData(Image image);

    /// <summary>Releases the device resources of a texture handle.</summary>
    void DestroyTextureData(object textureData);

    /// <summary>
    /// Updates the view-projection the draw calls are seen through, in
    /// System.Numerics row-vector order (<c>world * view * projection</c>
    /// composes into the clip transform).
    /// </summary>
    void SetCamera(in Matrix4x4 view, in Matrix4x4 projection);

    /// <summary>Clears the color and depth targets to start a frame.</summary>
    void ClearScene(Color color, float depth);

    /// <summary>Draws a mesh transformed by <paramref name="world"/>.</summary>
    void DrawMesh(object meshData, in Matrix4x4 world);

    /// <summary>
    /// Presents the rendered frame onto the canvas surface the engine is
    /// attached to.
    /// </summary>
    void Present();
}
