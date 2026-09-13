using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// <see cref="Canvas3D"/> implementation on the VMware SVGA II adapter's
/// SVGA3D command layer. Only constructed when the device negotiated 3D
/// support (real VMware); 2D-only devices get <see cref="SvgaIICanvas"/>
/// instead. The canvas owns one SVGA3D context and a color+depth render
/// target pair sized to the current mode; <see cref="Display"/> presents the
/// 3D target when the frame rendered 3D, and swaps the 2D framebuffer
/// otherwise.
/// </summary>
internal sealed class SvgaII3DCanvas : Canvas3D
{
    // SVGA3D fixed-function values (VMware svga3d_reg.h) not mirrored as HAL enums.
    private const uint ShadeModeSmooth = 2;    // SVGA3D_SHADEMODE_SMOOTH
    private const uint CompareLess = 2;        // SVGA3D_CMP_LESS
    private const uint FaceCullNone = 1;       // SVGA3D_FACE_CULL_NONE
    private const uint CombinerSelectArg1 = 2; // SVGA3D_TC_SELECTARG1
    private const uint ArgDiffuse = 3;         // SVGA3D_TA_DIFFUSE
    private const uint ArgTexture = 4;         // SVGA3D_TA_TEXTURE
    private const uint InvalidId = 0xFFFFFFFFu; // SVGA3D_INVALID_ID

    private const float NearPlane = 0.1f;
    private const float FarPlane = 1000f;

    private readonly VMWareSVGAII3D _driver3D;
    private readonly uint _context;
    private SVGA3dSurfaceImageId _colorTarget;
    private SVGA3dSurfaceImageId _depthTarget;
    private bool _hasRenderTargets;
    private bool _sceneOpen;
    private bool _displaying3D;
    private bool _cameraApplied;
    private bool _textureApplied;
    private Texture? _boundTexture;

    /// <summary>
    /// The 2D display driver, bound to the SVGA II PCI device.
    /// </summary>
    public SvgaIIDriver Driver { get; }

    /// <summary>
    /// Creates a canvas on the given SVGA II driver in the given mode. The
    /// driver must have negotiated 3D support.
    /// </summary>
    /// <param name="driver">The initialized VMware SVGA II display driver.</param>
    /// <param name="mode">The graphics mode to set; must be one of <see cref="AvailableModes"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The mode is not supported by this driver.</exception>
    public SvgaII3DCanvas(SvgaIIDriver driver, Mode mode)
        : this(driver, mode, applyMode: true)
    {
    }

    /// <summary>
    /// Creates the canvas, optionally without programming the mode.
    /// <paramref name="applyMode"/> is false only in the FIFO wire tests:
    /// skipping SetMode leaves the device disabled, so the 3D setup commands
    /// sit inert in FIFO memory for inspection instead of being consumed
    /// (QEMU's vmware-svga cannot parse them).
    /// </summary>
    internal SvgaII3DCanvas(SvgaIIDriver driver, Mode mode, bool applyMode)
        : base(mode)
    {
        Driver = driver;
        ThrowIfModeIsNotValid(mode);

        if (applyMode)
        {
            SvgaIIRender.ApplyMode(this, driver, mode);
        }

        _driver3D = new VMWareSVGAII3D(driver);
        _context = _driver3D.DefineContext();
        CreateRenderTargets();
        ApplySceneDefaults();
    }

    /// <inheritdoc />
    public override string Name => "VMWareSVGAII3D";

    /// <summary>
    /// Gets or sets the current graphics mode. Setting the mode recreates the
    /// 3D render targets at the new resolution.
    /// </summary>
    public override Mode Mode
    {
        get => base.Mode;
        protected internal set
        {
            ThrowIfModeIsNotValid(value);
            base.Mode = value;
            SvgaIIRender.ApplyMode(this, Driver, value);

            if (_hasRenderTargets)
            {
                _driver3D.DestroySurface(_colorTarget.sid);
                _driver3D.DestroySurface(_depthTarget.sid);
                CreateRenderTargets();
                _sceneOpen = false;
                _displaying3D = false;
            }
        }
    }

    /// <inheritdoc />
    public override Mode DefaultGraphicsMode => SvgaIIRender.DefaultMode;

    /// <inheritdoc />
    public override IReadOnlyList<Mode> AvailableModes { get; } = SvgaIIRender.CreateAvailableModes();

    /// <inheritdoc />
    internal override void Disable()
    {
        Driver.Disable();
    }

