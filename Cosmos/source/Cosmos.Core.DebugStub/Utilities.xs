namespace DebugStub 

// Uses EAX: expected difference.
// Modifies: EBX
function CheckStack {
    
    // after a call, the stack gets pushed to, so add 4 to the expected difference
    eax + 4
    EBX = EBP
    EBX + EAX

    if EBX != ESP {
        // stack corruption.
        EAX = ESP[0]
        .CallerEIP = EAX
        SendStackCorruptionOccurred()
      halt:
        goto halt
    }
}