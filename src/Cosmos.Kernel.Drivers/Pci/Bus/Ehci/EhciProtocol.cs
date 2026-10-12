// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// Register offsets, bit fields, data structure layouts and timings of the
/// EHCI 1.0 host controller interface, as <see cref="EhciDriver"/> and
/// <see cref="EhciState"/> use them. The capability registers are byte
/// offsets from BAR0; the operational ones are relative to CAPLENGTH. The
/// link pointers, queue head and transfer descriptor fields are those of
/// the 32-bit data structures (EHCI 1.0 section 3).
/// </summary>
internal static class EhciProtocol
{
    // --- Capability registers (EHCI 1.0 section 2.2) ---

    /// <summary>CAPLENGTH, one byte: the operational registers start this far from BAR0.</summary>
    public const ulong CapLength = 0x00;

    /// <summary>HCIVERSION, one word: the interface version in BCD, 0x0100 for a 1.0 controller.</summary>
    public const ulong HciVersion = 0x02;

    /// <summary>HCSPARAMS: N_PORTS bits 3:0, PPC bit 4, PRR bit 7, N_PCC bits 11:8, N_CC bits 15:12 (section 2.2.3).</summary>
    public const ulong HcsParams = 0x04;

    /// <summary>HCSPARAMS.N_PORTS, bits 3:0: root ports.</summary>
    public const uint HcsParamsPortCountMask = 0xF;

    /// <summary>HCSPARAMS.PPC, bit 4: software switches port power.</summary>
    public const uint HcsParamsPortPowerControl = 1u << 4;

    /// <summary>HCSPARAMS.PRR, bit 7: HCSP-PORTROUTE names the companion of each port.</summary>
    public const uint HcsParamsPortRoutingRules = 1u << 7;

    /// <summary>HCSPARAMS.N_PCC starts at bit 8.</summary>
    public const int HcsParamsCompanionPortCountShift = 8;

    /// <summary>HCSPARAMS.N_PCC, bits 11:8: ports per companion controller.</summary>
    public const uint HcsParamsCompanionPortCountMask = 0xF;

    /// <summary>HCSPARAMS.N_CC starts at bit 12.</summary>
    public const int HcsParamsCompanionCountShift = 12;

    /// <summary>HCSPARAMS.N_CC, bits 15:12: companion controllers.</summary>
    public const uint HcsParamsCompanionCountMask = 0xF;

    /// <summary>HCCPARAMS: AC64 bit 0, PFL bit 1, ASP bit 2, IST bits 7:4, EECP bits 15:8 (section 2.2.4).</summary>
    public const ulong HccParams = 0x08;

    /// <summary>HCCPARAMS.AC64, bit 0: the data structures carry 64-bit addresses and CTRLDSSEGMENT is used.</summary>
    public const uint HccParamsAddressing64 = 1u << 0;

    /// <summary>HCCPARAMS.EECP starts at bit 8.</summary>
    public const int HccParamsExtendedCapabilitiesShift = 8;

    /// <summary>HCCPARAMS.EECP, bits 15:8: the PCI configuration offset of the first EHCI extended capability, 0 when none; below 0x40 is reserved.</summary>
    public const uint HccParamsExtendedCapabilitiesMask = 0xFF;

    /// <summary>HCSP-PORTROUTE, 60 bits from offset 0x0C: one nibble per port naming its companion, 0xF for none (section 2.2.5).</summary>
    public const ulong HcspPortRoute = 0x0C;

    /// <summary>Bytes of HCSP-PORTROUTE the driver reads: two dwords.</summary>
    public const ulong HcspPortRouteBytes = 8;

    /// <summary>Bits per HCSP-PORTROUTE nibble.</summary>
    public const int PortRouteNibbleBits = 4;

    /// <summary>One HCSP-PORTROUTE nibble.</summary>
    public const uint PortRouteNibbleMask = 0xF;

