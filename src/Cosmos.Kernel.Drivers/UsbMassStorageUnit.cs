// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// One logical unit of a USB mass storage device (a USB stick, a card
/// reader slot, a USB disk), the block device <see cref="UsbMassStorageDriver"/>
/// publishes to the ring as <c>usb{index}</c>. It speaks the SCSI block
/// commands (SBC-3) the mass storage class carries, over the interface's
/// <see cref="UsbBulkOnlyTransport"/>, in chunks of at most 64 KiB. Once the
/// device is unplugged or its interface detached
/// (<see cref="IsDisconnected"/>) every read, write and flush throws
/// <see cref="IOException"/>. Every method is thread context, entered by
/// the probe or by the ring from any thread; a failed command surfaces as
/// an <see cref="IOException"/>, as the contract asks.
/// </summary>
public sealed class UsbMassStorageUnit : IBlockDevice
{
    // --- Constants ---

    /// <summary>TEST UNIT READY (SPC-4).</summary>
    private const byte TestUnitReadyCommand = 0x00;

    /// <summary>INQUIRY (SPC-4).</summary>
    private const byte InquiryCommand = 0x12;

    /// <summary>READ CAPACITY(10) (SBC-3).</summary>
    private const byte ReadCapacity10Command = 0x25;

    /// <summary>READ(10) (SBC-3).</summary>
    private const byte Read10Command = 0x28;

    /// <summary>WRITE(10) (SBC-3).</summary>
    private const byte Write10Command = 0x2A;

    /// <summary>SYNCHRONIZE CACHE(10) (SBC-3).</summary>
    private const byte SynchronizeCache10Command = 0x35;

    /// <summary>READ(16) (SBC-3).</summary>
    private const byte Read16Command = 0x88;

    /// <summary>WRITE(16) (SBC-3).</summary>
    private const byte Write16Command = 0x8A;

    /// <summary>SERVICE ACTION IN(16) (SBC-3), the operation code of READ CAPACITY(16).</summary>
    private const byte ServiceActionIn16Command = 0x9E;

    /// <summary>The service action of READ CAPACITY(16).</summary>
    private const byte ReadCapacity16ServiceAction = 0x10;

    /// <summary>Bytes of a 6-byte command block.</summary>
    private const int Command6Length = 6;

    /// <summary>Bytes of a 10-byte command block.</summary>
    private const int Command10Length = 10;

    /// <summary>Bytes of a 16-byte command block.</summary>
    private const int Command16Length = 16;

    /// <summary>The service action in a 16-byte command block.</summary>
    private const int ServiceActionOffset = 1;

    /// <summary>The allocation length in a 6-byte command block.</summary>
    private const int Command6AllocationLengthOffset = 4;

    /// <summary>The LBA in a READ or WRITE command block.</summary>
    private const int LbaOffset = 2;

    /// <summary>The transfer length in a 10-byte READ or WRITE.</summary>
    private const int Command10BlockCountOffset = 7;

    /// <summary>The transfer length in a 16-byte READ or WRITE.</summary>
    private const int Command16BlockCountOffset = 10;

    /// <summary>The allocation length in READ CAPACITY(16).</summary>
    private const int Command16AllocationLengthOffset = 10;

    /// <summary>Bytes of standard INQUIRY data asked for (SPC-4 section 6.4.2).</summary>
    private const int InquiryLength = 36;

    /// <summary>The least INQUIRY data that holds the peripheral byte and the version.</summary>
    private const int InquiryMinimumLength = 5;

    /// <summary>The peripheral qualifier's shift in the first INQUIRY byte.</summary>
    private const int PeripheralQualifierShift = 5;

    /// <summary>The peripheral device type's mask in the first INQUIRY byte.</summary>
    private const byte PeripheralDeviceTypeMask = 0x1F;

    /// <summary>Peripheral device type: direct access block device.</summary>
    private const byte DirectAccessBlockDevice = 0x00;

    /// <summary>Peripheral device type: simplified direct access device.</summary>
    private const byte SimplifiedDirectAccessDevice = 0x0E;

    /// <summary>T10 vendor identification in the INQUIRY data.</summary>
    private const int VendorOffset = 8;

    /// <summary>Bytes of the vendor identification.</summary>
    private const int VendorLength = 8;

    /// <summary>Product identification in the INQUIRY data.</summary>
    private const int ProductOffset = 16;

    /// <summary>Bytes of the product identification.</summary>
    private const int ProductLength = 16;

    /// <summary>Bytes of READ CAPACITY(10) data (SBC-3 section 5.15.2).</summary>
    private const int ReadCapacity10Length = 8;

