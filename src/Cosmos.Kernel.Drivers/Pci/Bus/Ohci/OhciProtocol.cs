// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Register offsets, bit fields, descriptor field layouts and timings of
/// the OpenHCI 1.0a host controller interface, as <see cref="OhciDriver"/>
/// and <see cref="OhciState"/> use them. Every register is a 32-bit byte
/// offset from BAR0.
/// </summary>
internal static class OhciProtocol
{
    // --- Operational registers (OHCI 1.0a section 7) ---

    /// <summary>HcRevision: the interface revision in bits 7:0 (section 7.1.1).</summary>
    public const ulong HcRevision = 0x00;

    /// <summary>HcRevision bits 7:0 hold the revision, 0x10 for 1.0 and 1.0a.</summary>
    public const uint RevisionMask = 0xFF;

    /// <summary>The one revision the driver drives.</summary>
    public const byte SupportedRevision = 0x10;

    /// <summary>HcControl: list enables, the functional state and the firmware bits (section 7.1.2).</summary>
    public const ulong HcControl = 0x04;

    /// <summary>HcControl.CBSR, bits 1:0: control EDs served per bulk ED; 3 is four to one.</summary>
    public const uint ControlBulkRatio4To1 = 0x3;

    /// <summary>HcControl.PLE, bit 2: the periodic list runs.</summary>
    public const uint PeriodicListEnable = 1u << 2;

    /// <summary>HcControl.CLE, bit 4: the control list runs.</summary>
    public const uint ControlListEnable = 1u << 4;

    /// <summary>HcControl.BLE, bit 5: the bulk list runs.</summary>
    public const uint BulkListEnable = 1u << 5;

    /// <summary>HcControl.HCFS, bits 7:6: the functional state.</summary>
    public const uint FunctionalStateMask = 0x3u << 6;

    /// <summary>HCFS = USBReset.</summary>
    public const uint FunctionalStateReset = 0x0u << 6;

    /// <summary>HCFS = USBResume.</summary>
    public const uint FunctionalStateResume = 0x1u << 6;

    /// <summary>HCFS = USBOperational.</summary>
    public const uint FunctionalStateOperational = 0x2u << 6;

    /// <summary>HCFS = USBSuspend, the state a software reset leaves.</summary>
    public const uint FunctionalStateSuspend = 0x3u << 6;

    /// <summary>HcControl.IR, bit 8: interrupts go to the SMI handler, the firmware owns the controller (section 5.1.1.3.2).</summary>
    public const uint InterruptRouting = 1u << 8;

    /// <summary>HcControl.RWC, bit 9: remote wakeup is connected, written by the firmware and preserved by every HcControl write.</summary>
    public const uint RemoteWakeupConnected = 1u << 9;

    /// <summary>HcCommandStatus: the reset and the list-filled bits; a write ORs them in (section 7.1.3).</summary>
    public const ulong HcCommandStatus = 0x08;

    /// <summary>HcCommandStatus.HCR, bit 0: software reset, self-clearing within 10 us.</summary>
    public const uint HostControllerReset = 1u << 0;

    /// <summary>HcCommandStatus.CLF, bit 1: the control list has a TD to run; set after every queue.</summary>
    public const uint ControlListFilled = 1u << 1;

    /// <summary>HcCommandStatus.BLF, bit 2: the bulk list has a TD to run; set after every queue.</summary>
    public const uint BulkListFilled = 1u << 2;

    /// <summary>HcCommandStatus.OCR, bit 3: ask the firmware to hand the controller over.</summary>
    public const uint OwnershipChangeRequest = 1u << 3;

    /// <summary>HcInterruptStatus: one bit per source, W1C (section 7.1.4).</summary>
    public const ulong HcInterruptStatus = 0x0C;

    /// <summary>HcInterruptEnable: a write ORs sources in (section 7.1.5).</summary>
    public const ulong HcInterruptEnable = 0x10;

    /// <summary>HcInterruptDisable: a write clears sources (section 7.1.6).</summary>
    public const ulong HcInterruptDisable = 0x14;

    /// <summary>SO, bit 0: scheduling overrun.</summary>
    public const uint SchedulingOverrun = 1u << 0;

