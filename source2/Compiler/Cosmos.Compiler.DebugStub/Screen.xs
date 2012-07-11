Group DebugStub

const VidBase = $B8000

function Cls {
	// VidBase
    ESI = #VidBase
    
BeginLoop:
	// Text
    ESI[0] = $00
    ESI++

	// Colour
    ESI[0] = $0A
    ESI++
	
	// End of Video Area
	// VidBase + 25 * 80 * 2 = B8FA0
	If ESI < $B8FA0 goto BeginLoop
}

function DisplayWaitMsg {
    // http://wiki.osdev.org/Text_UI
    // Later can cycle for x changes of second register:
    // http://wiki.osdev.org/Time_And_Date
    
	ESI = @..DebugWaitMsg

    EDI = #VidBase
    // 10 lines down, 20 cols in (10 * 80 + 20) * 2)
    EDI + 1640

    // Read and copy string till 0 terminator
	AL = 1
    while AL != 0 {
    //while ESI[0] != 0 {
		AL = ESI[0]
		EDI[0] = AL
		ESI++
		EDI + 2
	}
}
