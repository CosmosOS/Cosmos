// Exception Handling Assembly Stubs for RISC-V 64 (LP64D)
// Implements the low-level exception dispatching for NativeAOT
//
// Registers: a0-a7 = arguments, s0 (fp) and s1-s11 = callee-saved, ra = return address.

.section .text

// External managed functions
.extern RhThrowEx                    // C# exception dispatcher
.extern __cosmos_exinfo_head         // &head of the running thread's ExInfo chain (ExceptionNative)

//=============================================================================
// Structure offsets
//=============================================================================

// ExInfo offsets (the kernel's own record, the same on every architecture)
.equ SIZEOF__ExInfo,                  0x190
.equ OFFSETOF__ExInfo__m_pPrevExInfo, 0x00
.equ OFFSETOF__ExInfo__m_pExContext,  0x08
.equ OFFSETOF__ExInfo__m_exception,   0x10
.equ OFFSETOF__ExInfo__m_kind,        0x18
.equ OFFSETOF__ExInfo__m_passNumber,  0x19
.equ OFFSETOF__ExInfo__m_idxCurClause, 0x1c

// PAL_LIMITED_CONTEXT offsets for RISC-V 64: they must match RegisterContext.RISCV64.cs
.equ SIZEOF__PAL_LIMITED_CONTEXT,     0x78
.equ STACKSIZEOF_PAL_LIMITED_CONTEXT, 0x80
.equ OFFSETOF__PAL_LIMITED_CONTEXT__SP,  0x00
.equ OFFSETOF__PAL_LIMITED_CONTEXT__IP,  0x08
.equ OFFSETOF__PAL_LIMITED_CONTEXT__FP,  0x10
.equ OFFSETOF__PAL_LIMITED_CONTEXT__RA,  0x18
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S1,  0x20
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S2,  0x28
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S3,  0x30
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S4,  0x38
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S5,  0x40
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S6,  0x48
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S7,  0x50
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S8,  0x58
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S9,  0x60
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S10, 0x68
.equ OFFSETOF__PAL_LIMITED_CONTEXT__S11, 0x70

// REGDISPLAY offsets for RISC-V 64 (direct values, not pointers): RegisterContext.RISCV64.cs
.equ OFFSETOF__REGDISPLAY__SP,        0x00
.equ OFFSETOF__REGDISPLAY__FP,        0x08
.equ OFFSETOF__REGDISPLAY__S1,        0x10
.equ OFFSETOF__REGDISPLAY__S2,        0x18
.equ OFFSETOF__REGDISPLAY__S3,        0x20
.equ OFFSETOF__REGDISPLAY__S4,        0x28
.equ OFFSETOF__REGDISPLAY__S5,        0x30
.equ OFFSETOF__REGDISPLAY__S6,        0x38
.equ OFFSETOF__REGDISPLAY__S7,        0x40
.equ OFFSETOF__REGDISPLAY__S8,        0x48
.equ OFFSETOF__REGDISPLAY__S9,        0x50
.equ OFFSETOF__REGDISPLAY__S10,       0x58
.equ OFFSETOF__REGDISPLAY__S11,       0x60
.equ OFFSETOF__REGDISPLAY__RA,        0x68

// ExKind enum
.equ ExKind_Throw,        1

// Stack size for ExInfo (aligned to 16 bytes)
.equ STACKSIZEOF_ExInfo,  ((SIZEOF__ExInfo + 15) & ~15)

// Loads s0 and s1-s11 from the REGDISPLAY at \rd: the registers a funclet
// expects to find, the values its parent method had at the call that threw.
.macro LOAD_PRESERVED_FROM_REGDISPLAY rd
    ld      s0,  OFFSETOF__REGDISPLAY__FP(\rd)
    ld      s1,  OFFSETOF__REGDISPLAY__S1(\rd)
    ld      s2,  OFFSETOF__REGDISPLAY__S2(\rd)
    ld      s3,  OFFSETOF__REGDISPLAY__S3(\rd)
    ld      s4,  OFFSETOF__REGDISPLAY__S4(\rd)
    ld      s5,  OFFSETOF__REGDISPLAY__S5(\rd)
    ld      s6,  OFFSETOF__REGDISPLAY__S6(\rd)
    ld      s7,  OFFSETOF__REGDISPLAY__S7(\rd)
    ld      s8,  OFFSETOF__REGDISPLAY__S8(\rd)
    ld      s9,  OFFSETOF__REGDISPLAY__S9(\rd)
    ld      s10, OFFSETOF__REGDISPLAY__S10(\rd)
    ld      s11, OFFSETOF__REGDISPLAY__S11(\rd)
