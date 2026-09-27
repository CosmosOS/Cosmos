// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage.BulkOnly;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage.Scsi;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage;

/// <summary>
/// One logical unit of a USB mass storage interface (a USB stick, a card
/// reader slot, a USB disk) as a disk, named <c>usb</c> + a number no
/// other unit present uses, such as <c>usb0</c>. It speaks the SCSI block
/// commands (SBC-3) the class carries, over the interface's
/// <see cref="BulkOnlyTransport"/>, one command at a time with the
/// interface's other units.
///
/// <para>Error contract: a command the unit fails or that does not get
/// through the bus, and any I/O once the device left it, throws
/// <see cref="IOException"/>, so <see cref="ReadBlock"/> never hands back
/// data the unit did not send.</para>
/// </summary>
internal sealed class LogicalUnit : IBlockDevice
{
    // Command block lengths.
    private const int Command6Length = 6;
    private const int Command10Length = 10;
    private const int Command16Length = 16;

    // Command block fields.
    private const int ServiceActionOffset = 1;
    private const int Command6AllocationLengthOffset = 4;
    private const int LbaOffset = 2;
    private const int Command10BlockCountOffset = 7;
    private const int Command16BlockCountOffset = 10;
    private const int Command16AllocationLengthOffset = 10;

    /// <summary>The READ CAPACITY(16) service action of SERVICE ACTION IN(16).</summary>
    private const byte ReadCapacity16ServiceAction = 0x10;

    // READ CAPACITY data (SBC-3 s5.15.2, s5.16.2): the last LBA, then the block length.
    private const int ReadCapacity10Length = 8;
    private const int ReadCapacity16Length = 32;
    private const int Capacity10BlockLengthOffset = 4;
    private const int Capacity16BlockLengthOffset = 8;

    /// <summary>A READ CAPACITY(10) last LBA of all ones says the capacity only fits READ CAPACITY(16).</summary>
    private const uint Capacity10Overflow = 0xFFFFFFFF;

    /// <summary>First LBA past what a 10-byte READ or WRITE can address.</summary>
    private const ulong Command10LbaLimit = 1ul << 32;

    /// <summary>Largest READ or WRITE sent: Linux's usb-storage default is 120 KiB, since some devices fail larger ones.</summary>
    private const int MaxTransferLength = 64 * 1024;

    /// <summary>Runs of a command that fails for a reason that says nothing about the command itself.</summary>
    private const int CommandAttempts = 3;

    /// <summary>TEST UNIT READY polls while the medium spins up or the device finishes its reset: 5 s.</summary>
    private const int ReadyAttempts = 50;

    private const long ReadyRetryDelayMilliseconds = 100;

    private readonly UsbDeviceContext _context;
    private readonly BulkOnlyTransport _transport;
    private readonly byte _lun;

    /// <summary>Set once the unit rejected SYNCHRONIZE CACHE. Read and written holding the transport's turn.</summary>
    private bool _synchronizeCacheUnsupported;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ulong BlockCount { get; private set; }

    /// <inheritdoc />
    public ulong BlockSize { get; private set; }

    /// <summary>Number of the name, unique among the units present.</summary>
    internal uint Index { get; }

    /// <summary>Creates unit <paramref name="lun"/> of the interface behind <paramref name="context"/>, named <c>usb</c> + <paramref name="index"/>.</summary>
    /// <param name="context">The binding, which says whether the device is still on the bus and names the log lines.</param>
    /// <param name="transport">The transport of the interface the unit belongs to.</param>
    /// <param name="lun">The unit's number on that interface.</param>
    /// <param name="index">Number of the name, one no other unit present uses.</param>
    internal LogicalUnit(UsbDeviceContext context, BulkOnlyTransport transport, byte lun, uint index)
    {
        _context = context;
        _transport = transport;
        _lun = lun;
        Index = index;
        Name = $"usb{index}";
    }

    /// <summary>
    /// Identifies the unit and waits for its medium: INQUIRY, TEST UNIT
    /// READY until it passes, READ CAPACITY. Probe only: its commands run
    /// without taking the transport's turn, which no other thread asks for
    /// until Probe returns.
    /// </summary>
    /// <returns><see langword="false"/> when the unit is not a disk, has no medium or does not answer.</returns>
    internal bool Initialize()
    {
        Span<byte> sense = stackalloc byte[SenseData.Size];
        Span<byte> command = stackalloc byte[Command16Length];

        Span<byte> inquiryBuffer = stackalloc byte[InquiryData.Size];
        inquiryBuffer.Clear();
        command.Clear();
        command[0] = (byte)ScsiOpcode.Inquiry;
        command[Command6AllocationLengthOffset] = InquiryData.Size;
        if (Execute(command[..Command6Length], inquiryBuffer, [], sense, out uint residue) != BulkOnlyStatus.Passed
            || InquiryData.Size - residue < InquiryData.MinimumLength)
        {
            Log("INQUIRY failed");
            return false;
        }

        InquiryData inquiry = new(inquiryBuffer);
        if (!inquiry.IsDisk)
        {
            Log($"not a disk (peripheral 0x{inquiry.Peripheral:X2}), skipped");
            return false;
        }

        if (!WaitUntilReady(command, sense) || !ReadCapacity(command, sense))
        {
            return false;
        }

        Log($"{inquiry.Vendor} {inquiry.Product}, {BlockCount} blocks of {BlockSize} bytes");
        return true;
    }

