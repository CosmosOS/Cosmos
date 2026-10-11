// Register-context capture for the precise GC stack scan (issue #346).
//
// _native_capture_regdisplay(REGDISPLAY* rd) fills *rd with the calling frame's callee-saved
// register values + stack pointer (at the call site) and returns the caller's return address (IP)
// in X0. ARM64's REGDISPLAY stores register values directly (no save-location pointers). The
// managed wrapper then walks frames up from there, decoding each method's GCInfo at the matching IP.
//
// AAPCS64: X0 = first argument (REGDISPLAY*) and the return value. X9/X10 are caller-saved scratch.
// This is a leaf routine (no BL, no stack frame), so X30 (LR), X29 (FP) and SP are the caller's.

.global _native_capture_regdisplay

.text
.align 4

// REGDISPLAY field offsets: they must match the [FieldOffset(...)] values in RegisterContext.ARM64.cs (Size = 0x68).
.equ REGDISPLAY__SP,   0x00
.equ REGDISPLAY__FP,   0x08
.equ REGDISPLAY__X19,  0x10
.equ REGDISPLAY__X20,  0x18
.equ REGDISPLAY__X21,  0x20
.equ REGDISPLAY__X22,  0x28
.equ REGDISPLAY__X23,  0x30
.equ REGDISPLAY__X24,  0x38
.equ REGDISPLAY__X25,  0x40
.equ REGDISPLAY__X26,  0x48
.equ REGDISPLAY__X27,  0x50
.equ REGDISPLAY__X28,  0x58
.equ REGDISPLAY__LR,   0x60

_native_capture_regdisplay:
    mov     x9, x0                  // working copy of REGDISPLAY*; X0 is freed for the return value
    mov     x10, sp
    str     x10, [x9, #REGDISPLAY__SP]      // stack pointer at the call site
    str     x29, [x9, #REGDISPLAY__FP]      // FP (x29)
    str     x19, [x9, #REGDISPLAY__X19]
    str     x20, [x9, #REGDISPLAY__X20]
    str     x21, [x9, #REGDISPLAY__X21]
    str     x22, [x9, #REGDISPLAY__X22]
    str     x23, [x9, #REGDISPLAY__X23]
    str     x24, [x9, #REGDISPLAY__X24]
    str     x25, [x9, #REGDISPLAY__X25]
    str     x26, [x9, #REGDISPLAY__X26]
    str     x27, [x9, #REGDISPLAY__X27]
    str     x28, [x9, #REGDISPLAY__X28]
    str     x30, [x9, #REGDISPLAY__LR]      // LR (x30) = caller's return address
    mov     x0, x30                 // return the caller's return address (IP)
    ret
