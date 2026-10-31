namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// DWARF register numbers for RISC-V 64: x0-x31 are 0-31 (f0-f31, 32-63, are
/// not tracked: no managed frame keeps a reference in a floating-point register).
/// </summary>
internal enum DwarfReg : byte
{
    X0 = 0,
    RA = 1,   // x1: return address
    SP = 2,   // x2: stack pointer
    GP = 3, TP = 4, T0 = 5, T1 = 6, T2 = 7,
    FP = 8,   // x8/s0: frame pointer
    S1 = 9,
    A0 = 10, A1 = 11, A2 = 12, A3 = 13, A4 = 14, A5 = 15, A6 = 16, A7 = 17,
    S2 = 18, S3 = 19, S4 = 20, S5 = 21, S6 = 22, S7 = 23, S8 = 24, S9 = 25, S10 = 26, S11 = 27,
    T3 = 28, T4 = 29, T5 = 30, T6 = 31,
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
    public const byte CfaRegAtEntry = (byte)DwarfReg.SP;       // CFA = sp at function entry (LP64D)
    public const int CfaOffsetAtEntry = 0;
    public const int StackPointerReg = (int)DwarfReg.SP;       // Regs[2] = unwound caller SP
    public const int FramePointerReg = (int)DwarfReg.FP;       // Regs[8] = unwound caller frame pointer (s0)
    public const int RaColumn = (int)DwarfReg.RA;              // 1: ra is the RA reg (no pseudo-reg)
    public const int DefaultCodeAlignFactor = 4;               // ILC emits 4-byte instructions; CIEs override it
    public const RegSaveKind RaInitRule = RegSaveKind.SameValue;  // ra is a normal reg; SameValue = the seeded default
    public const int RaInitOffset = 0;
}
