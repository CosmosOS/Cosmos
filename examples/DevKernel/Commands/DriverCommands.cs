// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;
using Cosmos.Kernel.System.Diagnostics;
using DevKernel.Drivers;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// Driver kit introspection: the engine, the manifest, the device tree with
/// every offer made for each node, and the published devices, all read
/// through <see cref="DriverInfo"/>; and the synthetic device that lets
/// <see cref="SampleDriver"/> bind and detach on demand.
/// </summary>
internal static class DriverCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Drivers";

    /// <summary>Indent placed before every entry of a listing.</summary>
    private const string EntryIndent = "    ";

    /// <summary>Indent placed before the lines that belong to an entry.</summary>
    private const string DetailIndent = "      ";

    /// <summary>The synthetic node published for <see cref="SampleDriver"/>, null while none is.</summary>
    private static DeviceNode? s_sampleNode;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "drivers",
                Usage = "drivers",
                Description = "Show the driver engine, manifest, device tree and published devices",
                Execute = static (context, args) => ShowDriverInfo(),
            },
            new ShellCommand
            {
                Name = "sampledev",
                Usage = "sampledev <publish|retract>",
                Description = "Publish or retract the synthetic device SampleDriver binds to",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => SampleDevice(args[0]),
            });
    }

    /// <summary>
    /// Publishes the synthetic device under <see cref="SampleDriver.Key"/>, or
    /// takes it away again. After the driver stage, the bus returns once the
    /// engine has offered or torn down the node, so <c>drivers</c> shows the
    /// result at once.
    /// </summary>
    private static void SampleDevice(string action)
    {
        switch (action)
        {
            case "publish":
                if (s_sampleNode is not null)
                {
                    Terminal.Warning("sample device is already published as " + s_sampleNode.Path);
                    return;
                }

                s_sampleNode = SyntheticBus.Publish(SampleDriver.Key, []);
                Terminal.Success("published " + s_sampleNode.Path + ", now " + StateName(s_sampleNode.State));
                break;

            case "retract":
                if (s_sampleNode is null)
                {
                    Terminal.Warning("no sample device is published");
                    return;
                }

                SyntheticBus.Retract(s_sampleNode);
                Terminal.Success("retracted " + s_sampleNode.Path);
                s_sampleNode = null;
                break;

            default:
                Terminal.Error("usage: sampledev <publish|retract>");
                break;
        }
    }

    /// <summary>The name of a kit node state, spelled out: formatting an enum needs reflection.</summary>
    private static string StateName(NodeState state) => state switch
    {
        NodeState.Pending => "pending",
        NodeState.Bound => "bound",
        NodeState.Unbound => "unbound",
        NodeState.Retracted => "retracted",
        _ => "unknown",
    };

    private static void ShowDriverInfo()
    {
        Terminal.Header("Driver Information:");

        Terminal.StatusLine(
            "Engine",
            DriverInfo.IsStarted ? "STARTED" : "NOT STARTED",
            DriverInfo.IsStarted ? ConsoleColor.Green : ConsoleColor.Red);
        Terminal.InfoLine("Worker", DriverInfo.HasWorker ? "kit worker thread" : "inline, no worker");
        Terminal.InfoLine("Drivers", DriverInfo.DriverCount.ToString());
        Terminal.InfoLine("Nodes", DriverInfo.NodeCount.ToString());
        Terminal.InfoLine("Devices", DriverInfo.DeviceCount.ToString());
        Terminal.InfoLine("Held", DriverInfo.GetTotalHeldResourceCount() + " resources");
        Console.WriteLine();

        PrintManifest();
        PrintNodes();
        PrintDevices();
    }

    private static void PrintManifest()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Manifest:");
        Console.ResetColor();

        int count = DriverInfo.DriverCount;
        if (count == 0)
        {
            Terminal.Muted(EntryIndent + "no drivers registered");
        }

        for (int i = 0; i < count; i++)
        {
            if (!DriverInfo.TryGetDriver(i, out DriverEntryInfo driver))
            {
                continue;
            }

            Console.Write(EntryIndent);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[" + i + "] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(driver.Name);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" Pri=" + driver.Priority);
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    private static void PrintNodes()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Device tree:");
        Console.ResetColor();

        int count = DriverInfo.NodeCount;
        if (count == 0)
        {
            Terminal.Muted(EntryIndent + "no nodes published");
        }

        for (int i = 0; i < count; i++)
        {
            if (DriverInfo.TryGetNode(i, out DeviceNodeInfo node))
            {
                PrintNode(i, node);
            }
        }

        Console.WriteLine();
    }

    private static void PrintNode(int index, DeviceNodeInfo node)
    {
        Console.Write(EntryIndent);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(node.Path);
        Console.Write(" ");
        WriteState(node.State);

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" " + (node.DriverName ?? "no driver"));

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" Held=" + node.HeldResourceCount);
        Console.Write(" Published=" + node.PublishedDeviceCount);
        if (node.LeakedResourceCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" Leaked=" + node.LeakedResourceCount);
        }

        if (node.FaultCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" Faults=" + node.FaultCount);
        }

        Console.ResetColor();
        Console.WriteLine();

        Terminal.Muted(
            DetailIndent + node.Description
            + " on " + node.BusName
            + (node.ParentPath is null ? string.Empty : ", under " + node.ParentPath)
            + ", " + node.ResourceCount + " resources, " + node.InterruptCount + " interrupts, " + node.ChildCount + " children");

        if (node.LastFault is not null)
        {
            Terminal.Muted(DetailIndent + "last fault: " + node.LastFault);
        }

        if (node.OfferCount == 0)
        {
            Terminal.Muted(DetailIndent + node.State switch
            {
                DeviceNodeState.Pending => "not offered yet",
                DeviceNodeState.Retracted => "retracted before any offer",
                _ => "no driver matched",
            });
        }

        for (int offerIndex = 0; offerIndex < node.OfferCount; offerIndex++)
        {
            if (DriverInfo.TryGetOffer(index, offerIndex, out DeviceOfferInfo offer))
            {
                PrintOffer(offerIndex, offer);
            }
        }
    }

    private static void PrintOffer(int offerIndex, DeviceOfferInfo offer)
    {
        Console.Write(DetailIndent);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("offer " + offerIndex + ": ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(offer.DriverName);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" Pri=" + offer.Priority + " Spec=" + offer.Specificity + " ");
        WriteOutcome(offer.Outcome);

        if (offer.Reason is not null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" (" + offer.Reason + ")");
        }

        if (offer.ReleasedResourceCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" released " + offer.ReleasedResourceCount);
        }

        Console.ResetColor();
        Console.WriteLine();
    }

    private static void PrintDevices()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Published devices:");
        Console.ResetColor();

        int count = DriverInfo.DeviceCount;
        if (count == 0)
        {
            Terminal.Muted(EntryIndent + "no devices published");
        }

        for (int i = 0; i < count; i++)
        {
            if (!DriverInfo.TryGetDevice(i, out PublishedDeviceInfo device))
            {
                continue;
            }

            Console.Write(EntryIndent);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(device.Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" " + KindName(device.Kind));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" from " + (device.NodePath ?? "firmware"));
            Console.Write(" by " + (device.DriverName ?? "no driver"));

            if (device.IsWithdrawn)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" withdrawn");
            }
            else
            {
                Console.ForegroundColor = device.IsConsumed ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.Write(device.IsConsumed ? " consumed" : " not consumed");
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    private static void WriteState(DeviceNodeState state)
    {
        switch (state)
        {
            case DeviceNodeState.Bound:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Bound");
                break;
            case DeviceNodeState.Pending:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Pending");
                break;
            case DeviceNodeState.Unbound:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Unbound");
                break;
            case DeviceNodeState.Retracted:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Retracted");
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Unknown");
                break;
        }
    }

    private static void WriteOutcome(DeviceOfferOutcome outcome)
    {
        switch (outcome)
        {
            case DeviceOfferOutcome.Bound:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("bound");
                break;
            case DeviceOfferOutcome.Declined:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("declined");
                break;
            case DeviceOfferOutcome.Failed:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("failed");
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("unknown");
                break;
        }
    }

    private static string KindName(PublishedDeviceKind kind) => kind switch
    {
        PublishedDeviceKind.Keyboard => "keyboard",
        PublishedDeviceKind.Pointer => "pointer",
        PublishedDeviceKind.Network => "network",
        PublishedDeviceKind.Block => "block",
        PublishedDeviceKind.Display => "display",
        _ => "unknown",
    };
}
