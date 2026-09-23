// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// xHCI register block accessor. Wraps the BAR0 virtual base and locates
/// the four register sets it holds: capability, operational, runtime and
/// doorbell (xHCI 1.2 §5.3-§5.6). 64-bit registers are written low dword
/// first, which every controller accepts.
/// </summary>
internal sealed class XhciRegisters
{
    // Capability registers (xHCI 1.2 §5.3).
    private const ulong CapLengthOffset = 0x00;
    private const ulong HcsParams1Offset = 0x04;
    private const ulong HcsParams2Offset = 0x08;
    private const ulong HccParams1Offset = 0x10;
    private const ulong DoorbellArrayOffsetOffset = 0x14;
    private const ulong RuntimeOffsetOffset = 0x18;

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

    private const int UpperDwordShift = 32;

    /// <summary>HCIVERSION is the upper half of the first capability dword.</summary>
    private const int HciVersionShift = 16;

    private readonly ulong _capabilityBase;
    private readonly ulong _operationalBase;
    private readonly ulong _runtimeBase;
    private readonly ulong _doorbellBase;

    public XhciRegisters(ulong baseVirtAddress)
    {
        _capabilityBase = baseVirtAddress;
        _operationalBase = baseVirtAddress + Native.MMIO.Read8(baseVirtAddress + CapLengthOffset);
        _runtimeBase = baseVirtAddress + (Native.MMIO.Read32(baseVirtAddress + RuntimeOffsetOffset) & RuntimeOffsetMask);
        _doorbellBase = baseVirtAddress + (Native.MMIO.Read32(baseVirtAddress + DoorbellArrayOffsetOffset) & DoorbellOffsetMask);

        uint hcsParams1 = Native.MMIO.Read32(baseVirtAddress + HcsParams1Offset);
        MaxSlots = (byte)(hcsParams1 & MaxSlotsMask);
        MaxPorts = (byte)((hcsParams1 >> MaxPortsShift) & MaxPortsMask);

        uint hcsParams2 = Native.MMIO.Read32(baseVirtAddress + HcsParams2Offset);
        uint scratchpadHigh = (hcsParams2 >> ScratchpadHighShift) & ScratchpadPartMask;
        uint scratchpadLow = (hcsParams2 >> ScratchpadLowShift) & ScratchpadPartMask;
        MaxScratchpadBuffers = (int)((scratchpadHigh << ScratchpadHighPartBits) | scratchpadLow);

        uint hccParams1 = Native.MMIO.Read32(baseVirtAddress + HccParams1Offset);
        Is64BitCapable = (hccParams1 & AddressCapability64) != 0;
        ContextSize = (hccParams1 & ContextSize64) != 0 ? 64 : 32;
        HasPortPowerControl = (hccParams1 & PortPowerControl) != 0;
        ExtendedCapabilitiesAddress = ((hccParams1 >> ExtendedCapabilitiesShift) & ExtendedCapabilitiesMask) << 2;
    }

    public ulong CapabilityBase => _capabilityBase;

    /// <summary>Highest byte offset from BAR0 the driver touches, so the whole block can be mapped.</summary>
    public ulong MappedLength
    {
        get
        {
            ulong ports = _operationalBase + PortRegisterSetOffset + ((ulong)MaxPorts * PortRegisterSetStride);
            ulong runtime = _runtimeBase + Interrupter0Offset + InterrupterSetSize;
            ulong doorbells = _doorbellBase + ((ulong)(MaxSlots + 1) * DoorbellStride);
            return Math.Max(ports, Math.Max(runtime, doorbells)) - _capabilityBase;
        }
    }

    /// <summary>
    /// HCIVERSION, read through its dword (CAPLENGTH | HCIVERSION &lt;&lt; 16):
    /// some controllers, QEMU's among them, only answer dword reads of the
    /// capability registers, and return 0 for a 16-bit read at offset 2.
    /// </summary>
    public ushort HciVersion => (ushort)(Native.MMIO.Read32(_capabilityBase + CapLengthOffset) >> HciVersionShift);
    public byte MaxSlots { get; }
    public byte MaxPorts { get; }
    public int MaxScratchpadBuffers { get; }

