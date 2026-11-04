// RISC-V 64 address translation helpers (Sv39/Sv48/Sv57).

.global _native_riscv64_read_satp
.global _native_riscv64_sfence_vma_all
.global _native_riscv64_sfence_vma
.global _native_riscv64_fence_i

.text
.balign 4

// ulong _native_riscv64_read_satp(void)
// MODE in bits 63:60 (8 = Sv39, 9 = Sv48, 10 = Sv57), the root table's PPN in bits 43:0.
_native_riscv64_read_satp:
    csrr    a0, satp
    ret

// void _native_riscv64_sfence_vma_all(void)
// Orders the page table stores before every later translation, all addresses.
_native_riscv64_sfence_vma_all:
    sfence.vma zero, zero
    ret

// void _native_riscv64_sfence_vma(ulong virtualAddress)
_native_riscv64_sfence_vma:
    sfence.vma a0, zero
    ret

// void _native_riscv64_fence_i(void)
// Orders stores to instruction memory before later instruction fetches.
_native_riscv64_fence_i:
    fence.i
    ret
