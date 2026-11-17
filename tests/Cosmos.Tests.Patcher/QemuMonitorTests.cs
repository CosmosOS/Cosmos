// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Hosts;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// The QMP argument builders of the engine's monitor and the monitor every
/// run opens. The builders take the request split on spaces, as
/// <c>QemuMonitor.RunAsync</c> splits it.
/// </summary>
[Collection("PatcherTests")]
public class QemuMonitorTests
{
    private static string[] Words(string request) => request.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void KeyPressArguments_BuildsOneQcode()
    {
        Assert.Equal(
            "{\"keys\":[{\"type\":\"qcode\",\"data\":\"a\"}]}",
            QemuMonitor.KeyPressArguments(["key-press", "a"]).ToJsonString());
    }

    [Theory]
    [InlineData("key-press")]
    [InlineData("key-press a b")]
    public void KeyPressArguments_RejectsOtherWordCounts(string request)
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => QemuMonitor.KeyPressArguments(Words(request)));
        Assert.Equal("the key request takes one key name", ex.Message);
    }

    [Fact]
    public void MouseMoveArguments_BuildsTwoRelativeEvents()
    {
        Assert.Equal(
            "{\"events\":[{\"type\":\"rel\",\"data\":{\"axis\":\"x\",\"value\":10}},{\"type\":\"rel\",\"data\":{\"axis\":\"y\",\"value\":0}}]}",
            QemuMonitor.MouseMoveArguments(["mouse-move", "10", "0"]).ToJsonString());
    }

    [Theory]
    [InlineData("mouse-move 10")]
    [InlineData("mouse-move a 0")]
    public void MouseMoveArguments_RejectsNonIntegers(string request)
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => QemuMonitor.MouseMoveArguments(Words(request)));
        Assert.Equal("the mouse move takes two integers", ex.Message);
    }

    [Theory]
    [InlineData("down", true)]
    [InlineData("up", false)]
    public void MouseButtonArguments_BuildsOneButtonEvent(string state, bool down)
    {
        string expected = down
            ? "{\"events\":[{\"type\":\"btn\",\"data\":{\"down\":true,\"button\":\"left\"}}]}"
            : "{\"events\":[{\"type\":\"btn\",\"data\":{\"down\":false,\"button\":\"left\"}}]}";

        Assert.Equal(expected, QemuMonitor.MouseButtonArguments(["mouse-button", "left", state]).ToJsonString());
    }

    [Fact]
    public void MouseButtonArguments_RejectsOtherStates()
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => QemuMonitor.MouseButtonArguments(Words("mouse-button left pressed")));
        Assert.Equal("the mouse button takes a button name and down or up", ex.Message);
    }

    [Fact]
    public void PciPlugArguments_BuildsADeviceAddBehindThePort()
    {
        Assert.Equal(
            "{\"driver\":\"virtio-blk-pci\",\"drive\":\"vblkdisk0p1\",\"bus\":\"rp0\",\"id\":\"vblk0p1\"}",
            QemuMonitor.PciPlugArguments("vblkdisk0p1", "rp0", "vblk0p1").ToJsonString());
    }

    [Fact]
    public void PciUnplugArguments_BuildsADeviceDel()
    {
        Assert.Equal("{\"id\":\"vblk0\"}", QemuMonitor.PciUnplugArguments("vblk0").ToJsonString());
    }

    // A stick goes back on the bus the command line put it on and, on the
    // EHCI and OHCI, on the root port it left; the xHCI picks its own.
    [Fact]
    public void UsbPlugArguments_BuildsADeviceAddOnTheControllersBus()
    {
        Assert.Equal(
            "{\"driver\":\"usb-storage\",\"drive\":\"usbdisk0p1\",\"bus\":\"usbxhci0.0\",\"id\":\"usbstick0p1\"}",
            QemuMonitor.UsbPlugArguments("usbdisk0p1", "usbxhci0.0", null, "usbstick0p1").ToJsonString());
        Assert.Equal(
            "{\"driver\":\"usb-storage\",\"drive\":\"usbdisk0p1\",\"bus\":\"usbehci0.0\",\"id\":\"usbstick0p1\",\"port\":\"3\"}",
            QemuMonitor.UsbPlugArguments("usbdisk0p1", "usbehci0.0", "3", "usbstick0p1").ToJsonString());
    }

    // The replugged keyboard must be the device the boot-time one was: the
    // same bus, the same port where the controller pins one (behind the hub
    // on the hub cell) and the same usb_version when the launcher set one.
    [Fact]
    public void UsbKeyboardPlugArguments_RepeatsTheBusThePortAndTheVersion()
    {
        Assert.Equal(
            "{\"driver\":\"usb-kbd\",\"bus\":\"usbxhci0.0\",\"id\":\"usbkbd0p1\"}",
            QemuMonitor.UsbKeyboardPlugArguments("usbxhci0.0", null, QemuLauncher.DefaultUsbDeviceVersion, "usbkbd0p1").ToJsonString());
        Assert.Equal(
            "{\"driver\":\"usb-kbd\",\"bus\":\"usbohci0.0\",\"id\":\"usbkbd0p1\",\"port\":\"1.1\"}",
            QemuMonitor.UsbKeyboardPlugArguments("usbohci0.0", "1.1", QemuLauncher.DefaultUsbDeviceVersion, "usbkbd0p1").ToJsonString());
        Assert.Equal(
            "{\"driver\":\"usb-kbd\",\"bus\":\"usbehci0.0\",\"id\":\"usbkbd0p2\",\"port\":\"1\",\"usb_version\":1}",
            QemuMonitor.UsbKeyboardPlugArguments("usbehci0.0", "1", QemuLauncher.FullSpeedUsbDeviceVersion, "usbkbd0p2").ToJsonString());
    }

    // A run on another controller gets a monitor the same way, and what For
    // wired is what the replug arguments, built above, repeat: that
    // controller's bus, the keyboard's port behind the hub and its version.
    // A swapped or dropped argument would otherwise show only at the replug
    // step of a CI cell, minutes in, as QEMU's "Bus 'usbxhci0.0' not found".
    [Fact]
    public async Task For_TakesTheUsbHostControllerOfTheRun()
    {
        await using QemuMonitor monitor = QemuMonitor.For([], "usb-kbd", UsbHostControllerModel.Ohci, QemuLauncher.FullSpeedUsbDeviceVersion, usbHub: true);

        Assert.NotNull(monitor);
        Assert.True(monitor.Port > 0);
        Assert.Equal("usbohci0.0", monitor.UsbBus);
        Assert.Equal("1.1", monitor.KeyboardPort);
        Assert.Equal(QemuLauncher.FullSpeedUsbDeviceVersion, monitor.UsbDeviceVersion);
    }

    // The defaults are the xHCI, which picks its own ports, at QEMU's own
    // device version, left unsaid on the replug.
    [Fact]
    public async Task For_DefaultsToTheXhciWithNoPinnedPort()
    {
        await using QemuMonitor monitor = QemuMonitor.For([], null);

        Assert.Equal("usbxhci0.0", monitor.UsbBus);
        Assert.Null(monitor.KeyboardPort);
        Assert.Equal(QemuLauncher.DefaultUsbDeviceVersion, monitor.UsbDeviceVersion);
    }

    // The input requests need a monitor on a cell with no USB device, so a
    // run attaching nothing gets one too, listening before QEMU is launched.
    [Fact]
    public async Task For_OpensAMonitorForEveryRun()
    {
        await using QemuMonitor monitor = QemuMonitor.For([], null);

        Assert.NotNull(monitor);
        Assert.True(monitor.Port > 0);
    }
}
