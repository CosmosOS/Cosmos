// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Display.AmdDcn;

/// <summary>
/// The register map of the DCN 3.1.5 display engine (the Raphael and Granite
/// Ridge integrated GPUs), as Linux's amdgpu numbers it in
/// <c>dcn_3_1_5_offset.h</c> and <c>dcn_3_1_5_sh_mask.h</c>. A register is a
/// dword index into the MMIO BAR: its segment's base plus its offset, which
/// <see cref="Offset"/> turns into a byte offset. The segment bases are the
/// ones amdgpu's <c>dcn315_resource.c</c> hard-codes. The per-pipe blocks
/// (HUBP, HUBPREQ and the HUBP cursor share one stride, the DPP cursor has
/// its own) and the timing generators repeat at a fixed stride; every
/// constant below is instance 0's.
/// </summary>
internal static class AmdDcnRegisters
{
    // --- Layout ---

    /// <summary>DCN segment 1, in dwords: the VGA block.</summary>
    internal const uint Segment1 = 0x0000_00C0;

    /// <summary>DCN segment 2, in dwords: DCHUBBUB, the pipes and the timing generators.</summary>
    internal const uint Segment2 = 0x0000_34C0;

    /// <summary>Pipes (HUBP and DPP pairs) and timing generators the engine has.</summary>
    internal const int PipeCount = 4;

    /// <summary>Dwords between two pipes' HUBP, HUBPREQ and HUBP cursor registers.</summary>
    internal const uint PipeStride = 0xDC;

    /// <summary>Dwords between two pipes' DPP cursor registers.</summary>
    internal const uint DppStride = 0x16B;

    /// <summary>Dwords between two timing generators' registers.</summary>
    internal const uint TimingGeneratorStride = 0x80;

    /// <summary>Bytes the MMIO BAR must span: past the last timing generator's last register used here.</summary>
    internal const ulong WindowBytes = 0x2_0000;

    // --- NBIO (absolute dword index, as amdgpu_discovery.c reads it before any IP is known) ---

    /// <summary>RCC_CONFIG_MEMSIZE: the VRAM (on an APU, the carve-out) size in MiB; 0 or all ones when unknown.</summary>
    internal const uint ConfigMemorySize = 0x0DE3;

    // --- Segment 1 ---

    /// <summary>D1VGA_CONTROL: the legacy VGA engine of the first pipe.</summary>
    internal const uint VgaControl = 0x000C;

    /// <summary>D1VGA_MODE_ENABLE: the pipe scans out the VGA engine, not a surface.</summary>
    internal const uint VgaModeEnable = 0x0000_0001;

    // --- Segment 2, DCHUBBUB ---

    /// <summary>DCN_VM_FB_LOCATION_BASE: where VRAM starts in the GPU's address space, in 16 MiB units.</summary>
    internal const uint FbLocationBase = 0x0475;

    /// <summary>DCN_VM_FB_LOCATION_TOP: the last 16 MiB unit of VRAM in the GPU's address space.</summary>
    internal const uint FbLocationTop = 0x0476;

    /// <summary>DCN_VM_FB_OFFSET: where VRAM sits in system memory, in 16 MiB units; on an APU the carve-out.</summary>
    internal const uint FbOffset = 0x0477;

    /// <summary>The 24-bit field of the three FB location registers.</summary>
    internal const uint FbLocationMask = 0x00FF_FFFF;

    /// <summary>The shift from an FB location field to a GPU address.</summary>
    internal const int FbLocationShift = 24;

    // --- Segment 2, per pipe: HUBP ---

    /// <summary>HUBP_DCSURF_SURFACE_CONFIG: pixel format, rotation, mirroring.</summary>
    internal const uint SurfaceConfig = 0x05E5;

    /// <summary>SURFACE_PIXEL_FORMAT field of <see cref="SurfaceConfig"/>.</summary>
    internal const uint SurfacePixelFormatMask = 0x0000_007F;

