using System;

namespace Cosmos.HAL.Drivers.Video.SVGAII
{
    /// <summary>
    /// Capability values.
    /// </summary>
    [Flags]
    public enum Capability
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,
        /// <summary>
        /// Rectangle fill.
        /// </summary>
        RectFill = 1,
        /// <summary>
        /// Rectangle copy.
        /// </summary>
        RectCopy = 2,
        /// <summary>
        /// Rectangle pattern fill.
        /// </summary>
        RectPatFill = 4,
        /// <summary>
        /// Lecacy off screen.
        /// </summary>
        LecacyOffscreen = 8,
        /// <summary>
        /// Raster operation.
        /// </summary>
        RasterOp = 16,
        /// <summary>
        /// Cruser.
        /// </summary>
        Cursor = 32,
        /// <summary>
        /// Cursor bypass.
        /// </summary>
        CursorByPass = 64,
        /// <summary>
        /// Cursor bypass2.
        /// </summary>
        CursorByPass2 = 128,
        /// <summary>
        /// Eigth bit emulation.
        /// </summary>
        EigthBitEmulation = 256,
        /// <summary>
        /// Alpha cursor.
        /// </summary>
        AlphaCursor = 512,
        /// <summary>
        /// Glyph.
        /// </summary>
        Glyph = 1024,
        /// <summary>
        /// Glyph clipping.
        /// </summary>
        GlyphClipping = 0x00000800,
        /// <summary>
        /// Offscreen.
        /// </summary>
        Offscreen1 = 0x00001000,
        /// <summary>
        /// Alpha blend.
        /// </summary>
        AlphaBlend = 0x00002000,
        /// <summary>
        /// Three D.
        /// </summary>
        ThreeD = 0x00004000,
        /// <summary>
        /// Extended FIFO.
        /// </summary>
        ExtendedFifo = 0x00008000,
        /// <summary>
        /// Multi monitors.
        /// </summary>
        MultiMon = 0x00010000,
        /// <summary>
        /// Pitch lock.
        /// </summary>
        PitchLock = 0x00020000,
        /// <summary>
        /// IRQ mask.
        /// </summary>
        IrqMask = 0x00040000,
        /// <summary>
        /// Display topology.
        /// </summary>
        DisplayTopology = 0x00080000,
        /// <summary>
        /// GMR.
        /// </summary>
        Gmr = 0x00100000,
        /// <summary>
        /// Traces.
        /// </summary>
        Traces = 0x00200000,
        /// <summary>
        /// GMR2.
        /// </summary>
        Gmr2 = 0x00400000,
        /// <summary>
        /// Screen objects.
        /// </summary>
        ScreenObject2 = 0x00800000
    }
}