namespace Cosmos.Kernel.Core.Memory.VAS;

/// <summary>
/// Information about a CPU page fault / data abort.
/// </summary>
internal readonly struct PageFaultInfo
{
    /// <summary>
    /// Virtual address that caused the fault (CR2 on x64, FAR on ARM64).
    /// </summary>
    public ulong FaultAddress { get; }

    /// <summary>
    /// Architecture-specific error code (x64 error code or ESR_EL1 bits).
    /// </summary>
    public ulong ErrorCode { get; }

    /// <summary>
    /// True if the fault was caused by a write access.
    /// </summary>
    public bool WasWrite { get; }

    /// <summary>
    /// True if the fault was caused by an instruction fetch.
    /// </summary>
    public bool WasInstructionFetch { get; }

    public PageFaultInfo(ulong faultAddress, ulong errorCode, bool wasWrite, bool wasInstructionFetch)
    {
        FaultAddress = faultAddress;
        ErrorCode = errorCode;
        WasWrite = wasWrite;
        WasInstructionFetch = wasInstructionFetch;
    }
}
