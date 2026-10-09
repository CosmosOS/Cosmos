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

    [CommandOption("-l|--language")]
    [Description("Language of the kernel project (csharp, vb)")]
    [DefaultValue("csharp")]
    public string Language { get; set; } = "csharp";

    public override ValidationResult Validate()
    {
        return NewCommand.TemplateFor(Language) is null
            ? ValidationResult.Error($"Unknown language '{Language}': use csharp or vb.")
            : ValidationResult.Success();
    }

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
    /// <summary>
    /// The <c>dotnet new</c> template that creates a kernel in
    /// <paramref name="language"/>, or null for a language Cosmos has no
    /// template for.
    /// </summary>
    /// <param name="language"><c>csharp</c> or <c>vb</c>, in any case.</param>
    internal static string? TemplateFor(string language)
    {
        return language.ToLowerInvariant() switch
        {
            "csharp" => "cosmos-kernel-csharp",
            "vb" => "cosmos-kernel-vb",
            _ => null,
        };
    }

    public override async Task<int> ExecuteAsync(CommandContext context, NewSettings settings)
    {
        string template = TemplateFor(settings.Language)
            ?? throw new InvalidOperationException($"No template for language '{settings.Language}'");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [bold]Creating Cosmos Kernel Project[/]");
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.MarkupLine($"  Name: [blue]{settings.Name}[/]");
        AnsiConsole.MarkupLine($"  Language: [blue]{settings.Language.ToLowerInvariant()}[/]");
        // AnsiConsole.MarkupLine($"  Architecture: [blue]{settings.Arch}[/]");
        // AnsiConsole.MarkupLine($"  Graphics: [blue]{(settings.Graphics ? "Yes" : "No")}[/]");
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.WriteLine();

        string outputDir = settings.Output ?? Path.Combine(Directory.GetCurrentDirectory(), settings.Name);

        var args = new List<string>
        {
            "new", template,
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
