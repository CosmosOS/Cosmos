// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.Drivers;

namespace SampleDrivers;

/// <summary>
/// The mouse device the kit registers with the mouse manager for a
/// <see cref="MouseReporter"/>, an internal <c>PublishedMouse</c>, as the
/// unplug cells observe it: whether the kit withdrew it, and whether the
/// manager still wired its event handler. Reached through the helpers
/// described on <see cref="KitInternals"/>, each of which throws should its
/// target move. Every member can be read in any context.
/// </summary>
public sealed class PublishedMouseView
{
    private const string PublishedMouseType = "Cosmos.Kernel.HAL.Drivers.Engine.PublishedMouse, Cosmos.Kernel.HAL";
    private const string MouseDeviceType = "Cosmos.Kernel.HAL.Devices.Input.MouseDevice, Cosmos.Kernel.HAL";
    private const string MouseEventHandlerType = "Cosmos.Kernel.HAL.Devices.Input.MouseEventHandler, Cosmos.Kernel.HAL";

    /// <summary>
    /// The internal device itself, for a helper that hands it to a manager
    /// member taking the device's interface, and to compare two views by.
    /// </summary>
    public object Device { get; }

    /// <summary>True once the kit withdrew the mouse: on unplug, before the manager lets go of it.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the property.</exception>
    public bool IsWithdrawn => GetIsWithdrawn(Device);

    /// <summary>True while the mouse manager has an event handler wired to the device.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the property.</exception>
    public bool HasEventHandler => GetOnMouseEvent(Device) is not null;

    /// <summary>Views the device behind <paramref name="reporter"/>.</summary>
    /// <param name="reporter">A mouse a driver published.</param>
    /// <exception cref="MissingMethodException">The reporter no longer exposes its device.</exception>
    public PublishedMouseView(MouseReporter reporter)
    {
        Device = GetDevice(reporter);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Device")]
    [return: UnsafeAccessorType(PublishedMouseType)]
    private static extern object GetDevice(MouseReporter reporter);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsWithdrawn")]
    private static extern bool GetIsWithdrawn([UnsafeAccessorType(PublishedMouseType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_OnMouseEvent")]
    [return: UnsafeAccessorType(MouseEventHandlerType)]
    private static extern object? GetOnMouseEvent([UnsafeAccessorType(MouseDeviceType)] object device);
}
