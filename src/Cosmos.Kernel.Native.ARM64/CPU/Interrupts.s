// ARM64 Exception Vector Table
// Follows same pattern as x64 Interrupts.s

.global _native_arm64_exception_vectors
.global _native_arm64_init_exception_vectors

.extern __managed__irq

// The context switch is staged in the running CPU's per-CPU block (Core/CPU/PerCpuBlock.cs, the
// layout's home), reached through TPIDR_EL1, so the vector consumes its own CPU's staging on every exit.
.equ PERCPU_SWITCH_TARGET_SP,     0x18
.equ PERCPU_SWITCH_IS_NEW_THREAD, 0x20
.equ PERCPU_TEMP_IS_NEW_THREAD,   0x28
.equ PERCPU_SWITCH_PREV_ON_CPU,   0x60

// ============================================================================
// Exception Vector Table - must be 2KB aligned for VBAR_EL1
// Each entry is 0x80 (128) bytes
// ============================================================================
.section .text
.balign 0x800
_native_arm64_exception_vectors:

// Current EL with SP0 (4 vectors)
// Each vector saves x0 first, then sets interrupt type
.balign 0x80
    stp     x0, x1, [sp, #-16]! // Save x0, x1 to stack (we'll restore x1 in common)
    mov     x0, #0              // interrupt = SYNC (0)
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #1              // interrupt = IRQ (1)
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #2              // interrupt = FIQ (2)
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #3              // interrupt = SERROR (3)
    b       __exception_common

// Current EL with SPx (kernel mode - this is what we use)
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #0
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #1
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #2
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #3
    b       __exception_common

// Lower EL using AArch64
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #0
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #1
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #2
    b       __exception_common
.balign 0x80
    stp     x0, x1, [sp, #-16]!
    mov     x0, #3
    b       __exception_common

// Lower EL using AArch32 (not supported)
.balign 0x80
    b       .
.balign 0x80
    b       .
.balign 0x80
    b       .
.balign 0x80
    b       .

// ============================================================================
// Common exception handler
// On entry: x0 = interrupt type (0-3)
// Must build IRQContext struct matching C# layout:
//   x0-x30, sp, elr, spsr, interrupt, cpu_flags, far
//   Total: 37 fields × 8 = 296 bytes
// ============================================================================
__exception_common:
    // On entry: x0 = interrupt type, original x0/x1 saved at [sp] by vector entry
    
    // Allocate stack for NEON registers Q0-Q31 (32 * 16 = 512 bytes)
    // Plus IRQContext (296 bytes) = 816 bytes total (aligned)
    sub     sp, sp, #816

    // Save caller-saved registers that we need to use/clobber IMMEDIATELY
    // We use SP offsets because x10 is not set up yet
    str     x9, [sp, #584]      // x9 at offset 512 + 72
    str     x10, [sp, #592]     // x10 at offset 512 + 80
    str     x11, [sp, #600]     // x11 at offset 512 + 88
    str     x12, [sp, #608]     // x12 at offset 512 + 96

    // Now we can use x9, x10, x11, x12 safely

    // Save interrupt type in x9 (callee-saved)
    mov     x9, x0

    // Load original x0, x1 from where vector entry saved them
    // They are at sp + 816 (since we subbed 816)
    // ldp has a limited range (-512 to 504), so use two ldr instead
    ldr     x11, [sp, #816]          // x11 = original x0
    ldr     x12, [sp, #824]          // x12 = original x1

    // Save NEON/SIMD registers Q0-Q31 at bottom of stack (offsets 0-511)
    stp     q0, q1, [sp, #0]
    stp     q2, q3, [sp, #32]
    stp     q4, q5, [sp, #64]
    stp     q6, q7, [sp, #96]
    stp     q8, q9, [sp, #128]
    stp     q10, q11, [sp, #160]
    stp     q12, q13, [sp, #192]
    stp     q14, q15, [sp, #224]
    stp     q16, q17, [sp, #256]
    stp     q18, q19, [sp, #288]
    stp     q20, q21, [sp, #320]
    stp     q22, q23, [sp, #352]
    stp     q24, q25, [sp, #384]
    stp     q26, q27, [sp, #416]
    stp     q28, q29, [sp, #448]
    stp     q30, q31, [sp, #480]

    // IRQContext starts at offset 512
    // Use x10 as base pointer for GPR save area (ldp/stp limit is -512 to 504)
    add     x10, sp, #512

    // Save x0-x30 using x10 as base (offsets 0-240 relative to x10)
    // x11 has original x0, x12 has original x1
    str     x11, [x10, #0]          // x0 (original) at offset 0
    str     x12, [x10, #8]          // x1 (original) at offset 8
    stp     x2, x3, [x10, #16]      // x2,x3 at offset 16,24
    stp     x4, x5, [x10, #32]      // x4,x5 at offset 32,40
    stp     x6, x7, [x10, #48]      // x6,x7 at offset 48,56
    
    // Save x8. x9 is already saved (original) so don't overwrite it!
    str     x8, [x10, #64]          // x8 at offset 64
    
    // x10, x11, x12 are already saved (original). Do NOT overwrite them with xzr!
    
    stp     x13, x14, [x10, #104]   // x13,x14 at offset 104,112
    stp     x15, x16, [x10, #120]   // x15,x16 at offset 120,128
    stp     x17, x18, [x10, #136]   // x17,x18 at offset 136,144
    stp     x19, x20, [x10, #152]   // x19,x20 at offset 152,160
    stp     x21, x22, [x10, #168]   // x21,x22 at offset 168,176
    stp     x23, x24, [x10, #184]   // x23,x24 at offset 184,192
    stp     x25, x26, [x10, #200]   // x25,x26 at offset 200,208
    stp     x27, x28, [x10, #216]   // x27,x28 at offset 216,224
    stp     x29, x30, [x10, #232]   // x29,x30 at offset 232,240

    // Save sp (original sp before we modified it)
    // We subbed 816. The vector pushed 16. So original SP is sp + 832.
    // Every CPU runs EL1h: kmain selects SP_EL1 on the boot CPU before its first frame, the
    // application processor trampoline does the same, and the scheduler's threads start with
    // SPSR 0x3C5. So the interrupted code ran on this very stack and its sp is the frame's base.
    add     x0, sp, #832
    str     x0, [x10, #248]         // sp at offset 248

    // Save elr_el1 (exception return address)
    mrs     x0, elr_el1
    str     x0, [x10, #256]         // elr at offset 256

    // Save spsr_el1
    mrs     x0, spsr_el1
    str     x0, [x10, #264]         // spsr at offset 264

    // Save interrupt type
    str     x9, [x10, #272]         // interrupt at offset 272

    // Save esr_el1 as cpu_flags
    mrs     x0, esr_el1
    str     x0, [x10, #280]         // cpu_flags at offset 280

    // Save far_el1 (fault address register)
    mrs     x0, far_el1
    str     x0, [x10, #288]         // far at offset 288

    // Call managed handler: __managed__irq(IRQContext* ctx)
    mov     x0, x10
    bl      __managed__irq

    // =====================================================
    // CHECK FOR CONTEXT SWITCH
    // Staged in this CPU's per-CPU block (PerCpuBlock.cs), reached through TPIDR_EL1
    // =====================================================
    mrs     x11, tpidr_el1
    ldr     x12, [x11, #PERCPU_SWITCH_TARGET_SP]
    cbz     x12, __restore_context    // No context switch requested

    // Context switch requested - clear the slot
    str     xzr, [x11, #PERCPU_SWITCH_TARGET_SP]

    // Save the is_new_thread flag (we need it after stack switch), then clear it
    ldr     x13, [x11, #PERCPU_SWITCH_IS_NEW_THREAD]
    str     xzr, [x11, #PERCPU_SWITCH_IS_NEW_THREAD]

    // Save is_new_thread to the temp slot
    str     x13, [x11, #PERCPU_TEMP_IS_NEW_THREAD]

    // Switch to new context's stack
    // x12 = new SP (points to start of saved context)
    mov     sp, x12

    // Off the outgoing thread's stack now: clear its on-CPU marker through the
    // staged address with a release store, so the saved context and every
    // field the switch wrote are visible to the CPU that resumes it elsewhere.
    // x11 is still this CPU's block; x14 is restored from the context below.
    ldr     x14, [x11, #PERCPU_SWITCH_PREV_ON_CPU]
    cbz     x14, __restore_context
    stlr    xzr, [x14]
    str     xzr, [x11, #PERCPU_SWITCH_PREV_ON_CPU]

__restore_context:
    // Restore using sp as base (context is at sp)
    // NEON registers are at [sp+0..511]
    // IRQContext is at [sp+512..807]

    // Calculate base pointer for IRQContext
    add     x10, sp, #512

    // Load is_new_thread flag from this CPU's block to check exit path
    mrs     x11, tpidr_el1
    ldr     x13, [x11, #PERCPU_TEMP_IS_NEW_THREAD]

    // Check if this is a new thread
    cbnz    x13, __new_thread_start

    // =====================================================
    // RESUMED THREAD - use eret
    // =====================================================
    // Restore elr_el1 and spsr_el1
    ldr     x0, [x10, #256]
    msr     elr_el1, x0
    ldr     x0, [x10, #264]
    msr     spsr_el1, x0

    // Restore x1-x30 (skip x0 and x10, we'll restore x0 last, x10 is scratch)
    ldr     x1, [x10, #8]
    ldp     x2, x3, [x10, #16]
    ldp     x4, x5, [x10, #32]
    ldp     x6, x7, [x10, #48]
    ldp     x8, x9, [x10, #64]
    // Skip x10 restore (it's our base pointer) - we restore it LAST
    ldp     x11, x12, [x10, #88]
    ldp     x13, x14, [x10, #104]
    ldp     x15, x16, [x10, #120]
    ldp     x17, x18, [x10, #136]
    ldp     x19, x20, [x10, #152]
    ldp     x21, x22, [x10, #168]
    ldp     x23, x24, [x10, #184]
    ldp     x25, x26, [x10, #200]
    ldp     x27, x28, [x10, #216]
    ldp     x29, x30, [x10, #232]

    // Restore x0 last
    ldr     x0, [x10, #0]

    // Restore NEON/SIMD registers Q0-Q31
    ldp     q0, q1, [sp, #0]
    ldp     q2, q3, [sp, #32]
    ldp     q4, q5, [sp, #64]
    ldp     q6, q7, [sp, #96]
    ldp     q8, q9, [sp, #128]
    ldp     q10, q11, [sp, #160]
    ldp     q12, q13, [sp, #192]
    ldp     q14, q15, [sp, #224]
    ldp     q16, q17, [sp, #256]
    ldp     q18, q19, [sp, #288]
    ldp     q20, q21, [sp, #320]
    ldp     q22, q23, [sp, #352]
    ldp     q24, q25, [sp, #384]
    ldp     q26, q27, [sp, #416]
    ldp     q28, q29, [sp, #448]
    ldp     q30, q31, [sp, #480]

    // Restore x10 (which was used as base pointer)
    ldr     x10, [sp, #592]

    // Deallocate stack
    // We subbed 816, and the vector pushed 16. Total 832.
    add     sp, sp, #832

    eret

__new_thread_start:
    // =====================================================
    // NEW THREAD - load SP from context and branch to entry
    // =====================================================
    // Clear the temp flag (x11 still holds this CPU's block)
    str     xzr, [x11, #PERCPU_TEMP_IS_NEW_THREAD]

    // ThreadContext layout (at sp+512):
    //   x0 at offset 0
    //   ...
    //   x30 at offset 240
    //   sp at offset 248 (this is the thread's stack pointer)
    //   elr at offset 256 (this is the entry point)
    //   spsr at offset 264

    // Load the entry point (ELR)
    ldr     x11, [x10, #256]

    // Load the thread's stack pointer
    ldr     x12, [x10, #248]

    // Load SPSR for the new thread
    ldr     x13, [x10, #264]
    msr     spsr_el1, x13

    // Load x0 (first argument for the thread function)
    ldr     x0, [x10, #0]

    // Restore NEON/SIMD registers for the new thread
    ldp     q0, q1, [sp, #0]
    ldp     q2, q3, [sp, #32]
    ldp     q4, q5, [sp, #64]
    ldp     q6, q7, [sp, #96]
    ldp     q8, q9, [sp, #128]
    ldp     q10, q11, [sp, #160]
    ldp     q12, q13, [sp, #192]
    ldp     q14, q15, [sp, #224]
    ldp     q16, q17, [sp, #256]
    ldp     q18, q19, [sp, #288]
    ldp     q20, q21, [sp, #320]
    ldp     q22, q23, [sp, #352]
    ldp     q24, q25, [sp, #384]
    ldp     q26, q27, [sp, #416]
    ldp     q28, q29, [sp, #448]
    ldp     q30, q31, [sp, #480]

    // Set up the new thread's stack pointer
    mov     sp, x12

    // Clear frame pointer for new thread
    mov     x29, #0
    mov     x30, #0

    // Enable interrupts (clear DAIF.I and DAIF.F)
    msr     daifclr, #3

    // Branch to the entry point
    br      x11

// ============================================================================
// Initialize exception vectors - set VBAR_EL1
// void _native_arm64_init_exception_vectors(void)
// ============================================================================
.balign 4
_native_arm64_init_exception_vectors:
    adrp    x0, _native_arm64_exception_vectors
    add     x0, x0, :lo12:_native_arm64_exception_vectors
    msr     vbar_el1, x0
    isb
    ret

// ============================================================================
// Test interrupt trigger - triggers SVC to test exception handling
// void _native_arm64_test_svc(void)
// ============================================================================
.global _native_arm64_test_svc
.balign 4
_native_arm64_test_svc:
    svc     #0
    ret
