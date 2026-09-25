using System;
using System.IO;
using System.Linq;
using Cosmos.TestRunner.Engine;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// Covers the device axis of the QEMU test-profile catalog: the NIC and input
/// models a profile attaches, the "devices" and "usb" list axes with the
/// checks that hold them to known models, and the architecture filter that
/// lets one suite declare per-arch hardware (virtio over PCI on x64, over
/// MMIO on arm64).
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
          "gpu": "virtio-gpu-pci"
        },
        {
          "name": "virtio-mmio",
          "architectures": ["arm64"],
          "nic": "virtio-net-device",
          "keyboard": "virtio-keyboard-device",
          "mouse": "virtio-mouse-device",
          "vga": "virtio",
          "gpu": "virtio-gpu-pci"
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
    }

    // One suite, two architectures, one csproj: each arch keeps only the
    // profile describing hardware it can actually present. Uses the two
    // arch-pinned profiles — virtio-pci deliberately spans both, so it would
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
        Assert.Empty(profile.Devices);
        Assert.Empty(profile.UsbDevices);
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

    private const string ListAxesCatalog = """
    {
      "profiles": [
        {
          "name": "pci-extras",
          "devices": ["edu", "rtl8139", "e1000e"]
        },
        {
          "name": "hid",
          "usb": ["usb-mouse", "usb-kbd"]
        },
        {
          "name": "stick-and-hid",
          "disks": [
            { "type": "usb" }
          ],
          "usb": ["usb-mouse", "usb-kbd"]
        },
        {
          "name": "nic-and-extra",
          "nic": "virtio-net-pci",
          "devices": ["rtl8139"]
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

    // Catalog order is attach order, and the NIC flag is what later gives a
    // card its own netdev: an edu marked as a NIC would get a netdev it cannot
    // take, and a NIC left unmarked would sit unplugged with no DHCP server.
    [Fact]
    public void LoadFor_ReadsTheDevicesAxisInOrderAndMarksTheNetworkCards()
    {
        WriteCatalogAndSuite(ListAxesCatalog, "pci-extras");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "x64"));

        DeviceAttachment[] expected =
        [
            new DeviceAttachment { Model = "edu" },
            new DeviceAttachment { Model = "rtl8139", IsNetworkCard = true },
            new DeviceAttachment { Model = "e1000e", IsNetworkCard = true }
        ];
        Assert.Equal(expected, profile.Devices);
        Assert.Empty(profile.UsbDevices);
    }

    [Fact]
    public void LoadFor_ReadsTheUsbAxisInOrder()
    {
        WriteCatalogAndSuite(ListAxesCatalog, "hid");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "arm64"));

        Assert.Equal(new[] { "usb-mouse", "usb-kbd" }, profile.UsbDevices);
        Assert.Empty(profile.Devices);
    }

    // Same reasoning as for the NIC: a modifier rebuilt from scratch would
    // drop the lists, and the cell would boot without the hardware it names.
    [Fact]
    public void LoadFor_ModifierOverlayPreservesTheDevicesAndUsbAxes()
    {
        WriteCatalogAndSuite(ListAxesCatalog, "pci-extras,stick-and-hid", "gicv3");

        TestProfile[] cells = TestProfileLoader.LoadFor(_suiteDir, "arm64").ToArray();

        TestProfile devices = Assert.Single(cells, c => c.Name == "pci-extras+gicv3");
        Assert.Equal(new[] { "edu", "rtl8139", "e1000e" }, devices.Devices.Select(d => d.Model));
        TestProfile usb = Assert.Single(cells, c => c.Name == "stick-and-hid+gicv3");
        Assert.Equal(new[] { "usb-mouse", "usb-kbd" }, usb.UsbDevices);
        Assert.Equal(DiskKind.Usb, Assert.Single(usb.Disks).Kind);
    }

    // A non-NIC model is a bare -device; each NIC gets a netdev of its own,
    // numbered apart from net0 so it can never collide with the profile's NIC.
    [Theory]
    [InlineData("x64")]
    [InlineData("arm64")]
    public void DevicesProfile_AttachesEachModelWithANetdevPerNetworkCard(string architecture)
    {
        WriteCatalogAndSuite(ListAxesCatalog, "pci-extras");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, architecture));

        Assert.Equal(
            " -device edu" +
            " -netdev user,id=devnet0 -device rtl8139,netdev=devnet0" +
            " -netdev user,id=devnet1 -device e1000e,netdev=devnet1",
            ProfileLaunchArgs.For(profile, architecture));
    }

    [Fact]
    public void DevicesProfile_LeavesTheProfileNicOnNet0()
    {
        WriteCatalogAndSuite(ListAxesCatalog, "nic-and-extra");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Equal(
            " -netdev user,id=net0 -device virtio-net-pci,netdev=net0" +
            " -netdev user,id=devnet0 -device rtl8139,netdev=devnet0",
            ProfileLaunchArgs.For(profile, "x64"));
    }

    // Neither q35 nor virt has a USB bus of its own, so the axis brings the
    // controller; bus= pins each device to it.
    [Theory]
    [InlineData("x64")]
    [InlineData("arm64")]
    public void UsbProfile_AddsTheXhciControllerAndPutsEachDeviceOnItsRootHub(string architecture)
    {
        WriteCatalogAndSuite(ListAxesCatalog, "hid");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, architecture));

        Assert.Equal(
            " -device qemu-xhci,id=usbxhci0" +
            " -device usb-mouse,bus=usbxhci0.0" +
            " -device usb-kbd,bus=usbxhci0.0",
            ProfileLaunchArgs.For(profile, architecture));
    }

    // One controller, not two: a second would be a second xHCI function for
    // the kernel to bring up. The stick keeps the ids QemuHotPlug unplugs and
    // re-plugs it by (usbdisk0, usbstick0, bus usbxhci0.0).
    [Theory]
    [InlineData("x64")]
    [InlineData("arm64")]
    public void UsbProfileWithAUsbDisk_SharesOneControllerWithTheStick(string architecture)
    {
        WriteCatalogAndSuite(ListAxesCatalog, "stick-and-hid");

        TestProfile profile = Assert.Single(TestProfileLoader.LoadFor(_suiteDir, architecture));
        string args = ProfileLaunchArgs.For(profile, architecture);

        Assert.Equal(
            " -device qemu-xhci,id=usbxhci0" +
            " -drive file=\"/tmp/disk0.img\",if=none,id=usbdisk0,format=raw" +
            " -device usb-storage,drive=usbdisk0,bus=usbxhci0.0,id=usbstick0" +
            " -device usb-mouse,bus=usbxhci0.0" +
            " -device usb-kbd,bus=usbxhci0.0",
            args);
        Assert.Equal(args.IndexOf("qemu-xhci", StringComparison.Ordinal), args.LastIndexOf("qemu-xhci", StringComparison.Ordinal));
    }

    // A typo has to fail the catalog load, on the host, rather than QEMU
    // refusing the cell after a kernel was built for it. So does a list that
    // contradicts itself or another axis of the same profile.
    [Theory]
    [InlineData("\"devices\": [\"rtl8193\"]", "unknown 'devices' model 'rtl8193'")]
    [InlineData("\"devices\": [\"EDU\"]", "unknown 'devices' model 'EDU'")]
    [InlineData("\"usb\": [\"usb-mice\"]", "unknown 'usb' model 'usb-mice'")]
    [InlineData("\"devices\": [\"usb-mouse\"]", "'usb-mouse' is listed under 'usb' instead")]
    [InlineData("\"usb\": [\"edu\"]", "'edu' is listed under 'devices' instead")]
    [InlineData("\"devices\": [\"edu\", \"edu\"]", "lists 'edu' twice in its 'devices' list")]
    [InlineData("\"usb\": [\"usb-kbd\", \"usb-kbd\"]", "lists 'usb-kbd' twice in its 'usb' list")]
    [InlineData("\"devices\": []", "empty 'devices' list")]
    [InlineData("\"usb\": []", "empty 'usb' list")]
    [InlineData("\"devices\": [\"\"]", "blank entry in its 'devices' list")]
    [InlineData("\"usb\": [\"  \"]", "blank entry in its 'usb' list")]
    [InlineData("\"devices\": [null]", "blank entry in its 'devices' list")]
    [InlineData("\"nic\": \"e1000e\", \"devices\": [\"e1000e\"]", "its 'nic' axis already attaches that model")]
    [InlineData("\"mouse\": \"usb-mouse\", \"usb\": [\"usb-mouse\"]", "its 'mouse' axis already attaches that model")]
    [InlineData("\"keyboard\": \"usb-kbd\", \"usb\": [\"usb-kbd\"]", "its 'keyboard' axis already attaches that model")]
    [InlineData("\"devices\": [{ \"model\": \"edu\" }]", "list QEMU model names as strings")]
    [InlineData("\"usb\": \"usb-mouse\"", "list QEMU model names as strings")]
    public void LoadFor_RejectsAnInvalidDeviceList(string axes, string expected)
    {
        string catalog = $$"""
        {
          "profiles": [
            { "name": "oops", {{axes}} }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("profiles.json", ex.Message);
        Assert.Contains(expected, ex.Message);
    }

    // The error has to say what would have been accepted, and where a new
    // model goes, or the fix is a trip into the runner's source.
    [Fact]
    public void LoadFor_NamesTheKnownModelsWhenOneIsUnknown()
    {
        WriteCatalogAndSuite("""{ "profiles": [ { "name": "oops", "devices": ["rtl8193"] } ] }""", "oops");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("profile 'oops'", ex.Message);
        Assert.Contains("Known 'devices' models: edu, rtl8139, e1000e.", ex.Message);
        Assert.Contains("ProfileDeviceModels", ex.Message);
    }

    // The whole catalog is checked on every load, so a broken entry fails
    // every suite at once, and the real-catalog tests catch it on the host,
    // not just the one suite that happens to use it.
    [Fact]
    public void LoadFor_RejectsAnInvalidDeviceListInAProfileTheSuiteDoesNotRequest()
    {
        const string catalog = """
        {
          "profiles": [
            { "name": "fine" },
            { "name": "oops", "usb": ["usb-mice"] }
          ]
        }
        """;
        WriteCatalogAndSuite(catalog, "fine");

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => TestProfileLoader.LoadFor(_suiteDir, "x64"));

        Assert.Contains("profile 'oops'", ex.Message);
    }
}
