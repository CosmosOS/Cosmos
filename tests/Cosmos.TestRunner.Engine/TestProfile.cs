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
    /// <summary>Display label used to prefix test names — base profile name, or "profile+modifier".</summary>
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
    /// VGA adapter to swap in for the architecture default, as a QEMU
    /// <c>-vga</c> backend name (e.g. <c>vmware</c>), or null to leave the
    /// default in place. Replaces rather than adds — a second display adapter
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
    /// Architectures this profile applies to; null means any. Mirrors the
    /// modifier filter, for hardware that only one architecture can present
    /// (PS/2 on x64, virtio-mmio on the ARM64 virt machine).
    /// </summary>
    public IReadOnlyList<string>? Architectures { get; init; }

    public bool AppliesTo(string architecture) =>
        Architectures == null || Architectures.Contains(architecture, StringComparer.OrdinalIgnoreCase);

    /// <summary>Extra <c>-M</c> machine properties (e.g. <c>{"gic-version", "2"}</c>). Merged from base profile + applied modifier.</summary>
    public IReadOnlyDictionary<string, string> MachineOptions { get; init; } = new Dictionary<string, string>();

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
///   * <see cref="DeviceOptions"/> — per-disk-kind overrides (e.g. NVMe MSI-X).
///   * <see cref="MachineOptions"/> — <c>-M</c> properties (e.g. GIC version).
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

    public bool AppliesTo(TestProfile profile, string architecture)
    {
        if (Architectures != null && !Architectures.Contains(architecture, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        // Machine-only modifier — applies regardless of disks.
        if (DeviceOptions.Count == 0)
        {
            return MachineOptions.Count > 0;
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
            MachineOptions = mergedMachine
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

            // A profile pinned to another architecture drops out silently —
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
        // of its applicable modifiers (the matrix mix), so configs compose —
        // e.g. nvme+gicv2+acpi-off, not just nvme+gicv2 and nvme+acpi-off as
        // separate cells. A combination is dropped when two of its modifiers
        // write the same -M / device-option key (e.g. gicv2 + gicv3 both set
        // gic-version), which would be a contradictory machine.
        //
        // Cell count is 2^(applicable modifiers) per profile minus conflicting
        // subsets, so each added modifier roughly doubles a profile's cells —
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
    /// machine (<c>-M</c>) property or the same per-disk device option — a
    /// contradictory cell (e.g. gicv2 and gicv3 both set gic-version). Such
    /// combinations are dropped from the matrix.
    /// </summary>
    private static bool HasOptionConflict(List<TestModifier> combo)
    {
        var machineKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var deviceKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (TestModifier mod in combo)
        {
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
        if (parsed?.Profiles == null || parsed.Profiles.Count == 0)
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
            if (entry.Disks != null)
            {
                foreach (DiskEntry disk in entry.Disks)
                {
                    DiskKind kind = ParseDiskKind(path, $"profile '{entry.Name}'", disk.Type);
                    disks.Add(new TestProfileDisk
                    {
                        Kind = kind,
                        Options = disk.Options ?? new Dictionary<string, string>()
                    });
                }
            }

            profiles[entry.Name] = new TestProfile
            {
                Name = entry.Name,
                Disks = disks,
                NetworkCard = NullIfBlank(entry.Nic),
                KeyboardDevice = NullIfBlank(entry.Keyboard),
                MouseDevice = NullIfBlank(entry.Mouse),
                VgaAdapter = NullIfBlank(entry.Vga),
                GpuDevice = NullIfBlank(entry.Gpu),
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
            // the same hardware shape needs two profile names — which shows up
            // as two half-empty rows in the results matrix.
            var perArch = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
            if (entry.MachineOptions != null)
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
        if (parsed.Modifiers != null)
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

                bool hasMachine = entry.MachineOptions != null && entry.MachineOptions.Count > 0;
                bool hasDevice = entry.DeviceOptions != null && entry.DeviceOptions.Count > 0;
                if (!hasMachine && !hasDevice)
                {
                    throw new InvalidOperationException(
                        $"{path}: modifier '{entry.Name}' has neither 'machineOptions' nor 'deviceOptions'. A modifier with no targets can never apply.");
                }

                var perKind = new Dictionary<DiskKind, IReadOnlyDictionary<string, string>>();
                if (entry.DeviceOptions != null)
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
                    MachineOptions = entry.MachineOptions ?? new Dictionary<string, string>()
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
            _ => throw new InvalidOperationException(
                $"{path}: {context} references unknown device kind '{type}'. Expected 'ahci', 'nvme' or 'usb'.")
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
        string? Vga,
        string? Gpu,
        List<string>? Architectures,
        // Keyed by architecture, unlike a modifier's flat map: a modifier is
        // already scoped by its own "architectures" list, while a profile
        // describes one hardware shape that may span several.
        Dictionary<string, Dictionary<string, string>>? MachineOptions);
    private sealed record DiskEntry(string? Type, Dictionary<string, string>? Options);
    private sealed record ModifierEntry(
        string? Name,
        List<string>? Architectures,
        Dictionary<string, Dictionary<string, string>>? DeviceOptions,
        Dictionary<string, string>? MachineOptions);
}
