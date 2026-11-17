using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Cosmos.Tools.Launcher;

namespace Cosmos.TestRunner.Engine;

/// <summary>
/// One QEMU launch configuration, possibly composed from a base hardware
/// profile and a single device/machine modifier. Profiles and modifiers are
/// declared globally in <c>tests/profiles.json</c>; a suite opts in via
/// <c>&lt;CosmosTestProfile/&gt;</c> and <c>&lt;CosmosTestModifier/&gt;</c>
/// items in its csproj. The engine cross-products the two and emits one
/// <see cref="TestProfile"/> per matrix cell.
/// </summary>
public sealed record TestProfile
{
    /// <summary>Display label used to prefix test names: base profile name, or "profile+modifier".</summary>
    public required string Name { get; init; }

    /// <summary>Disks to attach for this profile, with any modifier options already merged in.</summary>
    public IReadOnlyList<TestProfileDisk> Disks { get; init; } = Array.Empty<TestProfileDisk>();

    /// <summary>
    /// NIC model to attach (e.g. <c>e1000e</c>, <c>virtio-net-pci</c>), or null
    /// to leave the architecture default in place. Lets a suite run its whole
    /// test list against more than one driver.
    /// </summary>
    public string? NetworkCard { get; init; }

    /// <summary>Keyboard model to attach (e.g. <c>virtio-keyboard-pci</c>), or null for none.</summary>
    public string? KeyboardDevice { get; init; }

    /// <summary>Mouse model to attach (e.g. <c>virtio-mouse-pci</c>), or null for none.</summary>
    public string? MouseDevice { get; init; }

    /// <summary>
    /// The host controller the USB disks, the USB keyboard and the USB mouse
    /// sit on (<c>usbController</c>): the xHCI unless the profile names
    /// <c>ehci</c>, <c>ohci</c> or <c>ehci-ohci</c>, an ICH9 EHCI with an
    /// OHCI companion that takes the full-speed devices.
    /// </summary>
    public UsbHostControllerModel UsbController { get; init; } = UsbHostControllerModel.Xhci;

    /// <summary>
    /// The USB version the usb-kbd and usb-mouse present (<c>usbDeviceVersion</c>):
    /// 2, or 1 for a device with the full-speed descriptor set alone, which
    /// an EHCI hands to its companion.
    /// </summary>
    public int UsbDeviceVersion { get; init; } = QemuLauncher.DefaultUsbDeviceVersion;

    /// <summary>Whether a usb-hub sits on root port 1 with the keyboard behind it, on its port 1 (<c>usbHub</c>); the hub is full speed only and needs an OHCI.</summary>
    public bool UsbHub { get; init; }

    /// <summary>
    /// VGA adapter to swap in for the architecture default, as a QEMU
    /// <c>-vga</c> backend name (e.g. <c>vmware</c>), or null to leave the
    /// default in place. Replaces rather than adds: a second display adapter
    /// would leave firmware free to pick either one as primary.
    /// </summary>
    public string? VgaAdapter { get; init; }

    /// <summary>
    /// Display adapter to attach as a <c>-device</c> line (e.g.
    /// <c>virtio-gpu-pci</c>), or null for none. Additive, unlike
    /// <see cref="VgaAdapter"/>: the machine's default adapter stays and keeps
    /// providing the framebuffer the bootloader hands the kernel, so this cell
    /// differs from the bare one only by the extra device on the PCI bus.
    /// </summary>
    public string? GpuDevice { get; init; }

    /// <summary>
    /// HD Audio controller to attach (e.g. <c>intel-hda</c>), with the codec
    /// the launcher adds beside it, or null for none. The engine plays it
    /// into QEMU's <c>none</c> backend: a CI runner has no audio server.
    /// </summary>
    public string? AudioDevice { get; init; }

    /// <summary>
    /// Architectures this profile applies to; null means any. Mirrors the
    /// modifier filter, for hardware that only one architecture can present
    /// (PS/2 on x64, virtio-mmio on the ARM64 virt machine).
    /// </summary>
    public IReadOnlyList<string>? Architectures { get; init; }

