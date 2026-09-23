// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Usb;
using SchedMutex = Cosmos.Kernel.Core.Scheduler.Mutex;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// USB Mass Storage Bulk-Only Transport (BOT 1.0), the transport of nearly
/// every USB stick and disk: a SCSI command goes out as a Command Block
/// Wrapper on the bulk OUT endpoint, its data (if any) follows on the
/// endpoint of its direction, and a Command Status Wrapper comes back on
/// the bulk IN endpoint. One per mass storage interface, shared by its
/// logical units, so it runs their commands one at a time.
/// </summary>
internal sealed class UsbBulkOnlyTransport
{
    /// <summary>Longest command block a CBW carries.</summary>
    public const int MaxCommandLength = 16;

    /// <summary>Fixed-format sense data REQUEST SENSE asks for (SPC-4 §4.5.3).</summary>
    public const int SenseLength = 18;

    /// <summary>Highest LUN number a CBW can address (4-bit field).</summary>
    private const byte MaxLun = 15;

    // Class requests (BOT 1.0 §3).
    private const byte MassStorageResetRequest = 0xFF;
    private const byte GetMaxLunRequest = 0xFE;

    // Command Block Wrapper (BOT 1.0 §5.1).
    private const int CbwLength = 31;
    private const uint CbwSignature = 0x43425355;
    private const int CbwTagOffset = 4;
    private const int CbwDataLengthOffset = 8;
    private const int CbwFlagsOffset = 12;
    private const int CbwLunOffset = 13;
    private const int CbwCommandLengthOffset = 14;
    private const int CbwCommandOffset = 15;
    private const byte CbwFlagDataIn = 0x80;

    // Command Status Wrapper (BOT 1.0 §5.2).
    private const int CswLength = 13;
    private const uint CswSignature = 0x53425355;
    private const int CswTagOffset = 4;
    private const int CswResidueOffset = 8;
    private const int CswStatusOffset = 12;
    private const byte CswCommandPassed = 0;
    private const byte CswCommandFailed = 1;

    /// <summary>REQUEST SENSE (SPC-4 §6.29), sent on the transport's own behalf after a failed command.</summary>
    private const byte RequestSenseCommand = 0x03;
    private const int RequestSenseCommandLength = 6;
    private const int RequestSenseAllocationLengthOffset = 4;

    private readonly UsbDevice _device;
    private readonly byte _interfaceNumber;
    private readonly UsbEndpoint _bulkIn;
    private readonly UsbEndpoint _bulkOut;
    private readonly SchedMutex _mutex = new();
    private uint _tag;

    public UsbBulkOnlyTransport(UsbDevice device, byte interfaceNumber, UsbEndpoint bulkIn, UsbEndpoint bulkOut)
    {
        _device = device;
        _interfaceNumber = interfaceNumber;
        _bulkIn = bulkIn;
        _bulkOut = bulkOut;
    }

    /// <summary>The USB device the interface belongs to.</summary>
    public UsbDevice Device => _device;

    /// <summary>bInterfaceNumber of the mass storage interface.</summary>
    public byte InterfaceNumber => _interfaceNumber;

    /// <summary>Opens both bulk endpoints.</summary>
    public bool Open() => _device.OpenBulkEndpoint(_bulkIn) && _device.OpenBulkEndpoint(_bulkOut);

    /// <summary>
    /// Highest LUN number of the device (GET MAX LUN, BOT 1.0 §3.2). A
    /// device with one LUN may STALL the request, which means 0.
    /// </summary>
    public byte GetMaxLun()
    {
        Span<byte> maxLun = stackalloc byte[1];
        UsbTransferStatus status = _device.ControlIn(UsbRequestType.Class | UsbRequestType.Interface,
            GetMaxLunRequest, 0, _interfaceNumber, maxLun);
        return status == UsbTransferStatus.Success ? Math.Min(maxLun[0], MaxLun) : (byte)0;
    }

