using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using DevKernel.Shell;
using MouseManager = Cosmos.Kernel.System.Mouse.MouseManager;
using SysThread = System.Threading.Thread;

namespace DevKernel.Graphics;

/// <summary>
/// Full-screen 3D demo behind <c>cube</c>: a cube with one color per face
/// spinning above a grid, rolling in the direction the mouse points away from
/// the center of the screen, the further out the faster. Exercises the
/// <see cref="Canvas3D"/> API end to end — mesh creation, camera, per-frame
/// clear, world transforms and present. Runs until Escape.
/// </summary>
/// <remarks>
/// 3D is only reachable on the VMware SVGA II adapter once it has negotiated
/// SVGA3D, so the demo reports the device it got and returns on every other
/// one, QEMU included.
/// </remarks>
internal static class SpinningCubeDemo
{
    /// <summary>Edge length (world units) of the cube.</summary>
    private const float CubeSize = 1f;

    /// <summary>Center of the cube, high enough above the grid that no corner crosses it.</summary>
    private static readonly Vector3 s_cubeCenter = new(0f, 1f, 0f);

    /// <summary>Where the camera stands.</summary>
    private static readonly Vector3 s_cameraPosition = new(0f, 2.6f, 4.6f);

    /// <summary>What the camera looks at: the middle of the cube.</summary>
    private static readonly Vector3 s_cameraTarget = new(0f, 0.9f, 0f);

    /// <summary>Cells along each side of the ground grid.</summary>
    private const int GridSlices = 12;

    /// <summary>Edge length (world units) of one grid cell.</summary>
    private const float GridSpacing = 0.5f;

    /// <summary>Height (world units) of the direction arrow above the grid, so the two do not z-fight.</summary>
    private const float ArrowHeight = 0.02f;

    /// <summary>Half-width (world units) of the base of the direction arrow.</summary>
    private const float ArrowHalfWidth = 0.12f;

    /// <summary>Length (world units) of the direction arrow at full deflection.</summary>
    private const float ArrowLength = 2.5f;

    /// <summary>Roll rate (radians per second) at full mouse deflection.</summary>
    private const float MaxRollRate = 3.5f;

    /// <summary>Rate (radians per second) the cube spins about Y at with the mouse centered.</summary>
    private const float IdleSpinRate = 0.6f;

    /// <summary>Mouse deflection below which the cube is left to its idle spin.</summary>
    private const float DeadZone = 0.04f;

    /// <summary>Longest step (seconds) integrated in one frame, so a stall does not spin the cube wildly.</summary>
    private const float MaxFrameSeconds = 0.1f;

    /// <summary>Delay (ms) between frames, targeting ~60 Hz.</summary>
    private const int FrameDelayMs = 16;

    /// <summary>Vertices per face of the cube: one quad, not shared with its neighbours so each face is flat-colored.</summary>
    private const int VerticesPerFace = 4;

    /// <summary>Runs the demo; returns when Escape is pressed.</summary>
    public static void Run()
    {
        Canvas canvas = Canvas.GetFullScreen();

        if (canvas is not Canvas3D canvas3D)
        {
            Terminal.Error("No 3D on this display device (" + canvas.Name + ").");
            Terminal.Hint("3D needs the VMware SVGA II adapter with SVGA3D negotiated: run the ISO in VMware, not QEMU.");
            Console.WriteLine();
            return;
        }

        Log.Write("[Cube3D] Canvas " + canvas3D.Name + " " + canvas3D.Mode + ", accelerated: " + canvas3D.IsAccelerated + "\n");

        if (KernelFeatures.Mouse)
        {
            MouseManager.SetScreenSize(canvas3D.Width, canvas3D.Height);
            MouseManager.SetPosition(canvas3D.Width / 2, canvas3D.Height / 2);
        }

        canvas3D.Camera = new Camera3D(s_cameraPosition, s_cameraTarget);

        Mesh cube = CreateCube(canvas3D);
        Mesh arrow = CreateArrow(canvas3D);

        Quaternion orientation = Quaternion.Identity;
        long frequency = Stopwatch.Frequency;
        long previousTicks = Stopwatch.GetTimestamp();

        while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
        {
            long nowTicks = Stopwatch.GetTimestamp();
            float elapsed = (float)(nowTicks - previousTicks) / frequency;
            previousTicks = nowTicks;

            if (elapsed > MaxFrameSeconds)
            {
                elapsed = MaxFrameSeconds;
            }

            Vector3 drive = ReadDrive(canvas3D);
            orientation = Advance(orientation, drive, elapsed);

            canvas3D.ClearScene(Color.FromArgb(0x10, 0x14, 0x20));
            canvas3D.DrawGrid(GridSlices, GridSpacing, Color.FromArgb(0x30, 0x3A, 0x50));
            DrawDriveArrow(canvas3D, arrow, drive);

            canvas3D.DrawMesh(
                cube,
                Matrix4x4.CreateScale(CubeSize) *
                Matrix4x4.CreateFromQuaternion(orientation) *
                Matrix4x4.CreateTranslation(s_cubeCenter));

            canvas3D.Display();
            SysThread.Sleep(FrameDelayMs);
        }

        cube.Dispose();
        arrow.Dispose();
        Console.Clear();
    }

