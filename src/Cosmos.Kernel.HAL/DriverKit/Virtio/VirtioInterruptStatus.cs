// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// What a virtio device signalled with one interrupt. The bit values are
/// the ISR status bits of the specification (section 4.1.4.5 over PCI, the
/// InterruptStatus register over MMIO), so a transport hands the register
/// through unchanged. Interrupt context; allocation-free.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
[Flags]
public enum VirtioInterruptStatus : uint
{
    /// <summary>Nothing pending.</summary>
    None = 0,

    /// <summary>A used ring was updated.</summary>
    Queue = 1,

    /// <summary>The device configuration changed.</summary>
    Config = 2,
}
