// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A driver's transmit path, as handed to
/// <see cref="DeviceContext.PublishNetworkLink"/>: hands one Ethernet frame
/// to the device. The kit calls it with interrupts masked and one call at a
/// time, so it needs no lock of its own against itself or against the
/// driver's interrupt handler. It may use the binding's register regions,
/// DMA buffers and <see cref="IrqSafeLock"/>s, and must not block: with
/// interrupts masked, nothing it could wait for would come.
/// </summary>
/// <param name="frame">
/// The frame, from the destination MAC address to the end of the payload,
/// without the CRC, which the device appends. Valid during the call only:
/// the driver copies it into its own DMA memory.
/// </param>
/// <returns>True when the device took the frame; false when it could not, such as with every transmit slot busy.</returns>
internal delegate bool NetworkTransmitHandler(ReadOnlySpan<byte> frame);
