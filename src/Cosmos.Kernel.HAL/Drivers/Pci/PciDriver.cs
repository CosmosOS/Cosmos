// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Pci;

/// <summary>
/// Base class of a driver for PCI functions. The kit creates one instance
/// per function it offers the driver, through the registration's factory,
/// and calls <see cref="Probe"/> on it once. A bound instance lives as long
/// as the kernel: a PCI function is never unbound in this version.
/// </summary>
internal abstract class PciDriver
{
    /// <summary>
    /// Decides whether this driver takes the function behind
    /// <paramref name="context"/> and, if it does, brings the function up
    /// with the resources the context hands out: mapped BARs, DMA memory, bus
    /// mastering. Runs in thread context with interrupts on, on a thread the
    /// driver must not depend on. It may allocate and busy-wait
    /// (<see cref="DeviceContext.Delay"/>), but must not sleep, since during
    /// the boot pass it runs on the idle thread, nor register drivers.
    /// </summary>
    /// <param name="context">The function on offer and everything the driver may acquire for it.</param>
    /// <returns>
    /// <see cref="ProbeResult.Bound"/> to keep the function. Anything else,
    /// or an exception, makes the kit release what the probe acquired and
    /// offer the function to the next candidate.
    /// </returns>
    protected internal abstract ProbeResult Probe(PciDeviceContext context);
}