    /// <inheritdoc />
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        ThrowIfRemoved();
        ThrowIfOutOfRange(blockNo, blockCount, (ulong)data.Length);

        Span<byte> sense = stackalloc byte[SenseData.Size];
        Span<byte> command = stackalloc byte[Command16Length];
        ulong maxBlocks = Math.Max((ulong)MaxTransferLength / BlockSize, 1);
        EnterTransport();
        try
        {
            while (blockCount != 0)
            {
                uint count = (uint)Math.Min(blockCount, maxBlocks);
                int length = (int)(count * BlockSize);
                int commandLength = WriteReadWriteCommand(command, write: false, blockNo, count);
                BulkOnlyStatus status = Execute(command[..commandLength], data[..length], [], sense, out uint residue);
                ThrowIfFailed("READ", status, residue, sense);

                data = data[length..];
                blockNo += count;
                blockCount -= count;
            }
        }
        finally
        {
            _transport.Exit();
        }
    }

    /// <inheritdoc />
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        ThrowIfRemoved();
        ThrowIfOutOfRange(blockNo, blockCount, (ulong)data.Length);

        Span<byte> sense = stackalloc byte[SenseData.Size];
        Span<byte> command = stackalloc byte[Command16Length];
        ulong maxBlocks = Math.Max((ulong)MaxTransferLength / BlockSize, 1);
        EnterTransport();
        try
        {
            while (blockCount != 0)
            {
                uint count = (uint)Math.Min(blockCount, maxBlocks);
                int length = (int)(count * BlockSize);
                int commandLength = WriteReadWriteCommand(command, write: true, blockNo, count);
                BulkOnlyStatus status = Execute(command[..commandLength], [], data[..length], sense, out uint residue);
                ThrowIfFailed("WRITE", status, residue, sense);

                data = data[length..];
                blockNo += count;
                blockCount -= count;
            }
        }
        finally
        {
            _transport.Exit();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Many flash drives have no cache to flush and reject SYNCHRONIZE
    /// CACHE; after the first rejection it is no longer sent.
    /// </remarks>
    public void Flush()
    {
        ThrowIfRemoved();

        Span<byte> sense = stackalloc byte[SenseData.Size];
        Span<byte> command = stackalloc byte[Command10Length];
        EnterTransport();
        try
        {
            if (_synchronizeCacheUnsupported)
            {
                return;
            }

            command.Clear();
            command[0] = (byte)ScsiOpcode.SynchronizeCache10;
            BulkOnlyStatus status = Execute(command, [], [], sense, out _);
            if (status == BulkOnlyStatus.Failed && new SenseData(sense).Key == SenseKey.IllegalRequest)
            {
                _synchronizeCacheUnsupported = true;
                return;
            }

            ThrowIfFailed("SYNCHRONIZE CACHE", status, 0, sense);
        }
        finally
        {
            _transport.Exit();
        }
    }

    /// <summary>
    /// Runs a command, again when it fails for a reason that says nothing
    /// about the command: a UNIT ATTENTION (the device telling of a reset
    /// or a medium change) or a transport error the transport recovered
    /// from.
    /// </summary>
    private BulkOnlyStatus Execute(ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut, Span<byte> sense, out uint residue)
    {
        for (int attempt = 1; ; attempt++)
        {
            BulkOnlyStatus status = _transport.Execute(_lun, command, dataIn, dataOut, sense, out residue);
            bool transient = status == BulkOnlyStatus.TransportError
                || (status == BulkOnlyStatus.Failed && new SenseData(sense).Key == SenseKey.UnitAttention);
            if (!transient || attempt >= CommandAttempts)
            {
                return status;
            }
        }
    }

    /// <summary>
    /// Polls TEST UNIT READY until the unit is ready. A card reader slot
    /// without a card answers "medium not present", and is given up at once.
    /// </summary>
    private bool WaitUntilReady(Span<byte> command, Span<byte> sense)
    {
        command.Clear();
        command[0] = (byte)ScsiOpcode.TestUnitReady;
        for (int attempt = 0; attempt < ReadyAttempts; attempt++)
        {
            BulkOnlyStatus status = Execute(command[..Command6Length], [], [], sense, out _);
            if (status == BulkOnlyStatus.Passed)
            {
                return true;
            }

            // Execute already retried it: a device that loses every command
            // is not going to become ready.
            if (status is BulkOnlyStatus.TransportError or BulkOnlyStatus.Disconnected)
            {
                Log("does not answer");
                return false;
            }

            if (new SenseData(sense).IsMediumNotPresent)
            {
                Log("no medium");
                return false;
            }

            _context.Delay(TimeSpan.FromMilliseconds(ReadyRetryDelayMilliseconds));
        }

        Log($"never became ready, {new SenseData(sense).Describe()}");
        return false;
    }

    /// <summary>Reads the geometry: READ CAPACITY(10), then (16) for a unit past 2^32 blocks.</summary>
    private bool ReadCapacity(Span<byte> command, Span<byte> sense)
    {
        Span<byte> capacity = stackalloc byte[ReadCapacity16Length];
        command.Clear();
        command[0] = (byte)ScsiOpcode.ReadCapacity10;
        if (Execute(command[..Command10Length], capacity[..ReadCapacity10Length], [], sense, out _) != BulkOnlyStatus.Passed)
        {
            Log($"READ CAPACITY failed, {new SenseData(sense).Describe()}");
            return false;
        }

        ulong lastLba = BinaryPrimitives.ReadUInt32BigEndian(capacity);
        uint blockLength = BinaryPrimitives.ReadUInt32BigEndian(capacity[Capacity10BlockLengthOffset..]);
        if (lastLba == Capacity10Overflow)
        {
            command.Clear();
            command[0] = (byte)ScsiOpcode.ServiceActionIn16;
            command[ServiceActionOffset] = ReadCapacity16ServiceAction;
            BinaryPrimitives.WriteUInt32BigEndian(command[Command16AllocationLengthOffset..], ReadCapacity16Length);
            if (Execute(command, capacity, [], sense, out _) != BulkOnlyStatus.Passed)
            {
                Log($"READ CAPACITY(16) failed, {new SenseData(sense).Describe()}");
                return false;
            }

            lastLba = BinaryPrimitives.ReadUInt64BigEndian(capacity);
            blockLength = BinaryPrimitives.ReadUInt32BigEndian(capacity[Capacity16BlockLengthOffset..]);
        }

        if (blockLength == 0 || blockLength > MaxTransferLength)
        {
            Log($"unsupported block length {blockLength}");
            return false;
        }

        BlockCount = lastLba + 1;
        BlockSize = blockLength;
        return true;
    }

    /// <summary>Fills a READ or WRITE command block: the 10-byte form while the LBA fits it, the 16-byte one past.</summary>
    /// <returns>The command block length.</returns>
    private static int WriteReadWriteCommand(Span<byte> command, bool write, ulong lba, uint blockCount)
    {
        command.Clear();
        if (lba + blockCount <= Command10LbaLimit)
        {
            command[0] = (byte)(write ? ScsiOpcode.Write10 : ScsiOpcode.Read10);
            BinaryPrimitives.WriteUInt32BigEndian(command[LbaOffset..], (uint)lba);
            BinaryPrimitives.WriteUInt16BigEndian(command[Command10BlockCountOffset..], (ushort)blockCount);
            return Command10Length;
        }

        command[0] = (byte)(write ? ScsiOpcode.Write16 : ScsiOpcode.Read16);
        BinaryPrimitives.WriteUInt64BigEndian(command[LbaOffset..], lba);
        BinaryPrimitives.WriteUInt32BigEndian(command[Command16BlockCountOffset..], blockCount);
        return Command16Length;
    }

    private void ThrowIfOutOfRange(ulong blockNo, ulong blockCount, ulong dataLength)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, dataLength / BlockSize, nameof(blockCount));
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo), "The blocks run past the end of the device.");
        }
    }

    /// <summary>Takes the transport's turn for this unit's I/O, or throws when the device left the bus.</summary>
    private void EnterTransport()
    {
        if (!_transport.TryEnter())
        {
            throw Removed();
        }
    }

    private void ThrowIfRemoved()
    {
        if (!_context.IsPresent)
        {
            throw Removed();
        }
    }

    /// <summary>Throws the error of a command that did not complete in full.</summary>
    private void ThrowIfFailed(string commandName, BulkOnlyStatus status, uint residue, ReadOnlySpan<byte> sense)
    {
        if (status == BulkOnlyStatus.Passed && residue == 0)
        {
            return;
        }

        // Pulled out mid-command: that is the error, not the command.
        if (status == BulkOnlyStatus.Disconnected)
        {
            throw Removed();
        }

        ThrowIfRemoved();

        string detail = status switch
        {
            BulkOnlyStatus.Passed => $"moved fewer bytes than asked, residue {residue}",
            BulkOnlyStatus.Failed => $"failed, {new SenseData(sense).Describe()}",
            _ => "did not get through the bus"
        };
        Log($"{commandName} {detail}");
        throw new IOException($"USB mass storage: {commandName} on {Name} {detail}.");
    }

    private IOException Removed() => new($"USB mass storage device {Name} was removed.");

    /// <summary>Logs <paramref name="message"/> about this unit. Thread context only: it builds a string.</summary>
    private void Log(string message) => _context.WriteLog($"{Name} (LUN {_lun}): {message}");
}