    /// <summary>The HCSP-PORTROUTE nibble of a port with no companion.</summary>
    public const uint PortRouteNoCompanion = 0xF;

    /// <summary>Bytes in a dword.</summary>
    public const ulong DwordBytes = 4;

    // --- Operational registers, relative to CAPLENGTH (EHCI 1.0 section 2.3) ---

    /// <summary>USBCMD: the command register (section 2.3.1).</summary>
    public const ulong UsbCmd = 0x00;

    /// <summary>USBCMD.RS, bit 0: run when set, stop when clear.</summary>
    public const uint UsbCmdRun = 1u << 0;

    /// <summary>USBCMD.HCRESET, bit 1: host controller reset, self-clearing.</summary>
    public const uint UsbCmdReset = 1u << 1;

    /// <summary>USBCMD.PSE, bit 4: the periodic schedule is enabled.</summary>
    public const uint UsbCmdPeriodicScheduleEnable = 1u << 4;

    /// <summary>USBCMD.ASE, bit 5: the asynchronous schedule is enabled.</summary>
    public const uint UsbCmdAsyncScheduleEnable = 1u << 5;

    /// <summary>USBCMD.IAAD, bit 6: the async advance doorbell; the controller clears it when it rings USBSTS.IAA.</summary>
    public const uint UsbCmdAsyncAdvanceDoorbell = 1u << 6;

    /// <summary>USBCMD.ITC starts at bit 16.</summary>
    public const int UsbCmdInterruptThresholdShift = 16;

    /// <summary>USBCMD.ITC value of one micro-frame: completions reach the CPU within 125 us.</summary>
    public const uint InterruptThresholdOneMicroFrame = 1;

    /// <summary>USBSTS: the status register; bits 5:0 are RW1C (section 2.3.2).</summary>
    public const ulong UsbSts = 0x04;

    /// <summary>USBSTS.USBINT, bit 0: a transfer with IOC completed, or a short packet landed.</summary>
    public const uint UsbStsInterrupt = 1u << 0;

    /// <summary>USBSTS.USBERRINT, bit 1: a transfer completed with an error.</summary>
    public const uint UsbStsErrorInterrupt = 1u << 1;

    /// <summary>USBSTS.PCD, bit 2: a port change bit was set.</summary>
    public const uint UsbStsPortChangeDetect = 1u << 2;

    /// <summary>USBSTS.HSE, bit 4: a host system error halted the controller.</summary>
    public const uint UsbStsHostSystemError = 1u << 4;

    /// <summary>USBSTS.IAA, bit 5: the async schedule advanced past the doorbell.</summary>
    public const uint UsbStsAsyncAdvance = 1u << 5;

    /// <summary>USBSTS.HCH, bit 12: the controller is halted.</summary>
    public const uint UsbStsHalted = 1u << 12;

    /// <summary>USBSTS bits 5:0, every RW1C status bit: written back to clear them all.</summary>
    public const uint UsbStsClearMask = 0x3F;

    /// <summary>
    /// The status bits the handler and the polled drain act on: USBINT,
    /// USBERRINT, PCD, HSE and IAA. A constant independent of USBINTR, so a
    /// polled controller, whose enables stay off, processes the same bits.
    /// </summary>
    public const uint InterruptMask = UsbStsInterrupt | UsbStsErrorInterrupt | UsbStsPortChangeDetect | UsbStsHostSystemError | UsbStsAsyncAdvance;

    /// <summary>USBINTR: the interrupt enables, same bits as USBSTS 5:0 (section 2.3.3).</summary>
    public const ulong UsbIntr = 0x08;

    /// <summary>FRINDEX: the frame list index, in micro-frames (section 2.3.4).</summary>
    public const ulong FrIndex = 0x0C;

    /// <summary>CTRLDSSEGMENT: the upper 32 bits of every data structure address when AC64 (section 2.3.5).</summary>
    public const ulong CtrlDsSegment = 0x10;

