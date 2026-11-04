.global _native_enable_simd
.global _native_arm64_disable_alignment_check
.global _native_arm64_select_sp_el1

.text
.align 4

// Run on SP_EL1 from here on: copies the stack pointer into SP_EL1 and selects it (EL1h), so
// the exceptions this CPU takes stack their frames below the running sp, on the stack it runs
// on, as every scheduler thread's do (ThreadContext starts them with SPSR 0x3C5). Limine enters
// every CPU in EL1t, on SP_EL0, with SP_EL1 left as the firmware had it: on the boot CPU the
// loader's UEFI stack in bootloader-reclaimable memory, on an application processor whatever
// PSCI handed over. Called first thing by kmain on the boot CPU and by the application
// processor trampoline. The return address is in x30, untouched, and the stack's contents do
// not move: only the bank the sp register names changes.
_native_arm64_select_sp_el1:
    mov     x0, sp
    msr     spsel, #1
    mov     sp, x0
    ret

// Enable NEON/FP (SIMD) access
// Sets CPACR_EL1.FPEN bits [21:20] to 0b11 to enable FP/SIMD at EL1 and EL0
_native_enable_simd:
    mrs     x0, cpacr_el1       // Read CPACR_EL1
    orr     x0, x0, #(3 << 20)  // Set FPEN bits [21:20] = 0b11 (full access)
    msr     cpacr_el1, x0       // Write back
    isb                         // Instruction synchronization barrier
    ret

// Disable alignment checking in SCTLR_EL1
// Clears the A bit (bit 1) to allow unaligned accesses
_native_arm64_disable_alignment_check:
    mrs     x0, sctlr_el1       // Read SCTLR_EL1
    bic     x0, x0, #(1 << 1)   // Clear A bit (alignment check)
    msr     sctlr_el1, x0       // Write back
    isb                         // Instruction synchronization barrier
    ret
