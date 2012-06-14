Group DebugStub

# Input: AL
# Output: None
# Modifies: EDX, ESI
procedure WriteALToComPort {
	+EAX
    ESI = ESP
    Call WriteByteToComPort
    # Is a local var, cant use Return(4). X# issues the return.
    # This also allows the function to preserve EAX.
    -EAX
}

# SERIAL DOCS
#
# All information relating to our serial usage should be documented in this comment.
#
# We do not use IRQs for debugstub serial. This is becuase DebugStub (DS)
# MUST be:
#  - As simple as possible
#  - Interact as minimal as possible wtih normal Cosmos code because
#    the debugstub must *always* work even if the normal code is fubarred
#
# The serial port that is used for DS should be 'hidden' from Cosmos main
# so that Cosmos main pretends it does not exist.
#
# IRQs would create a clash/mix of code.
# This does make the serial code in DebugStub inefficient, but its well worth
# the benefits received by following these rules.
#
# Baud rate is set to 115200. Likely our code could not exceed this rate
# anyways the way it is written and there are compatibility issues on some
# hardware above this rate.
#
# We assume a minimum level of a 16550A, which should be no problem on any 
# common hardware today. VMWare emulates the 16550A
#
# We do not handle flow control for outbound data (DS --> DC).
# The DebugConnector (DC, the code in the Visual Studio side) however is threaded
# and easily should be able to receive data faster than we can send it.
# Most things are transactional with data being sent only when asked for, but 
# with tracing we do send a data directly.
#
# Currently there is no inbound flow control either (DC --> DS)
# For now we assume all commands in bound are 16 bytes or less to ensure
# that they fit in the FIFO. Commands in DS must wait for a command ID ACK
# before sending another command.
# See notes in ProcessCommand.
#
# http:#www.nondot.org/sabre/os/files/Communication/ser_port.txt

# Todo Auto params
# Todo ebp frame ptr auto etc
# Input - Address on stack as x32
procedure InitSerial2 {
	# Disable interrupts
	DX = ESP[4]
	DX + 1
	AL = 0
	Port[DX] = AL

	# Enable DLAB (set baud rate divisor)
	DX = ESP[4]
	DX + 3
	AL = $80
	Port[DX] = AL

	# 0x01 - 0x00 - 115200
	# 0x02 - 0x00 - 57600
	# 0x03 - 0x00 - 38400
	#
	# Set divisor (lo byte)
	DX = ESP[4]
	AL = $01
	Port[DX] = AL
	# hi byte
	DX = ESP[4]
	DX + 1
	AL = $00
	Port[DX] = AL

	# 8N1
	DX = ESP[4]
	DX + 3
	AL = $03
	Port[DX] = AL

	# Enable FIFO, clear them
	# Set 14-byte threshold for IRQ.
	# We dont use IRQ, but you cant set it to 0
	# either. IRQ is enabled/diabled separately
	DX = ESP[4]
	DX + 2
	AL = $C7
	Port[DX] = AL

	# 0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it
	# 0x02 RTS
	# 0x01 DTR
	# Send 0x03 if no AFE
	DX = ESP[4]
	DX + 4
	AL = $03
	Port[DX] = AL
}
