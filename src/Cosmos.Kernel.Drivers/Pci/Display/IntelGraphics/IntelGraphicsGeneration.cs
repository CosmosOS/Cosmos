// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// What differs between the display engines <see cref="IntelGraphicsDriver"/>
/// covers, decided once from the platform: the register layout of the
/// primary plane, the number of pipes, the cursor's register block, the
/// graphics translation table's entry format and the display buffer's
/// watermarks. The values are the ones Linux's i915 uses for each platform
/// (<c>intel_display_device.c</c>, <c>intel_ggtt.c</c>, <c>intel_cursor.c</c>,
/// <c>skl_watermark.c</c>). Any context.
/// </summary>
internal sealed class IntelGraphicsGeneration
{
    /// <summary>The platform the device id named.</summary>
    internal IntelGraphicsPlatform Platform { get; }

    /// <summary>The platform's name for the log: the first and last processor families it covers.</summary>
    internal string Name { get; }

    /// <summary>The display engine version as i915 numbers it: 6, 7 (Ivy Bridge and Haswell), 8, 9, 11, 12 or 13.</summary>
    internal int DisplayVersion { get; }

    /// <summary>Pipes the engine has, each with a primary plane and a cursor.</summary>
    internal int PipeCount { get; }

    /// <summary>Whether the primary plane is a universal plane (Skylake on): <c>PLANE_CTL</c> encodings, its own size, position and display buffer.</summary>
    internal bool HasUniversalPlanes => DisplayVersion >= 9;

    /// <summary>Whether the plane's start in its surface is an x and y offset (Haswell on) rather than a linear byte offset.</summary>
    internal bool HasPlaneOffsetXY => Platform >= IntelGraphicsPlatform.Haswell;

    /// <summary>Whether a translation table entry is 64 bits (Broadwell on) rather than 32.</summary>
    internal bool HasWideGttEntries => Platform >= IntelGraphicsPlatform.Broadwell;

    /// <summary>Bytes BAR 0 spans: the registers in its lower half, the translation table in its upper half.</summary>
    internal ulong RegisterBarBytes => HasWideGttEntries ? 16UL * 1024 * 1024 : 4UL * 1024 * 1024;

    /// <summary>Bytes between two pipes' cursor registers: Sandy Bridge packs them, Ivy Bridge on gives each pipe its own block.</summary>
    internal uint CursorStride => Platform == IntelGraphicsPlatform.SandyBridge ? 0x40u : IntelGraphicsRegisters.PipeStride;

    /// <summary>Whether the cursor has <c>CUR_FBC_CTL</c>, whose height override this driver clears (Ivy Bridge to display 13).</summary>
    internal bool HasCursorFbcControl => DisplayVersion is >= 7 and <= 13;

    /// <summary>Whether the cursor takes the pipe's gamma and colour conversion enables from its control register (before Ice Lake).</summary>
    internal bool CursorFollowsPipeColor => DisplayVersion < 11;

    /// <summary>Whether the cursor needs its trickle feed disabled (Sandy Bridge and Ivy Bridge).</summary>
    internal bool CursorDisablesTrickleFeed => Platform is IntelGraphicsPlatform.SandyBridge or IntelGraphicsPlatform.IvyBridge;

    /// <summary>Whether the cursor needs one arbitration slot (Wa_22012358565, display 13).</summary>
    internal bool CursorNeedsArbitrationSlot => DisplayVersion == 13;

    /// <summary>The highest value the pipe's level 0 watermark takes for the cursor, before Skylake.</summary>
    internal uint CursorWatermark0Max => DisplayVersion >= 7 ? 63u : 31u;

    /// <summary>Watermark levels a universal plane and the cursor have: 6 where two registers went to the SAGV watermarks (display 13), 8 otherwise.</summary>
    internal int WatermarkLevelCount => HasSagvWatermarks ? 6 : 8;

    /// <summary>Whether planes and cursors have SAGV watermarks of their own (display 13).</summary>
    internal bool HasSagvWatermarks => DisplayVersion >= 13;

    /// <summary>The most lines a watermark level holds.</summary>
    internal uint MaxWatermarkLines => DisplayVersion >= 13 ? 255u : 31u;

    /// <summary>Whether the level 0 watermark's line count is used (Gemini Lake on); Skylake ignores it.</summary>
    internal bool Watermark0HasLines => DisplayVersion >= 10;

    /// <summary>Whether the stolen memory base is 64 bits wide, at <c>0xC0</c> (Ice Lake on), rather than at <c>0x5C</c>.</summary>
    internal bool HasWideStolenBase => DisplayVersion >= 11;

    /// <summary>Whether a translation table change needs the GT flush (before Ice Lake); later parts map the table uncached and need none.</summary>
    internal bool FlushesGttWrites => DisplayVersion < 11;

    /// <summary>The facts of <paramref name="platform"/>.</summary>
    /// <param name="platform">The platform the device id named.</param>
    internal IntelGraphicsGeneration(IntelGraphicsPlatform platform)
    {
        Platform = platform;
        Name = platform switch
        {
            IntelGraphicsPlatform.SandyBridge => "Sandy Bridge",
            IntelGraphicsPlatform.IvyBridge => "Ivy Bridge",
            IntelGraphicsPlatform.Haswell => "Haswell",
            IntelGraphicsPlatform.Broadwell => "Broadwell",
            IntelGraphicsPlatform.Skylake => "Skylake to Comet Lake",
            IntelGraphicsPlatform.IceLake => "Ice Lake",
            IntelGraphicsPlatform.TigerLake => "Tiger Lake to Raptor Lake-S",
            _ => "Alder Lake-P to Raptor Lake-P",
        };

        DisplayVersion = platform switch
        {
            IntelGraphicsPlatform.SandyBridge => 6,
            IntelGraphicsPlatform.IvyBridge or IntelGraphicsPlatform.Haswell => 7,
            IntelGraphicsPlatform.Broadwell => 8,
            IntelGraphicsPlatform.Skylake => 9,
            IntelGraphicsPlatform.IceLake => 11,
            IntelGraphicsPlatform.TigerLake => 12,
            _ => 13,
        };

        PipeCount = platform switch
        {
            IntelGraphicsPlatform.SandyBridge => 2,
            IntelGraphicsPlatform.TigerLake or IntelGraphicsPlatform.AlderLakeP => 4,
            _ => 3,
        };
    }
}
