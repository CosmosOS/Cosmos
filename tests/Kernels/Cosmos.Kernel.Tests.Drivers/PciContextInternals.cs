// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// Reads private state of a <see cref="PciDeviceContext"/> that no member
/// exposes, so the teardown cells can check a step a driver cannot see:
/// the timer polling a failed attempt's handler leaving the platform timer.
/// The suite's InternalsVisibleTo grant reaches internal members only.
/// Should the field be renamed, the accessor throws, which fails the cells
/// that use it rather than letting them pass.
/// </summary>
internal static class PciContextInternals
{
    /// <summary>
    /// The timer polling <paramref name="context"/>'s interrupt handler, or
    /// null when its interrupts go through MSI-X, were never granted, or
    /// were torn down. Read it during Probe, right after TryRequestInterrupts.
    /// </summary>
    public static SoftwareTimer? PollTimer(PciDeviceContext context) => PollTimerField(context);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_pollTimer")]
    private static extern ref SoftwareTimer? PollTimerField(PciDeviceContext context);
}
