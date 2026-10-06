.intel_syntax noprefix

.global _native_cpu_halt
.global _native_cpu_rdtsc
.global _native_cpu_disable_interrupts
.global _native_cpu_enable_interrupts
.global _native_cpu_save_irq_and_disable
.global _native_cpu_restore_irq
.global _native_cpu_read_cr3
.global _native_cpu_invlpg
.global _native_cpu_rdmsr
.global RhCpuIdEx

.text

_native_cpu_halt:
    hlt
    ret

// Disable interrupts (CLI)
_native_cpu_disable_interrupts:
    cli
    ret

// Enable interrupts (STI)
_native_cpu_enable_interrupts:
    sti
    ret

// Save RFLAGS and disable interrupts.
// Returns: previous RFLAGS in RAX (System V ABI).
_native_cpu_save_irq_and_disable:
    pushfq
    pop     rax
    cli
    ret

// Restore RFLAGS from first argument (RDI on System V ABI).
_native_cpu_restore_irq:
    push    rdi
    popfq
    ret

// Read Time Stamp Counter
// Returns: 64-bit TSC value in RAX
_native_cpu_rdtsc:
    rdtsc                   // EDX:EAX = timestamp counter
    shl     rdx, 32         // Shift high 32 bits to upper half of RDX
    or      rax, rdx        // Combine into RAX (return value)
    ret

// Read CR3 (page-table root physical address plus control flags).
// Returns: CR3 in RAX.
_native_cpu_read_cr3:
    mov     rax, cr3
    ret

// Invalidate the TLB entry covering the virtual address in RDI.
_native_cpu_invlpg:
    invlpg  [rdi]
    ret

// Read the model-specific register whose index is in EDI.
// Returns: the 64-bit MSR value in RAX.
_native_cpu_rdmsr:
    mov     ecx, edi        // MSR index
    rdmsr                   // EDX:EAX = MSR (upper halves zeroed)
    shl     rdx, 32         // Shift high 32 bits to upper half of RDX
    or      rax, rdx        // Combine into RAX (return value)
    ret

// NativeAOT runtime helper backing System.Runtime.Intrinsics.X86.X86Base.CpuId.
// void RhCpuIdEx(int* cpuInfo /*RDI*/, int functionId /*ESI*/, int subFunctionId /*EDX*/)
// Mirrors dotnet/runtime nativeaot amd64 MiscStubs; RBX is callee-saved and
// clobbered by CPUID, so preserve it.
RhCpuIdEx:
    push    rbx
    mov     r8, rdi         // cpuInfo out pointer (CPUID clobbers all four regs)
    mov     eax, esi        // leaf
    mov     ecx, edx        // subleaf
    cpuid
    mov     dword ptr [r8], eax
    mov     dword ptr [r8+4], ebx
    mov     dword ptr [r8+8], ecx
    mov     dword ptr [r8+12], edx
    pop     rbx
    ret
