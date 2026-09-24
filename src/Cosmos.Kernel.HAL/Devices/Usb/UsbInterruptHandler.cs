// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Receives the data of one completed interrupt IN transfer. Runs in
/// interrupt context (or from a polling caller with interrupts masked), so
/// it must not block; the span is only valid for the duration of the call.
/// </summary>
internal delegate void UsbInterruptHandler(ReadOnlySpan<byte> data);
