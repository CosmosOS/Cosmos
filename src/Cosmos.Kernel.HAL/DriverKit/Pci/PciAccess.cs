// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// The access object of a PCI node: the function's configuration space,
/// its sized base address registers, its capability list and the Command
/// register bits a driver turns on. Behind it, for the kit only, the bus
/// hooks: the function is quiesced before the first driver sees it (bus
/// mastering off, legacy line disabled, a firmware-enabled MSI-X masked
/// and disabled), quiesced again after each probe that did not bind, before
/// the probe's memory is freed, restored as firmware left it when nobody
/// binds, and quiesced once more after a binding is torn down. A bound
/// driver owns the state from its probe on and enables what it needs. Any
/// context; allocation-free.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PciAccess : INodeHooks
{
    /// <summary>Command register offset (PCI 3.0 6.2.2).</summary>
    private const ushort CommandOffset = 0x04;
    /// <summary>Status register offset.</summary>
    private const ushort StatusOffset = 0x06;
    /// <summary>Capabilities pointer offset of a type 0 or type 1 header.</summary>
    private const ushort CapabilityPointerOffset = 0x34;
    /// <summary>Offset of the next pointer within a capability header.</summary>
    private const ushort CapabilityNextOffset = 1;
    /// <summary>Mask clearing the two reserved low bits of a capability pointer.</summary>
    private const byte CapabilityPointerMask = 0xFC;
    /// <summary>Upper bound on capability list entries: the area 0x40..0xFF holds at most 48 dword-aligned ones.</summary>
    private const int MaxCapabilityEntries = 48;
    /// <summary>Highest header layout with a capabilities pointer at <see cref="CapabilityPointerOffset"/>.</summary>
    private const byte LastHeaderTypeWithCapabilityPointer = 1;
    /// <summary>Status bit 4: a capabilities list is present.</summary>
    private const ushort StatusCapabilitiesList = 0x0010;

    /// <summary>Command bit 0: I/O space decode.</summary>
    private const ushort CommandIoSpace = 0x0001;
    /// <summary>Command bit 1: memory space decode.</summary>
    private const ushort CommandMemorySpace = 0x0002;
    /// <summary>Command bit 2: bus mastering.</summary>
    private const ushort CommandBusMaster = 0x0004;
    /// <summary>Command bit 10: INTx disabled.</summary>
    private const ushort CommandInterruptDisable = 0x0400;

    /// <summary>The MSI-X capability id.</summary>
    private const byte MsiXCapabilityId = 0x11;
    /// <summary>Offset of Message Control within the MSI-X capability.</summary>
    private const ushort MsiXMessageControlOffset = 0x02;
    /// <summary>Message Control bit 15: MSI-X enable.</summary>
    private const ushort MsiXEnable = 0x8000;
    /// <summary>Message Control bit 14: function mask.</summary>
    private const ushort MsiXFunctionMask = 0x4000;
    /// <summary>Message Control bits 10:0: table size minus one.</summary>
    private const ushort MsiXTableSizeMask = 0x07FF;

    private readonly PciConfigSpace _configSpace;
    private readonly byte _bus;
    private readonly byte _device;
    private readonly byte _function;
    private readonly byte _headerType;
    private readonly PciBar[] _bars;
    private readonly byte _msiXCapability;
    private readonly int _messageInterruptCount;
    private ushort _savedCommand;
    private ushort _savedMessageControl;
    private bool _hasSnapshot;

    internal PciAccess(PciConfigSpace configSpace, byte bus, byte device, byte function, byte headerType, PciBar[] bars, byte interruptLine, byte interruptPin)
    {
        _configSpace = configSpace;
        _bus = bus;
        _device = device;
        _function = function;
        _headerType = headerType;
        _bars = bars;
        InterruptLine = interruptLine;
        InterruptPin = interruptPin;
        _msiXCapability = FindCapability(MsiXCapabilityId);
        _messageInterruptCount = _msiXCapability == 0
            ? 0
            : (ReadConfig16((ushort)(_msiXCapability + MsiXMessageControlOffset)) & MsiXTableSizeMask) + 1;
        MessageTable = _msiXCapability == 0
            ? null
            : new PciMessageTable(this, _msiXCapability, _messageInterruptCount);
    }

    /// <summary>The six base address registers as the host sized them; the resource at the same index is the mappable form.</summary>
    public ReadOnlySpan<PciBar> Bars => _bars;

    /// <summary>The interrupt line register at 0x3C as firmware wrote it, read at describe time.</summary>
    public byte InterruptLine { get; }

    /// <summary>The interrupt pin register at 0x3D, read at describe time: 0 for none, 1 to 4 for INTA to INTD.</summary>
    public byte InterruptPin { get; }

    /// <summary>True when the function has an MSI-X capability.</summary>
    public bool IsMsiXCapable => _msiXCapability != 0;

    /// <summary>The MSI-X table size (Message Control's table size plus one), or 0 without the capability.</summary>
    public int MessageInterruptCount => _messageInterruptCount;

    /// <summary>The MSI-X table the kit programs for the function's message interrupt sources; null without the capability.</summary>
    internal PciMessageTable? MessageTable { get; }

    /// <summary>The mechanism the function is reached through.</summary>
    internal PciConfigSpace ConfigSpace => _configSpace;

    /// <summary>The bus number.</summary>
    internal byte Bus => _bus;

    /// <summary>The device number.</summary>
    internal byte Device => _device;

    /// <summary>The function number.</summary>
    internal byte Function => _function;

    /// <summary>Reads one byte of this function's configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The register offset.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is past the mechanism's size (256 for the port mechanism, 4096 for ECAM).</exception>
    public byte ReadConfig8(ushort offset)
    {
        ThrowIfOutOfRange(offset, sizeof(byte));
        return _configSpace.Read8(_bus, _device, _function, offset);
    }

    /// <summary>Reads one word of this function's configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The register offset, even.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is odd or past the mechanism's size.</exception>
    public ushort ReadConfig16(ushort offset)
    {
        ThrowIfOutOfRange(offset, sizeof(ushort));
        return _configSpace.Read16(_bus, _device, _function, offset);
    }

    /// <summary>Reads one dword of this function's configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The register offset, dword aligned.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is not dword aligned or past the mechanism's size.</exception>
    public uint ReadConfig32(ushort offset)
    {
        ThrowIfOutOfRange(offset, sizeof(uint));
        return _configSpace.Read32(_bus, _device, _function, offset);
    }

    /// <summary>Writes one byte of this function's configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The register offset.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is past the mechanism's size.</exception>
    public void WriteConfig8(ushort offset, byte value)
    {
        ThrowIfOutOfRange(offset, sizeof(byte));
        _configSpace.Write8(_bus, _device, _function, offset, value);
    }

    /// <summary>Writes one word of this function's configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The register offset, even.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is odd or past the mechanism's size.</exception>
    public void WriteConfig16(ushort offset, ushort value)
    {
        ThrowIfOutOfRange(offset, sizeof(ushort));
        _configSpace.Write16(_bus, _device, _function, offset, value);
    }

    /// <summary>Writes one dword of this function's configuration space. Any context; allocation-free.</summary>
    /// <param name="offset">The register offset, dword aligned.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is not dword aligned or past the mechanism's size.</exception>
    public void WriteConfig32(ushort offset, uint value)
    {
        ThrowIfOutOfRange(offset, sizeof(uint));
        _configSpace.Write32(_bus, _device, _function, offset, value);
    }

    /// <summary>
    /// Walks the capability list for <paramref name="capabilityId"/>: from
    /// the capabilities pointer, or from the entry after
    /// <paramref name="after"/> to find the next one of the same id. The
    /// walk is bounded to 48 entries, so a malformed list ends. Any
    /// context; allocation-free.
    /// </summary>
    /// <param name="capabilityId">The capability id to look for.</param>
    /// <param name="after">The offset of a capability the walk starts after, or 0 for the head of the list.</param>
    /// <returns>The capability's offset, or 0 when absent.</returns>
    public byte FindCapability(byte capabilityId, byte after = 0)
    {
        if (_headerType > LastHeaderTypeWithCapabilityPointer)
        {
            return 0;
        }

        if ((ReadConfig16(StatusOffset) & StatusCapabilitiesList) == 0)
        {
            return 0;
        }

        ushort pointer = after == 0 ? CapabilityPointerOffset : (ushort)(after + CapabilityNextOffset);
        byte offset = (byte)(ReadConfig8(pointer) & CapabilityPointerMask);
        for (int i = 0; offset != 0 && i < MaxCapabilityEntries; i++)
        {
            if (ReadConfig8(offset) == capabilityId)
            {
                return offset;
            }

            offset = (byte)(ReadConfig8((ushort)(offset + CapabilityNextOffset)) & CapabilityPointerMask);
        }

        return 0;
    }

    /// <summary>Turns bus mastering on or off: a read-modify-write of Command under the mechanism's lock. Any context; allocation-free.</summary>
    /// <param name="enable">True to let the function initiate DMA.</param>
    public void EnableBusMastering(bool enable) => UpdateCommand(CommandBusMaster, enable);

    /// <summary>Turns memory space decoding on or off: a read-modify-write of Command under the mechanism's lock. Any context; allocation-free.</summary>
    /// <param name="enable">True to let the function decode its memory windows.</param>
    public void EnableMemorySpace(bool enable) => UpdateCommand(CommandMemorySpace, enable);

    /// <summary>Turns I/O space decoding on or off: a read-modify-write of Command under the mechanism's lock. Any context; allocation-free.</summary>
    /// <param name="enable">True to let the function decode its port ranges.</param>
    public void EnableIoSpace(bool enable) => UpdateCommand(CommandIoSpace, enable);

    /// <summary>Sets or clears Command's INTx disable bit, for the line source. Any context; allocation-free.</summary>
    /// <param name="disable">True to keep the function off its legacy line.</param>
    internal void SetInterruptDisable(bool disable) => UpdateCommand(CommandInterruptDisable, disable);

    /// <summary>
    /// Snapshots Command and, with the capability, MSI-X Message Control
    /// the first time it runs (the snapshot is firmware's state; a later
    /// call, after a probe changed the registers, keeps it), then turns bus
    /// mastering off, disables the legacy line and, when MSI-X is enabled,
    /// disables it with the function mask set. Worker only, from the bus
    /// hooks.
    /// </summary>
    internal void Quiesce()
    {
        using (_configSpace.AcquireLock())
        {
            ushort command = _configSpace.Read16(_bus, _device, _function, CommandOffset);
            ushort messageControl = 0;
            if (_msiXCapability != 0)
            {
                messageControl = _configSpace.Read16(_bus, _device, _function, MessageControlOffset());
            }

            if (!_hasSnapshot)
            {
                _savedCommand = command;
                _savedMessageControl = messageControl;
                _hasSnapshot = true;
            }

            _configSpace.Write16(_bus, _device, _function, CommandOffset, (ushort)((command & ~CommandBusMaster) | CommandInterruptDisable));
            if (_msiXCapability != 0 && (messageControl & MsiXEnable) != 0)
            {
                _configSpace.Write16(_bus, _device, _function, MessageControlOffset(), (ushort)((messageControl & ~MsiXEnable) | MsiXFunctionMask));
            }
        }
    }

    /// <summary>
    /// Writes the snapshot <see cref="Quiesce"/> took back: Command, then
    /// Message Control. Nothing without a snapshot. Worker only, from the
    /// bus hooks.
    /// </summary>
    internal void Restore()
    {
        using (_configSpace.AcquireLock())
        {
            if (!_hasSnapshot)
            {
                return;
            }

            _configSpace.Write16(_bus, _device, _function, CommandOffset, _savedCommand);
            if (_msiXCapability != 0)
            {
                _configSpace.Write16(_bus, _device, _function, MessageControlOffset(), _savedMessageControl);
            }
        }
    }

    /// <inheritdoc/>
    void INodeHooks.BeforeFirstOffer() => Quiesce();

    /// <inheritdoc/>
    void INodeHooks.AfterOfferDeclined() => Quiesce();

    /// <inheritdoc/>
    void INodeHooks.AfterUnbound() => Restore();

    /// <inheritdoc/>
    void INodeHooks.AfterTeardown(bool hardwarePresent)
    {
        if (hardwarePresent)
        {
            Quiesce();
        }
    }

    private ushort MessageControlOffset() => (ushort)(_msiXCapability + MsiXMessageControlOffset);

    private void UpdateCommand(ushort bits, bool set)
    {
        using (_configSpace.AcquireLock())
        {
            ushort command = _configSpace.Read16(_bus, _device, _function, CommandOffset);
            command = set ? (ushort)(command | bits) : (ushort)(command & ~bits);
            _configSpace.Write16(_bus, _device, _function, CommandOffset, command);
        }
    }

    /// <summary>Refuses an access the mechanism does not reach or the register does not align to.</summary>
    private void ThrowIfOutOfRange(ushort offset, int size)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + size, _configSpace.Size, nameof(offset));
        if ((offset & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "A configuration register is read at its natural alignment.");
        }
    }
}
