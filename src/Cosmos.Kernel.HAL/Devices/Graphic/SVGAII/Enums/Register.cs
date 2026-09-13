using System;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

internal enum Register : ushort
{
    Enable3D = 0x20,
    Guest3DScratchSize = 0x21,
    Capabilities3D = 0x22,
    ID = 0,
    Enable = 1,
    Width = 2,
    Height = 3,
    MaxWidth = 4,
    MaxHeight = 5,
    Depth = 6,
    BitsPerPixel = 7,
    PseudoColor = 8,
    RedMask = 9,
    GreenMask = 10,
    BlueMask = 11,
    BytesPerLine = 12,
    FrameBufferStart = 13,
    FrameBufferOffset = 14,
    VRamSize = 15,
    FrameBufferSize = 16,
    Capabilities = 17,
    MemStart = 18,
    MemSize = 19,
    ConfigDone = 20,
    Sync = 21,
    Busy = 22,
    GuestID = 23,
    CursorID = 24,
    CursorX = 25,
    CursorY = 26,
    CursorOn = 27,
    CursorCount = 0x0C,
    HostBitsPerPixel = 28,
    ScratchSize = 29,
    MemRegs = 30,
    NumDisplays = 31,
    PitchLock = 32,
    FifoNumRegisters = 293
}

internal enum Register3D
{
    SVGA_FIFO_MIN = 0,
    SVGA_FIFO_MAX,       /* The distance from MIN to MAX must be at least 10K */
    SVGA_FIFO_NEXT_CMD,
    SVGA_FIFO_STOP,

    SVGA_FIFO_CAPABILITIES = 4,
    SVGA_FIFO_FLAGS,
    // Valid with SVGA_FIFO_CAP_FENCE:
    SVGA_FIFO_FENCE,

    SVGA_FIFO_3D_HWVERSION,       /* See SVGA3dHardwareVersion in svga3d_reg.h */
    // Valid with SVGA_FIFO_CAP_PITCHLOCK:
    SVGA_FIFO_PITCHLOCK,

    // Valid with SVGA_FIFO_CAP_CURSOR_BYPASS_3:
    SVGA_FIFO_CURSOR_ON,          /* Cursor bypass 3 show/hide register */
    SVGA_FIFO_CURSOR_X,           /* Cursor bypass 3 x register */
    SVGA_FIFO_CURSOR_Y,           /* Cursor bypass 3 y register */
    SVGA_FIFO_CURSOR_COUNT,       /* Incremented when any of the other 3 change */
    SVGA_FIFO_CURSOR_LAST_UPDATED,/* Last time the host updated the cursor */

    // Valid with SVGA_FIFO_CAP_RESERVE:
    SVGA_FIFO_RESERVED,           /* Bytes past NEXT_CMD with real contents */

    SVGA_FIFO_CURSOR_SCREEN_ID,

    SVGA_FIFO_DEAD,

    SVGA_FIFO_3D_HWVERSION_REVISED,

    SVGA_FIFO_3D_CAPS = 32,
    SVGA_FIFO_3D_CAPS_LAST = 32 + 255,

    // Valid if register exists:
    SVGA_FIFO_GUEST_3D_HWVERSION, /* Guest driver's 3D version */
    SVGA_FIFO_FENCE_GOAL,         /* Matching target for SVGA_IRQFLAG_FENCE_GOAL */
    SVGA_FIFO_BUSY,               /* See "FIFO Synchronization Registers" */

    SVGA_FIFO_NUM_REGS
}
