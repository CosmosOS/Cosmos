// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A driver's interrupt handler, as handed to
/// <see cref="Pci.PciDeviceContext.TryRequestInterrupts"/>. It runs in
/// interrupt context with interrupts masked, whether the kit delivers the
/// device's MSI-X message or polls from the timer interrupt, so it must not
/// allocate, throw, block, build strings, call through an interface or take
/// any lock but an <see cref="IrqSafeLock"/>. It may use the binding's
/// register regions and DMA buffers, schedule a <see cref="DeviceWorkItem"/>
/// and signal a <see cref="DeviceEvent"/>. It never runs before the
/// driver's Probe returned Bound. When polled it runs on every timer tick
/// whether or not the device raised anything, so it reads the device's own
/// status to find out.
/// </summary>
/// <param name="vector">Which of the binding's interrupts fired: always 0 in this version, which grants one.</param>
internal delegate void DeviceInterruptHandler(int vector);
