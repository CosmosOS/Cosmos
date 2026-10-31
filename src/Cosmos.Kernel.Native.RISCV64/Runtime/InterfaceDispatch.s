// Interface dispatch stubs for Cosmos OS (RISC-V 64)
//
// Modeled after the ARM64 stub: every argument register the interface
// method may receive (a0-a7 and fa0-fa7) is saved around the call to
// RhpCidResolve, then restored, so the resolved target gets the original
// arguments.

.global RhpInitialDynamicInterfaceDispatch

// External C# function
.extern RhpCidResolve

.text
.balign 4

// Initial dispatch on an interface when we don't have a cache yet.
//
// On entry (LP64D):
//   a0 = 'this' pointer (the object we're dispatching on)
//   a1-a7 = parameters to the interface method
//   fa0-fa7 = floating-point parameters
//   t5 = pointer to the interface dispatch cell (riscv64 ILC convention)
//
RhpInitialDynamicInterfaceDispatch:
    // Trigger an exception if we're dispatching on a null this
    lb      zero, 0(a0)

    // Frame: 8 integer args (64) + ra (8) + pad (8) + 8 FP args (64) = 144 bytes, 16-byte aligned.
    addi    sp, sp, -144
    sd      a0, 0(sp)
    sd      a1, 8(sp)
    sd      a2, 16(sp)
    sd      a3, 24(sp)
    sd      a4, 32(sp)
    sd      a5, 40(sp)
    sd      a6, 48(sp)
    sd      a7, 56(sp)
    sd      ra, 64(sp)
    fsd     fa0, 80(sp)
    fsd     fa1, 88(sp)
    fsd     fa2, 96(sp)
    fsd     fa3, 104(sp)
    fsd     fa4, 112(sp)
    fsd     fa5, 120(sp)
    fsd     fa6, 128(sp)
    fsd     fa7, 136(sp)

    // RhpCidResolve(this = a0, cell = a1) returns the target in a0
    mv      a1, t5
    call    RhpCidResolve
    mv      t6, a0

    fld     fa0, 80(sp)
    fld     fa1, 88(sp)
    fld     fa2, 96(sp)
    fld     fa3, 104(sp)
    fld     fa4, 112(sp)
    fld     fa5, 120(sp)
    fld     fa6, 128(sp)
    fld     fa7, 136(sp)
    ld      ra, 64(sp)
    ld      a7, 56(sp)
    ld      a6, 48(sp)
    ld      a5, 40(sp)
    ld      a4, 32(sp)
    ld      a3, 24(sp)
    ld      a2, 16(sp)
    ld      a1, 8(sp)
    ld      a0, 0(sp)
    addi    sp, sp, 144

    // Tail-call the resolved method
    jr      t6

// void* get_interface_dispatch_cache(void)
// No cache of resolved calls here yet (the x64 stub has one): null, so
// CachedInterfaceDispatch.Remember keeps nothing.
.global get_interface_dispatch_cache
get_interface_dispatch_cache:
    li      a0, 0
    ret

// void* get_initial_dynamic_interface_dispatch(void)
// The stub RhNewInterfaceDispatchCell puts in the cells it creates at runtime
// (the type loader's generic dictionaries).
.global get_initial_dynamic_interface_dispatch
get_initial_dynamic_interface_dispatch:
    la      a0, RhpInitialDynamicInterfaceDispatch
    ret
