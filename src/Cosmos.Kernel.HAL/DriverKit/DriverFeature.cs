// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A kernel feature switch a driver can be tied to through
/// <see cref="DriverAttribute.Feature"/>. The manifest generator registers
/// such a driver only when the matching <c>CosmosEnable*</c> property is on,
/// so a kernel that turns the feature off never carries the driver. The
/// members mirror the properties of <c>Cosmos.Kernel.System.KernelFeatures</c>
/// by name; a host-side test keeps the two lists equal.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum DriverFeature
{
    /// <summary>No feature: the driver is registered in every kernel that carries its assembly.</summary>
    None,

    /// <summary><c>CosmosEnableInterrupts</c>.</summary>
    Interrupts,

    /// <summary><c>CosmosEnableUART</c>.</summary>
    Uart,

    /// <summary><c>CosmosEnablePCI</c>.</summary>
    Pci,

    /// <summary><c>CosmosEnableTimer</c>.</summary>
    Timer,

    /// <summary><c>CosmosEnableKeyboard</c>.</summary>
    Keyboard,

    /// <summary><c>CosmosEnableMouse</c>.</summary>
    Mouse,

    /// <summary><c>CosmosEnableNetwork</c>.</summary>
    Network,

    /// <summary><c>CosmosEnableStorage</c>.</summary>
    Storage,

    /// <summary><c>CosmosEnableFat</c>.</summary>
    Fat,

    /// <summary><c>CosmosEnableGraphics</c>.</summary>
    Graphics,

    /// <summary><c>CosmosEnableScheduler</c>.</summary>
    Scheduler,

    /// <summary><c>CosmosEnableUsb</c>.</summary>
    Usb,
}
