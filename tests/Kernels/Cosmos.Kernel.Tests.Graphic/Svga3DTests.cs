using System;
using System.Numerics;
using Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.TestRunner.Framework;

namespace Cosmos.Kernel.Tests.Graphic;

/// <summary>
/// Wire-format tests for the SVGA3D command layer (<see cref="VMWareSVGAII3D"/>).
/// QEMU's vmware-svga device negotiates no 3D capability, so the commands can
/// never be executed host-side; what CAN be validated under QEMU is the guest
/// half of the contract: each call must place a correctly formed command —
/// protocol command id, header size, field order and packing — into the FIFO.
/// The driver is bound without ever enabling the device (no SetMode), and QEMU
/// only consumes the FIFO while the device is enabled, so written commands sit
/// inert in FIFO memory for inspection and are then discarded by rewinding
/// NEXT_CMD. Expected ids and sizes are hard-coded from the SVGA3D protocol
/// (svga3d_reg.h), not read back from the kernel's own enums, so a wrong enum
/// value is caught instead of being compared against itself.
/// </summary>
public static unsafe class Svga3DTests
{
    /// <summary>Skip reason for the FIFO tests on cells without the adapter (the bare profile, and all of arm64).</summary>
    public const string SkipNoDevice = "VMware SVGA II adapter not present, needs the vmware-svga profile";

    /// <summary>PCI vendor id of VMware.</summary>
    private const ushort VMwareVendorId = 0x15AD;

    /// <summary>PCI device id of the SVGA II adapter.</summary>
    private const ushort SvgaIIDeviceId = 0x0405;

    /// <summary>SVGA_3D_CMD_SURFACE_DESTROY (SVGA_3D_CMD_BASE + 1).</summary>
    private const uint CmdSurfaceDestroy = 1041;

    /// <summary>SVGA_3D_CMD_SURFACE_DMA (SVGA_3D_CMD_BASE + 4).</summary>
    private const uint CmdSurfaceDma = 1044;

    /// <summary>SVGA_3D_CMD_CONTEXT_DESTROY (SVGA_3D_CMD_BASE + 6).</summary>
    private const uint CmdContextDestroy = 1046;

    /// <summary>SVGA_3D_CMD_SETMATERIAL (SVGA_3D_CMD_BASE + 12).</summary>
    private const uint CmdSetMaterial = 1052;

    /// <summary>SVGA_3D_CMD_SETLIGHTDATA (SVGA_3D_CMD_BASE + 13).</summary>
    private const uint CmdSetLightData = 1053;

    /// <summary>SVGA_3D_CMD_SETLIGHTENABLED (SVGA_3D_CMD_BASE + 14).</summary>
    private const uint CmdSetLightEnabled = 1054;

    /// <summary>SVGA_3D_CMD_SHADER_DESTROY (SVGA_3D_CMD_BASE + 20).</summary>
    private const uint CmdShaderDestroy = 1060;

    /// <summary>Size of SVGA3dCmdHeader (id + size) preceding every 3D command body.</summary>
    private const uint HeaderBytes = 8;

    /// <summary>Arbitrary context id used across the command tests.</summary>
    private const uint TestCid = 7;

    private static PciDevice? s_device;
    private static SvgaIIDriver? s_driver;
    private static VMWareSVGAII3D? s_svga3d;

    /// <summary>True when the SVGA II adapter was enumerated on the PCI bus.</summary>
    public static bool DevicePresent => s_device != null;

    /// <summary>True when <see cref="TestDriverBind"/> bound the driver and 3D layer.</summary>
    public static bool Ready => s_svga3d != null;

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
    /// Discard everything written since <paramref name="start"/>. The device is
    /// never enabled during this suite so nothing races the rewind, and it
    /// guarantees the FIFO holds no 3D commands (which QEMU cannot parse) if
    /// the device were ever enabled later.
    /// </summary>
    private static void Rewind(uint start) => s_driver!.SetFIFO(FIFO.NextCmd, start);

    /// <summary>
    /// Host-independent guard on the wire sizes of the command structs the 3D
    /// layer reserves FIFO space with. Pack = 1 sequential layout must match
    /// the SVGA3D protocol structs byte for byte; a drifted size corrupts the
    /// FIFO stream at the very first command.
    /// </summary>
    public static void TestCommandStructSizes()
    {
        Assert.Equal(12, sizeof(SVGA3dCmdSetLightEnabled), "SVGA3dCmdSetLightEnabled is cid+index+enabled");
        Assert.Equal(116, sizeof(SVGA3dLightData), "SVGA3dLightData matches the 116-byte protocol struct");
        Assert.Equal(124, sizeof(SVGA3dCmdSetLightData), "SVGA3dCmdSetLightData is cid+index+SVGA3dLightData");
        Assert.Equal(68, sizeof(SVGA3dMaterial), "SVGA3dMaterial matches the 68-byte protocol struct");
        Assert.Equal(76, sizeof(SVGA3dCmdSetMaterial), "SVGA3dCmdSetMaterial is cid+face+SVGA3dMaterial");
        Assert.Equal(12, sizeof(SVGA3dCmdDestroyShader), "SVGA3dCmdDestroyShader is cid+shid+type");
    }

