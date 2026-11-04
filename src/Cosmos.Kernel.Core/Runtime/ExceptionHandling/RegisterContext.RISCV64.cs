using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// PAL_LIMITED_CONTEXT structure matching the RISC-V 64 assembly offsets
/// (Cosmos.Kernel.Native.RISCV64, CPU/ExceptionHandling.s). SP, IP and FP come
/// first, in the ARM64 order Stdllib reads them in.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x78)]
internal unsafe struct PAL_LIMITED_CONTEXT
{
    [FieldOffset(0x00)] public nuint SP;    // Stack pointer
    [FieldOffset(0x08)] public nuint IP;    // Instruction pointer (the throw site's return address)
    [FieldOffset(0x10)] public nuint FP;    // Frame pointer (s0)
    [FieldOffset(0x18)] public nuint RA;    // Return address register (ra)
    [FieldOffset(0x20)] public nuint S1;
    [FieldOffset(0x28)] public nuint S2;
    [FieldOffset(0x30)] public nuint S3;
    [FieldOffset(0x38)] public nuint S4;
    [FieldOffset(0x40)] public nuint S5;
    [FieldOffset(0x48)] public nuint S6;
    [FieldOffset(0x50)] public nuint S7;
    [FieldOffset(0x58)] public nuint S8;
    [FieldOffset(0x60)] public nuint S9;
    [FieldOffset(0x68)] public nuint S10;
    [FieldOffset(0x70)] public nuint S11;
}

/// <summary>
/// REGDISPLAY structure for RISC-V 64 funclet calls.
/// Uses direct values instead of pointers to avoid stack corruption.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x70)]
internal unsafe struct REGDISPLAY
{
    // Stack pointer for resume
    [FieldOffset(0x00)] public nuint SP;

    // Direct values for callee-saved registers (not pointers)
    [FieldOffset(0x08)] public nuint FP;    // s0
    [FieldOffset(0x10)] public nuint S1;
    [FieldOffset(0x18)] public nuint S2;
    [FieldOffset(0x20)] public nuint S3;
    [FieldOffset(0x28)] public nuint S4;
    [FieldOffset(0x30)] public nuint S5;
    [FieldOffset(0x38)] public nuint S6;
    [FieldOffset(0x40)] public nuint S7;
    [FieldOffset(0x48)] public nuint S8;
    [FieldOffset(0x50)] public nuint S9;
    [FieldOffset(0x58)] public nuint S10;
    [FieldOffset(0x60)] public nuint S11;
    [FieldOffset(0x68)] public nuint RA;    // ra
}
