.global _native_cpu_halt
.global _native_cpu_disable_interrupts
.global _native_cpu_enable_interrupts
.global _native_cpu_save_irq_and_disable
.global _native_cpu_restore_irq
.global _native_cpu_read_irq_flags
.global _native_cpu_set_block
.global _native_cpu_read_index
.global _native_cpu_block_address

// The per-CPU block (SMP phase 1; Core/CPU/PerCpuBlock.cs is the layout's home): TPIDR_EL1 points
// at the running CPU's, so `mrs xN, tpidr_el1` then a load at a slot's offset reaches it with no
// table walk. The register is written once per CPU, from managed code, when the CPU installs its
// block. ILC's code generation does not use it (inline thread statics would use TPIDR_EL0, and the
// kernel builds without them: thread statics go through RhGetThreadStaticStorage).
.equ PERCPU_CPU_INDEX, 0x08

.text
.align 4

_native_cpu_halt:
    wfi                     // Wait for interrupt
    ret

// Disable interrupts by setting DAIF.I and DAIF.F bits
_native_cpu_disable_interrupts:
    msr     daifset, #3     // Set I and F bits (disable IRQ and FIQ)
    ret

// Enable interrupts by clearing DAIF.I and DAIF.F bits
_native_cpu_enable_interrupts:
    msr     daifclr, #3     // Clear I and F bits (enable IRQ and FIQ)
    ret

// Save DAIF and disable IRQ + FIQ.
// Returns: previous DAIF in x0 (AAPCS64).
_native_cpu_save_irq_and_disable:
    mrs     x0, daif
    msr     daifset, #3
    ret

// Restore DAIF from first argument (x0 on AAPCS64).
_native_cpu_restore_irq:
    msr     daif, x0
    ret

// Read DAIF without changing it: the interrupt state, with no window closed.
// Returns: DAIF in x0 (AAPCS64).
_native_cpu_read_irq_flags:
    mrs     x0, daif
    ret

// void _native_cpu_set_block(void* block)
// Makes `block` the calling CPU's per-CPU block: loads TPIDR_EL1 with its address.
// x0 = block
_native_cpu_set_block:
    msr     tpidr_el1, x0
    isb
    ret

// uint64_t _native_cpu_read_index(void)
// The index of the calling CPU: the register, then one load. Cheap enough for the paths that
// ask on every thread-static access and every allocation.
_native_cpu_read_index:
    mrs     x0, tpidr_el1
    ldr     x0, [x0, #PERCPU_CPU_INDEX]
    ret

// void* _native_cpu_block_address(void)
// The calling CPU's block.
_native_cpu_block_address:
    mrs     x0, tpidr_el1
    ret
