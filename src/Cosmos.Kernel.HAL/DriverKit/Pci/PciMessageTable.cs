// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// The MSI-X capability and table of one PCI function, owned by the kit
/// for the function's message interrupt sources. Created by the
/// <see cref="PciAccess"/> constructor when the capability exists. The
/// table is mapped through the HHDM alias at the first connect and the
/// capability enabled then, with every entry masked; each connect programs
/// one entry with the address and data the platform's message binder
/// hands out, each disconnect masks it and gives its routing slot back,
/// and the last disconnect disables the capability and releases the
/// binder's device state. Connect and disconnect run in thread context on
/// the kit worker, serialized by the kit; mask and unmask are one table
/// write each, allocation-free, from any context.
/// </summary>
internal sealed class PciMessageTable
{
    /// <summary>Offset of Message Control within the capability (PCI 3.0 6.8.2.3).</summary>
    private const ushort MessageControlOffset = 0x02;
    /// <summary>Message Control bit 15: MSI-X enable.</summary>
    private const ushort MessageControlEnable = 0x8000;
    /// <summary>Message Control bit 14: function mask, masking every entry while set.</summary>
    private const ushort MessageControlFunctionMask = 0x4000;
    /// <summary>Offset of the Table Offset/Table BIR register within the capability (PCI 3.0 6.8.2.4).</summary>
    private const ushort TableOffsetBirOffset = 0x04;
    /// <summary>Bits 2:0 of the Table Offset/Table BIR register: the base address register holding the table.</summary>
    private const uint TableBirMask = 0x7;
    /// <summary>Bits 31:3 of the Table Offset/Table BIR register: the table's qword-aligned offset into that register's window.</summary>
    private const uint TableOffsetMask = 0xFFFFFFF8;
    /// <summary>Bytes per table entry (PCI 3.0 6.8.2.9).</summary>
    private const uint EntryStride = 16;
    /// <summary>Entry offset of the message address's low dword.</summary>
    private const uint EntryAddressLow = 0;
    /// <summary>Entry offset of the message address's high dword.</summary>
    private const uint EntryAddressHigh = 4;
    /// <summary>Entry offset of the message data dword.</summary>
    private const uint EntryData = 8;
    /// <summary>Entry offset of the vector control dword.</summary>
    private const uint EntryVectorControl = 12;
    /// <summary>Vector control bit 0: the entry is masked.</summary>
    private const uint VectorControlMask = 1;
    /// <summary>Right shift extracting the high dword of the 64-bit message address.</summary>
    private const int AddressHighShift = 32;
    /// <summary>Command register offset (PCI 3.0 6.2.2).</summary>
    private const ushort CommandOffset = 0x04;
    /// <summary>Command bit 1: memory space decode, without which the table is not decoded.</summary>
    private const ushort CommandMemorySpace = 0x0002;
    /// <summary>The CPU every message is aimed at: the boot CPU, until SMP routes elsewhere.</summary>
    private const uint TargetCpu = 0;

    private readonly PciAccess _access;
    private readonly byte _capability;
    private readonly int _entryCount;
    private readonly bool[] _bound;
    private ulong _tableVirtual;
    private object? _deviceContext;
    private int _connectedCount;
    private bool _tableStale;

    /// <summary>The table of <paramref name="access"/>'s MSI-X capability at <paramref name="capability"/>, with <paramref name="entryCount"/> entries.</summary>
    /// <param name="access">The function's access object.</param>
    /// <param name="capability">The configuration offset of the MSI-X capability.</param>
    /// <param name="entryCount">Message Control's table size plus one.</param>
    internal PciMessageTable(PciAccess access, byte capability, int entryCount)
    {
        _access = access;
        _capability = capability;
        _entryCount = entryCount;
        _bound = new bool[entryCount];
    }

    /// <summary>The number of entries the table holds.</summary>
    internal int EntryCount => _entryCount;

