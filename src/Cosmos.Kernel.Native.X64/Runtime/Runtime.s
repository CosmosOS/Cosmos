// x64 NativeAOT Runtime Stubs
// Interlocked reference exchanges, EH section accessors and core math functions (x87 FPU)
//
// Core transcendentals (sin, cos, tan, exp, log, atan) use x87 hardware.
// Derived functions (asin, acos, atan2, pow, log2, log10) are shared C#
// implementations in Cosmos.Kernel.Core/Runtime/Math.cs.

.intel_syntax noprefix

.global RhpCheckedLockCmpXchg
.global RhpCheckedXchg

.global get_eh_frame_start
.global get_eh_frame_end
.global get_dotnet_eh_table_start
.global get_dotnet_eh_table_end

.extern __eh_frame_start
.extern __eh_frame_end
.extern __dotnet_eh_table_start
.extern __dotnet_eh_table_end

// core double math (x87 FPU)
.global cos
.global sin
.global tan
.global atan
.global exp
.global log

// core float math wrappers
.global cosf
.global sinf
.global tanf
.global expf
.global logf

.text

// =============================================================================
// Interlocked exchanges on object references
//
// CoreLib reaches these through RuntimeImports.InterlockedCompareExchange and
// InterlockedExchange: Interlocked.CompareExchange and Exchange on a reference,
// so Lock, LazyInitializer, the thread pool's queues and ConcurrentQueue. ILC
// inlines the integer forms; the reference forms are calls, and were plain
// read-then-write C#, which a tick between the two halves broke. The whole
// operation is one locked instruction here. No write barrier follows the
// store: the collector is neither generational nor concurrent, and
// RhpCheckedAssignRef is a plain store as well.
// =============================================================================

// object RhpCheckedLockCmpXchg(object* location, object value, object comparand)
// System V: rdi = location, rsi = value, rdx = comparand. Returns the value
// the location held before, in rax, whether or not it was replaced.
RhpCheckedLockCmpXchg:
    mov     rax, rdx
    lock cmpxchg [rdi], rsi
    ret

// object RhpCheckedXchg(object* location, object value)
// System V: rdi = location, rsi = value. Returns the previous value in rax.
// xchg with a memory operand is locked on its own.
RhpCheckedXchg:
    mov     rax, rsi
    xchg    [rdi], rax
    ret

// =============================================================================
// EH section accessors
// =============================================================================

get_eh_frame_start:
    lea rax, [rip + __eh_frame_start]
    ret

get_eh_frame_end:
    lea rax, [rip + __eh_frame_end]
    ret

get_dotnet_eh_table_start:
    lea rax, [rip + __dotnet_eh_table_start]
    ret

get_dotnet_eh_table_end:
    lea rax, [rip + __dotnet_eh_table_end]
    ret

// =============================================================================
// Helper: check if xmm0 is NaN or Inf (exponent == 0x7FF)
// Sets ZF if NaN/Inf. Trashes rax.
// =============================================================================
__check_nan_inf:
    movq rax, xmm0
    shr rax, 52
    and eax, 0x7FF
    cmp eax, 0x7FF
    ret

// =============================================================================
// Double-precision math (x87 FPU)
// =============================================================================

// double sin(double x)
sin:
    call __check_nan_inf
    je .Lsin_nan
    sub rsp, 8
    movsd [rsp], xmm0
    fld qword ptr [rsp]
    fsin
    fstp qword ptr [rsp]
    movsd xmm0, [rsp]
    add rsp, 8
    ret
.Lsin_nan:
    movsd xmm0, [rip + __math_nan]
    ret

// double cos(double x)
cos:
    call __check_nan_inf
    je .Lcos_nan
    sub rsp, 8
    movsd [rsp], xmm0
    fld qword ptr [rsp]
    fcos
    fstp qword ptr [rsp]
    movsd xmm0, [rsp]
    add rsp, 8
    ret
.Lcos_nan:
    movsd xmm0, [rip + __math_nan]
    ret

// double tan(double x)
tan:
    call __check_nan_inf
    je .Ltan_nan
    sub rsp, 8
    movsd [rsp], xmm0
    fld qword ptr [rsp]
    fptan
    fstp st(0)          // pop the 1.0 fptan pushes
    fstp qword ptr [rsp]
    movsd xmm0, [rsp]
    add rsp, 8
    ret
