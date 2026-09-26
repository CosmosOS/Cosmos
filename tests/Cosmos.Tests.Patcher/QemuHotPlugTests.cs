// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Hosts;
using Cosmos.Tools.Launcher;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// What <see cref="QemuHotPlug"/> sends QEMU for each guest request, checked
/// against a fake QMP monitor that answers the way QEMU 10.2 does and
/// records every command.
/// </summary>
public class QemuHotPlugTests
{
    /// <summary>Longest a test waits on the monitor before it fails instead of hanging.</summary>
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(10);

    /// <summary>The Drivers suite's usb-hid profile: a mouse, then a tablet.</summary>
    private static readonly string[] s_mouseAndTablet = ["usb-mouse", "usb-tablet"];

    [Fact]
    public void For_IsNullWithNoUsbStickAndNoUsbDevice()
    {
        Assert.Null(QemuHotPlug.For([], []));
        Assert.Null(QemuHotPlug.For([new DiskAttachment { Path = "/tmp/a.img", Kind = DiskKind.Ahci }], []));
    }

    [Fact]
    public async Task For_ListensForUsbDevicesWithoutAStick()
    {
        await using QemuHotPlug? hotPlug = QemuHotPlug.For([], s_mouseAndTablet);

        Assert.NotNull(hotPlug);
        Assert.NotEqual(0, hotPlug.Port);
    }

    [Fact]
    public async Task DispatchAsync_DropsARequestWhenThereIsNothingToPlug()
    {
        Assert.False(await QemuHotPlug.DispatchAsync(null, "usb-unplug", CancellationToken.None));
    }

