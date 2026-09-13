using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Diagnostics;
using DevKernel.Diagnostics;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Page-allocator statistics and garbage collector controls.
/// </summary>
internal static class MemoryCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Memory";

    /// <summary>Memory usage percentage below which usage is shown in green.</summary>
    private const ulong UsageWarnPercent = 50;

    /// <summary>Memory usage percentage below which usage is shown in yellow (red at or above).</summary>
    private const ulong UsageCriticalPercent = 80;

    /// <summary>Column width (chars) used to left-pad GC configuration variable names in the gcvar listing.</summary>
    private const int GcVarNameColumnWidth = 15;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "meminfo",
                Usage = "meminfo",
                Description = "Show memory allocator state",
                Execute = static (context, args) => ShowMemoryInfo(),
            },
            new ShellCommand
            {
                Name = "free",
                Usage = "free",
                Description = "Force a heap collection and report freed objects",
                Execute = static (context, args) => Terminal.Info(MemoryInfo.Collect() + " objects collected."),
            },
            new ShellCommand
            {
                Name = "gcstat",
                Usage = "gcstat",
                Description = "Give live information on the GC",
                Execute = static (context, args) => GcStat.Run(),
            },
            new ShellCommand
            {
                Name = "gcvar",
                Usage = "gcvar",
                Description = "List the GC configuration variables",
                Execute = static (context, args) =>
                {
                    foreach (KeyValuePair<string, object> variable in GC.GetConfigurationVariables())
                    {
                        Terminal.Info(variable.Key.PadLeft(GcVarNameColumnWidth) + ":" + variable.Value.ToString());
                    }
                },
            });
    }

    private static void ShowMemoryInfo()
    {
        Terminal.Header("Memory Information:");

        ulong totalPages = MemoryInfo.TotalPages;
        ulong freePages = MemoryInfo.FreePages;
        ulong usedPages = totalPages - freePages;
        ulong pageSize = MemoryInfo.PageSizeBytes;

        Terminal.InfoLine("Page Size", Units.ToKiB(pageSize).ToString() + " KB");
        Terminal.InfoLine("Total Pages", totalPages.ToString());
        Terminal.InfoLine("Used Pages", usedPages.ToString());
        Terminal.InfoLine("Free Pages", freePages.ToString());

        Console.WriteLine();

        Terminal.InfoLine("Total Memory", Units.ToMiB(totalPages * pageSize).ToString() + " MB");
        Terminal.InfoLine("Used Memory", Units.ToMiB(usedPages * pageSize).ToString() + " MB");
        Terminal.InfoLine("Free Memory", Units.ToMiB(freePages * pageSize).ToString() + " MB");

        ulong usagePercent = totalPages > 0 ? (usedPages * Units.PercentScale) / totalPages : 0;

        Console.WriteLine();
        Terminal.StatusLine("Usage", usagePercent.ToString() + "%", UsageColor(usagePercent));
    }

    private static ConsoleColor UsageColor(ulong usagePercent)
    {
        if (usagePercent < UsageWarnPercent)
        {
            return ConsoleColor.Green;
        }

        if (usagePercent < UsageCriticalPercent)
        {
            return ConsoleColor.Yellow;
        }

        return ConsoleColor.Red;
    }
}
