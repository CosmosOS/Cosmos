// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// Stands between the interrupt source and a driver's
/// <see cref="DeviceInterruptHandler"/>, whichever source the kit picked:
/// <see cref="OnMessage"/> is the MSI-X vector's handler and
/// <see cref="OnTimerTick"/> the polling timer's callback. Both do nothing
/// until the binding is armed, so neither a message the device raises
/// during Probe nor a tick that comes then reaches the driver, and nothing
/// does once a failed attempt disarmed it: a late message or tick lands
/// here and goes no further.
/// </summary>
internal sealed class InterruptTrampoline
{
    private readonly DeviceInterruptHandler _handler;

    /// <summary>
    /// Built in thread context when the interrupts are requested, so that
    /// the interrupt path, where a string cannot be built, can still name
    /// the driver and the device when a handler throws.
    /// </summary>
    private readonly string _failureMessage;

    /// <summary>Set once the binding is Bound; cleared first thing when an attempt is torn down.</summary>
    private volatile bool _armed;

    internal InterruptTrampoline(DeviceInterruptHandler handler, string driverName, string path)
    {
        _handler = handler;
        _failureMessage = $"[Drivers] {driverName} {path}: the interrupt handler threw";
    }

    /// <summary>Lets interrupts through to the driver's handler.</summary>
    internal void Arm() => _armed = true;

    /// <summary>Stops interrupts at the trampoline, from the next one on.</summary>
    internal void Disarm() => _armed = false;

    /// <summary>The MSI-X vector's handler. Interrupt context; the saved registers are not needed.</summary>
    internal void OnMessage(ref IRQContext _) => Invoke();

    /// <summary>The polling timer's callback. Interrupt context: the timer's own.</summary>
    internal void OnTimerTick() => Invoke();

    private void Invoke()
    {
        if (!_armed)
        {
            return;
        }

        try
        {
            _handler(0);
        }
        catch (Exception exception)
        {
            // A handler must not throw. Nothing above the interrupt dispatch
            // could catch it, so the kernel halts either way; this only makes
            // sure the panic says which driver it was.
            Serial.WriteString(exception.Message);
            Serial.WriteString("\n");
            Panic.Halt(_failureMessage);
        }
    }
}
