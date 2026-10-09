// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Microsoft.CodeAnalysis;

namespace Cosmos.Kernel.SourceGenerators;

/// <summary>
/// Turns the discovered driver candidates and the kernel's policy into the
/// drivers the manifest registers, reporting what was left out and why; the
/// kernel's <see cref="SourceWriter"/> spells them. The kernel's own drivers
/// come first, in declaration order (file path, then position), then the
/// referenced assemblies' drivers as <see cref="DriverDiscovery.FromReferences"/>
/// sorted them.
/// </summary>
internal static class DriverManifestBuilder
{
    /// <summary>
    /// The SDK joins the policy items with commas: the compiler's editorconfig
    /// reader treats a semicolon as the start of a comment and would cut the
    /// list after its first name. A semicolon is still accepted for a value
    /// that reaches the generator whole.
    /// </summary>
    private static readonly char[] s_listSeparators = [',', ';'];
    private const string ExcludeItemName = "CosmosDriverExclude";
    private const string IncludeItemName = "CosmosDriverInclude";

    /// <summary>
    /// Selects the drivers the manifest registers, in manifest order. Policy
    /// is applied first, so a driver the kernel excludes or does not opt into
    /// is never inspected; a driver that would be registered but cannot be is
    /// reported and dropped.
    /// </summary>
    /// <param name="context">Receives the diagnostics.</param>
    /// <param name="options">The kernel's policy lists.</param>
    /// <param name="sourceDrivers">Candidates from the kernel's own source, in any order.</param>
    /// <param name="referencedDrivers">Candidates from referenced assemblies, already sorted.</param>
    public static List<DriverCandidate> Build(
        SourceProductionContext context,
        GeneratorOptions options,
        EquatableArray<DriverCandidate> sourceDrivers,
        EquatableArray<DriverCandidate> referencedDrivers)
    {
        List<DriverCandidate> ordered = new(sourceDrivers.Count + referencedDrivers.Count);
        ordered.AddRange(sourceDrivers);
        ordered.Sort(CompareSource);
        ordered.AddRange(referencedDrivers);

        HashSet<string> known = new(StringComparer.Ordinal);
        foreach (DriverCandidate candidate in ordered)
        {
            known.Add(candidate.FullName);
        }

        HashSet<string> excluded = ParseList(options.ExcludeList);
        HashSet<string> included = ParseList(options.IncludeList);
        ReportUnknown(context, known, excluded, ExcludeItemName);
        ReportUnknown(context, known, included, IncludeItemName);

        List<DriverCandidate> registered = new(ordered.Count);
        foreach (DriverCandidate candidate in ordered)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (excluded.Contains(candidate.FullName))
            {
                continue;
            }

            if (!candidate.IsDefault && !included.Contains(candidate.FullName))
            {
                continue;
            }

            if (candidate.SkipReason != DriverSkipReason.None)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.DriverSkipped,
                    candidate.ClassLocation?.ToLocation() ?? Location.None,
                    candidate.FullName,
                    Describe(candidate.SkipReason)));
                continue;
            }

            if (candidate.UnmappedFeature is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.FeatureUnmapped,
                    (candidate.AttributeLocation ?? candidate.ClassLocation)?.ToLocation() ?? Location.None,
                    candidate.FullName,
                    candidate.UnmappedFeature));
                continue;
            }

            registered.Add(candidate);
        }

        return registered;
    }

    private static int CompareSource(DriverCandidate left, DriverCandidate right)
    {
        int byPath = string.CompareOrdinal(left.OrderPath, right.OrderPath);
        return byPath != 0 ? byPath : left.OrderPosition.CompareTo(right.OrderPosition);
    }

    private static HashSet<string> ParseList(string list)
    {
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (string part in list.Split(s_listSeparators))
        {
            string name = part.Trim();
            if (name.Length != 0)
            {
                names.Add(name);
            }
        }

        return names;
    }

    private static void ReportUnknown(SourceProductionContext context, HashSet<string> known, HashSet<string> names, string itemName)
    {
        List<string> unknown = new();
        foreach (string name in names)
        {
            if (!known.Contains(name))
            {
                unknown.Add(name);
            }
        }

        // Sorted so the order of the warnings does not depend on hashing.
        unknown.Sort(string.CompareOrdinal);
        foreach (string name in unknown)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.PolicyNameUnknown, Location.None, itemName, name));
        }
    }

    private static string Describe(DriverSkipReason reason) => reason switch
    {
        DriverSkipReason.Abstract => "the class is abstract",
        DriverSkipReason.Static => "the class is static",
        DriverSkipReason.Generic => "the class is generic, or nested in a generic type",
        DriverSkipReason.NotADriver => "the class does not derive from Cosmos.Kernel.HAL.DriverKit.Driver",
        DriverSkipReason.NoConstructor => "the class has no parameterless constructor the kernel assembly can call",
        DriverSkipReason.Inaccessible => "the class is not accessible from the kernel assembly",
        _ => "the class cannot be constructed by the manifest",
    };
}
