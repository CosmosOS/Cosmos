// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.Security;

/// <summary>
/// CPU timing jitter, the entropy source every machine has: how long a small
/// memory walk takes, read on the cycle counter (RDTSC on x64, CNTPCT_EL0 on
/// ARM64), varies with caches, pipelines, interrupts and the hypervisor.
/// <see cref="KernelRandom"/> always mixes it in, and relies on it alone when
/// the CPU has no random number instruction.
/// </summary>
/// <remarks>
/// <para>The walk reads and writes a 4 KiB stack buffer at addresses that
/// depend on the bytes it reads, with a fixed instruction count and no data
/// dependent branch, and only the walk is timed, between counter reads
/// serialized with LFENCE or ISB so that they bracket it. A counter that is
/// not really running yields the same delta every time. A deterministic one,
/// such as QEMU's under -icount, yields the same delta, or two deltas in a
/// short repeating pattern when the walk does not span a whole number of
/// ticks.</para>
/// <para>The deltas go through three health tests, parameterized for the
/// min-entropy credited to each sample, 1/8 bit, and a false alarm rate of
/// 2^-30: the repetition count and adaptive proportion tests of NIST SP
/// 800-90B section 4.4, which catch a delta that sticks or dominates, and a
/// lag prediction test after section 6.3.8, which catches a repeating
/// pattern: predicting each delta as the one d samples back, for the d that
/// has guessed best so far up to 64, may not succeed more often than a
/// credit of 1/8 bit allows. A failure means the timer looks stuck or
/// predictable; the samples are mixed in all the same, they just count for
/// nothing. Passing proves no entropy, though: a deterministic timer whose
/// pattern is long or irregular enough passes, which is why
/// <see cref="KernelRandom"/> warns when jitter is all it had.</para>
/// <para>The credit itself is an assumption rather than a measurement of
/// each machine: about a quarter of the most common value estimate of a
/// desktop TSC. The walk stays in the L1 cache, so the jitter comes from the
/// pipeline, interrupts and the hypervisor rather than from memory.</para>
/// </remarks>
internal static unsafe class JitterEntropy
{
    /// <summary>Samples of the first seeding: 512 bits at the credited 1/8 bit each.</summary>
    internal const int SeedSamples = 4096;

    /// <summary>Samples a reseed adds when the CPU's instruction supplied its words: fresh timing rather than credit.</summary>
    internal const int ReseedSamples = 64;

    /// <summary>Samples per credited bit: each sample counts for 1/8 bit of min-entropy.</summary>
    internal const int SamplesPerBit = 8;

    /// <summary>Repetition count cutoff, 1 + ceil(30 / H) for H = 1/8 (SP 800-90B 4.4.1).</summary>
    private const int RepetitionCutoff = 241;

    /// <summary>Adaptive proportion window for non-binary samples (SP 800-90B 4.4.2).</summary>
    private const int ProportionWindow = 512;

    /// <summary>Adaptive proportion cutoff for W = 512, H = 1/8, alpha = 2^-30 (binomial critical value).</summary>
    private const int ProportionCutoff = 502;

    /// <summary>Lags of the lag prediction test, 1 to 64 samples back; a power of two.</summary>
    private const int PredictionLags = 64;

    /// <summary>
    /// Lag prediction cutoff: correct predictions out of the 4032 a collection
    /// of <see cref="SeedSamples"/> makes once every lag has a delta to look
    /// back at, for a success rate of 2^-1/8, the most a credit of H = 1/8
    /// allows, and alpha = 2^-30 (binomial critical value, about 94%). A
    /// reseed's <see cref="ReseedSamples"/> are too few for the test.
    /// </summary>
    private const int PredictionCutoff = 3799;

    /// <summary>Bytes of the walked buffer; a power of two.</summary>
    private const int WalkSize = 4096;

    /// <summary>Memory accesses per sample to start with.</summary>
    private const int BaseAccesses = 64;

    /// <summary>Most accesses per sample: the walk stops growing there even on a slow counter.</summary>
    private const int MaxAccesses = 4096;

    /// <summary>
    /// Counter ticks the fastest of a few walks must take before the length is
    /// kept. A TSC passes at once; a generic timer of a few tens of MHz on a fast
    /// core makes the walk grow until a sample spans several ticks.
    /// </summary>
    private const ulong MinTicksPerSample = 16;

    /// <summary>Trial walks per candidate length when calibrating.</summary>
    private const int CalibrationTrials = 4;

