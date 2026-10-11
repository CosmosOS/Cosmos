// Interface dispatch stubs for Cosmos OS (ARM64)
//
// Modeled after the upstream NativeAOT UniversalTransition thunk.
// We must save ALL AAPCS64 argument registers (integer x0-x7 and
// FP d0-d7) before calling RhpCidResolve, then restore them so the
// resolved target method receives the original arguments intact.

.global RhpInitialDynamicInterfaceDispatch

// External C# function
.extern RhpCidResolve

.text
.align 4

// Initial dispatch on an interface when we don't have a cache yet.
//
// On entry (AAPCS64):
//   x0 = 'this' pointer (the object we're dispatching on)
//   x1-x7 = parameters to the interface method
//   d0-d7 = floating-point parameters
//   x11 = pointer to the interface dispatch cell
//
RhpInitialDynamicInterfaceDispatch:
    // Trigger an exception if we're dispatching on a null this
    ldrb    wzr, [x0]

    // Allocate stack frame: 8 int regs (64) + LR (8) + pad (8) + 8 FP regs (64) = 144 bytes
    // Frame is 16-byte aligned.
    sub     sp, sp, #144

    // Save all 8 integer argument registers
    stp     x0, x1, [sp, #0]
    stp     x2, x3, [sp, #16]
    stp     x4, x5, [sp, #32]
    stp     x6, x7, [sp, #48]

    // Save link register (return address)
    str     x30, [sp, #64]

    // Save all 8 floating-point argument registers
    stp     d0, d1, [sp, #80]
    stp     d2, d3, [sp, #96]
    stp     d4, d5, [sp, #112]
    stp     d6, d7, [sp, #128]

    // Set up parameters for RhpCidResolve (AAPCS64):
    //   x0 = 'this' pointer (already in x0)
    //   x1 = dispatch cell pointer
    mov     x1, x11

    // Call RhpCidResolve to resolve the interface method
    // It returns the target method address in x0
    bl      RhpCidResolve

    // Save the resolved method address in a scratch register
    mov     x16, x0

    // Restore all 8 floating-point argument registers
    ldp     d0, d1, [sp, #80]
    ldp     d2, d3, [sp, #96]
    ldp     d4, d5, [sp, #112]
    ldp     d6, d7, [sp, #128]

    // Restore link register
    ldr     x30, [sp, #64]

    // Restore all 8 integer argument registers
    ldp     x6, x7, [sp, #48]
    ldp     x4, x5, [sp, #32]
    ldp     x2, x3, [sp, #16]
    ldp     x0, x1, [sp, #0]

    // Deallocate stack frame
    add     sp, sp, #144

    // Tail-call to the resolved method address (in x16)
    br      x16

// void* get_interface_dispatch_cache(void)
// No cache of resolved calls here yet (the x64 stub has one): null, so
// CachedInterfaceDispatch.Remember keeps nothing.
.global get_interface_dispatch_cache
get_interface_dispatch_cache:
    mov     x0, #0
    ret

// void* get_initial_dynamic_interface_dispatch(void)
// The stub RhNewInterfaceDispatchCell puts in the cells it creates at runtime
// (the type loader's generic dictionaries).
.global get_initial_dynamic_interface_dispatch
get_initial_dynamic_interface_dispatch:
    adrp    x0, RhpInitialDynamicInterfaceDispatch
    add     x0, x0, :lo12:RhpInitialDynamicInterfaceDispatch
    ret
