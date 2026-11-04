// uACPI kernel API for Cosmos OS (Multi-Architecture)
//
// The host side uACPI calls into: the RSDP, memory mapping through Limine's
// HHDM, the heap, the clock, locks and port/PCI access.
//
// The table API runs from kmain (early table access, see acpi_wrapper.c) and
// only needs the RSDP, map/unmap and the log. The AML interpreter is brought
// up later, from the x64 power-off and reset paths, which run on one CPU with
// interrupts masked. That is what the choices below lean on:
// - the SCI is not routed, so no GPE or fixed event (power button) is ever
//   delivered;
// - deferred work runs synchronously in the caller;
// - sleeps and stalls spin on the monotonic clock;
// - every caller shares one thread id.

#include <stdint.h>
#include <stddef.h>

#include <uacpi/kernel_api.h>
#include <uacpi/status.h>

// Implemented in C# (Cosmos.Kernel.Core/Bridge/Export)
extern void* __cosmos_heap_alloc(size_t size);
extern void __cosmos_heap_free(void* ptr);
extern void __cosmos_serial_write(const char* message);
extern uint64_t __cosmos_get_nanoseconds(void);

// Cosmos.Kernel.Native.X64 / ARM64 (CPU/CpuOps.s, IO/PortIO.s). The port
// functions are stubs on ARM64, which has no port space.
extern uint64_t _native_cpu_save_irq_and_disable(void);
extern void _native_cpu_restore_irq(uint64_t state);
extern uint8_t _native_io_read_byte(uint16_t port);
extern uint16_t _native_io_read_word(uint16_t port);
extern uint32_t _native_io_read_dword(uint16_t port);
extern void _native_io_write_byte(uint16_t port, uint8_t value);
extern void _native_io_write_word(uint16_t port, uint16_t value);
extern void _native_io_write_dword(uint16_t port, uint32_t value);

// acpi_wrapper.c: the ECAM window the MCFG describes, if any.
extern int cosmos_acpi_ecam_lookup(uint16_t segment, uint8_t bus, uint64_t* out_base);

static uint64_t g_rsdp_address = 0;
static uint64_t g_hhdm_offset = 0;

// Called by acpi_early_init() before the first table lookup.
void cosmos_uacpi_set_boot_info(void* rsdp_address, uint64_t hhdm_offset)
{
    g_rsdp_address = (uint64_t)(uintptr_t)rsdp_address;
    g_hhdm_offset = hhdm_offset;
}

// ============================================================================
// Tables and memory
// ============================================================================

uacpi_status uacpi_kernel_get_rsdp(uacpi_phys_addr* out_rsdp_address)
{
    if (g_rsdp_address == 0)
    {
        return UACPI_STATUS_NOT_FOUND;
    }

    // Limine (base revision 0) hands the RSDP over as an HHDM address, uACPI
    // wants the physical one.
    uint64_t rsdp = g_rsdp_address;
    if (g_hhdm_offset != 0 && rsdp >= g_hhdm_offset)
    {
        rsdp -= g_hhdm_offset;
    }

    *out_rsdp_address = rsdp;
    return UACPI_STATUS_OK;
}

// Limine's blanket map (base revision 0) covers the low 4 GiB and every
// memory-map region through the HHDM, which holds the ACPI tables and the
// memory the firmware's AML regions point at.
void* uacpi_kernel_map(uacpi_phys_addr addr, uacpi_size len)
{
    (void)len;
    return (void*)(uintptr_t)(addr + g_hhdm_offset);
}

void uacpi_kernel_unmap(void* addr, uacpi_size len)
{
    (void)addr;
    (void)len;
}

void* uacpi_kernel_alloc(uacpi_size size)
{
    return __cosmos_heap_alloc(size);
}

void uacpi_kernel_free(void* mem)
{
    if (mem != UACPI_NULL)
    {
        __cosmos_heap_free(mem);
    }
}

// ============================================================================
// Logging
// ============================================================================

void uacpi_kernel_log(uacpi_log_level level, const uacpi_char* message)
{
    switch (level)
    {
        case UACPI_LOG_ERROR:
            __cosmos_serial_write("[uACPI ERROR] ");
            break;
        case UACPI_LOG_WARN:
            __cosmos_serial_write("[uACPI WARN] ");
            break;
        default:
            __cosmos_serial_write("[uACPI] ");
            break;
    }

    // Every message already ends with a newline (UACPI_END_OF_LOG_MSG).
    __cosmos_serial_write(message);
}

