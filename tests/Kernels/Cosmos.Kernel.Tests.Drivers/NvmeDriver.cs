// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A minimal NVMe driver, matched by class, that binds the controller the
/// device-matched <see cref="FailingNvmeDriver"/> failed on. It records how
/// the kit handed the function over, requests interrupts again, brings the
/// controller up with an admin queue pair and submits one Identify
/// Controller command. The completion arrives during Probe, when the kit
/// holds the interrupt back: with MSI-X the message waits in the entry's
/// pending bit and is sent when the kit unmasks it on Bound, polled the
/// handler finds the entry on the first tick after Bound. The handler
/// consumes it either way, so the NVMe cells see the interrupt of a real
/// command reach the handler, after Bound and never before.
/// </summary>
internal sealed class NvmeDriver : PciDriver
{
    /// <summary>
    /// The registration's name, and the owner the NVMe function gets. Not
    /// "nvme", which is the built-in driver's and which Register refuses.
    /// </summary>
    public const string Name = "nvme-identify";

    /// <summary>What the Identify Controller data starts with on QEMU: its PCI vendor ID.</summary>
    public const ushort ExpectedIdentifyVendorId = FailingNvmeDriver.VendorId;

    // Registers (NVMe 1.4 §3.1). CAP, ASQ and ACQ are 64-bit.
    private const int RegisterBar = 0;
    private const ulong CapabilitiesRegister = 0x00;
    private const ulong ConfigurationRegister = 0x14;
    private const ulong StatusRegister = 0x1C;
    private const ulong AdminQueueAttributesRegister = 0x24;
    private const ulong AdminSubmissionQueueRegister = 0x28;
    private const ulong AdminCompletionQueueRegister = 0x30;
    private const ulong DoorbellBase = 0x1000;

    // CAP fields: the doorbell stride (bits 35:32, a power of two times 4
    // bytes) and the ready timeout (bits 31:24, in 500 ms units).
    private const int DoorbellStrideShift = 32;
    private const ulong DoorbellStrideMask = 0xF;
    private const ulong DoorbellStrideUnit = 4;
    private const int ReadyTimeoutShift = 24;
    private const ulong ReadyTimeoutMask = 0xFF;
    private const ulong ReadyTimeoutUnitMilliseconds = 500;

    // CC: enable, with 64-byte submission (IOSQES=6) and 16-byte completion
    // (IOCQES=4) entries, 4 KiB pages and the NVM command set (both 0).
    private const uint ConfigurationEnable = 0x1;
    private const uint ConfigurationEntrySizes = (6u << 16) | (4u << 20);
    private const uint StatusReady = 0x1;

    /// <summary>Entries per admin queue: two, the fewest the specification allows.</summary>
    private const uint AdminQueueDepth = 2;
    private const int AdminCompletionQueueSizeShift = 16;

    private const int QueueBytes = 4096;
    private const int IdentifyBytes = 4096;
    private const int SubmissionEntryBytes = 64;

    // The Identify command: opcode 06h, CNS 1 (the controller), in
    // dword 10; the data pointer PRP1 in dwords 6-7.
    private const uint IdentifyOpcode = 0x06;
    private const uint IdentifyController = 1;
    private const ushort IdentifyCommandId = 0x42;
    private const int CommandIdShift = 16;
    private const int DataPointerOffset = 24;
    private const int CommandDword10Offset = 40;

    // The completion entry's dword 3: command ID (15:0), phase tag (16) and
    // status (31:17). A fresh queue is zeroed, so the first entry is new
    // once its phase tag reads 1.
    private const int CompletionDword3Offset = 12;
    private const uint CompletionPhaseBit = 1u << 16;
    private const int CompletionStatusShift = 17;

    /// <summary>1 ms polls Probe allows the Identify to complete in.</summary>
    private const int CompletionPollLimit = 1000;

    /// <summary>Config offset of the Command register.</summary>
    private const ushort CommandOffset = 0x04;

    // Written from the interrupt handler, read by the cells.
    private static int s_handlerCalls;
    private static uint s_completionDword3;
    private static volatile bool s_completionHandled;

    /// <summary>Set by Probe as its last step, just before it returns Bound.</summary>
    private static volatile bool s_probeReturned;

    // What the handler needs, set before the controller can complete anything.
    private static MmioRegion? s_registers;
    private static DmaBuffer? s_completionQueue;
    private static ulong s_completionDoorbell;

    /// <summary>True once Probe ran.</summary>
    public static bool Probed { get; private set; }