    /// <summary>Bytes of READ CAPACITY(16) data (SBC-3 section 5.16.2).</summary>
    private const int ReadCapacity16Length = 32;

    /// <summary>The block length in READ CAPACITY(10) data.</summary>
    private const int Capacity10BlockLengthOffset = 4;

    /// <summary>The block length in READ CAPACITY(16) data.</summary>
    private const int Capacity16BlockLengthOffset = 8;

    /// <summary>A READ CAPACITY(10) last LBA of all ones says the capacity only fits READ CAPACITY(16).</summary>
    private const uint Capacity10Overflow = 0xFFFFFFFF;

    /// <summary>First LBA past what a 10-byte READ or WRITE can address.</summary>
    private const ulong Command10LbaLimit = 1ul << 32;

    /// <summary>The sense key's byte in fixed-format sense data (SPC-4 section 4.5.3).</summary>
    private const int SenseKeyOffset = 2;

    /// <summary>The sense key's mask in its byte.</summary>
    private const byte SenseKeyMask = 0x0F;

    /// <summary>The additional sense code in fixed-format sense data.</summary>
    private const int AdditionalSenseCodeOffset = 12;

    /// <summary>The additional sense code qualifier in fixed-format sense data.</summary>
    private const int AdditionalSenseQualifierOffset = 13;

    /// <summary>Sense key NOT READY.</summary>
    private const byte SenseKeyNotReady = 0x02;

    /// <summary>Sense key ILLEGAL REQUEST.</summary>
    private const byte SenseKeyIllegalRequest = 0x05;

    /// <summary>Sense key UNIT ATTENTION.</summary>
    private const byte SenseKeyUnitAttention = 0x06;

    /// <summary>Additional sense code MEDIUM NOT PRESENT.</summary>
    private const byte MediumNotPresent = 0x3A;

    /// <summary>Largest READ or WRITE sent: Linux's usb-storage default is 120 KiB, since some devices fail larger ones.</summary>
    private const int MaxTransferLength = 64 * 1024;

    /// <summary>Runs of a command that fails for a reason that says nothing about the command itself.</summary>
    private const int CommandAttempts = 3;

    /// <summary>TEST UNIT READY polls while the medium spins up or the device finishes its reset: 5 s.</summary>
    private const int ReadyAttempts = 50;

    /// <summary>Milliseconds between two TEST UNIT READY polls.</summary>
    private const uint ReadyRetryDelayMs = 100;

    /// <summary>The first printable ASCII character of an INQUIRY string.</summary>
    private const byte FirstPrintable = 0x20;

    /// <summary>The first character past printable ASCII.</summary>
    private const byte PastPrintable = 0x7F;

    // --- Private fields ---

    private readonly UsbMassStorageState _state;
    private readonly UsbBulkOnlyTransport _transport;
    private readonly DeviceBinding _binding;
    private readonly byte _lun;
    private readonly uint _index;
    private readonly string _name;
    private ulong _blockCount;
    private ulong _blockSize;
    private string _vendor = string.Empty;
    private string _product = string.Empty;
    private bool _synchronizeCacheUnsupported;

    // --- Constructor ---

    /// <summary>Takes the unit's place on the interface and its name number; <see cref="Initialize"/> fills the rest. Thread context, from the probe.</summary>
    /// <param name="state">The interface's state, whose detached flag ends the unit's I/O.</param>
    /// <param name="transport">The transport of the interface the unit belongs to.</param>
    /// <param name="lun">The unit's number on that interface.</param>
    /// <param name="index">Number of the device name, <c>usb</c> + index.</param>
    internal UsbMassStorageUnit(UsbMassStorageState state, UsbBulkOnlyTransport transport, byte lun, uint index)
    {
        _state = state;
        _transport = transport;
        _binding = transport.Binding;
        _lun = lun;
        _index = index;
        _name = "usb" + index;
    }

    // --- IBlockDevice ---

    /// <inheritdoc/>
    public ulong BlockCount => _blockCount;

    /// <inheritdoc/>
    public ulong BlockSize => _blockSize;

    /// <summary><c>usb{index}</c>, the lowest number no unit present used when the probe ran. Any context.</summary>
    public string Name => _name;

    /// <summary>
    /// Reads whole blocks, READ(10) or READ(16) in chunks of at most 64 KiB.
    /// Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Where they go; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the blocks asked for, or the blocks run past the end of the device.</exception>
    /// <exception cref="IOException">The device was detached, or a command failed.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        ThrowIfDisconnected();
        ThrowIfOutOfRange(blockNo, blockCount, data.Length);

