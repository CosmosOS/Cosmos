// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// One logical unit of a USB mass storage device (a USB stick, a card
/// reader slot, a USB disk) as a block device. It speaks the SCSI block
/// commands (SBC-3) the mass storage class carries, over the interface's
/// <see cref="UsbBulkOnlyTransport"/>. Once the device is unplugged
/// (<see cref="IsRemoved"/>) every read, write and flush throws
/// <see cref="IOException"/>.
/// </summary>
internal sealed class UsbMassStorage : BlockDevice
{
    // SCSI operation codes (SPC-4, SBC-3).
    private const byte TestUnitReadyCommand = 0x00;
    private const byte InquiryCommand = 0x12;
    private const byte ReadCapacity10Command = 0x25;
    private const byte Read10Command = 0x28;
    private const byte Write10Command = 0x2A;
    private const byte SynchronizeCache10Command = 0x35;
    private const byte Read16Command = 0x88;
    private const byte Write16Command = 0x8A;
    private const byte ServiceActionIn16Command = 0x9E;
    private const byte ReadCapacity16ServiceAction = 0x10;

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

    // Standard INQUIRY data (SPC-4 §6.4.2).
    private const int InquiryLength = 36;
    private const int InquiryMinimumLength = 5;
    private const int PeripheralQualifierShift = 5;
    private const byte PeripheralDeviceTypeMask = 0x1F;
    private const byte DirectAccessBlockDevice = 0x00;
    private const byte SimplifiedDirectAccessDevice = 0x0E;
    private const int VendorOffset = 8;
    private const int VendorLength = 8;
    private const int ProductOffset = 16;
    private const int ProductLength = 16;

    // READ CAPACITY data (SBC-3 §5.15.2, §5.16.2).
    private const int ReadCapacity10Length = 8;
    private const int ReadCapacity16Length = 32;
    private const int Capacity10BlockLengthOffset = 4;
    private const int Capacity16BlockLengthOffset = 8;

    /// <summary>A READ CAPACITY(10) last LBA of all ones says the capacity only fits READ CAPACITY(16).</summary>
    private const uint Capacity10Overflow = 0xFFFFFFFF;

    /// <summary>First LBA past what a 10-byte READ or WRITE can address.</summary>
    private const ulong Command10LbaLimit = 1ul << 32;

    // Fixed-format sense data (SPC-4 §4.5.3).
    private const int SenseKeyOffset = 2;
    private const byte SenseKeyMask = 0x0F;
    private const int AdditionalSenseCodeOffset = 12;
    private const int AdditionalSenseQualifierOffset = 13;
    private const byte SenseKeyNotReady = 0x02;
    private const byte SenseKeyIllegalRequest = 0x05;
    private const byte SenseKeyUnitAttention = 0x06;
    private const byte MediumNotPresent = 0x3A;

    /// <summary>Largest READ or WRITE sent: Linux's usb-storage default is 120 KiB, since some devices fail larger ones.</summary>
    private const int MaxTransferLength = 64 * 1024;

    /// <summary>Runs of a command that fails for a reason that says nothing about the command itself.</summary>
    private const int CommandAttempts = 3;

    /// <summary>TEST UNIT READY polls while the medium spins up or the device finishes its reset: 5 s.</summary>
    private const int ReadyAttempts = 50;
    private const uint ReadyRetryDelayMs = 100;

    private readonly UsbBulkOnlyTransport _transport;
    private readonly byte _lun;
    private readonly string _name;
    private bool _synchronizeCacheUnsupported;

    /// <param name="transport">The transport of the interface the unit belongs to.</param>
    /// <param name="lun">The unit's number on that interface.</param>
    /// <param name="index">Number of the device name, "usb" + index.</param>
    public UsbMassStorage(UsbBulkOnlyTransport transport, byte lun, uint index)
    {
        _transport = transport;
        _lun = lun;
        Index = index;
        _name = BuildDeviceName("usb", index);
    }

    /// <inheritdoc />
    public override string Name => _name;

    /// <summary>Number of the device name, unique among the units present.</summary>
    public uint Index { get; }

    public UsbBulkOnlyTransport Transport => _transport;

    /// <summary>The device was unplugged: nothing reaches the unit any more.</summary>
    public bool IsRemoved => _transport.Device.IsDisconnected;

