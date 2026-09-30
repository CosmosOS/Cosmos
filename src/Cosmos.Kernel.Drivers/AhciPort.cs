// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// One SATA port of an AHCI HBA, the block device <see cref="AhciDriver"/>
/// publishes to the ring as <c>sata{n}</c>, with the geometry and the
/// strings IDENTIFY DEVICE reported. Every transfer moves through the
/// port's one bounce page, at most <see cref="MaxSectorsPerCommand"/>
/// sectors per command, and one command is in flight per port: a caller
/// claims the port's busy flag under the controller's lock, held only
/// around the flag, and releases it when its command and its copy are
/// done, so chunks of two callers may interleave but never overlap.
/// Commands are polled to completion. Every method is thread context,
/// entered by the ring from any thread; a task file error surfaces as an
/// <see cref="IOException"/>, a stuck port or a timeout as an
/// <see cref="InvalidOperationException"/>.
/// </summary>
public sealed class AhciPort : IBlockDevice
{
    // --- Constants ---

    /// <summary>Bytes of one page: the bounce buffer is one, page aligned. The kit exposes no page size.</summary>
    private const int PageBytes = 4096;

    /// <summary>Sectors one command moves at most: one bounce page, one PRDT entry.</summary>
    private const uint MaxSectorsPerCommand = 8;

    /// <summary>How long the task file gets to leave BSY and DRQ before a command is issued.</summary>
    private const uint TaskFileMilliseconds = 1000;

    /// <summary>Bits in the low dword of a 64-bit address.</summary>
    private const int DwordBits = 32;

    /// <summary>Bits in one byte, for the LBA and count lanes of the FIS.</summary>
    private const int ByteBits = 8;

    /// <summary>Bits in one IDENTIFY word, for the multi-word counts.</summary>
    private const int WordBits = 16;

    /// <summary>Mask of one byte, for the low byte of an IDENTIFY word.</summary>
    private const int ByteMask = 0xFF;

    // --- Private fields ---

    private readonly AhciState _controller;
    private readonly DeviceBinding _binding;
    private readonly uint _portNumber;
    private readonly string _name;
    private readonly DmaBuffer _bounce;
    private readonly ulong _blockCount;
    private readonly string _model;
    private readonly string _serial;
    private readonly string _firmware;
    private bool _busy;

    // --- Constructor ---

    /// <summary>
    /// Brings the port up: refuses an ATAPI device, allocates the bounce
    /// page (below 4 GiB on a 32-bit HBA), issues IDENTIFY DEVICE and reads
    /// the strings and the capacity out of it. Thread context, from the
    /// probe, after the port was rebased and its engine started.
    /// </summary>
    /// <param name="controller">The HBA the port belongs to.</param>
    /// <param name="binding">The function's binding, for the bounce page and the delays.</param>
    /// <param name="portNumber">The port index on the HBA.</param>
    /// <param name="index">The port's <c>sata{index}</c> number, global across controllers.</param>
    /// <exception cref="InvalidOperationException">The port holds an ATAPI device, no bounce page could be allocated, or IDENTIFY did not complete.</exception>
    /// <exception cref="IOException">IDENTIFY ended in a task file error.</exception>
    internal AhciPort(AhciState controller, DeviceBinding binding, uint portNumber, int index)
    {
        _controller = controller;
        _binding = binding;
        _portNumber = portNumber;
        _name = "sata" + index;

        if ((controller.ReadPort(portNumber, AhciProtocol.PxCmd) & AhciProtocol.CmdAtapi) != 0)
        {
            throw new InvalidOperationException("the port is not a SATA device");
        }

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

        ushort[] identify = new ushort[AhciProtocol.IdentifyWords];
        Claim();
        try
        {
            IssueCommand(AhciProtocol.AtaIdentify, 0, 1, isWrite: false, useLba48: false, hasData: true);
            MemoryMarshal.Cast<byte, ushort>(_bounce.Span).Slice(0, AhciProtocol.IdentifyWords).CopyTo(identify);
        }
        finally
        {
            Release();
        }

        _serial = IdentifyString(identify, AhciProtocol.IdentifySerialWord, AhciProtocol.IdentifySerialWords);
        _firmware = IdentifyString(identify, AhciProtocol.IdentifyFirmwareWord, AhciProtocol.IdentifyFirmwareWords);
        _model = IdentifyString(identify, AhciProtocol.IdentifyModelWord, AhciProtocol.IdentifyModelWords);
        if ((identify[AhciProtocol.IdentifyCommandSetsWord] & AhciProtocol.IdentifyLba48Supported) != 0)
        {
            _blockCount = identify[AhciProtocol.IdentifyLba48CountWord]
                | ((ulong)identify[AhciProtocol.IdentifyLba48CountWord + 1] << WordBits)
                | ((ulong)identify[AhciProtocol.IdentifyLba48CountWord + 2] << (2 * WordBits))
                | ((ulong)identify[AhciProtocol.IdentifyLba48CountWord + 3] << (3 * WordBits));
        }
        else
        {
            _blockCount = identify[AhciProtocol.IdentifyLba28CountWord]
                | ((ulong)identify[AhciProtocol.IdentifyLba28CountWord + 1] << WordBits);
        }
    }

