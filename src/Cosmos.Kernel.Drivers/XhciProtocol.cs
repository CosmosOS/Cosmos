// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Register offsets, bit fields, extended capability layouts and timings
/// of the xHCI 1.2 host controller interface, as <see cref="XhciDriver"/>
/// and <see cref="XhciState"/> use them. The capability registers are byte
/// offsets from BAR0; the operational ones are relative to CAPLENGTH, the
/// runtime ones to RTSOFF, the doorbells to DBOFF.
/// </summary>
internal static class XhciProtocol
{
    // --- Capability registers (xHCI 1.2 section 5.3) ---

    /// <summary>CAPLENGTH, one byte: the operational registers start this far from BAR0.</summary>
    public const ulong CapLength = 0x00;

    /// <summary>HCIVERSION is the upper half of the first capability dword: QEMU answers 0 to a 16-bit read at offset 2.</summary>
    public const int HciVersionShift = 16;

    /// <summary>HCSPARAMS1: MaxSlots in bits 7:0, MaxPorts in bits 31:24.</summary>
    public const ulong HcsParams1 = 0x04;

    /// <summary>HCSPARAMS1.MaxSlots, bits 7:0: device slots the controller supports.</summary>
    public const uint HcsParams1MaxSlotsMask = 0xFF;

    /// <summary>HCSPARAMS1.MaxPorts starts at bit 24.</summary>
    public const int HcsParams1MaxPortsShift = 24;

    /// <summary>HCSPARAMS1.MaxPorts, bits 31:24: root hub ports.</summary>
    public const uint HcsParams1MaxPortsMask = 0xFF;

    /// <summary>HCSPARAMS2: the scratchpad buffer count, split in a high part (bits 25:21) and a low part (bits 31:27).</summary>
    public const ulong HcsParams2 = 0x08;

    /// <summary>HCSPARAMS2: the high part of Max Scratchpad Buffers starts at bit 21.</summary>
    public const int ScratchpadHighShift = 21;

    /// <summary>HCSPARAMS2: the low part of Max Scratchpad Buffers starts at bit 27.</summary>
    public const int ScratchpadLowShift = 27;

    /// <summary>HCSPARAMS2: each part of Max Scratchpad Buffers is 5 bits.</summary>
    public const uint ScratchpadPartMask = 0x1F;

    /// <summary>HCSPARAMS2: the high part is shifted this far above the low one.</summary>
    public const int ScratchpadHighPartBits = 5;

    /// <summary>HCCPARAMS1: AC64 bit 0, CSZ bit 2, PPC bit 3, xECP in bits 31:16 as a dword offset.</summary>
    public const ulong HccParams1 = 0x10;

    /// <summary>HCCPARAMS1.AC64, bit 0: the controller takes 64-bit addresses.</summary>
    public const uint HccParams1Ac64 = 1u << 0;

    /// <summary>HCCPARAMS1.CSZ, bit 2: contexts are 64 bytes, not 32.</summary>
    public const uint HccParams1ContextSize64 = 1u << 2;

    /// <summary>HCCPARAMS1.PPC, bit 3: software switches port power.</summary>
    public const uint HccParams1PortPowerControl = 1u << 3;

    /// <summary>HCCPARAMS1.xECP starts at bit 16.</summary>
    public const int HccParams1ExtendedCapabilitiesShift = 16;

    /// <summary>HCCPARAMS1.xECP, bits 31:16: the first extended capability, as a dword offset from BAR0.</summary>
    public const uint HccParams1ExtendedCapabilitiesMask = 0xFFFF;

    /// <summary>DBOFF; bits 1:0 are reserved.</summary>
    public const ulong DoorbellOffset = 0x14;

    /// <summary>DBOFF: the doorbell array offset, its two reserved bits cleared.</summary>
    public const uint DoorbellOffsetMask = ~0x3u;

    /// <summary>RTSOFF; bits 4:0 are reserved.</summary>
    public const ulong RuntimeOffset = 0x18;

    /// <summary>RTSOFF: the runtime register offset, its five reserved bits cleared.</summary>
    public const uint RuntimeOffsetMask = ~0x1Fu;

    /// <summary>A dword offset becomes a byte offset.</summary>
    public const int DwordShift = 2;

    /// <summary>Bytes in a dword.</summary>
    public const ulong DwordBytes = 4;

    // --- Operational registers, relative to CAPLENGTH (xHCI 1.2 section 5.4) ---

    /// <summary>USBCMD: the command register.</summary>
    public const ulong UsbCmd = 0x00;

