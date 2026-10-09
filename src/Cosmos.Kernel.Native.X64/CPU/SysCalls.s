// x64 SYSCALL trap stubs - hook the SYSCALL instruction up to the managed
// SysCallNative.Dispatch entry (__managed__syscall), mirroring the IRQ stub
// pattern in CPU/Interrupts.s.
//
// SysCallContext frame built here (natural 8-byte alignment, matches the C#
// layout in src/Cosmos.Kernel.Core/SysCalls/SysCallContext.cs):
//   +0x00  uint  Number      (RAX on entry, 32-bit)
//   +0x08  ulong Arg0        (RDI)
//   +0x10  ulong Arg1        (RSI)
//   +0x18  ulong Arg2        (RDX)
//   +0x20  ulong Arg3        (R10 - syscall ABI uses R10, not RCX)
//   +0x28  ulong Arg4        (R8)
//   +0x30  ulong Arg5        (R9)
//   +0x38  ulong Rsp         (user RSP, captured before loading kernel RSP)
//   +0x40  ulong Rip        (RCX - SYSCALL saves user RIP in RCX)
//   +0x48  ulong Rflags     (R11 - SYSCALL saves RFLAGS in R11)
//   +0x50  void*  Thread    (0 - C# resolves via SchedulerManager)
//   total: 88 bytes, padded to 96 for 16-byte alignment before the call.
//
// GDT assumption: the current kernel CS (read at init) is a ring-0 64-bit
// code descriptor with its data descriptor at CS+8. Real ring-3 userland
// return also needs ring-3 code/data descriptors at (CS|3)/(CS+8|3); those
// are supplied by the userland/GDT work. The stub is ready to drive them.

.intel_syntax noprefix

.text

.extern __managed__syscall
.extern _kernel_cr3

// 16 KiB dedicated syscall kernel stack (single CPU; SchedulerManager is
// single-CPU today, see SchedulerManager.GetCurrentCpuId -> 0).
.balign 16
.global _native_x64_syscall_stack
_native_x64_syscall_stack:
    .zero 0x4000

// Per-CPU block reached via KERNEL_GS_BASE after the SYSCALL swapgs. Keep the
// offsets in sync with the gs: loads/stores in syscall_entry.
.balign 8
.global _native_x64_syscall_percpu
_native_x64_syscall_percpu:
    .quad 0      // +0x00  kernel RSP (loaded on entry)
    .quad 0      // +0x08  saved user RSP (restored on sysret)

// Saved user CR3 - restored on the way out (mirrors _irq_saved_cr3 in
// CPU/Interrupts.s). Syscalls never hand off the address space, but we keep
// the round-trip so the kernel CR3 swap matches the IRQ path.
.balign 8
.global _native_x64_syscall_saved_cr3
_native_x64_syscall_saved_cr3:
    .quad 0

// ----------------------------------------------------------------------------
// void _native_x64_init_syscall(void)
// One-time wiring of the SYSCALL fast path. Called once from managed init
// (Global.Init), gated by CosmosFeatures.SysCallsEnabled. Only touches caller-
// saved registers.
// ----------------------------------------------------------------------------
.global _native_x64_init_syscall
_native_x64_init_syscall:
    cli

    // Point the per-CPU block's kernel RSP at the top of the syscall stack.
    lea     rax, [rip + _native_x64_syscall_stack]
    add     rax, 0x4000
    lea     rcx, [rip + _native_x64_syscall_percpu]
    mov     [rcx], rax

    // KERNEL_GS_BASE = &_native_x64_syscall_percpu, so swapgs on entry hands
    // us gs: access to the block.
    lea     rax, [rip + _native_x64_syscall_percpu]
    mov     rdx, rax
    shr     rdx, 32
    mov     rcx, 0xC0000101               // KERNEL_GS_BASE
    wrmsr

    // STAR: SYSCALL CS = kernel code selector (0x08), SS = kernel CS + 8
    //              (= 0x10, the kernel data selector).
    //       SYSRET  CS = user code selector (0x1B, RPL=3), SS = user CS + 8
    //              (= 0x23, the user data selector).
    // Selectors must match the GDT installed by CPU/Gdt.s. STAR[47:32] =
    // SYSCALL CS; STAR[63:48] = SYSRET CS (the CPU adds +8 for SS itself).
    // 64-bit value to write: (0x1BULL << 48) | (0x08ULL << 32).
    mov     rdx, 0x001B
    shl     rdx, 32                          // STAR[63:48] = user CS 0x1B
    mov     rax, 0x0008
    shl     rax, 32                          // STAR[47:32] = kernel CS 0x08
    or      rax, rdx                         // rax = full 64-bit STAR value
    mov     rdx, rax
    shr     rdx, 32                          // EDX = STAR high
                                        // EAX already holds STAR low (0 here)
    xor     eax, eax
    mov     rcx, 0xC0000081                  // STAR
    wrmsr

    // LSTAR = syscall_entry
    lea     rax, [rip + syscall_entry]
    mov     rdx, rax
    shr     rdx, 32
    mov     rcx, 0xC0000082              // LSTAR
    wrmsr

    // FMASK = RFLAGS.IF (0x200) - SYSCALL clears IF on entry.
    xor     rdx, rdx
    mov     rax, 0x200
    mov     rcx, 0xC0000084              // FMASK
    wrmsr

    ret

