using System;
using System.Collections.Generic;
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

    // The mouse rides the same xHCI controller as the USB disks and the
    // keyboard; alone, it brings the controller itself, and never twice.
    [Fact]
    public void AppendUsbMouseArgs_PutsTheMouseOnOneXhciController()
    {
        QemuLaunchOptions mouseOnly = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            MouseDevice = "usb-mouse"
        };
        StringBuilder args = new();
        Assert.True(QemuLauncher.AppendUsbMouseArgs(args, mouseOnly, QemuLauncher.AppendUsbKeyboardArgs(args, mouseOnly, QemuLauncher.AppendStorageArgs(args, mouseOnly))));

        string text = args.ToString();
        Assert.Contains("-device qemu-xhci,id=usbxhci0", text);
        Assert.Equal(text.IndexOf("qemu-xhci", StringComparison.Ordinal), text.LastIndexOf("qemu-xhci", StringComparison.Ordinal));
        Assert.Contains(" -device usb-mouse,bus=usbxhci0.0,id=usbmouse0", text);
        Assert.DoesNotContain("usb-kbd", text);

        // The shared input path adds nothing for it, so the mouse is emitted once.
        args.Clear();
        QemuLauncher.AppendInputDevice(args, "usb-mouse");
        Assert.Equal(string.Empty, args.ToString());

        QemuLaunchOptions withKeyboardAndDisk = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            KeyboardDevice = "usb-kbd",
            MouseDevice = "usb-mouse",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Usb }]
        };
        args.Clear();
        QemuLauncher.AppendUsbMouseArgs(args, withKeyboardAndDisk, QemuLauncher.AppendUsbKeyboardArgs(args, withKeyboardAndDisk, QemuLauncher.AppendStorageArgs(args, withKeyboardAndDisk)));

        text = args.ToString();
        Assert.Equal(text.IndexOf("qemu-xhci", StringComparison.Ordinal), text.LastIndexOf("qemu-xhci", StringComparison.Ordinal));
        Assert.Contains("-device usb-storage,drive=usbdisk0,bus=usbxhci0.0,id=usbstick0", text);
        Assert.Contains(" -device usb-kbd,bus=usbxhci0.0,id=usbkbd0", text);
        Assert.Contains(" -device usb-mouse,bus=usbxhci0.0,id=usbmouse0", text);

        // Neither USB input device asked for: no controller from them.
        QemuLaunchOptions none = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso"
        };
        args.Clear();
        Assert.False(QemuLauncher.AppendUsbMouseArgs(args, none, QemuLauncher.AppendUsbKeyboardArgs(args, none, QemuLauncher.AppendStorageArgs(args, none))));
        Assert.DoesNotContain("qemu-xhci", args.ToString());
    }

    // The EHCI and the OHCI number their root ports in one space that QEMU
    // fills in command-line order, inserting a hub of its own on the last
    // free port, so every device is pinned: the keyboard on 1, the mouse on
    // 2, the sticks from 3. The controller is emitted once, ahead of them.
    [Theory]
    [InlineData(UsbHostControllerModel.Ehci, "usb-ehci,id=usbehci0", "usbehci0.0")]
    [InlineData(UsbHostControllerModel.Ohci, "pci-ohci,id=usbohci0,num-ports=6", "usbohci0.0")]
    public void AppendUsbKeyboardArgs_PinsTheDevicesToThePortsOfOneController(UsbHostControllerModel model, string controller, string bus)
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbController = model,
            KeyboardDevice = "usb-kbd",
            MouseDevice = "usb-mouse",
            Disks =
            [
                new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Usb },
                new DiskAttachment { Path = "/tmp/b.img", Kind = DiskKind.Usb }
            ]
        };
        StringBuilder args = new();
        Assert.True(QemuLauncher.AppendUsbMouseArgs(args, options, QemuLauncher.AppendUsbKeyboardArgs(args, options, QemuLauncher.AppendStorageArgs(args, options))));

        string text = args.ToString();
        Assert.Contains($" -device {controller}", text);
        Assert.Equal(text.IndexOf(controller, StringComparison.Ordinal), text.LastIndexOf(controller, StringComparison.Ordinal));
        Assert.True(text.IndexOf(controller, StringComparison.Ordinal) < text.IndexOf("usb-storage", StringComparison.Ordinal), "the controller precedes its devices");
        Assert.DoesNotContain("qemu-xhci", text);
        Assert.DoesNotContain("masterbus", text);
        Assert.DoesNotContain("usb-hub", text);
        Assert.DoesNotContain("usb_version", text);
        Assert.Contains($" -device usb-storage,drive=usbdisk0,bus={bus},id=usbstick0,port=3", text);
        Assert.Contains($" -device usb-storage,drive=usbdisk1,bus={bus},id=usbstick1,port=4", text);
        Assert.Contains($" -device usb-kbd,bus={bus},id=usbkbd0,port=1", text);
        Assert.Contains($" -device usb-mouse,bus={bus},id=usbmouse0,port=2", text);
    }

    // The port plan leaves root ports 3 to 6 to the sticks, so four fit a
    // port-pinning controller; QEMU would refuse a fifth at startup with
    // "usb port 7 not found", naming neither the plan nor the disk, so the
    // launcher refuses first. The xHCI picks its own ports and takes any number.
    [Theory]
    [InlineData(UsbHostControllerModel.Ehci)]
    [InlineData(UsbHostControllerModel.Ohci)]
    [InlineData(UsbHostControllerModel.EhciWithOhciCompanion)]
    public void AppendStorageArgs_RefusesAFifthUsbDiskOnAPortPinningController(UsbHostControllerModel model)
    {
        StringBuilder args = new();
        Assert.True(QemuLauncher.AppendStorageArgs(args, OptionsWithUsbDisks(model, 4)));
        Assert.Contains($",id=usbstick3,port={QemuLauncher.UsbRootPortCount}", args.ToString());

        args.Clear();
        ArgumentException ex = Assert.Throws<ArgumentException>(() => QemuLauncher.AppendStorageArgs(args, OptionsWithUsbDisks(model, 5)));
        Assert.Contains("At most 4 USB disks", ex.Message);
        Assert.Contains($"ports {QemuLauncher.FirstUsbDiskPort} to {QemuLauncher.UsbRootPortCount}", ex.Message);

        args.Clear();
        Assert.True(QemuLauncher.AppendStorageArgs(args, OptionsWithUsbDisks(UsbHostControllerModel.Xhci, 5)));
        Assert.Contains(",id=usbstick4", args.ToString());
        Assert.DoesNotContain("port=", args.ToString());
    }

    /// <summary>A run with <paramref name="count"/> USB sticks on the controller <paramref name="model"/> names.</summary>
    private static QemuLaunchOptions OptionsWithUsbDisks(UsbHostControllerModel model, int count)
    {
        List<DiskAttachment> disks = new(count);
        for (int i = 0; i < count; i++)
        {
            disks.Add(new DiskAttachment { Path = $"/tmp/{i}.img", Kind = DiskKind.Usb });
        }

        return new QemuLaunchOptions
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbController = model,
            Disks = disks
        };
    }

    // Only the ICH9 EHCI takes a companion in QEMU, and the companion must
    // precede the devices as the master does: QEMU resolves bus= against
    // what is already on the command line, and the OHCI registers its ports
    // with the EHCI when it is created. usb_version=1 goes on the HID
    // device alone: usb-storage has no such property.
    [Fact]
    public void AppendUsbKeyboardArgs_EmitsTheCompanionBetweenTheMasterAndTheDevices()
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbController = UsbHostControllerModel.EhciWithOhciCompanion,
            UsbDeviceVersion = QemuLauncher.FullSpeedUsbDeviceVersion,
            KeyboardDevice = "usb-kbd",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Usb }]
        };
        StringBuilder args = new();
        Assert.True(QemuLauncher.AppendUsbKeyboardArgs(args, options, QemuLauncher.AppendStorageArgs(args, options)));

        string text = args.ToString();
        Assert.StartsWith(" -device ich9-usb-ehci1,id=usbehci0 -device pci-ohci,id=usbohci0,masterbus=usbehci0.0,firstport=0,num-ports=6 -drive ", text, StringComparison.Ordinal);
        Assert.Equal(text.IndexOf("ich9-usb-ehci1", StringComparison.Ordinal), text.LastIndexOf("ich9-usb-ehci1", StringComparison.Ordinal));
        Assert.Equal(text.IndexOf("pci-ohci", StringComparison.Ordinal), text.LastIndexOf("pci-ohci", StringComparison.Ordinal));
        Assert.DoesNotContain(" -device usb-ehci,", text);
        Assert.Contains(" -device usb-storage,drive=usbdisk0,bus=usbehci0.0,id=usbstick0,port=3", text);
        Assert.Contains(" -device usb-kbd,bus=usbehci0.0,id=usbkbd0,port=1,usb_version=1", text);
        Assert.Equal(text.IndexOf("usb_version", StringComparison.Ordinal), text.LastIndexOf("usb_version", StringComparison.Ordinal));
    }

    // usb_version=1 leaves a HID device with its full-speed descriptor set
    // alone; on the xHCI it lands on a USB 2 port as the default one does,
    // and the default version is QEMU's own, left unsaid.
    [Fact]
    public void AppendUsbKeyboardArgs_AddsUsbVersionOneWhenAsked()
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbDeviceVersion = QemuLauncher.FullSpeedUsbDeviceVersion,
            KeyboardDevice = "usb-kbd",
            MouseDevice = "usb-mouse"
        };
        StringBuilder args = new();
        QemuLauncher.AppendUsbMouseArgs(args, options, QemuLauncher.AppendUsbKeyboardArgs(args, options, false));

        Assert.Equal(
            " -device qemu-xhci,id=usbxhci0 -device usb-kbd,bus=usbxhci0.0,id=usbkbd0,usb_version=1 -device usb-mouse,bus=usbxhci0.0,id=usbmouse0,usb_version=1",
            args.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void AppendUsbKeyboardArgs_RejectsAUsbVersionQemuDoesNotTake(int version)
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbDeviceVersion = version,
            KeyboardDevice = "usb-kbd"
        };
        StringBuilder args = new();

        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendUsbKeyboardArgs(args, options, false));
    }

    // The hub goes on root port 1 right after the controller, before any
    // device, and the keyboard behind it on the hub's port 1; a stick keeps
    // its own root port. The hub is emitted with the controller, so once.
    [Fact]
    public void AppendUsbKeyboardArgs_PutsTheHubOnPortOneAndTheKeyboardBehindIt()
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbController = UsbHostControllerModel.Ohci,
            UsbHub = true,
            KeyboardDevice = "usb-kbd",
            Disks = [new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Usb }]
        };
        StringBuilder args = new();
        Assert.True(QemuLauncher.AppendUsbKeyboardArgs(args, options, QemuLauncher.AppendStorageArgs(args, options)));

        string text = args.ToString();
        Assert.StartsWith(" -device pci-ohci,id=usbohci0,num-ports=6 -device usb-hub,bus=usbohci0.0,id=usbhub0,port=1 -drive ", text, StringComparison.Ordinal);
        Assert.Equal(text.IndexOf("usb-hub", StringComparison.Ordinal), text.LastIndexOf("usb-hub", StringComparison.Ordinal));
        Assert.Contains(" -device usb-storage,drive=usbdisk0,bus=usbohci0.0,id=usbstick0,port=3", text);
        Assert.Contains(" -device usb-kbd,bus=usbohci0.0,id=usbkbd0,port=1.1", text);
    }

    // On the companion model the hub sits on the EHCI's bus, where its full
    // speed routes it to the OHCI.
    [Fact]
    public void AppendUsbKeyboardArgs_TakesAHubOnTheCompanionModel()
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbController = UsbHostControllerModel.EhciWithOhciCompanion,
            UsbHub = true,
            KeyboardDevice = "usb-kbd"
        };
        StringBuilder args = new();
        QemuLauncher.AppendUsbKeyboardArgs(args, options, false);

        Assert.Contains(" -device usb-hub,bus=usbehci0.0,id=usbhub0,port=1 -device usb-kbd,bus=usbehci0.0,id=usbkbd0,port=1.1", args.ToString());
    }

    // The hub is full speed only: the xHCI's port 1 is a USB 3 port and a
    // standalone EHCI has high-speed ports alone, so QEMU would refuse the
    // attach at startup; the launcher refuses first, with the reason.
    [Theory]
    [InlineData(UsbHostControllerModel.Xhci)]
    [InlineData(UsbHostControllerModel.Ehci)]
    public void AppendUsbKeyboardArgs_RefusesAHubOnAHighSpeedOnlyController(UsbHostControllerModel model)
    {
        QemuLaunchOptions options = new()
        {
            Architecture = "x64",
            IsoPath = "/tmp/kernel.iso",
            UsbController = model,
            UsbHub = true,
            KeyboardDevice = "usb-kbd"
        };
        StringBuilder args = new();

        ArgumentException ex = Assert.Throws<ArgumentException>(() => QemuLauncher.AppendUsbKeyboardArgs(args, options, false));
        Assert.Contains("usb-hub", ex.Message);
    }

    // The engine's monitor replugs a device on the bus and the port the
    // command line used, which these name; the xHCI picks its own ports.
    [Theory]
    [InlineData(UsbHostControllerModel.Xhci, "usbxhci0", null, null, null)]
    [InlineData(UsbHostControllerModel.Ehci, "usbehci0", "1", "2", "3")]
    [InlineData(UsbHostControllerModel.Ohci, "usbohci0", "1", "2", "3")]
    [InlineData(UsbHostControllerModel.EhciWithOhciCompanion, "usbehci0", "1", "2", "3")]
    public void UsbControllerId_NamesTheControllerAndThePortsTheDevicesSitOn(UsbHostControllerModel model, string id, string? keyboardPort, string? mousePort, string? firstDiskPort)
    {
        Assert.Equal(id, QemuLauncher.UsbControllerId(model));
        Assert.Equal($"{id}.0", QemuLauncher.UsbBus(model));
        Assert.Equal(keyboardPort, QemuLauncher.UsbKeyboardPortOf(model, hub: false));
        Assert.Equal(mousePort, QemuLauncher.UsbMousePortOf(model));
        Assert.Equal(firstDiskPort, QemuLauncher.UsbDiskPortOf(model, 0));
    }

    [Fact]
    public void UsbKeyboardPortOf_PutsTheKeyboardBehindTheHubWhenAsked()
    {
        Assert.Equal("1.1", QemuLauncher.UsbKeyboardPortOf(UsbHostControllerModel.Ohci, hub: true));
        Assert.Equal("5", QemuLauncher.UsbDiskPortOf(UsbHostControllerModel.Ohci, 2));
        Assert.Null(QemuLauncher.UsbKeyboardPortOf(UsbHostControllerModel.Xhci, hub: true));
    }

    [Theory]
    [InlineData("xhci", UsbHostControllerModel.Xhci)]
    [InlineData("EHCI", UsbHostControllerModel.Ehci)]
    [InlineData(" ohci ", UsbHostControllerModel.Ohci)]
    [InlineData("ehci-ohci", UsbHostControllerModel.EhciWithOhciCompanion)]
    public void TryParseUsbController_ReadsTheFourSpellings(string name, UsbHostControllerModel expected)
    {
        Assert.True(QemuLauncher.TryParseUsbController(name, out UsbHostControllerModel model));
        Assert.Equal(expected, model);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("uhci")]
    [InlineData("ohci-ehci")]
    public void TryParseUsbController_RejectsAnyOtherSpelling(string? name)
    {
        Assert.False(QemuLauncher.TryParseUsbController(name, out UsbHostControllerModel model));
        Assert.Equal(UsbHostControllerModel.Xhci, model);
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

    [Fact]
    public void AppendNetworkCardArgs_PutsHostForwardsOnTheCardsBackend()
    {
        StringBuilder args = new();
        QemuLauncher.AppendNetworkCardArgs(args, "e1000e", ["tcp::2323-:23", "udp:127.0.0.1:5000-10.0.2.15:53"]);
        Assert.Equal(
            " -netdev user,id=net0,hostfwd=tcp::2323-:23,hostfwd=udp:127.0.0.1:5000-10.0.2.15:53 -device e1000e,netdev=net0",
            args.ToString());
    }

    [Fact]
    public void AppendNetworkCardArgs_RejectsHostForwardsWithoutACard()
    {
        StringBuilder args = new();

        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendNetworkCardArgs(args, "none", ["tcp::2323-:23"]));
    }

    [Theory]
    [InlineData(":2323-:23")] // the host address field is missing
    [InlineData("sctp::2323-:23")]
    [InlineData("tcp::2323-:23,hostfwd=tcp::1-:2")] // a comma would add backend options
    [InlineData("tcp::2323-:23 -device rm")] // whitespace injects new argv tokens
    [InlineData("tcp::0-:23")]
    [InlineData("tcp::2323-:65536")]
    [InlineData("tcp::２３２３-:23")] // non-ASCII digits
    public void AppendHostForwards_RejectsAnythingButAWholeRule(string forward)
    {
        StringBuilder args = new();

        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendHostForwards(args, [forward]));
    }

    [Fact]
    public void AppendHostForwards_TakesARuleWithoutAProtocol()
    {
        StringBuilder args = new();
        QemuLauncher.AppendHostForwards(args, [" ::8080-:80 "]);
        Assert.Equal(",hostfwd=::8080-:80", args.ToString());
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

    [Theory]
    [InlineData("intel-hda")]
    [InlineData("ich9-intel-hda")]
    public void AppendAudioDevice_EmitsControllerAndCodecForAModel(string model)
    {
        StringBuilder args = new();
        QemuLauncher.AppendAudioDevice(args, model);
        Assert.Equal($" -device {model} -device hda-duplex", args.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("none")]
    [InlineData("NONE")]
    public void AppendAudioDevice_AddsNothingWhenUnsetOrNone(string? model)
    {
        StringBuilder args = new();
        QemuLauncher.AppendAudioDevice(args, model);
        Assert.Equal(string.Empty, args.ToString());
    }

    [Fact]
    public void AppendAudioDevice_RejectsCharactersOutsideOptionAlphabet()
    {
        StringBuilder args = new();
        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendAudioDevice(args, "intel-hda -device rm"));
    }

    // A headless run has no audio server, so QEMU's default backend opens no
    // voice; a named backend is wired to the codec by id instead.
    [Fact]
    public void AppendAudioDevice_WiresTheCodecToANamedBackend()
    {
        StringBuilder args = new();
        QemuLauncher.AppendAudioDevice(args, "intel-hda", "none");
        Assert.Equal(
            $" -audiodev none,id={QemuLauncher.AudioBackendId} -device intel-hda -device hda-duplex,audiodev={QemuLauncher.AudioBackendId}",
            args.ToString());
    }

    [Fact]
    public void AppendAudioDevice_IgnoresTheBackendWithoutAController()
    {
        StringBuilder args = new();
        QemuLauncher.AppendAudioDevice(args, "none", "none");
        Assert.Equal(string.Empty, args.ToString());
    }

    [Fact]
    public void AppendAudioDevice_RejectsABackendOutsideOptionAlphabet()
    {
        StringBuilder args = new();
        Assert.Throws<ArgumentException>(() => QemuLauncher.AppendAudioDevice(args, "intel-hda", "none -device rm"));
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

    // The CPU count rides its own switch: one CPU is QEMU's default and stays
    // implicit, so a cell without the smp modifier launches as it always did.
    [Theory]
    [InlineData("x64")]
    [InlineData("arm64")]
    public void AppendCpuCount_EmitsSmpOnlyPastOneCpu(string architecture)
    {
        StringBuilder one = new();
        StringBuilder four = new();
        QemuLaunchOptions oneCpu = new() { Architecture = architecture, IsoPath = "/tmp/kernel.iso" };
        QemuLaunchOptions fourCpus = new() { Architecture = architecture, IsoPath = "/tmp/kernel.iso", CpuCount = 4 };
        if (architecture == "x64")
        {
            QemuLauncher.AppendX64Args(one, oneCpu);
            QemuLauncher.AppendX64Args(four, fourCpus);
        }
        else
        {
            QemuLauncher.AppendArm64Args(one, oneCpu);
            QemuLauncher.AppendArm64Args(four, fourCpus);
        }

        Assert.DoesNotContain("-smp", one.ToString());
        Assert.Contains(" -smp 4 ", four.ToString());
    }

    [Fact]
    public void AppendCpuCount_RejectsAGuestWithoutCpus()
    {
        StringBuilder args = new();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            QemuLauncher.AppendX64Args(args, new QemuLaunchOptions { Architecture = "x64", IsoPath = "/tmp/kernel.iso", CpuCount = 0 }));
    }
}