    /// <summary>
    /// Routes entry <paramref name="index"/> to <paramref name="handler"/>.
    /// The first connect maps the table, prepares the function with the
    /// platform's binder, masks every entry, enables the capability with
    /// the function mask clear and disables the legacy line. Every connect
    /// then masks the entry, binds its routing slot, writes the address
    /// and data and unmasks it. Thread context, on the kit worker.
    /// </summary>
    /// <param name="index">The entry, 0 to <see cref="EntryCount"/> - 1.</param>
    /// <param name="handler">The platform handler the message runs.</param>
    /// <returns>
    /// False when the entry is out of range or already bound, when the
    /// platform has no message binder, when memory space decoding is off
    /// in Command (the table is not decoded then: a driver enables memory
    /// space before it requests a message), when the table's base address
    /// register is unassigned or I/O, when the table cannot be mapped, or
    /// when the binder cannot route this function or has no slot left.
    /// </returns>
    internal bool TryConnect(int index, InterruptManager.IrqDelegate handler)
    {
        if (index < 0 || index >= _entryCount || _bound[index])
        {
            return false;
        }

        if (!MsiRouting.IsAvailable)
        {
            return false;
        }

        if ((_access.ReadConfig16(CommandOffset) & CommandMemorySpace) == 0)
        {
            return false;
        }

        if (!TryMapTable())
        {
            return false;
        }

        if (_deviceContext is null)
        {
            // A binder that cannot route this function (an ITS DeviceID
            // past the device table) throws; the request is refused and
            // the driver takes its polled path.
            try
            {
                _deviceContext = MsiRouting.PrepareDevice(_access.Bus, _access.Device, _access.Function, _entryCount);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        if (_connectedCount == 0)
        {
            // Every entry is masked before the capability is enabled, so
            // no stale entry can fire; the table is decoded at this point.
            for (int i = 0; i < _entryCount; i++)
            {
                Native.MMIO.Write32(EntryAddress(i) + EntryVectorControl, VectorControlMask);
            }

            _tableStale = false;
            ushort messageControl = _access.ReadConfig16(MessageControlRegister());
            messageControl = (ushort)((messageControl & ~MessageControlFunctionMask) | MessageControlEnable);
            _access.WriteConfig16(MessageControlRegister(), messageControl);
            _access.SetInterruptDisable(true);
        }
        else if (_tableStale)
        {
            // A decode-off disconnect skipped its table mask and set the
            // function mask instead: with the table decoded again, the
            // unbound entries are masked in the table before the function
            // mask is lifted from the entries that stayed bound.
            for (int i = 0; i < _entryCount; i++)
            {
                if (!_bound[i])
                {
                    Native.MMIO.Write32(EntryAddress(i) + EntryVectorControl, VectorControlMask);
                }
            }

            _tableStale = false;
            ushort messageControl = _access.ReadConfig16(MessageControlRegister());
            _access.WriteConfig16(MessageControlRegister(), (ushort)(messageControl & ~MessageControlFunctionMask));
        }

        // The mask comes first: an entry whose address or data changes
        // while unmasked is undefined (PCI 3.0 6.8.3.5).
        ulong entry = EntryAddress(index);
        Native.MMIO.Write32(entry + EntryVectorControl, VectorControlMask);

        ulong address;
        uint data;
        try
        {
            MsiRouting.BindEntry(_deviceContext, index, handler, TargetCpu, out address, out data);
        }
        catch (InvalidOperationException)
        {
            if (_connectedCount == 0)
            {
                DisableCapability();
            }

            return false;
        }

        Native.MMIO.Write32(entry + EntryAddressLow, (uint)address);
        Native.MMIO.Write32(entry + EntryAddressHigh, (uint)(address >> AddressHighShift));
        Native.MMIO.Write32(entry + EntryData, data);
        Native.MMIO.Write32(entry + EntryVectorControl, 0);

        _bound[index] = true;
        _connectedCount++;
        return true;
    }

    /// <summary>
    /// Sets the vector control mask bit of entry <paramref name="index"/>;
    /// nothing for an entry that is not bound. One table write, not
    /// guarded by the Command register: masking with memory decoding off
    /// is the driver's misuse. Allocation-free; any context.
    /// </summary>
    /// <param name="index">The entry.</param>
    internal void Mask(int index)
    {
        if (index < 0 || index >= _entryCount || !_bound[index])
        {
            return;
        }

        Native.MMIO.Write32(EntryAddress(index) + EntryVectorControl, VectorControlMask);
    }

    /// <summary>
    /// Clears the vector control mask bit of entry <paramref name="index"/>;
    /// nothing for an entry that is not bound. One table write, not
    /// guarded by the Command register. Allocation-free; any context.
    /// </summary>
    /// <param name="index">The entry.</param>
    internal void Unmask(int index)
    {
        if (index < 0 || index >= _entryCount || !_bound[index])
        {
            return;
        }

        Native.MMIO.Write32(EntryAddress(index) + EntryVectorControl, 0);
    }

    /// <summary>
    /// Undoes <see cref="TryConnect"/> for entry <paramref name="index"/>,
    /// configuration space first, as <c>MsiX.Disable</c> does. With memory
    /// decoding on, the entry is masked through the table and its vector
    /// control read back, so the posted write has landed. With decoding
    /// off the table is not touched (an unclaimed posted write is dropped
    /// on x64 and an asynchronous abort on some ARM64 root complexes): the
    /// function mask is set in Message Control and read back, and the
    /// table is marked stale for the next connect. Only after that
    /// read-back is the routing slot given back. The last disconnect
    /// disables the capability with the function mask left set and
    /// releases the binder's device state; the table mapping stays.
    /// Nothing for an entry that is not bound. Thread context, on the kit
    /// worker.
    /// </summary>
    /// <param name="index">The entry.</param>
    internal void Disconnect(int index)
    {
        if (index < 0 || index >= _entryCount || !_bound[index])
        {
            return;
        }

        if ((_access.ReadConfig16(CommandOffset) & CommandMemorySpace) != 0)
        {
            ulong entry = EntryAddress(index);
            Native.MMIO.Write32(entry + EntryVectorControl, VectorControlMask);
            _ = Native.MMIO.Read32(entry + EntryVectorControl);
        }
        else
        {
            ushort messageControl = _access.ReadConfig16(MessageControlRegister());
            _access.WriteConfig16(MessageControlRegister(), (ushort)(messageControl | MessageControlFunctionMask));
            _ = _access.ReadConfig16(MessageControlRegister());
            _tableStale = true;
        }

        MsiRouting.UnbindEntry(_deviceContext, index);
        _bound[index] = false;
        _connectedCount--;
        if (_connectedCount == 0)
        {
            DisableCapability();
        }
    }

    /// <summary>
    /// Locates the table through the capability's Table Offset/Table BIR
    /// register, maps its window and records the HHDM alias. Thread
    /// context.
    /// </summary>
    /// <returns>False when the base address register is unassigned or I/O, or the window cannot be mapped.</returns>
    private bool TryMapTable()
    {
        uint tableBirOffset = _access.ReadConfig32((ushort)(_capability + TableOffsetBirOffset));
        int bir = (int)(tableBirOffset & TableBirMask);
        ulong tableOffset = tableBirOffset & TableOffsetMask;
        if (bir >= _access.Bars.Length)
        {
            return false;
        }

        PciBar bar = _access.Bars[bir];
        if (!bar.IsAssigned || bar.IsIo)
        {
            return false;
        }

        ulong tablePhysical = bar.Base + tableOffset;
        if (!DeviceMemory.EnsureWindowMapped(tablePhysical, (ulong)_entryCount * EntryStride))
        {
            return false;
        }

        _tableVirtual = tablePhysical + DeviceMemory.HhdmOffset();
        return true;
    }

    /// <summary>
    /// Writes Message Control with the function mask set and MSI-X enable
    /// clear, reads it back (an ECAM store can retire before the write
    /// reaches the function; the read cannot complete ahead of it), then
    /// releases the binder's device state. Thread context.
    /// </summary>
    private void DisableCapability()
    {
        ushort messageControl = _access.ReadConfig16(MessageControlRegister());
        messageControl = (ushort)((messageControl | MessageControlFunctionMask) & ~MessageControlEnable);
        _access.WriteConfig16(MessageControlRegister(), messageControl);
        _ = _access.ReadConfig16(MessageControlRegister());
        MsiRouting.ReleaseDevice(_deviceContext);
        _deviceContext = null;
    }

    /// <summary>The configuration offset of Message Control.</summary>
    private ushort MessageControlRegister() => (ushort)(_capability + MessageControlOffset);

    /// <summary>The virtual address of entry <paramref name="index"/> through the HHDM alias.</summary>
    private ulong EntryAddress(int index) => _tableVirtual + (ulong)index * EntryStride;
}
