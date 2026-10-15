// ARM64 Page Table helpers for modifying TTBR1 mappings.
// Used to add Device MMIO entries to Limine's HHDM page tables.

.global _native_arm64_read_ttbr0_el1
.global _native_arm64_read_ttbr1_el1
.global _native_arm64_read_tcr_el1
.global _native_arm64_read_mair_el1
.global _native_arm64_tlbi_vale1
.global _native_arm64_tlbi_all
.global _native_arm64_dc_civac
.global _native_arm64_va_to_pa
.global _native_arm64_spare_l2_table_addr
.global _native_arm64_dsb_isb

// Pre-allocated L2 table (4KiB, 512 entries) for splitting 1GiB L1 blocks.
// Only one is provided — sufficient for a single 1GiB split.
.section .bss
.balign 4096
spare_l2_table:
    .space 4096

.section .text

// ulong _native_arm64_read_ttbr0_el1(void)
// Lower-half translation table root. The kernel heap is identity-mapped, so
// every heap pointer is a low-half VA served by this table; reading TTBR1
// alone makes the whole heap untranslatable.
.balign 4
_native_arm64_read_ttbr0_el1:
    mrs     x0, ttbr0_el1
    ret

// ulong _native_arm64_read_ttbr1_el1(void)
.balign 4
_native_arm64_read_ttbr1_el1:
    mrs     x0, ttbr1_el1
    ret

// ulong _native_arm64_read_tcr_el1(void)
// T0SZ is bits [5:0] and T1SZ is bits [21:16]. Each gives the low/high half's
// virtual-address size as 64 - T0SZ / 64 - T1SZ, i.e. how many bits below the
// sign-extension are actually translated. Reading these instead of assuming
// T1SZ=16 keeps the page-table walk correct whatever the bootloader chose.
.balign 4
_native_arm64_read_tcr_el1:
    mrs     x0, tcr_el1
    ret

// ulong _native_arm64_read_mair_el1(void)
.balign 4
_native_arm64_read_mair_el1:
    mrs     x0, mair_el1
    ret

// void _native_arm64_tlbi_vale1(ulong va_shifted)
// Flushes TLB for the given virtual address at all levels (caller must pass VA >> 12).
.balign 4
_native_arm64_tlbi_vale1:
    tlbi    vae1is, x0
    dsb     sy
    isb
    ret

// void _native_arm64_tlbi_all(void)
// Flushes ALL TLB entries for EL1 (inner shareable).
.balign 4
_native_arm64_tlbi_all:
    tlbi    vmalle1is
    dsb     sy
    isb
    ret

// void _native_arm64_dc_civac(ulong addr)
// Clean and Invalidate data cache line by VA to Point of Coherency.
.balign 4
_native_arm64_dc_civac:
    dc      civac, x0
    dsb     sy
    isb
    ret

// ulong _native_arm64_va_to_pa(ulong va)
// Translates a virtual address to physical via AT S1E1R.
// Returns the 4KiB-aligned physical address, or 0 on translation fault.
.balign 4
_native_arm64_va_to_pa:
    at      s1e1r, x0
    isb
    mrs     x1, par_el1
    tbnz    x1, #0, 1f
    and     x0, x1, #0x0000FFFFFFFFF000
    ret
1:
    mov     x0, #0
    ret

// ulong _native_arm64_spare_l2_table_addr(void)
// Returns the virtual address of the pre-allocated L2 table.
.balign 4
_native_arm64_spare_l2_table_addr:
    adrp    x0, spare_l2_table
    add     x0, x0, :lo12:spare_l2_table
    ret

// void _native_arm64_dsb_isb(void)
// Full DSB SY + ISB barrier sequence.
.balign 4
_native_arm64_dsb_isb:
    dsb     sy
    isb
    ret
