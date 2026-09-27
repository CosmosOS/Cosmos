// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Registers;

/// <summary>
/// The xHCI register block BAR0 maps, and the four register sets it holds:
/// capability, operational, runtime and doorbell (xHCI 1.2 §5.3-§5.6),
/// read and written through the binding's region, which orders every access
/// against the rings in DMA memory. 64-bit registers are written low dword
/// first, which every controller accepts. Any context: the accessors
/// allocate nothing.
/// </summary>
internal sealed class ControllerRegisters
{
    // Capability registers (xHCI 1.2 §5.3).
    private const ulong CapLengthOffset = 0x00;
    private const ulong HcsParams1Offset = 0x04;
    private const ulong HcsParams2Offset = 0x08;
    private const ulong HccParams1Offset = 0x10;
    private const ulong DoorbellArrayOffsetOffset = 0x14;
    private const ulong RuntimeOffsetOffset = 0x18;

    /// <summary>CAPLENGTH is the low byte of the first capability dword.</summary>
    private const uint CapLengthMask = 0xFF;

    /// <summary>DBOFF bits 1:0 and RTSOFF bits 4:0 are reserved.</summary>
    private const uint DoorbellOffsetMask = ~0x3u;
    private const uint RuntimeOffsetMask = ~0x1Fu;

    // Operational registers (xHCI 1.2 §5.4).
    private const ulong UsbCmdOffset = 0x00;
    private const ulong UsbStsOffset = 0x04;
    private const ulong PageSizeOffset = 0x08;
    private const ulong CrcrOffset = 0x18;
    private const ulong DcbaapOffset = 0x30;
    private const ulong ConfigOffset = 0x38;
    private const ulong PortRegisterSetOffset = 0x400;
    private const ulong PortRegisterSetStride = 0x10;

    // Interrupter register set 0 inside the runtime registers (xHCI 1.2 §5.5.2).
    private const ulong Interrupter0Offset = 0x20;
    private const ulong InterrupterSetSize = 0x20;
    private const ulong ImanOffset = 0x00;
    private const ulong ImodOffset = 0x04;
    private const ulong ErstszOffset = 0x08;
    private const ulong ErstbaOffset = 0x10;
    private const ulong ErdpOffset = 0x18;

    private const ulong DoorbellStride = 4;

    // HCSPARAMS1 fields.
    private const uint MaxSlotsMask = 0xFF;
    private const int MaxPortsShift = 24;
    private const uint MaxPortsMask = 0xFF;

    // HCSPARAMS2 scratchpad buffer count, split in a high (25:21) and low (31:27) part.
    private const int ScratchpadHighShift = 21;
    private const int ScratchpadLowShift = 27;
    private const uint ScratchpadPartMask = 0x1F;
    private const int ScratchpadHighPartBits = 5;

    // HCCPARAMS1 fields.
    private const uint AddressCapability64 = 1u << 0;
    private const uint ContextSize64 = 1u << 2;
    private const uint PortPowerControl = 1u << 3;
    private const int ExtendedCapabilitiesShift = 16;
    private const uint ExtendedCapabilitiesMask = 0xFFFF;
    private const int DwordShift = 2;

    private const int UpperDwordShift = 32;

    /// <summary>HCIVERSION is the upper half of the first capability dword.</summary>
    private const int HciVersionShift = 16;

    private readonly MmioRegion _region;
    private readonly ulong _operationalBase;
    private readonly ulong _runtimeBase;
    private readonly ulong _doorbellBase;

    /// <summary>
    /// Bytes from the start of BAR0 the driver touches: the whole block the
    /// region must cover, which the probe checks before it trusts the
    /// offsets the capability registers gave.
    /// </summary>
    internal ulong RequiredLength
    {
        get
        {
            ulong ports = _operationalBase + PortRegisterSetOffset + ((ulong)MaxPorts * PortRegisterSetStride);
            ulong runtime = _runtimeBase + Interrupter0Offset + InterrupterSetSize;
            ulong doorbells = _doorbellBase + ((ulong)(MaxSlots + 1) * DoorbellStride);
            return Math.Max(ports, Math.Max(runtime, doorbells));
        }
    }

    /// <summary>
    /// HCIVERSION, read through its dword (CAPLENGTH | HCIVERSION &lt;&lt; 16):
    /// some controllers, QEMU's among them, only answer dword reads of the
    /// capability registers, and return 0 for a 16-bit read at offset 2.
    /// </summary>
    internal ushort HciVersion => (ushort)(_region.Read32(CapLengthOffset) >> HciVersionShift);
    internal byte MaxSlots { get; }
    internal byte MaxPorts { get; }
    internal int MaxScratchpadBuffers { get; }

    /// <summary>HCCPARAMS1.AC64: the controller takes 64-bit DMA addresses.</summary>
    internal bool Is64BitCapable { get; }

    /// <summary>Size in bytes of one Slot/Endpoint/Input Control context (HCCPARAMS1.CSZ).</summary>
    internal int ContextSize { get; }

