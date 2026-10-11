// Hardware random number registers (FEAT_RNG), read by the kernel random
// generator (Cosmos.Kernel.Core/Security), and the serialized counter read
// its jitter samples are timed with. RNDR and RNDRRS are UNDEFINED on a CPU
// without FEAT_RNG, which includes QEMU's cortex-a53 and cortex-a72: read
// ID_AA64ISAR0_EL1 first and touch them only when its RNDR field (bits
// 63:60) is not zero.
//
// The registers are named by their encodings, S3_3_C2_C4_0 (RNDR) and
// S3_3_C2_C4_1 (RNDRRS), so the file assembles without +rng. A read that
// succeeds clears NZCV; one that fails returns 0 and sets NZCV to 0b0100, so
// Z clear means success.

.global _native_cpu_read_id_aa64isar0
.global _native_cpu_rndr64
.global _native_cpu_rndrrs64
.global _native_cpu_cntpct_ordered

.text

// ulong _native_cpu_read_id_aa64isar0(void)
// Returns ID_AA64ISAR0_EL1 in x0.
.balign 4
_native_cpu_read_id_aa64isar0:
    mrs     x0, id_aa64isar0_el1
    ret

// int _native_cpu_rndr64(ulong* value /*x0*/)
// Reads RNDR, the DRBG output reseeded at an implementation defined rate.
// Returns 1 with the word stored at [x0], or 0 after 10 failed reads.
.balign 4
_native_cpu_rndr64:
    mov     x2, x0
    mov     w3, #10
1:
    mrs     x1, s3_3_c2_c4_0
    b.ne    2f
    subs    w3, w3, #1
    b.ne    1b
    mov     w0, #0
    ret
2:
    str     x1, [x2]
    mov     w0, #1
    ret

// int _native_cpu_rndrrs64(ulong* value /*x0*/)
// Reads RNDRRS, which reseeds the DRBG from the entropy source before every
// read: the seed grade source, slower and more likely to fail when read back
// to back, so it gets 100 attempts with a YIELD between them. Returns 1 with
// the word stored at [x0], or 0 when the budget runs out.
.balign 4
_native_cpu_rndrrs64:
    mov     x2, x0
    mov     w3, #100
1:
    mrs     x1, s3_3_c2_c4_1
    b.ne    2f
    yield
    subs    w3, w3, #1
    b.ne    1b
    mov     w0, #0
    ret
2:
    str     x1, [x2]
    mov     w0, #1
    ret

// ulong _native_cpu_cntpct_ordered(void)
// CNTPCT_EL0 between two ISBs. A bare MRS of the counter may be read early
// or late relative to the instructions around it; the first ISB makes it
// wait for the work before it and the second holds back the work after it,
// so two reads bracket exactly what runs between them.
.balign 4
_native_cpu_cntpct_ordered:
    isb
    mrs     x0, cntpct_el0
    isb
    ret