    /// <summary>WDH, bit 1: HccaDoneHead was written.</summary>
    public const uint WritebackDoneHead = 1u << 1;

    /// <summary>SF, bit 2: start of frame, set every millisecond; never enabled nor handled.</summary>
    public const uint StartOfFrame = 1u << 2;

    /// <summary>RD, bit 3: resume detected.</summary>
    public const uint ResumeDetected = 1u << 3;

    /// <summary>UE, bit 4: unrecoverable error; the controller stopped itself.</summary>
    public const uint UnrecoverableError = 1u << 4;

    /// <summary>FNO, bit 5: frame number overflow.</summary>
    public const uint FrameNumberOverflow = 1u << 5;

    /// <summary>RHSC, bit 6: root hub status change.</summary>
    public const uint RootHubStatusChange = 1u << 6;

    /// <summary>OC, bit 30: ownership change.</summary>
    public const uint OwnershipChange = 1u << 30;

    /// <summary>MIE, bit 31: the master enable.</summary>
    public const uint MasterInterruptEnable = 1u << 31;

    /// <summary>Every source bit of the status register, for the clear in the probe.</summary>
    public const uint InterruptSourceMask = SchedulingOverrun | WritebackDoneHead | StartOfFrame | ResumeDetected
        | UnrecoverableError | FrameNumberOverflow | RootHubStatusChange | OwnershipChange;

    /// <summary>Every source and the master enable, for HcInterruptDisable.</summary>
    public const uint AllInterrupts = InterruptSourceMask | MasterInterruptEnable;

    /// <summary>
    /// The sources the handler and the polled drain act on, whatever the
    /// enable register holds: the done queue, the root hub and the
    /// unrecoverable error. Never SF, which is set every frame, and never
    /// MIE.
    /// </summary>
    public const uint HandledSources = WritebackDoneHead | RootHubStatusChange | UnrecoverableError;

    /// <summary>What the probe enables on an interrupt-driven controller.</summary>
    public const uint EnabledInterrupts = HandledSources | MasterInterruptEnable;

    /// <summary>HcHCCA: the 256-byte aligned HCCA (section 7.2.1).</summary>
    public const ulong HcHcca = 0x18;

    /// <summary>HcControlHeadED: the first ED of the control list, written once in the probe (section 7.2.3).</summary>
    public const ulong HcControlHeadEd = 0x20;

    /// <summary>HcBulkHeadED: the first ED of the bulk list, written once in the probe (section 7.2.5).</summary>
    public const ulong HcBulkHeadEd = 0x28;

    /// <summary>HcDoneHead, the controller's copy; the driver reads the HCCA's instead (section 7.2.7).</summary>
    public const ulong HcDoneHead = 0x30;

    /// <summary>HcFmInterval: FI in bits 13:0, FSMPS in bits 30:16, FIT in bit 31 (section 7.3.1).</summary>
    public const ulong HcFmInterval = 0x34;

    /// <summary>HcFmInterval.FI, bits 13:0: bit times per frame.</summary>
    public const uint FrameIntervalMask = 0x3FFF;

    /// <summary>The FI the specification names for a 1 ms frame: 11999 bit times.</summary>
    public const uint DefaultFrameInterval = 0x2EDF;

    /// <summary>HcFmInterval.FSMPS starts at bit 16.</summary>
    public const int LargestDataPacketShift = 16;

    /// <summary>HcFmInterval.FSMPS, bits 30:16: the largest data packet the controller starts in a frame; 0 after a reset, so a controller that did not take the programming write reads 0 (section 7.3.1).</summary>
    public const uint LargestDataPacketMask = 0x7FFFu << LargestDataPacketShift;

    /// <summary>HcFmInterval.FIT, bit 31: toggled by every write of a new interval.</summary>
    public const uint FrameIntervalToggle = 1u << 31;

    /// <summary>Bit times of per-packet overhead the FSMPS formula subtracts: FSMPS = (FI - 210) * 6 / 7 (section 7.3.1).</summary>
    public const uint MaxPacketOverheadBits = 210;

    /// <summary>FSMPS numerator and denominator: 6 bits of data per 7 on the wire with bit stuffing.</summary>
    public const uint LargestDataPacketNumerator = 6;
    public const uint LargestDataPacketDenominator = 7;

