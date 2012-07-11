using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Init : Cosmos.Assembler.Code {

		public Init(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			mAssembler.DataMembers.Add(new DataMember("DebugBPs", new int[256]));
			mAssembler.DataMembers.Add(new DataMember("DebugWaitMsg", "Waiting for debugger connection..."));
			new LiteralAssemblerCode("DebugStub_Init:");
			new LiteralAssemblerCode("Call DebugStub_Cls");
			new LiteralAssemblerCode("Call DebugStub_DisplayWaitMsg");
			new LiteralAssemblerCode("Call DebugStub_InitSerial");
			new LiteralAssemblerCode("Call DebugStub_WaitForDbgHandshake");
			new LiteralAssemblerCode("Call DebugStub_Cls");
			new LiteralAssemblerCode("DebugStub_Init_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_WaitForSignature:");
			new LiteralAssemblerCode("Mov EBX, 0");
			new LiteralAssemblerCode("DebugStub_WaitForSignature_Read:");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");
			new LiteralAssemblerCode("Mov BL, AL");
			new LiteralAssemblerCode("ROR EBX, 8");
			new LiteralAssemblerCode("Cmp EBX, DebugStub_Const_Signature");
			new LiteralAssemblerCode("JNE DebugStub_WaitForSignature_Read");
			new LiteralAssemblerCode("DebugStub_WaitForSignature_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_WaitForDbgHandshake:");
			new LiteralAssemblerCode("Mov AL, 0");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");
			new LiteralAssemblerCode("Push dword DebugStub_Const_Signature");
			new LiteralAssemblerCode("Mov ESI, ESP");
			new LiteralAssemblerCode("Call DebugStub_ComWrite32");
			new LiteralAssemblerCode("Pop EAX");
			new LiteralAssemblerCode("Mov AL, DebugStub_Const_Ds2Vs_Started");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");
			new LiteralAssemblerCode("Call DebugStub_WaitForSignature");
			new LiteralAssemblerCode("Call DebugStub_ProcessCommandBatch");
			new LiteralAssemblerCode("DebugStub_WaitForDbgHandshake_Exit:");
			new LiteralAssemblerCode("Ret");
		}
	}
}
