// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Storage.Ahci;

/// <summary>
/// One SATAPI port of an AHCI HBA: a CD-ROM or DVD drive, the block device
/// <see cref="AhciDriver"/> publishes to the ring as <c>cdrom{n}</c>, with
/// the strings INQUIRY reported and the geometry READ CAPACITY(10) gave.
/// The device takes SCSI MMC command blocks through the ATA PACKET
/// command: the 12-byte block sits in the command table's ATAPI area and
/// the header's A bit tells the HBA to send it. Every transfer moves
/// through the port's one bounce page, at most
/// <see cref="MaxSectorsPerCommand"/> sectors per command, one command in
/// flight per port under the controller's lock, polled to completion, as
/// on a SATA port. The medium is read-only: <see cref="WriteBlock"/>
/// throws and <see cref="Flush"/> has nothing to do. Every method is thread
/// context, entered by the ring from any thread; a command the device
/// rejects surfaces as an <see cref="IOException"/> naming the command, a
/// stuck port or a timeout as an <see cref="InvalidOperationException"/>.
/// </summary>
internal sealed class AhciAtapiPort : IBlockDevice, IRemovableMedia
{
    // --- Constants ---

    /// <summary>Bytes of one page: the bounce buffer is one, page aligned. The kit exposes no page size.</summary>
    private const int PageBytes = 4096;

    /// <summary>Sectors one command moves at most: one bounce page, one PRDT entry.</summary>
    private const uint MaxSectorsPerCommand = 2;

    /// <summary>How long the task file gets to leave BSY and DRQ before a command is issued.</summary>
    private const uint TaskFileMilliseconds = 1000;

    /// <summary>How many times TEST UNIT READY is asked before the drive is given up as not ready.</summary>
    private const int ReadyAttempts = 20;

    /// <summary>Pause between two TEST UNIT READY polls, long enough for a drive spinning up.</summary>
    private const uint ReadyRetryMilliseconds = 100;

    /// <summary>Bits in the low dword of a 64-bit address.</summary>
    private const int DwordBits = 32;

    /// <summary>Bits in one byte, for the byte count lanes of the FIS.</summary>
    private const int ByteBits = 8;

    /// <summary>The first printable ASCII code in an INQUIRY string.</summary>
    private const byte FirstPrintable = 0x20;

    /// <summary>One past the last printable ASCII code in an INQUIRY string.</summary>
    private const byte PastPrintable = 0x7F;

    // --- Private fields ---

    private readonly AhciState _controller;
    private readonly DeviceBinding _binding;
    private readonly uint _portNumber;
    private readonly string _name;
    private readonly DmaBuffer _bounce;
    private readonly ulong _blockCount;
    private readonly string _vendor;
    private readonly string _product;
    private bool _busy;

    // --- Constructor ---

    /// <summary>
    /// Brings the port up: allocates the bounce page (below 4 GiB on a
    /// 32-bit HBA), asks INQUIRY for the device's kind and strings, polls
    /// TEST UNIT READY until the medium is ready, and reads the geometry
    /// with READ CAPACITY(10). Thread context, from the probe, after the
    /// port was rebased and its engine started.
    /// </summary>
    /// <param name="controller">The HBA the port belongs to.</param>
    /// <param name="binding">The function's binding, for the bounce page and the delays.</param>
    /// <param name="portNumber">The port index on the HBA.</param>
    /// <param name="index">The port's <c>cdrom{index}</c> number, global across controllers.</param>
    /// <exception cref="InvalidOperationException">The device is not a CD-ROM, has no medium, never became ready, no bounce page could be allocated, or a command did not complete.</exception>
    /// <exception cref="IOException">A command ended in a task file error.</exception>
    internal AhciAtapiPort(AhciState controller, DeviceBinding binding, uint portNumber, int index)
    {
        _controller = controller;
        _binding = binding;
        _portNumber = portNumber;
        _name = "cdrom" + index;

        if (controller.Supports64Bit)
        {
            _bounce = binding.AllocateDma(PageBytes, PageBytes);
        }
        else if (binding.TryAllocateDma(PageBytes, PageBytes, DmaConstraints.Addressable32Bit, out DmaBuffer? bounce))
        {
            _bounce = bounce;
        }
        else
        {
            throw new InvalidOperationException("no DMA memory below 4 GiB for a 32-bit controller");
        }

        Span<byte> command = stackalloc byte[AhciProtocol.AtapiCommandBytes];
        Span<byte> inquiry = stackalloc byte[AhciProtocol.InquiryBytes];
        Claim();
        try
        {
            command.Clear();
            command[0] = AhciProtocol.ScsiInquiry;
            command[AhciProtocol.Command6AllocationLengthOffset] = AhciProtocol.InquiryBytes;
            IssuePacket(command, (uint)inquiry.Length);
            _bounce.Span.Slice(0, inquiry.Length).CopyTo(inquiry);
        }
        finally
        {
            Release();
        }

        byte peripheralType = (byte)(inquiry[0] & AhciProtocol.InquiryPeripheralTypeMask);
        if (peripheralType != AhciProtocol.PeripheralCdRom)
        {
            throw new InvalidOperationException($"not a CD-ROM device (peripheral type 0x{peripheralType:X2})");
        }

        _vendor = InquiryString(inquiry.Slice(AhciProtocol.InquiryVendorOffset, AhciProtocol.InquiryVendorBytes));
        _product = InquiryString(inquiry.Slice(AhciProtocol.InquiryProductOffset, AhciProtocol.InquiryProductBytes));

        WaitUntilReady(command);
        _blockCount = ReadCapacity(command);
    }

