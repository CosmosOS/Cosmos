// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.Scsi;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.BulkOnly;

/// <summary>
/// USB Mass Storage Bulk-Only Transport (BOT 1.0), the transport of nearly
/// every USB stick and disk: a SCSI command goes out as a Command Block
/// Wrapper on the bulk OUT endpoint, its data, if any, follows on the
/// endpoint of its direction, and a Command Status Wrapper comes back on
/// the bulk IN endpoint. One per mass storage interface, shared by its
/// logical units, which take turns: the stages of one command must not
/// interleave with another's.
/// </summary>
internal sealed class BulkOnlyTransport
{
    /// <summary>Highest logical unit number a Command Block Wrapper can address (a 4-bit field).</summary>
    private const byte MaxLun = 15;

    /// <summary>Length of the REQUEST SENSE command block, a 6-byte one.</summary>
    private const int RequestSenseCommandLength = 6;

    /// <summary>Byte offset of REQUEST SENSE's allocation length.</summary>
    private const int RequestSenseAllocationLengthOffset = 4;

    private readonly UsbDeviceContext _context;
    private readonly UsbBulkPipe _bulkIn;
    private readonly UsbBulkPipe _bulkOut;

    /// <summary>
    /// Holds one signal while no unit's command runs, so a wait on it takes
    /// the interface's turn. The kit cancels it when the device leaves the
    /// bus, which makes every wait fail at once.
    /// </summary>
    private readonly DeviceEvent _turn;

    /// <summary>The tag of the last command sent. Changed by the caller of <see cref="Execute"/> only.</summary>
    private uint _tag;

    /// <summary>Runs commands on the interface of <paramref name="context"/>, through its two bulk pipes.</summary>
    /// <param name="context">The binding, which issues the class requests and names the log lines.</param>
    /// <param name="bulkIn">The interface's bulk IN pipe.</param>
    /// <param name="bulkOut">The interface's bulk OUT pipe.</param>
    /// <param name="turn">An event of the binding, not yet signalled, that the units take turns through.</param>
    internal BulkOnlyTransport(UsbDeviceContext context, UsbBulkPipe bulkIn, UsbBulkPipe bulkOut, DeviceEvent turn)
    {
        _context = context;
        _bulkIn = bulkIn;
        _bulkOut = bulkOut;
        _turn = turn;

        // Free: the first unit to ask takes the turn at once.
        _turn.Signal();
    }

    /// <summary>
    /// Highest logical unit number of the device (GET MAX LUN, BOT 1.0
    /// s3.2). A device with one unit may STALL the request, which means 0.
    /// Thread context only.
    /// </summary>
    internal byte GetMaxLun()
    {
        Span<byte> maxLun = stackalloc byte[1];
        UsbTransferResult result = _context.ControlIn(UsbRequestKind.Class, UsbRecipient.Interface, (byte)BulkOnlyRequest.GetMaxLun,
            0, _context.Interface.Number, maxLun);
        return result.Status == UsbTransferStatus.Success && result.Length == maxLun.Length ? Math.Min(maxLun[0], MaxLun) : (byte)0;
    }

    /// <summary>
    /// Takes the interface's turn, waiting while another unit's command
    /// runs. Every command once Probe returned Bound runs holding it: from
    /// then on the units are the storage manager's, which reads them from
    /// any thread. Thread context only.
    /// </summary>
    /// <returns>False once the device left the bus: no command reaches it any more.</returns>
    internal bool TryEnter() => _turn.Wait();

    /// <summary>Hands the turn <see cref="TryEnter"/> took to the next unit.</summary>
    internal void Exit() => _turn.Signal();

    /// <summary>
    /// Runs one SCSI command on <paramref name="lun"/>. The data stage reads
    /// into <paramref name="dataIn"/> or writes <paramref name="dataOut"/>,
    /// whichever is not empty (at most one may be). When the device reports
    /// the command failed, its sense data is fetched into
    /// <paramref name="sense"/> before another command can clear it. The
    /// caller holds the turn (<see cref="TryEnter"/>), or is the driver's
    /// Probe, which no other thread can reach the interface during. Thread
    /// context only.
    /// </summary>
    /// <param name="lun">The logical unit, 0 to <see cref="GetMaxLun"/>.</param>
    /// <param name="command">The SCSI command block, at most <see cref="CommandBlockWrapper.MaxCommandLength"/> bytes.</param>
    /// <param name="dataIn">Receives the data of a command that reads; empty otherwise.</param>
    /// <param name="dataOut">The data of a command that writes; empty otherwise.</param>
    /// <param name="sense"><see cref="SenseData.Size"/> bytes; cleared unless the status is <see cref="BulkOnlyStatus.Failed"/>.</param>
    /// <param name="residue">Bytes of the data stage the device did not move.</param>
    internal BulkOnlyStatus Execute(byte lun, ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut,
        Span<byte> sense, out uint residue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lun, MaxLun);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(command.Length, CommandBlockWrapper.MaxCommandLength, nameof(command));
        ArgumentOutOfRangeException.ThrowIfLessThan(sense.Length, SenseData.Size, nameof(sense));
        if (!dataIn.IsEmpty && !dataOut.IsEmpty)
        {
            throw new ArgumentException("A command has one data stage, in or out.", nameof(dataOut));
        }

