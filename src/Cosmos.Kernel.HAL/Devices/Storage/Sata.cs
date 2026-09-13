// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// SATA drive port driver. The bounce buffer is a single page allocated
/// from <see cref="PageAllocator"/>; the HBA DMAs to its physical address
/// while <see cref="ReadDataBlock16"/> / <see cref="WriteDataBlock8"/>
/// touch the kernel-virtual address.
///
/// <para>Error contract: command issue throws on failure (no free slot,
/// device timeout, task-file error), so <see cref="ReadBlock"/> /
/// <see cref="WriteBlock"/> never silently hand back stale bounce-buffer
/// contents. All I/O on a port is serialized by an internal lock — the
/// port owns a single bounce buffer.</para>
/// </summary>
internal class Sata : BlockDevice
{
    /// <summary>
    /// Regular sector size (512 bytes).
    /// </summary>
    public const ulong RegularSectorSize = 512UL;

    /// <summary>
    /// Bounce-buffer capacity in sectors (one 4 KiB page). A command must
    /// never span more than this or the HBA would DMA past the page into
    /// adjacent kernel memory.
    /// </summary>
    private const uint MaxSectorsPerCommand = (uint)(PageAllocator.PageSize / RegularSectorSize);

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

    /// <summary>IDENTIFY word 10 - start of the serial number field.</summary>
    private const int IdentifyWordSerialNumber = 10;

    /// <summary>Serial number field length in characters (IDENTIFY words 10-19).</summary>
    private const int IdentifySerialNumberChars = 20;

    /// <summary>IDENTIFY word 23 - start of the firmware revision field.</summary>
    private const int IdentifyWordFirmwareRev = 23;

    /// <summary>Firmware revision field length in characters (IDENTIFY words 23-26).</summary>
    private const int IdentifyFirmwareRevChars = 8;

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
    private const int RegisterPollSpinLimit = 1000000;

    /// <summary>Bounded spin-loop iteration limit while waiting for command completion (PxCI slot clear).</summary>
    private const uint CommandCompletionSpinLimit = 50_000_000;

    /// <summary>Poll attempts while waiting for the port command engine (PxCMD.ST) to stop.</summary>
    private const int EngineStopRetries = 50;

    /// <summary><see cref="Ahci.Wait"/> ticks between PxCMD.ST stop polls.</summary>
    private const int EngineStopPollDelayTicks = 10000;

    /// <summary><see cref="Ahci.Wait"/> ticks to hold SCTL.DET = 1 during COMRESET.</summary>
    private const int ComresetHoldDelayTicks = 1000;

    private static uint s_nextIndex;

    private readonly string _name;
    private readonly PortRegisters _portReg;
    private readonly ulong _dataBufferVirt;
    private readonly ulong _dataBufferPhys;

    // Serializes command issue + bounce-buffer access on this port: the
    // buffer is shared state, so an unserialized concurrent WriteBlock
    // would interleave another caller's data into an in-flight command.
    private SchedSpinLock _ioLock;

    /// <inheritdoc />
    public override string Name => _name;

    public uint PortNumber => _portReg.PortNumber;

    public string SerialNo { get; }
    public string FirmwareRev { get; }
    public string ModelNo { get; }

