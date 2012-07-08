Group DebugStub

procedure Cls {
	# VidBase
    ESI = $B8000
    
BeginLoop:
	# Text
    AL = $00
    ESI[0] = AL
    ESI + 1

	# Colour
    AL = $0A
    ESI[0] = AL
    ESI + 1
	
	# End of Video Area
	# VidBase + 25 * 80 * 2 = B8FA0
	If (ESI < $B8FA0) goto BeginLoop
}

procedure DisplayWaitMsg {
    # http://wiki.osdev.org/Text_UI
    # Later can cycle for x changes of second register:
    # http://wiki.osdev.org/Time_And_Date
    
	ESI = @..DebugWaitMsg

	# VidBase
    EDI = $B8000
    # 10 lines down, 20 cols in (10 * 80 + 20) * 2)
    EDI + 1640

    # Read and copy string till 0 terminator
ReadChar:
    AL = ESI[0]
    if (AL = 0) goto AfterMsg
    ESI + 1
    EDI[0] = AL
    EDI + 2
    Goto ReadChar
AfterMsg:
}