    /// <summary>
    /// Times <paramref name="samples"/> memory walks and absorbs every delta,
    /// plus the first and last counter values, into <paramref name="pool"/>.
    /// Takes about a millisecond for <see cref="SeedSamples"/> on a TSC.
    /// </summary>
    /// <returns>True when the deltas passed the health tests.</returns>
    internal static bool Collect(ref Blake2s pool, int samples)
    {
        byte* walk = stackalloc byte[WalkSize];
        ulong start = RandomNative.ReadCounter();
        for (int i = 0; i < WalkSize; i++)
        {
            walk[i] = (byte)(i ^ (int)(start >> ((i & 7) * 8)));
        }

        pool.Update((byte*)&start, sizeof(ulong));
        uint index = (uint)start;

        // Grow the walk until even the fastest of a few takes enough ticks.
        int accesses = BaseAccesses;
        while (accesses < MaxAccesses)
        {
            ulong fastest = ulong.MaxValue;
            for (int trial = 0; trial < CalibrationTrials; trial++)
            {
                ulong before = RandomNative.ReadCounter();
                index = Walk(walk, index, accesses);
                ulong took = RandomNative.ReadCounter() - before;
                if (took < fastest)
                {
                    fastest = took;
                }
            }

            if (fastest >= MinTicksPerSample)
            {
                break;
            }

            accesses *= 2;
        }

        // Deltas are absorbed sixteen at a time.
        uint* batch = stackalloc uint[16];
        int batched = 0;

        bool healthy = true;
        uint previous = 0;
        int repetitions = 0;
        uint windowValue = 0;
        int windowMatches = 0;
        int windowPosition = 0;

        // The lag prediction test's last deltas, a ring indexed by sample, and
        // how often each lag would have predicted right so far.
        uint* history = stackalloc uint[PredictionLags];
        int* scores = stackalloc int[PredictionLags];
        for (int lag = 0; lag < PredictionLags; lag++)
        {
            scores[lag] = 0;
        }

        int bestLag = 0;
        int predictionHits = 0;

        for (int i = 0; i < samples; i++)
        {
            ulong before = RandomNative.ReadCounter();
            index = Walk(walk, index, accesses);
            ulong after = RandomNative.ReadCounter();
            uint delta = (uint)(after - before);

            // Repetition count test: the same delta too many times in a row.
            if (i > 0 && delta == previous)
            {
                repetitions++;
                if (repetitions >= RepetitionCutoff)
                {
                    healthy = false;
                }
            }
            else
            {
                repetitions = 1;
            }

            previous = delta;

            // Adaptive proportion test: the first delta of a window recurring
            // in too much of it.
            if (windowPosition == 0)
            {
                windowValue = delta;
                windowMatches = 1;
            }
            else if (delta == windowValue)
            {
                windowMatches++;
                if (windowMatches >= ProportionCutoff)
                {
                    healthy = false;
                }
            }

            windowPosition++;
            if (windowPosition == ProportionWindow)
            {
                windowPosition = 0;
            }

            // Lag prediction test: the lag that has guessed best so far
            // predicts this delta, then every lag is scored on it. Index lag
            // looks lag + 1 samples back.
            if (i >= PredictionLags)
            {
                if (i < SeedSamples && history[(i - 1 - bestLag) & (PredictionLags - 1)] == delta)
                {
                    predictionHits++;
                }

                for (int lag = 0; lag < PredictionLags; lag++)
                {
                    if (history[(i - 1 - lag) & (PredictionLags - 1)] == delta)
                    {
                        scores[lag]++;
                        if (scores[lag] >= scores[bestLag])
                        {
                            bestLag = lag;
                        }
                    }
                }
            }

            history[i & (PredictionLags - 1)] = delta;

            // The delta also steers the next walk.
            index ^= delta;

            batch[batched++] = delta;
            if (batched == 16)
            {
                pool.Update((byte*)batch, 16 * sizeof(uint));
                batched = 0;
            }
        }

        if (batched > 0)
        {
            pool.Update((byte*)batch, batched * sizeof(uint));
        }

        if (samples >= SeedSamples && predictionHits >= PredictionCutoff)
        {
            healthy = false;
        }

        ulong end = RandomNative.ReadCounter();
        pool.Update((byte*)&end, sizeof(ulong));

        CryptographicOperations.ZeroMemory(new Span<byte>(batch, 16 * sizeof(uint)));
        CryptographicOperations.ZeroMemory(new Span<byte>(history, PredictionLags * sizeof(uint)));
        CryptographicOperations.ZeroMemory(new Span<byte>(walk, WalkSize));
        return healthy;
    }

    /// <summary>
    /// The timed work: <paramref name="accesses"/> read-modify-writes, each at an
    /// address derived from the byte read before it. Constant instruction count.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uint Walk(byte* walk, uint index, int accesses)
    {
        for (int j = 0; j < accesses; j++)
        {
            uint at = index & (WalkSize - 1);
            byte value = walk[at];
            walk[at] = (byte)(value + 1 + j);
            index = (index * 0x9E3779B1u) + value + (uint)j;
        }

        return index;
    }
}
