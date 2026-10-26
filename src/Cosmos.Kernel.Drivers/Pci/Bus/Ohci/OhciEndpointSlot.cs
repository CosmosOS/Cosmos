// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The software side of one ED in one of the controller's lists: its pool
/// slot and the list links that mirror the hardware NextED chain, so the
/// predecessor of an ED can be found when it is taken out. The permanent
/// list heads are these too, with no slot owner and no head of their own.
/// Written under the controller's lock.
/// </summary>
internal sealed class OhciEndpointSlot
{
    /// <summary>Creates the software side of an ED over its pool slot. Thread context.</summary>
    /// <param name="descriptor">The ED's pool slot.</param>
    public OhciEndpointSlot(OhciDescriptorSlot descriptor)
    {
        Descriptor = descriptor;
    }

    /// <summary>The ED's pool slot.</summary>
    public OhciDescriptorSlot Descriptor { get; }

    /// <summary>The ED after this one in the list, as the hardware chain has it; null at the end.</summary>
    public OhciEndpointSlot? Next { get; set; }

    /// <summary>The permanent head of the list the ED is linked in; null while unlinked.</summary>
    public OhciEndpointSlot? Head { get; set; }

    /// <summary>True while the ED is in a list.</summary>
    public bool IsLinked => Head is not null;

    /// <summary>The ED's physical address, what a NextED field carries.</summary>
    public uint PhysicalAddress => Descriptor.PhysicalAddress;
}
