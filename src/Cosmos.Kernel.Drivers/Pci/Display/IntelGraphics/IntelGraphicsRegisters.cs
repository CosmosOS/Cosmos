// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// The registers <see cref="IntelGraphicsDriver"/> uses, as Linux's i915
/// names and places them (<c>i9xx_plane_regs.h</c>,
/// <c>skl_universal_plane_regs.h</c>, <c>intel_cursor_regs.h</c>,
/// <c>intel_display_regs.h</c>, <c>i9xx_wm_regs.h</c>, <c>i915_reg.h</c>,
/// <c>i915_drm.h</c>). A display register is a byte offset into BAR 0;
/// the per-pipe ones are pipe A's and repeat every <see cref="PipeStride"/>
/// bytes, except where a member says otherwise. The PCI configuration
/// registers are offsets into the function's configuration space.
/// </summary>
internal static class IntelGraphicsRegisters
{
    // --- PCI configuration space ---

    /// <summary>GGC, the graphics control the host bridge mirrors into the GPU: stolen memory size and translation table size.</summary>
    internal const ushort GraphicsControl = 0x50;

    /// <summary>The stolen memory size field of <see cref="GraphicsControl"/> before Broadwell, in 32 MiB units.</summary>
    internal const int StolenSizeShiftGen6 = 3;

    /// <summary>The mask of the stolen memory size field before Broadwell, after the shift.</summary>
    internal const uint StolenSizeMaskGen6 = 0x1F;

    /// <summary>The stolen memory size field of <see cref="GraphicsControl"/> from Broadwell on.</summary>
    internal const int StolenSizeShiftGen8 = 8;

    /// <summary>The mask of the stolen memory size field from Broadwell on, after the shift.</summary>
    internal const uint StolenSizeMaskGen8 = 0xFF;

    /// <summary>From Skylake on, the first stolen memory size code counted in 4 MiB steps from 4 MiB rather than in 32 MiB steps.</summary>
    internal const uint StolenSizeFirst4MiBCode = 0xF0;

    /// <summary>The translation table size field of <see cref="GraphicsControl"/> before Broadwell: the table's size in MiB.</summary>
    internal const int GttSizeShiftGen6 = 8;

    /// <summary>The translation table size field of <see cref="GraphicsControl"/> from Broadwell on: the table is 1 shifted left by it, in MiB.</summary>
    internal const int GttSizeShiftGen8 = 6;

    /// <summary>The mask of either translation table size field, after the shift.</summary>
    internal const uint GttSizeMask = 0x3;

    /// <summary>BDSM, the stolen memory base before Ice Lake.</summary>
    internal const ushort StolenBase = 0x5C;

    /// <summary>BDSM's low half from Ice Lake on.</summary>
    internal const ushort StolenBaseLowGen11 = 0xC0;

    /// <summary>BDSM's high half from Ice Lake on.</summary>
    internal const ushort StolenBaseHighGen11 = 0xC4;

    /// <summary>The address bits of BDSM: stolen memory starts on a MiB boundary.</summary>
    internal const uint StolenBaseMask = 0xFFF0_0000;

    // --- GT ---

    /// <summary>GFX_FLSH_CNTL_GEN6: a write flushes the translation table writes and invalidates the GT's view of the table.</summary>
    internal const uint GraphicsFlushControl = 0x10_1008;

    /// <summary>GFX_FLSH_CNTL_EN.</summary>
    internal const uint GraphicsFlushEnable = 0x0000_0001;

    /// <summary>GEN6_STOLEN_RESERVED: the top of stolen memory the firmware keeps (the WOPCM and others); 64 bits from Ice Lake on.</summary>
    internal const uint StolenReserved = 0x10_82C0;

    /// <summary>GEN6_STOLEN_RESERVED_ENABLE.</summary>
    internal const uint StolenReservedEnable = 0x0000_0001;

    /// <summary>The base field of <see cref="StolenReserved"/>, rounded to the MiB (Ivy Bridge and Haswell align it to 256 KiB, so this reserves at most a little more).</summary>
    internal const uint StolenReservedBaseMask = 0xFFF0_0000;

    // --- VGA ---

