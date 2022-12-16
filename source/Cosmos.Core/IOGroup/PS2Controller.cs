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
        public readonly int Data = 0x60;
        /// <summary>
        /// Status IO port.
        /// </summary>
        public readonly int Status = 0x64;
        /// <summary>
        /// Command IO port.
        /// </summary>
        public readonly int Command = 0x64;
    }
}