    /// <summary>HCCPARAMS1.AC64: the controller takes 64-bit DMA addresses.</summary>
    public bool Is64BitCapable { get; }

    /// <summary>Size in bytes of one Slot/Endpoint/Input Control context (HCCPARAMS1.CSZ).</summary>
    public int ContextSize { get; }

    /// <summary>HCCPARAMS1.PPC: software switches port power.</summary>
    public bool HasPortPowerControl { get; }

    /// <summary>Byte offset from BAR0 of the first extended capability, 0 when there is none.</summary>
    public ulong ExtendedCapabilitiesAddress { get; }

    public uint UsbCmd
    {
        get => Native.MMIO.Read32(_operationalBase + UsbCmdOffset);
        set => Native.MMIO.Write32(_operationalBase + UsbCmdOffset, value);
    }

    public uint UsbSts
    {
        get => Native.MMIO.Read32(_operationalBase + UsbStsOffset);
        set => Native.MMIO.Write32(_operationalBase + UsbStsOffset, value);
    }

    public uint PageSize => Native.MMIO.Read32(_operationalBase + PageSizeOffset);

    public ulong Crcr
    {
        set => Write64(_operationalBase + CrcrOffset, value);
    }

    public ulong Dcbaap
    {
        set => Write64(_operationalBase + DcbaapOffset, value);
    }

    public uint Config
    {
        get => Native.MMIO.Read32(_operationalBase + ConfigOffset);
        set => Native.MMIO.Write32(_operationalBase + ConfigOffset, value);
    }

    public uint Iman
    {
        get => Native.MMIO.Read32(_runtimeBase + Interrupter0Offset + ImanOffset);
        set => Native.MMIO.Write32(_runtimeBase + Interrupter0Offset + ImanOffset, value);
    }

    public uint Imod
    {
        set => Native.MMIO.Write32(_runtimeBase + Interrupter0Offset + ImodOffset, value);
    }

    public uint Erstsz
    {
        set => Native.MMIO.Write32(_runtimeBase + Interrupter0Offset + ErstszOffset, value);
    }

    public ulong Erstba
    {
        set => Write64(_runtimeBase + Interrupter0Offset + ErstbaOffset, value);
    }

    public ulong Erdp
    {
        set => Write64(_runtimeBase + Interrupter0Offset + ErdpOffset, value);
    }

    /// <summary>Reads PORTSC of a 1-based root port.</summary>
    public uint ReadPortSc(byte port) => Native.MMIO.Read32(PortScAddress(port));

    /// <summary>Writes PORTSC of a 1-based root port.</summary>
    public void WritePortSc(byte port, uint value) => Native.MMIO.Write32(PortScAddress(port), value);

    /// <summary>Reads a dword of the extended capability list at a byte offset from BAR0.</summary>
    public uint ReadCapability(ulong offset) => Native.MMIO.Read32(_capabilityBase + offset);

    public void WriteCapability(ulong offset, uint value) => Native.MMIO.Write32(_capabilityBase + offset, value);

    /// <summary>
    /// Rings doorbell <paramref name="slotId"/> (0 is the command ring) with
    /// <paramref name="target"/> (0 for commands, the endpoint's DCI otherwise).
    /// </summary>
    public void RingDoorbell(byte slotId, uint target)
    {
        // Ring and context stores must reach memory before the controller
        // is told to fetch them (ARM64 does not order Normal vs Device).
        XhciDma.Barrier();
        Native.MMIO.Write32(_doorbellBase + (slotId * DoorbellStride), target);
    }

    private ulong PortScAddress(byte port) =>
        _operationalBase + PortRegisterSetOffset + ((ulong)(port - 1) * PortRegisterSetStride);

    private static void Write64(ulong address, ulong value)
    {
        Native.MMIO.Write32(address, (uint)value);
        Native.MMIO.Write32(address + 4, (uint)(value >> UpperDwordShift));
    }
}
