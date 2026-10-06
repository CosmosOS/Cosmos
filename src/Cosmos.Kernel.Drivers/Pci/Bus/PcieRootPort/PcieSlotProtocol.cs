// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.PcieRootPort;

/// <summary>
/// The PCI Express capability's slot registers (PCI Express Base 7.5.3:
/// Link Capabilities, Slot Capabilities, Slot Control, Slot Status), their
/// bits, and the timing values <see cref="PcieRootPortDriver"/> and
/// <see cref="PcieRootPortState"/> use to follow a hot-plug slot. The
/// offsets are from the capability's start. Constants only; any context.
/// </summary>
internal static class PcieSlotProtocol
{
    // --- The capability ---

    /// <summary>The PCI Express capability id, what <c>PciAccess.FindCapability</c> takes.</summary>
    internal const byte ExpressCapabilityId = 0x10;

    // --- Register offsets from the capability ---

    /// <summary>Link Capabilities (32 bits).</summary>
    internal const ushort LinkCapabilitiesOffset = 0x0C;

    /// <summary>Slot Capabilities (32 bits).</summary>
    internal const ushort SlotCapabilitiesOffset = 0x14;

    /// <summary>Slot Control (16 bits).</summary>
    internal const ushort SlotControlOffset = 0x18;

    /// <summary>Slot Status (16 bits).</summary>
    internal const ushort SlotStatusOffset = 0x1A;

    // --- Slot Control ---

    /// <summary>Slot Control bit 0: Attention Button Pressed Enable.</summary>
    internal const ushort SlotControlAttentionButtonEnable = 0x0001;

    /// <summary>Slot Control bit 1: Power Fault Detected Enable.</summary>
    internal const ushort SlotControlPowerFaultDetectedEnable = 0x0002;

    /// <summary>Slot Control bit 2: MRL Sensor Changed Enable.</summary>
    internal const ushort SlotControlMrlSensorChangedEnable = 0x0004;

    /// <summary>Slot Control bit 3: Presence Detect Changed Enable.</summary>
    internal const ushort SlotControlPresenceChangedEnable = 0x0008;

    /// <summary>Slot Control bit 4: Command Completed Interrupt Enable.</summary>
    internal const ushort SlotControlCommandCompletedEnable = 0x0010;

    /// <summary>Slot Control bit 5: Hot-Plug Interrupt Enable.</summary>
    internal const ushort SlotControlHotPlugInterruptEnable = 0x0020;

    /// <summary>Slot Control Power Indicator Control (bits 9:8) set to on.</summary>
    internal const ushort SlotControlPowerIndicatorOn = 0x0100;

    /// <summary>Slot Control Power Indicator Control (bits 9:8) set to off.</summary>
    internal const ushort SlotControlPowerIndicatorOff = 0x0300;

    /// <summary>Slot Control Power Indicator Control field, bits 9:8.</summary>
    internal const ushort SlotControlPowerIndicatorMask = 0x0300;

    /// <summary>Slot Control bit 10: Power Controller Control, 0 for on, 1 for off.</summary>
    internal const ushort SlotControlPowerOff = 0x0400;

    /// <summary>Slot Control bit 12: Data Link Layer State Changed Enable.</summary>
    internal const ushort SlotControlDataLinkChangedEnable = 0x1000;

    /// <summary>The six enables of bits 0 to 5 and Data Link Layer State Changed Enable: what the probe clears first and the detach clears last.</summary>
    internal const ushort SlotControlEnableMask = 0x103F;

    // --- Slot Status ---

    /// <summary>Slot Status bit 0: Attention Button Pressed, write-one-to-clear.</summary>
    internal const ushort SlotStatusAttentionButton = 0x0001;

    /// <summary>Slot Status bit 1: Power Fault Detected, write-one-to-clear.</summary>
    internal const ushort SlotStatusPowerFault = 0x0002;

    /// <summary>Slot Status bit 2: MRL Sensor Changed, write-one-to-clear.</summary>
    internal const ushort SlotStatusMrlSensorChanged = 0x0004;

    /// <summary>Slot Status bit 3: Presence Detect Changed, write-one-to-clear.</summary>
    internal const ushort SlotStatusPresenceChanged = 0x0008;

    /// <summary>Slot Status bit 4: Command Completed, write-one-to-clear.</summary>
    internal const ushort SlotStatusCommandCompleted = 0x0010;

    /// <summary>Slot Status bit 6: Presence Detect State, set while a card is in the slot.</summary>
    internal const ushort SlotStatusPresent = 0x0040;

    /// <summary>Slot Status bit 8: Data Link Layer State Changed, write-one-to-clear.</summary>
    internal const ushort SlotStatusDataLinkChanged = 0x0100;

    /// <summary>The write-one-to-clear bits the slot thread reads and clears on every pass: the five change bits and Command Completed.</summary>
    internal const ushort SlotStatusEvents = 0x011F;

    // --- Slot Capabilities ---

    /// <summary>Slot Capabilities bit 1: Power Controller Present.</summary>
    internal const uint SlotCapabilityPowerController = 0x02;

    /// <summary>Slot Capabilities bit 4: Power Indicator Present.</summary>
    internal const uint SlotCapabilityPowerIndicator = 0x10;

    /// <summary>
    /// Slot Capabilities bit 18: No Command Completed Support. A port that
    /// sets it never raises Command Completed and takes the next Slot
    /// Control write at any time, so no wait follows a write.
    /// </summary>
    internal const uint SlotCapabilityNoCommandCompleted = 0x00040000;

    /// <summary>Shift down to the Physical Slot Number, Slot Capabilities bits 31:19.</summary>
    internal const int SlotCapabilityNumberShift = 19;

    // --- Link Capabilities ---

    /// <summary>Link Capabilities bit 20: Data Link Layer Link Active Reporting Capable.</summary>
    internal const uint LinkCapabilityDataLinkActiveReporting = 0x00100000;

    // --- Timing ---

    /// <summary>How long the slot thread waits for its event between passes, in milliseconds: the poll of last resort.</summary>
    internal const uint SlotWaitMilliseconds = 1000;

    /// <summary>How long a slot powered on is left before its function is described, in milliseconds.</summary>
    internal const uint PowerSettleMilliseconds = 100;

    /// <summary>Longest wait for Command Completed after a Slot Control write, in milliseconds.</summary>
    internal const uint CommandCompletedTimeoutMilliseconds = 1000;

    /// <summary>How long the Command Completed wait sleeps between reads of Slot Status, in milliseconds.</summary>
    internal const uint CommandPollMilliseconds = 10;
}