.endm

// Saves s0, ra and s1-s11 in a 0x68-byte block at sp, with their CFI.
.macro SAVE_CALLEE_SAVED_WITH_CFI
    sd      s0,  0x00(sp)
    .cfi_rel_offset s0, 0x00
    sd      ra,  0x08(sp)
    .cfi_rel_offset ra, 0x08
    sd      s1,  0x10(sp)
    .cfi_rel_offset s1, 0x10
    sd      s2,  0x18(sp)
    .cfi_rel_offset s2, 0x18
    sd      s3,  0x20(sp)
    .cfi_rel_offset s3, 0x20
    sd      s4,  0x28(sp)
    .cfi_rel_offset s4, 0x28
    sd      s5,  0x30(sp)
    .cfi_rel_offset s5, 0x30
    sd      s6,  0x38(sp)
    .cfi_rel_offset s6, 0x38
    sd      s7,  0x40(sp)
    .cfi_rel_offset s7, 0x40
    sd      s8,  0x48(sp)
    .cfi_rel_offset s8, 0x48
    sd      s9,  0x50(sp)
    .cfi_rel_offset s9, 0x50
    sd      s10, 0x58(sp)
    .cfi_rel_offset s10, 0x58
    sd      s11, 0x60(sp)
    .cfi_rel_offset s11, 0x60
.endm

.macro RESTORE_CALLEE_SAVED
    ld      s0,  0x00(sp)
    ld      ra,  0x08(sp)
    ld      s1,  0x10(sp)
    ld      s2,  0x18(sp)
    ld      s3,  0x20(sp)
    ld      s4,  0x28(sp)
    ld      s5,  0x30(sp)
    ld      s6,  0x38(sp)
    ld      s7,  0x40(sp)
    ld      s8,  0x48(sp)
    ld      s9,  0x50(sp)
    ld      s10, 0x58(sp)
    ld      s11, 0x60(sp)
.endm