    /// <summary>The Command register as Probe found it, after the failed attempt's teardown.</summary>
    public static ushort CommandAtProbe { get; private set; }

    /// <summary>MSI-X Enable as Probe found it, after the failed attempt's teardown.</summary>
    public static bool MsiXEnabledAtProbe { get; private set; }

    /// <summary>Free pages when Probe began.</summary>
    public static ulong FreePagesAtProbe { get; private set; }

    /// <summary>Bound dynamic vectors when Probe began.</summary>
    public static int BoundVectorsAtProbe { get; private set; }

    /// <summary>What TryRequestInterrupts answered.</summary>
    public static bool InterruptsGranted { get; private set; }

    /// <summary>Bound dynamic vectors right after the request.</summary>
    public static int BoundVectorsAfterRequest { get; private set; }

    /// <summary>MSI-X Enable right after the request.</summary>
    public static bool MsiXEnabledAfterRequest { get; private set; }

    /// <summary>The BAR holding the MSI-X table, mapped during Probe; null when the function has none or it did not map.</summary>
    public static MmioRegion? MsiXTable { get; private set; }

    /// <summary>Where entry 0's Vector Control register sits in <see cref="MsiXTable"/>.</summary>
    public static ulong Entry0Control { get; private set; }

    /// <summary>Entry 0's mask bit during Probe, after the request.</summary>
    public static bool Entry0MaskedInProbe { get; private set; }

    /// <summary>True when the controller reported ready after being enabled.</summary>
    public static bool ControllerReady { get; private set; }

    /// <summary>True when Probe saw the Identify's completion entry arrive in memory before it returned.</summary>
    public static bool CompletedDuringProbe { get; private set; }

    /// <summary>What went wrong in Probe, for the cells' reports; null when nothing did.</summary>
    public static string? ProbeFailure { get; private set; }

    /// <summary>True when the handler ran while Probe was still running.</summary>
    public static bool HandlerRanBeforeBound { get; private set; }

    /// <summary>The event the handler signals when it consumes the completion.</summary>
    public static DeviceEvent? Event { get; private set; }

    /// <summary>The Identify Controller data, once the command completed.</summary>
    public static DmaBuffer? IdentifyData { get; private set; }

    /// <summary>Calls of the handler: one per message with MSI-X, one per tick when polled.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <summary>True once the handler consumed the Identify's completion.</summary>
    public static bool CompletionHandled => s_completionHandled;

    /// <summary>The completion entry's status field as the handler read it: 0 is success.</summary>
    public static uint CompletionStatus => Volatile.Read(ref s_completionDword3) >> CompletionStatusShift;

    /// <summary>The command ID the completion entry names.</summary>
    public static ushort CompletionCommandId => (ushort)Volatile.Read(ref s_completionDword3);

    /// <summary>The command ID the Identify was submitted with.</summary>
    public static ushort SubmittedCommandId => IdentifyCommandId;