    /// <summary>CPU_VGACNTRL: the legacy VGA plane.</summary>
    internal const uint VgaControl = 0x4_1000;

    /// <summary>VGA_DISP_DISABLE: set when the VGA plane is off, as a UEFI boot leaves it.</summary>
    internal const uint VgaDisplayDisable = 0x8000_0000;

    // --- Pipes ---

    /// <summary>Bytes between two pipes' registers (and two transcoders' source size registers).</summary>
    internal const uint PipeStride = 0x1000;

    /// <summary>PIPESRC: the pipe's source size, width minus one high, height minus one low.</summary>
    internal const uint PipeSourceSize = 0x6_001C;

    /// <summary>PIPE_FRMCOUNT_G4X: the pipe's frame counter, which steps at the start of each vertical blank.</summary>
    internal const uint PipeFrameCount = 0x7_0040;

    // --- Primary plane (DSP* before Skylake, PLANE_*_1 from Skylake on) ---

    /// <summary>DSPCNTR or PLANE_CTL: enable, pixel format, tiling, rotation.</summary>
    internal const uint PlaneControl = 0x7_0180;

    /// <summary>DSPLINOFF: the linear byte offset of the plane's start in its surface, before Haswell.</summary>
    internal const uint PlaneLinearOffset = 0x7_0184;

    /// <summary>DSPSTRIDE or PLANE_STRIDE: the surface pitch, in bytes before Skylake and in 64-byte units from Skylake on.</summary>
    internal const uint PlaneStride = 0x7_0188;

    /// <summary>PLANE_POS: where the plane sits on the pipe, y high and x low (Skylake on).</summary>
    internal const uint PlanePosition = 0x7_018C;

    /// <summary>PLANE_SIZE: the plane's height minus one high and width minus one low (Skylake on).</summary>
    internal const uint PlaneSize = 0x7_0190;

    /// <summary>DSPSURF or PLANE_SURF: the surface's address in the graphics translation table; the write arms the plane's update at the next vertical blank.</summary>
    internal const uint PlaneSurface = 0x7_019C;

    /// <summary>DSPTILEOFF, DSPOFFSET or PLANE_OFFSET: the plane's start in its surface, y high and x low.</summary>
    internal const uint PlaneOffset = 0x7_01A4;

    /// <summary>DSPSURFLIVE or PLANE_SURFLIVE: the surface address the plane is scanning out now.</summary>
    internal const uint PlaneSurfaceLive = 0x7_01AC;

    /// <summary>PLANE_WM_1 level 0: the plane's watermarks, one register per level (Skylake on).</summary>
    internal const uint PlaneWatermark = 0x7_0240;

    /// <summary>PLANE_WM_SAGV_1 (display 13).</summary>
    internal const uint PlaneWatermarkSagv = 0x7_0258;

    /// <summary>PLANE_WM_SAGV_TRANS_1 (display 13).</summary>
    internal const uint PlaneWatermarkSagvTransition = 0x7_025C;

    /// <summary>PLANE_WM_TRANS_1: the transition watermark (Skylake on).</summary>
    internal const uint PlaneWatermarkTransition = 0x7_0268;

    /// <summary>PLANE_BUF_CFG_1: the plane's share of the display buffer, end block high and start block low, both inclusive (Skylake on).</summary>
    internal const uint PlaneBufferConfig = 0x7_027C;

    /// <summary>DISP_ENABLE or PLANE_CTL_ENABLE.</summary>
    internal const uint PlaneEnable = 0x8000_0000;

    /// <summary>DISP_ASYNC_FLIP or PLANE_CTL_ASYNC_FLIP: a surface write takes effect mid-frame.</summary>
    internal const uint PlaneAsyncFlip = 0x0000_0200;

    /// <summary>DISP_PIPE_GAMMA_ENABLE or PLANE_CTL_PIPE_GAMMA_ENABLE (before Ice Lake).</summary>
    internal const uint PlanePipeGammaEnable = 0x4000_0000;

    /// <summary>DISP_FORMAT_MASK, the pixel format before Skylake.</summary>
    internal const uint DisplayFormatMask = 0x3C00_0000;

