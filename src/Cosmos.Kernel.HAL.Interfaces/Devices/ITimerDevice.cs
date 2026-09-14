// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Delegate for handling timer tick events.
/// </summary>
internal delegate void TimerTickHandler();

/// <summary>
/// Interface for timer devices.
/// </summary>
internal interface ITimerDevice
{
    /// <summary>
    /// Initialize the timer device.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Gets the timer frequency in Hz.
    /// </summary>
    uint Frequency { get; }

    /// <summary>
    /// Sets the timer frequency in Hz. Devices divide a fixed input clock, so
    /// each has a range it can express and rejects the rest.
    /// </summary>
    /// <param name="frequency">Frequency in Hz.</param>
    /// <returns>
    /// True when the device accepted the frequency; false when it is outside
    /// what the device can divide to, in which case the tick is unchanged.
    /// </returns>
    bool SetFrequency(uint frequency);

    /// <summary>
    /// Blocks for the specified number of milliseconds.
    /// </summary>
    /// <param name="ms">Milliseconds to wait.</param>
    void Wait(uint ms);

    /// <summary>
    /// Registers a software timer driven by this device's periodic tick.
    /// </summary>
    /// <param name="timer">Timer to register.</param>
    void RegisterTimer(SoftwareTimer timer);

    /// <summary>
    /// Unregisters a previously registered software timer.
    /// </summary>
    /// <param name="timer">Timer to unregister.</param>
    /// <returns>True when the timer was registered and has been removed.</returns>
    bool UnregisterTimer(SoftwareTimer timer);

    /// <summary>
    /// Event handler for timer tick events.
    /// </summary>
    TimerTickHandler? OnTick { get; set; }
}
