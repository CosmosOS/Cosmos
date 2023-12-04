using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static Cosmos.Build.Tasks.OperatingSystem;

namespace Cosmos.Build.Tasks
{
    public class Ld : ToolTask
    {
        #region Task Parameters

        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Required]
        public string OutputFile { get; set; }

        public string Entry { get; set; }

        public string TextAddress { get; set; }

        public string DataAddress { get; set; }

        public string BssAddress { get; set; }

        [Required]
        public string TargetArchitecture { get; set; }

        #endregion

        protected override string ToolName => IsWindows() ? "ld.exe" : "ld";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        private static bool IsValidAddress(string aAddress)
        {
            if (ulong.TryParse(aAddress, out var xAddress))
            {
                return true;
            }

            if (aAddress.StartsWith("0x")
                && ulong.TryParse(aAddress.Remove(0, 2), NumberStyles.AllowHexSpecifier, null, out xAddress))
            {
                return true;
            }

            return false;
        }

        protected override bool ValidateParameters()
        {
            if (InputFiles.Length == 0)
            {
                Log.LogError("No input files specified!");
            }


            foreach (var xFile in InputFiles)
            {
                var xFullPath = xFile.GetMetadata("FullPath");

                if (string.IsNullOrWhiteSpace(xFullPath))
                {
                    Log.LogError($"Input file is an empty string! Input files: '{string.Join(";", InputFiles.Select(f => f.GetMetadata("Identity")))}'");
                }
                else if (!File.Exists(xFullPath))
                {
                    Log.LogError($"Input file '{xFullPath}' doesn't exist!");
                }
            }

            if (string.IsNullOrEmpty(OutputFile))
            {
                Log.LogError("No output file specified!");
            }

            if (string.IsNullOrWhiteSpace(Entry))
            {
                Entry = null;
            }

            if (string.IsNullOrWhiteSpace(TextAddress))
            {
                TextAddress = null;
            }
            else if (!IsValidAddress(TextAddress))
            {
                Log.LogError(nameof(TextAddress) + " isn't a valid 64-bit number!");
            }

            if (string.IsNullOrWhiteSpace(DataAddress))
            {
                DataAddress = null;
            }
            else if (!IsValidAddress(DataAddress))
            {
                Log.LogError(nameof(DataAddress) + " isn't a valid 64-bit number!");
            }

            if (string.IsNullOrWhiteSpace(BssAddress))
            {
                BssAddress = null;
            }
            else if (!IsValidAddress(BssAddress))
            {
                Log.LogError(nameof(BssAddress) + " isn't a valid 64-bit number!");
            }

            return !Log.HasLoggedErrors;
        }

        protected override string GenerateFullPathToTool()
        {
            if (string.IsNullOrWhiteSpace(ToolExe))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(ToolPath))
            {
                return Path.Combine(Directory.GetCurrentDirectory(), ToolExe);
            }

            return Path.Combine(Path.GetFullPath(ToolPath), ToolExe);
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder xBuilder = new();

            xBuilder.AppendSwitchIfNotNull("-o ", OutputFile);

            xBuilder.AppendFileNamesIfNotNull(InputFiles, " ");

            if (TargetArchitecture == "amd64")
            {
                var dir = Path.GetDirectoryName(OutputFile);
                var path = dir + "/linker.ld";
                xBuilder.AppendSwitch("-m elf_x86_64");
                xBuilder.AppendSwitch("-z text");
                xBuilder.AppendSwitchIfNotNull("-T ", path);



                File.WriteAllText(path, @"/* Tell the linker that we want an x86_64 ELF64 output file */
OUTPUT_FORMAT(elf64-x86-64)
OUTPUT_ARCH(i386:x86-64)
 
/* We want the symbol _start to be our entry point */
ENTRY(" + Entry + @")
 
/* Define the program headers we want so the bootloader gives us the right */
/* MMU permissions */
PHDRS
{
    text    PT_LOAD    FLAGS((1 << 0) | (1 << 2)) ; /* Execute + Read */
    rodata  PT_LOAD    FLAGS((1 << 2)) ;            /* Read only */
    data    PT_LOAD    FLAGS((1 << 1) | (1 << 2)) ; /* Write + Read */
    dynamic PT_DYNAMIC FLAGS((1 << 1) | (1 << 2)) ; /* Dynamic PHDR for relocations */
}
 
SECTIONS
{
    /* We wanna be placed in the topmost 2GiB of the address space, for optimisations */
    /* and because that is what the Limine spec mandates. */
    /* Any address in this region will do, but often 0xffffffff80000000 is chosen as */
    /* that is the beginning of the region. */
    . = 0xffffffff80000000;
 
    .text : {
        *(.text .text.*)
    } :text
 
    /* Move to the next memory page for .rodata */
    . += CONSTANT(MAXPAGESIZE);
 
    .rodata : {
        *(.rodata .rodata.*)
    } :rodata
 
    /* Move to the next memory page for .data */
    . += CONSTANT(MAXPAGESIZE);
 
    .data : {
        *(.data .data.*)
    } :data
 
    /* Dynamic section for relocations, both in its own PHDR and inside data PHDR */
    .dynamic : {
        *(.dynamic)
    } :data :dynamic
 
    /* NOTE: .bss needs to be the last thing mapped to :data, otherwise lots of */
    /* unnecessary zeros will be written to the binary. */
    /* If you need, for example, .init_array and .fini_array, those should be placed */
    /* above this. */
    .bss : {
        *(.bss .bss.*)
        *(COMMON)
    } :data
 
    /* Discard .note.* and .eh_frame since they may cause issues on some hosts. */
    /DISCARD/ : {
        *(.eh_frame)
        *(.note .note.*)
    }
}");
            }
            else
            {
                xBuilder.AppendSwitch("-m elf_i386");

                xBuilder.AppendSwitchIfNotNull("-Ttext ", TextAddress);
                xBuilder.AppendSwitchIfNotNull("-Tdata ", DataAddress);
                xBuilder.AppendSwitchIfNotNull("-Tbss ", BssAddress);
                xBuilder.AppendSwitchIfNotNull("-e ", Entry);
            }

            Log.LogMessage(MessageImportance.High, xBuilder.ToString());

            return xBuilder.ToString();
        }

        public override bool Execute()
        {
            Stopwatch xSW = Stopwatch.StartNew();
            try
            {
                return base.Execute();
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "LD task took {0}", xSW.Elapsed);
            }
        }

    }
}
