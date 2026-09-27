// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// One PCI function as <see cref="PciFunctions"/> read it from PCI
/// enumeration: its bus position, its owner when it was read, and the
/// seam's <see cref="PciFunction"/> view of it for its IDs and
/// configuration space.
/// </summary>
internal sealed class PciFunctionState
{
    /// <summary>The IDs, class and configuration space, through the seam's own view of the function.</summary>
    public PciFunction Function { get; }

    /// <summary>The bus number.</summary>
    public uint Bus { get; }

    /// <summary>The device number on the bus.</summary>
    public uint Slot { get; }

    /// <summary>The function number in the device.</summary>
    public uint FunctionNumber { get; }

    /// <summary>
    /// The function's owner when it was read: a built-in's or a
    /// registration's name, <c>gop</c> for the boot display's reservation,
    /// or null.
    /// </summary>
    public string? Owner { get; }

    /// <summary>
    /// The path the kit documents for the function, as in
    /// <c>pci/0000:00:04.0</c>: segment 0000, then bus and device as two hex
    /// digits and the function as one, worked out here from the enumerated
    /// position rather than read from the kit.
    /// </summary>
    public string Path => $"pci/0000:{Bus:x2}:{Slot:x2}.{FunctionNumber:x}";

    /// <summary>Sorts functions by bus, then device, then function.</summary>
    public ulong BusOrderKey => ((ulong)Bus << 16) | ((ulong)Slot << 8) | FunctionNumber;

    /// <summary>Records one enumerated function.</summary>
    /// <param name="function">The seam's view of it.</param>
    /// <param name="bus">Its bus number.</param>
    /// <param name="slot">Its device number.</param>
    /// <param name="functionNumber">Its function number.</param>
    /// <param name="owner">Its owner when read.</param>
    public PciFunctionState(PciFunction function, uint bus, uint slot, uint functionNumber, string? owner)
    {
        Function = function;
        Bus = bus;
        Slot = slot;
        FunctionNumber = functionNumber;
        Owner = owner;
    }
}
