using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.BlockDevice;

public abstract class Ata : BlockDevice
{
    public enum BusPositionEnum
    {
        Master,
        Slave
    }

    // In future may need to add a None for PCI ATA controllers.
    // Or maybe they all have Primary and Secondary on them as well.
    public enum ControllerIdEnum
    {
        Primary,
        Secondary
    }

    internal static Debugger AtaDebugger = new("HAL", "Ata");
    protected BusPositionEnum mBusPosition;
    protected ControllerIdEnum mControllerID;

    protected Ata()
    {
        mBlockSize = 512;
    }

    public ControllerIdEnum ControllerID => mControllerID;
    public BusPositionEnum BusPosition => mBusPosition;

    public override string ToString() => "Ata (Abstract)";
}
