// Supervisor Binary Interface calls (RISC-V SBI specification v2.0).
// The SBI firmware (OpenSBI under QEMU) runs in M-mode and serves the
// kernel's ecalls: the timer, inter-processor interrupts, system reset and,
// as a last resort, the debug console.

.global _native_riscv64_sbi_call

.text
.balign 4

// long _native_riscv64_sbi_call(ulong extension, ulong function,
//                               ulong arg0, ulong arg1, ulong arg2,
//                               ulong* value)
//
// In:  a0 = extension ID (EID, goes to a7), a1 = function ID (FID, goes to a6),
//      a2-a4 = arguments 0-2 (go to a0-a2), a5 = where to store the value
//      the call returns in a1, or null.
// Out: a0 = the SBI error code (0 = SBI_SUCCESS).
//
// The SBI preserves every register but a0 and a1, so t0 keeps the value
// pointer across the ecall.
_native_riscv64_sbi_call:
    mv      t0, a5
    mv      a7, a0
    mv      a6, a1
    mv      a0, a2
    mv      a1, a3
    mv      a2, a4
    ecall
    beqz    t0, 1f
    sd      a1, 0(t0)
1:
    ret
