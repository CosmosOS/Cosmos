using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.System.Graphics;
using Cosmos.TestRunner.Framework;

namespace Cosmos.Kernel.Tests.Graphic;

/// <summary>
/// The VMware SVGA II adapter through the facets its display publishes:
/// the <see cref="ISvgaAdapter"/> seam, the modes and the hardware cursor,
/// then the SVGA3D command layer on the wire. QEMU's vmware-svga adapter
/// negotiates no 3D, so the commands can never run host-side; what CI can
/// pin down is the guest half of the contract: every call must place a
/// correctly formed command (protocol id, body size, field order and
/// packing) in the FIFO. The wire tests hold the scanout off through
/// <see cref="ISvgaAdapter.SetEnabled"/>, since QEMU consumes the FIFO only
/// while the adapter is enabled, so the commands sit inert in FIFO memory
/// for inspection and are discarded by rewinding NEXT_CMD afterwards. The
/// 3D canvas comes from <see cref="ISvgaAdapter.CreateCanvas3D"/>, which
/// neither re-enables the adapter nor re-initialises the FIFO. Expected ids,
/// sizes, formats and offsets are hard-coded from the SVGA3D protocol
/// (svga3d_reg.h), never read back from the kernel's own enums, so a wrong
/// enum value is caught instead of being compared against itself.
/// </summary>
public static class Svga3DTests
{
    /// <summary>SVGA_3D_CMD_SURFACE_DEFINE (SVGA_3D_CMD_BASE + 0).</summary>
    private const uint CmdSurfaceDefine = 1040;

    /// <summary>SVGA_3D_CMD_SURFACE_DESTROY (SVGA_3D_CMD_BASE + 1).</summary>
    private const uint CmdSurfaceDestroy = 1041;

    /// <summary>SVGA_3D_CMD_SURFACE_DMA (SVGA_3D_CMD_BASE + 4).</summary>
    private const uint CmdSurfaceDma = 1044;

    /// <summary>SVGA_3D_CMD_CONTEXT_DEFINE (SVGA_3D_CMD_BASE + 5).</summary>
    private const uint CmdContextDefine = 1045;

    /// <summary>SVGA_3D_CMD_SETTRANSFORM (SVGA_3D_CMD_BASE + 7).</summary>
    private const uint CmdSetTransform = 1047;

    /// <summary>SVGA_3D_CMD_SETZRANGE (SVGA_3D_CMD_BASE + 8).</summary>
    private const uint CmdSetZRange = 1048;

    /// <summary>SVGA_3D_CMD_SETRENDERSTATE (SVGA_3D_CMD_BASE + 9).</summary>
    private const uint CmdSetRenderState = 1049;

    /// <summary>SVGA_3D_CMD_SETRENDERTARGET (SVGA_3D_CMD_BASE + 10).</summary>
    private const uint CmdSetRenderTarget = 1050;

    /// <summary>SVGA_3D_CMD_SETTEXTURESTATE (SVGA_3D_CMD_BASE + 11).</summary>
    private const uint CmdSetTextureState = 1051;

    /// <summary>SVGA_3D_CMD_SETVIEWPORT (SVGA_3D_CMD_BASE + 15).</summary>
    private const uint CmdSetViewport = 1055;

    /// <summary>SVGA_3D_CMD_CLEAR (SVGA_3D_CMD_BASE + 17).</summary>
    private const uint CmdClear = 1057;

    /// <summary>SVGA_3D_CMD_DRAW_PRIMITIVES (SVGA_3D_CMD_BASE + 23).</summary>
    private const uint CmdDrawPrimitives = 1063;

    /// <summary>SVGA3D_X8R8G8B8, the colour target's format.</summary>
    private const uint FormatX8R8G8B8 = 1;

    /// <summary>SVGA3D_A8R8G8B8, the format of an uploaded texture.</summary>
    private const uint FormatA8R8G8B8 = 2;

    /// <summary>SVGA3D_Z_D16, the depth target's format.</summary>
    private const uint FormatZD16 = 8;

    /// <summary>SVGA3D_BUFFER, the format of a vertex or index buffer surface.</summary>
    private const uint FormatBuffer = 37;

    /// <summary>SVGA3D_WRITE_HOST_VRAM, the direction of an upload.</summary>
    private const uint TransferWriteHostVram = 1;

    /// <summary>SVGA_GMR_FRAMEBUFFER, the guest memory region the DMA scratch lives in.</summary>
    private const uint FramebufferGmr = 0xFFFFFFFEu;

    /// <summary>Size of SVGA3dCmdHeader (id + size) preceding every 3D command body.</summary>
    private const uint HeaderBytes = 8;

    /// <summary>Body size of a SURFACE_DEFINE with one mip level: sid, flags, format, six faces, one size.</summary>
    private const uint SurfaceDefineBytes = 48;

    /// <summary>Body size of a single-box SURFACE_DMA: guest image, host image, transfer, one copy box.</summary>
    private const uint SurfaceDmaBytes = 64;

    /// <summary>Body size of a SETTRANSFORM: cid, type, sixteen floats.</summary>
    private const uint TransformBytes = 72;

