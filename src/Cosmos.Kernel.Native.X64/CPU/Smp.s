.intel_syntax noprefix

.global _native_cpu_ap_trampoline
.global _native_cpu_ap_trampoline_address
.extern __managed__ap_main
.extern _native_enable_simd

// Where the bootloader parks an application processor jumps once the kernel writes the entry's
// goto_address (SMP phase 2, Core/CPU/Smp.cs): rdi points at the CPU's limine_mp_info, rsp at the
// top of a stack of its own (the Stack Size request's size), interrupts are masked, no IDT is
// loaded, and FS/GS bases are 0. The kernel put the CPU's per-CPU block address in the entry's
// extra_argument before the write.
.equ LIMINE_MP_INFO_EXTRA_ARGUMENT, 24
.equ IA32_GS_BASE, 0xC0000101

.text

// What no managed code can do before identity exists: point IA32_GS_BASE at this CPU's block (the
// stubs and GetCurrentCpuId read through it) and enable SIMD (ILC emits SSE even in simple
// functions), then __managed__ap_main(limine_mp_info*, boot stack top). The IDT is loaded from
// managed code first thing there, through the pointer the boot CPU kept: a fault before that
// triple-faults, the same window the boot CPU has between kmain and its own IDT load.
_native_cpu_ap_trampoline:
    cli
    and     rsp, -16                // the ABI's alignment, whatever the loader left
    mov     rsi, rsp                // this CPU's boot stack top, __managed__ap_main's second argument
    mov     rax, [rdi + LIMINE_MP_INFO_EXTRA_ARGUMENT]
    mov     rdx, rax
    shr     rdx, 32
    mov     ecx, IA32_GS_BASE
    wrmsr                           // EDX:EAX into IA32_GS_BASE
    push    rdi
    push    rsi
    call    _native_enable_simd
    pop     rsi
    pop     rdi
    call    __managed__ap_main      // never returns
1:
    hlt
    jmp     1b

// void* _native_cpu_ap_trampoline_address(void)
// The trampoline's address, what the kernel writes into goto_address.
_native_cpu_ap_trampoline_address:
    lea     rax, [rip + _native_cpu_ap_trampoline]
    ret
