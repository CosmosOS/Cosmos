// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

#if ARCH_X64
using System.Runtime.Intrinsics.X86;
#endif
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Security;

/// <summary>A CPU random number instruction.</summary>
internal enum HardwareRandomSource
{
    /// <summary>No instruction: the CPU advertises none, or every advertised one failed its health checks.</summary>
    None,

    /// <summary>x64 RDSEED, the entropy conditioner's output.</summary>
    RdSeed,

    /// <summary>x64 RDRAND, the DRBG output.</summary>
    RdRand,

    /// <summary>ARM64 RNDRRS, the DRBG reseeded before every read.</summary>
    Rndrrs,

    /// <summary>ARM64 RNDR, the DRBG output.</summary>
    Rndr,
}

/// <summary>
/// The CPU's random number instructions: RDSEED and RDRAND on x64, RNDRRS
/// and RNDR (FEAT_RNG) on ARM64. <see cref="KernelRandom"/> mixes their output
/// into its entropy pool and never hands it out directly.
/// </summary>
/// <remarks>
/// <para>The instructions are probed once, through CPUID on x64 and
/// ID_AA64ISAR0_EL1 on ARM64, and never executed when not advertised: they
/// fault there (QEMU's qemu64, cortex-a53 and cortex-a72 have none). Two
/// threads racing through the first probe compute the same answer.</para>
/// <para>Every word is health checked: zero, all ones (the known failure of
/// some AMD RDRAND implementations) and a repeat of any word the same call
/// already returned, which also catches a source cycling through a few
/// values, are rejected. An instruction that returns three rejected words in
/// a row is taken out of use for the rest of the boot, with one line on the
/// serial log. No word is kept once the call returns, and nothing here
/// allocates or throws.</para>
/// </remarks>
internal static unsafe class HardwareRandom
{
    /// <summary>Reads of one instruction before a word counts as unobtainable or the instruction as broken.</summary>
    private const int AttemptsPerWord = 3;

    /// <summary>Non-zero once the probe results below are valid.</summary>
    private static int s_probed;

    /// <summary>The seed grade instruction (RDSEED, RNDRRS) is advertised and has not failed.</summary>
    private static bool s_seedUsable;

    /// <summary>The DRBG instruction (RDRAND, RNDR) is advertised and has not failed.</summary>
    private static bool s_randomUsable;

    /// <summary>
    /// The best instruction in use: the seed grade one when it works, else
    /// the DRBG one, else <see cref="HardwareRandomSource.None"/>.
    /// </summary>
    internal static HardwareRandomSource Source
    {
        get
        {
            Probe();
            if (s_seedUsable)
            {
                return InstructionOf(true);
            }

            if (s_randomUsable)
            {
                return InstructionOf(false);
            }

            return HardwareRandomSource.None;
        }
    }

    /// <summary>The instruction's mnemonic, for logs.</summary>
    internal static string NameOf(HardwareRandomSource source)
    {
        return source switch
        {
            HardwareRandomSource.RdSeed => "RDSEED",
            HardwareRandomSource.RdRand => "RDRAND",
            HardwareRandomSource.Rndrrs => "RNDRRS",
            HardwareRandomSource.Rndr => "RNDR",
            _ => "none",
        };
    }

    /// <summary>
    /// Writes up to <paramref name="count"/> healthy words to
    /// <paramref name="words"/>, each from the seed grade instruction when it
    /// delivers and from the DRBG instruction otherwise.
    /// </summary>
    /// <param name="words">Receives the words.</param>
    /// <param name="count">Words wanted.</param>
    /// <param name="source">
    /// The seed grade instruction when it supplied every word, the DRBG
    /// instruction when it supplied any, <see cref="HardwareRandomSource.None"/>
    /// when no word was written.
    /// </param>
    /// <returns>How many words were written: 0 without a usable instruction.</returns>
    internal static int Read(ulong* words, int count, out HardwareRandomSource source)
    {
        Probe();

        int written = 0;
        bool fellBack = false;
        for (int i = 0; i < count; i++)
        {
            if (s_seedUsable && TryRead(true, words, written))
            {
                written++;
            }
            else if (s_randomUsable && TryRead(false, words, written))
            {
                written++;
                fellBack = true;
            }
        }

        source = written == 0 ? HardwareRandomSource.None : InstructionOf(!fellBack);
        return written;
    }

    /// <summary>
    /// One healthy word from one instruction into <c>words[written]</c>, or
    /// false when it has none to give. A word equal to one of the
    /// <paramref name="written"/> before it is rejected.
    /// </summary>
    private static bool TryRead(bool seedGrade, ulong* words, int written)
    {
        for (int attempt = 0; attempt < AttemptsPerWord; attempt++)
        {
            ulong value = 0;
            if (Execute(seedGrade, &value) == 0)
            {
                // The retry budget in the native routine ran out: the source
                // is drained for now, which is not a fault.
                return false;
            }

            bool healthy = value != 0 && value != ulong.MaxValue;
            for (int i = 0; i < written && healthy; i++)
            {
                healthy = words[i] != value;
            }

            if (healthy)
            {
                words[written] = value;
                return true;
            }
        }

        // Every read succeeded and every word was rejected: the instruction is
        // broken, not drained. Stop using it.
        if (seedGrade)
        {
            s_seedUsable = false;
        }
        else
        {
            s_randomUsable = false;
        }

        Serial.WriteString("[Random] WARNING: ");
        Serial.WriteString(NameOf(InstructionOf(seedGrade)));
        Serial.WriteString(" returned zero, all-ones or repeated words; it is no longer used\n");
        return false;
    }

    /// <summary>The seed grade or the DRBG instruction of this architecture.</summary>
    private static HardwareRandomSource InstructionOf(bool seedGrade)
    {
#if ARCH_ARM64
        return seedGrade ? HardwareRandomSource.Rndrrs : HardwareRandomSource.Rndr;
#else
        return seedGrade ? HardwareRandomSource.RdSeed : HardwareRandomSource.RdRand;
#endif
    }

    private static int Execute(bool seedGrade, ulong* value)
    {
#if ARCH_X64
        return seedGrade ? RandomNative.RdSeed64(value) : RandomNative.RdRand64(value);
#elif ARCH_ARM64
        return seedGrade ? RandomNative.Rndrrs64(value) : RandomNative.Rndr64(value);
#else
        return 0;
#endif
    }

    /// <summary>Reads which instructions the CPU advertises, once.</summary>
    private static void Probe()
    {
        if (Volatile.Read(ref s_probed) != 0)
        {
            return;
        }

#if ARCH_X64
        // RDRAND: leaf 1, ECX bit 30. RDSEED: leaf 7 subleaf 0, EBX bit 18,
        // only when the CPU reports leaf 7 at all.
        (int maxLeaf, _, _, _) = X86Base.CpuId(0, 0);
        if (maxLeaf >= 1)
        {
            (_, _, int features, _) = X86Base.CpuId(1, 0);
            s_randomUsable = (features & (1 << 30)) != 0;
        }

        if (maxLeaf >= 7)
        {
            (_, int extendedFeatures, _, _) = X86Base.CpuId(7, 0);
            s_seedUsable = (extendedFeatures & (1 << 18)) != 0;
        }
#elif ARCH_ARM64
        // FEAT_RNG brings RNDR and RNDRRS together: ID_AA64ISAR0_EL1.RNDR,
        // bits 63:60, is 0b0001 when present.
        bool featRng = (RandomNative.ReadIdAa64Isar0() >> 60) != 0;
        s_seedUsable = featRng;
        s_randomUsable = featRng;
#endif

        Volatile.Write(ref s_probed, 1);
    }
}