    /// <summary>Body size of the cube's DRAW_PRIMITIVES: cid, two counts, two declarations of 36 bytes, one range of 28.</summary>
    private const uint CubeDrawBytes = 112;

    /// <summary>Bytes of one uploaded stream: the surface definition and the DMA, headers included.</summary>
    private const uint UploadBytes = HeaderBytes + SurfaceDefineBytes + HeaderBytes + SurfaceDmaBytes;

    /// <summary>The context id of a fresh 3D canvas.</summary>
    private const uint ContextId = 1;

    /// <summary>The colour target's surface id on a fresh 3D canvas.</summary>
    private const uint ColorTargetSid = 1;

    /// <summary>The depth target's surface id on a fresh 3D canvas.</summary>
    private const uint DepthTargetSid = 2;

    /// <summary>The surface id of the cube's position stream, the first upload after the targets.</summary>
    private const uint PositionSid = 3;

    /// <summary>The surface id of the cube's colour stream.</summary>
    private const uint ColorSid = 4;

    /// <summary>The surface id of the cube's index buffer.</summary>
    private const uint IndexSid = 5;

    /// <summary>The surface id of the texture created after the cube.</summary>
    private const uint TextureSid = 6;

    /// <summary>Vertices of the cube.</summary>
    private const int CubeVertexCount = 8;

    /// <summary>Indices of the cube: twelve triangles.</summary>
    private const int CubeIndexCount = 36;

    /// <summary>Bytes of one position: three floats.</summary>
    private const uint PositionBytes = 12;

    /// <summary>Bytes of one packed colour.</summary>
    private const uint ColorBytes = 4;

    /// <summary>Bytes of one 16-bit index.</summary>
    private const uint IndexBytes = 2;

    /// <summary>Width of the mode the console chose, which the modes facet test expects.</summary>
    private const int ConsoleWidth = 1024;

    /// <summary>Height of the mode the console chose.</summary>
    private const int ConsoleHeight = 768;

    /// <summary>Bits per pixel of every listed mode.</summary>
    private const int ListedBitsPerPixel = 32;

    /// <summary>Side of the square image the cursor and texture tests build.</summary>
    private const int SmallImageSide = 2;

    private static ISvgaAdapter? s_adapter;
    private static Canvas3D? s_canvas;
    private static Mesh? s_cube;
    private static bool s_wasEnabled;

    // ==================== Facets ====================

    /// <summary>
    /// The adapter facet is on the primary display and reports what QEMU's
    /// adapter is: capabilities present, a FIFO with room, no SVGA3D
    /// negotiated (so no 3D factory), and the scanout enabled since the
    /// console programmed a mode.
    /// </summary>
    public static void TestAdapterFacet()
    {
        if (!TryGetAdapter(out DisplayDevice? primary, out ISvgaAdapter? adapter))
        {
            return;
        }

        Assert.True(adapter.Capabilities != 0, "the adapter reports capabilities");
        Assert.True(adapter.FifoMin < adapter.FifoMax, "the FIFO has room past its registers");
        Assert.False(adapter.Is3DNegotiated, "QEMU negotiates no SVGA3D version");
        Assert.Equal(0u, adapter.Svga3DVersion, "no SVGA3D version without a negotiation");
        Assert.False(primary.TryGetFacet<ICanvas3DFactory>(out _), "a display without 3D publishes no 3D factory");
        Assert.True(adapter.IsEnabled, "the console programmed a mode, which enabled the scanout");
    }

