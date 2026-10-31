// RISC-V 64 trap entry
//
// Every trap taken in S-mode, exception or interrupt, arrives at the one
// entry stvec names (direct mode). The kernel runs in S-mode only, so a trap
// stays on the stack of the code it interrupted: the frame below is pushed
// there, handed to the managed dispatcher, and popped from whichever stack
// the scheduler picked, then sret.
//
// Frame layout = ThreadContext (Cosmos.Kernel.Core/Scheduler/ThreadContext.RISCV64.cs):
//     0   f0-f31                       (32 * 8)
//   256   fcsr
//   264   (pad)
//   272   IRQContext (Cosmos.Kernel.Core/CPU/IRQContext.cs):
//           x0 (always 0), x1 ra, x2 sp, x3 gp, ... x31 t6   (32 * 8, xN at 8 * N)
//           sepc, sstatus, interrupt, cpu_flags (scause), fault_address (stval)
//   568   (pad to 576, 16-byte aligned)

.global _native_riscv64_trap_vector
.global _native_riscv64_init_trap_vector

.extern __managed__irq
.extern _context_switch_target_sp
.extern _context_switch_is_new_thread

.equ CTX_FP,           0
.equ CTX_FCSR,         256
.equ CTX_IRQ,          272
.equ IRQ_SEPC,         CTX_IRQ + 256
.equ IRQ_SSTATUS,      CTX_IRQ + 264
.equ IRQ_INTERRUPT,    CTX_IRQ + 272
.equ IRQ_CPU_FLAGS,    CTX_IRQ + 280
.equ IRQ_FAULT_ADDR,   CTX_IRQ + 288
.equ CTX_SIZE,         576

// Values the stub writes to IRQContext.interrupt, as the ARM64 stub does
.equ TRAP_KIND_EXCEPTION, 0
.equ TRAP_KIND_INTERRUPT, 1

.section .text
.balign 4
_native_riscv64_trap_vector:
    addi    sp, sp, -CTX_SIZE

    // General-purpose registers, xN at CTX_IRQ + 8 * N (x2, sp, below)
    .irp n, 1,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31
    sd      x\n, (CTX_IRQ + \n * 8)(sp)
    .endr
    sd      zero, (CTX_IRQ + 0)(sp)
    addi    t0, sp, CTX_SIZE            // the interrupted code's sp
    sd      t0, (CTX_IRQ + 2 * 8)(sp)

    // Floating-point registers. sstatus.FS is never Off in this kernel (the
    // boot code turns the unit on and every thread starts with it on).
    .irp n, 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31
    fsd     f\n, (CTX_FP + \n * 8)(sp)
    .endr
    frcsr   t0
    sd      t0, CTX_FCSR(sp)

    // Trap state
    csrr    t0, sepc
    sd      t0, IRQ_SEPC(sp)
    csrr    t0, sstatus
    sd      t0, IRQ_SSTATUS(sp)
    csrr    t0, stval
    sd      t0, IRQ_FAULT_ADDR(sp)
    csrr    t0, scause
    sd      t0, IRQ_CPU_FLAGS(sp)
    srli    t0, t0, 63                  // scause bit 63: 1 = interrupt, 0 = exception
    sd      t0, IRQ_INTERRUPT(sp)

    // Call managed handler: __managed__irq(IRQContext* ctx)
    addi    a0, sp, CTX_IRQ
    call    __managed__irq

    // =====================================================
    // CHECK FOR CONTEXT SWITCH
    // =====================================================
    la      t1, _context_switch_target_sp
    ld      t2, 0(t1)
    beqz    t2, __restore_context       // No context switch requested
    sd      zero, 0(t1)                 // clear the request

    // A new thread's context is restored like a resumed one: ThreadContext
    // gives it sepc = entry, sp = its stack, a0 = its argument and an sstatus
    // whose SPP and SPIE make sret enter it in S-mode with interrupts on.
    la      t1, _context_switch_is_new_thread
    sd      zero, 0(t1)

    // Switch to the new context's stack (t2 points at its saved frame)
    mv      sp, t2

__restore_context:
    ld      t0, IRQ_SEPC(sp)
    csrw    sepc, t0
    // sstatus as saved: SIE stays 0 here, SPIE carries the interrupted
    // code's SIE, which sret puts back
    ld      t0, IRQ_SSTATUS(sp)
    csrw    sstatus, t0

    ld      t0, CTX_FCSR(sp)
    fscsr   t0
    .irp n, 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31
    fld     f\n, (CTX_FP + \n * 8)(sp)
    .endr

    .irp n, 1,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31
    ld      x\n, (CTX_IRQ + \n * 8)(sp)
    .endr
    // sp last: it is the base of every load above
    ld      sp, (CTX_IRQ + 2 * 8)(sp)

    sret

// ============================================================================
// Install the trap entry: stvec = _native_riscv64_trap_vector, direct mode
// void _native_riscv64_init_trap_vector(void)
// ============================================================================
.balign 4
_native_riscv64_init_trap_vector:
    la      t0, _native_riscv64_trap_vector
    csrw    stvec, t0
    ret

// ============================================================================
// Test trap: an ebreak, a breakpoint exception (scause 3)
// void _native_riscv64_test_ebreak(void)
// ============================================================================
.global _native_riscv64_test_ebreak
.balign 4
_native_riscv64_test_ebreak:
    ebreak
    ret
