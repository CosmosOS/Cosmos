// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>Completion codes an event TRB reports (xHCI 1.2 section 6.4.5).</summary>
internal enum XhciCompletionCode : byte
{
    /// <summary>Not a controller value: no completion arrived in time.</summary>
    Invalid = 0,

    /// <summary>The TRB completed.</summary>
    Success = 1,

    /// <summary>The controller could not reach the data buffer.</summary>
    DataBufferError = 2,

    /// <summary>The device sent more data than the TRB allowed.</summary>
    BabbleDetected = 3,

    /// <summary>The device did not answer the transaction, or answered badly (a pulled device fails this way).</summary>
    UsbTransactionError = 4,

    /// <summary>A TRB field was invalid.</summary>
    TrbError = 5,

    /// <summary>The device answered STALL.</summary>
    StallError = 6,

    /// <summary>The controller ran out of an internal resource for the command.</summary>
    ResourceError = 7,

    /// <summary>The periodic schedule has no room for the endpoint.</summary>
    BandwidthError = 8,

    /// <summary>Enable Slot found every device slot in use.</summary>
    NoSlotsAvailable = 9,

    /// <summary>The stream context type was invalid.</summary>
    InvalidStreamType = 10,

    /// <summary>The command named a slot that is not enabled.</summary>
    SlotNotEnabled = 11,

    /// <summary>The doorbell or command named an endpoint that is not enabled.</summary>
    EndpointNotEnabled = 12,

    /// <summary>The TRB completed with fewer bytes than it asked for.</summary>
    ShortPacket = 13,

    /// <summary>An isochronous OUT ring ran out of TRBs.</summary>
    RingUnderrun = 14,

    /// <summary>An isochronous IN ring ran out of TRBs.</summary>
    RingOverrun = 15,

    /// <summary>A virtual function's event ring was full.</summary>
    VfEventRingFull = 16,

    /// <summary>A context field was invalid.</summary>
    ParameterError = 17,

    /// <summary>An isochronous endpoint exceeded its reserved bandwidth.</summary>
    BandwidthOverrun = 18,

    /// <summary>The endpoint or slot was not in the state the command needs.</summary>
    ContextStateError = 19,

    /// <summary>A high-speed OUT endpoint did not answer PING in time.</summary>
    NoPingResponse = 20,

    /// <summary>The event ring was full and an event was lost.</summary>
    EventRingFull = 21,

    /// <summary>The device is incompatible with the controller.</summary>
    IncompatibleDevice = 22,

    /// <summary>An isochronous endpoint missed its service interval.</summary>
    MissedService = 23,

    /// <summary>The command ring was stopped by software.</summary>
    CommandRingStopped = 24,

    /// <summary>The command was aborted by software.</summary>
    CommandAborted = 25,

    /// <summary>A Stop Endpoint command stopped the endpoint on this TRB.</summary>
    Stopped = 26,

    /// <summary>A Stop Endpoint command stopped the endpoint on this TRB and the transfer length is not valid.</summary>
    StoppedLengthInvalid = 27,

    /// <summary>A Stop Endpoint command stopped the endpoint on this TRB after a short packet.</summary>
    StoppedShortPacket = 28,

    /// <summary>The requested Max Exit Latency exceeds what the controller supports.</summary>
    MaxExitLatencyTooLarge = 29,

    /// <summary>An isochronous IN transfer received more than the buffer holds.</summary>
    IsochBufferOverrun = 31,

    /// <summary>The controller lost an event (xHCI 1.2 section 4.10.3.2).</summary>
    EventLost = 32,

    /// <summary>An error the controller does not classify.</summary>
    UndefinedError = 33,

    /// <summary>The stream id was invalid.</summary>
    InvalidStreamId = 34,

    /// <summary>A secondary bandwidth domain has no room for the endpoint.</summary>
    SecondaryBandwidthError = 35,

    /// <summary>A split transaction through a Transaction Translator failed.</summary>
    SplitTransactionError = 36
}