    public bool AppliesTo(string architecture) =>
        Architectures == null || Architectures.Contains(architecture, StringComparer.OrdinalIgnoreCase);

    /// <summary>Extra <c>-M</c> machine properties (e.g. <c>{"gic-version", "2"}</c>). Merged from base profile + applied modifier.</summary>
    public IReadOnlyDictionary<string, string> MachineOptions { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// CPUs the cell's machine gets (<c>-smp</c>): 1 unless a modifier sets it (<c>smp4</c>),
    /// since every suite is written against one CPU and the ones that prove the application
    /// processors come up opt in.
    /// </summary>
    public int CpuCount { get; init; } = 1;

    /// <summary>True when this is the synthetic single-profile fallback used by suites that opt into nothing.</summary>
    public bool IsDefault { get; init; }

    public static TestProfile Default => new() { Name = string.Empty, IsDefault = true };
}

/// <summary>
/// One disk entry: backend type and a dictionary of QEMU properties spliced
/// onto the <c>-device</c> line.
/// </summary>
public sealed record TestProfileDisk
{
    public required DiskKind Kind { get; init; }
    public IReadOnlyDictionary<string, string> Options { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// The disk sits behind a PCI Express root port of its own and may be
    /// pulled and replugged by the pci-unplug and pci-plug requests;
    /// virtio-blk only.
    /// </summary>
    public bool HotPlug { get; init; }

    public string FormatOptions()
    {
        if (Options.Count == 0)
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        bool first = true;
        foreach (KeyValuePair<string, string> kv in Options)
        {
            if (!first)
            {
                sb.Append(',');
            }
            sb.Append(kv.Key);
            sb.Append('=');
            sb.Append(kv.Value);
            first = false;
        }
        return sb.ToString();
    }
}

/// <summary>
/// A named overlay of QEMU options that compose with a profile to produce a
/// variant. Two orthogonal axes:
///   * <see cref="DeviceOptions"/>: per-disk-kind overrides (e.g. NVMe MSI-X).
///   * <see cref="MachineOptions"/>: <c>-M</c> properties (e.g. GIC version).
/// <see cref="Architectures"/> gates the modifier to one or more arches; null
/// means any. A modifier is silently skipped for a profile when its arch
/// doesn't match or when it has only device options none of which target a
/// device kind in the profile.
/// </summary>
internal sealed record TestModifier
{
    public required string Name { get; init; }
    public IReadOnlyList<string>? Architectures { get; init; }
    public IReadOnlyDictionary<DiskKind, IReadOnlyDictionary<string, string>> DeviceOptions { get; init; }
        = new Dictionary<DiskKind, IReadOnlyDictionary<string, string>>();
    public IReadOnlyDictionary<string, string> MachineOptions { get; init; } = new Dictionary<string, string>();

    /// <summary>The CPU count (<c>-smp</c>) the modifier sets, or null to leave the profile's.</summary>
    public int? CpuCount { get; init; }

