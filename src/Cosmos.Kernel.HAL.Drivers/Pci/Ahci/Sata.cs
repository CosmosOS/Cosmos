// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Ata;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.CommandList;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Registers;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci;

/// <summary>
/// One SATA disk behind an AHCI port. Every transfer goes through the
/// port's one-page bounce buffer, which the HBA reaches by DMA: at most
/// eight sectors per command.
///
/// <para>Error contract: command issue throws on failure (no free slot,
/// device timeout, task-file error), so <see cref="ReadBlock"/> /
/// <see cref="WriteBlock"/> never silently hand back stale bounce-buffer
/// contents. All I/O on a port is serialized by a lock of its own — the
/// port owns a single bounce buffer.</para>
/// </summary>
internal sealed class Sata : IBlockDevice
{
    /// <summary>Regular sector size (512 bytes).</summary>
    private const ulong RegularSectorSize = 512UL;

    /// <summary>Bytes in the bounce buffer: one page, the kit's DMA allocation unit.</summary>
    private const int BounceBufferBytes = 4096;

    /// <summary>
    /// Bounce-buffer capacity in sectors. A command must never span more
    /// than this or the HBA would DMA past the page into adjacent memory.
    /// </summary>
    private const uint MaxSectorsPerCommand = (uint)((ulong)BounceBufferBytes / RegularSectorSize);

    /// <summary>PxTFD status mask: BSY (bit 7) | DRQ (bit 3) - device busy or requesting a data transfer.</summary>
    private const uint TfdBusyDrqMask = (uint)(AtaDeviceStatus.Busy | AtaDeviceStatus.DRQ);

    /// <summary>Device register LBA-mode bit (bit 6), required for LBA48 commands.</summary>
    private const byte DeviceLbaMode = 1 << 6;

    /// <summary>Mask isolating one byte when splitting LBA / sector-count values into FIS byte lanes.</summary>
    private const int ByteMask = 0xFF;

    /// <summary>Shift to byte lane 1 (bits 8-15).</summary>
    private const int Byte1Shift = 8;

    /// <summary>Shift to byte lane 2 (bits 16-23).</summary>
    private const int Byte2Shift = 16;

    /// <summary>Shift to byte lane 3 (bits 24-31).</summary>
    private const int Byte3Shift = 24;

    /// <summary>Shift to byte lane 4 (bits 32-39).</summary>
    private const int Byte4Shift = 32;

    /// <summary>Shift to byte lane 5 (bits 40-47).</summary>
    private const int Byte5Shift = 40;

    /// <summary>IDENTIFY DEVICE response size in 16-bit words (ATA/ACS).</summary>
    private const int IdentifyBufferWords = 256;

    /// <summary>IDENTIFY word 27 - start of the model number field.</summary>
    private const int IdentifyWordModelNumber = 27;

    /// <summary>Model number field length in characters (IDENTIFY words 27-46).</summary>
    private const int IdentifyModelNumberChars = 40;

    /// <summary>IDENTIFY word 83 - command sets supported.</summary>
    private const int IdentifyWordCommandSets = 83;

    /// <summary>IDENTIFY word 83 bit 10 - 48-bit Address feature set supported.</summary>
    private const int Lba48SupportedMask = 1 << 10;

    /// <summary>IDENTIFY word 60 - low word of the LBA28 addressable sector count.</summary>
    private const int IdentifyWordLba28SectorsLow = 60;

    /// <summary>IDENTIFY word 61 - high word of the LBA28 addressable sector count.</summary>
    private const int IdentifyWordLba28SectorsHigh = 61;

    /// <summary>IDENTIFY word 100 - word 0 (bits 15:0) of the LBA48 addressable sector count.</summary>
    private const int IdentifyWordLba48Sectors0 = 100;

    /// <summary>IDENTIFY word 101 - word 1 (bits 31:16) of the LBA48 addressable sector count.</summary>
    private const int IdentifyWordLba48Sectors1 = 101;

    /// <summary>IDENTIFY word 102 - word 2 (bits 47:32) of the LBA48 addressable sector count.</summary>
    private const int IdentifyWordLba48Sectors2 = 102;

    /// <summary>IDENTIFY word 103 - word 3 (bits 63:48) of the LBA48 addressable sector count.</summary>
    private const int IdentifyWordLba48Sectors3 = 103;