    public unsafe Sata(PortRegisters portReg)
    {
        // Check if it is really a SATA Port!
        if (portReg.PortType != PortType.Sata || (portReg.CMD & (uint)CommandAndStatus.ATAPIDevice) != 0)
        {
            // Constant message on purpose: this ctor runs in the phase-3 init
            // window where CoreLib int formatting triple-faults, and this guard
            // is exactly the path GetPorts' try/catch is meant to contain — an
            // interpolated port number would crash instead of throwing.
            Serial.WriteString("[SATA] Port ");
            Serial.WriteNumber(portReg.PortNumber);
            Serial.WriteString(" is not a SATA device\n");
            throw new Exception("AHCI port is not a SATA device");
        }

        _portReg = portReg;

        // Unique per device instance ("sata0", "sata1", ...) so multi-disk
        // systems get distinguishable device and partition names. Built via
        // BuildDeviceName: CoreLib int formatting crashes this early in boot.
        _name = BuildDeviceName("sata", s_nextIndex++);

        // One page (4 KiB) of contiguous DMA-able memory, well above the 2-byte
        // alignment AHCI's PRDT.DBA requires. PageAllocator hands out the
        // kernel-virtual; VirtualToPhysical gets the address the HBA will see.
        // OOM at boot would have killed everything earlier, so we trust the
        // alloc; logging on the 32-bit-controller / high-physical edge case
        // (the HBA will then see a truncated DMA address) is enough.
        _dataBufferVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        _dataBufferPhys = PageAllocator.VirtualToPhysical(_dataBufferVirt);

        if (!_portReg.Controller.Supports64BitAddressing && _dataBufferPhys > AhciController.Max32BitAddress)
        {
            Serial.WriteString("[SATA] WARNING: 32-bit-only controller with bounce buffer above 4 GiB at phys=0x");
            Serial.WriteHex(_dataBufferPhys);
            Serial.WriteString(" — DMA addressing will be truncated\n");
        }

        // Setting block size
        BlockSize = RegularSectorSize;

        // Send Identify command to get drive info
        SendSataCommand(AtaCommands.Identify);

        // Read identify data
        ushort[] xBuffer = new ushort[IdentifyBufferWords];
        ReadDataBlock16(xBuffer);

        SerialNo = GetString(xBuffer, IdentifyWordSerialNumber, IdentifySerialNumberChars);
        FirmwareRev = GetString(xBuffer, IdentifyWordFirmwareRev, IdentifyFirmwareRevChars);
        ModelNo = GetString(xBuffer, IdentifyWordModelNumber, IdentifyModelNumberChars);

        // Capacity: all I/O here is issued as READ/WRITE DMA EXT, so prefer
        // the 48-bit count in IDENTIFY words 100-103 (word 83 bit 10 = 48-bit
        // feature set supported). Words 60-61 saturate at 0x0FFFFFFF
        // (~128 GiB) and would silently truncate larger disks — misplacing
        // anything derived from BlockCount, like the backup GPT header.
        if ((xBuffer[IdentifyWordCommandSets] & Lba48SupportedMask) != 0)
        {
            BlockCount = (ulong)xBuffer[IdentifyWordLba48Sectors3] << IdentifyWord3Shift | (ulong)xBuffer[IdentifyWordLba48Sectors2] << IdentifyWord2Shift
                       | (ulong)xBuffer[IdentifyWordLba48Sectors1] << IdentifyWord1Shift | xBuffer[IdentifyWordLba48Sectors0];
        }
        else
        {
            BlockCount = (uint)xBuffer[IdentifyWordLba28SectorsHigh] << IdentifyWord1Shift | xBuffer[IdentifyWordLba28SectorsLow];
        }

        Serial.WriteString("[SATA] Initialized: ");
        Serial.WriteString(ModelNo.Trim());
        Serial.WriteString(" (");
        Serial.WriteNumber(BlockCount);
        Serial.WriteString(" sectors)\n");
    }

