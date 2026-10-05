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
using Cosmos.TestRunner.Protocol;
using Cosmos.Tools.Launcher;

namespace Cosmos.TestRunner.Engine.Hosts;

/// <summary>
/// QEMU's QMP monitor for one run: plugs the profile's USB sticks and USB
/// keyboard in and out and injects keys and pointer events into the running
/// guest when one of its tests asks (<see cref="Ds2Vs.HostRequest"/>). QEMU
/// connects the monitor to <see cref="Port"/> at startup, so an instance
/// exists, and listens, before QEMU is launched; every run has one.
/// </summary>
public sealed class QemuMonitor : IAsyncDisposable
{
    /// <summary>Pulls stick n (0 when omitted) off the xHCI controller.</summary>
    public const string UsbUnplugRequest = "usb-unplug";

    /// <summary>Plugs stick n (0 when omitted) back in, on the same image.</summary>
    public const string UsbPlugRequest = "usb-plug";

    /// <summary>Pulls the profile's USB keyboard off the xHCI controller.</summary>
    public const string UsbKeyboardUnplugRequest = "usb-kbd-unplug";

    /// <summary>Plugs it back in.</summary>
    public const string UsbKeyboardPlugRequest = "usb-kbd-plug";

    /// <summary>
    /// Presses and releases one key: <c>key-press &lt;qcode&gt;</c> with a QEMU
    /// QKeyCode name such as <c>a</c>, <c>ret</c>, <c>spc</c>; QEMU releases
    /// after its default hold time of 100 ms.
    /// </summary>
    public const string KeyPressRequest = "key-press";

    /// <summary>
    /// Moves the pointer: <c>mouse-move &lt;dx&gt; &lt;dy&gt;</c> in device
    /// units, one <c>input-send-event</c> with two relative events and one sync.
    /// </summary>
    public const string MouseMoveRequest = "mouse-move";

    /// <summary>Presses or releases a pointer button: <c>mouse-button &lt;left|middle|right&gt; &lt;down|up&gt;</c>.</summary>
    public const string MouseButtonRequest = "mouse-button";

    private readonly TcpListener _listener;
    private readonly IReadOnlyList<DiskAttachment> _sticks;

    /// <summary>Whether the profile attaches a USB keyboard at all.</summary>
    private readonly bool _keyboard;

    /// <summary>QEMU id of each stick's device, null while it is unplugged.</summary>
    private readonly string?[] _deviceIds;

    /// <summary>QEMU id of the keyboard's device, null while it is unplugged.</summary>
    private string? _keyboardId;

    /// <summary>Numbers the devices and drives of each plug, whose ids QEMU may not have released yet.</summary>
    private int _plugCount;

    /// <summary>Numbers the keyboard's plugs, for the same reason.</summary>
    private int _keyboardPlugCount;

    private Task? _connected;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    /// <summary>Port QEMU connects its monitor to (<see cref="QemuLaunchOptions.MonitorPort"/>).</summary>
    public int Port { get; }

    private QemuMonitor(IReadOnlyList<DiskAttachment> sticks, bool keyboard)
    {
        _sticks = sticks;
        _keyboard = keyboard;
        _keyboardId = keyboard ? QemuLauncher.UsbKeyboardId : null;
        _deviceIds = new string?[sticks.Count];
        for (int i = 0; i < sticks.Count; i++)
        {
            _deviceIds[i] = QemuLauncher.UsbDeviceId(i);
        }

        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    }

    /// <summary>
    /// An instance for a run attaching <paramref name="disks"/> and the
    /// keyboard model <paramref name="keyboardDevice"/>. The USB sticks among
    /// the disks and a <see cref="QemuLauncher.UsbKeyboardModel"/> keyboard are
    /// what the hot-plug requests act on; the input requests need no device,
    /// so a run with neither gets a monitor too.
    /// </summary>
    public static QemuMonitor For(IReadOnlyList<DiskAttachment> disks, string? keyboardDevice)
    {
        List<DiskAttachment> sticks = disks.Where(d => d.Kind == DiskKind.Usb).ToList();
        bool keyboard = string.Equals(keyboardDevice, QemuLauncher.UsbKeyboardModel, StringComparison.OrdinalIgnoreCase);
        return new QemuMonitor(sticks, keyboard);
    }

    /// <summary>Takes QEMU's monitor connection. Called once QEMU was started.</summary>
    public void Attach(CancellationToken cancellationToken) => _connected ??= ConnectAsync(cancellationToken);

