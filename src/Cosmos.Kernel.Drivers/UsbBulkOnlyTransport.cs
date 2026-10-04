// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// USB Mass Storage Bulk-Only Transport (BOT 1.0), the transport of nearly
/// every USB stick and disk: a SCSI command goes out as a Command Block
/// Wrapper on the bulk OUT pipe, its data (if any) follows on the pipe of
/// its direction, and a Command Status Wrapper comes back on the bulk IN
/// pipe, all through the kit's <see cref="UsbAccess"/>. One per mass storage
/// interface, shared by its logical units, so it runs their commands one at
/// a time: a caller claims the busy flag under the binding's lock, held
/// only around the flag, and sleeps between looks while another command
/// holds it (a ring caller on another thread may wait here for a whole bulk
/// timeout). The lock is never held across a transfer. Every member is
/// thread context: the probe, or the ring on any thread.
/// </summary>
internal sealed class UsbBulkOnlyTransport
{
    // --- Constants ---

    /// <summary>Longest command block a CBW carries.</summary>
    internal const int MaxCommandLength = 16;

    /// <summary>Fixed-format sense data REQUEST SENSE asks for (SPC-4 section 4.5.3).</summary>
    internal const int SenseLength = 18;

    /// <summary>Highest LUN number a CBW can address (4-bit field).</summary>
    private const byte MaxLun = 15;

    /// <summary>Bulk-Only Mass Storage Reset (BOT 1.0 section 3.1).</summary>
    private const byte MassStorageResetRequest = 0xFF;

    /// <summary>GET MAX LUN (BOT 1.0 section 3.2).</summary>
    private const byte GetMaxLunRequest = 0xFE;

    /// <summary>Bytes of a Command Block Wrapper (BOT 1.0 section 5.1).</summary>
    private const int CbwLength = 31;

    /// <summary>dCBWSignature, "USBC".</summary>
    private const uint CbwSignature = 0x43425355;

    /// <summary>dCBWTag.</summary>
    private const int CbwTagOffset = 4;

    /// <summary>dCBWDataTransferLength.</summary>
    private const int CbwDataLengthOffset = 8;

    /// <summary>bmCBWFlags.</summary>
    private const int CbwFlagsOffset = 12;

    /// <summary>bCBWLUN.</summary>
    private const int CbwLunOffset = 13;

    /// <summary>bCBWCBLength.</summary>
    private const int CbwCommandLengthOffset = 14;

    /// <summary>CBWCB, the command block.</summary>
    private const int CbwCommandOffset = 15;

    /// <summary>bmCBWFlags bit 7: the data stage comes from the device.</summary>
    private const byte CbwFlagDataIn = 0x80;

    /// <summary>Bytes of a Command Status Wrapper (BOT 1.0 section 5.2).</summary>
    private const int CswLength = 13;

    /// <summary>dCSWSignature, "USBS".</summary>
    private const uint CswSignature = 0x53425355;

    /// <summary>dCSWTag.</summary>
    private const int CswTagOffset = 4;

    /// <summary>dCSWDataResidue.</summary>
    private const int CswResidueOffset = 8;

    /// <summary>bCSWStatus.</summary>
    private const int CswStatusOffset = 12;

    /// <summary>bCSWStatus: the command passed.</summary>
    private const byte CswCommandPassed = 0;

    /// <summary>bCSWStatus: the command failed.</summary>
    private const byte CswCommandFailed = 1;

    /// <summary>REQUEST SENSE (SPC-4 section 6.29), sent on the transport's own behalf after a failed command.</summary>
    private const byte RequestSenseCommand = 0x03;

    /// <summary>Bytes of a REQUEST SENSE command block.</summary>
    private const int RequestSenseCommandLength = 6;

    /// <summary>The allocation length in a REQUEST SENSE command block.</summary>
    private const int RequestSenseAllocationLengthOffset = 4;

    /// <summary>How long a caller sleeps between two looks at the busy flag, in milliseconds.</summary>
    private const uint BusyRetryMs = 1;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly UsbAccess _usb;
    private readonly UsbPipe _bulkIn;
    private readonly UsbPipe _bulkOut;
    private readonly DeviceLock _lock;
    private bool _busy;
    private uint _tag;

    // --- Constructor ---

    /// <summary>Takes the access, the two open bulk pipes and the lock the probe created. Thread context, from the probe.</summary>
    /// <param name="binding">The interface's binding, for the log and the sleeps.</param>
    /// <param name="usb">The kit's access to the mass storage interface.</param>
    /// <param name="bulkIn">The open bulk IN pipe.</param>
    /// <param name="bulkOut">The open bulk OUT pipe.</param>
    /// <param name="deviceLock">The lock the busy flag is taken under.</param>
    internal UsbBulkOnlyTransport(DeviceBinding binding, UsbAccess usb, UsbPipe bulkIn, UsbPipe bulkOut, DeviceLock deviceLock)
    {
        _binding = binding;
        _usb = usb;
        _bulkIn = bulkIn;
        _bulkOut = bulkOut;
        _lock = deviceLock;
    }

    // --- Properties ---

    /// <summary>True once the device left the bus: nothing reaches the units any more. Any context.</summary>
    internal bool IsDisconnected => _usb.IsDisconnected;

    /// <summary>The interface's binding, for the units' log lines.</summary>
    internal DeviceBinding Binding => _binding;

    // --- Internal methods ---

