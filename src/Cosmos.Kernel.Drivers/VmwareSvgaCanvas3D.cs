// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using System.Numerics;
using Cosmos.Kernel.Drivers.Svga;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The <see cref="Canvas3D"/> of a VMware SVGA II adapter, over the
/// SVGA3D command layer and the ring's buffer: 2D drawing is the base's,
/// into its buffer, and <see cref="Display"/> presents the 3D colour
/// target when a scene is open and copies the buffer to the frame
/// otherwise. The canvas owns one SVGA3D context and a colour and depth
/// target pair sized to the canvas, created in the constructor with the
/// scene defaults (the two binds, the viewport, the depth range, seven
/// render states and the untextured stage), recreated when the mode
/// changes and destroyed by <see cref="Disable"/>. Only meaningful when
/// the host negotiated 3D; on QEMU the commands sit in FIFO memory for the
/// wire tests. Thread context.
/// </summary>
internal sealed class VmwareSvgaCanvas3D : Canvas3D
{
    // --- Constants ---

    /// <summary>SVGA3D_SHADEMODE_SMOOTH.</summary>
    private const uint ShadeModeSmooth = 2;

    /// <summary>SVGA3D_CMP_LESS.</summary>
    private const uint CompareLess = 2;

    /// <summary>SVGA3D_FACE_CULL_NONE.</summary>
    private const uint FaceCullNone = 1;

    /// <summary>SVGA3D_TC_SELECTARG1.</summary>
    private const uint CombinerSelectArg1 = 2;

    /// <summary>SVGA3D_TA_DIFFUSE.</summary>
    private const uint ArgDiffuse = 3;

    /// <summary>SVGA3D_TA_TEXTURE.</summary>
    private const uint ArgTexture = 4;

    /// <summary>SVGA3D_INVALID_ID: the unbound texture.</summary>
    private const uint InvalidId = 0xFFFFFFFFu;

    /// <summary>The near plane of the projection, in world units from the camera.</summary>
    private const float NearPlane = 0.1f;

    /// <summary>The far plane of the projection, in world units from the camera.</summary>
    private const float FarPlane = 1000f;

    /// <summary>Bytes of one float, for the stream strides.</summary>
    private const uint FloatBytes = sizeof(float);

    /// <summary>Bytes of one 16-bit index.</summary>
    private const uint IndexBytes = sizeof(ushort);

    /// <summary>Indices per triangle.</summary>
    private const int IndicesPerTriangle = 3;

    /// <summary>Indices per line.</summary>
    private const int IndicesPerLine = 2;

    // --- Private fields ---

    private readonly VmwareSvga3D _driver3D;
    private readonly uint _context;
    private SVGA3dSurfaceImageId _colorTarget;
    private SVGA3dSurfaceImageId _depthTarget;
    private bool _hasRenderTargets;
    private bool _sceneOpen;
    private bool _displaying3D;
    private bool _cameraApplied;
    private bool _textureApplied;
    private Texture? _boundTexture;

    // --- Constructor ---

    /// <summary>
    /// Creates the canvas over the display the adapter publishes, in the
    /// mode the registers hold, and emits the scene setup: the context,
    /// the colour and depth targets sized to the canvas, the two binds, the
    /// viewport, the depth range, the seven render states and the untextured
    /// stage, after the base sized the buffer. Thread context.
    /// </summary>
    /// <param name="state">The adapter's state.</param>
    /// <param name="display">The display the adapter published.</param>
    internal VmwareSvgaCanvas3D(VmwareSvgaState state, DisplayDevice display)
        : base(display)
    {
        _driver3D = new VmwareSvga3D(state.Fifo, state.Vram);
        _context = _driver3D.DefineContext();
        CreateRenderTargets();
        ApplySceneDefaults();
    }

    // --- Properties ---

    /// <summary>
    /// Whether <see cref="GetImage"/> reads the 3D colour target rather
    /// than the buffer: true while a scene is being composed and while the
    /// last presented frame was a 3D scene (a 3D present bypasses the
    /// frame, so VRAM never holds the rendered pixels). Any context.
    /// </summary>
    public bool ReadsFrom3DScene => _sceneOpen || _displaying3D;

    // --- Public methods ---

    /// <summary>Presents the 3D colour target when a scene is open, else copies the buffer to the frame. Thread context.</summary>
    public override void Display()
    {
        if (_sceneOpen)
        {
            _driver3D.Present(_colorTarget, FullRect());
            _sceneOpen = false;
            _displaying3D = true;
            return;
        }

        base.Display();
        _displaying3D = false;
    }