    /// <summary>HcFmRemaining (section 7.3.2).</summary>
    public const ulong HcFmRemaining = 0x38;

    /// <summary>HcFmNumber: the 16-bit frame counter (section 7.3.3).</summary>
    public const ulong HcFmNumber = 0x3C;

    /// <summary>HcPeriodicStart: the bit time the periodic list starts at, 90 percent of FI (section 7.3.4).</summary>
    public const ulong HcPeriodicStart = 0x40;

    /// <summary>HcPeriodicStart is FI * 9 / 10.</summary>
    public const uint PeriodicStartNumerator = 9;
    public const uint PeriodicStartDenominator = 10;

    /// <summary>HcLSThreshold: the bit time below which no low-speed packet starts (section 7.3.5).</summary>
    public const ulong HcLsThreshold = 0x44;

    /// <summary>The HcLSThreshold value the specification names.</summary>
    public const uint DefaultLowSpeedThreshold = 0x628;

    // --- Root hub registers (section 7.4) ---

    /// <summary>HcRhDescriptorA: NDP in bits 7:0, PSM bit 8, NPS bit 9, POTPGT in bits 31:24.</summary>
    public const ulong HcRhDescriptorA = 0x48;

    /// <summary>HcRhDescriptorA.NDP, bits 7:0: downstream ports.</summary>
    public const uint PortCountMask = 0xFF;

    /// <summary>HcRhDescriptorA.PSM, bit 8: power is switched per port.</summary>
    public const uint PowerSwitchingMode = 1u << 8;

    /// <summary>HcRhDescriptorA.NPS, bit 9: ports are always powered.</summary>
    public const uint NoPowerSwitching = 1u << 9;

    /// <summary>HcRhDescriptorA.POTPGT starts at bit 24, in units of 2 ms.</summary>
    public const int PowerOnToPowerGoodShift = 24;

    /// <summary>HcRhDescriptorA.POTPGT unit.</summary>
    public const uint PowerOnToPowerGoodUnitMs = 2;

    /// <summary>HcRhDescriptorB: DR in bits 15:0, PPCM in bits 31:16.</summary>
    public const ulong HcRhDescriptorB = 0x4C;

    /// <summary>HcRhStatus: global power and over-current (section 7.4.3).</summary>
    public const ulong HcRhStatus = 0x50;

    /// <summary>HcRhStatus bit 16 written: SetGlobalPower.</summary>
    public const uint SetGlobalPower = 1u << 16;

    /// <summary>HcRhStatus.OCIC, bit 17, W1C.</summary>
    public const uint OverCurrentIndicatorChange = 1u << 17;

    /// <summary>HcRhPortStatus of 1-based port n is at <c>0x54 + 4 * (n - 1)</c> (section 7.4.4).</summary>
    public const ulong HcRhPortStatusBase = 0x54;

    /// <summary>Bytes from one port register to the next.</summary>
    public const ulong PortRegisterStride = 4;

    /// <summary>HcRhPortStatus.CCS, bit 0: a device is connected; written as 1 it clears PES.</summary>
    public const uint CurrentConnectStatus = 1u << 0;

    /// <summary>HcRhPortStatus.PES, bit 1: the port is enabled; written as 1 it enables.</summary>
    public const uint PortEnableStatus = 1u << 1;

    /// <summary>HcRhPortStatus.PRS, bit 4: a reset is in progress; written as 1 it starts one.</summary>
    public const uint PortResetStatus = 1u << 4;

    /// <summary>HcRhPortStatus.PPS, bit 8: the port is powered; written as 1 it powers up.</summary>
    public const uint PortPowerStatus = 1u << 8;

    /// <summary>HcRhPortStatus.LSDA, bit 9: a low-speed device is attached; written as 1 it powers the port down.</summary>
    public const uint LowSpeedDeviceAttached = 1u << 9;

    /// <summary>HcRhPortStatus.CSC, bit 16, W1C: the connection changed.</summary>
    public const uint ConnectStatusChange = 1u << 16;

