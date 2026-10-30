// Exception Handling Assembly Stubs for ARM64 (AAPCS64)
// Implements the low-level exception dispatching for NativeAOT

.section .text

// External managed functions
.extern RhThrowEx                    // C# exception dispatcher

// The head of the running thread's ExInfo chain is reached through the per-CPU block
// (Core/CPU/PerCpuBlock.cs, the layout's home): its CurrentExInfoHead slot holds the address of
// the running thread's chain head, kept current by the scheduler's switch path, and the boot
// code's before a thread is current. TPIDR_EL1 then one load, no callout.
.equ PERCPU_CURRENT_EXINFO_HEAD,  0x38

//=============================================================================
// Structure offsets
//=============================================================================

// ExInfo offsets
.equ SIZEOF__ExInfo,                  0x190
.equ OFFSETOF__ExInfo__m_pPrevExInfo, 0x00
.equ OFFSETOF__ExInfo__m_pExContext,  0x08
.equ OFFSETOF__ExInfo__m_exception,   0x10
.equ OFFSETOF__ExInfo__m_kind,        0x18
.equ OFFSETOF__ExInfo__m_passNumber,  0x19
.equ OFFSETOF__ExInfo__m_idxCurClause, 0x1c

// PAL_LIMITED_CONTEXT offsets for ARM64
.equ SIZEOF__PAL_LIMITED_CONTEXT,     0x70
.equ OFFSETOF__PAL_LIMITED_CONTEXT__SP,  0x00
.equ OFFSETOF__PAL_LIMITED_CONTEXT__IP,  0x08
.equ OFFSETOF__PAL_LIMITED_CONTEXT__FP,  0x10
.equ OFFSETOF__PAL_LIMITED_CONTEXT__LR,  0x18
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X19, 0x20
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X20, 0x28
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X21, 0x30
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X22, 0x38
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X23, 0x40
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X24, 0x48
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X25, 0x50
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X26, 0x58
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X27, 0x60
.equ OFFSETOF__PAL_LIMITED_CONTEXT__X28, 0x68

// REGDISPLAY offsets for ARM64 (direct values, not pointers)
.equ OFFSETOF__REGDISPLAY__SP,        0x00
.equ OFFSETOF__REGDISPLAY__FP,        0x08
.equ OFFSETOF__REGDISPLAY__X19,       0x10
.equ OFFSETOF__REGDISPLAY__X20,       0x18
.equ OFFSETOF__REGDISPLAY__X21,       0x20
.equ OFFSETOF__REGDISPLAY__X22,       0x28
.equ OFFSETOF__REGDISPLAY__X23,       0x30
.equ OFFSETOF__REGDISPLAY__X24,       0x38
.equ OFFSETOF__REGDISPLAY__X25,       0x40
.equ OFFSETOF__REGDISPLAY__X26,       0x48
.equ OFFSETOF__REGDISPLAY__X27,       0x50
.equ OFFSETOF__REGDISPLAY__X28,       0x58
.equ OFFSETOF__REGDISPLAY__LR,        0x60

// ExKind enum
.equ ExKind_Throw,        1

// Stack size for ExInfo (aligned to 16 bytes)
.equ STACKSIZEOF_ExInfo,  ((SIZEOF__ExInfo + 15) & ~15)

