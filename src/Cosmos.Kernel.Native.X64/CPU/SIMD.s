.intel_syntax noprefix

.global _native_enable_simd

.text

// Enable SSE/AVX support for x86-64
_native_enable_simd:
    // Read CR0 register
    mov rax, cr0
    and rax, ~(1 << 2)  // Clear EM (bit 2) - Emulation
    and rax, ~(1 << 3)  // Clear TS (bit 3) - Task switched / FPU unavailable
    or  rax, (1 << 1)   // Set MP (bit 1) - Monitor co-processor
    mov cr0, rax

    // Read CR4 register
    mov rax, cr4
    or  rax, (3 << 9)   // Set OSFXSR (bit 9) and OSXMMEXCPT (bit 10)
    mov cr4, rax

    // Reset the x87 control/status state after clearing TS. Some firmware and
    // hypervisors leave TS asserted even though SSE is otherwise enabled.
    fninit

    ret