    /// <summary>
    /// The modes facet lists the classic VMware modes, 1024x768x32 among
    /// them, and the console picked that one.
    /// </summary>
    public static void TestDisplayModesFacet()
    {
        DisplayDevice? primary = DisplayManager.Primary;
        Assert.NotNull(primary, "a display should be published on the vmware-svga cell");
        if (primary is null)
        {
            return;
        }

        if (!primary.TryGetFacet(out IDisplayModes? modes))
        {
            Assert.Fail("the SVGA display publishes no modes facet");
            return;
        }

        ReadOnlySpan<DisplayMode> list = modes.Modes;
        Assert.True(list.Length > 0, "the modes list is not empty");

        bool listsConsoleMode = false;
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].Width == ConsoleWidth && list[i].Height == ConsoleHeight && list[i].BitsPerPixel == ListedBitsPerPixel)
            {
                listsConsoleMode = true;
            }
        }

        Assert.True(listsConsoleMode, "the modes list holds 1024x768x32");
        Assert.Equal(ConsoleWidth, primary.Width, "the console chose 1024x768");
        Assert.Equal(ConsoleHeight, primary.Height, "the console chose 1024x768");
    }

    /// <summary>
    /// The cursor facet is present; QEMU's adapter has no alpha cursor
    /// capability, so defining an image is refused, while moving the cursor
    /// through the registers is accepted.
    /// </summary>
    public static void TestHardwareCursorFacet()
    {
        DisplayDevice? primary = DisplayManager.Primary;
        Assert.NotNull(primary, "a display should be published on the vmware-svga cell");
        if (primary is null)
        {
            return;
        }

        if (!primary.TryGetFacet(out IHardwareCursor? cursor))
        {
            Assert.Fail("the SVGA display publishes no hardware cursor facet");
            return;
        }

        uint[] pixels = new uint[SmallImageSide * SmallImageSide];
        Assert.False(cursor.TryDefine(0, 0, SmallImageSide, SmallImageSide, pixels), "QEMU has no alpha cursor capability, so no image is defined");

        cursor.Set(1, 1, false);
        Assert.True(true, "moving the hidden cursor does not throw");
    }

    // ==================== FIFO wire tests ====================

    /// <summary>
    /// Opens the wire block: the FIFO is re-initialised through a mode set
    /// of the mode the display is in (so the sequence starts at FIFO_MIN and
    /// never wraps), the scanout is held off, and the 3D canvas is created,
    /// which must emit the exact scene setup: context, colour and depth
    /// targets sized to the canvas, the two binds, viewport, depth range,
    /// the seven fixed-function render states and the untextured stage,
    /// with SVGA3D_INVALID_ID as the unbound texture.
    /// </summary>
    public static void TestSceneSetupFifo()
    {
        if (!TryGetAdapter(out DisplayDevice? primary, out ISvgaAdapter? adapter))
        {
            return;
        }

        if (!primary.TryGetFacet(out IDisplayModes? modes) || !modes.TrySetMode(primary.Width, primary.Height, ListedBitsPerPixel))
        {
            Assert.Fail("re-programming the display's own mode should succeed and restart the FIFO");
            return;
        }

        s_adapter = adapter;
        s_wasEnabled = adapter.IsEnabled;
        adapter.SetEnabled(false);
        Assert.False(adapter.IsEnabled, "the scanout is off for the wire tests");

        uint start = adapter.NextCommand;
        Assert.Equal(adapter.FifoMin, start, "the mode set restarted the FIFO at FIFO_MIN");

        Canvas3D canvas;
        try
        {
            canvas = adapter.CreateCanvas3D(primary);
        }
        catch (Exception ex)
        {
            Assert.Fail("creating the SVGA3D canvas threw: " + ex.Message);
            adapter.SetEnabled(s_wasEnabled);
            return;
        }

        s_canvas = canvas;
        uint width = (uint)canvas.Width;
        uint height = (uint)canvas.Height;
        Assert.Equal((uint)primary.Width, width, "the canvas is as wide as the display");
        Assert.Equal((uint)primary.Height, height, "the canvas is as tall as the display");

        uint at = start;

        Assert.Equal(CmdContextDefine, FifoDword(at), "context defined first");
        Assert.Equal(4u, FifoDword(at + 4), "context body is the cid");
        Assert.Equal(ContextId, FifoDword(at + 8), "context id 1");
        at += HeaderBytes + 4;

        at = AssertSurfaceDefine(at, ColorTargetSid, FormatX8R8G8B8, width, height, "colour target");
        at = AssertSurfaceDefine(at, DepthTargetSid, FormatZD16, width, height, "depth target");

        Assert.Equal(CmdSetRenderTarget, FifoDword(at), "colour target bound");
        Assert.Equal(20u, FifoDword(at + 4), "render target body: cid+type+image id");
        Assert.Equal(ContextId, FifoDword(at + 8), "cid");
        Assert.Equal(2u, FifoDword(at + 12), "target type RT_COLOR0");
        Assert.Equal(ColorTargetSid, FifoDword(at + 16), "bound to the colour surface");
        Assert.Equal(0u, FifoDword(at + 20), "face 0");
        Assert.Equal(0u, FifoDword(at + 24), "mipmap 0");
        at += HeaderBytes + 20;

        Assert.Equal(CmdSetRenderTarget, FifoDword(at), "depth target bound");
        Assert.Equal(0u, FifoDword(at + 12), "target type RT_DEPTH");
        Assert.Equal(DepthTargetSid, FifoDword(at + 16), "bound to the depth surface");
        at += HeaderBytes + 20;

        Assert.Equal(CmdSetViewport, FifoDword(at), "viewport set");
        Assert.Equal(20u, FifoDword(at + 4), "viewport body: cid+rect");
        Assert.Equal(0u, FifoDword(at + 12), "viewport x");
        Assert.Equal(0u, FifoDword(at + 16), "viewport y");
        Assert.Equal(width, FifoDword(at + 20), "viewport width");
        Assert.Equal(height, FifoDword(at + 24), "viewport height");
        at += HeaderBytes + 20;

        Assert.Equal(CmdSetZRange, FifoDword(at), "depth range set");
        Assert.Equal(12u, FifoDword(at + 4), "zrange body: cid+min+max");
        Assert.Equal(FloatBits(0f), FifoDword(at + 12), "depth range min 0");
        Assert.Equal(FloatBits(1f), FifoDword(at + 16), "depth range max 1");
        at += HeaderBytes + 12;

        Assert.Equal(CmdSetRenderState, FifoDword(at), "render states set");
        Assert.Equal(60u, FifoDword(at + 4), "render state body: cid + 7 state pairs");
        Assert.Equal(30u, FifoDword(at + 12), "SHADEMODE");
        Assert.Equal(2u, FifoDword(at + 16), "smooth shading");
        Assert.Equal(9u, FifoDword(at + 20), "LIGHTINGENABLE");
        Assert.Equal(0u, FifoDword(at + 24), "lighting off");
        Assert.Equal(5u, FifoDword(at + 28), "BLENDENABLE");
        Assert.Equal(0u, FifoDword(at + 32), "blending off");
        Assert.Equal(1u, FifoDword(at + 36), "ZENABLE");
        Assert.Equal(1u, FifoDword(at + 40), "depth test on");
        Assert.Equal(2u, FifoDword(at + 44), "ZWRITEENABLE");
        Assert.Equal(1u, FifoDword(at + 48), "depth writes on");
        Assert.Equal(36u, FifoDword(at + 52), "ZFUNC");
        Assert.Equal(2u, FifoDword(at + 56), "compare LESS");
        Assert.Equal(35u, FifoDword(at + 60), "CULLMODE");
        Assert.Equal(1u, FifoDword(at + 64), "cull NONE");
        at += HeaderBytes + 60;

        Assert.Equal(CmdSetTextureState, FifoDword(at), "texture stage set");
        Assert.Equal(52u, FifoDword(at + 4), "texture state body: cid + 4 state triplets");
        Assert.Equal(0u, FifoDword(at + 12), "stage 0");
        Assert.Equal(1u, FifoDword(at + 16), "BIND_TEXTURE");
        Assert.Equal(0xFFFFFFFFu, FifoDword(at + 20), "unbound texture is SVGA3D_INVALID_ID, not float -1 bits");
        Assert.Equal(2u, FifoDword(at + 28), "COLOROP");
        Assert.Equal(2u, FifoDword(at + 32), "SELECTARG1");
        Assert.Equal(3u, FifoDword(at + 40), "COLORARG1");
        Assert.Equal(3u, FifoDword(at + 44), "diffuse colour");
        Assert.Equal(6u, FifoDword(at + 52), "ALPHAARG1");
        Assert.Equal(3u, FifoDword(at + 56), "diffuse alpha");
        at += HeaderBytes + 52;

        Assert.Equal(at, adapter.NextCommand, "setup emits exactly these commands");
        Assert.False(adapter.IsEnabled, "creating the canvas did not re-enable the scanout");

        Rewind(start);
    }

    /// <summary>
    /// Creating the cube uploads one buffer surface per attribute stream and
    /// one for the indices, each as a SURFACE_DEFINE of an SVGA3D_BUFFER as
    /// wide as the data and a SURFACE_DMA into it from the framebuffer GMR,
    /// with the surface ids 3, 4 and 5 after the two render targets. The
    /// adapter is disabled, so the uploads queue without waiting for a
    /// fence. The mesh is kept for the draw tests.
    /// </summary>
    public static void TestMeshUploadFifo()
    {
        if (!Ready(out ISvgaAdapter? adapter, out Canvas3D? canvas))
        {
            return;
        }

        uint start = adapter.NextCommand;

        Mesh cube;
        try
        {
            cube = canvas.CreateMesh(CubePositions(), CubeColors(), CubeIndices());
        }
        catch (Exception ex)
        {
            Assert.Fail("creating the cube threw: " + ex.Message);
            return;
        }

        s_cube = cube;
        Assert.Equal(CubeVertexCount, cube.VertexCount, "the cube has eight vertices");
        Assert.Equal(CubeIndexCount, cube.IndexCount, "the cube has thirty-six indices");

        uint at = start;
        uint positionScratch = FifoDword(at + HeaderBytes + SurfaceDefineBytes + HeaderBytes + 4);
        at = AssertUpload(at, PositionSid, CubeVertexCount * PositionBytes, positionScratch, "position stream");
        at = AssertUpload(at, ColorSid, CubeVertexCount * ColorBytes, positionScratch, "colour stream");
        at = AssertUpload(at, IndexSid, CubeIndexCount * IndexBytes, positionScratch, "index buffer");

        Assert.Equal(at, adapter.NextCommand, "the upload emits exactly three surface definitions and three DMAs");

        Rewind(start);
    }

    /// <summary>
    /// Mesh validation must reject bad data before anything reaches the
    /// device: mismatched colour count, a non-triangle index count, and an
    /// index referring past the vertices.
    /// </summary>
    public static void TestMeshValidation()
    {
        if (!Ready(out ISvgaAdapter? adapter, out Canvas3D? canvas))
        {
            return;
        }

        uint start = adapter.NextCommand;
        Vector3[] positions = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY];

        try
        {
            canvas.CreateMesh(positions, [0xFF0000u], [0, 1, 2]);
            Assert.Fail("mismatched colour count accepted");
        }
        catch (ArgumentException)
        {
            Assert.True(true, "mismatched colour count rejected");
        }

        try
        {
            canvas.CreateMesh(positions, [1u, 2u, 3u], [0, 1]);
            Assert.Fail("non-triangle index count accepted");
        }
        catch (ArgumentException)
        {
            Assert.True(true, "non-triangle index count rejected");
        }

        try
        {
            canvas.CreateMesh(positions, [1u, 2u, 3u], [0, 1, 7]);
            Assert.Fail("out-of-range index accepted");
        }
        catch (ArgumentException)
        {
            Assert.True(true, "out-of-range index rejected");
        }

        Assert.Equal(start, adapter.NextCommand, "rejected meshes write nothing to the FIFO");
    }

    /// <summary>
    /// One frame of the rotating cube through the public API: ClearScene
    /// and DrawMesh must emit the view and projection computed from the
    /// camera, the clear, the world transform, and a draw payload reading
    /// the cube's two uploaded streams and its index buffer as twelve
    /// indexed triangles.
    /// </summary>
    public static void TestDrawCubeFifo()
    {
        if (!Ready(out ISvgaAdapter? adapter, out Canvas3D? canvas))
        {
            return;
        }

        if (s_cube is null)
        {
            Assert.Fail("cube mesh not built (Canvas3D_MeshUpload_Fifo did not run)");
            return;
        }

        Vector3 eye = new(0f, 1.5f, 3f);
        canvas.Camera = new Camera3D(eye, Vector3.Zero);

        // The demo's per-frame transform (its "view"), which is the world
        // transform of the cube in the new API.
        Matrix4x4 world =
            Matrix4x4.CreateScale(0.5f) *
            Matrix4x4.CreateRotationX(30f * (MathF.PI / 180f)) *
            Matrix4x4.CreateRotationY(0.4f) *
            Matrix4x4.CreateTranslation(new Vector3(0f, 0f, -3f));

        uint width = (uint)canvas.Width;
        uint height = (uint)canvas.Height;
        uint start = adapter.NextCommand;

        canvas.ClearScene(Color.FromArgb(unchecked((int)0xFF113366)));
        canvas.DrawMesh(s_cube, world);

        uint at = start;

        Matrix4x4 view = Matrix4x4.CreateLookAt(eye, Vector3.Zero, Vector3.UnitY);
        at = AssertTransform(at, 2, view, "view");

        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(60f * (MathF.PI / 180f), width / (float)height, 0.1f, 1000f);
        at = AssertTransform(at, 3, projection, "projection");

        Assert.Equal(CmdClear, FifoDword(at), "clear follows the camera transforms");
        Assert.Equal(36u, FifoDword(at + 4), "clear body: cid+flags+color+depth+stencil+1 rect");
        Assert.Equal(ContextId, FifoDword(at + 8), "cid");
        Assert.Equal(3u, FifoDword(at + 12), "clears colour and depth");
        Assert.Equal(0xFF113366u, FifoDword(at + 16), "clear colour");
        Assert.Equal(FloatBits(1f), FifoDword(at + 20), "depth reset to 1");
        Assert.Equal(0u, FifoDword(at + 24), "stencil untouched");
        Assert.Equal(0u, FifoDword(at + 28), "clear rect x");
        Assert.Equal(0u, FifoDword(at + 32), "clear rect y");
        Assert.Equal(width, FifoDword(at + 36), "clear rect width");
        Assert.Equal(height, FifoDword(at + 40), "clear rect height");
        at += HeaderBytes + 36;

        // The mesh is untextured and the stage was configured at setup, so
        // the world transform comes next, no texture state in the stream.
        at = AssertTransform(at, 1, world, "world");

        Assert.Equal(CmdDrawPrimitives, FifoDword(at), "draw command");
        Assert.Equal(CubeDrawBytes, FifoDword(at + 4), "draw body: cid+counts + 2 decls (36 each) + 1 range (28)");
        Assert.Equal(ContextId, FifoDword(at + 8), "cid");
        Assert.Equal(2u, FifoDword(at + 12), "two vertex declarations");
        Assert.Equal(1u, FifoDword(at + 16), "one primitive range");

        uint decl0 = at + 20;
        Assert.Equal(2u, FifoDword(decl0), "decl 0 type FLOAT3");
        Assert.Equal(0u, FifoDword(decl0 + 4), "decl 0 default method");
        Assert.Equal(0u, FifoDword(decl0 + 8), "decl 0 usage POSITION");
        Assert.Equal(0u, FifoDword(decl0 + 12), "decl 0 usage index 0");
        Assert.Equal(PositionSid, FifoDword(decl0 + 16), "decl 0 reads the position buffer");
        Assert.Equal(0u, FifoDword(decl0 + 20), "decl 0 offset 0");
        Assert.Equal(PositionBytes, FifoDword(decl0 + 24), "decl 0 stride 12");

        uint decl1 = decl0 + 36;
        Assert.Equal(4u, FifoDword(decl1), "decl 1 type D3DCOLOR");
        Assert.Equal(10u, FifoDword(decl1 + 8), "decl 1 usage COLOR");
        Assert.Equal(ColorSid, FifoDword(decl1 + 16), "decl 1 reads the colour buffer");
        Assert.Equal(ColorBytes, FifoDword(decl1 + 24), "decl 1 stride 4");

        uint range = decl1 + 36;
        Assert.Equal(1u, FifoDword(range), "TRIANGLELIST");
        Assert.Equal(12u, FifoDword(range + 4), "12 triangles");
        Assert.Equal(IndexSid, FifoDword(range + 8), "indices read from the index buffer");
        Assert.Equal(0u, FifoDword(range + 12), "index array offset 0");
        Assert.Equal(IndexBytes, FifoDword(range + 16), "index array stride 2");
        Assert.Equal(IndexBytes, FifoDword(range + 20), "16-bit indices");
        Assert.Equal(0u, FifoDword(range + 24), "no index bias");
        at += HeaderBytes + CubeDrawBytes;

        Assert.Equal(at, adapter.NextCommand, "the frame emits exactly these commands");

        Rewind(start);
    }

    /// <summary>
    /// Camera and texture state must be cached between draws: a second
    /// DrawMesh emits only the world transform and the draw itself, and
    /// assigning <see cref="Canvas3D.Camera"/> re-emits view and projection
    /// on the next draw.
    /// </summary>
    public static void TestCameraCachingFifo()
    {
        if (!Ready(out ISvgaAdapter? adapter, out Canvas3D? canvas))
        {
            return;
        }

        if (s_cube is null)
        {
            Assert.Fail("cube mesh not built (Canvas3D_MeshUpload_Fifo did not run)");
            return;
        }

        uint start = adapter.NextCommand;
        uint transformCommandBytes = HeaderBytes + TransformBytes;
        uint drawCommandBytes = HeaderBytes + CubeDrawBytes;

        canvas.DrawMesh(s_cube, Matrix4x4.Identity);

        Assert.Equal(CmdSetTransform, FifoDword(start), "cached camera: the world transform comes first");
        Assert.Equal(1u, FifoDword(start + 12), "transform type WORLD");
        Assert.Equal(CmdDrawPrimitives, FifoDword(start + transformCommandBytes), "the draw follows immediately");
        Assert.Equal(start + transformCommandBytes + drawCommandBytes, adapter.NextCommand, "no other commands emitted");

        Rewind(start);

        canvas.Camera = new Camera3D(new Vector3(2f, 2f, 2f), Vector3.Zero);
        canvas.DrawMesh(s_cube, Matrix4x4.Identity);

        Assert.Equal(CmdSetTransform, FifoDword(start), "camera change re-applies the transforms");
        Assert.Equal(2u, FifoDword(start + 12), "view re-emitted first");
        Assert.Equal(3u, FifoDword(start + transformCommandBytes + 12), "projection re-emitted second");
        Assert.Equal(1u, FifoDword(start + 2 * transformCommandBytes + 12), "world transform follows");
        Assert.Equal(start + 3 * transformCommandBytes + drawCommandBytes, adapter.NextCommand, "view+projection+world+draw and nothing else");

        Rewind(start);
    }

    /// <summary>
    /// A texture is uploaded as an A8R8G8B8 SURFACE_DEFINE and a
    /// SURFACE_DMA with the next surface id; disposing it destroys that
    /// surface and leaves every mesh that still maps it undrawable: DrawMesh
    /// rejects the mesh before anything reaches the FIFO, so the freed id is
    /// never bound again. Closes the wire block: the scanout goes back to
    /// what it was.
    /// </summary>
    public static void TestDisposedTextureRejected()
    {
        if (!Ready(out ISvgaAdapter? adapter, out Canvas3D? canvas))
        {
            return;
        }

        try
        {
            uint start = adapter.NextCommand;

            Bitmap image = new(SmallImageSide, SmallImageSide, ColorDepth.ColorDepth32);
            for (int i = 0; i < image.RawData.Length; i++)
            {
                image.RawData[i] = unchecked((int)0xFF204060u) + i;
            }

            Texture texture = canvas.CreateTexture(image);
            Assert.Equal(SmallImageSide, texture.Width, "the texture is as wide as the image");
            Assert.Equal(SmallImageSide, texture.Height, "the texture is as tall as the image");

            uint at = start;
            at = AssertSurfaceDefine(at, TextureSid, FormatA8R8G8B8, SmallImageSide, SmallImageSide, "texture");
            Assert.Equal(CmdSurfaceDma, FifoDword(at), "texture upload is a SURFACE_DMA");
            Assert.Equal(SurfaceDmaBytes, FifoDword(at + 4), "DMA body: guest image + host image + transfer + 1 copy box");
            Assert.Equal(FramebufferGmr, FifoDword(at + 8), "texture pixels come from the framebuffer GMR");
            Assert.Equal(0u, FifoDword(at + 16), "tightly packed (pitch 0)");
            Assert.Equal(TextureSid, FifoDword(at + 20), "DMA into the texture's surface");
            Assert.Equal(TransferWriteHostVram, FifoDword(at + 32), "transfer WRITE_HOST_VRAM");
            Assert.Equal((uint)SmallImageSide, FifoDword(at + 48), "box width is the image width");
            Assert.Equal((uint)SmallImageSide, FifoDword(at + 52), "box height is the image height");
            at += HeaderBytes + SurfaceDmaBytes;
            Assert.Equal(at, adapter.NextCommand, "the texture emits its definition and one DMA");

            Vector3[] positions = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY, new Vector3(1f, 1f, 0f)];
            Vector2[] uvs = [Vector2.Zero, Vector2.UnitX, Vector2.UnitY, Vector2.One];
            ushort[] indices = [0, 1, 2, 2, 1, 3];
            Mesh quad = canvas.CreateMesh(positions, uvs, texture, indices);
            uint afterQuad = adapter.NextCommand;
            Assert.Equal(at + 3 * UploadBytes, afterQuad, "the quad uploads two streams and its indices");

            texture.Dispose();

            Assert.True(texture.IsDisposed, "the texture reports disposed");
            Assert.Equal(CmdSurfaceDestroy, FifoDword(afterQuad), "dispose destroys the surface");
            Assert.Equal(4u, FifoDword(afterQuad + 4), "destroy body is the sid");
            Assert.Equal(TextureSid, FifoDword(afterQuad + 8), "the destroyed surface is the texture's");
            Assert.Equal(afterQuad + HeaderBytes + 4, adapter.NextCommand, "dispose emits the destroy and nothing else");

            uint afterDispose = adapter.NextCommand;
            try
            {
                canvas.DrawMesh(quad, Matrix4x4.Identity);
                Assert.Fail("a mesh mapping a disposed texture was drawn");
            }
            catch (ArgumentException)
            {
                Assert.True(true, "a mesh mapping a disposed texture is rejected");
            }

            Assert.Equal(afterDispose, adapter.NextCommand, "the rejected draw writes nothing to the FIFO");

            quad.Dispose();
            Rewind(start);
        }
        finally
        {
            adapter.SetEnabled(s_wasEnabled);
        }

        Assert.Equal(s_wasEnabled, adapter.IsEnabled, "the scanout is back to what it was before the wire tests");
    }

    // ==================== Helpers ====================

    /// <summary>The primary display and its adapter facet; fails the test when either is missing.</summary>
    private static bool TryGetAdapter([NotNullWhen(true)] out DisplayDevice? primary, [NotNullWhen(true)] out ISvgaAdapter? adapter)
    {
        adapter = null;
        primary = DisplayManager.Primary;
        Assert.NotNull(primary, "a display should be published on the vmware-svga cell");
        if (primary is null)
        {
            return false;
        }

        if (!primary.TryGetFacet(out adapter))
        {
            Assert.Fail("the primary display publishes no ISvgaAdapter facet: " + primary.DriverName + " \"" + primary.Name + "\"");
            return false;
        }

        return true;
    }

    /// <summary>The adapter and the canvas the scene setup test created; fails the test when they are missing.</summary>
    private static bool Ready([NotNullWhen(true)] out ISvgaAdapter? adapter, [NotNullWhen(true)] out Canvas3D? canvas)
    {
        adapter = s_adapter;
        canvas = s_canvas;
        if (adapter is null || canvas is null)
        {
            Assert.Fail("the SVGA3D canvas was not created (Canvas3D_SceneSetup_Fifo did not run)");
            return false;
        }

        return true;
    }

    /// <summary>Bit pattern of a float, for asserting float fields through the dword-wide FIFO view.</summary>
    private static uint FloatBits(float value) => BitConverter.SingleToUInt32Bits(value);

    /// <summary>Reads the FIFO dword at a byte offset.</summary>
    private static uint FifoDword(uint byteOffset) => s_adapter!.ReadFifo(byteOffset);

    /// <summary>
    /// Discards everything written since <paramref name="start"/>. The
    /// scanout is off, so nothing races the rewind, and it guarantees the
    /// FIFO holds no 3D commands (which QEMU cannot parse) when the scanout
    /// is enabled again.
    /// </summary>
    private static void Rewind(uint start)
    {
        s_adapter!.NextCommand = start;
    }

    /// <summary>
    /// Asserts a one-mip SURFACE_DEFINE at <paramref name="at"/> with the
    /// given id, format and size, and returns the offset of the next command.
    /// </summary>
    private static uint AssertSurfaceDefine(uint at, uint sid, uint format, uint width, uint height, string label)
    {
        Assert.Equal(CmdSurfaceDefine, FifoDword(at), label + " defined");
        Assert.Equal(SurfaceDefineBytes, FifoDword(at + 4), label + " surface body: sid+flags+format+6 faces+1 mip size");
        Assert.Equal(sid, FifoDword(at + 8), label + " sid");
        Assert.Equal(0u, FifoDword(at + 12), label + " has no surface flags");
        Assert.Equal(format, FifoDword(at + 16), label + " format");
        Assert.Equal(1u, FifoDword(at + 20), label + " face 0 has one mip level");
        Assert.Equal(0u, FifoDword(at + 24), label + " face 1 unused");
        Assert.Equal(0u, FifoDword(at + 40), label + " face 5 unused");
        Assert.Equal(width, FifoDword(at + 44), label + " mip width");
        Assert.Equal(height, FifoDword(at + 48), label + " mip height");
        Assert.Equal(1u, FifoDword(at + 52), label + " mip depth 1");
        return at + HeaderBytes + SurfaceDefineBytes;
    }

    /// <summary>
    /// Asserts one buffer upload at <paramref name="at"/>: a SURFACE_DEFINE
    /// of an SVGA3D_BUFFER <paramref name="bytes"/> wide and one row high,
    /// then a SURFACE_DMA of that box into it from the framebuffer GMR at
    /// <paramref name="scratch"/>, and returns the offset of the next command.
    /// </summary>
    private static uint AssertUpload(uint at, uint sid, uint bytes, uint scratch, string label)
    {
        at = AssertSurfaceDefine(at, sid, FormatBuffer, bytes, 1, label);

        Assert.Equal(CmdSurfaceDma, FifoDword(at), label + " uploaded by a SURFACE_DMA");
        Assert.Equal(SurfaceDmaBytes, FifoDword(at + 4), label + " DMA body: guest image + host image + transfer + 1 copy box");
        Assert.Equal(FramebufferGmr, FifoDword(at + 8), label + " comes from the framebuffer GMR");
        Assert.Equal(scratch, FifoDword(at + 12), label + " comes from the DMA scratch, given back after each upload");
        Assert.Equal(0u, FifoDword(at + 16), label + " tightly packed (pitch 0)");
        Assert.Equal(sid, FifoDword(at + 20), label + " DMA into its own surface");
        Assert.Equal(0u, FifoDword(at + 24), label + " host image face 0");
        Assert.Equal(0u, FifoDword(at + 28), label + " host image mipmap 0");
        Assert.Equal(TransferWriteHostVram, FifoDword(at + 32), label + " transfer WRITE_HOST_VRAM");
        Assert.Equal(0u, FifoDword(at + 36), label + " box x");
        Assert.Equal(0u, FifoDword(at + 40), label + " box y");
        Assert.Equal(0u, FifoDword(at + 44), label + " box z");
        Assert.Equal(bytes, FifoDword(at + 48), label + " box width is the byte size");
        Assert.Equal(1u, FifoDword(at + 52), label + " box height 1");
        Assert.Equal(1u, FifoDword(at + 56), label + " box depth 1");
        Assert.Equal(0u, FifoDword(at + 60), label + " source x untouched");
        Assert.Equal(0u, FifoDword(at + 64), label + " source y untouched");
        Assert.Equal(0u, FifoDword(at + 68), label + " source z untouched");
        return at + HeaderBytes + SurfaceDmaBytes;
    }

    /// <summary>
    /// Asserts a SETTRANSFORM command at <paramref name="at"/> carrying the
    /// given matrix bit for bit, and returns the offset of the next command.
    /// </summary>
    private static uint AssertTransform(uint at, uint type, Matrix4x4 expected, string label)
    {
        Assert.Equal(CmdSetTransform, FifoDword(at), label + " transform command");
        Assert.Equal(TransformBytes, FifoDword(at + 4), label + " transform body: cid+type+16 floats");
        Assert.Equal(ContextId, FifoDword(at + 8), label + " cid");
        Assert.Equal(type, FifoDword(at + 12), label + " transform type");

        ReadOnlySpan<uint> elements = MemoryMarshal.Cast<Matrix4x4, uint>(MemoryMarshal.CreateReadOnlySpan(ref expected, 1));
        for (int i = 0; i < elements.Length; i++)
        {
            if (FifoDword(at + 16 + (uint)(i * sizeof(uint))) != elements[i])
            {
                Assert.Fail(label + " matrix element " + i + " does not match");
                return at + HeaderBytes + TransformBytes;
            }
        }

        Assert.True(true, label + " matrix payload matches bit for bit");
        return at + HeaderBytes + TransformBytes;
    }

    /// <summary>The eight corners of a unit cube centred on the origin.</summary>
    private static Vector3[] CubePositions() =>
    [
        new(-0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f),
        new(0.5f, 0.5f, -0.5f),
        new(-0.5f, 0.5f, -0.5f),
        new(-0.5f, -0.5f, 0.5f),
        new(0.5f, -0.5f, 0.5f),
        new(0.5f, 0.5f, 0.5f),
        new(-0.5f, 0.5f, 0.5f),
    ];

    /// <summary>One packed colour per corner of the cube.</summary>
    private static uint[] CubeColors() =>
    [
        0xFFFF0000u, 0xFF00FF00u, 0xFF0000FFu, 0xFFFFFF00u,
        0xFFFF00FFu, 0xFF00FFFFu, 0xFFFFFFFFu, 0xFF808080u,
    ];

    /// <summary>The twelve triangles of the cube, two per face.</summary>
    private static ushort[] CubeIndices() =>
    [
        0, 1, 2, 2, 3, 0,
        4, 6, 5, 6, 4, 7,
        0, 4, 5, 5, 1, 0,
        3, 2, 6, 6, 7, 3,
        0, 3, 7, 7, 4, 0,
        1, 5, 6, 6, 2, 1,
    ];
}