    /// <summary>DISP_FORMAT_BGRX888: 32 bits per pixel, blue in the low byte.</summary>
    internal const uint DisplayFormatXrgb8888 = 0x1800_0000;

    /// <summary>DISP_PIPE_CSC_ENABLE, before Skylake.</summary>
    internal const uint DisplayPipeCscEnable = 0x0100_0000;

    /// <summary>DISP_ROTATE_180, before Skylake.</summary>
    internal const uint DisplayRotate180 = 0x0000_8000;

    /// <summary>DISP_TILED: an X-tiled surface, before Skylake.</summary>
    internal const uint DisplayTiled = 0x0000_0400;

    /// <summary>PLANE_CTL_FORMAT_MASK_ICL: the pixel format from Skylake on; bit 23 is the pipe CSC enable before Ice Lake.</summary>
    internal const uint PlaneFormatMask = 0x0F80_0000;

    /// <summary>The format bits Skylake has: <see cref="PlaneFormatMask"/> without bit 23.</summary>
    internal const uint PlaneFormatMaskGen9 = 0x0F00_0000;

    /// <summary>PLANE_CTL_FORMAT_XRGB_8888: 32 bits per pixel.</summary>
    internal const uint PlaneFormatXrgb8888 = 0x0400_0000;

    /// <summary>PLANE_CTL_PIPE_CSC_ENABLE, Skylake only.</summary>
    internal const uint PlanePipeCscEnable = 0x0080_0000;

    /// <summary>PLANE_CTL_ORDER_RGBX: red in the low byte rather than blue.</summary>
    internal const uint PlaneOrderRgbx = 0x0010_0000;

    /// <summary>PLANE_CTL_TILED_MASK; 0 is linear.</summary>
    internal const uint PlaneTiledMask = 0x0000_1C00;

    /// <summary>PLANE_CTL_FLIP_HORIZONTAL (Ice Lake on).</summary>
    internal const uint PlaneFlipHorizontal = 0x0000_0100;

    /// <summary>PLANE_CTL_ROTATE_MASK; 0 is unrotated.</summary>
    internal const uint PlaneRotateMask = 0x0000_0003;

    /// <summary>PLANE_STRIDE's field, in 64-byte units for a linear surface.</summary>
    internal const uint PlaneStrideMask = 0x0000_0FFF;

    /// <summary>Bytes in one unit of <see cref="PlaneStrideMask"/>.</summary>
    internal const int PlaneStrideUnit = 64;

    /// <summary>DSPSTRIDE's bytes, a multiple of 64.</summary>
    internal const uint DisplayStrideMask = 0xFFFF_FFC0;

    /// <summary>DISP_ADDR_MASK or PLANE_SURF_ADDR_MASK: a surface starts on a page.</summary>
    internal const uint SurfaceAddressMask = 0xFFFF_F000;

    /// <summary>The shift of the high half of a size, position or offset register.</summary>
    internal const int HighHalfShift = 16;

    /// <summary>The mask of either half of a size, position or offset register.</summary>
    internal const uint HalfMask = 0xFFFF;

    // --- Cursor (pipe A's; see IntelGraphicsGeneration.CursorStride) ---

    /// <summary>CURCNTR or CUR_CTL: mode, size and the pipe colour enables.</summary>
    internal const uint CursorControl = 0x7_0080;

    /// <summary>CURBASE or CUR_BASE: the image's address in the translation table; the write arms the cursor's update.</summary>
    internal const uint CursorBase = 0x7_0084;

    /// <summary>CURPOS or CUR_POS: where the image's top left corner sits on the pipe, sign and magnitude.</summary>
    internal const uint CursorPosition = 0x7_0088;

    /// <summary>CUR_FBC_CTL: a height override for a cursor shorter than it is wide (Ivy Bridge to display 13).</summary>
    internal const uint CursorFbcControl = 0x7_00A0;

    /// <summary>CUR_WM level 0: the cursor's watermarks, one register per level (Skylake on, every pipe at <see cref="PipeStride"/>).</summary>
    internal const uint CursorWatermark = 0x7_0140;

    /// <summary>CUR_WM_SAGV (display 13).</summary>
    internal const uint CursorWatermarkSagv = 0x7_0158;

