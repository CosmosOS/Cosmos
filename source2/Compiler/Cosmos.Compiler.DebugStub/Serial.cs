using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Serial : Cosmos.Assembler.Code {

		public Serial(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new LiteralAssemblerCode(";  mComPortAddresses = 0x3F8, 0x2F8, 0x3E8, 0x2E8;");

			new LiteralAssemblerCode(";  Currently hardcoded to COM1.");

			new Comment("X#: var ComAddr = $03F8");
			mAssembler.DataMembers.Add(new DataMember("DebugStub_ComAddr", 0x03F8));

			new LiteralAssemblerCode(";  All information relating to our serial usage should be documented in this comment.");

			new LiteralAssemblerCode(";  http://wiki.osdev.org/Serial_ports");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  We do not use IRQs for debugstub serial. This is becuase DebugStub (DS)");

			new LiteralAssemblerCode(";  MUST be:");

			new LiteralAssemblerCode(";   - As simple as possible");

			new LiteralAssemblerCode(";   - Interact as minimal as possible wtih normal Cosmos code because");

			new LiteralAssemblerCode(";     the debugstub must *always* work even if the normal code is fubarred");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  The serial port that is used for DS should be 'hidden' from Cosmos main");

			new LiteralAssemblerCode(";  so that Cosmos main pretends it does not exist.");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  IRQs would create a clash/mix of code.");

			new LiteralAssemblerCode(";  This does make the serial code in DebugStub inefficient, but its well worth");

			new LiteralAssemblerCode(";  the benefits received by following these rules.");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  Baud rate is set to 115200. Likely our code could not exceed this rate");

			new LiteralAssemblerCode(";  anyways the way it is written and there are compatibility issues on some");

			new LiteralAssemblerCode(";  hardware above this rate.");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  We assume a minimum level of a 16550A, which should be no problem on any");

			new LiteralAssemblerCode(";  common hardware today. VMWare emulates the 16550A");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  We do not handle flow control for outbound data (DS --> DC).");

			new LiteralAssemblerCode(";  The DebugConnector (DC, the code in the Visual Studio side) however is threaded");

			new LiteralAssemblerCode(";  and easily should be able to receive data faster than we can send it.");

			new LiteralAssemblerCode(";  Most things are transactional with data being sent only when asked for, but");

			new LiteralAssemblerCode(";  with tracing we do send a data directly.");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  Currently there is no inbound flow control either (DC --> DS)");

			new LiteralAssemblerCode(";  For now we assume all commands in bound are 16 bytes or less to ensure");

			new LiteralAssemblerCode(";  that they fit in the FIFO. Commands in DS must wait for a command ID ACK");

			new LiteralAssemblerCode(";  before sending another command.");

			new LiteralAssemblerCode(";  See notes in ProcessCommand.");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  http://www.nondot.org/sabre/os/files/Communication/ser_port.txt");

			new LiteralAssemblerCode(";  Todo Auto params");

			new LiteralAssemblerCode(";  Todo ebp frame ptr auto etc");

			new Comment("X#: procedure InitSerial {");
			new LiteralAssemblerCode("DebugStub_InitSerial:");

			new Comment("X#: DX = .ComAddr");
			new LiteralAssemblerCode("Mov DX, [DebugStub_ComAddr]");

			new LiteralAssemblerCode(";  Disable interrupts");

			new Comment("X#: BX = DX");
			new LiteralAssemblerCode("Mov BX, DX");

			new Comment("X#: DX + 1");
			new LiteralAssemblerCode("Add DX, 1");

			new Comment("X#: AL = 0");
			new LiteralAssemblerCode("Mov AL, 0");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new LiteralAssemblerCode(";  Enable DLAB (set baud rate divisor)");

			new Comment("X#: DX = BX");
			new LiteralAssemblerCode("Mov DX, BX");

			new Comment("X#: DX + 3");
			new LiteralAssemblerCode("Add DX, 3");

			new Comment("X#: AL = $80");
			new LiteralAssemblerCode("Mov AL, 0x80");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new LiteralAssemblerCode(";  0x01, 0x00 - 115200");

			new LiteralAssemblerCode(";  0x02, 0x00 - 57600");

			new LiteralAssemblerCode(";  0x03, 0x00 - 38400");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  Set divisor (lo byte)");

			new Comment("X#: DX = BX");
			new LiteralAssemblerCode("Mov DX, BX");

			new Comment("X#: AL = $01");
			new LiteralAssemblerCode("Mov AL, 0x01");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new LiteralAssemblerCode(";  hi byte");

			new Comment("X#: DX = BX");
			new LiteralAssemblerCode("Mov DX, BX");

			new Comment("X#: DX + 1");
			new LiteralAssemblerCode("Add DX, 1");

			new Comment("X#: AL = $00");
			new LiteralAssemblerCode("Mov AL, 0x00");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new LiteralAssemblerCode(";  8N1");

			new Comment("X#: DX = BX");
			new LiteralAssemblerCode("Mov DX, BX");

			new Comment("X#: DX + 3");
			new LiteralAssemblerCode("Add DX, 3");

			new Comment("X#: AL = $03");
			new LiteralAssemblerCode("Mov AL, 0x03");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new LiteralAssemblerCode(";  Enable FIFO, clear them");

			new LiteralAssemblerCode(";  Set 14-byte threshold for IRQ.");

			new LiteralAssemblerCode(";  We dont use IRQ, but you cant set it to 0");

			new LiteralAssemblerCode(";  either. IRQ is enabled/diabled separately");

			new Comment("X#: DX = BX");
			new LiteralAssemblerCode("Mov DX, BX");

			new Comment("X#: DX + 2");
			new LiteralAssemblerCode("Add DX, 2");

			new Comment("X#: AL = $C7");
			new LiteralAssemblerCode("Mov AL, 0xC7");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new LiteralAssemblerCode(";  0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it");

			new LiteralAssemblerCode(";  0x02 RTS");

			new LiteralAssemblerCode(";  0x01 DTR");

			new LiteralAssemblerCode(";  Send 0x03 if no AFE");

			new Comment("X#: DX = BX");
			new LiteralAssemblerCode("Mov DX, BX");

			new Comment("X#: DX + 4");
			new LiteralAssemblerCode("Add DX, 4");

			new Comment("X#: AL = $03");
			new LiteralAssemblerCode("Mov AL, 0x03");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_InitSerial_Exit:");
			new LiteralAssemblerCode("Ret");

			new LiteralAssemblerCode(";  Modifies: AL, DX");

			new Comment("X#: procedure ComReadAL {");
			new LiteralAssemblerCode("DebugStub_ComReadAL:");

			new Comment("X#: DX = .ComAddr");
			new LiteralAssemblerCode("Mov DX, [DebugStub_ComAddr]");

			new Comment("X#: DX + 5");
			new LiteralAssemblerCode("Add DX, 5");

			new Comment("X#: Wait:");
			new LiteralAssemblerCode("DebugStub_ComReadAL_Wait:");

			new Comment("X#: AL = Port[DX]");
			new LiteralAssemblerCode("In AL, DX");

			new Comment("X#: AL ?& $01");
			new LiteralAssemblerCode("Test AL, 0x01");

			new Comment("X#: if 0 goto Wait");
			new LiteralAssemblerCode("JZ DebugStub_ComReadAL_Wait");

			new Comment("X#: DX = .ComAddr");
			new LiteralAssemblerCode("Mov DX, [DebugStub_ComAddr]");

			new Comment("X#: AL = Port[DX]");
			new LiteralAssemblerCode("In AL, DX");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComReadAL_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComReadEAX {");
			new LiteralAssemblerCode("DebugStub_ComReadEAX:");

			new Comment("X#: repeat 4 times {");

			new Comment("X#: ComReadAL()");

			new Comment("X#: EAX ~> 8");

			new Comment("X#: }");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");
			new LiteralAssemblerCode("ROR EAX, 8");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");
			new LiteralAssemblerCode("ROR EAX, 8");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");
			new LiteralAssemblerCode("ROR EAX, 8");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");
			new LiteralAssemblerCode("ROR EAX, 8");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComReadEAX_Exit:");
			new LiteralAssemblerCode("Ret");

			new LiteralAssemblerCode(";  Input: EDI");

			new LiteralAssemblerCode(";  Output: [EDI]");

			new LiteralAssemblerCode(";  Modified: AL, DX, EDI (+1)");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  Reads a byte into [EDI] and does EDI + 1");

			new Comment("X#: procedure ComRead8  {");
			new LiteralAssemblerCode("DebugStub_ComRead8:");

			new Comment("X#: ComReadAL()");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");

			new Comment("X#: EDI[0] = AL");
			new LiteralAssemblerCode("Mov [EDI + 0], AL");

			new Comment("X#: EDI + 1");
			new LiteralAssemblerCode("Add EDI, 1");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComRead8_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComRead16 {");
			new LiteralAssemblerCode("DebugStub_ComRead16:");

			new Comment("X#: repeat 2 times {");

			new Comment("X#: ComRead8()");

			new Comment("X#: }");
			new LiteralAssemblerCode("Call DebugStub_ComRead8");
			new LiteralAssemblerCode("Call DebugStub_ComRead8");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComRead16_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComRead32 {");
			new LiteralAssemblerCode("DebugStub_ComRead32:");

			new Comment("X#: repeat 4 times {");

			new Comment("X#: ComRead8()");

			new Comment("X#: }");
			new LiteralAssemblerCode("Call DebugStub_ComRead8");
			new LiteralAssemblerCode("Call DebugStub_ComRead8");
			new LiteralAssemblerCode("Call DebugStub_ComRead8");
			new LiteralAssemblerCode("Call DebugStub_ComRead8");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComRead32_Exit:");
			new LiteralAssemblerCode("Ret");

			new LiteralAssemblerCode(";  Input: AL");

			new LiteralAssemblerCode(";  Output: None");

			new LiteralAssemblerCode(";  Modifies: EDX, ESI");

			new Comment("X#: procedure ComWriteAL {");
			new LiteralAssemblerCode("DebugStub_ComWriteAL:");

			new Comment("X#: +EAX");
			new LiteralAssemblerCode("Push EAX");

			new Comment("X#: ESI = ESP");
			new LiteralAssemblerCode("Mov ESI, ESP");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new LiteralAssemblerCode(";  Is a local var, cant use Return(4). X// issues the return.");

			new LiteralAssemblerCode(";  This also allows the function to preserve EAX.");

			new Comment("X#: -EAX");
			new LiteralAssemblerCode("Pop EAX");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWriteAL_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComWriteAX {");
			new LiteralAssemblerCode("DebugStub_ComWriteAX:");

			new LiteralAssemblerCode(";  Input: AX");

			new LiteralAssemblerCode(";  Output: None");

			new LiteralAssemblerCode(";  Modifies: EDX, ESI");

			new Comment("X#: +EAX");
			new LiteralAssemblerCode("Push EAX");

			new Comment("X#: ESI = ESP");
			new LiteralAssemblerCode("Mov ESI, ESP");

			new Comment("X#: ComWrite16()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite16");

			new LiteralAssemblerCode(";  Is a local var, cant use Return(4). X// issues the return.");

			new LiteralAssemblerCode(";  This also allow the function to preserve EAX.");

			new Comment("X#: -EAX");
			new LiteralAssemblerCode("Pop EAX");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWriteAX_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComWriteEAX {");
			new LiteralAssemblerCode("DebugStub_ComWriteEAX:");

			new LiteralAssemblerCode(";  Input: EAX");

			new LiteralAssemblerCode(";  Output: None");

			new LiteralAssemblerCode(";  Modifies: EDX, ESI");

			new Comment("X#: +EAX");
			new LiteralAssemblerCode("Push EAX");

			new Comment("X#: ESI = ESP");
			new LiteralAssemblerCode("Mov ESI, ESP");

			new Comment("X#: ComWrite32()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite32");

			new LiteralAssemblerCode(";  Is a local var, cant use Return(4). X// issues the return.");

			new LiteralAssemblerCode(";  This also allow the function to preserve EAX.");

			new Comment("X#: -EAX");
			new LiteralAssemblerCode("Pop EAX");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWriteEAX_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComWrite8 {");
			new LiteralAssemblerCode("DebugStub_ComWrite8:");

			new LiteralAssemblerCode(";  Input: ESI");

			new LiteralAssemblerCode(";  Output: None");

			new LiteralAssemblerCode(";  Modifies: EAX, EDX");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  Sends byte at [ESI] to com port and does esi + 1");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  This sucks to use the stack, but x86 can only read and write ports from AL and");

			new LiteralAssemblerCode(";  we need to read a port before we can write out the value to another port.");

			new LiteralAssemblerCode(";  The overhead is a lot, but compared to the speed of the serial and the fact");

			new LiteralAssemblerCode(";  that we wait on the serial port anyways, its a wash.");

			new LiteralAssemblerCode("; ");

			new LiteralAssemblerCode(";  This could be changed to use interrupts, but that then complicates");

			new LiteralAssemblerCode(";  the code and causes interaction with other code. DebugStub should be");

			new LiteralAssemblerCode(";  as isolated as possible from any other code.");

			new LiteralAssemblerCode(";  Sucks again to use DX just for this, but x86 only supports");

			new LiteralAssemblerCode(";  8 bit address for literals on ports");

			new Comment("X#: DX = .ComAddr");
			new LiteralAssemblerCode("Mov DX, [DebugStub_ComAddr]");

			new Comment("X#: DX + 5");
			new LiteralAssemblerCode("Add DX, 5");

			new LiteralAssemblerCode(";  Wait for serial port to be ready");

			new LiteralAssemblerCode(";  Bit 5 (0x20) test for Transmit Holding Register to be empty.");

			new Comment("X#: Wait:");
			new LiteralAssemblerCode("DebugStub_ComWrite8_Wait:");

			new Comment("X#: AL = Port[DX]");
			new LiteralAssemblerCode("In AL, DX");

			new Comment("X#: AL ?& $20");
			new LiteralAssemblerCode("Test AL, 0x20");

			new Comment("X#: if 0 goto Wait");
			new LiteralAssemblerCode("JZ DebugStub_ComWrite8_Wait");

			new LiteralAssemblerCode(";  Set address of port");

			new Comment("X#: DX = $03F8");
			new LiteralAssemblerCode("Mov DX, 0x03F8");

			new LiteralAssemblerCode(";  Get byte to send");

			new Comment("X#: AL = ESI[0]");
			new LiteralAssemblerCode("Mov AL, [ESI + 0]");

			new LiteralAssemblerCode(";  Send the byte");

			new Comment("X#: Port[DX] = AL");
			new LiteralAssemblerCode("Out DX, AL");

			new Comment("X#: ESI++");
			new LiteralAssemblerCode("Inc ESI");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWrite8_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComWrite16 {");
			new LiteralAssemblerCode("DebugStub_ComWrite16:");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWrite16_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComWrite32 {");
			new LiteralAssemblerCode("DebugStub_ComWrite32:");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWrite32_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ComWriteX {");
			new LiteralAssemblerCode("DebugStub_ComWriteX:");

			new Comment("X#: More:");
			new LiteralAssemblerCode("DebugStub_ComWriteX_More:");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ECX--");
			new LiteralAssemblerCode("Dec ECX");

			new Comment("X#: if !0 goto More");
			new LiteralAssemblerCode("JNZ DebugStub_ComWriteX_More");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ComWriteX_Exit:");
			new LiteralAssemblerCode("Ret");

		}
	}
}