    /// <summary>
    /// Issue a non-LBA command (Identify, CacheFlushExt, …). Data-in
    /// commands (the Identify family) read up to one sector into the
    /// bounce buffer; everything else runs as a non-data command with an
    /// empty PRDT. Throws on failure.
    /// </summary>
    public void SendSataCommand(AtaCommands command)
    {
        bool hasData = command == AtaCommands.Identify
            || command == AtaCommands.IdentifyPacket
            || command == AtaCommands.IdentifyDma;
        _ioLock.Acquire();
        try
        {
            IssueCommandCore((byte)command, lba: 0, count: 0, isWrite: false, useLba48: false, hasData: hasData);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// Issue an LBA48 read or write spanning <paramref name="count"/>
    /// sectors starting at <paramref name="start"/>. The transfer moves
    /// through the port's single-page bounce buffer, so
    /// <paramref name="count"/> is limited to
    /// <see cref="MaxSectorsPerCommand"/>. Throws on failure.
    /// </summary>
    public void SendSata48Command(AtaCommands command, ulong start, uint count)
    {
        if (count == 0 || count > MaxSectorsPerCommand)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "SATA transfers are limited to the single-page bounce buffer.");
        }

        bool isWrite = command == AtaCommands.WriteDmaExt || command == AtaCommands.WriteDma;
        _ioLock.Acquire();
        try
        {
            IssueCommandCore((byte)command, start, count, isWrite, useLba48: true, hasData: true);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    /// <summary>
    /// Build the command-list / FIS / PRDT for one in-flight command, ring
    /// the doorbell, and spin (bounded) until the slot drops. Caller must
    /// hold <see cref="_ioLock"/>. Throws on any failure — a silent return
    /// would let the caller consume stale bounce-buffer data as if the
    /// command had completed.
    /// </summary>
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
            throw new InvalidOperationException("SATA: no free command slot.");
        }

        ulong clbVirt = _portReg.Controller.PortCommandListVirt(PortNumber);
        ulong ctbaPhys = _portReg.Controller.PortCommandTablePhys(PortNumber, (uint)slot);
        ulong ctbaVirt = _portReg.Controller.PortCommandTableVirt(PortNumber, (uint)slot);

        HbaCommandHeader cmdHeader = new(clbVirt, (uint)slot)
        {
            CFL = FisRegisterH2D.FisDwordCount,
            PRDTL = hasData ? (ushort)1 : (ushort)0,
            Write = (byte)(isWrite ? 1 : 0),
            CTBA = (uint)(ctbaPhys & AhciController.Low32BitsMask),
            CTBAU = (uint)(ctbaPhys >> AhciController.High32Shift)
        };

        HbaCommandTable cmdTable = new(ctbaVirt, cmdHeader.PRDTL);
        if (hasData)
        {
            cmdTable.PRDTEntry[0].DBA = (uint)(_dataBufferPhys & AhciController.Low32BitsMask);
            cmdTable.PRDTEntry[0].DBAU = (uint)(_dataBufferPhys >> AhciController.High32Shift);
            cmdTable.PRDTEntry[0].DBC = useLba48 ? (uint)(count * RegularSectorSize) - 1 : (uint)RegularSectorSize - 1;
            cmdTable.PRDTEntry[0].InterruptOnCompletion = 1;
        }

        FisRegisterH2D cmdFIS = new(cmdTable.CFIS)
        {
            FisType = (byte)FisType.RegisterH2D,
            IsCommand = 1,
            Command = command,
            Device = useLba48 ? DeviceLbaMode : (byte)0
        };
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
            throw new InvalidOperationException("SATA: port stuck busy (TFD BSY/DRQ) before command issue.");
        }