    /// <summary>HcRhPortStatus.PESC, bit 17, W1C: the enable changed (the controller disabled the port on an error).</summary>
    public const uint PortEnableStatusChange = 1u << 17;

    /// <summary>HcRhPortStatus.PSSC, bit 18, W1C: the suspend ended.</summary>
    public const uint PortSuspendStatusChange = 1u << 18;

    /// <summary>HcRhPortStatus.OCIC, bit 19, W1C: over-current changed.</summary>
    public const uint PortOverCurrentIndicatorChange = 1u << 19;

    /// <summary>HcRhPortStatus.PRSC, bit 20, W1C: the reset completed.</summary>
    public const uint PortResetStatusChange = 1u << 20;

    /// <summary>The five W1C change bits 20:16.</summary>
    public const uint PortChangeBits = ConnectStatusChange | PortEnableStatusChange | PortSuspendStatusChange
        | PortOverCurrentIndicatorChange | PortResetStatusChange;

    // --- Endpoint descriptor fields (section 4.2) ---

    /// <summary>ED dword 0 FA, bits 6:0: the function address.</summary>
    public const uint FunctionAddressMask = 0x7F;

    /// <summary>ED dword 0 EN starts at bit 7.</summary>
    public const int EndpointNumberShift = 7;

    /// <summary>ED dword 0 D starts at bit 11: 00 and 11 take the direction from each TD.</summary>
    public const int EndpointDirectionShift = 11;

    /// <summary>ED D = 00: direction from the TD.</summary>
    public const uint EndpointDirectionFromTd = 0;

    /// <summary>ED D = 01: OUT.</summary>
    public const uint EndpointDirectionOut = 1;

    /// <summary>ED D = 10: IN.</summary>
    public const uint EndpointDirectionIn = 2;

    /// <summary>ED dword 0 S, bit 13: a low-speed device.</summary>
    public const uint EndpointLowSpeed = 1u << 13;

    /// <summary>ED dword 0 K, bit 14: the controller skips the ED.</summary>
    public const uint EndpointSkip = 1u << 14;

    /// <summary>ED dword 0 MPS starts at bit 16.</summary>
    public const int EndpointMaxPacketSizeShift = 16;

    /// <summary>ED dword 0 MPS, bits 26:16.</summary>
    public const uint EndpointMaxPacketSizeMask = 0x7FF;

    /// <summary>ED dword 2 H, bit 0: the endpoint is halted; cleared by software.</summary>
    public const uint EndpointHalted = 1u << 0;

    /// <summary>ED dword 2 C, bit 1: the toggle carry, the data toggle of the next TD that takes it from the ED.</summary>
    public const uint EndpointToggleCarry = 1u << 1;

    /// <summary>The 16-byte aligned pointer in a head, tail or next field, less its flag bits.</summary>
    public const uint DescriptorPointerMask = ~0xFu;

    // --- Transfer descriptor fields (section 4.3.1) ---

    /// <summary>TD dword 0 R, bit 18: a short packet is not an error.</summary>
    public const uint BufferRounding = 1u << 18;

    /// <summary>TD dword 0 DP starts at bit 19.</summary>
    public const int DirectionPidShift = 19;

    /// <summary>TD DP = 00: SETUP.</summary>
    public const uint DirectionPidSetup = 0;

    /// <summary>TD DP = 01: OUT.</summary>
    public const uint DirectionPidOut = 1;

    /// <summary>TD DP = 10: IN.</summary>
    public const uint DirectionPidIn = 2;

    /// <summary>TD dword 0 DI starts at bit 21: frames before the done queue is written back, 7 for never.</summary>
    public const int DelayInterruptShift = 21;

    /// <summary>DI of the SETUP and DATA stages: the longest delay that still counts down, so a stage of a transfer whose STATUS never completes is flushed within seven frames.</summary>
    public const uint DelayInterruptStages = 6;

    /// <summary>DI of a final TD: the done queue is written back at the next frame.</summary>
    public const uint DelayInterruptImmediate = 0;

    /// <summary>TD dword 0 T starts at bit 24.</summary>
    public const int DataToggleShift = 24;

    /// <summary>TD T = 0x: the toggle comes from the ED's carry.</summary>
    public const uint DataToggleFromCarry = 0;