        Span<byte> sense = stackalloc byte[UsbBulkOnlyTransport.SenseLength];
        Span<byte> command = stackalloc byte[Command16Length];
        ulong maxBlocks = Math.Max((ulong)MaxTransferLength / _blockSize, 1);
        while (blockCount != 0)
        {
            uint count = (uint)Math.Min(blockCount, maxBlocks);
            int length = (int)(count * _blockSize);
            int commandLength = WriteReadWriteCommand(command, write: false, blockNo, count);
            BulkOnlyStatus status = Execute(command.Slice(0, commandLength), data.Slice(0, length), [], sense, out uint residue);
            ThrowIfFailed("READ", status, residue, sense);

            data = data.Slice(length);
            blockNo += count;
            blockCount -= count;
        }
    }

    /// <summary>
    /// Writes whole blocks, WRITE(10) or WRITE(16) in chunks of at most
    /// 64 KiB. Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first block.</param>
    /// <param name="blockCount">How many blocks.</param>
    /// <param name="data">Their bytes; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the blocks given, or the blocks run past the end of the device.</exception>
    /// <exception cref="IOException">The device was detached, or a command failed.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        ThrowIfDisconnected();
        ThrowIfOutOfRange(blockNo, blockCount, data.Length);

        Span<byte> sense = stackalloc byte[UsbBulkOnlyTransport.SenseLength];
        Span<byte> command = stackalloc byte[Command16Length];
        ulong maxBlocks = Math.Max((ulong)MaxTransferLength / _blockSize, 1);
        while (blockCount != 0)
        {
            uint count = (uint)Math.Min(blockCount, maxBlocks);
            int length = (int)(count * _blockSize);
            int commandLength = WriteReadWriteCommand(command, write: true, blockNo, count);
            BulkOnlyStatus status = Execute(command.Slice(0, commandLength), [], data.Slice(0, length), sense, out uint residue);
            ThrowIfFailed("WRITE", status, residue, sense);

            data = data.Slice(length);
            blockNo += count;
            blockCount -= count;
        }
    }

    /// <summary>
    /// SYNCHRONIZE CACHE(10). Many flash drives have no cache to flush and
    /// reject it as an ILLEGAL REQUEST; after the first rejection it is no
    /// longer sent. Thread context; any thread.
    /// </summary>
    /// <exception cref="IOException">The device was detached, or the command failed for another reason.</exception>
    public void Flush()
    {
        ThrowIfDisconnected();
        if (_synchronizeCacheUnsupported)
        {
            return;
        }

        Span<byte> sense = stackalloc byte[UsbBulkOnlyTransport.SenseLength];
        Span<byte> command = stackalloc byte[Command10Length];
        command.Clear();
        command[0] = SynchronizeCache10Command;
        BulkOnlyStatus status = Execute(command, [], [], sense, out _);
        if (status == BulkOnlyStatus.Failed && SenseKey(sense) == SenseKeyIllegalRequest)
        {
            _synchronizeCacheUnsupported = true;
            return;
        }

        ThrowIfFailed("SYNCHRONIZE CACHE", status, 0, sense);
    }

    // --- Properties ---

    /// <summary>Number of the device name, unique among the units present. Any context.</summary>
    public uint Index => _index;

    /// <summary>The unit's number on its interface. Any context.</summary>
    public byte Lun => _lun;

    /// <summary>The T10 vendor identification INQUIRY returned, trimmed. Any context after <see cref="Initialize"/>.</summary>
    public string Vendor => _vendor;

    /// <summary>The product identification INQUIRY returned, trimmed. Any context after <see cref="Initialize"/>.</summary>
    public string Product => _product;

    /// <summary>True once the device left the bus or its interface was detached: nothing reaches the unit any more. Any context.</summary>
    public bool IsDisconnected => _transport.IsDisconnected || _state.Detached;

    // --- Internal methods ---

    /// <summary>
    /// Identifies the logical unit and waits for its medium: INQUIRY, TEST
    /// UNIT READY until it passes, READ CAPACITY. Thread context, the probe;
    /// logs through the binding.
    /// </summary>
    /// <returns>False when the unit is not a disk, has no medium or does not answer.</returns>
    internal bool Initialize()
    {
        Span<byte> sense = stackalloc byte[UsbBulkOnlyTransport.SenseLength];
        Span<byte> command = stackalloc byte[Command16Length];

        Span<byte> inquiry = stackalloc byte[InquiryLength];
        inquiry.Clear();
        command.Clear();
        command[0] = InquiryCommand;
        command[Command6AllocationLengthOffset] = InquiryLength;
        if (Execute(command.Slice(0, Command6Length), inquiry, [], sense, out uint residue) != BulkOnlyStatus.Passed
            || InquiryLength - residue < InquiryMinimumLength)
        {
            Log("INQUIRY failed");
            return false;
        }

        byte deviceType = (byte)(inquiry[0] & PeripheralDeviceTypeMask);
        if (inquiry[0] >> PeripheralQualifierShift != 0
            || deviceType is not (DirectAccessBlockDevice or SimplifiedDirectAccessDevice))
        {
            Log($"not a disk (peripheral 0x{inquiry[0]:x2}), skipped");
            return false;
        }

        if (!WaitUntilReady(command, sense) || !ReadCapacity(command, sense))
        {
            return false;
        }

        _vendor = InquiryString(inquiry.Slice(VendorOffset, VendorLength));
        _product = InquiryString(inquiry.Slice(ProductOffset, ProductLength));
        Log($"{_vendor} {_product}, {_blockCount} blocks of {_blockSize} bytes");
        return true;
    }

    // --- Private methods ---

    /// <summary>
    /// Runs a command, again when it fails for a reason that says nothing
    /// about the command: a UNIT ATTENTION (the device telling of a reset or
    /// a medium change) or a transport error the transport recovered from,
    /// unless the error was the device leaving. Thread context.
    /// </summary>
    private BulkOnlyStatus Execute(ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut, Span<byte> sense, out uint residue)
    {
        for (int attempt = 1; ; attempt++)
        {
            BulkOnlyStatus status = _transport.Execute(_lun, command, dataIn, dataOut, sense, out residue);
            bool transient = (status == BulkOnlyStatus.TransportError && !IsDisconnected)
                || (status == BulkOnlyStatus.Failed && SenseKey(sense) == SenseKeyUnitAttention);
            if (!transient || attempt >= CommandAttempts)
            {
                return status;
            }
        }
    }

    /// <summary>
    /// Polls TEST UNIT READY until the unit is ready, sleeping between
    /// polls. A card reader slot without a card answers "medium not
    /// present", and is given up at once. Thread context, the probe.
    /// </summary>
    private bool WaitUntilReady(Span<byte> command, Span<byte> sense)
    {
        command.Clear();
        command[0] = TestUnitReadyCommand;
        for (int attempt = 0; attempt < ReadyAttempts; attempt++)
        {
            BulkOnlyStatus status = Execute(command.Slice(0, Command6Length), [], [], sense, out _);
            if (status == BulkOnlyStatus.Passed)
            {
                return true;
            }

            // Execute already retried it: a device that loses every command
            // is not going to become ready.
            if (status == BulkOnlyStatus.TransportError)
            {
                Log("does not answer");
                return false;
            }

            if (status == BulkOnlyStatus.Failed
                && SenseKey(sense) == SenseKeyNotReady
                && sense[AdditionalSenseCodeOffset] == MediumNotPresent)
            {
                Log("no medium");
                return false;
            }

            _binding.Sleep(ReadyRetryDelayMs);
        }

        Log("never became ready" + SenseSuffix(sense));
        return false;
    }

    /// <summary>Reads the geometry: READ CAPACITY(10), then (16) for a unit past 2^32 blocks. Thread context, the probe.</summary>
    private bool ReadCapacity(Span<byte> command, Span<byte> sense)
    {
        Span<byte> capacity = stackalloc byte[ReadCapacity16Length];
        command.Clear();
        command[0] = ReadCapacity10Command;
        if (Execute(command.Slice(0, Command10Length), capacity.Slice(0, ReadCapacity10Length), [], sense, out _) != BulkOnlyStatus.Passed)
        {
            Log("READ CAPACITY failed" + SenseSuffix(sense));
            return false;
        }

        ulong lastLba = BinaryPrimitives.ReadUInt32BigEndian(capacity);
        uint blockLength = BinaryPrimitives.ReadUInt32BigEndian(capacity.Slice(Capacity10BlockLengthOffset));
        if (lastLba == Capacity10Overflow)
        {
            command.Clear();
            command[0] = ServiceActionIn16Command;
            command[ServiceActionOffset] = ReadCapacity16ServiceAction;
            BinaryPrimitives.WriteUInt32BigEndian(command.Slice(Command16AllocationLengthOffset), ReadCapacity16Length);
            if (Execute(command, capacity, [], sense, out _) != BulkOnlyStatus.Passed)
            {
                Log("READ CAPACITY(16) failed" + SenseSuffix(sense));
                return false;
            }

            lastLba = BinaryPrimitives.ReadUInt64BigEndian(capacity);
            blockLength = BinaryPrimitives.ReadUInt32BigEndian(capacity.Slice(Capacity16BlockLengthOffset));
        }

        if (blockLength == 0 || blockLength > MaxTransferLength)
        {
            Log($"unsupported block length {blockLength}");
            return false;
        }

        _blockCount = lastLba + 1;
        _blockSize = blockLength;
        return true;
    }

    /// <summary>Fills a READ or WRITE command block: the 10-byte form while the LBA fits it, the 16-byte one past. Any context.</summary>
    /// <returns>The command block length.</returns>
    private static int WriteReadWriteCommand(Span<byte> command, bool write, ulong lba, uint blockCount)
    {
        command.Clear();
        if (lba + blockCount <= Command10LbaLimit)
        {
            command[0] = write ? Write10Command : Read10Command;
            BinaryPrimitives.WriteUInt32BigEndian(command.Slice(LbaOffset), (uint)lba);
            BinaryPrimitives.WriteUInt16BigEndian(command.Slice(Command10BlockCountOffset), (ushort)blockCount);
            return Command10Length;
        }

        command[0] = write ? Write16Command : Read16Command;
        BinaryPrimitives.WriteUInt64BigEndian(command.Slice(LbaOffset), lba);
        BinaryPrimitives.WriteUInt32BigEndian(command.Slice(Command16BlockCountOffset), blockCount);
        return Command16Length;
    }

    /// <summary>Refuses a buffer shorter than the blocks, or blocks past the end of the device. Any context.</summary>
    private void ThrowIfOutOfRange(ulong blockNo, ulong blockCount, int dataLength)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)dataLength / _blockSize, nameof(blockCount));
        if (blockNo > _blockCount || blockCount > _blockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo), "The blocks run past the end of the device.");
        }
    }

    /// <summary>Throws once the device left the bus or its interface was detached. Any context.</summary>
    /// <exception cref="IOException">The device was detached.</exception>
    private void ThrowIfDisconnected()
    {
        if (IsDisconnected)
        {
            throw new IOException("USB device detached");
        }
    }

    /// <summary>Throws the error of a command that did not complete in full, after logging it. Thread context.</summary>
    /// <exception cref="IOException">The device was detached mid-command, or the command failed.</exception>
    private void ThrowIfFailed(string commandName, BulkOnlyStatus status, uint residue, ReadOnlySpan<byte> sense)
    {
        if (status == BulkOnlyStatus.Passed && residue == 0)
        {
            return;
        }

        // Pulled out mid-command: that is the error, not the command.
        ThrowIfDisconnected();

        if (status == BulkOnlyStatus.Passed)
        {
            Log($"{commandName} moved fewer bytes than asked, residue {residue}");
        }
        else
        {
            Log(commandName + (status == BulkOnlyStatus.Failed ? " failed" : " transport error") + SenseSuffix(sense));
        }

        throw new IOException($"USB mass storage {commandName} failed on {Name}.");
    }

    /// <summary>The sense key of fixed-format sense data. Any context.</summary>
    private static byte SenseKey(ReadOnlySpan<byte> sense) => (byte)(sense[SenseKeyOffset] & SenseKeyMask);

    /// <summary>The sense key, ASC and ASCQ as a log suffix. Thread context.</summary>
    private static string SenseSuffix(ReadOnlySpan<byte> sense) =>
        $", sense key 0x{SenseKey(sense):x2} ASC 0x{sense[AdditionalSenseCodeOffset]:x2} ASCQ 0x{sense[AdditionalSenseQualifierOffset]:x2}";

    /// <summary>An INQUIRY text field as a string: ASCII, padded with spaces, which are dropped; a byte outside printable ASCII reads as a question mark. Thread context.</summary>
    private static string InquiryString(ReadOnlySpan<byte> field)
    {
        int length = field.Length;
        while (length > 0 && field[length - 1] is (byte)' ' or 0)
        {
            length--;
        }

        Span<char> text = stackalloc char[ProductLength];
        for (int i = 0; i < length; i++)
        {
            byte b = field[i];
            text[i] = b is >= FirstPrintable and < PastPrintable ? (char)b : '?';
        }

        return new string(text.Slice(0, length));
    }

    /// <summary>One log line through the binding, prefixed <c>usb{N} (LUN n): </c>. Thread context.</summary>
    private void Log(string message) => _binding.Log($"{_name} (LUN {_lun}): {message}");
}
