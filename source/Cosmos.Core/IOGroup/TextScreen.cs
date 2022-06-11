namespace Cosmos.Core.IOGroup;

/// <summary>
///     IOGroup text screen.
/// </summary>
public class TextScreen : IOGroup
{
    /// <summary>
    ///     First IOPort data.
    /// </summary>
    public readonly IOPort Data1 = new(0x03C5);

    /// <summary>
    ///     Second IOPort data.
    /// </summary>
    public readonly IOPort Data2 = new(0x03CF);

    /// <summary>
    ///     Third IOPort data.
    /// </summary>
    public readonly IOPort Data3 = new(0x03D5);

    /// <summary>
    ///     First IOPort index.
    /// </summary>
    public readonly IOPort Idx1 = new(0x03C4);

    /// <summary>
    ///     Second IOPort index.
    /// </summary>
    public readonly IOPort Idx2 = new(0x03CE);

    /// <summary>
    ///     Third IOPort index.
    /// </summary>
    public readonly IOPort Idx3 = new(0x03D4);

    // These should probably move to a VGA class later, or this class should be remade into a VGA class
    /// <summary>
    ///     Misc. output.
    /// </summary>
    public readonly IOPort MiscOutput = new(0x03C2);

    /// <summary>
    ///     Memory.
    /// </summary>
    public MemoryBlock Memory = new(0xB8000, 80 * 25 * 2);
}
