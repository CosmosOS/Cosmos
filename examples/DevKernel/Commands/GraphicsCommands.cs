using System;
using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using DevKernel.Diagnostics;
using DevKernel.Graphics;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Framebuffer demos: a background drawing thread, and the full-screen monitor;
/// and the displays the driver kit published, with a hardware cursor test.
/// </summary>
internal static class GraphicsCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Graphics";

    /// <summary>Label column width of the display listing.</summary>
    private const int LabelWidth = 14;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "gfx",
                Usage = "gfx",
                Description = "Start graphics thread (draws color-cycling square)",
                Execute = static (context, args) =>
                {
                    Log.WriteString("[GfxThread] Starting graphics thread\n");
                    Terminal.Info("Starting graphics thread (draws color-cycling square)...");

                    ColorSquareWorker.Start();

                    Terminal.Success("Graphics thread started!");
                    Console.WriteLine();
                },
            },
            new ShellCommand
            {
                Name = "startx",
                Usage = "startx",
                Description = "Full-screen memory/GC/FPS monitor (runs until reset)",
                Execute = static (context, args) => SystemMonitor.Run(),
            },
            new ShellCommand
            {
                Name = "display",
                Usage = "display [cursor]",
                Description = "List the displays, or move a hardware cursor around the primary one",
                MaxArgs = 1,
                Execute = static (context, args) =>
                {
                    if (args.Count == 0)
                    {
                        ListDisplays();
                    }
                    else if (args[0] == "cursor")
                    {
                        HardwareCursorDemo.Run();
                    }
                    else
                    {
                        args.PrintUsage();
                    }
                },
            },
            new ShellCommand
            {
                Name = "cube",
                Usage = "cube",
                Description = "Spinning 3D cube rolled by the mouse (VMware SVGA II only, Esc to exit)",
                Execute = static (context, args) => SpinningCubeDemo.Run(),
            });
    }

    /// <summary>Prints every published display: who drives it, its mode and its facets, the primary first.</summary>
    private static void ListDisplays()
    {
        int count = DisplayManager.Count;
        if (count == 0)
        {
            Terminal.Warning("No display published.");
            return;
        }

        Terminal.Header($"Displays ({count})");
        for (int i = 0; i < count; i++)
        {
            if (!DisplayManager.TryGet(i, out DisplayDevice? display))
            {
                continue;
            }

            string facets = (display.TryGetFacet(out IDisplayModes? _) ? "modes " : string.Empty)
                + (display.TryGetFacet(out IHardwareCursor? _) ? "hardware-cursor" : string.Empty);
            Terminal.InfoLine("name", i == 0 ? $"{display.Name} (primary)" : display.Name, LabelWidth);
            Terminal.InfoLine("driver", display.DriverName, LabelWidth);
            Terminal.InfoLine("node", display.NodePath ?? "(firmware)", LabelWidth);
            Terminal.InfoLine("mode", $"{display.Width}x{display.Height}x{display.BitsPerPixel} @ {display.RefreshRate} Hz, pitch {display.Pitch}", LabelWidth);
            Terminal.InfoLine("facets", facets.Length == 0 ? "(none)" : facets.TrimEnd(), LabelWidth);
            Console.WriteLine();
        }
    }
}
