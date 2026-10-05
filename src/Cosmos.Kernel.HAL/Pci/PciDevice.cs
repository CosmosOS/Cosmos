// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Pci.Enums;

namespace Cosmos.Kernel.HAL.Pci;

internal class PciDevice
{
    public readonly uint Bus;
    public readonly uint Slot;
    public readonly uint Function;

    public readonly uint Bar0;

    public readonly ushort VendorId;
    public readonly ushort DeviceId;

    public readonly ushort Status;

    public readonly byte RevisionId;
    public readonly byte ProgIf;
    public readonly byte Subclass;
    public readonly byte ClassCode;
    public readonly byte SecondaryBusNumber;

    public readonly bool DeviceExists;

    public readonly PciHeaderType HeaderType;
    public readonly PciBist Bist;
    public readonly PciInterruptPin InterruptPin;

    // Capability list walking (register offsets come from the Config enum).
    /// <summary>Offset of the next-capability pointer within a capability header.</summary>
    private const byte CapabilityNextPointerOffset = 1;
    /// <summary>Mask clearing the two reserved low bits of a capability pointer (dword aligned).</summary>
    private const byte CapabilityPointerMask = 0xFC;
    /// <summary>Upper bound on capability-list entries (the cap area spans 0x40..0xFF, 4-byte aligned).</summary>
    private const int MaxCapabilityEntries = 48;
    /// <summary>Status register bit 4: Capabilities List present.</summary>
    private const ushort StatusCapabilitiesListMask = 0x0010;
    /// <summary>Header type bits 6:0, the layout; bit 7 only flags a multi-function device.</summary>
    private const byte HeaderLayoutMask = 0x7F;

    // Type-0 header geometry (BAR slots start at Config.Bar0).
    /// <summary>Size in bytes of one BAR slot in configuration space.</summary>
    private const int BarSlotSizeBytes = 4;
    /// <summary>Number of Base Address Registers in a Type-0 (Normal) PCI header.</summary>
    private const int BarCount = 6;

    // BAR bit-field layout (PCI 3.0 §6.2.5.1). PciDevice owns BAR decoding;
    // these are public so BAR-manipulating consumers share one definition.
    /// <summary>BAR bit 0: set when the BAR maps I/O space instead of memory space.</summary>
    public const uint BarIoSpaceMask = 0x1;
    /// <summary>Mask selecting the address bits of a memory BAR (low 4 bits are flags).</summary>
    public const uint BarMemoryAddressMask = 0xFFFFFFF0;
    /// <summary>Shift down to the memory BAR type field (bits 2:1).</summary>
    public const int BarTypeShift = 1;
    /// <summary>Mask for the memory BAR type field after shifting.</summary>
    public const uint BarTypeMask = 0x3;
    /// <summary>Memory BAR type value indicating a 64-bit BAR.</summary>
    public const uint BarType64Bit = 0x2;
    /// <summary>Shift placing the upper BAR half into bits 63:32 of the combined address.</summary>
    public const int BarUpperHalfShift = 32;

    /// <summary>Vendor ID value read back from an absent device (bus reads as all ones).</summary>
    private const uint InvalidVendorId = 0xFF;
    /// <summary>Device ID value read back from an absent device (bus reads as all ones).</summary>
    private const uint InvalidDeviceId = 0xFFFF;

    /// <summary>Command register flags set by EnableMemory: I/O Space, Memory Space and Bus Master (bits 2:0).</summary>
    private const ushort CommandEnableFlags = (ushort)(PciCommand.Io | PciCommand.Memory | PciCommand.Master);

    public readonly PciBaseAddressBar[]? BaseAddressBar;

    public byte InterruptLine { get; private set; }

    public PciCommand Command
    {
        get => (PciCommand)ReadRegister16((byte)Config.Command);
        set => WriteRegister16((byte)Config.Command, (ushort)value);
    }

    /// <summary>
    /// Has this device been claimed by a driver
    /// </summary>
    public bool Claimed { get; set; }

