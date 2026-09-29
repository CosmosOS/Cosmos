// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// Everything a PCI host driver publishes for one function, as
/// <see cref="PciHostAccess.TryDescribeFunction"/> read it: the arguments
/// of <see cref="DeviceBinding.PublishChild"/>, in order. A default
/// instance, which the describe returns with false, holds nothing.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct PciFunctionDescription
{
    internal PciFunctionDescription(PciIdentity identity, DeviceResource[] resources, InterruptSource[] interrupts, PciAccess access)
    {
        Identity = identity;
        Resources = resources;
        Interrupts = interrupts;
        Access = access;
    }

    /// <summary>The function's identity.</summary>
    public PciIdentity Identity { get; }

    /// <summary>Six resources, one per base address register; an unassigned register is <see cref="DeviceResource.None"/>.</summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The array is handed to PublishChild unchanged.")]
    public DeviceResource[] Resources { get; }

    /// <summary>The function's interrupt sources: its legacy line at index 0, then one message source per described MSI-X table entry.</summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The array is handed to PublishChild unchanged.")]
    public InterruptSource[] Interrupts { get; }

    /// <summary>The access object of the function's node.</summary>
    public PciAccess Access { get; }
}
