// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.AmdDcn;

/// <summary>
/// One pipe of the display engine: the HUBP that fetches a surface (with its
/// HUBPREQ requester and its cursor) and the DPP that colours it. The probe
/// reads what the firmware left in it; the display flips its surface address
/// and programs its cursor, the way amdgpu's <c>hubp2_*</c> and
/// <c>dpp1_*</c>/<c>dpp3_*</c> functions do on DCN 3.1. Thread context.
/// </summary>
internal sealed class AmdDcnPipe
{
    /// <summary>The mask of a GPU address the display engine takes: 48 bits.</summary>
    internal const ulong AddressMask = 0x0000_FFFF_FFFF_FFFF;

    private readonly RegisterWindow _registers;
    private readonly uint _stride;
    private readonly uint _dppStride;

    /// <summary>Which pipe this is, 0 to <see cref="AmdDcnRegisters.PipeCount"/> exclusive.</summary>
    internal int Index { get; }

    /// <summary>Whether the pipe fetches a surface: its clock runs and it is not blanked.</summary>
    internal bool IsActive
    {
        get
        {
            bool clocked = (Read(AmdDcnRegisters.HubpClockControl) & AmdDcnRegisters.HubpClockEnable) != 0;
            bool blanked = (Read(AmdDcnRegisters.HubpControl) & AmdDcnRegisters.HubpBlankEnable) != 0;
            return clocked && !blanked;
        }
    }

    /// <summary>The timing generator the pipe follows: HUBP_VTG_SEL.</summary>
    internal int TimingGenerator => (int)((Read(AmdDcnRegisters.HubpControl) & AmdDcnRegisters.HubpTimingGeneratorMask) >> AmdDcnRegisters.HubpTimingGeneratorShift);

    /// <summary>SURFACE_PIXEL_FORMAT; <see cref="AmdDcnRegisters.PixelFormat32Bit"/> for 32-bit RGB.</summary>
    internal uint PixelFormat => Read(AmdDcnRegisters.SurfaceConfig) & AmdDcnRegisters.SurfacePixelFormatMask;

    /// <summary>Whether the surface is rotated or mirrored.</summary>
    internal bool IsRotatedOrMirrored => (Read(AmdDcnRegisters.SurfaceConfig) & AmdDcnRegisters.SurfaceRotationMirrorMask) != 0;

    /// <summary>SW_MODE; 0 for a linear surface.</summary>
    internal uint SwizzleMode => Read(AmdDcnRegisters.TilingConfig) & AmdDcnRegisters.SwizzleModeMask;

    /// <summary>Whether the surface is compressed (DCC) or encrypted (TMZ).</summary>
    internal bool IsCompressedOrEncrypted => (Read(AmdDcnRegisters.SurfaceControl) & AmdDcnRegisters.SurfaceTmzOrDccMask) != 0;

    /// <summary>The GPU VM context the pipe's addresses go through; 0 for physical addresses.</summary>
    internal uint Vmid => Read(AmdDcnRegisters.VmidSettings) & AmdDcnRegisters.VmidMask;

    /// <summary>The surface pitch in pixels.</summary>
    internal int PitchPixels => (int)(Read(AmdDcnRegisters.SurfacePitch) & AmdDcnRegisters.SurfacePitchMask) + 1;

    /// <summary>The viewport's left edge in the surface.</summary>
    internal int ViewportX => (int)(Read(AmdDcnRegisters.ViewportStart) & AmdDcnRegisters.ViewportFieldMask);

    /// <summary>The viewport's top edge in the surface.</summary>
    internal int ViewportY => (int)((Read(AmdDcnRegisters.ViewportStart) >> AmdDcnRegisters.ViewportHighShift) & AmdDcnRegisters.ViewportFieldMask);

    /// <summary>The viewport's width in pixels.</summary>
    internal int ViewportWidth => (int)(Read(AmdDcnRegisters.ViewportDimension) & AmdDcnRegisters.ViewportFieldMask);

