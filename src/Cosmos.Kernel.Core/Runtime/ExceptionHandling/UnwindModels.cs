using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// How the CFI rule table describes where a register's caller-side value lives.
/// </summary>
internal enum RegSaveKind : byte
{
    Undefined = 0,    // Register value is unknown after the unwind.
    SameValue = 1,    // Register keeps its value (the callee-saved default).
    AtCfaOffset = 2,  // Register was spilled to memory at CFA + Offset.
    InRegister = 3,   // Register's value is currently held in another register.
}

/// <summary>
/// One row of the CFI rule table: how a register is saved at the current PC in a function.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct RegLocation
{
    [FieldOffset(0)] public RegSaveKind Kind;
    [FieldOffset(1)] public byte Register;   // For InRegister: the source DWARF register.
    [FieldOffset(4)] public int Offset;      // For AtCfaOffset: offset (typically negative) from the CFA.
}

/// <summary>
/// Register state during a DWARF CFI stack unwind. Indexed by DWARF register number — the unwound
/// register values live in <c>Regs</c>, the per-register save-location rules in <c>RegLocations</c>.
/// The DWARF interpreter (<see cref="DwarfCfiParser"/>) is single-source across architectures; only
/// the slot count, the CFA seed at function entry, and the return-address column differ — those
/// come from <see cref="CfiArch"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct UnwindState
{
    public byte CfaRegister;   // DWARF register number the CFA is expressed relative to.
    public int CfaOffset;      // CFA = Regs[CfaRegister] + CfaOffset.

    // Per-register save-location rules. `fixed` sizes need a compile-time constant; CfiArch.RegCount is one.
    public fixed byte RegLocations[CfiArch.RegCount * 8];   // RegLocation[CfiArch.RegCount]

    // Unwound register values, indexed by DWARF register number. Stored as ulong because C# `fixed`
    // buffers only accept primitive types — nuint isn't on the list (CS1663). On the kernel's two
    // targets (x64 and ARM64) nuint and ulong are byte-identical, so the casts at the accessor
    // boundaries below are runtime no-ops. Scratch slots the unwind never touches stay zero — fine,
    // the CFI rules wouldn't reference them anyway.
    public fixed ulong Regs[CfiArch.RegCount];

    /// <summary>
    /// IP to unwind from; after a successful unwind, the caller's return address.
    /// Also lands in <c>Regs[CfiArch.RaColumn]</c> via <see cref="DwarfCfiParser.ApplyUnwindRules"/>;
    /// kept here as an explicit field so callers don't need to know the RA column.
    /// </summary>
    public nuint ReturnAddress;

    public RegLocation* GetRegLocation(int reg)
    {
        if ((uint)reg >= CfiArch.RegCount)
        {
            reg = 0;   // paranoia: a malformed FDE reg# must not index past the buffer
        }
        fixed (byte* p = RegLocations)
        {
            return (RegLocation*)(p + reg * sizeof(RegLocation));
        }
    }

    public void SetRegLocation(int reg, RegSaveKind kind, int offset = 0, byte inReg = 0)
    {
        if ((uint)reg >= CfiArch.RegCount)
        {
            return;   // ignore out-of-range reg# from a malformed FDE
        }
        RegLocation* loc = GetRegLocation(reg);
        loc->Kind = kind;
        loc->Offset = offset;
        loc->Register = inReg;
    }

    public nuint GetRegValue(int reg)
    {
        return (uint)reg < CfiArch.RegCount ? (nuint)Regs[reg] : 0;
    }

    public void SetRegValue(int reg, nuint value)
    {
        if ((uint)reg < CfiArch.RegCount)
        {
            Regs[reg] = (ulong)value;
        }
    }

    /// <summary>
    /// Arch-neutral name for the caller-side stack pointer the unwind resolves to
    /// (DWARF reg 7 on x64, reg 31 on ARM64).
    /// </summary>
    public nuint StackPointer
    {
        get => (nuint)Regs[CfiArch.StackPointerReg];
        set => Regs[CfiArch.StackPointerReg] = (ulong)value;
    }
}
