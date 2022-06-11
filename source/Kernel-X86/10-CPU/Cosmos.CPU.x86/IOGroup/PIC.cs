namespace Cosmos.CPU.x86.IOGroup;

public class PIC : IOGroup
{
    public readonly IOPort Cmd = new(0x20);
    public readonly IOPort Data = new(0x21);

    internal PIC(bool aSlave)
    {
        var aBase = (byte)(aSlave ? 0xA0 : 0x20);
        Cmd = new IOPort(aBase);
        Data = new IOPort((byte)(aBase + 1));
    }
}