    /// <summary>
    /// Identifies the logical unit and waits for its medium: INQUIRY, TEST
    /// UNIT READY until it passes, READ CAPACITY. Runs during enumeration,
    /// before CoreLib number formatting is safe, so it logs through
    /// <see cref="Serial"/> only.
    /// </summary>
    /// <returns><see langword="false"/> when the unit is not a disk, has no medium or does not answer.</returns>
    public bool Initialize()
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
            WriteLogPrefix();
            Serial.WriteString("INQUIRY failed\n");
            return false;
        }

        byte deviceType = (byte)(inquiry[0] & PeripheralDeviceTypeMask);
        if (inquiry[0] >> PeripheralQualifierShift != 0
            || deviceType is not (DirectAccessBlockDevice or SimplifiedDirectAccessDevice))
        {
            WriteLogPrefix();
            Serial.WriteString("not a disk (peripheral 0x");
            Serial.WriteHex((uint)inquiry[0]);
            Serial.WriteString("), skipped\n");
            return false;
        }

        if (!WaitUntilReady(command, sense) || !ReadCapacity(command, sense))
        {
            return false;
        }

        WriteLogPrefix();
        WriteInquiryString(inquiry.Slice(VendorOffset, VendorLength));
        Serial.WriteString(" ");
        WriteInquiryString(inquiry.Slice(ProductOffset, ProductLength));
        Serial.WriteString(", ");
        Serial.WriteNumber(BlockCount);
        Serial.WriteString(" blocks of ");
        Serial.WriteNumber(BlockSize);
        Serial.WriteString(" bytes\n");
        return true;
    }

    /// <inheritdoc />
    public override void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        ThrowIfRemoved();
        ThrowIfOutOfRange(blockNo, blockCount, (ulong)data.Length);

        Span<byte> sense = stackalloc byte[UsbBulkOnlyTransport.SenseLength];
        Span<byte> command = stackalloc byte[Command16Length];
        ulong maxBlocks = Math.Max((ulong)MaxTransferLength / BlockSize, 1);
        while (blockCount != 0)
        {
            uint count = (uint)Math.Min(blockCount, maxBlocks);
            int length = (int)(count * BlockSize);
            int commandLength = WriteReadWriteCommand(command, write: false, blockNo, count);
            BulkOnlyStatus status = Execute(command.Slice(0, commandLength), data.Slice(0, length), [], sense, out uint residue);
            ThrowIfFailed("READ", status, residue, sense);

            data = data.Slice(length);
            blockNo += count;
            blockCount -= count;
        }
    }

    /// <inheritdoc />
    public override void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        ThrowIfRemoved();
        ThrowIfOutOfRange(blockNo, blockCount, (ulong)data.Length);

        Span<byte> sense = stackalloc byte[UsbBulkOnlyTransport.SenseLength];
        Span<byte> command = stackalloc byte[Command16Length];
        ulong maxBlocks = Math.Max((ulong)MaxTransferLength / BlockSize, 1);
        while (blockCount != 0)
        {
            uint count = (uint)Math.Min(blockCount, maxBlocks);
            int length = (int)(count * BlockSize);
            int commandLength = WriteReadWriteCommand(command, write: true, blockNo, count);
            BulkOnlyStatus status = Execute(command.Slice(0, commandLength), [], data.Slice(0, length), sense, out uint residue);
            ThrowIfFailed("WRITE", status, residue, sense);

            data = data.Slice(length);
            blockNo += count;
            blockCount -= count;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Many flash drives have no cache to flush and reject SYNCHRONIZE
    /// CACHE; after the first rejection it is no longer sent.
    /// </remarks>
    public override void Flush()
    {
        ThrowIfRemoved();
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

    /// <summary>
    /// Runs a command, again when it fails for a reason that says nothing
    /// about the command: a UNIT ATTENTION (the device telling of a reset
    /// or a medium change) or a transport error the transport recovered from,
    /// unless the error was the device leaving.
    /// </summary>
    private BulkOnlyStatus Execute(ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut, Span<byte> sense, out uint residue)
    {
        for (int attempt = 1; ; attempt++)
        {
            BulkOnlyStatus status = _transport.Execute(_lun, command, dataIn, dataOut, sense, out residue);
            bool transient = (status == BulkOnlyStatus.TransportError && !IsRemoved)
                || (status == BulkOnlyStatus.Failed && SenseKey(sense) == SenseKeyUnitAttention);
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
                WriteLogPrefix();
                Serial.WriteString("does not answer\n");
                return false;
            }

            if (status == BulkOnlyStatus.Failed
                && SenseKey(sense) == SenseKeyNotReady
                && sense[AdditionalSenseCodeOffset] == MediumNotPresent)
            {
                WriteLogPrefix();
                Serial.WriteString("no medium\n");
                return false;
            }

            UsbManager.DelayMilliseconds(ReadyRetryDelayMs);
        }

        WriteLogPrefix();
        Serial.WriteString("never became ready");
        WriteSense(sense);
        return false;
    }

    /// <summary>Reads the geometry: READ CAPACITY(10), then (16) for a unit past 2^32 blocks.</summary>
    private bool ReadCapacity(Span<byte> command, Span<byte> sense)
    {
        Span<byte> capacity = stackalloc byte[ReadCapacity16Length];
        command.Clear();
        command[0] = ReadCapacity10Command;
        if (Execute(command.Slice(0, Command10Length), capacity.Slice(0, ReadCapacity10Length), [], sense, out _) != BulkOnlyStatus.Passed)
        {
            WriteLogPrefix();
            Serial.WriteString("READ CAPACITY failed");
            WriteSense(sense);
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
                WriteLogPrefix();
                Serial.WriteString("READ CAPACITY(16) failed");
                WriteSense(sense);
                return false;
            }

            lastLba = BinaryPrimitives.ReadUInt64BigEndian(capacity);
            blockLength = BinaryPrimitives.ReadUInt32BigEndian(capacity.Slice(Capacity16BlockLengthOffset));
        }

        if (blockLength == 0 || blockLength > MaxTransferLength)
        {
            WriteLogPrefix();
            Serial.WriteString("unsupported block length ");
            Serial.WriteNumber(blockLength);
            Serial.WriteString("\n");
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

    private void ThrowIfOutOfRange(ulong blockNo, ulong blockCount, ulong dataLength)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, dataLength / BlockSize, nameof(blockCount));
        if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
        {
            throw new ArgumentOutOfRangeException(nameof(blockNo), "The blocks run past the end of the device.");
        }
    }

    private void ThrowIfRemoved()
    {
        if (IsRemoved)
        {
            throw new IOException("USB mass storage device " + _name + " was removed.");
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
        ThrowIfRemoved();

        // The message stays constant: this can run while partitions are
        // scanned at boot, before CoreLib number formatting is safe.
        WriteLogPrefix();
        Serial.WriteString(commandName);
        if (status == BulkOnlyStatus.Passed)
        {
            Serial.WriteString(" moved fewer bytes than asked, residue ");
            Serial.WriteNumber(residue);
            Serial.WriteString("\n");
        }
        else
        {
            Serial.WriteString(status == BulkOnlyStatus.Failed ? " failed" : " transport error");
            WriteSense(sense);
        }

        throw new IOException("USB mass storage " + commandName + " failed on " + _name + ".");
    }

    private static byte SenseKey(ReadOnlySpan<byte> sense) => (byte)(sense[SenseKeyOffset] & SenseKeyMask);

    private static void WriteSense(ReadOnlySpan<byte> sense)
    {
        Serial.WriteString(", sense key 0x");
        Serial.WriteHex((uint)SenseKey(sense));
        Serial.WriteString(" ASC 0x");
        Serial.WriteHex((uint)sense[AdditionalSenseCodeOffset]);
        Serial.WriteString(" ASCQ 0x");
        Serial.WriteHex((uint)sense[AdditionalSenseQualifierOffset]);
        Serial.WriteString("\n");
    }

    /// <summary>Writes an INQUIRY text field: ASCII, padded with spaces, which are dropped.</summary>
    private static void WriteInquiryString(ReadOnlySpan<byte> field)
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
            text[i] = b is >= 0x20 and < 0x7F ? (char)b : '?';
        }

        Serial.WriteString(new string(text.Slice(0, length)));
    }

    private void WriteLogPrefix()
    {
        Serial.WriteString("[USB storage] ");
        Serial.WriteString(_name);
        Serial.WriteString(" (LUN ");
        Serial.WriteNumber((uint)_lun);
        Serial.WriteString("): ");
    }
}
