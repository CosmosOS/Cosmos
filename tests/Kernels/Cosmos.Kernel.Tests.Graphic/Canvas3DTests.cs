using System;
using System.Drawing;
using System.Numerics;
using Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.System.Graphics;
using Cosmos.TestRunner.Framework;

namespace Cosmos.Kernel.Tests.Graphic;

/// <summary>
/// Tests for the public <see cref="Canvas3D"/> API. The camera-default,
/// mesh-layout and discovery tests hold on every cell. The FIFO tests drive
/// the rotating-cube scene of the original 3D demo kernel through the public
/// API on the vmware-svga profile and assert every command the SVGA backend
/// emits — scene setup, camera and world transforms, clear, and the full
/// draw payload — using the same disabled-device inspect-and-rewind
/// technique as <see cref="Svga3DTests"/>: QEMU negotiates no 3D, so the
/// guest half of the contract is what CI can pin down, and the device must
/// stay disabled so the commands sit inert in FIFO memory. Two consequences:
/// mesh buffers cannot be uploaded (the DMA path fences, which needs a live
/// device), so draw tests inject known surface ids through
/// <see cref="SvgaII3DCanvas.BuildMeshData"/>; and <see cref="Canvas.Display"/>
/// (which presents and fences) is never called on the inspected canvas.
/// Expected ids, sizes and offsets are hard-coded from the SVGA3D protocol
/// (svga3d_reg.h), not read back from the kernel's own enums.
/// </summary>
public static unsafe class Canvas3DTests
{
    /// <summary>Skip reason for the FIFO tests on cells without the adapter (the bare profile, and all of arm64).</summary>
    public const string SkipNoDevice = "VMware SVGA II adapter not present, needs the vmware-svga profile";

    /// <summary>PCI vendor id of VMware.</summary>
    private const ushort VMwareVendorId = 0x15AD;

    /// <summary>PCI device id of the SVGA II adapter.</summary>
    private const ushort SvgaIIDeviceId = 0x0405;

    /// <summary>SVGA_3D_CMD_SURFACE_DEFINE (SVGA_3D_CMD_BASE + 0).</summary>
    private const uint CmdSurfaceDefine = 1040;

    /// <summary>SVGA_3D_CMD_SURFACE_DESTROY (SVGA_3D_CMD_BASE + 1).</summary>
    private const uint CmdSurfaceDestroy = 1041;

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

    /// <summary>Size of SVGA3dCmdHeader (id + size) preceding every 3D command body.</summary>
    private const uint HeaderBytes = 8;

    /// <summary>Injected surface id of the cube's position stream (the demo's vao).</summary>
    private const uint PositionSid = 111;

    /// <summary>Injected surface id of the cube's color stream.</summary>
    private const uint ColorSid = 222;

    /// <summary>Injected surface id of the cube's index buffer (the demo's ebo).</summary>
    private const uint IndexSid = 333;

    /// <summary>Injected surface id of the texture the disposed-texture test creates and destroys.</summary>
    private const uint TextureSid = 444;

    private static PciDevice? s_device;
    private static SvgaIIDriver? s_driver;
    private static SvgaII3DCanvas? s_canvas;
    private static Mesh? s_cube;

    /// <summary>True when the SVGA II adapter was enumerated on the PCI bus.</summary>
    public static bool DevicePresent => s_device != null;

    /// <summary>True when <see cref="TestSceneSetupFifo"/> constructed the inspected canvas.</summary>
    public static bool Ready => s_canvas != null;

    /// <summary>
    /// Locate the SVGA II adapter on the PCI bus. Called once from BeforeRun;
    /// the FIFO tests gate on the result so the bare cell skips cleanly.
    /// </summary>
    public static void Discover()
    {
        if (PciManager.Devices == null)
        {
            return;
        }

        for (int i = 0; i < (int)PciManager.Count; i++)
        {
            PciDevice device = PciManager.Devices[i];
            if (device.VendorId == VMwareVendorId && device.DeviceId == SvgaIIDeviceId)
            {
                s_device = device;
                return;
            }
        }
    }

    /// <summary>Bit pattern of a float, for asserting float fields through the dword-wide FIFO view.</summary>
    private static uint FloatBits(float value) => *(uint*)&value;