    /// <summary>
    /// Bind the driver and 3D layer to the adapter. Deliberately no SetMode:
    /// the device stays disabled, the suite's GOP framebuffer keeps the
    /// display, and QEMU never starts consuming the FIFO.
    /// </summary>
    public static void TestDriverBind()
    {
        try
        {
            SvgaIIDriver driver = new SvgaIIDriver(s_device!);
            s_driver = driver;
            s_svga3d = new VMWareSVGAII3D(driver);
        }
        catch (Exception ex)
        {
            Assert.Fail("SVGAII driver bind threw: " + ex.Message);
            return;
        }

        Assert.True(s_driver!.GetFIFO(FIFO.Min) < s_driver.GetFIFO(FIFO.Max), "FIFO Min/Max initialized");
        Assert.Equal(s_driver.GetFIFO(FIFO.Min), s_driver.GetFIFO(FIFO.NextCmd), "FIFO NEXT_CMD starts at Min");
    }

    public static void TestSetLightEnabled()
    {
        uint start = CaptureStart();

        s_svga3d!.SetLightEnable(TestCid, 2, true);

        Assert.Equal(CmdSetLightEnabled, FifoDword(start), "command id is SETLIGHTENABLED");
        Assert.Equal(12u, FifoDword(start + 4), "header size is the 12-byte body");
        Assert.Equal(TestCid, FifoDword(start + 8), "cid");
        Assert.Equal(2u, FifoDword(start + 12), "light index");
        Assert.Equal(1u, FifoDword(start + 16), "enabled=true encodes as 1");
        Assert.Equal(start + HeaderBytes + 12, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);

        s_svga3d.SetLightEnable(TestCid, 2, false);
        Assert.Equal(0u, FifoDword(start + 16), "enabled=false encodes as 0");

        Rewind(start);
    }

    public static void TestSetLightData()
    {
        SVGA3dLightData data = new SVGA3dLightData
        {
            type = LightType.SVGA3D_LIGHTTYPE_SPOT1,
            inWorldSpace = 1,
            diffuse = new Vector4(1f, 2f, 3f, 4f),
            specular = new Vector4(5f, 6f, 7f, 8f),
            ambient = new Vector4(9f, 10f, 11f, 12f),
            position = new Vector4(13f, 14f, 15f, 16f),
            direction = new Vector4(17f, 18f, 19f, 20f),
            range = 21f,
            falloff = 22f,
            attenuation = new Vector3(23f, 24f, 25f),
            theta = 26f,
            phi = 27f,
        };

        uint start = CaptureStart();

        s_svga3d!.SetLightData(TestCid, 1, data);

        Assert.Equal(CmdSetLightData, FifoDword(start), "command id is SETLIGHTDATA");
        Assert.Equal(124u, FifoDword(start + 4), "header size is the 124-byte body");
        Assert.Equal(TestCid, FifoDword(start + 8), "cid");
        Assert.Equal(1u, FifoDword(start + 12), "light index");
        Assert.Equal((uint)LightType.SVGA3D_LIGHTTYPE_SPOT1, FifoDword(start + 16), "light type");
        Assert.Equal(1u, FifoDword(start + 20), "inWorldSpace");
        Assert.Equal(FloatBits(1f), FifoDword(start + 24), "diffuse.X");
        Assert.Equal(FloatBits(4f), FifoDword(start + 36), "diffuse.W");
        Assert.Equal(FloatBits(5f), FifoDword(start + 40), "specular.X");
        Assert.Equal(FloatBits(9f), FifoDword(start + 56), "ambient.X");
        Assert.Equal(FloatBits(13f), FifoDword(start + 72), "position.X");
        Assert.Equal(FloatBits(17f), FifoDword(start + 88), "direction.X");
        Assert.Equal(FloatBits(21f), FifoDword(start + 104), "range");
        Assert.Equal(FloatBits(22f), FifoDword(start + 108), "falloff");
        Assert.Equal(FloatBits(23f), FifoDword(start + 112), "attenuation.X");
        Assert.Equal(FloatBits(25f), FifoDword(start + 120), "attenuation.Z");
        Assert.Equal(FloatBits(26f), FifoDword(start + 124), "theta");
        // phi is the last dword of the body: it landing here proves the whole
        // 116-byte SVGA3dLightData copied with no packing drift anywhere.
        Assert.Equal(FloatBits(27f), FifoDword(start + 128), "phi at end of body");
        Assert.Equal(start + HeaderBytes + 124, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);
    }

