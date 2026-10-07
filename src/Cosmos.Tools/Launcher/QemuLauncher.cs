using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Tools.Platform;

namespace Cosmos.Tools.Launcher;

public sealed class QemuLaunchOptions
{
    /// <summary>Default guest RAM size passed to QEMU's <c>-m</c> flag, in megabytes.</summary>
    private const int DefaultMemoryMb = 512;

    public required string Architecture { get; init; }
    public required string IsoPath { get; init; }
    public int MemoryMb { get; init; } = DefaultMemoryMb;
    public bool Headless { get; init; }
    public bool Debug { get; init; }
    /// <summary>If null, serial goes to stdio (interactive CLI). Otherwise, to this file path (test runner).</summary>
    public string? SerialOutputFile { get; init; }
    /// <summary>Adds the test-runner port forwards (UDP 5556, TCP 5558) needed by network tests.</summary>
    public bool EnableNetworkTesting { get; init; }

    /// <summary>
    /// NIC model exposed to the guest. <c>null</c> leaves QEMU's default NIC in
    /// place (the historical behaviour); <c>"none"</c> emits <c>-nic none</c> so
    /// the guest gets no card at all; any other value emits a user-mode
    /// <c>-netdev</c> + <c>-device &lt;model&gt;</c> pair (e.g. <c>e1000e</c>,
    /// <c>virtio-net-device</c>). When <see cref="EnableNetworkTesting"/> is
    /// set this still selects the model, but that path always adds its port
    /// forwards and rejects <c>"none"</c>.
    /// </summary>
    public string? NetworkCard { get; init; }

    /// <summary>
    /// Keyboard device attached to the guest, or <c>null</c> to add none.
    /// <c>"ps2"</c>/<c>"none"</c> add nothing (x64's PS/2 keyboard is built into
    /// the q35 chipset); any other value (e.g. <c>virtio-keyboard-device</c>,
    /// required on the ARM64 <c>virt</c> machine which has no PS/2 controller) is
    /// emitted as a <c>-device</c> line.
    /// </summary>
    public string? KeyboardDevice { get; init; }

    /// <summary>
    /// Mouse device attached to the guest, or <c>null</c> to add none. Same
    /// <c>"ps2"</c>/<c>"none"</c> handling as <see cref="KeyboardDevice"/>;
    /// <c>virtio-mouse-device</c> is the ARM64 <c>virt</c> option.
    /// </summary>
    public string? MouseDevice { get; init; }

    /// <summary>
    /// VGA adapter exposed to the guest, as a <c>-vga</c> backend name (e.g.
    /// <c>vmware</c>, <c>virtio</c>, <c>none</c>). <c>null</c> keeps the
    /// architecture default (stdvga on q35, nothing beyond ramfb on virt).
    /// Unlike <see cref="KeyboardDevice"/> this is not a <c>-device</c> model:
    /// <c>-vga</c> replaces the machine's default adapter instead of adding a
    /// second one alongside it.
    /// </summary>
    public string? VgaAdapter { get; init; }

    /// <summary>
    /// Display device attached as a <c>-device</c> line (e.g.
    /// <c>virtio-gpu-pci</c>), or <c>null</c>/<c>"none"</c> to add none.
    /// Unlike <see cref="VgaAdapter"/> this ADDS an adapter alongside the
    /// machine's default one rather than replacing it, so the firmware keeps
    /// the framebuffer it boots Limine on (VBE over q35's std VGA, ramfb on
    /// virt) while the guest driver gets a second device to bind. That is the
    /// only shape that works on both arches: <c>-vga virtio</c> resolves to
    /// the VGA-compatible <c>virtio-vga</c> on q35, but the virt machine has
    /// no such variant and rejects it with "Virtio VGA not available".
    /// </summary>
    public string? GpuDevice { get; init; }

    /// <summary>
    /// Disks to attach. Each <see cref="DiskAttachment"/> carries the image
    /// path, the controller type (ahci or nvme), and an optional comma-prefixed
    /// suffix appended to the QEMU <c>-device</c> line, used by test profiles
    /// to toggle things like <c>msix=off</c> or <c>msix_qsize=1</c>. AHCI disks
    /// share one <c>ich9-ahci</c> controller across successive ports; NVMe
    /// disks each get a dedicated <c>nvme</c> controller. Honoured on x64 (q35)
    /// and ARM64 (virt).
    /// </summary>
    public IReadOnlyList<DiskAttachment> Disks { get; init; } = Array.Empty<DiskAttachment>();

