// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The kit's serial log: one fixed-format line per event, prefixed
/// <c>[Drivers]</c>, the same facts the diagnostics view exposes. Lines are
/// built in thread context, then written with interrupts masked so a
/// preemption cannot cut one in half; never called from interrupt context.
/// </summary>
internal static class DriverLog
{
    private const string Prefix = "[Drivers] ";

    /// <summary>The manifest, once, when the engine starts: <c>manifest: A(prio 0) B(prio 10)</c>.</summary>
    internal static void Manifest(IReadOnlyList<Driver> drivers)
    {
        string line = "manifest:";
        if (drivers.Count == 0)
        {
            line = $"{line} (empty)";
        }

        for (int i = 0; i < drivers.Count; i++)
        {
            line = $"{line} {drivers[i].Name}(prio {drivers[i].Priority})";
        }

        WriteLine(line);
    }

    /// <summary>How the engine runs: <c>engine started, worker thread</c> or <c>engine started, inline (no worker)</c>.</summary>
    internal static void Started(bool hasWorker) =>
        WriteLine(hasWorker ? "engine started, worker thread" : "engine started, inline (no worker)");

    /// <summary>The ordered candidates for a node: <c>synthetic:k candidates: High(prio 10, spec 1) Low(prio 0, spec 1)</c>.</summary>
    internal static void Candidates(DeviceNode node, string candidates) =>
        WriteLine($"{node.Path} candidates:{candidates}");

    /// <summary>One offer's outcome: <c>synthetic:k offer High -> bound</c>, <c>... -> declined: reason</c>, <c>... -> failed: message</c>.</summary>
    internal static void Offer(DeviceNode node, Driver driver, ProbeResult result)
    {
        string outcome = result.Outcome switch
        {
            ProbeOutcome.Bound => "bound",
            ProbeOutcome.Declined => $"declined: {result.Reason}",
            _ => $"failed: {result.Reason}",
        };
        WriteLine($"{node.Path} offer {driver.Name} -> {outcome}");
    }

    /// <summary>No registered driver matched: <c>synthetic:k no driver</c>.</summary>
    internal static void NoDriver(DeviceNode node) => WriteLine($"{node.Path} no driver");

    /// <summary>A driver published a device: <c>synthetic:k High published keyboard "name" (consumed)</c>.</summary>
    internal static void Published(DeviceNode node, Driver driver, PublishedDevice device) =>
        WriteLine($"{node.Path} {driver.Name} published {KindName(device.Kind)} \"{device.Name}\" {(device.IsConsumed ? "(consumed)" : "(no consumer)")}");

    /// <summary>The engine published a firmware device: <c>firmware published display "framebuffer" (consumed)</c>.</summary>
    internal static void FirmwarePublished(PublishedDevice device) =>
        WriteLine($"firmware published {KindName(device.Kind)} \"{device.Name}\" {(device.IsConsumed ? "(consumed)" : "(no consumer)")}");

    /// <summary>A firmware display was withdrawn because a driver bound the function holding it: <c>firmware display "framebuffer" retired: inside pci:0000:00:01.0 bar 1</c>.</summary>
    internal static void FirmwareRetired(PublishedDevice device, DeviceNode node, int barIndex) =>
        WriteLine($"firmware {KindName(device.Kind)} \"{device.Name}\" retired: inside {node.Path} bar {barIndex}");

    /// <summary>A published device was withdrawn: <c>synthetic:k High withdrew keyboard "name"</c>.</summary>
    internal static void Withdrew(DeviceNode node, Driver driver, PublishedDevice device) =>
        WriteLine($"{node.Path} {driver.Name} withdrew {KindName(device.Kind)} \"{device.Name}\"");

    /// <summary>A node left the tree: <c>synthetic:k retracted</c>.</summary>
    internal static void Retracted(DeviceNode node) => WriteLine($"{node.Path} retracted");

    /// <summary>A driver's own message: <c>synthetic:k High: message</c>.</summary>
    internal static void DriverMessage(DeviceNode node, Driver driver, string message) =>
        WriteLine($"{node.Path} {driver.Name}: {message}");