    public PciDevice(uint bus, uint slot, uint function)
    {
        Serial.WriteString("[PciDevice] Init");
        Serial.WriteNumber(bus);
        Serial.WriteString(",");
        Serial.WriteNumber(slot);
        Serial.WriteString(",");
        Serial.WriteNumber(function);
        Serial.WriteString("\n");
        Bus = bus;
        Slot = slot;
        Function = function;

        VendorId = ReadRegister16((byte)Config.VendorId);
        DeviceId = ReadRegister16((byte)Config.DeviceId);

        Bar0 = ReadRegister32((byte)Config.Bar0);

        //Command = ReadRegister16((byte)Config.Command);
        //Status = ReadRegister16((byte)Config.Status);

        RevisionId = ReadRegister8((byte)Config.RevisionId);
        ProgIf = ReadRegister8((byte)Config.ProgIf);
        Subclass = ReadRegister8((byte)Config.SubClass);
        ClassCode = ReadRegister8((byte)Config.Class);
        SecondaryBusNumber = ReadRegister8((byte)Config.SecondaryBusNo);

        HeaderType = (PciHeaderType)(ReadRegister8((byte)Config.HeaderType) & HeaderLayoutMask);
        Bist = (PciBist)ReadRegister8((byte)Config.Bist);
        InterruptPin = (PciInterruptPin)ReadRegister8((byte)Config.InterruptPin);
        InterruptLine = ReadRegister8((byte)Config.InterruptLine);

        if ((uint)VendorId == InvalidVendorId && (uint)DeviceId == InvalidDeviceId)
        {
            DeviceExists = false;
        }
        else
        {
            DeviceExists = true;
        }

        if (HeaderType == PciHeaderType.Normal)
        {
            BaseAddressBar = new PciBaseAddressBar[BarCount];
            for (int i = 0; i < BarCount; i++)
            {
                BaseAddressBar[i] = new PciBaseAddressBar(ReadRegister32((byte)((byte)Config.Bar0 + i * BarSlotSizeBytes)));
            }
        }

        Serial.WriteString("[PciDevice] Init Done \n");
    }

    public void EnableDevice() => Command |= PciCommand.Master | PciCommand.Io | PciCommand.Memory;

    /// <summary>
    /// Returns the full physical base address of memory BAR
    /// <paramref name="barIndex"/>. For 64-bit BARs this combines the
    /// lower BAR with the immediately-following upper BAR; for 32-bit
    /// BARs it returns just the lower 32 bits. I/O BARs, out-of-range
    /// indices, and a 64-bit claim with no following BAR return 0.
    /// Both halves are read live from config space; the ctor-cached
    /// <see cref="BaseAddressBar"/> copy is an enumeration-time snapshot,
    /// and splicing it with a live upper half would combine two different
    /// addresses once a BAR is reprogrammed.
    /// </summary>
    public ulong GetBar64Address(int barIndex)
    {
        if (BaseAddressBar is null || barIndex < 0 || barIndex >= BaseAddressBar.Length)
        {
            return 0;
        }

        uint lower = ReadRegister32((byte)((byte)Config.Bar0 + barIndex * BarSlotSizeBytes));
        if ((lower & BarIoSpaceMask) == 1)
        {
            return 0; // I/O BAR
        }

        ulong addr = lower & BarMemoryAddressMask;
        if (((lower >> BarTypeShift) & BarTypeMask) == BarType64Bit)
        {
            // 64-bit BAR: the next BAR slot holds the upper half. A 64-bit
            // claim on the last slot is malformed: report 0 rather than a
            // lower-half-only address.
            if (barIndex + 1 >= BaseAddressBar.Length)
            {
                return 0;
            }

            ulong upper = ReadRegister32((byte)((byte)Config.Bar0 + (barIndex + 1) * BarSlotSizeBytes));
            addr |= upper << BarUpperHalfShift;
        }
        return addr;
    }

    /// <summary>
    /// Walks the PCI capabilities linked list and returns the config-space
    /// offset of the first capability whose ID matches <paramref name="capId"/>,
    /// or 0 if not found. The list is gated on Status[4] (Capabilities List)
    /// and the capability pointer at config offset 0x34 (only valid for
    /// Type-0 / Normal headers).
    /// </summary>
    public byte FindCapability(byte capId)
    {
        if (HeaderType != PciHeaderType.Normal)
        {
            return 0;
        }

        ushort status = ReadRegister16((byte)Config.Status);
        if ((status & StatusCapabilitiesListMask) == 0)
        {
            return 0;
        }

        byte offset = (byte)(ReadRegister8((byte)Config.CapabilityPointer) & CapabilityPointerMask);
        // The list is at most 48 entries long (the cap area is 0x40..0xFF).
        // Bound the walk so a malformed list cannot loop forever.
        for (int i = 0; offset != 0 && i < MaxCapabilityEntries; i++)
        {
            byte id = ReadRegister8(offset);
            if (id == capId)
            {
                return offset;
            }
            offset = (byte)(ReadRegister8((byte)(offset + CapabilityNextPointerOffset)) & CapabilityPointerMask);
        }
        return 0;
    }