// ============================================================================
// Time
// ============================================================================

uacpi_u64 uacpi_kernel_get_nanoseconds_since_boot(void)
{
    return __cosmos_get_nanoseconds();
}

static inline void cpu_relax(void)
{
#if defined(__aarch64__)
    __asm__ volatile("yield");
#elif defined(__riscv)
    // Zihintpause's pause, spelled out because -march=rv64gc lacks the
    // extension: a FENCE hint that cores without it run as a no-op.
    __asm__ volatile(".insn i 0x0F, 0, x0, x0, 0x010");
#else
    __asm__ volatile("pause");
#endif
}

static void spin_for_nanoseconds(uint64_t ns)
{
    uint64_t start = __cosmos_get_nanoseconds();
    while (__cosmos_get_nanoseconds() - start < ns)
    {
        cpu_relax();
    }
}

void uacpi_kernel_stall(uacpi_u8 usec)
{
    spin_for_nanoseconds((uint64_t)usec * 1000);
}

// Spins too: the interpreter runs with interrupts masked, so the scheduler
// cannot put the caller to sleep.
void uacpi_kernel_sleep(uacpi_u64 msec)
{
    spin_for_nanoseconds(msec * 1000000);
}

// ============================================================================
// Interrupt state
// ============================================================================

uacpi_interrupt_state uacpi_kernel_disable_interrupts(void)
{
    return _native_cpu_save_irq_and_disable();
}

void uacpi_kernel_restore_interrupts(uacpi_interrupt_state state)
{
    _native_cpu_restore_irq(state);
}

// ============================================================================
// Synchronization
// ============================================================================

typedef struct
{
    volatile uint32_t units;
} cosmos_semaphore;

typedef struct
{
    volatile uint32_t locked;
} cosmos_spinlock;

uacpi_handle uacpi_kernel_create_semaphore(uacpi_u32 initial_units)
{
    cosmos_semaphore* sem = (cosmos_semaphore*)__cosmos_heap_alloc(sizeof(cosmos_semaphore));
    if (sem == UACPI_NULL)
    {
        return UACPI_NULL;
    }
    sem->units = initial_units;
    return sem;
}

void uacpi_kernel_free_semaphore(uacpi_handle handle)
{
    uacpi_kernel_free(handle);
}

static int semaphore_try_take(cosmos_semaphore* sem)
{
    uint32_t units = __atomic_load_n(&sem->units, __ATOMIC_ACQUIRE);
    while (units != 0)
    {
        if (__atomic_compare_exchange_n(&sem->units, &units, units - 1, 0,
                                        __ATOMIC_ACQUIRE, __ATOMIC_ACQUIRE))
        {
            return 1;
        }
    }
    return 0;
}

uacpi_status uacpi_kernel_wait_for_semaphore(uacpi_handle handle, uacpi_u16 timeout)
{
    cosmos_semaphore* sem = (cosmos_semaphore*)handle;
    uint64_t start = __cosmos_get_nanoseconds();
    uint64_t limit = (uint64_t)timeout * 1000000;

    for (;;)
    {
        if (semaphore_try_take(sem))
        {
            return UACPI_STATUS_OK;
        }
        if (timeout == 0)
        {
            return UACPI_STATUS_TIMEOUT;
        }
        if (timeout != 0xFFFF && __cosmos_get_nanoseconds() - start >= limit)
        {
            return UACPI_STATUS_TIMEOUT;
        }
        cpu_relax();
    }
}

void uacpi_kernel_signal_semaphore(uacpi_handle handle)
{
    cosmos_semaphore* sem = (cosmos_semaphore*)handle;
    __atomic_add_fetch(&sem->units, 1, __ATOMIC_RELEASE);
}

// Never UACPI_THREAD_ID_NONE ((void*)-1). One id is enough while every call
// comes from the masked power paths: nothing can preempt them.
uacpi_thread_id uacpi_kernel_get_thread_id(void)
{
    return (uacpi_thread_id)1;
}

