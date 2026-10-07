// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Interface dispatch stubs for Cosmos OS (x64)
//
// Modeled after the upstream NativeAOT UniversalTransition thunk.
// We must save ALL System V ABI argument registers before calling
// RhpCidResolve (which clobbers all caller-saved registers), then
// restore them so the resolved target method receives the original
// arguments intact.

.intel_syntax noprefix

.text

.extern RhpCidResolve

// The calls resolved so far: 4096 entries of 32 bytes (cell, type, target,
// sequence), each at the index a hash of its cell and its type gives.
// CachedInterfaceDispatch.Remember writes them, the sequence odd meanwhile;
// the stub below reads them without a lock.
.bss
.balign 64
g_interfaceDispatchCache:
    .zero 4096 * 32

.text

// Dispatch on an interface: the target from the cache of resolved calls,
// else resolved by RhpCidResolve (which caches it). Every dispatch site
// calls it, its cell never changes.
//
// On entry (System V ABI):
//   rdi = 'this' pointer (the object we're dispatching on)
//   r11 = pointer to the interface dispatch cell
//
// The dispatch cell structure is:
//   Cell[0].m_pStub  = pointer to this function (RhpInitialDynamicInterfaceDispatch)
//   Cell[0].m_pCache = interface type pointer | flags
//   Cell[1].m_pStub  = 0
//   Cell[1].m_pCache = interface slot number
//
.global RhpInitialDynamicInterfaceDispatch
.balign 16
RhpInitialDynamicInterfaceDispatch:
    // The object's type. On a null this, the AV the caller expects.
    mov     rax, [rdi]

    // The cache entry of this cell and type, at
    // ((cell ^ type) * 0x9E3779B1) >> 52, times 32 (as Remember computes
    // it). rax and r10 carry no argument: they are free until the target.
    mov     r10, rax
    xor     r10, r11
    imul    r10, r10, -1640531535
    shr     r10, 52
    shl     r10, 5
    lea     rax, [rip + g_interfaceDispatchCache]
    add     r10, rax
    mov     rax, [rdi]

    // A hit: the entry holds this cell and this type, and was not written
    // while it was read (its sequence even, and the same after).
    push    qword ptr [r10 + 24]
    test    byte ptr [rsp], 1
    jnz     .Lresolve
    cmp     r11, [r10]
    jne     .Lresolve
    cmp     rax, [r10 + 8]
    jne     .Lresolve
    mov     rax, [r10 + 16]
    mov     r10, [r10 + 24]
    cmp     r10, [rsp]
    jne     .Lresolve
    add     rsp, 8
    jmp     rax

.Lresolve:
    add     rsp, 8

    // Allocate stack frame: 6 integer regs (48) + 8 XMM regs (128) = 176 bytes
    // Plus 8 bytes padding for 16-byte alignment = 184 bytes total
    // At entry RSP is 8-misaligned (return addr pushed by caller).
    // 184 + 8(retaddr) = 192 = 16*12, so aligned.
    sub     rsp, 184

    // Save all 6 integer argument registers
    mov     [rsp + 0x00], rdi
    mov     [rsp + 0x08], rsi
    mov     [rsp + 0x10], rdx
    mov     [rsp + 0x18], rcx
    mov     [rsp + 0x20], r8
    mov     [rsp + 0x28], r9

    // Save all 8 floating-point argument registers
    movdqu  [rsp + 0x30], xmm0
    movdqu  [rsp + 0x40], xmm1
    movdqu  [rsp + 0x50], xmm2
    movdqu  [rsp + 0x60], xmm3
    movdqu  [rsp + 0x70], xmm4
    movdqu  [rsp + 0x80], xmm5
    movdqu  [rsp + 0x90], xmm6
    movdqu  [rsp + 0xA0], xmm7

    // Set up parameters for RhpCidResolve (System V ABI):
    //   rdi = 'this' pointer (already in rdi)
    //   rsi = dispatch cell pointer (from r11)
    mov     rsi, r11

    // Call RhpCidResolve to resolve the interface method.
    // It returns the target method address in RAX.
    call    RhpCidResolve

    // Save resolved target before restoring registers
    mov     r10, rax

    // Restore all 8 floating-point argument registers
    movdqu  xmm0, [rsp + 0x30]
    movdqu  xmm1, [rsp + 0x40]
    movdqu  xmm2, [rsp + 0x50]
    movdqu  xmm3, [rsp + 0x60]
    movdqu  xmm4, [rsp + 0x70]
    movdqu  xmm5, [rsp + 0x80]
    movdqu  xmm6, [rsp + 0x90]
    movdqu  xmm7, [rsp + 0xA0]

    // Restore all 6 integer argument registers
    mov     rdi, [rsp + 0x00]
    mov     rsi, [rsp + 0x08]
    mov     rdx, [rsp + 0x10]
    mov     rcx, [rsp + 0x18]
    mov     r8,  [rsp + 0x20]
    mov     r9,  [rsp + 0x28]

    // Deallocate stack frame
    add     rsp, 184

    // Tail-call to the resolved method address (in r10)
    jmp     r10

// void* get_interface_dispatch_cache(void)
// The cache RhpInitialDynamicInterfaceDispatch looks calls up in, for
// CachedInterfaceDispatch.Remember to fill.
.global get_interface_dispatch_cache
get_interface_dispatch_cache:
    lea     rax, [rip + g_interfaceDispatchCache]
    ret

// void* get_initial_dynamic_interface_dispatch(void)
// The stub RhNewInterfaceDispatchCell puts in the cells it creates at runtime
// (the type loader's generic dictionaries).
.global get_initial_dynamic_interface_dispatch
get_initial_dynamic_interface_dispatch:
    lea     rax, [rip + RhpInitialDynamicInterfaceDispatch]
    ret