    // --- IBlockDevice ---

    /// <inheritdoc/>
    public ulong BlockCount => _blockCount;

    /// <summary>512: the sector size every command here assumes. Any context.</summary>
    public ulong BlockSize => AhciProtocol.SectorBytes;

    /// <summary><c>sata{index}</c>, the number taken from the driver's counter before IDENTIFY. Any context.</summary>
    public string Name => _name;

    /// <summary>
    /// Reads whole sectors with READ DMA EXT, at most
    /// <see cref="MaxSectorsPerCommand"/> per command through the bounce
    /// page, the port claimed per chunk. Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first sector.</param>
    /// <param name="blockCount">How many sectors.</param>
    /// <param name="data">Where they go; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the sectors asked for.</exception>
    /// <exception cref="IOException">A command ended in a task file error.</exception>
    /// <exception cref="InvalidOperationException">No free slot, the port stuck busy, or a command timed out.</exception>
    public void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
    {
        int sector = (int)AhciProtocol.SectorBytes;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));
        ulong done = 0;
        while (done < blockCount)
        {
            ulong remaining = blockCount - done;
            uint chunk = remaining >= MaxSectorsPerCommand ? MaxSectorsPerCommand : (uint)remaining;
            int bytes = sector * (int)chunk;
            Claim();
            try
            {
                IssueCommand(AhciProtocol.AtaReadDmaExt, blockNo + done, chunk, isWrite: false, useLba48: true, hasData: true);
                _bounce.Span.Slice(0, bytes).CopyTo(data.Slice((int)((long)done * sector), bytes));
            }
            finally
            {
                Release();
            }

            done += chunk;
        }
    }

    /// <summary>
    /// Writes whole sectors with WRITE DMA EXT, at most
    /// <see cref="MaxSectorsPerCommand"/> per command through the bounce
    /// page, the port claimed per chunk. Thread context; any thread.
    /// </summary>
    /// <param name="blockNo">The first sector.</param>
    /// <param name="blockCount">How many sectors.</param>
    /// <param name="data">Their bytes; at least <paramref name="blockCount"/> times <see cref="BlockSize"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the sectors given.</exception>
    /// <exception cref="IOException">A command ended in a task file error.</exception>
    /// <exception cref="InvalidOperationException">No free slot, the port stuck busy, or a command timed out.</exception>
    public void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
    {
        int sector = (int)AhciProtocol.SectorBytes;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / (uint)sector, nameof(blockCount));
        ulong done = 0;
        while (done < blockCount)
        {
            ulong remaining = blockCount - done;
            uint chunk = remaining >= MaxSectorsPerCommand ? MaxSectorsPerCommand : (uint)remaining;
            int bytes = sector * (int)chunk;
            Claim();
            try
            {
                data.Slice((int)((long)done * sector), bytes).CopyTo(_bounce.Span);
                IssueCommand(AhciProtocol.AtaWriteDmaExt, blockNo + done, chunk, isWrite: true, useLba48: true, hasData: true);
            }
            finally
            {
                Release();
            }

            done += chunk;
        }
    }

    /// <summary>Issues FLUSH CACHE EXT so completed writes reach the media. Thread context; any thread.</summary>
    /// <exception cref="IOException">The command ended in a task file error.</exception>
    /// <exception cref="InvalidOperationException">No free slot, the port stuck busy, or the command timed out.</exception>
    public void Flush()
    {
        Claim();
        try
        {
            IssueCommand(AhciProtocol.AtaCacheFlushExt, 0, 0, isWrite: false, useLba48: false, hasData: false);
        }
        finally
        {
            Release();
        }
    }

    // --- Properties ---

    /// <summary>The model number IDENTIFY reported, trimmed. Any context.</summary>
    public string Model => _model;

    /// <summary>The serial number IDENTIFY reported, trimmed. Any context.</summary>
    public string Serial => _serial;

    /// <summary>The firmware revision IDENTIFY reported, trimmed. Any context.</summary>
    public string Firmware => _firmware;

    /// <summary>The port index on the HBA. Any context.</summary>
    public uint PortNumber => _portNumber;

    /// <summary>The HBA the port belongs to. Any context.</summary>
    public AhciState Controller => _controller;

    // --- Private methods ---

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
        while (true)
        {
            using (deviceLock.Acquire())
            {
                if (!_busy)
                {
                    _busy = true;
                    return;
                }
            }

            _binding.Delay(AhciState.PollMicroseconds);
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

    /// <summary>
    /// Builds and issues one command on a free slot and polls it to
    /// completion: clears PxIS and PxSERR, writes the header, the PRDT
    /// entry over the bounce page when the command carries data, and the
    /// H2D FIS, waits for the task file to leave BSY and DRQ, orders the
    /// stores, writes PxCI, then polls the slot's bit until it clears. The
    /// caller holds the busy flag. Thread context; any thread.
    /// </summary>
    /// <param name="command">The ATA command.</param>
    /// <param name="lba">The first sector, used when <paramref name="useLba48"/>.</param>
    /// <param name="count">The sector count: the PRDT byte count for a data command, the FIS count when <paramref name="useLba48"/>.</param>
    /// <param name="isWrite">The data goes to the device.</param>
    /// <param name="useLba48">The FIS carries the LBA and the count.</param>
    /// <param name="hasData">The command moves data through the bounce page.</param>
    /// <exception cref="IOException">The port raised a task file error.</exception>
    /// <exception cref="InvalidOperationException">No slot was free, the port stuck busy, or the command timed out.</exception>
    private void IssueCommand(byte command, ulong lba, uint count, bool isWrite, bool useLba48, bool hasData)
    {
        uint port = _portNumber;
        _controller.WritePort(port, AhciProtocol.PxIs, AhciProtocol.Rw1CClearAll);
        _controller.WritePort(port, AhciProtocol.PxSerr, AhciProtocol.Rw1CClearAll);

        int slot = FindFreeSlot();
        if (slot < 0)
        {
            throw new InvalidOperationException("SATA: no free command slot.");
        }

        DmaBuffer region = _controller.CommandRegion;
        ulong tablePhysical = region.PhysicalAddress + (ulong)AhciProtocol.CommandTableOffset(port, slot);
        Span<byte> list = region.Span.Slice(AhciProtocol.CommandListOffset(port), AhciProtocol.CommandListBytes);
        Span<HbaCommandHeader> headers = MemoryMarshal.Cast<byte, HbaCommandHeader>(list);
        headers[slot] = new HbaCommandHeader
        {
            Flags0 = (byte)(AhciProtocol.HeaderFisDwords | (isWrite ? AhciProtocol.HeaderWrite : 0)),
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
                ByteCountAndInterrupt = ((count * AhciProtocol.SectorBytes - 1) & AhciProtocol.PrdtByteCountMask) | AhciProtocol.PrdtInterruptOnCompletion,
            };
        }

        Span<FisRegisterH2D> fis = MemoryMarshal.Cast<byte, FisRegisterH2D>(table.Slice(AhciProtocol.CommandTableFisOffset, AhciProtocol.FisRegisterH2DBytes));
        fis[0] = new FisRegisterH2D
        {
            FisType = AhciProtocol.FisTypeRegisterH2D,
            Flags = AhciProtocol.FisCommandFlag,
            Command = command,
            Device = useLba48 ? AhciProtocol.DeviceLbaMode : (byte)0,
            Lba0 = useLba48 ? (byte)lba : (byte)0,
            Lba1 = useLba48 ? (byte)(lba >> ByteBits) : (byte)0,
            Lba2 = useLba48 ? (byte)(lba >> (2 * ByteBits)) : (byte)0,
            Lba3 = useLba48 ? (byte)(lba >> (3 * ByteBits)) : (byte)0,
            Lba4 = useLba48 ? (byte)(lba >> (4 * ByteBits)) : (byte)0,
            Lba5 = useLba48 ? (byte)(lba >> (5 * ByteBits)) : (byte)0,
            CountLow = useLba48 ? (byte)count : (byte)0,
            CountHigh = useLba48 ? (byte)(count >> ByteBits) : (byte)0,
        };

        long busyDeadline = AhciState.DeadlineAfter(TaskFileMilliseconds);
        while ((_controller.ReadPort(port, AhciProtocol.PxTfd) & (AhciProtocol.TfdBusy | AhciProtocol.TfdDataRequest)) != 0)
        {
            if (Stopwatch.GetTimestamp() >= busyDeadline)
            {
                throw new InvalidOperationException("SATA: port stuck busy (TFD BSY/DRQ) before command issue.");
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
                throw new IOException("SATA Fatal error: Command aborted");
            }

            if (Stopwatch.GetTimestamp() >= deadline)
            {
                _controller.CountTimeout();
                throw new InvalidOperationException("SATA: command completion timeout.");
            }

            _binding.Delay(AhciState.PollMicroseconds);
        }

        DmaBuffer.ReadBarrier();
        _controller.CountCommand();
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

    /// <summary>An IDENTIFY string: each word gives its high byte first, then its low byte; trimmed. Any context.</summary>
    private static string IdentifyString(ushort[] words, int firstWord, int wordCount)
    {
        char[] chars = new char[wordCount * 2];
        for (int i = 0; i < wordCount; i++)
        {
            ushort word = words[firstWord + i];
            chars[i * 2] = (char)(word >> ByteBits);
            chars[i * 2 + 1] = (char)(word & ByteMask);
        }

        return new string(chars).Trim();
    }
}
