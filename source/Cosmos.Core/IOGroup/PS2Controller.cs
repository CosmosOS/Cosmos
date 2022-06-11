namespace Cosmos.Core.IOGroup;

/// <summary>
///     PS/2 controller.
/// </summary>
public class PS2Controller
{
    /// <summary>
    ///     Command IO port.
    /// </summary>
    public readonly IOPortWrite Command = new(0x64);

    /// <summary>
    ///     Data IO port.
    /// </summary>
    public readonly IOPort Data = new(0x60);

    /// <summary>
    ///     Status IO port.
    /// </summary>
    public readonly IOPortRead Status = new(0x64);
}
