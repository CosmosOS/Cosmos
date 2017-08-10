namespace DebugStub

// Helper functions which make it easier to use serial stuff

function ComReadEAX {
	repeat 4 times {
		ComReadAL()
		EAX ~> 8
	}
}

// Input: EDI
// Output: [EDI]
// Modified: AL, DX, EDI (+1)
//
// Reads a byte into [EDI] and does EDI + 1
function ComRead8  {
    ComReadAL()
    EDI[0] = AL
    EDI + 1
}
function ComRead16 {
	repeat 2 times {
		ComRead8()
	}
}
function ComRead32 {
	repeat 4 times {
		ComRead8()
	}
}

// Input: AL
// Output: None
// Modifies: EDX
function ComWriteAL {
	+ESI
    +EAX
	ESI = ESP
    ComWrite8()
    // Is a local var, cant use Return(4). X// issues the return.
    // This also allows the function to preserve EAX.
    -EAX
	-ESI
}
function ComWriteAX {
    // Input: AX
    // Output: None
    // Modifies: EDX, ESI
    +EAX
    ESI = ESP
    ComWrite16()
    // Is a local var, cant use Return(4). X// issues the return.
    // This also allow the function to preserve EAX.
    -EAX
}
function ComWriteEAX {
    // Input: EAX
    // Output: None
    // Modifies: EDX, ESI
    +EAX
    ESI = ESP
    ComWrite32()
    // Is a local var, cant use Return(4). X// issues the return.
    // This also allow the function to preserve EAX.
    -EAX
}

function ComWrite16 {
	ComWrite8()
	ComWrite8()
}
function ComWrite32 {
	ComWrite8()
	ComWrite8()
	ComWrite8()
	ComWrite8()
}
function ComWriteX {
More:
	ComWrite8()
	ECX--
	if !0 goto More
}