    /// <summary>PERIODICLISTBASE: the frame list's physical address, 4 KiB aligned (section 2.3.6).</summary>
    public const ulong PeriodicListBase = 0x14;

    /// <summary>ASYNCLISTADDR: the physical address of the async schedule's next queue head, 32-byte aligned (section 2.3.7).</summary>
    public const ulong AsyncListAddr = 0x18;

    /// <summary>CONFIGFLAG: bit 0 routes every port to this controller (section 2.3.8).</summary>
    public const ulong ConfigFlag = 0x40;

    /// <summary>CONFIGFLAG.CF, bit 0: ports are routed to the EHCI rather than the companions.</summary>
    public const uint ConfigFlagConfigured = 1u << 0;

    /// <summary>PORTSC of 1-based port n is at <c>0x44 + 4 * (n - 1)</c> (section 2.3.9).</summary>
    public const ulong PortScOffset = 0x44;

    /// <summary>Bytes from one PORTSC to the next.</summary>
    public const ulong PortScStride = 4;

    // --- PORTSC (EHCI 1.0 section 2.3.9) ---

    /// <summary>PORTSC.CCS, bit 0: a device is connected.</summary>
    public const uint PortConnected = 1u << 0;

    /// <summary>PORTSC.CSC, bit 1, RW1C: the connection changed.</summary>
    public const uint PortConnectChange = 1u << 1;

    /// <summary>PORTSC.PED, bit 2: the port is enabled; cleared by writing 0, set by the controller after a reset alone.</summary>
    public const uint PortEnabled = 1u << 2;

    /// <summary>PORTSC.PEDC, bit 3, RW1C: the controller disabled the port after an error.</summary>
    public const uint PortEnableChange = 1u << 3;

    /// <summary>PORTSC.OCC, bit 5, RW1C: over-current changed.</summary>
    public const uint PortOverCurrentChange = 1u << 5;

    /// <summary>PORTSC.Suspend, bit 7: the port is suspended.</summary>
    public const uint PortSuspend = 1u << 7;

    /// <summary>PORTSC.PR, bit 8: port reset asserted while set; software clears it to end the reset.</summary>
    public const uint PortReset = 1u << 8;

    /// <summary>PORTSC.Line Status starts at bit 10.</summary>
    public const int PortLineStatusShift = 10;

    /// <summary>PORTSC.Line Status, bits 11:10: the D+/D- state of a disabled port.</summary>
    public const uint PortLineStatusMask = 0x3;

    /// <summary>Line Status 01b, K-state: a low-speed device is connected and must go to a companion.</summary>
    public const uint PortLineStatusKState = 0x1;

    /// <summary>PORTSC.PP, bit 12: the port is powered.</summary>
    public const uint PortPower = 1u << 12;

    /// <summary>PORTSC.Port Owner, bit 13: the port is routed to a companion controller.</summary>
    public const uint PortOwner = 1u << 13;

    /// <summary>PORTSC.PIC, bits 15:14: the port indicator.</summary>
    public const uint PortIndicatorMask = 0x3u << 14;

    /// <summary>PORTSC.WKCNNT_E, WKDSCNNT_E and WKOC_E, bits 22:20: the wake enables.</summary>
    public const uint PortWakeMask = 0x7u << 20;

    /// <summary>RW1C change bits: CSC, PEDC and OCC.</summary>
    public const uint PortChangeBits = PortConnectChange | PortEnableChange | PortOverCurrentChange;

    /// <summary>
    /// PORTSC bits a write carries back as read, the RW and RWS ones: PP,
    /// PED, Suspend, Port Owner, the indicator and the wake enables. The
    /// change bits, PR, FPR and the test mode are zeroed so a write clears
    /// no change bit and starts nothing by accident; PED is carried back
    /// because writing it as 0 disables the port.
    /// </summary>
    public const uint PortPreserveMask = PortPower | PortEnabled | PortSuspend | PortOwner | PortIndicatorMask | PortWakeMask;

