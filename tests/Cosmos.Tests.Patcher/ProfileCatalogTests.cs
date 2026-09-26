using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cosmos.TestRunner.Engine;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// Validates the real <c>tests/profiles.json</c> against the real suite
/// csproj files, rather than a synthetic catalog like
/// <see cref="TestProfileLoaderTests"/> does.
///
/// A suite naming a profile that does not exist, or whose profiles are all
/// pinned to the other architecture, only surfaces today once CI has built a
/// kernel and booted QEMU — minutes in, for what is a typo. These run in
/// milliseconds and need no kernel build.
/// </summary>
[Collection("PatcherTests")]
public class ProfileCatalogTests
{
    private static readonly string[] Architectures = ["x64", "arm64"];

    /// <summary>Walks up from the test assembly to the repo root (the directory holding tests/profiles.json).</summary>
    private static string FindRepoRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "tests", "profiles.json")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate the repo root from " + AppContext.BaseDirectory);
    }

    /// <summary>Every kernel suite directory that opts into at least one profile.</summary>
    public static TheoryData<string> SuitesDeclaringProfiles()
    {
        string kernels = Path.Combine(FindRepoRoot(), "tests", "Kernels");
        var data = new TheoryData<string>();

        foreach (string dir in Directory.EnumerateDirectories(kernels))
        {
            bool declares = Directory.EnumerateFiles(dir, "*.csproj")
                .Any(p => File.ReadAllText(p).Contains("CosmosTestProfile", StringComparison.Ordinal));

            if (declares)
            {
                data.Add(Path.GetFileName(dir));
            }
        }

        Assert.NotEmpty(data);
        return data;
    }

    // Resolution throws on an unknown profile name and on a suite left with no
    // applicable cell, so simply loading every suite for every architecture is
    // the whole assertion. The non-empty check pins the outcome: a suite that
    // silently resolved to zero cells would report green having tested nothing.
    [Theory]
    [MemberData(nameof(SuitesDeclaringProfiles))]
    public void EverySuiteResolvesOnEveryArchitecture(string suiteName)
    {
        string suiteDir = Path.Combine(FindRepoRoot(), "tests", "Kernels", suiteName);

        foreach (string architecture in Architectures)
        {
            IReadOnlyList<TestProfile> cells = TestProfileLoader.LoadFor(suiteDir, architecture);
            Assert.NotEmpty(cells);
        }
    }

    // A virtio PCI profile is one hardware shape covering both arches, so each
    // must produce a cell on each. Two profiles differing only by architecture
    // would render as two half-empty rows in the results matrix.
    [Theory]
    [InlineData("Virtio", "virtio-pci")]
    [InlineData("Network", "virtio-net-pci")]
    public void VirtioPciProfilesProduceACellOnBothArchitectures(string suiteName, string profileName)
    {
        string suiteDir = Path.Combine(FindRepoRoot(), "tests", "Kernels", "Cosmos.Kernel.Tests." + suiteName);

        foreach (string architecture in Architectures)
        {
            IReadOnlyList<TestProfile> cells = TestProfileLoader.LoadFor(suiteDir, architecture);

            TestProfile cell = Assert.Single(cells, c => c.Name == profileName);
            Assert.Equal("virtio-net-pci", cell.NetworkCard);

            // MSI-X on arm64 is routed by the GICv3 ITS, and the virt machine
            // defaults to GICv2, which has none — losing this would not fail
            // loudly, the device would simply never take an interrupt. q35
            // rejects the property outright, so x64 must not carry it.
            if (architecture == "arm64")
            {
                Assert.Equal("3", cell.MachineOptions["gic-version"]);
            }
            else
            {
                Assert.False(cell.MachineOptions.ContainsKey("gic-version"));
            }
        }
    }

    // The vmware-svga cell is the only run where the VMware SVGA II adapter
    // is present at all — losing it would fail nothing, the suite would just
    // never see that hardware again (and the future SVGAII driver would run
    // untested). The bare cell keeps the default-adapter path covered.
    [Fact]
    public void GraphicSuiteCoversBothVgaAdapters()
    {
        string suiteDir = Path.Combine(FindRepoRoot(), "tests", "Kernels", "Cosmos.Kernel.Tests.Graphic");

        IReadOnlyList<TestProfile> x64Cells = TestProfileLoader.LoadFor(suiteDir, "x64");
        Assert.Single(x64Cells, c => c.Name == "bare" && c.VgaAdapter == null);
        TestProfile svga = Assert.Single(x64Cells, c => c.Name == "vmware-svga");
        Assert.Equal("vmware", svga.VgaAdapter);

        // The adapter is programmed through PCI port I/O, so the arm64 column
        // runs only the cells whose hardware the virt machine can present.
        IReadOnlyList<TestProfile> arm64Cells = TestProfileLoader.LoadFor(suiteDir, "arm64");
        Assert.DoesNotContain(arm64Cells, c => c.Name == "vmware-svga");
        TestProfile bare = Assert.Single(arm64Cells, c => c.Name == "bare");
        Assert.Null(bare.VgaAdapter);
    }

    // The "devices" and "usb" axes are additive: a profile naming neither
    // must emit, byte for byte, the arguments it emitted before they existed.
    // Pinned on the cells that run through the code the axes share: a USB
    // stick (the xHCI controller the "usb" axis now joins, with the ids
    // QemuHotPlug addresses the stick by) and a NIC (AppendNetworkCardArgs,
    // which the "devices" axis reuses for its NIC models).
    [Theory]
    [InlineData("Storage", "usb", "x64")]
    [InlineData("Storage", "usb", "arm64")]
    public void UsbStickProfileKeepsItsOriginalArguments(string suiteName, string profileName, string architecture)
    {
        TestProfile cell = LoadCell(suiteName, profileName, architecture);

        Assert.Empty(cell.UsbDevices);
        Assert.Equal(
            " -device qemu-xhci,id=usbxhci0" +
            " -drive file=\"/tmp/disk0.img\",if=none,id=usbdisk0,format=raw" +
            " -device usb-storage,drive=usbdisk0,bus=usbxhci0.0,id=usbstick0",
            ProfileLaunchArgs.For(cell, architecture));
    }

    [Theory]
    [InlineData("x64", "virtio-pci", " -netdev user,id=net0 -device virtio-net-pci,netdev=net0 -device virtio-keyboard-pci -device virtio-mouse-pci")]
    [InlineData("arm64", "virtio-mmio", " -netdev user,id=net0 -device virtio-net-device,netdev=net0 -device virtio-keyboard-device -device virtio-mouse-device")]
    public void VirtioProfilesKeepTheirOriginalArguments(string architecture, string profileName, string expected)
    {
        TestProfile cell = LoadCell("Virtio", profileName, architecture);

        Assert.Empty(cell.Devices);
        Assert.Equal(expected, ProfileLaunchArgs.For(cell, architecture));
    }

    // The Drivers suite's cells are the only runs with hardware no built-in
    // driver claims, and the driver kit's binding cells will be written
    // against them. Losing one would fail nothing: the suite would just stop
    // presenting that device. Pinned here are the cells, what each attaches,
    // and the arguments that attach it.
    [Theory]
    [InlineData("x64", "edu", " -device edu")]
    [InlineData("x64", "rtl8139", " -netdev user,id=devnet0 -device rtl8139,netdev=devnet0")]
    [InlineData("x64", "usb-hid", " -device qemu-xhci,id=usbxhci0 -device usb-mouse,bus=usbxhci0.0,id=usbdev0 -device usb-tablet,bus=usbxhci0.0,id=usbdev1 -device usb-kbd,bus=usbxhci0.0,id=usbdev2")]
    [InlineData("arm64", "edu", " -device edu")]
    [InlineData("arm64", "rtl8139", " -netdev user,id=devnet0 -device rtl8139,netdev=devnet0")]
    [InlineData("arm64", "e1000e-arm64", " -netdev user,id=devnet0 -device e1000e,netdev=devnet0")]
    [InlineData("arm64", "usb-hid", " -device qemu-xhci,id=usbxhci0 -device usb-mouse,bus=usbxhci0.0,id=usbdev0 -device usb-tablet,bus=usbxhci0.0,id=usbdev1 -device usb-kbd,bus=usbxhci0.0,id=usbdev2")]
    public void DriversSuiteAttachesOneUnclaimedDevicePerProfile(string architecture, string profileName, string expected)
    {
        TestProfile cell = LoadCell("Drivers", profileName, architecture);

        Assert.Empty(cell.Disks);
        Assert.Null(cell.NetworkCard);
        Assert.Equal(expected, ProfileLaunchArgs.For(cell, architecture));
    }

    // The E1000E built-in claims the 82574L on x64, so only arm64 presents it
    // unclaimed, and a driver there gets MSI-X only through the GICv3 ITS.
    // The USB devices run on both GICs: xHCI takes MSI-X on GICv3 and is
    // polled on GICv2.
    [Fact]
    public void DriversSuiteCoversTheGicVersionsItsCellsNeed()
    {
        string suiteDir = Path.Combine(FindRepoRoot(), "tests", "Kernels", "Cosmos.Kernel.Tests.Drivers");

        IReadOnlyList<TestProfile> x64Cells = TestProfileLoader.LoadFor(suiteDir, "x64");
        string[] x64Names = ["edu", "rtl8139", "usb-hid", "nvme"];
        Assert.Equal(x64Names, x64Cells.Select(c => c.Name).ToArray());
        Assert.All(x64Cells, c => Assert.False(c.MachineOptions.ContainsKey("gic-version")));

        IReadOnlyList<TestProfile> arm64Cells = TestProfileLoader.LoadFor(suiteDir, "arm64");
        Assert.Equal("3", Assert.Single(arm64Cells, c => c.Name == "e1000e-arm64+gicv3").MachineOptions["gic-version"]);
        Assert.Equal("2", Assert.Single(arm64Cells, c => c.Name == "usb-hid+gicv2").MachineOptions["gic-version"]);
        Assert.Equal("3", Assert.Single(arm64Cells, c => c.Name == "usb-hid+gicv3").MachineOptions["gic-version"]);
    }

    /// <summary>Loads one suite for one architecture and returns its bare cell for <paramref name="profileName"/>.</summary>
    private static TestProfile LoadCell(string suiteName, string profileName, string architecture)
    {
        string suiteDir = Path.Combine(FindRepoRoot(), "tests", "Kernels", $"Cosmos.Kernel.Tests.{suiteName}");
        return Assert.Single(TestProfileLoader.LoadFor(suiteDir, architecture), c => c.Name == profileName);
    }

    // The virtio-gpu cell is the only run with a virtio GPU on the bus, and it
    // has to stay ADDITIVE: it attaches the device with -device and leaves the
    // machine default alone, because that default is what hands Limine the
    // framebuffer the whole suite renders into. A well-meaning rewrite to
    // "vga": "virtio" would still pass on x64 (which has the VGA-compatible
    // virtio-vga) and break arm64, where QEMU refuses it outright.
    [Fact]
    public void GraphicSuiteCoversVirtioGpuOnBothArchitectures()
    {
        string suiteDir = Path.Combine(FindRepoRoot(), "tests", "Kernels", "Cosmos.Kernel.Tests.Graphic");

        foreach (string architecture in Architectures)
        {
            IReadOnlyList<TestProfile> cells = TestProfileLoader.LoadFor(suiteDir, architecture);

            TestProfile cell = Assert.Single(cells, c => c.Name == "virtio-gpu");
            Assert.Equal("virtio-gpu-pci", cell.GpuDevice);
            Assert.Null(cell.VgaAdapter);

            // virtio-pci MSI-X is routed by the GICv3 ITS and virt defaults to
            // GICv2, which has none; q35 rejects the property outright.
            if (architecture == "arm64")
            {
                Assert.Equal("3", cell.MachineOptions["gic-version"]);
            }
            else
            {
                Assert.False(cell.MachineOptions.ContainsKey("gic-version"));
            }
        }
    }
}