    /// <summary>Read the FIFO dword at an arbitrary byte offset.</summary>
    private static uint FifoDword(uint byteOffset) => s_driver!.GetFIFO((FIFO)byteOffset);

    /// <summary>NEXT_CMD before a command is written — where its header will land.</summary>
    private static uint CaptureStart() => s_driver!.GetFIFO(FIFO.NextCmd);

    /// <summary>
    /// Discard everything written since <paramref name="start"/>. The device
    /// is never enabled on the inspected canvas so nothing races the rewind,
    /// and it guarantees the FIFO holds no 3D commands (which QEMU cannot
    /// parse) when the discovery test enables the device later.
    /// </summary>
    private static void Rewind(uint start) => s_driver!.SetFIFO(FIFO.NextCmd, start);

    /// <summary>
    /// A default-initialized or partially-initialized <see cref="Camera3D"/>
    /// must fall back to a usable up direction and field of view.
    /// </summary>
    public static void TestCamera3DDefaults()
    {
        Camera3D unset = default;
        Assert.True(unset.Up == Vector3.UnitY, "default camera up falls back to +Y");
        Assert.True(unset.FovY == 60f, "default camera fov falls back to 60 degrees");

        Camera3D partial = new Camera3D { Position = new Vector3(1f, 2f, 3f), Target = Vector3.Zero };
        Assert.True(partial.Up == Vector3.UnitY, "object initializer keeps the up fallback");
        Assert.True(partial.FovY == 60f, "object initializer keeps the fov fallback");

        Camera3D full = new Camera3D(new Vector3(0f, 1.5f, 3f), Vector3.Zero, Vector3.UnitZ, 45f);
        Assert.True(full.Up == Vector3.UnitZ, "explicit up is kept");
        Assert.True(full.FovY == 45f, "explicit fov is kept");
        Assert.True(full.Position == new Vector3(0f, 1.5f, 3f), "position is kept");
    }

    /// <summary>
    /// The draw payload built for a mesh must match the layout the original
    /// 3D demo proved on real VMware: one stream per attribute with the
    /// D3D-style declaration types, and an indexed 16-bit primitive range.
    /// Host-independent (pure function), runs on every cell.
    /// </summary>
    public static void TestMeshLayout()
    {
        SvgaMeshData cube = SvgaII3DCanvas.BuildMeshData([PositionSid, ColorSid], hasColors: true, hasUvs: false, IndexSid, 36, MeshTopology.Triangles);

        Assert.Equal(2, cube.Decls.Length, "position + color streams");
        Assert.Equal(2u, (uint)cube.Decls[0].identity.type, "position decl is FLOAT3");
        Assert.Equal(0u, (uint)cube.Decls[0].identity.usage, "position decl usage is POSITION");
        Assert.Equal(PositionSid, cube.Decls[0].array.surfaceId, "position decl reads its own stream");
        Assert.Equal(12u, cube.Decls[0].array.stride, "position stride is 3 floats");
        Assert.Equal(0u, cube.Decls[0].array.offset, "separate streams need no offset");
        Assert.Equal(4u, (uint)cube.Decls[1].identity.type, "color decl is D3DCOLOR");
        Assert.Equal(10u, (uint)cube.Decls[1].identity.usage, "color decl usage is COLOR");
        Assert.Equal(ColorSid, cube.Decls[1].array.surfaceId, "color decl reads its own stream");
        Assert.Equal(4u, cube.Decls[1].array.stride, "color stride is one packed dword");

        Assert.Equal(1, cube.Ranges.Length, "one primitive range");
        Assert.Equal(1u, (uint)cube.Ranges[0].primType, "TRIANGLELIST");
        Assert.Equal(12u, cube.Ranges[0].primitiveCount, "36 indices form 12 triangles");
        Assert.Equal(IndexSid, cube.Ranges[0].indexArray.surfaceId, "index buffer surface");
        Assert.Equal(2u, cube.Ranges[0].indexArray.stride, "16-bit index stride");
        Assert.Equal(2u, cube.Ranges[0].indexWidth, "16-bit index width");

        SvgaMeshData textured = SvgaII3DCanvas.BuildMeshData([5, 6], hasColors: false, hasUvs: true, 7, 6, MeshTopology.Triangles);
        Assert.Equal(1u, (uint)textured.Decls[1].identity.type, "uv decl is FLOAT2");
        Assert.Equal(5u, (uint)textured.Decls[1].identity.usage, "uv decl usage is TEXCOORD");
        Assert.Equal(8u, textured.Decls[1].array.stride, "uv stride is 2 floats");

        SvgaMeshData line = SvgaII3DCanvas.BuildMeshData([9], hasColors: false, hasUvs: false, 10, 2, MeshTopology.Lines);
        Assert.Equal(3u, (uint)line.Ranges[0].primType, "LINELIST");
        Assert.Equal(1u, line.Ranges[0].primitiveCount, "2 indices form 1 line");
    }

