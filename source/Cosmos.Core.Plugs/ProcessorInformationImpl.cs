using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;

using System;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(global::Cosmos.Core.ProcessorInformation))]
    public unsafe class ProcessorInformationImpl
    {
        private static int* __cyclesrdtscptr; // I declare this as an extra field due to reflection -- don't like it, but can't change it :/
        private static int* __raterdmsrptr; // I declare this as an extra field due to reflection -- don't like it, but can't change it :/
        private static int* __vendortargetptr; // I declare this as an extra field due to reflection -- don't like it, but can't change it :/
        private static long __ticktate = -1;


        public static long GetCycleCount()
        {
            int[] val = new int[2];

            fixed (int* ptr = val)
                __cyclesrdtsc(ptr);

            return ((long)val[0] << 32) | (uint)val[1];
        }

        public static long GetCycleRate()
        {
            if (__ticktate == -1)
            {
                int[] raw = new int[4];

                fixed (int* ptr = raw)
                    __raterdmsr(ptr);

                ulong l1 = ((ulong)raw[0] << 32) | (uint)raw[1];
                ulong l2 = ((ulong)raw[2] << 32) | (uint)raw[3];

                __ticktate = (long)((double)l2 / (double)l1); // * cpu_rate
            }

            return __ticktate;
        }

        [Inline]
        private static void __cyclesrdtsc(int* target)
        {
            /*
             * push eax
             * push ecx
             * push edx
             * lea esi, target
             * rdtsc
             * mov [esi+4], eax
             * mov [esi], edx
             * pop edx
             * pop ecx
             * pop eax
             * ret
             */
            __cyclesrdtscptr = target;

            string intname = LabelName.GetFullName(typeof(CPUImpl).GetField(nameof(__cyclesrdtscptr)));

            ElementReference targ = ElementReference.New(intname);
            new Push
            {
                DestinationReg = RegistersEnum.EAX
            };
            new Push
            {
                DestinationReg = RegistersEnum.ECX
            };
            new Push
            {
                DestinationReg = RegistersEnum.EDX
            };
            new Lea
            {
                DestinationReg = RegistersEnum.ESI,
                SourceRef = targ,
            };
            new Rdtsc();
            new Mov
            {
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 4,
                SourceReg = RegistersEnum.EAX,
            };
            new Mov
            {
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                SourceReg = RegistersEnum.EDX,
            };
            new Pop
            {
                DestinationReg = RegistersEnum.EDX
            };
            new Pop
            {
                DestinationReg = RegistersEnum.ECX
            };
            new Pop
            {
                DestinationReg = RegistersEnum.EAX
            };
            new Return();
        }

        [Inline]
        private static void __raterdmsr(int* target)
        {
            /*
             * ; esi register layout: (mperf_hi, mperf_lo, aperf_hi, aperf_lo)
             * ;
             * ; int* ptr = new int[4];
             * ;
             * lea esi,        ptr  ;equivalent with `mov esi, &ptr`
             * mov ecx,        e7h
             * rdmsr
             * mov [esi + 4],  eax
             * mov [esi],      edx
             * mov ecx,        e8h
             * rdmsr
             * mov [esi + 12], eax
             * mov [esi + 8],  edx
             * xor eax,        eax  ;reset to zero
             * ret
             */
            __raterdmsrptr = target;

            string intname = LabelName.GetFullName(typeof(CPUImpl).GetField(nameof(__raterdmsrptr)));

            ElementReference targ = ElementReference.New(intname);
            new Lea
            {
                DestinationReg = RegistersEnum.ESI,
                SourceRef = targ,
            };
            new Mov
            {
                DestinationReg = RegistersEnum.ECX,
                SourceValue = 0xe7,
            };
            new Rdmsr();
            new Mov
            {
                SourceReg = RegistersEnum.EAX,
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 4,
            };
            new Mov
            {
                SourceReg = RegistersEnum.EDX,
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 0,
            };
            new Mov
            {
                DestinationReg = RegistersEnum.ECX,
                SourceValue = 0xe8,
            };
            new Rdmsr();
            new Mov
            {
                SourceReg = RegistersEnum.EAX,
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 12,
            };
            new Mov
            {
                SourceReg = RegistersEnum.EDX,
                DestinationReg = RegistersEnum.ESI,
                DestinationIsIndirect = true,
                DestinationDisplacement = 8,
            };
            new Xor
            {
                SourceReg = RegistersEnum.EAX,
                DestinationReg = RegistersEnum.EAX,
            };
            new Return();
        }
        
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