        sense.Clear();
        BulkOnlyStatus status = Transport(lun, command, dataIn, dataOut, out residue);
        if (status == BulkOnlyStatus.Failed)
        {
            Span<byte> requestSense = stackalloc byte[RequestSenseCommandLength];
            requestSense.Clear();
            requestSense[0] = (byte)ScsiOpcode.RequestSense;
            requestSense[RequestSenseAllocationLengthOffset] = SenseData.Size;
            if (Transport(lun, requestSense, sense[..SenseData.Size], [], out _) != BulkOnlyStatus.Passed)
            {
                sense.Clear();
            }
        }

        return status;
    }

    /// <summary>One command through its three stages (BOT 1.0 s5, s6.7).</summary>
    private BulkOnlyStatus Transport(byte lun, ReadOnlySpan<byte> command, Span<byte> dataIn, ReadOnlySpan<byte> dataOut, out uint residue)
    {
        residue = 0;
        uint tag = ++_tag;
        bool isIn = !dataIn.IsEmpty;
        int dataLength = isIn ? dataIn.Length : dataOut.Length;

        Span<byte> buffer = stackalloc byte[CommandBlockWrapper.Size];
        CommandBlockWrapper wrapper = new(buffer, tag, lun, command, dataLength, isIn);
        UsbTransferResult sent = _bulkOut.Write(wrapper.Bytes);
        if (sent.Status != UsbTransferStatus.Success)
        {
            return ResetRecovery(sent.Status, "command transport failed");
        }

        // A STALL in the data stage is how the device ends it early; the
        // status stage still follows once the endpoint is cleared (s6.7.2, s6.7.3).
        if (dataLength != 0)
        {
            UsbBulkPipe pipe = isIn ? _bulkIn : _bulkOut;
            UsbTransferResult data = isIn ? _bulkIn.Read(dataIn) : _bulkOut.Write(dataOut);
            if (data.Status == UsbTransferStatus.Stall)
            {
                pipe.ClearHalt();
            }
            else if (data.Status != UsbTransferStatus.Success)
            {
                return ResetRecovery(data.Status, "data transport failed");
            }
        }

        // A STALL on the status wrapper is retried once after clearing it (s5.3.3, figure 2).
        Span<byte> received = stackalloc byte[CommandStatusWrapper.Size];
        UsbTransferResult result = _bulkIn.Read(received);
        if (result.Status == UsbTransferStatus.Stall)
        {
            _bulkIn.ClearHalt();
            result = _bulkIn.Read(received);
        }

        CommandStatusWrapper status = new(received[..result.Length]);
        if (result.Status != UsbTransferStatus.Success || !status.IsValidFor(tag))
        {
            return ResetRecovery(result.Status, "no valid status");
        }

        residue = Math.Min(status.DataResidue, (uint)dataLength);
        return status.Status switch
        {
            CommandStatusWrapper.CommandPassed => BulkOnlyStatus.Passed,
            CommandStatusWrapper.CommandFailed => BulkOnlyStatus.Failed,
            _ => ResetRecovery(result.Status, "phase error")
        };
    }

    /// <summary>
    /// Brings the device back in step after a transport failure: Bulk-Only
    /// Mass Storage Reset, then the halt of both bulk endpoints cleared
    /// (BOT 1.0 s5.3.4). A device that left the bus is not reset: there is
    /// nothing left to bring back.
    /// </summary>
    /// <param name="cause">The status of the transfer that failed; success when it went through but what came back was wrong.</param>
    /// <param name="reason">What went wrong, for the log.</param>
    private BulkOnlyStatus ResetRecovery(UsbTransferStatus cause, string reason)
    {
        if (cause == UsbTransferStatus.Disconnected || !_context.IsPresent)
        {
            return BulkOnlyStatus.Disconnected;
        }

        _context.WriteLog($"{reason}, resetting the device");
        _context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, (byte)BulkOnlyRequest.Reset, 0, _context.Interface.Number);
        _bulkIn.ClearHalt();
        _bulkOut.ClearHalt();
        return BulkOnlyStatus.TransportError;
    }
}