        // Order the command header/table/FIS/bounce-buffer stores (Normal
        // memory) before the doorbell store (Device memory): ARM64 does not
        // order the two on its own, so the HBA could otherwise fetch a
        // half-written command.
        PlatformHAL.Initializer?.DmaBarrier();

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
                throw new Exception("SATA Fatal error: Command aborted");
            }
            if (++waitSpin > CommandCompletionSpinLimit)
            {
                // Bounded like the TFD wait above: a wedged device (link
                // drop, hot removal) must not spin the kernel forever.
                throw new InvalidOperationException("SATA: command completion timeout.");
            }
        }

        // Read barrier: the CI-clear observation is the completion signal;
        // the DMA'd bounce-buffer contents the caller consumes next must
        // not be read ahead of it on weakly-ordered ARM64.
        PlatformHAL.Initializer?.DmaBarrier();
    }

    /// <summary>
    /// Reset the port. Returns false when the command engine refused to
    /// stop — the port must then be left untouched (reprogramming CLB/FB
    /// on a running engine lets the HBA fetch garbage command headers and
    /// issue arbitrary DMA), not fixed up with a whole-HBA reset that
    /// would wipe every sibling port.
    /// </summary>
    public static bool PortReset(PortRegisters port)
    {
        // Stop the port command engine, then wait for CMD.ST to actually clear.
        port.CMD &= ~(uint)CommandAndStatus.StartProcess;
        for (int i = 0; i <= EngineStopRetries; i++)
        {
            if ((port.CMD & (uint)CommandAndStatus.StartProcess) == 0)
            {
                break;
            }
            Ahci.Wait(EngineStopPollDelayTicks);
        }
        if ((port.CMD & (uint)CommandAndStatus.StartProcess) != 0)
        {
            Serial.WriteString("[SATA] Port engine refused to stop; leaving port offline\n");
            return false;
        }

        // COMRESET: set SCTL.DET=1 while preserving the SPD/IPM restriction
        // fields, hold it, then clear DET so the PHY retrains.
        port.SCTL = (port.SCTL & ~PortRegisters.SctlDetMask) | PortRegisters.SctlDetComreset;
        Ahci.Wait(ComresetHoldDelayTicks);
        port.SCTL &= ~PortRegisters.SctlDetMask;

        // Wait (bounded) for the PHY to report an established link (DET == 3); a
        // missing or wedged device must not spin the boot CPU forever.
        int spin = 0;
        while ((DeviceDetectionStatus)(port.SSTS & PortRegisters.SstsDetMask) != DeviceDetectionStatus.DeviceDetectedWithPhy && spin < RegisterPollSpinLimit)
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

    private string GetString(ushort[] buffer, int indexStart, int stringLength)
    {
        char[] chars = new char[stringLength];
        for (int i = 0; i < stringLength / 2; i++)
        {
            ushort xChar = buffer[indexStart + i];
            chars[i * 2] = (char)(xChar >> Byte1Shift);
            chars[i * 2 + 1] = (char)xChar;
        }
        return new string(chars);
    }

    private unsafe void ReadDataBlock16(ushort[] buffer)
    {
        ushort* ptr = (ushort*)_dataBufferVirt;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = ptr[i];
        }
    }

    private unsafe void ReadDataBlock8(Span<byte> data)
    {
        byte* ptr = (byte*)_dataBufferVirt;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ptr[i];
        }
    }

    private unsafe void WriteDataBlock8(ReadOnlySpan<byte> data)
    {
        byte* ptr = (byte*)_dataBufferVirt;
        for (int i = 0; i < data.Length; i++)
        {
            ptr[i] = data[i];
        }
    }

    // BlockDevice implementation. The bounce buffer (_dataBufferVirt) is a
    // single page, so I/O is chunked at MaxSectorsPerCommand sectors per
    // command; the port lock keeps each command + bounce-buffer copy atomic
    // against other threads. The up-front span check plus long offset math
    // replaces the old `(int)i * sector`, which wrapped for >= 4M-block
    // spans and silently sliced at wrong offsets.
    /// <inheritdoc />
    public override void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int sector = (int)BlockSize;
        if (blockCount > (ulong)data.Length / (uint)sector)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCount), "Span shorter than the requested transfer.");
        }

        _ioLock.Acquire();
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
            _ioLock.Release();
        }
    }

    /// <inheritdoc />
    public override void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int sector = (int)BlockSize;
        if (blockCount > (ulong)data.Length / (uint)sector)
        {
            throw new ArgumentOutOfRangeException(nameof(blockCount), "Span shorter than the requested transfer.");
        }

        _ioLock.Acquire();
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
            _ioLock.Release();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Issues FLUSH CACHE EXT so completed writes reach stable media.
    /// WriteBlock deliberately does not flush per write — durability is the
    /// caller's policy via <see cref="Flush"/>, matching the shared
    /// IBlockDevice contract (and NVMe's behavior).
    /// </remarks>
    public override void Flush()
    {
        _ioLock.Acquire();
        try
        {
            IssueCommandCore((byte)AtaCommands.CacheFlushExt, lba: 0, count: 0, isWrite: false, useLba48: false, hasData: false);
        }
        finally
        {
            _ioLock.Release();
        }
    }
}
