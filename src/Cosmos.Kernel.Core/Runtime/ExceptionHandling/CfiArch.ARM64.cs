namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// DWARF register numbers for AArch64 (ARM64).
/// </summary>
internal enum DwarfReg : byte
{
    X0 = 0, X1 = 1, X2 = 2, X3 = 3, X4 = 4, X5 = 5, X6 = 6, X7 = 7,
    X8 = 8, X9 = 9, X10 = 10, X11 = 11, X12 = 12, X13 = 13, X14 = 14, X15 = 15,
    X16 = 16, X17 = 17, X18 = 18, X19 = 19, X20 = 20, X21 = 21, X22 = 22, X23 = 23,
    X24 = 24, X25 = 25, X26 = 26, X27 = 27, X28 = 28,
    FP = 29,  // X29 — Frame Pointer
    LR = 30,  // X30 — Link Register (return address)
    SP = 31,  // Stack Pointer
    MAX = 32
}

/// <summary>
/// Compile-time knobs the (otherwise arch-neutral) DWARF CFI unwinder needs to drive
/// <see cref="UnwindState"/>: how big the register table is, which slot is the SP, which slot is
/// the return-address column, and what the default rules at function entry are.
/// </summary>
internal static class CfiArch
{
    public const int RegCount = (int)DwarfReg.MAX;             // 32 slots
    public const byte CfaRegAtEntry = (byte)DwarfReg.SP;       // CFA = SP at function entry (AAPCS64)
    public const int CfaOffsetAtEntry = 0;
    public const int StackPointerReg = (int)DwarfReg.SP;       // Regs[31] = unwound caller SP
    public const int FramePointerReg = (int)DwarfReg.FP;       // Regs[29] = unwound caller frame pointer (X29)
    public const int RaColumn = (int)DwarfReg.LR;              // 30 — LR is the RA reg (no pseudo-reg)
    public const int DefaultCodeAlignFactor = 4;               // every ARM64 instruction is 4 bytes
    public const RegSaveKind RaInitRule = RegSaveKind.SameValue;  // LR is a normal reg; SameValue = the seeded default
    public const int RaInitOffset = 0;
}