    /// <summary>The ARGB8888 (and ABGR8888, swapped by the crossbar) pixel format: 32 bits per pixel.</summary>
    internal const uint PixelFormat32Bit = 8;

    /// <summary>ROTATION_ANGLE and H_MIRROR_EN fields of <see cref="SurfaceConfig"/>.</summary>
    internal const uint SurfaceRotationMirrorMask = 0x0000_0700;

    /// <summary>HUBP_DCSURF_TILING_CONFIG: the swizzle mode.</summary>
    internal const uint TilingConfig = 0x05E7;

    /// <summary>SW_MODE field of <see cref="TilingConfig"/>; 0 is linear.</summary>
    internal const uint SwizzleModeMask = 0x0000_001F;

    /// <summary>HUBP_DCSURF_PRI_VIEWPORT_START: the viewport's top left corner in the surface.</summary>
    internal const uint ViewportStart = 0x05E9;

    /// <summary>HUBP_DCSURF_PRI_VIEWPORT_DIMENSION: the viewport's width and height.</summary>
    internal const uint ViewportDimension = 0x05EA;

    /// <summary>The 14-bit X or width field, low half of the viewport registers.</summary>
    internal const uint ViewportFieldMask = 0x3FFF;

    /// <summary>The shift of the Y or height field of the viewport registers.</summary>
    internal const int ViewportHighShift = 16;

    /// <summary>HUBP_DCHUBP_CNTL: blanking and the timing generator the pipe follows.</summary>
    internal const uint HubpControl = 0x05F3;

    /// <summary>HUBP_BLANK_EN bit of <see cref="HubpControl"/>.</summary>
    internal const uint HubpBlankEnable = 0x0000_0001;

    /// <summary>HUBP_VTG_SEL field of <see cref="HubpControl"/>: the timing generator.</summary>
    internal const uint HubpTimingGeneratorMask = 0x0000_00F0;

    /// <summary>The shift of <see cref="HubpTimingGeneratorMask"/>.</summary>
    internal const int HubpTimingGeneratorShift = 4;

    /// <summary>HUBP_CLK_CNTL: the pipe's clock gate.</summary>
    internal const uint HubpClockControl = 0x05F4;

    /// <summary>HUBP_CLOCK_ENABLE bit of <see cref="HubpClockControl"/>.</summary>
    internal const uint HubpClockEnable = 0x0000_0001;

    // --- Segment 2, per pipe: HUBPREQ ---

    /// <summary>HUBPREQ_DCSURF_SURFACE_PITCH: the surface pitch in pixels, minus one.</summary>
    internal const uint SurfacePitch = 0x0607;

    /// <summary>PITCH field of <see cref="SurfacePitch"/>.</summary>
    internal const uint SurfacePitchMask = 0x0000_3FFF;

    /// <summary>HUBPREQ_VMID_SETTINGS_0: the GPU VM context the pipe's addresses go through.</summary>
    internal const uint VmidSettings = 0x0609;

    /// <summary>VMID field of <see cref="VmidSettings"/>.</summary>
    internal const uint VmidMask = 0x0000_000F;

    /// <summary>HUBPREQ_DCSURF_PRIMARY_SURFACE_ADDRESS: the surface's GPU address, low 32 bits; writing it latches the flip.</summary>
    internal const uint PrimarySurfaceAddress = 0x060A;

    /// <summary>HUBPREQ_DCSURF_PRIMARY_SURFACE_ADDRESS_HIGH: bits 47:32 of the surface's GPU address.</summary>
    internal const uint PrimarySurfaceAddressHigh = 0x060B;

    /// <summary>The 16-bit field of the HIGH address registers.</summary>
    internal const uint AddressHighMask = 0x0000_FFFF;

    /// <summary>HUBPREQ_DCSURF_SURFACE_CONTROL: compression and encryption of the surfaces.</summary>
    internal const uint SurfaceControl = 0x061A;