    /// <summary>
    /// Reads how far the mouse sits from the center of the screen as a push
    /// on the ground plane: screen right is +X, screen down is +Z, and the
    /// magnitude is 0 at the center and 1 at the edges.
    /// </summary>
    private static Vector3 ReadDrive(Canvas3D canvas)
    {
        if (!KernelFeatures.Mouse)
        {
            return Vector3.Zero;
        }

        float halfWidth = canvas.Width * 0.5f;
        float halfHeight = canvas.Height * 0.5f;

        Vector3 drive = new(
            (MouseManager.X - halfWidth) / halfWidth,
            0f,
            (MouseManager.Y - halfHeight) / halfHeight);

        float deflection = drive.Length();

        if (deflection < DeadZone)
        {
            return Vector3.Zero;
        }

        // The screen is wider than it is tall, so a corner reads past 1.
        return deflection > 1f ? drive / deflection : drive;
    }

    /// <summary>
    /// Turns the cube by one frame. A cube rolling on the ground in direction
    /// <paramref name="drive"/> turns about the axis perpendicular to both the
    /// ground normal and the push, at a rate proportional to how hard it is
    /// pushed; with no push it keeps its idle spin about Y.
    /// </summary>
    private static Quaternion Advance(Quaternion orientation, Vector3 drive, float elapsed)
    {
        Vector3 spin = (Vector3.Cross(Vector3.UnitY, drive) * MaxRollRate) + (Vector3.UnitY * IdleSpinRate);
        float rate = spin.Length();

        if (rate <= 0f)
        {
            return orientation;
        }

        Quaternion step = Quaternion.CreateFromAxisAngle(spin / rate, rate * elapsed);
        return Quaternion.Normalize(Quaternion.Concatenate(orientation, step));
    }

    /// <summary>
    /// Draws the arrow flat on the grid, pointing where the mouse pushes and
    /// as long as the push is hard. Nothing is drawn while the mouse rests in
    /// the dead zone.
    /// </summary>
    private static void DrawDriveArrow(Canvas3D canvas, Mesh arrow, Vector3 drive)
    {
        float deflection = drive.Length();

        if (deflection <= 0f)
        {
            return;
        }

        // The arrow points along +Z and is one unit wide; its world transform
        // maps +Z onto the push direction and +X onto the perpendicular.
        Vector3 forward = drive / deflection;
        Vector3 right = Vector3.Cross(Vector3.UnitY, forward);
        float length = ArrowLength * deflection;

        Matrix4x4 world = new(
            right.X * ArrowHalfWidth, right.Y * ArrowHalfWidth, right.Z * ArrowHalfWidth, 0f,
            0f, 1f, 0f, 0f,
            forward.X * length, forward.Y * length, forward.Z * length, 0f,
            0f, ArrowHeight, 0f, 1f);

        canvas.DrawMesh(arrow, world);
    }

    /// <summary>
    /// Builds the unit cube, one color per face. Faces do not share vertices:
    /// shading is smooth, so a corner shared by three faces would blend their
    /// colors and the rotation would be far harder to read.
    /// </summary>
    private static Mesh CreateCube(Canvas3D canvas)
    {
        const float H = 0.5f;

        ReadOnlySpan<Vector3> positions =
        [
            new(H, -H, H), new(H, -H, -H), new(H, H, -H), new(H, H, H),         // +X
            new(-H, -H, -H), new(-H, -H, H), new(-H, H, H), new(-H, H, -H),     // -X
            new(-H, H, H), new(H, H, H), new(H, H, -H), new(-H, H, -H),         // +Y
            new(-H, -H, -H), new(H, -H, -H), new(H, -H, H), new(-H, -H, H),     // -Y
            new(-H, -H, H), new(H, -H, H), new(H, H, H), new(-H, H, H),         // +Z
            new(H, -H, -H), new(-H, -H, -H), new(-H, H, -H), new(H, H, -H),     // -Z
        ];

        ReadOnlySpan<Color> faceColors =
        [
            Color.Crimson,
            Color.MediumSeaGreen,
            Color.Gold,
            Color.DarkOrange,
            Color.DodgerBlue,
            Color.MediumOrchid,
        ];

        Span<uint> colors = stackalloc uint[positions.Length];
        Span<ushort> indices = stackalloc ushort[faceColors.Length * 6];

        for (int face = 0; face < faceColors.Length; face++)
        {
            uint argb = (uint)faceColors[face].ToArgb();
            int first = face * VerticesPerFace;

            for (int corner = 0; corner < VerticesPerFace; corner++)
            {
                colors[first + corner] = argb;
            }

            int index = face * 6;
            indices[index] = (ushort)first;
            indices[index + 1] = (ushort)(first + 1);
            indices[index + 2] = (ushort)(first + 2);
            indices[index + 3] = (ushort)(first + 2);
            indices[index + 4] = (ushort)(first + 3);
            indices[index + 5] = (ushort)first;
        }

        return canvas.CreateMesh(positions, colors, indices);
    }

    /// <summary>
    /// Builds the direction arrow: a flat triangle two units wide, pointing
    /// one unit along +Z, that <see cref="DrawDriveArrow"/> scales and aims.
    /// </summary>
    private static Mesh CreateArrow(Canvas3D canvas)
    {
        ReadOnlySpan<Vector3> positions =
        [
            new(-1f, 0f, 0f),
            new(1f, 0f, 0f),
            new(0f, 0f, 1f),
        ];

        uint argb = (uint)Color.Aqua.ToArgb();
        ReadOnlySpan<uint> colors = [argb, argb, argb];
        ReadOnlySpan<ushort> indices = [0, 1, 2];

        return canvas.CreateMesh(positions, colors, indices);
    }
}
