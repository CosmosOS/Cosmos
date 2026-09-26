// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
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
/// Carries out what the running guest's tests ask of the machine under them
/// (<see cref="Ds2Vs.HostRequest"/>, parsed by <see cref="HostRequest"/>):
/// plugging the profile's USB sticks and <c>"usb"</c> devices in and out,
/// and moving its USB mice. It goes through QEMU's QMP monitor, which QEMU
/// connects to <see cref="Port"/> at startup, so an instance exists, and
/// listens, before QEMU is launched. One instance serves one QEMU run.
/// </summary>
public sealed class QemuHotPlug : IAsyncDisposable
{
    /// <summary>The <c>"usb"</c> model <see cref="HostRequestKind.PointerMove"/> drives: the one relative pointer.</summary>
    private const string UsbMouseModel = "usb-mouse";

    /// <summary>
    /// Name <c>query-mice</c> gives the input handler of every <c>usb-mouse</c>
    /// (QEMU's hw/input/hid.c), and no other device's.
    /// </summary>
    private const string UsbMouseHandlerName = "QEMU HID Mouse";

    /// <summary>QMP's name of each button bit of <see cref="HostRequest.Buttons"/>.</summary>
    private static readonly (int Bit, string Name)[] s_buttonNames =
    [
        (HostRequest.LeftButton, "left"),
        (HostRequest.RightButton, "right"),
        (HostRequest.MiddleButton, "middle")
    ];

    private readonly TcpListener _listener;
    private readonly IReadOnlyList<DiskAttachment> _sticks;
    private readonly IReadOnlyList<string> _devices;

    /// <summary>QEMU id of each stick's device, null while it is unplugged.</summary>
    private readonly string?[] _stickIds;

    /// <summary>QEMU id of each <c>"usb"</c> device, null while it is unplugged.</summary>
    private readonly string?[] _deviceIds;

    /// <summary>
    /// <c>query-mice</c> index of each <c>"usb"</c> device's input handler:
    /// null for a model other than <c>usb-mouse</c>, while it is unplugged,
    /// or when QEMU's mice could not be matched to the profile's.
    /// </summary>
    private readonly int?[] _mouseHandlers;

    /// <summary>Buttons each <c>"usb"</c> device holds since the last pointer request, none once plugged in.</summary>
    private readonly int[] _heldButtons;

    /// <summary>Numbers the devices and drives of each plug, whose ids QEMU may not have released yet.</summary>
    private int _plugCount;

    /// <summary>Set once the first request looked up the mice QEMU was started with.</summary>
    private bool _startupMiceMatched;

    private Task? _connected;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    /// <summary>Port QEMU connects its monitor to (<see cref="QemuLaunchOptions.MonitorPort"/>).</summary>
    public int Port { get; }

    private QemuHotPlug(IReadOnlyList<DiskAttachment> sticks, IReadOnlyList<string> devices)
    {
        _sticks = sticks;
        _stickIds = new string?[sticks.Count];
        for (int i = 0; i < sticks.Count; i++)
        {
            _stickIds[i] = QemuLauncher.UsbStickId(i);
        }

        _devices = devices;
        _deviceIds = new string?[devices.Count];
        for (int i = 0; i < devices.Count; i++)
        {
            _deviceIds[i] = QemuLauncher.UsbDeviceId(i);
        }

        _mouseHandlers = new int?[devices.Count];
        _heldButtons = new int[devices.Count];

        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    }

    /// <summary>
    /// An instance for a run attaching <paramref name="disks"/> and the
    /// <c>"usb"</c> models <paramref name="usbDevices"/>, or null when there
    /// is neither a USB stick nor a USB device, so nothing can be plugged.
    /// </summary>
    public static QemuHotPlug? For(IReadOnlyList<DiskAttachment> disks, IReadOnlyList<string> usbDevices)
    {
        List<DiskAttachment> sticks = disks.Where(d => d.Kind == DiskKind.Usb).ToList();
        return sticks.Count == 0 && usbDevices.Count == 0 ? null : new QemuHotPlug(sticks, usbDevices);
    }

    /// <summary>Takes QEMU's monitor connection. Called once QEMU was started.</summary>
    public void Attach(CancellationToken cancellationToken) => _connected ??= ConnectAsync(cancellationToken);

