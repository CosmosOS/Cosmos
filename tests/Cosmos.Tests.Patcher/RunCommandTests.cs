using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Tools.Commands;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

[Collection("PatcherTests")]
public class RunCommandTests
{
    private static string CreateTempImage(string suffix = ".img")
    {
        string path = Path.Combine(Path.GetTempPath(), $"cosmos-disk-{Guid.NewGuid():N}{suffix}");
        File.WriteAllBytes(path, Array.Empty<byte>());
        return path;
    }

    [Fact]
    public void ParseDisks_DefaultsToAhciWhenNoKindSuffix()
    {
        string img = CreateTempImage();
        try
        {
            List<DiskAttachment> disks = RunCommand.ParseDisks([img]);

            Assert.Single(disks);
            Assert.Equal(DiskKind.Ahci, disks[0].Kind);
            Assert.Equal(Path.GetFullPath(img), disks[0].Path);
        }
        finally
        {
            File.Delete(img);
        }
    }

    [Theory]
    [InlineData("nvme", DiskKind.Nvme)]
    [InlineData("NVME", DiskKind.Nvme)]
    [InlineData("ahci", DiskKind.Ahci)]
    [InlineData("usb", DiskKind.Usb)]
    public void ParseDisks_HonorsExplicitKindSuffix(string suffix, DiskKind expected)
    {
        string img = CreateTempImage();
        try
        {
            List<DiskAttachment> disks = RunCommand.ParseDisks([$"{img},{suffix}"]);

            Assert.Single(disks);
            Assert.Equal(expected, disks[0].Kind);
            Assert.Equal(Path.GetFullPath(img), disks[0].Path);
        }
        finally
        {
            File.Delete(img);
        }
    }

    [Fact]
    public void ParseDisks_KeepsCommaInPathWhenSuffixIsNotAKind()
    {
        // A filename that legitimately contains a comma must survive: the split
        // only fires when the trailing token is a real kind, so here the whole
        // string is the path and the kind stays at the AHCI default.
        string img = CreateTempImage(",data.img");
        try
        {
            List<DiskAttachment> disks = RunCommand.ParseDisks([img]);

            Assert.Single(disks);
            Assert.Equal(DiskKind.Ahci, disks[0].Kind);
            Assert.Equal(Path.GetFullPath(img), disks[0].Path);
        }
        finally
        {
            File.Delete(img);
        }
    }

    [Fact]
    public void ParseDisks_ThrowsWhenImageMissing()
    {
        string missing = Path.Combine(Path.GetTempPath(), $"cosmos-missing-{Guid.NewGuid():N}.img");

        Assert.Throws<ArgumentException>(() => RunCommand.ParseDisks([$"{missing},nvme"]));
    }

    [Fact]
    public void ParseDisks_ThrowsOnEmptyPath()
    {
        Assert.Throws<ArgumentException>(() => RunCommand.ParseDisks([",nvme"]));
    }

    [Fact]
    public void ResolveInputDevice_DefaultsToVirtioOnArm64()
    {
        // The virt machine has no built-in PS/2 controller, so an unspecified
        // input device must fall back to the virtio model (#383).
        Assert.Equal("virtio-keyboard-device",
            RunCommand.ResolveInputDevice("arm64", null, "virtio-keyboard-device"));
    }

    [Fact]
    public void ResolveInputDevice_AddsNothingByDefaultOnX64()
    {
        Assert.Null(RunCommand.ResolveInputDevice("x64", null, "virtio-keyboard-device"));
    }

    [Theory]
    [InlineData("none")]
    [InlineData("usb-kbd")]
    public void ResolveInputDevice_ExplicitValueWinsOnArm64(string requested)
    {
        Assert.Equal(requested,
            RunCommand.ResolveInputDevice("arm64", requested, "virtio-keyboard-device"));
    }
}