    /// <summary>
    /// Reads a rectangle of pixels back: from the 3D colour target through
    /// a surface DMA while <see cref="ReadsFrom3DScene"/>, else from the
    /// buffer like every other canvas. Thread context.
    /// </summary>
    /// <param name="x">The starting X coordinate of the region to copy.</param>
    /// <param name="y">The starting Y coordinate of the region to copy.</param>
    /// <param name="width">The width of the region to copy.</param>
    /// <param name="height">The height of the region to copy.</param>
    /// <returns>A new <see cref="Bitmap"/> containing the copied region.</returns>
    public override Bitmap GetImage(int x, int y, int width, int height)
    {
        if (!ReadsFrom3DScene)
        {
            return base.GetImage(x, y, width, height);
        }

        Bitmap bitmap = new(width, height, ColorDepth.ColorDepth32);
        if (width > 0 && height > 0 && x >= 0 && y >= 0)
        {
            _driver3D.PresentToImage(_colorTarget, new SVGA3dRect((uint)x, (uint)y, (uint)width, (uint)height), bitmap.RawData);
        }

        return bitmap;
    }

    /// <inheritdoc/>
    public override void ClearScene(Color color, float depth = 1f)
    {
        EnsureCamera();
        _driver3D.Clear3D(_context, ClearFlags.Color | ClearFlags.Depth, FullRect(), (uint)color.ToArgb(), depth);
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

        if (DriverDataOf(mesh) is not SvgaMeshData data)
        {
            throw new ArgumentException("The mesh holds no device resources.", nameof(mesh));
        }

        EnsureCamera();
        BindTexture(mesh.Texture);
        _driver3D.SetTransform(_context, SVGA3dTransformType.SVGA3D_TRANSFORM_WORLD, in world);
        _driver3D.DrawPrimitives(_context, data.Decls, data.Ranges);
        _sceneOpen = true;
    }

    /// <inheritdoc/>
    public override Texture CreateTexture(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        SVGA3dSurfaceImageId surface = _driver3D.DefineSurfaceFromImage(image.RawData, (uint)image.Width, (uint)image.Height);
        return CreateTextureHandle(image.Width, image.Height, surface);
    }

    // --- Protected methods ---

    /// <summary>Destroys the render targets and the context; the canvas is dead afterwards. Thread context.</summary>
    protected override void Disable()
    {
        if (_hasRenderTargets)
        {
            _driver3D.DestroySurface(_colorTarget.sid);
            _driver3D.DestroySurface(_depthTarget.sid);
            _hasRenderTargets = false;
        }

        _driver3D.DestroyContext(_context);
        _sceneOpen = false;
        _displaying3D = false;
        base.Disable();
    }

    /// <summary>Recreates the render targets at the new size and drops the open scene. Thread context.</summary>
    protected override void OnModeChanged()
    {
        base.OnModeChanged();
        if (!_hasRenderTargets)
        {
            return;
        }

        _driver3D.DestroySurface(_colorTarget.sid);
        _driver3D.DestroySurface(_depthTarget.sid);
        CreateRenderTargets();
        _sceneOpen = false;
        _displaying3D = false;
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
        int streamCount = 1 + (colors.IsEmpty ? 0 : 1) + (uvs.IsEmpty ? 0 : 1);
        uint[] streamSids = new uint[streamCount];

        int stream = 0;
        streamSids[stream++] = _driver3D.CreateStaticArrayBuffer(positions);
        if (!colors.IsEmpty)
        {
            streamSids[stream++] = _driver3D.CreateStaticArrayBuffer(colors);
        }

        if (!uvs.IsEmpty)
        {
            streamSids[stream++] = _driver3D.CreateStaticArrayBuffer(uvs);
        }

        uint indexSid = _driver3D.CreateStaticArrayBuffer(indices);
        SvgaMeshData data = BuildMeshData(streamSids, !colors.IsEmpty, !uvs.IsEmpty, indexSid, indices.Length, topology);
        return CreateMeshHandle(positions.Length, indices.Length, texture, topology, data);
    }

    /// <summary>Destroys the mesh's stream and index surfaces. Thread context.</summary>
    /// <param name="mesh">The mesh being disposed.</param>
    protected override void DestroyMesh(Mesh mesh)
    {
        if (DriverDataOf(mesh) is SvgaMeshData data)
        {
            for (int i = 0; i < data.StreamSids.Length; i++)
            {
                _driver3D.DestroySurface(data.StreamSids[i]);
            }

            _driver3D.DestroySurface(data.IndexSid);
        }

        SetDriverData(mesh, null);
    }