    // --- Link pointers and the frame list (EHCI 1.0 sections 3.1 and 3.3) ---

    /// <summary>Link pointer bit 0, T: the pointer is not valid, the list ends here.</summary>
    public const uint LinkTerminate = 1u << 0;

    /// <summary>Link pointer Typ bits 2:1 at 01b: the target is a queue head. The only type the driver ever writes.</summary>
    public const uint LinkTypeQueueHead = 0x2;

    /// <summary>Entries of the periodic frame list, the only size the driver programs (USBCMD.FLS 00b).</summary>
    public const int FrameListEntries = 1024;

    /// <summary>Bytes of the frame list: one page.</summary>
    public const int FrameListBytes = FrameListEntries * 4;

    // --- Queue head endpoint characteristics, dword 1 (EHCI 1.0 section 3.6.2) ---

    /// <summary>Device address, bits 6:0.</summary>
    public const uint QhDeviceAddressMask = 0x7F;

    /// <summary>Endpoint number starts at bit 8.</summary>
    public const int QhEndpointShift = 8;

    /// <summary>Endpoint number, bits 11:8.</summary>
    public const uint QhEndpointMask = 0xF;

    /// <summary>EPS starts at bit 12.</summary>
    public const int QhSpeedShift = 12;

    /// <summary>EPS 00b: full speed.</summary>
    public const uint QhSpeedFull = 0;

    /// <summary>EPS 01b: low speed.</summary>
    public const uint QhSpeedLow = 1;

    /// <summary>EPS 10b: high speed.</summary>
    public const uint QhSpeedHigh = 2;

    /// <summary>DTC, bit 14: the data toggle comes from each qTD rather than the overlay.</summary>
    public const uint QhDataToggleControl = 1u << 14;

    /// <summary>H, bit 15: the head of the reclamation list, the async schedule's anchor.</summary>
    public const uint QhHead = 1u << 15;

    /// <summary>Maximum packet length starts at bit 16.</summary>
    public const int QhMaxPacketShift = 16;

    /// <summary>Maximum packet length, bits 26:16.</summary>
    public const uint QhMaxPacketMask = 0x7FF;

    /// <summary>C, bit 27: a full or low-speed control endpoint, so the controller handles the toggle of the split status stage.</summary>
    public const uint QhControlEndpoint = 1u << 27;

    // --- Queue head endpoint capabilities, dword 2 (EHCI 1.0 section 3.6.2) ---

    /// <summary>Interrupt schedule mask (S-mask) starts at bit 0.</summary>
    public const int QhStartMaskShift = 0;

    /// <summary>Split completion mask (C-mask) starts at bit 8.</summary>
    public const int QhCompleteMaskShift = 8;

    /// <summary>Hub address of the transaction translator starts at bit 16.</summary>
    public const int QhHubAddressShift = 16;

    /// <summary>Port number on that hub starts at bit 23.</summary>
    public const int QhHubPortShift = 23;

    /// <summary>Mult starts at bit 30.</summary>
    public const int QhMultShift = 30;

    /// <summary>Mult, bits 31:30: transactions per micro-frame; 0 is undefined and never written.</summary>
    public const uint QhMultMask = 0x3u << QhMultShift;

    /// <summary>Mult 1: one transaction per micro-frame, what every non-periodic queue head carries.</summary>
    public const uint QhMultOne = 1u << QhMultShift;

    /// <summary>Mult 3, before the shift: the most transactions per micro-frame a high-speed periodic endpoint carries (section 3.6.2). wMaxPacketSize bits 12:11 at 11b are reserved (USB 2.0 section 9.6.6), so AdditionalTransactions + 1 is clamped to it.</summary>
    public const uint QhMultMax = 3;

    /// <summary>S-mask for a high-speed interrupt endpoint polled every micro-frame (bInterval 1).</summary>
    public const uint StartMaskEveryMicroFrame = 0xFF;

