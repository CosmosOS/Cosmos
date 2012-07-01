using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Serial : Cosmos.Assembler.Code {
		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("Input: AL");

			new Comment("Output: None");

			new Comment("Modifies: EDX, ESI");

			new Comment("X#: procedure WriteALToComPort {");
			new Label("DebugStub_WriteALToComPort");

			new Comment("X#: +EAX");
			new Push { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: ESI = ESP");
			new Mov{ DestinationReg = RegistersEnum.ESI, SourceReg = RegistersEnum.ESP };

			new Comment("X#: Call .WriteByteToComPort");
			new Call { DestinationLabel = "DebugStub_WriteByteToComPort" };

			new Comment("Is a local var, cant use Return(4). X# issues the return.");

			new Comment("This also allows the function to preserve EAX.");

			new Comment("X#: -EAX");
			new Pop { DestinationReg = RegistersEnum.EAX};

			new Comment("X#: }");
			new Label("DebugStub_WriteALToComPort_Exit");
			new Return();

			new Comment("SERIAL DOCS");

			new Comment("");

			new Comment("All information relating to our serial usage should be documented in this comment.");

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

			new Comment("Input - Address on stack as x32");

			new Comment("X#: procedure InitSerial2 {");
			new Label("DebugStub_InitSerial2");

			new Comment("Disable interrupts");

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

			new Comment("X#: DX + 1");
			new INC { DestinationReg = RegistersEnum.DX };

			new Comment("X#: AL = 0");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("Enable DLAB (set baud rate divisor)");

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

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

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

			new Comment("X#: AL = $01");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x01 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("hi byte");

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

			new Comment("X#: DX + 1");
			new INC { DestinationReg = RegistersEnum.DX };

			new Comment("X#: AL = $00");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x00 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("8N1");

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

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

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

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

			new Comment("X#: DX = ESP[4]");
			new Mov{ DestinationReg = RegistersEnum.DX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };

			new Comment("X#: DX + 4");
			new Add { DestinationReg = RegistersEnum.DX, SourceValue = 4 };

			new Comment("X#: AL = $03");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x03 };

			new Comment("X#: Port[DX] = AL");
			new Out { DestinationReg = RegistersEnum.AL};

			new Comment("X#: }");
			new Label("DebugStub_InitSerial2_Exit");
			new Return();

		}
	}
}
