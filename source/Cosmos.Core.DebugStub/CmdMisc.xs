namespace DebugStub

function Ping {
    // Ds2Vs.Pong
    AL = 13
    ComWriteAL()
}

function TraceOn {
    // Tracing.On
    .TraceMode = 1
}

function TraceOff {
    // Tracing.Off
    .TraceMode = 0
}