    /// <summary>Destroys the texture's surface and unbinds it when bound. Thread context.</summary>
    /// <param name="texture">The texture being disposed.</param>
    protected override void DestroyTexture(Texture texture)
    {
        if (DriverDataOf(texture) is SVGA3dSurfaceImageId surface)
        {
            _driver3D.DestroySurface(surface.sid);
        }

        SetDriverData(texture, null);
        if (ReferenceEquals(_boundTexture, texture))
        {
            _boundTexture = null;
            _textureApplied = false;
        }
    }

    /// <summary>Marks the view and projection for re-emission on the next draw. Thread context.</summary>
    protected override void OnCameraChanged()
    {
        base.OnCameraChanged();
        _cameraApplied = false;
    }

    // --- Private methods ---

    /// <summary>Defines the colour and depth targets at the canvas size, binds them, and sets the viewport and depth range.</summary>
    private void CreateRenderTargets()
    {
        uint width = (uint)Width;
        uint height = (uint)Height;

        _colorTarget = _driver3D.DefineSurface(width, height, SVGA3dSurfaceFormat.SVGA3D_X8R8G8B8);
        _depthTarget = _driver3D.DefineSurface(width, height, SVGA3dSurfaceFormat.SVGA3D_Z_D16);

        _driver3D.SetRenderTarget(_context, SVGA3dRenderTargetType.Color, _colorTarget);
        _driver3D.SetRenderTarget(_context, SVGA3dRenderTargetType.Depth, _depthTarget);
        _driver3D.SetViewport(_context, FullRect());
        _driver3D.SetDepthRange(_context, 0f, 1f);

        _hasRenderTargets = true;
        _cameraApplied = false;
    }

    /// <summary>The seven fixed-function render states of the scene and the untextured stage.</summary>
    private void ApplySceneDefaults()
    {
        ReadOnlySpan<SVGA3dRenderState> states =
        [
            new(SVGA3dRenderStateName.SVGA3D_RS_SHADEMODE, ShadeModeSmooth),
            new(SVGA3dRenderStateName.SVGA3D_RS_LIGHTINGENABLE, 0u),
            new(SVGA3dRenderStateName.SVGA3D_RS_BLENDENABLE, 0u),
            new(SVGA3dRenderStateName.SVGA3D_RS_ZENABLE, 1u),
            new(SVGA3dRenderStateName.SVGA3D_RS_ZWRITEENABLE, 1u),
            new(SVGA3dRenderStateName.SVGA3D_RS_ZFUNC, CompareLess),
            new(SVGA3dRenderStateName.SVGA3D_RS_CULLMODE, FaceCullNone),
        ];
        _driver3D.SetRenderState(_context, states);
        BindTexture(null);
    }

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

