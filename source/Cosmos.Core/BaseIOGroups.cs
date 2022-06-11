using Cosmos.Core.IOGroup;

namespace Cosmos.Core;

/// <summary>
///     Base IO Groups. Used to easily access IO devices.
/// </summary>
public class BaseIOGroups
{
    /// <summary>
    ///     PC speaker.
    /// </summary>
    public static readonly PCSpeaker PCSpeaker = new();

    /// <summary>
    ///     Primary ATA.
    /// </summary>
    public readonly ATA ATA1 = new(false);

    /// <summary>
    ///     Secondary ATA.
    /// </summary>
    public readonly ATA ATA2 = new(true);

    /// <summary>
    ///     PIT.
    /// </summary>
    public readonly PIT PIT = new();

    // These are common/fixed pieces of hardware. PCI, USB etc should be self discovering
    // and not hardcoded like this.
    // Further more some kind of security needs to be applied to these, but even now
    // at least we have isolation between the consumers that use these.
    /// <summary>
    ///     PS/2 controller.
    /// </summary>
    public readonly PS2Controller PS2Controller = new();

    /// <summary>
    ///     Real time clock.
    /// </summary>
    public readonly RTC RTC = new();

    /// <summary>
    ///     Text screen.
    /// </summary>
    public readonly TextScreen TextScreen = new();

    /// <summary>
    ///     VBE.
    /// </summary>
    public readonly VBEIOGroup VBE = new();
}
