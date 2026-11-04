// RISC-V 64 NativeAOT Runtime Stubs
// Write barriers, interlocked reference exchanges and EH section accessors
//
// The helper names and register conventions are the ones ILC's riscv64 code
// calls (JitHelper.cs and the runtime's riscv64/WriteBarriers.S): the
// destination in t3, the reference in t4, the by-reference source in t5.
// The collector is neither generational nor concurrent, so a barrier is the
// store alone: no card table to mark. RhpAssignRef, the standard calling
// convention form, is Cosmos.Kernel.Core's managed export (Stdllib.cs).

.global RhpCheckedLockCmpXchg
.global RhpCheckedXchg
.global RhpAssignRefRiscV64
.global RhpCheckedAssignRef
.global RhpByRefAssignRef
.global RhpAssignRefAVLocation
.global RhpCheckedAssignRefAVLocation
.global RhpByRefAssignRefAVLocation1
.global get_eh_frame_start
.global get_eh_frame_end
.global get_dotnet_eh_table_start
.global get_dotnet_eh_table_end

.text
.balign 4

// ============================================================================
// Interlocked exchanges on object references
//
// CoreLib reaches these through RuntimeImports.InterlockedCompareExchange and
// InterlockedExchange. LR/SC with acquire and release, then a full fence, as
// the runtime's own riscv64 helpers do: Interlocked promises a full barrier.
// ============================================================================

// object RhpCheckedLockCmpXchg(object* location, object value, object comparand)
// a0 = location, a1 = value, a2 = comparand. Returns the value the location
// held before, in a0, whether or not it was replaced.
RhpCheckedLockCmpXchg:
1:
        lr.d.aqrl   t0, (a0)
        bne         t0, a2, 2f          // not the comparand: nothing stored
        sc.d.rl     t1, a1, (a0)        // t1 = 0 when the store took
        bnez        t1, 1b
2:
        fence       rw, rw
        mv          a0, t0
        ret

// object RhpCheckedXchg(object* location, object value)
// a0 = location, a1 = value. Returns the previous value in a0.
RhpCheckedXchg:
        amoswap.d.aqrl t1, a1, (a0)
        fence       rw, rw
        mv          a0, t1
        ret

// void RhpByRefAssignRef()
// Copies the reference at [t5] to [t3].
// On exit: t5 and t3 incremented by 8, t4 trashed.
RhpByRefAssignRef:
RhpByRefAssignRefAVLocation1:
        ld          t4, 0(t5)
        addi        t5, t5, 8
        j           RhpCheckedAssignRef

// void RhpCheckedAssignRef()
// Store to a location that may not be on the managed heap (statics, stack).
// t3 = destination, t4 = reference. On exit: t3 incremented by 8.
RhpCheckedAssignRef:
RhpCheckedAssignRefAVLocation:
        sd          t4, 0(t3)
        addi        t3, t3, 8
        ret

// void RhpAssignRefRiscV64()
// Store to a location on the managed heap. The fence orders the object's
// initializing stores before the store that publishes it.
// t3 = destination, t4 = reference. On exit: t3 incremented by 8.
RhpAssignRefRiscV64:
        fence       rw, rw
RhpAssignRefAVLocation:
        sd          t4, 0(t3)
        addi        t3, t3, 8
        ret

// void* get_eh_frame_start(void)
// Returns pointer to start of .eh_frame section
get_eh_frame_start:
        la          a0, __eh_frame_start
        ret

// void* get_eh_frame_end(void)
// Returns pointer to end of .eh_frame section
get_eh_frame_end:
        la          a0, __eh_frame_end
        ret

// void* get_dotnet_eh_table_start(void)
// Returns pointer to start of .dotnet_eh_table section
get_dotnet_eh_table_start:
        la          a0, __dotnet_eh_table_start
        ret

// void* get_dotnet_eh_table_end(void)
// Returns pointer to end of .dotnet_eh_table section
get_dotnet_eh_table_end:
        la          a0, __dotnet_eh_table_end
        ret

// ============================================================================
// Math functions for RISC-V are implemented in managed C# (RuntimeExport) in
// Cosmos.Kernel.Core/Runtime/Math.cs, as on ARM64: the F and D extensions
// have no transcendental instructions.
// ============================================================================
