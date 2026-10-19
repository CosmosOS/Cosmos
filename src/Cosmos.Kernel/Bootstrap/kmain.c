#include "kmain.h"

// CPU features definition
int g_cpuFeatures = 0;

// Top of the bootloader-provided stack, captured at kmain entry so every
// managed frame lies below it (RhGetCurrentThreadStackBounds, GC stack scan).
static uintptr_t g_bootStackTop = 0;

uintptr_t __cosmos_get_boot_stack_top(void)
{
    return g_bootStackTop;
}

// Entry point
void kmain()
{
    // Capture the boot stack top before anything else runs; kmain's own frame
    // is the highest thing on the Limine-provided stack.
    g_bootStackTop = (uintptr_t)__builtin_frame_address(0);

    // Enable SIMD/XMM FIRST before ANY code execution
    // Without optimizations (-O), ILC generates XMM instructions even in simple functions
    _native_enable_simd();

    // Initialize serial port (115200 baud, 8N1)
    __cosmos_serial_init();

    // === Boot Banner ===
    __cosmos_serial_write("\n");
    __cosmos_serial_write("========================================\n");
    __cosmos_serial_write("  CosmosOS v" COSMOS_VERSION_STRING " (" COSMOS_CODENAME ")\n");
#ifdef __aarch64__
    __cosmos_serial_write("  Architecture: ARM64/AArch64\n");
#else
    __cosmos_serial_write("  Architecture: x86-64\n");
#endif
    __cosmos_serial_write("========================================\n");
    __cosmos_serial_write("\n");

    // === Phase 1: CPU Initialization ===
    __cosmos_serial_write("[KMAIN] Phase 1: CPU initialization\n");

#ifdef __aarch64__
    __cosmos_serial_write("[KMAIN]   - Disabling alignment check (SCTLR_EL1.A)...\n");
    _native_arm64_disable_alignment_check();
    __cosmos_serial_write("[KMAIN]   - Alignment check disabled\n");
#endif

    // === Phase 2: Platform-specific early init ===
    __cosmos_serial_write("\n");
    __cosmos_serial_write("[KMAIN] Phase 2: Platform initialization\n");

    __cosmos_serial_write("[KMAIN]   - Querying Limine for RSDP...\n");
    void* rsdp_address = __get_limine_rsdp_address();
    uint64_t hhdm_offset = __get_limine_hhdm_offset();

    if (rsdp_address != 0)
    {
        __cosmos_serial_write("[KMAIN]   - RSDP found at: 0x");
        __cosmos_serial_write_hex_u64((uint64_t)rsdp_address);
        __cosmos_serial_write("\n");
        __cosmos_serial_write("[KMAIN]   - HHDM offset: 0x");
        __cosmos_serial_write_hex_u64(hhdm_offset);
        __cosmos_serial_write("\n");

        __cosmos_serial_write("[KMAIN]   - Initializing ACPI...\n");
        acpi_early_init(rsdp_address, hhdm_offset);
        __cosmos_serial_write("[KMAIN]   - ACPI initialized\n");
    }
    else
    {
        __cosmos_serial_write("[KMAIN]   - WARNING: RSDP not found!\n");
    }

    // === Phase 3: Managed Kernel Initialization ===
    __cosmos_serial_write("\n");
    __cosmos_serial_write("[KMAIN] Phase 3: Managed kernel initialization\n");
    RhpRegisterOsModule(__kernel_start);
    __managed__Startup();

    // === Phase 4: User Kernel ===
    __cosmos_serial_write("\n");
    __cosmos_serial_write("[KMAIN] Phase 4: User kernel\n");
    
    int argc;
    char **argv;

    argv = __build_argv(__get_limine_cmd_line(), &argc);
    __managed__Main(argc, argv);

    // Should never reach here
    __cosmos_serial_write("[KMAIN] ERROR: Main() returned unexpectedly!\n");
    while(1) {}
}

// Return the size of the '__modules' section and
// populates 'modules' with a pointer to the start of the section.
size_t GetModules(void** modules)
{
    *modules = __Modules_start;
    return __Modules_end - __Modules_start;
}

size_t GetTlsRegion(void** tlsStart){
    *tlsStart = __tdata_start;
    return __tdata_end - __tdata_start;
}