    /// <summary>
    /// Extra <c>-M</c> machine properties spliced after the architecture
    /// defaults (e.g. <c>{"gic-version", "2"}</c> on ARM64 to force GICv2).
    /// Empty by default. Caller is responsible for passing properties that
    /// match the active architecture; nothing here filters on that.
    /// </summary>
    public IReadOnlyDictionary<string, string> MachineOptions { get; init; }
        = new Dictionary<string, string>();

    /// <summary>
    /// When false (default, dev path), x64 launches with <c>-no-shutdown</c> so a guest-initiated
    /// ACPI _S5 / panic just pauses the VM and the user can inspect it. When true (test path),
    /// the flag is omitted so a working <c>Power.Shutdown()</c> actually exits QEMU and the test
    /// runner can observe a clean process exit.
    /// </summary>
    public bool AllowGuestShutdown { get; init; }

    /// <summary>
    /// QEMU CPU model (<c>-cpu</c>), e.g. <c>host</c>, <c>max</c>, <c>qemu64</c>,
    /// <c>cortex-a72</c>. Null/empty = auto: on x64 that is <c>host</c> when KVM
    /// is available and <c>max</c> under TCG; on ARM64 it is <c>cortex-a72</c>.
    /// <c>host</c> is KVM-only, so it degrades to <c>max</c> when KVM is absent
    /// rather than aborting QEMU at startup.
    /// </summary>
    public string? CpuModel { get; init; }

    /// <summary>
    /// TCP port on 127.0.0.1 that QEMU connects its QMP monitor to at
    /// startup, or <c>null</c> for no monitor. Lets a test runner change the
    /// machine under a running guest, such as pulling a USB stick out. The
    /// port must already be listening: QEMU exits when it cannot connect.
    /// </summary>
    public int? MonitorPort { get; init; }

    public IReadOnlyList<string> ExtraArgs { get; init; } = Array.Empty<string>();
}

/// <summary>The controller or transport a <see cref="DiskAttachment"/> reaches the guest through.</summary>
public enum DiskKind
{
    /// <summary>A SATA disk on the shared <c>ich9-ahci</c> controller.</summary>
    Ahci,

    /// <summary>A namespace on a dedicated <c>nvme</c> controller.</summary>
    Nvme,

    /// <summary>A <c>usb-storage</c> stick on the shared <c>qemu-xhci</c> controller.</summary>
    Usb,

    /// <summary>
    /// A virtio-blk-pci function on the root bus, or behind a PCI Express
    /// root port of its own when the attachment is hot-pluggable.
    /// </summary>
    VirtioBlk,

    /// <summary>A virtio-blk-device in the arm64 virt machine's virtio-mmio window; refused on x64.</summary>
    VirtioBlkMmio
}

/// <summary>
/// One disk image to expose to the guest. <see cref="ExtraDeviceOptions"/> is
/// appended verbatim to the QEMU <c>-device</c> line (without the leading
/// comma, the launcher inserts that), so a profile can pass things like
/// <c>"msix=off"</c> or <c>"msix_qsize=1"</c> to exercise the kernel's
/// interrupt-fallback paths.
/// </summary>
public sealed record DiskAttachment
{
    public required string Path { get; init; }
    public required DiskKind Kind { get; init; }
    public string ExtraDeviceOptions { get; init; } = string.Empty;

    /// <summary>
    /// The disk sits behind a PCI Express root port of its own, so the test
    /// engine can pull it out and plug it back in; <see cref="DiskKind.VirtioBlk"/> only.
    /// </summary>
    public bool HotPlug { get; init; }
}

public sealed record QemuLaunchPlan(string BinaryPath, string Arguments, ToolSource Source);

/// <summary>
/// Single source of truth for QEMU command-line construction. Used by both
/// `cosmos run` and `Cosmos.TestRunner.Engine` so a tweak (e.g. dropping a
/// broken display backend) propagates to all callers.
/// </summary>
public static class QemuLauncher
{
    /// <summary>UDP port forwarded host-to-guest for the network test runner's datagram traffic.</summary>
    private const int NetworkTestUdpPort = 5556;

