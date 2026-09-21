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
