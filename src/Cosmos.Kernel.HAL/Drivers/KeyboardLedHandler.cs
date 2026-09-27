// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A driver's lamp path, as handed to
/// <see cref="DeviceContext.PublishKeyboard(KeyboardLedHandler)"/>: sets the
/// lock lamps of the keyboard the driver published. The kit calls it
/// whenever the kernel's keyboard manager toggles a lock, which happens
/// inside the very report that carried the lock key, so it runs in the
/// context the driver reported from, an interrupt handler included: it must
/// not allocate, throw, block or run a transfer, and a driver that has to do
/// one records the lamps and schedules a
/// <see cref="DeviceWorkItem"/>. An exception it throws is logged with the
/// driver's name and the device's path, and the report the manager was
/// handling still goes through.
/// </summary>
/// <param name="leds">The lamps that should be lit, as the kit tracked them from the lock keys the driver reported.</param>
[Experimental(Experimentals.DriverKitDiagId)]
public delegate void KeyboardLedHandler(KeyboardLeds leds);
