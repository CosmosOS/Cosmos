// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// Receives one completed transfer of an interrupt IN pipe. Interrupt
/// context, under no lock, on a controller with a message interrupt; on a
/// polled one, thread context on whichever thread drains the controller's
/// events (its hot-plug thread, or any thread waiting on one of its commands
/// or transfers), under the host's own <see cref="DeviceLock"/>; the span
/// is valid for the call only; the handler must not block, allocate, or
/// call any <see cref="UsbAccess"/> or <see cref="DeviceBinding"/> member;
/// it may call a sink and <see cref="DeviceEvent.Signal"/>, and may use
/// <see cref="Interlocked"/> on its own fields.
/// </summary>
/// <param name="report">The bytes the device sent in this transfer.</param>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public delegate void UsbReportHandler(ReadOnlySpan<byte> report);
