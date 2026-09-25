// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Linq;
using System.Text;
using Cosmos.TestRunner.Engine;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// The QEMU arguments a resolved <see cref="TestProfile"/> contributes, built
/// the way the engine and the QEMU hosts build them. Covers the two blocks
/// that depend on the profile: the disks with the USB bus
/// (<c>AppendStorageArgs</c>) and the NIC, input, display and extra devices
/// (<c>AppendAttachedDevices</c>). The machine, CPU and KVM part in front of
/// them depends on the host, so it is left out.
/// </summary>
internal static class ProfileLaunchArgs
{
    /// <summary>Returns the two profile-dependent argument blocks, back to back.</summary>
    /// <param name="profile">A profile as <see cref="TestProfileLoader"/> resolved it.</param>
    /// <param name="architecture"><c>x64</c> or <c>arm64</c>.</param>
    internal static string For(TestProfile profile, string architecture)
    {
        QemuLaunchOptions options = new()
        {
            Architecture = architecture,
            IsoPath = "/tmp/kernel.iso",
            Disks = profile.Disks
                .Select((disk, index) => new DiskAttachment
                {
                    Path = $"/tmp/disk{index}.img",
                    Kind = disk.Kind,
                    ExtraDeviceOptions = disk.FormatOptions()
                })
                .ToArray(),
            MachineOptions = profile.MachineOptions,
            NetworkCard = profile.NetworkCard,
            KeyboardDevice = profile.KeyboardDevice,
            MouseDevice = profile.MouseDevice,
            VgaAdapter = profile.VgaAdapter,
            GpuDevice = profile.GpuDevice,
            Devices = profile.Devices,
            UsbDevices = profile.UsbDevices
        };

        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, options);
        QemuLauncher.AppendAttachedDevices(args, options);
        return args.ToString();
    }
}