    /// <inheritdoc />
    public override void ClearScene(Color color, float depth = 1f)
    {
        EnsureCamera();
        _driver3D.Clear3D(_context, ClearFlags.Color | ClearFlags.Depth, FullRect(), (uint)color.ToArgb(), depth);
        _sceneOpen = true;
    }

    /// <inheritdoc />
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

        EnsureCamera();
        BindTexture(mesh.Texture);
        _driver3D.SetTransform(_context, SVGA3dTransformType.SVGA3D_TRANSFORM_WORLD, world);

        SvgaMeshData data = (SvgaMeshData)mesh.DriverData!;
        _driver3D.DrawPrimitives(_context, data.Decls, data.Ranges);
        _sceneOpen = true;
    }

    /// <inheritdoc />
    public override Texture CreateTexture(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        SVGA3dSurfaceImageId surface = _driver3D.DefineSurfaceFromImage(image.RawData, (uint)image.Width, (uint)image.Height);
        return new Texture(this, image.Width, image.Height, surface);
    }

    /// <summary>
    /// Presents the 3D scene when the frame rendered one, and swaps the 2D
    /// framebuffer otherwise.
    /// </summary>
    public override void Display()
    {
        if (_sceneOpen)
        {
            _driver3D.Present(_colorTarget, FullRect());
            _sceneOpen = false;
            _displaying3D = true;
        }
        else
        {
            Driver.Swap();
            _displaying3D = false;
        }
    }

    /// <summary>
    /// Whether <see cref="GetImage"/> reads the 3D color target rather than
    /// the 2D framebuffer: true while a scene is being composed and while
    /// the last presented frame was a 3D scene (a 3D present bypasses the
    /// guest framebuffer, so the VRAM never holds the rendered pixels).
    /// </summary>
    internal bool ReadsFrom3DScene => _sceneOpen || _displaying3D;

    private protected override Mesh CreateMeshCore(
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

        return new Mesh(this, positions.Length, indices.Length, texture, topology)
        {
            DriverData = BuildMeshData(streamSids, !colors.IsEmpty, !uvs.IsEmpty, indexSid, indices.Length, topology),
        };
    }

    /// <summary>
    /// Builds the draw-command payload for a mesh whose attribute streams
    /// were uploaded to the given buffer surfaces, in stream order: position,
    /// then colors when present, then texture coordinates when present. Pure;
    /// separated from the upload so the FIFO wire tests can validate the
    /// layout with known surface ids (the upload's DMA/fence path cannot run
    /// on a disabled device).
    /// </summary>
    internal static SvgaMeshData BuildMeshData(uint[] streamSids, bool hasColors, bool hasUvs, uint indexSid, int indexCount, MeshTopology topology)
    {
        SVGA3dVertexDecl[] decls = new SVGA3dVertexDecl[streamSids.Length];

        int stream = 0;
        decls[stream] = MakeDecl(SVGA3dDeclType.SVGA3D_DECLTYPE_FLOAT3, SVGA3dDeclUsage.SVGA3D_DECLUSAGE_POSITION, streamSids[stream], 3 * sizeof(float));
        stream++;

        if (hasColors)
        {
            decls[stream] = MakeDecl(SVGA3dDeclType.SVGA3D_DECLTYPE_D3DCOLOR, SVGA3dDeclUsage.SVGA3D_DECLUSAGE_COLOR, streamSids[stream], sizeof(uint));
            stream++;
        }

        if (hasUvs)
        {
            decls[stream] = MakeDecl(SVGA3dDeclType.SVGA3D_DECLTYPE_FLOAT2, SVGA3dDeclUsage.SVGA3D_DECLUSAGE_TEXCOORD, streamSids[stream], 2 * sizeof(float));
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
                primitiveCount = (uint)(indexCount / (lines ? 2 : 3)),
                indexArray = new() { surfaceId = indexSid, stride = sizeof(ushort) },
                indexWidth = sizeof(ushort),
            },
        ];

        return new SvgaMeshData
        {
            StreamSids = streamSids,
            IndexSid = indexSid,
            Decls = decls,
            Ranges = ranges,
        };
    }

    internal override void DestroyMesh(Mesh mesh)
    {
        SvgaMeshData data = (SvgaMeshData)mesh.DriverData!;

        foreach (uint sid in data.StreamSids)
        {
            _driver3D.DestroySurface(sid);
        }

        _driver3D.DestroySurface(data.IndexSid);
        mesh.DriverData = null;
    }

    internal override void DestroyTexture(Texture texture)
    {
        SVGA3dSurfaceImageId surface = (SVGA3dSurfaceImageId)texture.DriverData!;
        _driver3D.DestroySurface(surface.sid);
        texture.DriverData = null;

        if (ReferenceEquals(_boundTexture, texture))
        {
            _boundTexture = null;
            _textureApplied = false;
        }
    }

    private protected override void OnCameraChanged()
    {
        _cameraApplied = false;
    }

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

    private void ApplySceneDefaults()
    {
        _driver3D.SetRenderState(_context,
        [
            new(SVGA3dRenderStateName.SVGA3D_RS_SHADEMODE, ShadeModeSmooth),
            new(SVGA3dRenderStateName.SVGA3D_RS_LIGHTINGENABLE, 0u),
            new(SVGA3dRenderStateName.SVGA3D_RS_BLENDENABLE, 0u),
            new(SVGA3dRenderStateName.SVGA3D_RS_ZENABLE, 1u),
            new(SVGA3dRenderStateName.SVGA3D_RS_ZWRITEENABLE, 1u),
            new(SVGA3dRenderStateName.SVGA3D_RS_ZFUNC, CompareLess),
            new(SVGA3dRenderStateName.SVGA3D_RS_CULLMODE, FaceCullNone),
        ]);

        BindTexture(null);
    }

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

        _driver3D.SetTransform(_context, SVGA3dTransformType.SVGA3D_TRANSFORM_VIEW, view);
        _driver3D.SetTransform(_context, SVGA3dTransformType.SVGA3D_TRANSFORM_PROJECTION, projection);
        _cameraApplied = true;
    }

    private void BindTexture(Texture? texture)
    {
        if (_textureApplied && ReferenceEquals(texture, _boundTexture))
        {
            return;
        }

        if (texture == null)
        {
            _driver3D.SetTextureState(_context,
            [
                new(SVGA3dTextureStateName.SVGA3D_TS_BIND_TEXTURE, InvalidId),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLOROP, CombinerSelectArg1),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLORARG1, ArgDiffuse),
                new(SVGA3dTextureStateName.SVGA3D_TS_ALPHAARG1, ArgDiffuse),
            ]);
        }
        else
        {
            SVGA3dSurfaceImageId surface = (SVGA3dSurfaceImageId)texture.DriverData!;
            _driver3D.SetTextureState(_context,
            [
                new(SVGA3dTextureStateName.SVGA3D_TS_BIND_TEXTURE, surface.sid),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLOROP, CombinerSelectArg1),
                new(SVGA3dTextureStateName.SVGA3D_TS_COLORARG1, ArgTexture),
                new(SVGA3dTextureStateName.SVGA3D_TS_ALPHAARG1, ArgTexture),
            ]);
        }

        _boundTexture = texture;
        _textureApplied = true;
    }

    private SVGA3dRect FullRect() => new(0, 0, (uint)Width, (uint)Height);

    private static SVGA3dVertexDecl MakeDecl(SVGA3dDeclType type, SVGA3dDeclUsage usage, uint surfaceId, uint stride)
    {
        return new SVGA3dVertexDecl
        {
            identity = new SVGA3dVertexArrayIdentity { type = type, usage = usage },
            array = new SVGA3dArray { surfaceId = surfaceId, stride = stride },
        };
    }

    /// <inheritdoc />
    public override void DrawPoint(Color color, int x, int y)
    {
        SvgaIIRender.DrawPoint(this, Driver, color, x, y);
    }

    /// <inheritdoc />
    public override void DrawPoint(uint color, int x, int y)
    {
        SvgaIIRender.DrawRawPoint(this, Driver, color, x, y);
    }

    /// <inheritdoc />
    public override void DrawPoint(int color, int x, int y)
    {
        SvgaIIRender.DrawRawPoint(this, Driver, (uint)color, x, y);
    }

    /// <inheritdoc />
    public override void DrawArray(Color[] colors, int x, int y, int width, int height)
    {
        SvgaIIRender.DrawArray(this, colors, x, y, width, height);
    }

    /// <inheritdoc />
    public override void DrawArray(int[] colors, int x, int y, int width, int height)
    {
        Driver.CopyBuffer(colors.AsMemory(), x, y, width, height);
    }

    /// <inheritdoc />
    public override void DrawArray(int[] colors, int x, int y, int width, int height, int startIndex)
    {
        Driver.CopyBuffer(colors.AsMemory(startIndex), x, y, width, height);
    }

    /// <inheritdoc />
    public override void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
    {
        SvgaIIRender.DrawFilledRectangle(this, Driver, color, xStart, yStart, width, height);
    }

    /// <inheritdoc />
    public override void DrawRectangle(Color color, int x, int y, int width, int height)
    {
        SvgaIIRender.DrawRectangle(this, color, x, y, width, height);
    }

    /// <inheritdoc />
    public override void Clear(int color)
    {
        Driver.ClearScreen((uint)color);
    }

    /// <inheritdoc />
    public override void Clear(Color color)
    {
        Driver.ClearScreen((uint)color.ToArgb());
    }

    /// <summary>
    /// Whether the device composes a 32-bit alpha hardware cursor on the host
    /// side. When true, callers can define a shape once with
    /// <see cref="DefineAlphaCursor"/> and move it with <see cref="SetCursor"/>
    /// instead of blitting a software cursor every frame.
    /// </summary>
    public bool HasHardwareCursor => Driver.HasAlphaCursor;

    /// <summary>
    /// Moves the hardware cursor and toggles its visibility.
    /// </summary>
    /// <param name="visible">Whether the cursor is shown.</param>
    /// <param name="x">The X coordinate of the cursor.</param>
    /// <param name="y">The Y coordinate of the cursor.</param>
    public void SetCursor(bool visible, int x, int y)
    {
        Driver.SetCursor(visible, (uint)x, (uint)y);
    }

    /// <summary>
    /// Define the hardware cursor shape. <paramref name="data"/> is
    /// width×height premultiplied 32-bit BGRA pixels.
    /// </summary>
    public void DefineAlphaCursor(int hotspotX, int hotspotY, int width, int height, int[] data)
    {
        Driver.DefineAlphaCursor((uint)hotspotX, (uint)hotspotY, (uint)width, (uint)height, data);
    }

    /// <summary>
    /// Defines the default hardware cursor shape on the device.
    /// </summary>
    public void CreateCursor()
    {
        Driver.DefineCursor();
    }

    /// <inheritdoc />
    public override void CopyPixels(int srcX, int srcY, int dstX, int dstY, int width, int height)
    {
        SvgaIIRender.CopyPixels(this, Driver, srcX, srcY, dstX, dstY, width, height);
    }

    /// <inheritdoc />
    public override Color GetPointColor(int x, int y)
    {
        return Color.FromArgb((int)Driver.GetPixel(x, y));
    }

    /// <inheritdoc />
    public override int GetRawPointColor(int x, int y)
    {
        return (int)Driver.GetPixel(x, y);
    }

    /// <summary>
    /// Reads back a rectangle of pixels. While a 3D scene is composed or
    /// displayed (see <see cref="ReadsFrom3DScene"/>), the pixels come from
    /// the 3D color target through a surface DMA readback; otherwise from
    /// the 2D framebuffer like every other canvas.
    /// </summary>
    public override Bitmap GetImage(int x, int y, int width, int height)
    {
        if (!ReadsFrom3DScene)
        {
            return SvgaIIRender.GetImage(this, Driver, x, y, width, height);
        }

        int[] data = new int[width * height];
        int[]? pixels = _driver3D.PresentToImage(_colorTarget, new SVGA3dRect((uint)x, (uint)y, (uint)width, (uint)height));

        if (pixels != null)
        {
            // PresentToImage returns the driver's reused readback buffer;
            // copy so the bitmap survives the next readback.
            Array.Copy(pixels, data, data.Length);
        }

        Bitmap bitmap = new Bitmap(width, height, ColorDepth.ColorDepth32)
        {
            RawData = data,
        };

        return bitmap;
    }

    /// <inheritdoc />
    public override void DrawImage(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        SvgaIIRender.DrawImage(this, Driver, image, x, y, preventOffBoundPixels);
    }

    /// <inheritdoc />
    public override void CroppedDrawImage(Image image, int x, int y, int width, int height, bool preventOffBoundPixels = true)
    {
        SvgaIIRender.CroppedDrawImage(this, Driver, image, x, y, width, height, preventOffBoundPixels);
    }
}

/// <summary>
/// Device-side resources of a mesh on the SVGA3D backend: one vertex-stream
/// surface per attribute, the index surface, and the pre-built draw command
/// payload.
/// </summary>
internal sealed class SvgaMeshData
{
    /// <summary>One buffer surface id per vertex attribute stream.</summary>
    public uint[] StreamSids = [];

    /// <summary>The buffer surface id holding the indices.</summary>
    public uint IndexSid;

    /// <summary>The vertex declarations submitted with every draw.</summary>
    public SVGA3dVertexDecl[] Decls = [];

    /// <summary>The primitive range submitted with every draw.</summary>
    public SVGA3dPrimitiveRange[] Ranges = [];
}