uacpi_handle uacpi_kernel_create_spinlock(void)
{
    cosmos_spinlock* lock = (cosmos_spinlock*)__cosmos_heap_alloc(sizeof(cosmos_spinlock));
    if (lock == UACPI_NULL)
    {
        return UACPI_NULL;
    }
    lock->locked = 0;
    return lock;
}

void uacpi_kernel_free_spinlock(uacpi_handle handle)
{
    uacpi_kernel_free(handle);
}

uacpi_cpu_flags uacpi_kernel_lock_spinlock(uacpi_handle handle)
{
    cosmos_spinlock* lock = (cosmos_spinlock*)handle;
    uacpi_cpu_flags flags = _native_cpu_save_irq_and_disable();
    while (__atomic_exchange_n(&lock->locked, 1, __ATOMIC_ACQUIRE) != 0)
    {
        cpu_relax();
    }
    return flags;
}

void uacpi_kernel_unlock_spinlock(uacpi_handle handle, uacpi_cpu_flags flags)
{
    cosmos_spinlock* lock = (cosmos_spinlock*)handle;
    __atomic_store_n(&lock->locked, 0, __ATOMIC_RELEASE);
    _native_cpu_restore_irq(flags);
}

// ============================================================================
// Firmware requests, interrupts and deferred work
// ============================================================================

uacpi_status uacpi_kernel_handle_firmware_request(uacpi_firmware_request* request)
{
    switch (request->type)
    {
        case UACPI_FIRMWARE_REQUEST_TYPE_BREAKPOINT:
            __cosmos_serial_write("[uACPI] AML Breakpoint ignored\n");
            break;
        case UACPI_FIRMWARE_REQUEST_TYPE_FATAL:
            __cosmos_serial_write("[uACPI ERROR] AML Fatal raised by the firmware\n");
            break;
        default:
            break;
    }
    return UACPI_STATUS_OK;
}

// The SCI is not routed (see the top of the file). The handle only has to be
// non-null for uACPI to consider the handler installed.
uacpi_status uacpi_kernel_install_interrupt_handler(
    uacpi_u32 irq, uacpi_interrupt_handler handler, uacpi_handle ctx,
    uacpi_handle* out_irq_handle)
{
    (void)irq;
    (void)handler;
    (void)ctx;
    *out_irq_handle = (uacpi_handle)1;
    return UACPI_STATUS_OK;
}

uacpi_status uacpi_kernel_uninstall_interrupt_handler(
    uacpi_interrupt_handler handler, uacpi_handle irq_handle)
{
    (void)handler;
    (void)irq_handle;
    return UACPI_STATUS_OK;
}

// Work runs right away, so a work item carries nothing; uACPI only needs a
// value other than UACPI_NULL.
uacpi_handle uacpi_kernel_create_work_item(void)
{
    return (uacpi_handle)1;
}

void uacpi_kernel_free_work_item(uacpi_handle work_item)
{
    (void)work_item;
}

void uacpi_kernel_schedule_work(
    uacpi_work_type type, uacpi_handle work_item, uacpi_work_handler handler,
    uacpi_handle ctx)
{
    (void)type;
    (void)work_item;
    handler(ctx);
}

uacpi_status uacpi_kernel_wait_for_work_completion(void)
{
    return UACPI_STATUS_OK;
}

// ============================================================================
// Port I/O
// ============================================================================

// The handle is the base port itself. ARM64 has no port space: mapping fails,
// so the accessors below are never reached there.
uacpi_status uacpi_kernel_io_map(uacpi_io_addr base, uacpi_size len, uacpi_handle* out_handle)
{
#ifdef ARCH_X64
    if (base > 0xFFFF || len > 0x10000 - base)
    {
        return UACPI_STATUS_INVALID_ARGUMENT;
    }
    *out_handle = (uacpi_handle)(uintptr_t)base;
    return UACPI_STATUS_OK;
#else
    (void)base;
    (void)len;
    (void)out_handle;
    return UACPI_STATUS_UNIMPLEMENTED;
#endif
}

void uacpi_kernel_io_unmap(uacpi_handle handle)
{
    (void)handle;
}

#define IO_PORT(handle, offset) ((uint16_t)((uintptr_t)(handle) + (offset)))

