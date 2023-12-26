namespace Cosmos.HAL
{
    public enum COMPort : ushort
    {
        // com1 is used by qemu by default
        /// <summary>
        /// IO port for COM1 port
        /// </summary>
        COM1 = 0x3F8,
        /// <summary>
        /// IO port for COM2 port
        /// </summary>
        COM2 = 0x2F8,
        /// <summary>
        /// IO port for COM3 port
        /// </summary>
        COM3 = 0x3E8,
        /// <summary>
        /// IO port for COM4 port
        /// </summary>
        COM4 = 0x2E8,
        /// <summary>
        /// IO port for COM5 port
        /// </summary>
        COM5 = 0x5F8,
        /// <summary>
        /// IO port for COM6 port
        /// </summary>
        COM6 = 0x4F8,
        /// <summary>
        /// IO port for COM7 port
        /// </summary>
        COM7 = 0x5E8,
        /// <summary>
        /// IO port for COM8 port
        /// </summary>
        COM8 = 0x4E8
    }
}