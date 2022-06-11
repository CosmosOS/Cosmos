namespace Cosmos.Core.IOGroup;

/// <summary>
///     Keyboard class. See also: <seealso cref="IOGroup" />.
/// </summary>
public class Keyboard : IOGroup
{
    /// <summary>
    ///     Data port.
    /// </summary>
    public readonly IOPort Port60 = new(0x60);
}
