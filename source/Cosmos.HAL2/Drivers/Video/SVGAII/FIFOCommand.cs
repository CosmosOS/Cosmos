namespace Cosmos.HAL.Drivers.Video.SVGAII
{
    /// <summary>
    /// FIFO command values.
    /// </summary>
    public enum FIFOCommand
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
        DEFINE_ALPHA_CURSOR = 22
    }
}