// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

// -----------------------------------------------------------------------------
// Cosmos.Kernel.Tests.Random — the kernel CSPRNG (Cosmos.Kernel.Core/Security)
// and the BCL randomness routed to it: known-answer tests of its BLAKE2s and
// ChaCha20 primitives, fast key erasure, the hardware detection paths, and
// RandomNumberGenerator, GetInt32 and Guid.NewGuid on top. The serial log
// names the entropy source that seeded the generator on each cell.
// -----------------------------------------------------------------------------

using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using Cosmos.Kernel.Core.Security;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timers;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using SysThread = System.Threading.Thread;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Random;

public unsafe class Kernel : Sys.Kernel
{
    /// <summary>Number of test cases registered with the TestRunner in BeforeRun.</summary>
    private const int ExpectedTestCount = 16;

    /// <summary>Size of the statistics draw (64 KiB).</summary>
    private const int StatisticsDrawBytes = 64 * 1024;

    /// <summary>Largest distance from half of the drawn bits, 1% of half (over 7 standard deviations).</summary>
    private const int BitBalanceTolerance = StatisticsDrawBytes * 8 / 2 / 100;

    /// <summary>Each byte value is expected 256 times in 64 KiB; the bounds are about 7 standard deviations out.</summary>
    private const int MinByteCount = 144;
    private const int MaxByteCount = 368;

    /// <summary>Draws of the GetInt32 range test.</summary>
    private const int RangeDraws = 1000;

    /// <summary>Requests of the reseed test: more than KernelRandom.ReseedRequestInterval.</summary>
    private const int ReseedTestRequests = KernelRandom.ReseedRequestInterval + 44;

    /// <summary>
    /// Fewest requests each thread makes in the concurrency test, together
    /// enough to cross several reseeds however slow the cell.
    /// </summary>
    private const int ConcurrentMinRequestsPerThread = 600;

    /// <summary>How long both threads draw in the concurrency test: many 10 ms scheduler quanta.</summary>
    private const uint ConcurrentWindowMs = 300;

    /// <summary>Draws per thread whose first 8 bytes the concurrency test keeps for the duplicate check.</summary>
    private const int ConcurrentRecordedDraws = 4096;

    /// <summary>Longest wait for the concurrency test's threads to start or finish, in 100 ms steps.</summary>
    private const int ConcurrentWaitSteps = 100;

    /// <summary>Threads of the concurrency test.</summary>
    private const int ConcurrentThreads = 2;

    private static int s_threadsReady;
    private static int s_threadsDone;
    private static volatile bool s_drawGo;
    private static volatile bool s_drawStop;
    private static int s_drawSequence;

    /// <summary>What one thread of the concurrency test drew.</summary>
    private sealed class DrawRecord
    {
        /// <summary>The first 8 bytes of each of the first draws.</summary>
        public readonly ulong[] Prefixes = new ulong[ConcurrentRecordedDraws];

        /// <summary>Draws made, recorded or not.</summary>
        public int Count;

        /// <summary>Sequence number taken before the first draw, or -1 before any.</summary>
        public int FirstSequence = -1;

        /// <summary>Sequence number taken before the last draw.</summary>
        public int LastSequence = -1;
    }