    /// <summary>TCP port forwarded host-to-guest for the network test runner's stream traffic.</summary>
    private const int NetworkTestTcpPort = 5558;

    /// <summary>
    /// TCP port of the test runner's raw-Ethernet listener (IcmpTestServer). Slirp's
    /// hostfwd only forwards TCP/UDP, so host-sourced ICMP rides a stream netdev
    /// hubbed with slirp: frames are 4-byte big-endian-length-prefixed Ethernet.
    /// </summary>
    private const int NetworkTestRawSocketPort = 5560;

    /// <summary>QEMU id of the xHCI controller USB disks and the USB keyboard sit on; its root hub is the bus <c>usbxhci0.0</c>.</summary>
    public const string UsbControllerId = "usbxhci0";

    /// <summary>The keyboard model a profile names to get a USB keyboard on the xHCI controller, and QEMU's driver name for it.</summary>
    public const string UsbKeyboardModel = "usb-kbd";

    /// <summary>QEMU id of the USB keyboard plugged in at boot, the one to unplug.</summary>
    public const string UsbKeyboardId = "usbkbd0";

    /// <summary>QEMU id of the drive behind the <paramref name="index"/>th USB disk.</summary>
    public static string UsbDriveId(int index) => $"usbdisk{index}";

    /// <summary>QEMU id of the <paramref name="index"/>th USB disk's usb-storage device, the one to unplug.</summary>
    public static string UsbDeviceId(int index) => $"usbstick{index}";

    /// <summary>QEMU id of the drive behind the <paramref name="index"/>th virtio-blk disk, both kinds counted together.</summary>
    public static string VirtioBlkDriveId(int index) => $"vblkdisk{index}";

    /// <summary>QEMU id of the <paramref name="index"/>th virtio-blk device, the one a hot-plug request deletes.</summary>
    public static string VirtioBlkDeviceId(int index) => $"vblk{index}";

    /// <summary>QEMU's driver name for a virtio-blk disk behind a PCI function.</summary>
    public const string VirtioBlkPciModel = "virtio-blk-pci";

    /// <summary>QEMU's driver name for a virtio-blk disk in the virt machine's virtio-mmio window.</summary>
    public const string VirtioBlkMmioModel = "virtio-blk-device";

    /// <summary>QEMU id of the root port the <paramref name="index"/>th virtio-blk disk sits behind, also its <c>bus=</c> name.</summary>
    public static string RootPortId(int index) => $"rp{index}";

    /// <summary>QEMU's driver name for a PCI Express root port.</summary>
    public const string RootPortModel = "pcie-root-port";

    /// <summary>
    /// The global that turns q35's root ports to native PCI Express hot-plug;
    /// without it QEMU 8.2 routes a root port hot-plug through ACPI, which the
    /// kernel does not interpret; stage9-experiments.md E3. The virt machine's
    /// ports are native always and the global is not emitted there.
    /// </summary>
    public const string Ich9NativeHotPlugGlobal = "ICH9-LPC.acpi-pci-hotplug-with-bridge-support=off";