    public bool AppliesTo(TestProfile profile, string architecture)
    {
        if (Architectures is not null && !Architectures.Contains(architecture, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        // Machine-only modifier: applies regardless of disks.
        if (DeviceOptions.Count == 0)
        {
            return MachineOptions.Count > 0 || CpuCount is not null;
        }

        foreach (TestProfileDisk disk in profile.Disks)
        {
            if (DeviceOptions.ContainsKey(disk.Kind))
            {
                return true;
            }
        }
        return false;
    }

    public TestProfile ApplyTo(TestProfile baseProfile)
    {
        var newDisks = new List<TestProfileDisk>(baseProfile.Disks.Count);
        foreach (TestProfileDisk disk in baseProfile.Disks)
        {
            if (!DeviceOptions.TryGetValue(disk.Kind, out IReadOnlyDictionary<string, string>? extras))
            {
                newDisks.Add(disk);
                continue;
            }

            var merged = new Dictionary<string, string>(disk.Options);
            foreach (KeyValuePair<string, string> kv in extras)
            {
                merged[kv.Key] = kv.Value;
            }
            newDisks.Add(disk with { Options = merged });
        }

        var mergedMachine = new Dictionary<string, string>(baseProfile.MachineOptions);
        foreach (KeyValuePair<string, string> kv in MachineOptions)
        {
            mergedMachine[kv.Key] = kv.Value;
        }

        // `with` rather than a fresh record: everything a modifier does not
        // touch (NIC, input devices, VGA adapter, arch filter) has to survive
        // the overlay.
        return baseProfile with
        {
            Name = $"{baseProfile.Name}+{Name}",
            Disks = newDisks,
            MachineOptions = mergedMachine,
            CpuCount = CpuCount ?? baseProfile.CpuCount
        };
    }
}

/// <summary>
/// Resolves the per-suite profile list by:
/// 1. Loading the global catalog (profiles + modifiers) from the nearest
///    <c>profiles.json</c> walking upward from the suite directory.
/// 2. Reading <c>CosmosTestProfile</c> + <c>CosmosTestModifier</c> items from
///    the suite's csproj.
/// 3. Mixing them: for each requested profile, emit every conflict-free
///    COMBINATION of its applicable modifiers (gated on architecture and on a
///    device-kind match for device-targeting modifiers), so configs compose
///    (e.g. nvme+gicv2+acpi-off). Combinations whose modifiers write the same
///    option key (e.g. gicv2 + gicv3) are dropped as contradictory.
/// A suite that opts into nothing falls back to a single anonymous profile.
/// </summary>
public static class TestProfileLoader
{
    private const string CatalogFileName = "profiles.json";

    public static IReadOnlyList<TestProfile> LoadFor(string kernelProjectPath, string architecture)
    {
        SuiteOptIn optIn = ReadSuiteOptIn(kernelProjectPath);
        if (optIn.Profiles.Count == 0 && optIn.Modifiers.Count == 0)
        {
            return new[] { TestProfile.Default };
        }

        if (optIn.Profiles.Count == 0)
        {
            throw new InvalidOperationException(
                $"Suite at '{kernelProjectPath}' declares CosmosTestModifier items but no CosmosTestProfile. " +
                "A modifier needs a base profile to compose with.");
        }

        string catalogPath = FindCatalog(kernelProjectPath)
            ?? throw new InvalidOperationException(
                $"Suite at '{kernelProjectPath}' declares profile/modifier items but no '{CatalogFileName}' was found in any ancestor directory.");

        Catalog catalog = LoadCatalog(catalogPath);

        var resolvedProfiles = new List<TestProfile>(optIn.Profiles.Count);
        foreach (string name in optIn.Profiles)
        {
            if (!catalog.Profiles.TryGetValue(name, out TestProfile? profile))
            {
                throw new InvalidOperationException(
                    $"Suite at '{kernelProjectPath}' requested profile '{name}' which is not defined in '{catalogPath}'. " +
                    $"Known profiles: {string.Join(", ", catalog.Profiles.Keys)}.");
            }

            // A profile pinned to another architecture drops out silently:
            // that is how one suite declares per-arch hardware (virtio-mmio is
            // arm64-only) in a single csproj.
            if (!profile.AppliesTo(architecture))
            {
                continue;
            }

            // Now that the architecture is fixed, collapse the profile's
            // per-arch machine options down to the ones this cell gets.
            if (catalog.ProfileMachineOptions.TryGetValue(name, out IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? perArch)
                && perArch.TryGetValue(architecture, out IReadOnlyDictionary<string, string>? forThisArch))
            {
                resolvedProfiles.Add(profile with { MachineOptions = forThisArch });
            }
            else
            {
                resolvedProfiles.Add(profile);
            }
        }

        if (resolvedProfiles.Count == 0)
        {
            // Running zero cells would report a green suite that tested
            // nothing, so a suite whose every profile is pinned elsewhere is
            // a configuration error rather than a silent skip.
            throw new InvalidOperationException(
                $"Suite at '{kernelProjectPath}' requested profiles [{string.Join(", ", optIn.Profiles)}], " +
                $"none of which apply to architecture '{architecture}'. Add a profile that covers it.");
        }

        var resolvedModifiers = new List<TestModifier>(optIn.Modifiers.Count);
        foreach (string name in optIn.Modifiers)
        {
            if (!catalog.Modifiers.TryGetValue(name, out TestModifier? mod))
            {
                throw new InvalidOperationException(
                    $"Suite at '{kernelProjectPath}' requested modifier '{name}' which is not defined in '{catalogPath}'. " +
                    $"Known modifiers: {(catalog.Modifiers.Count == 0 ? "(none)" : string.Join(", ", catalog.Modifiers.Keys))}.");
            }
            resolvedModifiers.Add(mod);
        }

        // Build the cell list: for each profile, every conflict-free COMBINATION
        // of its applicable modifiers (the matrix mix), so configs compose:
        // e.g. nvme+gicv2+acpi-off, not just nvme+gicv2 and nvme+acpi-off as
        // separate cells. A combination is dropped when two of its modifiers
        // write the same -M / device-option key (e.g. gicv2 + gicv3 both set
        // gic-version), which would be a contradictory machine.
        //
        // Cell count is 2^(applicable modifiers) per profile minus conflicting
        // subsets, so each added modifier roughly doubles a profile's cells:
        // keep the catalog lean.
        var matrix = new List<TestProfile>();
        foreach (TestProfile profile in resolvedProfiles)
        {
            var applicable = new List<TestModifier>();
            foreach (TestModifier mod in resolvedModifiers)
            {
                if (mod.AppliesTo(profile, architecture))
                {
                    applicable.Add(mod);
                }
            }

            // Enumerate every subset via a bitmask; bit i selects applicable[i].
            // mask 0 is the bare profile (no modifiers).
            for (int mask = 0; mask < (1 << applicable.Count); mask++)
            {
                var combo = new List<TestModifier>();
                for (int i = 0; i < applicable.Count; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        combo.Add(applicable[i]);
                    }
                }

                if (HasOptionConflict(combo))
                {
                    continue;
                }

                TestProfile cell = profile;
                foreach (TestModifier mod in combo)
                {
                    cell = mod.ApplyTo(cell);
                }
                matrix.Add(cell);
            }
        }
        return matrix;
    }

    /// <summary>
    /// True when two modifiers in <paramref name="combo"/> write the same
    /// machine (<c>-M</c>) property, the same per-disk device option or both
    /// set the CPU count: a contradictory cell (e.g. gicv2 and gicv3 both set
    /// gic-version). Such combinations are dropped from the matrix.
    /// </summary>
    private static bool HasOptionConflict(List<TestModifier> combo)
    {
        var machineKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var deviceKeys = new HashSet<string>(StringComparer.Ordinal);
        bool cpuCountSet = false;
        foreach (TestModifier mod in combo)
        {
            if (mod.CpuCount is not null)
            {
                if (cpuCountSet)
                {
                    return true;
                }
                cpuCountSet = true;
            }
            foreach (string key in mod.MachineOptions.Keys)
            {
                if (!machineKeys.Add(key))
                {
                    return true;
                }
            }
            foreach (KeyValuePair<DiskKind, IReadOnlyDictionary<string, string>> perKind in mod.DeviceOptions)
            {
                foreach (string key in perKind.Value.Keys)
                {
                    if (!deviceKeys.Add($"{perKind.Key}:{key}"))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private readonly record struct SuiteOptIn(IReadOnlyList<string> Profiles, IReadOnlyList<string> Modifiers);

    private static SuiteOptIn ReadSuiteOptIn(string kernelProjectPath)
    {
        string? csproj = Directory.EnumerateFiles(kernelProjectPath, "*.csproj").FirstOrDefault();
        if (csproj == null)
        {
            return new SuiteOptIn(Array.Empty<string>(), Array.Empty<string>());
        }

        XDocument doc;
        try
        {
            doc = XDocument.Load(csproj);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse '{csproj}': {ex.Message}", ex);
        }

        // MSBuild csprojs may or may not declare a default xmlns. LocalName
        // matching ignores that wrinkle.
        return new SuiteOptIn(
            CollectIncludes(doc, "CosmosTestProfile"),
            CollectIncludes(doc, "CosmosTestModifier"));
    }

    private static IReadOnlyList<string> CollectIncludes(XDocument doc, string elementName)
    {
        var result = new List<string>();
        foreach (XElement el in doc.Descendants().Where(e => e.Name.LocalName == elementName))
        {
            string? include = el.Attribute("Include")?.Value;
            if (!string.IsNullOrWhiteSpace(include))
            {
                result.Add(include.Trim());
            }
        }
        return result;
    }

    private static string? FindCatalog(string startDir)
    {
        DirectoryInfo? dir = new DirectoryInfo(Path.GetFullPath(startDir));
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, CatalogFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            dir = dir.Parent;
        }
        return null;
    }

    private sealed record Catalog(
        IReadOnlyDictionary<string, TestProfile> Profiles,
        IReadOnlyDictionary<string, TestModifier> Modifiers,
        // Profile name → architecture → -M options that only apply there.
        // Kept out of TestProfile because a TestProfile is a *resolved* cell:
        // by the time one exists the architecture is known and its machine
        // options are already final.
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> ProfileMachineOptions);

    /// <summary>Architectures a profile may scope machine options to.</summary>
    private static readonly string[] KnownArchitectures = ["x64", "arm64"];

    private static Catalog LoadCatalog(string path)
    {
        string json = File.ReadAllText(path);

        ProfilesFile? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ProfilesFile>(json, JsonOpts);
        }
        catch (JsonException ex)
        {
            // Raw JsonExceptions name neither the file nor the likely cause.
            // The common one is writing a profile's machineOptions flat
            // ({"gic-version": "3"}) instead of keyed by architecture, which
            // fails here because the value is a string where an object is
            // expected.
            throw new InvalidOperationException(
                $"{path}: could not be parsed. Note that a profile's 'machineOptions' is keyed by architecture " +
                $"({string.Join(", ", KnownArchitectures)}), e.g. \"machineOptions\": {{ \"arm64\": {{ \"gic-version\": \"3\" }} }}. " +
                $"Parser error: {ex.Message}", ex);
        }
        if (parsed?.Profiles is null || parsed.Profiles.Count == 0)
        {
            throw new InvalidOperationException($"{path}: 'profiles' array is missing or empty.");
        }

        var profiles = new Dictionary<string, TestProfile>(parsed.Profiles.Count, StringComparer.Ordinal);
        var profileMachineOptions =
            new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>(StringComparer.Ordinal);
        foreach (ProfileEntry entry in parsed.Profiles)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                throw new InvalidOperationException($"{path}: every profile needs a non-empty 'name'.");
            }
            if (profiles.ContainsKey(entry.Name))
            {
                throw new InvalidOperationException($"{path}: duplicate profile name '{entry.Name}'.");
            }

            var disks = new List<TestProfileDisk>();
            if (entry.Disks is not null)
            {
                foreach (DiskEntry disk in entry.Disks)
                {
                    DiskKind kind = ParseDiskKind(path, $"profile '{entry.Name}'", disk.Type);
                    bool hotPlug = disk.HotPlug ?? false;
                    if (hotPlug && kind != DiskKind.VirtioBlk)
                    {
                        throw new InvalidOperationException(
                            $"{path}: profile '{entry.Name}' sets hotplug on a '{disk.Type}' disk; only a 'virtio-blk' disk can sit behind a PCI Express root port.");
                    }

                    disks.Add(new TestProfileDisk
                    {
                        Kind = kind,
                        Options = disk.Options ?? new Dictionary<string, string>(),
                        HotPlug = hotPlug
                    });
                }
            }

            // The USB host controller axis: the launcher's four spellings,
            // the two device versions QEMU's HID devices take, a full-speed
            // device only where a full-speed port exists, and a hub only
            // where its full speed can attach. Each is checked here so a
            // typo fails the catalog rather than a cell minutes later.
            UsbHostControllerModel usbController = UsbHostControllerModel.Xhci;
            string? usbControllerName = NullIfBlank(entry.UsbController);
            if (usbControllerName is not null && !QemuLauncher.TryParseUsbController(usbControllerName, out usbController))
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{entry.Name}' names unknown USB host controller '{usbControllerName}'. Expected {QemuLauncher.UsbControllerNames}.");
            }

            int usbDeviceVersion = entry.UsbDeviceVersion ?? QemuLauncher.DefaultUsbDeviceVersion;
            if (usbDeviceVersion != QemuLauncher.DefaultUsbDeviceVersion && usbDeviceVersion != QemuLauncher.FullSpeedUsbDeviceVersion)
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{entry.Name}' sets usbDeviceVersion to {usbDeviceVersion}; QEMU's usb-kbd and usb-mouse take {QemuLauncher.FullSpeedUsbDeviceVersion} or {QemuLauncher.DefaultUsbDeviceVersion}.");
            }

            // usb-ehci's root ports are high speed only and the standalone
            // model has no companion to hand a full-speed device to, so
            // QEMU refuses the attach at startup.
            if (usbDeviceVersion == QemuLauncher.FullSpeedUsbDeviceVersion && usbController == UsbHostControllerModel.Ehci)
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{entry.Name}' sets usbDeviceVersion {QemuLauncher.FullSpeedUsbDeviceVersion} on the 'ehci' controller; usb-ehci's ports are high speed only, so a full-speed-only device cannot attach (use 'ohci' or 'ehci-ohci').");
            }