    /// <summary>TD T = 10: DATA0.</summary>
    public const uint DataToggle0 = 2;

    /// <summary>TD T = 11: DATA1.</summary>
    public const uint DataToggle1 = 3;

    /// <summary>TD dword 0 CC starts at bit 28.</summary>
    public const int ConditionCodeShift = 28;

    /// <summary>TD dword 0 CC, bits 31:28.</summary>
    public const uint ConditionCodeMask = 0xF;

    /// <summary>The CC every TD the driver builds starts with, so a TD the controller never touched is never read as a success.</summary>
    public const uint ConditionCodeNotAccessed = (uint)OhciConditionCode.NotAccessed << ConditionCodeShift;

    /// <summary>A general TD spans at most two 4 KiB pages.</summary>
    public const int TransferMaxBytes = 8192;

    /// <summary>Bytes of the SETUP packet a SETUP TD carries.</summary>
    public const int SetupPacketBytes = 8;

    // --- HCCA (section 4.4) ---

    /// <summary>HccaDoneHead bit 0: another interrupt source was pending when the head was written.</summary>
    public const uint DoneHeadInterruptPending = 1u << 0;

    /// <summary>Interrupt EDs sit in one of six interval lists, periods 1 to 32 ms.</summary>
    public const int IntervalHeadCount = 6;

    /// <summary>The longest interrupt period the tree serves, in frames.</summary>
    public const int MaxIntervalFrames = 32;

    // --- Addresses and endpoints ---

    /// <summary>The highest USB device address.</summary>
    public const int MaxAddress = 127;

    /// <summary>SET_ADDRESS, which the kit never issues and so does not name (USB 2.0 table 9-4).</summary>
    public const byte SetAddressRequest = 5;

    /// <summary>ENDPOINT_HALT feature selector (USB 2.0 table 9-6).</summary>
    public const ushort EndpointHaltFeature = 0;

    /// <summary>bMaxPacketSize0 before the device descriptor is read: 8 is valid for every low and full-speed device.</summary>
    public const ushort DefaultMaxPacketSize0 = 8;

    /// <summary>Highest endpoint index (endpoint 15 IN, as <c>number * 2 + 1</c>).</summary>
    public const byte MaxEndpointIndex = 31;

    /// <summary>The device descriptor's first 8 bytes carry bMaxPacketSize0 (USB 2.0 section 9.6.1).</summary>
    public const int DeviceDescriptorHeadLength = 8;

    /// <summary>Byte 7 of the device descriptor: bMaxPacketSize0.</summary>
    public const int MaxPacketSize0Offset = 7;

    // --- Sizes ---

    /// <summary>The least BAR0 may span: the registers up to the first port.</summary>
    public const ulong MinimumRegisterWindowBytes = HcRhPortStatusBase;

    /// <summary>Bytes of one ED or TD slot of the descriptor pool.</summary>
    public const int DescriptorBytes = 16;

    /// <summary>Descriptor slots in one page of the pool.</summary>
    public const int DescriptorsPerPage = 256;

    /// <summary>TD slots of a device set: SETUP, DATA, STATUS and the dummy.</summary>
    public const int DeviceTransferSlots = 4;

    /// <summary>TD slots of a bulk set: one transfer and the dummy.</summary>
    public const int BulkTransferSlots = 2;

    /// <summary>Transfers kept in flight on an interrupt pipe; a keyboard needs one, a few absorb bursts between drains.</summary>
    public const int MaxBuffers = 8;

    /// <summary>TD slots of an interrupt set: one per buffer and the dummy.</summary>
    public const int InterruptTransferSlots = MaxBuffers + 1;

    /// <summary>The fewest TD slots a pipe set can queue a transfer with: one TD and the dummy TailP names (section 5.2.8.2).</summary>
    public const int MinimumPipeTransferSlots = 2;

    /// <summary>The fewest TD slots a device set can run a control transfer with: SETUP, STATUS and the dummy (sections 4.3.1 and 5.2.8.2).</summary>
    public const int MinimumDeviceTransferSlots = 3;

    /// <summary>The bulk bounce buffer: 64 KiB, 64 KiB aligned.</summary>
    public const int BulkBufferBytes = 65536;

