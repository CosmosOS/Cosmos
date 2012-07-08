using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Serial : Cosmos.Assembler.Code {

		public Serial(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("mComPortAddresses = 0x3F8, 0x2F8, 0x3E8, 0x2E8;");

			new Comment("Currently hardcoded to COM1.");

			new Comment("X#: var .ComAddr = $03F8");
			mAssembler.DataMembers.Add(new DataMember("DebugStub_ComAddr", 0x03F8));

			new Comment("All information relating to our serial usage should be documented in this comment.");

			new Comment("http://wiki.osdev.org/Serial_ports");

			new Comment("");

			new Comment("We do not use IRQs for debugstub serial. This is becuase DebugStub (DS)");

			new Comment("MUST be:");

			new Comment("- As simple as possible");

			new Comment("- Interact as minimal as possible wtih normal Cosmos code because");

			new Comment("the debugstub must *always* work even if the normal code is fubarred");

			new Comment("");

			new Comment("The serial port that is used for DS should be 'hidden' from Cosmos main");

			new Comment("so that Cosmos main pretends it does not exist.");

			new Comment("");

			new Comment("IRQs would create a clash/mix of code.");

			new Comment("This does make the serial code in DebugStub inefficient, but its well worth");

			new Comment("the benefits received by following these rules.");

			new Comment("");

			new Comment("Baud rate is set to 115200. Likely our code could not exceed this rate");

			new Comment("anyways the way it is written and there are compatibility issues on some");

			new Comment("hardware above this rate.");

			new Comment("");

			new Comment("We assume a minimum level of a 16550A, which should be no problem on any");

			new Comment("common hardware today. VMWare emulates the 16550A");

			new Comment("");

			new Comment("We do not handle flow control for outbound data (DS --> DC).");

			new Comment("The DebugConnector (DC, the code in the Visual Studio side) however is threaded");

			new Comment("and easily should be able to receive data faster than we can send it.");

			new Comment("Most things are transactional with data being sent only when asked for, but");

			new Comment("with tracing we do send a data directly.");

			new Comment("");

			new Comment("Currently there is no inbound flow control either (DC --> DS)");

			new Comment("For now we assume all commands in bound are 16 bytes or less to ensure");

			new Comment("that they fit in the FIFO. Commands in DS must wait for a command ID ACK");

			new Comment("before sending another command.");

			new Comment("See notes in ProcessCommand.");

			new Comment("");

			new Comment("http:#www.nondot.org/sabre/os/files/Communication/ser_port.txt");

			new Comment("Todo Auto params");

			new Comment("Todo ebp frame ptr auto etc");

			new Comment("X#: procedure InitSerial {");
			new Label("DebugStub_InitSerial");

			new Comment("X#: DX = .ComAddr");
			new Mov { DestinationReg = RegistersEnum.DX , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_ComAddr"), SourceIsIndirect = true };

			new Comment("Disable interrupts");

			new Comment("X#: BX = DX");
			new Mov{ DestinationReg = RegistersEnum.BX, SourceReg = RegistersEnum.DX };

			new Comment("X#: DX + 1");
			new INC { DestinationReg = RegistersEnum.DX };

			new Comment("X#: AL = 0");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("Enable DLAB (set baud rate divisor)");

			new Comment("X#: DX = BX");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.BX };

			new Comment("X#: DX + 3");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 3 };

			new Comment("X#: AL = $80");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x80 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("0x01, 0x00 - 115200");

			new Comment("0x02, 0x00 - 57600");

			new Comment("0x03, 0x00 - 38400");

			new Comment("");

			new Comment("Set divisor (lo byte)");

			new Comment("X#: DX = BX");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.BX };

			new Comment("X#: AL = $01");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x01 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("hi byte");

			new Comment("X#: DX = BX");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.BX };

			new Comment("X#: DX + 1");
			new INC { DestinationReg = RegistersEnum.DX };

			new Comment("X#: AL = $00");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x00 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("8N1");

			new Comment("X#: DX = BX");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.BX };

			new Comment("X#: DX + 3");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 3 };

			new Comment("X#: AL = $03");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x03 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("Enable FIFO, clear them");

			new Comment("Set 14-byte threshold for IRQ.");

			new Comment("We dont use IRQ, but you cant set it to 0");

			new Comment("either. IRQ is enabled/diabled separately");

			new Comment("X#: DX = BX");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.BX };

			new Comment("X#: DX + 2");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 2 };

			new Comment("X#: AL = $C7");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0xC7 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it");

			new Comment("0x02 RTS");

			new Comment("0x01 DTR");

			new Comment("Send 0x03 if no AFE");

			new Comment("X#: DX = BX");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.BX };

			new Comment("X#: DX + 4");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 4 };

			new Comment("X#: AL = $03");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x03 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("X#: }");
			new Label("DebugStub_InitSerial_Exit");
			new Return();

			new Comment("Modifies: AL, DX");

			new Comment("X#: procedure ComReadAL {");
			new Label("DebugStub_ComReadAL");

			new Comment("X#: DX = .ComAddr");
			new Mov { DestinationReg = RegistersEnum.DX , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_ComAddr"), SourceIsIndirect = true };

			new Comment("X#: DX + 5");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 5 };

			new Comment("Wait for port to be ready");

			new Comment("X#: Wait:");
			new Label("DebugStub_ComReadAL_Wait");

			new Comment("X#: AL = Port[DX]");
			new IN { DestinationReg = RegistersEnum.AL};

			new Comment("X#: AL ?& $01");
			new Test { DestinationReg = RegistersEnum.AL, SourceValue = 0x01 };

			new Comment("X#: if 0 goto Wait");
			new ConditionalJump { Condition = ConditionalTestEnum.Zero, DestinationLabel = "DebugStub_ComReadAL_Wait" };

			new Comment("X#: DX = .ComAddr");
			new Mov { DestinationReg = RegistersEnum.DX , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_ComAddr"), SourceIsIndirect = true };

			new Comment("Read byte");

			new Comment("X#: AL = Port[DX]");
			new IN { DestinationReg = RegistersEnum.AL};

			new Comment("X#: }");
			new Label("DebugStub_ComReadAL_Exit");
			new Return();

			new Comment("Input: EDI");

			new Comment("Output: [EDI]");

			new Comment("Modified: AL, DX, EDI (+1)");

			new Comment("");

			new Comment("Reads a byte into [EDI] and does EDI + 1");

			new Comment("X#: procedure ComRead8  {");
			new Label("DebugStub_ComRead8");

			new Comment("X#: Call .ComReadAL");
			new Call { DestinationLabel = "DebugStub_ComReadAL" };

			new Comment("X#: EDI[0] = AL");
			new Mov{ DestinationReg = RegistersEnum.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0, SourceReg = RegistersEnum.AL };

			new Comment("X#: EDI + 1");
			new INC { DestinationReg = RegistersEnum.EDI };

			new Comment("X#: }");
			new Label("DebugStub_ComRead8_Exit");
			new Return();

			new Comment("X#: procedure ComRead16 {");
			new Label("DebugStub_ComRead16");

			new Comment("X#: Call .ComRead8");
			new Call { DestinationLabel = "DebugStub_ComRead8" };

			new Comment("X#: Call .ComRead8");
			new Call { DestinationLabel = "DebugStub_ComRead8" };

			new Comment("X#: }");
			new Label("DebugStub_ComRead16_Exit");
			new Return();

			new Comment("X#: procedure ComRead32 {");
			new Label("DebugStub_ComRead32");

			new Comment("X#: Call .ComRead8");
			new Call { DestinationLabel = "DebugStub_ComRead8" };

			new Comment("X#: Call .ComRead8");
			new Call { DestinationLabel = "DebugStub_ComRead8" };

			new Comment("X#: Call .ComRead8");
			new Call { DestinationLabel = "DebugStub_ComRead8" };

			new Comment("X#: Call .ComRead8");
			new Call { DestinationLabel = "DebugStub_ComRead8" };

			new Comment("X#: }");
			new Label("DebugStub_ComRead32_Exit");
			new Return();

			new Comment("Input: AL");

			new Comment("Output: None");

			new Comment("Modifies: EDX, ESI");

			new Comment("X#: procedure ComWriteAL {");
			new Label("DebugStub_ComWriteAL");

			new Comment("X#: +EAX");
			new Push { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: ESI = ESP");
			new Mov{ DestinationReg = RegistersEnum.ESI, SourceReg = RegistersEnum.ESP };

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("Is a local var, cant use Return(4). X# issues the return.");

			new Comment("This also allows the function to preserve EAX.");

			new Comment("X#: -EAX");
			new Pop { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: }");
			new Label("DebugStub_ComWriteAL_Exit");
			new Return();

			new Comment("X#: procedure ComWriteAX {");
			new Label("DebugStub_ComWriteAX");

			new Comment("Input: AX");

			new Comment("Output: None");

			new Comment("Modifies: EDX, ESI");

			new Comment("X#: +EAX");
			new Push { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: ESI = ESP");
			new Mov{ DestinationReg = RegistersEnum.ESI, SourceReg = RegistersEnum.ESP };

			new Comment("X#: Call .ComWrite16");
			new Call { DestinationLabel = "DebugStub_ComWrite16" };

			new Comment("Is a local var, cant use Return(4). X# issues the return.");

			new Comment("This also allow the function to preserve EAX.");

			new Comment("X#: -EAX");
			new Pop { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: }");
			new Label("DebugStub_ComWriteAX_Exit");
			new Return();

			new Comment("X#: procedure ComWriteEAX {");
			new Label("DebugStub_ComWriteEAX");

			new Comment("Input: EAX");

			new Comment("Output: None");

			new Comment("Modifies: EDX, ESI");

			new Comment("X#: +EAX");
			new Push { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: ESI = ESP");
			new Mov{ DestinationReg = RegistersEnum.ESI, SourceReg = RegistersEnum.ESP };

			new Comment("X#: Call .ComWrite32");
			new Call { DestinationLabel = "DebugStub_ComWrite32" };

			new Comment("Is a local var, cant use Return(4). X# issues the return.");

			new Comment("This also allow the function to preserve EAX.");

			new Comment("X#: -EAX");
			new Pop { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: }");
			new Label("DebugStub_ComWriteEAX_Exit");
			new Return();

			new Comment("X#: procedure ComWrite8 {");
			new Label("DebugStub_ComWrite8");

			new Comment("Input: ESI");

			new Comment("Output: None");

			new Comment("Modifies: EAX, EDX");

			new Comment("");

			new Comment("Sends byte at [ESI] to com port and does esi + 1");

			new Comment("");

			new Comment("This sucks to use the stack, but x86 can only read and write ports from AL and");

			new Comment("we need to read a port before we can write out the value to another port.");

			new Comment("The overhead is a lot, but compared to the speed of the serial and the fact");

			new Comment("that we wait on the serial port anyways, its a wash.");

			new Comment("");

			new Comment("This could be changed to use interrupts, but that then complicates");

			new Comment("the code and causes interaction with other code. DebugStub should be");

			new Comment("as isolated as possible from any other code.");

			new Comment("Sucks again to use DX just for this, but x86 only supports");

			new Comment("8 bit address for literals on ports");

			new Comment("X#: DX = .ComAddr");
			new Mov { DestinationReg = RegistersEnum.DX , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_ComAddr"), SourceIsIndirect = true };

			new Comment("X#: DX + 5");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 5 };

			new Comment("Wait for serial port to be ready");

			new Comment("Bit 5 (0x20) test for Transmit Holding Register to be empty.");

			new Comment("X#: Wait:");
			new Label("DebugStub_ComWrite8_Wait");

			new Comment("X#: AL = Port[DX]");
			new IN { DestinationReg = RegistersEnum.AL};

			new Comment("X#: AL ?& $20");
			new Test { DestinationReg = RegistersEnum.AL, SourceValue = 0x20 };

			new Comment("X#: if 0 goto Wait");
			new ConditionalJump { Condition = ConditionalTestEnum.Zero, DestinationLabel = "DebugStub_ComWrite8_Wait" };

			new Comment("Set address of port");

			new Comment("X#: DX = $03F8");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceValue = 0x03F8 };

			new Comment("Get byte to send");

			new Comment("X#: AL = ESI[0]");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceReg = RegistersEnum.ESI, SourceIsIndirect = true, SourceDisplacement = 0 };

			new Comment("Send the byte");

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("X#: ESI++");
			new INC { DestinationReg = RegistersEnum.ESI };

			new Comment("X#: }");
			new Label("DebugStub_ComWrite8_Exit");
			new Return();

			new Comment("X#: procedure ComWrite16 {");
			new Label("DebugStub_ComWrite16");

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: }");
			new Label("DebugStub_ComWrite16_Exit");
			new Return();

			new Comment("X#: procedure ComWrite32 {");
			new Label("DebugStub_ComWrite32");

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: }");
			new Label("DebugStub_ComWrite32_Exit");
			new Return();

			new Comment("X#: procedure ComWriteX {");
			new Label("DebugStub_ComWriteX");

			new Comment("X#: More:");
			new Label("DebugStub_ComWriteX_More");

			new Comment("X#: Call .ComWrite8");
			new Call { DestinationLabel = "DebugStub_ComWrite8" };

			new Comment("X#: ECX--");
			new Dec { DestinationReg = RegistersEnum.ECX };

			new Comment("X#: if !0 goto More");
			new ConditionalJump { Condition = ConditionalTestEnum.NotZero, DestinationLabel = "DebugStub_ComWriteX_More" };

			new Comment("X#: }");
			new Label("DebugStub_ComWriteX_Exit");
			new Return();

		}
	}
}
