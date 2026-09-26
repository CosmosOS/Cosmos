using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine.Hosts;
using Cosmos.Tools.Launcher;

namespace Cosmos.TestRunner.Engine;

/// <summary>
/// Device models a profile attaches beyond its disks. A null member leaves
/// the architecture default alone (no input device; the arch's default NIC
/// and VGA adapter).
/// </summary>
/// <param name="NetworkCard">NIC model, e.g. <c>e1000e</c> or <c>virtio-net-pci</c>.</param>
/// <param name="KeyboardDevice">Keyboard model, e.g. <c>virtio-keyboard-pci</c>.</param>
/// <param name="MouseDevice">Mouse model, e.g. <c>virtio-mouse-pci</c>.</param>
/// <param name="VgaAdapter">VGA adapter as a <c>-vga</c> backend name, e.g. <c>vmware</c> — replaces the default adapter rather than adding a second one.</param>
/// <param name="GpuDevice">Display adapter attached as a <c>-device</c> line, e.g. <c>virtio-gpu-pci</c> — added alongside the default adapter rather than replacing it.</param>
/// <param name="Devices">Extra <c>-device</c> models from the profile's "devices" axis, e.g. <c>edu</c>; null or empty for none.</param>
/// <param name="UsbDevices">USB models from the profile's "usb" axis, e.g. <c>usb-mouse</c>, placed on the xHCI controller USB disks share; null or empty for none.</param>
public sealed record ProfileDevices(
    string? NetworkCard,
    string? KeyboardDevice,
    string? MouseDevice,
    string? VgaAdapter,
    string? GpuDevice = null,
    IReadOnlyList<DeviceAttachment>? Devices = null,
    IReadOnlyList<string>? UsbDevices = null);

/// <summary>
/// Interface for QEMU virtual machine hosts that can run test kernels
/// </summary>
public interface IQemuHost
{
    /// <summary>
    /// Architecture this host targets (x64, ARM64, etc.)
    /// </summary>
    string Architecture { get; }

    /// <summary>
    /// Run a kernel ISO in QEMU and capture UART output
    /// </summary>
    /// <param name="isoPath">Path to the bootable ISO</param>
    /// <param name="uartLogPath">Path to write UART log output</param>
    /// <param name="timeoutSeconds">Maximum time to run (default 30s)</param>
    /// <param name="showDisplay">Show QEMU display window (default false = headless)</param>
    /// <param name="enableNetworkTesting">Enable UDP test server for network tests (default false)</param>
    /// <param name="disks">Per-profile disk attachments. AHCI entries share one <c>ich9-ahci</c> controller; NVMe entries each get their own <c>nvme</c> controller; USB entries share one <c>qemu-xhci</c> controller. Per-disk extra device options (e.g. <c>msix=off</c>) flow through.</param>
    /// <param name="machineOptions">Extra <c>-M</c> properties (e.g. <c>{"gic-version", "2"}</c> on ARM64). Caller is responsible for passing arch-appropriate keys.</param>
    /// <param name="devices">Per-profile NIC, input, display, extra and USB device models; null leaves the architecture defaults in place.</param>
    /// <param name="hotPlug">Carries out the guest's requests to plug its USB sticks and USB devices in and out and to move its USB mice, through the QMP monitor QEMU is launched with; null when the run attaches no USB stick or USB device.</param>
    /// <returns>Exit code and UART log content</returns>
    Task<QemuRunResult> RunKernelAsync(string isoPath, string uartLogPath, int timeoutSeconds = QemuHostDefaults.DefaultTimeoutSeconds, bool showDisplay = false, bool enableNetworkTesting = false, IReadOnlyList<DiskAttachment>? disks = null, IReadOnlyDictionary<string, string>? machineOptions = null, ProfileDevices? devices = null, QemuHotPlug? hotPlug = null);
}

/// <summary>
/// Outcome of the UART log monitor task.
/// </summary>
public enum UartMonitorOutcome
{
    /// <summary>Cancelled before any decision could be made.</summary>
    NotFinished,
    /// <summary>Kernel emitted the suite-end marker (0xDEADBEEFCAFEBABE).</summary>
    EndMarkerSeen,
    /// <summary>UART went quiet after a TestPass — the kernel is hung after reaching a test.</summary>
    Stalled
}

/// <summary>
/// Result of running a kernel in QEMU
/// </summary>
public record QemuRunResult
{
    public int ExitCode { get; init; }
    public string UartLog { get; init; } = string.Empty;
    public bool TimedOut { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// True if the kernel emitted the suite-end marker (0xDEADBEEFCAFEBABE)
    /// before QEMU exited. False means QEMU exited on its own (e.g. guest
    /// rebooted or shut down) — which the multi-boot loop treats as a cue
    /// to re-launch with the next <c>skip=N</c>.
    /// </summary>
    public bool SuiteMarkerSeen { get; init; }
}
