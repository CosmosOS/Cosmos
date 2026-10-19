// Inline Thread Local Storage (TLS) helpers for x64.

.intel_syntax noprefix

.global _native_set_tls

.text

// SetTls(void*)
_native_set_tls:
    // RDI = 64-bit FS base
    mov rax, rdi
    mov r8, rax
    mov ecx, 0xC0000100
    mov eax, r8d
    shr r8, 32
    mov edx, r8d
    wrmsr
    ret