    /// <summary>
    /// Carries out one request of the guest. A request that is malformed,
    /// names a stick or device the profile lacks, or fails is reported and
    /// dropped: the guest's test then times out waiting for the change, and
    /// fails with that.
    /// </summary>
    /// <returns>True when the request was carried out.</returns>
    public static async Task<bool> DispatchAsync(QemuHotPlug? hotPlug, string request, CancellationToken cancellationToken)
    {
        if (hotPlug is null)
        {
            Console.WriteLine($"[HotPlug] Ignored '{request}': the profile attaches no USB stick or USB device");
            return false;
        }

        try
        {
            await hotPlug.RunAsync(HostRequest.Parse(request), cancellationToken);
            Console.WriteLine($"[HotPlug] {request}: done");
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"[HotPlug] {request} failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// The <c>input-send-event</c> events of one pointer request: the
    /// movement on both axes, then a press or a release for each button
    /// whose state changes from <paramref name="heldButtons"/> to
    /// <paramref name="buttons"/>. QEMU folds the whole batch into the
    /// device's next report, so a button pressed and released in one batch
    /// would never reach the guest: a click takes two requests.
    /// </summary>
    internal static JsonArray PointerEvents(int deltaX, int deltaY, int heldButtons, int buttons)
    {
        // Both axes always, 0 included, so the batch is never empty.
        JsonArray events =
        [
            new JsonObject { ["type"] = "rel", ["data"] = new JsonObject { ["axis"] = "x", ["value"] = deltaX } },
            new JsonObject { ["type"] = "rel", ["data"] = new JsonObject { ["axis"] = "y", ["value"] = deltaY } }
        ];

        foreach ((int bit, string name) in s_buttonNames)
        {
            if (((heldButtons ^ buttons) & bit) != 0)
            {
                events.Add(new JsonObject
                {
                    ["type"] = "btn",
                    ["data"] = new JsonObject { ["down"] = (buttons & bit) != 0, ["button"] = name }
                });
            }
        }

        return events;
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

    private async Task RunAsync(HostRequest request, CancellationToken cancellationToken)
    {
        bool onStick = request.Kind is HostRequestKind.StickUnplug or HostRequestKind.StickPlug;
        if (onStick && request.Index >= _sticks.Count)
        {
            throw new InvalidOperationException($"no such USB stick (the profile attaches {_sticks.Count})");
        }

        if (!onStick && request.Index >= _devices.Count)
        {
            throw new InvalidOperationException($"no such USB device (the profile attaches {_devices.Count})");
        }

        await (_connected ?? throw new InvalidOperationException("QEMU's monitor was never attached"));
        if (!_startupMiceMatched)
        {
            // Before the first request changes anything, and once the guest
            // runs, so QEMU has long created every device on its command line.
            _startupMiceMatched = true;
            if (_devices.Contains(UsbMouseModel))
            {
                await MatchStartupMiceAsync(cancellationToken);
            }
        }

        switch (request.Kind)
        {
            case HostRequestKind.StickUnplug:
                await UnplugStickAsync(request.Index, cancellationToken);
                break;
            case HostRequestKind.StickPlug:
                await PlugStickAsync(request.Index, cancellationToken);
                break;
            case HostRequestKind.DeviceUnplug:
                await UnplugDeviceAsync(request.Index, cancellationToken);
                break;
            case HostRequestKind.DevicePlug:
                await PlugDeviceAsync(request.Index, cancellationToken);
                break;
            case HostRequestKind.PointerMove:
                await MovePointerAsync(request, cancellationToken);
                break;
        }
    }

    private async Task UnplugStickAsync(int index, CancellationToken cancellationToken)
    {
        string deviceId = _stickIds[index] ?? throw new InvalidOperationException($"USB stick {index} is already unplugged");

        // QEMU deletes the stick's drive with it, as it does for any drive
        // declared with -drive or drive_add.
        await ExecuteAsync("device_del", new JsonObject { ["id"] = deviceId }, cancellationToken);
        _stickIds[index] = null;
    }

    private async Task PlugStickAsync(int index, CancellationToken cancellationToken)
    {
        if (_stickIds[index] is not null)
        {
            throw new InvalidOperationException($"USB stick {index} is already plugged in");
        }

        _plugCount++;
        string driveId = $"{QemuLauncher.UsbDriveId(index)}p{_plugCount}";
        string deviceId = $"{QemuLauncher.UsbStickId(index)}p{_plugCount}";

        // A drive like the one on the command line, so the next unplug
        // deletes it the same way. QMP's own blockdev-add would leave the
        // image open, and locked, after the device is gone.
        string path = _sticks[index].Path.Replace(",", ",,");
        string output = await HumanMonitorAsync($"drive_add 0 if=none,id={driveId},file={path},format=raw", cancellationToken);
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
        _stickIds[index] = deviceId;
    }

    private async Task UnplugDeviceAsync(int index, CancellationToken cancellationToken)
    {
        string deviceId = _deviceIds[index] ?? throw new InvalidOperationException($"USB device {index} is already unplugged");
        await ExecuteAsync("device_del", new JsonObject { ["id"] = deviceId }, cancellationToken);
        _deviceIds[index] = null;
        _mouseHandlers[index] = null;
        _heldButtons[index] = 0;
    }

    private async Task PlugDeviceAsync(int index, CancellationToken cancellationToken)
    {
        if (_deviceIds[index] is not null)
        {
            throw new InvalidOperationException($"USB device {index} is already plugged in");
        }

        string model = _devices[index];
        bool isMouse = model == UsbMouseModel;
        IReadOnlyList<int> miceBefore = isMouse ? await QueryUsbMiceAsync(cancellationToken) : [];

        _plugCount++;
        string deviceId = $"{QemuLauncher.UsbDeviceId(index)}p{_plugCount}";
        await ExecuteAsync(
            "device_add",
            new JsonObject
            {
                ["driver"] = model,
                ["bus"] = $"{QemuLauncher.UsbControllerId}.0",
                ["id"] = deviceId
            },
            cancellationToken);
        _deviceIds[index] = deviceId;

        if (isMouse)
        {
            // The device's handler exists once device_add returned: it is
            // the one usb-mouse handler that was not there before.
            List<int> added = (await QueryUsbMiceAsync(cancellationToken)).Except(miceBefore).ToList();
            _mouseHandlers[index] = added.Count == 1 ? added[0] : null;
        }
    }

    private async Task MovePointerAsync(HostRequest request, CancellationToken cancellationToken)
    {
        int index = request.Index;
        if (_devices[index] != UsbMouseModel)
        {
            throw new InvalidOperationException($"USB device {index} is a {_devices[index]}, not a {UsbMouseModel}");
        }

        if (_deviceIds[index] is null)
        {
            throw new InvalidOperationException($"USB device {index} is unplugged");
        }

        int handler = _mouseHandlers[index]
            ?? throw new InvalidOperationException($"QEMU's mice could not be matched to USB device {index}");

        // input-send-event's "device" names a display, not an input device:
        // QEMU looks it up among its consoles, and 10.2.2 aborts on the
        // first text console it meets when it names a USB device. So the
        // events go to the current mouse, and mouse_set makes it this one
        // first. It is set on every request because a guest that starts
        // polling another pointer makes that one current.
        string output = await HumanMonitorAsync($"mouse_set {handler}", cancellationToken);
        if (output.Length != 0)
        {
            throw new InvalidOperationException($"mouse_set: {output}");
        }

        await ExecuteAsync(
            "input-send-event",
            new JsonObject { ["events"] = PointerEvents(request.DeltaX, request.DeltaY, _heldButtons[index], request.Buttons) },
            cancellationToken);
        _heldButtons[index] = request.Buttons;
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

    /// <summary>
    /// Finds the input handler of each <c>usb-mouse</c> QEMU was started
    /// with. QEMU numbers handlers as it creates devices, the command line's
    /// in order, and names every <c>usb-mouse</c>'s alike, so the n-th of
    /// those by index is the n-th <c>usb-mouse</c> in the profile's list.
    /// </summary>
    private async Task MatchStartupMiceAsync(CancellationToken cancellationToken)
    {
        List<int> handlers = (await QueryUsbMiceAsync(cancellationToken)).Order().ToList();
        List<int> mice = Enumerable.Range(0, _devices.Count).Where(i => _devices[i] == UsbMouseModel).ToList();
        if (handlers.Count != mice.Count)
        {
            // Pointer requests report the mismatch.
            return;
        }

        for (int i = 0; i < mice.Count; i++)
        {
            _mouseHandlers[mice[i]] = handlers[i];
        }
    }

    /// <summary>The <c>query-mice</c> indexes of every <c>usb-mouse</c>'s input handler.</summary>
    private async Task<IReadOnlyList<int>> QueryUsbMiceAsync(CancellationToken cancellationToken)
    {
        JsonNode? mice = await ExecuteAsync("query-mice", null, cancellationToken);
        if (mice is not JsonArray list)
        {
            throw new InvalidOperationException("query-mice returned no list");
        }

        return list
            .OfType<JsonObject>()
            .Where(m => (string?)m["name"] == UsbMouseHandlerName)
            .Select(m => (int?)m["index"] ?? throw new InvalidOperationException("query-mice listed a mouse without an index"))
            .ToList();
    }

    /// <summary>Runs one human monitor command and returns what it printed, trimmed.</summary>
    private async Task<string> HumanMonitorAsync(string commandLine, CancellationToken cancellationToken)
    {
        JsonNode? output = await ExecuteAsync("human-monitor-command", new JsonObject { ["command-line"] = commandLine }, cancellationToken);
        return output?.GetValue<string>().Trim() ?? string.Empty;
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