    /// <summary>
    /// Constructing the SVGA 3D canvas must emit the exact scene-setup
    /// sequence the demo performed by hand: context, color+depth targets,
    /// viewport, depth range, the fixed-function render states, and the
    /// untextured texture stage — including SVGA3D_INVALID_ID as the unbound
    /// texture (the demo's -1 silently bound to the float overload).
    /// </summary>
    public static void TestSceneSetupFifo()
    {
        try
        {
            s_driver = new SvgaIIDriver(s_device!);
        }
        catch (Exception ex)
        {
            Assert.Fail("SVGAII driver bind threw: " + ex.Message);
            return;
        }

        uint start = CaptureStart();

        s_canvas = new SvgaII3DCanvas(s_driver, new Mode(1280, 720, ColorDepth.ColorDepth32), applyMode: false);

        uint at = start;

        Assert.Equal(CmdContextDefine, FifoDword(at), "context defined first");
        Assert.Equal(4u, FifoDword(at + 4), "context body is the cid");
        Assert.Equal(1u, FifoDword(at + 8), "context id 1");
        at += HeaderBytes + 4;

        Assert.Equal(CmdSurfaceDefine, FifoDword(at), "color target defined");
        Assert.Equal(48u, FifoDword(at + 4), "surface body: sid+flags+format+6 faces+1 mip size");
        Assert.Equal(1u, FifoDword(at + 8), "color target sid 1");
        Assert.Equal(0u, FifoDword(at + 12), "no surface flags");
        Assert.Equal(1u, FifoDword(at + 16), "format X8R8G8B8");
        Assert.Equal(1u, FifoDword(at + 20), "face 0 has one mip level");
        Assert.Equal(0u, FifoDword(at + 40), "face 5 unused");
        Assert.Equal(1280u, FifoDword(at + 44), "mip width matches the mode");
        Assert.Equal(720u, FifoDword(at + 48), "mip height matches the mode");
        Assert.Equal(1u, FifoDword(at + 52), "mip depth 1");
        at += HeaderBytes + 48;

        Assert.Equal(CmdSurfaceDefine, FifoDword(at), "depth target defined");
        Assert.Equal(2u, FifoDword(at + 8), "depth target sid 2");
        Assert.Equal(8u, FifoDword(at + 16), "format Z_D16");
        Assert.Equal(1280u, FifoDword(at + 44), "depth mip width matches the mode");
        Assert.Equal(720u, FifoDword(at + 48), "depth mip height matches the mode");
        at += HeaderBytes + 48;

        Assert.Equal(CmdSetRenderTarget, FifoDword(at), "color target bound");
        Assert.Equal(20u, FifoDword(at + 4), "render target body: cid+type+image id");
        Assert.Equal(1u, FifoDword(at + 8), "cid");
        Assert.Equal(2u, FifoDword(at + 12), "target type RT_COLOR0");
        Assert.Equal(1u, FifoDword(at + 16), "bound to the color surface");
        Assert.Equal(0u, FifoDword(at + 20), "face 0");
        Assert.Equal(0u, FifoDword(at + 24), "mipmap 0");
        at += HeaderBytes + 20;

        Assert.Equal(CmdSetRenderTarget, FifoDword(at), "depth target bound");
        Assert.Equal(0u, FifoDword(at + 12), "target type RT_DEPTH");
        Assert.Equal(2u, FifoDword(at + 16), "bound to the depth surface");
        at += HeaderBytes + 20;

        Assert.Equal(CmdSetViewport, FifoDword(at), "viewport set");
        Assert.Equal(20u, FifoDword(at + 4), "viewport body: cid+rect");
        Assert.Equal(0u, FifoDword(at + 12), "viewport x");
        Assert.Equal(0u, FifoDword(at + 16), "viewport y");
        Assert.Equal(1280u, FifoDword(at + 20), "viewport width");
        Assert.Equal(720u, FifoDword(at + 24), "viewport height");
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
        Assert.Equal(3u, FifoDword(at + 44), "diffuse color");
        Assert.Equal(6u, FifoDword(at + 52), "ALPHAARG1");
        Assert.Equal(3u, FifoDword(at + 56), "diffuse alpha");
        at += HeaderBytes + 52;

        Assert.Equal(at, s_driver.GetFIFO(FIFO.NextCmd), "setup emits exactly these commands");
        Assert.False(s_canvas.ReadsFrom3DScene, "no scene composed yet, GetImage reads the 2D framebuffer");

        Rewind(start);
    }