    /// <summary>Bytes of one bulk TD: two pages, the most a general TD spans.</summary>
    public const int BulkChunkBytes = TransferMaxBytes;

    // --- Timings, in milliseconds unless the name says otherwise ---

    /// <summary>How long the firmware has to clear IR once OCR is set.</summary>
    public const uint FirmwareHandoffTimeoutMs = 1000;

    /// <summary>How long HCR has to self-clear (section 7.1.3: 10 us; a generous bound).</summary>
    public const uint ResetTimeoutMs = 10;

    /// <summary>Granularity of the HCR poll.</summary>
    public const uint ResetPollMicroseconds = 10;

    /// <summary>Settle after HCFS = USBResume before the reset (section 5.1.1.3).</summary>
    public const uint ResumeSettleMs = 10;

    /// <summary>Settle after HCFS = USBReset, a bus reset, before the software reset (USB 2.0 section 7.1.7.5).</summary>
    public const uint BusResetSettleMs = 50;

    /// <summary>Port power-on to power-good when the descriptor gives none.</summary>
    public const uint PortPowerSettleMs = 20;

    /// <summary>After the controller runs: the reset dropped every device, so they reconnect.</summary>
    public const uint PortConnectSettleMs = 200;

    /// <summary>How long a port reset has to raise PRSC.</summary>
    public const uint PortResetTimeoutMs = 500;

    /// <summary>TRSTRCY: recovery after a reset before the device must answer (USB 2.0 section 7.1.7.5).</summary>
    public const uint PortResetRecoveryMs = 10;

    /// <summary>TATTDB: a connection must be stable this long before the port is reset (USB 2.0 section 7.1.7.3).</summary>
    public const uint ConnectDebounceMs = 100;

    /// <summary>Upper bound for a control transfer with a data stage (USB 2.0 section 9.2.6.4).</summary>
    public const uint TransferTimeoutMs = 5000;

    /// <summary>Budget for one bulk TD: a flash drive may stall a write for seconds while it erases.</summary>
    public const uint BulkTimeoutMs = 10_000;

    /// <summary>Budget to claim address 0: a holder may spend a full transfer budget on SET_ADDRESS plus its recovery delays.</summary>
    public const uint AddressClaimTimeoutMs = 2 * TransferTimeoutMs;

    /// <summary>SET_ADDRESS recovery interval before the next request (USB 2.0 section 9.2.6.3).</summary>
    public const uint SetAddressRecoveryMs = 2;

    /// <summary>Granularity of every claim loop and of the polled waits.</summary>
    public const uint PollMicroseconds = 10;

    /// <summary>How often the hot-plug thread drains the controller on a polled one.</summary>
    public const uint PolledDrainIntervalMs = 20;

    /// <summary>How long the hot-plug thread waits for a port change before it looks at the ports again.</summary>
    public const uint HotPlugWaitMs = 1000;

    /// <summary>Frames to wait after an ED was taken off a list or skipped before any of its dwords is written (section 5.2.7.1.2).</summary>
    public const uint EndpointRemovalFrames = 2;

    /// <summary>Frames to wait after an ED was skipped before its static fields are rewritten (section 5.2.7.1.3).</summary>
    public const uint EndpointModifyFrames = 1;

    /// <summary>Frames a quiesce waits for the controller to deliver the retired TDs of a skipped ED: DI 6 flushes them within seven.</summary>
    public const uint DoneQueueFlushFrames = 8;

    /// <summary>Recoveries of an interrupt pipe without a good transfer in between before it is given up.</summary>
    public const int MaxPipeRecoveries = 3;

    /// <summary>Pipes the hot-plug thread recovers per pass; more wait for the next pass.</summary>
    public const int RecoveryBatch = 32;

    /// <summary>Passes one drain makes over HcInterruptStatus before it gives up: a function that vanished reads all ones for good.</summary>
    public const int MaxDrainPasses = 16;

    /// <summary>Microseconds in a millisecond, for <see cref="Cosmos.Kernel.HAL.DriverKit.DeviceBinding.Delay"/>.</summary>
    public const uint MicrosecondsPerMillisecond = 1000;
}
