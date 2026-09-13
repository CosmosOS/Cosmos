using System;

namespace Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

internal enum FIFOCommand
{
    /// <summary>
    /// Update.
    /// </summary>
    Update = 1,
    /// <summary>
    /// Rectange fill.
    /// </summary>
    RECT_FILL = 2,
    /// <summary>
    /// Rectange copy.
    /// </summary>
    RECT_COPY = 3,
    /// <summary>
    /// Define bitmap.
    /// </summary>
    DEFINE_BITMAP = 4,
    /// <summary>
    /// Define bitmap scanline.
    /// </summary>
    DEFINE_BITMAP_SCANLINE = 5,
    /// <summary>
    /// Define pixmap.
    /// </summary>
    DEFINE_PIXMAP = 6,
    /// <summary>
    /// Define pixmap scanline.
    /// </summary>
    DEFINE_PIXMAP_SCANLINE = 7,
    /// <summary>
    /// Rectange bitmap fill.
    /// </summary>
    RECT_BITMAP_FILL = 8,
    /// <summary>
    /// Rectange pixmap fill.
    /// </summary>
    RECT_PIXMAP_FILL = 9,
    /// <summary>
    /// Rectange bitmap copy.
    /// </summary>
    RECT_BITMAP_COPY = 10,
    /// <summary>
    /// Rectange pixmap fill.
    /// </summary>
    RECT_PIXMAP_COPY = 11,
    /// <summary>
    /// Free object.
    /// </summary>
    FREE_OBJECT = 12,
    /// <summary>
    /// Rectangle raster operation fill.
    /// </summary>
    RECT_ROP_FILL = 13,
    /// <summary>
    /// Rectangle raster operation copy.
    /// </summary>
    RECT_ROP_COPY = 14,
    /// <summary>
    /// Rectangle raster operation bitmap fill.
    /// </summary>
    RECT_ROP_BITMAP_FILL = 15,
    /// <summary>
    /// Rectangle raster operation pixmap fill.
    /// </summary>
    RECT_ROP_PIXMAP_FILL = 16,
    /// <summary>
    /// Rectangle raster operation bitmap copy.
    /// </summary>
    RECT_ROP_BITMAP_COPY = 17,
    /// <summary>
    /// Rectangle raster operation pixmap copy.
    /// </summary>
    RECT_ROP_PIXMAP_COPY = 18,
    /// <summary>
    /// Define cursor.
    /// </summary>
    DEFINE_CURSOR = 19,
    /// <summary>
    /// Display cursor.
    /// </summary>
    DISPLAY_CURSOR = 20,
    /// <summary>
    /// Move cursor.
    /// </summary>
    MOVE_CURSOR = 21,
    /// <summary>
    /// Define alpha cursor.
    /// </summary>
    DEFINE_ALPHA_CURSOR = 22,

    DEFINE_SURFACE = 1040,
    SURFACE_COPY = 1040 + 2,
    SETMATERIAL = 1040 + 12,
    SETLIGHTDATA = 1040 + 13,
    SETLIGHTENABLE = 1040 + 14,
    SETVIEWPORT = 1040 + 15,
    SETZRANGE = 1040 + 8,

    DEFINE_CONTEXT = 1040 + 5,
    DESTROY_CONTEXT = 1040 + 6,
    DEFINE_SURFACE_V2 = 1040 + 30,  // Use V2 surface definition
    DESTROY_SURFACE = 1040 + 1,
    DESTROY_SHADER = 1040 + 20,
    SET_RENDER_TARGET = 1040 + 10,
    CLEAR = 1040 + 17,
    SET_VIEWPORT = 1040 + 15,
    SET_ZRANGE = 1040 + 8,
    PRESENT = 1040 + 18,
    SETRENDERSTATE = 1040 + 9,
    SURFACE_DMA = 1040 + 4,
    DRAW_PRIMITIVES = 1040 + 23,
    SETTRANSFORM = 1040 + 7,
    SETTEXTURESTATE = 1040 + 11,
    SHADER_DEFINE = 1040 + 19,
    SET_SHADER = 1040 + 21,
    SET_SHADER_CONST = 1040 + 22,
}
