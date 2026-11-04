// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.RISCV64.Cpu;

/// <summary>
/// The RISC-V Platform-Level Interrupt Controller (PLIC, RISC-V PLIC
/// specification 1.0): routes the platform's interrupt sources to the
/// supervisor external interrupt of a hart context. Each source has a
/// priority, each context an enable bit per source and a threshold; a context
/// claims the highest-priority pending source and completes it when served.
/// </summary>
internal static class Plic
{
    /// <summary>Physical base of the PLIC on the QEMU virt machine.</summary>
    public const ulong QemuVirtBase = 0x0c000000;

    /// <summary>
    /// The context of hart 0's S-mode on the QEMU virt machine: contexts come
    /// in M-mode/S-mode pairs per hart, hart 0's M-mode is context 0.
    /// </summary>
    public const uint QemuVirtSupervisorContext = 1;

    /// <summary>Interrupt sources on the QEMU virt machine (riscv,ndev = 95), plus source 0, which does not exist.</summary>
    public const uint QemuVirtSourceCount = 96;

    /// <summary>Most sources the PLIC architecture allows (1..1023).</summary>
    public const uint MaxSourceCount = 1024;

    /// <summary>Offset of the source priority registers (one 32-bit word per source).</summary>
    private const ulong PriorityOffset = 0x000000;
    /// <summary>Offset of the enable bit arrays (0x80 bytes per context).</summary>
    private const ulong EnableOffset = 0x002000;
    /// <summary>Bytes of enable bits per context.</summary>
    private const ulong EnableStride = 0x80;
    /// <summary>Offset of the per-context threshold and claim/complete registers (0x1000 bytes per context).</summary>
    private const ulong ContextOffset = 0x200000;
    /// <summary>Bytes of registers per context.</summary>
    private const ulong ContextStride = 0x1000;
    /// <summary>Offset of the claim/complete register in a context's block.</summary>
    private const ulong ClaimOffset = 4;

    /// <summary>Priority given to a routed source: any non-zero priority beats the threshold of 0.</summary>
    public const uint DefaultPriority = 1;

    private static ulong s_base = QemuVirtBase;
    private static uint s_context = QemuVirtSupervisorContext;
    private static uint s_sourceCount = QemuVirtSourceCount;
    private static Scheduler.SpinLock s_enableLock;

    /// <summary>Physical base address of the PLIC registers.</summary>
    public static ulong Base => s_base;

    /// <summary>Number of interrupt sources, source 0 included.</summary>
    public static uint SourceCount => s_sourceCount;

    /// <summary>
    /// Sets where the PLIC is and which context the kernel serves, from the
    /// platform description. Call before <see cref="Initialize"/>.
    /// </summary>
    /// <param name="baseAddress">Physical base of the PLIC.</param>
    /// <param name="context">The context of the boot hart's S-mode.</param>
    /// <param name="sourceCount">Number of sources, source 0 included.</param>
    public static void Configure(ulong baseAddress, uint context, uint sourceCount)
    {
        s_base = baseAddress;
        s_context = context;
        s_sourceCount = sourceCount > MaxSourceCount ? MaxSourceCount : sourceCount;
    }

    /// <summary>
    /// Masks every source for the kernel's context, gives each priority 0
    /// and opens the context's threshold, so a source interrupts once it is
    /// routed (<see cref="SetPriority"/>, <see cref="Enable"/>).
    /// </summary>
    public static void Initialize()
    {
        for (uint word = 0; word < (s_sourceCount + 31) / 32; word++)
        {
            Native.MMIO.Write32(EnableWordAddress(word), 0);
        }

        for (uint source = 1; source < s_sourceCount; source++)
        {
            Native.MMIO.Write32(s_base + PriorityOffset + (source * 4UL), 0);
        }

        Native.MMIO.Write32(ContextBase() + 0, 0);

        // Complete anything a previous owner (the firmware) left claimed.
        for (uint i = 0; i < s_sourceCount; i++)
        {
            uint claimed = Claim();
            if (claimed == 0)
            {
                break;
            }

            Complete(claimed);
        }
    }

    /// <summary>Sets the priority of <paramref name="source"/>; 0 never interrupts.</summary>
    /// <param name="source">The source.</param>
    /// <param name="priority">The priority.</param>
    public static void SetPriority(uint source, uint priority)
    {
        if (source == 0 || source >= s_sourceCount)
        {
            return;
        }

        Native.MMIO.Write32(s_base + PriorityOffset + (source * 4UL), priority);
    }

    /// <summary>Lets <paramref name="source"/> interrupt the kernel's context.</summary>
    /// <param name="source">The source.</param>
    public static void Enable(uint source) => UpdateEnable(source, true);

    /// <summary>Stops <paramref name="source"/> from interrupting the kernel's context.</summary>
    /// <param name="source">The source.</param>
    public static void Disable(uint source) => UpdateEnable(source, false);

    /// <summary>Claims the highest-priority pending source: 0 when none is pending.</summary>
    public static uint Claim() => Native.MMIO.Read32(ContextBase() + ClaimOffset);

    /// <summary>Completes a claimed source, so it can interrupt again.</summary>
    /// <param name="source">The source <see cref="Claim"/> returned.</param>
    public static void Complete(uint source) => Native.MMIO.Write32(ContextBase() + ClaimOffset, source);

    private static void UpdateEnable(uint source, bool enable)
    {
        if (source == 0 || source >= s_sourceCount)
        {
            return;
        }

        ulong address = EnableWordAddress(source / 32);
        uint bit = 1u << (int)(source % 32);
        s_enableLock.Acquire();
        try
        {
            uint value = Native.MMIO.Read32(address);
            value = enable ? value | bit : value & ~bit;
            Native.MMIO.Write32(address, value);
        }
        finally
        {
            s_enableLock.Release();
        }
    }

    private static ulong EnableWordAddress(uint word) => s_base + EnableOffset + (s_context * EnableStride) + (word * 4UL);

    private static ulong ContextBase() => s_base + ContextOffset + (s_context * ContextStride);
}