    /// <summary>USBCMD.R/S, bit 0: run when set, stop when clear.</summary>
    public const uint UsbCmdRun = 1u << 0;

    /// <summary>USBCMD.HCRST, bit 1: host controller reset, self-clearing.</summary>
    public const uint UsbCmdReset = 1u << 1;

    /// <summary>USBCMD.INTE, bit 2: interrupters may raise interrupts.</summary>
    public const uint UsbCmdInterrupterEnable = 1u << 2;

    /// <summary>USBSTS: the status register.</summary>
    public const ulong UsbSts = 0x04;

    /// <summary>USBSTS.HCH, bit 0: the controller is halted.</summary>
    public const uint UsbStsHalted = 1u << 0;

    /// <summary>USBSTS.EINT, bit 3, RW1C: an interrupter raised an interrupt.</summary>
    public const uint UsbStsEventInterrupt = 1u << 3;

    /// <summary>USBSTS.CNR, bit 11: the controller is not ready for register writes.</summary>
    public const uint UsbStsControllerNotReady = 1u << 11;

    /// <summary>PAGESIZE bit 0: the controller supports 4 KiB pages.</summary>
    public const ulong PageSize = 0x08;

    /// <summary>PAGESIZE bit 0: 4 KiB pages are supported.</summary>
    public const uint PageSize4KiB = 1u << 0;

    /// <summary>CRCR; RCS (bit 0) is the command ring's initial cycle state.</summary>
    public const ulong Crcr = 0x18;

    /// <summary>CRCR.RCS, bit 0: the consumer cycle state the command ring starts with.</summary>
    public const ulong CrcrRingCycleState = 1;

    /// <summary>DCBAAP, 64-bit: the device context base address array.</summary>
    public const ulong Dcbaap = 0x30;

    /// <summary>CONFIG: MaxSlotsEn, how many device slots are enabled, in bits 7:0.</summary>
    public const ulong Config = 0x38;

    /// <summary>PORTSC of 1-based port n is at <c>0x400 + 0x10 * (n - 1)</c>.</summary>
    public const ulong PortRegisterSetOffset = 0x400;

    /// <summary>Bytes from one port register set to the next.</summary>
    public const ulong PortRegisterSetStride = 0x10;

    // --- Runtime registers, relative to RTSOFF (xHCI 1.2 section 5.5) ---

    /// <summary>Interrupter register set 0 starts 0x20 into the runtime registers and spans 0x20 bytes.</summary>
    public const ulong Interrupter0Offset = 0x20;

    /// <summary>Bytes of one interrupter register set.</summary>
    public const ulong InterrupterSetSize = 0x20;

    /// <summary>IMAN: IP (bit 0) is RW1C, IE is bit 1.</summary>
    public const ulong Iman = 0x00;

    /// <summary>IMAN.IP, bit 0, RW1C: the interrupter has an interrupt pending.</summary>
    public const uint ImanInterruptPending = 1u << 0;

    /// <summary>IMAN.IE, bit 1: the interrupter may raise interrupts.</summary>
    public const uint ImanInterruptEnable = 1u << 1;

    /// <summary>IMOD, in 250 ns units: 160 is 40 us, the default moderation Linux uses.</summary>
    public const ulong Imod = 0x04;

    /// <summary>IMOD.IMODI value: 160 x 250 ns, 40 us between interrupts.</summary>
    public const uint InterruptModerationInterval = 160;

    /// <summary>ERSTSZ: how many segments the event ring segment table holds.</summary>
    public const ulong Erstsz = 0x08;

    /// <summary>ERSTBA, 64-bit: the event ring segment table; writing it arms the event ring.</summary>
    public const ulong Erstba = 0x10;

    /// <summary>ERDP; EHB (bit 3) is RW1C and cleared with every dequeue pointer update.</summary>
    public const ulong Erdp = 0x18;

    /// <summary>ERDP.EHB, bit 3, RW1C: the event handler is busy; cleared with every dequeue pointer update.</summary>
    public const ulong ErdpEventHandlerBusy = 1ul << 3;

    /// <summary>Doorbell n is at <c>DBOFF + 4 * n</c>; 0 is the command ring.</summary>
    public const ulong DoorbellStride = 4;

    // --- PORTSC (xHCI 1.2 section 5.4.8) ---

    /// <summary>PORTSC.CCS, bit 0: a device is connected.</summary>
    public const uint PortConnected = 1u << 0;

    /// <summary>PORTSC.PED, bit 1: the port is enabled; writing 1 disables it.</summary>
    public const uint PortEnabled = 1u << 1;