//=============================================================================
// RhpThrowEx - Entry point for throwing a managed exception
//
// INPUT:  a0 = exception object
//
// Called by ILC-generated code when a 'throw' statement executes.
//
// CFI: the prologue (the PAL_LIMITED_CONTEXT built on the stack, then the
// ExInfo space) is described so the GC stack scan and the exception unwinder
// can step from inside the dispatcher up through this stub into the throwing
// method. RhThrowEx never returns, so the post-call IP is the only CFI target.
//=============================================================================
.global RhpThrowEx
.balign 4
.cfi_startproc
RhpThrowEx:
    // The throw site's SP and its return address
    mv      t0, sp
    mv      t1, ra

    addi    sp, sp, -STACKSIZEOF_PAL_LIMITED_CONTEXT
    .cfi_adjust_cfa_offset STACKSIZEOF_PAL_LIMITED_CONTEXT

    sd      t0,  OFFSETOF__PAL_LIMITED_CONTEXT__SP(sp)
    sd      t1,  OFFSETOF__PAL_LIMITED_CONTEXT__IP(sp)
    sd      s0,  OFFSETOF__PAL_LIMITED_CONTEXT__FP(sp)
    .cfi_rel_offset s0, OFFSETOF__PAL_LIMITED_CONTEXT__FP
    sd      ra,  OFFSETOF__PAL_LIMITED_CONTEXT__RA(sp)
    .cfi_rel_offset ra, OFFSETOF__PAL_LIMITED_CONTEXT__RA
    sd      s1,  OFFSETOF__PAL_LIMITED_CONTEXT__S1(sp)
    .cfi_rel_offset s1, OFFSETOF__PAL_LIMITED_CONTEXT__S1
    sd      s2,  OFFSETOF__PAL_LIMITED_CONTEXT__S2(sp)
    .cfi_rel_offset s2, OFFSETOF__PAL_LIMITED_CONTEXT__S2
    sd      s3,  OFFSETOF__PAL_LIMITED_CONTEXT__S3(sp)
    .cfi_rel_offset s3, OFFSETOF__PAL_LIMITED_CONTEXT__S3
    sd      s4,  OFFSETOF__PAL_LIMITED_CONTEXT__S4(sp)
    .cfi_rel_offset s4, OFFSETOF__PAL_LIMITED_CONTEXT__S4
    sd      s5,  OFFSETOF__PAL_LIMITED_CONTEXT__S5(sp)
    .cfi_rel_offset s5, OFFSETOF__PAL_LIMITED_CONTEXT__S5
    sd      s6,  OFFSETOF__PAL_LIMITED_CONTEXT__S6(sp)
    .cfi_rel_offset s6, OFFSETOF__PAL_LIMITED_CONTEXT__S6
    sd      s7,  OFFSETOF__PAL_LIMITED_CONTEXT__S7(sp)
    .cfi_rel_offset s7, OFFSETOF__PAL_LIMITED_CONTEXT__S7
    sd      s8,  OFFSETOF__PAL_LIMITED_CONTEXT__S8(sp)
    .cfi_rel_offset s8, OFFSETOF__PAL_LIMITED_CONTEXT__S8
    sd      s9,  OFFSETOF__PAL_LIMITED_CONTEXT__S9(sp)
    .cfi_rel_offset s9, OFFSETOF__PAL_LIMITED_CONTEXT__S9
    sd      s10, OFFSETOF__PAL_LIMITED_CONTEXT__S10(sp)
    .cfi_rel_offset s10, OFFSETOF__PAL_LIMITED_CONTEXT__S10
    sd      s11, OFFSETOF__PAL_LIMITED_CONTEXT__S11(sp)
    .cfi_rel_offset s11, OFFSETOF__PAL_LIMITED_CONTEXT__S11

    // Keep the exception object in a callee-saved register
    mv      s1, a0

    // Space for the ExInfo
    addi    sp, sp, -STACKSIZEOF_ExInfo
    .cfi_adjust_cfa_offset STACKSIZEOF_ExInfo

    // a4 = &head of this thread's ExInfo chain. A managed callout: it may
    // clobber the argument and temporary registers and ra, so it runs before
    // the ExInfo is set up. The throw site's ra is already in the context.
    call    __cosmos_exinfo_head
    mv      a4, a0

    // a1 = ExInfo* (current sp)
    mv      a1, sp

    sd      s1, OFFSETOF__ExInfo__m_exception(a1)      // RhpRethrow reads it back
    li      a2, 1
    sb      a2, OFFSETOF__ExInfo__m_passNumber(a1)     // passNumber = 1
    li      a2, -1
    sw      a2, OFFSETOF__ExInfo__m_idxCurClause(a1)   // idxCurClause = -1
    li      a2, ExKind_Throw
    sb      a2, OFFSETOF__ExInfo__m_kind(a1)           // kind = Throw

    // Link the ExInfo into this thread's chain
    ld      a3, 0(a4)
    sd      a3, OFFSETOF__ExInfo__m_pPrevExInfo(a1)
    sd      a1, 0(a4)

    // The exception context pointer
    addi    a3, sp, STACKSIZEOF_ExInfo                 // a3 = PAL_LIMITED_CONTEXT*
    sd      a3, OFFSETOF__ExInfo__m_pExContext(a1)

    // RhThrowEx(exception = a0, ExInfo* = a1)
    mv      a0, s1
    call    RhThrowEx

    // If we return, something went wrong (should never happen)
    ebreak
1:  j       1b
.cfi_endproc