// ----------------------------------------------------------------------------
// SYSCALL entry point (loaded into LSTAR). On entry the CPU has:
//   - saved user RIP in RCX, user RFLAGS in R11
//   - cleared RFLAGS bits per FMASK
//   - loaded CS/SS from STAR; left RSP unchanged (= user RSP)
//   - RAX = syscall number, RDI/RSI/RDX/R10/R8/R9 = args
// ----------------------------------------------------------------------------
.global syscall_entry
syscall_entry:
    // ----- enter kernel context -----
    swapgs                              // GS := KERNEL_GS_BASE (per-CPU block)
    mov     gs:[8], rsp                 // save user RSP (CPU left it here)
    mov     rsp, gs:[0]                 // load per-CPU kernel RSP (16-aligned)

    // ----- build SysCallContext (88 bytes, padded to 96 for 16B alignment) -----
    sub     rsp, 96
    mov     [rsp + 0], eax              // Number (RAX = syscall number, 32-bit)
    mov     [rsp + 8], rdi              // Arg0
    mov     [rsp + 16], rsi             // Arg1
    mov     [rsp + 24], rdx             // Arg2
    mov     [rsp + 32], r10             // Arg3 (syscall ABI: R10, not RCX)
    mov     [rsp + 40], r8              // Arg4
    mov     [rsp + 48], r9              // Arg5
    mov     rax, gs:[8]                 // user RSP (saved above)
    mov     [rsp + 56], rax             // Rsp
    mov     [rsp + 64], rcx             // Rip (SYSCALL saved user RIP in RCX)
    mov     [rsp + 72], r11             // Rflags (SYSCALL saved RFLAGS in R11)
    mov     qword ptr [rsp + 80], 0     // Thread = 0 (C# resolves the thread)

    // Swap to the kernel page-table root (mirror the IRQ stub). Process
    // page tables only guarantee the shared higher-half is mapped; the
    // dispatcher and its handlers may touch identity-mapped MMIO. Done once
    // the context holds the syscall number, which arrived in RAX.
    mov     rax, cr3
    mov     [rip + _native_x64_syscall_saved_cr3], rax
    mov     rax, [rip + _kernel_cr3]
    test    rax, rax
    jz      .Lsyscall_no_kcr3
    mov     cr3, rax
.Lsyscall_no_kcr3:

    mov     rdi, rsp                    // first arg = &SysCallContext
    call    __managed__syscall          // returns packed long result in RAX

    // ----- return to user -----
    // RAX = result (keep). Restore user GPRs (Linux-like: preserved across
    // syscall), then restore the user CR3 via a scratch we re-fill from the
    // context anyway (RDX is caller-clobbered and reloaded below).
    mov     rdx, [rip + _native_x64_syscall_saved_cr3]
    test    rdx, rdx
    jz      .Lsyscall_cr3_done
    mov     cr3, rdx
    mov     qword ptr [rip + _native_x64_syscall_saved_cr3], 0
.Lsyscall_cr3_done:

    mov     rdi, [rsp + 8]             // restore user Arg0
    mov     rsi, [rsp + 16]
    mov     rdx, [rsp + 24]
    mov     r10, [rsp + 32]
    mov     r8,  [rsp + 40]
    mov     r9,  [rsp + 48]
    mov     rcx, [rsp + 64]            // user RIP (for sysret)
    mov     r11, [rsp + 72]            // user RFLAGS (for sysret)

    add     rsp, 96                    // drop context (+ padding)
    mov     rsp, gs:[8]                // restore user RSP
    swapgs                             // GS := user GS (held in GS_BASE)
    sysret                             // RCX->RIP, R11->RFLAGS, ring-3 CS/SS

// NOTE: FP/SSE (XMM0-15) state is not saved/restored here. The IRQ stub does
// because interrupts are asynchronous; syscalls are synchronous callers and
// can declare XMM clobbered in the syscall ABI until the FP-preservation
// pass lands (parity with Linux needed only once real userland FP is used).