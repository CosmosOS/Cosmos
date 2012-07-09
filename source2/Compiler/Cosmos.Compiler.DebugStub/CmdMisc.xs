Group DebugStub

procedure Ping {
    # Ds2Vs.Pong
    AL = 13
    ComWriteAL()
}

procedure TraceOn {
    # Tracing.On
    .TraceMode = 1
}

procedure TraceOff {
    # Tracing.Off
    .TraceMode = 0
}