    protected override void BeforeRun()
    {
        Log.WriteString("[Random] BeforeRun() reached!\n");

        TR.Start("Random Tests", expectedTests: ExpectedTestCount);

        // Primitives, known-answer tests.
        TR.Run("ChaCha20_Block_Rfc8439", Test_ChaCha20_Block);
        TR.Run("ChaCha20_Keystream_Rfc8439", Test_ChaCha20_Keystream);
        TR.Run("Blake2s_Abc_Rfc7693", Test_Blake2s_Abc);
        TR.Run("Blake2s_Empty", Test_Blake2s_Empty);
        TR.Run("Blake2s_SelfTest_Rfc7693", Test_Blake2s_SelfTest);
        TR.Run("FastKeyErasure_NextKeyNeverOutput", Test_FastKeyErasure);

        // Entropy sources and seeding.
        TR.Run("Hardware_Detection", Test_Hardware_Detection);
        TR.Run("KernelRandom_Seeding", Test_KernelRandom_Seeding);

        // The BCL on top.
        TR.Run("Fill_TwoDrawsDiffer", Test_Fill_TwoDrawsDiffer);
        TR.Run("Fill_ZeroLength", Test_Fill_ZeroLength);
        TR.Run("Fill_64KiB_Statistics", Test_Fill_Statistics);
        TR.Run("Create_GetBytes_NonZero", Test_Create_GetBytes);
        TR.Run("GetInt32_Range", Test_GetInt32_Range);
        TR.Run("Guid_NewGuid_Differ", Test_Guid_NewGuid);
        TR.Run("Fill_AcrossReseed", Test_Fill_AcrossReseed);
        TR.Run("Fill_ConcurrentThreads", Test_Fill_ConcurrentThreads);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // =========================================================================
    // Known-answer vectors
    // =========================================================================

    /// <summary>RFC 8439 section 2.3.2: key 00..1f, nonce 00:00:00:09:00:00:00:4a:00:00:00:00, counter 1.</summary>
    private static ReadOnlySpan<byte> ChaChaBlockExpected =>
    [
        0x10, 0xF1, 0xE7, 0xE4, 0xD1, 0x3B, 0x59, 0x15, 0x50, 0x0F, 0xDD, 0x1F, 0xA3, 0x20, 0x71, 0xC4,
        0xC7, 0xD1, 0xF4, 0xC7, 0x33, 0xC0, 0x68, 0x03, 0x04, 0x22, 0xAA, 0x9A, 0xC3, 0xD4, 0x6C, 0x4E,
        0xD2, 0x82, 0x64, 0x46, 0x07, 0x9F, 0xAA, 0x09, 0x14, 0xC2, 0xD7, 0x05, 0xD9, 0x8B, 0x02, 0xA2,
        0xB5, 0x12, 0x9C, 0xD1, 0xDE, 0x16, 0x4E, 0xB9, 0xCB, 0xD0, 0x83, 0xE8, 0xA2, 0x50, 0x3C, 0x4E,
    ];

    /// <summary>RFC 8439 section 2.4.2: the "sunscreen" plaintext under key 00..1f, nonce ...4a..., counter 1.</summary>
    private static ReadOnlySpan<byte> ChaChaCiphertextExpected =>
    [
        0x6E, 0x2E, 0x35, 0x9A, 0x25, 0x68, 0xF9, 0x80, 0x41, 0xBA, 0x07, 0x28, 0xDD, 0x0D, 0x69, 0x81,
        0xE9, 0x7E, 0x7A, 0xEC, 0x1D, 0x43, 0x60, 0xC2, 0x0A, 0x27, 0xAF, 0xCC, 0xFD, 0x9F, 0xAE, 0x0B,
        0xF9, 0x1B, 0x65, 0xC5, 0x52, 0x47, 0x33, 0xAB, 0x8F, 0x59, 0x3D, 0xAB, 0xCD, 0x62, 0xB3, 0x57,
        0x16, 0x39, 0xD6, 0x24, 0xE6, 0x51, 0x52, 0xAB, 0x8F, 0x53, 0x0C, 0x35, 0x9F, 0x08, 0x61, 0xD8,
        0x07, 0xCA, 0x0D, 0xBF, 0x50, 0x0D, 0x6A, 0x61, 0x56, 0xA3, 0x8E, 0x08, 0x8A, 0x22, 0xB6, 0x5E,
        0x52, 0xBC, 0x51, 0x4D, 0x16, 0xCC, 0xF8, 0x06, 0x81, 0x8C, 0xE9, 0x1A, 0xB7, 0x79, 0x37, 0x36,
        0x5A, 0xF9, 0x0B, 0xBF, 0x74, 0xA3, 0x5B, 0xE6, 0xB4, 0x0B, 0x8E, 0xED, 0xF2, 0x78, 0x5E, 0x42,
        0x87, 0x4D,
    ];

    /// <summary>RFC 7693 Appendix B: BLAKE2s-256("abc").</summary>
    private static ReadOnlySpan<byte> Blake2sAbcExpected =>
    [
        0x50, 0x8C, 0x5E, 0x8C, 0x32, 0x7C, 0x14, 0xE2, 0xE1, 0xA7, 0x2B, 0xA3, 0x4E, 0xEB, 0x45, 0x2F,
        0x37, 0x45, 0x8B, 0x20, 0x9E, 0xD6, 0x3A, 0x29, 0x4D, 0x99, 0x9B, 0x4C, 0x86, 0x67, 0x59, 0x82,
    ];

    /// <summary>BLAKE2s-256 of the empty input.</summary>
    private static ReadOnlySpan<byte> Blake2sEmptyExpected =>
    [
        0x69, 0x21, 0x7A, 0x30, 0x79, 0x90, 0x80, 0x94, 0xE1, 0x11, 0x21, 0xD0, 0x42, 0x35, 0x4A, 0x7C,
        0x1F, 0x55, 0xB6, 0x48, 0x2C, 0xA1, 0xA5, 0x1E, 0x1B, 0x25, 0x0D, 0xFD, 0x1E, 0xD0, 0xEE, 0xF9,
    ];

    /// <summary>RFC 7693 Appendix E: the hash of the self-test's 48 keyed and unkeyed hashes.</summary>
    private static ReadOnlySpan<byte> Blake2sSelfTestExpected =>
    [
        0x6A, 0x41, 0x1F, 0x08, 0xCE, 0x25, 0xAD, 0xCD, 0xFB, 0x02, 0xAB, 0xA6, 0x41, 0x45, 0x1C, 0xEC,
        0x53, 0xC5, 0x98, 0xB2, 0x4F, 0x4F, 0xC7, 0x87, 0xFB, 0xDC, 0x88, 0x79, 0x7F, 0x4C, 0x1D, 0xFE,
    ];

    private static byte[] CountingKey()
    {
        byte[] key = new byte[ChaCha20.KeySize];
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)i;
        }

