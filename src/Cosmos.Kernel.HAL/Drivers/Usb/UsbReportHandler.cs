// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// Receives one report of an interrupt IN endpoint a driver opened through
/// <see cref="UsbDeviceContext.OpenInterruptIn"/>. Runs in interrupt
/// context: the host controller's interrupt handler, or a thread draining
/// the controller's events with interrupts masked. It must not allocate,
/// throw, block, take a lock other than an <see cref="IrqSafeLock"/>, call
/// through an interface or build a string; a <see cref="MouseReporter"/>,
/// a <see cref="DeviceWorkItem"/> and a <see cref="DeviceEvent"/> are safe
/// to call. It never runs before the driver's Probe returned Bound, nor
/// once the attempt was declined or failed or the device left the bus.
/// </summary>
/// <param name="report">The report, as long as the device sent it; valid only during the call.</param>
[Experimental(Experimentals.DriverKitDiagId)]
public delegate void UsbReportHandler(ReadOnlySpan<byte> report);
