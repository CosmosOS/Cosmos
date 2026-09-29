using System.Numerics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.TestRunner.Framework;

namespace Cosmos.Kernel.Tests.Graphic;

/// <summary>
/// Tests for the public <see cref="Canvas3D"/> API that hold on every cell:
/// the camera's defaults, and the ring's 3D discovery idiom on a display
/// that cannot render 3D. The 3D canvas itself comes from a display driver
/// through <see cref="ICanvas3DFactory"/>, and no CI cell has a display
/// that negotiates 3D, so the canvas the ring hands out is never a
/// <see cref="Canvas3D"/> here.
/// </summary>
public static class Canvas3DTests
{
    /// <summary>Width of the mode the discovery test requests.</summary>
    private const int RequestedWidth = 640;

    /// <summary>Height of the mode the discovery test requests.</summary>
    private const int RequestedHeight = 480;

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
    /// 3D discovery must fail without throwing on a display that cannot
    /// render 3D, and the 2D canvas handed out instead must not masquerade
    /// as a <see cref="Canvas3D"/>. Runs last: it acquires the full-screen
    /// canvas with a mode request, which a display that switches modes
    /// honours under the console's canvas.
    /// </summary>
    public static void TestCanvas3DDiscovery()
    {
        // The ring's documented 3D discovery idiom: acquire, then test the type.
        Canvas canvas = Canvas.GetFullScreen(new Mode(RequestedWidth, RequestedHeight, ColorDepth.ColorDepth32));

        Assert.True(canvas is not Canvas3D, "no CI display device negotiates 3D");
    }
}
