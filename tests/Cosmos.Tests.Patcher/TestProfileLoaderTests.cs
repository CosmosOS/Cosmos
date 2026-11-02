using System;
using System.IO;
using System.Linq;
using Cosmos.TestRunner.Engine;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// Covers the device axis of the QEMU test-profile catalog: the NIC and input
/// models a profile attaches, and the architecture filter that lets one suite
/// declare per-arch hardware (virtio over PCI on x64, over MMIO on arm64).
/// </summary>
[Collection("PatcherTests")]
public class TestProfileLoaderTests : IDisposable
{
    private readonly string _root;
    private readonly string _suiteDir;

    public TestProfileLoaderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "cosmos-profile-tests-" + Guid.NewGuid().ToString("N"));
        _suiteDir = Path.Combine(_root, "SuiteUnderTest");
        Directory.CreateDirectory(_suiteDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    /// <summary>Writes the catalog at the root and a suite csproj opting into the given items.</summary>
    private void WriteCatalogAndSuite(string catalogJson, string profiles, string modifiers = "")
    {
        File.WriteAllText(Path.Combine(_root, "profiles.json"), catalogJson);

        string items = string.Join(Environment.NewLine,
            profiles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => $"    <CosmosTestProfile Include=\"{p.Trim()}\" />")
                .Concat(modifiers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => $"    <CosmosTestModifier Include=\"{m.Trim()}\" />")));

        File.WriteAllText(Path.Combine(_suiteDir, "SuiteUnderTest.csproj"),
            $"<Project Sdk=\"Microsoft.NET.Sdk\">{Environment.NewLine}  <ItemGroup>{Environment.NewLine}{items}{Environment.NewLine}  </ItemGroup>{Environment.NewLine}</Project>");
    }

    private const string DeviceCatalog = """
    {
      "profiles": [
        {
          "name": "e1000e",
          "architectures": ["x64"],
          "nic": "e1000e"
        },
        {
          "name": "virtio-pci",
          "architectures": ["x64", "arm64"],
          "machineOptions": { "arm64": { "gic-version": "3" } },
          "nic": "virtio-net-pci",
          "keyboard": "virtio-keyboard-pci",
          "mouse": "virtio-mouse-pci",
          "vga": "virtio",
          "gpu": "virtio-gpu-pci",
          "audio": "intel-hda"
        },
        {
          "name": "virtio-mmio",
          "architectures": ["arm64"],
          "nic": "virtio-net-device",
          "keyboard": "virtio-keyboard-device",
          "mouse": "virtio-mouse-device",
          "vga": "virtio",
          "gpu": "virtio-gpu-pci",
          "audio": "ich9-intel-hda"
        },
        {
          "name": "plain"
        }
      ],
      "modifiers": [
        {
          "name": "gicv3",
          "architectures": ["arm64"],
          "machineOptions": { "gic-version": "3" }
        },
        {
          "name": "gicv2",
          "architectures": ["arm64"],
          "machineOptions": { "gic-version": "2" }
        }
      ]
    }
    """;

    [Fact]
    public void LoadFor_ReadsTheDeviceModelsOffTheProfile()
    {
        WriteCatalogAndSuite(DeviceCatalog, "virtio-pci,virtio-mmio");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Equal("virtio-pci", profile.Name);
        Assert.Equal("virtio-net-pci", profile.NetworkCard);
        Assert.Equal("virtio-keyboard-pci", profile.KeyboardDevice);
        Assert.Equal("virtio-mouse-pci", profile.MouseDevice);
        Assert.Equal("virtio", profile.VgaAdapter);
        Assert.Equal("virtio-gpu-pci", profile.GpuDevice);
        Assert.Equal("intel-hda", profile.AudioDevice);
    }

    // The two virtio-blk kinds a profile names reach the launcher as their
    // DiskKind, in the order the profile lists them.
    [Fact]
    public void LoadFor_ReadsTheVirtioBlkDiskKinds()
    {
        const string catalog = """
        {
          "profiles": [
            {
              "name": "vb",
              "architectures": ["arm64"],
              "disks": [
                { "type": "virtio-blk" },
                { "type": "virtio-blk-mmio" }
              ]
            }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "vb");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "arm64"));

        Assert.Equal("vb", profile.Name);
        Assert.Collection(profile.Disks,
            disk => Assert.Equal(DiskKind.VirtioBlk, disk.Kind),
            disk => Assert.Equal(DiskKind.VirtioBlkMmio, disk.Kind));
    }

    // The hotplug flag puts a virtio-blk disk behind a root port of its own;
    // a disk without it stays on the root bus.
    [Fact]
    public void LoadFor_ReadsTheHotPlugFlagOffTheDisk()
    {
        const string catalog = """
        {
          "profiles": [
            {
              "name": "vb",
              "disks": [
                { "type": "virtio-blk", "hotplug": true },
                { "type": "virtio-blk" }
              ]
            }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "vb");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Collection(profile.Disks,
            disk => Assert.True(disk.HotPlug),
            disk => Assert.False(disk.HotPlug));
    }

    // Only a virtio-blk-pci function sits behind a root port in the launcher,
    // so the flag on any other kind is a catalog error, not a silent no-op.
    [Fact]
    public void LoadFor_RejectsHotPlugOnANonVirtioBlkDisk()
    {
        const string catalog = """
        {
          "profiles": [
            {
              "name": "oops",
              "disks": [
                { "type": "nvme", "hotplug": true }
              ]
            }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("only a 'virtio-blk' disk", ex.Message);
    }

    // The USB host controller axis reaches the launcher as its model, with
    // the HID devices' USB version and the hub beside it; a profile naming
    // none of them keeps the xHCI, version 2 and no hub.
    [Fact]
    public void LoadFor_ReadsTheUsbControllerAxisOffTheProfile()
    {
        const string catalog = """
        {
          "profiles": [
            {
              "name": "companion",
              "keyboard": "usb-kbd",
              "usbController": "ehci-ohci",
              "usbDeviceVersion": 1
            },
            {
              "name": "hub",
              "keyboard": "usb-kbd",
              "usbController": "ohci",
              "usbHub": true
            },
            {
              "name": "stick",
              "disks": [
                { "type": "usb" }
              ]
            }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "companion,hub,stick");

        TestProfile[] cells = TestProfileLoader.LoadFor(_suiteDir, "x64").ToArray();

        TestProfile companion = Assert.Single(cells, c => c.Name == "companion");
        Assert.Equal(UsbHostControllerModel.EhciWithOhciCompanion, companion.UsbController);
        Assert.Equal(QemuLauncher.FullSpeedUsbDeviceVersion, companion.UsbDeviceVersion);
        Assert.False(companion.UsbHub);

        TestProfile hub = Assert.Single(cells, c => c.Name == "hub");
        Assert.Equal(UsbHostControllerModel.Ohci, hub.UsbController);
        Assert.Equal(QemuLauncher.DefaultUsbDeviceVersion, hub.UsbDeviceVersion);
        Assert.True(hub.UsbHub);

        TestProfile stick = Assert.Single(cells, c => c.Name == "stick");
        Assert.Equal(UsbHostControllerModel.Xhci, stick.UsbController);
        Assert.Equal(QemuLauncher.DefaultUsbDeviceVersion, stick.UsbDeviceVersion);
        Assert.False(stick.UsbHub);
    }

    // The launcher knows four spellings; a fifth would otherwise boot the
    // xHCI the profile was written to avoid.
    [Fact]
    public void LoadFor_RejectsAnUnknownUsbControllerNamingTheChoices()
    {
        const string catalog = """
        {
          "profiles": [
            { "name": "oops", "keyboard": "usb-kbd", "usbController": "uhci" }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("uhci", ex.Message);
        Assert.Contains("ehci-ohci", ex.Message);
    }

    // QEMU's usb-kbd and usb-mouse take usb_version 1 or 2 and nothing else.
    [Fact]
    public void LoadFor_RejectsAUsbDeviceVersionQemuDoesNotTake()
    {
        const string catalog = """
        {
          "profiles": [
            { "name": "oops", "keyboard": "usb-kbd", "usbController": "ehci", "usbDeviceVersion": 3 }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("usbDeviceVersion", ex.Message);
    }

    // usb-ehci's root ports are high speed only and the standalone model has
    // no companion, so a usb_version=1 keyboard cannot attach: QEMU refuses
    // it at startup, after the build, and the cell reports a failed boot.
    [Fact]
    public void LoadFor_RejectsAFullSpeedDeviceOnTheStandaloneEhci()
    {
        const string catalog = """
        {
          "profiles": [
            { "name": "oops", "keyboard": "usb-kbd", "usbController": "ehci", "usbDeviceVersion": 1 }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("usbDeviceVersion", ex.Message);
        Assert.Contains("'ehci'", ex.Message);
        Assert.Contains("ehci-ohci", ex.Message);
    }

    // The hub is full speed only: port 1 of qemu-xhci is a USB 3 port and a
    // standalone EHCI has no full-speed port, so QEMU would refuse the
    // attach at startup, minutes into a CI run.
    [Theory]
    [InlineData("xhci")]
    [InlineData("ehci")]
    public void LoadFor_RejectsAHubOnAHighSpeedOnlyController(string controller)
    {
        string catalog = $$"""
        {
          "profiles": [
            { "name": "oops", "keyboard": "usb-kbd", "usbController": "{{controller}}", "usbHub": true }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("usbHub", ex.Message);
        Assert.Contains(controller, ex.Message);
    }

    // The overlay copies the profile, so the controller axis survives a
    // modifier as the device models do.
    [Fact]
    public void LoadFor_ModifierOverlayPreservesTheUsbControllerAxis()
    {
        const string catalog = """
        {
          "profiles": [
            {
              "name": "ohci-hub",
              "architectures": ["arm64"],
              "keyboard": "usb-kbd",
              "usbController": "ohci",
              "usbHub": true
            }
          ],
          "modifiers": [
            {
              "name": "gicv3",
              "architectures": ["arm64"],
              "machineOptions": { "gic-version": "3" }
            }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "ohci-hub", "gicv3");

        TestProfile[] cells = TestProfileLoader.LoadFor(_suiteDir, "arm64").ToArray();

        TestProfile withModifier = Assert.Single(cells, c => c.Name == "ohci-hub+gicv3");
        Assert.Equal(UsbHostControllerModel.Ohci, withModifier.UsbController);
        Assert.True(withModifier.UsbHub);
        Assert.Equal("usb-kbd", withModifier.KeyboardDevice);
        Assert.Equal("3", withModifier.MachineOptions["gic-version"]);
    }

    // One suite, two architectures, one csproj: each arch keeps only the
    // profile describing hardware it can actually present. Uses the two
    // arch-pinned profiles: virtio-pci deliberately spans both, so it would
    // not exercise the filter.
    [Theory]
    [InlineData("x64", "e1000e", "e1000e")]
    [InlineData("arm64", "virtio-mmio", "virtio-net-device")]
    public void LoadFor_KeepsOnlyProfilesForTheTargetArchitecture(string architecture, string expectedName, string expectedNic)
    {
        WriteCatalogAndSuite(DeviceCatalog, "e1000e,virtio-mmio");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, architecture));

        Assert.Equal(expectedName, profile.Name);
        Assert.Equal(expectedNic, profile.NetworkCard);
    }

    [Fact]
    public void LoadFor_LeavesDeviceModelsNullWhenTheProfileNamesNone()
    {
        WriteCatalogAndSuite(DeviceCatalog, "plain");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Null(profile.NetworkCard);
        Assert.Null(profile.KeyboardDevice);
        Assert.Null(profile.MouseDevice);
        Assert.Null(profile.VgaAdapter);
        Assert.Null(profile.GpuDevice);
        Assert.Null(profile.AudioDevice);
        Assert.Equal(UsbHostControllerModel.Xhci, profile.UsbController);
        Assert.Equal(QemuLauncher.DefaultUsbDeviceVersion, profile.UsbDeviceVersion);
        Assert.False(profile.UsbHub);
    }

    // A modifier overlays machine/device options; everything it does not touch
    // has to survive. Rebuilding the profile from scratch instead of copying it
    // would silently drop the NIC and leave the cell testing the wrong driver.
    [Fact]
    public void LoadFor_ModifierOverlayPreservesTheDeviceModels()
    {
        WriteCatalogAndSuite(DeviceCatalog, "virtio-mmio", "gicv3");

        TestProfile[] cells = TestProfileLoader.LoadFor(_suiteDir, "arm64").ToArray();

        TestProfile withModifier = Assert.Single(cells, c => c.Name == "virtio-mmio+gicv3");
        Assert.Equal("virtio-net-device", withModifier.NetworkCard);
        Assert.Equal("virtio-keyboard-device", withModifier.KeyboardDevice);
        Assert.Equal("virtio-mouse-device", withModifier.MouseDevice);
        Assert.Equal("virtio", withModifier.VgaAdapter);
        Assert.Equal("virtio-gpu-pci", withModifier.GpuDevice);
        Assert.Equal("ich9-intel-hda", withModifier.AudioDevice);
        Assert.Equal("3", withModifier.MachineOptions["gic-version"]);
    }

    // Running zero cells would report a green suite that tested nothing, so a
    // suite whose every profile is pinned to another architecture is an error.
    [Fact]
    public void LoadFor_ThrowsWhenNoProfileAppliesToTheArchitecture()
    {
        WriteCatalogAndSuite(DeviceCatalog, "e1000e");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "arm64"));

        Assert.Contains("arm64", ex.Message);
    }

    // A machine property can belong to the hardware shape itself: virtio-pci
    // on arm64 needs MSI-X, which needs a GICv3 ITS, which only exists with
    // gic-version=3. Without this the cell would boot on the virt machine's
    // GICv2 default and the device could never take an interrupt.
    //
    // Scoping is what lets ONE profile span both arches: gic-version is a virt
    // property that q35 rejects, so the x64 cell must not inherit it.
    [Theory]
    [InlineData("arm64", "3")]
    [InlineData("x64", null)]
    public void LoadFor_AppliesOnlyTheMachineOptionsScopedToTheArchitecture(string architecture, string? expected)
    {
        WriteCatalogAndSuite(DeviceCatalog, "virtio-pci");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, architecture));

        Assert.Equal("virtio-net-pci", profile.NetworkCard);
        if (expected == null)
        {
            Assert.False(profile.MachineOptions.ContainsKey("gic-version"));
        }
        else
        {
            Assert.Equal(expected, profile.MachineOptions["gic-version"]);
        }
    }

    // An explicit modifier overlay beats the profile's own default, so a
    // suite can still sweep GIC versions over a profile that pins one.
    [Fact]
    public void LoadFor_ModifierMachineOptionOverridesTheProfileDefault()
    {
        WriteCatalogAndSuite(DeviceCatalog, "virtio-pci", "gicv2");

        TestProfile[] cells = TestProfileLoader.LoadFor(_suiteDir, "arm64").ToArray();

        Assert.Equal("3", Assert.Single(cells, c => c.Name == "virtio-pci").MachineOptions["gic-version"]);

        TestProfile overridden = Assert.Single(cells, c => c.Name == "virtio-pci+gicv2");
        Assert.Equal("2", overridden.MachineOptions["gic-version"]);
        Assert.Equal("virtio-net-pci", overridden.NetworkCard);
    }

    // Writing a profile's machineOptions flat is the natural mistake. It fails
    // in the parser (a string where an object is expected), which on its own
    // reports neither the file nor the fix, so the loader restates both.
    [Fact]
    public void LoadFor_RejectsFlatMachineOptionsWithGuidance()
    {
        const string flat = """
        {
          "profiles": [
            { "name": "oops", "machineOptions": { "gic-version": "3" } }
          ]
        }
        """;
        WriteCatalogAndSuite(flat, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "arm64"));

        Assert.Contains("profiles.json", ex.Message);
        Assert.Contains("keyed by architecture", ex.Message);
    }

    // Correctly shaped but scoped to something that is not an architecture:
    // this parses fine and would otherwise be silently dropped, leaving the
    // cell on the virt machine's GICv2 default with no ITS and no MSI-X.
    [Fact]
    public void LoadFor_RejectsMachineOptionsScopedToAnUnknownArchitecture()
    {
        const string wrongArch = """
        {
          "profiles": [
            { "name": "oops", "machineOptions": { "aarch64": { "gic-version": "3" } } }
          ]
        }
        """;
        WriteCatalogAndSuite(wrongArch, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "arm64"));

        Assert.Contains("aarch64", ex.Message);
        Assert.Contains("keyed by architecture", ex.Message);
    }

    // A modifier may carry only a CPU count: it applies to every profile of
    // its architectures, lands on the cell as its -smp, and two of them in one
    // combination contradict each other the way two gic-versions do.
    [Fact]
    public void LoadFor_CpuCountModifierSetsTheCellsCpus()
    {
        const string catalog = """
        {
          "profiles": [
            { "name": "plain" }
          ],
          "modifiers": [
            { "name": "smp4", "cpus": 4 },
            { "name": "smp2", "cpus": 2 }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "plain", "smp4,smp2");

        TestProfile[] cells = TestProfileLoader.LoadFor(_suiteDir, "x64").ToArray();

        Assert.Equal(3, cells.Length);
        Assert.Equal(1, Assert.Single(cells, c => c.Name == "plain").CpuCount);
        Assert.Equal(4, Assert.Single(cells, c => c.Name == "plain+smp4").CpuCount);
        Assert.Equal(2, Assert.Single(cells, c => c.Name == "plain+smp2").CpuCount);
    }

    [Fact]
    public void LoadFor_RejectsACpuCountBelowOne()
    {
        const string catalog = """
        {
          "profiles": [ { "name": "plain" } ],
          "modifiers": [ { "name": "smp0", "cpus": 0 } ]
        }
        """;
        WriteCatalogAndSuite(catalog, "plain", "smp0");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => TestProfileLoader.LoadFor(_suiteDir, "x64"));
        Assert.Contains("smp0", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LoadFor_FallsBackToTheDefaultProfileWhenTheSuiteOptsIntoNothing()
    {
        File.WriteAllText(Path.Combine(_root, "profiles.json"), DeviceCatalog);
        File.WriteAllText(Path.Combine(_suiteDir, "SuiteUnderTest.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.True(profile.IsDefault);
        Assert.Null(profile.NetworkCard);
    }
}