//=============================================================================
// RhpThrowEx - Entry point for throwing a managed exception
//
// INPUT:  x0 = exception object
//
// This is called by the ILC-generated code when a 'throw' statement executes.
// AAPCS64: x0-x7 = arguments, x19-x28 = callee-saved
//
// CFI: the prologue (the PAL_LIMITED_CONTEXT this builds on the stack, then the ExInfo space) is
// described so the precise GC stack scan / exception unwinder (issue #346) can step from inside the
// dispatcher up through this stub into the throwing method. The post-`bl RhThrowEx` IP is the only
// CFI target here — RhThrowEx transfers to a handler or halts, it never returns — so describing just
// the prologue is sufficient. x19 (clobbered below) and x30 (clobbered by `bl RhThrowEx`) come from
// their saved slots; x9/x10 are scratch.
//=============================================================================
.global RhpThrowEx
.balign 4
.cfi_startproc
RhpThrowEx:
    // Save the SP of the throw site and LR (return address)
    mov     x9, sp                      // x9 = original SP at throw site
    mov     x10, lr                     // x10 = return address (throw site IP)

    // Allocate space for PAL_LIMITED_CONTEXT (0x70 bytes, 16-byte aligned)
    sub     sp, sp, #SIZEOF__PAL_LIMITED_CONTEXT
    .cfi_adjust_cfa_offset SIZEOF__PAL_LIMITED_CONTEXT

    // Build PAL_LIMITED_CONTEXT structure on stack
    str     x9, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__SP]   // SP
    str     x10, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__IP]  // IP (return address)
    str     x29, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__FP]  // FP
    .cfi_rel_offset x29, OFFSETOF__PAL_LIMITED_CONTEXT__FP
    str     x30, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__LR]  // LR
    .cfi_rel_offset x30, OFFSETOF__PAL_LIMITED_CONTEXT__LR
    str     x19, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X19]
    .cfi_rel_offset x19, OFFSETOF__PAL_LIMITED_CONTEXT__X19
    str     x20, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X20]
    .cfi_rel_offset x20, OFFSETOF__PAL_LIMITED_CONTEXT__X20
    str     x21, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X21]
    .cfi_rel_offset x21, OFFSETOF__PAL_LIMITED_CONTEXT__X21
    str     x22, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X22]
    .cfi_rel_offset x22, OFFSETOF__PAL_LIMITED_CONTEXT__X22
    str     x23, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X23]
    .cfi_rel_offset x23, OFFSETOF__PAL_LIMITED_CONTEXT__X23
    str     x24, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X24]
    .cfi_rel_offset x24, OFFSETOF__PAL_LIMITED_CONTEXT__X24
    str     x25, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X25]
    .cfi_rel_offset x25, OFFSETOF__PAL_LIMITED_CONTEXT__X25
    str     x26, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X26]
    .cfi_rel_offset x26, OFFSETOF__PAL_LIMITED_CONTEXT__X26
    str     x27, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X27]
    .cfi_rel_offset x27, OFFSETOF__PAL_LIMITED_CONTEXT__X27
    str     x28, [sp, #OFFSETOF__PAL_LIMITED_CONTEXT__X28]
    .cfi_rel_offset x28, OFFSETOF__PAL_LIMITED_CONTEXT__X28

    // Save exception object in callee-saved register
    mov     x19, x0                     // x19 = exception object

    // Allocate space for ExInfo
    sub     sp, sp, #STACKSIZEOF_ExInfo
    .cfi_adjust_cfa_offset STACKSIZEOF_ExInfo

    // x4 = &head of this thread's ExInfo chain: TPIDR_EL1 then one load through the per-CPU block.
    mrs     x4, tpidr_el1
    ldr     x4, [x4, #PERCPU_CURRENT_EXINFO_HEAD]

    // x1 = ExInfo* (current SP)
    mov     x1, sp

    // Initialize ExInfo fields
    str     x19, [x1, #OFFSETOF__ExInfo__m_exception]      // exception object — RhpRethrow reads it back
    mov     w2, #1
    strb    w2, [x1, #OFFSETOF__ExInfo__m_passNumber]      // passNumber = 1
    mov     w2, #0xFFFFFFFF
    str     w2, [x1, #OFFSETOF__ExInfo__m_idxCurClause]    // idxCurClause = -1
    mov     w2, #ExKind_Throw
    strb    w2, [x1, #OFFSETOF__ExInfo__m_kind]            // kind = Throw

    // Link ExInfo into this thread's exception chain
    ldr     x3, [x4]                                        // x3 = current head
    str     x3, [x1, #OFFSETOF__ExInfo__m_pPrevExInfo]     // pExInfo->m_pPrevExInfo = head
    str     x1, [x4]                                        // head = pExInfo

    // Set the exception context pointer
    add     x3, sp, #STACKSIZEOF_ExInfo                    // x3 = PAL_LIMITED_CONTEXT*
    str     x3, [x1, #OFFSETOF__ExInfo__m_pExContext]

    // Call managed exception handler
    // x0 = exception object (restore from x19)
    // x1 = ExInfo* (already set)
    mov     x0, x19
    bl      RhThrowEx

    // If we return, something went wrong (should never happen)
    brk     #0
    b       .
.cfi_endproc

//=============================================================================
// RhpCallCatchFunclet - Call a catch handler funclet
//
// INPUT:  x0 = exception object
//         x1 = handler funclet address
//         x2 = REGDISPLAY*
//         x3 = ExInfo*
//
// OUTPUT: x0 = resume address (where to continue after catch)
//
// The catch funclet expects the exception object in x0.
// After the funclet returns, we resume at the address it returns.
//
// CFI: the prologue is described so the precise GC stack scan / exception unwinder (issue #346)
// can step from inside the funclet up through this trampoline into the dispatcher's managed frame.
// The CFA stays SP-based (not switched to x29) on purpose — the body reloads x29 from REGDISPLAY,
// so an x29-based CFA would be wrong at the post-`blr` IP, which is the only CFI target here. The
// `mov sp, x11 / br x9` resume tail runs strictly after that point.
//=============================================================================
.global RhpCallCatchFunclet
.balign 4
.cfi_startproc
RhpCallCatchFunclet:
    // Save callee-saved registers (like x64 does). No `mov x29, sp` frame-pointer setup:
    // x29 is reloaded from REGDISPLAY below for the funclet call, so it would be dead — and the
    // CFA is kept SP-based anyway (see the note above).
    stp     x29, x30, [sp, #-0x80]!
    .cfi_def_cfa_offset 0x80
    .cfi_rel_offset x29, 0
    .cfi_rel_offset x30, 8
    stp     x19, x20, [sp, #0x10]
    .cfi_rel_offset x19, 0x10
    .cfi_rel_offset x20, 0x18
    stp     x21, x22, [sp, #0x20]
    .cfi_rel_offset x21, 0x20
    .cfi_rel_offset x22, 0x28
    stp     x23, x24, [sp, #0x30]
    .cfi_rel_offset x23, 0x30
    .cfi_rel_offset x24, 0x38
    stp     x25, x26, [sp, #0x40]
    .cfi_rel_offset x25, 0x40
    .cfi_rel_offset x26, 0x48
    stp     x27, x28, [sp, #0x50]
    .cfi_rel_offset x27, 0x50
    .cfi_rel_offset x28, 0x58

    // Save arguments on stack
    str     x0, [sp, #0x60]             // exception object
    str     x1, [sp, #0x68]             // handler address
    str     x2, [sp, #0x70]             // REGDISPLAY*
    str     x3, [sp, #0x78]             // ExInfo*

    // Restore callee-saved registers from REGDISPLAY
    // These are the registers the funclet expects to have from the throwing method
    // x2 = REGDISPLAY*

    ldr     x29, [x2, #OFFSETOF__REGDISPLAY__FP]
    ldr     x19, [x2, #OFFSETOF__REGDISPLAY__X19]
    ldr     x20, [x2, #OFFSETOF__REGDISPLAY__X20]
    ldr     x21, [x2, #OFFSETOF__REGDISPLAY__X21]
    ldr     x22, [x2, #OFFSETOF__REGDISPLAY__X22]
    ldr     x23, [x2, #OFFSETOF__REGDISPLAY__X23]
    ldr     x24, [x2, #OFFSETOF__REGDISPLAY__X24]
    ldr     x25, [x2, #OFFSETOF__REGDISPLAY__X25]
    ldr     x26, [x2, #OFFSETOF__REGDISPLAY__X26]
    ldr     x27, [x2, #OFFSETOF__REGDISPLAY__X27]
    ldr     x28, [x2, #OFFSETOF__REGDISPLAY__X28]

    // Load exception object and call handler
    ldr     x0, [sp, #0x60]             // exception object
    ldr     x8, [sp, #0x68]             // handler address
    blr     x8                          // call handler funclet

    // x0 = the resume address. The head of this thread's ExInfo chain is TPIDR_EL1 then one load
    // through the per-CPU block (the funclet may have switched CPUs, under SMP, so it is read
    // now, not before the call).
    mov     x9, x0                      // x9 = resume address
    mrs     x8, tpidr_el1
    ldr     x8, [x8, #PERCPU_CURRENT_EXINFO_HEAD]   // x8 = &head of this thread's ExInfo chain

    // Reload REGDISPLAY* from stack (x2 may have been clobbered)
    ldr     x2, [sp, #0x70]             // REGDISPLAY*

    // Get resume SP from REGDISPLAY (this is the handler frame's FP)
    ldr     x11, [x2, #OFFSETOF__REGDISPLAY__SP]   // x11 = handler frame's FP

    // Pop ExInfo entries that are below the resume SP
.pop_exinfo_loop:
    ldr     x10, [x8]                   // current ExInfo
    cbz     x10, .pop_exinfo_done       // null = done
    cmp     x10, x11
    b.ge    .pop_exinfo_done            // >= resume SP = done
    ldr     x10, [x10, #OFFSETOF__ExInfo__m_pPrevExInfo]
    str     x10, [x8]                   // pop it
    b       .pop_exinfo_loop

.pop_exinfo_done:
    // Resume execution after the catch block
    // x9 contains the resume address returned by the funclet
    // x11 contains the handler frame's FP (from REGDISPLAY.SP)

    // Restore handler's Frame Pointer
    mov     x29, x11

    // Restore handler's Stack Pointer (assuming SP == FP in handler frame)
    mov     sp, x11

    // Jump to the resume address
    br      x9
.cfi_endproc


//=============================================================================
// RhpCallFilterFunclet - Call a filter funclet to evaluate exception filter
//
// INPUT:  x0 = exception object
//         x1 = filter funclet address
//         x2 = REGDISPLAY*
//
// OUTPUT: x0 = 1 if filter matched (should catch), 0 if not
//
// The filter funclet expects the exception object in x0.
// It returns non-zero if the exception should be caught by this handler.
//
// CFI: see the note on RhpCallCatchFunclet. The epilogue here is the ordinary 5x ldp + indexed
// ldp + ret; GC cannot fire in it, so prologue CFI is sufficient.
//=============================================================================
.global RhpCallFilterFunclet
.balign 4
.cfi_startproc
RhpCallFilterFunclet:
    // Save callee-saved registers and allocate space for arguments. No `mov x29, sp`:
    // x29 is reloaded from REGDISPLAY below, so it would be dead; CFA stays SP-based.
    stp     x29, x30, [sp, #-0x70]!
    .cfi_def_cfa_offset 0x70
    .cfi_rel_offset x29, 0
    .cfi_rel_offset x30, 8
    stp     x19, x20, [sp, #0x10]
    .cfi_rel_offset x19, 0x10
    .cfi_rel_offset x20, 0x18
    stp     x21, x22, [sp, #0x20]
    .cfi_rel_offset x21, 0x20
    .cfi_rel_offset x22, 0x28
    stp     x23, x24, [sp, #0x30]
    .cfi_rel_offset x23, 0x30
    .cfi_rel_offset x24, 0x38
    stp     x25, x26, [sp, #0x40]
    .cfi_rel_offset x25, 0x40
    .cfi_rel_offset x26, 0x48
    stp     x27, x28, [sp, #0x50]
    .cfi_rel_offset x27, 0x50
    .cfi_rel_offset x28, 0x58

    // Save arguments on stack (funclet may clobber callee-saved registers)
    str     x0, [sp, #0x60]             // Save exception object
    str     x1, [sp, #0x68]             // Save filter address

    // Restore callee-saved registers from REGDISPLAY BEFORE calling funclet
    // The funclet expects these registers to have the values from the method
    // x2 = REGDISPLAY*

    ldr     x29, [x2, #OFFSETOF__REGDISPLAY__FP]
    ldr     x19, [x2, #OFFSETOF__REGDISPLAY__X19]
    ldr     x20, [x2, #OFFSETOF__REGDISPLAY__X20]
    ldr     x21, [x2, #OFFSETOF__REGDISPLAY__X21]
    ldr     x22, [x2, #OFFSETOF__REGDISPLAY__X22]
    ldr     x23, [x2, #OFFSETOF__REGDISPLAY__X23]
    ldr     x24, [x2, #OFFSETOF__REGDISPLAY__X24]
    ldr     x25, [x2, #OFFSETOF__REGDISPLAY__X25]
    ldr     x26, [x2, #OFFSETOF__REGDISPLAY__X26]
    ldr     x27, [x2, #OFFSETOF__REGDISPLAY__X27]
    ldr     x28, [x2, #OFFSETOF__REGDISPLAY__X28]

    // Call filter funclet
    // Load exception object and filter address from stack (registers may have been restored)
    ldr     x0, [sp, #0x60]             // exception object
    ldr     x8, [sp, #0x68]             // filter address
    blr     x8                          // call filter funclet

    // x0 now contains the filter result (0 = no match, non-zero = match)
    mov     x9, x0                      // save result

    // Restore our callee-saved registers from stack
    ldp     x27, x28, [sp, #0x50]
    ldp     x25, x26, [sp, #0x40]
    ldp     x23, x24, [sp, #0x30]
    ldp     x21, x22, [sp, #0x20]
    ldp     x19, x20, [sp, #0x10]
    ldp     x29, x30, [sp], #0x70

    // Return filter result
    mov     x0, x9
    ret
.cfi_endproc


//=============================================================================
// RhpCallFinallyFunclet - Call a finally (or fault) funclet while an exception unwinds its frame
//
// INPUT:  x0 = handler funclet address
//         x1 = REGDISPLAY*
//
// The funclet runs with the frame's FP/x19-x28 (restored below) and returns here, and this stub
// returns to the dispatcher, which goes on with the next clause or frame. The funclet keeps the
// callee-saved registers as it found them, so nothing is written back to the REGDISPLAY.
//
// CFI: see the note on RhpCallCatchFunclet. The epilogue here is the ordinary 5x ldp + indexed
// ldp + ret; GC cannot fire in it, so prologue CFI is sufficient.
//=============================================================================
.global RhpCallFinallyFunclet
.balign 4
.cfi_startproc
RhpCallFinallyFunclet:
    // Save callee-saved registers. No `mov x29, sp`: x29 is reloaded from REGDISPLAY below, so it
    // would be dead; CFA stays SP-based.
    stp     x29, x30, [sp, #-0x60]!
    .cfi_def_cfa_offset 0x60
    .cfi_rel_offset x29, 0
    .cfi_rel_offset x30, 8
    stp     x19, x20, [sp, #0x10]
    .cfi_rel_offset x19, 0x10
    .cfi_rel_offset x20, 0x18
    stp     x21, x22, [sp, #0x20]
    .cfi_rel_offset x21, 0x20
    .cfi_rel_offset x22, 0x28
    stp     x23, x24, [sp, #0x30]
    .cfi_rel_offset x23, 0x30
    .cfi_rel_offset x24, 0x38
    stp     x25, x26, [sp, #0x40]
    .cfi_rel_offset x25, 0x40
    .cfi_rel_offset x26, 0x48
    stp     x27, x28, [sp, #0x50]
    .cfi_rel_offset x27, 0x50
    .cfi_rel_offset x28, 0x58

    // Restore the frame's callee-saved registers from REGDISPLAY (x1); x8 keeps the handler address
    mov     x8, x0
    ldr     x29, [x1, #OFFSETOF__REGDISPLAY__FP]
    ldr     x19, [x1, #OFFSETOF__REGDISPLAY__X19]
    ldr     x20, [x1, #OFFSETOF__REGDISPLAY__X20]
    ldr     x21, [x1, #OFFSETOF__REGDISPLAY__X21]
    ldr     x22, [x1, #OFFSETOF__REGDISPLAY__X22]
    ldr     x23, [x1, #OFFSETOF__REGDISPLAY__X23]
    ldr     x24, [x1, #OFFSETOF__REGDISPLAY__X24]
    ldr     x25, [x1, #OFFSETOF__REGDISPLAY__X25]
    ldr     x26, [x1, #OFFSETOF__REGDISPLAY__X26]
    ldr     x27, [x1, #OFFSETOF__REGDISPLAY__X27]
    ldr     x28, [x1, #OFFSETOF__REGDISPLAY__X28]

    blr     x8                          // call finally funclet

    // Restore our callee-saved registers from stack
    ldp     x27, x28, [sp, #0x50]
    ldp     x25, x26, [sp, #0x40]
    ldp     x23, x24, [sp, #0x30]
    ldp     x21, x22, [sp, #0x20]
    ldp     x19, x20, [sp, #0x10]
    ldp     x29, x30, [sp], #0x60
    ret
.cfi_endproc


//=============================================================================
// RhpRethrow - Rethrow the current exception
//
// Called when a 'throw;' statement (without exception object) is executed.
//=============================================================================
.global RhpRethrow
.balign 4
RhpRethrow:
    // The exception to rethrow is the one at the head of this thread's ExInfo chain: TPIDR_EL1
    // then one load through the per-CPU block. LR, the rethrow site RhpThrowEx records, is kept.
    mrs     x0, tpidr_el1
    ldr     x0, [x0, #PERCPU_CURRENT_EXINFO_HEAD]
    ldr     x0, [x0]
    cbz     x0, .halt

    // Get exception object from ExInfo
    ldr     x0, [x0, #OFFSETOF__ExInfo__m_exception]
    cbz     x0, .halt

    // Rethrow using normal throw path
    b       RhpThrowEx

.halt:
    brk     #0
    b       .
