using System;
using Cosmos.Core;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm;

[Plug(Target = typeof(CPU))]
public class CPUImpl
{
    [PlugMethod(Assembler = typeof(CPUUpdateIDTAsm))]
    public static void UpdateIDT(bool aEnableInterruptsImmediately) => throw null;

    [PlugMethod(Assembler = typeof(CPUGetEndOfKernelAsm))]
    public static uint GetEndOfKernel() => throw null;

    [PlugMethod(Assembler = typeof(CPUGetEBPValue))]
    public static uint GetEBPValue() => throw null;

    [PlugMethod(Assembler = typeof(CPUGetStackStart))]
    public static uint GetStackStart() => throw null;

    [PlugMethod(Assembler = typeof(CPUZeroFillAsm))]
    // TODO: implement this using REP STOSB and REPO STOSD
    public static void ZeroFill(uint aStartAddress, uint aLength) => throw null;

    [PlugMethod(Assembler = typeof(CPUInitFloatAsm))]
    public static void InitFloat() => throw null;

    [PlugMethod(Assembler = typeof(CPUInitSSEAsm))]
    public static void InitSSE() => throw null;

    [PlugMethod(Assembler = typeof(CPUHaltAsm))]
    public static void Halt() => throw null;

    [PlugMethod(Assembler = typeof(CPUDisableINTsAsm))]
    public static void DoDisableInterrupts() => throw null;

    [PlugMethod(Assembler = typeof(CPUEnableINTsAsm))]
    public static void DoEnableInterrupts() => throw null;

    [PlugMethod(Assembler = typeof(CPUCanReadCPUIDAsm))]
    public static int CanReadCPUID() => throw new NotImplementedException();

    [PlugMethod(Assembler = typeof(CPUReadCPUIDAsm))]
    public static void ReadCPUID(uint type, ref int eax, ref int ebx, ref int ecx, ref int edx) =>
        throw new NotImplementedException();

    [PlugMethod(Assembler = typeof(CPUReadTimestampCounterAsm))]
    public static ulong ReadTimestampCounter() => throw new NotImplementedException();

    [PlugMethod(Assembler = typeof(CPUReadModelSpecificRegisterAsm))]
    public static int[] ReadFromModelSpecificRegister() => throw new NotImplementedException();
}
