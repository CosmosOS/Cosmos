// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Security.Cryptography;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Security;

/// <summary>
/// The kernel's cryptographically secure random number generator. The
/// plugs route <c>RandomNumberGenerator</c> (and so BouncyCastle's
/// <c>SecureRandom</c> and every TLS key) and <c>Guid.NewGuid</c> here.
/// </summary>
/// <remarks>
/// <para>The design follows Linux's random driver. Entropy goes into a pool
/// hashed with BLAKE2s-256: the CPU's random number instruction when it has
/// one (RDSEED, else RDRAND, on x64; RNDRRS, else RNDR, on ARM64; see
/// <see cref="HardwareRandom"/>), CPU timing jitter always (see
/// <see cref="JitterEntropy"/>), and per-boot personalization that is not
/// credited (the bootloader's boot time, the HHDM offset, a stack address).
/// A 32-byte ChaCha20 key is extracted from the pool, and output comes from
/// that key with fast key erasure (see <see cref="FastKeyErasure"/>). Every
/// <see cref="ReseedRequestInterval"/> requests or
/// <see cref="ReseedByteInterval"/> bytes, fresh hardware words and timing
/// go into the pool and a new key is extracted. A reseed whose hardware
/// words fall short of 256 bits collects the full
/// <see cref="JitterEntropy.SeedSamples"/> rather than a few samples: a new
/// key from only a few guessable bits would let someone who once captured
/// the state follow it through every reseed.</para>
/// <para>Seeding is lazy: the first request gathers the entropy, which takes
/// a few milliseconds of jitter. Two threads that race into it both seed, and
/// both seedings land in the pool; neither waits for the other.</para>
/// <para>It never throws and never fails. A first seeding credited with less
/// than <see cref="MinimumSeedBits"/>, as on a kernel without a working
/// random number instruction whose timer also fails the jitter health tests,
/// still seeds from whatever there is, and the serial log gets one loud
/// warning; one that rests on jitter alone gets a shorter warning, since the
/// health tests cannot prove a timer unpredictable. An exception here would
/// surface in static constructors such as BouncyCastle's
/// <c>SecureRandom</c>, and a static constructor that throws poisons its type
/// for the whole boot.</para>
/// <para>Shared state is guarded by an interrupt-safe spin lock, released
/// explicitly rather than in a <c>finally</c>: nothing done while it is held
/// can throw, only fixed-size arithmetic and wipes through the unchecked
/// pointer entry points of <see cref="ChaCha20"/>, <see cref="Blake2s"/> and
/// <see cref="FastKeyErasure"/>, plus counter updates; entropy is gathered
/// and output expanded outside it. Callers may be on any thread. It is not
/// meant for interrupt handlers: the first request, and every reseed, does
/// far more work than an interrupt handler should.</para>
/// </remarks>
internal static unsafe class KernelRandom
{
    /// <summary>Requests between reseeds.</summary>
    internal const int ReseedRequestInterval = 256;

    /// <summary>Output bytes between reseeds.</summary>
    internal const long ReseedByteInterval = 1024 * 1024;

    /// <summary>Hardware words read for the first seeding (512 bits).</summary>
    private const int SeedHardwareWords = 8;

    /// <summary>Hardware words read for a reseed (256 bits).</summary>
    private const int ReseedHardwareWords = 4;

    /// <summary>Credited bits below which the first seeding prints the loud warning.</summary>
    private const int MinimumSeedBits = 256;

    /// <summary>Guards the pool, the key and the reseed counters.</summary>
    private static Scheduler.SpinLock s_lock;

    /// <summary>The entropy pool. Guarded by <see cref="s_lock"/>.</summary>
    private static Blake2s s_pool;

    /// <summary>Whether <see cref="s_pool"/> has been initialized. Guarded by <see cref="s_lock"/>.</summary>
    private static bool s_poolReady;

    /// <summary>The key and the reseed counters. Guarded by <see cref="s_lock"/>.</summary>
    private static GeneratorState s_generator;

    /// <summary>Non-zero once a seeding has keyed the generator.</summary>
    private static int s_seeded;

    /// <summary>Non-zero once the first seeding has been reported on the serial log.</summary>
    private static int s_reported;

    /// <summary>The hardware instruction the first seeding drew from.</summary>
    private static HardwareRandomSource s_seedSource;

    /// <summary>Whether the first seeding's jitter passed its health tests.</summary>
    private static bool s_jitterHealthy;

    /// <summary>Reseeds completed since boot.</summary>
    private static int s_reseeds;

    /// <summary>The generator's state, the key inline so it needs no allocation.</summary>
    private struct GeneratorState
    {
        /// <summary>The ChaCha20 key the next request retires.</summary>
        public fixed byte Key[ChaCha20.KeySize];

        /// <summary>Requests served since the last reseed.</summary>
        public int RequestsSinceReseed;

        /// <summary>Bytes served since the last reseed.</summary>
        public long BytesSinceReseed;
    }

    /// <summary>True once the generator has been seeded, by a request or by <see cref="EnsureSeeded"/>.</summary>
    internal static bool IsSeeded => Volatile.Read(ref s_seeded) != 0;

