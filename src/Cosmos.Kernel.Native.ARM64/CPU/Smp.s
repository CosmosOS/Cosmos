.global _native_cpu_ap_trampoline
.global _native_cpu_ap_trampoline_address
.extern __managed__ap_main
.extern _native_enable_simd
.extern _native_arm64_disable_alignment_check
.extern _native_arm64_init_exception_vectors

// Where the bootloader parks an application processor jumps once the kernel writes the entry's
// goto_address (SMP phase 2, Core/CPU/Smp.cs): x0 points at the CPU's limine_mp_info, sp (SP_EL0,
// EL1t) at the top of a stack of its own (the Stack Size request's size), DAIF is all set,
// VBAR_EL1 is undefined and SP_EL1 is whatever PSCI handed over. The kernel put the CPU's per-CPU
// block address in the entry's extra_argument before the write.
.equ LIMINE_MP_INFO_EXTRA_ARGUMENT, 32

.text
.align 4

// What no managed code can do before identity exists, in the order kmain does it for the boot
// CPU: run EL1h on this stack (exception frames land below sp, not on an undefined SP_EL1), point
// TPIDR_EL1 at this CPU's block (the stubs and GetCurrentCpuId read through it), SIMD on and
// alignment checks off (ILC emits NEON and unaligned accesses), the vector table (a fault from here
// on is dispatched), then __managed__ap_main(limine_mp_info*, boot stack top).
_native_cpu_ap_trampoline:
    msr     daifset, #0xf
    mov     x1, sp
    and     x1, x1, #-16            // the ABI's alignment, whatever the loader left
    msr     spsel, #1
    mov     sp, x1
    ldr     x2, [x0, #LIMINE_MP_INFO_EXTRA_ARGUMENT]
    msr     tpidr_el1, x2
    isb
    mov     x19, x0                 // the helpers clobber x0; x19/x20 survive them
    mov     x20, x1
    bl      _native_enable_simd
    bl      _native_arm64_disable_alignment_check
    bl      _native_arm64_init_exception_vectors
    mov     x0, x19
    mov     x1, x20                 // this CPU's boot stack top
    bl      __managed__ap_main      // never returns
1:
    wfi
    b       1b

// void* _native_cpu_ap_trampoline_address(void)
// The trampoline's address, what the kernel writes into goto_address.
_native_cpu_ap_trampoline_address:
    adrp    x0, _native_cpu_ap_trampoline
    add     x0, x0, :lo12:_native_cpu_ap_trampoline
    ret