    /// <summary>The viewport's height in lines.</summary>
    internal int ViewportHeight => (int)((Read(AmdDcnRegisters.ViewportDimension) >> AmdDcnRegisters.ViewportHighShift) & AmdDcnRegisters.ViewportFieldMask);

    /// <summary>The surface address the pipe is programmed with: the next frame once a flip is pending.</summary>
    internal ulong SurfaceAddress => ReadAddress(AmdDcnRegisters.PrimarySurfaceAddress, AmdDcnRegisters.PrimarySurfaceAddressHigh);

    /// <summary>The surface address the pipe still fetches from: the frame on screen.</summary>
    internal ulong EarliestInUseAddress => ReadAddress(AmdDcnRegisters.EarliestInUse, AmdDcnRegisters.EarliestInUseHigh);

    /// <summary>Whether a written surface address has not latched yet.</summary>
    internal bool IsFlipPending => (Read(AmdDcnRegisters.FlipControl) & AmdDcnRegisters.FlipPending) != 0;

    /// <summary>Whether surface address writes are held: SURFACE_UPDATE_LOCK.</summary>
    internal bool IsSurfaceUpdateLocked => (Read(AmdDcnRegisters.FlipControl) & AmdDcnRegisters.FlipUpdateLock) != 0;

    /// <summary>A view of pipe <paramref name="index"/> over the MMIO window.</summary>
    /// <param name="registers">The MMIO BAR.</param>
    /// <param name="index">The pipe, 0 to <see cref="AmdDcnRegisters.PipeCount"/> exclusive.</param>
    internal AmdDcnPipe(RegisterWindow registers, int index)
    {
        _registers = registers;
        Index = index;
        _stride = (uint)index * AmdDcnRegisters.PipeStride;
        _dppStride = (uint)index * AmdDcnRegisters.DppStride;
    }

    /// <summary>Makes a written surface address take effect at the next vertical update rather than mid-frame.</summary>
    internal void SetVerticalFlip()
    {
        uint control = Read(AmdDcnRegisters.FlipControl);
        Write(AmdDcnRegisters.FlipControl, control & ~AmdDcnRegisters.FlipTypeImmediate);
    }

    /// <summary>
    /// Programs the surface address: the high half, then the low half, whose
    /// write latches the pair for the next vertical update.
    /// </summary>
    /// <param name="address">The surface's GPU address.</param>
    internal void ProgramSurfaceAddress(ulong address)
    {
        Write(AmdDcnRegisters.PrimarySurfaceAddressHigh, (uint)(address >> 32) & AmdDcnRegisters.AddressHighMask);
        Write(AmdDcnRegisters.PrimarySurfaceAddress, (uint)address);
    }

    /// <summary>
    /// Programs the cursor image: its address, size, premultiplied ARGB mode,
    /// pitch and fetch chunking on the HUBP, and the mode and an identity
    /// scale on the DPP. The enables are left as they are.
    /// </summary>
    /// <param name="address">The image's GPU address.</param>
    /// <param name="width">Width in pixels, 1 to 256.</param>
    /// <param name="height">Height in pixels, 1 to 256.</param>
    /// <param name="pitchCode">CURSOR_PITCH: 0, 1 or 2 for 64, 128 or 256 pixels.</param>
    /// <param name="linesPerChunk">CURSOR_LINES_PER_CHUNK for the width.</param>
    internal void ProgramCursorImage(ulong address, int width, int height, uint pitchCode, uint linesPerChunk)
    {
        Write(AmdDcnRegisters.CursorSurfaceAddressHigh, (uint)(address >> 32) & AmdDcnRegisters.AddressHighMask);
        Write(AmdDcnRegisters.CursorSurfaceAddress, (uint)address);
        Write(AmdDcnRegisters.CursorSize, ((uint)width << AmdDcnRegisters.CursorXShift) | (uint)height);

        uint control = Read(AmdDcnRegisters.CursorControl) & ~AmdDcnRegisters.CursorAttributeMask;
        control |= AmdDcnRegisters.CursorModePremultipliedAlpha << AmdDcnRegisters.CursorModeShift;
        control |= pitchCode << AmdDcnRegisters.CursorPitchShift;
        control |= linesPerChunk << AmdDcnRegisters.CursorLinesPerChunkShift;
        Write(AmdDcnRegisters.CursorControl, control);
        Write(AmdDcnRegisters.CursorSettings, AmdDcnRegisters.CursorSettingsDefault);

        uint dppControl = ReadDpp(AmdDcnRegisters.DppCursorControl) & ~AmdDcnRegisters.DppCursorAttributeMask;
        dppControl |= AmdDcnRegisters.CursorModePremultipliedAlpha << AmdDcnRegisters.DppCursorModeShift;
        WriteDpp(AmdDcnRegisters.DppCursorControl, dppControl);
        WriteDpp(AmdDcnRegisters.DppCursorScaleBias, AmdDcnRegisters.DppCursorScaleBiasIdentity);
    }