    public static async Task<QemuLaunchPlan> BuildAsync(QemuLaunchOptions options)
    {
        CommandToolDefinition tool = options.Architecture switch
        {
            "x64" => ToolDefinitions.QemuX64,
            "arm64" => ToolDefinitions.QemuArm64,
            _ => throw new ArgumentException($"Unsupported architecture: {options.Architecture}", nameof(options))
        };

        ResolvedTool resolved = await ToolResolver.ResolveAsync(tool);
        if (resolved.Source == ToolSource.NotFound)
        {
            throw new InvalidOperationException(
                $"QEMU for {options.Architecture} not found. Run `cosmos install` to fetch the bundled toolchain, " +
                $"or install qemu-system-{(options.Architecture == "x64" ? "x86_64" : "aarch64")} system-wide.");
        }

        StringBuilder args = new();

        // Single rule, all OSes: when QEMU is bundled, point it at the bundle's
        // share/qemu/ for BIOS/firmware lookup. The MSYS2 Windows build never
        // auto-discovers its data dir, and even the Linux/macOS build's runtime
        // search depends on the build's compile-time prefix matching the install
        // prefix: we ship a portable bundle, so neither holds. Explicit -L is
        // the only universally reliable mechanism.
        if (resolved.Source == ToolSource.Bundle)
        {
            string shareQemu = Path.GetFullPath(
                Path.Combine(Path.GetDirectoryName(resolved.Path)!, "..", "share", "qemu"));
            args.Append($"-L \"{shareQemu}\" ");
        }

        RejectQuotes(options.IsoPath, "ISO path");
        if (options.SerialOutputFile is not null)
        {
            RejectQuotes(options.SerialOutputFile, "serial output path");
        }

        if (options.Architecture == "x64")
        {
            AppendX64Args(args, options);
        }
        else
        {
            AppendArm64Args(args, options);
        }

        // Display: omit -display when a window is wanted so QEMU picks its compiled-in
        // default (SDL on Windows, GTK on Linux/macOS). Passing a backend QEMU wasn't
        // built with causes it to abort with "Display 'X' is not available".
        if (options.Headless)
        {
            args.Append(" -display none");
        }

        // Serial: file for tests (parseable log), stdio for CLI (interactive).
        if (options.SerialOutputFile is not null)
        {
            args.Append($" -serial file:\"{options.SerialOutputFile}\"");
        }
        else
        {
            args.Append(" -serial stdio");
        }

        if (options.EnableNetworkTesting)
        {
            string nic = ResolveNetworkTestNic(options.Architecture, options.NetworkCard);
            // One hub joins three ports: slirp (DHCP/NAT + the UDP/TCP hostfwds),
            // a raw-Ethernet stream socket to the runner's IcmpTestServer, and the
            // guest NIC. The stream port exists because slirp cannot forward
            // host-sourced ICMP; the runner must be listening before QEMU starts.
            args.Append($" -netdev user,id=net0,hostfwd=udp::{NetworkTestUdpPort}-:{NetworkTestUdpPort},hostfwd=tcp::{NetworkTestTcpPort}-:{NetworkTestTcpPort}");
            args.Append($" -netdev stream,id=rawnet0,addr.type=inet,addr.host=127.0.0.1,addr.port={NetworkTestRawSocketPort}");
            args.Append(" -netdev hubport,id=hubport0,hubid=0,netdev=net0");
            args.Append(" -netdev hubport,id=hubport1,hubid=0,netdev=rawnet0");
            args.Append(" -netdev hubport,id=hubport2,hubid=0");
            args.Append($" -device {nic},netdev=hubport2");
        }
        else if (options.NetworkCard is not null)
        {
            AppendNetworkCardArgs(args, options.NetworkCard);
        }

        AppendInputDevice(args, options.KeyboardDevice);
        AppendInputDevice(args, options.MouseDevice);
        AppendVgaAdapter(args, options.VgaAdapter);
        AppendGpuDevice(args, options.GpuDevice);

        if (options.MonitorPort is int monitorPort)
        {
            args.Append($" -chardev socket,id=qmp0,host=127.0.0.1,port={monitorPort} -mon chardev=qmp0,mode=control");
        }

        if (options.Debug)
        {
            args.Append(" -s -S");
        }

        foreach (string extra in options.ExtraArgs)
        {
            args.Append(' ');
            args.Append(extra);
        }

        return new QemuLaunchPlan(resolved.Path, args.ToString().TrimStart(), resolved.Source);
    }

    private static void AppendX64Args(StringBuilder args, QemuLaunchOptions options)
    {
        args.Append("-M q35");
        AppendMachineOptions(args, options.MachineOptions);
        // KVM when the host offers it: TCG software emulation is roughly an
        // order of magnitude slower and tanks guest-side rendering (every
        // pixel blend and framebuffer memmove is binary-translated).
        bool kvm = KvmAvailable();
        string cpu = string.IsNullOrWhiteSpace(options.CpuModel)
            ? (kvm ? "host" : "max")
            : options.CpuModel.Trim();
        if (!kvm && cpu.Equals("host", StringComparison.OrdinalIgnoreCase))
        {
            // -cpu host is KVM-only; QEMU aborts at startup under TCG.
            cpu = "max";
        }
        ValidateOptionToken(cpu, "cpu model");
        if (kvm)
        {
            args.Append(" -enable-kvm");
        }
        args.Append($" -cpu {cpu} -m {options.MemoryMb}M");
        // Explicit CD drive with bootindex=0 so SeaBIOS picks the ISO over
        // any attached HDDs whose 0xAA55 MBR signature would otherwise
        // satisfy the BIOS and hang when their boot code is empty (the case
        // for our Mbr.Create / Gpt protective-MBR writes).
        args.Append($" -drive file=\"{EscapeDriveFileValue(options.IsoPath)}\",if=none,id=cosmoscd,format=raw,readonly=on");
        args.Append(" -device ide-cd,drive=cosmoscd,bootindex=0");
        args.Append(" -boot d -no-reboot");
        if (!options.AllowGuestShutdown)
        {
            args.Append(" -no-shutdown");
        }
        // A profile-selected adapter is emitted later (shared path) and -vga
        // may only appear once, so the windowed default steps aside for it.
        if (!options.Headless && options.VgaAdapter is null)
        {
            args.Append(" -vga std");
        }
        AppendUsbKeyboardArgs(args, options, AppendStorageArgs(args, options));
    }

