namespace Cosmos.Core.IOGroup;

/// <summary>
///     VBE class.
/// </summary>
public class VBEIOGroup : IOGroup
{
    /*
     * This not a lot optimal as we are taking a lot of memory and then maybe the driver is configured to go at 320*240!
     */
    /// <summary>
    ///     Frame buffer memory block.
    /// </summary>
    public MemoryBlock LinearFrameBuffer;

    /// <summary>
    ///     Data IOPort.
    /// </summary>
    public IOPort VbeData = new(0x01CF);

    /// <summary>
    ///     Index IOPort.
    /// </summary>
    public IOPort VbeIndex = new(0x01CE);
    //public MemoryBlock LinearFrameBuffer = new MemoryBlock(0xE0000000, 1024 * 768 * 4);
}
