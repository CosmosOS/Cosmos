namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// PS/2 controller.
    /// </summary>
    public class PS2Controller
    {
        /// <summary>
        /// Data IO port.
        /// </summary>
        public readonly IOPort Data = new IOPort(0x60);
        /// <summary>
        /// Status IO port.
        /// </summary>
        public readonly IOPortRead Status = new IOPortRead(0x64);
        /// <summary>
        /// Command IO port.
        /// </summary>
        public readonly IOPortWrite Command = new IOPortWrite(0x64);
    }
}
