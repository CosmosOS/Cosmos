// Exception Handling Assembly Stubs for x86-64 (System V ABI / Linux)
// Implements the low-level exception dispatching for NativeAOT

.intel_syntax noprefix

.text

// External managed functions
.extern RhThrowEx                   // C# exception dispatcher
.extern __cosmos_exinfo_head        // &head of the running thread's ExInfo chain (ExceptionNative)

//=============================================================================
// Structure offsets (from AsmOffsetsCpu.h)
//=============================================================================

// ExInfo offsets
.equ SIZEOF__ExInfo,                  0x190
.equ OFFSETOF__ExInfo__m_pPrevExInfo, 0x00
.equ OFFSETOF__ExInfo__m_pExContext,  0x08
.equ OFFSETOF__ExInfo__m_exception,   0x10
.equ OFFSETOF__ExInfo__m_kind,        0x18
.equ OFFSETOF__ExInfo__m_passNumber,  0x19
.equ OFFSETOF__ExInfo__m_idxCurClause, 0x1c

// REGDISPLAY offsets
.equ OFFSETOF__REGDISPLAY__SP,        0x78
.equ OFFSETOF__REGDISPLAY__pRbx,      0x18
.equ OFFSETOF__REGDISPLAY__pRbp,      0x20
.equ OFFSETOF__REGDISPLAY__pRsi,      0x28
.equ OFFSETOF__REGDISPLAY__pRdi,      0x30
.equ OFFSETOF__REGDISPLAY__pR12,      0x58
.equ OFFSETOF__REGDISPLAY__pR13,      0x60
.equ OFFSETOF__REGDISPLAY__pR14,      0x68
.equ OFFSETOF__REGDISPLAY__pR15,      0x70

// ExKind enum
.equ ExKind_Throw,        1

// Stack size for ExInfo (aligned to 16 bytes)
.equ STACKSIZEOF_ExInfo,  ((SIZEOF__ExInfo + 15) & ~15)

//=============================================================================
// RhpThrowEx - Entry point for throwing a managed exception
//
// INPUT:  RDI = exception object
//
// This is called by the ILC-generated code when a 'throw' statement executes.
// System V AMD64 ABI: RDI = first argument (exception object)
//
// CFI: the prologue (the PAL_LIMITED_CONTEXT this builds on the stack, then the ExInfo space) is
// described so the precise GC stack scan / exception unwinder (issue #346) can step from inside the
// dispatcher up through this stub into the throwing method. The post-`call RhThrowEx` IP is the
// only CFI target here — RhThrowEx transfers to a handler or halts, it never returns — so describing
// just the prologue is sufficient. RBX/RBP/R12-R15 are written into the context, so their throw-site
// values live at known stack slots; RAX/RDX/RSI/RDI are scratch.
//=============================================================================
.global RhpThrowEx
.cfi_startproc
RhpThrowEx:
    // Save the RSP of the throw site (before call pushed return address)
    lea     rax, [rsp + 8]          // rax = original RSP at throw site
    mov     rsi, [rsp]              // rsi = return address (throw site IP)

    // Align stack to 16 bytes
    xor     rdx, rdx
    push    rdx                     // padding for alignment
    .cfi_adjust_cfa_offset 8

    // Build PAL_LIMITED_CONTEXT structure on stack
    // Push in reverse order so they end up at correct offsets
    push    r15                     // +0x48: R15
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r15, 0
    push    r14                     // +0x40: R14
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r14, 0
    push    r13                     // +0x38: R13
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r13, 0
    push    r12                     // +0x30: R12
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r12, 0
    push    rdx                     // +0x28: Rdx (0)
    .cfi_adjust_cfa_offset 8
    push    rbx                     // +0x20: Rbx
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbx, 0
    push    rdx                     // +0x18: Rax (0)
    .cfi_adjust_cfa_offset 8
    push    rbp                     // +0x10: Rbp
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbp, 0
    push    rax                     // +0x08: Rsp (original)
    .cfi_adjust_cfa_offset 8
    push    rsi                     // +0x00: IP (return address)
    .cfi_adjust_cfa_offset 8

    // Now RSP points to PAL_LIMITED_CONTEXT
    // Allocate space for ExInfo
    sub     rsp, STACKSIZEOF_ExInfo
    .cfi_adjust_cfa_offset STACKSIZEOF_ExInfo

    // Save exception object temporarily
    mov     rbx, rdi                // rbx = exception object

    // RAX = &head of this thread's ExInfo chain. A managed callout, so it may clobber every
    // caller-saved register: it runs before RSI is set up below. The exception object sits in RBX,
    // which it preserves, and RSP is 16-byte aligned here (the pushes above plus
    // STACKSIZEOF_ExInfo are a multiple of 16).
    call    __cosmos_exinfo_head

    // RSI = ExInfo* (current RSP)
    mov     rsi, rsp

    // Initialize ExInfo fields
    mov     [rsi + OFFSETOF__ExInfo__m_exception], rbx                       // exception object — RhpRethrow reads it back
    mov     byte ptr [rsi + OFFSETOF__ExInfo__m_passNumber], 1               // passNumber = 1
    mov     dword ptr [rsi + OFFSETOF__ExInfo__m_idxCurClause], 0xFFFFFFFF   // idxCurClause = -1
    mov     byte ptr [rsi + OFFSETOF__ExInfo__m_kind], ExKind_Throw          // kind = Throw

    // Link ExInfo into this thread's exception chain
    mov     rdx, [rax]                                                       // rdx = current head
    mov     [rsi + OFFSETOF__ExInfo__m_pPrevExInfo], rdx                     // pExInfo->m_pPrevExInfo = head
    mov     [rax], rsi                                                       // head = pExInfo

    // Set the exception context pointer
    lea     rdx, [rsp + STACKSIZEOF_ExInfo]                                  // rdx = PAL_LIMITED_CONTEXT*
    mov     [rsi + OFFSETOF__ExInfo__m_pExContext], rdx

    // Call managed exception handler
    // RDI = exception object (restore from rbx)
    // RSI = ExInfo* (already set)
    mov     rdi, rbx
    call    RhThrowEx

    // If we return, something went wrong (should never happen)
    // RhThrowEx should either transfer to a handler or halt
