using System.ComponentModel;
using Cosmos.Build.API.Enum;
using Cosmos.Patcher.Logging;
using Cosmos.Patcher.Patching;
using Cosmos.Patcher.Resolution;
using Mono.Cecil;
using Spectre.Console.Cli;

namespace Cosmos.Patcher;

public sealed class PatchCommand : Command<PatchCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--target <TARGET>")]
        [Description("Path to the target assembly.")]
        public required string TargetAssembly { get; set; }

        [CommandOption("--target-platform <TARGET-PLATFORM>")]
        [Description("Target platform for the patching process.")]
        public required string TargetPlatform { get; set; }

        [CommandOption("--plugs <PLUGS>")]
        [Description("Plug assemblies, separated by ';' or ','.")]
        public required string PlugsReferencesRaw { get; set; }


        [CommandOption("--output <OUTPUT>")]
        [Description("Output path for the patched dll")]
        public required string OutputPath { get; set; }

        [CommandOption("--search <DIRECTORIES>")]
        [Description("Directories the referenced assemblies are resolved from, separated by ';' or ','. The directories of the target, the plugs and the output are always searched.")]
        public string? SearchDirectoriesRaw { get; set; }

        [CommandOption("--coverage")]
        [Description("Enable plug-map generation for coverage tracking.")]
        [DefaultValue(false)]
        public bool Coverage { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        ConsoleBuildLogger logger = new();
        logger.Info("Running PatchCommand...");

        if (!File.Exists(settings.TargetAssembly))
        {
            logger.Error($"Error: Target assembly '{settings.TargetAssembly}' not found.");
            return -1;
        }

        char[] separators = [';', ',', Path.PathSeparator];
        string[] plugPaths = [.. (settings.PlugsReferencesRaw ?? string.Empty)
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()];

        if (plugPaths.Length == 0)
        {
            logger.Error("Error: No plug assemblies specified. Use --plugs \"a.dll;b.dll\"");
            return -1;
        }

        plugPaths = [.. plugPaths.Where(File.Exists)];
        if (plugPaths.Length == 0)
        {
            logger.Error("Error: No valid plug assemblies provided (files not found).");
            return -1;
        }

        try
        {
            string finalPath = settings.OutputPath ??
                               Path.Combine(
                                   Path.GetDirectoryName(settings.TargetAssembly)!,
                                   Path.GetFileNameWithoutExtension(settings.TargetAssembly) + "_patched.dll");

            // Writing the patched assembly back resolves the types a constant
            // is typed with, so every assembly the target references must be
            // findable: next to the target, next to the plugs, in the output
            // directory the build copies the references to, and wherever the
            // build says to look.
            PatcherAssemblyResolver resolver = new();
            resolver.AddAssemblyDirectory(settings.TargetAssembly);
            foreach (string plug in plugPaths)
            {
                resolver.AddAssemblyDirectory(plug);
            }

            resolver.AddAssemblyDirectory(finalPath);
            resolver.AddDirectories(settings.SearchDirectoriesRaw);

            logger.Info("Resolving referenced assemblies from:");
            foreach (string directory in resolver.Directories)
            {
                logger.Info($" - {directory}");
            }

            // Read with symbols to preserve debug info; without them when no
            // matching PDB sits next to the assembly.
            AssemblyDefinition targetAssembly = PatcherAssemblyLoader.ReadTarget(settings.TargetAssembly, resolver, out bool hasSymbols);
            logger.Info(hasSymbols
                ? $"Loaded target assembly with symbols: {settings.TargetAssembly}"
                : $"Loaded target assembly (no symbols): {settings.TargetAssembly}");

            AssemblyDefinition[] plugAssemblies = [.. plugPaths.Select(plug => PatcherAssemblyLoader.Read(plug, resolver))];
            PlatformArchitecture targetPlatform = Enum.Parse<PlatformArchitecture>(settings.TargetPlatform.ToUpperInvariant());

            logger.Info("Loaded plug assemblies:");
            foreach (string plug in plugPaths)
            {
                logger.Info($" - {plug}");
            }

            PlugPatcher plugPatcher = new(new PlugScanner(logger))
            {
                CoverageEnabled = settings.Coverage
            };

            plugPatcher.PatchAssembly(targetAssembly, targetPlatform, plugAssemblies);

            // Write plug map for coverage tracking (plug method → target method)
            // Only generated when --coverage is passed; uses assembly-specific filename to
            // avoid overwrites (MSBuild batches one assembly per invocation)
            if (settings.Coverage && plugPatcher.PlugMappings.Count > 0)
            {
                string assemblyName = Path.GetFileNameWithoutExtension(settings.TargetAssembly);
                string plugMapPath = Path.Combine(
                    Path.GetDirectoryName(finalPath) ?? ".",
                    $"plug-map-{assemblyName}.txt");
                WritePlugMap(plugMapPath, plugPatcher.PlugMappings);
                logger.Info($"Plug map written: {plugPatcher.PlugMappings.Count} mappings to {plugMapPath}");
            }

            // Write with symbols if we read them
            if (hasSymbols)
            {
                var writerParams = new Mono.Cecil.WriterParameters { WriteSymbols = true };
                targetAssembly.Write(finalPath, writerParams);
            }
            else
            {
                targetAssembly.Write(finalPath);
            }

            logger.Info($"Patched assembly saved to: {finalPath}");
            logger.Info("Patching completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            logger.Error($"Error during patching: {ex}");
            return -1;
        }
    }

    private static void WritePlugMap(string path, List<PlugMapping> mappings)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("# Plug Map - generated by cosmos.patcher patch");
        writer.WriteLine("# PlugAssembly\tPlugType\tPlugMethod\tTargetAssembly\tTargetType\tTargetMethod");
        foreach (PlugMapping m in mappings)
        {
            writer.WriteLine($"{m.PlugAssembly}\t{m.PlugType}\t{m.PlugMethod}\t{m.TargetAssembly}\t{m.TargetType}\t{m.TargetMethod}");
        }
    }
}