    /// <summary>
    /// The hardware instruction the first seeding drew from, or
    /// <see cref="HardwareRandomSource.None"/> when it had none and relied on jitter.
    /// </summary>
    internal static HardwareRandomSource SeedSource => s_seedSource;

    /// <summary>Whether the first seeding's jitter samples passed the SP 800-90B health tests.</summary>
    internal static bool JitterHealthy => s_jitterHealthy;

    /// <summary>Reseeds completed since boot, the first seeding not counted.</summary>
    internal static int ReseedCount => Volatile.Read(ref s_reseeds);

    /// <summary>Seeds the generator now if no request has yet.</summary>
    internal static void EnsureSeeded()
    {
        if (Volatile.Read(ref s_seeded) == 0)
        {
            Seed(true);
        }
    }

    /// <summary>Fills <paramref name="buffer"/> with random bytes. Never throws.</summary>
    internal static void Fill(Span<byte> buffer)
    {
        fixed (byte* p = buffer)
        {
            Fill(p, buffer.Length);
        }
    }

    /// <summary>
    /// Writes <paramref name="count"/> random bytes at <paramref name="buffer"/>;
    /// nothing when <paramref name="count"/> is not positive. Never throws.
    /// </summary>
    internal static void Fill(byte* buffer, int count)
    {
        if (count <= 0 || buffer == null)
        {
            return;
        }

        if (Volatile.Read(ref s_seeded) == 0)
        {
            Seed(true);
        }
        else if (Volatile.Read(ref s_generator.RequestsSinceReseed) >= ReseedRequestInterval
            || Volatile.Read(ref s_generator.BytesSinceReseed) >= ReseedByteInterval)
        {
            // The counters are read without the lock: a stale value only moves
            // the reseed by a request. Two threads may both see the threshold
            // and both reseed; that only mixes in more.
            Seed(false);
        }

        byte* requestKey = stackalloc byte[ChaCha20.KeySize];
        byte* firstBlock = stackalloc byte[ChaCha20.BlockSize];

        // Held for one ChaCha20 block, two 32-byte copies and two counters.
        IrqLockScope guard = s_lock.AcquireIrqSafe();
        fixed (byte* key = s_generator.Key)
        {
            FastKeyErasure.Ratchet(key, requestKey, firstBlock);
        }

        s_generator.RequestsSinceReseed++;
        s_generator.BytesSinceReseed += count;
        guard.Dispose();

        // The output comes from the retired key, now private to this call.
        FastKeyErasure.Expand(requestKey, firstBlock, buffer, count);
    }

    /// <summary>
    /// Gathers entropy, mixes it into the pool and extracts a new key. The
    /// first seeding takes 512 bits of hardware output and
    /// <see cref="JitterEntropy.SeedSamples"/> jitter samples; a reseed takes
    /// 256 bits and <see cref="JitterEntropy.ReseedSamples"/>, or all
    /// <see cref="JitterEntropy.SeedSamples"/> when the hardware fell short.
    /// </summary>
    private static void Seed(bool initial)
    {
        // Everything slow happens before the lock: hardware reads, jitter and
        // timestamps touch nothing shared, and are condensed to one digest.
        ulong* words = stackalloc ulong[SeedHardwareWords];
        byte* digest = stackalloc byte[Blake2s.HashSize];
        byte* seed = stackalloc byte[Blake2s.HashSize];
        byte* next = stackalloc byte[Blake2s.HashSize];

        Blake2s gather = default;
        gather.Initialize(Blake2s.HashSize, null, 0);

        int hardwareWords = HardwareRandom.Read(words, initial ? SeedHardwareWords : ReseedHardwareWords, out HardwareRandomSource source);
        gather.Update((byte*)words, hardwareWords * sizeof(ulong));

        // Without the hardware's 256 bits, a reseed credits jitter alone, so
        // it takes as many samples as the first seeding: a few milliseconds
        // every ReseedRequestInterval requests on such a machine.
        int samples = initial || hardwareWords < ReseedHardwareWords ? JitterEntropy.SeedSamples : JitterEntropy.ReseedSamples;
        bool jitterHealthy = JitterEntropy.Collect(ref gather, samples);

        AddPersonalization(ref gather, initial);
        gather.Finish(digest);

        // Held for the pool update and the key extraction: a few BLAKE2s
        // compressions over fixed-size buffers.
        IrqLockScope guard = s_lock.AcquireIrqSafe();
        if (!s_poolReady)
        {
            s_pool.Initialize(Blake2s.HashSize, null, 0);
            s_poolReady = true;
        }

        s_pool.Update(digest, Blake2s.HashSize);
        fixed (byte* key = s_generator.Key)
        {
            ExtractKey(key, seed, next);
        }

        s_generator.RequestsSinceReseed = 0;
        s_generator.BytesSinceReseed = 0;
        guard.Dispose();

        if (!initial)
        {
            Interlocked.Increment(ref s_reseeds);
        }

        // The first seeding to finish describes itself, before the generator
        // is marked seeded so that a reader of IsSeeded sees its sources.
        bool report = initial && Interlocked.Exchange(ref s_reported, 1) == 0;
        if (report)
        {
            s_seedSource = source;
            s_jitterHealthy = jitterHealthy;
        }

        Volatile.Write(ref s_seeded, 1);

        CryptographicOperations.ZeroMemory(new Span<byte>(words, SeedHardwareWords * sizeof(ulong)));
        CryptographicOperations.ZeroMemory(new Span<byte>(digest, Blake2s.HashSize));
        CryptographicOperations.ZeroMemory(new Span<byte>(seed, Blake2s.HashSize));
        CryptographicOperations.ZeroMemory(new Span<byte>(next, Blake2s.HashSize));

        if (report)
        {
            Report(source, hardwareWords, jitterHealthy);
        }
    }