        _driver3D.SetTransform(_context, SVGA3dTransformType.SVGA3D_TRANSFORM_VIEW, in view);
        _driver3D.SetTransform(_context, SVGA3dTransformType.SVGA3D_TRANSFORM_PROJECTION, in projection);
        _cameraApplied = true;
    }

    /// <summary>Binds a texture to stage 0, or the diffuse colour when null, unless that binding is current.</summary>
    private void BindTexture(Texture? texture)
    {
        if (_textureApplied && ReferenceEquals(texture, _boundTexture))
        {
            return;
        }

        if (texture is null)
        {
            ReadOnlySpan<SVGA3dTextureState> untextured =
            [
                new(SVGA3dTextureStateName.SVGA3D_TS_BIND_TEXTURE, InvalidId),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLOROP, CombinerSelectArg1),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLORARG1, ArgDiffuse),
                new(SVGA3dTextureStateName.SVGA3D_TS_ALPHAARG1, ArgDiffuse),
            ];
            _driver3D.SetTextureState(_context, untextured);
        }
        else
        {
            uint sid = DriverDataOf(texture) is SVGA3dSurfaceImageId surface ? surface.sid : InvalidId;
            ReadOnlySpan<SVGA3dTextureState> textured =
            [
                new(SVGA3dTextureStateName.SVGA3D_TS_BIND_TEXTURE, sid),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLOROP, CombinerSelectArg1),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLORARG1, ArgTexture),
                new(SVGA3dTextureStateName.SVGA3D_TS_ALPHAARG1, ArgTexture),
            ];
            _driver3D.SetTextureState(_context, textured);
        }

        _boundTexture = texture;
        _textureApplied = true;
    }

    /// <summary>The whole canvas as a rectangle.</summary>
    private SVGA3dRect FullRect() => new(0, 0, (uint)Width, (uint)Height);

    /// <summary>
    /// Builds the draw payload for a mesh whose attribute streams were
    /// uploaded to the given buffer surfaces, in stream order: position,
    /// then colours when present, then texture coordinates when present,
    /// and one indexed 16-bit primitive range.
    /// </summary>
    private static SvgaMeshData BuildMeshData(uint[] streamSids, bool hasColors, bool hasUvs, uint indexSid, int indexCount, MeshTopology topology)
    {
        SVGA3dVertexDecl[] decls = new SVGA3dVertexDecl[streamSids.Length];

        int stream = 0;
        decls[stream] = MakeDecl(SVGA3dDeclType.SVGA3D_DECLTYPE_FLOAT3, SVGA3dDeclUsage.SVGA3D_DECLUSAGE_POSITION, streamSids[stream], 3 * FloatBytes);
        stream++;

        if (hasColors)
        {
            decls[stream] = MakeDecl(SVGA3dDeclType.SVGA3D_DECLTYPE_D3DCOLOR, SVGA3dDeclUsage.SVGA3D_DECLUSAGE_COLOR, streamSids[stream], sizeof(uint));
            stream++;
        }

        if (hasUvs)
        {
            decls[stream] = MakeDecl(SVGA3dDeclType.SVGA3D_DECLTYPE_FLOAT2, SVGA3dDeclUsage.SVGA3D_DECLUSAGE_TEXCOORD, streamSids[stream], 2 * FloatBytes);
            stream++;
        }

        bool lines = topology == MeshTopology.Lines;
        SVGA3dPrimitiveRange[] ranges =
        [
            new()
            {
                primType = lines
                    ? SVGA3dPrimitiveType.SVGA3D_PRIMITIVE_LINELIST
                    : SVGA3dPrimitiveType.SVGA3D_PRIMITIVE_TRIANGLELIST,
                primitiveCount = (uint)(indexCount / (lines ? IndicesPerLine : IndicesPerTriangle)),
                indexArray = new SVGA3dArray { surfaceId = indexSid, offset = 0, stride = IndexBytes },
                indexWidth = IndexBytes,
                indexBias = 0,
            },
        ];

        return new SvgaMeshData(streamSids, indexSid, decls, ranges);
    }

    /// <summary>One vertex declaration reading a whole buffer surface at the given stride.</summary>
    private static SVGA3dVertexDecl MakeDecl(SVGA3dDeclType type, SVGA3dDeclUsage usage, uint surfaceId, uint stride)
    {
        return new SVGA3dVertexDecl
        {
            identity = new SVGA3dVertexArrayIdentity
            {
                type = type,
                method = SVGA3dDeclMethod.SVGA3D_DECLMETHOD_DEFAULT,
                usage = usage,
                usageIndex = 0,
            },
            array = new SVGA3dArray { surfaceId = surfaceId, offset = 0, stride = stride },
            rangeHint = new SVGA3dArrayRangeHint { first = 0, last = 0 },
        };
    }

    /// <summary>
    /// The device resources of a mesh: one buffer surface per attribute
    /// stream, the index surface, and the pre-built draw payload.
    /// </summary>
    private sealed class SvgaMeshData
    {
        /// <summary>Records the resources.</summary>
        /// <param name="streamSids">One buffer surface id per vertex attribute stream.</param>
        /// <param name="indexSid">The buffer surface id holding the indices.</param>
        /// <param name="decls">The vertex declarations submitted with every draw.</param>
        /// <param name="ranges">The primitive range submitted with every draw.</param>
        internal SvgaMeshData(uint[] streamSids, uint indexSid, SVGA3dVertexDecl[] decls, SVGA3dPrimitiveRange[] ranges)
        {
            StreamSids = streamSids;
            IndexSid = indexSid;
            Decls = decls;
            Ranges = ranges;
        }

        /// <summary>One buffer surface id per vertex attribute stream.</summary>
        internal uint[] StreamSids { get; }

        /// <summary>The buffer surface id holding the indices.</summary>
        internal uint IndexSid { get; }

        /// <summary>The vertex declarations submitted with every draw.</summary>
        internal SVGA3dVertexDecl[] Decls { get; }

        /// <summary>The primitive range submitted with every draw.</summary>
        internal SVGA3dPrimitiveRange[] Ranges { get; }
    }
}