            bool usbHub = entry.UsbHub ?? false;
            if (usbHub && !QemuLauncher.TakesUsbHub(usbController))
            {
                throw new InvalidOperationException(
                    $"{path}: profile '{entry.Name}' sets usbHub on the '{usbControllerName ?? "xhci"}' controller; the hub is full speed only and needs 'ohci' or 'ehci-ohci'.");
            }

            profiles[entry.Name] = new TestProfile
            {
                Name = entry.Name,
                Disks = disks,
                NetworkCard = NullIfBlank(entry.Nic),
                KeyboardDevice = NullIfBlank(entry.Keyboard),
                MouseDevice = NullIfBlank(entry.Mouse),
                UsbController = usbController,
                UsbDeviceVersion = usbDeviceVersion,
                UsbHub = usbHub,
                VgaAdapter = NullIfBlank(entry.Vga),
                GpuDevice = NullIfBlank(entry.Gpu),
                AudioDevice = NullIfBlank(entry.Audio),
                Architectures = entry.Architectures
            };

            // A machine property can be part of the hardware shape itself, not
            // just an overlay: virtio-pci needs MSI-X, which on arm64 needs a
            // GICv3 ITS, which only exists with gic-version=3. Expressing that
            // as a modifier would cross-product it onto every other profile the
            // suite declares.
            //
            // Scoped per architecture so ONE profile can span both: gic-version
            // is a virt property that q35 would reject, and without the scoping
            // the same hardware shape needs two profile names, which shows up
            // as two half-empty rows in the results matrix.
            var perArch = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
            if (entry.MachineOptions is not null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> kv in entry.MachineOptions)
                {
                    // Catches the flat {"gic-version": "3"} spelling, which
                    // would otherwise parse as an architecture named
                    // "gic-version" with no options and silently do nothing.
                    if (Array.IndexOf(KnownArchitectures, kv.Key) < 0)
                    {
                        throw new InvalidOperationException(
                            $"{path}: profile '{entry.Name}' scopes machineOptions to unknown architecture '{kv.Key}'. " +
                            $"Profile machine options are keyed by architecture ({string.Join(", ", KnownArchitectures)}), " +
                            $"e.g. \"machineOptions\": {{ \"arm64\": {{ \"gic-version\": \"3\" }} }}.");
                    }

                    perArch[kv.Key] = kv.Value;
                }
            }
            profileMachineOptions[entry.Name] = perArch;
        }

