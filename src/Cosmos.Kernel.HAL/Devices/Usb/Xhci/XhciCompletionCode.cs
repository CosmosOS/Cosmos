// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>Completion codes an event TRB reports (xHCI 1.2 §6.4.5).</summary>
internal enum XhciCompletionCode : byte
{
    /// <summary>Not a controller value: no completion arrived in time.</summary>
    Invalid = 0,

    Success = 1,
    DataBufferError = 2,
    BabbleDetected = 3,
    UsbTransactionError = 4,
    TrbError = 5,
    StallError = 6,
    ResourceError = 7,
    BandwidthError = 8,
    NoSlotsAvailable = 9,
    InvalidStreamType = 10,
    SlotNotEnabled = 11,
    EndpointNotEnabled = 12,
    ShortPacket = 13,
    RingUnderrun = 14,
    RingOverrun = 15,
    VfEventRingFull = 16,
    ParameterError = 17,
    BandwidthOverrun = 18,
    ContextStateError = 19,
    NoPingResponse = 20,
    EventRingFull = 21,
    IncompatibleDevice = 22,
    MissedService = 23,
    CommandRingStopped = 24,
    CommandAborted = 25,
    Stopped = 26,
    StoppedLengthInvalid = 27,
    StoppedShortPacket = 28,
    MaxExitLatencyTooLarge = 29,
    IsochBufferOverrun = 31,
    EventLost = 32,
    UndefinedError = 33,
    InvalidStreamId = 34,
    SecondaryBandwidthError = 35,
    SplitTransactionError = 36
}
