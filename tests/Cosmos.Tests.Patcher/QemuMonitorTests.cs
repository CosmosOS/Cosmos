// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Hosts;

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
