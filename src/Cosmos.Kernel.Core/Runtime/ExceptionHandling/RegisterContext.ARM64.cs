using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// PAL_LIMITED_CONTEXT structure matching ARM64 assembly offsets.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x70)]
internal unsafe struct PAL_LIMITED_CONTEXT
{
    [FieldOffset(0x00)] public nuint SP;    // Stack pointer
    [FieldOffset(0x08)] public nuint IP;    // Instruction pointer (PC/LR)
    [FieldOffset(0x10)] public nuint FP;    // Frame pointer (x29)
    [FieldOffset(0x18)] public nuint LR;    // Link register (x30)
    [FieldOffset(0x20)] public nuint X19;
    [FieldOffset(0x28)] public nuint X20;
    [FieldOffset(0x30)] public nuint X21;
    [FieldOffset(0x38)] public nuint X22;
    [FieldOffset(0x40)] public nuint X23;
    [FieldOffset(0x48)] public nuint X24;
    [FieldOffset(0x50)] public nuint X25;
    [FieldOffset(0x58)] public nuint X26;
    [FieldOffset(0x60)] public nuint X27;
    [FieldOffset(0x68)] public nuint X28;
}

/// <summary>
/// REGDISPLAY structure for ARM64 funclet calls.
/// Uses direct values instead of pointers to avoid stack corruption.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x68)]
internal unsafe struct REGDISPLAY
{
    // Stack pointer for resume
    [FieldOffset(0x00)] public nuint SP;

    // Direct values for callee-saved registers (not pointers)
    [FieldOffset(0x08)] public nuint FP;    // x29
    [FieldOffset(0x10)] public nuint X19;
    [FieldOffset(0x18)] public nuint X20;
    [FieldOffset(0x20)] public nuint X21;
    [FieldOffset(0x28)] public nuint X22;
    [FieldOffset(0x30)] public nuint X23;
    [FieldOffset(0x38)] public nuint X24;
    [FieldOffset(0x40)] public nuint X25;
    [FieldOffset(0x48)] public nuint X26;
    [FieldOffset(0x50)] public nuint X27;
    [FieldOffset(0x58)] public nuint X28;
    [FieldOffset(0x60)] public nuint LR;    // x30
}
