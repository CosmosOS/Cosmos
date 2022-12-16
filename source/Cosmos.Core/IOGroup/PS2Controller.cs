namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// PS/2 controller.
    /// </summary>
    public static class PS2Controller
    {
        /// <summary>
        /// Data IO port.
        /// </summary>
        public const int Data = 0x60;
        /// <summary>
        /// Status IO port.
        /// </summary>
        public const int Status = 0x64;
        /// <summary>
        /// Command IO port.
        /// </summary>
        public const int Command = 0x64;
    }
}