.Lthrow_unreachable:
    int     3
    jmp     .Lthrow_unreachable
.cfi_endproc

//=============================================================================
// RhpCallCatchFunclet - Call a catch handler funclet
//
// INPUT:  RDI = exception object
//         RSI = handler funclet address
//         RDX = REGDISPLAY*  (REGDISPLAY.SP = the catching method's body RSP at the protected
//                              region; pRbx/pRbp/pR12..pR15 -> the establisher's body register
//                              values; the dispatcher fills these from the CFI unwind)
//         RCX = ExInfo*       (unused — the ExInfo pop loop below walks this thread's chain from
//                              the head __cosmos_exinfo_head returns, so RCX is not saved)
//
// The catch funclet runs with the establisher frame's RBP/RBX/R12-R15 (restored below) and the
// exception object in RDI; it returns, in RAX, the IP in the catching method right after the
// protected region. We then set RSP to the catching method's body RSP (REGDISPLAY.SP) and jump
// there — the method's own frame is intact, so its ordinary epilogue restores our caller's
// callee-saved registers. (This used to instead force-return from the catching method, which only
// worked for one hard-coded call shape; #346.)
//
// Stack alignment: the prologue pushes 6 callee-saved regs + 3 arg slots = 9 eight-byte slots
// (odd), so RSP is 16-byte aligned at the `call qword ptr [rsp + 8]` below — the System V x86-64
// ABI requires it, and the funclet (plus everything it calls — allocators, ctors, ...) faults on
// 16-byte-aligned SSE accesses if RSP is off by 8. (A 10th push here is what broke
// GC_FuncletNoFalseRoot; #346.)
//
// CFI: the prologue is described so the precise GC stack scan / exception unwinder (issue #346)
// can step from inside the funclet up through this trampoline into the dispatcher's managed frame.
// Only the post-`call qword ptr [rsp + 8]` IP is ever a CFI target here (GC cannot fire elsewhere
// in this stub — interrupts off, no allocations; the __cosmos_exinfo_head callout allocates
// nothing), and the `mov rsp, r9 / jmp r8` resume tail runs strictly after that point, so
// describing just the prologue is sufficient and correct.
//=============================================================================
.global RhpCallCatchFunclet
.cfi_startproc
RhpCallCatchFunclet:
    // Save callee-saved registers
    push    rbp
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbp, 0
    push    rbx
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbx, 0
    push    r12
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r12, 0
    push    r13
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r13, 0
    push    r14
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r14, 0
    push    r15
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r15, 0

    // Save arguments (caller-saved — only the CFA offset advances). Three slots: see the alignment
    // note above. ExInfo* (RCX) is intentionally not saved.
    push    rdi                     // [rsp+16] exception object
    .cfi_adjust_cfa_offset 8
    push    rsi                     // [rsp+8]  handler address
    .cfi_adjust_cfa_offset 8
    push    rdx                     // [rsp+0]  REGDISPLAY*
    .cfi_adjust_cfa_offset 8

    // Restore callee-saved registers from REGDISPLAY
    // These are the registers the funclet expects to have the values from the throwing method
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pRbx]
    mov     rbx, [rax]
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pRbp]
    mov     rbp, [rax]
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pRsi]
    test    rax, rax
    jz      .Lskip_rsi
    mov     rsi, [rax]
