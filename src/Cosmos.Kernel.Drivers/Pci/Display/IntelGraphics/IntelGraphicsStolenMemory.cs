// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// Stolen memory (the DSM): the RAM the firmware sets aside for the
/// integrated GPU below the top of memory, where the UEFI firmware puts the
/// surface it leaves on screen. The CPU never maps it, so it holds no CPU
/// cache lines, and the CPU reaches it only through the translation table
/// and the aperture; the display reads it directly. Its base, size and the
/// reserved part at its top the firmware keeps are read the way Linux's
/// <c>early-quirks.c</c> (<c>gen3_stolen_base</c>, <c>gen11_stolen_base</c>,
/// <c>gen6_stolen_size</c>, <c>gen8_stolen_size</c>,
/// <c>gen9_stolen_size</c>) and i915's <c>i915_gem_stolen.c</c> read them.
/// Read once, from the probe.
/// </summary>
internal sealed class IntelGraphicsStolenMemory
{
    /// <summary>Bytes in a mebibyte.</summary>
    private const ulong BytesPerMebibyte = 1024 * 1024;

    /// <summary>The physical address stolen memory starts at.</summary>
    internal ulong Base { get; }

    /// <summary>Bytes the firmware set aside.</summary>
    internal ulong Size { get; }

    /// <summary>The physical address the firmware's reserved part starts at: the end of what a driver may use.</summary>
    internal ulong UsableEnd { get; }

    private IntelGraphicsStolenMemory(ulong baseAddress, ulong size, ulong usableEnd)
    {
        Base = baseAddress;
        Size = size;
        UsableEnd = usableEnd;
    }

    /// <summary>Reads where stolen memory is, how large, and where its reserved top starts. Reads only.</summary>
    /// <param name="pci">The GPU's configuration space, for BDSM.</param>
    /// <param name="registers">BAR 0, for GEN6_STOLEN_RESERVED.</param>
    /// <param name="generation">The engine's generation, which picks the encodings.</param>
    /// <param name="graphicsControl">GGC, which gives the size.</param>
    /// <returns>What was read; <see cref="Size"/> is 0 when the firmware set none aside.</returns>
    internal static IntelGraphicsStolenMemory Read(PciAccess pci, RegisterWindow registers, IntelGraphicsGeneration generation, ushort graphicsControl)
    {
        ulong baseAddress;
        if (generation.HasWideStolenBase)
        {
            baseAddress = pci.ReadConfig32(IntelGraphicsRegisters.StolenBaseLowGen11) & IntelGraphicsRegisters.StolenBaseMask;
            baseAddress |= (ulong)pci.ReadConfig32(IntelGraphicsRegisters.StolenBaseHighGen11) << 32;
        }
        else
        {
            baseAddress = pci.ReadConfig32(IntelGraphicsRegisters.StolenBase) & IntelGraphicsRegisters.StolenBaseMask;
        }

        ulong size = SizeFromControl(generation.Platform, graphicsControl);
        ulong end = baseAddress + size;

        // The reserved part sits at the top. From Ice Lake on the register
        // is 64 bits and i915 takes its base without the enable bit.
        ulong usableEnd = end;
        uint reserved = registers.Read32(IntelGraphicsRegisters.StolenReserved);
        if ((reserved & IntelGraphicsRegisters.StolenReservedEnable) != 0 || generation.HasWideStolenBase)
        {
            ulong reservedBase = reserved & IntelGraphicsRegisters.StolenReservedBaseMask;
            if (generation.HasWideStolenBase)
            {
                reservedBase |= (ulong)registers.Read32(IntelGraphicsRegisters.StolenReserved + sizeof(uint)) << 32;
            }

            if (reservedBase > baseAddress && reservedBase < end)
            {
                usableEnd = reservedBase;
            }
        }

        return new IntelGraphicsStolenMemory(baseAddress, size, usableEnd);
    }

    /// <summary>Whether <paramref name="length"/> bytes at <paramref name="address"/> lie inside stolen memory.</summary>
    internal bool Contains(ulong address, ulong length)
    {
        return address >= Base && length <= Size && address - Base <= Size - length;
    }

    /// <summary>The size field of GGC, decoded for the platform.</summary>
    private static ulong SizeFromControl(IntelGraphicsPlatform platform, ushort graphicsControl)
    {
        if (platform < IntelGraphicsPlatform.Broadwell)
        {
            uint units = ((uint)graphicsControl >> IntelGraphicsRegisters.StolenSizeShiftGen6) & IntelGraphicsRegisters.StolenSizeMaskGen6;
            return units * 32 * BytesPerMebibyte;
        }

        uint code = ((uint)graphicsControl >> IntelGraphicsRegisters.StolenSizeShiftGen8) & IntelGraphicsRegisters.StolenSizeMaskGen8;
        if (platform == IntelGraphicsPlatform.Broadwell || code < IntelGraphicsRegisters.StolenSizeFirst4MiBCode)
        {
            return code * 32 * BytesPerMebibyte;
        }

        return ((code - IntelGraphicsRegisters.StolenSizeFirst4MiBCode) * 4 + 4) * BytesPerMebibyte;
    }
}