    /// <summary>S-mask for every other micro-frame (bInterval 2).</summary>
    public const uint StartMaskEveryOtherMicroFrame = 0x55;

    /// <summary>S-mask for every fourth micro-frame (bInterval 3).</summary>
    public const uint StartMaskEveryFourthMicroFrame = 0x11;

    /// <summary>S-mask for one start-split per frame, the first micro-frame.</summary>
    public const uint StartMaskFirstMicroFrame = 0x01;

    /// <summary>C-mask for a full or low-speed interrupt endpoint: complete-splits in micro-frames 2, 3 and 4 (section 4.12.2.1).</summary>
    public const uint CompleteMaskSplit = 0x1C;

    // --- Transfer descriptor token, dword 2 (EHCI 1.0 section 3.5.3) ---

    /// <summary>Status bit 7, Active: the controller owns the qTD.</summary>
    public const uint TokenActive = 1u << 7;

    /// <summary>Status bit 6, Halted: a serious error stopped the queue.</summary>
    public const uint TokenHalted = 1u << 6;

    /// <summary>Status bit 4, Babble Detected.</summary>
    public const uint TokenBabble = 1u << 4;

    /// <summary>Status bit 2, Missed Micro-Frame: a complete-split was missed.</summary>
    public const uint TokenMissedMicroFrame = 1u << 2;

    /// <summary>The token's status byte, bits 7:0.</summary>
    public const uint TokenStatusMask = 0xFF;

    /// <summary>PID Code starts at bit 8.</summary>
    public const int TokenPidShift = 8;

    /// <summary>PID Code 00b: OUT.</summary>
    public const uint PidOut = 0;

    /// <summary>PID Code 01b: IN.</summary>
    public const uint PidIn = 1;

    /// <summary>PID Code 10b: SETUP.</summary>
    public const uint PidSetup = 2;

    /// <summary>PID Code, bits 9:8, once shifted down.</summary>
    public const uint TokenPidMask = 0x3;

    /// <summary>CERR starts at bit 10.</summary>
    public const int TokenErrorCounterShift = 10;

    /// <summary>CERR 3: three transaction errors before the queue halts, the value the spec recommends.</summary>
    public const uint TokenErrorCounterMax = 3;

    /// <summary>CERR, bits 11:10, once shifted down: a transaction error decrements it, a STALL, Babble or a data buffer error does not (Table 3-16).</summary>
    public const uint TokenErrorCounterMask = 0x3;

    /// <summary>IOC, bit 15: raise USBINT when the qTD completes.</summary>
    public const uint TokenInterruptOnComplete = 1u << 15;

    /// <summary>Total Bytes to Transfer starts at bit 16.</summary>
    public const int TokenTotalBytesShift = 16;

    /// <summary>Total Bytes to Transfer, bits 30:16: what is left when the qTD retires.</summary>
    public const uint TokenTotalBytesMask = 0x7FFF;

    /// <summary>DT, bit 31: the data toggle, read from the qTD when the queue head has DTC set, kept in the overlay otherwise.</summary>
    public const uint TokenDataToggle = 1u << 31;

    // --- Transfer descriptor buffers (EHCI 1.0 section 3.5.4) ---

    /// <summary>Buffer pointer 0 carries the current offset in bits 11:0; the others are page aligned.</summary>
    public const uint QtdBufferOffsetMask = 0xFFF;

    /// <summary>Bytes one qTD moves at most when its buffer starts on a page boundary: five pages.</summary>
    public const int QtdMaxTransferBytes = 20480;

    // --- USB Legacy Support capability (EHCI 1.0 section 2.1.7) ---

    /// <summary>Capability id of USBLEGSUP.</summary>
    public const byte LegacySupportCapabilityId = 1;

    /// <summary>EHCI extended capabilities live at PCI configuration offsets from 0x40; a lower EECP means none.</summary>
    public const byte LegacyCapabilityMinOffset = 0x40;