    public static void TestSetMaterial()
    {
        SVGA3dMaterial material = new SVGA3dMaterial
        {
            diffuse = new Vector4(1f, 2f, 3f, 4f),
            ambient = new Vector4(5f, 6f, 7f, 8f),
            specular = new Vector4(9f, 10f, 11f, 12f),
            emissive = new Vector4(13f, 14f, 15f, 16f),
            shininess = 32f,
        };

        uint start = CaptureStart();

        s_svga3d!.SetMaterial(TestCid, Face.SVGA3D_FACE_FRONT_BACK, material);

        Assert.Equal(CmdSetMaterial, FifoDword(start), "command id is SETMATERIAL");
        Assert.Equal(76u, FifoDword(start + 4), "header size is the 76-byte body");
        Assert.Equal(TestCid, FifoDword(start + 8), "cid");
        Assert.Equal((uint)Face.SVGA3D_FACE_FRONT_BACK, FifoDword(start + 12), "face");
        Assert.Equal(FloatBits(1f), FifoDword(start + 16), "diffuse.X");
        Assert.Equal(FloatBits(5f), FifoDword(start + 32), "ambient.X");
        Assert.Equal(FloatBits(9f), FifoDword(start + 48), "specular.X");
        Assert.Equal(FloatBits(13f), FifoDword(start + 64), "emissive.X");
        // shininess is the last dword of the body (see phi in TestSetLightData).
        Assert.Equal(FloatBits(32f), FifoDword(start + 80), "shininess at end of body");
        Assert.Equal(start + HeaderBytes + 76, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);
    }

    public static void TestDestroyContext()
    {
        uint start = CaptureStart();

        s_svga3d!.DestroyContext(TestCid);

        Assert.Equal(CmdContextDestroy, FifoDword(start), "command id is CONTEXT_DESTROY");
        Assert.Equal(4u, FifoDword(start + 4), "header size is the 4-byte body");
        Assert.Equal(TestCid, FifoDword(start + 8), "cid");
        Assert.Equal(start + HeaderBytes + 4, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);
    }

    public static void TestDestroySurface()
    {
        uint start = CaptureStart();

        s_svga3d!.DestroySurface(42);

        Assert.Equal(CmdSurfaceDestroy, FifoDword(start), "command id is SURFACE_DESTROY");
        Assert.Equal(4u, FifoDword(start + 4), "header size is the 4-byte body");
        Assert.Equal(42u, FifoDword(start + 8), "sid");
        Assert.Equal(start + HeaderBytes + 4, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);
    }

    /// <summary>
    /// The single-box surface DMA used by the 3D readback path
    /// (PresentToImage, and Canvas3D's GetImage on a rendered scene) must
    /// encode the guest pointer, host image, transfer direction and copy box
    /// exactly. Only the command is enqueued here — the fence/sync that
    /// completes a real transfer needs a live device.
    /// </summary>
    public static void TestSurfaceDmaReadback()
    {
        uint start = CaptureStart();

        s_svga3d!.EnqueueSurfaceDma(
            new SVGA3dSurfaceImageId { sid = 9 },
            new SVGAGuestPtr { gmrId = 0xFFFFFFFE, offset = 0x1234 },
            new SVGA3dRect(3, 5, 640, 360),
            SVGA3dTransferType.SVGA3D_READ_HOST_VRAM);

        Assert.Equal(CmdSurfaceDma, FifoDword(start), "command id is SURFACE_DMA");
        Assert.Equal(64u, FifoDword(start + 4), "header size is guest image + host image + transfer + 1 copy box");
        Assert.Equal(0xFFFFFFFEu, FifoDword(start + 8), "guest pointer gmr id (framebuffer GMR)");
        Assert.Equal(0x1234u, FifoDword(start + 12), "guest pointer offset");
        Assert.Equal(0u, FifoDword(start + 16), "tightly packed (pitch 0)");
        Assert.Equal(9u, FifoDword(start + 20), "host image sid");
        Assert.Equal(0u, FifoDword(start + 24), "host image face");
        Assert.Equal(0u, FifoDword(start + 28), "host image mipmap");
        Assert.Equal(2u, FifoDword(start + 32), "transfer READ_HOST_VRAM");
        Assert.Equal(3u, FifoDword(start + 36), "box x");
        Assert.Equal(5u, FifoDword(start + 40), "box y");
        Assert.Equal(0u, FifoDword(start + 44), "box z");
        Assert.Equal(640u, FifoDword(start + 48), "box width");
        Assert.Equal(360u, FifoDword(start + 52), "box height");
        Assert.Equal(1u, FifoDword(start + 56), "box depth 1");
        Assert.Equal(0u, FifoDword(start + 60), "source x untouched");
        Assert.Equal(0u, FifoDword(start + 68), "source z untouched");
        Assert.Equal(start + HeaderBytes + 64, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);
    }

    public static void TestDestroyShader()
    {
        uint start = CaptureStart();

        s_svga3d!.DestroyShader(TestCid, 3, SVGA3dShaderType.SVGA3D_SHADERTYPE_PS);

        Assert.Equal(CmdShaderDestroy, FifoDword(start), "command id is SHADER_DESTROY");
        Assert.Equal(12u, FifoDword(start + 4), "header size is the 12-byte body");
        Assert.Equal(TestCid, FifoDword(start + 8), "cid");
        Assert.Equal(3u, FifoDword(start + 12), "shid");
        Assert.Equal((uint)SVGA3dShaderType.SVGA3D_SHADERTYPE_PS, FifoDword(start + 16), "shader type");
        Assert.Equal(start + HeaderBytes + 12, s_driver!.GetFIFO(FIFO.NextCmd), "NEXT_CMD advanced by header+body");

        Rewind(start);
    }
}