    /// <summary>Shift placing IDENTIFY word 1 of a multi-word value (bits 16-31).</summary>
    private const int IdentifyWord1Shift = 16;

    /// <summary>Shift placing IDENTIFY word 2 of a multi-word value (bits 32-47).</summary>
    private const int IdentifyWord2Shift = 32;

    /// <summary>Shift placing IDENTIFY word 3 of a multi-word value (bits 48-63).</summary>
    private const int IdentifyWord3Shift = 48;

    /// <summary>Bounded spin-loop iteration limit when polling port registers (TFD busy, SSTS/SCTL DET).</summary>
    private const int RegisterPollSpinLimit = 1_000_000;

    /// <summary>Bounded spin-loop iteration limit while waiting for command completion (PxCI slot clear).</summary>
    private const uint CommandCompletionSpinLimit = 50_000_000;

    /// <summary>Poll attempts while waiting for the port command engine (PxCMD.ST) to stop.</summary>
    private const int EngineStopRetries = 50;

    /// <summary>Microseconds between PxCMD.ST stop polls.</summary>
    private const int EngineStopPollDelayMicroseconds = 10_000;

    /// <summary>Microseconds to hold SCTL.DET = 1 during COMRESET.</summary>
    private const int ComresetHoldDelayMicroseconds = 1000;

    // Across every controller, so each disk gets a name of its own
    // ("sata0", "sata1", ...) and multi-disk systems get distinguishable
    // device and partition names. Probes run one at a time.
    private static uint s_nextIndex;

    private readonly PortRegisters _portReg;
    private readonly DmaBuffer _bounceBuffer;

    // Serializes command issue + bounce-buffer access on this port: the
    // buffer is shared state, so an unserialized concurrent WriteBlock
    // would interleave another caller's data into an in-flight command.
    // 1 while held. A plain busy spin with interrupts left on, held across a
    // whole multi-command transfer: masking interrupts for milliseconds of
    // polled I/O is not an option, and the kit's IrqSafeLock does exactly
    // that, while parking or backing off by sleeping is illegal on the idle
    // thread the partition scan runs on.
    private int _ioBusy;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ulong BlockCount { get; }

    /// <inheritdoc />
    public ulong BlockSize => RegularSectorSize;

    /// <summary>The AHCI port the disk sits on.</summary>
    internal uint PortNumber => _portReg.PortNumber;

    /// <summary>
    /// Brings up the disk on <paramref name="portReg"/>: names it, allocates
    /// its bounce buffer and reads its IDENTIFY data. Probe only: it
    /// allocates DMA memory.
    /// </summary>
    /// <param name="portReg">A rebased port whose signature said SATA.</param>
    /// <exception cref="InvalidOperationException">
    /// The port is not a SATA device, or no bounce buffer could be allocated
    /// where the HBA can reach it.
    /// </exception>
    /// <exception cref="IOException">The IDENTIFY command failed.</exception>
    internal Sata(PortRegisters portReg)
    {
        AhciController controller = portReg.Controller;

        if (portReg.PortType != PortType.Sata || (portReg.CMD & (uint)CommandAndStatus.ATAPIDevice) != 0)
        {
            controller.Context.WriteLog($"Port {portReg.PortNumber} is not a SATA device");
            throw new InvalidOperationException("AHCI port is not a SATA device");
        }

        _portReg = portReg;
        Name = $"sata{s_nextIndex++}";

        // Refused rather than warned about: a 32-bit HBA handed a buffer
        // above 4 GiB would DMA to the truncated address, over whatever
        // memory sits there. The port is skipped instead.
        if (!controller.Context.TryAllocateDma(BounceBufferBytes, controller.MaximumDeviceAddress, out DmaBuffer? bounceBuffer))
        {
            throw new InvalidOperationException("No DMA memory the HBA can reach for the port's bounce buffer.");
        }

        _bounceBuffer = bounceBuffer;

        SendSataCommand(AtaCommands.Identify);

        ushort[] identify = new ushort[IdentifyBufferWords];
        ReadDataBlock16(identify);

        string model = GetString(identify, IdentifyWordModelNumber, IdentifyModelNumberChars);

        // Capacity: all I/O here is issued as READ/WRITE DMA EXT, so prefer
        // the 48-bit count in IDENTIFY words 100-103 (word 83 bit 10 = 48-bit
        // feature set supported). Words 60-61 saturate at 0x0FFFFFFF
        // (~128 GiB) and would silently truncate larger disks — misplacing
        // anything derived from BlockCount, like the backup GPT header.
        if ((identify[IdentifyWordCommandSets] & Lba48SupportedMask) != 0)
        {
            BlockCount = (ulong)identify[IdentifyWordLba48Sectors3] << IdentifyWord3Shift | (ulong)identify[IdentifyWordLba48Sectors2] << IdentifyWord2Shift
                       | (ulong)identify[IdentifyWordLba48Sectors1] << IdentifyWord1Shift | identify[IdentifyWordLba48Sectors0];
        }
        else
        {
            BlockCount = (uint)identify[IdentifyWordLba28SectorsHigh] << IdentifyWord1Shift | identify[IdentifyWordLba28SectorsLow];
        }

        controller.Context.WriteLog($"SATA initialized: {model.Trim()} ({BlockCount} sectors)");
    }