uacpi_status uacpi_kernel_io_read8(uacpi_handle handle, uacpi_size offset, uacpi_u8* out_value)
{
    *out_value = _native_io_read_byte(IO_PORT(handle, offset));
    return UACPI_STATUS_OK;
}

uacpi_status uacpi_kernel_io_read16(uacpi_handle handle, uacpi_size offset, uacpi_u16* out_value)
{
    *out_value = _native_io_read_word(IO_PORT(handle, offset));
    return UACPI_STATUS_OK;
}

uacpi_status uacpi_kernel_io_read32(uacpi_handle handle, uacpi_size offset, uacpi_u32* out_value)
{
    *out_value = _native_io_read_dword(IO_PORT(handle, offset));
    return UACPI_STATUS_OK;
}

uacpi_status uacpi_kernel_io_write8(uacpi_handle handle, uacpi_size offset, uacpi_u8 in_value)
{
    _native_io_write_byte(IO_PORT(handle, offset), in_value);
    return UACPI_STATUS_OK;
}

uacpi_status uacpi_kernel_io_write16(uacpi_handle handle, uacpi_size offset, uacpi_u16 in_value)
{
    _native_io_write_word(IO_PORT(handle, offset), in_value);
    return UACPI_STATUS_OK;
}

uacpi_status uacpi_kernel_io_write32(uacpi_handle handle, uacpi_size offset, uacpi_u32 in_value)
{
    _native_io_write_dword(IO_PORT(handle, offset), in_value);
    return UACPI_STATUS_OK;
}

// ============================================================================
// PCI configuration space
// ============================================================================

typedef struct
{
    uacpi_pci_address address;
    // HHDM address of the function's 4 KiB ECAM page, 0 when the MCFG does
    // not cover it.
    uint64_t ecam;
} cosmos_pci_device;

uacpi_status uacpi_kernel_pci_device_open(uacpi_pci_address address, uacpi_handle* out_handle)
{
#ifndef ARCH_X64
    // ARM64 never brings the AML interpreter up, so AML never reaches PCI.
    (void)address;
    (void)out_handle;
    return UACPI_STATUS_NOT_FOUND;
#else
    uint64_t ecam_base = 0;
    int has_ecam = cosmos_acpi_ecam_lookup(address.segment, address.bus, &ecam_base);

    // Configuration mechanism #1 only reaches segment 0.
    if (!has_ecam && address.segment != 0)
    {
        return UACPI_STATUS_NOT_FOUND;
    }

    cosmos_pci_device* device = (cosmos_pci_device*)__cosmos_heap_alloc(sizeof(cosmos_pci_device));
    if (device == UACPI_NULL)
    {
        return UACPI_STATUS_OUT_OF_MEMORY;
    }

    device->address = address;
    device->ecam = 0;
    if (has_ecam)
    {
        device->ecam = ecam_base + g_hhdm_offset
            + ((uint64_t)address.bus << 20)
            + ((uint64_t)address.device << 15)
            + ((uint64_t)address.function << 12);
    }

    *out_handle = device;
    return UACPI_STATUS_OK;
#endif
}

void uacpi_kernel_pci_device_close(uacpi_handle handle)
{
    uacpi_kernel_free(handle);
}

#ifdef ARCH_X64

#define PCI_CONFIG_ADDRESS 0xCF8
#define PCI_CONFIG_DATA    0xCFC

// Selects the dword holding 'offset' through mechanism #1 and returns the data
// port to access it at. Runs with interrupts masked: the address/data pair is
// shared with every other user of the ports.
static uint16_t pci_select(const cosmos_pci_device* device, uacpi_size offset)
{
    uint32_t address = 0x80000000u
        | ((uint32_t)device->address.bus << 16)
        | ((uint32_t)(device->address.device & 0x1F) << 11)
        | ((uint32_t)(device->address.function & 0x07) << 8)
        | ((uint32_t)offset & 0xFC);
    _native_io_write_dword(PCI_CONFIG_ADDRESS, address);
    return (uint16_t)(PCI_CONFIG_DATA + (offset & 3));
}

#endif

