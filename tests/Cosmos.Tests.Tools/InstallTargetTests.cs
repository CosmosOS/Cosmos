using Cosmos.Tools.Commands;
using Cosmos.Tools.Platform;

namespace Cosmos.Tests.Tools;

public class InstallTargetTests
{
    [Theory]
    [InlineData(OSPlatform.Windows, false, "win-x64")]
    [InlineData(OSPlatform.Windows, true, "win-arm64")]
    [InlineData(OSPlatform.MacOS, true, "darwin-arm64")]
    [InlineData(OSPlatform.MacOS, false, "darwin-arm64")]
    [InlineData(OSPlatform.Linux, false, "linux-x64")]
    [InlineData(OSPlatform.Linux, true, "linux-x64")]
    public void GetPlatformTarget_NamesTheArchivesOfTheMachine(OSPlatform os, bool arm64, string expected)
    {
        Assert.Equal(expected, InstallCommand.GetPlatformTarget(os, arm64));
    }

    [Theory]
    [InlineData("llvm-tools", "win-arm64", "win-arm64")]
    [InlineData("qemu", "win-arm64", "win-arm64")]
    [InlineData("gdb", "win-arm64", "win-arm64")]
    [InlineData("xorriso", "win-arm64", "win-x64")]
    [InlineData("xorriso", "win-x64", "win-x64")]
    [InlineData("xorriso", "linux-x64", "linux-x64")]
    [InlineData("xorriso", "darwin-arm64", "darwin-arm64")]
    public void GetAssetTarget_OnlyXorrisoComesFromTheX64ArchiveOnWindowsArm64(string releaseAsset, string platform, string expected)
    {
        Assert.Equal(expected, InstallCommand.GetAssetTarget(releaseAsset, platform));
    }

    [Fact]
    public void GetAssetTarget_EveryBundledToolHasAnArchiveOnWindowsArm64()
    {
        // A tool added to ToolDefinitions lands on win-arm64 unless it is listed as x64 only:
        // this spells out which archives a Windows on ARM machine downloads.
        string[] assets = ToolDefinitions.GetAllTools()
            .OfType<CommandToolDefinition>()
            .Where(t => t.ReleaseAsset is not null)
            .Select(t => $"{t.ReleaseAsset}-{InstallCommand.GetAssetTarget(t.ReleaseAsset!, "win-arm64")}")
            .Distinct()
            .Order()
            .ToArray();

        Assert.Equal(["gdb-win-arm64", "llvm-tools-win-arm64", "qemu-win-arm64", "xorriso-win-x64"], assets);
    }

    [Fact]
    public void BundleTargetDiffers_NoBundle_IsFalse()
    {
        string missing = Path.Combine(Path.GetTempPath(), $"cosmos-target-test-{Guid.NewGuid():N}");

        Assert.False(InstallCommand.BundleTargetDiffers(missing, "win-arm64"));
    }

    [Theory]
    [InlineData("win-arm64", true)]
    [InlineData("win-x64", false)]
    [InlineData("linux-x64", false)]
    [InlineData("darwin-arm64", false)]
    public void BundleTargetDiffers_UnstampedBundle_DiffersFromWinArm64Only(string wanted, bool expected)
    {
        WithBundle(stamp: null, bundleDir => Assert.Equal(expected, InstallCommand.BundleTargetDiffers(bundleDir, wanted)));
    }

    [Theory]
    [InlineData("win-x64\n", "win-arm64", true)]
    [InlineData("win-arm64\r\n", "win-arm64", false)]
    [InlineData("win-arm64", "win-x64", true)]
    [InlineData("win-x64", "win-x64", false)]
    public void BundleTargetDiffers_StampedBundle_ComparesTheStamp(string stamp, string wanted, bool expected)
    {
        WithBundle(stamp, bundleDir => Assert.Equal(expected, InstallCommand.BundleTargetDiffers(bundleDir, wanted)));
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("bundle", null, true)]
    [InlineData("bundle", "x64", true)]
    [InlineData("bundle", "arm64", true)]
    [InlineData(null, "arm64", false)]
    [InlineData("bundle", "x86", false)]
    [InlineData("bundle", "ARM64", false)]
    public void Validate_ArchGoesWithSetupAndNamesAKnownArchitecture(string? setup, string? arch, bool valid)
    {
        InstallSettings settings = new InstallSettings { Setup = setup, Arch = arch };

        Assert.Equal(valid, settings.Validate().Successful);
    }

    // A bundle directory as an extracted archive leaves it: a VERSION stamp, and a TARGET one when given.
    private static void WithBundle(string? stamp, Action<string> check)
    {
        string bundleDir = Path.Combine(Path.GetTempPath(), $"cosmos-target-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(bundleDir);
        try
        {
            File.WriteAllText(Path.Combine(bundleDir, "VERSION"), "10.2.2\n");
            if (stamp is not null)
            {
                File.WriteAllText(Path.Combine(bundleDir, "TARGET"), stamp);
            }
            check(bundleDir);
        }
        finally
        {
            Directory.Delete(bundleDir, recursive: true);
        }
    }
}
