using System;
using Cosmos.Kernel.System.Diagnostics;
using DevKernel.Diagnostics;
using DevKernel.Graphics;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Framebuffer demos: a background drawing thread, and the full-screen monitor.
/// </summary>
internal static class GraphicsCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Graphics";

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
                Name = "cube",
                Usage = "cube",
                Description = "Spinning 3D cube rolled by the mouse (VMware SVGA II only, Esc to exit)",
                Execute = static (context, args) => SpinningCubeDemo.Run(),
            });
    }
}