// Offsets past the 256-byte legacy space need the ECAM window; the rest uses
// mechanism #1, which needs no mapping at all.
static int pci_use_ecam(const cosmos_pci_device* device, uacpi_size offset)
{
#ifdef ARCH_X64
    return offset >= 0x100 && device->ecam != 0;
#else
    (void)offset;
    return device->ecam != 0;
#endif
}

static uacpi_status pci_check_offset(const cosmos_pci_device* device, uacpi_size offset, uacpi_size width)
{
    if (offset + width > 0x1000)
    {
        return UACPI_STATUS_INVALID_ARGUMENT;
    }
    if (offset + width > 0x100 && device->ecam == 0)
    {
        return UACPI_STATUS_UNIMPLEMENTED;
    }
    return UACPI_STATUS_OK;
}

#define PCI_READ_IMPL(bits, type)                                                       \
    uacpi_status uacpi_kernel_pci_read##bits(uacpi_handle handle, uacpi_size offset,    \
                                             type* value)                               \
    {                                                                                   \
        cosmos_pci_device* device = (cosmos_pci_device*)handle;                         \
        uacpi_status ret = pci_check_offset(device, offset, sizeof(type));              \
        if (ret != UACPI_STATUS_OK)                                                     \
        {                                                                               \
            return ret;                                                                 \
        }                                                                               \
        if (pci_use_ecam(device, offset))                                               \
        {                                                                               \
            *value = *(volatile type*)(uintptr_t)(device->ecam + offset);               \
            return UACPI_STATUS_OK;                                                     \
        }                                                                               \
        PCI_LEGACY_READ(bits, type);                                                    \
        return UACPI_STATUS_OK;                                                         \
    }

#define PCI_WRITE_IMPL(bits, type)                                                      \
    uacpi_status uacpi_kernel_pci_write##bits(uacpi_handle handle, uacpi_size offset,   \
                                              type value)                               \
    {                                                                                   \
        cosmos_pci_device* device = (cosmos_pci_device*)handle;                         \
        uacpi_status ret = pci_check_offset(device, offset, sizeof(type));              \
        if (ret != UACPI_STATUS_OK)                                                     \
        {                                                                               \
            return ret;                                                                 \
        }                                                                               \
        if (pci_use_ecam(device, offset))                                               \
        {                                                                               \
            *(volatile type*)(uintptr_t)(device->ecam + offset) = value;                \
            return UACPI_STATUS_OK;                                                     \
        }                                                                               \
        PCI_LEGACY_WRITE(bits, type);                                                   \
        return UACPI_STATUS_OK;                                                         \
    }

#ifdef ARCH_X64
#define PCI_PORT_READ8 _native_io_read_byte
#define PCI_PORT_READ16 _native_io_read_word
#define PCI_PORT_READ32 _native_io_read_dword
#define PCI_PORT_WRITE8 _native_io_write_byte
#define PCI_PORT_WRITE16 _native_io_write_word
#define PCI_PORT_WRITE32 _native_io_write_dword
#define PCI_LEGACY_READ(bits, type)                                                     \
    do                                                                                  \
    {                                                                                   \
        uacpi_interrupt_state state = _native_cpu_save_irq_and_disable();               \
        *value = PCI_PORT_READ##bits(pci_select(device, offset));                       \
        _native_cpu_restore_irq(state);                                                 \
    } while (0)
#define PCI_LEGACY_WRITE(bits, type)                                                    \
    do                                                                                  \
    {                                                                                   \
        uacpi_interrupt_state state = _native_cpu_save_irq_and_disable();               \
        PCI_PORT_WRITE##bits(pci_select(device, offset), value);                        \
        _native_cpu_restore_irq(state);                                                 \
    } while (0)
#else
// Unreachable: pci_device_open never hands out a device on ARM64.
#define PCI_LEGACY_READ(bits, type) return UACPI_STATUS_UNIMPLEMENTED
#define PCI_LEGACY_WRITE(bits, type) return UACPI_STATUS_UNIMPLEMENTED
#endif

PCI_READ_IMPL(8, uacpi_u8)
PCI_READ_IMPL(16, uacpi_u16)
PCI_READ_IMPL(32, uacpi_u32)
PCI_WRITE_IMPL(8, uacpi_u8)
PCI_WRITE_IMPL(16, uacpi_u16)
PCI_WRITE_IMPL(32, uacpi_u32)
