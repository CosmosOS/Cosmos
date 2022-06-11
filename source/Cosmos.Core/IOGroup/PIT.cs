namespace Cosmos.Core.IOGroup;

/// <summary>
///     Programmable Interval Timer (PIT) class. See also: <seealso cref="IOGroup" />.
/// </summary>
public class PIT : IOGroup
{
    /// <summary>
    ///     Command register port.
    /// </summary>
    public readonly IOPortWrite Command = new(0x43);

    /// <summary>
    ///     Channel 0 data port.
    /// </summary>
    public readonly IOPort Data0 = new(0x40);

    /// <summary>
    ///     Channel 1 data port.
    /// </summary>
    public readonly IOPort Data1 = new(0x41);

    /// <summary>
    ///     Channel 2 data port.
    /// </summary>
    public readonly IOPort Data2 = new(0x42);
}