    /// <summary>
    /// Resets the port. Returns false when the command engine refused to
    /// stop — the port must then be left untouched (reprogramming CLB/FB
    /// on a running engine lets the HBA fetch garbage command headers and
    /// issue arbitrary DMA), not fixed up with a whole-HBA reset that
    /// would wipe every sibling port.
    /// </summary>
    internal static bool PortReset(PortRegisters port)
    {
        AhciController controller = port.Controller;

        // Stop the port command engine, then wait for CMD.ST to actually clear.
        port.CMD &= ~(uint)CommandAndStatus.StartProcess;
        for (int i = 0; i <= EngineStopRetries; i++)
        {
            if ((port.CMD & (uint)CommandAndStatus.StartProcess) == 0)
            {
                break;
            }

            controller.Wait(EngineStopPollDelayMicroseconds);
        }

        if ((port.CMD & (uint)CommandAndStatus.StartProcess) != 0)
        {
            controller.Context.WriteLog("Port engine refused to stop; leaving port offline");
            return false;
        }

        // COMRESET: set SCTL.DET=1 while preserving the SPD/IPM restriction
        // fields, hold it, then clear DET so the PHY retrains.
        port.SCTL = (port.SCTL & ~PortRegisters.SctlDetMask) | PortRegisters.SctlDetComreset;
        controller.Wait(ComresetHoldDelayMicroseconds);
        port.SCTL &= ~PortRegisters.SctlDetMask;

        // Wait (bounded) for the PHY to report an established link (DET == 3); a
        // missing or wedged device must not spin the boot CPU forever.
        int spin = 0;
        while ((port.SSTS & PortRegisters.SstsDetMask) != (uint)DeviceDetectionStatus.DeviceDetectedWithPhy && spin < RegisterPollSpinLimit)
        {
            spin++;
        }

        // RW1C: clear every latched error bit, not just DIAG bit 0.
        port.SERR = PortRegisters.Rw1CClearAll;

        // Wait (bounded) for the COMRESET request bit to clear.
        spin = 0;
        while ((port.SCTL & PortRegisters.SctlDetMask) != 0 && spin < RegisterPollSpinLimit)
        {
            spin++;
        }

        return true;
    }

