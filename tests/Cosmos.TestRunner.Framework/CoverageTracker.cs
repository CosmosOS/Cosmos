using System.Runtime.CompilerServices;

namespace Cosmos.TestRunner.Framework;

/// <summary>
/// Lightweight code coverage tracker for bare-metal kernel tests.
/// Instrumented methods call Hit(id) at entry. After tests complete,
/// Flush() sends hit data via the serial protocol.
///
/// CRITICAL: Hit() must be safe to call at ANY point during boot, including
/// before the managed heap, GC, or static constructors are ready.
/// Therefore this class uses a fixed-size buffer (placed in BSS by the linker)
/// instead of a managed byte[] allocation, eliminating all cctor/heap dependency.
/// </summary>
public static unsafe class CoverageTracker
{
    private const int MaxMethods = 16384;

    /// <summary>
    /// Fixed-size buffer stored as a value-type struct field.
    /// NativeAOT places this in the BSS segment (zero-initialized by the loader).
    /// No managed allocation, no cctor, no GC interaction.
    /// </summary>
    private unsafe struct HitBuffer
    {
        public fixed byte Data[MaxMethods];
    }

    private static HitBuffer s_hits;

    // Protocol constants (must match Cosmos.TestRunner.Protocol)
    private const byte CoverageData = 107;

    /// <summary>
    /// Record a method hit. Called by IL-instrumented method bodies.
    /// Entirely allocation-free and cctor-free — safe at any boot phase.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Hit(int id)
    {
        if ((uint)id < MaxMethods)
        {
            s_hits.Data[id] = 1;
        }
    }

    /// <summary>
    /// Send all coverage data via serial protocol.
    /// Called by TestRunner.Finish() after tests complete.
    /// </summary>
    public static void Flush()
    {
        // Count hits across entire buffer (16K scan is trivial)
        int hitCount = 0;
        for (int i = 0; i < MaxMethods; i++)
        {
            if (s_hits.Data[i] != 0)
            {
                hitCount++;
            }
        }

        if (hitCount == 0)
        {
            return;
        }

        // Build payload: [HitCount:2][HitId1:2][HitId2:2]...
        int payloadSize = 2 + hitCount * 2;
        var payload = new byte[payloadSize];

        // Write HitCount (LE ushort)
        payload[0] = (byte)(hitCount & 0xFF);
        payload[1] = (byte)((hitCount >> 8) & 0xFF);

        // Write hit method IDs
        int offset = 2;
        for (int i = 0; i < MaxMethods; i++)
        {
            if (s_hits.Data[i] != 0)
            {
                payload[offset] = (byte)(i & 0xFF);
                payload[offset + 1] = (byte)((i >> 8) & 0xFF);
                offset += 2;
            }
        }

        TestRunner.SendMessage(CoverageData, payload);
    }
}
