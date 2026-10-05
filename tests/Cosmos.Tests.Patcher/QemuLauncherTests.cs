using System;
using System.Text;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

[Collection("PatcherTests")]
public class QemuLauncherTests
{
    private static QemuLaunchOptions OptionsWithDisk(string path, DiskKind kind, string extra = "")
        => new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            Disks = [new DiskAttachment { Path = path, Kind = kind, ExtraDeviceOptions = extra }]
        };

    [Fact]
    public void AppendStorageArgs_DoublesCommasInDrivePaths()
    {
        // QEMU's -drive option parser truncates file= at an unescaped comma
        // and parses the remainder as bogus drive options ("Could not open
        // '/tmp/a'"); doubled commas are its escape.
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, OptionsWithDisk("/tmp/a,b.img", DiskKind.Nvme));

        Assert.Contains("file=\"/tmp/a,,b.img\"", args.ToString());
    }

    [Fact]
    public void AppendStorageArgs_PutsUsbDisksOnOneXhciController()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, new QemuLaunchOptions
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            Disks =
            [
                new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Usb },
                new DiskAttachment { Path = "/tmp/b.img", Kind = DiskKind.Usb }
            ]
        });

        string text = args.ToString();
        Assert.Contains("-device qemu-xhci,id=usbxhci0", text);
        Assert.Equal(text.IndexOf("qemu-xhci", StringComparison.Ordinal), text.LastIndexOf("qemu-xhci", StringComparison.Ordinal));
        Assert.Contains("-device usb-storage,drive=usbdisk0,bus=usbxhci0.0,id=usbstick0", text);
        Assert.Contains("-device usb-storage,drive=usbdisk1,bus=usbxhci0.0,id=usbstick1", text);
    }

    [Fact]
    public void AppendStorageArgs_EmitsAVirtioBlkPciFunctionOnTheRootBus()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, OptionsWithDisk("/tmp/a.img", DiskKind.VirtioBlk));

        string text = args.ToString();
        Assert.Contains(" -drive file=\"/tmp/a.img\",if=none,id=vblkdisk0,format=raw -device virtio-blk-pci,drive=vblkdisk0,id=vblk0", text);
        Assert.DoesNotContain("pcie-root-port", text);
    }

    [Fact]
    public void AppendStorageArgs_EmitsAVirtioBlkDeviceOnArm64()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, new QemuLaunchOptions
        {
            Architecture = "arm64",
            IsoPath = "/tmp/kernel.iso",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.VirtioBlkMmio }]
        });

        Assert.Contains(" -device virtio-blk-device,drive=vblkdisk0,id=vblk0", args.ToString());
    }

    // q35 has no virtio-mmio window, so the launcher refuses the kind before
    // QEMU would fail on a device with no bus to sit on.
    [Fact]
    public void AppendStorageArgs_RefusesVirtioBlkMmioOnX64()
    {
        StringBuilder args = new();

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            QemuLauncher.AppendStorageArgs(args, OptionsWithDisk("/tmp/a.img", DiskKind.VirtioBlkMmio)));
        Assert.Contains("virt machine", ex.Message);
    }

    [Fact]
    public void AppendStorageArgs_NumbersBothVirtioBlkKindsTogether()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, new QemuLaunchOptions
        {
            Architecture = "arm64",
            IsoPath = "/tmp/kernel.iso",
            Disks =
            [
                new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.VirtioBlk },
                new DiskAttachment { Path = "/tmp/b.img", Kind = DiskKind.VirtioBlkMmio }
            ]
        });

        string text = args.ToString();
        Assert.Contains(" -device virtio-blk-pci,drive=vblkdisk0,id=vblk0", text);
        Assert.Contains(" -device virtio-blk-device,drive=vblkdisk1,id=vblk1", text);
    }

    // A hot-pluggable disk gets a root port of its own ahead of it, since
    // QEMU resolves bus= against devices already on the command line; on q35
    // the ICH9 global turns the port to native hot-plug (stage9-experiments.md E3).
    [Fact]
    public void AppendStorageArgs_PutsAHotPluggableVirtioBlkBehindARootPort()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, new QemuLaunchOptions
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.VirtioBlk, HotPlug = true }]
        });

        Assert.Equal(
            " -global ICH9-LPC.acpi-pci-hotplug-with-bridge-support=off -device pcie-root-port,id=rp0,bus=pcie.0,chassis=1,slot=1 -drive file=\"/tmp/a.img\",if=none,id=vblkdisk0,format=raw -device virtio-blk-pci,drive=vblkdisk0,bus=rp0,id=vblk0",
            args.ToString());
    }

    // The virt machine's root ports are native always, and the virt machine
    // has no ICH9-LPC device for the global to name.
    [Fact]
    public void AppendStorageArgs_EmitsNoIch9GlobalOnArm64()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, new QemuLaunchOptions
        {
            Architecture = "arm64",
            IsoPath = "/tmp/kernel.iso",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.VirtioBlk, HotPlug = true }]
        });

        string text = args.ToString();
        Assert.Contains(" -device pcie-root-port,id=rp0,bus=pcie.0,chassis=1,slot=1", text);
        Assert.Contains(" -device virtio-blk-pci,drive=vblkdisk0,bus=rp0,id=vblk0", text);
        Assert.DoesNotContain("-global", text);
    }

    // QEMU refuses a duplicate chassis and slot pair, so each port numbers
    // its own; the global is machine-wide and appears once.
    [Fact]
    public void AppendStorageArgs_EmitsTheIch9GlobalOnceForTwoPorts()
    {
        StringBuilder args = new();
        QemuLauncher.AppendStorageArgs(args, new QemuLaunchOptions
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            Disks =
            [
                new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.VirtioBlk, HotPlug = true },
                new DiskAttachment { Path = "/tmp/b.img", Kind = DiskKind.VirtioBlk, HotPlug = true }
            ]
        });

        string text = args.ToString();
        Assert.Contains(" -global ICH9-LPC.acpi-pci-hotplug-with-bridge-support=off", text);
        Assert.Equal(text.IndexOf("-global", StringComparison.Ordinal), text.LastIndexOf("-global", StringComparison.Ordinal));
        Assert.Contains(" -device pcie-root-port,id=rp0,bus=pcie.0,chassis=1,slot=1", text);
        Assert.Contains(" -device pcie-root-port,id=rp1,bus=pcie.0,chassis=2,slot=2", text);
        Assert.Contains(" -device virtio-blk-pci,drive=vblkdisk0,bus=rp0,id=vblk0", text);
        Assert.Contains(" -device virtio-blk-pci,drive=vblkdisk1,bus=rp1,id=vblk1", text);
    }

    // The virt machine turns -cdrom into a virtio-blk-pci function a
    // virtio-blk driver would bind; virtio-scsi keeps the ISO off the block
    // layer (stage9-experiments.md E5).
    [Fact]
    public void AppendArm64Args_AttachesTheIsoThroughVirtioScsi()
    {
        StringBuilder args = new();
        QemuLauncher.AppendArm64Args(args, new QemuLaunchOptions { Architecture = "arm64", IsoPath = "/tmp/kernel.iso" });

        string text = args.ToString();
        Assert.Contains(" -device virtio-scsi-pci,id=scsi0 -drive file=\"/tmp/kernel.iso\",if=none,id=cosmoscd,format=raw,readonly=on,media=cdrom -device scsi-cd,drive=cosmoscd,bus=scsi0.0,bootindex=0", text);
        Assert.DoesNotContain("-cdrom", text);
    }

    // The keyboard rides the xHCI controller the USB disks share, under the
    // id the engine unplugs; a run with no USB disk gets the controller from
    // the keyboard itself, and never twice.
    [Fact]
    public void AppendUsbKeyboardArgs_PutsTheKeyboardOnOneXhciController()
    {
        QemuLaunchOptions keyboardOnly = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            KeyboardDevice = "usb-kbd"
        };
        StringBuilder args = new();
        QemuLauncher.AppendUsbKeyboardArgs(args, keyboardOnly, QemuLauncher.AppendStorageArgs(args, keyboardOnly));

        string text = args.ToString();
        Assert.Contains("-device qemu-xhci,id=usbxhci0", text);
        Assert.Equal(text.IndexOf("qemu-xhci", StringComparison.Ordinal), text.LastIndexOf("qemu-xhci", StringComparison.Ordinal));
        Assert.Contains(" -device usb-kbd,bus=usbxhci0.0,id=usbkbd0", text);

        // The shared input path adds nothing for it, so the keyboard is emitted once.
        args.Clear();
        QemuLauncher.AppendInputDevice(args, "usb-kbd");
        Assert.Equal(string.Empty, args.ToString());

        QemuLaunchOptions withDisk = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            KeyboardDevice = "usb-kbd",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Usb }]
        };
        args.Clear();
        QemuLauncher.AppendUsbKeyboardArgs(args, withDisk, QemuLauncher.AppendStorageArgs(args, withDisk));

        text = args.ToString();
        Assert.Equal(text.IndexOf("qemu-xhci", StringComparison.Ordinal), text.LastIndexOf("qemu-xhci", StringComparison.Ordinal));
        Assert.Contains("-device usb-storage,drive=usbdisk0,bus=usbxhci0.0,id=usbstick0", text);
        Assert.Contains(" -device usb-kbd,bus=usbxhci0.0,id=usbkbd0", text);
    }

    [Fact]
    public void AppendStorageArgs_RejectsQuotesInDrivePaths()
    {
        StringBuilder args = new();

        Assert.Throws<ArgumentException>(() =>
            QemuLauncher.AppendStorageArgs(args, OptionsWithDisk("/tmp/a\"b.img", DiskKind.Ahci)));
    }

    [Fact]
    public void AppendDeviceOptions_SplicesValidOptionsWithOneComma()
    {
        StringBuilder args = new();
        QemuLauncher.AppendDeviceOptions(args, "msix=off");
        Assert.Equal(",msix=off", args.ToString());

        args.Clear();
        QemuLauncher.AppendDeviceOptions(args, ",msix_qsize=1");
        Assert.Equal(",msix_qsize=1", args.ToString());
    }

    [Theory]
    [InlineData("msix=off -device e1000e")] // whitespace injects new argv tokens
    [InlineData("msix=\"off\"")]
    [InlineData("msix=off;rm")]
    public void AppendDeviceOptions_RejectsCharactersOutsideOptionAlphabet(string extra)
    {
        StringBuilder args = new();

        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendDeviceOptions(args, extra));
    }

    [Fact]
    public void EscapeDriveFileValue_LeavesPlainPathsUntouched()
    {
        Assert.Equal("/tmp/plain.img", QemuLauncher.EscapeDriveFileValue("/tmp/plain.img"));
    }

    [Fact]
    public void AppendNetworkCardArgs_NoneDisablesTheDefaultNic()
    {
        StringBuilder args = new();
        QemuLauncher.AppendNetworkCardArgs(args, "none");
        Assert.Equal(" -nic none", args.ToString());
    }

    [Theory]
    [InlineData("e1000e")]
    [InlineData("virtio-net-device")]
    public void AppendNetworkCardArgs_AttachesUserModeNicForAModel(string model)
    {
        StringBuilder args = new();
        QemuLauncher.AppendNetworkCardArgs(args, model);
        Assert.Equal($" -netdev user,id=net0 -device {model},netdev=net0", args.ToString());
    }

    [Theory]
    [InlineData("e1000e -device rm")] // whitespace injects new argv tokens
    [InlineData("e1000e,netdev=x;rm")]
    public void AppendNetworkCardArgs_RejectsCharactersOutsideOptionAlphabet(string model)
    {
        StringBuilder args = new();

        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendNetworkCardArgs(args, model));
    }

    [Theory]
    [InlineData("virtio-keyboard-device")]
    [InlineData("virtio-mouse-device")]
    public void AppendInputDevice_EmitsDeviceForRealModels(string model)
    {
        StringBuilder args = new();
        QemuLauncher.AppendInputDevice(args, model);
        Assert.Equal($" -device {model}", args.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("none")]
    [InlineData("ps2")]
    [InlineData("PS2")]
    public void AppendInputDevice_AddsNothingForSentinels(string? model)
    {
        StringBuilder args = new();
        QemuLauncher.AppendInputDevice(args, model);
        Assert.Equal(string.Empty, args.ToString());
    }

    [Fact]
    public void AppendInputDevice_RejectsCharactersOutsideOptionAlphabet()
    {
        StringBuilder args = new();
        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendInputDevice(args, "virtio-keyboard-device -device rm"));
    }

    // "none" passes through deliberately: it is QEMU's own spelling for "no
    // VGA adapter", not a sentinel of ours like the input devices' "ps2".
    [Theory]
    [InlineData("vmware")]
    [InlineData("virtio")]
    [InlineData("none")]
    public void AppendVgaAdapter_EmitsVgaForAModel(string adapter)
    {
        StringBuilder args = new();
        QemuLauncher.AppendVgaAdapter(args, adapter);
        Assert.Equal($" -vga {adapter}", args.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AppendVgaAdapter_AddsNothingWhenUnset(string? adapter)
    {
        StringBuilder args = new();
        QemuLauncher.AppendVgaAdapter(args, adapter);
        Assert.Equal(string.Empty, args.ToString());
    }

    [Fact]
    public void AppendVgaAdapter_RejectsCharactersOutsideOptionAlphabet()
    {
        StringBuilder args = new();
        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendVgaAdapter(args, "vmware -device rm"));
    }

    // -device, not -vga: the adapter is ADDED beside the machine default so
    // the firmware framebuffer Limine boots on survives. -vga would replace
    // it, and on the arm64 virt machine "-vga virtio" is not even accepted.
    [Fact]
    public void AppendGpuDevice_EmitsDeviceForAModel()
    {
        StringBuilder args = new();
        QemuLauncher.AppendGpuDevice(args, "virtio-gpu-pci");
        Assert.Equal(" -device virtio-gpu-pci", args.ToString());
    }

    // "none" is a sentinel here, unlike -vga none: there is no such -device.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("none")]
    [InlineData("NONE")]
    public void AppendGpuDevice_AddsNothingWhenUnsetOrNone(string? model)
    {
        StringBuilder args = new();
        QemuLauncher.AppendGpuDevice(args, model);
        Assert.Equal(string.Empty, args.ToString());
    }

    [Fact]
    public void AppendGpuDevice_RejectsCharactersOutsideOptionAlphabet()
    {
        StringBuilder args = new();
        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendGpuDevice(args, "virtio-gpu-pci -device rm"));
    }

    [Theory]
    [InlineData("x64", "e1000e")]
    [InlineData("arm64", "virtio-net-device")]
    public void ResolveNetworkTestNic_FallsBackToTheArchitectureDefault(string architecture, string expected)
    {
        Assert.Equal(expected, QemuLauncher.ResolveNetworkTestNic(architecture, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveNetworkTestNic_TreatsBlankAsUnset(string requested)
    {
        Assert.Equal("e1000e", QemuLauncher.ResolveNetworkTestNic("x64", requested));
    }

    // A profile naming a NIC has to win over the architecture default, which
    // is what lets the network suite run its protocol tests over virtio-net-pci
    // on x64 instead of only E1000E.
    [Theory]
    [InlineData("x64", "virtio-net-pci")]
    [InlineData("arm64", "virtio-net-device")]
    public void ResolveNetworkTestNic_PrefersTheProfileModel(string architecture, string model)
    {
        Assert.Equal(model, QemuLauncher.ResolveNetworkTestNic(architecture, model));
    }

    // "none" is legal for a plain `cosmos run`, but a network-testing run with
    // no card would boot and then fail every test on an unreachable guest.
    [Fact]
    public void ResolveNetworkTestNic_RejectsNone()
    {
        Assert.Throws<ArgumentException>(() => QemuLauncher.ResolveNetworkTestNic("x64", "none"));
    }

    [Theory]
    [InlineData("virtio-net-pci -device rm")]
    [InlineData("virtio-net-pci,netdev=x;rm")]
    public void ResolveNetworkTestNic_RejectsCharactersOutsideOptionAlphabet(string model)
    {
        Assert.Throws<ArgumentException>(() => QemuLauncher.ResolveNetworkTestNic("x64", model));
    }
}
