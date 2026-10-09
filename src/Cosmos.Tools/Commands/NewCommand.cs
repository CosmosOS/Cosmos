using System.ComponentModel;
using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cosmos.Tools.Commands;

public class NewSettings : CommandSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Name of the new kernel project")]
    public string Name { get; set; } = string.Empty;

    [CommandOption("-o|--output")]
    [Description("Output directory (defaults to current directory)")]
    public string? Output { get; set; }

    /*
    [CommandOption("-a|--arch")]
    [Description("Target architecture (x64, arm64)")]
    [DefaultValue("x64")]
    public string Arch { get; set; } = "x64";

    [CommandOption("-g|--graphics")]
    [Description("Include graphics support")]
    [DefaultValue(true)]
    public bool Graphics { get; set; } = true;
    */
}

public class NewCommand : AsyncCommand<NewSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, NewSettings settings)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [bold]Creating Cosmos Kernel Project[/]");
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.MarkupLine($"  Name: [blue]{settings.Name}[/]");
        // AnsiConsole.MarkupLine($"  Architecture: [blue]{settings.Arch}[/]");
        // AnsiConsole.MarkupLine($"  Graphics: [blue]{(settings.Graphics ? "Yes" : "No")}[/]");
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.WriteLine();

        string outputDir = settings.Output ?? Path.Combine(Directory.GetCurrentDirectory(), settings.Name);

        var args = new List<string>
        {
            "new", "cosmos-kernel-csharp",
            "-n", settings.Name,
            "-o", outputDir
            /*
            "--TargetArch", settings.Arch,
            "--EnableGraphics", settings.Graphics.ToString().ToLower()
            */
        };

        AnsiConsole.MarkupLine($"  [dim]Running: dotnet {string.Join(" ", args)}[/]");
        AnsiConsole.WriteLine();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                AnsiConsole.MarkupLine("  [red]Failed to start dotnet[/]");
                return 1;
            }

            string outputText = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(outputText))
            {
                foreach (string line in outputText.Split('\n'))
                {
                    AnsiConsole.WriteLine($"  {line}");
                }
            }

            if (process.ExitCode != 0)
            {
                AnsiConsole.MarkupLine($"  [red]Failed to create project: {Markup.Escape(error)}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("  If the template is not found, install it with:");
                AnsiConsole.MarkupLine("    [blue]dotnet new install Cosmos.Build.Templates[/]");
                return 1;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  [green]Project created successfully![/]");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("  Next steps:");
            AnsiConsole.MarkupLine($"    [blue]cd {settings.Name}[/]");
            AnsiConsole.MarkupLine("    [blue]cosmos build[/]");
            AnsiConsole.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"  [red]Error: {Markup.Escape(ex.Message)}[/]");
            return 1;
        }
    }
}
