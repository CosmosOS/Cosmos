Group DebugStub

procedure Ping {
    # DsVsip.Pong
    AL = 13
    Call .ComWriteAL
}

procedure TraceOn {
    # Tracing.On
    #.TraceMode = 1
}

procedure TraceOff {
    # Tracing.Off
    #.TraceMode = 0
}