    /// <summary>HCCPARAMS1.PPC: software switches port power.</summary>
    internal bool HasPortPowerControl { get; }

    /// <summary>Byte offset from BAR0 of the first extended capability, 0 when there is none.</summary>
    internal ulong ExtendedCapabilitiesAddress { get; }

    internal uint UsbCmd
    {
        get => _region.Read32(_operationalBase + UsbCmdOffset);
        set => _region.Write32(_operationalBase + UsbCmdOffset, value);
    }

    internal uint UsbSts
    {
        get => _region.Read32(_operationalBase + UsbStsOffset);
        set => _region.Write32(_operationalBase + UsbStsOffset, value);
    }

    internal uint PageSize => _region.Read32(_operationalBase + PageSizeOffset);

    internal ulong Crcr
    {
        set => Write64(_operationalBase + CrcrOffset, value);
    }

    internal ulong Dcbaap
    {
        set => Write64(_operationalBase + DcbaapOffset, value);
    }

    internal uint Config
    {
        get => _region.Read32(_operationalBase + ConfigOffset);
        set => _region.Write32(_operationalBase + ConfigOffset, value);
    }

    internal uint Iman
    {
        get => _region.Read32(_runtimeBase + Interrupter0Offset + ImanOffset);
        set => _region.Write32(_runtimeBase + Interrupter0Offset + ImanOffset, value);
    }

    internal uint Imod
    {
        set => _region.Write32(_runtimeBase + Interrupter0Offset + ImodOffset, value);
    }

    internal uint Erstsz
    {
        set => _region.Write32(_runtimeBase + Interrupter0Offset + ErstszOffset, value);
    }

    internal ulong Erstba
    {
        set => Write64(_runtimeBase + Interrupter0Offset + ErstbaOffset, value);
    }

    internal ulong Erdp
    {
        set => Write64(_runtimeBase + Interrupter0Offset + ErdpOffset, value);
    }

    /// <summary>Reads the capability registers, which locate the other three sets.</summary>
    /// <param name="region">BAR0, as the kit mapped it.</param>
    internal ControllerRegisters(MmioRegion region)
    {
        _region = region;
        _operationalBase = region.Read32(CapLengthOffset) & CapLengthMask;
        _runtimeBase = region.Read32(RuntimeOffsetOffset) & RuntimeOffsetMask;
        _doorbellBase = region.Read32(DoorbellArrayOffsetOffset) & DoorbellOffsetMask;

        uint hcsParams1 = region.Read32(HcsParams1Offset);
        MaxSlots = (byte)(hcsParams1 & MaxSlotsMask);
        MaxPorts = (byte)((hcsParams1 >> MaxPortsShift) & MaxPortsMask);

        uint hcsParams2 = region.Read32(HcsParams2Offset);
        uint scratchpadHigh = (hcsParams2 >> ScratchpadHighShift) & ScratchpadPartMask;
        uint scratchpadLow = (hcsParams2 >> ScratchpadLowShift) & ScratchpadPartMask;
        MaxScratchpadBuffers = (int)((scratchpadHigh << ScratchpadHighPartBits) | scratchpadLow);

        uint hccParams1 = region.Read32(HccParams1Offset);
        Is64BitCapable = (hccParams1 & AddressCapability64) != 0;
        ContextSize = (hccParams1 & ContextSize64) != 0 ? 64 : 32;
        HasPortPowerControl = (hccParams1 & PortPowerControl) != 0;
        ExtendedCapabilitiesAddress = ((hccParams1 >> ExtendedCapabilitiesShift) & ExtendedCapabilitiesMask) << DwordShift;
    }

    /// <summary>Reads PORTSC of a 1-based root port.</summary>
    internal uint ReadPortSc(byte port) => _region.Read32(PortScOffset(port));

    /// <summary>Writes PORTSC of a 1-based root port.</summary>
    internal void WritePortSc(byte port, uint value) => _region.Write32(PortScOffset(port), value);

    /// <summary>Reads a dword of the extended capability list at a byte offset from BAR0.</summary>
    internal uint ReadCapability(ulong offset) => _region.Read32(offset);

    internal void WriteCapability(ulong offset, uint value) => _region.Write32(offset, value);

    /// <summary>
    /// Rings doorbell <paramref name="slotId"/> (0 is the command ring) with
    /// <paramref name="target"/> (0 for commands, the endpoint's DCI
    /// otherwise). The region's write barrier makes the ring and context
    /// stores reach memory before the controller is told to fetch them.
    /// </summary>
    internal void RingDoorbell(byte slotId, uint target) =>
        _region.Write32(_doorbellBase + (slotId * DoorbellStride), target);

    private ulong PortScOffset(byte port) =>
        _operationalBase + PortRegisterSetOffset + ((ulong)(port - 1) * PortRegisterSetStride);

    private void Write64(ulong offset, ulong value)
    {
        _region.Write32(offset, (uint)value);
        _region.Write32(offset + 4, (uint)(value >> UpperDwordShift));
    }
}
