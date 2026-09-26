// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Drivers;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// The driver kit's view of the machine: every PCI function and USB
/// interface, and the driver that owns it.
/// </summary>
internal static class DriverCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Drivers";

    /// <summary>What the driver column shows for a device no driver owns.</summary>
    private const string NoDriver = "-";

    /// <summary>
    /// The owner the kit records for the boot display's PCI function: a
    /// reservation, not a driver, so the summary does not count it as bound.
    /// </summary>
    private const string BootDisplayReservation = "gop";

    /// <summary>Header of the path column, which also sets its narrowest width.</summary>
    private const string PathHeader = "PATH";

    /// <summary>Spaces between two columns.</summary>
    private const int ColumnGap = 2;

    /// <summary>Indent placed before every row.</summary>
    private const string RowIndent = "  ";

    /// <summary>Width of the <c>vvvv:dddd</c> ID column, and of its header.</summary>
    private const int IdColumnWidth = 9;

    /// <summary>Width of the <c>cc/ss/pp</c> class column, and of its header.</summary>
    private const int ClassColumnWidth = 8;

    /// <summary>Adds the driver commands to <paramref name="shell"/>, under the <c>Drivers</c> help section.</summary>
    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "lsdev",
                Usage = "lsdev",
                Description = "List PCI functions and USB interfaces with their drivers",
                Execute = static (context, args) => ListDevices(),
            });
    }

    private static void ListDevices()
    {
        IReadOnlyList<DeviceInfo> devices = DriverManager.Devices;
        if (devices.Count == 0)
        {
            // The pass has run by the time the shell does, so an empty list
            // means the kernel was built without PCI.
            Terminal.Error("No devices listed: the kernel is built without PCI.");
            return;
        }

        int pathWidth = PathHeader.Length;
        int bound = 0;
        bool hasBootDisplay = false;
        for (int i = 0; i < devices.Count; i++)
        {
            pathWidth = Math.Max(pathWidth, devices[i].Path.Length);
            string? driver = devices[i].DriverName;
            if (driver == BootDisplayReservation)
            {
                hasBootDisplay = true;
            }
            else if (driver is not null)
            {
                bound++;
            }
        }

        pathWidth += ColumnGap;
        Terminal.Header("Devices:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(RowIndent);
        Console.Write(PathHeader.PadRight(pathWidth));
        Console.Write("ID".PadRight(IdColumnWidth + ColumnGap));
        Console.Write("CLASS".PadRight(ClassColumnWidth + ColumnGap));
        Console.WriteLine("DRIVER");
        Console.ResetColor();

        for (int i = 0; i < devices.Count; i++)
        {
            PrintDevice(devices[i], pathWidth);
        }

        Console.WriteLine();
        Terminal.Muted($"{devices.Count} devices, {bound} with a driver.");
        if (hasBootDisplay)
        {
            Terminal.Muted($"'{BootDisplayReservation}' marks the boot display: reserved, so no registered driver is offered it.");
        }
    }

    /// <summary>
    /// Writes one row: path, IDs, class triple, and the owner: a driver in
    /// green, the boot display's reservation in yellow, nothing in dark gray.
    /// </summary>
    private static void PrintDevice(DeviceInfo device, int pathWidth)
    {
        Console.Write(RowIndent);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(device.Path.PadRight(pathWidth));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"{device.VendorId:x4}:{device.DeviceId:x4}".PadRight(IdColumnWidth + ColumnGap));
        Console.Write($"{device.Class:x2}/{device.Subclass:x2}/{device.Protocol:x2}".PadRight(ClassColumnWidth + ColumnGap));

        string? driver = device.DriverName;
        if (driver is null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(NoDriver);
        }
        else
        {
            Console.ForegroundColor = driver == BootDisplayReservation ? ConsoleColor.Yellow : ConsoleColor.Green;
            Console.WriteLine(driver);
        }

        Console.ResetColor();
    }
}
