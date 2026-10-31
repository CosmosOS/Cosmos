// Register-context capture for the precise GC stack scan (issue #346).
//
// _native_capture_regdisplay(REGDISPLAY* rd) fills *rd with the calling
// frame's callee-saved register values and stack pointer (at the call site)
// and returns the caller's return address in a0. The managed wrapper then
// walks frames up from there, decoding each method's GCInfo at the matching IP.
//
// Leaf routine (no call, no frame): ra, s0 and sp are the caller's.

.global _native_capture_regdisplay

.text
.balign 4

// REGDISPLAY field offsets: they must match RegisterContext.RISCV64.cs (Size = 0x70).
.equ REGDISPLAY__SP,   0x00
.equ REGDISPLAY__FP,   0x08
.equ REGDISPLAY__S1,   0x10
.equ REGDISPLAY__S2,   0x18
.equ REGDISPLAY__S3,   0x20
.equ REGDISPLAY__S4,   0x28
.equ REGDISPLAY__S5,   0x30
.equ REGDISPLAY__S6,   0x38
.equ REGDISPLAY__S7,   0x40
.equ REGDISPLAY__S8,   0x48
.equ REGDISPLAY__S9,   0x50
.equ REGDISPLAY__S10,  0x58
.equ REGDISPLAY__S11,  0x60
.equ REGDISPLAY__RA,   0x68

_native_capture_regdisplay:
    sd      sp,  REGDISPLAY__SP(a0)     // stack pointer at the call site
    sd      s0,  REGDISPLAY__FP(a0)
    sd      s1,  REGDISPLAY__S1(a0)
    sd      s2,  REGDISPLAY__S2(a0)
    sd      s3,  REGDISPLAY__S3(a0)
    sd      s4,  REGDISPLAY__S4(a0)
    sd      s5,  REGDISPLAY__S5(a0)
    sd      s6,  REGDISPLAY__S6(a0)
    sd      s7,  REGDISPLAY__S7(a0)
    sd      s8,  REGDISPLAY__S8(a0)
    sd      s9,  REGDISPLAY__S9(a0)
    sd      s10, REGDISPLAY__S10(a0)
    sd      s11, REGDISPLAY__S11(a0)
    sd      ra,  REGDISPLAY__RA(a0)     // the caller's return address
    mv      a0, ra
    ret
