namespace Cosmos.HAL.Drivers.Video.SVGAII
{
    /// <summary>
    /// Register values.
    /// </summary>
    public enum Register : ushort
    {
        /// <summary>
        /// ID.
        /// </summary>
        ID = 0,
        /// <summary>
        /// Enabled.
        /// </summary>
        Enable = 1,
        /// <summary>
        /// Width.
        /// </summary>
        Width = 2,
        /// <summary>
        /// Height.
        /// </summary>
        Height = 3,
        /// <summary>
        /// Max width.
        /// </summary>
        MaxWidth = 4,
        /// <summary>
        /// Max height.
        /// </summary>
        MaxHeight = 5,
        /// <summary>
        /// Depth.
        /// </summary>
        Depth = 6,
        /// <summary>
        /// Bits per pixel.
        /// </summary>
        BitsPerPixel = 7,
        /// <summary>
        /// Pseudo color.
        /// </summary>
        PseudoColor = 8,
        /// <summary>
        /// Red mask.
        /// </summary>
        RedMask = 9,
        /// <summary>
        /// Green mask.
        /// </summary>
        GreenMask = 10,
        /// <summary>
        /// Blue mask.
        /// </summary>
        BlueMask = 11,
        /// <summary>
        /// Bytes per line.
        /// </summary>
        BytesPerLine = 12,
        /// <summary>
        /// Frame buffer start.
        /// </summary>
        FrameBufferStart = 13,
        /// <summary>
        /// Frame buffer offset.
        /// </summary>
        FrameBufferOffset = 14,
        /// <summary>
        /// VRAM size.
        /// </summary>
        VRamSize = 15,
        /// <summary>
        /// Frame buffer size.
        /// </summary>
        FrameBufferSize = 16,
        /// <summary>
        /// Capabilities.
        /// </summary>
        Capabilities = 17,
        /// <summary>
        /// Memory start.
        /// </summary>
        MemStart = 18,
        /// <summary>
        /// Memory size.
        /// </summary>
        MemSize = 19,
        /// <summary>
        /// Config done.
        /// </summary>
        ConfigDone = 20,
        /// <summary>
        /// Sync.
        /// </summary>
        Sync = 21,
        /// <summary>
        /// Busy.
        /// </summary>
        Busy = 22,
        /// <summary>
        /// Guest ID.
        /// </summary>
        GuestID = 23,
        /// <summary>
        /// Cursor ID.
        /// </summary>
        CursorID = 24,
        /// <summary>
        /// Cursor X.
        /// </summary>
        CursorX = 25,
        /// <summary>
        /// Cursor Y.
        /// </summary>
        CursorY = 26,
        /// <summary>
        /// Cursor on.
        /// </summary>
        CursorOn = 27,
        /// <summary>
        /// Cursor count.
        /// </summary>
        CursorCount = 0x0C,
        /// <summary>
        /// Host bits per pixel.
        /// </summary>
        HostBitsPerPixel = 28,
        /// <summary>
        /// Scratch size.
        /// </summary>
        ScratchSize = 29,
        /// <summary>
        /// Memory registers.
        /// </summary>
        MemRegs = 30,
        /// <summary>
        /// Number of displays.
        /// </summary>
        NumDisplays = 31,
        /// <summary>
        /// Pitch lock.
        /// </summary>
        PitchLock = 32,
        /// <summary>
        /// Indicates maximum size of FIFO Registers.
        /// </summary>
        FifoNumRegisters = 293
    }
}