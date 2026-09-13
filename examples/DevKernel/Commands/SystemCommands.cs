using System;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Timer;
using DevKernel.Shell;
using Sys = Cosmos.Kernel.System;

namespace DevKernel.Commands;

/// <summary>
/// Shell basics: help, screen, system identity, and the power transitions.
/// </summary>
internal static class SystemCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "System";

    /// <summary>Milliseconds in one second; delay of each countdown step of the timer test.</summary>
    private const uint OneSecondMs = 1000;

    /// <summary>Number of seconds counted down by the timer test.</summary>
    private const int CountdownStartSeconds = 10;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "help",
                Usage = "help",
                Description = "Show this help message",
                Execute = static (context, args) => context.Shell.PrintHelp(),
            },
            new ShellCommand
            {
                Name = "clear",
                Aliases = ["cls"],
                Usage = "clear",
                Description = "Clear the screen",
                Execute = static (context, args) => Console.Clear(),
            },
            new ShellCommand
            {
                Name = "echo",
                Usage = "echo <text>",
                Description = "Echo back text",
                MaxArgs = ShellCommand.UnlimitedArgs,
                Execute = static (context, args) =>
                {
                    if (args.Count > 0)
                    {
                        Console.WriteLine(args.RawTail);
                    }
                },
            },
            new ShellCommand
            {
                Name = "info",
                Aliases = ["sysinfo"],
                Usage = "info",
                Description = "Show system information",
                Execute = static (context, args) => PrintSystemInfo(),
            },
            new ShellCommand
            {
                Name = "timer",
                Usage = "timer",
                Description = "Test 10 second countdown timer",
                Execute = static (context, args) => RunTimerTest(),
            },
            new ShellCommand
            {
                Name = "halt",
                Usage = "halt",
                Description = "Halt the CPU (does not power off)",
                Execute = static (context, args) =>
                {
                    Terminal.Warning("Halting CPU...");
                    Sys.Power.Halt();
                },
            },
            new ShellCommand
            {
                Name = "reboot",
                Usage = "reboot",
                Description = "Restart the machine",
                Execute = static (context, args) =>
                {
                    Terminal.Warning("Rebooting...");
                    Sys.Power.Reboot();
                },
            },
            new ShellCommand
            {
                Name = "shutdown",
                Usage = "shutdown",
                Description = "Power off the machine",
                Execute = static (context, args) =>
                {
                    Terminal.Warning("Shutting down...");
                    Sys.Power.Shutdown();
                },
            });
    }

    private static void PrintSystemInfo()
    {
        Terminal.Header("System Information:");

        Terminal.InfoLine("OS", $"CosmosOS v{Sys.Kernel.VersionString} (gen3)");
        Terminal.InfoLine("Runtime", "NativeAOT");
#if ARCH_X64
        Terminal.InfoLine("Architecture", "x86-64");
#elif ARCH_ARM64
        Terminal.InfoLine("Architecture", "ARM64");
#endif
        // Terminal writes through System.Console, which is backed by this same
        // console, so reaching this line means it is initialized. The test is
        // what tells the compiler Default is non-null.
        if (KernelConsole.IsInitialized)
        {
            Terminal.InfoLine("Console", KernelConsole.Default.Cols + "x" + KernelConsole.Default.Rows + " chars");

            Mode mode = KernelConsole.Default.Canvas.Mode;
            Terminal.InfoLine(
                "Framebuffer",
                mode.Width + "x" + mode.Height + "x" + (int)mode.ColorDepth + " (" + KernelConsole.Default.Canvas.Name + ")");
        }
    }

    private static void RunTimerTest()
    {
        Terminal.Info($"Starting {CountdownStartSeconds} second countdown...");
        for (int i = CountdownStartSeconds; i > 0; i--)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(i.ToString());
            Console.ResetColor();
            Console.WriteLine("...");
            TimerManager.Wait(OneSecondMs);
        }

        Terminal.Success("Timer test complete!");
        Console.WriteLine();
    }
}