    /// <inheritdoc />
    protected internal override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);
        Probed = true;
        CommandAtProbe = context.Function.ReadConfig16(CommandOffset);
        MsiXEnabledAtProbe = MsiXState.IsEnabled(context.Function);
        FreePagesAtProbe = KernelState.FreePages;
        BoundVectorsAtProbe = KernelState.BoundVectors;

        if (!context.TryMapBar(RegisterBar, out MmioRegion? registers))
        {
            return Fail("BAR 0 did not map");
        }

        ulong capabilities = registers.Read64(CapabilitiesRegister);
        ulong doorbellStride = DoorbellStrideUnit << (int)((capabilities >> DoorbellStrideShift) & DoorbellStrideMask);
        ulong readyTimeoutUnits = Math.Max(1UL, (capabilities >> ReadyTimeoutShift) & ReadyTimeoutMask);
        int readyPolls = (int)(readyTimeoutUnits * ReadyTimeoutUnitMilliseconds);

        registers.Write32(ConfigurationRegister, registers.Read32(ConfigurationRegister) & ~ConfigurationEnable);
        if (!WaitForReady(context, registers, false, readyPolls))
        {
            return Fail("the controller did not stop");
        }

        if (!context.TryAllocateDma(QueueBytes, ulong.MaxValue, out DmaBuffer? submissionQueue)
            || !context.TryAllocateDma(QueueBytes, ulong.MaxValue, out DmaBuffer? completionQueue)
            || !context.TryAllocateDma(IdentifyBytes, ulong.MaxValue, out DmaBuffer? identify))
        {
            return Fail("no DMA memory for the admin queues");
        }

        uint depth = AdminQueueDepth - 1;
        registers.Write32(AdminQueueAttributesRegister, (depth << AdminCompletionQueueSizeShift) | depth);
        registers.Write64(AdminSubmissionQueueRegister, submissionQueue.DeviceAddress);
        registers.Write64(AdminCompletionQueueRegister, completionQueue.DeviceAddress);

        // The admin completion queue interrupts through vector 0, the entry
        // the kit programs; set before anything can complete.
        s_registers = registers;
        s_completionQueue = completionQueue;
        s_completionDoorbell = DoorbellBase + doorbellStride;
        Event = context.CreateEvent();

        InterruptsGranted = context.TryRequestInterrupts(OnInterrupt);
        BoundVectorsAfterRequest = KernelState.BoundVectors;
        MsiXEnabledAfterRequest = MsiXState.IsEnabled(context.Function);
        if (MsiXState.TryMapEntry0Control(context, out MmioRegion? table, out ulong entryControl))
        {
            MsiXTable = table;
            Entry0Control = entryControl;
            Entry0MaskedInProbe = (table.Read32(entryControl) & MsiXState.EntryMaskBit) != 0;
        }

        context.EnableBusMastering();
        registers.Write32(ConfigurationRegister, ConfigurationEntrySizes | ConfigurationEnable);
        if (!WaitForReady(context, registers, true, readyPolls))
        {
            return Fail("the controller did not become ready");
        }

        ControllerReady = true;
        IdentifyData = identify;
        WriteIdentify(submissionQueue.Span, identify.DeviceAddress);

        // The tail doorbell of submission queue 0: one command. The region's
        // write barrier orders the command's stores before it.
        registers.Write32(DoorbellBase, 1);
        CompletedDuringProbe = WaitForCompletion(context, completionQueue);

        s_probeReturned = true;
        return ProbeResult.Bound;
    }

    private static ProbeResult Fail(string reason)
    {
        ProbeFailure = reason;
        return ProbeResult.Failed;
    }

    private static bool WaitForReady(PciDeviceContext context, MmioRegion registers, bool ready, int polls)
    {
        for (int poll = 0; poll < polls; poll++)
        {
            if (((registers.Read32(StatusRegister) & StatusReady) != 0) == ready)
            {
                return true;
            }

            context.Delay(TimeSpan.FromMilliseconds(1));
        }

        return false;
    }

    private static void WriteIdentify(Span<byte> submissionQueue, ulong dataAddress)
    {
        Span<byte> command = submissionQueue[..SubmissionEntryBytes];
        command.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(command, IdentifyOpcode | ((uint)IdentifyCommandId << CommandIdShift));
        BinaryPrimitives.WriteUInt64LittleEndian(command[DataPointerOffset..], dataAddress);
        BinaryPrimitives.WriteUInt32LittleEndian(command[CommandDword10Offset..], IdentifyController);
    }

    /// <summary>
    /// Watches the completion queue's first entry in memory, without
    /// consuming it: that is the handler's job, once the kit lets it run.
    /// </summary>
    private static bool WaitForCompletion(PciDeviceContext context, DmaBuffer completionQueue)
    {
        for (int poll = 0; poll < CompletionPollLimit; poll++)
        {
            if ((ReadCompletionDword3(completionQueue) & CompletionPhaseBit) != 0)
            {
                return true;
            }

            context.Delay(TimeSpan.FromMilliseconds(1));
        }

        return false;
    }

    private static uint ReadCompletionDword3(DmaBuffer completionQueue) =>
        BinaryPrimitives.ReadUInt32LittleEndian(completionQueue.Span[CompletionDword3Offset..]);

    /// <summary>
    /// The interrupt handler. Interrupt context: no allocation, no throw, no
    /// string. Consumes the one completion the driver expects, once, and
    /// tells the controller through the completion queue's head doorbell.
    /// </summary>
    private static void OnInterrupt(int vector)
    {
        s_handlerCalls++;
        if (!s_probeReturned)
        {
            HandlerRanBeforeBound = true;
        }

        if (s_completionHandled || s_completionQueue is not { } completionQueue || s_registers is not { } registers)
        {
            return;
        }

        uint dword3 = ReadCompletionDword3(completionQueue);
        if ((dword3 & CompletionPhaseBit) == 0)
        {
            return;
        }

        // The phase tag says the entry and the Identify data are written:
        // no load of either may run ahead of it.
        DmaBuffer.ReadBarrier();
        Volatile.Write(ref s_completionDword3, dword3);
        registers.Write32(s_completionDoorbell, 1);
        s_completionHandled = true;
        Event?.Signal();
    }
}
