namespace Cosmos.Core.IOGroup;

/// <summary>
///     VGA class. See also: <seealso cref="IOGroup" />.
/// </summary>
public class VGA : IOGroup
{
    /// <summary>
    ///     Attribute controller index port.
    /// </summary>
    public readonly IOPortWrite AttributeController_Index = new(0x3C0);

    /// <summary>
    ///     Attribute controller read port.
    /// </summary>
    public readonly IOPortRead AttributeController_Read = new(0x3C1);

    /// <summary>
    ///     Attribute controller write port.
    /// </summary>
    public readonly IOPortWrite AttributeController_Write = new(0x3C0);

    /// <summary>
    ///     32KB at 0xB8000
    /// </summary>
    public readonly MemoryBlock CGATextMemoryBlock = new(0xB8000, 1024 * 32);

    /// <summary>
    ///     CRT controller data port.
    /// </summary>
    public readonly IOPort CRTController_Data = new(0x3D5);

    /// <summary>
    ///     CRT controller index port.
    /// </summary>
    public readonly IOPortWrite CRTController_Index = new(0x3D4);

    /// <summary>
    ///     DAC data port.
    /// </summary>
    public readonly IOPort DAC_Data = new(0x3C9);

    /// <summary>
    ///     DAC index read port.
    /// </summary>
    public readonly IOPortWrite DACIndex_Read = new(0x3C7);

    /// <summary>
    ///     DAC index write port.
    /// </summary>
    public readonly IOPortWrite DACIndex_Write = new(0x3C8);

    /// <summary>
    ///     Graphics controller data port.
    /// </summary>
    public readonly IOPort GraphicsController_Data = new(0x3CF);

    /// <summary>
    ///     Graphics controller index port.
    /// </summary>
    public readonly IOPortWrite GraphicsController_Index = new(0x3CE);

    /// <summary>
    ///     Instant read port.
    /// </summary>
    public readonly IOPortRead Instat_Read = new(0x3DA);

    /// <summary>
    ///     Miscellaneous output write port.
    /// </summary>
    public readonly IOPortWrite MiscellaneousOutput_Write = new(0x3C2);

    /// <summary>
    ///     32KB at 0xB0000
    /// </summary>
    public readonly MemoryBlock MonochromeTextMemoryBlock = new(0xB0000, 1024 * 32);

    /// <summary>
    ///     Sequencer data port.
    /// </summary>
    public readonly IOPort Sequencer_Data = new(0x3C5);

    /// <summary>
    ///     Sequencer index port.
    /// </summary>
    public readonly IOPortWrite Sequencer_Index = new(0x3C4);

    /// <summary>
    ///     128KB at 0xA0000
    /// </summary>
    public readonly MemoryBlock VGAMemoryBlock = new(0xA0000, 1024 * 128);
}