    /// <summary>
    /// Mesh validation must reject bad data before anything reaches the
    /// device: mismatched color count, a non-triangle index count, and an
    /// index referring past the vertices.
    /// </summary>
    public static void TestMeshValidation()
    {
        uint start = CaptureStart();
        Vector3[] positions = [Vector3.Zero, Vector3.UnitX, Vector3.UnitY];

        try
        {
            s_canvas!.CreateMesh(positions, [0xFF0000u], [0, 1, 2]);
            Assert.Fail("mismatched color count accepted");
        }
        catch (ArgumentException)
        {
        }

        try
        {
            s_canvas!.CreateMesh(positions, [1u, 2u, 3u], [0, 1]);
            Assert.Fail("non-triangle index count accepted");
        }
        catch (ArgumentException)
        {
        }

        try
        {
            s_canvas!.CreateMesh(positions, [1u, 2u, 3u], [0, 1, 7]);
            Assert.Fail("out-of-range index accepted");
        }
        catch (ArgumentException)
        {
        }

        Assert.Equal(start, s_driver!.GetFIFO(FIFO.NextCmd), "rejected meshes write nothing to the FIFO");
    }

    /// <summary>
    /// One frame of the demo's rotating cube through the public API:
    /// ClearScene + DrawMesh must emit the view and projection computed from
    /// the camera, the clear, the world transform, and a draw payload with
    /// the cube's two vertex streams and 12 indexed triangles — the same
    /// stream the demo built by hand.
    /// </summary>
    public static void TestDrawCubeFifo()
    {
        s_cube = new Mesh(s_canvas!, 8, 36, null, MeshTopology.Triangles)
        {
            DriverData = SvgaII3DCanvas.BuildMeshData([PositionSid, ColorSid], hasColors: true, hasUvs: false, IndexSid, 36, MeshTopology.Triangles),
        };

        Vector3 eye = new(0f, 1.5f, 3f);
        s_canvas!.Camera = new Camera3D(eye, Vector3.Zero);

        // The demo's per-frame transform (its "view"), which is the world
        // transform of the cube in the new API.
        Matrix4x4 world =
            Matrix4x4.CreateScale(0.5f) *
            Matrix4x4.CreateRotationX(30f * (MathF.PI / 180f)) *
            Matrix4x4.CreateRotationY(0.4f) *
            Matrix4x4.CreateTranslation(new Vector3(0f, 0f, -3f));

        uint start = CaptureStart();

        s_canvas.ClearScene(Color.FromArgb(unchecked((int)0xFF113366)));
        s_canvas.DrawMesh(s_cube, world);

        uint at = start;

        Matrix4x4 view = Matrix4x4.CreateLookAt(eye, Vector3.Zero, Vector3.UnitY);
        at = AssertTransform(at, 2, view, "view");

        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(60f * (MathF.PI / 180f), 1280f / 720f, 0.1f, 1000f);
        at = AssertTransform(at, 3, projection, "projection");

        Assert.Equal(CmdClear, FifoDword(at), "clear follows the camera transforms");
        Assert.Equal(36u, FifoDword(at + 4), "clear body: cid+flags+color+depth+stencil+1 rect");
        Assert.Equal(1u, FifoDword(at + 8), "cid");
        Assert.Equal(3u, FifoDword(at + 12), "clears color and depth");
        Assert.Equal(0xFF113366u, FifoDword(at + 16), "clear color");
        Assert.Equal(FloatBits(1f), FifoDword(at + 20), "depth reset to 1");
        Assert.Equal(0u, FifoDword(at + 24), "stencil untouched");
        Assert.Equal(0u, FifoDword(at + 28), "clear rect x");
        Assert.Equal(0u, FifoDword(at + 32), "clear rect y");
        Assert.Equal(1280u, FifoDword(at + 36), "clear rect width");
        Assert.Equal(720u, FifoDword(at + 40), "clear rect height");
        at += HeaderBytes + 36;

        // The mesh is untextured and the stage was configured at setup, so
        // the world transform comes next — no texture state in the stream.
        at = AssertTransform(at, 1, world, "world");

        Assert.Equal(CmdDrawPrimitives, FifoDword(at), "draw command");
        Assert.Equal(112u, FifoDword(at + 4), "draw body: cid+counts + 2 decls (36 each) + 1 range (28)");
        Assert.Equal(1u, FifoDword(at + 8), "cid");
        Assert.Equal(2u, FifoDword(at + 12), "two vertex declarations");
        Assert.Equal(1u, FifoDword(at + 16), "one primitive range");

        uint decl0 = at + 20;
        Assert.Equal(2u, FifoDword(decl0), "decl 0 type FLOAT3");
        Assert.Equal(0u, FifoDword(decl0 + 4), "decl 0 default method");
        Assert.Equal(0u, FifoDword(decl0 + 8), "decl 0 usage POSITION");
        Assert.Equal(0u, FifoDword(decl0 + 12), "decl 0 usage index 0");
        Assert.Equal(PositionSid, FifoDword(decl0 + 16), "decl 0 reads the position buffer");
        Assert.Equal(0u, FifoDword(decl0 + 20), "decl 0 offset 0");
        Assert.Equal(12u, FifoDword(decl0 + 24), "decl 0 stride 12");

        uint decl1 = decl0 + 36;
        Assert.Equal(4u, FifoDword(decl1), "decl 1 type D3DCOLOR");
        Assert.Equal(10u, FifoDword(decl1 + 8), "decl 1 usage COLOR");
        Assert.Equal(ColorSid, FifoDword(decl1 + 16), "decl 1 reads the color buffer");
        Assert.Equal(4u, FifoDword(decl1 + 24), "decl 1 stride 4");

        uint range = decl1 + 36;
        Assert.Equal(1u, FifoDword(range), "TRIANGLELIST");
        Assert.Equal(12u, FifoDword(range + 4), "12 triangles");
        Assert.Equal(IndexSid, FifoDword(range + 8), "indices read from the index buffer");
        Assert.Equal(0u, FifoDword(range + 12), "index array offset 0");
        Assert.Equal(2u, FifoDword(range + 16), "index array stride 2");
        Assert.Equal(2u, FifoDword(range + 20), "16-bit indices");
        Assert.Equal(0u, FifoDword(range + 24), "no index bias");
        at += HeaderBytes + 112;

        Assert.Equal(at, s_driver!.GetFIFO(FIFO.NextCmd), "the frame emits exactly these commands");
        Assert.True(s_canvas.ReadsFrom3DScene, "an open scene routes GetImage to the 3D color target");

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
        if (s_cube == null)
        {
            Assert.Fail("cube mesh not built (TestDrawCubeFifo did not run)");
            return;
        }

        uint start = CaptureStart();

        s_canvas!.DrawMesh(s_cube, Matrix4x4.Identity);

        Assert.Equal(CmdSetTransform, FifoDword(start), "cached camera: the world transform comes first");
        Assert.Equal(1u, FifoDword(start + 12), "transform type WORLD");
        Assert.Equal(CmdDrawPrimitives, FifoDword(start + 80), "the draw follows immediately");
        Assert.Equal(start + 200, s_driver!.GetFIFO(FIFO.NextCmd), "no other commands emitted");

        Rewind(start);

        s_canvas.Camera = new Camera3D(new Vector3(2f, 2f, 2f), Vector3.Zero);
        s_canvas.DrawMesh(s_cube, Matrix4x4.Identity);

        Assert.Equal(CmdSetTransform, FifoDword(start), "camera change re-applies the transforms");
        Assert.Equal(2u, FifoDword(start + 12), "view re-emitted first");
        Assert.Equal(3u, FifoDword(start + 92), "projection re-emitted second");
        Assert.Equal(1u, FifoDword(start + 172), "world transform follows");
        Assert.Equal(start + 360, s_driver.GetFIFO(FIFO.NextCmd), "view+projection+world+draw and nothing else");

        Rewind(start);
    }

