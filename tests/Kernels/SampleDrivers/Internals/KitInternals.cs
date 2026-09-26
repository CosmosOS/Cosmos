// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace SampleDrivers;

/// <summary>
/// Engine state of the driver kit that no public member reports, read for
/// the Drivers suite's cells: the timer that polls a PCI attempt's
/// interrupt handler, and whether the <c>driver-work</c> thread is running a
/// work item of a binding. A driver never needs either.
/// </summary>
/// <remarks>
/// Like every helper in this folder, it reaches internals through
/// <see cref="UnsafeAccessorAttribute"/>, and
/// <see cref="UnsafeAccessorTypeAttribute"/> for a type the seam does not
/// expose, which ILC binds at build time by name and signature, with no
/// InternalsVisibleTo grant (docs/articles/dev/accessing-internals.md).
/// Should a target be renamed, retyped or removed, its accessor becomes a
/// stub that throws <see cref="MissingFieldException"/> or
/// <see cref="MissingMethodException"/>, which fails the cell reading it
/// rather than letting it pass.
/// </remarks>
internal static class KitInternals
{
    private const string DriverWorkQueueType = "Cosmos.Kernel.HAL.Drivers.Engine.DriverWorkQueue, Cosmos.Kernel.HAL";

    /// <summary>
    /// The timer polling <paramref name="context"/>'s interrupt handler, or
    /// null when its interrupts go through MSI-X, were never granted, or
    /// were torn down. Read it during Probe, right after
    /// <see cref="PciDeviceContext.TryRequestInterrupts"/>. Thread context.
    /// </summary>
    /// <param name="context">The context of the attempt whose poll timer to read.</param>
    /// <returns>The poll timer, which the kit keeps in a private field of the context.</returns>
    /// <exception cref="MissingFieldException">The field is gone from <see cref="PciDeviceContext"/>.</exception>
    public static SoftwareTimer? PollTimer(PciDeviceContext context) => PollTimerField(context);

    /// <summary>
    /// True when the <c>driver-work</c> thread is running a work item that
    /// <paramref name="context"/>'s binding created, as the kit's unplug
    /// asks before it calls the driver's Remove. Any context.
    /// </summary>
    /// <param name="context">The binding's context.</param>
    /// <returns>What the kit's work queue answers.</returns>
    /// <exception cref="MissingMethodException">The kit's work queue no longer has the method.</exception>
    public static bool IsRunningWorkItemOf(DeviceContext context) => IsRunningItemOf(null, context);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_pollTimer")]
    private static extern ref SoftwareTimer? PollTimerField(PciDeviceContext context);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "IsRunningItemOf")]
    private static extern bool IsRunningItemOf([UnsafeAccessorType(DriverWorkQueueType)] object? queue, DeviceContext context);
}
