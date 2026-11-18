// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;

namespace DevKernel.Diagnostics.Smp;

// Colors of the SMP monitor. A thread keeps one color wherever it runs, so the eye can
// follow it from one CPU's lane to another's.
internal static class SmpPalette
{
    public static readonly Color Background = Color.FromArgb(10, 12, 18);
    public static readonly Color Panel = Color.FromArgb(21, 25, 36);
    public static readonly Color Plot = Color.FromArgb(13, 15, 22);
    public static readonly Color Gridline = Color.FromArgb(34, 40, 56);
    public static readonly Color BarTrack = Color.FromArgb(40, 46, 62);
    public static readonly Color Text = Color.FromArgb(222, 227, 238);
    public static readonly Color Dim = Color.FromArgb(124, 134, 156);
    public static readonly Color Highlight = Color.FromArgb(255, 255, 255);

    // A CPU's load bar, by how much of its time is taken.
    public static readonly Color LoadLow = Color.FromArgb(61, 220, 132);
    public static readonly Color LoadBusy = Color.FromArgb(255, 176, 0);
    public static readonly Color LoadSaturated = Color.FromArgb(255, 92, 92);

    // The label of a chip that is filled with its thread's color.
    public static readonly Color ChipInk = Color.FromArgb(12, 14, 20);

    public static readonly Color Monitor = Color.FromArgb(232, 235, 242);
    public static readonly Color Managed = Color.FromArgb(92, 104, 134);
    public static readonly Color Kernel = Color.FromArgb(66, 76, 102);

    // As many as SmpScenario.MaxWorkers, so that no two workers share a color.
    private static readonly Color[] s_workers =
    [
        Color.FromArgb(255, 92, 92),
        Color.FromArgb(255, 176, 0),
        Color.FromArgb(61, 220, 132),
        Color.FromArgb(51, 181, 255),
        Color.FromArgb(199, 125, 255),
        Color.FromArgb(255, 126, 182),
        Color.FromArgb(77, 225, 193),
        Color.FromArgb(230, 224, 92),
    ];

    // As many as SmpScenario.MaxPoolThreads, so that no two pool threads share a color.
    private static readonly Color[] s_pool =
    [
        Color.FromArgb(46, 196, 182),
        Color.FromArgb(120, 170, 255),
        Color.FromArgb(160, 230, 120),
        Color.FromArgb(255, 160, 110),
        Color.FromArgb(190, 140, 255),
        Color.FromArgb(255, 130, 190),
        Color.FromArgb(235, 205, 90),
        Color.FromArgb(255, 110, 110),
    ];

    // One color per step of the scenario, for its banner.
    private static readonly Color[] s_steps =
    [
        Color.FromArgb(120, 170, 255),
        Color.FromArgb(61, 220, 132),
        Color.FromArgb(77, 225, 193),
        Color.FromArgb(255, 176, 0),
        Color.FromArgb(255, 92, 92),
        Color.FromArgb(199, 125, 255),
        Color.FromArgb(255, 126, 182),
        Color.FromArgb(46, 196, 182),
        Color.FromArgb(230, 224, 92),
    ];

    // The thread that plays the scenario is drawn as any managed thread: it is not part
    // of what the scenario shows.
    public static Color Of(SmpThreadRole role, int ordinal)
    {
        return role switch
        {
            SmpThreadRole.Worker => s_workers[ordinal % s_workers.Length],
            SmpThreadRole.Pool => s_pool[ordinal % s_pool.Length],
            SmpThreadRole.Monitor => Monitor,
            SmpThreadRole.Director => Managed,
            SmpThreadRole.Managed => Managed,
            _ => Kernel,
        };
    }

    public static Color OfStep(int step)
    {
        return s_steps[step % s_steps.Length];
    }

    // The color at a fraction of its brightness.
    public static Color Shade(Color color, int percent)
    {
        return Color.FromArgb(
            color.R * percent / SmpSampler.PercentMax,
            color.G * percent / SmpSampler.PercentMax,
            color.B * percent / SmpSampler.PercentMax);
    }
}
