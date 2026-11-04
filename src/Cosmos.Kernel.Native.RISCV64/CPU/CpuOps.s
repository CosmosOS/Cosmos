// CPU operations for RISC-V 64 in S-mode.
// The interrupt state is sstatus.SIE (bit 1): the "IRQ flags" these return
// and take are sstatus values.

.global _native_cpu_halt
.global _native_cpu_disable_interrupts
.global _native_cpu_enable_interrupts
.global _native_cpu_save_irq_and_disable
.global _native_cpu_restore_irq
.global _native_cpu_read_irq_flags

.equ SSTATUS_SIE, 2

.text
.balign 4

_native_cpu_halt:
    wfi                         // Wait for interrupt
    ret

_native_cpu_disable_interrupts:
    csrci   sstatus, SSTATUS_SIE
    ret

_native_cpu_enable_interrupts:
    csrsi   sstatus, SSTATUS_SIE
    ret

// Save sstatus and disable interrupts.
// Returns: the previous sstatus in a0.
_native_cpu_save_irq_and_disable:
    csrrci  a0, sstatus, SSTATUS_SIE
    ret

// Restore the interrupt state from a saved sstatus (a0): SIE takes the value
// it had, nothing else changes.
_native_cpu_restore_irq:
    andi    t0, a0, SSTATUS_SIE
    csrci   sstatus, SSTATUS_SIE
    csrs    sstatus, t0
    ret

// Read sstatus without changing it.
_native_cpu_read_irq_flags:
    csrr    a0, sstatus
    ret