        return key;
    }

    // =========================================================================
    // Primitives
    // =========================================================================

    private static void Test_ChaCha20_Block()
    {
        byte[] nonce = [0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x4A, 0x00, 0x00, 0x00, 0x00];
        byte[] block = new byte[ChaCha20.BlockSize];
        ChaCha20.Block(CountingKey(), 1, nonce, block);
        Assert.True(block.AsSpan().SequenceEqual(ChaChaBlockExpected), "ChaCha20 block matches RFC 8439 2.3.2");
    }

    private static void Test_ChaCha20_Keystream()
    {
        ReadOnlySpan<byte> plaintext = "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it."u8;
        byte[] nonce = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4A, 0x00, 0x00, 0x00, 0x00];
        byte[] ciphertext = new byte[plaintext.Length];
        ChaCha20.Keystream(CountingKey(), 1, nonce, ciphertext);
        for (int i = 0; i < ciphertext.Length; i++)
        {
            ciphertext[i] ^= plaintext[i];
        }

        Assert.Equal(114, ciphertext.Length, "The RFC plaintext is 114 bytes, a partial last block");
        Assert.True(ciphertext.AsSpan().SequenceEqual(ChaChaCiphertextExpected), "ChaCha20 encryption matches RFC 8439 2.4.2");
    }

    private static void Test_Blake2s_Abc()
    {
        byte[] digest = new byte[Blake2s.HashSize];
        Blake2s.Hash(default, "abc"u8, digest);
        Assert.True(digest.AsSpan().SequenceEqual(Blake2sAbcExpected), "BLAKE2s-256(\"abc\") matches RFC 7693 Appendix B");
    }

    private static void Test_Blake2s_Empty()
    {
        byte[] digest = new byte[Blake2s.HashSize];
        Blake2s.Hash(default, default, digest);
        Assert.True(digest.AsSpan().SequenceEqual(Blake2sEmptyExpected), "BLAKE2s-256 of the empty input");
    }

    /// <summary>
    /// RFC 7693 Appendix E: unkeyed and keyed hashes of 0, 3, 64, 65, 255 and
    /// 1024 bytes at digest sizes 16, 20, 28 and 32, all hashed together. Covers
    /// keys, short digests and inputs on and off the block boundary.
    /// </summary>
    private static void Test_Blake2s_SelfTest()
    {
        int[] digestSizes = [16, 20, 28, 32];
        int[] inputSizes = [0, 3, 64, 65, 255, 1024];

        Blake2s grand = default;
        grand.Initialize(Blake2s.HashSize);
        byte[] digest = new byte[Blake2s.HashSize];
        for (int d = 0; d < digestSizes.Length; d++)
        {
            int outLength = digestSizes[d];
            for (int s = 0; s < inputSizes.Length; s++)
            {
                int inLength = inputSizes[s];
                byte[] input = SelfTestSequence(inLength, (uint)inLength);
                Span<byte> md = digest.AsSpan(0, outLength);
                Blake2s.Hash(default, input, md);
                grand.Update(md);
                byte[] key = SelfTestSequence(outLength, (uint)outLength);
                Blake2s.Hash(key, input, md);
                grand.Update(md);
            }
        }

        grand.Finish(digest);
        Assert.True(digest.AsSpan().SequenceEqual(Blake2sSelfTestExpected), "BLAKE2s self-test grand hash matches RFC 7693 Appendix E");
    }

    /// <summary>The deterministic byte sequence of RFC 7693 Appendix E.</summary>
    private static byte[] SelfTestSequence(int length, uint seed)
    {
        byte[] output = new byte[length];
        uint a = 0xDEAD4BAD * seed;
        uint b = 1;
        for (int i = 0; i < length; i++)
        {
            uint t = a + b;
            a = b;
            b = t;
            output[i] = (byte)(t >> 24);
        }

        return output;
    }

    /// <summary>
    /// One request through the generator's own construction: the next key is
    /// the first 32 bytes of the retired key's keystream and never appears in
    /// the output, which is the rest of that keystream; the scratch is wiped.
    /// </summary>
    private static void Test_FastKeyErasure()
    {
        byte[] key = CountingKey();
        byte[] retired = CountingKey();
        byte[] requestKey = new byte[ChaCha20.KeySize];
        byte[] firstBlock = new byte[ChaCha20.BlockSize];
        byte[] output = new byte[200];
        fixed (byte* k = key)
        fixed (byte* rk = requestKey)
        fixed (byte* fb = firstBlock)
        fixed (byte* o = output)
        {
            FastKeyErasure.Ratchet(k, rk, fb);
            FastKeyErasure.Expand(rk, fb, o, output.Length);
        }

        byte[] reference = new byte[ChaCha20.KeySize + output.Length];
        ChaCha20.Keystream(retired, 0, new byte[ChaCha20.NonceSize], reference);

        Assert.True(key.AsSpan().SequenceEqual(reference.AsSpan(0, ChaCha20.KeySize)), "The next key is the first 32 keystream bytes");
        Assert.False(key.AsSpan().SequenceEqual(retired), "The key changed");
        Assert.True(output.AsSpan().SequenceEqual(reference.AsSpan(ChaCha20.KeySize)), "The output is the keystream after the next key");
        Assert.True(output.AsSpan().IndexOf(key) < 0, "The next key never appears in the output");
        Assert.True(requestKey.AsSpan().IndexOfAnyExcept((byte)0) < 0, "The retired key copy is wiped");
        Assert.True(firstBlock.AsSpan().IndexOfAnyExcept((byte)0) < 0, "The first block is wiped");
    }

    // =========================================================================
    // Entropy sources
    // =========================================================================

    /// <summary>
    /// The CPUID / ID_AA64ISAR0 probe runs without faulting, and an advertised
    /// instruction delivers healthy words. Logs what this cell has: x64 under
    /// KVM or -cpu max has RDSEED and RDRAND; QEMU's cortex-a72 has neither
    /// RNDR nor RNDRRS.
    /// </summary>
    private static void Test_Hardware_Detection()
    {
        HardwareRandomSource source = HardwareRandom.Source;
        Log.WriteString("[Random] Hardware random instruction: ");
        Log.WriteString(HardwareRandom.NameOf(source));
        Log.WriteString("\n");

        ulong* words = stackalloc ulong[4];
        int read = HardwareRandom.Read(words, 4, out HardwareRandomSource used);
        Log.WriteString("[Random] Hardware words read: ");
        Log.WriteNumber(read);
        Log.WriteString(" from ");
        Log.WriteString(HardwareRandom.NameOf(used));
        Log.WriteString("\n");

        if (source == HardwareRandomSource.None)
        {
            Assert.Equal(0, read, "No instruction, no words");
            return;
        }

        Assert.True(read > 0, "An advertised instruction delivers words");
        for (int i = 0; i < read; i++)
        {
            Assert.True(words[i] != 0 && words[i] != ulong.MaxValue, "Hardware words are neither zero nor all ones");
            for (int j = 0; j < i; j++)
            {
                Assert.True(words[i] != words[j], "Hardware words do not repeat");
            }
        }
    }

    /// <summary>Seeding completes and reports its sources; the jitter verdict is logged, not asserted.</summary>
    private static void Test_KernelRandom_Seeding()
    {
        KernelRandom.EnsureSeeded();
        Assert.True(KernelRandom.IsSeeded, "The generator is seeded");

        Log.WriteString("[Random] Seeded from: ");
        Log.WriteString(HardwareRandom.NameOf(KernelRandom.SeedSource));
        Log.WriteString(KernelRandom.JitterHealthy ? " + jitter (health tests passed)\n" : " + jitter (health tests FAILED)\n");

        if (HardwareRandom.Source != HardwareRandomSource.None)
        {
            Assert.True(KernelRandom.SeedSource != HardwareRandomSource.None, "An advertised instruction fed the first seeding");
        }
    }

    // =========================================================================
    // The BCL on top
    // =========================================================================

    private static void Test_Fill_TwoDrawsDiffer()
    {
        byte[] first = new byte[32];
        byte[] second = new byte[32];
        RandomNumberGenerator.Fill(first);
        RandomNumberGenerator.Fill(second);

        Assert.True(first.AsSpan().IndexOfAnyExcept((byte)0) >= 0, "The first draw is not all zero");
        Assert.True(second.AsSpan().IndexOfAnyExcept((byte)0) >= 0, "The second draw is not all zero");
        Assert.False(first.AsSpan().SequenceEqual(second), "Two draws differ");
    }

    private static void Test_Fill_ZeroLength()
    {
        RandomNumberGenerator.Fill(Span<byte>.Empty);
        byte[] empty = RandomNumberGenerator.GetBytes(0);
        Assert.Equal(0, empty.Length, "GetBytes(0) is empty");

        // A zero-length request leaves the buffer alone.
        byte[] untouched = [0xA5];
        RandomNumberGenerator.Fill(untouched.AsSpan(0, 0));
        Assert.Equal((byte)0xA5, untouched[0], "A zero-length Fill writes nothing");

        // The BCL never passes a zero length down, so the generator's own guard
        // is reached directly: no count, a negative one, and a null buffer.
        byte[] guarded = new byte[16];
        guarded.AsSpan().Fill(0xA5);
        fixed (byte* p = guarded)
        {
            KernelRandom.Fill(p, 0);
            KernelRandom.Fill(p, -1);
            KernelRandom.Fill(null, guarded.Length);
        }

        Assert.True(guarded.AsSpan().IndexOfAnyExcept((byte)0xA5) < 0, "KernelRandom.Fill writes nothing for a count of 0 or less, or a null buffer");
    }

    /// <summary>
    /// 64 KiB: bit balance within 1% of half, every byte value present in a
    /// plausible number, and no 8-byte word appearing twice.
    /// </summary>
    private static void Test_Fill_Statistics()
    {
        byte[] data = new byte[StatisticsDrawBytes];
        RandomNumberGenerator.Fill(data);

        long ones = 0;
        int[] counts = new int[256];
        for (int i = 0; i < data.Length; i++)
        {
            ones += BitOperations.PopCount(data[i]);
            counts[data[i]]++;
        }

        long half = StatisticsDrawBytes * 8L / 2;
        Log.WriteString("[Random] 64 KiB draw: ");
        Log.WriteNumber(ones);
        Log.WriteString(" one bits of ");
        Log.WriteNumber(half * 2);
        Log.WriteString("\n");
        Assert.True(ones > half - BitBalanceTolerance && ones < half + BitBalanceTolerance, "Bit balance within 1% of half");

        int lowest = int.MaxValue;
        int highest = 0;
        for (int v = 0; v < 256; v++)
        {
            lowest = Math.Min(lowest, counts[v]);
            highest = Math.Max(highest, counts[v]);
        }

        Assert.True(lowest >= MinByteCount && highest <= MaxByteCount, "Every byte value appears a plausible number of times");

        ulong[] words = new ulong[StatisticsDrawBytes / sizeof(ulong)];
        fixed (byte* p = data)
        {
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = ((ulong*)p)[i];
            }
        }

        Array.Sort(words);
        bool repeated = false;
        for (int i = 1; i < words.Length; i++)
        {
            if (words[i] == words[i - 1])
            {
                repeated = true;
            }
        }

        Assert.False(repeated, "No 8-byte word repeats in 64 KiB");
    }

    /// <summary>The instance API: Create, GetBytes over an array slice, GetNonZeroBytes.</summary>
    private static void Test_Create_GetBytes()
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        Assert.NotNull(rng, "Create returns an instance");

        byte[] buffer = new byte[48];
        buffer[0] = 0x5A;
        buffer[47] = 0x5A;
        rng.GetBytes(buffer, 1, 46);
        Assert.Equal((byte)0x5A, buffer[0], "GetBytes(offset, count) leaves the byte before alone");
        Assert.Equal((byte)0x5A, buffer[47], "GetBytes(offset, count) leaves the byte after alone");
        Assert.True(buffer.AsSpan(1, 46).IndexOfAnyExcept((byte)0) >= 0, "The slice was filled");

        byte[] nonZero = new byte[1024];
        rng.GetNonZeroBytes(nonZero);
        Assert.True(nonZero.AsSpan().IndexOf((byte)0) < 0, "GetNonZeroBytes returns no zero");
    }

    private static void Test_GetInt32_Range()
    {
        int[] hits = new int[10];
        bool inRange = true;
        for (int i = 0; i < RangeDraws; i++)
        {
            int value = RandomNumberGenerator.GetInt32(10, 20);
            if (value < 10 || value >= 20)
            {
                inRange = false;
                continue;
            }

            hits[value - 10]++;
        }

        Assert.True(inRange, "GetInt32(10, 20) stays in [10, 20)");
        for (int v = 0; v < hits.Length; v++)
        {
            Assert.True(hits[v] > 0, "Every value of [10, 20) comes up in 1000 draws");
        }

        int bounded = RandomNumberGenerator.GetInt32(1);
        Assert.Equal(0, bounded, "GetInt32(1) is 0");
    }

    private static void Test_Guid_NewGuid()
    {
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        Assert.True(first != Guid.Empty, "NewGuid is not empty");
        Assert.True(first != second, "Two NewGuid values differ");
        Assert.Equal(4, first.Version, "NewGuid is a version 4 GUID");
    }

    /// <summary>
    /// More requests than the reseed interval, plus one draw past the byte
    /// interval: each reseeds the generator, and output stays distinct across
    /// the reseeds.
    /// </summary>
    private static void Test_Fill_AcrossReseed()
    {
        int reseedsBefore = KernelRandom.ReseedCount;
        byte[] previous = new byte[16];
        byte[] current = new byte[16];
        RandomNumberGenerator.Fill(previous);
        bool allDistinct = true;
        for (int i = 0; i < ReseedTestRequests; i++)
        {
            RandomNumberGenerator.Fill(current);
            if (current.AsSpan().SequenceEqual(previous))
            {
                allDistinct = false;
            }

            current.AsSpan().CopyTo(previous);
        }

        Assert.True(allDistinct, "Consecutive draws differ across a request-count reseed");
        int reseedsAfterRequests = KernelRandom.ReseedCount;
        Assert.True(reseedsAfterRequests > reseedsBefore, "More requests than the reseed interval reseed the generator");

        // The byte counter passes its interval within the large draw, so the
        // request after it reseeds whatever the request counter says.
        byte[] large = new byte[(int)KernelRandom.ReseedByteInterval + 4096];
        RandomNumberGenerator.Fill(large);
        RandomNumberGenerator.Fill(current);
        Assert.True(large.AsSpan(large.Length - 4096).IndexOfAnyExcept((byte)0) >= 0, "The tail of a 1 MiB draw is filled");
        Assert.False(current.AsSpan().SequenceEqual(previous), "Draws differ across a byte-count reseed");
        Assert.True(KernelRandom.ReseedCount > reseedsAfterRequests, "A request after more than 1 MiB of output reseeds the generator");

        Log.WriteString("[Random] Reseeds during the test: ");
        Log.WriteNumber(KernelRandom.ReseedCount - reseedsBefore);
        Log.WriteString("\n");
    }

    /// <summary>
    /// Two threads draw at the same time, through several reseeds. Both wait
    /// at a start gate until the other exists, then draw for many scheduler
    /// quanta while this one waits on the timer, so the tick preempts them in
    /// the middle of their draws. A shared sequence number taken before every
    /// draw proves the two ran interleaved rather than one after the other,
    /// and no two of their draws may be equal, which a key retired by both at
    /// once would produce.
    /// </summary>
    private static void Test_Fill_ConcurrentThreads()
    {
        s_threadsReady = 0;
        s_threadsDone = 0;
        s_drawGo = false;
        s_drawStop = false;
        s_drawSequence = 0;
        int reseedsBefore = KernelRandom.ReseedCount;

        DrawRecord a = new DrawRecord();
        DrawRecord b = new DrawRecord();
        SysThread threadA = new SysThread(() => DrawUntilStopped(a));
        SysThread threadB = new SysThread(() => DrawUntilStopped(b));
        threadA.Start();
        threadB.Start();

        for (int i = 0; i < ConcurrentWaitSteps && Volatile.Read(ref s_threadsReady) < ConcurrentThreads; i++)
        {
            TimerManager.Wait(100);
        }

        s_drawGo = true;
        TimerManager.Wait(ConcurrentWindowMs);
        s_drawStop = true;

        for (int i = 0; i < ConcurrentWaitSteps && Volatile.Read(ref s_threadsDone) < ConcurrentThreads; i++)
        {
            TimerManager.Wait(100);
        }

        Log.WriteString("[Random] Concurrent draws: ");
        Log.WriteNumber(a.Count);
        Log.WriteString(" + ");
        Log.WriteNumber(b.Count);
        Log.WriteString(", reseeds: ");
        Log.WriteNumber(KernelRandom.ReseedCount - reseedsBefore);
        Log.WriteString("\n");

        Assert.Equal(ConcurrentThreads, Volatile.Read(ref s_threadsReady), "Both drawing threads started");
        Assert.Equal(ConcurrentThreads, Volatile.Read(ref s_threadsDone), "Both drawing threads finished");
        Assert.True(a.Count >= ConcurrentMinRequestsPerThread && b.Count >= ConcurrentMinRequestsPerThread, "Both threads made their draws");
        Assert.True(a.FirstSequence < b.LastSequence && b.FirstSequence < a.LastSequence, "The threads drew interleaved, not one after the other");
        Assert.True(KernelRandom.ReseedCount > reseedsBefore, "The generator reseeded while both threads drew");

        int recordedA = Math.Min(a.Count, ConcurrentRecordedDraws);
        int recordedB = Math.Min(b.Count, ConcurrentRecordedDraws);
        ulong[] prefixes = new ulong[recordedA + recordedB];
        Array.Copy(a.Prefixes, 0, prefixes, 0, recordedA);
        Array.Copy(b.Prefixes, 0, prefixes, recordedA, recordedB);
        Array.Sort(prefixes);
        bool repeated = false;
        for (int i = 1; i < prefixes.Length; i++)
        {
            if (prefixes[i] == prefixes[i - 1])
            {
                repeated = true;
            }
        }

        Assert.False(repeated, "No two draws of the two threads are equal");
    }

    /// <summary>
    /// One thread of the concurrency test: signals it is ready, spins at the
    /// gate (Thread.Yield does nothing here; the tick preempts the spin), then
    /// draws until told to stop and at least its minimum number of times.
    /// </summary>
    private static void DrawUntilStopped(DrawRecord record)
    {
        Interlocked.Increment(ref s_threadsReady);
        while (!s_drawGo)
        {
            // wait for the other thread and the main thread to open the window
        }

        byte[] buffer = new byte[32];
        while (!s_drawStop || record.Count < ConcurrentMinRequestsPerThread)
        {
            int sequence = Interlocked.Increment(ref s_drawSequence);
            RandomNumberGenerator.Fill(buffer);
            if (record.FirstSequence < 0)
            {
                record.FirstSequence = sequence;
            }

            record.LastSequence = sequence;
            if (record.Count < ConcurrentRecordedDraws)
            {
                fixed (byte* p = buffer)
                {
                    record.Prefixes[record.Count] = *(ulong*)p;
                }
            }

            record.Count++;
        }

        Interlocked.Increment(ref s_threadsDone);
    }
}
