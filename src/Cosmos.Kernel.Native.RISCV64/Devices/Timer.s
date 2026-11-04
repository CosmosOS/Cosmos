// RISC-V 64 timer and interrupt-enable CSRs, in S-mode.
//
// time (CSR 0xC01) counts at the platform's timebase frequency, which the
// device tree gives (/cpus/timebase-frequency; 10 MHz on QEMU virt). The
// timer interrupt is programmed through the SBI TIME extension, or directly
// through stimecmp (CSR 0x14D) when the CPU has Sstc.

.global _native_riscv64_read_time
.global _native_riscv64_write_stimecmp
.global _native_riscv64_read_sie
.global _native_riscv64_set_sie
.global _native_riscv64_clear_sie
.global _native_riscv64_read_sip
.global _native_riscv64_set_sip
.global _native_riscv64_clear_sip

.text
.balign 4

// ulong _native_riscv64_read_time(void)
_native_riscv64_read_time:
    csrr    a0, 0xC01               // time
    ret

// void _native_riscv64_write_stimecmp(ulong value) (Sstc only)
_native_riscv64_write_stimecmp:
    csrw    0x14D, a0               // stimecmp
    ret

// ulong _native_riscv64_read_sie(void)
_native_riscv64_read_sie:
    csrr    a0, sie
    ret

// void _native_riscv64_set_sie(ulong bits): SSIE = 1 << 1, STIE = 1 << 5, SEIE = 1 << 9
_native_riscv64_set_sie:
    csrs    sie, a0
    ret

// void _native_riscv64_clear_sie(ulong bits)
_native_riscv64_clear_sie:
    csrc    sie, a0
    ret

// ulong _native_riscv64_read_sip(void)
_native_riscv64_read_sip:
    csrr    a0, sip
    ret

// void _native_riscv64_set_sip(ulong bits): S-mode may set and clear SSIP only
_native_riscv64_set_sip:
    csrs    sip, a0
    ret

// void _native_riscv64_clear_sip(ulong bits)
_native_riscv64_clear_sip:
    csrc    sip, a0
    ret