    /// <summary>
    /// Runs one SCSI command on <paramref name="lun"/>. The data stage reads
    /// into <paramref name="dataIn"/> or writes <paramref name="dataOut"/>,
    /// whichever is not empty (at most one may be). When the device reports
    /// the command failed, its sense data is fetched into
    /// <paramref name="sense"/> before another command can clear it.
    /// Thread context only.
    /// </summary>
    /// <param name="lun">The logical unit, 0 to <see cref="GetMaxLun"/>.</param>
    /// <param name="command">The SCSI command block, at most <see cref="MaxCommandLength"/> bytes.</param>
    /// <param name="dataIn">Receives the data of a command that reads; empty otherwise.</param>
    /// <param name="dataOut">The data of a command that writes; empty otherwise.</param>
    /// <param name="sense"><see cref="SenseLength"/> bytes; cleared unless the status is <see cref="BulkOnlyStatus.Failed"/>.</param>
    /// <param name="residue">Bytes of the data stage the device did not move.</param>
    public BulkOnlyStatus Execute(byte lun, ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut,
        Span<byte> sense, out uint residue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(command.Length, MaxCommandLength, nameof(command));
        ArgumentOutOfRangeException.ThrowIfLessThan(sense.Length, SenseLength, nameof(sense));
        if (!dataIn.IsEmpty && !dataOut.IsEmpty)
        {
            throw new ArgumentException("A command has one data stage, in or out.", nameof(dataOut));
        }

        _mutex.Acquire();
        try
        {
            sense.Clear();
            BulkOnlyStatus status = Transport(lun, command, dataIn, dataOut, out residue);
            if (status == BulkOnlyStatus.Failed)
            {
                Span<byte> requestSense = stackalloc byte[RequestSenseCommandLength];
                requestSense.Clear();
                requestSense[0] = RequestSenseCommand;
                requestSense[RequestSenseAllocationLengthOffset] = SenseLength;
                if (Transport(lun, requestSense, sense.Slice(0, SenseLength), [], out _) != BulkOnlyStatus.Passed)
                {
                    sense.Clear();
                }
            }

            return status;
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>One command through its three stages (BOT 1.0 §5, §6.7). The caller holds the mutex.</summary>
    private BulkOnlyStatus Transport(byte lun, ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut, out uint residue)
    {
        residue = 0;
        uint tag = ++_tag;
        bool isIn = !dataIn.IsEmpty;
        int dataLength = isIn ? dataIn.Length : dataOut.Length;

        Span<byte> cbw = stackalloc byte[CbwLength];
        cbw.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(cbw, CbwSignature);
        BinaryPrimitives.WriteUInt32LittleEndian(cbw.Slice(CbwTagOffset), tag);
        BinaryPrimitives.WriteUInt32LittleEndian(cbw.Slice(CbwDataLengthOffset), (uint)dataLength);
        cbw[CbwFlagsOffset] = isIn ? CbwFlagDataIn : (byte)0;
        cbw[CbwLunOffset] = lun;
        cbw[CbwCommandLengthOffset] = (byte)command.Length;
        command.CopyTo(cbw.Slice(CbwCommandOffset));

        if (_device.BulkOut(_bulkOut, cbw, out _) != UsbTransferStatus.Success)
        {
            return ResetRecovery("command transport failed");
        }

        // A STALL in the data stage is how the device ends it early; the
        // status stage still follows once the endpoint is cleared (§6.7.2, §6.7.3).
        if (dataLength != 0)
        {
            UsbEndpoint endpoint = isIn ? _bulkIn : _bulkOut;
            UsbTransferStatus data = isIn ? _device.BulkIn(_bulkIn, dataIn, out _) : _device.BulkOut(_bulkOut, dataOut, out _);
            if (data == UsbTransferStatus.Stall)
            {
                _device.ClearHalt(endpoint);
            }
            else if (data != UsbTransferStatus.Success)
            {
                return ResetRecovery("data transport failed");
            }
        }

        // A STALL on the CSW is retried once after clearing it (§5.3.3, figure 2).
        Span<byte> csw = stackalloc byte[CswLength];
        UsbTransferStatus status = _device.BulkIn(_bulkIn, csw, out int received);
        if (status == UsbTransferStatus.Stall)
        {
            _device.ClearHalt(_bulkIn);
            status = _device.BulkIn(_bulkIn, csw, out received);
        }

        if (status != UsbTransferStatus.Success
            || received != CswLength
            || BinaryPrimitives.ReadUInt32LittleEndian(csw) != CswSignature
            || BinaryPrimitives.ReadUInt32LittleEndian(csw.Slice(CswTagOffset)) != tag)
        {
            return ResetRecovery("no valid status");
        }

        residue = Math.Min(BinaryPrimitives.ReadUInt32LittleEndian(csw.Slice(CswResidueOffset)), (uint)dataLength);
        return csw[CswStatusOffset] switch
        {
            CswCommandPassed => BulkOnlyStatus.Passed,
            CswCommandFailed => BulkOnlyStatus.Failed,
            _ => ResetRecovery("phase error")
        };
    }

    /// <summary>
    /// Brings the device back in step after a transport failure: Bulk-Only
    /// Mass Storage Reset, then the halt of both bulk endpoints cleared
    /// (BOT 1.0 §5.3.4). A device that left the bus is not reset: there is
    /// nothing left to bring back.
    /// </summary>
    private BulkOnlyStatus ResetRecovery(string reason)
    {
        if (_device.IsDisconnected)
        {
            return BulkOnlyStatus.TransportError;
        }

        Serial.WriteString("[USB storage] ");
        Serial.WriteString(reason);
        Serial.WriteString(", resetting the device\n");

        _device.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, MassStorageResetRequest, 0, _interfaceNumber);
        _device.ClearHalt(_bulkIn);
        _device.ClearHalt(_bulkOut);
        return BulkOnlyStatus.TransportError;
    }
}

/// <summary>How a command ended on the Bulk-Only Transport.</summary>
internal enum BulkOnlyStatus
{
    /// <summary>The device ran the command.</summary>
    Passed,

    /// <summary>The device reports the command failed; the sense data says why.</summary>
    Failed,

    /// <summary>The command did not make it through the bus; the device was reset.</summary>
    TransportError
}