    /// <summary>A handler threw, logged afterwards in thread context: <c>synthetic:k High interrupt handler threw: message</c>.</summary>
    internal static void HandlerThrew(DeviceNode node, Driver driver, string? message) =>
        WriteLine($"{node.Path} {driver.Name} interrupt handler threw: {message}");

    /// <summary>A driver thread outlived the join: <c>synthetic:k High thread "name" did not stop in 500 ms; 3 resources leaked</c>.</summary>
    internal static void ThreadDidNotStop(DeviceNode node, Driver driver, string threadName, uint timeoutMilliseconds, int leakedResources) =>
        WriteLine($"{node.Path} {driver.Name} thread \"{threadName}\" did not stop in {timeoutMilliseconds} ms; {leakedResources} resources leaked");

    /// <summary><c>synthetic:k High OnDetach threw: message</c>.</summary>
    internal static void OnDetachThrew(DeviceNode node, Driver driver, string message) =>
        WriteLine($"{node.Path} {driver.Name} OnDetach threw: {message}");

    /// <summary><c>synthetic:k High work item threw: message</c>.</summary>
    internal static void WorkItemThrew(DeviceNode node, Driver driver, string message) =>
        WriteLine($"{node.Path} {driver.Name} work item threw: {message}");

    /// <summary>A teardown step threw and the walk went on: <c>synthetic:k High teardown step "withdraw" threw: message</c>.</summary>
    internal static void TeardownStepThrew(DeviceNode node, Driver driver, string step, string message) =>
        WriteLine($"{node.Path} {driver.Name} teardown step \"{step}\" threw: {message}");

    /// <summary>A bus hook threw and the engine went on: <c>pci:0000:00:03.0 bus hook "quiesce" threw: message</c>.</summary>
    internal static void HookThrew(DeviceNode node, string step, string message) =>
        WriteLine($"{node.Path} bus hook \"{step}\" threw: {message}");

    /// <summary>A node was left unbound with no offer because the bus could not quiesce it: <c>pci:0000:00:03.0 not offered: reason</c>.</summary>
    internal static void NodeSkipped(DeviceNode node, string reason) =>
        WriteLine($"{node.Path} not offered: {reason}");

    /// <summary>An ECAM window could not be mapped past a bus, so the host ends there: <c>pci host at 0x3f000000: buses 10 to ff not mapped, enumeration ends at bus 0f</c>.</summary>
    internal static void EcamWindowClamped(ulong physicalBase, byte firstUnmappedBus, byte endBus, byte lastMappedBus) =>
        WriteLine($"pci host at 0x{physicalBase:x}: buses {firstUnmappedBus:x2} to {endBus:x2} not mapped, enumeration ends at bus {lastMappedBus:x2}");

    /// <summary>A virtio device did not acknowledge a reset in time, and the kit went on as if it had: <c>virtio type 1: status did not return to 0 within 100 ms after reset</c>. Keyed on the type: the access has no node when the handshake starts.</summary>
    internal static void VirtioResetTimedOut(VirtioDeviceType deviceType, uint timeoutMilliseconds) =>
        WriteLine($"virtio type {(uint)deviceType}: status did not return to 0 within {timeoutMilliseconds} ms after reset");

    /// <summary>A kit-internal work item, or the worker itself, threw: <c>engine: message</c>.</summary>
    internal static void EngineError(string message) => WriteLine($"engine: {message}");

    /// <summary>A device kind in words, for the lines above.</summary>
    internal static string KindName(DeviceKind kind) => kind switch
    {
        DeviceKind.Keyboard => "keyboard",
        DeviceKind.Pointer => "pointer",
        DeviceKind.Network => "network",
        DeviceKind.Block => "block",
        DeviceKind.Display => "display",
        _ => "device",
    };

    private static void WriteLine(string line)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            Serial.WriteString(Prefix);
            Serial.WriteString(line);
            Serial.WriteString("\n");
        }
    }
}
