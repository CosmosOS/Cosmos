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
/// Plugs the profile's USB sticks in and out of the running guest when one
/// of its tests asks (<see cref="Ds2Vs.HostRequest"/>), through QEMU's QMP
/// monitor. QEMU connects the monitor to <see cref="Port"/> at startup, so
/// an instance exists, and listens, before QEMU is launched. One instance
/// serves one QEMU run.
/// </summary>
public sealed class QemuHotPlug : IAsyncDisposable
{
    /// <summary>Pulls stick n (0 when omitted) off the xHCI controller.</summary>
    public const string UsbUnplugRequest = "usb-unplug";

    /// <summary>Plugs stick n (0 when omitted) back in, on the same image.</summary>
    public const string UsbPlugRequest = "usb-plug";

    private readonly TcpListener _listener;
    private readonly IReadOnlyList<DiskAttachment> _sticks;

    /// <summary>QEMU id of each stick's device, null while it is unplugged.</summary>
    private readonly string?[] _deviceIds;

    /// <summary>Numbers the devices and drives of each plug, whose ids QEMU may not have released yet.</summary>
    private int _plugCount;

    private Task? _connected;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    /// <summary>Port QEMU connects its monitor to (<see cref="QemuLaunchOptions.MonitorPort"/>).</summary>
    public int Port { get; }

    private QemuHotPlug(IReadOnlyList<DiskAttachment> sticks)
    {
        _sticks = sticks;
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
    /// An instance for a run attaching <paramref name="disks"/>, or null
    /// when none of them is a USB stick, so nothing can be plugged.
    /// </summary>
    public static QemuHotPlug? For(IReadOnlyList<DiskAttachment> disks)
    {
        List<DiskAttachment> sticks = disks.Where(d => d.Kind == DiskKind.Usb).ToList();
        return sticks.Count == 0 ? null : new QemuHotPlug(sticks);
    }

    /// <summary>Takes QEMU's monitor connection. Called once QEMU was started.</summary>
    public void Attach(CancellationToken cancellationToken) => _connected ??= ConnectAsync(cancellationToken);

    /// <summary>
    /// Carries out one request of the guest. A request that fails is
    /// reported and dropped: the guest's test then times out waiting for
    /// the change, and fails with that.
    /// </summary>
    public static async Task DispatchAsync(QemuHotPlug? hotPlug, string request, CancellationToken cancellationToken)
    {
        if (hotPlug is null)
        {
            Console.WriteLine($"[HotPlug] Ignored '{request}': the profile attaches no USB stick");
            return;
        }

        try
        {
            await hotPlug.RunAsync(request, cancellationToken);
            Console.WriteLine($"[HotPlug] {request}: done");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"[HotPlug] {request} failed: {ex.Message}");
        }
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
        int index = 0;
        if (words.Length == 0 || words.Length > 2
            || (words.Length == 2 && !int.TryParse(words[1], out index))
            || index < 0 || index >= _sticks.Count)
        {
            throw new InvalidOperationException($"no such USB stick (the profile attaches {_sticks.Count})");
        }

        await (_connected ?? throw new InvalidOperationException("QEMU's monitor was never attached"));
        switch (words[0])
        {
            case UsbUnplugRequest:
                await UnplugAsync(index, cancellationToken);
                break;
            case UsbPlugRequest:
                await PlugAsync(index, cancellationToken);
                break;
            default:
                throw new InvalidOperationException("unknown request");
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