    /// <summary>
    /// Disposing a texture must destroy its surface and leave every mesh
    /// that still maps it undrawable: DrawMesh rejects the mesh before
    /// anything reaches the FIFO, so the freed surface id is never bound
    /// again. The texture is injected with a known id because uploading one
    /// needs a live device. Without the rejection the draw bound the dead id
    /// and the device sampled whatever surface had reused it since.
    /// </summary>
    public static void TestDisposedTextureRejected()
    {
        uint start = CaptureStart();
        Texture texture = new(s_canvas!, 2, 2, new SVGA3dSurfaceImageId { sid = TextureSid });
        Mesh quad = new(s_canvas!, 4, 6, texture, MeshTopology.Triangles)
        {
            DriverData = SvgaII3DCanvas.BuildMeshData([5, 6], hasColors: false, hasUvs: true, 7, 6, MeshTopology.Triangles),
        };

        texture.Dispose();

        Assert.Equal(CmdSurfaceDestroy, FifoDword(start), "dispose destroys the surface");
        Assert.Equal(TextureSid, FifoDword(start + 8), "the destroyed surface is the texture's");
        Assert.True(texture.DriverData is null, "the driver slot goes with the surface");

        uint afterDispose = CaptureStart();
        try
        {
            s_canvas!.DrawMesh(quad, Matrix4x4.Identity);
            Assert.Fail("a mesh mapping a disposed texture was drawn");
        }
        catch (ArgumentException)
        {
        }

        Assert.Equal(afterDispose, s_driver!.GetFIFO(FIFO.NextCmd), "the rejected draw writes nothing to the FIFO");

        quad.Dispose();
        Rewind(start);
    }

