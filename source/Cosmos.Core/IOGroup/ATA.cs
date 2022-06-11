namespace Cosmos.Core.IOGroup;

public class ATA : IOGroup
{
    /// <summary>
    ///     Alternate Status IOPort.
    /// </summary>
    public readonly IOPortRead AlternateStatus; // BAR1 + 2 - read only

    /// <summary>
    ///     Command IOPort.
    /// </summary>
    public readonly IOPortWrite Command; // BAR0 + 7 - write only

    /// <summary>
    ///     Control IOPort.
    /// </summary>
    public readonly IOPortWrite Control; // BAR1 + 2 - write only

    /// <summary>
    ///     Data IOPort.
    /// </summary>
    public readonly IOPort Data; // BAR0

    /// <summary>
    ///     Device select IOPort.
    /// </summary>
    public readonly IOPort DeviceSelect; // BAR0 + 6

    /// <summary>
    ///     Error IOPort
    /// </summary>
    public readonly IOPort Error; // BAR0 + 1 - read only

    /// <summary>
    ///     Features IOPort.
    /// </summary>
    public readonly IOPortWrite Features; // BAR0 + 1 - write only

    /// <summary>
    ///     LBA0 IOPort.
    /// </summary>
    public readonly IOPort LBA0; // BAR0 + 3

    /// <summary>
    ///     LBA1 IOPort.
    /// </summary>
    public readonly IOPort LBA1; // BAR0 + 4

    /// <summary>
    ///     LBA2 IOPort.
    /// </summary>
    public readonly IOPort LBA2; // BAR0 + 5

    /// <summary>
    ///     LBA3 IOPort.
    /// </summary>
    public readonly IOPort LBA3; // BAR0 + 9

    /// <summary>
    ///     LBA4 IOPort.
    /// </summary>
    public readonly IOPort LBA4; // BAR0 + 10

    /// <summary>
    ///     LBA5 IOPort.
    /// </summary>
    public readonly IOPort LBA5; // BAR0 + 11

    /// <summary>
    ///     Sector Count IOPort.
    /// </summary>
    public readonly IOPort SectorCount; // BAR0 + 2

    /// <summary>
    ///     Sector count IOPort.
    /// </summary>
    public readonly IOPort SectorCountLBA48; // BAR0 + 8

    /// <summary>
    ///     Status IOPort.
    /// </summary>
    public readonly IOPortRead Status; // BAR0 + 7 - read only

    /// <summary>
    ///     Constructor for ATA-spec device (including ATAPI?)
    ///     aSecondary boolean to check if Primary or Secondary channel, used in modern ATA controllers
    /// </summary>
    /// <param name="aSecondary"></param>
    public ATA(bool aSecondary)
    {
        if (aSecondary)
        {
            Global.mDebugger.Send("Creating Secondary ATA IOGroup");
        }
        else
        {
            Global.mDebugger.Send("Creating Primary ATA IOGroup");
        }

        var xBAR0 = GetBAR0(aSecondary);
        var xBAR1 = GetBAR1(aSecondary);
        Error = new IOPort(xBAR0, 1);
        Features = new IOPortWrite(xBAR0, 1);
        Data = new IOPort(xBAR0);
        SectorCount = new IOPort(xBAR0, 2);
        LBA0 = new IOPort(xBAR0, 3);
        LBA1 = new IOPort(xBAR0, 4);
        LBA2 = new IOPort(xBAR0, 5);
        DeviceSelect = new IOPort(xBAR0, 6);
        Status = new IOPortRead(xBAR0, 7);
        Command = new IOPortWrite(xBAR0, 7);
        SectorCountLBA48 = new IOPort(xBAR0, 8);
        LBA3 = new IOPort(xBAR0, 9);
        LBA4 = new IOPort(xBAR0, 10);
        LBA5 = new IOPort(xBAR0, 11);
        AlternateStatus = new IOPortRead(xBAR1, 2);
        Control = new IOPortWrite(xBAR1, 2);
    }

    /// <summary>
    ///     Waits for IO operations to complete.
    /// </summary>
    public void Wait()
    {
        // Used for the PATA and IOPort latency
        // Widely accepted method is to read the status register 4 times - approx. 400ns delay.
        byte wait;
        wait = Status.Byte;
        wait = Status.Byte;
        wait = Status.Byte;
        wait = Status.Byte;
    }

    /// <summary>
    ///     Get control base address.
    /// </summary>
    /// <param name="aSecondary">True if secondary ATA.</param>
    /// <returns>ushort value.</returns>
    private static ushort GetBAR1(bool aSecondary)
    {
        var xBAR1 = (ushort)(aSecondary ? 0x0374 : 0x03F4);
        return xBAR1;
    }

    /// <summary>
    ///     Get command base address.
    /// </summary>
    /// <param name="aSecondary">True if secondary ATA.</param>
    /// <returns>ushort value.</returns>
    private static ushort GetBAR0(bool aSecondary)
    {
        var xBAR0 = (ushort)(aSecondary ? 0x0170 : 0x01F0);
        return xBAR0;
    }
}