    /// <summary>PRIMARY_SURFACE_TMZ and PRIMARY_SURFACE_DCC_EN bits of <see cref="SurfaceControl"/>.</summary>
    internal const uint SurfaceTmzOrDccMask = 0x0000_0003;

    /// <summary>HUBPREQ_DCSURF_FLIP_CONTROL: how and when a new address takes effect.</summary>
    internal const uint FlipControl = 0x061B;

    /// <summary>SURFACE_UPDATE_LOCK bit of <see cref="FlipControl"/>: address writes are held.</summary>
    internal const uint FlipUpdateLock = 0x0000_0001;

    /// <summary>SURFACE_FLIP_TYPE bit of <see cref="FlipControl"/>: set flips at the next line, clear at the next vertical update.</summary>
    internal const uint FlipTypeImmediate = 0x0000_0002;

    /// <summary>SURFACE_FLIP_PENDING bit of <see cref="FlipControl"/>: a written address has not latched yet.</summary>
    internal const uint FlipPending = 0x0000_0100;

    /// <summary>HUBPREQ_DCSURF_SURFACE_EARLIEST_INUSE: low 32 bits of the address the pipe still fetches from.</summary>
    internal const uint EarliestInUse = 0x0625;

    /// <summary>HUBPREQ_DCSURF_SURFACE_EARLIEST_INUSE_HIGH: bits 47:32 of that address.</summary>
    internal const uint EarliestInUseHigh = 0x0626;

    /// <summary>HUBPREQ_CURSOR_SETTINGS: the cursor fetch schedule.</summary>
    internal const uint CursorSettings = 0x065C;

    /// <summary>CURSOR0_CHUNK_HDL_ADJUST of 3, the value amdgpu programs; CURSOR0_DST_Y_OFFSET stays 0.</summary>
    internal const uint CursorSettingsDefault = 3u << 8;

    // --- Segment 2, per pipe: HUBP cursor ---

    /// <summary>CURSOR0_CURSOR_CONTROL: enable, mode, pitch and fetch chunking.</summary>
    internal const uint CursorControl = 0x0678;

    /// <summary>CURSOR_ENABLE bit of <see cref="CursorControl"/>.</summary>
    internal const uint CursorEnable = 0x0000_0001;

    /// <summary>CURSOR_2X_MAGNIFY, CURSOR_MODE, CURSOR_PITCH and CURSOR_LINES_PER_CHUNK fields of <see cref="CursorControl"/>.</summary>
    internal const uint CursorAttributeMask = 0x1F03_0710;

    /// <summary>The shift of CURSOR_MODE in <see cref="CursorControl"/>.</summary>
    internal const int CursorModeShift = 8;

    /// <summary>The shift of CURSOR_PITCH in <see cref="CursorControl"/>.</summary>
    internal const int CursorPitchShift = 16;

    /// <summary>The shift of CURSOR_LINES_PER_CHUNK in <see cref="CursorControl"/>.</summary>
    internal const int CursorLinesPerChunkShift = 24;

    /// <summary>The colour mode of a premultiplied ARGB cursor, in both CURSOR_MODE and CUR0_MODE.</summary>
    internal const uint CursorModePremultipliedAlpha = 2;

    /// <summary>CURSOR0_CURSOR_SURFACE_ADDRESS: the cursor image's GPU address, low 32 bits.</summary>
    internal const uint CursorSurfaceAddress = 0x0679;

    /// <summary>CURSOR0_CURSOR_SURFACE_ADDRESS_HIGH: bits 47:32 of that address.</summary>
    internal const uint CursorSurfaceAddressHigh = 0x067A;

    /// <summary>CURSOR0_CURSOR_SIZE: height in the low half, width in the high half.</summary>
    internal const uint CursorSize = 0x067B;

    /// <summary>CURSOR0_CURSOR_POSITION: Y in the low half, X in the high half, 14 bits each.</summary>
    internal const uint CursorPosition = 0x067C;

    /// <summary>The 14-bit field of <see cref="CursorPosition"/>.</summary>
    internal const uint CursorPositionMask = 0x3FFF;

