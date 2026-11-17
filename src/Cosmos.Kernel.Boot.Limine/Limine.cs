namespace Cosmos.Kernel.Boot.Limine;

/// <summary>
/// The requests the kernel makes of the Limine bootloader. Each one is a static whose
/// 64-bit ID the loader finds by scanning the loaded image before it jumps to the entry
/// point, and whose <c>Response</c> it fills in then: a request the loader did not answer
/// (a feature the firmware lacks, or a loader too old for it) keeps a null response, and
/// every reader below treats that as the feature being absent rather than an error. The
/// responses point into bootloader-reclaimable memory, which the page allocator never
/// hands out, so they stay readable for the life of the kernel.
/// </summary>
public static class Limine
{
    /// <summary>
    /// The framebuffer the firmware set up (GOP on UEFI, VBE on BIOS): address, geometry,
    /// pixel format and EDID. <c>EarlyGop</c> draws the boot console on it before any
    /// display driver binds, and <c>FirmwareDisplay</c> publishes it as the display the
    /// kit offers until one does. Without a response the kernel runs headless, on the
    /// serial port alone.
    /// </summary>
    public static readonly LimineFramebufferRequest Framebuffer = new();

    /// <summary>
    /// The higher-half direct map: the offset the loader added to every physical address
    /// to map all of memory at once. The page allocator, the device mappers and the GIC
    /// discovery turn physical addresses (MMIO bases, ACPI and device tree pointers) into
    /// virtual ones with it; the native boot code reads it through
    /// <c>__get_limine_hhdm_offset</c> for the early ACPI parse. The loader always answers.
    /// </summary>
    public static readonly LimineHHDMRequest HHDM = new();

    /// <summary>
    /// The physical memory map: sorted, non-overlapping entries typed usable, reserved,
    /// ACPI, bootloader-reclaimable, executable, framebuffer. <c>PageAllocator</c> builds
    /// its free pages from the usable entries alone and leaves the reclaimable ones in
    /// place, which is what keeps every other response (and Limine's own stacks) intact.
    /// The loader always answers; without a map the allocator finds no region for the
    /// heap and throws.
    /// </summary>
    public static readonly LimineMemmapRequest MemoryMap = new();

    /// <summary>
    /// The ACPI RSDP: the root pointer the native boot code hands to <c>acpi_early_init</c>
    /// (through <c>__get_limine_rsdp_address</c>) to find the MADT, the MCFG and the rest.
    /// A firmware without ACPI (the virt machine with ACPI off) leaves it unanswered, and
    /// the ARM64 kernel then reads the PCI host and the virtio-mmio slots from the device
    /// tree instead.
    /// </summary>
    public static readonly LimineRsdpRequest Rsdp = new();

    /// <summary>
    /// The flattened device tree the firmware passed, answered only when there is one:
    /// EDK2 on the virt machine with ACPI off, never q35. <c>BootFirmware</c> parses it,
    /// and the ARM64 platform initializer takes the PCI host's ECAM window and the
    /// virtio-mmio nodes from it (the GIC and the timer sit at the virt machine's fixed
    /// addresses, tree or not).
    /// </summary>
    public static readonly LimineDTBRequest DTB = new();

    /// <summary>
    /// The wall-clock time at boot, in Unix seconds (UTC), read from the firmware's clock.
    /// The RTC drivers (<c>CmosRtc</c>, <c>PL031Rtc</c>) fall back on it when the EFI clock
    /// cannot be read, and <c>KernelRandom</c> stirs it into the entropy pool. A loader
    /// that could not read the clock answers with zero or a negative value, which the
    /// readers treat as no time.
    /// </summary>
    public static readonly LimineBootTimeRequest BootTime = new();

    /// <summary>
    /// The EFI system table, answered on UEFI boots only. Its runtime services stay callable
    /// after <c>ExitBootServices</c>, and <c>EfiRtc</c> reads the firmware clock through
    /// their <c>GetTime</c>, the RTC drivers' first source; a BIOS boot has no table and
    /// the drivers go to their next one.
    /// </summary>
    public static readonly LimineEfiSystemTableRequest EfiSystemTable = new();

    /// <summary>
    /// The command line from <c>cmdline:</c> in limine.conf, as one C string. The native
    /// boot code splits it into <c>argv</c> for the kernel's entry (through
    /// <c>__get_limine_cmd_line</c>), and <see cref="Cmdline"/> exposes it to managed code.
    /// Unanswered or empty, the kernel starts with no arguments.
    /// </summary>
    public static readonly LimineExecutableCmdlineRequest ExecutableCmdline = new();

    /// <summary>
    /// Asks for a boot stack larger than the protocol's 64 KiB default: the whole main
    /// thread runs on it, and <c>EnsureSufficientExecutionStack</c> needs more than
    /// 128 KiB to ever succeed. <c>BootStack.Size</c> reports the requested size when the
    /// loader answered and the default when it did not; the loader gives every CPU it
    /// starts (see <see cref="Mp"/>) a stack of the same size.
    /// </summary>
    public static readonly LimineStackSizeRequest StackSize = new(LimineStackSizeRequest.DefaultRequestedSize);

    /// <summary>
    /// Asks the loader to start every application processor and park it until the kernel
    /// writes its <c>goto_address</c>; the response lists the CPUs with the identity their
    /// interrupt controller knows them by. <c>Smp</c> takes the CPU count from it and brings
    /// the parked CPUs up (SMP phase 2); without a response the kernel has one CPU. The flag
    /// stays clear: with x2APIC the Local APIC's registers move from MMIO to MSRs, and the
    /// kernel's Local APIC code drives the xAPIC MMIO window.
    /// </summary>
    public static readonly LimineMpRequest Mp = new(flags: 0);

    /// <summary>
    /// Pointer to the kernel command line (null-terminated C string) passed
    /// via <c>cmdline:</c> in limine.conf. Returns null if Limine did not
    /// answer the request or the cmdline is unset.
    /// </summary>
    public static unsafe byte* Cmdline
    {
        get
        {
            if (ExecutableCmdline.Response == null)
            {
                return null;
            }
            return ExecutableCmdline.Response->Cmdline;
        }
    }
}
