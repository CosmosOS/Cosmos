using System;
using Cosmos.Kernel.System.Diagnostics;
using DevKernel.Commands;
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

    private readonly ShellContext _shell = new(CommandRegistry.CreateDefault());

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
        Console.WriteLine();
    }

    protected override void Run()
    {
        Terminal.WritePrompt(_shell.Prompt, _shell.Cwd);

        try
        {
            string? input = Console.ReadLine();
            if (input == null)
            {
                // No console left to read from; end the main loop rather than
                // spin on it forever.
                Stop();
                return;
            }

            if (input.Trim().Length == 0)
            {
                return;
            }

            _shell.Shell.Execute(_shell, input);
        }
        catch (Exception ex)
        {
            Terminal.Error($"Exception: {ex.Message}");
            Stop();
        }
    }

    protected override void AfterRun()
    {
        Log.WriteString("[DevKernel] AfterRun() called\n");
        Console.WriteLine("Goodbye!");
    }
}
