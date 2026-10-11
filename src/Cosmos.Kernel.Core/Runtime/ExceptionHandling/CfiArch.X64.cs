namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// DWARF register numbers for x86-64.
/// </summary>
internal enum DwarfReg : byte
{
    RAX = 0, RDX = 1, RCX = 2, RBX = 3,
    RSI = 4, RDI = 5, RBP = 6, RSP = 7,
    R8 = 8, R9 = 9, R10 = 10, R11 = 11,
    R12 = 12, R13 = 13, R14 = 14, R15 = 15,
    RA = 16,  // Return address (pseudo-register; x86-64 has no architectural RA reg).
    MAX = 17
}

/// <summary>
/// Compile-time knobs the (otherwise arch-neutral) DWARF CFI unwinder needs to drive
/// <see cref="UnwindState"/>: how big the register table is, which slot is the SP, which slot is
/// the return-address column, and what the default rules at function entry are.
/// </summary>
internal static class CfiArch
{
    public const int RegCount = (int)DwarfReg.MAX;               // 17 slots (0..15 + RA pseudo)
    public const byte CfaRegAtEntry = (byte)DwarfReg.RSP;        // CFA = RSP + 8 at function entry
    public const int CfaOffsetAtEntry = 8;
    public const int StackPointerReg = (int)DwarfReg.RSP;        // Regs[7] = unwound caller SP
    public const int FramePointerReg = (int)DwarfReg.RBP;        // Regs[6] = unwound caller frame pointer
    public const int RaColumn = (int)DwarfReg.RA;                // 16 — default return-address register
    public const int DefaultCodeAlignFactor = 1;
    public const RegSaveKind RaInitRule = RegSaveKind.AtCfaOffset;  // return address at CFA - 8 by default
    public const int RaInitOffset = -8;
}