    // The bounce buffer is a single page, so I/O is chunked at
    // MaxSectorsPerCommand sectors per command; the port lock keeps each
    // command + bounce-buffer copy atomic against other threads. The
    // up-front span check plus long offset math keeps spans of 4M blocks
    // or more from wrapping the slice offsets.
    /// <inheritdoc />
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int sector = (int)BlockSize;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));

        AcquireIo();
        try
        {
            ulong done = 0;
            while (done < blockCount)
            {
                ulong remaining = blockCount - done;
                uint chunk = remaining >= MaxSectorsPerCommand ? MaxSectorsPerCommand : (uint)remaining;
                IssueCommandCore((byte)AtaCommands.ReadDmaExt, blockNo + done, chunk, isWrite: false, useLba48: true, hasData: true);
                ReadDataBlock8(data.Slice((int)((long)done * sector), sector * (int)chunk));
                done += chunk;
            }
        }
        finally
        {
            ReleaseIo();
        }
    }

    /// <inheritdoc />
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int sector = (int)BlockSize;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));

        AcquireIo();
        try
        {
            ulong done = 0;
            while (done < blockCount)
            {
                ulong remaining = blockCount - done;
                uint chunk = remaining >= MaxSectorsPerCommand ? MaxSectorsPerCommand : (uint)remaining;
                WriteDataBlock8(data.Slice((int)((long)done * sector), sector * (int)chunk));
                IssueCommandCore((byte)AtaCommands.WriteDmaExt, blockNo + done, chunk, isWrite: true, useLba48: true, hasData: true);
                done += chunk;
            }
        }
        finally
        {
            ReleaseIo();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Issues FLUSH CACHE EXT so completed writes reach stable media.
    /// WriteBlock deliberately does not flush per write — durability is the
    /// caller's policy via <see cref="Flush"/>, matching the shared
    /// IBlockDevice contract (and NVMe's behavior).
    /// </remarks>
    public void Flush()
    {
        AcquireIo();
        try
        {
            IssueCommandCore((byte)AtaCommands.CacheFlushExt, lba: 0, count: 0, isWrite: false, useLba48: false, hasData: false);
        }
        finally
        {
            ReleaseIo();
        }
    }

    private static string GetString(ushort[] buffer, int indexStart, int stringLength)
    {
        char[] chars = new char[stringLength];
        for (int i = 0; i < stringLength / 2; i++)
        {
            ushort word = buffer[indexStart + i];
            chars[i * 2] = (char)(word >> Byte1Shift);
            chars[i * 2 + 1] = (char)word;
        }

        return new string(chars);
    }

    /// <summary>
    /// Issues a non-LBA command (Identify, CacheFlushExt, …). Data-in
    /// commands (the Identify family) read up to one sector into the
    /// bounce buffer; everything else runs as a non-data command with an
    /// empty PRDT. Throws on failure.
    /// </summary>
    private void SendSataCommand(AtaCommands command)
    {
        bool hasData = command == AtaCommands.Identify
            || command == AtaCommands.IdentifyPacket
            || command == AtaCommands.IdentifyDma;
        AcquireIo();
        try
        {
            IssueCommandCore((byte)command, lba: 0, count: 0, isWrite: false, useLba48: false, hasData: hasData);
        }
        finally
        {
            ReleaseIo();
        }
    }

    /// <summary>
    /// Builds the command-list / FIS / PRDT for one in-flight command, rings
    /// the doorbell, and spins (bounded) until the slot drops. The caller
    /// holds the port's I/O lock. Throws on any failure — a silent return
    /// would let the caller consume stale bounce-buffer data as if the
    /// command had completed.
    /// </summary>
    /// <exception cref="IOException">No command slot is free, the port stays busy, the device aborted the command, or it never completed.</exception>
    private void IssueCommandCore(byte command, ulong lba, uint count, bool isWrite, bool useLba48, bool hasData)
    {
        // PxIS and PxSERR are RW1C: write all ones so stale error bits
        // (including TFES, bit 30) latched by a previous command can't
        // poison this one. Writing a partial mask (or zero) clears nothing.
        _portReg.IS = PortRegisters.Rw1CClearAll;
        _portReg.SERR = PortRegisters.Rw1CClearAll;

        int slot = FindCMDSlot();
        if (slot == -1)
        {
            throw new IOException("SATA: no free command slot.");
        }

        AhciController controller = _portReg.Controller;
        DmaBuffer region = controller.CommandRegion;
        int clbOffset = AhciController.CommandListOffset(PortNumber);
        int ctbaOffset = AhciController.CommandTableOffset(PortNumber, (uint)slot);
        ulong ctbaPhys = controller.DeviceAddressOf(ctbaOffset);

        HbaCommandHeader cmdHeader = new(region, clbOffset, (uint)slot);
        cmdHeader.CFL = FisRegisterH2D.FisDwordCount;
        cmdHeader.PRDTL = hasData ? (ushort)1 : (ushort)0;
        cmdHeader.Write = (byte)(isWrite ? 1 : 0);
        cmdHeader.CTBA = (uint)(ctbaPhys & AhciController.Low32BitsMask);
        cmdHeader.CTBAU = (uint)(ctbaPhys >> AhciController.High32Shift);

        HbaCommandTable cmdTable = new(region, ctbaOffset, cmdHeader.PRDTL);
        if (hasData)
        {
            HbaPrdtEntry entry = cmdTable.GetPrdtEntry(0);
            ulong bufferAddress = _bounceBuffer.DeviceAddress;
            entry.DBA = (uint)(bufferAddress & AhciController.Low32BitsMask);
            entry.DBAU = (uint)(bufferAddress >> AhciController.High32Shift);
            entry.DBC = useLba48 ? (uint)(count * RegularSectorSize) - 1 : (uint)RegularSectorSize - 1;
            entry.InterruptOnCompletion = 1;
        }

        FisRegisterH2D cmdFIS = new(region, cmdTable.CommandFisOffset);
        cmdFIS.FisType = (byte)FisType.RegisterH2D;
        cmdFIS.IsCommand = 1;
        cmdFIS.Command = command;
        cmdFIS.Device = useLba48 ? DeviceLbaMode : (byte)0;
        if (useLba48)
        {
            cmdFIS.LBA0 = (byte)((lba >> 0) & ByteMask);
            cmdFIS.LBA1 = (byte)((lba >> Byte1Shift) & ByteMask);
            cmdFIS.LBA2 = (byte)((lba >> Byte2Shift) & ByteMask);
            cmdFIS.LBA3 = (byte)((lba >> Byte3Shift) & ByteMask);
            cmdFIS.LBA4 = (byte)((lba >> Byte4Shift) & ByteMask);
            cmdFIS.LBA5 = (byte)((lba >> Byte5Shift) & ByteMask);
            cmdFIS.CountL = (byte)(count & ByteMask);
            cmdFIS.CountH = (byte)((count >> Byte1Shift) & ByteMask);
        }

        int spin = 0;
        while ((_portReg.TFD & TfdBusyDrqMask) != 0 && spin < RegisterPollSpinLimit)
        {
            spin++;
        }

        if (spin == RegisterPollSpinLimit)
        {
            throw new IOException("SATA: port stuck busy (TFD BSY/DRQ) before command issue.");
        }

        // Order the command header/table/FIS/bounce-buffer stores (Normal
        // memory) before the doorbell store (Device memory): ARM64 does not
        // order the two on its own, so the HBA could otherwise fetch a
        // half-written command. The region's write already fences; this
        // keeps the order explicit where the driver always had it.
        DmaBuffer.WriteBarrier();

        // Ring the doorbell for the slot the command was actually built in —
        // PxCI is one bit per slot.
        _portReg.CI = 1U << slot;

        uint waitSpin = 0;
        while (true)
        {
            if ((_portReg.CI & (1U << slot)) == 0)
            {
                break;
            }

            if ((_portReg.IS & (uint)InterruptStatus.TaskFileErrorStatus) != 0)
            {
                throw new IOException("SATA fatal error: command aborted.");
            }

            if (++waitSpin > CommandCompletionSpinLimit)
            {
                // Bounded like the TFD wait above: a wedged device (link
                // drop, hot removal) must not spin the kernel forever.
                throw new IOException("SATA: command completion timeout.");
            }
        }

        // Read barrier: the CI-clear observation is the completion signal;
        // the DMA'd bounce-buffer contents the caller consumes next must
        // not be read ahead of it on weakly-ordered ARM64.
        DmaBuffer.ReadBarrier();
    }

    private int FindCMDSlot()
    {
        // If not set in SACT and CI, the slot is free. Only CAP.NCS slots
        // are implemented — selecting a slot past that is undefined per
        // AHCI 1.3.1 (unreachable today with fully serialized I/O, but the
        // bound keeps a future concurrent path honest).
        uint slots = _portReg.SACT | _portReg.CI;
        uint slotCount = _portReg.Controller.NumberOfCommandSlots;

        for (int i = 0; i < slotCount; i++)
        {
            if ((slots & 1) == 0)
            {
                return i;
            }

            slots >>= 1;
        }

        return -1;
    }

    private void ReadDataBlock16(ushort[] buffer)
    {
        ReadOnlySpan<byte> bytes = _bounceBuffer.Span;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(i * sizeof(ushort)));
        }
    }

    private void ReadDataBlock8(Span<byte> data) => _bounceBuffer.Span.Slice(0, data.Length).CopyTo(data);

    private void WriteDataBlock8(ReadOnlySpan<byte> data) => data.CopyTo(_bounceBuffer.Span);

    private void AcquireIo()
    {
        while (Interlocked.CompareExchange(ref _ioBusy, 1, 0) != 0)
        {
            // Spin until the transfer holding the port finishes.
        }
    }

    private void ReleaseIo() => Interlocked.Exchange(ref _ioBusy, 0);
}
