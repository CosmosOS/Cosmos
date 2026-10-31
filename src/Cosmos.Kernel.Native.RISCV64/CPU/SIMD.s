.global _native_enable_simd

.text
.balign 4

// Turn the floating-point unit on: sstatus.FS (bits 14:13) leaves Off.
// ILC's riscv64 code uses the F and D registers freely, and an FP
// instruction with FS = Off is an illegal instruction trap. The JIT's
// baseline is RV64GC, so there is no vector unit to enable.
_native_enable_simd:
    li      t0, (1 << 13)           // FS = Initial (or Dirty if it was Clean)
    csrs    sstatus, t0
    fscsr   zero                    // round to nearest, no exception flags
    ret