//=============================================================================
// RhpCallCatchFunclet - Call a catch handler funclet
//
// INPUT:  a0 = exception object
//         a1 = handler funclet address
//         a2 = REGDISPLAY*
//         a3 = ExInfo*
//
// The funclet runs with the catching method's s0-s11 (loaded from the
// REGDISPLAY) and returns the address to resume at. It preserves those
// registers, so on return they hold the catching method's values: the stub
// pops the ExInfo records below the resume SP, sets sp to the REGDISPLAY's
// SP (the catching method's body SP, as upstream's riscv64 stub does) and
// jumps to the resume address.
//
// CFI: describes the prologue, so the unwinder can step from inside the
// funclet up through this trampoline into the dispatcher's managed frame.
//=============================================================================
.global RhpCallCatchFunclet
.balign 4
.cfi_startproc
RhpCallCatchFunclet:
    addi    sp, sp, -0x80
    .cfi_def_cfa_offset 0x80
    SAVE_CALLEE_SAVED_WITH_CFI

    sd      a0, 0x68(sp)                // exception object
    sd      a1, 0x70(sp)                // handler address
    sd      a2, 0x78(sp)                // REGDISPLAY*

    LOAD_PRESERVED_FROM_REGDISPLAY a2

    ld      a0, 0x68(sp)                // exception object
    ld      t0, 0x70(sp)                // handler address
    jalr    t0                          // call the catch funclet

    // a0 = the resume address. Keep it across the callout for the head of this
    // thread's ExInfo chain, a managed call that may clobber a0-a7, t0-t6 and
    // ra but preserves s0-s11, which now hold the catching method's values.
    sd      a0, 0x70(sp)
    call    __cosmos_exinfo_head
    mv      t2, a0                      // t2 = &head of this thread's ExInfo chain
    ld      t3, 0x70(sp)                // t3 = resume address
    ld      a2, 0x78(sp)                // a2 = REGDISPLAY*
    ld      t4, OFFSETOF__REGDISPLAY__SP(a2)   // t4 = resume SP

    // Pop the ExInfo entries below the resume SP
1:
    ld      t5, 0(t2)                   // current ExInfo
    beqz    t5, 2f                      // null = done
    bgeu    t5, t4, 2f                  // at or above the resume SP = done
    ld      t5, OFFSETOF__ExInfo__m_pPrevExInfo(t5)
    sd      t5, 0(t2)                   // pop it
    j       1b
2:
    // Resume after the catch block on the catching method's stack
    mv      sp, t4
    jr      t3
.cfi_endproc


//=============================================================================
// RhpCallFilterFunclet - Call a filter funclet to evaluate exception filter
//
// INPUT:  a0 = exception object
//         a1 = filter funclet address
//         a2 = REGDISPLAY*
//
// OUTPUT: a0 = non-zero if the filter matched (should catch), 0 if not
//=============================================================================
.global RhpCallFilterFunclet
.balign 4
.cfi_startproc
RhpCallFilterFunclet:
    addi    sp, sp, -0x80
    .cfi_def_cfa_offset 0x80
    SAVE_CALLEE_SAVED_WITH_CFI

    sd      a0, 0x68(sp)                // exception object
    sd      a1, 0x70(sp)                // filter address

    LOAD_PRESERVED_FROM_REGDISPLAY a2

    ld      a0, 0x68(sp)
    ld      t0, 0x70(sp)
    jalr    t0                          // call the filter funclet

    // a0 = the filter result
    RESTORE_CALLEE_SAVED
    addi    sp, sp, 0x80
    ret
.cfi_endproc


//=============================================================================
// RhpCallFinallyFunclet - Call a finally (or fault) funclet while an exception unwinds its frame
//
// INPUT:  a0 = handler funclet address
//         a1 = REGDISPLAY*
//
// The funclet runs with the frame's s0-s11 (loaded below) and returns here;
// this stub returns to the dispatcher, which goes on with the next clause or
// frame. The funclet keeps the callee-saved registers as it found them, so
// nothing is written back to the REGDISPLAY.
//=============================================================================
.global RhpCallFinallyFunclet
.balign 4
.cfi_startproc
RhpCallFinallyFunclet:
    addi    sp, sp, -0x70
    .cfi_def_cfa_offset 0x70
    SAVE_CALLEE_SAVED_WITH_CFI

    mv      t0, a0                      // t0 keeps the handler address
    LOAD_PRESERVED_FROM_REGDISPLAY a1

    jalr    t0                          // call the finally funclet

    RESTORE_CALLEE_SAVED
    addi    sp, sp, 0x70
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
    // The exception to rethrow is the one at the head of this thread's ExInfo
    // chain. The head comes from a managed callout, which clobbers ra, the
    // rethrow site RhpThrowEx records: keep it.
    addi    sp, sp, -16
    sd      ra, 0(sp)
    call    __cosmos_exinfo_head
    ld      ra, 0(sp)
    addi    sp, sp, 16
    ld      a0, 0(a0)
    beqz    a0, 1f

    // Exception object from the ExInfo
    ld      a0, OFFSETOF__ExInfo__m_exception(a0)
    beqz    a0, 1f

    // Rethrow through the normal throw path
    j       RhpThrowEx

1:
    ebreak
2:  j       2b
