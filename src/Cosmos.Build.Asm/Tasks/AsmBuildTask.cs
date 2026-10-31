// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Asm.Tasks;

public sealed class AsmBuildTask : ToolTask
{
    [Required] public string? ClangPath { get; set; }
    [Required] public string[]? SourceFiles { get; set; }
    [Required] public string? OutputPath { get; set; }
    public string? TargetArchitecture { get; set; }

    protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.Normal;

    protected override string GenerateFullPathToTool() =>
        ClangPath!;

    private string? FilePath { get; set; }
    private string? FileName { get; set; }

    protected override string GenerateCommandLineCommands()
    {
        Log.LogMessage(MessageImportance.Low, $"[Debug] Generating command-line args for {FilePath} -> {FileName}");

        // RISC-V also names the base ISA and the float ABI: lld refuses to link
        // objects whose ABIs differ, and ILC's objects use the double-float one.
        string target = TargetArchitecture switch
        {
            "arm64" => "--target=aarch64-none-elf",
            "riscv64" => "--target=riscv64-unknown-elf -march=rv64gc -mabi=lp64d",
            _ => "--target=x86_64-elf",
        };

        StringBuilder sb = new();
        sb.Append($" {target} -c ");
        sb.Append($" -o {Path.Combine(OutputPath!, FileName!)} ");
        sb.Append($" {FilePath} ");
        return sb.ToString();
    }

    public override bool Execute()
    {
        LogStandardErrorAsError = true;
        Log.LogMessage(MessageImportance.High, "Running Cosmos.Asm...");
        Log.LogMessage(MessageImportance.High, $"Tool Path: {ClangPath}");

        string paths = string.Join(",", SourceFiles!);
        Log.LogMessage(MessageImportance.High, $"Source Files: {paths}");
        Log.LogMessage(MessageImportance.Low, "[Debug] Beginning file matching");

        if (!Directory.Exists(OutputPath))
        {
            Log.LogMessage(MessageImportance.Low, $"[Debug] Creating output directory: {OutputPath}");
            Directory.CreateDirectory(OutputPath!);
        }

        using SHA1? hasher = SHA1.Create();
        var validOutputFiles = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string file in SourceFiles!)
        {
            FilePath = file;
            using FileStream stream = File.OpenRead(FilePath);
            byte[] fileHash = hasher.ComputeHash(stream);
            FileName = $"{Path.GetFileNameWithoutExtension(file)}-{BitConverter.ToString(fileHash).Replace("-", "").ToLower()}.obj";

            string outputFilePath = Path.GetFullPath(Path.Combine(OutputPath!, FileName));
            validOutputFiles.Add(outputFilePath);

            // Skip if output already exists (content-hash filename = up-to-date)
            if (File.Exists(outputFilePath))
            {
                Log.LogMessage(MessageImportance.Normal, $"Skipping {file} (up to date: {FileName})");
                continue;
            }

            Log.LogMessage(MessageImportance.High, $"[Debug] About to run base.Execute() for {FileName}");

            if (!base.Execute())
            {
                Log.LogError($"[Debug] AsmBuildTask failed for {FilePath}");
                return false;
            }
        }

        // Remove orphan object files (from renamed/deleted source files) to avoid stale symbols at link time
        foreach (string existing in Directory.GetFiles(OutputPath!, "*.obj"))
        {
            string normalizedExisting = Path.GetFullPath(existing);
            if (!validOutputFiles.Contains(normalizedExisting))
            {
                Log.LogMessage(MessageImportance.Normal, $"Removing orphan object: {Path.GetFileName(existing)}");
                File.Delete(existing);
            }
        }

        Log.LogMessage(MessageImportance.High, "AsmBuildTask completed successfully.");
        return true;
    }

    protected override string ToolName => "Cosmos.Asm";
}