    /// <summary>
    /// Programs where the cursor is and whether this pipe shows it. The
    /// position is the hot spot's, in surface coordinates; the image's top
    /// left corner lands at the position minus the hot spot.
    /// </summary>
    /// <param name="x">The hot spot's column, 0 or more.</param>
    /// <param name="y">The hot spot's row, 0 or more.</param>
    /// <param name="hotSpotX">The hot spot's column in the image, 0 to 255.</param>
    /// <param name="hotSpotY">The hot spot's row in the image, 0 to 255.</param>
    /// <param name="enabled">True to show the cursor on this pipe.</param>
    internal void ProgramCursorPosition(int x, int y, int hotSpotX, int hotSpotY, bool enabled)
    {
        uint position = (((uint)x & AmdDcnRegisters.CursorPositionMask) << AmdDcnRegisters.CursorXShift) | ((uint)y & AmdDcnRegisters.CursorPositionMask);
        uint hotSpot = (((uint)hotSpotX & AmdDcnRegisters.CursorHotSpotMask) << AmdDcnRegisters.CursorXShift) | ((uint)hotSpotY & AmdDcnRegisters.CursorHotSpotMask);
        Write(AmdDcnRegisters.CursorPosition, position);
        Write(AmdDcnRegisters.CursorHotSpot, hotSpot);
        Write(AmdDcnRegisters.CursorDestinationOffset, 0);
        SetCursorEnabled(enabled);
    }

    /// <summary>Shows or hides the cursor on this pipe: the HUBP's enable and the DPP's.</summary>
    /// <param name="enabled">True to show the cursor.</param>
    internal void SetCursorEnabled(bool enabled)
    {
        uint control = Read(AmdDcnRegisters.CursorControl);
        uint dppControl = ReadDpp(AmdDcnRegisters.DppCursorControl);
        if (enabled)
        {
            Write(AmdDcnRegisters.CursorControl, control | AmdDcnRegisters.CursorEnable);
            WriteDpp(AmdDcnRegisters.DppCursorControl, dppControl | AmdDcnRegisters.DppCursorEnable);
        }
        else
        {
            Write(AmdDcnRegisters.CursorControl, control & ~AmdDcnRegisters.CursorEnable);
            WriteDpp(AmdDcnRegisters.DppCursorControl, dppControl & ~AmdDcnRegisters.DppCursorEnable);
        }
    }

    private ulong ReadAddress(uint low, uint high)
    {
        ulong upper = Read(high) & AmdDcnRegisters.AddressHighMask;
        return (upper << 32) | Read(low);
    }

    private uint Read(uint register)
    {
        return _registers.Read32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, register + _stride));
    }

    private void Write(uint register, uint value)
    {
        _registers.Write32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, register + _stride), value);
    }

    private uint ReadDpp(uint register)
    {
        return _registers.Read32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, register + _dppStride));
    }

    private void WriteDpp(uint register, uint value)
    {
        _registers.Write32(AmdDcnRegisters.Offset(AmdDcnRegisters.Segment2, register + _dppStride), value);
    }
}
