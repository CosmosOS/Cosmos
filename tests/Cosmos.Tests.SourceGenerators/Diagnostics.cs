// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Cosmos.Tests.SourceGenerators;

/// <summary>
/// The generator's diagnostics as a test expects them: by id, severity and
/// the exact message, since the message is what a kernel author reads.
/// </summary>
internal static class Diagnostics
{
    /// <summary>COSMOSGEN001 for <paramref name="fullName"/>, at markup location <paramref name="markupKey"/>.</summary>
    /// <param name="markupKey">The <c>{|#n:...|}</c> key around the class name.</param>
    /// <param name="fullName">The driver's full type name.</param>
    /// <param name="reason">The reason clause of the message.</param>
    public static DiagnosticResult DriverSkipped(int markupKey, string fullName, string reason) =>
        new DiagnosticResult("COSMOSGEN001", DiagnosticSeverity.Warning)
            .WithLocation(markupKey)
            .WithMessage($"Driver class '{fullName}' is left out of the manifest: {reason}");

    /// <summary>COSMOSGEN002 for a policy item that names no driver; it has no source location.</summary>
    /// <param name="itemName"><c>CosmosDriverExclude</c> or <c>CosmosDriverInclude</c>.</param>
    /// <param name="name">The item's value.</param>
    public static DiagnosticResult PolicyNameUnknown(string itemName, string name) =>
        new DiagnosticResult("COSMOSGEN002", DiagnosticSeverity.Warning)
            .WithNoLocation()
            .WithMessage($"The {itemName} item '{name}' matches no [Driver] class visible to this kernel");

    /// <summary>COSMOSGEN003 for <paramref name="fullName"/>, at markup location <paramref name="markupKey"/>.</summary>
    /// <param name="markupKey">The <c>{|#n:...|}</c> key around the attribute.</param>
    /// <param name="fullName">The driver's full type name.</param>
    /// <param name="feature">The <c>DriverFeature</c> member the attribute names.</param>
    public static DiagnosticResult FeatureUnmapped(int markupKey, string fullName, string feature) =>
        new DiagnosticResult("COSMOSGEN003", DiagnosticSeverity.Error)
            .WithLocation(markupKey)
            .WithMessage($"Driver class '{fullName}' names feature '{feature}', which has no matching Cosmos.Kernel.System.KernelFeatures property");
}