    /// <summary>
    /// Highest LUN number of the device (GET MAX LUN, BOT 1.0 section 3.2).
    /// A device with one LUN may STALL the request, which means 0. Thread
    /// context, the probe.
    /// </summary>
    internal byte GetMaxLun()
    {
        Span<byte> maxLun = stackalloc byte[1];
        UsbTransferStatus status = _usb.ControlIn(UsbRequestType.Class | UsbRequestType.Interface, GetMaxLunRequest, 0, _usb.InterfaceNumber, maxLun);
        return status == UsbTransferStatus.Success ? Math.Min(maxLun[0], MaxLun) : (byte)0;
    }

    /// <summary>
    /// Runs one SCSI command on <paramref name="lun"/>. The data stage reads
    /// into <paramref name="dataIn"/> or writes <paramref name="dataOut"/>,
    /// whichever is not empty (at most one may be). When the device reports
    /// the command failed, its sense data is fetched into
    /// <paramref name="sense"/> before another command can clear it. Thread
    /// context, any thread; one command at a time per interface.
    /// </summary>
    /// <param name="lun">The logical unit, 0 to <see cref="GetMaxLun"/>.</param>
    /// <param name="command">The SCSI command block, at most <see cref="MaxCommandLength"/> bytes.</param>
    /// <param name="dataIn">Receives the data of a command that reads; empty otherwise.</param>
    /// <param name="dataOut">The data of a command that writes; empty otherwise.</param>
    /// <param name="sense"><see cref="SenseLength"/> bytes; cleared unless the status is <see cref="BulkOnlyStatus.Failed"/>.</param>
    /// <param name="residue">Bytes of the data stage the device did not move.</param>
    /// <exception cref="ArgumentOutOfRangeException">The command is longer than a CBW carries, or the sense buffer is short.</exception>
    /// <exception cref="ArgumentException">Both data spans are given.</exception>
    internal BulkOnlyStatus Execute(byte lun, ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut, Span<byte> sense, out uint residue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(command.Length, MaxCommandLength, nameof(command));
        ArgumentOutOfRangeException.ThrowIfLessThan(sense.Length, SenseLength, nameof(sense));
        if (!dataIn.IsEmpty && !dataOut.IsEmpty)
        {
            throw new ArgumentException("A command has one data stage, in or out.", nameof(dataOut));
        }

        Claim();
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
            using (_lock.Acquire())
            {
                _busy = false;
            }
        }
    }

    // --- Private methods ---

    /// <summary>Takes the busy flag under the lock; while another command holds it, the lock is released and the caller sleeps a millisecond before looking again. Thread context.</summary>
    private void Claim()
    {
        while (true)
        {
            bool claimed;
            using (_lock.Acquire())
            {
                claimed = !_busy;
                if (claimed)
                {
                    _busy = true;
                }
            }

            if (claimed)
            {
                return;
            }

            _binding.Sleep(BusyRetryMs);
        }
    }

    /// <summary>One command through its three stages (BOT 1.0 sections 5 and 6.7). The caller holds the busy flag. Thread context.</summary>
    /// <param name="lun">The logical unit.</param>
    /// <param name="command">The SCSI command block.</param>
    /// <param name="dataIn">The IN buffer, or empty.</param>
    /// <param name="dataOut">The OUT data, or empty.</param>
    /// <param name="residue">Bytes of the data stage the device did not move.</param>
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

        if (_usb.BulkOut(_bulkOut, cbw, out _) != UsbTransferStatus.Success)
        {
            return ResetRecovery("command transport failed");
        }

        // A STALL in the data stage is how the device ends it early; the
        // status stage still follows once the endpoint is cleared (sections
        // 6.7.2 and 6.7.3).
        if (dataLength != 0)
        {
            UsbPipe pipe = isIn ? _bulkIn : _bulkOut;
            UsbTransferStatus data = isIn ? _usb.BulkIn(_bulkIn, dataIn, out _) : _usb.BulkOut(_bulkOut, dataOut, out _);
            if (data == UsbTransferStatus.Stall)
            {
                _usb.ClearHalt(pipe);
            }
            else if (data != UsbTransferStatus.Success)
            {
                return ResetRecovery("data transport failed");
            }
        }

        // A STALL on the CSW is retried once after clearing it (section
        // 5.3.3, figure 2).
        Span<byte> csw = stackalloc byte[CswLength];
        UsbTransferStatus status = _usb.BulkIn(_bulkIn, csw, out int received);
        if (status == UsbTransferStatus.Stall)
        {
            _usb.ClearHalt(_bulkIn);
            status = _usb.BulkIn(_bulkIn, csw, out received);
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
    /// Mass Storage Reset, then the halt of both bulk pipes cleared (BOT 1.0
    /// section 5.3.4). A device that left the bus is not reset: there is
    /// nothing left to bring back. Thread context; logs through the binding.
    /// </summary>
    /// <param name="reason">What failed, for the log.</param>
    private BulkOnlyStatus ResetRecovery(string reason)
    {
        if (_usb.IsDisconnected)
        {
            return BulkOnlyStatus.TransportError;
        }

        _binding.Log($"{reason}, resetting the device");
        _usb.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, MassStorageResetRequest, 0, _usb.InterfaceNumber);
        _usb.ClearHalt(_bulkIn);
        _usb.ClearHalt(_bulkOut);
        return BulkOnlyStatus.TransportError;
    }
}
