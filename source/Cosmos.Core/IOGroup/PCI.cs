namespace Cosmos.Core.IOGroup;

/// <summary>
///     Peripheral Component Interconnect (PCI) class. See also: <seealso cref="IOGroup" />.
/// </summary>
public class PCI : IOGroup
{
    /// <summary>
    ///     Configuration address port.
    /// </summary>
    public IOPort ConfigAddressPort = new(0xCF8);

    /// <summary>
    ///     Configuration data port.
    /// </summary>
    public IOPort ConfigDataPort = new(0xCFC);
}
