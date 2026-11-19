.intel_syntax noprefix

.global _native_cpu_halt
.global _native_cpu_rdtsc
.global _native_cpu_disable_interrupts
.global _native_cpu_enable_interrupts
.global _native_cpu_save_irq_and_disable
.global _native_cpu_restore_irq
.global _native_cpu_read_irq_flags
.global _native_cpu_read_cr3
.global _native_cpu_invlpg
.global _native_cpu_rdmsr
.global _native_cpu_set_block
.global _native_cpu_read_index
.global _native_cpu_block_address
.global RhCpuIdEx

// The per-CPU block (SMP phase 1; Core/CPU/PerCpuBlock.cs is the layout's home): IA32_GS_BASE
// points at the running CPU's, so a `gs:[slot]` operand is one load with no table walk. The
// kernel has no user mode, so swapgs never enters the picture and the register is written once
// per CPU, from managed code, when the CPU installs its block. ILC's code generation does not
// use GS in this kernel (thread statics go through RhGetThreadStaticStorage; inline thread
// statics, when they come, use FS).
.equ PERCPU_SELF,      0x00
.equ PERCPU_CPU_INDEX, 0x08
.equ IA32_GS_BASE,     0xC0000101

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

// Read RFLAGS without changing it: the interrupt state, with no window closed.
// Returns: RFLAGS in RAX.
_native_cpu_read_irq_flags:
    pushfq
    pop     rax
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
    or      rax, rdx                // Combine into RAX (return value)
    ret

// void _native_cpu_set_block(void* block)
// Makes `block` the calling CPU's per-CPU block: loads IA32_GS_BASE with its address.
// rdi = block
_native_cpu_set_block:
    mov     eax, edi                // wrmsr takes EDX:EAX
    mov     rdx, rdi
    shr     rdx, 32
    mov     ecx, IA32_GS_BASE
    wrmsr
    ret

// uint64_t _native_cpu_read_index(void)
// The index of the calling CPU: one load through the per-CPU register. Cheap enough for the
// paths that ask on every thread-static access and every allocation.
_native_cpu_read_index:
    mov     rax, qword ptr gs:[PERCPU_CPU_INDEX]
    ret

// void* _native_cpu_block_address(void)
// The calling CPU's block: its Self slot, since the GS base is not readable without FSGSBASE.
_native_cpu_block_address:
    mov     rax, qword ptr gs:[PERCPU_SELF]
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