    /// <summary>CURSOR0_CURSOR_HOT_SPOT: Y in the low half, X in the high half, 8 bits each.</summary>
    internal const uint CursorHotSpot = 0x067D;

    /// <summary>The 8-bit field of <see cref="CursorHotSpot"/>.</summary>
    internal const uint CursorHotSpotMask = 0xFF;

    /// <summary>CURSOR0_CURSOR_DST_OFFSET: how far into the line the cursor fetch may start.</summary>
    internal const uint CursorDestinationOffset = 0x067F;

    /// <summary>The shift of the X (or width) half of the cursor size, position and hot spot registers.</summary>
    internal const int CursorXShift = 16;

    // --- Segment 2, per DPP: the cursor's colour path ---

    /// <summary>CNVC_CUR_CURSOR0_CONTROL: the DPP's half of the cursor enable and its mode.</summary>
    internal const uint DppCursorControl = 0x0CF1;

    /// <summary>CUR0_ENABLE bit of <see cref="DppCursorControl"/>.</summary>
    internal const uint DppCursorEnable = 0x0000_0001;

    /// <summary>CUR0_EXPANSION_MODE, CUR0_ROM_EN and CUR0_MODE fields of <see cref="DppCursorControl"/>.</summary>
    internal const uint DppCursorAttributeMask = 0x0000_007A;

    /// <summary>The shift of CUR0_MODE in <see cref="DppCursorControl"/>.</summary>
    internal const int DppCursorModeShift = 4;

    /// <summary>CNVC_CUR_CURSOR0_FP_SCALE_BIAS: the cursor's scale (low half) and bias (high half), half floats.</summary>
    internal const uint DppCursorScaleBias = 0x0CF4;

    /// <summary>A scale of 1.0 and a bias of 0, the SDR value amdgpu programs.</summary>
    internal const uint DppCursorScaleBiasIdentity = 0x0000_3C00;

    // --- Segment 2, per timing generator ---

    /// <summary>OTG_H_BLANK_START_END: horizontal blank start (low half) and end (high half).</summary>
    internal const uint OtgHorizontalBlank = 0x1B2B;

    /// <summary>OTG_V_BLANK_START_END: vertical blank start (low half) and end (high half).</summary>
    internal const uint OtgVerticalBlank = 0x1B36;

    /// <summary>The 15-bit field of the timing registers.</summary>
    internal const uint OtgTimingMask = 0x7FFF;

    /// <summary>The shift of the end half of the blank registers.</summary>
    internal const int OtgBlankEndShift = 16;

    /// <summary>OTG_CONTROL: the timing generator's master enable.</summary>
    internal const uint OtgControl = 0x1B41;

    /// <summary>OTG_MASTER_EN bit of <see cref="OtgControl"/>.</summary>
    internal const uint OtgMasterEnable = 0x0000_0001;

    /// <summary>OTG_STATUS_FRAME_COUNT: frames scanned out since the generator started.</summary>
    internal const uint OtgFrameCount = 0x1B4C;

    /// <summary>The 24-bit field of <see cref="OtgFrameCount"/>.</summary>
    internal const uint OtgFrameCountMask = 0x00FF_FFFF;

    /// <summary>OTG_MASTER_UPDATE_LOCK: double-buffered registers are held while set.</summary>
    internal const uint OtgMasterUpdateLock = 0x1B8B;

    /// <summary>OTG_MASTER_UPDATE_LOCK bit of <see cref="OtgMasterUpdateLock"/>.</summary>
    internal const uint OtgUpdateLocked = 0x0000_0001;

    /// <summary>The byte offset in the MMIO BAR of a register in a segment.</summary>
    /// <param name="segment">The segment base, in dwords.</param>
    /// <param name="register">The register's offset in the segment, in dwords.</param>
    internal static ulong Offset(uint segment, uint register)
    {
        return (ulong)(segment + register) * sizeof(uint);
    }
}