    /// <summary>Byte 1 of a capability: the configuration offset of the next one, 0 ending the walk.</summary>
    public const byte CapabilityNextPointerOffset = 1;

    /// <summary>USBLEGSUP.HC BIOS Owned Semaphore, bit 16.</summary>
    public const uint LegacyBiosOwned = 1u << 16;

    /// <summary>The BIOS Owned byte lies 2 bytes into USBLEGSUP.</summary>
    public const byte LegacyBiosOwnedByteOffset = 2;

    /// <summary>The OS Owned byte lies 3 bytes into USBLEGSUP; written alone so the dword write cannot re-set the BIOS bit.</summary>
    public const byte LegacyOsOwnedByteOffset = 3;

    /// <summary>USBLEGCTLSTS lies 4 bytes into the capability; written 0 to turn every SMI source off.</summary>
    public const byte LegacyControlStatusOffset = 4;

    // --- Addresses, endpoints and schedule shape ---

    /// <summary>USB device addresses run 1 to 127; the table is indexed by address, 0 never in it.</summary>
    public const int AddressCount = 128;

    /// <summary>Highest endpoint id: endpoint 15 IN, with ids of <c>number * 2 + IN</c>.</summary>
    public const byte MaxEndpointId = 31;

    /// <summary>Interval heads of the periodic schedule: periods of 1 to 128 frames.</summary>
    public const int IntervalHeadCount = 8;

    /// <summary>The longest interval head's index, 128 frames.</summary>
    public const int MaxIntervalIndex = IntervalHeadCount - 1;

    /// <summary>bInterval of a high-speed endpoint is clamped to 1..16 (USB 2.0 section 9.6.6).</summary>
    public const int HighSpeedMaxInterval = 16;

    /// <summary>High-speed bInterval values up to 3 are micro-frame periods served from the every-frame head.</summary>
    public const int HighSpeedMicroFrameIntervalLimit = 3;

    /// <summary>High-speed bInterval 4 is a one-frame period: <c>k = bInterval - 4</c> selects the interval head.</summary>
    public const int HighSpeedFrameIntervalBase = 4;

    /// <summary>Longest full or low-speed interrupt period in frames the schedule holds.</summary>
    public const int FullSpeedMaxFrames = 128;

    /// <summary>The device descriptor's first 8 bytes carry bMaxPacketSize0 (USB 2.0 section 9.6.1).</summary>
    public const int DeviceDescriptorHeadLength = 8;

    /// <summary>Byte 7 of the device descriptor: bMaxPacketSize0.</summary>
    public const int MaxPacketSize0Offset = 7;

    /// <summary>Bytes of a SETUP packet.</summary>
    public const int SetupPacketLength = 8;

    /// <summary>SET_ADDRESS (USB 2.0 table 9-4); the kit's standard request enum carries no value for it.</summary>
    public const byte SetAddressRequest = 5;

    /// <summary>ENDPOINT_HALT feature selector (USB 2.0 table 9-6).</summary>
    public const ushort EndpointHaltFeature = 0;

    // --- Timings, in milliseconds unless the name says otherwise ---

    /// <summary>How long the firmware has to clear BIOS Owned once OS Owned is set.</summary>
    public const uint FirmwareHandoffTimeoutMs = 1000;

    /// <summary>How long the controller has to halt after RS is cleared, or to start after it is set (section 2.3.1: 16 micro-frames).</summary>
    public const uint HaltTimeoutMs = 100;

    /// <summary>How long HCRESET has to self-clear.</summary>
    public const uint ResetTimeoutMs = 1000;

    /// <summary>Port power-on to power-good when software switches port power.</summary>
    public const uint PortPowerSettleMs = 20;

    /// <summary>After the controller starts: the reset dropped every device, so they reconnect.</summary>
    public const uint PortConnectSettleMs = 200;

    /// <summary>TDRST: how long a port reset stays asserted (USB 2.0 section 7.1.7.5, at least 10 ms).</summary>
    public const uint PortResetAssertMs = 50;