.Lskip_rsi:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pRdi]
    test    rax, rax
    jz      .Lskip_rdi
    mov     rdi, [rax]
.Lskip_rdi:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR12]
    mov     r12, [rax]
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR13]
    mov     r13, [rax]
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR14]
    mov     r14, [rax]
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR15]
    mov     r15, [rax]

    // Load exception object and call handler funclet.
    mov     rdi, [rsp + 16]         // exception object
    call    qword ptr [rsp + 8]     // call handler funclet

    // RAX = the IP in the catching method right after the protected region. Keep it in the handler
    // slot, dead now, across the callout for the head of this thread's ExInfo chain: a managed call
    // that may clobber every caller-saved register. RSP is still 16-byte aligned here, as it was
    // for the funclet call. Then reload REGDISPLAY* (the funclet may have clobbered RDX).
    // Stack layout from the prologue pushes: [rsp+0]=REGDISPLAY* [rsp+8]=handler [rsp+16]=exObj
    mov     [rsp + 8], rax          // [rsp+8] = resume IP
    call    __cosmos_exinfo_head
    mov     r11, rax                // r11 = &head of this thread's ExInfo chain
    mov     r8, [rsp + 8]           // r8 = resume IP
    mov     rdx, [rsp]              // rdx = REGDISPLAY*

    // r9 = the catching method's body RSP at the protected region (REGDISPLAY.SP). The dispatcher
    // set this from the CFI unwind; resuming there leaves the method's frame intact.
    mov     r9, [rdx + OFFSETOF__REGDISPLAY__SP]

    // Pop ExInfo entries that belong to the frames we unwound past (those below the resume SP).
    // r10 is scratch.
.Lpop_exinfo_loop:
    mov     r10, [r11]              // current ExInfo
    test    r10, r10
    jz      .Lpop_exinfo_done       // null = done
    cmp     r10, r9
    jge     .Lpop_exinfo_done       // >= resume SP = done
    mov     r10, [r10 + OFFSETOF__ExInfo__m_pPrevExInfo]
    mov     [r11], r10              // pop it
    jmp     .Lpop_exinfo_loop

.Lpop_exinfo_done:
    // Resume in the catching method, after the protected region. RBP/RBX/R12-R15 are already the
    // establisher's body values (restored from REGDISPLAY before the call; the funclet preserved
    // them, except for any establisher local it deliberately updated). RSP <- body RSP.
    mov     rsp, r9
    jmp     r8
.cfi_endproc


//=============================================================================
// RhpCallFilterFunclet - Call a filter funclet to evaluate exception filter
//
// INPUT:  RDI = exception object
//         RSI = filter funclet address
//         RDX = REGDISPLAY*
//
// OUTPUT: RAX = 1 if filter matched (should catch), 0 if not
//
// The filter funclet expects the exception object in RDI.
// It returns non-zero if the exception should be caught by this handler.
//
// CFI: see the note on RhpCallCatchFunclet. The epilogue here is the ordinary
// `add rsp, 24` + 6 pops + `ret`; GC cannot fire in it, so prologue CFI is sufficient.
//=============================================================================
.global RhpCallFilterFunclet
.cfi_startproc
RhpCallFilterFunclet:
    // Save callee-saved registers
    push    rbp
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbp, 0
    push    rbx
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbx, 0
    push    r12
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r12, 0
    push    r13
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r13, 0
    push    r14
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r14, 0
    push    r15
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r15, 0

    // Save arguments (caller-saved — only the CFA offset advances)
    push    rdi                     // exception object
    .cfi_adjust_cfa_offset 8
    push    rsi                     // filter address
    .cfi_adjust_cfa_offset 8
    push    rdx                     // REGDISPLAY*
    .cfi_adjust_cfa_offset 8

    // Restore callee-saved registers from REGDISPLAY
    // These are the registers the funclet expects to have the values from the method
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pRbx]
    test    rax, rax
    jz      .Lskip_rbx_f
    mov     rbx, [rax]
.Lskip_rbx_f:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pRbp]
    test    rax, rax
    jz      .Lskip_rbp_f
    mov     rbp, [rax]
.Lskip_rbp_f:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR12]
    test    rax, rax
    jz      .Lskip_r12_f
    mov     r12, [rax]
