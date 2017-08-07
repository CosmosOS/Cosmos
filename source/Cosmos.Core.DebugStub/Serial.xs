namespace DebugStub
// optionally exclude this serial version

// All information relating to our serial usage should be documented in this comment.
// http://wiki.osdev.org/Serial_ports
//
// We do not use IRQs for debugstub serial. This is becuase DebugStub (DS)
// MUST be:
//  - As simple as possible
//  - Interact as minimal as possible wtih normal Cosmos code because
//    the debugstub must *always* work even if the normal code is fubarred
//
// The serial port that is used for DS should be 'hidden' from Cosmos main
// so that Cosmos main pretends it does not exist.
//
// IRQs would create a clash/mix of code.
// This does make the serial code in DebugStub inefficient, but its well worth
// the benefits received by following these rules.
//
// Baud rate is set to 115200. Likely our code could not exceed this rate
// anyways the way it is written and there are compatibility issues on some
// hardware above this rate.
//
// We assume a minimum level of a 16550A, which should be no problem on any
// common hardware today. VMWare emulates the 16550A
//
// We do not handle flow control for outbound data (DS --> DC).
// The DebugConnector (DC, the code in the Visual Studio side) however is threaded
// and easily should be able to receive data faster than we can send it.
// Most things are transactional with data being sent only when asked for, but
// with tracing we do send a data directly.
//
// Currently there is no inbound flow control either (DC --> DS)
// For now we assume all commands in bound are 16 bytes or less to ensure
// that they fit in the FIFO. Commands in DS must wait for a command ID ACK
// before sending another command.
// See notes in ProcessCommand.
//
// http://www.nondot.org/sabre/os/files/Communication/ser_port.txt

! %ifndef Exclude_IOPort_Based_SerialInit

// Todo Auto params
// Todo ebp frame ptr auto etc
function InitSerial {
	// Disable interrupts
  DX = 1
	AL = 0
  WriteRegister()

	// Enable DLAB (set baud rate divisor)
	DX = 3
	AL = $80
	WriteRegister()

	// 0x01, 0x00 - 115200
	// 0x02, 0x00 - 57600
	// 0x03, 0x00 - 38400
	//
	// Set divisor (lo byte)
	DX = 0
	AL = $01
	WriteRegister()

	// hi byte
	DX = 1
	AL = $00
	WriteRegister()

	// 8N1
	DX = 3
	AL = $03
	WriteRegister()

	// Enable FIFO, clear them
	// Set 14-byte threshold for IRQ.
	// We dont use IRQ, but you cant set it to 0
	// either. IRQ is enabled/disabled separately
  DX = 2
	AL = $C7
	WriteRegister()

	// 0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it
	// 0x02 RTS
	// 0x01 DTR
	// Send 0x03 if no AFE
	DX = 4
	AL = $03
	WriteRegister()
}

// Modifies: AL, DX
function ComReadAL {
	DX = 5
Wait:
    ReadRegister()
    AL ?& $01
    if 0 goto Wait

	DX = 0
  ReadRegister()
}

function ComWrite8 {
	// Input: ESI
	// Output: None
	// Modifies: EAX, EDX
	//
	// Sends byte at [ESI] to com port and does esi + 1
	//
	// This sucks to use the stack, but x86 can only read and write ports from AL and
	// we need to read a port before we can write out the value to another port.
	// The overhead is a lot, but compared to the speed of the serial and the fact
	// that we wait on the serial port anyways, its a wash.
	//
	// This could be changed to use interrupts, but that then complicates
	// the code and causes interaction with other code. DebugStub should be
	// as isolated as possible from any other code.

	// Sucks again to use DX just for this, but x86 only supports
	// 8 bit address for literals on ports
	DX = 5

	// Wait for serial port to be ready
	// Bit 5 (0x20) test for Transmit Holding Register to be empty.
Wait:
    ReadRegister()
	  AL ?& $20
	  if 0 goto Wait

  // Set address of port
	DX = 0
	// Get byte to send
  AL = ESI[0]
	// Send the byte
	WriteRegister()

	ESI++
}

! %endif
