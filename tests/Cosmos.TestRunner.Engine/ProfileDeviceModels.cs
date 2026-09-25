// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.Tools.Launcher;

namespace Cosmos.TestRunner.Engine;

/// <summary>
/// The device models a profile may list in its <c>"devices"</c> and
/// <c>"usb"</c> axes, and the checks that hold a profile to them. A model is
/// an allow-list entry rather than free text checked for syntax because a
/// typo such as <c>rtl8193</c> is well-formed: only the list catches it at
/// catalog load, on the host, instead of QEMU refusing the cell after a
/// kernel was built for it. The list is also what knows that a NIC model
/// needs a netdev behind it, so a profile never spells one.
/// </summary>
internal static class ProfileDeviceModels
{
    /// <summary>Name of the axis listing PCI functions, as spelled in <c>profiles.json</c>.</summary>
    internal const string DevicesAxis = "devices";

    /// <summary>Name of the axis listing USB devices, as spelled in <c>profiles.json</c>.</summary>
    internal const string UsbAxis = "usb";

    // A model is listed only once QEMU attaches it on both q35 and virt. The
    // loader has no per-architecture check, so a model that only one machine
    // presents would pass the load and then fail QEMU on the other; a cell
    // that must run on one architecture pins it with the profile's own
    // "architectures" filter instead.

    /// <summary>PCI functions attached as a bare <c>-device</c>.</summary>
    private static readonly string[] s_deviceModels =
    [
        // QEMU's educational device (1234:11e8): an MMIO BAR, MSI and a DMA
        // engine, with no built-in driver to claim it.
        "edu"
    ];

    /// <summary>NIC models, each attached with a user-mode netdev of its own.</summary>
    private static readonly string[] s_networkCardModels =
    [
        // Realtek RTL8139 (10ec:8139), which no built-in driver claims.
        "rtl8139",
        // Intel 82574L (8086:10d3). The E1000E built-in claims it on x64
        // only, so on arm64 it is a NIC no built-in driver takes.
        "e1000e"
    ];

    /// <summary>Every model the <c>"devices"</c> axis accepts.</summary>
    private static readonly string[] s_devicesAxisModels = [.. s_deviceModels, .. s_networkCardModels];

    /// <summary>USB models, placed on the xHCI root hub.</summary>
    private static readonly string[] s_usbModels = ["usb-mouse", "usb-kbd"];

    /// <summary>
    /// Checks a profile's <c>"devices"</c> list and resolves each entry to
    /// what the launcher attaches.
    /// </summary>
    /// <param name="path">Catalog path, named in the error.</param>
    /// <param name="profile">Profile name, named in the error.</param>
    /// <param name="entries">The list as parsed, or null when the profile has no <c>"devices"</c> key.</param>
    /// <param name="fixedAxisModels">Models the profile's fixed axes attach, each mapped to its axis name.</param>
    internal static IReadOnlyList<DeviceAttachment> ResolveDevices(
        string path,
        string profile,
        IReadOnlyList<string?>? entries,
        IReadOnlyDictionary<string, string> fixedAxisModels)
    {
        List<string> models = Validate(path, profile, DevicesAxis, entries, s_devicesAxisModels, UsbAxis, s_usbModels, fixedAxisModels);
        return models
            .Select(model => new DeviceAttachment { Model = model, IsNetworkCard = s_networkCardModels.Contains(model) })
            .ToList();
    }

    /// <summary>Checks a profile's <c>"usb"</c> list and returns its models in order.</summary>
    /// <param name="path">Catalog path, named in the error.</param>
    /// <param name="profile">Profile name, named in the error.</param>
    /// <param name="entries">The list as parsed, or null when the profile has no <c>"usb"</c> key.</param>
    /// <param name="fixedAxisModels">Models the profile's fixed axes attach, each mapped to its axis name.</param>
    internal static IReadOnlyList<string> ResolveUsbDevices(
        string path,
        string profile,
        IReadOnlyList<string?>? entries,
        IReadOnlyDictionary<string, string> fixedAxisModels) =>
        Validate(path, profile, UsbAxis, entries, s_usbModels, DevicesAxis, s_devicesAxisModels, fixedAxisModels);

    private static List<string> Validate(
        string path,
        string profile,
        string axis,
        IReadOnlyList<string?>? entries,
        string[] known,
        string otherAxis,
        string[] otherKnown,
        IReadOnlyDictionary<string, string> fixedAxisModels)
    {
        List<string> models = new();
        if (entries is null)
        {
            return models;
        }

        // An empty list attaches nothing, so it is a half-edited entry rather
        // than a way to say "none": leaving the key out already says that.
        if (entries.Count == 0)
        {
            throw new InvalidOperationException(
                $"{path}: profile '{profile}' has an empty '{axis}' list. List at least one model, or leave the key out.");
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (string? entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{profile}' has a blank entry in its '{axis}' list.");
            }

            // Ordinal: QEMU's model names are case-sensitive, so "EDU" would
            // pass a case-insensitive check and then fail QEMU.
            string model = entry.Trim();
            if (Array.IndexOf(known, model) < 0)
            {
                string elsewhere = Array.IndexOf(otherKnown, model) >= 0
                    ? $" '{model}' is listed under '{otherAxis}' instead."
                    : string.Empty;
                throw new InvalidOperationException(
                    $"{path}: profile '{profile}' lists unknown '{axis}' model '{model}'.{elsewhere} " +
                    $"Known '{axis}' models: {string.Join(", ", known)}. Disks, the NIC that replaces the default, " +
                    "input and display devices have axes of their own (disks, nic, keyboard, mouse, vga, gpu). " +
                    $"A new model is added to {nameof(ProfileDeviceModels)} once QEMU attaches it on both q35 and virt.");
            }

            // Every planned cell attaches a model once, so a repeat is a copy
            // slip, and it would still boot: the kernel would just see a
            // second function to bind, changing what the cell tests.
            if (!seen.Add(model))
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{profile}' lists '{model}' twice in its '{axis}' list. A profile attaches each model once.");
            }

            if (fixedAxisModels.TryGetValue(model, out string? fixedAxis))
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{profile}' lists '{model}' in its '{axis}' list, but its '{fixedAxis}' axis already attaches that model.");
            }

            models.Add(model);
        }

        return models;
    }
}