    /// <summary>CUR_WM_SAGV_TRANS (display 13).</summary>
    internal const uint CursorWatermarkSagvTransition = 0x7_015C;

    /// <summary>CUR_WM_TRANS: the cursor's transition watermark (Skylake on).</summary>
    internal const uint CursorWatermarkTransition = 0x7_0168;

    /// <summary>CUR_BUF_CFG: the cursor's share of the display buffer (Skylake on, every pipe at <see cref="PipeStride"/>).</summary>
    internal const uint CursorBufferConfig = 0x7_017C;

    /// <summary>MCURSOR_MODE_64_ARGB_AX: a 64 by 64 premultiplied ARGB cursor.</summary>
    internal const uint CursorMode64 = 0x27;

    /// <summary>MCURSOR_MODE_128_ARGB_AX.</summary>
    internal const uint CursorMode128 = 0x22;

    /// <summary>MCURSOR_MODE_256_ARGB_AX.</summary>
    internal const uint CursorMode256 = 0x23;

    /// <summary>MCURSOR_PIPE_GAMMA_ENABLE (before Ice Lake).</summary>
    internal const uint CursorPipeGammaEnable = 0x0400_0000;

    /// <summary>MCURSOR_PIPE_CSC_ENABLE (before Ice Lake).</summary>
    internal const uint CursorPipeCscEnable = 0x0100_0000;

    /// <summary>MCURSOR_TRICKLE_FEED_DISABLE.</summary>
    internal const uint CursorTrickleFeedDisable = 0x0000_4000;

    /// <summary>MCURSOR_ARB_SLOTS(1).</summary>
    internal const uint CursorOneArbitrationSlot = 0x1000_0000;

    /// <summary>CURSOR_POS_Y_SIGN.</summary>
    internal const uint CursorPositionYSign = 0x8000_0000;

    /// <summary>CURSOR_POS_X_SIGN.</summary>
    internal const uint CursorPositionXSign = 0x0000_8000;

    /// <summary>The magnitude of either half of <see cref="CursorPosition"/>.</summary>
    internal const uint CursorPositionMask = 0x7FFF;

    // --- Display buffer and watermarks (Skylake on) ---

    /// <summary>The block fields of a buffer configuration: start low, end high at <see cref="HighHalfShift"/>.</summary>
    internal const uint BufferBlockMask = 0x1FFF;

    /// <summary>PLANE_WM_EN or CUR_WM_EN.</summary>
    internal const uint WatermarkEnable = 0x8000_0000;

    /// <summary>The shift of a watermark's line count.</summary>
    internal const int WatermarkLinesShift = 14;

    /// <summary>The mask of a watermark's line count, after the shift.</summary>
    internal const uint WatermarkLinesMask = 0x1FFF;

    /// <summary>The mask of a watermark's block count.</summary>
    internal const uint WatermarkBlocksMask = 0x1FFF;

    /// <summary>Bytes in one display buffer block.</summary>
    internal const int BufferBlockBytes = 512;

    // --- Watermarks before Skylake ---

    /// <summary>WM0_PIPE_ILK for pipe A: the level 0 watermarks, the cursor's in the low byte.</summary>
    internal const uint PipeWatermark0A = 0x4_5100;

    /// <summary>WM0_PIPE_ILK for pipe B.</summary>
    internal const uint PipeWatermark0B = 0x4_5104;

    /// <summary>WM0_PIPE_ILK for pipe C (Ivy Bridge on).</summary>
    internal const uint PipeWatermark0C = 0x4_5200;

    /// <summary>WM1_LP_ILK, the first of three low power watermarks; WM2 and WM3 follow every 4 bytes.</summary>
    internal const uint LowPowerWatermark1 = 0x4_5108;

    /// <summary>Low power watermark levels.</summary>
    internal const int LowPowerWatermarkCount = 3;

    /// <summary>WM_LP_ENABLE.</summary>
    internal const uint LowPowerWatermarkEnable = 0x8000_0000;

    /// <summary>WM0_PIPE_CURSOR_MASK or WM_LP_CURSOR_MASK: the cursor's watermark, in 64-byte lines.</summary>
    internal const uint WatermarkCursorMask = 0xFF;
}
