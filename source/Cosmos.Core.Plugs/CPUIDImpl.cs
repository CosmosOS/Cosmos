using System;

using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(CPUID))]
    public static unsafe class CPUIDImpl
    {
        private static int* __vendortargetptr; // I declare this as an extra field due to reflection -- don't like it, but can't change it :/

        [Inline]
        internal static void fetchcpuvendor(int* target)
        {
            /*
             * lea esi, target
             * xor eax, eax
             * cpuid
             * mov [esi], ebx
             * mov [esi + 4], edx
             * mov [esi + 8], ecx
             * ret
             */
            __vendortargetptr = target;

            string intname = LabelName.GetFullName(typeof(CPUImpl).GetField(nameof(__vendortargetptr)));

            ElementReference targ = ElementReference.New(intname);
            new Lea
            {
                DestinationReg = RegistersEnum.ESI,
                SourceRef = targ,
            };
            new CpuId();
            new Mov
            {
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                SourceReg = RegistersEnum.EBX,
            };
            new Mov
            {
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 4,
                SourceReg = RegistersEnum.EDX,
            };
            new Mov
            {
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 8,
                SourceReg = RegistersEnum.ECX,
            };
            new Return();
        }

        [Inline]
        internal static int canreadcpuid()
        {
            /*
             * pushfd
             * pushfd
             * xor dword [esp], 00200000h
             * popfd
             * pushfd
             * pop eax
             * xor eax, [esp]
             * and eax, 00200000h
             * ret
             */
            new Pushfd();
            new Pushfd();
            new Xor
            {
                DestinationReg = RegistersEnum.ESP,
                DestinationIsIndirect = true,
                SourceValue = 0x00200000
            };
            new Popfd();
            new Pushfd();
            new Pop
            {
                DestinationReg = RegistersEnum.EAX,
            };
            new Xor
            {
                DestinationReg = RegistersEnum.EAX,
                SourceReg = RegistersEnum.ESP,
                SourceIsIndirect = true
            };
            new Popfd();
            new And
            {
                DestinationReg = RegistersEnum.EAX,
                SourceValue = 0x00200000
            };
            new Return();
            return 0;
        }
    }
}