    /// <summary>
    /// Carries out one request of the guest. A request that fails is
    /// reported and dropped: the guest's test then times out waiting for
    /// the change, and fails with that.
    /// </summary>
    public static async Task DispatchAsync(QemuMonitor? monitor, string request, CancellationToken cancellationToken)
    {
        if (monitor is null)
        {
            Console.WriteLine($"[Monitor] Ignored '{request}': the run has no monitor");
            return;
        }

        try
        {
            await monitor.RunAsync(request, cancellationToken);
            Console.WriteLine($"[Monitor] {request}: done");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"[Monitor] {request} failed: {ex.Message}");
        }
    }

    /// <summary>
    /// The <c>send-key</c> arguments of a <see cref="KeyPressRequest"/>:
    /// one QKeyCode key, <c>{"keys":[{"type":"qcode","data":"a"}]}</c>.
    /// </summary>
    /// <param name="words">The request split on spaces: the request name and the key name.</param>
    public static JsonObject KeyPressArguments(string[] words)
    {
        if (words.Length != 2)
        {
            throw new InvalidOperationException("the key request takes one key name");
        }

        return new JsonObject
        {
            ["keys"] = new JsonArray(new JsonObject { ["type"] = "qcode", ["data"] = words[1] })
        };
    }

    /// <summary>
    /// The <c>input-send-event</c> arguments of a <see cref="MouseMoveRequest"/>:
    /// a relative x event and a relative y event,
    /// <c>{"events":[{"type":"rel","data":{"axis":"x","value":10}},{"type":"rel","data":{"axis":"y","value":0}}]}</c>.
    /// </summary>
    /// <param name="words">The request split on spaces: the request name, dx and dy as integers.</param>
    public static JsonObject MouseMoveArguments(string[] words)
    {
        if (words.Length != 3
            || !int.TryParse(words[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dx)
            || !int.TryParse(words[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dy))
        {
            throw new InvalidOperationException("the mouse move takes two integers");
        }

        return new JsonObject
        {
            ["events"] = new JsonArray(RelativeEvent("x", dx), RelativeEvent("y", dy))
        };
    }

    /// <summary>
    /// The <c>input-send-event</c> arguments of a <see cref="MouseButtonRequest"/>:
    /// one button event, <c>{"events":[{"type":"btn","data":{"down":true,"button":"left"}}]}</c>.
    /// The button name passes through; QEMU rejects one it does not know
    /// (its names are <c>left</c>, <c>middle</c>, <c>right</c>, <c>wheel-up</c>,
    /// <c>wheel-down</c>, <c>side</c>, <c>extra</c>).
    /// </summary>
    /// <param name="words">The request split on spaces: the request name, the button name and <c>down</c> or <c>up</c>.</param>
    public static JsonObject MouseButtonArguments(string[] words)
    {
        if (words.Length != 3 || (words[2] != "down" && words[2] != "up"))
        {
            throw new InvalidOperationException("the mouse button takes a button name and down or up");
        }

        return new JsonObject
        {
            ["events"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "btn",
                    ["data"] = new JsonObject { ["down"] = words[2] == "down", ["button"] = words[1] }
                })
        };
    }

    /// <summary>One relative pointer event on <paramref name="axis"/> by <paramref name="value"/> units.</summary>
    private static JsonObject RelativeEvent(string axis, int value)
    {
        return new JsonObject
        {
            ["type"] = "rel",
            ["data"] = new JsonObject { ["axis"] = axis, ["value"] = value }
        };
    }

    /// <summary>
    /// Stops listening, waits for the monitor connection attempt to settle,
    /// then closes the connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // QEMU is gone by now. If it exited before connecting its monitor,
        // only stopping the listener ends the accept: the token it waits on
        // belongs to a source the host disposed without cancelling.
        _listener.Stop();

        if (_connected is not null)
        {
            try
            {
                await _connected;
            }
            catch (Exception)
            {
                // Reported by the request that needed the monitor, if any.
            }
        }

        _client?.Dispose();
    }

    private async Task RunAsync(string request, CancellationToken cancellationToken)
    {
        string[] words = request.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            throw new InvalidOperationException("unknown request");
        }

        await (_connected ?? throw new InvalidOperationException("QEMU's monitor was never attached"));
        switch (words[0])
        {
            case UsbUnplugRequest:
                await UnplugAsync(StickIndex(words), cancellationToken);
                break;
            case UsbPlugRequest:
                await PlugAsync(StickIndex(words), cancellationToken);
                break;
            case UsbKeyboardUnplugRequest:
                RequireKeyboard(words);
                await UnplugKeyboardAsync(cancellationToken);
                break;
            case UsbKeyboardPlugRequest:
                RequireKeyboard(words);
                await PlugKeyboardAsync(cancellationToken);
                break;
            case KeyPressRequest:
                await ExecuteAsync("send-key", KeyPressArguments(words), cancellationToken);
                break;
            case MouseMoveRequest:
                await ExecuteAsync("input-send-event", MouseMoveArguments(words), cancellationToken);
                break;
            case MouseButtonRequest:
                await ExecuteAsync("input-send-event", MouseButtonArguments(words), cancellationToken);
                break;
            default:
                throw new InvalidOperationException("unknown request");
        }
    }

    /// <summary>The stick a request names: its second word, 0 when omitted, below the number of sticks.</summary>
    private int StickIndex(string[] words)
    {
        int index = 0;
        if (words.Length > 2 || (words.Length == 2 && !int.TryParse(words[1], out index)) || index < 0 || index >= _sticks.Count)
        {
            throw new InvalidOperationException($"no such USB stick (the profile attaches {_sticks.Count})");
        }

        return index;
    }

    /// <summary>Checks a keyboard request against the run: no index, and a profile with the keyboard.</summary>
    private void RequireKeyboard(string[] words)
    {
        if (words.Length > 1)
        {
            throw new InvalidOperationException("the keyboard request takes no index");
        }

        if (!_keyboard)
        {
            throw new InvalidOperationException("the profile attaches no USB keyboard");
        }
    }

    private async Task UnplugAsync(int index, CancellationToken cancellationToken)
    {
        string deviceId = _deviceIds[index] ?? throw new InvalidOperationException($"USB stick {index} is already unplugged");

        // QEMU deletes the stick's drive with it, as it does for any drive
        // declared with -drive or drive_add.
        await ExecuteAsync("device_del", new JsonObject { ["id"] = deviceId }, cancellationToken);
        _deviceIds[index] = null;
    }

    private async Task PlugAsync(int index, CancellationToken cancellationToken)
    {
        if (_deviceIds[index] is not null)
        {
            throw new InvalidOperationException($"USB stick {index} is already plugged in");
        }

        _plugCount++;
        string driveId = $"{QemuLauncher.UsbDriveId(index)}p{_plugCount}";
        string deviceId = $"{QemuLauncher.UsbDeviceId(index)}p{_plugCount}";

        // A drive like the one on the command line, so the next unplug
        // deletes it the same way. QMP's own blockdev-add would leave the
        // image open, and locked, after the device is gone.
        string path = _sticks[index].Path.Replace(",", ",,");
        JsonNode? added = await ExecuteAsync(
            "human-monitor-command",
            new JsonObject { ["command-line"] = $"drive_add 0 if=none,id={driveId},file={path},format=raw" },
            cancellationToken);
        string output = added?.GetValue<string>().Trim() ?? string.Empty;
        if (output != "OK")
        {
            throw new InvalidOperationException($"drive_add: {output}");
        }

        await ExecuteAsync(
            "device_add",
            new JsonObject
            {
                ["driver"] = "usb-storage",
                ["drive"] = driveId,
                ["bus"] = $"{QemuLauncher.UsbControllerId}.0",
                ["id"] = deviceId
            },
            cancellationToken);
        _deviceIds[index] = deviceId;
    }

    private async Task UnplugKeyboardAsync(CancellationToken cancellationToken)
    {
        string deviceId = _keyboardId ?? throw new InvalidOperationException("the USB keyboard is already unplugged");
        await ExecuteAsync("device_del", new JsonObject { ["id"] = deviceId }, cancellationToken);
        _keyboardId = null;
    }

    private async Task PlugKeyboardAsync(CancellationToken cancellationToken)
    {
        if (_keyboardId is not null)
        {
            throw new InvalidOperationException("the USB keyboard is already plugged in");
        }

        _keyboardPlugCount++;
        string deviceId = $"{QemuLauncher.UsbKeyboardId}p{_keyboardPlugCount}";

        // No drive: the keyboard is the device alone, on the same root hub
        // the command line put it on.
        await ExecuteAsync(
            "device_add",
            new JsonObject
            {
                ["driver"] = QemuLauncher.UsbKeyboardModel,
                ["bus"] = $"{QemuLauncher.UsbControllerId}.0",
                ["id"] = deviceId
            },
            cancellationToken);
        _keyboardId = deviceId;
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _client = await _listener.AcceptTcpClientAsync(cancellationToken);
        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { NewLine = "\n" };

        // QEMU greets first, then takes commands once capabilities are negotiated.
        _ = await _reader.ReadLineAsync(cancellationToken) ?? throw new IOException("QEMU closed its monitor");
        await ExecuteAsync("qmp_capabilities", null, cancellationToken);
    }

    /// <summary>Runs one QMP command and returns what it returned; a QMP error throws.</summary>
    private async Task<JsonNode?> ExecuteAsync(string command, JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (_reader is null || _writer is null)
        {
            throw new InvalidOperationException("QEMU's monitor was never attached");
        }

        JsonObject message = new() { ["execute"] = command };
        if (arguments is not null)
        {
            message["arguments"] = arguments;
        }

        await _writer.WriteLineAsync(message.ToJsonString().AsMemory(), cancellationToken);
        await _writer.FlushAsync(cancellationToken);

        while (true)
        {
            string line = await _reader.ReadLineAsync(cancellationToken) ?? throw new IOException("QEMU closed its monitor");
            if (JsonNode.Parse(line) is not JsonObject reply)
            {
                continue;
            }

            if (reply.TryGetPropertyValue("error", out JsonNode? error))
            {
                throw new InvalidOperationException($"{command}: {error?["desc"]}");
            }

            if (reply.TryGetPropertyValue("return", out JsonNode? result))
            {
                return result;
            }

            // Anything else is an event (DEVICE_DELETED, ...), which nothing waits for.
        }
    }
}