    /// <summary>
    /// Emits the arm64 virt machine, its CPU, memory and EDK2 firmware, the
    /// ISO behind a virtio-scsi controller, <c>ramfb</c>, the disks and the
    /// USB keyboard.
    /// </summary>
    internal static void AppendArm64Args(StringBuilder args, QemuLaunchOptions options)
    {
        // -bios takes a bare filename when not absolute: QEMU resolves it
        // through its data dir search, which our `-L "<exe>/../share/qemu"`
        // (added above when Source==Bundle) points at the bundle's
        // edk2-aarch64-code.fd. No separate firmware-lookup logic needed.
        args.Append("-M virt,highmem=off");
        AppendMachineOptions(args, options.MachineOptions);
        string cpu = string.IsNullOrWhiteSpace(options.CpuModel)
            ? "cortex-a72"
            : options.CpuModel.Trim();
        ValidateOptionToken(cpu, "cpu model");
        args.Append($" -cpu {cpu} -m {options.MemoryMb}M");
        args.Append(" -bios edk2-aarch64-code.fd");
        // The ISO goes through a virtio-scsi controller, not -cdrom: the virt
        // machine attaches -cdrom as a virtio-blk-pci function, which the
        // shipped virtio-blk driver would bind and publish as a read-only disk
        // on every cell; virtio-scsi has no driver, so the boot medium stays
        // invisible to the block layer, as q35's ATAPI drive does on x64. EDK2
        // and Limine boot it the same (stage9-experiments.md E5). A -drive file
        // value doubles its commas, unlike -cdrom.
        args.Append(" -device virtio-scsi-pci,id=scsi0");
        args.Append($" -drive file=\"{EscapeDriveFileValue(options.IsoPath)}\",if=none,id=cosmoscd,format=raw,readonly=on,media=cdrom");
        args.Append(" -device scsi-cd,drive=cosmoscd,bus=scsi0.0,bootindex=0");
        args.Append(" -boot d -no-reboot");
        // ramfb is required for Limine framebuffer support even when headless.
        args.Append(" -device ramfb");
        AppendUsbKeyboardArgs(args, options, AppendStorageArgs(args, options));
    }