    /// <summary>
    /// 3D discovery must fail without throwing on a display device that
    /// cannot render 3D, and the 2D canvas handed out instead must not
    /// masquerade as a <see cref="Canvas3D"/>. Runs last: it constructs the
    /// real canvas, which enables the SVGA device on the vmware-svga profile.
    /// </summary>
    public static void TestCanvas3DDiscovery()
    {
        // The ring's documented 3D discovery idiom: acquire, then test the type.
        Canvas canvas = Canvas.GetFullScreen(new Mode(640, 480, ColorDepth.ColorDepth32));

        Assert.True(canvas is not Canvas3D, "no CI display device negotiates 3D");
        Assert.True(FullScreenCanvas.Current is not Canvas3D, "the cached canvas does not report Canvas3D either");
    }

    /// <summary>
    /// Asserts a SETTRANSFORM command at <paramref name="at"/> carrying the
    /// given matrix bit for bit, and returns the offset of the next command.
    /// </summary>
    private static uint AssertTransform(uint at, uint type, Matrix4x4 expected, string label)
    {
        Assert.Equal(CmdSetTransform, FifoDword(at), label + " transform command");
        Assert.Equal(72u, FifoDword(at + 4), label + " transform body: cid+type+16 floats");
        Assert.Equal(1u, FifoDword(at + 8), label + " cid");
        Assert.Equal(type, FifoDword(at + 12), label + " transform type");

        float* m = (float*)&expected;
        for (int i = 0; i < 16; i++)
        {
            if (FifoDword(at + 16 + (uint)(i * 4)) != FloatBits(m[i]))
            {
                Assert.Fail(label + " matrix element " + i + " does not match");
                return at + HeaderBytes + 72;
            }
        }

        Assert.True(true, label + " matrix payload matches bit for bit");
        return at + HeaderBytes + 72;
    }
}