    /// <summary>
    /// Replaces <paramref name="key"/> with a key extracted from the pool, the
    /// way Linux's extract_entropy does: seed = BLAKE2s(pool), next =
    /// BLAKE2s(key: seed, 0x00), key = BLAKE2s(key: seed, 0x01), and the pool
    /// carries on keyed with next, so everything mixed in so far stays in it
    /// while neither the key nor the old pool state can be recovered from the
    /// other. Called with <see cref="s_lock"/> held; never throws.
    /// </summary>
    /// <param name="key">Receives the new 32-byte key.</param>
    /// <param name="seed">32 bytes of scratch.</param>
    /// <param name="next">32 bytes of scratch.</param>
    private static void ExtractKey(byte* key, byte* seed, byte* next)
    {
        s_pool.Finish(seed);

        byte label = 0;
        Blake2s prf = default;
        prf.Initialize(Blake2s.HashSize, seed, Blake2s.HashSize);
        prf.Update(&label, 1);
        prf.Finish(next);

        label = 1;
        prf.Initialize(Blake2s.HashSize, seed, Blake2s.HashSize);
        prf.Update(&label, 1);
        prf.Finish(key);

        s_pool.Initialize(Blake2s.HashSize, next, Blake2s.HashSize);
    }

    /// <summary>
    /// Per-boot and per-call values that make two machines, or two boots,
    /// unlikely to share a seed even with no entropy at all. Not credited.
    /// </summary>
    private static void AddPersonalization(ref Blake2s gather, bool initial)
    {
        ulong now = RandomNative.ReadCounter();
        gather.Update((byte*)&now, sizeof(ulong));

        if (!initial)
        {
            return;
        }

        if (Limine.BootTime.Response != null)
        {
            long bootTime = Limine.BootTime.Response->BootTime;
            gather.Update((byte*)&bootTime, sizeof(long));
        }

        if (Limine.HHDM.Response != null)
        {
            ulong hhdm = Limine.HHDM.Response->Offset;
            gather.Update((byte*)&hhdm, sizeof(ulong));
        }

        nuint stack = (nuint)(&now);
        gather.Update((byte*)&stack, sizeof(nuint));
    }

    /// <summary>
    /// One line on what the first seeding used, plus a warning when it had too
    /// little credited entropy, or none from the hardware.
    /// </summary>
    private static void Report(HardwareRandomSource source, int hardwareWords, bool jitterHealthy)
    {
        Serial.WriteString("[Random] Kernel CSPRNG seeded: ");
        if (source != HardwareRandomSource.None)
        {
            Serial.WriteString(HardwareRandom.NameOf(source));
            Serial.WriteString(" (");
            Serial.WriteNumber(hardwareWords * 64);
            Serial.WriteString(" bits) + ");
        }

        Serial.WriteNumber(JitterEntropy.SeedSamples);
        Serial.WriteString(" jitter samples, health tests ");
        Serial.WriteString(jitterHealthy ? "passed" : "FAILED");
        Serial.WriteString("\n");

        // Every hardware word counts for its 64 bits and healthy jitter for
        // its credit; a word from an instruction found broken later in the
        // same read still counts, so this errs on the generous side.
        int creditedBits = (hardwareWords * 64) + (jitterHealthy ? JitterEntropy.SeedSamples / JitterEntropy.SamplesPerBit : 0);
        if (creditedBits < MinimumSeedBits)
        {
            Serial.WriteString(
                "[Random] ***************************************************************\n" +
                "[Random] WARNING: NO SECURE ENTROPY. This CPU has no working random\n" +
                "[Random] number instruction (RDSEED/RDRAND, RNDRRS/RNDR) and its timer\n" +
                "[Random] failed the jitter health tests: less than 256 bits of the seed\n" +
                "[Random] are credited. RandomNumberGenerator, Guid.NewGuid and every TLS\n" +
                "[Random] key may be predictable: do not rely on them for security on\n" +
                "[Random] this machine.\n" +
                "[Random] ***************************************************************\n");
        }
        else if (hardwareWords == 0)
        {
            Serial.WriteString(
                "[Random] WARNING: no CPU random number instruction; the seed rests on\n" +
                "[Random] timer jitter alone. Its health tests catch a stuck or repeating\n" +
                "[Random] timer, not every predictable one: on a machine that runs\n" +
                "[Random] deterministically (QEMU -icount or record/replay) the output\n" +
                "[Random] is predictable.\n");
        }
    }
}
