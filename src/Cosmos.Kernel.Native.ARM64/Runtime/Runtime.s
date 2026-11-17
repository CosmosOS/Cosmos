// ARM64 NativeAOT Runtime Stubs
// Write barriers, interlocked reference exchanges and EH section accessors

.global RhpCheckedLockCmpXchg
.global RhpCheckedXchg
.global RhpAssignRefArm64
.global RhpCheckedAssignRefArm64
.global RhpByRefAssignRefArm64
.global RhpAssignRefAVLocation
.global RhpCheckedAssignRefAVLocation
.global RhpByRefAssignRefAVLocation1
.global get_eh_frame_start
.global get_eh_frame_end
.global get_dotnet_eh_table_start
.global get_dotnet_eh_table_end

.text
.align 4

// ============================================================================
// Interlocked exchanges on object references
//
// CoreLib reaches these through RuntimeImports.InterlockedCompareExchange and
// InterlockedExchange: Interlocked.CompareExchange and Exchange on a reference,
// so Lock, LazyInitializer, the thread pool's queues and ConcurrentQueue. ILC
// inlines the integer forms; the reference forms are calls, and were plain
// read-then-write C#, which a tick between the two halves broke.
//
// Exclusive load/store loops (ARMv8.0): the cores the kernel is built and
// tested for (cortex-a53, cortex-a72) have no LSE, where casal and swpal are
// undefined instructions. A CPU-features word read at boot can select them
// later. The trailing dmb ish makes the operation a full fence, as
// Interlocked promises and as the runtime's own non-LSE path does. No write
// barrier follows the store: the collector is neither generational nor
// concurrent, and RhpCheckedAssignRefArm64 is a plain store as well.
// ============================================================================

// object RhpCheckedLockCmpXchg(object* location, object value, object comparand)
// AAPCS64: x0 = location, x1 = value, x2 = comparand. Returns the value the
// location held before, in x0, whether or not it was replaced.
RhpCheckedLockCmpXchg:
1:
        ldaxr   x9, [x0]            // load, exclusive and acquire
        cmp     x9, x2
        b.ne    2f                  // not the comparand: nothing stored
        stlxr   w10, x1, [x0]       // store, release; w10 = 1 when the monitor was lost
        cbnz    w10, 1b
2:
        dmb     ish
        mov     x0, x9
        ret

// object RhpCheckedXchg(object* location, object value)
// AAPCS64: x0 = location, x1 = value. Returns the previous value in x0.
RhpCheckedXchg:
1:
        ldaxr   x9, [x0]
        stlxr   w10, x1, [x0]
        cbnz    w10, 1b
        dmb     ish
        mov     x0, x9
        ret

// void RhpByRefAssignRefArm64()
//
// Write barrier for by-reference assignments (copies object reference from one location to another)
//
// On entry:
//   x13 : source address (points to object reference to copy)
//   x14 : destination address (where to write the reference)
//
// On exit:
//   x13 : incremented by 8
//   x14 : incremented by 8
//   x15 : trashed
RhpByRefAssignRefArm64:
RhpByRefAssignRefAVLocation1:
        ldr     x15, [x13], #8      // Load source object reference, post-increment x13
        b       RhpCheckedAssignRefArm64

// void RhpCheckedAssignRefArm64()
//
// Write barrier for assignments to locations that may not be on the managed heap
// (e.g., static fields, stack locations)
//
// On entry:
//   x14 : destination address
//   x15 : object reference to store
//
// On exit:
//   x14 : incremented by 8
RhpCheckedAssignRefArm64:
RhpCheckedAssignRefAVLocation:
        // Store the object reference
        str     x15, [x14], #8      // Store and post-increment x14
        ret

// void RhpAssignRefArm64()
//
// Write barrier for assignments to the managed heap
// Ensures proper memory ordering for concurrent GC
//
// On entry:
//   x14 : destination address (on managed heap)
//   x15 : object reference to store
//
// On exit:
//   x14 : incremented by 8
RhpAssignRefArm64:
RhpAssignRefAVLocation:
        // Store the object reference
        str     x15, [x14], #8      // Store and post-increment x14

        // Memory barrier to ensure stores are visible to other cores and GC
        dmb     ish                 // Inner shareable domain barrier (sufficient for ARM64)

        ret

// void* get_eh_frame_start(void)
// Returns pointer to start of .eh_frame section
get_eh_frame_start:
    adrp    x0, __eh_frame_start
    add     x0, x0, :lo12:__eh_frame_start
    ret

// void* get_eh_frame_end(void)
// Returns pointer to end of .eh_frame section
get_eh_frame_end:
    adrp    x0, __eh_frame_end
    add     x0, x0, :lo12:__eh_frame_end
    ret

// void* get_dotnet_eh_table_start(void)
// Returns pointer to start of .dotnet_eh_table section
get_dotnet_eh_table_start:
    adrp    x0, __dotnet_eh_table_start
    add     x0, x0, :lo12:__dotnet_eh_table_start
    ret

// void* get_dotnet_eh_table_end(void)
// Returns pointer to end of .dotnet_eh_table section
get_dotnet_eh_table_end:
    adrp    x0, __dotnet_eh_table_end
    add     x0, x0, :lo12:__dotnet_eh_table_end
    ret

// ============================================================================
// Math functions for ARM64 are implemented in managed C# (RuntimeExport)
// in Cosmos.Kernel.Core/Runtime/Math.cs — ARM64 has no hardware
// transcendentals (unlike x87 FSIN/FCOS/FPATAN/F2XM1 on x64).
// ============================================================================