    /// <summary>PORTSC.PR, bit 4: port reset in progress; set to start one.</summary>
    public const uint PortReset = 1u << 4;

    /// <summary>PORTSC.PP, bit 9: the port is powered.</summary>
    public const uint PortPower = 1u << 9;

    /// <summary>PORTSC.Port Speed starts at bit 10.</summary>
    public const int PortSpeedShift = 10;

    /// <summary>PORTSC.Port Speed, bits 13:10: the protocol speed id, which is a <c>UsbSpeed</c> value.</summary>
    public const uint PortSpeedMask = 0xF;

    /// <summary>PORTSC.CSC, bit 17, RW1C: the connection changed.</summary>
    public const uint PortConnectChange = 1u << 17;

    /// <summary>PORTSC.PEC, bit 18, RW1C: the port was disabled by the controller after an error.</summary>
    public const uint PortEnableChange = 1u << 18;

    /// <summary>PORTSC.WRC, bit 19, RW1C: a warm reset completed.</summary>
    public const uint PortWarmResetChange = 1u << 19;

    /// <summary>PORTSC.PRC, bit 21, RW1C: a reset completed.</summary>
    public const uint PortResetChange = 1u << 21;

    /// <summary>PORTSC.WPR, bit 31: start a warm reset (USB 3 ports).</summary>
    public const uint PortWarmReset = 1u << 31;

    /// <summary>RW1C change bits 23:17: CSC, PEC, WRC, OCC, PRC, PLC, CEC.</summary>
    public const uint PortChangeBits = 0x7Fu << 17;

    /// <summary>
    /// PORTSC bits a write carries back as read, the RO and RWS ones (Linux
    /// xhci_port_state_to_neutral). PED is left out on purpose: writing it
    /// as 1 disables the port.
    /// </summary>
    public const uint PortPreserveMask =
        (1u << 0) | (1u << 3) | (0xFu << 10) | (1u << 30)
        | (0xFu << 5) | (1u << 9) | (0x3u << 14) | (0x7u << 25);

    // --- Extended capabilities (xHCI 1.2 section 7) ---

    /// <summary>Capability id in bits 7:0; the next pointer in bits 15:8 as a dword offset, 0 ending the list.</summary>
    public const uint ExtendedCapabilityIdMask = 0xFF;

    /// <summary>The next capability pointer starts at bit 8.</summary>
    public const int ExtendedCapabilityNextShift = 8;

    /// <summary>The next capability pointer, bits 15:8, as a dword offset from this capability; 0 ends the list.</summary>
    public const uint ExtendedCapabilityNextMask = 0xFF;

    /// <summary>USB Legacy Support (section 7.1): BIOS Owned bit 16, OS Owned bit 24, USBLEGCTLSTS at +4.</summary>
    public const byte LegacySupportCapabilityId = 1;

    /// <summary>USBLEGSUP.HC BIOS Owned Semaphore, bit 16.</summary>
    public const uint LegacyBiosOwned = 1u << 16;

    /// <summary>USBLEGSUP.HC OS Owned Semaphore, bit 24.</summary>
    public const uint LegacyOsOwned = 1u << 24;

    /// <summary>USBLEGCTLSTS lies 4 bytes into the capability.</summary>
    public const ulong LegacyControlStatusOffset = 4;

    /// <summary>USBLEGCTLSTS bits kept on write, the RsvdP ones; every SMI enable is cleared (Linux XHCI_LEGACY_DISABLE_SMI).</summary>
    public const uint LegacyDisableSmiMask = (0x7u << 1) | (0xFFu << 5) | (0x7u << 17);

    /// <summary>USBLEGCTLSTS bits 31:29, RW1C SMI event flags.</summary>
    public const uint LegacySmiEvents = 0x7u << 29;

    /// <summary>Supported Protocol (section 7.2): the major revision in bits 31:24; at +8 the Compatible Port Offset (bits 7:0) and Count (bits 15:8).</summary>
    public const byte SupportedProtocolCapabilityId = 2;

    /// <summary>The protocol's major revision (2 or 3) starts at bit 24 of the first dword.</summary>
    public const int SupportedProtocolRevisionShift = 24;

    /// <summary>The port range dword lies 8 bytes into the capability.</summary>
    public const ulong SupportedProtocolPortsOffset = 8;

    /// <summary>Compatible Port Offset, bits 7:0 of the port range dword: the first 1-based port of the range.</summary>
    public const uint CompatiblePortOffsetMask = 0xFF;

    /// <summary>Compatible Port Count starts at bit 8 of the port range dword.</summary>
    public const int CompatiblePortCountShift = 8;

