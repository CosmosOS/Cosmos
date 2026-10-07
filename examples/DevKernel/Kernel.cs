using System;
using Cosmos.Kernel.System.Diagnostics;
using DevKernel.Commands;
using DevKernel.Network;
using DevKernel.Shell;
using DevKernel.Storage;
using Sys = Cosmos.Kernel.System;

namespace DevKernel;

/// <summary>
/// DevKernel - Test kernel for Cosmos gen3 development. Boots, registers the
/// FAT driver, then runs an interactive shell; the commands themselves live
/// under <c>Commands/</c> and are assembled by <see cref="CommandRegistry"/>.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Rule drawn above and below the boot banner.</summary>
    private const string BannerRule = "========================================";

    /// <summary>Virtual consoles opened at boot beside the primary one, as tty2 onwards.</summary>
    private const int BootVirtualConsoles = 3;

    private readonly ShellContext _shell = new(CommandRegistry.CreateDefault(), new NetworkSession());

    protected override void BeforeRun()
    {
        Log.WriteString("[DevKernel] BeforeRun() called\n");

        FatBootstrap.RegisterAndAutoMount();

        Console.Clear();
        Console.WriteLine(BannerRule);
        Console.WriteLine($"         CosmosOS {Sys.Kernel.VersionString} Shell       ");
        Console.WriteLine(BannerRule);
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Parameters:");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (string param in Environment.GetCommandLineArgs())
        {
            Console.Write('\t');
            Console.WriteLine(param);
        }

        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Cosmos booted successfully!");
        Console.ResetColor();
        Console.WriteLine("Type 'help' for available commands.");

        OpenVirtualConsoles();
        Console.WriteLine();
    }

    protected override void Run()
    {
        if (!ShellLoop.RunOnce(_shell))
        {
            Stop();
        }
    }

    /// <summary>
    /// Opens the virtual consoles beside the primary one, each with a shell
    /// of its own, so Alt and a function key switches between them.
    /// </summary>
    private void OpenVirtualConsoles()
    {
        for (int i = 0; i < BootVirtualConsoles; i++)
        {
            try
            {
                SessionShells.OpenVirtualConsole(_shell);
            }
            catch (InvalidOperationException ex)
            {
                Terminal.Warning($"No virtual consoles: {ex.Message}");
                return;
            }
        }

        Terminal.Hint($"Alt+F1..F{BootVirtualConsoles + 1} switches between the consoles; 'telnetd' serves shells over Telnet.");
    }

    protected override void AfterRun()
    {
        Log.WriteString("[DevKernel] AfterRun() called\n");
        Console.WriteLine("Goodbye!");
    }
}
