// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// One 16-byte slot of the descriptor pool, holding an ED, a TD or a SETUP
/// packet: its page and its index in it. The physical address is what the
/// controller sees and what a done queue entry names; <see cref="None"/>
/// is a slot that was never taken, or one a quarantine took away. Any
/// context, immutable.
/// </summary>
/// <param name="Page">The pool page, or null for <see cref="None"/>.</param>
/// <param name="Index">The slot's index in the page.</param>
internal readonly record struct OhciDescriptorSlot(OhciDescriptorPage? Page, int Index)
{
    /// <summary>The empty slot.</summary>
    public static OhciDescriptorSlot None => default;

    /// <summary>True when no slot stands behind the value.</summary>
    public bool IsNone => Page is null;

    /// <summary>The slot's physical address, 0 for <see cref="None"/>; every OHCI pointer is 32 bits wide.</summary>
    public uint PhysicalAddress => Page is null ? 0 : (uint)(Page.PhysicalAddress + ((ulong)Index * OhciProtocol.DescriptorBytes));
}