    /// <summary>Compatible Port Count, bits 15:8 of the port range dword.</summary>
    public const uint CompatiblePortCountMask = 0xFF;

    /// <summary>The major revision a USB 3 port range reports.</summary>
    public const byte UsbMajorRevision3 = 3;

    // --- Timings, in milliseconds unless the name says otherwise ---

    /// <summary>How long the firmware has to clear BIOS Owned once OS Owned is set.</summary>
    public const uint FirmwareHandoffTimeoutMs = 1000;

    /// <summary>How long the controller has to halt after Run is cleared, or to start after it is set (xHCI 1.2 section 5.4.1: 16 ms).</summary>
    public const uint HaltTimeoutMs = 100;

    /// <summary>How long HCRST and CNR have to clear.</summary>
    public const uint ResetTimeoutMs = 1000;

    /// <summary>Some controllers hang when read within 1 ms of HCRST; Linux's xhci_reset waits the same.</summary>
    public const uint ResetSettleMs = 1;

    /// <summary>Port power-on to power-good when software switches port power.</summary>
    public const uint PortPowerSettleMs = 20;

    /// <summary>After the controller starts: the reset dropped every device, so they reconnect and USB 3 links retrain.</summary>
    public const uint PortConnectSettleMs = 200;

    /// <summary>How long a port reset has to complete.</summary>
    public const uint PortResetTimeoutMs = 500;

    /// <summary>How long a USB 3 port has to enable on its own after a connect before a warm reset is tried.</summary>
    public const uint LinkTrainingTimeoutMs = 500;

    /// <summary>TRSTRCY: recovery after a reset before the device must answer (USB 2.0 section 7.1.7.5).</summary>
    public const uint PortResetRecoveryMs = 10;

    /// <summary>TATTDB: a connection must be stable this long before the port is reset (USB 2.0 section 7.1.7.3).</summary>
    public const uint ConnectDebounceMs = 100;

    /// <summary>Budget for one command; Address Device includes the device's SET_ADDRESS (Linux XHCI_CMD_DEFAULT_TIMEOUT).</summary>
    public const uint CommandTimeoutMs = 5000;

    /// <summary>Upper bound for a control transfer with a data stage (USB 2.0 section 9.2.6.4).</summary>
    public const uint TransferTimeoutMs = 5000;

    /// <summary>Budget for one bulk TRB: a flash drive may stall a write for seconds while it erases, and a USB disk spins up.</summary>
    public const uint BulkTimeoutMs = 10_000;

    /// <summary>SET_ADDRESS recovery interval before the next request (USB 2.0 section 9.2.6.3).</summary>
    public const uint SetAddressRecoveryMs = 2;

    /// <summary>Granularity of every claim loop and of the polled waits.</summary>
    public const uint PollMicroseconds = 10;

    /// <summary>How often the hot-plug thread drains the event ring on a polled controller.</summary>
    public const uint PolledDrainIntervalMs = 20;

    /// <summary>How long the hot-plug thread waits for a port change before it looks at the ports again.</summary>
    public const uint HotPlugWaitMs = 1000;

    /// <summary>Recoveries of an interrupt pipe without a good transfer in between before it is given up.</summary>
    public const int MaxPipeRecoveries = 3;

    /// <summary>Controllers from 0.96 take a hub's Slot Context through Configure Endpoint, earlier ones through Evaluate Context (Linux xhci_update_hub_device).</summary>
    public const ushort HubConfigureEndpointMinVersion = 0x0096;

    /// <summary>Average TRB length the spec recommends for control endpoints (xHCI 1.2 section 4.14.1.1).</summary>
    public const ushort ControlAverageTrbLength = 8;

    /// <summary>Average TRB length the spec recommends for bulk endpoints (xHCI 1.2 section 4.14.1.1).</summary>
    public const ushort BulkAverageTrbLength = 3072;

    /// <summary>Transfers kept in flight on an interrupt pipe; a keyboard needs one, a few absorb bursts between interrupts.</summary>
    public const int MaxBuffers = 8;

    /// <summary>The bulk bounce buffer: 64 KiB, 64 KiB aligned, so no TRB ever splits at a 64 KiB boundary (xHCI 1.2 section 6.4.1.1).</summary>
    public const int BulkBufferBytes = 65536;

    /// <summary>Microseconds in a millisecond, for <see cref="Cosmos.Kernel.HAL.DriverKit.DeviceBinding.Delay"/>.</summary>
    public const uint MicrosecondsPerMillisecond = 1000;
}