        var modifiers = new Dictionary<string, TestModifier>(StringComparer.Ordinal);
        if (parsed.Modifiers is not null)
        {
            foreach (ModifierEntry entry in parsed.Modifiers)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    throw new InvalidOperationException($"{path}: every modifier needs a non-empty 'name'.");
                }
                if (modifiers.ContainsKey(entry.Name))
                {
                    throw new InvalidOperationException($"{path}: duplicate modifier name '{entry.Name}'.");
                }

                bool hasMachine = entry.MachineOptions is not null && entry.MachineOptions.Count > 0;
                bool hasDevice = entry.DeviceOptions is not null && entry.DeviceOptions.Count > 0;
                bool hasCpus = entry.Cpus is not null;
                if (!hasMachine && !hasDevice && !hasCpus)
                {
                    throw new InvalidOperationException(
                        $"{path}: modifier '{entry.Name}' has neither 'machineOptions', 'deviceOptions' nor 'cpus'. A modifier with no targets can never apply.");
                }

                if (entry.Cpus is int cpus && cpus < 1)
                {
                    throw new InvalidOperationException(
                        $"{path}: modifier '{entry.Name}' sets cpus to {cpus}; a machine has at least one CPU.");
                }

                var perKind = new Dictionary<DiskKind, IReadOnlyDictionary<string, string>>();
                if (entry.DeviceOptions is not null)
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> kv in entry.DeviceOptions)
                    {
                        DiskKind kind = ParseDiskKind(path, $"modifier '{entry.Name}'", kv.Key);
                        perKind[kind] = kv.Value;
                    }
                }

                modifiers[entry.Name] = new TestModifier
                {
                    Name = entry.Name,
                    Architectures = entry.Architectures,
                    DeviceOptions = perKind,
                    MachineOptions = entry.MachineOptions ?? new Dictionary<string, string>(),
                    CpuCount = entry.Cpus
                };
            }
        }

        return new Catalog(profiles, modifiers, profileMachineOptions);
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DiskKind ParseDiskKind(string path, string context, string? type)
    {
        return (type ?? string.Empty).ToLowerInvariant() switch
        {
            "ahci" or "sata" => DiskKind.Ahci,
            "nvme" => DiskKind.Nvme,
            "usb" => DiskKind.Usb,
            "virtio-blk" => DiskKind.VirtioBlk,
            "virtio-blk-mmio" => DiskKind.VirtioBlkMmio,
            _ => throw new InvalidOperationException(
                $"{path}: {context} references unknown device kind '{type}'. Expected 'ahci', 'nvme', 'usb', 'virtio-blk' or 'virtio-blk-mmio'.")
        };
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private sealed record ProfilesFile(List<ProfileEntry>? Profiles, List<ModifierEntry>? Modifiers);
    private sealed record ProfileEntry(
        string? Name,
        List<DiskEntry>? Disks,
        string? Nic,
        string? Keyboard,
        string? Mouse,
        // The USB host controller axis: xhci, ehci, ohci or ehci-ohci; 1 or
        // 2 for the HID devices' USB version; a usb-hub on port 1.
        string? UsbController,
        int? UsbDeviceVersion,
        bool? UsbHub,
        string? Vga,
        string? Gpu,
        string? Audio,
        List<string>? Architectures,
        // Keyed by architecture, unlike a modifier's flat map: a modifier is
        // already scoped by its own "architectures" list, while a profile
        // describes one hardware shape that may span several.
        Dictionary<string, Dictionary<string, string>>? MachineOptions);
    private sealed record DiskEntry(string? Type, Dictionary<string, string>? Options, bool? HotPlug);
    private sealed record ModifierEntry(
        string? Name,
        List<string>? Architectures,
        Dictionary<string, Dictionary<string, string>>? DeviceOptions,
        Dictionary<string, string>? MachineOptions,
        // The CPU count (-smp), the one knob that is neither a -M property nor
        // a device option: QEMU takes it on its own switch.
        int? Cpus);
}
