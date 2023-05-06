namespace Cosmos.HAL.Drivers.Video.SVGAII
{
    /// <summary>
    /// ID values.
    /// </summary>
    public enum ID : uint
    {
        /// <summary>
        /// Magic starting point.
        /// </summary>
        Magic = 0x900000,
        /// <summary>
        /// V0.
        /// </summary>
        V0 = Magic << 8,
        /// <summary>
        /// V1.
        /// </summary>
        V1 = Magic << 8 | 1,
        /// <summary>
        /// V2.
        /// </summary>
        V2 = Magic << 8 | 2,
        /// <summary>
        /// Invalid
        /// </summary>
        Invalid = 0xFFFFFFFF
    }
}