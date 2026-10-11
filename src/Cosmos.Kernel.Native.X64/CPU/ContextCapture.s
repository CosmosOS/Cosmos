// Register-context capture for the precise GC stack scan (issue #346).
//
// _native_capture_regdisplay(REGDISPLAY* rd) fills *rd with the calling frame's callee-saved
// register values + stack pointer (at the call site), points the pRxx save-location pointers at the
// value slots inside *rd, and returns the caller's return address (IP) in RAX. The managed wrapper
// then walks frames up from there, decoding each method's GCInfo at the matching IP.
//
// System V AMD64 ABI: RDI = first argument (REGDISPLAY*). RAX / RCX are caller-saved scratch.

.intel_syntax noprefix

.global _native_capture_regdisplay

.text

// REGDISPLAY field offsets: they must match the [FieldOffset(...)] values in RegisterContext.X64.cs (Size = 0x88).
.equ REGDISPLAY__Rbx,   0x00
.equ REGDISPLAY__Rbp,   0x08
.equ REGDISPLAY__Rsi,   0x10
.equ REGDISPLAY__pRbx,  0x18
.equ REGDISPLAY__pRbp,  0x20
.equ REGDISPLAY__pRsi,  0x28
.equ REGDISPLAY__pRdi,  0x30
.equ REGDISPLAY__Rdi,   0x38
.equ REGDISPLAY__R12,   0x40
.equ REGDISPLAY__R13,   0x48
.equ REGDISPLAY__R14,   0x50
.equ REGDISPLAY__pR12,  0x58
.equ REGDISPLAY__pR13,  0x60
.equ REGDISPLAY__pR14,  0x68
.equ REGDISPLAY__pR15,  0x70
.equ REGDISPLAY__SP,    0x78
.equ REGDISPLAY__R15,   0x80

_native_capture_regdisplay:
    // rax = working pointer to the REGDISPLAY; rdi (the arg) is preserved until we overwrite Rdi below.
    mov     rax, rdi

    // Callee-saved register values, exactly as the caller had them at the call site.
    mov     [rax + REGDISPLAY__Rbx], rbx
    mov     [rax + REGDISPLAY__Rbp], rbp
    mov     [rax + REGDISPLAY__Rsi], rsi
    // The caller's real RDI was already overwritten to pass the argument; RDI is caller-saved on
    // SysV, so GCInfo never reports it live at a call site — store 0 rather than the argument value.
    mov     qword ptr [rax + REGDISPLAY__Rdi], 0
    mov     [rax + REGDISPLAY__R12], r12
    mov     [rax + REGDISPLAY__R13], r13
    mov     [rax + REGDISPLAY__R14], r14
    mov     [rax + REGDISPLAY__R15], r15

    // Stack pointer at the call site (RSP here points at the pushed return address).
    lea     rcx, [rsp + 8]
    mov     [rax + REGDISPLAY__SP], rcx

    // pRxx -> the value slots inside *rd (valid as long as *rd is alive in the caller).
    lea     rcx, [rax + REGDISPLAY__Rbx]
    mov     [rax + REGDISPLAY__pRbx], rcx
    lea     rcx, [rax + REGDISPLAY__Rbp]
    mov     [rax + REGDISPLAY__pRbp], rcx
    lea     rcx, [rax + REGDISPLAY__Rsi]
    mov     [rax + REGDISPLAY__pRsi], rcx
    lea     rcx, [rax + REGDISPLAY__Rdi]
    mov     [rax + REGDISPLAY__pRdi], rcx
    lea     rcx, [rax + REGDISPLAY__R12]
    mov     [rax + REGDISPLAY__pR12], rcx
    lea     rcx, [rax + REGDISPLAY__R13]
    mov     [rax + REGDISPLAY__pR13], rcx
    lea     rcx, [rax + REGDISPLAY__R14]
    mov     [rax + REGDISPLAY__pR14], rcx
    lea     rcx, [rax + REGDISPLAY__R15]
    mov     [rax + REGDISPLAY__pR15], rcx

    // Return the caller's return address (its current IP) so the walk knows where frame 0 is.
    mov     rax, [rsp]
    ret
