using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// PAL_LIMITED_CONTEXT structure matching x64 assembly offsets.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x50)]
internal unsafe struct PAL_LIMITED_CONTEXT
{
    [FieldOffset(0x00)] public nuint IP;    // Instruction pointer (return address)
    [FieldOffset(0x08)] public nuint Rsp;   // Stack pointer
    [FieldOffset(0x10)] public nuint Rbp;   // Frame pointer
    [FieldOffset(0x18)] public nuint Rax;
    [FieldOffset(0x20)] public nuint Rbx;
    [FieldOffset(0x28)] public nuint Rdx;
    [FieldOffset(0x30)] public nuint R12;
    [FieldOffset(0x38)] public nuint R13;
    [FieldOffset(0x40)] public nuint R14;
    [FieldOffset(0x48)] public nuint R15;
}

/// <summary>
/// REGDISPLAY structure for x64 funclet calls - matches assembly offsets exactly.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x88)]
internal unsafe struct REGDISPLAY
{
    // Storage for register values
    [FieldOffset(0x00)] public nuint Rbx;
    [FieldOffset(0x08)] public nuint Rbp;
    [FieldOffset(0x10)] public nuint Rsi;

    // Pointers to register values
    [FieldOffset(0x18)] public nuint* pRbx;
    [FieldOffset(0x20)] public nuint* pRbp;
    [FieldOffset(0x28)] public nuint* pRsi;
    [FieldOffset(0x30)] public nuint* pRdi;

    // More storage
    [FieldOffset(0x38)] public nuint Rdi;
    [FieldOffset(0x40)] public nuint R12;
    [FieldOffset(0x48)] public nuint R13;
    [FieldOffset(0x50)] public nuint R14;

    // More pointers
    [FieldOffset(0x58)] public nuint* pR12;
    [FieldOffset(0x60)] public nuint* pR13;
    [FieldOffset(0x68)] public nuint* pR14;
    [FieldOffset(0x70)] public nuint* pR15;

    // Stack pointer for resume
    [FieldOffset(0x78)] public nuint SP;

    // R15 storage
    [FieldOffset(0x80)] public nuint R15;
}
