using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;
using XSharp.Compiler;
using Assembler = Cosmos.Assembler.Assembler;
using CPUAll = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.Core.Plugs {
	[Plug(Target = typeof(Core.CPU))]
	public unsafe class CPUImpl {
		[PlugMethod(Assembler = typeof(UpdateIDT))]
	    public static void UpdateIDT(CPU aThis, bool aEnableInterruptsImmediately) {
		}

		public class GetAmountOfRAMAsm : AssemblerMethod {
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
				XS.Set(XSRegisters.EAX, "MultiBootInfo_Memory_High", sourceIsIndirect: true);
				XS.Xor(XSRegisters.EDX, XSRegisters.EDX);
				XS.Set(XSRegisters.ECX, 1024);
				XS.Divide(XSRegisters.ECX);
				XS.Add(XSRegisters.EAX, 1);
				XS.Push(XSRegisters.EAX);
			}
		}

		[PlugMethod(Assembler = typeof(GetAmountOfRAMAsm))]
		public static uint GetAmountOfRAM() { return 0; }

		public class GetEndOfKernelAsm : AssemblerMethod {
			public override void AssembleNew(CPUAll.Assembler aAssembler, object aMethodInfo) {
				XS.Push("_end_code");
			}
		}

		[PlugMethod(Assembler = typeof(GetEndOfKernelAsm))]
		public static uint GetEndOfKernel() {
		  return 0;
		}

		public class ZeroFillAsm : AssemblerMethod {
			public override void AssembleNew(CPUAll.Assembler aAssembler, object aMethodInfo) {
				XS.ClearDirectionFlag();
				XS.Set(XSRegisters.EDI, XSRegisters.EBP, sourceDisplacement: 0xC); //address
				XS.Set(XSRegisters.ECX, XSRegisters.EBP, sourceDisplacement: 0x8); //length
				// set EAX to value of fill (zero)
				XS.Xor(XSRegisters.EAX, XSRegisters.EAX);
				XS.ShiftRight(XSRegisters.ECX, 1);
				XS.Jump(CPUx86.ConditionalTestEnum.NotBelow, ".step2");
				XS.StoreByteInString();
				XS.Label(".step2");
				XS.ShiftRight(XSRegisters.ECX, 1);
				XS.Jump(CPUx86.ConditionalTestEnum.NotBelow, ".step3");
				XS.StoreWordInString();
				XS.Label(".step3");
				new CPUx86.Stos { Size = 32, Prefixes = CPUx86.InstructionPrefixes.Repeat };
			}
		}
		[PlugMethod(Assembler = typeof(ZeroFillAsm))]
		// TODO: implement this using REP STOSB and REPO STOSD
		public static void ZeroFill(uint aStartAddress, uint aLength) { }

		public class InitFloatAsm : AssemblerMethod {
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
				XS.FPU.FloatInit();
			}
		}

	  [PlugMethod(Assembler = typeof(InitFloatAsm))]
		public static void InitFloat(CPU aThis) { }

		public class HaltAsm : AssemblerMethod {
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
				XS.Halt();
			}
		}

	  [PlugMethod(Assembler = typeof(HaltAsm))]
		public static void Halt(CPU aThis) { }


	//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetEndOfStack))]
	//    public static uint GetEndOfStack() {
	//        return 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetCurrentESP))]
	//    public static uint GetCurrentESP() {
	//        return 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.DoTest))]
	//    public static void DoTest() {
	//    }


	//    [PlugMethod(Assembler = typeof(P2.Assemblers.CPUIDSupport))]
	//    public static uint HasCPUIDSupport() {
	//        return 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.GetCPUIDInternal))]
	//    public static void GetCPUId(out uint d, out uint c, out uint b, out uint a, uint v) {
	//        d = 0;
	//        c = 0;
	//        b = 0;
	//        a = 0;
	//    }

	//    [PlugMethod(Assembler = typeof(P2.Assemblers.Halt))]
	//    public static void Halt() { }

		[PlugMethod(Assembler = typeof(DisableINTsAsm))]
		public static void DoDisableInterrupts()
		{
		}

		[PlugMethod(Assembler = typeof(EnableINTsAsm))]
		public static void DoEnableInterrupts() {
		}

		public class DisableINTsAsm : AssemblerMethod
		{
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
			{
				XS.ClearInterruptFlag();
			}
		}

		public class EnableINTsAsm : AssemblerMethod
		{
			public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo)
			{
				XS.EnableInterrupts();
			}
		}

        //    [PlugMethod(Assembler = typeof(P2.Assemblers.Interrupt30))]
        //    public static void Interrupt30(ref uint aEAX, ref uint aEBX, ref uint aECX, ref uint aEDX) {
        //        aEAX = 0;
        //    }

        //    [PlugMethod(Assembler = typeof(P2.Assemblers.GetMBIAddress))]
        //    public static uint GetMBIAddress() {
        //        return 0;
        //    }

        #region CPU Cycle + Frequency count
            
        private static int* __cyclesrdtscptr; // I declare this as an extra field due to reflection -- don't like it, but can't change it :/
        private static int* __raterdmsrptr; // I declare this as an extra field due to reflection -- don't like it, but can't change it :/
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

        #endregion
    }
}