    // --- IBlockDevice ---

    /// <inheritdoc/>
    public ulong BlockCount => _blockCount;

    /// <summary>2048: the logical block of a CD-ROM, the size READ CAPACITY reported. Any context.</summary>
    public ulong BlockSize => AhciProtocol.CdSectorBytes;

    /// <summary><c>cdrom{index}</c>, the number taken from the driver's counter before INQUIRY. Any context.</summary>
    public string Name => _name;

    /// <summary>
    /// Reads whole sectors with READ(12), at most
    /// <see cref="MaxSectorsPerCommand"/> per command through the bounce
    /// page, the port claimed per chunk. Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first sector.</param>
    /// <param name="blockCount">How many sectors.</param>
    /// <param name="data">Where they go; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the sectors asked for, or the sectors run past the medium.</exception>
    /// <exception cref="IOException">The drive rejected a command.</exception>
    /// <exception cref="InvalidOperationException">No free slot, the port stuck busy, or a command timed out.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int sector = (int)AhciProtocol.CdSectorBytes;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));
        if (blockNo > _blockCount || blockCount > _blockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo), "The sectors run past the end of the medium.");
        }

        Span<byte> command = stackalloc byte[AhciProtocol.AtapiCommandBytes];
        ulong done = 0;
        while (done < blockCount)
        {
            ulong remaining = blockCount - done;
            uint chunk = remaining >= MaxSectorsPerCommand ? MaxSectorsPerCommand : (uint)remaining;
            int bytes = sector * (int)chunk;
            Claim();
            try
            {
                command.Clear();
                command[0] = AhciProtocol.ScsiRead12;
                BinaryPrimitives.WriteUInt32BigEndian(command.Slice(AhciProtocol.Read12LbaOffset), (uint)(blockNo + done));
                BinaryPrimitives.WriteUInt32BigEndian(command.Slice(AhciProtocol.Read12LengthOffset), chunk);
                IssuePacket(command, (uint)bytes);
                _bounce.Span.Slice(0, bytes).CopyTo(data.Slice((int)((long)done * sector), bytes));
            }
            finally
            {
                Release();
            }

            done += chunk;
        }
    }

    /// <summary>A CD-ROM takes no writes. Any context.</summary>
    /// <exception cref="IOException">Always: the medium is read-only.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        throw new IOException($"{_name} is read-only.");
    }

    /// <summary>A CD-ROM has no volatile cache: nothing to do, as <see cref="IBlockDevice.Flush"/> allows for such a device. Any context.</summary>
    public void Flush()
    {
    }

    // --- IRemovableMedia ---

    /// <summary>True: the medium takes no writes. Any context.</summary>
    public bool IsReadOnly => true;

    /// <summary>True: the disc can be taken out. Any context.</summary>
    public bool IsRemovable => true;

    /// <summary>True: the probe published the port only once TEST UNIT READY passed, and the driver does not watch the drive for a disc taken out afterwards. Any context.</summary>
    public bool HasMedia => true;

    // --- Properties ---

    /// <summary>The vendor identification INQUIRY reported, trimmed. Any context.</summary>
    public string Vendor => _vendor;

    /// <summary>The product identification INQUIRY reported, trimmed. Any context.</summary>
    public string Product => _product;

    // --- Private methods ---

    /// <summary>
    /// Polls TEST UNIT READY until it passes: a drive answers "not ready,
    /// becoming ready" while the disc spins up, and "unit attention" once
    /// after a reset, which clears on the next command. A drive with no
    /// disc is given up at once. Thread context, the probe.
    /// </summary>
    /// <exception cref="InvalidOperationException">No medium, or the drive never became ready.</exception>
    private void WaitUntilReady(Span<byte> command)
    {
        Span<byte> sense = stackalloc byte[AhciProtocol.SenseBytes];
        for (int attempt = 0; attempt < ReadyAttempts; attempt++)
        {
            Claim();
            bool ready;
            try
            {
                command.Clear();
                command[0] = AhciProtocol.ScsiTestUnitReady;
                ready = TryIssuePacket(command, 0);
                if (!ready)
                {
                    RequestSense(command, sense);
                }
            }
            finally
            {
                Release();
            }

            if (ready)
            {
                return;
            }

            byte senseKey = (byte)(sense[AhciProtocol.SenseKeyOffset] & AhciProtocol.SenseKeyMask);
            if (senseKey == AhciProtocol.SenseKeyNotReady && sense[AhciProtocol.SenseAdditionalCodeOffset] == AhciProtocol.SenseMediumNotPresent)
            {
                throw new InvalidOperationException("no medium in the drive");
            }

            if (senseKey != AhciProtocol.SenseKeyUnitAttention)
            {
                _binding.Sleep(ReadyRetryMilliseconds);
            }
        }

        throw new InvalidOperationException("the drive never became ready");
    }

    /// <summary>Reads the geometry with READ CAPACITY(10) and checks the block length is a CD sector. Thread context, the probe.</summary>
    /// <returns>The sector count: the last LBA plus one.</returns>
    /// <exception cref="InvalidOperationException">The block length is not 2048.</exception>
    private ulong ReadCapacity(Span<byte> command)
    {
        Span<byte> capacity = stackalloc byte[AhciProtocol.ReadCapacity10Bytes];
        Claim();
        try
        {
            command.Clear();
            command[0] = AhciProtocol.ScsiReadCapacity10;
            IssuePacket(command, (uint)capacity.Length);
            _bounce.Span.Slice(0, capacity.Length).CopyTo(capacity);
        }
        finally
        {
            Release();
        }

        uint lastLba = BinaryPrimitives.ReadUInt32BigEndian(capacity);
        uint blockLength = BinaryPrimitives.ReadUInt32BigEndian(capacity.Slice(AhciProtocol.ReadCapacity10BlockLengthOffset));
        if (blockLength != AhciProtocol.CdSectorBytes)
        {
            throw new InvalidOperationException($"unsupported block length {blockLength}");
        }

        return (ulong)lastLba + 1;
    }

    /// <summary>Asks REQUEST SENSE why the last command failed, into <paramref name="sense"/>; a failure of the request itself leaves it zeroed. The caller holds the busy flag. Thread context.</summary>
    private void RequestSense(Span<byte> command, Span<byte> sense)
    {
        sense.Clear();
        command.Clear();
        command[0] = AhciProtocol.ScsiRequestSense;
        command[AhciProtocol.Command6AllocationLengthOffset] = AhciProtocol.SenseBytes;
        if (TryIssuePacket(command, AhciProtocol.SenseBytes))
        {
            _bounce.Span.Slice(0, sense.Length).CopyTo(sense);
        }
    }

    /// <summary>
    /// Claims the port's busy flag under the controller's lock, pausing
    /// <see cref="AhciState.PollMicroseconds"/> outside the lock while
    /// another caller holds it; no deadline, since the holder's command is
    /// bounded by <see cref="AhciState.CommandTimeoutMilliseconds"/>.
    /// Thread context; any thread.
    /// </summary>
    private void Claim()
    {
        DeviceLock deviceLock = _controller.RequireLock();
        bool claimed = false;
        while (!claimed)
        {
            using (deviceLock.Acquire())
            {
                if (!_busy)
                {
                    _busy = true;
                    claimed = true;
                }
            }

            if (!claimed)
            {
                _binding.Delay(AhciState.PollMicroseconds);
            }
        }
    }

    /// <summary>Releases the port's busy flag under the controller's lock. Thread context; any thread.</summary>
    private void Release()
    {
        using (_controller.RequireLock().Acquire())
        {
            _busy = false;
        }
    }

    /// <summary>Issues a packet command and throws when the drive rejected it. The caller holds the busy flag. Thread context.</summary>
    /// <exception cref="IOException">The drive rejected the command.</exception>
    /// <exception cref="InvalidOperationException">No slot was free, the port stuck busy, or the command timed out.</exception>
    private void IssuePacket(ReadOnlySpan<byte> command, uint dataBytes)
    {
        if (!TryIssuePacket(command, dataBytes))
        {
            throw new IOException($"ATAPI command 0x{command[0]:X2} rejected by {_name}.");
        }
    }

    /// <summary>
    /// Builds and issues one PACKET command on a free slot and polls it to
    /// completion: clears PxIS and PxSERR, writes the header with its A bit,
    /// the PRDT entry over the bounce page when the command moves data, the
    /// H2D FIS with the DMA feature and the byte count in the LBA mid and
    /// high lanes (the ATAPI byte count limit the device echoes), and the
    /// command block in the table's ATAPI area; waits for the task file to
    /// leave BSY and DRQ, orders the stores, writes PxCI, then polls the
    /// slot's bit until it clears. A task file error is the drive's answer
    /// (a check condition), not a port fault: it clears the error and
    /// returns false so the caller can ask REQUEST SENSE. The caller holds
    /// the busy flag. Thread context; any thread.
    /// </summary>
    /// <param name="command">The command block, 12 or 16 bytes.</param>
    /// <param name="dataBytes">Bytes the device returns through the bounce page, or 0.</param>
    /// <returns>False when the drive rejected the command.</returns>
    /// <exception cref="InvalidOperationException">No slot was free, the port stuck busy, or the command timed out.</exception>
    private bool TryIssuePacket(ReadOnlySpan<byte> command, uint dataBytes)
    {
        uint port = _portNumber;
        _controller.WritePort(port, AhciProtocol.PxIs, AhciProtocol.Rw1CClearAll);
        _controller.WritePort(port, AhciProtocol.PxSerr, AhciProtocol.Rw1CClearAll);

        int slot = FindFreeSlot();
        if (slot < 0)
        {
            throw new InvalidOperationException("SATAPI: no free command slot.");
        }

        DmaBuffer region = _controller.CommandRegion;
        ulong tablePhysical = region.PhysicalAddress + (ulong)AhciProtocol.CommandTableOffset(port, slot);
        Span<byte> list = region.Span.Slice(AhciProtocol.CommandListOffset(port), AhciProtocol.CommandListBytes);
        Span<HbaCommandHeader> headers = MemoryMarshal.Cast<byte, HbaCommandHeader>(list);
        bool hasData = dataBytes > 0;
        headers[slot] = new HbaCommandHeader
        {
            Flags0 = (byte)(AhciProtocol.HeaderFisDwords | AhciProtocol.HeaderAtapi),
            PrdtLength = hasData ? (ushort)1 : (ushort)0,
            CommandTableBase = (uint)tablePhysical,
            CommandTableBaseUpper = (uint)(tablePhysical >> DwordBits),
        };

        Span<byte> table = region.Span.Slice(AhciProtocol.CommandTableOffset(port, slot), AhciProtocol.CommandTableBytes);
        if (hasData)
        {
            ulong bouncePhysical = _bounce.PhysicalAddress;
            Span<HbaPrdtEntry> prdt = MemoryMarshal.Cast<byte, HbaPrdtEntry>(table.Slice(AhciProtocol.CommandTablePrdtOffset, AhciProtocol.PrdtEntryBytes));
            prdt[0] = new HbaPrdtEntry
            {
                DataBase = (uint)bouncePhysical,
                DataBaseUpper = (uint)(bouncePhysical >> DwordBits),
                ByteCountAndInterrupt = ((dataBytes - 1) & AhciProtocol.PrdtByteCountMask) | AhciProtocol.PrdtInterruptOnCompletion,
            };
        }

        Span<byte> atapi = table.Slice(AhciProtocol.CommandTableAtapiOffset, AhciProtocol.AtapiCommandBytes);
        atapi.Clear();
        command.CopyTo(atapi);

        Span<FisRegisterH2D> fis = MemoryMarshal.Cast<byte, FisRegisterH2D>(table.Slice(AhciProtocol.CommandTableFisOffset, AhciProtocol.FisRegisterH2DBytes));
        fis[0] = new FisRegisterH2D
        {
            FisType = AhciProtocol.FisTypeRegisterH2D,
            Flags = AhciProtocol.FisCommandFlag,
            Command = AhciProtocol.AtaPacket,
            FeaturesLow = hasData ? AhciProtocol.PacketFeatureDma : (byte)0,
            Lba1 = (byte)dataBytes,
            Lba2 = (byte)(dataBytes >> ByteBits),
        };

        long busyDeadline = AhciState.DeadlineAfter(TaskFileMilliseconds);
        while ((_controller.ReadPort(port, AhciProtocol.PxTfd) & (AhciProtocol.TfdBusy | AhciProtocol.TfdDataRequest)) != 0)
        {
            if (Stopwatch.GetTimestamp() >= busyDeadline)
            {
                throw new InvalidOperationException("SATAPI: port stuck busy (TFD BSY/DRQ) before command issue.");
            }

            _binding.Delay(AhciState.PollMicroseconds);
        }

        DmaBuffer.WriteBarrier();
        uint slotBit = 1u << slot;
        _controller.WritePort(port, AhciProtocol.PxCi, slotBit);

        long deadline = AhciState.DeadlineAfter(AhciState.CommandTimeoutMilliseconds);
        while ((_controller.ReadPort(port, AhciProtocol.PxCi) & slotBit) != 0)
        {
            if ((_controller.ReadPort(port, AhciProtocol.PxIs) & AhciProtocol.IsTaskFileError) != 0)
            {
                RecoverFromCheckCondition();
                return false;
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                _controller.CountTimeout();
                throw new InvalidOperationException("SATAPI: command completion timeout.");
            }

            _binding.Delay(AhciState.PollMicroseconds);
        }

        DmaBuffer.ReadBarrier();
        _controller.CountCommand();
        return true;
    }

    /// <summary>
    /// Puts the port back to work after a check condition: the HBA halts
    /// the command list on a task file error (AHCI 1.3.1 section 6.2.2.1),
    /// so the engine is stopped, PxSERR and PxIS cleared, and the engine
    /// started again. Thread context.
    /// </summary>
    private void RecoverFromCheckCondition()
    {
        _controller.StopCommandEngine(_portNumber);
        _controller.WritePort(_portNumber, AhciProtocol.PxSerr, AhciProtocol.Rw1CClearAll);
        _controller.WritePort(_portNumber, AhciProtocol.PxIs, AhciProtocol.Rw1CClearAll);
        _controller.StartCommandEngine(_portNumber);
    }

    /// <summary>The lowest slot clear in both PxSACT and PxCI, below the HBA's slot count, or -1. Any thread.</summary>
    private int FindFreeSlot()
    {
        uint used = _controller.ReadPort(_portNumber, AhciProtocol.PxSact) | _controller.ReadPort(_portNumber, AhciProtocol.PxCi);
        int slots = _controller.CommandSlots;
        for (int i = 0; i < slots; i++)
        {
            if ((used & (1u << i)) == 0)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>An INQUIRY text field as a string: ASCII padded with spaces, which are dropped; a byte outside printable ASCII reads as a question mark. Any context.</summary>
    private static string InquiryString(ReadOnlySpan<byte> field)
    {
        int length = field.Length;
        while (length > 0 && field[length - 1] is (byte)' ' or 0)
        {
            length--;
        }

        Span<char> text = stackalloc char[AhciProtocol.InquiryProductBytes];
        for (int i = 0; i < length; i++)
        {
            byte b = field[i];
            text[i] = b is >= FirstPrintable and < PastPrintable ? (char)b : '?';
        }

        return new string(text.Slice(0, length));
    }
}
