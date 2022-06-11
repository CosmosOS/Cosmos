namespace Cosmos.Core.IOGroup;

/// <summary>
///     Real time clock class.
/// </summary>
public class RTC : IOGroup
{
    /// <summary>
    ///     Address IOPort.
    /// </summary>
    public readonly IOPort Address = new(0x70);

    /// <summary>
    ///     Data IOPort.
    /// </summary>
    public readonly IOPort Data = new(0x71);
}
