// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The kind of a <see cref="DeviceResource"/>: what its base and length mean
/// and how the kit maps it.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum DeviceResourceKind
{
    /// <summary>
    /// A window of physical address space the device decodes: base is a
    /// physical address, length is in bytes. The kit maps it as device memory.
    /// </summary>
    MemoryWindow,

    /// <summary>
    /// A range of I/O ports (x64 only): base is the first port, length is the
    /// port count.
    /// </summary>
    PortRange,

    /// <summary>
    /// A window of ordinary RAM the kernel already maps as normal memory (a
    /// synthetic node's register page): base is the virtual address the
    /// allocator returned, length is in bytes. The kit never asks the
    /// platform to remap it: on ARM64 that would turn the 2 MiB block around
    /// it, heap included, into uncacheable device memory.
    /// </summary>
    RamWindow,

    /// <summary>
    /// No resource: a slot the bus keeps so the indices after it stay
    /// stable (a PCI function's six BARs) with nothing assigned to it. The
    /// binding refuses to map it.
    /// </summary>
    None,
}
