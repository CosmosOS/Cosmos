.global _native_cpu_halt
.global _native_cpu_disable_interrupts
.global _native_cpu_enable_interrupts
.global _native_cpu_save_irq_and_disable
.global _native_cpu_restore_irq
.global _native_cpu_read_irq_flags

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