    /// <summary>
    /// True when the x64 guest can run under KVM: Linux x64 host with an
    /// openable /dev/kvm (the open is the canonical access check, which also
    /// catches "exists but not in the kvm group"). Set COSMOS_NO_KVM=1 to
    /// force TCG, e.g. to reproduce a TCG-only bug.
    /// </summary>
    private static bool KvmAvailable()
    {
        if (Environment.GetEnvironmentVariable("COSMOS_NO_KVM") == "1")
        {
            return false;
        }
        if (!OperatingSystem.IsLinux() || RuntimeInformation.OSArchitecture != Architecture.X64)
        {
            return false;
        }
        try
        {
            using FileStream fs = File.Open("/dev/kvm", FileMode.Open, FileAccess.ReadWrite);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void AppendMachineOptions(StringBuilder args, IReadOnlyDictionary<string, string> opts)
    {
        foreach (KeyValuePair<string, string> kv in opts)
        {
            ValidateOptionToken(kv.Key, "machine option key");
            ValidateOptionToken(kv.Value, "machine option value");
            args.Append(',');
            args.Append(kv.Key);
            args.Append('=');
            args.Append(kv.Value);
        }
    }

    /// <summary>
    /// Attach AHCI/SATA, NVMe, USB and virtio-blk disks. AHCI disks share one <c>ich9-ahci</c>
    /// controller and consume successive ports; NVMe disks each get a
    /// dedicated <c>nvme</c> controller so the guest exercises multi-controller
    /// binding; USB disks are <c>usb-storage</c> sticks on one shared
    /// <c>qemu-xhci</c> controller; <see cref="DiskKind.VirtioBlk"/> disks are
    /// one <c>virtio-blk-pci</c> function each, a hot-pluggable one behind a
    /// <c>pcie-root-port</c> of its own (on x64 with the ICH9 global that turns
    /// the ports to native hot-plug, emitted once), and <see cref="DiskKind.VirtioBlkMmio"/>
    /// disks one <c>virtio-blk-device</c> slot each (arm64 only), both kinds
    /// numbered together. Per-disk <see cref="DiskAttachment.ExtraDeviceOptions"/> is
    /// appended after the standard device properties so profiles can flip
    /// things like <c>msix=off</c>.
    /// </summary>
    /// <returns>True when the xHCI controller was emitted, so the USB keyboard can share it.</returns>
    internal static bool AppendStorageArgs(StringBuilder args, QemuLaunchOptions options)
    {
        int ahciIndex = 0;
        int nvmeIndex = 0;
        int usbIndex = 0;
        int virtioBlkIndex = 0;
        bool ahciControllerEmitted = false;
        bool usbControllerEmitted = false;
        bool ich9GlobalEmitted = false;

        foreach (DiskAttachment disk in options.Disks)
        {
            switch (disk.Kind)
            {
                case DiskKind.Ahci:
                    if (!ahciControllerEmitted)
                    {
                        args.Append(" -device ich9-ahci,id=ahci0");
                        ahciControllerEmitted = true;
                    }
                    args.Append($" -drive file=\"{EscapeDriveFileValue(disk.Path)}\",if=none,id=ahcidisk{ahciIndex},format=raw");
                    args.Append($" -device ide-hd,drive=ahcidisk{ahciIndex},bus=ahci0.{ahciIndex}");
                    AppendDeviceOptions(args, disk.ExtraDeviceOptions);
                    ahciIndex++;
                    break;

                case DiskKind.Nvme:
                    args.Append($" -drive file=\"{EscapeDriveFileValue(disk.Path)}\",if=none,id=nvmedisk{nvmeIndex},format=raw");
                    args.Append($" -device nvme,id=nvme{nvmeIndex},drive=nvmedisk{nvmeIndex},serial=cosmos-nvme-{nvmeIndex}");
                    AppendDeviceOptions(args, disk.ExtraDeviceOptions);
                    nvmeIndex++;
                    break;

                case DiskKind.Usb:
                    if (!usbControllerEmitted)
                    {
                        args.Append($" -device qemu-xhci,id={UsbControllerId}");
                        usbControllerEmitted = true;
                    }
                    args.Append($" -drive file=\"{EscapeDriveFileValue(disk.Path)}\",if=none,id={UsbDriveId(usbIndex)},format=raw");
                    args.Append($" -device usb-storage,drive={UsbDriveId(usbIndex)},bus={UsbControllerId}.0,id={UsbDeviceId(usbIndex)}");
                    AppendDeviceOptions(args, disk.ExtraDeviceOptions);
                    usbIndex++;
                    break;

                case DiskKind.VirtioBlk:
                    if (disk.HotPlug)
                    {
                        if (!ich9GlobalEmitted && options.Architecture.Equals("x64", StringComparison.OrdinalIgnoreCase))
                        {
                            args.Append($" -global {Ich9NativeHotPlugGlobal}");
                            ich9GlobalEmitted = true;
                        }
                        // QEMU resolves bus= against the devices already on the
                        // command line, so the port precedes the disk; the
                        // chassis and slot pair must be unique per port.
                        args.Append($" -device {RootPortModel},id={RootPortId(virtioBlkIndex)},bus=pcie.0,chassis={virtioBlkIndex + 1},slot={virtioBlkIndex + 1}");
                        args.Append($" -drive file=\"{EscapeDriveFileValue(disk.Path)}\",if=none,id={VirtioBlkDriveId(virtioBlkIndex)},format=raw");
                        args.Append($" -device {VirtioBlkPciModel},drive={VirtioBlkDriveId(virtioBlkIndex)},bus={RootPortId(virtioBlkIndex)},id={VirtioBlkDeviceId(virtioBlkIndex)}");
                    }
                    else
                    {
                        args.Append($" -drive file=\"{EscapeDriveFileValue(disk.Path)}\",if=none,id={VirtioBlkDriveId(virtioBlkIndex)},format=raw");
                        args.Append($" -device {VirtioBlkPciModel},drive={VirtioBlkDriveId(virtioBlkIndex)},id={VirtioBlkDeviceId(virtioBlkIndex)}");
                    }
                    AppendDeviceOptions(args, disk.ExtraDeviceOptions);
                    virtioBlkIndex++;
                    break;

                case DiskKind.VirtioBlkMmio:
                    if (!options.Architecture.Equals("arm64", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("A virtio-blk disk over MMIO needs the arm64 virt machine: q35 has no virtio-mmio window.");
                    }
                    if (disk.HotPlug)
                    {
                        throw new ArgumentException("A virtio-blk disk over MMIO cannot be hot-pluggable: only a virtio-blk-pci function sits behind a root port.");
                    }
                    args.Append($" -drive file=\"{EscapeDriveFileValue(disk.Path)}\",if=none,id={VirtioBlkDriveId(virtioBlkIndex)},format=raw");
                    args.Append($" -device {VirtioBlkMmioModel},drive={VirtioBlkDriveId(virtioBlkIndex)},id={VirtioBlkDeviceId(virtioBlkIndex)}");
                    AppendDeviceOptions(args, disk.ExtraDeviceOptions);
                    virtioBlkIndex++;
                    break;
            }
        }

        return usbControllerEmitted;
    }

    /// <summary>
    /// Attaches the USB keyboard when <see cref="QemuLaunchOptions.KeyboardDevice"/>
    /// is <see cref="UsbKeyboardModel"/>: on the xHCI controller the USB
    /// disks emitted, or on one added here when the run has no USB disk, so
    /// the keyboard always sits on <c>usbxhci0.0</c> under the id the engine
    /// unplugs and replugs.
    /// </summary>
    internal static void AppendUsbKeyboardArgs(StringBuilder args, QemuLaunchOptions options, bool controllerEmitted)
    {
        if (!string.Equals(options.KeyboardDevice, UsbKeyboardModel, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!controllerEmitted)
        {
            args.Append($" -device qemu-xhci,id={UsbControllerId}");
        }

        args.Append($" -device {UsbKeyboardModel},bus={UsbControllerId}.0,id={UsbKeyboardId}");
    }

    /// <summary>
    /// Emits the NIC selection: <c>"none"</c> disables QEMU's default card with
    /// <c>-nic none</c>; any other value attaches a user-mode NIC of that model.
    /// The model is validated as an option token so it can't splice extra
    /// arguments into the command line.
    /// </summary>
    internal static void AppendNetworkCardArgs(StringBuilder args, string card)
    {
        if (card.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            args.Append(" -nic none");
            return;
        }

        ValidateOptionToken(card, "network card model");
        args.Append($" -netdev user,id=net0 -device {card},netdev=net0");
    }

    /// <summary>
    /// Attaches an input device (keyboard/mouse) as a <c>-device</c> line.
    /// <c>null</c>/empty and the sentinels <c>"none"</c>/<c>"ps2"</c> add
    /// nothing (PS/2 is part of the x64 chipset, not a device you attach), so
    /// only real QEMU models (e.g. <c>virtio-keyboard-device</c>) are emitted.
    /// <see cref="UsbKeyboardModel"/> adds nothing here either: that keyboard
    /// goes on the xHCI controller through <see cref="AppendUsbKeyboardArgs"/>.
    /// </summary>
    internal static void AppendInputDevice(StringBuilder args, string? model)
    {
        if (string.IsNullOrWhiteSpace(model)
            || model.Equals("none", StringComparison.OrdinalIgnoreCase)
            || model.Equals("ps2", StringComparison.OrdinalIgnoreCase)
            || model.Equals(UsbKeyboardModel, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ValidateOptionToken(model, "input device model");
        args.Append($" -device {model}");
    }

    /// <summary>
    /// Selects the guest's VGA adapter as a <c>-vga</c> backend. <c>null</c>/empty
    /// keeps the architecture default; any other value, including QEMU's own
    /// <c>none</c>, is passed through. Emitted for both arches: the profile
    /// catalog's architecture filter is what keeps an adapter off a machine
    /// that cannot present it.
    /// </summary>
    internal static void AppendVgaAdapter(StringBuilder args, string? adapter)
    {
        if (string.IsNullOrWhiteSpace(adapter))
        {
            return;
        }

        ValidateOptionToken(adapter, "vga adapter");
        args.Append($" -vga {adapter}");
    }

    /// <summary>
    /// Attaches a display adapter as a <c>-device</c> line, additively: the
    /// machine's default adapter stays, so this never disturbs the framebuffer
    /// the bootloader came up on. <c>null</c>/empty and the sentinel
    /// <c>"none"</c> add nothing. Emitted for both arches (q35 accepts
    /// <c>virtio-gpu-pci</c> beside its std VGA, and virt accepts it beside
    /// ramfb), so the catalog needs no architecture filter for it.
    /// </summary>
    internal static void AppendGpuDevice(StringBuilder args, string? model)
    {
        if (string.IsNullOrWhiteSpace(model)
            || model.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ValidateOptionToken(model, "gpu device model");
        args.Append($" -device {model}");
    }

    /// <summary>
    /// Picks the NIC model for a network-testing run. A test profile selects
    /// the model when it has an opinion (e.g. driving the suite over
    /// virtio-net-pci rather than the architecture default); the port forwards
    /// are added either way, since the runner reaches the guest through them
    /// whatever the card is. <c>"none"</c> is rejected rather than silently
    /// producing a run with no way to reach the guest.
    /// </summary>
    internal static string ResolveNetworkTestNic(string architecture, string? requested)
    {
        if (requested is not null && requested.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Network testing needs a NIC, but the network card is set to \"none\".", nameof(requested));
        }

        string nic = string.IsNullOrWhiteSpace(requested)
            ? architecture == "x64" ? "e1000e" : "virtio-net-device"
            : requested.Trim();

        ValidateOptionToken(nic, "network card model");
        return nic;
    }

    internal static void AppendDeviceOptions(StringBuilder args, string extra)
    {
        if (string.IsNullOrWhiteSpace(extra))
        {
            return;
        }
        // Allow callers to pass "msix=off" or ",msix=off": normalize to a
        // single leading comma so it splices cleanly onto the -device line.
        string trimmed = extra.Trim();
        ValidateOptionToken(trimmed, "ExtraDeviceOptions");
        if (!trimmed.StartsWith(','))
        {
            args.Append(',');
        }
        args.Append(trimmed);
    }

    /// <summary>
    /// Validates and escapes a path spliced into a QEMU <c>-drive file=</c>
    /// value: commas are doubled (QEMU's option-parser escape; an unescaped
    /// comma truncates the filename and turns the remainder into bogus drive
    /// options), and quotes are rejected because the surrounding
    /// <c>file="…"</c> token has no way to carry one through the argument
    /// string. Do not use for <c>-cdrom</c>, whose filename QEMU takes
    /// verbatim.
    /// </summary>
    internal static string EscapeDriveFileValue(string path)
    {
        RejectQuotes(path, "drive path");
        return path.Replace(",", ",,");
    }

    private static void RejectQuotes(string value, string what)
    {
        if (value.Contains('"'))
        {
            throw new ArgumentException($"QEMU {what} cannot contain a double quote: {value}");
        }
    }

    // QEMU option splices (-M properties, -device properties) only ever need
    // [A-Za-z0-9_.,=-]. Anything else, whitespace above all, would leave
    // the current token and splice new arguments into the command line, so
    // reject it instead of passing it through.
    private static void ValidateOptionToken(string value, string what)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (!char.IsAsciiLetterOrDigit(c) && c is not ('_' or '.' or ',' or '=' or '-'))
            {
                throw new ArgumentException(
                    $"{what} may only contain [A-Za-z0-9_.,=-], found '{c}' in: {value}");
            }
        }
    }

    public static ProcessStartInfo ToProcessStartInfo(QemuLaunchPlan plan)
    {
        return new ProcessStartInfo
        {
            FileName = plan.BinaryPath,
            Arguments = plan.Arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };
    }
}