    /// <summary>
    /// Get header type.
    /// </summary>
    /// <param name="bus">A bus.</param>
    /// <param name="slot">A slot.</param>
    /// <param name="function">A function.</param>
    /// <returns>ushort value.</returns>
    public static ushort GetHeaderType(ushort bus, ushort slot, ushort function)
    {
        return ReadConfig8(bus, slot, function, (byte)Config.HeaderType);
    }

    /// <summary>
    /// Get vendor ID.
    /// </summary>
    /// <param name="bus">A bus.</param>
    /// <param name="slot">A slot.</param>
    /// <param name="function">A function.</param>
    /// <returns>UInt16 value.</returns>
    public static ushort GetVendorId(ushort bus, ushort slot, ushort function)
    {
        return ReadConfig16(bus, slot, function, (byte)Config.VendorId);
    }

    #region IOReadWrite

    public byte ReadRegister8(byte aRegister)
    {
        return ReadConfig8((ushort)Bus, (ushort)Slot, (ushort)Function, aRegister);
    }

    public void WriteRegister8(byte aRegister, byte value)
    {
        WriteConfig8((ushort)Bus, (ushort)Slot, (ushort)Function, aRegister, value);
    }

    public ushort ReadRegister16(byte aRegister)
    {
        return ReadConfig16((ushort)Bus, (ushort)Slot, (ushort)Function, aRegister);
    }

    public void WriteRegister16(byte aRegister, ushort value)
    {
        WriteConfig16((ushort)Bus, (ushort)Slot, (ushort)Function, aRegister, value);
    }

    public uint ReadRegister32(byte aRegister)
    {
        return ReadConfig32((ushort)Bus, (ushort)Slot, (ushort)Function, aRegister);
    }

    public void WriteRegister32(byte aRegister, uint value)
    {
        WriteConfig32((ushort)Bus, (ushort)Slot, (ushort)Function, aRegister, value);
    }

    #endregion

    #region ConfigSpaceAccess

    // The six primitives every legacy path lands on: the mechanism is
    // PciConfigSpace.Legacy, chosen per architecture by SetEcamBase, so
    // the port and ECAM paths compile on both architectures.

    private static byte ReadConfig8(ushort bus, ushort slot, ushort func, byte offset) =>
        PciConfigSpace.Legacy.Read8((byte)bus, (byte)slot, (byte)func, offset);

    private static void WriteConfig8(ushort bus, ushort slot, ushort func, byte offset, byte value) =>
        PciConfigSpace.Legacy.Write8((byte)bus, (byte)slot, (byte)func, offset, value);

    private static ushort ReadConfig16(ushort bus, ushort slot, ushort func, byte offset) =>
        PciConfigSpace.Legacy.Read16((byte)bus, (byte)slot, (byte)func, offset);

    private static void WriteConfig16(ushort bus, ushort slot, ushort func, byte offset, ushort value) =>
        PciConfigSpace.Legacy.Write16((byte)bus, (byte)slot, (byte)func, offset, value);

    private static uint ReadConfig32(ushort bus, ushort slot, ushort func, byte offset) =>
        PciConfigSpace.Legacy.Read32((byte)bus, (byte)slot, (byte)func, offset);

    private static void WriteConfig32(ushort bus, ushort slot, ushort func, byte offset, uint value) =>
        PciConfigSpace.Legacy.Write32((byte)bus, (byte)slot, (byte)func, offset, value);

    #endregion

    /// <summary>
    /// Chooses the configuration mechanism of the legacy paths for this
    /// architecture (<see cref="PciConfigSpace.SelectLegacy"/>) from the
    /// ECAM base ACPI MCFG reports, 0 when the table is absent. Called by
    /// the HAL library initializer before the PCI scan.
    /// </summary>
    internal static void SetEcamBase(ulong physBase)
    {
        if (physBase != 0)
        {
            Serial.WriteString("[PciDevice] ECAM base from ACPI MCFG: 0x");
            Serial.WriteHex(physBase);
            Serial.WriteString("\n");
        }

        PciConfigSpace.SelectLegacy(physBase);
    }

    /// <summary>
    /// Enable memory.
    /// </summary>
    /// <param name="enable">bool value.</param>
    public void EnableMemory(bool enable)
    {
        ushort command = ReadRegister16((byte)Config.Command);

        ushort flags = CommandEnableFlags;

        if (enable)
        {
            command |= flags;
        }
        else
        {
            command &= (ushort)~flags;
        }

        WriteRegister16((byte)Config.Command, command);
    }

    public void EnableBusMaster(bool enable)
    {
        ushort command = ReadRegister16((byte)Config.Command);

        ushort flags = (ushort)PciCommand.Master;

        if (enable)
        {
            command |= flags;
        }
        else
        {
            command &= (ushort)~flags;
        }

        WriteRegister16((byte)Config.Command, command);
    }
}
