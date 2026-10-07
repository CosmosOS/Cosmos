// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// The diagnostics the manifest generator reports. Every rule is listed in
/// <c>AnalyzerReleases.Unshipped.md</c> next to the project, which is what the
/// release tracking analyzer (RS2008) checks.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Cosmos.DriverKit";

    /// <summary>
    /// COSMOSGEN001: a class marked <c>[Driver]</c> that the manifest cannot
    /// construct (abstract, static, generic, not a <c>Driver</c>, no
    /// accessible parameterless constructor, or not accessible from the
    /// kernel) is left out.
    /// </summary>
    public static DiagnosticDescriptor DriverSkipped { get; } = new(
        id: "COSMOSGEN001",
        title: "Driver class left out of the manifest",
        messageFormat: "Driver class '{0}' is left out of the manifest: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The manifest constructs every [Driver] class with a parameterless constructor and registers it. A class it cannot construct is skipped: make it a concrete, non-generic class deriving from Cosmos.Kernel.HAL.DriverKit.Driver with a parameterless constructor the kernel assembly can reach.");

    /// <summary>
    /// COSMOSGEN002: a <c>CosmosDriverExclude</c> or <c>CosmosDriverInclude</c>
    /// item names no driver the build can see.
    /// </summary>
    public static DiagnosticDescriptor PolicyNameUnknown { get; } = new(
        id: "COSMOSGEN002",
        title: "Driver policy item matches no driver",
        messageFormat: "The {0} item '{1}' matches no [Driver] class visible to this kernel",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "CosmosDriverExclude and CosmosDriverInclude items name a driver by its full type name, without global:: and with nested types joined by dots. An item that matches nothing is stale or misspelled.");

    /// <summary>
    /// COSMOSGEN003: a <c>[Driver]</c> names a <c>DriverFeature</c> the
    /// generator cannot turn into a <c>Cosmos.Kernel.System.KernelFeatures</c>
    /// property read, so the registration cannot be guarded.
    /// </summary>
    public static DiagnosticDescriptor FeatureUnmapped { get; } = new(
        id: "COSMOSGEN003",
        title: "Driver feature has no KernelFeatures property",
        messageFormat: "Driver class '{0}' names feature '{1}', which has no matching Cosmos.Kernel.System.KernelFeatures property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The manifest guards a driver's registration with the KernelFeatures property of the same name as its DriverFeature member. A member without such a property cannot be guarded; add the property or pick another feature.");
}
