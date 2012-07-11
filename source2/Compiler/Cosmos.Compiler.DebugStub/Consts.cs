using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Consts : Cosmos.Assembler.Code {

		public Consts(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");
			new Comment("X#: const Signature = $19740807");
			new LiteralAssemblerCode("DebugStub_Const_Signature equ 0x19740807");
			new Comment("X#: const Tracing_Off = 0");
			new LiteralAssemblerCode("DebugStub_Const_Tracing_Off equ 0");
			new Comment("X#: const Tracing_On = 1");
			new LiteralAssemblerCode("DebugStub_Const_Tracing_On equ 1");
			new LiteralAssemblerCode(";  Current status of OS Debug Stub");
			new Comment("X#: const Status_Run = 0");
			new LiteralAssemblerCode("DebugStub_Const_Status_Run equ 0");
			new Comment("X#: const Status_Break = 1");
			new LiteralAssemblerCode("DebugStub_Const_Status_Break equ 1");
			new Comment("X#: const StepTrigger_None = 0");
			new LiteralAssemblerCode("DebugStub_Const_StepTrigger_None equ 0");
			new Comment("X#: const StepTrigger_Into = 1");
			new LiteralAssemblerCode("DebugStub_Const_StepTrigger_Into equ 1");
			new Comment("X#: const StepTrigger_Over = 2");
			new LiteralAssemblerCode("DebugStub_Const_StepTrigger_Over equ 2");
			new Comment("X#: const StepTrigger_Out = 3");
			new LiteralAssemblerCode("DebugStub_Const_StepTrigger_Out equ 3");
			new Comment("X#: const Vs2Ds_Noop = 0");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_Noop equ 0");
			new Comment("X#: const Vs2Ds_TraceOff = 1");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_TraceOff equ 1");
			new Comment("X#: const Vs2Ds_TraceOn = 2");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_TraceOn equ 2");
			new Comment("X#: const Vs2Ds_Break = 3");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_Break equ 3");
			new Comment("X#: const Vs2Ds_Continue = 4");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_Continue equ 4");
			new Comment("X#: const Vs2Ds_BreakOnAddress = 6");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_BreakOnAddress equ 6");
			new Comment("X#: const Vs2Ds_BatchBegin = 7");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_BatchBegin equ 7");
			new Comment("X#: const Vs2Ds_BatchEnd = 8");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_BatchEnd equ 8");
			new Comment("X#: const Vs2Ds_StepInto = 5");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_StepInto equ 5");
			new Comment("X#: const Vs2Ds_StepOver = 11");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_StepOver equ 11");
			new Comment("X#: const Vs2Ds_StepOut = 12");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_StepOut equ 12");
			new Comment("X#: const Vs2Ds_SendMethodContext = 9");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_SendMethodContext equ 9");
			new Comment("X#: const Vs2Ds_SendMemory = 10");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_SendMemory equ 10");
			new Comment("X#: const Vs2Ds_SendRegisters = 13");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_SendRegisters equ 13");
			new Comment("X#: const Vs2Ds_SendFrame = 14");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_SendFrame equ 14");
			new Comment("X#: const Vs2Ds_SendStack = 15");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_SendStack equ 15");
			new Comment("X#: const Vs2Ds_Ping = 17");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_Ping equ 17");
			new Comment("X#: const Vs2Ds_Max = 18");
			new LiteralAssemblerCode("DebugStub_Const_Vs2Ds_Max equ 18");
			new Comment("X#: const Ds2Vs_Noop = 0");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Noop equ 0");
			new Comment("X#: const Ds2Vs_TracePoint = 1");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_TracePoint equ 1");
			new Comment("X#: const Ds2Vs_Message = 2");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Message equ 2");
			new Comment("X#: const Ds2Vs_BreakPoint = 3");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_BreakPoint equ 3");
			new Comment("X#: const Ds2Vs_Error = 4");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Error equ 4");
			new Comment("X#: const Ds2Vs_Pointer = 5");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Pointer equ 5");
			new Comment("X#: const Ds2Vs_Started = 6");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Started equ 6");
			new Comment("X#: const Ds2Vs_MethodContext = 7");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_MethodContext equ 7");
			new Comment("X#: const Ds2Vs_MemoryData = 8");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_MemoryData equ 8");
			new Comment("X#: const Ds2Vs_CmdCompleted = 9");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_CmdCompleted equ 9");
			new Comment("X#: const Ds2Vs_Registers = 10");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Registers equ 10");
			new Comment("X#: const Ds2Vs_Frame = 11");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Frame equ 11");
			new Comment("X#: const Ds2Vs_Stack = 12");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Stack equ 12");
			new Comment("X#: const Ds2Vs_Pong = 13");
			new LiteralAssemblerCode("DebugStub_Const_Ds2Vs_Pong equ 13");
		}
	}
}
