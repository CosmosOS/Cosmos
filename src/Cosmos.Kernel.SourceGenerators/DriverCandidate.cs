// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// One class marked <c>[Driver]</c>, as the pipeline carries it: strings,
/// values and by-value locations only, never a symbol, so the incremental
/// cache can compare two runs and skip the manifest when nothing changed.
/// </summary>
/// <param name="FullName">The class's full name in C# display form without <c>global::</c>, nested types joined with dots.</param>
/// <param name="AssemblyName">Name of the assembly declaring the class.</param>
/// <param name="IsDefault">The attribute's <c>Default</c>: false for a driver a kernel opts into.</param>
/// <param name="FeatureGuard">The <c>KernelFeatures</c> member guarding the registration, or null for no guard.</param>
/// <param name="UnmappedFeature">The feature the attribute names when no <c>KernelFeatures</c> member matches it; null otherwise.</param>
/// <param name="SkipReason">Why the class cannot be registered, or <see cref="DriverSkipReason.None"/>.</param>
/// <param name="OrderPath">Path of the declaring file, the first ordering key for the kernel's own drivers.</param>
/// <param name="OrderPosition">Position of the declaration in that file, the second ordering key.</param>
/// <param name="ClassLocation">Where the class name is declared, for diagnostics; null for a referenced assembly.</param>
/// <param name="AttributeLocation">Where the attribute is applied, for diagnostics; null for a referenced assembly.</param>
internal sealed record DriverCandidate(
    string FullName,
    string AssemblyName,
    bool IsDefault,
    string? FeatureGuard,
    string? UnmappedFeature,
    DriverSkipReason SkipReason,
    string OrderPath,
    int OrderPosition,
    SourceLocationInfo? ClassLocation,
    SourceLocationInfo? AttributeLocation);
