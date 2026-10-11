.intel_syntax noprefix

// Hardware random number instructions, read by the kernel random generator
// (Cosmos.Kernel.Core/Security), and the serialized time stamp counter read
// its jitter samples are timed with. Neither random instruction may run on a
// CPU whose CPUID does not advertise it: RDRAND (leaf 1 ECX bit 30) and
// RDSEED (leaf 7 subleaf 0 EBX bit 18) raise #UD where absent, as on QEMU's
// qemu64 model.

.global _native_cpu_rdrand64
.global _native_cpu_rdseed64
.global _native_cpu_rdtsc_ordered

.text

// int _native_cpu_rdrand64(ulong* value /*RDI*/)
// Reads the DRBG output. Returns 1 with the word stored at [RDI] when CF is
// set, or 0 after 10 failed attempts, the retry budget Intel's DRNG guide
// recommends: RDRAND only fails when the DRBG is starved, which is rare.
_native_cpu_rdrand64:
    mov     ecx, 10
1:
    rdrand  rax
    jc      2f
    dec     ecx
    jnz     1b
    xor     eax, eax
    ret
2:
    mov     [rdi], rax
    mov     eax, 1
    ret

// int _native_cpu_rdseed64(ulong* value /*RDI*/)
// Reads the entropy conditioner directly. RDSEED fails far more often than
// RDRAND when called back to back, since every word drains fresh entropy, so
// it retries up to 100 times with a PAUSE between attempts. Returns 1 with
// the word stored at [RDI], or 0 when the budget runs out.
_native_cpu_rdseed64:
    mov     ecx, 100
1:
    rdseed  rax
    jc      2f
    pause
    dec     ecx
    jnz     1b
    xor     eax, eax
    ret
2:
    mov     [rdi], rax
    mov     eax, 1
    ret

// ulong _native_cpu_rdtsc_ordered(void)
// RDTSC between two LFENCEs. A bare RDTSC may execute before earlier
// instructions finish or after later ones start; here the first LFENCE waits
// for the work before it and the second holds back the work after it, so
// two reads bracket exactly what runs between them. Returns the 64-bit TSC.
_native_cpu_rdtsc_ordered:
    lfence
    rdtsc
    lfence
    shl     rdx, 32
    or      rax, rdx
    ret
