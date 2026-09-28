// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// Marks a <see cref="Driver"/> class for the manifest the build generates.
/// The kernel project's manifest registers every marked class it can see,
/// in a fixed order (the kernel's own classes in declaration order, then
/// referenced assemblies by name), subject to the kernel's policy: a
/// <c>CosmosDriverExclude</c> item drops a driver, a <c>CosmosDriverInclude</c>
/// item adds one whose <see cref="Default"/> is false, and
/// <see cref="Feature"/> ties the registration to a feature switch.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class DriverAttribute : Attribute
{
    /// <summary>
    /// The feature switch the driver depends on. The manifest guards the
    /// registration with the matching <c>KernelFeatures</c> property, so the
    /// driver and everything only it references fold out of a kernel that
    /// turns the feature off. <see cref="DriverFeature.None"/> by default.
    /// </summary>
    public DriverFeature Feature { get; set; }

    /// <summary>
    /// Whether the driver is registered without being asked for. False for a
    /// driver a kernel must opt into with a <c>CosmosDriverInclude</c> item.
    /// True by default.
    /// </summary>
    public bool Default { get; set; } = true;
}