    /// <summary>How long PR has to read 0 after software clears it (section 2.3.9: 2 ms).</summary>
    public const uint PortResetCompleteMs = 20;

    /// <summary>TRSTRCY: recovery after a reset before the device must answer (USB 2.0 section 7.1.7.5).</summary>
    public const uint PortResetRecoveryMs = 10;

    /// <summary>TATTDB: a connection must be stable this long before the port is reset (USB 2.0 section 7.1.7.3).</summary>
    public const uint ConnectDebounceMs = 100;

    /// <summary>Upper bound for a control transfer with a data stage (USB 2.0 section 9.2.6.4).</summary>
    public const uint TransferTimeoutMs = 5000;

    /// <summary>Budget for one bulk qTD: a flash drive may stall a write for seconds while it erases, and a USB disk spins up.</summary>
    public const uint BulkTimeoutMs = 10_000;

    /// <summary>Budget for the default address claim: a holder may spend a transfer budget on SET_ADDRESS plus its unlink and recovery delays.</summary>
    public const uint AddressClaimTimeoutMs = 2 * TransferTimeoutMs;

    /// <summary>How long the controller has to ring USBSTS.IAA after the doorbell (section 4.8.2: one traversal of the async schedule).</summary>
    public const uint DoorbellTimeoutMs = 100;

    /// <summary>How often the doorbell wait reads USBSTS.IAA and USBCMD.IAAD back while it waits: a controller may ring IAA without raising the line, or clear IAAD without setting IAA (Linux's ehci_iaa_watchdog, 10 ms).</summary>
    public const uint DoorbellWatchdogMs = 10;

    /// <summary>Frames waited after a queue head leaves the periodic schedule before it is written or reused (section 4.8.2 for the periodic list: the controller may be positioned on it for a frame).</summary>
    public const uint PeriodicUnlinkFrames = 2;

    /// <summary>SET_ADDRESS recovery interval before the next request (USB 2.0 section 9.2.6.3).</summary>
    public const uint SetAddressRecoveryMs = 2;

    /// <summary>Granularity of every claim loop and of the polled waits.</summary>
    public const uint PollMicroseconds = 10;

    /// <summary>How often the hot-plug thread drains the completions on a polled controller.</summary>
    public const uint PolledDrainIntervalMs = 20;

    /// <summary>How long the hot-plug thread waits for a port change before it looks at the ports again.</summary>
    public const uint HotPlugWaitMs = 1000;

    /// <summary>Recoveries of an interrupt pipe without a good transfer in between before it is given up.</summary>
    public const int MaxPipeRecoveries = 3;

    /// <summary>Transfers kept in flight on an interrupt pipe; a keyboard needs one, a few absorb bursts between interrupts.</summary>
    public const int MaxBuffers = 8;

    /// <summary>Transfer descriptor slots an interrupt pipe's memory set holds: one per buffer plus the dummy.</summary>
    public const int InterruptTransferSlots = MaxBuffers + 1;

    /// <summary>Transfer descriptor slots a bulk pipe's memory set holds: the transfer and the dummy, rotating.</summary>
    public const int BulkTransferSlots = 2;

    /// <summary>Transfer descriptor slots a device's memory set holds: SETUP, DATA, STATUS and the dummy, rotating.</summary>
    public const int ControlTransferSlots = 4;

    /// <summary>The bulk bounce buffer: 64 KiB, 64 KiB aligned, so every chunk starts on a page boundary.</summary>
    public const int BulkBufferBytes = 65536;

    /// <summary>Bytes of one bulk qTD: five pages from a page-aligned start.</summary>
    public const int BulkChunkBytes = QtdMaxTransferBytes;

    /// <summary>Microseconds in a millisecond, for <see cref="Cosmos.Kernel.HAL.DriverKit.DeviceBinding.Delay"/>.</summary>
    public const uint MicrosecondsPerMillisecond = 1000;
}