.Ltan_nan:
    movsd xmm0, [rip + __math_nan]
    ret

// double atan(double x)
// atan(x) = fpatan(x, 1.0) = atan2(x, 1)
atan:
    // x == 0 -> return 0 exactly
    xorpd xmm1, xmm1
    ucomisd xmm0, xmm1
    jne .Latan_compute
    jp .Latan_compute
    ret
.Latan_compute:
    sub rsp, 8
    movsd [rsp], xmm0
    fld qword ptr [rsp]             // ST0 = x
    fld1                            // ST0 = 1.0, ST1 = x
    fpatan                          // atan2(ST1, ST0) = atan2(x, 1.0) = atan(x)
    fstp qword ptr [rsp]
    movsd xmm0, [rsp]
    add rsp, 8
    ret

// double exp(double x)
// exp(x) = 2^(x * log2(e))
exp:
    // Check NaN/Inf
    movq rax, xmm0
    mov rcx, rax
    shr rcx, 52
    and ecx, 0x7FF
    cmp ecx, 0x7FF
    je .Lexp_special

    sub rsp, 8
    movsd [rsp], xmm0
    fld qword ptr [rsp]             // ST0 = x
    fldl2e                          // ST0 = log2(e), ST1 = x
    fmulp                           // ST0 = x * log2(e)

    // 2^ST0: split into int + frac
    fld st(0)                       // dup
    frndint                         // ST0 = int part
    fsub st(1), st(0)               // ST1 = frac part
    fxch st(1)                      // ST0 = frac, ST1 = int
    f2xm1                           // ST0 = 2^frac - 1
    fld1
    faddp                           // ST0 = 2^frac
    fscale                          // ST0 = 2^frac * 2^int = exp(x)
    fstp st(1)                      // pop int
    fstp qword ptr [rsp]
    movsd xmm0, [rsp]
    add rsp, 8
    ret

.Lexp_special:
    // rax has the original bits
    bt rax, 63                      // check sign bit
    jc .Lexp_neg_inf
    // +Inf or NaN: check mantissa
    mov rcx, rax
    shl rcx, 12                     // shift out sign+exponent
    test rcx, rcx
    jnz .Lexp_nan                   // non-zero mantissa = NaN
    // +Inf -> +Inf
    movsd xmm0, [rip + __math_pos_inf]
    ret
.Lexp_neg_inf:
    // -Inf -> 0
    xorpd xmm0, xmm0
    ret
.Lexp_nan:
    movsd xmm0, [rip + __math_nan]
    ret

// double log(double x) -- natural log
// log(x) = ln(2) * log2(x)
log:
    sub rsp, 8
    movsd [rsp], xmm0
    fld1
    fld qword ptr [rsp]
    fyl2x                           // log2(x)
    fldln2                          // ln(2)
    fmulp                           // ln(2) * log2(x) = ln(x)
    fstp qword ptr [rsp]
    movsd xmm0, [rsp]
    add rsp, 8
    ret

// =============================================================================
// Single-precision float wrappers (promote -> call double -> demote)
// =============================================================================

sinf:
    cvtss2sd xmm0, xmm0
    call sin
    cvtsd2ss xmm0, xmm0
    ret

cosf:
    cvtss2sd xmm0, xmm0
    call cos
    cvtsd2ss xmm0, xmm0
    ret

tanf:
    cvtss2sd xmm0, xmm0
    call tan
    cvtsd2ss xmm0, xmm0
    ret

expf:
    cvtss2sd xmm0, xmm0
    call exp
    cvtsd2ss xmm0, xmm0
    ret

logf:
    cvtss2sd xmm0, xmm0
    call log
    cvtsd2ss xmm0, xmm0
    ret

// =============================================================================
// Derived math functions (asin, acos, atan2, pow, log2, log10) are now
// shared C# implementations in Cosmos.Kernel.Core/Runtime/Math.cs.
// =============================================================================

// =============================================================================
// Constants
// =============================================================================
.section .rodata
.balign 16
__math_nan:       .quad 0x7FF8000000000000
__math_pos_inf:   .quad 0x7FF0000000000000