.Lskip_r12_f:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR13]
    test    rax, rax
    jz      .Lskip_r13_f
    mov     r13, [rax]
.Lskip_r13_f:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR14]
    test    rax, rax
    jz      .Lskip_r14_f
    mov     r14, [rax]
.Lskip_r14_f:
    mov     rax, [rdx + OFFSETOF__REGDISPLAY__pR15]
    test    rax, rax
    jz      .Lskip_r15_f
    mov     r15, [rax]
.Lskip_r15_f:

    // Load exception object and call filter
    mov     rdi, [rsp + 16]         // exception object
    call    qword ptr [rsp + 8]     // call filter funclet

    // RAX now contains the filter result (0 = no match, non-zero = match)
    mov     r8, rax                 // save result

    // Clean up stack and restore our callee-saved registers
    add     rsp, 24                 // pop saved args
    pop     r15
    pop     r14
    pop     r13
    pop     r12
    pop     rbx
    pop     rbp

    // Return filter result
    mov     rax, r8
    ret
.cfi_endproc


//=============================================================================
// RhpCallFinallyFunclet - Call a finally (or fault) funclet while an exception unwinds its frame
//
// INPUT:  RDI = handler funclet address
//         RSI = REGDISPLAY*  (pRbx/pRbp/pR12..pR15 -> the frame's body register values at the call
//                              the exception unwinds; the dispatcher fills these from the CFI unwind)
//
// The funclet runs with the frame's RBP/RBX/R12-R15 (restored below) and returns here, and this
// stub returns to the dispatcher, which goes on with the next clause or frame. The funclet keeps
// the callee-saved registers as it found them, so nothing is written back to the REGDISPLAY.
//
// Stack alignment: the prologue pushes 6 callee-saved regs + the handler address = 7 eight-byte
// slots (odd), so RSP is 16-byte aligned at the `call qword ptr [rsp]` below (see
// RhpCallCatchFunclet).
//
// CFI: see the note on RhpCallCatchFunclet. The epilogue here is the ordinary `add rsp, 8` + 6 pops
// + `ret`; GC cannot fire in it, so prologue CFI is sufficient.
//=============================================================================
.global RhpCallFinallyFunclet
.cfi_startproc
RhpCallFinallyFunclet:
    // Save callee-saved registers
    push    rbp
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbp, 0
    push    rbx
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset rbx, 0
    push    r12
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r12, 0
    push    r13
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r13, 0
    push    r14
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r14, 0
    push    r15
    .cfi_adjust_cfa_offset 8
    .cfi_rel_offset r15, 0

    // Save the handler address (caller-saved — only the CFA offset advances)
    push    rdi                     // [rsp+0] handler address
    .cfi_adjust_cfa_offset 8

    // Restore the frame's callee-saved registers from REGDISPLAY
    mov     rax, [rsi + OFFSETOF__REGDISPLAY__pRbx]
    mov     rbx, [rax]
    mov     rax, [rsi + OFFSETOF__REGDISPLAY__pRbp]
    mov     rbp, [rax]
    mov     rax, [rsi + OFFSETOF__REGDISPLAY__pR12]
    mov     r12, [rax]
    mov     rax, [rsi + OFFSETOF__REGDISPLAY__pR13]
    mov     r13, [rax]
    mov     rax, [rsi + OFFSETOF__REGDISPLAY__pR14]
    mov     r14, [rax]
    mov     rax, [rsi + OFFSETOF__REGDISPLAY__pR15]
    mov     r15, [rax]

    call    qword ptr [rsp]         // call finally funclet

    // Clean up stack and restore our callee-saved registers
    add     rsp, 8                  // pop handler address
    pop     r15
    pop     r14
    pop     r13
    pop     r12
    pop     rbx
    pop     rbp
    ret
.cfi_endproc


//=============================================================================
// RhpRethrow - Rethrow the current exception
//
// Called when a 'throw;' statement (without exception object) is executed.
//=============================================================================
.global RhpRethrow
RhpRethrow:
    // The exception to rethrow is the one at the head of this thread's ExInfo chain. The head comes
    // from a managed callout, which wants a 16-byte aligned stack: on entry here, right after the
    // funclet's call, RSP is off by 8.
    sub     rsp, 8
    call    __cosmos_exinfo_head
    add     rsp, 8
    mov     rax, [rax]
    test    rax, rax
    jz      .Lhalt
    // Get exception object from ExInfo
    mov     rdi, [rax + OFFSETOF__ExInfo__m_exception]
    test    rdi, rdi
    jz      .Lhalt

    // Rethrow using normal throw path
    jmp     RhpThrowEx

.Lhalt:
    int     3
    jmp     .Lhalt