    // The storage suite's hot-plug: the stick goes by the id the launcher
    // gave it, and comes back as a fresh drive and device, since QEMU may
    // not have released the old ids yet.
    [Fact]
    public async Task Stick_UnplugsByItsLauncherIdAndReplugsUnderFreshIds()
    {
        using CancellationTokenSource cts = new(s_timeout);
        DiskAttachment stick = new() { Path = "/tmp/a,b.img", Kind = DiskKind.Usb };
        await using QemuHotPlug hotPlug = QemuHotPlug.For([stick], []) ?? throw new InvalidOperationException("no hot-plug for a USB stick");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, [], cts.Token);

        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-unplug", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-plug 0", cts.Token));

        string[] expected =
        [
            """{"execute":"qmp_capabilities"}""",
            """{"execute":"device_del","arguments":{"id":"usbstick0"}}""",
            """{"execute":"human-monitor-command","arguments":{"command-line":"drive_add 0 if=none,id=usbdisk0p1,file=/tmp/a,,b.img,format=raw"}}""",
            """{"execute":"device_add","arguments":{"driver":"usb-storage","drive":"usbdisk0p1","bus":"usbxhci0.0","id":"usbstick0p1"}}"""
        ];
        Assert.Equal(expected, qemu.Commands);
    }

    [Fact]
    public async Task Device_UnplugsByItsLauncherIdAndReplugsAsTheSameModelUnderAFreshId()
    {
        using CancellationTokenSource cts = new(s_timeout);
        await using QemuHotPlug hotPlug = QemuHotPlug.For([], s_mouseAndTablet) ?? throw new InvalidOperationException("no hot-plug for USB devices");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, s_mouseAndTablet, cts.Token);

        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-unplug 1", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-plug 1", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-unplug 1", cts.Token));

        string[] expected =
        [
            """{"execute":"qmp_capabilities"}""",
            // The first request looks the startup mice up, before it changes anything.
            """{"execute":"query-mice"}""",
            """{"execute":"device_del","arguments":{"id":"usbdev1"}}""",
            """{"execute":"device_add","arguments":{"driver":"usb-tablet","bus":"usbxhci0.0","id":"usbdev1p1"}}""",
            """{"execute":"device_del","arguments":{"id":"usbdev1p1"}}"""
        ];
        Assert.Equal(expected, qemu.Commands);
    }

    // input-send-event cannot name the mouse (its "device" is a display),
    // so the mouse is made current first. Buttons are a state: the second
    // request, holding none, releases the one the first pressed.
    [Fact]
    public async Task PointerMove_MakesTheMouseCurrentThenSendsTheMovementAndTheButtonChanges()
    {
        using CancellationTokenSource cts = new(s_timeout);
        await using QemuHotPlug hotPlug = QemuHotPlug.For([], s_mouseAndTablet) ?? throw new InvalidOperationException("no hot-plug for USB devices");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, s_mouseAndTablet, cts.Token);

        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 0 5 -3 1", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 0 0 0", cts.Token));

        string[] expected =
        [
            """{"execute":"qmp_capabilities"}""",
            """{"execute":"query-mice"}""",
            """{"execute":"human-monitor-command","arguments":{"command-line":"mouse_set 3"}}""",
            """{"execute":"input-send-event","arguments":{"events":[""" +
                """{"type":"rel","data":{"axis":"x","value":5}},""" +
                """{"type":"rel","data":{"axis":"y","value":-3}},""" +
                """{"type":"btn","data":{"down":true,"button":"left"}}]}}""",
            """{"execute":"human-monitor-command","arguments":{"command-line":"mouse_set 3"}}""",
            """{"execute":"input-send-event","arguments":{"events":[""" +
                """{"type":"rel","data":{"axis":"x","value":0}},""" +
                """{"type":"rel","data":{"axis":"y","value":0}},""" +
                """{"type":"btn","data":{"down":false,"button":"left"}}]}}"""
        ];
        Assert.Equal(expected, qemu.Commands);
    }

    // Plugged back in, the mouse is a new QEMU device with a new input
    // handler, holding no button.
    [Fact]
    public async Task PointerMove_ReachesAReplugedMouseThroughItsNewHandler()
    {
        using CancellationTokenSource cts = new(s_timeout);
        await using QemuHotPlug hotPlug = QemuHotPlug.For([], s_mouseAndTablet) ?? throw new InvalidOperationException("no hot-plug for USB devices");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, s_mouseAndTablet, cts.Token);

        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 0 1 1 2", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-unplug 0", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-plug 0", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 0 4 4", cts.Token));

        string[] expected =
        [
            """{"execute":"device_del","arguments":{"id":"usbdev0"}}""",
            """{"execute":"query-mice"}""",
            """{"execute":"device_add","arguments":{"driver":"usb-mouse","bus":"usbxhci0.0","id":"usbdev0p1"}}""",
            """{"execute":"query-mice"}""",
            """{"execute":"human-monitor-command","arguments":{"command-line":"mouse_set 5"}}""",
            """{"execute":"input-send-event","arguments":{"events":[""" +
                """{"type":"rel","data":{"axis":"x","value":4}},""" +
                """{"type":"rel","data":{"axis":"y","value":4}}]}}"""
        ];
        Assert.Equal(expected, qemu.Commands.Skip(4));
    }

    // Each is reported and dropped, and none reaches QEMU as a change.
    [Theory]
    [InlineData("usb-eject 0")]
    [InlineData("usb-pointer-move 0 1")]
    [InlineData("usb-unplug")]
    [InlineData("usb-device-unplug 2")]
    [InlineData("usb-device-plug 0")]
    [InlineData("usb-pointer-move 1 1 1")]
    public async Task DispatchAsync_DropsARequestItCannotCarryOut(string request)
    {
        using CancellationTokenSource cts = new(s_timeout);
        await using QemuHotPlug hotPlug = QemuHotPlug.For([], s_mouseAndTablet) ?? throw new InvalidOperationException("no hot-plug for USB devices");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, s_mouseAndTablet, cts.Token);

        Assert.False(await QemuHotPlug.DispatchAsync(hotPlug, request, cts.Token));

        Assert.All(qemu.Commands, c => Assert.True(
            c is """{"execute":"qmp_capabilities"}""" or """{"execute":"query-mice"}""",
            $"'{request}' sent {c}"));
    }

    [Fact]
    public async Task PointerMove_IsDroppedWhileTheMouseIsUnplugged()
    {
        using CancellationTokenSource cts = new(s_timeout);
        await using QemuHotPlug hotPlug = QemuHotPlug.For([], s_mouseAndTablet) ?? throw new InvalidOperationException("no hot-plug for USB devices");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, s_mouseAndTablet, cts.Token);

        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-unplug 0", cts.Token));
        Assert.False(await QemuHotPlug.DispatchAsync(hotPlug, "usb-device-unplug 0", cts.Token));
        Assert.False(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 0 1 1", cts.Token));

        Assert.Equal("""{"execute":"device_del","arguments":{"id":"usbdev0"}}""", qemu.Commands[^1]);
    }

    // Two usb-mice: the n-th one by handler index is the n-th in the list.
    [Fact]
    public async Task PointerMove_TellsTwoMiceApartByTheirOrderOnTheCommandLine()
    {
        using CancellationTokenSource cts = new(s_timeout);
        string[] devices = ["usb-kbd", "usb-mouse", "usb-tablet", "usb-mouse"];
        await using QemuHotPlug hotPlug = QemuHotPlug.For([], devices) ?? throw new InvalidOperationException("no hot-plug for USB devices");
        await using FakeQemuMonitor qemu = await FakeQemuMonitor.StartAsync(hotPlug, devices, cts.Token);

        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 3 1 0", cts.Token));
        Assert.True(await QemuHotPlug.DispatchAsync(hotPlug, "usb-pointer-move 1 1 0", cts.Token));

        string[] mouseSets = qemu.Commands.Where(c => c.Contains("mouse_set", StringComparison.Ordinal)).ToArray();
        string[] expected =
        [
            """{"execute":"human-monitor-command","arguments":{"command-line":"mouse_set 6"}}""",
            """{"execute":"human-monitor-command","arguments":{"command-line":"mouse_set 4"}}"""
        ];
        Assert.Equal(expected, mouseSets);
    }

    [Theory]
    [InlineData(0, 1, """[{"type":"rel","data":{"axis":"x","value":5}},{"type":"rel","data":{"axis":"y","value":-3}},{"type":"btn","data":{"down":true,"button":"left"}}]""")]
    [InlineData(3, 4, """[{"type":"rel","data":{"axis":"x","value":5}},{"type":"rel","data":{"axis":"y","value":-3}},{"type":"btn","data":{"down":false,"button":"left"}},{"type":"btn","data":{"down":false,"button":"right"}},{"type":"btn","data":{"down":true,"button":"middle"}}]""")]
    [InlineData(2, 2, """[{"type":"rel","data":{"axis":"x","value":5}},{"type":"rel","data":{"axis":"y","value":-3}}]""")]
    public void PointerEvents_PressesAndReleasesOnlyTheButtonsThatChange(int heldButtons, int buttons, string expected)
    {
        Assert.Equal(expected, QemuHotPlug.PointerEvents(5, -3, heldButtons, buttons).ToJsonString());
    }

    /// <summary>
    /// QEMU's side of the QMP monitor: connects to the hot-plug's port,
    /// greets, records every command as the JSON it was sent as, and answers
    /// as QEMU 10.2 does. Its input handlers mirror QEMU's: a PS/2 mouse
    /// numbered 2, then one per USB device numbered in command-line order,
    /// and one more for each device added; <c>query-mice</c> lists all but
    /// the keyboards'.
    /// </summary>
    private sealed class FakeQemuMonitor : IAsyncDisposable
    {
        private const string UsbMouseName = "QEMU HID Mouse";
        private const string UsbTabletName = "QEMU HID Tablet";
        private const string UsbKeyboardName = "QEMU HID Keyboard";

        private readonly TcpClient _client;
        private readonly List<string> _commands = new();

        /// <summary>Input handlers by QEMU device id, with their index and name.</summary>
        private readonly Dictionary<string, (int Index, string Name)> _handlers = new();

        private int _nextHandler = 3;
        private Task _loop = Task.CompletedTask;

        /// <summary>Every command received so far, oldest first.</summary>
        public IReadOnlyList<string> Commands
        {
            get
            {
                lock (_commands)
                {
                    return _commands.ToArray();
                }
            }
        }

        private FakeQemuMonitor(TcpClient client, IReadOnlyList<string> usbDevices)
        {
            _client = client;
            _handlers["ps2-mouse"] = (2, "QEMU PS/2 Mouse");
            for (int i = 0; i < usbDevices.Count; i++)
            {
                AddHandler(QemuLauncher.UsbDeviceId(i), usbDevices[i]);
            }
        }

        /// <summary>Connects to <paramref name="hotPlug"/> the way QEMU does at startup, and starts answering.</summary>
        public static async Task<FakeQemuMonitor> StartAsync(QemuHotPlug hotPlug, IReadOnlyList<string> usbDevices, CancellationToken cancellationToken)
        {
            hotPlug.Attach(cancellationToken);
            TcpClient client = new();
            await client.ConnectAsync(IPAddress.Loopback, hotPlug.Port, cancellationToken);
            FakeQemuMonitor monitor = new(client, usbDevices);
            monitor._loop = monitor.AnswerAsync(cancellationToken);
            return monitor;
        }

        public async ValueTask DisposeAsync()
        {
            _client.Dispose();
            try
            {
                await _loop;
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or OperationCanceledException)
            {
                // The hot-plug or the test closed the connection.
            }
        }

        private void AddHandler(string deviceId, string model)
        {
            // Every input device takes a number, a keyboard's included,
            // though query-mice leaves keyboards out.
            _handlers[deviceId] = (_nextHandler, model switch
            {
                "usb-mouse" => UsbMouseName,
                "usb-tablet" => UsbTabletName,
                _ => UsbKeyboardName
            });
            _nextHandler++;
        }

        private async Task AnswerAsync(CancellationToken cancellationToken)
        {
            NetworkStream stream = _client.GetStream();
            using StreamReader reader = new(stream, Encoding.UTF8);
            await using StreamWriter writer = new(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { NewLine = "\n", AutoFlush = true };

            await writer.WriteLineAsync("""{"QMP": {"version": {"qemu": {"micro": 2, "minor": 2, "major": 10}, "package": ""}, "capabilities": ["oob"]}}""");
            while (await reader.ReadLineAsync(cancellationToken) is string line)
            {
                lock (_commands)
                {
                    _commands.Add(line);
                }

                JsonObject command = JsonNode.Parse(line)?.AsObject() ?? throw new InvalidDataException($"not a QMP command: {line}");
                string name = (string?)command["execute"] ?? throw new InvalidDataException($"no command name: {line}");
                JsonObject arguments = command["arguments"]?.AsObject() ?? new JsonObject();
                if (name == "device_del")
                {
                    // QEMU reports the removal as an event, which may come
                    // before the command's own reply.
                    _handlers.Remove((string?)arguments["id"] ?? string.Empty);
                    await writer.WriteLineAsync("""{"timestamp": {"seconds": 0, "microseconds": 0}, "event": "DEVICE_DELETED", "data": {"path": "/machine/peripheral/x"}}""");
                }

                JsonNode result = name switch
                {
                    "query-mice" => QueryMice(),
                    "human-monitor-command" => HumanMonitor((string?)arguments["command-line"] ?? string.Empty),
                    "device_add" => AddDevice(arguments),
                    _ => new JsonObject()
                };
                await writer.WriteLineAsync(new JsonObject { ["return"] = result }.ToJsonString());
            }
        }

        private JsonArray QueryMice()
        {
            // QEMU lists the most recently added handler first.
            JsonArray mice = [];
            foreach ((int index, string name) in _handlers.Values.Where(h => h.Name != UsbKeyboardName).OrderByDescending(h => h.Index))
            {
                mice.Add(new JsonObject { ["index"] = index, ["name"] = name, ["current"] = false, ["absolute"] = name == UsbTabletName });
            }

            return mice;
        }

        private JsonNode HumanMonitor(string commandLine)
        {
            if (commandLine.StartsWith("mouse_set ", StringComparison.Ordinal))
            {
                int index = int.Parse(commandLine["mouse_set ".Length..], CultureInfo.InvariantCulture);
                return _handlers.Values.Any(h => h.Index == index && h.Name != UsbKeyboardName) ? string.Empty : $"Mouse at index '{index}' not found\r\n";
            }

            return commandLine.StartsWith("drive_add ", StringComparison.Ordinal) ? "OK\r\n" : "unknown command\r\n";
        }

        private JsonObject AddDevice(JsonObject arguments)
        {
            AddHandler((string?)arguments["id"] ?? string.Empty, (string?)arguments["driver"] ?? string.Empty);
            return new JsonObject();
        }
    }
}
