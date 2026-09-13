using System.Runtime;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// Heap-allocated thread snapshot buffer the kernel updates from the timer
/// tick. The Cosmos VS Code extension reads it over QEMU's QMP socket while
/// the guest is running, so it can show live thread state without pausing
/// the kernel. <see cref="GetSnapshotAddr"/> exposes the buffer address for
/// the host to capture once at the first stop.
///
/// Layout (little-endian, packed):
///   0:  u32 magic = 0xC05D0001
///   4:  u32 version = 1
///   8:  u32 count            (valid entries)
///  12:  u32 reserved
///  16:  u64 seq              (seqlock; even = stable, odd = writing)
///  24:  entries[MaxEntries] of { u32 id, u32 cpu, u32 state, u32 flags }
/// </summary>
internal static unsafe class DebugLiveSnapshot
{
    private const uint Magic = 0xC05D0001u;
    private const uint Version = 1u;
    private const int MaxEntries = 64;
    private const int HeaderSize = 24;
    private const int EntrySize = 16;
    private const int BufferSize = HeaderSize + MaxEntries * EntrySize;

    private static byte* s_buffer;

    internal static void Initialize()
    {
        if (s_buffer != null)
        {
            return;
        }
        s_buffer = (byte*)MemoryOp.Alloc(BufferSize);
        if (s_buffer == null)
        {
            return;
        }
        for (int i = 0; i < BufferSize; i++)
        {
            s_buffer[i] = 0;
        }
        *(uint*)(s_buffer + 0) = Magic;
        *(uint*)(s_buffer + 4) = Version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Update()
    {
        byte* buf = s_buffer;
        if (buf == null)
        {
            return;
        }
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        if (threads == null)
        {
            return;
        }

        // Seqlock: bump to odd before writing, even after.
        ulong* seqPtr = (ulong*)(buf + 16);
        ulong seq = *seqPtr;
        *seqPtr = seq + 1;

        int count = 0;
        int n = threads.Length;
        if (n > MaxEntries)
        {
            n = MaxEntries;
        }
        for (int i = 0; i < n; i++)
        {
            SchedulerThread? t = threads[i];
            if (t == null)
            {
                continue;
            }
            byte* e = buf + HeaderSize + count * EntrySize;
            *(uint*)(e + 0) = t.Id;
            *(uint*)(e + 4) = t.CpuId;
            *(uint*)(e + 8) = (uint)t.State;
            *(uint*)(e + 12) = (uint)t.Flags;
            count++;
        }
        *(uint*)(buf + 8) = (uint)count;

        // Publish: make seq even again (and advance).
        *seqPtr = seq + 2;
    }

    [RuntimeExport("CosmosDbg_GetSnapshotAddr")]
    internal static ulong GetSnapshotAddr()
    {
        return (ulong)s_buffer;
    }
}
