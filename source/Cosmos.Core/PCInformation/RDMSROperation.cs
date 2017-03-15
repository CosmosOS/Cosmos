namespace Cosmos.Core.PCInformation
{
    /// <summary>
    /// Credit to: http://www.sandpile.org/x86/msr.htm
    /// There is a lot more of registers. I will not add them
    /// </summary>
    public enum RDMSROperation : uint
    {
        GetTimeStampCounterValue = 0x00000010,
        GetTimeStampCounterAdjustement = 0x0000003B,
        GetProcessorId = 0xC0000103,
        GetMaximumFrequencyClockCount = 0x000000E7,
        GetActualFrequencyClockCount = 0x000000E8
    }
}