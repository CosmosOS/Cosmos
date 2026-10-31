// Block memory copy and fill for RISC-V 64
//
// The entry points Cosmos.Kernel.Core/Bridge/Import/SimdNative.cs imports.
// The kernel's RISC-V baseline (RV64GC, what ILC targets) has no vector
// unit, so the blocks move through integer registers: 64-bit loads and
// stores when both addresses are 8-byte aligned, single bytes otherwise.
// A misaligned 64-bit access is legal but traps to the SBI firmware, which
// emulates it one access at a time.

.global _simd_copy_16
.global _simd_copy_32
.global _simd_copy_64
.global _simd_copy_128
.global _simd_copy_128_blocks
.global _simd_copy_nt_128_blocks
.global _simd_fill_16_blocks

.text
.balign 4

// Copies a2 bytes (a multiple of 8) from a1 to a0. Clobbers a0-a2, t0-t2.
__cosmos_copy_bytes:
    beqz    a2, 3f
    or      t0, a0, a1
    andi    t0, t0, 7
    bnez    t0, 2f
1:                                  // aligned: 32 bytes per iteration, then 8
    li      t0, 32
    bltu    a2, t0, 4f
    ld      t0, 0(a1)
    ld      t1, 8(a1)
    ld      t2, 16(a1)
    sd      t0, 0(a0)
    sd      t1, 8(a0)
    sd      t2, 16(a0)
    ld      t0, 24(a1)
    sd      t0, 24(a0)
    addi    a0, a0, 32
    addi    a1, a1, 32
    addi    a2, a2, -32
    j       1b
4:
    beqz    a2, 3f
    ld      t0, 0(a1)
    sd      t0, 0(a0)
    addi    a0, a0, 8
    addi    a1, a1, 8
    addi    a2, a2, -8
    j       4b
2:                                  // misaligned: byte by byte
    lbu     t0, 0(a1)
    sb      t0, 0(a0)
    addi    a0, a0, 1
    addi    a1, a1, 1
    addi    a2, a2, -1
    bnez    a2, 2b
3:
    ret

// void _simd_copy_16(void* dest, void* src)
_simd_copy_16:
    li      a2, 16
    j       __cosmos_copy_bytes

// void _simd_copy_32(void* dest, void* src)
_simd_copy_32:
    li      a2, 32
    j       __cosmos_copy_bytes

// void _simd_copy_64(void* dest, void* src)
_simd_copy_64:
    li      a2, 64
    j       __cosmos_copy_bytes

// void _simd_copy_128(void* dest, void* src)
_simd_copy_128:
    li      a2, 128
    j       __cosmos_copy_bytes

// void _simd_copy_128_blocks(void* dest, void* src, int blockCount)
_simd_copy_128_blocks:
    slli    a2, a2, 32
    srli    a2, a2, 32 - 7          // zero-extended count * 128
    j       __cosmos_copy_bytes

// void _simd_copy_nt_128_blocks(void* dest, void* src, int blockCount)
// No non-temporal hint in the base ISA: the same copy.
_simd_copy_nt_128_blocks:
    slli    a2, a2, 32
    srli    a2, a2, 32 - 7
    j       __cosmos_copy_bytes

// void _simd_fill_16_blocks(void* dest, int value, int blockCount)
// Fills blockCount 16-byte blocks with the 32-bit value.
_simd_fill_16_blocks:
    slli    a2, a2, 32
    srli    a2, a2, 32              // zero-extended block count
    beqz    a2, 3f
    slli    t0, a1, 32
    srli    t0, t0, 32              // value in the low half
    slli    t1, t0, 32
    or      t0, t0, t1              // value in both halves
    andi    t1, a0, 7
    bnez    t1, 2f
1:                                  // aligned: two 64-bit stores per block
    sd      t0, 0(a0)
    sd      t0, 8(a0)
    addi    a0, a0, 16
    addi    a2, a2, -1
    bnez    a2, 1b
    ret
2:                                  // 4-byte aligned at least (a 32-bit pattern buffer)
    andi    t1, a0, 3
    bnez    t1, 4f
5:
    sw      a1, 0(a0)
    sw      a1, 4(a0)
    sw      a1, 8(a0)
    sw      a1, 12(a0)
    addi    a0, a0, 16
    addi    a2, a2, -1
    bnez    a2, 5b
    ret
4:                                  // byte-aligned: the pattern a byte at a time
    li      t2, 4
6:
    srli    t1, a1, 0
    sb      t1, 0(a0)
    srli    t1, a1, 8
    sb      t1, 1(a0)
    srli    t1, a1, 16
    sb      t1, 2(a0)
    srli    t1, a1, 24
    sb      t1, 3(a0)
    addi    a0, a0, 4
    addi    t2, t2, -1
    bnez    t2, 6b
    addi    a2, a2, -1
    bnez    a2, 4b
3:
    ret
